#include "VKGpuDevice.h"
#include "VKCommandList.h"
#include "VKShader.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "VKEvent.h"
#include "VKInputAssembly.h"
#include "VKFrameBuffers.h"
#include "VKEffect.h"
#include "VKDrawcall.h"
#include "VKDescriptorSet.h"
#include "../NxEffect.h"
#include "../../Base/thread/vfxthread.h"

#if defined(HasModule_GpuDump)
#include "../../Bricks/GpuDump/NvAftermath.h"
#endif

#define new VNEW

NS_BEGIN

namespace NxRHI
{
#define ImplVKFunctionPtr(name) PFN_##name VKGpuSystem::name = VK_NULL_HANDLE;
#define GetVKFunctionPtr(name) name = (PFN_##name)vkGetInstanceProcAddr(mVKInstance, #name);

	ImplVKFunctionPtr(vkSetDebugUtilsObjectNameEXT);
	ImplVKFunctionPtr(vkCmdBeginDebugUtilsLabelEXT);
	ImplVKFunctionPtr(vkCmdEndDebugUtilsLabelEXT);
	ImplVKFunctionPtr(vkQueueBeginDebugUtilsLabelEXT);
	ImplVKFunctionPtr(vkQueueEndDebugUtilsLabelEXT);

	ImplVKFunctionPtr(vkDebugMarkerSetObjectNameEXT);
	ImplVKFunctionPtr(vkCmdDebugMarkerBeginEXT);
	ImplVKFunctionPtr(vkCmdDebugMarkerEndEXT);
	
	ImplVKFunctionPtr(vkQueueSubmit2);
	ImplVKFunctionPtr(vkGetSemaphoreCounterValue);
	ImplVKFunctionPtr(vkSignalSemaphore);
	ImplVKFunctionPtr(vkWaitSemaphores);

