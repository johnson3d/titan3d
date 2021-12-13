#include "IVKRenderPipeline.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"
#include "IVKShaderProgram.h"
#include "IVKInputLayout.h"
#include "IVKRasterizerState.h"
#include "IVKDepthStencilState.h"
#include "IVKBlendState.h"
#include "IVKFrameBuffers.h"
#include "IVKPass.h"

#define new VNEW

NS_BEGIN

IVKRenderPipeline::IVKRenderPipeline()
{
	mGraphicsPipeline = nullptr;
}

IVKRenderPipeline::~IVKRenderPipeline()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mGraphicsPipeline != nullptr)
	{
		//vkDestroyPipeline(rc->mLogicalDevice, mGraphicsPipeline, rc->GetVkAllocCallBacks());
		mGraphicsPipeline = nullptr;
	}
}

void IVKRenderPipeline::SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout)
{
	//IRenderPipeline::SetRasterizerState(cmd, Layout);
}

void IVKRenderPipeline::SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State)
{
	//IRenderPipeline::SetDepthStencilState(cmd, State);
}

void IVKRenderPipeline::SetBlendState(ICommandList* cmd, IBlendState* State)
{
	//IRenderPipeline::SetBlendState(cmd, State);
}

bool IVKRenderPipeline::Init(IVKRenderContext* rc, const IRenderPipelineDesc* desc)
{
	mRenderContext.FromObject(rc);
	BindRenderPass(desc->RenderPass);
	BindBlendState(desc->Blend);
	BindDepthStencilState(desc->DepthStencil);
	BindGpuProgram(desc->GpuProgram);
	BindRasterizerState(desc->Rasterizer);
	return true;
}

void IVKRenderPipeline::OnSetPipeline(IVKCommandList* cmd, EPrimitiveType dpType)
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;

	if (this->mIsDirty == false && mGraphicsPipeline != nullptr)
	{
		vkCmdBindPipeline(cmd->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, mGraphicsPipeline);
		return;
	}
	
	IRenderPipelineDesc psoDesc;
	psoDesc.RenderPass = cmd->mFrameBuffer->mRenderPass;
	psoDesc.PrimitiveType = dpType;
	psoDesc.Blend = mBlendState;
	psoDesc.Rasterizer = mRasterizerState;
	psoDesc.GpuProgram = mGpuProgram;
	psoDesc.DepthStencil = mDepthStencilState;
	psoDesc.SampleMask = mSampleMask;
	psoDesc.RenderPass = mRenderPass;

	mGraphicsPipeline = rc->mPipelineManager->GetOrAddPipeline(&psoDesc, dpType, &cmd->mVkViewport);

	vkCmdBindPipeline(cmd->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, mGraphicsPipeline);
	this->mIsDirty = false;
}

//////////////////////////////////////////////////////////////////////////

IVKRenderPipelineManager::IVKRenderPipelineManager(IVKRenderContext* rc)
{
	mRenderContext = rc;
}

IVKRenderPipelineManager::~IVKRenderPipelineManager()
{
	Cleanup();
}

void IVKRenderPipelineManager::Cleanup()
{
	for (auto& i : mPipelines)
	{
		vkDestroyPipeline(mRenderContext->mLogicalDevice, i.second, mRenderContext->GetVkAllocCallBacks());
	}
	mPipelines.clear();
}

