#pragma once
#include "../IRenderContext.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderSystem;
class IVKRenderPipelineManager;
class IVKCommandList;
class IVKShaderResourceView;
class IVKConstantBuffer;
class IVKSamplerState;
class IVKRenderContext;

class IVKDescriptorPool : public VIUnknown
{
public:
	VSLLock							mLocker;
	VkDescriptorPool				mDescriptorPool;
	UINT							mNumSet;
	VkDescriptorPoolSize			mPoolSize[5];
public:
	bool InitPool(IVKRenderContext* rc, UINT setCount, UINT ubCount, UINT texCount, UINT samplerCount, UINT sbCount, UINT stbCount);
	struct FDescriptorAlloc
	{
		FDescriptorAlloc()
		{
			UniformNum = 0;
			TextureNum = 0;
			SamplerNum = 0;
			StorageNum = 0;
			StorageTexelNum = 0;
		}
		UINT UniformNum;
		UINT TextureNum;
		UINT SamplerNum;
		UINT StorageNum;
		UINT StorageTexelNum;
	};
	VkDescriptorPool AllocDescriptor(const FDescriptorAlloc& arg);
	VkDescriptorPool FreeDescriptor(const FDescriptorAlloc& arg);
};

class IVKRenderContext : public IRenderContext
{
public:
	IVKRenderContext();
	~IVKRenderContext();

	virtual ERHIType GetRHIType() override
	{
		return RHT_VULKAN;
	}

	virtual ICommandList* GetImmCommandList() override;
	virtual void FlushImmContext() override;

	virtual ISwapChain* CreateSwapChain(const ISwapChainDesc* desc) override;
	virtual ICommandList* CreateCommandList(const ICommandListDesc* desc) override;
	
	virtual IDrawCall* CreateDrawCall() override;
	virtual IComputeDrawcall* CreateComputeDrawcall() override;

	virtual IRenderPipeline* CreateRenderPipeline(const IRenderPipelineDesc* desc) override;

	virtual IVertexBuffer* CreateVertexBuffer(const IVertexBufferDesc* desc) override;
	virtual IIndexBuffer* CreateIndexBuffer(const IIndexBufferDesc* desc) override;
	virtual IInputLayout* CreateInputLayout(const IInputLayoutDesc* desc) override;
	virtual IGeometryMesh* CreateGeometryMesh() override;

	virtual IIndexBuffer* CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer) override;
	virtual IVertexBuffer* CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer) override;
	
	virtual IRenderPass* CreateRenderPass(const IRenderPassDesc* desc) override;
	virtual IFrameBuffers* CreateFrameBuffers(const IFrameBuffersDesc* desc) override;
	virtual IRenderTargetView* CreateRenderTargetView(const IRenderTargetViewDesc* desc) override;
	virtual IDepthStencilView* CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc) override;
	virtual ITexture2D* CreateTexture2D(const ITexture2DDesc* desc) override;
	virtual IShaderResourceView* CreateShaderResourceView(const IShaderResourceViewDesc* desc) override;
	virtual IGpuBuffer* CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData) override;
	virtual IUnorderedAccessView* CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc) override;
	virtual IShaderResourceView* LoadShaderResourceView(const char* file) override;
	virtual ISamplerState* CreateSamplerState(const ISamplerStateDesc* desc) override;
	virtual IRasterizerState* CreateRasterizerState(const IRasterizerStateDesc* desc) override;
	virtual IDepthStencilState* CreateDepthStencilState(const IDepthStencilStateDesc* desc) override;
	virtual IBlendState* CreateBlendState(const IBlendStateDesc* desc) override;
	//shader
	virtual IShaderProgram* CreateShaderProgram(const IShaderProgramDesc* desc) override;
	virtual IVertexShader* CreateVertexShader(const IShaderDesc* desc) override;
	virtual IPixelShader* CreatePixelShader(const IShaderDesc* desc) override;
	virtual IComputeShader* CreateComputeShader(const IShaderDesc* desc) override;

	virtual IConstantBuffer* CreateConstantBuffer(const IConstantBufferDesc* desc) override;

	virtual IFence* CreateFence() override;
	virtual ISemaphore* CreateGpuSemaphore() override;
public:
	VkFormat FindSupportedFormat(const std::vector<VkFormat>& candidates, VkImageTiling tiling, VkFormatFeatureFlags features) {
		for (VkFormat format : candidates) {
			VkFormatProperties props;
			vkGetPhysicalDeviceFormatProperties(mPhysicalDevice, format, &props);

			if (tiling == VK_IMAGE_TILING_LINEAR && (props.linearTilingFeatures & features) == features) {
				return format;
			}
			else if (tiling == VK_IMAGE_TILING_OPTIMAL && (props.optimalTilingFeatures & features) == features) {
				return format;
			}
		}

		return VK_FORMAT_UNDEFINED;
	}
	static void* VKAPI_PTR vkAllocationFunction(
		void* pUserData,
		size_t                                      size,
		size_t                                      alignment,
		VkSystemAllocationScope                     allocationScope);

	static void VKAPI_PTR vkFreeFunction(
		void* pUserData,
		void* pMemory);

	static void VKAPI_PTR vkInternalAllocationNotification(
		void* pUserData,
		size_t                                      size,
		VkInternalAllocationType                    allocationType,
		VkSystemAllocationScope                     allocationScope);

	static void VKAPI_PTR vkInternalFreeNotification(
		void* pUserData,
		size_t                                      size,
		VkInternalAllocationType                    allocationType,
		VkSystemAllocationScope                     allocationScope);

	static void* VKAPI_PTR vkReallocationFunction(
		void* pUserData,
		void* pOriginal,
		size_t                                      size,
		size_t                                      alignment,
		VkSystemAllocationScope                     allocationScope);
public:
	TObjectHandle<IVKRenderSystem>	mRenderSystem;
	VkPhysicalDeviceProperties		mDeviceProperties;

	VkPhysicalDevice				mPhysicalDevice;
	VkDevice						mLogicalDevice;

	VkPhysicalDeviceFeatures		mDeviceFeatures;

	VkQueue							mGraphicsQueue;
	VkQueue							mGraphicsQueueForSingleTime;
	VkQueue							mPresentQueue;

	VkAllocationCallbacks			mAllocCallback;
	VkAllocationCallbacks* GetVkAllocCallBacks() {
		return nullptr;
		//return &mAllocCallback;
	}
	VkCommandPool					mCommandPool;
	VkCommandPool					mCommandPoolForSingleTime;
	AutoRef<IVKDescriptorPool>		mDescriptorPool;
	AutoRef<IVKCommandList>			mImmCmdList;

	bool Init(const IRenderContextDesc* desc, IVKRenderSystem* pSys, VkPhysicalDevice device, VkSurfaceKHR surface);

	AutoRef<IVKRenderPipelineManager>	mPipelineManager;
	VkDescriptorSetLayout			mCBDescSetLayout;
	void InitDescLayout(VkDevice device);

	AutoRef<IVKShaderResourceView>		mNullRsv;
	AutoRef<IVKConstantBuffer>			mNullCBuffer;
	AutoRef<IVKSamplerState>			mNullSampler;
};

NS_END