#include "IVKIndexBuffer.h"
#include "IVKRenderContext.h"
#include "IVKGpuBuffer.h"

#define new VNEW

NS_BEGIN

IVKIndexBuffer::IVKIndexBuffer()
{
	
}


IVKIndexBuffer::~IVKIndexBuffer()
{
	
}

void IVKIndexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{

}

void IVKIndexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	mBuffer->UpdateBufferData(cmd, 0, ptr, size);
}

bool IVKIndexBuffer::Init(IVKRenderContext* rc, const IIndexBufferDesc* desc)
{
	mDesc = *desc;

	auto pVB = new IVKGpuBuffer();
	IGpuBufferDesc vbDesc;
	vbDesc.SetDefault();
	vbDesc.Type = GBT_IndexBuffer;
	vbDesc.ByteWidth = desc->ByteWidth;
	vbDesc.CPUAccessFlags = desc->CPUAccess;
	//vbDesc.MiscFlags = ;
	vbDesc.Usage = EGpuUsage::USAGE_DEFAULT;
	vbDesc.StructureByteStride = (desc->Type == IBT_Int16) ? 2 : 4;
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

bool IVKIndexBuffer::Init(IVKRenderContext* rc, IVKGpuBuffer* pBuffer)
{
	mBuffer.WeakRef(pBuffer);
	return true;
}

NS_END