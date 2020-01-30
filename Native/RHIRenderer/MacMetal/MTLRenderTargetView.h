#pragma once
#include "../IRenderTargetView.h"
#include "MTLRenderContext.h"
#include "MTLTexture2D.h"


NS_BEGIN

class MtlContext;

class MtlRTV : public IRenderTargetView
{
public:
	MtlRTV();
	~MtlRTV();

public:
	bool Init(MtlContext* pCtx, const IRenderTargetViewDesc* pDesc);
};

NS_END