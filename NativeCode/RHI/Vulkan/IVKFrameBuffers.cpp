#include "IVKFrameBuffers.h"
#include "IVKRenderTargetView.h"
#include "IVKDepthStencilView.h"
#include "IVKShaderResourceView.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"
#include "IVKSwapChain.h"
#include "IVKTexture2D.h"
#include "IVKGpuBuffer.h"

#define new VNEW

NS_BEGIN

IVKFrameBuffers::IVKFrameBuffers()
{
	Width = 0;
	Height = 0;
}

IVKFrameBuffers::~IVKFrameBuffers()
{
	PostVkExecute([FrameBuffers = mFrameBuffers](IVKRenderContext* rc)
	{
		for (auto& i : FrameBuffers)
		{
			vkDestroyFramebuffer(rc->mLogicalDevice, i, rc->GetVkAllocCallBacks());
		}
	});	
	mFrameBuffers.clear();
	mFramebufferInfos.clear();
}

bool IVKFrameBuffers::Init(IVKRenderContext* rc, const IFrameBuffersDesc* desc)
{
	mDesc = *desc;
	mRenderPass.StrongRef(desc->RenderPass);
	mRenderContext.FromObject(rc);
	return true;
}

void IVKFrameBuffers::BindSwapChain(UINT index, ISwapChain* swapchain)
{
	mSwapChainIndex = index;
	mSwapChain.StrongRef(swapchain);

	/*IVKRenderTargetView* pRTV = new IVKRenderTargetView();
	auto pTexture = swapchain->GetBackBuffer(0);
	IRenderTargetViewDesc rtvDesc;
	rtvDesc.SetTexture2D();
	rtvDesc.Format = pTexture->mTextureDesc.Format;
	rtvDesc.Width = pTexture->mTextureDesc.Width;
	rtvDesc.Height = pTexture->mTextureDesc.Height;
	rtvDesc.mGpuBuffer = pTexture;
	pRTV->Init(mRenderContext.GetPtr(), &rtvDesc);
	Safe_Release(mRenderTargets[index]);
	mRenderTargets[index] = pRTV;*/
}

VkFramebuffer IVKFrameBuffers::GetFrameBuffer()
{
	if (mFrameBuffers.size() == 0)
		return nullptr;
	auto pVKSwapChain = this->mSwapChain.UnsafeConvertTo<IVKSwapChain>();
	if (pVKSwapChain != nullptr)
	{
		ASSERT(pVKSwapChain->mCurrentImageIndex < mFrameBuffers.size());
		return mFrameBuffers[pVKSwapChain->mCurrentImageIndex];
	}
	else
	{
		return mFrameBuffers[0];
	}
}

void IVKFrameBuffers::OnBeginPass(IVKRenderContext* rc, IVKCommandList* cmdlist)
{
	for (int i = 0; i < MAX_MRT_NUM; i++)
	{
		if (mRenderTargets[i] == nullptr)
			continue;

		auto gpuBuffer = ((IVKRenderTargetView*)mRenderTargets[i])->mTextureSRV.UnsafeConvertTo<IVKShaderResourceView>()->GetGpuBuffer();
		auto pTexture = (IVKTexture2D*)gpuBuffer;
		pTexture->TransitionImageToLayout(rc, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL, cmdlist->mCommandBuffer);
	}

	if (m_pDepthStencilView != nullptr)
	{
		auto gpuBuffer = ((IVKDepthStencilView*)m_pDepthStencilView)->mTextureSRV.UnsafeConvertTo<IVKShaderResourceView>()->GetGpuBuffer();
		auto pTexture = (IVKTexture2D*)gpuBuffer;
		pTexture->TransitionImageToLayout(rc, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL, cmdlist->mCommandBuffer);
	}
	
	/*for (auto& i : mReferSRV)
	{
		auto pTexture = (IVKTexture2D*)i->GetGpuBuffer();
		if (pTexture->mTextureDesc.BindFlags & BF_DEPTH_STENCIL)
		{
			pTexture->TransitionImageLayout(rc, pTexture->mVkImage, Format2VKFormat(pTexture->mTextureDesc.Format),
				VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL);
		}
		else
		{
			pTexture->TransitionImageLayout(rc, pTexture->mVkImage, Format2VKFormat(pTexture->mTextureDesc.Format),
				VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL_KHR);
		}
	}*/
}

