#pragma once
#include "../NxGpuDevice.h"
#include "../NxEvent.h"
#include "VKPreHead.h"
#include "../../Base/allocator/PagedAllocator.h"

NS_BEGIN

namespace NxRHI
{
	class VKBuffer;
	class VKSrView;
	class VKSampler;
	class VKGpuDevice;
	class VKCommandList;
	class VKCmdQueue;
	class VKCmdBufferManager;
	class VKBinaryFence;
	class VKFrameBufferCache;

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
		virtual void GetDeviceDesc(int index, FGpuDeviceDesc* desc) const override;
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
		VkSurfaceKHR				mSurface = (VkSurfaceKHR)nullptr;
		std::vector<VkPhysicalDevice>	mHwDevices;

		std::vector<VkExtensionProperties>	mDeviceExtensions;
		std::vector<VkLayerProperties>	mLayerProperties;

		#define DefineVKFunctionPtr(name) static PFN_##name name;
		DefineVKFunctionPtr(vkSetDebugUtilsObjectNameEXT);
		DefineVKFunctionPtr(vkCmdBeginDebugUtilsLabelEXT);
		DefineVKFunctionPtr(vkCmdEndDebugUtilsLabelEXT);
		DefineVKFunctionPtr(vkDebugMarkerSetObjectNameEXT);
		DefineVKFunctionPtr(vkCmdDebugMarkerBeginEXT);
		DefineVKFunctionPtr(vkCmdDebugMarkerEndEXT);
		DefineVKFunctionPtr(vkQueueSubmit2);
		DefineVKFunctionPtr(vkGetSemaphoreCounterValue);
		DefineVKFunctionPtr(vkSignalSemaphore);
		DefineVKFunctionPtr(vkWaitSemaphores);

