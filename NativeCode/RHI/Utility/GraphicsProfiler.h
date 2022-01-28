#pragma once
#include "../PreHead.h"

NS_BEGIN

class ICommandList;
class IPixelShader;
class IRenderContext;
class IFrameBuffers;

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
GraphicsProfiler : public VIUnknown
{
public:
	vBOOL					mNoPixelWrite;
	vBOOL					mNoPixelShader;
	AutoRef<IPixelShader>	mPSEmpty;
	AutoRef<IFrameBuffers>	mOnePixelFB;
public:
	ENGINE_RTTI(GraphicsProfiler);

	GraphicsProfiler();
	~GraphicsProfiler();

	void SetEmptyPixelShader(IPixelShader* ps);
	void SetOnePixelFrameBuffers(IFrameBuffers* fb);
};

NS_END
