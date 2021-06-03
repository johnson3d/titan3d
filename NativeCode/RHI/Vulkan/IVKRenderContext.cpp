#include "IVKRenderSystem.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"
#include "IVKRenderPipeline.h"
#include "IVKConstantBuffer.h"
#include "IVKVertexBuffer.h"
#include "IVKIndexBuffer.h"
#include "IVKRenderTargetView.h"
#include "IVKDepthStencilView.h"
#include "IVKVertexShader.h"
#include "IVKPixelShader.h"
#include "IVKInputLayout.h"
#include "IVKSwapChain.h"
#include "IVKFrameBuffers.h"
#include "IVKShaderProgram.h"
#include "IVKGeometryMesh.h"
#include "IVKPass.h"
#include <set>

#define new VNEW

NS_BEGIN

IVKRenderContext::IVKRenderContext()
{
	memset(&mDeviceFeatures, 0, sizeof(mDeviceFeatures));
	mPhysicalDevice = nullptr;
	mLogicalDevice = nullptr;
	mGraphicsQueue = nullptr;
	mPresentQueue = nullptr;
	mCommandPool = nullptr;
}


IVKRenderContext::~IVKRenderContext()
{
	if (mGraphicsQueue != nullptr)
	{
		mGraphicsQueue = nullptr;
	}
	if (mPresentQueue != nullptr)
	{
		mPresentQueue = nullptr;
	}
	if (mCommandPool != nullptr)
	{
		vkDestroyCommandPool(mLogicalDevice, mCommandPool, nullptr);
		mCommandPool = nullptr;
	}
	if (mLogicalDevice != nullptr)
	{
		vkDestroyDevice(mLogicalDevice, nullptr);
		mLogicalDevice = nullptr;
	}
	if (mPhysicalDevice != nullptr)
	{
		mPhysicalDevice = nullptr;
	}
}

void IVKRenderContext::InitDescLayout(VkDevice device)
{
	VkDescriptorSetLayoutBinding uboLayoutBinding[MaxCB];
	for (int i = 0; i < MaxCB; i++)
	{
		uboLayoutBinding[i].binding = i;
		uboLayoutBinding[i].descriptorCount = 1;
		uboLayoutBinding[i].descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
		uboLayoutBinding[i].pImmutableSamplers = nullptr;
		uboLayoutBinding[i].stageFlags = VK_SHADER_STAGE_VERTEX_BIT;
	}
	VkDescriptorSetLayoutCreateInfo layoutInfo = {};
	layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
	layoutInfo.bindingCount = MaxCB;
	layoutInfo.pBindings = uboLayoutBinding;
	if (vkCreateDescriptorSetLayout(device, &layoutInfo, nullptr, &mCBDescSetLayout) != VK_SUCCESS)
	{
		return;
	}
}

bool IVKRenderContext::Init(const IRenderContextDesc* desc, IVKRenderSystem* pSys, VkPhysicalDevice device, VkSurfaceKHR surface)
{
	mRenderSystem.FromObject(pSys);
	mPhysicalDevice = device;
	auto family = pSys->FindQueueFamilies(device, surface);

	std::vector<VkDeviceQueueCreateInfo> queueCreateInfos;
	std::set<uint32_t> uniqueQueueFamilies = { family.graphicsFamily, family.presentFamily };

	float queuePriority = 1.0f;
	for (uint32_t queueFamily : uniqueQueueFamilies)
	{
		VkDeviceQueueCreateInfo queueCreateInfo = {};
		queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
		queueCreateInfo.queueFamilyIndex = queueFamily;
		queueCreateInfo.queueCount = 1;
		queueCreateInfo.pQueuePriorities = &queuePriority;
		queueCreateInfos.push_back(queueCreateInfo);
	}

	VkDeviceCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;

	createInfo.queueCreateInfoCount = static_cast<uint32_t>(queueCreateInfos.size());
	createInfo.pQueueCreateInfos = queueCreateInfos.data();

	createInfo.pEnabledFeatures = &mDeviceFeatures;

	createInfo.enabledExtensionCount = static_cast<uint32_t>(pSys->mDeviceExtensions.size());
	createInfo.ppEnabledExtensionNames = pSys->mDeviceExtensions.data();

	if (desc->CreateDebugLayer) 
	{
		createInfo.enabledLayerCount = static_cast<uint32_t>(pSys->mValidationLayers.size());
		createInfo.ppEnabledLayerNames = pSys->mValidationLayers.data();
	}
	else {
		createInfo.enabledLayerCount = 0;
	}

	if (vkCreateDevice(device, &createInfo, nullptr, &mLogicalDevice) != VK_SUCCESS) {
		return false;
	}

	vkGetDeviceQueue(mLogicalDevice, family.graphicsFamily, 0, &mGraphicsQueue);
	vkGetDeviceQueue(mLogicalDevice, family.presentFamily, 0, &mPresentQueue);

	VkCommandPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
	poolInfo.queueFamilyIndex = family.graphicsFamily;

	if (vkCreateCommandPool(mLogicalDevice, &poolInfo, nullptr, &mCommandPool) != VK_SUCCESS) {
		return false;
	}

	return true;
}