		static VkDebugReportObjectTypeEXT VKObjectTypeToDebugReportObjectType(VkObjectType type)
		{
			switch (type)
			{
			case VkObjectType::VK_OBJECT_TYPE_BUFFER:
				return VkDebugReportObjectTypeEXT::VK_DEBUG_REPORT_OBJECT_TYPE_BUFFER_EXT;
			case VkObjectType::VK_OBJECT_TYPE_IMAGE:
				return VkDebugReportObjectTypeEXT::VK_DEBUG_REPORT_OBJECT_TYPE_IMAGE_EXT;
			case VkObjectType::VK_OBJECT_TYPE_BUFFER_VIEW:
				return VK_DEBUG_REPORT_OBJECT_TYPE_BUFFER_VIEW_EXT;
			case VkObjectType::VK_OBJECT_TYPE_IMAGE_VIEW:
				return VK_DEBUG_REPORT_OBJECT_TYPE_IMAGE_VIEW_EXT;
			case VkObjectType::VK_OBJECT_TYPE_PIPELINE:
				return VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT;
			case VkObjectType::VK_OBJECT_TYPE_SEMAPHORE:
				return VK_DEBUG_REPORT_OBJECT_TYPE_SEMAPHORE_EXT;
			default:
				ASSERT(false);
				break;
			}
			return VkDebugReportObjectTypeEXT::VK_DEBUG_REPORT_OBJECT_TYPE_UNKNOWN_EXT;
		}
		static void SetVkObjectDebugName(VkDevice device, VkObjectType type, void* pObj, const char* name)
		{
			if (VKGpuSystem::vkSetDebugUtilsObjectNameEXT != nullptr)
			{
				VkDebugUtilsObjectNameInfoEXT dbgNameInfo{};
				dbgNameInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_OBJECT_NAME_INFO_EXT;
				dbgNameInfo.pObjectName = name;
				dbgNameInfo.objectType = type;
				dbgNameInfo.objectHandle = (uint64_t)pObj;
			}
			else if (VKGpuSystem::vkDebugMarkerSetObjectNameEXT != nullptr)
			{
				VkDebugMarkerObjectNameInfoEXT dbgNameInfo{};
				dbgNameInfo.sType = VK_STRUCTURE_TYPE_DEBUG_MARKER_OBJECT_NAME_INFO_EXT;
				dbgNameInfo.pObjectName = name;
				dbgNameInfo.objectType = VKObjectTypeToDebugReportObjectType(type);
				dbgNameInfo.object = (uint64_t)pObj;
				VKGpuSystem::vkDebugMarkerSetObjectNameEXT(device, &dbgNameInfo);
			}
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

		virtual IGraphicDraw* CreateGraphicDraw(const char* file, int line) override;
		virtual IComputeDraw* CreateComputeDraw(const char* file, int line) override;
		virtual IGpuScope* CreateGpuScope(const char* file, int line) override;
		virtual void SetBreakOnID(int id, bool open) override;

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
		bool HasExtension(const char* name) const{
			for (auto& i : mDeviceExtensions)
			{
				if (strcmp(i.extensionName, name) == 0)
					return true;
			}
			return false;
		}
	public:
		TWeakRefHandle<VKGpuSystem>		mGpuSystem;
		VkPhysicalDeviceProperties		mDeviceProperties{};
		VkPhysicalDeviceFeatures		mDeviceFeatures{};
		VkPhysicalDeviceMemoryProperties mMemProperties{};
		VkPhysicalDevice				mPhysicalDevice = nullptr;
		VkSurfaceKHR					mSurface = (VkSurfaceKHR)nullptr;
		VkDevice						mDevice = nullptr;
		VkCommandPool					mCommandPool = (VkCommandPool)nullptr;
		VkDebugReportCallbackEXT		mDebugReportCallback = (VkDebugReportCallbackEXT)nullptr;
		std::vector<VkExtensionProperties>	mDeviceExtensions;

		AutoRef<VKCmdQueue>				mCmdQueue;
		AutoRef<VKCmdBufferManager>		mCmdAllocatorManager;

		AutoRef<VKGpuPooledMemAllocator>	mCBufferAllocator;

		AutoRef<VKGpuDefaultMemAllocator>	mDefaultBufferAllocator;

		AutoRef<VKFrameBufferCache>			mFrameBufferCache;

		AutoRef<VKBuffer>					mNullUBO;
		AutoRef<VKBuffer>					mNullSSBO;
		AutoRef<VKBuffer>					mNullVB;
		AutoRef<VKSrView>					mNullSampledImage;
		AutoRef<VKSampler>					mNullSampler;

		VmaAllocator						mVmaAllocator = nullptr;

		struct FVulkanExt
		{
			bool IsDynamicRendering = false;
			bool IsDynamicRenderingLocalRead = false;
			bool IsSynchronization2 = false;
		};
		FVulkanExt mVulkanExt;
	private:
		bool GetAllocatorInfo(VkBufferUsageFlags flags, VkMemoryPropertyFlags prop, UINT& typeIndex, UINT& alignment);
		void CreateNullObjects();
	};

	class VKCmdQueue : public ICmdQueue
	{
	public:
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) override;
		virtual ICommandList* GetIdleCmdlist() override;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) override;
		virtual UINT64 Flush(EQueueType type) override;
		virtual void WaitFence(IFence* fence, UINT64 value, EQueueType type) override;
		bool GraphicsEqualPresentQueue() const {
			return mGraphicsQueueIndex == mPresentQueueIndex;
		}
		VkResult SafeQueueSubmit(uint32_t submitCount, const VkSubmitInfo* pSubmits, VkFence fence, EQueueType type);
		VkResult SafeQueuePresentKHR(const VkPresentInfoKHR* pPresentInfo, EQueueType type);
	public:
		VKCmdQueue();
		~VKCmdQueue();
		void Init(VKGpuDevice* device);
		void ClearIdleCmdlists();
		void TryRecycle();
		VKGpuDevice*					mDevice = nullptr;
		VCritical						mQueueLocker;
		VCritical						mCmdLocker;
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

		AutoRef<IFence>					mFlushFence;
	};
}

NS_END