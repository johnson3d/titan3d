#pragma once
#include "../IConstantBuffer.h"
#include "MTLRenderContext.h"

NS_BEGIN

class ITextureBase;
class MtlContext;

class MtlConstBuffer : public IConstantBuffer
{
public:
	MtlConstBuffer();
	~MtlConstBuffer();

	virtual bool UpdateContent(ICommandList* pCmdList, void* pBuffer, UINT Size) override;

public:
	bool Init(MtlContext* pCtx, const IConstantBufferDesc* pDesc);

public:
	id<MTLBuffer> m_pConstBuffer;
	id<MTLBuffer> m_pNilBuffer;
};

NS_END