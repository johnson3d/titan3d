#pragma once
#include "../IRenderSystem.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderSystem : public IRenderSystem
{
public:
	struct QueueFamilyIndices 
	{
		QueueFamilyIndices()
		{
			graphicsFamily = 0xFFFFFFFF;
			presentFamily = 0xFFFFFFFF;
		}
		uint32_t graphicsFamily;
		uint32_t presentFamily;

		bool isComplete() {
			return graphicsFamily != 0xFFFFFFFF && presentFamily != 0xFFFFFFFF;
		}
	};
	struct SwapChainSupportDetails {
		VkSurfaceCapabilitiesKHR capabilities;
		std::vector<VkSurfaceFormatKHR> formats;
		std::vector<VkPresentModeKHR> presentModes;
	};
public:
	IVKRenderSystem();
	~IVKRenderSystem();

	virtual bool Init(const IRenderSystemDesc* desc) override;

	virtual UINT32 GetContextNumber() override;
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* desc) override;
	virtual IRenderContext* CreateContext(const IRenderContextDesc* desc) override;

	vBOOL OnVKDebugCallback(VkDebugUtilsMessageSeverityFlagBitsEXT messageSeverity, VkDebugUtilsMessageTypeFlagsEXT messageType, const VkDebugUtilsMessengerCallbackDataEXT* pCallbackData);

	bool IsDeviceSuitable(VkPhysicalDevice device, VkSurfaceKHR surface);
	QueueFamilyIndices FindQueueFamilies(VkPhysicalDevice device, VkSurfaceKHR surface);
	bool CheckDeviceExtensionSupport(VkPhysicalDevice device);
	SwapChainSupportDetails QuerySwapChainSupport(VkPhysicalDevice device, VkSurfaceKHR surface);

	VkPhysicalDevice GetPhysicalDevice(UINT index);
public:
	VkInstance					mVKInstance;
	std::vector<const char*>	mDeviceExtensions;

	std::vector<const char*>	mValidationLayers;
};

NS_END