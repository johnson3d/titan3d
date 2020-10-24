#include "GfxEngine.h"
#include "../Core/io/vfxfile.h"
#include "../Core/thread/vfxthread.h"

#define new VNEW

Hash64	Hash64::Empty;

namespace EngineNS
{
	RTTI_IMPL(EngineNS::GfxEngine, EngineNS::VIUnknown);

	GfxEngine*	GfxEngine::Instance = nullptr;

	GfxEngine::GfxEngine()
	{
		Instance = this;
		CoreRttiManager::GetInstance()->BuildRtti();
	}

	GfxEngine::~GfxEngine()
	{
		Cleanup();
		Instance = nullptr;
	}

	void GfxEngine::Cleanup()
	{
		RResourceSwapChain::GetInstance()->TickSwap(mRenderContext);

		//DelayExecuter::Instance.Cleanup();
		if (mRenderContext != nullptr)
		{
			mRenderContext->Cleanup();
			mRenderContext.Clear();
		}
		if (mRenderSystem != nullptr)
		{
			mRenderSystem->Cleanup();
			mRenderSystem.Clear();
		}
	}

	vBOOL GfxEngine::Init(ERHIType RHIType, IRenderSystem* rs, IRenderContext* rc, 
		const char* contentPath, const char* enginePath , const char* editorPath)
	{
		mContentPath = contentPath;
		mEnginePath = enginePath;
		mEditorPath = editorPath;

		mRenderSystem.StrongRef(rs);
		mRenderContext.StrongRef(rc);
		return TRUE;
	}

	/*std::string GfxEngine::LoadTextFromFile(const char* file)
	{
		std::string text;
		ViseFile io;
		if (io.Open(file, VFile::modeRead) == FALSE)
			return "Error File";
		auto len = io.GetLength();
		text.resize(len + 1);
		io.Read(&text[0], len);
		text[len] = '\0';
		return text;
	}*/
}

using namespace EngineNS;

extern "C"
{
	VFX_API EngineNS::GfxEngine* SDK_GfxEngine_NewObject()
	{
		return new EngineNS::GfxEngine();
	}
	
	Cpp2CS6(EngineNS, GfxEngine, Init);
	Cpp2CS1(EngineNS, GfxEngine, SetEngineTime);
}