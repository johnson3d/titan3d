#include "IVKPass.h"
#include "IVKCommandList.h"
#include "IVKRenderPipeline.h"
#include "IVKRenderContext.h"
#include "IVKConstantBuffer.h"

#define new VNEW

NS_BEGIN

IVKPass::IVKPass()
{
	mCBDescSet = nullptr;
}

IVKPass::~IVKPass()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mCBDescSet != nullptr)
	{
		vkFreeDescriptorSets(rc->mLogicalDevice, mDescriptorPool, 1, &mCBDescSet);
		mCBDescSet = nullptr;
	}
}

bool IVKPass::Init(IVKRenderContext* rc)
{
	mRenderContext.FromObject(rc);
	auto device = rc->mLogicalDevice;

	VkDescriptorPoolSize poolSize = {};
	poolSize.type = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	poolSize.descriptorCount = MaxCB;

	VkDescriptorPoolCreateInfo poolInfo = {};
	poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
	poolInfo.poolSizeCount = 1;
	poolInfo.pPoolSizes = &poolSize;
	poolInfo.maxSets = 1;

	if (vkCreateDescriptorPool(device, &poolInfo, nullptr, &mDescriptorPool) != VK_SUCCESS) 
	{
		return false;
	}

	VkDescriptorSetAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
	allocInfo.descriptorPool = mDescriptorPool;
	allocInfo.descriptorSetCount = 1;
	allocInfo.pSetLayouts = &rc->mCBDescSetLayout;

	if (vkAllocateDescriptorSets(device, &allocInfo, &mCBDescSet) != VK_SUCCESS)
	{
		//throw std::runtime_error("failed to allocate descriptor sets!");
		return false;
	}
	return true;
}

void IVKPass::UpdateCBufferLayoutSet(IVKRenderContext* rc)
{
	VkDescriptorBufferInfo bufferInfo[MaxCB];
	memset(bufferInfo, 0, sizeof(bufferInfo));
	for (int i = 0; i < MaxCB; i++)
	{
		auto vkCB = (IVKConstantBuffer*)CBuffersVS[0];
		if(vkCB ==nullptr)
			continue;

		bufferInfo[i].buffer = vkCB->mBuffer;
		bufferInfo[i].offset = 0;
		bufferInfo[i].range = vkCB->Desc.Size;
	}

	VkWriteDescriptorSet descriptorWrite = {};
	descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
	descriptorWrite.dstSet = mCBDescSet;
	descriptorWrite.dstBinding = 0;
	descriptorWrite.dstArrayElement = 0;
	descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	descriptorWrite.descriptorCount = MaxCB;
	descriptorWrite.pBufferInfo = bufferInfo;

	vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);
}

void IVKPass::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	//////////////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////////////
	
}

void IVKPass::SetViewport(ICommandList* cmd, IViewPort* vp)
{

}

void IVKPass::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{

}

//����Ⱦ״̬
void IVKPass::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline)
{
	
}

void IVKPass::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{

}

void IVKPass::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{

}

void IVKPass::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void IVKPass::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void IVKPass::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void IVKPass::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void IVKPass::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void IVKPass::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void IVKPass::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void IVKPass::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void IVKPass::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{

}

NS_END