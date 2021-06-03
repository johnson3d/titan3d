#pragma once
#include "../IIndexBuffer.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlIndexBuffer : public IIndexBuffer
{
public:
	MtlIndexBuffer();
	~MtlIndexBuffer();

	virtual void GetBufferData(IRenderContext* pCtx, IBlobObject* pData) override;

public:
	bool Init(MtlContext* pCtx, const IIndexBufferDesc* pDesc);

public:
	id<MTLBuffer> m_pIndexBuffer;
	MTLIndexType mMtlIndexType;
	NSUInteger mMtlIndexCount;
};

NS_END