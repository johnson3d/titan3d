#include "NullGpuDevice.h"
#include "NullCommandList.h"
#include "NullShader.h"
#include "NullBuffer.h"
#include "NullGpuState.h"
#include "NullEvent.h"
#include "NullInputAssembly.h"
#include "NullFrameBuffers.h"
#include "../NxDescriptorSet.h"
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
		Desc = *desc;
		mCmdQueue = MakeWeakRef(new NullCmdQueue());
		mCmdQueue->mDevice = this;
		return true;
	}
	void NullGpuDevice::QueryDevice()
	{
		
	}
	IBuffer* NullGpuDevice::CreateBuffer(const FBufferDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullBuffer>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITexture* NullGpuDevice::CreateTexture(const FTextureDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullTexture>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICbView* NullGpuDevice::CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullCbView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IVbView* NullGpuDevice::CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullVbView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IIbView* NullGpuDevice::CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullIbView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISrView* NullGpuDevice::CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullSrView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IUaView* NullGpuDevice::CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullUaView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderTargetView* NullGpuDevice::CreateRTV(ITexture* pBuffer, const FRtvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullRenderTargetView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IDepthStencilView* NullGpuDevice::CreateDSV(ITexture* pBuffer, const FDsvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullDepthStencilView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISampler* NullGpuDevice::CreateSampler(const FSamplerDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullSampler>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISwapChain* NullGpuDevice::CreateSwapChain(const FSwapChainDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullSwapChain>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderPass* NullGpuDevice::CreateRenderPass(const FRenderPassDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<IRenderPass>(file ? file : __FILE__, line);
		result->Desc = *desc;
		result->SetViewInstanceLocations();
		return result;
	}
	IFrameBuffers* NullGpuDevice::CreateFrameBuffers(IRenderPass* rpass, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullFrameBuffers>(file ? file : __FILE__, line);
		result->mRenderPass = rpass;
		return result;
	}
	IAccelerationStructure* NullGpuDevice::CreateAccelerationStructure(const FAccelerationStructureDesc* rpass, const char* file, int line)
	{
		return nullptr;
	}
	IAStructureInstance* NullGpuDevice::CreateAccelerationStructureInstance(const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure, const char* file, int line)
	{
		return nullptr;
	}
	ITopAccelerationStructure* NullGpuDevice::CreateTopAccelerationStructure(const FTopAccelerationStructureDesc* desc, const char* file, int line)
	{
		return nullptr;
	}
	IGpuPipeline* NullGpuDevice::CreatePipeline(const FGpuPipelineDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullGpuPipeline>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGpuDrawState* NullGpuDevice::CreateGpuDrawState(const char* file, int line)
	{
		return NewObjectWithInfo<IGpuDrawState>(file ? file : __FILE__, line);
	}
	IInputLayout* NullGpuDevice::CreateInputLayout(FInputLayoutDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullInputLayout>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICommandList* NullGpuDevice::CreateCommandList(const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullCommandList>(file ? file : __FILE__, line);
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IShader* NullGpuDevice::CreateShader(FShaderDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullShader>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicsEffect* NullGpuDevice::CreateShaderEffect(const char* file, int line)
	{
		return NewObjectWithInfo<IGraphicsEffect>(file ? file : __FILE__, line);
	}
	IComputeEffect* NullGpuDevice::CreateComputeEffect(const char* file, int line)
	{
		return NewObjectWithInfo<IComputeEffect>(file ? file : __FILE__, line);
	}
	IFence* NullGpuDevice::CreateFence(const FFenceDesc* desc, const char* name, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullFence>(file ? file : __FILE__, line);
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IEvent* NullGpuDevice::CreateGpuEvent(const FEventDesc* desc, const char* name, const char* file, int line)
	{
		auto result = NewObjectWithInfo<NullEvent>(file ? file : __FILE__, line, name);
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
		while (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.pop();
		}
	}
	void NullCmdQueue::ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type)
	{

	}
	void NullCmdQueue::WaitFence(IFence* fence, UINT64 value, EQueueType type)
	{

	}
	ICommandList* NullCmdQueue::GetIdleCmdlist()
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		if (mIdleCmdlist.empty())
		{
			mIdleCmdlist.push(MakeWeakRef(mDevice->CreateCommandList(__FILE__, __LINE__)));
		}
		auto result = mIdleCmdlist.front();
		result->AddRef();
		mIdleCmdlist.pop();
		return result;
	}
	void NullCmdQueue::ReleaseIdleCmdlist(ICommandList* cmd)
	{
		VAutoVSLLock locker(mImmCmdListLocker);
		mIdleCmdlist.push(cmd);
		cmd->Release();
		return;
	}
	UINT64 NullCmdQueue::Flush(EQueueType type)
	{
		return 0;
	}
}

NS_END