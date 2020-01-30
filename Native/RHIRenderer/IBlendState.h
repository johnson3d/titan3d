#pragma once

#include "IRenderResource.h"

NS_BEGIN

enum EBlend
{
	BLD_ZERO = 1,
	BLD_ONE = 2,
	BLD_SRC_COLOR = 3,
	BLD_INV_SRC_COLOR = 4,
	BLD_SRC_ALPHA = 5,
	BLD_INV_SRC_ALPHA = 6,
	BLD_DEST_ALPHA = 7,
	BLD_INV_DEST_ALPHA = 8,
	BLD_DEST_COLOR = 9,
	BLD_INV_DEST_COLOR = 10,
	BLD_SRC_ALPHA_SAT = 11,
	BLD_BLEND_FACTOR = 14,
	BLD_INV_BLEND_FACTOR = 15,
	BLD_SRC1_COLOR = 16,
	BLD_INV_SRC1_COLOR = 17,
	BLD_SRC1_ALPHA = 18,
	BLD_INV_SRC1_ALPHA = 19
};

enum EBlendOp
{
	BLDOP_ADD = 1,
	BLDOP_SUBTRACT = 2,
	BLDOP_REV_SUBTRACT = 3,
	BLDOP_MIN = 4,
	BLDOP_MAX = 5
};

struct RenderTargetBlendDesc
{
	RenderTargetBlendDesc()
	{
		BlendEnable = FALSE;
		SrcBlend = BLD_SRC_ALPHA;
		DestBlend = BLD_INV_SRC_ALPHA;
		BlendOp = BLDOP_ADD;
		SrcBlendAlpha = BLD_SRC_ALPHA;
		DestBlendAlpha = BLD_INV_SRC_ALPHA;
		BlendOpAlpha = BLDOP_ADD;
		RenderTargetWriteMask = 0x0F;
	}
	vBOOL BlendEnable;
	EBlend SrcBlend;
	EBlend DestBlend;
	EBlendOp BlendOp;
	EBlend SrcBlendAlpha;
	EBlend DestBlendAlpha;
	EBlendOp BlendOpAlpha;
	UINT8 RenderTargetWriteMask;
};

struct IBlendStateDesc
{
	IBlendStateDesc()
	{
		AlphaToCoverageEnable = FALSE;
		IndependentBlendEnable = FALSE;
	}
	vBOOL AlphaToCoverageEnable;
	vBOOL IndependentBlendEnable;
	RenderTargetBlendDesc RenderTarget[8];
};

class IBlendState : public IRenderResource
{
public:
	IBlendState();
	~IBlendState();

	void GetDesc(IBlendStateDesc* desc)
	{
		*desc = mDesc;
	}
	void SetDesc(const IBlendStateDesc* desc);
protected:
	IBlendStateDesc		mDesc;
};

NS_END