void IVKFrameBuffers::OnEndPass(IVKRenderContext* rc, IVKCommandList* cmdlist)
{
	for (int i = 0; i < MAX_MRT_NUM; i++)
	{
		if (mRenderTargets[i] == nullptr)
			continue;

		auto gpuBuffer = ((IVKRenderTargetView*)mRenderTargets[i])->mTextureSRV.UnsafeConvertTo<IVKShaderResourceView>()->GetGpuBuffer();
		auto pTexture = (IVKTexture2D*)gpuBuffer;
		pTexture->TransitionImageToLayout(rc, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, cmdlist->mCommandBuffer);
	}

	if (m_pDepthStencilView != nullptr)
	{
		auto gpuBuffer = ((IVKDepthStencilView*)m_pDepthStencilView)->mTextureSRV.UnsafeConvertTo<IVKShaderResourceView>()->GetGpuBuffer();
		auto pTexture = (IVKTexture2D*)gpuBuffer;
		pTexture->TransitionImageToLayout(rc, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, cmdlist->mCommandBuffer);
	}
	/*for (auto& i : mReferSRV)
	{
		auto pTexture = (IVKTexture2D*)i->GetGpuBuffer();
		if (pTexture->mTextureDesc.BindFlags & BF_DEPTH_STENCIL)
		{
			pTexture->TransitionImageLayout(rc, pTexture->mVkImage, Format2VKFormat(pTexture->mTextureDesc.Format),
				VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
		}
		else
		{
			pTexture->TransitionImageLayout(rc, pTexture->mVkImage, Format2VKFormat(pTexture->mTextureDesc.Format),
				VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL_KHR, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL);
		}
	}*/
}

bool IVKFrameBuffers::UpdateFrameBuffers(IRenderContext* InRc, float width, float height)
{
	IVKRenderContext* rc = (IVKRenderContext*)InRc;
	PostVkExecute([FrameBuffers = mFrameBuffers, ReferSRV = mReferSRV](IVKRenderContext* rc)
	{
		for (auto& i : FrameBuffers)
		{
			vkDestroyFramebuffer(rc->mLogicalDevice, i, rc->GetVkAllocCallBacks());
		}
	});
	
	mFrameBuffers.clear();
	mFramebufferInfos.clear();
	mReferSRV.clear();

	if (mSwapChain == nullptr || mSwapChainIndex < 0)
	{
		UpdateFrameBuffersWithSwapChain(rc, mRenderPass, 0, width, height);
	}
	else
	{
		for (UINT i = 0; i < mSwapChain->GetBackBufferNum(); i++)
		{
			UpdateFrameBuffersWithSwapChain(rc, mRenderPass, i, width, height);
		}
	}
	return true;
}

bool IVKFrameBuffers::UpdateFrameBuffersWithSwapChain(IVKRenderContext* rc, IRenderPass* renderPass, UINT iBackBuffer, float width, float height)
{
	std::vector<VkImageView> attachments;
	for (UINT i = 0; i < renderPass->mDesc.NumOfMRT; i++)
	{
		IVKShaderResourceView* srv;
		if (i == mSwapChainIndex)
		{
			srv = mSwapChain.UnsafeConvertTo<IVKSwapChain>()->GetBackSRV(iBackBuffer);
			auto pVkView = srv->mImageView;
			attachments.push_back(pVkView);
		}
		else
		{
			srv = ((IVKRenderTargetView*)mRenderTargets[i])->mTextureSRV;
			auto pVkView = srv->mImageView;
			attachments.push_back(pVkView);
		}
		if (width > srv->mTxDesc.Width)
		{
			width = (float)srv->mTxDesc.Width;
		}
		if (height > srv->mTxDesc.Height)
		{
			height = (float)srv->mTxDesc.Height;
		}
		AutoRef<IVKShaderResourceView> tmp;
		tmp.StrongRef(srv);
		mReferSRV.push_back(tmp);
	}
	if (this->m_pDepthStencilView != nullptr)
	{
		auto srv = ((IVKDepthStencilView*)m_pDepthStencilView)->mTextureSRV;
		attachments.push_back(srv->mImageView);

		if (width > srv->mTxDesc.Width)
		{
			width = (float)srv->mTxDesc.Width;
		}
		if (height > srv->mTxDesc.Height)
		{
			height = (float)srv->mTxDesc.Height;
		}
		AutoRef<IVKShaderResourceView> tmp;
		tmp.StrongRef(srv);
		mReferSRV.push_back(tmp);
	}
	if (attachments.size() == 0)
		return true;
	VkFramebufferCreateInfo framebufferInfo{};
	framebufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
	framebufferInfo.renderPass = ((IVKRenderPass*)renderPass)->mRenderPass;
	framebufferInfo.attachmentCount = static_cast<uint32_t>(attachments.size());
	framebufferInfo.pAttachments = &attachments[0];
	framebufferInfo.width = (UINT)width;
	framebufferInfo.height = (UINT)height;
	framebufferInfo.layers = 1;

	VkFramebuffer vkfb;
	if (vkCreateFramebuffer(rc->mLogicalDevice, &framebufferInfo, rc->GetVkAllocCallBacks(), &vkfb) != VK_SUCCESS)
	{
		return false;
	}
	Width = (UINT)width;
	Height = (UINT)height;
	mFramebufferInfos.push_back(framebufferInfo);
	mFrameBuffers.push_back(vkfb);
	return true;
}

