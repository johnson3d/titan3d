#include "IVKVertexBuffer.h"
#include "IVKRenderContext.h"
#include "IVKGpuBuffer.h"

#define new VNEW

NS_BEGIN

IVKVertexBuffer::IVKVertexBuffer()
{
}


IVKVertexBuffer::~IVKVertexBuffer()
{
	
}

void IVKVertexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{

}

void IVKVertexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	mBuffer->UpdateBufferData(cmd, 0, ptr, size);
}

bool IVKVertexBuffer::Init(IVKRenderContext* rc, const IVertexBufferDesc* desc)
{
	mDesc = *desc;

	auto pVB = new IVKGpuBuffer();
	IGpuBufferDesc vbDesc;
	vbDesc.SetDefault();
	vbDesc.Type = GBT_VertexBuffer;
	vbDesc.ByteWidth = desc->ByteWidth;
	vbDesc.CPUAccessFlags = desc->CPUAccess;
	//vbDesc.MiscFlags = ;
	vbDesc.Usage = EGpuUsage::USAGE_DEFAULT;
	vbDesc.StructureByteStride = desc->Stride;
	//vbDesc.BindFlags = ;
	if (pVB->Init(rc, &vbDesc) == false)
		return false;

	if (desc->InitData != nullptr)
	{
		pVB->UpdateBufferData(rc->GetImmCommandList(), 0, desc->InitData, desc->ByteWidth);
	}

	mBuffer.WeakRef(pVB);

	return true;
}

bool IVKVertexBuffer::Init(IVKRenderContext* rc, IVKGpuBuffer* pBuffer)
{
	mBuffer.WeakRef(pBuffer);
	return true;
}

NS_END