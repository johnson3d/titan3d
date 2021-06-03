#include "IVKPass.h"
#include "IVKCommandList.h"
#include "IVKRenderPipeline.h"
#include "IVKRenderContext.h"
#include "IVKConstantBuffer.h"

#define new VNEW

NS_BEGIN

IVKDrawCall::IVKDrawCall()
{
	mCBDescSet = nullptr;
}

IVKDrawCall::~IVKDrawCall()
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

bool IVKDrawCall::Init(IVKRenderContext* rc)
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

void IVKDrawCall::UpdateCBufferLayoutSet(IVKRenderContext* rc)
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

void IVKDrawCall::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	//////////////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////////////
	
}

void IVKDrawCall::SetViewport(ICommandList* cmd, IViewPort* vp)
{

}

void IVKDrawCall::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{

}

//����Ⱦ״̬
void IVKDrawCall::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline)
{
	
}

void IVKDrawCall::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{

}

void IVKDrawCall::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{

}

void IVKDrawCall::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void IVKDrawCall::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void IVKDrawCall::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void IVKDrawCall::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void IVKDrawCall::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void IVKDrawCall::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void IVKDrawCall::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void IVKDrawCall::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void IVKDrawCall::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{

}

NS_END