IVKRenderPass::IVKRenderPass()
{
	mRenderPass = nullptr;
}

IVKRenderPass::~IVKRenderPass()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mRenderPass != nullptr)
	{
		vkDestroyRenderPass(rc->mLogicalDevice, mRenderPass, rc->GetVkAllocCallBacks());
		mRenderPass = nullptr;
	}
}

bool IVKRenderPass::Init(IVKRenderContext* rc, const IRenderPassDesc* desc)
{
	mRenderContext.FromObject(rc);
	mDesc = *desc;

	std::vector<VkAttachmentDescription> attachments;
	std::vector<VkAttachmentReference> attachmentRefs;
	VkAttachmentReference depthAttachmentRef{};
	for (UINT i = 0; i < desc->NumOfMRT; i++)
	{
		VkAttachmentDescription colorAttachment{};

		colorAttachment.format = Format2VKFormat(desc->AttachmentMRTs[i].Format);
		colorAttachment.samples = VK_SAMPLE_COUNT_1_BIT;
		colorAttachment.loadOp = FrameBufferLoadAction2VK(desc->AttachmentMRTs[i].LoadAction);
		colorAttachment.storeOp = FrameBufferStoreAction2VK(desc->AttachmentMRTs[i].StoreAction);
		colorAttachment.stencilLoadOp = FrameBufferLoadAction2VK(desc->AttachmentMRTs[i].StencilLoadAction);
		colorAttachment.stencilStoreOp = FrameBufferStoreAction2VK(desc->AttachmentMRTs[i].StencilStoreAction);
		colorAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		if (desc->AttachmentMRTs[i].IsSwapChain)
		{
			colorAttachment.finalLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
		}
		else
		{
			colorAttachment.finalLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;//VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
		}
		attachments.push_back(colorAttachment);

		VkAttachmentReference colorAttachmentRef{};
		colorAttachmentRef.attachment = i;
		colorAttachmentRef.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
		attachmentRefs.push_back(colorAttachmentRef);
	}
	
	if (desc->AttachmentDepthStencil.Format != PXF_UNKNOWN)
	{
		VkAttachmentDescription depthAttachment{};
		depthAttachment.format = Format2VKFormat(desc->AttachmentDepthStencil.Format);
		depthAttachment.samples = VK_SAMPLE_COUNT_1_BIT;
		depthAttachment.loadOp = FrameBufferLoadAction2VK(desc->AttachmentDepthStencil.LoadAction);;
		depthAttachment.storeOp = FrameBufferStoreAction2VK(desc->AttachmentDepthStencil.StoreAction);
		depthAttachment.stencilLoadOp = FrameBufferLoadAction2VK(desc->AttachmentDepthStencil.StencilLoadAction);;
		depthAttachment.stencilStoreOp = FrameBufferStoreAction2VK(desc->AttachmentDepthStencil.StencilStoreAction);;
		depthAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		depthAttachment.finalLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
		
		depthAttachmentRef.attachment = (UINT)attachments.size();
		depthAttachmentRef.layout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

		attachments.push_back(depthAttachment);
	}
	
	VkSubpassDescription subpass{};
	subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
	subpass.colorAttachmentCount = desc->NumOfMRT;
	if (desc->NumOfMRT > 0)
		subpass.pColorAttachments = &attachmentRefs[0];
	else
		subpass.pColorAttachments = nullptr;
	if (desc->AttachmentDepthStencil.Format != PXF_UNKNOWN)
	{
		subpass.pDepthStencilAttachment = &depthAttachmentRef;
	}
	//subpass.inputAttachmentCount TBDR GPU optimizer
	//subpass.pInputAttachments
	//https://zhuanlan.zhihu.com/p/131392827

	VkSubpassDependency dependency{};
	dependency.srcSubpass = VK_SUBPASS_EXTERNAL;
	dependency.dstSubpass = 0;
	dependency.srcStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT | VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
	dependency.srcAccessMask = 0;
	dependency.dstStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT | VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
	dependency.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;

	VkRenderPassCreateInfo renderPassInfo{};
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
	renderPassInfo.attachmentCount = static_cast<uint32_t>(attachments.size());
	renderPassInfo.pAttachments = &attachments[0];
	renderPassInfo.subpassCount = 1;
	renderPassInfo.pSubpasses = &subpass;
	renderPassInfo.dependencyCount = 1;
	renderPassInfo.pDependencies = &dependency;

	if (vkCreateRenderPass(rc->mLogicalDevice, &renderPassInfo, rc->GetVkAllocCallBacks(), &mRenderPass) != VK_SUCCESS)
	{
		return false;
	}

	return true;
}

NS_END