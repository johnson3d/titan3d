#pragma once
#include "../GfxPreHead.h"

namespace EngineNS
{
	class GfxSceneRenderLayer : public VIUnknown
	{
	public:
		GfxSceneRenderLayer();
		~GfxSceneRenderLayer();

		virtual void Cleanup() override;

		auto& GetRHIPassArrayRef() { return mRHIPassArray; }

	protected:
		std::vector<IPass*>			mRHIPassArray;
	};
}

