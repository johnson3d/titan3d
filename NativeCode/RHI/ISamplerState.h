#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
ISamplerStateDesc
{
	ISamplerStateDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		Filter = SPF_MIN_MAG_MIP_LINEAR;
		CmpMode = CMP_NEVER;
		AddressU = ADM_WRAP;
		AddressV = ADM_WRAP;
		AddressW = ADM_WRAP;
		MaxAnisotropy = 0;
		MipLODBias = 0;
		RgbaToColor4(0, BorderColor);
		MinLOD = 0; 
		MaxLOD = 3.402823466e+38f;
	}
	ESamplerFilter		Filter;
	EComparisionMode	CmpMode;
	EAddressMode		AddressU;
	EAddressMode		AddressV;
	EAddressMode		AddressW;
	UINT				MaxAnisotropy;
	float				MipLODBias;
	float				BorderColor[4];
	float				MinLOD;
	float				MaxLOD;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ISamplerState : public IRenderResource
{
public:
	ISamplerState();
	~ISamplerState();

public:
	TR_MEMBER()
	ISamplerStateDesc			mDesc;
};

NS_END