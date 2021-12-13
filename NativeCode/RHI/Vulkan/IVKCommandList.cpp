#include "IVKCommandList.h"
#include "IVKPass.h"
#include "IVKRenderContext.h"
#include "IVKFrameBuffers.h"
#include "IVKSwapChain.h"
#include "IVKRenderPipeline.h"
#include "IVKVertexBuffer.h"
#include "IVKIndexBuffer.h"
#include "IVKGpuBuffer.h"
#include "IVKShaderResourceView.h"
#include "IVKFence.h"

#define new VNEW

NS_BEGIN

IVKCommandList::IVKCommandList()
{
	mCommandBuffer = nullptr;
	mFence = nullptr;
	mPassRendering = false;
	mIsRecording = false;

	mVkViewport.x = 0;
	mVkViewport.y = 0;
	mVkViewport.width = 1;
	mVkViewport.height = 1;
	mVkViewport.minDepth = 0;
	mVkViewport.maxDepth = 1;

	mCanCommit = false;
}

IVKCommandList::~IVKCommandList()
{
	if (mCommandBuffer != nullptr)
	{
		PostVkExecute([CommandBuffer = mCommandBuffer, Fence = mFence](IVKRenderContext* rc)
		{
			vkQueueWaitIdle(rc->mGraphicsQueue);
			vkFreeCommandBuffers(rc->mLogicalDevice, rc->mCommandPool, 1, &CommandBuffer);
			//vkDestroyFence(rc->mLogicalDevice, Fence, rc->GetVkAllocCallBacks());
		});
		
		mCommandBuffer = nullptr;

		mFence = nullptr;
	}
}

