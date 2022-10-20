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
		mCmdQueue = nullptr;
		Safe_Release(mDevice);
		Safe_Release(mDXGIFactory);
	}
	ICmdQueue* DX12GpuDevice::GetCmdQueue()
	{
		return mCmdQueue;
	}
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
	bool DX12GpuDevice::InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc)
	{
		Desc = *desc;

		UINT dxgiFlags = 0;
		if (D3D12GetDebugInterface(IID_PPV_ARGS(mDebugLayer.GetAddressOf())) == S_OK)
		{
			mDebugLayer->EnableDebugLayer();
			//mDebugLayer->SetEnableGPUBasedValidation(TRUE);
			//debugLayer->SetGPUBasedValidationFlags(D3D12_GPU_BASED_VALIDATION_FLAGS_NONE);
			dxgiFlags |= DXGI_CREATE_FACTORY_DEBUG;
		}

		//SafeCreateDXGIFactory(&mDXGIFactory, dxgiFlags);
		CreateDXGIFactory2(dxgiFlags, IID_PPV_ARGS(&mDXGIFactory));
		//CreateDXGIFactory1(IID_PPV_ARGS(&mDXGIFactory));

		mCmdQueue = MakeWeakRef(new DX12CmdQueue());
		mCmdQueue->mDevice = this;
		if (desc->DeviceHandle != nullptr)
		{
			
		}

		IDXGIAdapter* pIAdapter;
		mDXGIFactory->EnumAdapters(desc->AdapterId, &pIAdapter);

		auto hr = D3D12CreateDevice(pIAdapter, D3D_FEATURE_LEVEL_12_1, IID_PPV_ARGS(&mDevice));
		if (FAILED(hr))
			return false;

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

			D3D12_INFO_QUEUE_FILTER filter{};
			D3D12_MESSAGE_ID denyIds[]{
				D3D12_MESSAGE_ID_CLEARRENDERTARGETVIEW_MISMATCHINGCLEARVALUE,
				D3D12_MESSAGE_ID_DRAW_EMPTY_SCISSOR_RECTANGLE,
			};
			filter.DenyList.NumIDs = _countof(denyIds);
			filter.DenyList.pIDList = denyIds;
			mDebugInfoQueue->PushStorageFilter(&filter);
		}

		D3D12_COMMAND_QUEUE_DESC queueDesc{};
		queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
		//queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_DISABLE_GPU_TIMEOUT;
		mDevice->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(mCmdQueue->mCmdQueue.GetAddressOf()));

		mCmdQueue->Init(this);
		
		auto tRtv = new DX12DescriptorSetAllocator();
		mRtvHeapManager = MakeWeakRef(tRtv);
		{
			mRtvHeapManager->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
			mRtvHeapManager->Creator.mDesc.NumDescriptors = 1;
			mRtvHeapManager->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
			mRtvHeapManager->Creator.mShaderEffect = nullptr;
			mRtvHeapManager->Creator.IsSampler = false;
			mRtvHeapManager->Creator.mDeviceRef.FromObject(this);
			mRtvHeapManager->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
		}
		auto tDsv = new DX12DescriptorSetAllocator();
		mDsvHeapManager = MakeWeakRef(tDsv);
		{
			mDsvHeapManager->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_DSV;
			mDsvHeapManager->Creator.mDesc.NumDescriptors = 1;
			mDsvHeapManager->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
			mDsvHeapManager->Creator.mShaderEffect = nullptr;
			mDsvHeapManager->Creator.IsSampler = false;
			mDsvHeapManager->Creator.mDeviceRef.FromObject(this);
			mDsvHeapManager->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_DSV);
		}
		auto tSampler = new DX12DescriptorSetAllocator();
		mSamplerAllocHeapManager = MakeWeakRef(tSampler);
		{
			mSamplerAllocHeapManager->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER;
			mSamplerAllocHeapManager->Creator.mDesc.NumDescriptors = 1;
			mSamplerAllocHeapManager->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;// D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			mSamplerAllocHeapManager->Creator.mShaderEffect = nullptr;
			mSamplerAllocHeapManager->Creator.IsSampler = true;
			mSamplerAllocHeapManager->Creator.mDeviceRef.FromObject(this);
			mSamplerAllocHeapManager->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}
		auto tSrv = new DX12DescriptorSetAllocator();
		mSrvAllocHeapManager = MakeWeakRef(tSrv);
		{
			mSrvAllocHeapManager->Creator.mDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
			mSrvAllocHeapManager->Creator.mDesc.NumDescriptors = 1;
			mSrvAllocHeapManager->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;//D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			mSrvAllocHeapManager->Creator.mShaderEffect = nullptr;
			mSrvAllocHeapManager->Creator.IsSampler = false;
			mSrvAllocHeapManager->Creator.mDeviceRef.FromObject(this);
			mSrvAllocHeapManager->mDescriptorStride = mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}

		mCmdAllocatorManager = MakeWeakRef(new DX12CommandAllocatorManager());
		
		QueryDevice();

		FFenceDesc fcDesc{};
		mFrameFence = MakeWeakRef(this->CreateFence(&fcDesc, "Dx12 Frame Fence"));

		if (CmdSigForIndirectDrawIndex == nullptr)
		{
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			D3D12_INDIRECT_ARGUMENT_DESC argDesc{};
			argDesc.Type = D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED;
			desc.ByteStride = 20;
			desc.NumArgumentDescs = 1;
			desc.pArgumentDescs = &argDesc;
			mDevice->CreateCommandSignature(&desc, nullptr, IID_PPV_ARGS(CmdSigForIndirectDrawIndex.GetAddressOf()));
		}

		if (CmdSigForIndirectDispatch == nullptr)
		{
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			D3D12_INDIRECT_ARGUMENT_DESC argDesc{};
			argDesc.Type = D3D12_INDIRECT_ARGUMENT_TYPE_DISPATCH;
			desc.ByteStride = 12;
			desc.NumArgumentDescs = 1;
			desc.pArgumentDescs = &argDesc;
			mDevice->CreateCommandSignature(&desc, nullptr, IID_PPV_ARGS(CmdSigForIndirectDispatch.GetAddressOf()));
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

		{
			mNullCBV = mSrvAllocHeapManager->Alloc<DX12DescriptorSetPagedObject>();
			mNullSampler = mSamplerAllocHeapManager->Alloc<DX12DescriptorSetPagedObject>();
		}
		return true;
	}
	void DX12GpuDevice::QueryDevice()
	{
		
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
		FFenceDesc fcDesc{};
		mQueueFence = MakeWeakRef(device->CreateFence(&fcDesc, "CmdQueue Fence"));;
		mQueueExecuteFence = MakeWeakRef(device->CreateFence(&fcDesc, "QueueExecute"));
	}
	void DX12CmdQueue::ClearIdleCmdlists()
	{
		if (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.pop();
		}
	}
	void DX12CmdQueue::ExecuteCommandList(ICommandList* Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists)
	{
		auto dx11Cmd = (DX12CommandList*)Cmdlist;
		VAutoVSLLock locker(mImmCmdListLocker);
		if (NumOfWait == UINT_MAX)
		{
			this->WaitFence(mQueueExecuteFence, mQueueExecuteFence->GetAspectValue());
		}
		else
		{
			for (UINT i = 0; i < NumOfWait; i++)
			{
				this->WaitFence(ppWaitCmdlists[i]->mCommitFence, ppWaitCmdlists[i]->mCommitFence->GetAspectValue());
			}
		}
		dx11Cmd->Commit(this);
		this->IncreaseSignal(dx11Cmd->mCommitFence);
		this->IncreaseSignal(mQueueExecuteFence);
	}
	void DX12CmdQueue::ExecuteCommandList(UINT num, ICommandList** ppCmdlist)
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		for (UINT i = 0; i < num; i++)
		{
			auto dx11Cmd = (DX12CommandList*)ppCmdlist[i];
			dx11Cmd->Commit(this);
		}
	}
	UINT64 DX12CmdQueue::SignalFence(IFence* fence, UINT64 value)
	{
		fence->Signal(this, value);
		return fence->GetCompletedValue();
	}
	void DX12CmdQueue::WaitFence(IFence* fence, UINT64 value)
	{
		auto dxFence = (DX12Fence*)fence;
		dxFence->mFence->SetEventOnCompletion(value, dxFence->mEvent->mHandle);
		mCmdQueue->Wait(dxFence->mFence, value);
	}
	void DX12CmdQueue::TryRecycle()
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		auto fenceValue = mQueueFence->GetCompletedValue();
		for (size_t i = 0; i < mWaitRecycleCmdlists.size(); i++)
		{
			if (fenceValue >= mWaitRecycleCmdlists[i].WaitFenceValue)
			{
				mIdleCmdlist.push(mWaitRecycleCmdlists[i].CmdList);
				mWaitRecycleCmdlists.erase(mWaitRecycleCmdlists.begin() + i);
				i--;
			}
		}
	}
	ICommandList* DX12CmdQueue::GetIdleCmdlist(EQueueCmdlist type)
	{
		TryRecycle();

		switch (type)
		{
		case QCL_Read:
			{
				mImmCmdListLocker.Lock();
				return nullptr;
			}
		case QCL_Transient:
			{
				VAutoVSLLock locker(mImmCmdListLocker);
				if (mIdleCmdlist.empty())
				{
					mIdleCmdlist.push(MakeWeakRef(mDevice->CreateCommandList()));
				}
				auto result = mIdleCmdlist.front();
				result->AddRef();
				mIdleCmdlist.pop();
				return result;
			}
		case QCL_FramePost:
			{
				mImmCmdListLocker.Lock();
				if (mFramePost == nullptr)
				{
					mFramePost = MakeWeakRef((DX12CommandList*)mDevice->CreateCommandList());
				}
				return mFramePost;
			}
			break;
		default:
			return nullptr;
		}
	}
	void DX12CmdQueue::ReleaseIdleCmdlist(ICommandList* cmd, EQueueCmdlist type)
	{
		switch (type)
		{
			case QCL_Read:
			{
				mImmCmdListLocker.Unlock();
				return;
			}
			case QCL_Transient:
			{
				VAutoVSLLock locker(mImmCmdListLocker);
				FWaitRecycle tmp;
				tmp.CmdList = cmd;
				tmp.WaitFenceValue = mQueueFence->GetAspectValue() + 1;
				SignalFence(mQueueFence, tmp.WaitFenceValue);
				mWaitRecycleCmdlists.push_back(tmp);
				cmd->Release();
				return;
			}
			case QCL_FramePost:
			{
				mImmCmdListLocker.Unlock();
				return;
			}
			default:
				return;
		}
	}
	void DX12CmdQueue::Flush()
	{
		FFenceDesc fenceDesc{};
		auto fence = MakeWeakRef(mDevice->CreateFence(&fenceDesc, "Upload Texture"));
		SignalFence(fence, 1);
		fence->Wait(1);
	}
}

NS_END