	void populateDebugMessengerCreateInfo(VkDebugUtilsMessengerCreateInfoEXT& createInfo) 
	{
		createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
		createInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT |
			VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT |
			VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT |
			VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT;
		createInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
		createInfo.pfnUserCallback = &VKGpuSystem::debugCallback;
	}
	VkBool32 VKGpuSystem::debugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
	{
		auto pSys = (VKGpuSystem*)pUserData;
		return pSys->OnVKDebugCallback(messageSeverity, messageType, pCallbackData);
	}
	VKGpuSystem::VKGpuSystem()
	{

	}
	VKGpuSystem::~VKGpuSystem()
	{
		if (mVKInstance != nullptr)
		{
			vkDestroyInstance(mVKInstance, nullptr);
			mVKInstance = nullptr;
		}
	}
	bool isDebugSafe()
	{
#if PLATFORM_WIN
		// 最简单但可靠的检测：只有明确检测到调试工具时才启用
		static bool checked = false;
		static bool safe = false;

		if (!checked) {
			safe = (::GetModuleHandleA("renderdoc.dll") != nullptr) ||
				(::GetModuleHandleA("Nsight.Graphics.Hook.dll") != nullptr) ||
				(std::getenv("VK_INSTANCE_LAYERS") &&
					strstr(std::getenv("VK_INSTANCE_LAYERS"), "validation"));
			checked = true;
		}

		return safe;
#else
		return false;
#endif
	}
	vBOOL VKGpuSystem::OnVKDebugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData)
	{
		VFX_LTRACE(ELTT_Graphics, "Vulkan: %s\r\n", pCallbackData->pMessage);
		return FALSE;
	}
	bool VKGpuSystem::InitGpuSystem(ERhiType type, const FGpuSystemDesc* desc)
	{
		uint32_t apiVersion = 0;
		VkResult result = vkEnumerateInstanceVersion(&apiVersion);

		VkApplicationInfo appInfo = {};
		appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
		appInfo.pApplicationName = "Titan3D";
		appInfo.applicationVersion = VK_MAKE_VERSION(1, 0, 0);
		appInfo.pEngineName = "Titan3D";
		appInfo.engineVersion = VK_MAKE_VERSION(1, 0, 0);
		appInfo.apiVersion = VK_API_VERSION_1_3;//2080 only support vk 1.2

		uint32_t extensionCount = 0;
		vkEnumerateInstanceExtensionProperties(nullptr, &extensionCount, nullptr);
		mDeviceExtensions.resize(extensionCount);
		vkEnumerateInstanceExtensionProperties(nullptr, &extensionCount, mDeviceExtensions.data());

		UINT NumOfLayer = 0;
		vkEnumerateInstanceLayerProperties(&NumOfLayer, nullptr);
		mLayerProperties.resize(NumOfLayer);
		vkEnumerateInstanceLayerProperties(&NumOfLayer, &mLayerProperties[0]);

		VkInstanceCreateInfo createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
		createInfo.pApplicationInfo = &appInfo;

		std::vector<const char*> extensionNames;
		/*for (uint32_t i = 0; i < extensionCount; i++)
		{
			extensionNames.push_back(mDeviceExtensions[i].extensionName);
		}*/
		if (FindExtension(VK_EXT_DEBUG_REPORT_EXTENSION_NAME))
			extensionNames.push_back(VK_EXT_DEBUG_REPORT_EXTENSION_NAME);
		//debug utils is an instance extension(BeginEvent/EndEvent labels)
		if (FindExtension(VK_EXT_DEBUG_UTILS_EXTENSION_NAME))
			extensionNames.push_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
		if (FindExtension(VK_KHR_SURFACE_EXTENSION_NAME))
			extensionNames.push_back(VK_KHR_SURFACE_EXTENSION_NAME);
		if (FindExtension(VK_NV_RAY_TRACING_EXTENSION_NAME))
			extensionNames.push_back(VK_NV_RAY_TRACING_EXTENSION_NAME);
#if defined(PLATFORM_WIN)
		if (FindExtension(VK_KHR_WIN32_SURFACE_EXTENSION_NAME))
			extensionNames.push_back(VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
#elif defined(PLATFORM_DROID)
		if (FindExtension(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME))
			extensionNames.push_back(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME);
#endif
		
		

		VkValidationFeaturesEXT validationFeatures{};
		validationFeatures.sType = VK_STRUCTURE_TYPE_VALIDATION_FEATURES_EXT;
		VkValidationFeatureEnableEXT enableFeatures[] = { VK_VALIDATION_FEATURE_ENABLE_GPU_ASSISTED_EXT,
			VK_VALIDATION_FEATURE_ENABLE_GPU_ASSISTED_RESERVE_BINDING_SLOT_EXT ,
			//VK_VALIDATION_FEATURE_ENABLE_BEST_PRACTICES_EXT ,
			VK_VALIDATION_FEATURE_ENABLE_DEBUG_PRINTF_EXT ,
			VK_VALIDATION_FEATURE_ENABLE_SYNCHRONIZATION_VALIDATION_EXT };
		validationFeatures.enabledValidationFeatureCount = sizeof(enableFeatures) / sizeof(VkValidationFeatureEnableEXT);
		validationFeatures.pEnabledValidationFeatures = enableFeatures;

		VkDebugUtilsMessengerCreateInfoEXT debugCreateInfo{};
		debugCreateInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
		std::vector<const char*>	mValidationLayers;
		if (desc->UseRenderDoc)
		{
			auto pLayer = FindLayer("VK_LAYER_RENDERDOC_Capture");
			if (pLayer != nullptr)
			{
				//RenderDoc's layer should NEVER be activated manually. Do not include it in vkCreateInstance's instance layers.
				//Vulkan: RenderDoc's layer should NEVER be activated manually. Do not include it in vkCreateInstance's instance layers.

				//mValidationLayers.push_back("VK_LAYER_RENDERDOC_Capture");
			}
		}
		if (desc->CreateDebugLayer)
		{
			auto pLayer = FindLayer("VK_LAYER_KHRONOS_validation");
			if (pLayer != nullptr)
			{
				mValidationLayers.push_back("VK_LAYER_KHRONOS_validation");
			}

			populateDebugMessengerCreateInfo(debugCreateInfo);
			debugCreateInfo.pUserData = this;
			createInfo.pNext = &debugCreateInfo;
			debugCreateInfo.pNext = &validationFeatures;
		}

		createInfo.enabledExtensionCount = static_cast<uint32_t>(extensionNames.size());
		createInfo.ppEnabledExtensionNames = extensionNames.data();
		createInfo.enabledLayerCount = static_cast<uint32_t>(mValidationLayers.size());
		if (mValidationLayers.size() > 0)
			createInfo.ppEnabledLayerNames = mValidationLayers.data();

		if (vkCreateInstance(&createInfo, nullptr, &mVKInstance) != VK_SUCCESS) 
		{
			return false;
		}

		UINT						mDeviceNumber = 0;
		vkEnumeratePhysicalDevices(mVKInstance, &mDeviceNumber, nullptr);
		mHwDevices.resize(mDeviceNumber);
		vkEnumeratePhysicalDevices(mVKInstance, &mDeviceNumber, mHwDevices.data());

		//volkLoadInstance(mVKInstance);

		//vkCmdBeginDebugUtilsLabelEXT
		

		GetVKFunctionPtr(vkSetDebugUtilsObjectNameEXT);
		GetVKFunctionPtr(vkCmdBeginDebugUtilsLabelEXT);
		GetVKFunctionPtr(vkCmdEndDebugUtilsLabelEXT);
		GetVKFunctionPtr(vkQueueBeginDebugUtilsLabelEXT);
		GetVKFunctionPtr(vkQueueEndDebugUtilsLabelEXT);

		if (isDebugSafe())
		{
			GetVKFunctionPtr(vkDebugMarkerSetObjectNameEXT);
			GetVKFunctionPtr(vkCmdDebugMarkerBeginEXT);
			GetVKFunctionPtr(vkCmdDebugMarkerEndEXT);
		}
		
		GetVKFunctionPtr(vkQueueSubmit2);
		GetVKFunctionPtr(vkGetSemaphoreCounterValue);
		GetVKFunctionPtr(vkSignalSemaphore);
		GetVKFunctionPtr(vkWaitSemaphores);
		
#ifdef PLATFORM_WIN
		{
			VkWin32SurfaceCreateInfoKHR createInfo_surf = {};
			createInfo_surf.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
			createInfo_surf.pNext = nullptr;
			createInfo_surf.hinstance = nullptr;
			createInfo_surf.hwnd = (HWND)desc->WindowHandle; 
			vkCreateWin32SurfaceKHR(mVKInstance, &createInfo_surf, nullptr, &mSurface);
		}
#endif
		return true;
	}
	void VKGpuSystem::GetDeviceDesc(int index, FGpuDeviceDesc* desc) const
	{
		if (index < 0 || index >= (int)mHwDevices.size())
			return;
		VkPhysicalDeviceProperties dxdesc{};
		vkGetPhysicalDeviceProperties(mHwDevices[index], &dxdesc);
		desc->RhiType = ERhiType::RHI_VK;
		desc->VendorId = dxdesc.vendorID;
		desc->AdapterId = index;
		//desc->DedicatedVideoMemory = dxdesc.limits.memor dxdesc.DedicatedVideoMemory;
		strcpy(desc->Name, dxdesc.deviceName);
	}
	IGpuDevice* VKGpuSystem::CreateDevice(const FGpuDeviceDesc* desc)
	{
		auto result = new VKGpuDevice();
		result->InitDevice(this, desc);
		result->Desc.RhiType = ERhiType::RHI_VK;
		return result;
	}
	VKGpuDevice::VKGpuDevice()
	{
		
	}
	VKGpuDevice::~VKGpuDevice()
	{
		for (int i = 0; i < 5; i++)
		{
			this->TickPostEvents();
		}

		mNullUBO = nullptr;
		mNullSSBO = nullptr;
		mNullVB = nullptr;
		mNullSampledImage = nullptr;
		mNullSampler = nullptr;
		
		if (mCmdAllocatorManager != nullptr)
		{
			mCmdAllocatorManager->FinalCleanup();
			mCmdAllocatorManager = nullptr;
		}

		mCmdQueue->ClearIdleCmdlists();
		mCmdQueue = nullptr;
		
		if (mSurface != nullptr)
		{
			vkDestroySurfaceKHR(GetVkInstance(), mSurface, this->GetVkAllocCallBacks());
			mSurface = nullptr;
		}
		
		mCBufferAllocator = nullptr;
		mDefaultBufferAllocator = nullptr;
		mPipelineManager = nullptr;
		mFrameFence = nullptr;
		mDescriptorPoolManager = nullptr;
		mFrameBufferCache = nullptr;

		if (mVmaAllocator)
		{
			vmaDestroyAllocator(mVmaAllocator);
			mVmaAllocator = nullptr;
		}
		
		if (mDevice != nullptr)
		{
			vkDestroyDevice(mDevice, nullptr);
			mDevice = nullptr;
		}
		if (mDebugReportCallback != nullptr)
		{
			auto fn_vkDestroyDebugReportCallbackEXT = (PFN_vkDestroyDebugReportCallbackEXT)vkGetInstanceProcAddr(GetVkInstance(), "vkDestroyDebugReportCallbackEXT");
			if (fn_vkDestroyDebugReportCallbackEXT != nullptr)
			{
				fn_vkDestroyDebugReportCallbackEXT(GetVkInstance(), mDebugReportCallback, GetVkAllocCallBacks());
			}
			mDebugReportCallback = nullptr;
		}
	}
	ICmdQueue* VKGpuDevice::GetCmdQueue()
	{
		return mCmdQueue;
	}
	VkBool32 VKAPI_PTR OnVK_DebugReportCallbackEXT(
		VkDebugReportFlagsEXT                       flags,
		VkDebugReportObjectTypeEXT                  objectType,
		uint64_t                                    object,
		size_t                                      location,
		int32_t                                     messageCode,
		const char* pLayerPrefix,
		const char* pMessage,
		void* pUserData)
	{
		if (IGpuDevice::IsTryFinalize())
			return FALSE;
		const char* Mode = "Default";
		//auto device = (VKGpuDevice*)pUserData;
		if (flags & VK_DEBUG_REPORT_WARNING_BIT_EXT)
		{
			Mode = "Warning";
		}
		else if (flags & VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT)
		{
			Mode = "PerfWarning";
		}
		else if (flags & VK_DEBUG_REPORT_ERROR_BIT_EXT)
		{
			Mode = "Error";
		}
		else if (flags & VK_DEBUG_REPORT_DEBUG_BIT_EXT)
		{
			Mode = "Debug";
		}
		else if (flags & VK_DEBUG_REPORT_INFORMATION_BIT_EXT)
		{
			Mode = "Info";
		}
		if (strstr(pMessage, "VK_PIPELINE_STAGE_ALL_COMMANDS_BIT") != nullptr)
		{
			return FALSE;
		}
		auto device = (VKGpuDevice*)pUserData;
		if (device != nullptr && device->IsMessageHidden(messageCode))
		{
			return FALSE;
		}
		VFX_LTRACE(ELTT_Graphics, "Vk[%s]: %s\r\n", Mode, pMessage);
		if (device != nullptr && device->IsBreakOnID(messageCode))
		{
#if defined(PLATFORM_WIN)
			__debugbreak();
#else
			ASSERT(false);
#endif
		}
		return FALSE;
	}
	struct VkStructureHead
	{
		VkStructureType                     sType;
		const void* pNext;
	};
	void VKGpuDevice::SetBreakOnID(int id, bool open)
	{
		//align with DX12GpuDevice::SetBreakOnID(ID3D12InfoQueue::SetBreakOnID)
		if (open)
			mBreakOnIDs.insert(id);
		else
			mBreakOnIDs.erase(id);
	}
	void VKGpuDevice::ShowDeviceMessage(int id, bool show)
	{
		//align with DX12GpuDevice::ShowDeviceMessage(ID3D12InfoQueue message filter)
		if (show)
			mHiddenMessageIDs.erase(id);
		else
			mHiddenMessageIDs.insert(id);
	}
	void VKGpuDevice::OnDeviceRemoved()
	{
		//align with DX12GpuDevice::OnDeviceRemoved, only handle the first VK_ERROR_DEVICE_LOST
		if (mDeviceRemovedHandled.exchange(true))
			return;

		VFX_LTRACE(ELTT_Graphics, "Vulkan: VK_ERROR_DEVICE_LOST\r\n");
#if defined(HasModule_GpuDump)
		//official Aftermath device lost flow: wait for the crash dump before notifying the application
		GpuDump::NvAftermath::WaitDumpComplete();
#endif
		if (fn_vkGetDeviceFaultInfoEXT != nullptr)
		{
			VkDeviceFaultCountsEXT faultCounts{};
			faultCounts.sType = VK_STRUCTURE_TYPE_DEVICE_FAULT_COUNTS_EXT;
			if (fn_vkGetDeviceFaultInfoEXT(mDevice, &faultCounts, nullptr) == VK_SUCCESS)
			{
				std::vector<VkDeviceFaultAddressInfoEXT> addressInfos(faultCounts.addressInfoCount);
				std::vector<VkDeviceFaultVendorInfoEXT> vendorInfos(faultCounts.vendorInfoCount);
				faultCounts.vendorBinarySize = 0;
				VkDeviceFaultInfoEXT faultInfo{};
				faultInfo.sType = VK_STRUCTURE_TYPE_DEVICE_FAULT_INFO_EXT;
				faultInfo.pAddressInfos = addressInfos.empty() ? nullptr : addressInfos.data();
				faultInfo.pVendorInfos = vendorInfos.empty() ? nullptr : vendorInfos.data();
				if (fn_vkGetDeviceFaultInfoEXT(mDevice, &faultCounts, &faultInfo) == VK_SUCCESS)
				{
					VFX_LTRACE(ELTT_Graphics, "Vulkan DeviceFault: %s\r\n", faultInfo.description);
					for (UINT i = 0; i < faultCounts.addressInfoCount; i++)
					{
						VFX_LTRACE(ELTT_Graphics, "Vulkan DeviceFault Address[%u]: type=%d addr=0x%llx precision=0x%llx\r\n",
							i, (int)addressInfos[i].addressType, (unsigned long long)addressInfos[i].reportedAddress, (unsigned long long)addressInfos[i].addressPrecision);
					}
					for (UINT i = 0; i < faultCounts.vendorInfoCount; i++)
					{
						VFX_LTRACE(ELTT_Graphics, "Vulkan DeviceFault Vendor[%u]: %s code=0x%llx data=0x%llx\r\n",
							i, vendorInfos[i].description, (unsigned long long)vendorInfos[i].vendorFaultCode, (unsigned long long)vendorInfos[i].vendorFaultData);
					}
				}
			}
		}
		if (CoreSDK::OnGpuDeviceRemoved != nullptr)
		{
			CoreSDK::OnGpuDeviceRemoved(this);
		}
	}
	bool VKGpuDevice::InitDevice(IGpuSystem* pGpuSystem, const FGpuDeviceDesc* desc)
	{
		mDeviceThreadId = vfxThread::GetCurrentThreadId();
		auto pVkGpuSys = (VKGpuSystem*)pGpuSystem;

		Desc = *desc;
		mGpuSystem.FromObject(pGpuSystem);

		mCmdQueue = MakeWeakRef(new VKCmdQueue());
		mCmdQueue->mDevice = this;

		mPhysicalDevice = ((VKGpuSystem*)pGpuSystem)->mHwDevices[desc->AdapterId];
		mSurface = ((VKGpuSystem*)pGpuSystem)->mSurface;

		VkSurfaceFormatKHR SwapchainFormats[16];
		vkGetPhysicalDeviceSurfaceFormatsKHR(mPhysicalDevice, mSurface, &mCaps.NumOfSwapchainFormats, nullptr);
		if (mCaps.NumOfSwapchainFormats > 16)
		{
			ASSERT(false);
		}
		vkGetPhysicalDeviceSurfaceFormatsKHR(mPhysicalDevice, mSurface, &mCaps.NumOfSwapchainFormats, SwapchainFormats);
		for (UINT i = 0; i < mCaps.NumOfSwapchainFormats; i++)
		{
			mCaps.SwapchainFormats[i] = VKFormat2Format(SwapchainFormats[i].format);
		}
		//vkGetPhysicalDeviceFormatProperties(mPhysicalDevice, VkFormat::VK_FORMAT_UNDEFINED, VkFormatProperties)
		
		UINT graphicsFamily = -1, presentFamily = -1;

		{
			uint32_t queueFamilyCount = 0;
			vkGetPhysicalDeviceQueueFamilyProperties(mPhysicalDevice, &queueFamilyCount, nullptr);
			std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
			vkGetPhysicalDeviceQueueFamilyProperties(mPhysicalDevice, &queueFamilyCount, queueFamilies.data());
			for (UINT i = 0; i < (UINT)queueFamilies.size(); i++)
			{
				if (queueFamilies[i].queueFlags & VK_QUEUE_GRAPHICS_BIT)
				{
					graphicsFamily = i;
				}

				VkBool32 presentSupport = FALSE;
				vkGetPhysicalDeviceSurfaceSupportKHR(mPhysicalDevice, i, mSurface, &presentSupport);
				if (presentSupport)
				{
					presentFamily = i;
				}
				if (presentFamily != -1 && graphicsFamily != -1)
					break;
			}

			ASSERT(presentFamily != -1 && graphicsFamily != -1);
		}
		
		mCmdQueue->mGraphicsQueueIndex = graphicsFamily;
		mCmdQueue->mPresentQueueIndex = presentFamily;
		
		UINT deviceExtCount = 0;
		vkEnumerateDeviceExtensionProperties(mPhysicalDevice, nullptr, &deviceExtCount, nullptr);
		mDeviceExtensions.resize(deviceExtCount);
		vkEnumerateDeviceExtensionProperties(mPhysicalDevice, nullptr, &deviceExtCount, &mDeviceExtensions[0]);
		std::vector<const char*>	extensions;
		//align with DX12 DRED: device fault info on device lost
		bool bDeviceFault = false;
		{
			/*for (auto& i : mDeviceExtensions)
			{
				extensions.push_back(i.extensionName);
			}*/
			if (HasExtension(VK_KHR_16BIT_STORAGE_EXTENSION_NAME))
				extensions.push_back(VK_KHR_16BIT_STORAGE_EXTENSION_NAME);
			if (HasExtension(VK_KHR_SWAPCHAIN_EXTENSION_NAME))
				extensions.push_back(VK_KHR_SWAPCHAIN_EXTENSION_NAME);
			if (HasExtension(VK_KHR_16BIT_STORAGE_EXTENSION_NAME))
				extensions.push_back(VK_KHR_16BIT_STORAGE_EXTENSION_NAME);
			if (HasExtension(VK_KHR_SHADER_FLOAT16_INT8_EXTENSION_NAME))
				extensions.push_back(VK_KHR_SHADER_FLOAT16_INT8_EXTENSION_NAME);
			if (HasExtension(VK_KHR_TIMELINE_SEMAPHORE_EXTENSION_NAME))
				extensions.push_back(VK_KHR_TIMELINE_SEMAPHORE_EXTENSION_NAME);
			if (HasExtension(VK_EXT_DEBUG_MARKER_EXTENSION_NAME))
			{
				extensions.push_back(VK_EXT_DEBUG_MARKER_EXTENSION_NAME);
			}
			else
			{
				if (HasExtension(VK_EXT_DEBUG_UTILS_EXTENSION_NAME))
					extensions.push_back(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
			}
			if (HasExtension(VK_EXT_TOOLING_INFO_EXTENSION_NAME))
				extensions.push_back(VK_EXT_TOOLING_INFO_EXTENSION_NAME);
			
			if (HasExtension(VK_NV_DEVICE_DIAGNOSTIC_CHECKPOINTS_EXTENSION_NAME))	
				extensions.push_back(VK_NV_DEVICE_DIAGNOSTIC_CHECKPOINTS_EXTENSION_NAME);
			if (HasExtension(VK_NV_DEVICE_DIAGNOSTICS_CONFIG_EXTENSION_NAME))
				extensions.push_back(VK_NV_DEVICE_DIAGNOSTICS_CONFIG_EXTENSION_NAME);
			if (HasExtension(VK_NV_RAY_TRACING_EXTENSION_NAME))
				extensions.push_back(VK_NV_RAY_TRACING_EXTENSION_NAME);
			if (HasExtension(VK_KHR_SHADER_DRAW_PARAMETERS_EXTENSION_NAME))
				extensions.push_back(VK_KHR_SHADER_DRAW_PARAMETERS_EXTENSION_NAME);
			if (HasExtension("VK_NV_per_stage_descriptor_set"))
				extensions.push_back("VK_NV_per_stage_descriptor_set");
			if (HasExtension(VK_EXT_ROBUSTNESS_2_EXTENSION_NAME))
				extensions.push_back(VK_EXT_ROBUSTNESS_2_EXTENSION_NAME);
			if (HasExtension(VK_KHR_DYNAMIC_RENDERING_EXTENSION_NAME))
				extensions.push_back(VK_KHR_DYNAMIC_RENDERING_EXTENSION_NAME);
			if (HasExtension(VK_KHR_DYNAMIC_RENDERING_LOCAL_READ_EXTENSION_NAME))
				extensions.push_back(VK_KHR_DYNAMIC_RENDERING_LOCAL_READ_EXTENSION_NAME);
			//align with DX12: DXR(CheckFeatureSupport D3D12_FEATURE_D3D12_OPTIONS5)
			if (HasExtension(VK_KHR_ACCELERATION_STRUCTURE_EXTENSION_NAME) && 
				HasExtension(VK_KHR_RAY_QUERY_EXTENSION_NAME) && 
				HasExtension(VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME))
			{
				extensions.push_back(VK_KHR_ACCELERATION_STRUCTURE_EXTENSION_NAME);
				extensions.push_back(VK_KHR_RAY_QUERY_EXTENSION_NAME);
				extensions.push_back(VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME);
				mVulkanExt.IsRayQuery = true;
			}
			//align with DX12: MeshShader(CheckFeatureSupport D3D12_FEATURE_D3D12_OPTIONS7)
			if (HasExtension(VK_EXT_MESH_SHADER_EXTENSION_NAME))
			{
				extensions.push_back(VK_EXT_MESH_SHADER_EXTENSION_NAME);
				mVulkanExt.IsMeshShader = true;
			}
			//align with DX12 DRED: device fault info on device lost
			if (desc->IsGpuDred && HasExtension(VK_EXT_DEVICE_FAULT_EXTENSION_NAME))
			{
				extensions.push_back(VK_EXT_DEVICE_FAULT_EXTENSION_NAME);
				bDeviceFault = true;
			}
			
			//extensions.push_back(VK_GOOGLE_HLSL_FUNCTIONALITY1_EXTENSION_NAME);
			/*extensions.push_back(VK_KHR_SWAPCHAIN_EXTENSION_NAME);
			extensions.push_back(VK_KHR_16BIT_STORAGE_EXTENSION_NAME);
			extensions.push_back(VK_KHR_SHADER_FLOAT16_INT8_EXTENSION_NAME);
			extensions.push_back(VK_KHR_TIMELINE_SEMAPHORE_EXTENSION_NAME);*/
			//mDeviceExtensions.push_back(VK_GOOGLE_HLSL_FUNCTIONALITY1_EXTENSION_NAME);
			//mDeviceExtensions.push_back("SPV_GOOGLE_user_type");
		}

		vkGetPhysicalDeviceProperties(mPhysicalDevice, &mDeviceProperties);
		if (mDeviceProperties.apiVersion < VK_API_VERSION_1_2)
		{
			ASSERT(false);
		}

		VkPhysicalDeviceInheritedViewportScissorFeaturesNV dynScissorFeatures{};
		dynScissorFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_INHERITED_VIEWPORT_SCISSOR_FEATURES_NV;
		dynScissorFeatures.inheritedViewportScissor2D = VK_TRUE;

		VkPhysicalDeviceSynchronization2Features sync2Features = {};
		sync2Features.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_SYNCHRONIZATION_2_FEATURES;
		//sync2Features.synchronization2 = VK_TRUE;

		VkPhysicalDeviceDynamicRenderingLocalReadFeatures localReadFeatures = {};
		localReadFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_DYNAMIC_RENDERING_LOCAL_READ_FEATURES;
		
		VkPhysicalDeviceDynamicRenderingFeatures dynamicRenderingFeatures = {};
		dynamicRenderingFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_DYNAMIC_RENDERING_FEATURES;
		//dynamicRenderingFeatures.dynamicRendering = VK_TRUE;

		VkPhysicalDeviceRobustness2FeaturesEXT robustness2Features = {};
		robustness2Features.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_ROBUSTNESS_2_FEATURES_EXT;
		robustness2Features.nullDescriptor = VK_TRUE;

		VkPhysicalDeviceAccelerationStructureFeaturesKHR asFeatures = {};
		asFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_ACCELERATION_STRUCTURE_FEATURES_KHR;
		asFeatures.accelerationStructure = VK_TRUE;

		VkPhysicalDeviceRayQueryFeaturesKHR rayQueryFeatures = {};
		rayQueryFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_QUERY_FEATURES_KHR;
		rayQueryFeatures.rayQuery = VK_TRUE;

		VkPhysicalDeviceMeshShaderFeaturesEXT meshShaderFeatures = {};
		meshShaderFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_MESH_SHADER_FEATURES_EXT;
		meshShaderFeatures.meshShader = VK_TRUE;
		meshShaderFeatures.taskShader = VK_TRUE;

		VkPhysicalDeviceFaultFeaturesEXT deviceFaultFeatures = {};
		deviceFaultFeatures.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_FAULT_FEATURES_EXT;
		deviceFaultFeatures.deviceFault = VK_TRUE;

		VkPhysicalDeviceVulkan11Features devfeatures11{};
		devfeatures11.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_1_FEATURES;
		devfeatures11.multiview = VK_TRUE;
		devfeatures11.uniformAndStorageBuffer16BitAccess = VK_TRUE;
		devfeatures11.shaderDrawParameters = VK_TRUE;

		VkPhysicalDeviceVulkan12Features devfeatures12{};
		devfeatures12.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_2_FEATURES;
		devfeatures12.timelineSemaphore = VK_TRUE;
		devfeatures12.shaderFloat16 = VK_TRUE;
		devfeatures12.shaderInt8 = VK_TRUE;

		VkPhysicalDeviceFeatures2 features2{};
		features2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_FEATURES_2_KHR;
		features2.pNext = &dynamicRenderingFeatures;
		dynamicRenderingFeatures.pNext = &localReadFeatures;
		localReadFeatures.pNext = &sync2Features;
		vkGetPhysicalDeviceFeatures2(mPhysicalDevice, &features2);
		features2.features.robustBufferAccess = VK_TRUE;
		features2.features.geometryShader = VK_TRUE;
		features2.features.multiViewport = VK_TRUE;

		mVulkanExt.IsDynamicRendering = (dynamicRenderingFeatures.dynamicRendering == VK_TRUE);
		mVulkanExt.IsDynamicRenderingLocalRead = (localReadFeatures.dynamicRenderingLocalRead == VK_TRUE);
		mVulkanExt.IsSynchronization2 = (sync2Features.synchronization2 == VK_TRUE);
		ASSERT(mVulkanExt.IsDynamicRendering);
		ASSERT(mVulkanExt.IsDynamicRenderingLocalRead);
		ASSERT(mVulkanExt.IsSynchronization2);

		VkPhysicalDeviceFeatures features1{};
		vkGetPhysicalDeviceFeatures(mPhysicalDevice, &features1);
		features1.robustBufferAccess = VK_TRUE;
		features1.geometryShader = VK_TRUE;
		features1.multiViewport = VK_TRUE;

		features2.pNext = &devfeatures12;
		devfeatures12.pNext = &devfeatures11;
		devfeatures11.pNext = &robustness2Features;
		robustness2Features.pNext = &dynamicRenderingFeatures;
		dynamicRenderingFeatures.pNext = &localReadFeatures;
		localReadFeatures.pNext = &sync2Features;
		sync2Features.pNext = &dynScissorFeatures;

		VkDeviceCreateInfo createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
		createInfo.pNext = &features2;

		[[maybe_unused]]VkStructureHead* curDeviceCreateChainTail = nullptr;
		curDeviceCreateChainTail = (VkStructureHead*)&dynScissorFeatures;
		if (mVulkanExt.IsRayQuery)
		{
			//VK_KHR_acceleration_structure requires bufferDeviceAddress(core in 1.2)
			devfeatures12.bufferDeviceAddress = VK_TRUE;
			curDeviceCreateChainTail->pNext = &asFeatures;
			asFeatures.pNext = &rayQueryFeatures;
			curDeviceCreateChainTail = (VkStructureHead*)&rayQueryFeatures;
		}
		if (mVulkanExt.IsMeshShader)
		{
			curDeviceCreateChainTail->pNext = &meshShaderFeatures;
			curDeviceCreateChainTail = (VkStructureHead*)&meshShaderFeatures;
		}
		if (bDeviceFault)
		{
			curDeviceCreateChainTail->pNext = &deviceFaultFeatures;
			curDeviceCreateChainTail = (VkStructureHead*)&deviceFaultFeatures;
		}
		float queuePriority = 1.0f;
		VkDeviceQueueCreateInfo queueCreateInfos[2]{};
		{
			if (graphicsFamily == presentFamily)
			{
				queueCreateInfos[0].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
				queueCreateInfos[0].queueFamilyIndex = graphicsFamily;
				queueCreateInfos[0].queueCount = 1;
				queueCreateInfos[0].pQueuePriorities = &queuePriority;

				createInfo.queueCreateInfoCount = 1;
				createInfo.pQueueCreateInfos = queueCreateInfos;
			}
			else
			{
				queueCreateInfos[0].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
				queueCreateInfos[0].queueFamilyIndex = graphicsFamily;
				queueCreateInfos[0].queueCount = 1;
				queueCreateInfos[0].pQueuePriorities = &queuePriority;

				queueCreateInfos[1].sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
				queueCreateInfos[1].queueFamilyIndex = presentFamily;
				queueCreateInfos[1].queueCount = 1;
				queueCreateInfos[1].pQueuePriorities = &queuePriority;

				createInfo.queueCreateInfoCount = 2;
				createInfo.pQueueCreateInfos = queueCreateInfos;
			}
		}

		if (false)
		{
			//warning: createInfo.pNext is set, pEnabledFeatures must be nullptr
			/*mDeviceFeatures.shaderInt16 = VK_TRUE;
			mDeviceFeatures.samplerAnisotropy = VK_TRUE;
			mDeviceFeatures.depthClamp = VK_TRUE;
			mDeviceFeatures.fillModeNonSolid = VK_TRUE;
			mDeviceFeatures.shaderUniformBufferArrayDynamicIndexing = VK_TRUE;
			mDeviceFeatures.shaderSampledImageArrayDynamicIndexing = VK_TRUE;
			mDeviceFeatures.shaderStorageBufferArrayDynamicIndexing = VK_TRUE;
			mDeviceFeatures.shaderStorageImageArrayDynamicIndexing = VK_TRUE;
			mDeviceFeatures.robustBufferAccess = VK_TRUE;

			createInfo.pEnabledFeatures = &mDeviceFeatures;*/
		}

		std::vector<const char*>	mValidationLayers;
		if (desc->CreateDebugLayer)
		{
			auto pLayer = pVkGpuSys->FindLayer("VK_LAYER_KHRONOS_validation");
			if (pLayer != nullptr)
			{
				mValidationLayers.push_back("VK_LAYER_KHRONOS_validation");
			}
		}

#if defined(HasModule_GpuDump)
		VkDeviceDiagnosticsConfigCreateInfoNV nvDiagnosticsInfo{};
		if (desc->IsAftermath && desc->IsNVIDIA())
		{
			GpuDump::NvAftermath::InitDump(NxRHI::RHI_VK);
			curDeviceCreateChainTail->pNext = &nvDiagnosticsInfo;
			nvDiagnosticsInfo.sType = VK_STRUCTURE_TYPE_DEVICE_DIAGNOSTICS_CONFIG_CREATE_INFO_NV;
			nvDiagnosticsInfo.flags = VkDeviceDiagnosticsConfigFlagBitsNV::VK_DEVICE_DIAGNOSTICS_CONFIG_ENABLE_RESOURCE_TRACKING_BIT_NV |
				VkDeviceDiagnosticsConfigFlagBitsNV::VK_DEVICE_DIAGNOSTICS_CONFIG_ENABLE_AUTOMATIC_CHECKPOINTS_BIT_NV |
				VkDeviceDiagnosticsConfigFlagBitsNV::VK_DEVICE_DIAGNOSTICS_CONFIG_ENABLE_SHADER_DEBUG_INFO_BIT_NV;
		}
#endif

		if (extensions.size() > 0)
		{
			createInfo.enabledExtensionCount = static_cast<uint32_t>(extensions.size());
			createInfo.ppEnabledExtensionNames = extensions.data();
		}
		if (mValidationLayers.size() > 0)
		{
			createInfo.enabledLayerCount = static_cast<uint32_t>(mValidationLayers.size());
			createInfo.ppEnabledLayerNames = mValidationLayers.data();
		}
		else
		{
			createInfo.enabledLayerCount = 0;
		}
		auto hr = vkCreateDevice(mPhysicalDevice, &createInfo, GetVkAllocCallBacks(), &mDevice);
		if (hr != VK_SUCCESS)
		{
			ASSERT(false);
			return false;
		}
#if defined(HasModule_GpuDump)
		if (desc->IsAftermath && desc->IsNVIDIA())
		{
			//align with DX12GpuDevice::InitDevice
			GpuDump::NvAftermath::DeviceCreated(NxRHI::RHI_VK, this);
		}
#endif
		QueryDevice();

		VmaAllocatorCreateInfo allocatorInfo = {};
		allocatorInfo.vulkanApiVersion = VK_API_VERSION_1_2;
		allocatorInfo.physicalDevice = mPhysicalDevice;
		allocatorInfo.device = mDevice;
		allocatorInfo.instance = GetVkInstance();
		if (mVulkanExt.IsRayQuery)
		{
			allocatorInfo.flags |= VMA_ALLOCATOR_CREATE_BUFFER_DEVICE_ADDRESS_BIT;
		}
		if (vmaCreateAllocator(&allocatorInfo, &mVmaAllocator) != VK_SUCCESS) 
		{
			std::cerr << "Failed to create VMA allocator!" << std::endl;
			return false;
		}
		//vmaCreateBuffer,vmaDestroyBuffer,vmaMapMemory,vmaUnmapMemory
		//vmaCreateImage,vmaDestroyAllocator
		
		auto fn_vkCreateDebugReportCallbackEXT = (PFN_vkCreateDebugReportCallbackEXT)vkGetInstanceProcAddr(GetVkInstance(), "vkCreateDebugReportCallbackEXT");
		if (fn_vkCreateDebugReportCallbackEXT != nullptr)
		{
			VkDebugReportCallbackCreateInfoEXT cbInfo{};
			cbInfo.sType = VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT;
			cbInfo.flags = VkDebugReportFlagBitsEXT::VK_DEBUG_REPORT_ERROR_BIT_EXT | VkDebugReportFlagBitsEXT::VK_DEBUG_REPORT_WARNING_BIT_EXT;
			cbInfo.pfnCallback = OnVK_DebugReportCallbackEXT;
			cbInfo.pUserData = this;
			fn_vkCreateDebugReportCallbackEXT(this->GetVkInstance(), &cbInfo, this->GetVkAllocCallBacks(), &mDebugReportCallback);
		}
		
		vkGetDeviceQueue(mDevice, graphicsFamily, 0, &mCmdQueue->mGraphicsQueue);
		vkGetDeviceQueue(mDevice, presentFamily, 0, &mCmdQueue->mPresentQueue);

		if (mVulkanExt.IsRayQuery)
		{
			fn_vkCreateAccelerationStructureKHR = (PFN_vkCreateAccelerationStructureKHR)vkGetDeviceProcAddr(mDevice, "vkCreateAccelerationStructureKHR");
			fn_vkDestroyAccelerationStructureKHR = (PFN_vkDestroyAccelerationStructureKHR)vkGetDeviceProcAddr(mDevice, "vkDestroyAccelerationStructureKHR");
			fn_vkGetAccelerationStructureBuildSizesKHR = (PFN_vkGetAccelerationStructureBuildSizesKHR)vkGetDeviceProcAddr(mDevice, "vkGetAccelerationStructureBuildSizesKHR");
			fn_vkCmdBuildAccelerationStructuresKHR = (PFN_vkCmdBuildAccelerationStructuresKHR)vkGetDeviceProcAddr(mDevice, "vkCmdBuildAccelerationStructuresKHR");
			fn_vkGetAccelerationStructureDeviceAddressKHR = (PFN_vkGetAccelerationStructureDeviceAddressKHR)vkGetDeviceProcAddr(mDevice, "vkGetAccelerationStructureDeviceAddressKHR");
			fn_vkCmdCopyAccelerationStructureKHR = (PFN_vkCmdCopyAccelerationStructureKHR)vkGetDeviceProcAddr(mDevice, "vkCmdCopyAccelerationStructureKHR");
			if (fn_vkCreateAccelerationStructureKHR == nullptr || fn_vkCmdBuildAccelerationStructuresKHR == nullptr)
			{
				mVulkanExt.IsRayQuery = false;
				mCaps.IsSupportRayTracing = false;
			}
		}
		if (mVulkanExt.IsMeshShader)
		{
			fn_vkCmdDrawMeshTasksEXT = (PFN_vkCmdDrawMeshTasksEXT)vkGetDeviceProcAddr(mDevice, "vkCmdDrawMeshTasksEXT");
			fn_vkCmdDrawMeshTasksIndirectEXT = (PFN_vkCmdDrawMeshTasksIndirectEXT)vkGetDeviceProcAddr(mDevice, "vkCmdDrawMeshTasksIndirectEXT");
			if (fn_vkCmdDrawMeshTasksEXT == nullptr)
			{
				mVulkanExt.IsMeshShader = false;
				mCaps.IsSupportMeshShader = false;
			}
		}
		if (bDeviceFault)
		{
			fn_vkGetDeviceFaultInfoEXT = (PFN_vkGetDeviceFaultInfoEXT)vkGetDeviceProcAddr(mDevice, "vkGetDeviceFaultInfoEXT");
		}
		
		FFenceDesc fcDesc{};
		mFrameFence = MakeWeakRef(this->CreateFence(&fcDesc, "Vulkan Frame Fence", __FILE__, __LINE__));
		
		{
			auto cmdAllocator = new VKCmdBufferManager();
			cmdAllocator->Initialize(this);
			mCmdAllocatorManager = MakeWeakRef(cmdAllocator);

			/*auto manager = mCmdAllocatorManager;
			FContextTickableManager::GetInstance()->PushTickable([manager]()->bool
				{
					auto context = manager->GetThreadContext();
					if (context == nullptr)
						return true;
					context->TickForRecycle(manager->mDevice);
					return false;
				});*/
		}

		mCmdQueue->Init(this);
		
		mGpuResourceAlignment.TexturePitchAlignment = (UINT)mDeviceProperties.limits.minTexelBufferOffsetAlignment;
		mGpuResourceAlignment.TextureAlignment = (UINT)mGpuResourceAlignment.TexturePitchAlignment;
		mGpuResourceAlignment.MsaaAlignment = (UINT)mDeviceProperties.limits.minTexelBufferOffsetAlignment;
		mGpuResourceAlignment.RawSrvUavAlignment = (UINT)mDeviceProperties.limits.minStorageBufferOffsetAlignment;
		mGpuResourceAlignment.UavCounterAlignment = (UINT)mDeviceProperties.limits.minStorageBufferOffsetAlignment;
		
		mCaps.IsSupportBufferToTexture = true;
		mCaps.IsSupportSSBO_VS = true;

		vkGetPhysicalDeviceMemoryProperties(mPhysicalDevice, &mMemProperties);

		mDefaultBufferAllocator = MakeWeakRef(new VKGpuDefaultMemAllocator());

		mDescriptorPoolManager = MakeWeakRef(new VKDesriptorPoolManager());
		mDescriptorPoolManager->Initialize(this);

		mFrameBufferCache = MakeWeakRef(new VKFrameBufferCache());
		CreateNullObjects();
		return true;
	}
	void VKGpuDevice::CreateNullObjects()
	{
		{
			FBufferDesc vbDesc{};
			vbDesc.Type = EBufferType::BFT_UAV;
			vbDesc.Size = 1;
			vbDesc.RowPitch = 1;
			vbDesc.DepthPitch = 1;
			vbDesc.StructureStride = 1;
			mNullSSBO = MakeWeakRef((VKBuffer*)this->CreateBuffer(&vbDesc, __FILE__, __LINE__));
			mNullSSBO->SetDebugName("NullSSBO");
		}
		{
			FBufferDesc vbDesc{};
			vbDesc.Type = EBufferType::BFT_CBuffer;
			vbDesc.Size = 1;
			vbDesc.RowPitch = 1;
			vbDesc.DepthPitch = 1;
			vbDesc.StructureStride = 1;
			mNullUBO = MakeWeakRef((VKBuffer*)this->CreateBuffer(&vbDesc, __FILE__, __LINE__));
			mNullUBO->SetDebugName("NullUBO");
		}
		{
			FBufferDesc vbDesc{};
			vbDesc.Type = EBufferType::BFT_Vertex;
			vbDesc.Size = 1;
			vbDesc.RowPitch = 1;
			vbDesc.DepthPitch = 1;
			vbDesc.StructureStride = 1;
			mNullVB = MakeWeakRef((VKBuffer*)this->CreateBuffer(&vbDesc, __FILE__, __LINE__));
			mNullVB->SetDebugName("NullVB");
		}
		{
			FTextureDesc texDesc{};
			texDesc.SetDefault();
			texDesc.BindFlags = (EBufferType)(EBufferType::BFT_SRV);
			auto pTex = MakeWeakRef((VKTexture*)this->CreateTexture(&texDesc, __FILE__, __LINE__));
			FSrvDesc srvDesc{};
			srvDesc.SetTexture2D();
			srvDesc.Format = texDesc.Format;
			srvDesc.Texture2D.MipLevels = 1;
			mNullSampledImage = MakeWeakRef((VKSrView*)this->CreateSRV(pTex, &srvDesc, __FILE__, __LINE__));
			mNullSampledImage->SetDebugName("NullSampledImage");
		}
		{
			FSamplerDesc samplerDesc{};
			samplerDesc.SetDefault();
			mNullSampler = MakeWeakRef((VKSampler*)this->CreateSampler(&samplerDesc, __FILE__, __LINE__));
			mNullSampler->SetDebugName("NullSampler");
		}
	}
	bool VKGpuDevice::GetAllocatorInfo(VkBufferUsageFlags flags, VkMemoryPropertyFlags prop, UINT& typeIndex, UINT& alignment)
	{
		VkBufferCreateInfo bufferInfo = {};
		bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		bufferInfo.size = flags;
		bufferInfo.flags = 0;
		bufferInfo.usage |= VK_BUFFER_USAGE_INDIRECT_BUFFER_BIT;
		bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		bufferInfo.queueFamilyIndexCount = 0;
		VkBuffer tmp = nullptr;
		if (vkCreateBuffer(mDevice, &bufferInfo, GetVkAllocCallBacks(), &tmp) != VK_SUCCESS)
		{
			ASSERT(false);
			return false;
		}
		VkMemoryRequirements memRequirements;
		vkGetBufferMemoryRequirements(mDevice, tmp, &memRequirements);
		vkDestroyBuffer(mDevice, tmp, GetVkAllocCallBacks());
		tmp = nullptr;

		alignment = (UINT)memRequirements.alignment;
		typeIndex = this->FindMemoryType(memRequirements.memoryTypeBits, prop);
		return true;
	}
	void VKGpuDevice::QueryDevice()
	{
		//align with DX12GpuDevice caps query
		mCaps.IsSupportRayTracing = mVulkanExt.IsRayQuery;
		mCaps.IsSupportMeshShader = mVulkanExt.IsMeshShader;
		mCaps.IsSupportCSInRenderPass = mVulkanExt.IsDynamicRendering;

		VkPhysicalDeviceMultiviewProperties multiviewProps = {};
		multiviewProps.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_MULTIVIEW_PROPERTIES;
		VkPhysicalDeviceProperties2 props2 = {};
		props2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_PROPERTIES_2;
		props2.pNext = &multiviewProps;
		vkGetPhysicalDeviceProperties2(mPhysicalDevice, &props2);
		mCaps.MaxViewInstanceCount = multiviewProps.maxMultiviewViewCount;
	}
	IBuffer* VKGpuDevice::CreateBuffer(const FBufferDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKBuffer>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITexture* VKGpuDevice::CreateTexture(const FTextureDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKTexture>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICbView* VKGpuDevice::CreateCBV(IBuffer* pBuffer, const FCbvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKCbView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IVbView* VKGpuDevice::CreateVBV(IBuffer* pBuffer, const FVbvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKVbView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IIbView* VKGpuDevice::CreateIBV(IBuffer* pBuffer, const FIbvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKIbView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISrView* VKGpuDevice::CreateSRV(IGpuBufferData* pBuffer, const FSrvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKSrView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IUaView* VKGpuDevice::CreateUAV(IGpuBufferData* pBuffer, const FUavDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKUaView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderTargetView* VKGpuDevice::CreateRTV(ITexture* pBuffer, const FRtvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKRenderTargetView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IDepthStencilView* VKGpuDevice::CreateDSV(ITexture* pBuffer, const FDsvDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKDepthStencilView>(file ? file : __FILE__, line);
		if (result->Init(this, pBuffer, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISampler* VKGpuDevice::CreateSampler(const FSamplerDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKSampler>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ISwapChain* VKGpuDevice::CreateSwapChain(const FSwapChainDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKSwapChain>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IRenderPass* VKGpuDevice::CreateRenderPass(const FRenderPassDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKRenderPass>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IFrameBuffers* VKGpuDevice::CreateFrameBuffers(IRenderPass* rpass, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKFrameBuffers>(file ? file : __FILE__, line);
		result->mRenderPass = rpass;
		result->mDeviceRef.FromObject(this);
		return result;
	}
	IAccelerationStructure* VKGpuDevice::CreateAccelerationStructure(const FAccelerationStructureDesc* rpass, const char* file, int line)
	{
		if (mVulkanExt.IsRayQuery == false)
			return nullptr;
		auto result = NewObjectWithInfo<VKAccelerationStructure>(file ? file : __FILE__, line);
		if (result->Init(this, rpass) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IAStructureInstance* VKGpuDevice::CreateAccelerationStructureInstance(const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure, const char* file, int line)
	{
		if (mVulkanExt.IsRayQuery == false)
			return nullptr;
		auto result = NewObjectWithInfo<VKAStructureInstance>(file ? file : __FILE__, line);
		if (result->Init(this, desc, pAStructrure) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ITopAccelerationStructure* VKGpuDevice::CreateTopAccelerationStructure(const FTopAccelerationStructureDesc* desc, const char* file, int line)
	{
		if (mVulkanExt.IsRayQuery == false)
			return nullptr;
		auto result = NewObjectWithInfo<VKTopAccelerationStructure>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGpuPipeline* VKGpuDevice::CreatePipeline(const FGpuPipelineDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKGpuPipeline>(file ? file : __FILE__, line);
		if (result->Init(this, *desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGpuDrawState* VKGpuDevice::CreateGpuDrawState(const char* file, int line)
	{
		return NewObjectWithInfo<VKGpuDrawState>(file ? file : __FILE__, line);
	}
	IInputLayout* VKGpuDevice::CreateInputLayout(FInputLayoutDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKInputLayout>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	ICommandList* VKGpuDevice::CreateCommandList(const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKCommandList>(file ? file : __FILE__, line);
		if (result->Init(this) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IShader* VKGpuDevice::CreateShader(FShaderDesc* desc, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKShader>(file ? file : __FILE__, line);
		if (result->Init(this, desc) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicsEffect* VKGpuDevice::CreateShaderEffect(const char* file, int line)
	{
		return NewObjectWithInfo<VKGraphicsEffect>(file ? file : __FILE__, line);
	}
	IComputeEffect* VKGpuDevice::CreateComputeEffect(const char* file, int line)
	{
		return NewObjectWithInfo<VKComputeEffect>(file ? file : __FILE__, line);
	}
	IFence* VKGpuDevice::CreateFence(const FFenceDesc* desc, const char* name, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKFence>(file ? file : __FILE__, line);
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IEvent* VKGpuDevice::CreateGpuEvent(const FEventDesc* desc, const char* name, const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKEvent>(file ? file : __FILE__, line, name);
		if (result->Init(this, *desc, name) == false)
		{
			result->Release();
			return nullptr;
		}
		return result;
	}
	IGraphicDraw* VKGpuDevice::CreateGraphicDraw(const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKGraphicDraw>(file ? file : __FILE__, line);
		result->mDeviceRef.FromObject(this);
		return result;
	}
	IComputeDraw* VKGpuDevice::CreateComputeDraw(const char* file, int line)
	{
		auto result = NewObjectWithInfo<VKComputeDraw>(file ? file : __FILE__, line);
		result->mDeviceRef.FromObject(this);
		return result;
	}
	IGpuScope* VKGpuDevice::CreateGpuScope(const char* file, int line)
	{
		return nullptr;
	}
	void VKGpuDevice::TickPostEvents()
	{
		IsSyncStage = true;

		if (mCmdQueue != nullptr)
			mCmdQueue->TryRecycle();
		mCmdAllocatorManager->TickRecycle();

		IGpuDevice::TickPostEvents();
		
		if (mIsTryFinalize)
		{
			mFrameFence->WaitToExpect();
			mCmdQueue->Flush(EQueueType::QU_ALL);
			bool allocatorRecycle = mCmdAllocatorManager->FinalCleanup();
			bool cmdRecycle = mCmdQueue->mWaitRecycleCmdlists.size() == 0;
			bool post = mTickingPostEvents.size() == 0 && mPostEvents.size() == 0;

			if (cmdRecycle && allocatorRecycle && post)
			{
				mCmdQueue->ClearIdleCmdlists();
				mIsFinalized = true;
			}
		}

		IsSyncStage = false;
	}

	VKCmdQueue::VKCmdQueue()
	{

	}
	VKCmdQueue::~VKCmdQueue()
	{
		if (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.pop();
		}
	}
	void VKCmdQueue::ClearIdleCmdlists()
	{
		while (mIdleCmdlist.empty() == false)
		{
			mIdleCmdlist.pop();
		}
	}
	void VKCmdQueue::Init(VKGpuDevice* device)
	{
		FFenceDesc fcDesc{};
		mFlushFence = MakeWeakRef(device->CreateFence(&fcDesc, "CmdQueue Fence", __FILE__, __LINE__));
	}
	void VKCmdQueue::ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type)
	{
		for (UINT i = 0; i < NumOfWait; i++)
		{
			ppWaitCmdlists[i]->mCommitFence->WaitToExpect();
		}

		VKCommandList* vkCmd = nullptr;
		for (UINT i = 0; i < NumOfExe; i++)
		{
			vkCmd = (VKCommandList*)Cmdlist[i];
			vkCmd->Commit(this, type);
			this->IncreaseSignal(vkCmd->mCommitFence, type);
			//vkCmd->ResetGpuDraws();
		}
	}
	
	void VKCmdQueue::WaitFence(IFence* fence, UINT64 value, EQueueType type)
	{
		auto waitFence = (VKFence*)fence;
		VkSubmitInfo submitInfo{};
		submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
		submitInfo.commandBufferCount = 0;
		//submitInfo.pCommandBuffers = &mDummyCmdList->mCommandBuffer->RealObject;

		VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;/*VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT |
			VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT |
			VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;*/
		VkSemaphore waitSmp[1]{};
		waitSmp[0] = waitFence->mSemaphore;
		submitInfo.waitSemaphoreCount = 1;
		submitInfo.pWaitSemaphores = waitSmp;
		VkPipelineStageFlags waitStages[1];
		waitStages[0] = waitStage;
		submitInfo.pWaitDstStageMask = waitStages;

		submitInfo.signalSemaphoreCount = 0;
		submitInfo.pSignalSemaphores = nullptr;

		VkTimelineSemaphoreSubmitInfo timelineInfo{};
		submitInfo.pNext = &timelineInfo;
		timelineInfo.sType = VK_STRUCTURE_TYPE_TIMELINE_SEMAPHORE_SUBMIT_INFO;

		UINT64 waitValue[1]{};
		waitValue[0] = value;
		timelineInfo.waitSemaphoreValueCount = 1;
		timelineInfo.pWaitSemaphoreValues = waitValue;

		timelineInfo.signalSemaphoreValueCount = 0;
		timelineInfo.pSignalSemaphoreValues = nullptr;

		auto hr = SafeQueueSubmit(1, &submitInfo, nullptr, type);
		ASSERT(hr == VK_SUCCESS);
	}

	void VKCmdQueue::TryRecycle()
	{
		VAutoVSLLock locker(mCmdLocker);
		for (size_t i = 0; i < mWaitRecycleCmdlists.size(); i++)
		{
			auto& cur = mWaitRecycleCmdlists[i];
			auto fenceValue = cur.CmdList->GetCommitFence()->GetCompletedValue();
			if (fenceValue >= cur.WaitFenceValue)
			{
				auto pRecorder = mWaitRecycleCmdlists[i].CmdList->GetCmdRecorder();
				if (pRecorder != nullptr)
					pRecorder->ResetGpuDraws();
				mIdleCmdlist.push(mWaitRecycleCmdlists[i].CmdList);
				mWaitRecycleCmdlists.erase(mWaitRecycleCmdlists.begin() + i);
				i--;
			}
		}
	}
	ICommandList* VKCmdQueue::GetIdleCmdlist()
	{
		//TryRecycle();

		VAutoVSLLock locker(mCmdLocker);
		if (mIdleCmdlist.empty())
		{
			mIdleCmdlist.push(MakeWeakRef(mDevice->CreateCommandList(__FILE__, __LINE__)));
		}
		auto result = mIdleCmdlist.front();
		result->AddRef();
		mIdleCmdlist.pop();
		return result;
	}
	void VKCmdQueue::ReleaseIdleCmdlist(ICommandList* cmd)
	{
		VAutoVSLLock locker(mCmdLocker);
		FWaitRecycle tmp;
		tmp.CmdList = cmd;
		tmp.WaitFenceValue = cmd->GetCommitFence()->GetExpectValue();
		mWaitRecycleCmdlists.push_back(tmp);
		cmd->Release();
		return;
	}
	UINT64 VKCmdQueue::Flush(EQueueType type)
	{
		return mFlushFence->WaitToExpect();
	}
	void VKCmdQueue::BeginEvent(const char* info, DWORD color)
	{
		//align with DX12CmdQueue::BeginEvent(PIXBeginEvent on queue)
		if (VKGpuSystem::vkQueueBeginDebugUtilsLabelEXT != nullptr && mGraphicsQueue != nullptr)
		{
			VkDebugUtilsLabelEXT markerInfo{};
			markerInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_LABEL_EXT;
			markerInfo.pLabelName = info;
			markerInfo.color[0] = ((color >> 16) & 0xFF) / 255.0f;
			markerInfo.color[1] = ((color >> 8) & 0xFF) / 255.0f;
			markerInfo.color[2] = (color & 0xFF) / 255.0f;
			markerInfo.color[3] = ((color >> 24) & 0xFF) / 255.0f;
			VAutoVSLLock locker(mQueueLocker);
			VKGpuSystem::vkQueueBeginDebugUtilsLabelEXT(mGraphicsQueue, &markerInfo);
		}
	}
	void VKCmdQueue::EndEvent(const char* info)
	{
		if (VKGpuSystem::vkQueueEndDebugUtilsLabelEXT != nullptr && mGraphicsQueue != nullptr)
		{
			VAutoVSLLock locker(mQueueLocker);
			VKGpuSystem::vkQueueEndDebugUtilsLabelEXT(mGraphicsQueue);
		}
	}
	VkResult VKCmdQueue::SafeQueueSubmit(uint32_t submitCount, const VkSubmitInfo* pSubmits, VkFence fence, EQueueType type)
	{
		VkResult hr;
		{
			VAutoVSLLock locker(mQueueLocker);
			hr = vkQueueSubmit(mGraphicsQueue, submitCount, pSubmits, fence);
		}
		if (hr == VK_ERROR_DEVICE_LOST)
		{
			//align with DX12(DXGI_ERROR_DEVICE_REMOVED at the submit points)
			mDevice->OnDeviceRemoved();
		}
		return hr;
	}
	VkResult VKCmdQueue::SafeQueuePresentKHR(const VkPresentInfoKHR* pPresentInfo, EQueueType type)
	{
		VkResult hr;
		{
			VAutoVSLLock locker(mQueueLocker);
			hr = vkQueuePresentKHR(mPresentQueue, pPresentInfo);
		}
		if (hr == VK_ERROR_DEVICE_LOST)
		{
			mDevice->OnDeviceRemoved();
		}
		return hr;
	}
}

NS_END