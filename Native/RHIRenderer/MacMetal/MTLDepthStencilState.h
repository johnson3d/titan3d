#pragma once
#include "../IDepthStencilState.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlDepthStencilState : public IDepthStencilState
{
public:
	MtlDepthStencilState();
	~MtlDepthStencilState();

public:
	bool Init(MtlContext* pCtx, const IDepthStencilStateDesc* pDesc);

public:
	id<MTLDepthStencilState> m_pDepthStencilState;
	UINT32 mStencilRefValue;
};

NS_END