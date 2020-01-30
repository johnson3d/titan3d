#pragma once
#include "GfxPreHead.h"

namespace EngineNS
{
	class GfxSceneView;
	class GfxRenderPolicy : public VIUnknown
	{
	public:
		RTTI_DEF(GfxRenderPolicy, 0x9106e3e75b018930, true);
		GfxRenderPolicy();
		~GfxRenderPolicy();

		//virtual void DoPolicy(ICommandList* cmd, GfxViewPort* view);
	};
}