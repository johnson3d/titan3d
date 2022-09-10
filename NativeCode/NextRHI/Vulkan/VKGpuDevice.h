#pragma once
#include "../NxGpuDevice.h"
#include "../NxEvent.h"
#include "VKPreHead.h"
#include "../../Base/allocator/PagedAllocator.h"

NS_BEGIN

namespace NxRHI
{
	class VKBuffer;
	class VKGpuDevice;
	class VKCommandList;
	class VKCmdQueue;
	class VKCmdBufferManager;
	class VKGpuSystem : public IGpuSystem
	{
	public:
		VKGpuSystem();
		~VKGpuSystem();
		virtual bool InitGpuSystem(ERhiType type, const FGpuSystemDesc* desc) override;
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc* desc) override;
		virtual int GetNumOfGpuDevice() const override{
			return (int)mHwDevices.size();
		}
		const VkExtensionProperties* FindExtension(const char* name) const
		{
			for (auto& i : mDeviceExtensions)
			{
				if (strcmp(i.extensionName, name) == 0)
					return &i;
			}
			return nullptr;
		}
		const VkLayerProperties* FindLayer(const char* name) const
		{
			for (auto& i : mLayerProperties)
			{
				if (strcmp(i.layerName, name) == 0)
					return &i;
			}
			return nullptr;
		}
	public:
		VkInstance					mVKInstance = nullptr;
		VkSurfaceKHR				mSurface = nullptr;
		std::vector<VkPhysicalDevice>	mHwDevices;

		std::vector<VkExtensionProperties>	mDeviceExtensions;
		std::vector<VkLayerProperties>	mLayerProperties;

		static PFN_vkCmdBeginDebugUtilsLabelEXT fn_vkCmdBeginDebugUtilsLabelEXT;
		static PFN_vkCmdEndDebugUtilsLabelEXT fn_vkCmdEndDebugUtilsLabelEXT;
		static PFN_vkDebugMarkerSetObjectNameEXT fn_vkDebugMarkerSetObjectNameEXT;
		static PFN_vkCmdDebugMarkerBeginEXT fn_vkCmdDebugMarkerBeginEXT;
		static PFN_vkCmdDebugMarkerEndEXT fn_vkCmdDebugMarkerEndEXT;
		static void SetVkObjectDebugName(VkDevice device, VkDebugReportObjectTypeEXT type, void* pObj, const char* name)
		{
			VkDebugMarkerObjectNameInfoEXT dbgNameInfo{};
			dbgNameInfo.sType = VK_STRUCTURE_TYPE_DEBUG_MARKER_OBJECT_NAME_INFO_EXT;
			dbgNameInfo.pObjectName = name;
			dbgNameInfo.objectType = type;
			dbgNameInfo.object = (uint64_t)pObj;
			VKGpuSystem::fn_vkDebugMarkerSetObjectNameEXT(device, &dbgNameInfo);
		}
	public:
		static VKAPI_ATTR VkBool32 VKAPI_CALL debugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData);
		vBOOL OnVKDebugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData);
	};

	class VKGpuDevice : public IGpuDevice
	{
	public:
		VKGpuDevice();
		~VKGpuDevice();
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

		virtual IGraphicDraw* CreateGraphicDraw() override;
		virtual IComputeDraw* CreateComputeDraw() override;

		virtual void TickPostEvents() override;
	private: 
		void QueryDevice();
	public:
		VkAllocationCallbacks			mAllocCallback{};
		VkAllocationCallbacks* GetVkAllocCallBacks() {
			return nullptr;
			//return &mAllocCallback;
		}
		VkInstance GetVkInstance() {
			return mGpuSystem.GetPtr()->mVKInstance;
		}
		UINT FindMemoryType(UINT typeFilter, VkMemoryPropertyFlags properties)
		{
			for (uint32_t i = 0; i < mMemProperties.memoryTypeCount; i++)
			{
				if ((typeFilter & (1 << i)) && (mMemProperties.memoryTypes[i].propertyFlags & properties) == properties)
				{
					return i;
				}
			}
			return 0xFFFFFFFF;
		}
	public:
		TObjectHandle<VKGpuSystem>		mGpuSystem;
		VkPhysicalDeviceProperties		mDeviceProperties{};
		VkPhysicalDeviceFeatures		mDeviceFeatures{};
		VkPhysicalDeviceMemoryProperties mMemProperties{};
		VkPhysicalDevice				mPhysicalDevice = nullptr;
		VkSurfaceKHR					mSurface = nullptr;
		VkDevice						mDevice = nullptr;
		VkCommandPool					mCommandPool = nullptr;
		VkDebugReportCallbackEXT		mDebugReportCallback = nullptr;
		std::vector<VkExtensionProperties>	mDeviceExtensions;

		AutoRef<VKCmdQueue>				mCmdQueue;
		AutoRef<VKCmdBufferManager>		mCmdAllocatorManager;

		AutoRef<VKGpuPooledMemAllocator>	mCBufferAllocator;
		AutoRef<VKGpuLinearMemAllocator>	mSsboAllocator;
		AutoRef<VKGpuLinearMemAllocator>	mVbIbAllocator;

		AutoRef<VKGpuLinearMemAllocator>	mUploadBufferAllocator;
		AutoRef<VKGpuLinearMemAllocator>	mReadBackAllocator;
		
		AutoRef<VKGpuDefaultMemAllocator>	mDefaultBufferAllocator;

		AutoRef<VKBuffer>					mNullVB;

	private:
		bool GetAllocatorInfo(VkBufferUsageFlags flags, VkMemoryPropertyFlags prop, UINT& typeIndex, UINT& alignment);
	};

	class VKCmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT num, ICommandList** ppCmdlist) override;
		virtual UINT64 SignalFence(IFence* fence, UINT64 value) override;
		virtual ICommandList* GetIdleCmdlist(EQueueCmdlist type) override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd, EQueueCmdlist type) override;
		virtual void Flush() override;

		bool GraphicsEqualPresentQueue() const {
			return mGraphicsQueueIndex == mPresentQueueIndex;
		}
	public:
		VKCmdQueue();
		~VKCmdQueue();
		void ClearIdleCmdlists();
		void TryRecycle();
		VKGpuDevice*					mDevice = nullptr;
		VSLLock							mGraphicsQueueLocker;
		VCritical						mImmCmdListLocker;
		AutoRef<VKCommandList>			mFramePost;
		std::queue<AutoRef<ICommandList>>	mIdleCmdlist;
		struct FWaitRecycle 
		{
			UINT64						WaitFenceValue = 0;
			AutoRef<ICommandList>		CmdList;
		};
		std::vector<FWaitRecycle>		mWaitRecycleCmdlists;

		UINT							mGraphicsQueueIndex = -1;
		UINT							mPresentQueueIndex = -1;
		VkQueue							mGraphicsQueue = nullptr;
		VkQueue							mPresentQueue = nullptr;
	};
}

NS_END