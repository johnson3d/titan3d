#pragma once
#include "PreHead.h"

NS_BEGIN

class ICommandList;
class IPixelShader;
class IRenderContext;
class IFrameBuffers;
class GraphicsProfiler : public VIUnknown
{
public:
	vBOOL					mNoPixelWrite;
	vBOOL					mNoPixelShader;
	AutoRef<IPixelShader>	mPSEmpty;
	AutoRef<IFrameBuffers>	mOnePixelFB;
public:
	RTTI_DEF(GraphicsProfiler, 0xf46200e35cbfc070, true);

	GraphicsProfiler();
	~GraphicsProfiler();

	void SetEmptyPixelShader(IPixelShader* ps);
	void SetOnePixelFrameBuffers(IFrameBuffers* fb);
	
	VDef_ReadWrite(vBOOL, NoPixelShader, m);
	VDef_ReadWrite(vBOOL, NoPixelWrite, m);
};

NS_END
