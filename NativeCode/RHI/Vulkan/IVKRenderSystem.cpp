#include "IVKRenderSystem.h"
#include "IVKRenderContext.h"
#include <set>

#define new VNEW

NS_BEGIN

IVKRenderSystem::IVKRenderSystem()
{
	mDeviceNumber = 0;
	mDeviceExtensions.push_back(VK_KHR_SWAPCHAIN_EXTENSION_NAME);
	mValidationLayers.push_back("VK_LAYER_KHRONOS_validation");
}


IVKRenderSystem::~IVKRenderSystem()
{
	if (mVKInstance != nullptr)
	{
		vkDestroyInstance(mVKInstance, nullptr);
		mVKInstance = nullptr;
	}
}

vBOOL IVKRenderSystem::OnVKDebugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData)
{
	VFX_LTRACE(ELTT_Graphics, "Vulkan: %s\r\n", pCallbackData->pMessage);
	return FALSE;
}

static VKAPI_ATTR VkBool32 VKAPI_CALL debugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData) {
	auto pSys = (IVKRenderSystem*)pUserData;
	return pSys->OnVKDebugCallback(messageSeverity, messageType, pCallbackData);
}

void populateDebugMessengerCreateInfo(VkDebugUtilsMessengerCreateInfoEXT& createInfo) {
	createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT;
	createInfo.messageSeverity = VK_DEBUG_UTILS_MESSAGE_SEVERITY_VERBOSE_BIT_EXT |
				VK_DEBUG_UTILS_MESSAGE_SEVERITY_WARNING_BIT_EXT | 
				VK_DEBUG_UTILS_MESSAGE_SEVERITY_ERROR_BIT_EXT |
				VK_DEBUG_UTILS_MESSAGE_SEVERITY_INFO_BIT_EXT;
	createInfo.messageType = VK_DEBUG_UTILS_MESSAGE_TYPE_GENERAL_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_VALIDATION_BIT_EXT | VK_DEBUG_UTILS_MESSAGE_TYPE_PERFORMANCE_BIT_EXT;
	createInfo.pfnUserCallback = debugCallback;
}

bool IVKRenderSystem::Init(const IRenderSystemDesc* desc)
{
	VkApplicationInfo appInfo = {};
	appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
	appInfo.pApplicationName = "Titan3D";
	appInfo.applicationVersion = VK_MAKE_VERSION(1, 0, 0);
	appInfo.pEngineName = "Titan3D";
	appInfo.engineVersion = VK_MAKE_VERSION(1, 0, 0);
	appInfo.apiVersion = VK_API_VERSION_1_2;

	VkInstanceCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
	createInfo.pApplicationInfo = &appInfo;

	uint32_t extensionCount = 0;
	vkEnumerateInstanceExtensionProperties(nullptr, &extensionCount, nullptr);
	std::vector<VkExtensionProperties> extensions(extensionCount);
	vkEnumerateInstanceExtensionProperties(nullptr, &extensionCount, extensions.data());

	std::vector<const char*> extensionNames(extensionCount);
	for (uint32_t i = 0; i < extensionCount; i++)
	{
		extensionNames[i] = extensions[i].extensionName;
	}
	createInfo.enabledExtensionCount = static_cast<uint32_t>(extensionNames.size());
	createInfo.ppEnabledExtensionNames = extensionNames.data();

	VkDebugUtilsMessengerCreateInfoEXT debugCreateInfo;
	if (desc->CreateDebugLayer) {
		createInfo.enabledLayerCount = static_cast<uint32_t>(mValidationLayers.size());
		createInfo.ppEnabledLayerNames = mValidationLayers.data();

		populateDebugMessengerCreateInfo(debugCreateInfo);
		debugCreateInfo.pUserData = this;
		createInfo.pNext = (VkDebugUtilsMessengerCreateInfoEXT*)&debugCreateInfo;
	}
	else {
		createInfo.enabledLayerCount = 0;

		createInfo.pNext = nullptr;
	}

	if (vkCreateInstance(&createInfo, nullptr, &mVKInstance) != VK_SUCCESS) {
		return false;
	}

	vkEnumeratePhysicalDevices(mVKInstance, &mDeviceNumber, nullptr);

	mHwDevices.resize(mDeviceNumber);
	vkEnumeratePhysicalDevices(mVKInstance, &mDeviceNumber, mHwDevices.data());
	/*VkSurfaceKHR surface = nullptr;
#ifdef PLATFORM_WIN
	{
		VkWin32SurfaceCreateInfoKHR createInfo_surf = {};
		createInfo_surf.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
		createInfo_surf.pNext = nullptr;
		createInfo_surf.hinstance = nullptr;
		createInfo_surf.hwnd = (HWND)desc->WindowHandle;
		vkCreateWin32SurfaceKHR(mVKInstance, &createInfo_surf, nullptr, &surface);
	}
#endif

	uint32_t deviceCount = 0;
	vkEnumeratePhysicalDevices(mVKInstance, &deviceCount, nullptr);

	if (deviceCount == 0) {
		return false;
	}

	std::vector<VkPhysicalDevice> devices(deviceCount);
	vkEnumeratePhysicalDevices(mVKInstance, &deviceCount, devices.data());

	VkPhysicalDevice physicalDevice = nullptr;
	for (const auto& device : devices) {
		if (IsDeviceSuitable(device, surface)) {
			physicalDevice = device;
			break;
		}
	}

	vkDestroySurfaceKHR(mVKInstance, surface, nullptr);

	if (physicalDevice == VK_NULL_HANDLE) {
		return false;
	}*/

	return true;
}

