#include "IVKGpuBuffer.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"

#define new VNEW

NS_BEGIN

IVKGpuBuffer::IVKGpuBuffer()
{
	mBuffer = nullptr;
}

IVKGpuBuffer::~IVKGpuBuffer()
{
	if (mBuffer != nullptr)
	{
		PostVkExecute([buffer = mBuffer, mem = mMemory](IVKRenderContext* rc)
		{
			vkDestroyBuffer(rc->mLogicalDevice, buffer, rc->GetVkAllocCallBacks());
			VK_FreeGpuMemory(rc, mem);
		});		
	}
	mBuffer = nullptr;
	mMemory = nullptr;
}

bool IVKGpuBuffer::Init(IVKRenderContext* rc, const IGpuBufferDesc* desc)
{
	mRenderContext.FromObject(rc);
	mBufferDesc = *desc;

	//VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT

	VkBufferUsageFlags usage = 0;
	if (desc->Type & GBT_VertexBuffer)
	{
		usage |= VK_BUFFER_USAGE_VERTEX_BUFFER_BIT;
	}
	if (desc->Type & GBT_IndexBuffer)
	{
		usage |= VK_BUFFER_USAGE_INDEX_BUFFER_BIT;
	}
	if (desc->Type & GBT_UavBuffer)
	{
		usage |= VK_BUFFER_USAGE_STORAGE_BUFFER_BIT;
	}
	if (desc->Type & GBT_TBufferBuffer)
	{
		usage |= VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;
	}
	if (desc->Type & GBT_IndirectBuffer)
	{
		usage |= VK_BUFFER_USAGE_INDIRECT_BUFFER_BIT;
	}

	if (false == VK_CreateBuffer(rc, desc->ByteWidth, usage, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, mBuffer, mMemory))
		return false;
	return true;
}

vBOOL IVKGpuBuffer::GetBufferData(IRenderContext* rc, IBlobObject* blob)
{
	
	return TRUE;
}

vBOOL IVKGpuBuffer::Map(IRenderContext* rc,
	UINT Subresource,
	EGpuMAP MapType,
	UINT MapFlags,
	IMappedSubResource* mapRes)
{
	if (vkMapMemory(((IVKRenderContext*)rc)->mLogicalDevice, mMemory->GetDeviceMemory(), mMemory->Offset, mBufferDesc.ByteWidth, 0, &mapRes->pData) != VK_SUCCESS)
	{
		return FALSE;
	}
	mapRes->RowPitch = 0;
	mapRes->DepthPitch = 0;
	return TRUE;
}

void IVKGpuBuffer::Unmap(IRenderContext* rc, UINT Subresource)
{
	auto dxCtx = (IVKRenderContext*)rc;	
	vkUnmapMemory(((IVKRenderContext*)rc)->mLogicalDevice, mMemory->GetDeviceMemory());
}

vBOOL IVKGpuBuffer::UpdateBufferData(ICommandList* cmd, UINT offset, void* data, UINT size)
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return false;
	ASSERT(size <= mBufferDesc.ByteWidth);
	void* pBuffer;
	if (vkMapMemory(rc->mLogicalDevice, mMemory->GetDeviceMemory(), mMemory->Offset, mBufferDesc.ByteWidth, 0, &pBuffer) != VK_SUCCESS)
	{
		return false;
	}
	memcpy((BYTE*)pBuffer + offset, data, size);
	vkUnmapMemory(rc->mLogicalDevice, mMemory->GetDeviceMemory());
	return TRUE;
}

NS_END

