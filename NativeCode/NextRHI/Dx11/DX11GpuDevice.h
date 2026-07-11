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
		virtual void TryFinalizeDevice(IGpuSystem* pGpuSystem) override;
		virtual IBuffer* CreateBuffer(const FBufferDesc* desc, const char* file, int line) override;
		virtual ITexture* CreateTexture(const FTextureDesc* desc, const char* file, int line) override;
		virtual ITexture* CreateTexture(void* pSharedObject, const char* file, int line) override;
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

		virtual IGraphicDraw* CreateGraphicDraw(const char* file, int line) override;

		virtual IGpuScope* CreateGpuScope(const char* file, int line) override;

		virtual void SetBreakOnID(int id, bool open) override;

		virtual void TickPostEvents() override;
		void OnDeviceRemoved();
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

		AutoRef<DX11CommandList>		mPostCmdList;
	};

	class DX11CmdQueue : public ICmdQueue
	{
	public:
		void Init(DX11GpuDevice* device);
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) override;
		virtual ICommandList* GetIdleCmdlist() override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) override;
		virtual UINT64 Flush(EQueueType type) override;
		virtual void WaitFence(IFence* fence, UINT64 value, EQueueType type) override;
	public:
		DX11CmdQueue();
		~DX11CmdQueue();
		void ClearIdleCmdlists();
		DX11GpuDevice*					mDevice = nullptr;
		VCritical						mImmCmdListLocker;
		AutoRef<DX11CommandList>		mHardwareContext;
		std::queue<AutoRef<ICommandList>>		mIdleCmdlist;
	};
}

NS_END