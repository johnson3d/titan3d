#pragma once
#include "../NxGpuDevice.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullCommandList;
	class NullCmdQueue;
	class NullGpuSystem : public IGpuSystem
	{
	public:
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc* desc) override;
	};

	class NullGpuDevice : public IGpuDevice
	{
	public:
		NullGpuDevice();
		~NullGpuDevice();
		virtual bool InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc) override;
		virtual IBuffer* CreateBuffer(const FBufferDesc* desc, const char* file, int line) override;
		virtual ITexture* CreateTexture(const FTextureDesc* desc, const char* file, int line) override;
		virtual ICbView* CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc, const char* file, int line) override;
		virtual IVbView* CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc, const char* file, int line) override;
		virtual IIbView* CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc, const char* file, int line) override;
		virtual ISrView* CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc, const char* file, int line) override;
		virtual IUaView* CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc, const char* file, int line) override;
		virtual IRenderTargetView* CreateRTV(ITexture* pBuffer, const FRtvDesc* desc, const char* file, int line) override;
		virtual IDepthStencilView* CreateDSV(ITexture* pBuffer, const FDsvDesc* desc, const char* file, int line) override;
		virtual ISampler* CreateSampler(const FSamplerDesc* desc, const char* file, int line) override;
		virtual ISwapChain* CreateSwapChain(const FSwapChainDesc* desc, const char* file, int line) override;
		virtual IRenderPass* CreateRenderPass(const FRenderPassDesc* desc, const char* file, int line) override;
		virtual IFrameBuffers* CreateFrameBuffers(IRenderPass* rpass, const char* file, int line) override;
		virtual IAccelerationStructure* CreateAccelerationStructure(const FAccelerationStructureDesc* rpass, const char* file, int line) override;
		virtual IAStructureInstance* CreateAccelerationStructureInstance(const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure, const char* file, int line) override;
		virtual ITopAccelerationStructure* CreateTopAccelerationStructure(const FTopAccelerationStructureDesc* desc, const char* file, int line) override;

		virtual IGpuPipeline* CreatePipeline(const FGpuPipelineDesc* desc, const char* file, int line) override;
		virtual IGpuDrawState* CreateGpuDrawState(const char* file, int line) override;
		virtual IInputLayout* CreateInputLayout(FInputLayoutDesc* desc, const char* file, int line) override;
		virtual ICommandList* CreateCommandList(const char* file, int line) override;
		virtual IShader* CreateShader(FShaderDesc* desc, const char* file, int line) override;
		virtual IGraphicsEffect* CreateShaderEffect(const char* file, int line) override;
		virtual IComputeEffect* CreateComputeEffect(const char* file, int line) override;
		virtual IFence* CreateFence(const FFenceDesc* desc, const char* name, const char* file, int line) override;
		virtual IEvent* CreateGpuEvent(const FEventDesc* desc, const char* name, const char* file, int line) override;
		virtual ICmdQueue* GetCmdQueue() override;
		virtual IGpuScope* CreateGpuScope(const char* file, int line) override
		{
			return nullptr;
		}
		virtual void SetBreakOnID(int id, bool open) override
		{

		}
		virtual void TickPostEvents() override
		{
			if (mIsTryFinalize)
			{
				mIsFinalized = true;
			}
		}
	private:
		void QueryDevice();
	public:
		AutoRef<NullCmdQueue>			mCmdQueue;
	};

	class NullCmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) override;
		virtual ICommandList* GetIdleCmdlist() override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) override;
		virtual UINT64 Flush(EQueueType type) override;
		virtual void WaitFence(IFence* fence, UINT64 value, EQueueType type) override;
	public:
		NullCmdQueue();
		~NullCmdQueue();
		void ClearIdleCmdlists();
		NullGpuDevice*					mDevice = nullptr;
		VCritical						mImmCmdListLocker;
		std::queue<AutoRef<ICommandList>>	mIdleCmdlist;
	};
}

NS_END