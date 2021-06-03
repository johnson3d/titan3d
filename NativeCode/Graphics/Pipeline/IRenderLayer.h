#pragma once

#include "../../RHI/RHI.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IRenderLayer : public VIUnknown
{
public:
	TR_CONSTRUCTOR()
	IRenderLayer()
	{

	}
	TR_FUNCTION()
	void PushDrawCall(IDrawCall* primitive) {
		primitive->AddRef();
		mPrimitive.push_back(primitive);
	}
	TR_FUNCTION()
	void ClearPrimitives()
	{
		for (auto i : mPrimitive)
		{
			i->Release();
		}
		mPrimitive.clear();
	}
	TR_FUNCTION()
	void DrawPrimitives(ICommandList* cmdlist);
protected:
	std::vector<IDrawCall*>		mPrimitive;
};

NS_END