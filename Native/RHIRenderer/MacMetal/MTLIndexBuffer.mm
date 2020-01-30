#include "MTLIndexBuffer.h"

#define new VNEW

NS_BEGIN

MtlIndexBuffer::MtlIndexBuffer()
{
	m_pIndexBuffer = nil;
	mMtlIndexType = MTLIndexTypeUInt16;
	mMtlIndexCount = 0;
}

MtlIndexBuffer::~MtlIndexBuffer()
{
}

void MtlIndexBuffer::GetBufferData(IRenderContext* pCtx, IBlobObject* pCpuBuffer)
{
	if (mDesc.InitData != nullptr)
	{
		pCpuBuffer->ReSize(mDesc.ByteWidth);
		memcpy(pCpuBuffer->GetData(), mDesc.InitData, mDesc.ByteWidth);
	}
	else
	{
		pCpuBuffer->ReSize(mDesc.ByteWidth);
		memcpy(pCpuBuffer->GetData(), m_pIndexBuffer.contents, mDesc.ByteWidth);
	}
}

MTLIndexType TranslateIndexType_RHI2Mtl(EIndexBufferType index_type)
{
	static const MTLIndexType MtlIndexTypeArray[2] =
	{
		MTLIndexTypeUInt16,
		MTLIndexTypeUInt32
	};
	UINT32 idx = (UINT32)index_type;
	return MtlIndexTypeArray[idx];
}

bool MtlIndexBuffer::Init(MtlContext* pCtx, const IIndexBufferDesc* pDesc)
{
	mDesc.ByteWidth = pDesc->ByteWidth;
	mDesc.CPUAccess = pDesc->CPUAccess;
	mDesc.Type = pDesc->Type;

	MTLResourceOptions ResOptions = MTLResourceCPUCacheModeDefaultCache;

	if (pDesc->InitData != nullptr)
	{
		m_pIndexBuffer = [pCtx->m_pDevice newBufferWithBytes : (const void*)pDesc->InitData length : (NSUInteger)pDesc->ByteWidth options : ResOptions];
		mDesc.InitData = new BYTE[pDesc->ByteWidth];
		memcpy(mDesc.InitData, pDesc->InitData, pDesc->ByteWidth);
	}
	else
	{
		m_pIndexBuffer = [pCtx->m_pDevice newBufferWithLength : (NSUInteger)pDesc->ByteWidth options : ResOptions];
		mDesc.InitData = nullptr;
	}

	mMtlIndexType = TranslateIndexType_RHI2Mtl(pDesc->Type);
	NSUInteger IndexByteLength = mMtlIndexType == MTLIndexTypeUInt16 ? 2 : 4;
	mMtlIndexCount = [m_pIndexBuffer length] / IndexByteLength;

	return true;
}


NS_END