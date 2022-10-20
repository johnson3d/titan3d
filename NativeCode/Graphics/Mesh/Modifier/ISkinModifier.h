#pragma once
#include "IModifier.h"
#include "../../../NextRHI/NxRHI.h"

NS_BEGIN

class TR_CLASS()
	ISkinModifier : public IModifier
{
public:
	ISkinModifier();
	virtual void SetInputStreams(EngineNS::NxRHI::FMeshPrimitives * mesh, EngineNS::NxRHI::FVertexArray * vao);
	virtual void GetInputStreams(UINT * pOutStreams);
	virtual void GetProvideStreams(UINT * pOutStreams);
};

NS_END