#include "NullGpuDevice.h"
#include "NullCommandList.h"
#include "NullShader.h"
#include "NullBuffer.h"
#include "NullGpuState.h"
#include "NullEvent.h"
#include "NullInputAssembly.h"
#include "NullFrameBuffers.h"
#include "../NxEffect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	IGpuDevice* NullGpuSystem::CreateDevice(const FGpuDeviceDesc* desc)
	{
		auto result = new NullGpuDevice();
		result->InitDevice(this, desc);
		return result;
	}
	NullGpuDevice::NullGpuDevice()
	{
		//mFeatureLevel = 0;12
	}
	NullGpuDevice::~NullGpuDevice()
	{
	}
	ICmdQueue* NullGpuDevice::GetCmdQueue()
	{
		return mCmdQueue;
	}
	bool NullGpuDevice::InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc)
	{
		return true;
	}
	void NullGpuDevice::QueryDevice()
	{
		
	}
	IBuffer* NullGpuDevice::CreateBuffer(const FBufferDesc* desc)
	{
		auto result = new NullBuffer();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITexture* NullGpuDevice::CreateTexture(const FTextureDesc* desc)
	{
		auto result = new NullTexture();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICbView* NullGpuDevice::CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc)
	{
		auto result = new NullCbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IVbView* NullGpuDevice::CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc)
	{
		auto result = new NullVbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IIbView* NullGpuDevice::CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc)
	{
		auto result = new NullIbView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISrView* NullGpuDevice::CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc)
	{
		auto result = new NullSrView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IUaView* NullGpuDevice::CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc)
	{
		auto result = new NullUaView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderTargetView* NullGpuDevice::CreateRTV(ITexture* pBuffer, const FRtvDesc* desc)
	{
		auto result = new NullRenderTargetView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IDepthStencilView* NullGpuDevice::CreateDSV(ITexture* pBuffer, const FDsvDesc* desc)
	{
		auto result = new NullDepthStencilView();
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISampler* NullGpuDevice::CreateSampler(const FSamplerDesc* desc)
	{
		auto result = new NullSampler();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISwapChain* NullGpuDevice::CreateSwapChain(const FSwapChainDesc* desc)
	{
		auto result = new NullSwapChain();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderPass* NullGpuDevice::CreateRenderPass(const FRenderPassDesc* desc)
	{
		auto result = new IRenderPass();
		result->Desc = *desc;
		return result;
	}
	IFrameBuffers* NullGpuDevice::CreateFrameBuffers(IRenderPass* rpass)
	{
		auto result = new NullFrameBuffers();
		result->mRenderPass = rpass;
		return result;
	}
	IGpuPipeline* NullGpuDevice::CreatePipeline(const FGpuPipelineDesc* desc)
	{
		auto result = new NullGpuPipeline();
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGpuDrawState* NullGpuDevice::CreateGpuDrawState()
	{
		return new IGpuDrawState();
	}
	IInputLayout* NullGpuDevice::CreateInputLayout(FInputLayoutDesc* desc)
	{
		auto result = new NullInputLayout();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICommandList* NullGpuDevice::CreateCommandList()
	{
		auto result = new NullCommandList();
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IShader* NullGpuDevice::CreateShader(FShaderDesc* desc)
	{
		auto result = new NullShader();
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IShaderEffect* NullGpuDevice::CreateShaderEffect()
	{
		return new IShaderEffect();
	}
	IComputeEffect* NullGpuDevice::CreateComputeEffect()
	{
		return new IComputeEffect();
	}
	IFence* NullGpuDevice::CreateFence(const FFenceDesc* desc, const char* name)
	{
		auto result = new NullFence();
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IEvent* NullGpuDevice::CreateGpuEvent(const FEventDesc* desc, const char* name)
	{
		auto result = new NullEvent(name);
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}

	NullCmdQueue::NullCmdQueue()
	{

	}
	NullCmdQueue::~NullCmdQueue()
	{
		
	}
	void NullCmdQueue::ClearIdleCmdlists()
	{
		
	}
	void NullCmdQueue::ExecuteCommandList(UINT num, ICommandList** ppCmdlist)
	{
		
	}
	UINT64 NullCmdQueue::SignalFence(IFence* fence, UINT64 value)
	{
		return value;
	}
	ICommandList* NullCmdQueue::GetIdleCmdlist(EQueueCmdlist type)
	{
		return nullptr;
	}
	void NullCmdQueue::ReleaseIdleCmdlist(ICommandList* cmd, EQueueCmdlist type)
	{
		
	}
	void NullCmdQueue::Flush()
	{

	}
}

NS_END