#pragma once
#include "../NxGpuDevice.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12CommandList;
	class DX12CmdQueue;
	class DX12GpuSystem : public IGpuSystem
	{
	public:
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc* desc) override;
	};

	class DX12GpuDevice : public IGpuDevice
	{
	public:
		DX12GpuDevice();
		~DX12GpuDevice();
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

		virtual void TickPostEvents() override;
	private: 
		void QueryDevice();
	public:
		IDXGIFactory4*					mDXGIFactory = nullptr;
		ID3D12Device*					mDevice;
		D3D_FEATURE_LEVEL               mFeatureLevel;
		
		AutoRef<ID3D12Debug>			mDebugLayer;
		AutoRef<ID3D12InfoQueue>		mDebugInfoQueue;
		AutoRef<DX12CmdQueue>			mCmdQueue;
		AutoRef<DX12AllocHeapManager>	mRtvHeapManager;
		AutoRef<DX12AllocHeapManager>	mDsvHeapManager;
		AutoRef<DX12AllocHeapManager>	mSamplerAllocHeapManager;
		AutoRef<DX12AllocHeapManager>	mSrvAllocHeapManager;

		AutoRef<DX12TableHeapManager>		mSrvTableHeapManager;
		AutoRef<DX12TableHeapManager>		mSamplerTableHeapManager;

		AutoRef<DX12CommandAllocatorManager>	mCmdAllocatorManager;
		AutoRef<DX12GpuPooledMemAllocator>		mCBufferMemAllocator;
		AutoRef<DX12GpuDefaultMemAllocator>		mDefaultBufferMemAllocator;
		AutoRef<DX12GpuDefaultMemAllocator>		mUploadBufferMemAllocator;
		AutoRef<DX12GpuDefaultMemAllocator>		mUavBufferMemAllocator;

		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDrawIndex;
		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDispatch;
	};

	class DX12CmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT num, ICommandList** ppCmdlist) override;
		virtual UINT64 SignalFence(IFence* fence, UINT64 value) override;
		virtual ICommandList* GetIdleCmdlist(EQueueCmdlist type) override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd, EQueueCmdlist type) override;
		virtual void Flush() override;
	public:
		DX12CmdQueue();
		~DX12CmdQueue();
		void ClearIdleCmdlists();
		void TryRecycle();
		DX12GpuDevice*					mDevice = nullptr;
		VCritical						mImmCmdListLocker;
		AutoRef<DX12CommandList>		mFramePost;
		std::queue<AutoRef<ICommandList>>	mIdleCmdlist;
		struct FWaitRecycle 
		{
			UINT64						WaitFenceValue = 0;
			AutoRef<ICommandList>		CmdList;
		};
		std::vector<FWaitRecycle>		mWaitRecycleCmdlists;

		AutoRef<ID3D12CommandQueue>		mCmdQueue;
	};
}

NS_END