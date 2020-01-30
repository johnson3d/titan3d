#include "GfxSceneRenderLayer.h"

#define new VNEW

namespace EngineNS
{
	GfxSceneRenderLayer::GfxSceneRenderLayer()
	{
	}

	GfxSceneRenderLayer::~GfxSceneRenderLayer()
	{
		Cleanup();
	}

	void GfxSceneRenderLayer::Cleanup()
	{
		for (auto i : mRHIPassArray)
		{
			i->Release();
		}
		mRHIPassArray.clear();
	}
}