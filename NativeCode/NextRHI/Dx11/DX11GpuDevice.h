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
		~DX11GpuSystem();
		virtual bool InitGpuSystem(ERhiType type, const FGpuSystemDesc* desc) override;
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc* desc) override;
		virtual int GetNumOfGpuDevice() const override;
		virtual void GetDeviceDesc(int index, FGpuDeviceDesc* desc) const override;
	public:
		AutoRef<IDXGIFactory>					mDXGIFactory;
		std::vector<AutoRef<IDXGIAdapter>>		mGIAdapters;
	};

	class DX11GpuDevice : public IGpuDevice
	{
	public:
		DX11GpuDevice();
		~DX11GpuDevice();
		virtual bool InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc) override;
		virtual IBuffer* CreateBuffer(const FBufferDesc* desc) override;
		virtual ITexture* CreateTexture(const FTextureDesc* desc) override;
		virtual ITexture* CreateTexture(void* pSharedObject) override;
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

		virtual IGraphicDraw* CreateGraphicDraw() override;

		virtual IGpuScope* CreateGpuScope() override;

		virtual void SetBreakOnID(int id, bool open) override;

		virtual void TickPostEvents() override;
	private:
		void QueryDevice();
	public:
		AutoRef<IDXGIFactory>			mDXGIFactory;
		ID3D11Device*					mDevice;
		ID3D11Device5*					mDevice5;
		D3D_FEATURE_LEVEL               mFeatureLevel;
		
		AutoRef<ID3D11InfoQueue>		mDebugInfoQueue;
		ID3DUserDefinedAnnotation*		mDefinedAnnotation = nullptr;

		AutoRef<DX11CmdQueue>			mCmdQueue;
	};

	class DX11CmdQueue : public ICmdQueue
	{
	public:
		void Init(DX11GpuDevice* device);
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) override;
		virtual ICommandList* GetIdleCmdlist() override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) override;
		virtual UINT64 Flush(EQueueType type) override;
	public:
		DX11CmdQueue();
		~DX11CmdQueue();
		void ClearIdleCmdlists();
		DX11GpuDevice*					mDevice = nullptr;
		VCritical						mImmCmdListLocker;
		AutoRef<DX11CommandList>		mHardwareContext;
		std::queue<ICommandList*>		mIdleCmdlist;
	};
}

NS_END