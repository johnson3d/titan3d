#pragma once

#include "IRenderResource.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8)
	RenderTargetBlendDesc
{
	RenderTargetBlendDesc()
	{
		SetDefault();
	}
	void SetDefault()
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

struct TR_CLASS(SV_LayoutStruct = 8)
	IBlendStateDesc
{
	IBlendStateDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		AlphaToCoverageEnable = FALSE;
		IndependentBlendEnable = FALSE;
		//NumOfRT = 1;
		for (int i = 0; i < 8; i++)
		{
			RenderTarget[i].SetDefault();
		}
	}
	vBOOL AlphaToCoverageEnable;
	vBOOL IndependentBlendEnable;
	RenderTargetBlendDesc RenderTarget[8];
	//UINT NumOfRT;
};

class TR_CLASS()
	IBlendState : public IRenderResource
{
public:
	IBlendState();
	~IBlendState();

	TR_FUNCTION()
	void GetDesc(IBlendStateDesc* desc)
	{
		*desc = mDesc;
	}
	void SetDesc(const IBlendStateDesc* desc);
protected:
	IBlendStateDesc		mDesc;
};

NS_END