bool IVKCommandList::BeginCommand()
{
	auto rc = (IVKRenderContext*)mRHIContext.GetPtr();
	if (rc == nullptr)
		return false;
	ICommandList::BeginCommand();

	vkWaitForFences(rc->mLogicalDevice, 1, &mFence, VK_TRUE, UINT64_MAX);

	vkResetCommandBuffer(mCommandBuffer, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
	VkCommandBufferBeginInfo beginInfo{};
	beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	//beginInfo.flags =
	if (vkBeginCommandBuffer(mCommandBuffer, &beginInfo) != VK_SUCCESS)
	{
		return false;
	}
	vkCmdSetViewport(mCommandBuffer, 0, 1, &mVkViewport);
	mIsRecording = true;
	return true;
}

void IVKCommandList::EndCommand()
{
	if (mIsRecording)
		vkEndCommandBuffer(mCommandBuffer);
	mIsRecording = false;
	mCanCommit = true;
	ICommandList::EndCommand();
}

bool IVKCommandList::BeginRenderPass(IFrameBuffers* pFrameBuffer, const IRenderPassClears* passClears, const char* debugName)
{
	auto pVKFrameBuffers = ((IVKFrameBuffers*)pFrameBuffer);
	mFrameBuffer.StrongRef(pFrameBuffer);
	{
		pVKFrameBuffers->OnBeginPass((IVKRenderContext*)mRHIContext.GetPtr(), this);
	}
	IRenderPass* pRenderPass = pFrameBuffer->mRenderPass;
	if (mIsRecording == false)
		return false;
	
	VkRenderPassBeginInfo renderPassInfo{};
	renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
	renderPassInfo.renderPass = ((IVKRenderPass*)pRenderPass)->mRenderPass;
	renderPassInfo.framebuffer = pVKFrameBuffers->GetFrameBuffer();
	if (renderPassInfo.framebuffer == nullptr)
		return false;
	
	renderPassInfo.renderArea.offset = { 0, 0 };
	renderPassInfo.renderArea.extent.width = pVKFrameBuffers->Width;
	renderPassInfo.renderArea.extent.height = pVKFrameBuffers->Height;

	VkClearValue clearValues[9]{};
	int NumOfClear = pRenderPass->mDesc.NumOfMRT;	
	if (passClears != nullptr)
	{
		for (int i = 0; i < NumOfClear; i++)
		{
			memcpy(&clearValues[i].color, &passClears->ClearColor[i], sizeof(v3dxColor4));
		}
		if (pRenderPass->mDesc.AttachmentDepthStencil.Format != PXF_UNKNOWN)
		{
			clearValues[NumOfClear].depthStencil = { passClears->DepthClearValue, passClears->StencilClearValue };
			NumOfClear++;
		}
	}

	renderPassInfo.clearValueCount = NumOfClear;// pRenderPass->mDesc.NumOfMRT;
	renderPassInfo.pClearValues = clearValues;

	mPassRendering = true;
	vkCmdBeginRenderPass(mCommandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
	return true;
}

void IVKCommandList::EndRenderPass()
{
	if (mPassRendering)
	{
		vkCmdEndRenderPass(mCommandBuffer);
	}
	mPassRendering = false;
	auto pVKFrameBuffers = mFrameBuffer.UnsafeConvertTo<IVKFrameBuffers>();
	if (pVKFrameBuffers != nullptr)
	{
		pVKFrameBuffers->OnEndPass((IVKRenderContext*)mRHIContext.GetPtr(), this);
		mFrameBuffer.Clear();
	}
}

void IVKCommandList::ExecCommited()
{
	VAutoVSLLock locker(ComitLocker);
	for (auto i : Commited)
	{
		i(this);
	}
	Commited.clear();
}

void IVKCommandList::Commit(IRenderContext* pRHICtx)
{
	if (mCanCommit == false)
		return;

	mCanCommit = false;
	auto rc = (IVKRenderContext*)pRHICtx;

	VkSubmitInfo submitInfo{};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;

	VkSemaphore waitSemaphores[] = { nullptr };
	VkPipelineStageFlags waitStages[] = { VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT };
	submitInfo.waitSemaphoreCount = 0;//1
	submitInfo.pWaitSemaphores = waitSemaphores;
	submitInfo.pWaitDstStageMask = waitStages;

	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &mCommandBuffer;

	submitInfo.signalSemaphoreCount = 1;
	submitInfo.pSignalSemaphores = &mSemaphore.UnsafeConvertTo<IVKSemaphore>()->mSemaphore;// &mFinishedSemaphore;

	vkResetFences(rc->mLogicalDevice, 1, &mFence);

	if (vkQueueSubmit(rc->mGraphicsQueue, 1, &submitInfo, mFence) != VK_SUCCESS)
	{
		return;
	}
 	ExecCommited();
	/*VkSemaphoreWaitInfo wi{};
	wi.flags = VK_STRUCTURE_TYPE_SEMAPHORE_WAIT_INFO;
	wi.semaphoreCount = 1;
	wi.pSemaphores = &mFinishedSemaphore;
	UINT64 time = UINT64_MAX;
	wi.pValues = &time;
	vkWaitSemaphores(rc->mLogicalDevice, &wi, UINT64_MAX);*/
}

void IVKCommandList::SetRasterizerState(IRasterizerState* State)
{

}

void IVKCommandList::SetDepthStencilState(IDepthStencilState* State)
{

}

void IVKCommandList::SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask)
{

}

void IVKCommandList::SetComputeShader(IComputeShader* ComputerShader)
{

}

void IVKCommandList::CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{
	
}

void IVKCommandList::CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT* pUAVInitialCounts)
{
	
}

void IVKCommandList::CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
{
	
}

void IVKCommandList::CSDispatch(UINT x, UINT y, UINT z)
{

}

