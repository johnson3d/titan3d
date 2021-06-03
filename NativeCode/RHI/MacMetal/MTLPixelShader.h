#pragma once
#include "../IPixelShader.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlPixelShader : public IPixelShader
{
public:
	MtlPixelShader();
	~MtlPixelShader();

public:
	bool Init(MtlContext* pCtx, const IShaderDesc* pDesc);

public:
	id<MTLFunction> m_pFragFunc;
};

NS_END