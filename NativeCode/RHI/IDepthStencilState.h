#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
StencilOpDesc
{
	EStencilOp StencilFailOp;
	EStencilOp StencilDepthFailOp;
	EStencilOp StencilPassOp;
	EComparisionMode StencilFunc;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IDepthStencilStateDesc
{
	IDepthStencilStateDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		DepthEnable = TRUE;
		DepthWriteMask = DSWM_ALL;
		DepthFunc = CMP_LESS_EQUAL;
		StencilEnable = FALSE;
		StencilReadMask = 0xFF;
		StencilWriteMask = 0xFF;

		FrontFace.StencilDepthFailOp = STOP_KEEP;
		FrontFace.StencilFailOp = STOP_KEEP;
		FrontFace.StencilFunc = CMP_NEVER;

		BackFace.StencilDepthFailOp = STOP_KEEP;
		BackFace.StencilFailOp = STOP_KEEP;
		BackFace.StencilFunc = CMP_NEVER;

		StencilRef = 0;
	}
	vBOOL DepthEnable;
	EDepthWriteMask DepthWriteMask;
	EComparisionMode DepthFunc;
	vBOOL StencilEnable;
	BYTE StencilReadMask;
	BYTE StencilWriteMask;
	StencilOpDesc FrontFace;
	StencilOpDesc BackFace;

	UINT					StencilRef;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IDepthStencilState : public IRenderResource
{
protected:
	IDepthStencilStateDesc		mDesc;
public:
	IDepthStencilState();
	~IDepthStencilState();

	TR_FUNCTION()
	void GetDesc(IDepthStencilStateDesc* desc)
	{
		*desc = mDesc;
	}
	const IDepthStencilStateDesc* GetDescPtr() const{
		return &mDesc;
	}
};

NS_END