void IVKCommandList::CSDispatchIndirect(IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{

}

void IVKCommandList::SetScissorRect(IScissorRect* sr)
{
	mScissors.StrongRef(sr);	

	/*if (sr == nullptr)
	{
		VkRect2D scissor{};
		scissor.offset = { (int)mVkViewport.x, (int)(mVkViewport.y - mVkViewport.height) };
		scissor.extent = { (UINT)mVkViewport.width, (UINT)(-mVkViewport.height) };
		vkCmdSetScissor(mCommandBuffer, 0, 1, &scissor);
		return;
	}

	VkRect2D* pRects = (VkRect2D*)alloca(sizeof(VkRect2D) * sr->Rects.size());
	for (size_t i = 0; i < sr->Rects.size(); i++)
	{
		pRects[i].offset = { sr->Rects[i].MinX, sr->Rects[i].MinY };
		pRects[i].extent = { (UINT)(sr->Rects[i].MaxX - sr->Rects[i].MinX), (UINT)(sr->Rects[i].MaxY - sr->Rects[i].MinY) };
	}
	vkCmdSetScissor(mCommandBuffer, 0, (UINT)sr->Rects.size(), pRects);*/
}

void IVKCommandList::SetVertexBuffer(UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	VkDeviceSize offset = Offset;
	if (VertexBuffer == nullptr)
	{	
		if (mCurVB != nullptr)
		{
			auto& buffer = mCurVB->mBuffer->mBuffer;
			vkCmdBindVertexBuffers(mCommandBuffer, StreamIndex, 1, &buffer, &offset);
		}
		return;
	}
	mCurVB.StrongRef((IVKVertexBuffer*)VertexBuffer);
	auto& buffer = ((IVKVertexBuffer*)VertexBuffer)->mBuffer->mBuffer;
	vkCmdBindVertexBuffers(mCommandBuffer, StreamIndex, 1, &buffer, &offset);
}

void IVKCommandList::SetIndexBuffer(IIndexBuffer* IndexBuffer)
{
	if (IndexBuffer == nullptr)
	{
		return;
	}
	auto mIndexBuffer = ((IVKIndexBuffer*)IndexBuffer)->mBuffer->mBuffer;
	auto mIndexType = (IndexBuffer->mDesc.Type == IBT_Int16)? VK_INDEX_TYPE_UINT16 : VK_INDEX_TYPE_UINT32;

	vkCmdBindIndexBuffer(mCommandBuffer, mIndexBuffer, 0, mIndexType);
}

void IVKCommandList::DrawPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	UINT indexCount = NumPrimitives * 3;

	if (PrimitiveType != EPT_TriangleList)
	{
		PrimitiveTopology2VK(PrimitiveType, NumPrimitives, indexCount);
		//vkCmdSetPrimitiveTopologyEXT(mCommandBuffer, PrimitiveTopology2VK(PrimitiveType, NumPrimitives, indexCount));
	}
	vkCmdDraw(mCommandBuffer, indexCount, NumInstances, BaseVertexIndex, 0);
}

void IVKCommandList::DrawIndexedPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	UINT indexCount = NumPrimitives * 3;
	
	if (PrimitiveType != EPT_TriangleList)
	{
		PrimitiveTopology2VK(PrimitiveType, NumPrimitives, indexCount);
		//vkCmdSetPrimitiveTopologyEXT(mCommandBuffer, PrimitiveTopology2VK(PrimitiveType, NumPrimitives, indexCount));
	}
	//vkCmdSetViewport(mCommandBuffer, 0, 1, &mVkViewport);
	auto sr = mScissors;
	if (sr == nullptr)
	{
		VkRect2D scissor{};
		scissor.offset = { (int)mVkViewport.x, (int)(mVkViewport.y + mVkViewport.height) };
		scissor.extent = { (UINT)mVkViewport.width, (UINT)(-mVkViewport.height) };
		vkCmdSetScissor(mCommandBuffer, 0, 1, &scissor);
	}
	else
	{
		VkRect2D* pRects = (VkRect2D*)alloca(sizeof(VkRect2D) * sr->Rects.size());
		for (size_t i = 0; i < sr->Rects.size(); i++)
		{
			pRects[i].offset = { sr->Rects[i].MinX, sr->Rects[i].MinY };
			pRects[i].extent = { (UINT)(sr->Rects[i].MaxX - sr->Rects[i].MinX), (UINT)(sr->Rects[i].MaxY - sr->Rects[i].MinY) };
		}
		vkCmdSetScissor(mCommandBuffer, 0, (UINT)sr->Rects.size(), pRects);
	}
	vkCmdDrawIndexed(mCommandBuffer, indexCount, NumInstances, StartIndex, BaseVertexIndex, 0);
}

