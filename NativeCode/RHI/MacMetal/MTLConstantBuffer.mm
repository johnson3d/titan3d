#include "MTLConstantBuffer.h"

#define new VNEW

NS_BEGIN

MtlConstBuffer::MtlConstBuffer()
{
	m_pConstBuffer = nil;
}

MtlConstBuffer::~MtlConstBuffer()
{
}

bool MtlConstBuffer::UpdateContent(ICommandList* pCmdList, void* pRamBuffer, UINT ByteSize)
{
	memcpy(m_pConstBuffer.contents, pRamBuffer, ByteSize);
	return true;
}


bool MtlConstBuffer::Init(MtlContext* pCtx, const IConstantBufferDesc* pDesc)
{
	Desc = *pDesc;
	MTLResourceOptions ResOptions = MTLResourceCPUCacheModeDefaultCache;
	m_pConstBuffer = [pCtx->m_pDevice newBufferWithLength: pDesc->Size options: ResOptions];
	return true;
}
NS_END