VkPipeline IVKRenderPipelineManager::GetOrAddPipeline(const IRenderPipelineDesc* desc, EPrimitiveType dpType, const VkViewport* vp)
{
	auto iter = mPipelines.find(*desc);
	if (iter != mPipelines.end())
	{
		return iter->second;
	}

	auto pShaderProgram = (IVKShaderProgram*)desc->GpuProgram;
	auto pInputLayout = (IVKInputLayout*)pShaderProgram->GetInputLayout();
	VkPipelineShaderStageCreateInfo shaderStages[] = { pShaderProgram->mVSCreateInfo, pShaderProgram->mPSCreateInfo };

	VkPipelineMultisampleStateCreateInfo multisampling{};
	multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
	multisampling.sampleShadingEnable = VK_FALSE;
	multisampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;

	VkPipelineInputAssemblyStateCreateInfo inputAssembly{};
	inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
	UINT indexCount;
	inputAssembly.topology = PrimitiveTopology2VK(dpType, 0, indexCount);
	inputAssembly.primitiveRestartEnable = VK_FALSE;

	VkPipelineViewportStateCreateInfo viewportState{};
	viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;

	VkRect2D scissor{};
	scissor.offset = { (int)vp->x, (int)(vp->y + vp->height) };
	scissor.extent = { (UINT)vp->width, (UINT)(-vp->height) };

	viewportState.viewportCount = 1;
	viewportState.pViewports = vp;
	viewportState.scissorCount = 1;
	viewportState.pScissors = &scissor;

	//viewportState.flags = 0;
	/*viewportState.viewportCount = 0;
	viewportState.pViewports = nullptr;
	viewportState.scissorCount = 0;
	viewportState.pScissors = nullptr;*/

	//vkCmdSetPrimitiveTopologyEXT()
	//vkCmdSetViewport()
	//vkCmdSetScissor()

	VkDynamicState dynVPState[2];
	dynVPState[0] = VK_DYNAMIC_STATE_VIEWPORT;
	dynVPState[1] = VK_DYNAMIC_STATE_SCISSOR;

	VkPipelineDynamicStateCreateInfo dynStateInfo{};
	dynStateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
	dynStateInfo.dynamicStateCount = 2;
	dynStateInfo.pDynamicStates = dynVPState;

	VkGraphicsPipelineCreateInfo pipelineInfo{};
	pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
	pipelineInfo.flags = 0;
	pipelineInfo.stageCount = 2;
	pipelineInfo.pStages = shaderStages;

	pipelineInfo.pInputAssemblyState = &inputAssembly;
	pipelineInfo.pViewportState = &viewportState;
	pipelineInfo.pMultisampleState = &multisampling;
	pipelineInfo.pDynamicState = &dynStateInfo;

	ASSERT(desc->Rasterizer != nullptr);
	{
		pipelineInfo.pRasterizationState = &((IVKRasterizerState*)desc->Rasterizer)->mCreateInfo;
	}
	ASSERT(desc->DepthStencil != nullptr);
	{
		pipelineInfo.pDepthStencilState = &((IVKDepthStencilState*)desc->DepthStencil)->CreateInfo;
	}
	ASSERT(desc->Blend != nullptr);
	{
		pipelineInfo.pColorBlendState = &((IVKBlendState*)desc->Blend)->ColorBlending;
		((VkPipelineColorBlendStateCreateInfo*)pipelineInfo.pColorBlendState)->attachmentCount = desc->RenderPass->mDesc.NumOfMRT;
	}

	ASSERT(pInputLayout != nullptr);
	pipelineInfo.pVertexInputState = &pInputLayout->CreateInfo;

	ASSERT(pShaderProgram->mPipelineLayout != nullptr);
	pipelineInfo.layout = pShaderProgram->mPipelineLayout;
	ASSERT(desc->RenderPass != nullptr);
	pipelineInfo.renderPass = ((IVKRenderPass*)desc->RenderPass)->mRenderPass;
	pipelineInfo.subpass = 0;
	pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;

	VkPipeline pipeline;
	if (vkCreateGraphicsPipelines(mRenderContext->mLogicalDevice, VK_NULL_HANDLE, 1, &pipelineInfo,
		mRenderContext->GetVkAllocCallBacks(), &pipeline) != VK_SUCCESS)
	{
		return nullptr;
	}

	mPipelines.insert(std::make_pair(*desc, pipeline));
	return pipeline;
}

NS_END