void IVKCommandList::DrawIndexedInstancedIndirect(EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	vkCmdDrawIndexedIndirect(mCommandBuffer, ((IVKGpuBuffer*)pBufferForArgs)->mBuffer, AlignedByteOffsetForArgs, 1, sizeof(UINT)*5);
}

void IVKCommandList::IASetInputLayout(IInputLayout* pInputLayout)
{

}

void IVKCommandList::VSSetShader(IVertexShader* pVertexShader, void** ppClassInstances, UINT NumClassInstances)
{

}

void IVKCommandList::PSSetShader(IPixelShader* pPixelShader, void** ppClassInstances, UINT NumClassInstances)
{

}

void IVKCommandList::SetViewport(IViewPort* vp)
{
	mVkViewport.x = vp->TopLeftX;
	//mVkViewport.y = vp->TopLeftY;
	mVkViewport.y = vp->TopLeftY + vp->Height;
	mVkViewport.width = vp->Width;
	//mVkViewport.height = vp->Height;
	mVkViewport.height = -vp->Height;
	mVkViewport.minDepth = vp->MinDepth;
	mVkViewport.maxDepth = vp->MaxDepth;
	if (mIsRecording)
		vkCmdSetViewport(mCommandBuffer, 0, 1, &mVkViewport);
}

void IVKCommandList::VSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
{

}

void IVKCommandList::PSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
{

}

void IVKCommandList::VSSetShaderResource(UINT32 Index, IShaderResourceView* pSRV)
{

}

void IVKCommandList::PSSetShaderResource(UINT32 Index, IShaderResourceView* pSRV)
{

}

void IVKCommandList::VSSetSampler(UINT32 Index, ISamplerState* Sampler)
{

}

void IVKCommandList::PSSetSampler(UINT32 Index, ISamplerState* Sampler)
{

}

void IVKCommandList::SetRenderPipeline(IRenderPipeline* pipeline, EPrimitiveType dpType)
{
	((IVKRenderPipeline*)pipeline)->OnSetPipeline(this, dpType);
}

vBOOL IVKCommandList::CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers)
{
	return FALSE;
}

bool IVKCommandList::Init(IVKRenderContext* rc, const ICommandListDesc* desc)
{
	mRHIContext.FromObject(rc);

	VkCommandBufferAllocateInfo allocInfo{};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.commandPool = rc->mCommandPool;
	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	allocInfo.commandBufferCount = 1;

	if (vkAllocateCommandBuffers(rc->mLogicalDevice, &allocInfo, &mCommandBuffer) != VK_SUCCESS)
	{
		return false;
	}

	VkFenceCreateInfo fenceInfo{};
	fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
	fenceInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;
	if (vkCreateFence(rc->mLogicalDevice, &fenceInfo, rc->GetVkAllocCallBacks(), &mFence) != VK_SUCCESS)
	{
		return false;
	}

	/*VkSemaphoreTypeCreateInfoKHR type_info = { VK_STRUCTURE_TYPE_SEMAPHORE_TYPE_CREATE_INFO_KHR };
	type_info.semaphoreType = VK_SEMAPHORE_TYPE_TIMELINE_KHR;
	type_info.initialValue = 0;
	VkSemaphoreCreateInfo semaphoreInfo{};
	semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	semaphoreInfo.pNext = &type_info;
	vkCreateSemaphore(rc->mLogicalDevice, &semaphoreInfo, rc->GetVkAllocCallBacks(), &mFinishedSemaphore);*/
	return true;
}

NS_END