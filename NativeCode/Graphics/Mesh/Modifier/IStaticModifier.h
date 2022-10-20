#pragma once
#include "IModifier.h"

NS_BEGIN

class TR_CLASS()
	IStaticModifier : public IModifier
{
public:
	IStaticModifier();
	virtual void SetInputStreams(EngineNS::NxRHI::FMeshPrimitives* mesh, EngineNS::NxRHI::FVertexArray* vao);
	virtual void GetInputStreams(UINT *  pOutStreams);
	virtual void GetProvideStreams(UINT *  pOutStreams);
};

NS_END