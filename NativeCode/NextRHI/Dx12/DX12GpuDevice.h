#pragma once
#include "../NxGpuDevice.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12CommandList;
	class DX12CmdQueue;
	struct DX12DescriptorSetPagedObject;
	struct DX12DescriptorSetAllocator;
	
	class DX12GpuSystem : public IGpuSystem
	{
	public:
		virtual bool InitGpuSystem(ERhiType type, const FGpuSystemDesc* desc) override;
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc* desc) override;
		virtual int GetNumOfGpuDevice() const override;
		virtual void GetDeviceDesc(int index, FGpuDeviceDesc* desc) const override;
	public:
		AutoRef<IDXGIFactory>					mDXGIFactory;
		std::vector<AutoRef<IDXGIAdapter>>		mGIAdapters;
		AutoRef<ID3D12Debug1>					mDebugLayer;
	};

	class DX12GpuDevice : public IGpuDevice
	{
	public:
		DX12GpuDevice();
		~DX12GpuDevice();
		virtual bool InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc) override;
		virtual void TryFinalizeDevice(IGpuSystem* pGpuSystem) override;
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

		virtual IGraphicDraw* CreateGraphicDraw() override;
		virtual IComputeDraw* CreateComputeDraw() override;
		virtual IGpuScope* CreateGpuScope() override;

		virtual void SetBreakOnID(int id, bool open) override;
		virtual void TickPostEvents() override;

		typedef void FDeviceRemovedCallback();
		std::function<FDeviceRemovedCallback> mDeviceRemovedCallback;
		void OnDeviceRemoved();
	private: 
		void QueryDevice();
	public:
		TWeakRefHandle<DX12GpuSystem>	mGpuSystem;
		AutoRef<ID3D12Device>			mDevice;
		AutoRef<ID3D12DebugDevice>		mDebugDevice;
		AutoRef<ID3D12DebugDevice1>		mDebugDevice1;
		D3D_FEATURE_LEVEL               mFeatureLevel;
		
		AutoRef<ID3D12InfoQueue>		mDebugInfoQueue;
		VCritical						mDredLocker;
		AutoRef<ID3D12DeviceRemovedExtendedDataSettings>	mDredSettings;
		AutoRef<DX12CmdQueue>			mCmdQueue;
		
		AutoRef<DX12CommandAllocatorManager>	mCmdAllocatorManager;
		AutoRef<DX12GpuPooledMemAllocator>		mCBufferMemAllocator;
		AutoRef<DX12GpuDefaultMemAllocator>		mDefaultBufferMemAllocator;
		AutoRef<DX12GpuDefaultMemAllocator>		mUploadBufferMemAllocator;
		AutoRef<DX12GpuDefaultMemAllocator>		mUavBufferMemAllocator;

		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDrawIndex;
		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDispatch;

		AutoRef<DX12DescriptorSetAllocator>	mRtvAllocator;
		AutoRef<DX12DescriptorSetAllocator>	mDsvAllocator;
		AutoRef<DX12DescriptorSetAllocator>	mSamplerAllocator;
		AutoRef<DX12DescriptorSetAllocator>	mCbvSrvUavAllocator;
		AutoRef<DX12DescriptorAllocatorManager>	mDescriptorSetAllocator;

		AutoRef<DX12DescriptorSetPagedObject>	mNullCBV_SRV_UAV;
		AutoRef<DX12DescriptorSetPagedObject>	mNullSampler;
		AutoRef<DX12DescriptorSetPagedObject>	mNullRTV;
		AutoRef<DX12DescriptorSetPagedObject>	mNullDSV;
	};

	class DX12CmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) override;
		virtual ICommandList* GetIdleCmdlist() override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) override;
		virtual UINT64 Flush(EQueueType type) override;
	public:
		DX12CmdQueue();
		~DX12CmdQueue();
		void Init(DX12GpuDevice* device);
		void ClearIdleCmdlists();
		void TryRecycle();
		DX12GpuDevice*					mDevice = nullptr;
		VCritical						mQueueLocker;
		std::queue<AutoRef<ICommandList>>	mIdleCmdlist;
		struct FWaitRecycle 
		{
			UINT64						WaitFenceValue = 0;
			AutoRef<ICommandList>		CmdList;
		};
		std::vector<FWaitRecycle>		mWaitRecycleCmdlists;

		AutoRef<ID3D12CommandQueue>		mCmdQueue;
		AutoRef<IFence>					mFlushFence;
	};
}

NS_END