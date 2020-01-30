#pragma once
#include "../IVertexBuffer.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlVertexBuffer : public IVertexBuffer
{
public:
	MtlVertexBuffer();
	~MtlVertexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(IRenderContext* rc, void* ptr, UINT size) override;
public:
	bool Init(MtlContext* pCtx, const IVertexBufferDesc* pDesc);

public:
	id<MTLBuffer> m_pVtxBuffer;
};

NS_END