#pragma once
#include "../IRasterizerState.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlRasterState : public IRasterizerState
{
public:
	MtlRasterState();
	~MtlRasterState();

public:
	bool Init(MtlContext* pCtx, const IRasterizerStateDesc* pDesc);

public:
	
	MTLTriangleFillMode mFillMode;
	MTLCullMode mCullMode;
	MTLWinding mFrontFaceWinding;
	float mDepthBias;
	float mDepthBiasClamp;
	float mSlopeScaledDepthBias;
	//MTLDepthClipMode mDepthClipMode
};

NS_END