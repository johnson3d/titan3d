#include "GraphicsProfiler.h"
#include "ICommandList.h"
#include "IPass.h"
#include "IPixelShader.h"
#include "IFrameBuffers.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GraphicsProfiler, EngineNS::VIUnknown);

GraphicsProfiler::GraphicsProfiler()
{
	mNoPixelWrite = FALSE;
	mNoPixelShader = FALSE;
}


GraphicsProfiler::~GraphicsProfiler()
{
}

void GraphicsProfiler::SetEmptyPixelShader(IPixelShader* ps)
{
	mPSEmpty.StrongRef(ps);
}

void GraphicsProfiler::SetOnePixelFrameBuffers(IFrameBuffers* fb)
{
	mOnePixelFB.StrongRef(fb);
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, GraphicsProfiler, SetEmptyPixelShader, IPixelShader*);
	CSharpAPI1(EngineNS, GraphicsProfiler, SetOnePixelFrameBuffers, IFrameBuffers*);

	CSharpReturnAPI0(vBOOL, EngineNS, GraphicsProfiler, GetNoPixelWrite);
	CSharpAPI1(EngineNS, GraphicsProfiler, SetNoPixelWrite, vBOOL);

	CSharpReturnAPI0(vBOOL, EngineNS, GraphicsProfiler, GetNoPixelShader);
	CSharpAPI1(EngineNS, GraphicsProfiler, SetNoPixelShader, vBOOL);
}