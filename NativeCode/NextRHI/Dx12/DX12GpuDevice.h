#pragma once
#include "../NxGpuDevice.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class ICmdRecorder;
	class DX12CbView;
	class DX12SrView;
	class DX12UaView;
	class DX12Sampler;
	class DX12RenderTargetView;
	class DX12DepthStencilView;
	class DX12CommandList;
	class DX12CmdQueue;
	struct DX12PagedHeap;
	struct DX12HeapAllocator;
	class DX12RootSignatureCache;
	
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

	class TR_CLASS()
		DX12GpuDevice : public IGpuDevice
	{
	public:
		DX12GpuDevice();
		~DX12GpuDevice();
		virtual bool InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc) override;
		virtual void TryFinalizeDevice(IGpuSystem* pGpuSystem) override;
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
		virtual IAccelerationStructure* CreateAccelerationStructure(const FAccelerationStructureDesc* desc, const char* file, int line) override;
		virtual IAStructureInstance* CreateAccelerationStructureInstance(const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure, const char* file, int line) override;
		virtual ITopAccelerationStructure* CreateTopAccelerationStructure(const FTopAccelerationStructureDesc* desc, const char* file, int line) override;

		virtual IGpuPipeline* CreatePipeline(const FGpuPipelineDesc* desc, const char* file, int line) override;
		virtual IGpuDrawState* CreateGpuDrawState(const char* file, int line) override;
		virtual IInputLayout* CreateInputLayout(FInputLayoutDesc* desc, const char* file, int line) override;
		virtual ICommandList* CreateCommandList(const char* file, int line) override;
		virtual IShader* CreateShader(FShaderDesc* desc, const char* file, int line) override;
		virtual IGraphicsEffect* CreateShaderEffect(const char* file, int line) override;
		virtual IComputeEffect* CreateComputeEffect(const char* file, int line) override;
		virtual IRayTracingEffect* CreateRayTracingEffect(const char* file, int line) override;
		virtual IFence* CreateFence(const FFenceDesc* desc, const char* name, const char* file, int line) override;
		virtual IEvent* CreateGpuEvent(const FEventDesc* desc, const char* name, const char* file, int line) override;
		virtual ICmdQueue* GetCmdQueue() override;

		virtual IGraphicDraw* CreateGraphicDraw(const char* file, int line) override;
		virtual IComputeDraw* CreateComputeDraw(const char* file, int line) override;
		virtual IRayTracingDraw* CreateRayTracingDraw(const char* file, int line) override;
		virtual IGpuScope* CreateGpuScope(const char* file, int line) override;
		virtual FVertexArray* CreateVertexArray(const char* file, int line) override;

		virtual void SetBreakOnID(int id, bool open) override;
		virtual void ShowDeviceMessage(int id, bool show) override;
		virtual void TickPostEvents() override;

		typedef void FDeviceRemovedCallback();
		std::function<FDeviceRemovedCallback> mDeviceRemovedCallback;
		void OnDeviceRemoved();
		DX12CommandAllocatorManager* GetCommandAllocatorManager() {
			return mCmdAllocatorManager;
		}
		DX12PagedGpuMemAllocator* GetCBufferMemAllocator() {
			return mCBufferMemAllocator;
		}
		DX12DefaultGpuMemAllocator* GetDefaultBufferMemAllocator() {
			return mDefaultBufferMemAllocator;
		}
		DX12DefaultGpuMemAllocator* GetUploadBufferMemAllocator() {
			return mUploadBufferMemAllocator;
		}
		DX12DefaultGpuMemAllocator* GetUavBufferMemAllocator() {
			return mUavBufferMemAllocator;
		}
		DX12HeapAllocator* GetRtvAllocator() {
			return mRtvAllocator;
		}
		DX12HeapAllocator* GetDsvAllocator() {
			return mDsvAllocator;
		}
		DX12HeapAllocator* GetSamplerAllocator() {
			return mSamplerAllocator;
		}
		DX12HeapAllocator* GetCbvSrvUavAllocator() {
			return mCbvSrvUavAllocator;
		}
		DX12HeapAllocatorManager* GetDescriptorSetAllocator() {
			return mDescriptorSetAllocator;
		}
	private: 
		void QueryDevice();
	public:
		TWeakRefHandle<DX12GpuSystem>	mGpuSystem;
		AutoRef<ID3D12Device>			mDevice;
		AutoRef<ID3D12Device5>			mLastDevice;
		AutoRef<ID3D12DebugDevice>		mDebugDevice;
		AutoRef<ID3D12DebugDevice1>		mDebugDevice1;
		D3D_FEATURE_LEVEL               mFeatureLevel;
		
		AutoRef<ID3D12InfoQueue>		mDebugInfoQueue;
		VCritical						mDredLocker;
		AutoRef<ID3D12DeviceRemovedExtendedDataSettings1>	mDredSettings;
		AutoRef<DX12CmdQueue>			mCmdQueue;
		
		AutoRef<DX12RootSignatureCache> mRootSignatureCache;
		AutoRef<DX12CommandAllocatorManager>	mCmdAllocatorManager;
		AutoRef<DX12PagedGpuMemAllocator>		mCBufferMemAllocator;
		AutoRef<DX12DefaultGpuMemAllocator>		mDefaultBufferMemAllocator;
		AutoRef<DX12DefaultGpuMemAllocator>		mUploadBufferMemAllocator;
		AutoRef<DX12DefaultGpuMemAllocator>		mUavBufferMemAllocator;

		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDraw;
		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDrawIndex;
		AutoRef<ID3D12CommandSignature>		CmdSigForIndirectDispatch;

		AutoRef<DX12HeapAllocator>	mRtvAllocator;
		AutoRef<DX12HeapAllocator>	mDsvAllocator;
		AutoRef<DX12HeapAllocator>	mSamplerAllocator;
		AutoRef<DX12HeapAllocator>	mCbvSrvUavAllocator;
		AutoRef<DX12HeapAllocatorManager>	mDescriptorSetAllocator;

		AutoRef<DX12CbView>	mNullCBV;
		AutoRef<DX12SrView>	mNullSRV;
		AutoRef<DX12UaView>	mNullUAV;
		AutoRef<DX12Sampler>	mNullSampler;
		AutoRef<DX12RenderTargetView>	mNullRTV;
		AutoRef<DX12DepthStencilView>	mNullDSV;

		AutoRef<DX12CommandList>			mPostCmdList;
		AutoRef<ICmdRecorder>				mPostCmdRecorder;
		std::vector<D3D12_MESSAGE_ID>		mDenyMessages;
	};

	class DX12CmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) override;
		virtual ICommandList* GetIdleCmdlist() override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) override;
		virtual UINT64 Flush(EQueueType type) override;
		virtual void WaitFence(IFence* fence, UINT64 value, EQueueType type) override;
	public:
		DX12CmdQueue();
		~DX12CmdQueue();
		void Init(DX12GpuDevice* device);
		void ClearIdleCmdlists();
		void TryRecycle();

		virtual void BeginEvent(const char* info, DWORD color = 0) override;
		virtual void EndEvent(const char* info) override;

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