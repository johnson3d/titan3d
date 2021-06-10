#pragma once
#include "../../../RHI/RHI.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IModifier : public VIUnknown
{
public:
	virtual void SetInputStreams(IMeshPrimitives* mesh, IVertexArray* vao) {}
	virtual void GetInputStreams(UINT* pOutStreams) {}
	virtual void GetProvideStreams(UINT* pOutStreams) {}
};

NS_END