ISwapChain* IVKRenderContext::CreateSwapChain(const ISwapChainDesc* desc)
{
	auto swapchain = new IVKSwapChain();
	if (swapchain->Init(this, desc) == false)
	{
		swapchain->Release();
		return nullptr;
	}
	return swapchain;
}

ICommandList* IVKRenderContext::CreateCommandList(const ICommandListDesc* desc)
{
	auto cmd_list = new IVKCommandList();
	return cmd_list;
}


IDrawCall* IVKRenderContext::CreateDrawCall()
{
	auto pass = new IVKDrawCall();
	return pass;
}


IRenderPipeline* IVKRenderContext::CreateRenderPipeline(const IRenderPipelineDesc* desc)
{
	auto rpl = new IVKRenderPipeline();
	return rpl;
}

IVertexBuffer* IVKRenderContext::CreateVertexBuffer(const IVertexBufferDesc* desc)
{
	auto vb = new IVKVertexBuffer();
	return vb;
}

IIndexBuffer* IVKRenderContext::CreateIndexBuffer(const IIndexBufferDesc* desc)
{
	auto ib = new IVKIndexBuffer();
	return ib;
}

IIndexBuffer* IVKRenderContext::CreateIndexBufferFromBuffer(const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	return nullptr;
}

IVertexBuffer* IVKRenderContext::CreateVertexBufferFromBuffer(const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	return nullptr;
}

IGeometryMesh* IVKRenderContext::CreateGeometryMesh()
{
	return new IVKGeometryMesh();
}
//��Դ��Ϣ
IFrameBuffers* IVKRenderContext::CreateFrameBuffers(const IFrameBuffersDesc* desc)
{
	auto rt = new IVKFrameBuffers();
	return rt;
}
IRenderTargetView* IVKRenderContext::CreateRenderTargetView(const IRenderTargetViewDesc* desc)
{
	auto rt = new IVKRenderTargetView();
	return rt;
}

IDepthStencilView* IVKRenderContext::CreateDepthRenderTargetView(const IDepthStencilViewDesc* desc)
{
	auto drt = new IVKDepthStencilView();
	return drt;
}

ITexture2D* IVKRenderContext::CreateTexture2D(const ITexture2DDesc* desc)
{
	return nullptr;
}
IShaderResourceView* IVKRenderContext::CreateShaderResourceView(const IShaderResourceViewDesc* desc)
{
	return nullptr;
}
IShaderResourceView* IVKRenderContext::CreateShaderResourceViewFromBuffer(IGpuBuffer* pBuffer, const ISRVDesc* desc)
{
	return nullptr;
}
IGpuBuffer* IVKRenderContext::CreateGpuBuffer(const IGpuBufferDesc* desc, void* pInitData)
{
	return nullptr;
}
IUnorderedAccessView* IVKRenderContext::CreateUnorderedAccessView(IGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	return nullptr;
}
IShaderResourceView* IVKRenderContext::LoadShaderResourceView(const char* file)
{
	return nullptr;
}
ISamplerState* IVKRenderContext::CreateSamplerState(const ISamplerStateDesc* desc)
{
	return nullptr;
}
IRasterizerState* IVKRenderContext::CreateRasterizerState(const IRasterizerStateDesc* desc)
{
	return nullptr;
}
IDepthStencilState* IVKRenderContext::CreateDepthStencilState(const IDepthStencilStateDesc* desc)
{
	return nullptr;
}
IBlendState* IVKRenderContext::CreateBlendState(const IBlendStateDesc* desc)
{
	return nullptr;
}
//shader
IShaderProgram* IVKRenderContext::CreateShaderProgram(const IShaderProgramDesc* desc)
{
	auto program = new IVKShaderProgram();
	if (program->Init(this, desc) == false)
	{
		program->Release();
		return nullptr;
	}
	return program;
}

IVertexShader* IVKRenderContext::CreateVertexShader(const IShaderDesc* desc)
{
	auto vs = new IVKVertexShader();
	return vs;
}

IPixelShader* IVKRenderContext::CreatePixelShader(const IShaderDesc* desc)
{
	auto ps = new IVKPixelShader();
	return ps;
}

IComputeShader* IVKRenderContext::CreateComputeShader(const IShaderDesc* desc)
{
	return nullptr;
}

IInputLayout* IVKRenderContext::CreateInputLayout(const IInputLayoutDesc* desc)
{
	auto layout = new IVKInputLayout();
	return layout;
}

IConstantBuffer* IVKRenderContext::CreateConstantBuffer(const IConstantBufferDesc* desc)
{
	auto cb = new IVKConstantBuffer();
	if (cb->Init(this, desc) == false)
	{
		cb->Release();
		return nullptr;
	}
	return cb;
}

NS_END