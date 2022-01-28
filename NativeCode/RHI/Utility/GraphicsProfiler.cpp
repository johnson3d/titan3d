#include "GraphicsProfiler.h"
#include "../ICommandList.h"
#include "../IDrawCall.h"
#include "../IPixelShader.h"
#include "../IFrameBuffers.h"
#include "../ISamplerState.h"
#include "../IShaderResourceView.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::GraphicsProfiler);

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
