#pragma once
#include "IModifier.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IStaticModifier : public IModifier
{
public:
	TR_CONSTRUCTOR()
	IStaticModifier();
	virtual void SetInputStreams(IMeshPrimitives* mesh, IVertexArray* vao);
	virtual void GetInputStreams(UINT *  pOutStreams);
	virtual void GetProvideStreams(UINT *  pOutStreams);
};

NS_END