#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct ISamplerStateDesc
{
	ISamplerStateDesc()
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

class ISamplerState : public IRenderResource
{
public:
	ISamplerState();
	~ISamplerState();

public:
	ISamplerStateDesc			mDesc;
};

NS_END