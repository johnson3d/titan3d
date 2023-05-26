#pragma once

#include "../../NextRHI/NxRHI.h"

NS_BEGIN

class TR_CLASS()
	IRenderLayer : public IWeakReference
{
public:
	IRenderLayer()
	{

	}
	void PushDrawCall(EngineNS::NxRHI::IGpuDraw* primitive) {
		primitive->AddRef();
		mPrimitive.push_back(primitive);
	}
	void ClearPrimitives()
	{
		for (auto i : mPrimitive)
		{
			i->Release();
		}
		mPrimitive.clear();
	}
protected:
	std::vector<NxRHI::IGpuDraw*>		mPrimitive;
};

NS_END