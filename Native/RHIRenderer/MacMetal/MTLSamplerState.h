#pragma once
#include "../ISamplerState.h"
#include "MTLRenderContext.h"


NS_BEGIN

class MtlContext;

class MtlSamplerState : public ISamplerState
{
public:
	MtlSamplerState();
	~MtlSamplerState();

public:
	bool Init(MtlContext* pCtx, const ISamplerStateDesc* pDesc);

public:
	id<MTLSamplerState> m_pSamplerState;
};

NS_END