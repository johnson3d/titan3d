#pragma once
#include "GfxPreHead.h"

namespace EngineNS
{
	class GfxEngine : public VIUnknown
	{
	public:
		static GfxEngine*	Instance;
	public:
		RTTI_DEF(GfxEngine, 0xbdf907d25b023227, true);
		GfxEngine();
		~GfxEngine();

		virtual void Cleanup() override;

		static GfxEngine* GetInstance() {
			return Instance;
		}

		vBOOL Init(ERHIType RHIType, IRenderSystem* rs, IRenderContext* rc, const char* contentPath
			, const char* enginePath
			, const char* editorPath);

		VDef_ReadOnly(RenderSystem);
		VDef_ReadOnly(RenderContext);

		void SetEngineTime(vTimeTick time) {
			VIUnknown::EngineTime = time;
		}
		const std::string& GetContentPath() const {
			return mContentPath;
		}
		const std::string& GetEnginePath() const {
			return mEnginePath;
		}
		const std::string& GetEditorPath() const {
			return mEditorPath;
		}
	protected:
		AutoRef<IRenderSystem>		mRenderSystem;
		AutoRef<IRenderContext>		mRenderContext;
		std::string					mContentPath;
		std::string					mEnginePath;
		std::string					mEditorPath;
	};
}

