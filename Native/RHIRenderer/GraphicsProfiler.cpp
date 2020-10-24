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
	Cpp2CS1(EngineNS, GraphicsProfiler, SetEmptyPixelShader);
	Cpp2CS1(EngineNS, GraphicsProfiler, SetOnePixelFrameBuffers);

	Cpp2CS0(EngineNS, GraphicsProfiler, GetNoPixelWrite);
	Cpp2CS1(EngineNS, GraphicsProfiler, SetNoPixelWrite);

	Cpp2CS0(EngineNS, GraphicsProfiler, GetNoPixelShader);
	Cpp2CS1(EngineNS, GraphicsProfiler, SetNoPixelShader);
}