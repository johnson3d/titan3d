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
		virtual IBuffer* CreateBuffer(const FBufferDesc* desc) override;
		virtual ITexture* CreateTexture(const FTextureDesc* desc) override;
		virtual ICbView* CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc) override;
		virtual IVbView* CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc) override;
		virtual IIbView* CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc) override;
		virtual ISrView* CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc) override;
		virtual IUaView* CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc) override;
		virtual IRenderTargetView* CreateRTV(ITexture* pBuffer, const FRtvDesc* desc) override;
		virtual IDepthStencilView* CreateDSV(ITexture* pBuffer, const FDsvDesc* desc) override;
		virtual ISampler* CreateSampler(const FSamplerDesc* desc) override;
		virtual ISwapChain* CreateSwapChain(const FSwapChainDesc* desc) override;
		virtual IRenderPass* CreateRenderPass(const FRenderPassDesc* desc) override;
		virtual IFrameBuffers* CreateFrameBuffers(IRenderPass* rpass) override;

		virtual IGpuPipeline* CreatePipeline(const FGpuPipelineDesc* desc) override;
		virtual IGpuDrawState* CreateGpuDrawState() override;
		virtual IInputLayout* CreateInputLayout(FInputLayoutDesc* desc) override;
		virtual ICommandList* CreateCommandList() override;
		virtual IShader* CreateShader(FShaderDesc* desc) override;
		virtual IGraphicsEffect* CreateShaderEffect() override;
		virtual IComputeEffect* CreateComputeEffect() override;
		virtual IFence* CreateFence(const FFenceDesc* desc, const char* name) override;
		virtual IEvent* CreateGpuEvent(const FEventDesc* desc, const char* name) override;
		virtual ICmdQueue* GetCmdQueue() override;
		virtual IGpuScope* CreateGpuScope() override
		{
			return nullptr;
		}
		virtual void SetBreakOnID(int id, bool open) override
		{

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
	public:
		NullCmdQueue();
		~NullCmdQueue();
		void ClearIdleCmdlists();
		NullGpuDevice*					mDevice = nullptr;
		VCritical						mImmCmdListLocker;
		std::queue<ICommandList*>		mIdleCmdlist;
	};
}

NS_END