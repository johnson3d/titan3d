#include "GfxRenderPolicy.h"
#include "Scene/GfxSceneView.h"
#include "Scene/GfxSceneRenderLayer.h"

#define new VNEW

namespace EngineNS
{
	RTTI_IMPL(EngineNS::GfxRenderPolicy, EngineNS::VIUnknown);

	GfxRenderPolicy::GfxRenderPolicy()
	{
	}

	GfxRenderPolicy::~GfxRenderPolicy()
	{
	}

	//void GfxRenderPolicy::DoPolicy(ICommandList* cmd, GfxSceneView* view)
	//{
		//cmd->BeginCommand();
		//cmd->SetRenderTargets(view->GetFrameBuffer());
		////view->CmdClearMRT(cmd);
		///*if (view->mClearColors.size() > 0)
		//{
		//	cmd->ClearMRT(&view->mClearColors[0], (int)view->mClearColors.size(), view->mClearDepth, view->mClearDepth,
		//		view->mClearStencil, view->mStencil);
		//}*/

		//cmd->ClearPasses();
		//auto pipes = view->GetDrawPipes();
		//for (auto i : pipes)
		//{
		//	auto& drawCalls = i->GetRHIPassArrayRef();
		//	for (auto j : drawCalls)
		//	{
		//		cmd->PushPass(j);
		//	}
		//}
		//cmd->EndCommand();
	//}
}