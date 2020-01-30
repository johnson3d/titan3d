#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct IRasterizerStateDesc
{
	IRasterizerStateDesc()
	{
		FillMode = FMD_SOLID;
		CullMode = CMD_BACK;
		FrontCounterClockwise = FALSE;
		DepthBias = 0;
		DepthBiasClamp = 0;
		SlopeScaledDepthBias = 0;
		DepthClipEnable = FALSE;
		ScissorEnable = FALSE;
		MultisampleEnable = FALSE;
		AntialiasedLineEnable = FALSE;
	}
	EFillMode FillMode;
	ECullMode CullMode;
	vBOOL FrontCounterClockwise;
	INT DepthBias;
	FLOAT DepthBiasClamp;
	FLOAT SlopeScaledDepthBias;
	vBOOL DepthClipEnable;
	vBOOL ScissorEnable;
	vBOOL MultisampleEnable;
	vBOOL AntialiasedLineEnable;
};

class IRasterizerState : public IRenderResource
{
public:
	IRasterizerState();
	~IRasterizerState();

	void GetDesc(IRasterizerStateDesc* desc)
	{
		*desc = mDesc;
	}
public:
	IRasterizerStateDesc		mDesc;
};

NS_END