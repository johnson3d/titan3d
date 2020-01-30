#include "MTLVertexBuffer.h"

#define new VNEW

NS_BEGIN

MtlVertexBuffer::MtlVertexBuffer()
{
	m_pVtxBuffer = nil;
}


MtlVertexBuffer::~MtlVertexBuffer()
{
}

//get vertex buffer data;
void MtlVertexBuffer::GetBufferData(IRenderContext* pCtx, IBlobObject* pCpuBuffer)
{
	if (mDesc.InitData != nullptr)
	{
		pCpuBuffer->ReSize(mDesc.ByteWidth);
		memcpy(pCpuBuffer->GetData(), mDesc.InitData, mDesc.ByteWidth);
	}
	else
	{
		pCpuBuffer->ReSize(mDesc.ByteWidth);
		memcpy(pCpuBuffer->GetData(), m_pVtxBuffer.contents, mDesc.ByteWidth);
	}
}

//update vertex buffer data;
void MtlVertexBuffer::UpdateGPUBuffData(IRenderContext* pCtx, void* pCpuBuffer, UINT ByteSize)
{
	if (pCpuBuffer == nullptr)
	{
		return;
	}
	if (mDesc.ByteWidth < ByteSize)
	{
		AssertRHI(false);
		return;
	}
	memcpy(m_pVtxBuffer.contents, pCpuBuffer, ByteSize);
}

bool MtlVertexBuffer::Init(MtlContext* pCtx, const IVertexBufferDesc* pDesc)
{
	mDesc.ByteWidth = pDesc->ByteWidth;
	mDesc.CPUAccess = pDesc->CPUAccess;
	mDesc.Stride = pDesc->Stride;

	MTLResourceOptions ResOptions = MTLResourceCPUCacheModeDefaultCache;
	
	if (pDesc->InitData != nullptr)
	{
		m_pVtxBuffer = [pCtx->m_pDevice newBufferWithBytes : (const void*)pDesc->InitData length : (NSUInteger)pDesc->ByteWidth options : ResOptions];

		mDesc.InitData = new BYTE[pDesc->ByteWidth];
		memcpy(mDesc.InitData, pDesc->InitData, pDesc->ByteWidth);
	}
	else
	{
		m_pVtxBuffer = [pCtx->m_pDevice newBufferWithLength : (NSUInteger)pDesc->ByteWidth options : ResOptions];
		mDesc.InitData = nullptr;
	}
	
	return true;
}

NS_END