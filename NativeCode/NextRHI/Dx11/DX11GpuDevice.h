#pragma once
#include "../NxGpuDevice.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11CommandList;
	class DX11CmdQueue;
	class DX11GpuSystem : public IGpuSystem
	{
	public:
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc* desc) override;
	};

	class DX11GpuDevice : public IGpuDevice
	{
	public:
		DX11GpuDevice();
		~DX11GpuDevice();
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
		virtual IShaderEffect* CreateShaderEffect() override;
		virtual IComputeEffect* CreateComputeEffect() override;
		virtual IFence* CreateFence(const FFenceDesc* desc, const char* name) override;
		virtual IEvent* CreateGpuEvent(const FEventDesc* desc, const char* name) override;
		virtual ICmdQueue* GetCmdQueue() override;
	private:
		void QueryDevice();
	public:
		IDXGIFactory*					mDXGIFactory = nullptr;
		ID3D11Device*					mDevice;
		ID3D11Device5*					mDevice5;
		D3D_FEATURE_LEVEL               mFeatureLevel;
		
		ID3DUserDefinedAnnotation*		mDefinedAnnotation = nullptr;

		AutoRef<DX11CmdQueue>			mCmdQueue;
	};

	class DX11CmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT num, ICommandList** ppCmdlist) override;
		virtual UINT64 SignalFence(IFence* fence, UINT64 value) override;
		virtual ICommandList* GetIdleCmdlist(EQueueCmdlist type) override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd, EQueueCmdlist type) override;
		virtual void Flush() override;
	public:
		DX11CmdQueue();
		~DX11CmdQueue();
		void ClearIdleCmdlists();
		DX11GpuDevice*					mDevice = nullptr;
		VCritical						mImmCmdListLocker;
		AutoRef<DX11CommandList>		mHardwareContext;
		AutoRef<DX11CommandList>		mFramePost;
		std::queue<ICommandList*>		mIdleCmdlist;
	};
}

NS_END