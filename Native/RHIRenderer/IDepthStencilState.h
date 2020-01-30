#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct StencilOpDesc
{
	EStencilOp StencilFailOp;
	EStencilOp StencilDepthFailOp;
	EStencilOp StencilPassOp;
	EComparisionMode StencilFunc;
};

StructBegin(StencilOpDesc, EngineNS)
	StructMember(StencilFailOp);
	StructMember(StencilDepthFailOp);
	StructMember(StencilPassOp);
	StructMember(StencilFunc);
StructEnd(void)

struct IDepthStencilStateDesc
{
	IDepthStencilStateDesc()
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
	UINT8 StencilReadMask;
	UINT8 StencilWriteMask;
	StencilOpDesc FrontFace;
	StencilOpDesc BackFace;

	UINT					StencilRef;
};

StructBegin(IDepthStencilStateDesc, EngineNS)
	StructMember(DepthEnable);
	StructMember(DepthWriteMask);
	StructMember(DepthFunc);
	StructMember(StencilEnable);
	StructMember(StencilReadMask);
	StructMember(StencilWriteMask);
	StructMember(FrontFace);
	StructMember(BackFace);
	StructMember(StencilRef);
StructEnd(void)

class IDepthStencilState : public IRenderResource
{
protected:
	IDepthStencilStateDesc		mDesc;
public:
	IDepthStencilState();
	~IDepthStencilState();

	void GetDesc(IDepthStencilStateDesc* desc)
	{
		*desc = mDesc;
	}
	const IDepthStencilStateDesc* GetDescPtr() const{
		return &mDesc;
	}
};

NS_END