bool IVKRenderSystem::IsDeviceSuitable(VkPhysicalDevice device, VkSurfaceKHR surface)
{
	QueueFamilyIndices indices = FindQueueFamilies(device, surface);

	bool extensionsSupported = CheckDeviceExtensionSupport(device);

	bool swapChainAdequate = false;
	if (extensionsSupported) {
		SwapChainSupportDetails swapChainSupport = QuerySwapChainSupport(device, surface);
		swapChainAdequate = !swapChainSupport.formats.empty() && !swapChainSupport.presentModes.empty();
	}

	return indices.isComplete() && extensionsSupported && swapChainAdequate;
}

IVKRenderSystem::QueueFamilyIndices IVKRenderSystem::FindQueueFamilies(VkPhysicalDevice device, VkSurfaceKHR surface)
{
	QueueFamilyIndices indices;

	uint32_t queueFamilyCount = 0;
	vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, nullptr);

	std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
	vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies.data());

	int i = 0;
	for (const auto& queueFamily : queueFamilies) {
		if (queueFamily.queueFlags & VK_QUEUE_GRAPHICS_BIT) {
			indices.graphicsFamily = i;
		}

		VkBool32 presentSupport = false;
		vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &presentSupport);

		if (presentSupport) {
			indices.presentFamily = i;
		}

		if (indices.isComplete()) {
			break;
		}

		i++;
	}

	return indices;
}

bool IVKRenderSystem::CheckDeviceExtensionSupport(VkPhysicalDevice device) 
{
	uint32_t extensionCount;
	vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, nullptr);

	std::vector<VkExtensionProperties> availableExtensions(extensionCount);
	vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, availableExtensions.data());

	std::set<std::string> requiredExtensions(mDeviceExtensions.begin(), mDeviceExtensions.end());

	for (const auto& extension : availableExtensions) {
		requiredExtensions.erase(extension.extensionName);
	}

	return requiredExtensions.empty();
}

IVKRenderSystem::SwapChainSupportDetails IVKRenderSystem::QuerySwapChainSupport(VkPhysicalDevice device, VkSurfaceKHR surface)
{
	SwapChainSupportDetails details;

	vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, surface, &details.capabilities);

	uint32_t formatCount;
	vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, nullptr);

	if (formatCount != 0) {
		details.formats.resize(formatCount);
		vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, details.formats.data());
	}

	uint32_t presentModeCount;
	vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &presentModeCount, nullptr);

	if (presentModeCount != 0) {
		details.presentModes.resize(presentModeCount);
		vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &presentModeCount, details.presentModes.data());
	}

	return details;
}

UINT32 IVKRenderSystem::GetContextNumber()
{
	return mDeviceNumber;
}

VkPhysicalDevice IVKRenderSystem::GetPhysicalDevice(UINT index)
{
	uint32_t deviceCount = 0;
	vkEnumeratePhysicalDevices(mVKInstance, &deviceCount, nullptr);
	if (index >= deviceCount)
		return nullptr;
	std::vector<VkPhysicalDevice> devices(deviceCount);
	vkEnumeratePhysicalDevices(mVKInstance, &deviceCount, devices.data());

	VkPhysicalDevice phyDevice = devices[index];
	return phyDevice;
}

vBOOL IVKRenderSystem::GetContextDesc(UINT32 index, IRenderContextDesc* desc)
{
	VkPhysicalDevice phyDevice = GetPhysicalDevice(index);

	VkPhysicalDeviceFeatures phyFeatures;
	vkGetPhysicalDeviceFeatures(phyDevice, &phyFeatures);

	VkPhysicalDeviceProperties phyDevProp;
	vkGetPhysicalDeviceProperties(phyDevice, &phyDevProp);

	desc->AdapterId = index;
	desc->DeviceId = phyDevProp.deviceID;

	strcpy(desc->DeviceName, phyDevProp.deviceName);

	return TRUE;
}

IRenderContext* IVKRenderSystem::CreateContext(const IRenderContextDesc* desc)
{
	auto rc = new IVKRenderContext();

	VkSurfaceKHR surface = nullptr;
#ifdef PLATFORM_WIN
	{
		VkWin32SurfaceCreateInfoKHR createInfo_surf = {};
		createInfo_surf.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
		createInfo_surf.pNext = nullptr;
		createInfo_surf.hinstance = nullptr;
		createInfo_surf.hwnd = (HWND)desc->AppHandle;
		vkCreateWin32SurfaceKHR(mVKInstance, &createInfo_surf, nullptr, &surface);
	}
#endif

	auto phyDevice = GetPhysicalDevice(desc->AdapterId);
	
	if (rc->Init(desc, this, phyDevice, surface) == false)
	{
		vkDestroySurfaceKHR(mVKInstance, surface, nullptr);
		rc->Release();
		return nullptr;
	}

	vkDestroySurfaceKHR(mVKInstance, surface, nullptr);
	return rc;
}

NS_END