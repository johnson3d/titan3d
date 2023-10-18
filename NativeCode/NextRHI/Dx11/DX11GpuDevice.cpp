#include "DX11GpuDevice.h"
#include "DX11CommandList.h"
#include "DX11Shader.h"
#include "DX11Buffer.h"
#include "DX11GpuState.h"
#include "DX11Event.h"
#include "DX11InputAssembly.h"
#include "DX11FrameBuffers.h"
#include "DX11Drawcall.h"
#include "../NxEffect.h"
#include "../../Base/thread/vfxthread.h"

#if defined(HasModule_GpuDump)
#include "../../Bricks/GpuDump/NvAftermath.h"
#endif

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	static void SafeCreateDXGIFactory(IDXGIFactory** ppDXGIFactory)
	{
		HMODULE  hmDXGI_DLL = LoadLibraryA("dxgi.dll");

		typedef HRESULT(WINAPI* FnpCreateDXGIFactory)(REFIID ridd, void** ppFactory);

		FnpCreateDXGIFactory fnpCreateDXGIFactory = (FnpCreateDXGIFactory)GetProcAddress(hmDXGI_DLL, "CreateDXGIFactory1");

		if (fnpCreateDXGIFactory == NULL)
		{
			fnpCreateDXGIFactory = (FnpCreateDXGIFactory)GetProcAddress(hmDXGI_DLL, "CreateDXGIFactory");
			if (fnpCreateDXGIFactory == NULL)
			{
				fnpCreateDXGIFactory = CreateDXGIFactory1;
			}
		}

		fnpCreateDXGIFactory(__uuidof(IDXGIFactory), (void**)ppDXGIFactory);
	}
	DX11GpuSystem::~DX11GpuSystem()
	{
	}

	bool DX11GpuSystem::InitGpuSystem(ERhiType type, const FGpuSystemDesc* desc)
	{
		SafeCreateDXGIFactory(mDXGIFactory.GetAddressOf());
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
	int DX11GpuSystem::GetNumOfGpuDevice() const
	{
		return (int)mGIAdapters.size();
	}
	void DX11GpuSystem::GetDeviceDesc(int index, FGpuDeviceDesc* desc) const
	{
		if (index < 0 || index >= (int)mGIAdapters.size())
			return;
		DXGI_ADAPTER_DESC dxdesc{};
		mGIAdapters[index]->GetDesc(&dxdesc);
		desc->RhiType = ERhiType::RHI_D3D11;
		desc->VendorId = dxdesc.VendorId;
		desc->AdapterId = index;
		desc->DedicatedVideoMemory = dxdesc.DedicatedVideoMemory;
		auto text = StringHelper::wstrtostr(dxdesc.Description);
		strcpy(desc->Name, text.c_str());
	}
	IGpuDevice* DX11GpuSystem::CreateDevice(const FGpuDeviceDesc* desc)
	{
		auto result = new DX11GpuDevice();
		result->InitDevice(this, desc);
		result->Desc.RhiType = ERhiType::RHI_D3D11;
		return result;
	}
	DX11GpuDevice::DX11GpuDevice()
	{
		mDevice = nullptr;
		mDevice5 = nullptr;
		//mFeatureLevel = 0;12
	}
	DX11GpuDevice::~DX11GpuDevice()
	{
		mPostCmdList = nullptr;
		mCmdQueue->ClearIdleCmdlists();
		mCmdQueue = nullptr;
		Safe_Release(mDefinedAnnotation);
		Safe_Release(mDevice5);
		Safe_Release(mDevice);
	}
	void DX11GpuDevice::TryFinalizeDevice(IGpuSystem* pGpuSystem)
	{
		mIsTryFinalize = true;
		mIsFinalized = false;
	}
	ICmdQueue* DX11GpuDevice::GetCmdQueue()
	{
		return mCmdQueue;
	}
	
	void DX11GpuDevice::SetBreakOnID(int id, bool open)
	{
		if (mDebugInfoQueue != nullptr)
		{
			mDebugInfoQueue->SetBreakOnID((D3D11_MESSAGE_ID)id, open ? TRUE : FALSE);
		}
	}

	bool DX11GpuDevice::InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc)
	{
		mDeviceThreadId = vfxThread::GetCurrentThreadId();
		Desc = *desc;

		mDXGIFactory = ((DX11GpuSystem*)pGpuSystem)->mDXGIFactory;
		mCmdQueue = MakeWeakRef(new DX11CmdQueue());
		mCmdQueue->mDevice = this;
		ID3D11DeviceContext* pImmContext;
		if (desc->DeviceHandle != nullptr)
		{
			mDevice = (ID3D11Device*)desc->DeviceHandle;
			mDevice->AddRef();
			pImmContext = (ID3D11DeviceContext*)desc->DeviceContextHandle;
			mCmdQueue->mHardwareContext = MakeWeakRef(new DX11CommandList());
			mCmdQueue->mHardwareContext->Init(this, pImmContext);
			QueryDevice();

			mCmdQueue->Init(this);
			return true;
		}

		UINT createDeviceFlags = 0;
		if (desc->CreateDebugLayer)
			createDeviceFlags |= (D3D11_CREATE_DEVICE_DEBUG | D3D11_CREATE_DEVICE_DISABLE_GPU_TIMEOUT);
		D3D_FEATURE_LEVEL featureLevels[] =
		{
			D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL_11_1,
			D3D_FEATURE_LEVEL_10_0,
		};
		UINT numFeatureLevels = ARRAYSIZE(featureLevels);
#if defined(HasModule_GpuDump)
		if (desc->GpuDump && desc->IsNVIDIA())
		{
			GpuDump::NvAftermath::InitDump(NxRHI::RHI_D3D11);
		}
#endif
		auto hr = D3D11CreateDevice(NULL, D3D_DRIVER_TYPE_HARDWARE, NULL/*(HMODULE)desc->AppHandle*/, createDeviceFlags,
			featureLevels, numFeatureLevels, D3D11_SDK_VERSION, &mDevice, &mFeatureLevel, &pImmContext);
		if (FAILED(hr))
			return false;

#if defined(HasModule_GpuDump)
		if (desc->GpuDump && desc->IsNVIDIA())
		{
			GpuDump::NvAftermath::DeviceCreated(NxRHI::RHI_D3D11, this);
		}
#endif
		/*UINT required = D3D11_FORMAT_SUPPORT_RENDER_TARGET | D3D11_FORMAT_SUPPORT_DISPLAY;
		for (DXGI_FORMAT i = DXGI_FORMAT::DXGI_FORMAT_UNKNOWN; i < DXGI_FORMAT::DXGI_FORMAT_B4G4R4A4_UNORM; i++)
		{
			UINT flags = 0;
			if (mDevice->CheckFormatSupport(i, &flags) == S_OK)
			{
				if ((flags & required) == required)
				{
				}
			}
		}*/

		mDevice->QueryInterface(IID_ID3D11InfoQueue, (void**)mDebugInfoQueue.GetAddressOf());
		if (mDebugInfoQueue != nullptr)
		{
			//mDebugInfoQueue->SetBreakOnID(D3D11_MESSAGE_ID_CORRUPTED_PARAMETER1, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D11_MESSAGE_ID_DEVICE_UNORDEREDACCESSVIEW_RETURN_TYPE_MISMATCH, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D11_MESSAGE_ID_COPYSUBRESOURCEREGION_INVALIDSOURCE, TRUE); 
			mDebugInfoQueue->SetBreakOnID(D3D11_MESSAGE_ID_DEVICE_CSSETUNORDEREDACCESSVIEWS_HAZARD, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D11_MESSAGE_ID_DEVICE_DRAW_SAMPLER_MISMATCH, TRUE);
			mDebugInfoQueue->SetBreakOnID(D3D11_MESSAGE_ID_CORRUPTED_MULTITHREADING, TRUE);
		}
		mCaps.NumOfSwapchainFormats = 6;
		mCaps.SwapchainFormats[0] = EPixelFormat::PXF_R8G8B8A8_UNORM;
		mCaps.SwapchainFormats[1] = EPixelFormat::PXF_R8G8B8A8_UNORM_SRGB;
		mCaps.SwapchainFormats[2] = EPixelFormat::PXF_B8G8R8A8_UNORM;
		mCaps.SwapchainFormats[3] = EPixelFormat::PXF_B8G8R8A8_UNORM_SRGB;
		mCaps.SwapchainFormats[4] = EPixelFormat::PXF_R10G10B10A2_UNORM;
		mCaps.SwapchainFormats[5] = EPixelFormat::PXF_R16G16B16A16_FLOAT;

		mCmdQueue->mHardwareContext = MakeWeakRef(new DX11CommandList());
		mCmdQueue->mHardwareContext->Init(this, pImmContext);
		mCmdQueue->mHardwareContext->IsImmContext = true;
		pImmContext->Release();

		QueryDevice();
		mCmdQueue->Init(this);

		FFenceDesc fcDesc{};
		
		mFrameFence = MakeWeakRef(this->CreateFence(&fcDesc, "Dx11 Frame Fence"));

		mCaps.IsSupoortBufferToTexture = false;
		mCaps.IsSupportSSBO_VS = true;
		
		/*mGpuResourceAlignment.CBufferAlignment = 256;
		mGpuResourceAlignment.TexturePitchAlignment = 256;
		mGpuResourceAlignment.TextureAlignment = 512;
		mGpuResourceAlignment.MsaaAlignment = 4194304;
		mGpuResourceAlignment.RawSrvUavAlignment = 16;
		mGpuResourceAlignment.UavCounterAlignment = 4096;*/

		mPostCmdList = MakeWeakRef((DX11CommandList*)this->CreateCommandList());
		mPostCmdList->BeginCommand();

		return true;
	}
	void DX11GpuDevice::QueryDevice()
	{
		if (mDevice->QueryInterface(IID_ID3D11Device5, (void**)&mDevice5) == S_OK)
		{
			mCaps.IsSupportFence = true;
		}
		else
		{
			mCaps.IsSupportFence = false;
		}
		if (mCmdQueue->mHardwareContext->mContext->QueryInterface(__uuidof(ID3DUserDefinedAnnotation), reinterpret_cast<void**>(&mDefinedAnnotation)) == S_OK)
		{
			
		}
	}
	IBuffer* DX11GpuDevice::CreateBuffer(const FBufferDesc* desc)
	{
		auto result = new DX11Buffer();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITexture* DX11GpuDevice::CreateTexture(const FTextureDesc* desc)
	{
		auto result = new DX11Texture();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITexture* DX11GpuDevice::CreateTexture(void* pSharedObject)
	{
		auto result = new DX11Texture();
		if (result->Init(this, pSharedObject) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICbView* DX11GpuDevice::CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc)
	{
		auto result = new DX11CbView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IVbView* DX11GpuDevice::CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc)
	{
		auto result = new DX11VbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IIbView* DX11GpuDevice::CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc)
	{
		auto result = new DX11IbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISrView* DX11GpuDevice::CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc)
	{
		auto result = new DX11SrView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IUaView* DX11GpuDevice::CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc)
	{
		auto result = new DX11UaView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderTargetView* DX11GpuDevice::CreateRTV(ITexture* pBuffer, const FRtvDesc* desc)
	{
		auto result = new DX11RenderTargetView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IDepthStencilView* DX11GpuDevice::CreateDSV(ITexture* pBuffer, const FDsvDesc* desc)
	{
		auto result = new DX11DepthStencilView();
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISampler* DX11GpuDevice::CreateSampler(const FSamplerDesc* desc)
	{
		auto result = new DX11Sampler();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISwapChain* DX11GpuDevice::CreateSwapChain(const FSwapChainDesc* desc)
	{
		auto result = new DX11SwapChain();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderPass* DX11GpuDevice::CreateRenderPass(const FRenderPassDesc* desc)
	{
		auto result = new IRenderPass();
		result->Desc = *desc;
		return result;
	}
	IFrameBuffers* DX11GpuDevice::CreateFrameBuffers(IRenderPass* rpass)
	{
		auto result = new DX11FrameBuffers();
		result->mRenderPass = rpass;
		return result;
	}
	IGpuPipeline* DX11GpuDevice::CreatePipeline(const FGpuPipelineDesc* desc)
	{
		auto result = new DX11GpuPipeline();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGpuDrawState* DX11GpuDevice::CreateGpuDrawState()
	{
		return new DX11GpuDrawState();
	}
	IInputLayout* DX11GpuDevice::CreateInputLayout(FInputLayoutDesc* desc)
	{
		auto result = new DX11InputLayout();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICommandList* DX11GpuDevice::CreateCommandList()
	{
		auto result = new DX11CommandList();
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IShader* DX11GpuDevice::CreateShader(FShaderDesc* desc)
	{
		auto result = new DX11Shader();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicsEffect* DX11GpuDevice::CreateShaderEffect()
	{
		return new IGraphicsEffect();
	}
	IComputeEffect* DX11GpuDevice::CreateComputeEffect()
	{
		return new IComputeEffect();
	}
	IFence* DX11GpuDevice::CreateFence(const FFenceDesc* desc, const char* name)
	{
		auto result = new DX11Fence();
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IEvent* DX11GpuDevice::CreateGpuEvent(const FEventDesc* desc, const char* name)
	{
		auto result = new DX11Event(name);
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicDraw* DX11GpuDevice::CreateGraphicDraw()
	{
		auto result = new DX11GraphicDraw();
		return result;
	}
	IGpuScope* DX11GpuDevice::CreateGpuScope()
	{
		auto result = new DX11GpuScope();
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}

	void DX11GpuDevice::TickPostEvents()
	{
		IsSyncStage = true;
		if (mPostCmdList != nullptr)
		{
			mPostCmdList->FlushDraws();
			mPostCmdList->EndCommand();
			GetCmdQueue()->ExecuteCommandListSingle(mPostCmdList, EQueueType::QU_Default);
			mPostCmdList->BeginCommand();
		}
		IGpuDevice::TickPostEvents();
		if (mIsTryFinalize)
		{
			mFrameFence->WaitToExpect();
			mCmdQueue->Flush(EQueueType::QU_ALL);
			bool post = mTickingPostEvents.size() == 0 && mPostEvents.size() == 0;

			if (post)
			{
				mCmdQueue->ClearIdleCmdlists();
				mIsFinalized = true;
			}
		}
		IsSyncStage = false;
	}

	DX11CmdQueue::DX11CmdQueue()
	{

	}
	DX11CmdQueue::~DX11CmdQueue()
	{
		if (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.front()->Release();
			mIdleCmdlist.pop();
		}
	}
	void DX11CmdQueue::ClearIdleCmdlists()
	{
		if (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.front()->Release();
			mIdleCmdlist.pop();
		}
	}
	void DX11CmdQueue::Init(DX11GpuDevice* device)
	{
		mDefaultQueueFrequence = 1;
	}
	void DX11CmdQueue::ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type)
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		for (UINT i = 0; i < NumOfWait; i++)
		{
			ppWaitCmdlists[i]->mCommitFence->WaitToExpect();
		}

		for (UINT i = 0; i < NumOfExe; i++)
		{
			auto dx11Cmd = (DX11CommandList*)Cmdlist[i];
			dx11Cmd->Commit(mHardwareContext->mContext);
			this->IncreaseSignal(dx11Cmd->mCommitFence, type);
		}
	}
	ICommandList* DX11CmdQueue::GetIdleCmdlist()
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		if (mIdleCmdlist.empty())
		{
			mIdleCmdlist.push(mDevice->CreateCommandList());
		}
		auto result = mIdleCmdlist.front();
		mIdleCmdlist.pop();
		return result;
	}
	void DX11CmdQueue::ReleaseIdleCmdlist(ICommandList* cmd)
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		mIdleCmdlist.push(cmd);
		return;
	}
	UINT64 DX11CmdQueue::Flush(EQueueType type)
	{
		if (type == EQueueType::QU_Unknown)
			return 0;
		mHardwareContext->mContext->Flush();
		return 0;
	}
}

NS_END