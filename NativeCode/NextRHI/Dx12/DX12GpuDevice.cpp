#include "DX12GpuDevice.h"
#include "DX12CommandList.h"
#include "DX12Shader.h"
#include "DX12Buffer.h"
#include "DX12GpuState.h"
#include "DX12Event.h"
#include "DX12InputAssembly.h"
#include "DX12FrameBuffers.h"
#include "DX12Effect.h"
#include "DX12Drawcall.h"
#include "../NxEffect.h"
#include <dxgi1_3.h>

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	static void SafeCreateDXGIFactory(IDXGIFactory** ppDXGIFactory, UINT flags)
	{
		HMODULE  hmDXGI_DLL = LoadLibraryA("dxgi.dll");

		typedef HRESULT(WINAPI* FnpCreateDXGIFactory)(UINT Flags, REFIID ridd, void** ppFactory);

		FnpCreateDXGIFactory fnpCreateDXGIFactory = (FnpCreateDXGIFactory)GetProcAddress(hmDXGI_DLL, "CreateDXGIFactory1");

		if (fnpCreateDXGIFactory == NULL)
		{
			fnpCreateDXGIFactory = (FnpCreateDXGIFactory)GetProcAddress(hmDXGI_DLL, "CreateDXGIFactory");
			if (fnpCreateDXGIFactory == NULL)
			{
				//fnpCreateDXGIFactory = CreateDXGIFactory1;
			}
		}

		fnpCreateDXGIFactory(flags, __uuidof(IDXGIFactory), (void**)ppDXGIFactory);
	}

	bool DX12GpuSystem::InitGpuSystem(ERhiType type, const FGpuSystemDesc* desc)
	{
		//SafeCreateDXGIFactory(mDXGIFactory.GetAddressOf(), 0);
		UINT dxgiFlags = 0;
		if (desc->CreateDebugLayer && D3D12GetDebugInterface(IID_PPV_ARGS(mDebugLayer.GetAddressOf())) == S_OK)
		{
			mDebugLayer->EnableDebugLayer();
			//mDebugLayer->SetEnableGPUBasedValidation(TRUE);
			//debugLayer->SetGPUBasedValidationFlags(D3D12_GPU_BASED_VALIDATION_FLAGS_NONE);
			dxgiFlags |= DXGI_CREATE_FACTORY_DEBUG;
		}
		CreateDXGIFactory2(dxgiFlags, IID_PPV_ARGS(mDXGIFactory.GetAddressOf()));

		if (mDXGIFactory == nullptr)
			return false;

		mGIAdapters.clear();
		UINT index = 0;
		while (true)
		{
			AutoRef<IDXGIAdapter> adapter;
			auto hr = mDXGIFactory->EnumAdapters(index, adapter.GetAddressOf());
			if (hr != S_OK)
				break;
			mGIAdapters.push_back(adapter);
			index++;
		}

		return true;
	}
	int DX12GpuSystem::GetNumOfGpuDevice() const
	{
		return (int)mGIAdapters.size();
	}
	void DX12GpuSystem::GetDeviceDesc(int index, FGpuDeviceDesc* desc) const
	{
		if (index < 0 || index >= (int)mGIAdapters.size())
			return;
		DXGI_ADAPTER_DESC dxdesc{};
		mGIAdapters[index]->GetDesc(&dxdesc);
		desc->RhiType = ERhiType::RHI_D3D11;
		desc->AdapterId = index;
		desc->DedicatedVideoMemory = dxdesc.DedicatedVideoMemory;
		auto text = StringHelper::wstrtostr(dxdesc.Description);
		strcpy(desc->Name, text.c_str());
	}
	IGpuDevice* DX12GpuSystem::CreateDevice(const FGpuDeviceDesc* desc)
	{
		auto result = new DX12GpuDevice();
		result->InitDevice(this, desc);
		result->Desc.RhiType = ERhiType::RHI_D3D12;
		return result;
	}
	DX12GpuDevice::DX12GpuDevice()
	{
		mDevice = nullptr;
		//mFeatureLevel = 0;12
	}
	DX12GpuDevice::~DX12GpuDevice()
	{
		mCmdQueue->ClearIdleCmdlists();
		mCmdAllocatorManager = nullptr;
		mCmdQueue = nullptr;
		//Safe_Release(mDevice);
		//Safe_Release(mDXGIFactory);
	}
	ICmdQueue* DX12GpuDevice::GetCmdQueue()
	{
		return mCmdQueue;
	}
	
	void DX12GpuDevice::SetBreakOnID(int id, bool open)
	{
		if (mDebugInfoQueue != nullptr)
		{
			mDebugInfoQueue->SetBreakOnID((D3D12_MESSAGE_ID)id, open ? TRUE : FALSE);
		}
	}
	bool DX12GpuDevice::InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc)
	{
		Desc = *desc;
		mGpuSystem.FromObject(pGpuSystem);

		/*UINT dxgiFlags = 0;
		if (desc->CreateDebugLayer && D3D12GetDebugInterface(IID_PPV_ARGS(mDebugLayer.GetAddressOf())) == S_OK)
		{
			mDebugLayer->EnableDebugLayer();
			//mDebugLayer->SetEnableGPUBasedValidation(TRUE);
			//debugLayer->SetGPUBasedValidationFlags(D3D12_GPU_BASED_VALIDATION_FLAGS_NONE);
			dxgiFlags |= DXGI_CREATE_FACTORY_DEBUG;
		}

		//SafeCreateDXGIFactory(&mDXGIFactory, dxgiFlags);
		CreateDXGIFactory2(dxgiFlags, IID_PPV_ARGS(mDXGIFactory.GetAddressOf()));
		//CreateDXGIFactory1(IID_PPV_ARGS(&mDXGIFactory));*/

		mCmdQueue = MakeWeakRef(new DX12CmdQueue());
		mCmdQueue->mDevice = this;
		if (desc->DeviceHandle != nullptr)
		{
			
		}

		IDXGIAdapter* pIAdapter;
		((DX12GpuSystem*)pGpuSystem)->mDXGIFactory->EnumAdapters(desc->AdapterId, &pIAdapter);

		auto hr = D3D12CreateDevice(pIAdapter, D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(mDevice.GetAddressOf()));
		if (FAILED(hr))
			return false;
		/*ID3D12Device* tmp;
		hr = D3D12CreateDevice(pIAdapter, D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(&tmp));*/

		{
			DXGI_ADAPTER_DESC adapterDesc{};
			pIAdapter->GetDesc(&adapterDesc);
			auto n = StringHelper::wstrtostr(adapterDesc.Description);
			VFX_LTRACE(ELTT_Graphics, "Create Adapter {%s}\r\n", n.c_str());
		}

		mCaps.NumOfSwapchainFormats = 6;
		mCaps.SwapchainFormats[0] = EPixelFormat::PXF_R8G8B8A8_UNORM;
		mCaps.SwapchainFormats[1] = EPixelFormat::PXF_R8G8B8A8_UNORM_SRGB;
		mCaps.SwapchainFormats[2] = EPixelFormat::PXF_B8G8R8A8_UNORM;
		mCaps.SwapchainFormats[3] = EPixelFormat::PXF_B8G8R8A8_UNORM_SRGB;
		mCaps.SwapchainFormats[4] = EPixelFormat::PXF_R10G10B10A2_UNORM;
		mCaps.SwapchainFormats[5] = EPixelFormat::PXF_R16G16B16A16_FLOAT;

		mDevice->QueryInterface(IID_ID3D12InfoQueue, (void**)mDebugInfoQueue.GetAddressOf());
		if (mDebugInfoQueue != nullptr)
		{
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_COMMAND_LIST_CLOSED, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_COMMAND_ALLOCATOR_RESET, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_RENDER_TARGET_FORMAT_MISMATCH_PIPELINE_STATE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_INVALID_SUBRESOURCE_STATE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_MAP_INVALIDHEAP, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATEGRAPHICSPIPELINESTATE_RENDERTARGETVIEW_NOT_SET, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATEUNORDEREDACCESSVIEW_INVALIDFORMAT, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATERESOURCEANDHEAP_INVALIDHEAPPROPERTIES, TRUE);
			//mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_RESOURCE_BARRIER_BEFORE_AFTER_MISMATCH, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATE_CONSTANT_BUFFER_VIEW_INVALID_RESOURCE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_RESOURCE_BARRIER_INVALID_HEAP, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_COPYRESOURCE_INVALIDDSTRESOURCE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_RESOURCE_BARRIER_MATCHING_STATES, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_OBJECT_DELETED_WHILE_STILL_IN_USE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATESHADERRESOURCEVIEW_INVALIDFORMAT, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATEUNORDEREDACCESSVIEW_INVALIDDIMENSIONS, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_DEPTH_STENCIL_FORMAT_MISMATCH_PIPELINE_STATE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATESHADERRESOURCEVIEW_INVALIDFORMAT, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_SET_DESCRIPTOR_HEAP_INVALID, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_SET_DESCRIPTOR_TABLE_INVALID, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_INVALID_DESCRIPTOR_HANDLE, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_RESOURCE_BARRIER_BEFORE_AFTER_MISMATCH, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_AT_FAULT, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_UNMAP_RANGE_NOT_EMPTY, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_EXECUTECOMMANDLISTS_OPENCOMMANDLIST, TRUE);
			//mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_CREATE_COMMANDLIST12, TRUE);
			//mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_DESTROY_COMMANDLIST12, TRUE);
			//mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_DESTROY_HEAP, TRUE);
			//mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_DESTROY_COMMANDALLOCATOR, TRUE);
			//mDebugInfoQueue->SetBreakOnID(D3D12_MESSAGE_ID_DESTROY_MONITOREDFENCE, TRUE);

			D3D12_INFO_QUEUE_FILTER filter{};
			D3D12_MESSAGE_ID denyIds[]{
				D3D12_MESSAGE_ID_CLEARRENDERTARGETVIEW_MISMATCHINGCLEARVALUE,
				D3D12_MESSAGE_ID_DRAW_EMPTY_SCISSOR_RECTANGLE,
				D3D12_MESSAGE_ID_CREATE_COMMANDLIST12,
				D3D12_MESSAGE_ID_DESTROY_COMMANDLIST12,
			};
			filter.DenyList.NumIDs = _countof(denyIds);
			filter.DenyList.pIDList = denyIds;
			mDebugInfoQueue->PushStorageFilter(&filter);
		}

		mCmdAllocatorManager = MakeWeakRef(new DX12CommandAllocatorManager());
		mDescriptorSetAllocator = MakeWeakRef(new DX12DescriptorAllocatorManager());

		D3D12_COMMAND_QUEUE_DESC queueDesc{};
		queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
		if (desc->CreateDebugLayer)
			queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_DISABLE_GPU_TIMEOUT;
		mDevice->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(mCmdQueue->mCmdQueue.GetAddressOf()));

		mCmdQueue->Init(this);
		
		QueryDevice();

		FFenceDesc fcDesc{};
		mFrameFence = MakeWeakRef(this->CreateFence(&fcDesc, "Dx12 Frame Fence"));

		if (CmdSigForIndirectDrawIndex == nullptr)
		{
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			
			D3D12_INDIRECT_ARGUMENT_DESC argDesc[1]{};
			argDesc[0].Type = D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED;
			//argDesc[1].Type = D3D12_INDIRECT_ARGUMENT_TYPE_CONSTANT;
			//argDesc[1].Constant.RootParameterIndex = 18;//shader register...
			//argDesc[1].Constant.DestOffsetIn32BitValues = 0;
			//argDesc[1].Constant.Num32BitValuesToSet = 1;

			desc.ByteStride = sizeof(FIndirectDrawArgument);
			desc.NumArgumentDescs = sizeof(argDesc) / sizeof(D3D12_INDIRECT_ARGUMENT_DESC);
			desc.pArgumentDescs = argDesc;
			auto hr = mDevice->CreateCommandSignature(&desc, nullptr, IID_PPV_ARGS(CmdSigForIndirectDrawIndex.GetAddressOf()));
			ASSERT(hr == S_OK);
		}

		if (CmdSigForIndirectDispatch == nullptr)
		{
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			D3D12_INDIRECT_ARGUMENT_DESC argDesc[1]{};
			argDesc[0].Type = D3D12_INDIRECT_ARGUMENT_TYPE_DISPATCH;
			desc.ByteStride = sizeof(FIndirectDispatchArgument);
			desc.NumArgumentDescs = sizeof(argDesc) / sizeof(D3D12_INDIRECT_ARGUMENT_DESC);
			desc.pArgumentDescs = argDesc;
			auto hr = mDevice->CreateCommandSignature(&desc, nullptr, IID_PPV_ARGS(CmdSigForIndirectDispatch.GetAddressOf()));
			ASSERT(hr == S_OK);
		}

		mGpuResourceAlignment.CBufferAlignment = D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT;
		mGpuResourceAlignment.TexturePitchAlignment = D3D12_TEXTURE_DATA_PITCH_ALIGNMENT;
		mGpuResourceAlignment.TextureAlignment = D3D12_TEXTURE_DATA_PLACEMENT_ALIGNMENT;
		mGpuResourceAlignment.MsaaAlignment = D3D12_DEFAULT_MSAA_RESOURCE_PLACEMENT_ALIGNMENT;
		mGpuResourceAlignment.RawSrvUavAlignment = D3D12_RAW_UAV_SRV_BYTE_ALIGNMENT;
		mGpuResourceAlignment.UavCounterAlignment = D3D12_UAV_COUNTER_PLACEMENT_ALIGNMENT;

		mCaps.IsSupoortBufferToTexture = true;
		mCaps.IsSupportSSBO_VS = true;

		mDefaultBufferMemAllocator = MakeWeakRef(new DX12GpuDefaultMemAllocator());
		mDefaultBufferMemAllocator->mHeapProperties.Type = D3D12_HEAP_TYPE_DEFAULT;
		mDefaultBufferMemAllocator->mHeapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY::D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
		mDefaultBufferMemAllocator->mHeapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL::D3D12_MEMORY_POOL_UNKNOWN;
		mDefaultBufferMemAllocator->mResDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		mDefaultBufferMemAllocator->mResDesc.Alignment = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;//64k
		mDefaultBufferMemAllocator->mResDesc.Width = mDefaultBufferMemAllocator->mResDesc.Alignment * 2;//128k
		mDefaultBufferMemAllocator->mResDesc.Height = 1;
		mDefaultBufferMemAllocator->mResDesc.DepthOrArraySize = 1;
		mDefaultBufferMemAllocator->mResDesc.MipLevels = 1;
		mDefaultBufferMemAllocator->mResDesc.Format = DXGI_FORMAT::DXGI_FORMAT_UNKNOWN;
		mDefaultBufferMemAllocator->mResDesc.SampleDesc.Count = 1;
		mDefaultBufferMemAllocator->mResDesc.SampleDesc.Quality = 0;
		mDefaultBufferMemAllocator->mResDesc.Layout = D3D12_TEXTURE_LAYOUT::D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
		mDefaultBufferMemAllocator->mResDesc.Flags = D3D12_RESOURCE_FLAGS::D3D12_RESOURCE_FLAG_NONE;
		mDefaultBufferMemAllocator->mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_COMMON;

		mUploadBufferMemAllocator = MakeWeakRef(new DX12GpuDefaultMemAllocator());
		mUploadBufferMemAllocator->mHeapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
		mUploadBufferMemAllocator->mHeapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY::D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
		mUploadBufferMemAllocator->mHeapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL::D3D12_MEMORY_POOL_UNKNOWN;
		mUploadBufferMemAllocator->mResDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		mUploadBufferMemAllocator->mResDesc.Alignment = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;//64k
		mUploadBufferMemAllocator->mResDesc.Width = mUploadBufferMemAllocator->mResDesc.Alignment * 2;//128k
		mUploadBufferMemAllocator->mResDesc.Height = 1;
		mUploadBufferMemAllocator->mResDesc.DepthOrArraySize = 1;
		mUploadBufferMemAllocator->mResDesc.MipLevels = 1;
		mUploadBufferMemAllocator->mResDesc.Format = DXGI_FORMAT::DXGI_FORMAT_UNKNOWN;
		mUploadBufferMemAllocator->mResDesc.SampleDesc.Count = 1;
		mUploadBufferMemAllocator->mResDesc.SampleDesc.Quality = 0;
		mUploadBufferMemAllocator->mResDesc.Layout = D3D12_TEXTURE_LAYOUT::D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
		mUploadBufferMemAllocator->mResDesc.Flags = D3D12_RESOURCE_FLAGS::D3D12_RESOURCE_FLAG_NONE;
		mUploadBufferMemAllocator->mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;

		mUavBufferMemAllocator = MakeWeakRef(new DX12GpuDefaultMemAllocator());
		mUavBufferMemAllocator->mHeapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
		mUavBufferMemAllocator->mHeapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY::D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
		mUavBufferMemAllocator->mHeapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL::D3D12_MEMORY_POOL_UNKNOWN;
		mUavBufferMemAllocator->mResDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		mUavBufferMemAllocator->mResDesc.Alignment = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;//64k
		mUavBufferMemAllocator->mResDesc.Width = mUavBufferMemAllocator->mResDesc.Alignment * 2;//128k
		mUavBufferMemAllocator->mResDesc.Height = 1;
		mUavBufferMemAllocator->mResDesc.DepthOrArraySize = 1;
		mUavBufferMemAllocator->mResDesc.MipLevels = 1;
		mUavBufferMemAllocator->mResDesc.Format = DXGI_FORMAT::DXGI_FORMAT_UNKNOWN;
		mUavBufferMemAllocator->mResDesc.SampleDesc.Count = 1;
		mUavBufferMemAllocator->mResDesc.SampleDesc.Quality = 0;
		mUavBufferMemAllocator->mResDesc.Layout = D3D12_TEXTURE_LAYOUT::D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
		mUavBufferMemAllocator->mResDesc.Flags = D3D12_RESOURCE_FLAGS::D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
		mUavBufferMemAllocator->mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;

		mCBufferMemAllocator = MakeWeakRef(new DX12GpuPooledMemAllocator());
		mCBufferMemAllocator->mHeapProperties.Type = D3D12_HEAP_TYPE_UPLOAD;
		mCBufferMemAllocator->mHeapProperties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY::D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
		mCBufferMemAllocator->mHeapProperties.MemoryPoolPreference = D3D12_MEMORY_POOL::D3D12_MEMORY_POOL_UNKNOWN;
		mCBufferMemAllocator->mResDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		mCBufferMemAllocator->mResDesc.Alignment = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;//64k
		mCBufferMemAllocator->mResDesc.Width = mCBufferMemAllocator->mResDesc.Alignment * 2;//128k
		mCBufferMemAllocator->mResDesc.Height = 1;
		mCBufferMemAllocator->mResDesc.DepthOrArraySize = 1;
		mCBufferMemAllocator->mResDesc.MipLevels = 1;
		mCBufferMemAllocator->mResDesc.Format = DXGI_FORMAT::DXGI_FORMAT_UNKNOWN;
		mCBufferMemAllocator->mResDesc.SampleDesc.Count = 1;
		mCBufferMemAllocator->mResDesc.SampleDesc.Quality = 0;
		mCBufferMemAllocator->mResDesc.Layout = D3D12_TEXTURE_LAYOUT::D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
		mCBufferMemAllocator->mResDesc.Flags = D3D12_RESOURCE_FLAGS::D3D12_RESOURCE_FLAG_NONE;
		mCBufferMemAllocator->mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;

		auto tRtv = new DX12DescriptorSetAllocator();
		mRtvAllocator = MakeWeakRef(tRtv);
		{
			mRtvAllocator->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
			mRtvAllocator->Creator.mDesc.NumDescriptors = 1;
			mRtvAllocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
			mRtvAllocator->Creator.mDeviceRef.FromObject(this);
			mRtvAllocator->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
		}
		auto tDsv = new DX12DescriptorSetAllocator();
		mDsvAllocator = MakeWeakRef(tDsv);
		{
			mDsvAllocator->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
			mDsvAllocator->Creator.mDesc.NumDescriptors = 1;
			mDsvAllocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
			mDsvAllocator->Creator.mDeviceRef.FromObject(this);
			mDsvAllocator->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_DSV);
		}
		auto tSampler = new DX12DescriptorSetAllocator();
		mSamplerAllocator = MakeWeakRef(tSampler);
		{
			mSamplerAllocator->Creator.DebugName = L"Sampler";
			mSamplerAllocator->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER;
			mSamplerAllocator->Creator.mDesc.NumDescriptors = 1;
			mSamplerAllocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			mSamplerAllocator->Creator.mDeviceRef.FromObject(this);
			mSamplerAllocator->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}
		auto tSrv = new DX12DescriptorSetAllocator();
		mCbvSrvUavAllocator = MakeWeakRef(tSrv);
		{
			mCbvSrvUavAllocator->Creator.DebugName = L"CSU";
			mCbvSrvUavAllocator->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			mCbvSrvUavAllocator->Creator.mDesc.NumDescriptors = 1;
			mCbvSrvUavAllocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;//D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			mCbvSrvUavAllocator->Creator.mDeviceRef.FromObject(this);
			mCbvSrvUavAllocator->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		mNullCBV_SRV_UAV = mCbvSrvUavAllocator->Alloc<DX12DescriptorSetPagedObject>();
		mNullSampler = mSamplerAllocator->Alloc<DX12DescriptorSetPagedObject>();
		mNullRTV = mRtvAllocator->Alloc<DX12DescriptorSetPagedObject>();
		mNullDSV = mDsvAllocator->Alloc<DX12DescriptorSetPagedObject>();
		return true;
	}
	void DX12GpuDevice::QueryDevice()
	{
		D3D12_FEATURE_DATA_SHADER_MODEL sm{};
		mDevice->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_SHADER_MODEL, &sm, sizeof(sm));
		D3D12_FEATURE_DATA_D3D12_OPTIONS4 op4{};
		mDevice->CheckFeatureSupport(D3D12_FEATURE::D3D12_FEATURE_D3D12_OPTIONS4, &op4, sizeof(op4));
		if (op4.Native16BitShaderOpsSupported == false)
		{
			VFX_LTRACE(ELTT_Warning, "Native16BitShaderOpsSupported = false");
		}
		//ASSERT(op4.Native16BitShaderOpsSupported);
	}
	IBuffer* DX12GpuDevice::CreateBuffer(const FBufferDesc* desc)
	{
		auto result = new DX12Buffer();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITexture* DX12GpuDevice::CreateTexture(const FTextureDesc* desc)
	{
		auto result = new DX12Texture();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICbView* DX12GpuDevice::CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc)
	{
		auto result = new DX12CbView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IVbView* DX12GpuDevice::CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc)
	{
		auto result = new DX12VbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IIbView* DX12GpuDevice::CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc)
	{
		auto result = new DX12IbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISrView* DX12GpuDevice::CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc)
	{
		auto result = new DX12SrView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IUaView* DX12GpuDevice::CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc)
	{
		auto result = new DX12UaView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderTargetView* DX12GpuDevice::CreateRTV(ITexture* pBuffer, const FRtvDesc* desc)
	{
		auto result = new DX12RenderTargetView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IDepthStencilView* DX12GpuDevice::CreateDSV(ITexture* pBuffer, const FDsvDesc* desc)
	{
		auto result = new DX12DepthStencilView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISampler* DX12GpuDevice::CreateSampler(const FSamplerDesc* desc)
	{
		auto result = new DX12Sampler();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISwapChain* DX12GpuDevice::CreateSwapChain(const FSwapChainDesc* desc)
	{
		auto result = new DX12SwapChain();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderPass* DX12GpuDevice::CreateRenderPass(const FRenderPassDesc* desc)
	{
		auto result = new IRenderPass();
		result->Desc = *desc;
		return result;
	}
	IFrameBuffers* DX12GpuDevice::CreateFrameBuffers(IRenderPass* rpass)
	{
		auto result = new DX12FrameBuffers();
		result->mRenderPass = rpass;
		return result;
	}
	IGpuPipeline* DX12GpuDevice::CreatePipeline(const FGpuPipelineDesc* desc)
	{
		auto result = new DX12GpuPipeline();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGpuDrawState* DX12GpuDevice::CreateGpuDrawState()
	{
		return new DX12GpuDrawState();
	}
	IInputLayout* DX12GpuDevice::CreateInputLayout(FInputLayoutDesc* desc)
	{
		auto result = new DX12InputLayout();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICommandList* DX12GpuDevice::CreateCommandList()
	{
		auto result = new DX12CommandList();
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IShader* DX12GpuDevice::CreateShader(FShaderDesc* desc)
	{
		auto result = new DX12Shader();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicsEffect* DX12GpuDevice::CreateShaderEffect()
	{
		return new DX12GraphicsEffect();
	}
	IComputeEffect* DX12GpuDevice::CreateComputeEffect()
	{
		return new DX12ComputeEffect();
	}
	IFence* DX12GpuDevice::CreateFence(const FFenceDesc* desc, const char* name)
	{
		auto result = new DX12Fence();
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IEvent* DX12GpuDevice::CreateGpuEvent(const FEventDesc* desc, const char* name)
	{
		auto result = new DX12Event(name);
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicDraw* DX12GpuDevice::CreateGraphicDraw()
	{
		auto result = new DX12GraphicDraw();
		result->mDeviceRef.FromObject(this);
		return result;
	}
	IComputeDraw* DX12GpuDevice::CreateComputeDraw()
	{
		auto result = new DX12ComputeDraw();
		result->mDeviceRef.FromObject(this);
		return result;
	}
	IGpuScope* DX12GpuDevice::CreateGpuScope()
	{
		auto result = new DX12GpuScope();
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	void DX12GpuDevice::TickPostEvents()
	{
		mCmdAllocatorManager->TickRecycle();
		IGpuDevice::TickPostEvents();
	}

	DX12CmdQueue::DX12CmdQueue()
	{

	}
	DX12CmdQueue::~DX12CmdQueue()
	{
		if (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.pop();
		}
	}
	void DX12CmdQueue::Init(DX12GpuDevice* device)
	{
		mFramePost = MakeWeakRef(device->CreateCommandList());
		mFramePost->BeginCommand();

		FFenceDesc fcDesc{};
		mFlushFence = MakeWeakRef(device->CreateFence(&fcDesc, "CmdQueue Flush"));;

		mCmdQueue->GetTimestampFrequency(&mDefaultQueueFrequence);
	}
	void DX12CmdQueue::ClearIdleCmdlists()
	{
		if (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.pop();
		}
	}
	void DX12CmdQueue::ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type)
	{
		VAutoVSLLock locker(mQueueLocker);
		for (UINT i = 0; i < NumOfWait; i++)
		{
			ppWaitCmdlists[i]->mCommitFence->WaitToExpect();
		}

		for (UINT i = 0; i < NumOfExe; i++)
		{
			auto dx11Cmd = (DX12CommandList*)Cmdlist[i];
			dx11Cmd->Commit(this, type);
		}
	}
	void DX12CmdQueue::TryRecycle()
	{
		VAutoVSLLock locker(mQueueLocker);
		for (size_t i = 0; i < mWaitRecycleCmdlists.size(); i++)
		{
			auto& cur = mWaitRecycleCmdlists[i];
			auto fenceValue = cur.CmdList->GetCommitFence()->GetCompletedValue();
			if (fenceValue >= cur.WaitFenceValue)
			{
				mIdleCmdlist.push(mWaitRecycleCmdlists[i].CmdList);
				mWaitRecycleCmdlists.erase(mWaitRecycleCmdlists.begin() + i);
				i--;
			}
		}
	}
	ICommandList* DX12CmdQueue::GetIdleCmdlist()
	{
		TryRecycle();

		VAutoVSLLock locker(mQueueLocker);
		if (mIdleCmdlist.empty())
		{
			mIdleCmdlist.push(MakeWeakRef(mDevice->CreateCommandList()));
		}
		auto result = mIdleCmdlist.front();
		result->AddRef();
		mIdleCmdlist.pop();
		return result;
	}
	void DX12CmdQueue::ReleaseIdleCmdlist(ICommandList* cmd)
	{
		VAutoVSLLock locker(mQueueLocker);
		FWaitRecycle tmp;
		tmp.CmdList = cmd;
		tmp.WaitFenceValue = cmd->GetCommitFence()->GetExpectValue();
		mWaitRecycleCmdlists.push_back(tmp);
		cmd->Release();
		return;
	}
	void DX12CmdQueue::Flush(EQueueType type)
	{
		IncreaseSignal(mFlushFence, type);
		mFlushFence->WaitToExpect();
	}
}

NS_END