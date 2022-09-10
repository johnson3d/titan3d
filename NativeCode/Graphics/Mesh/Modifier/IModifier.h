#pragma once
#include "../../../NextRHI/NxRHI.h"

NS_BEGIN

class TR_CLASS()
	IModifier : public VIUnknown
{
public:
	virtual void SetInputStreams(NxRHI::FMeshPrimitives* mesh, NxRHI::FVertexArray* vao) {}
	virtual void GetInputStreams(UINT* pOutStreams) {}
	virtual void GetProvideStreams(UINT* pOutStreams) {}
};

NS_END