#include "GfxSceneView.h"
#include "GfxSceneRenderLayer.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxSceneView, EngineNS::VIUnknown);

GfxSceneView::GfxSceneView()
{
	mViewportPosId = -1;
}

GfxSceneView::~GfxSceneView()
{
	Cleanup();
}

void GfxSceneView::Cleanup()
{
	for (auto i : mSceneRenderLayer)
	{
		i->Cleanup();
		i->Release();
	}
	mSceneRenderLayer.clear();
}

vBOOL GfxSceneView::Init(IRenderContext* rc, UINT width, UINT height)
{
	mSceneRenderLayer.resize(RL_Num);
	for (int i = 0; i < RL_Num; i++)
	{
		mSceneRenderLayer[i] = new GfxSceneRenderLayer();
	}

	//rc.CreateConstantBuffer(, index);
	return TRUE;
}

//void GfxSceneView::ClearMRT(ICommandList* pCmdList, const std::pair<BYTE, DWORD>* ClearColors, int ColorNum,
//	bool bClearDepth, float Depth, bool bClearStencil, UINT32 Stencil)
//{
//	mClearColors.resize(ColorNum);
//	for (int i = 0; i < ColorNum; i++)
//	{
//		mClearColors[i] = ClearColors[i];
//	}
//	mClearDepth = bClearDepth;
//	mDepth = Depth;
//	mClearStencil = bClearStencil;
//	mStencil = Stencil;
//
//	if (mClearColors.size() > 0)
//	{
//		pCmdList->ClearMRT(&mClearColors[0], (int)mClearColors.size(), mClearDepth, mClearDepth,
//			mClearStencil, mStencil);
//	}
//}

void GfxSceneView::SetViewport(IViewPort* vp)
{
	mViewport.StrongRef(vp);

	//UpdateConstBufferData();
}

void GfxSceneView::ResizeViewport(UINT TopLeftX, UINT TopLeftY,UINT width, UINT height)
{
	mViewport->TopLeftX = (float)TopLeftX;
	mViewport->TopLeftY = (float)TopLeftY;
	mViewport->Width = (float)width;
	mViewport->Height = (float)height;

	//UpdateConstBufferData();
}

//void GfxSceneView::UpdateConstBufferData()
//{
//	v3dxVector2 ViewportPos;
//	ViewportPos.x = (float)mViewport->TopLeftX;
//	ViewportPos.y = (float)mViewport->TopLeftY;
//	mConstBuffer->SetVarValuePtr(mViewportPosId, &ViewportPos, sizeof(ViewportPos), 0);
//}

void GfxSceneView::SetFrameBuffers(IFrameBuffers* fb)
{
	mFrameBuffer.StrongRef(fb);
}

void GfxSceneView::BindConstBuffer(IConstantBuffer* cbuffer)
{
	mConstBuffer.StrongRef(cbuffer);
	mViewportPosId = mConstBuffer->FindVar("ViewportInfo");
}

void GfxSceneView::ClearSpecRenderLayerData(ERenderLayer channel)
{
	if (channel >= (int)mSceneRenderLayer.size())
		return;

	auto& drawcalls = mSceneRenderLayer[channel]->GetRHIPassArrayRef();
	for (auto i : drawcalls)
	{
		Safe_Release(i);
	}
	auto saved = drawcalls.size();
	if (saved > 0)
	{
		drawcalls.clear();
		drawcalls.reserve(saved);
	}
}

void GfxSceneView::SendPassToCorrectRenderLayer(ERenderLayer channel, IPass* pass)
{
	ASSERT(pass);
	if (channel >= (int)mSceneRenderLayer.size())
		return;

	if (mSceneRenderLayer.size() == 0)
		return;
	auto& refPassArray = mSceneRenderLayer[channel]->GetRHIPassArrayRef();
	pass->AddRef();
	pass->BindViewPort(this->mViewport);
	refPassArray.push_back(pass);
}

void GfxSceneView::PushSpecRenderLayerDataToRHI(ICommandList* pCmdList, ERenderLayer index)
{
	if (index >= (ERenderLayer)mSceneRenderLayer.size())
	{
		return;
	}
		
	auto& refPassArray = mSceneRenderLayer[index]->GetRHIPassArrayRef();
	
	for (UINT32 idx = 0; idx< refPassArray.size(); idx++)
	{
		pCmdList->PushPass(refPassArray[idx]);
	}
	
	ClearSpecRenderLayerData(index);
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS3(EngineNS, GfxSceneView, Init);
	Cpp2CS1(EngineNS, GfxSceneView, SetViewport);
	Cpp2CS2(EngineNS, GfxSceneView, SendPassToCorrectRenderLayer);
	Cpp2CS1(EngineNS, GfxSceneView, SetFrameBuffers);
	Cpp2CS1(EngineNS, GfxSceneView, BindConstBuffer);
	
	Cpp2CS4(EngineNS, GfxSceneView, ResizeViewport);

	typedef const std::pair<BYTE, DWORD>* ClearColorArg;
	Cpp2CS0(EngineNS, GfxSceneView, GetRenderLayerSize);
	Cpp2CS2(EngineNS, GfxSceneView, PushSpecRenderLayerDataToRHI);
}