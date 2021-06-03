#pragma once
#include "../IVertexShader.h"
#include "MTLRenderContext.h"


NS_BEGIN

class MtlVertexShader : public IVertexShader
{
public:
	MtlVertexShader();
	~MtlVertexShader();

public:
	bool Init(MtlContext* pCtx, const IShaderDesc* pDesc);

public:
	id<MTLFunction> m_pVtxFunc;
};

NS_END