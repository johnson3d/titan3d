#pragma once
#include "../IBlendState.h"
#include "MTLRenderContext.h"
#include "../IFrameBuffers.h"

NS_BEGIN

class MtlContext;

struct MtlBlendStateDesc
{
	bool mEnable;
	MTLBlendOperation mBlendOperationRGB;
	MTLBlendOperation mBlendOperationAlpha;
	MTLBlendFactor mBlendFactorSrcRGB;
	MTLBlendFactor mBlendFactorSrcAlpha;
	MTLBlendFactor mBlendFactorDstRGB;
	MTLBlendFactor mBlendFactorDstAlpha;
	MTLColorWriteMask mWriteMask;
};


class MtlBlendState : public IBlendState
{
public:
	MtlBlendState();
	~MtlBlendState();

public:
	bool Init(MtlContext* pCtx, const IBlendStateDesc* pDesc);

public:
	MtlBlendStateDesc mMtlBlendStateDescArray[MAX_MRT_NUM];
};

NS_END