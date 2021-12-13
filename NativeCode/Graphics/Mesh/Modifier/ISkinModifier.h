#pragma once
#include "IModifier.h"
#include "../../../RHI/IConstantBuffer.h"
#include "../../../RHI/Utility/IMeshPrimitives.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
	ISkinModifier : public IModifier
{
public:
	TR_CONSTRUCTOR()
		ISkinModifier();
	virtual void SetInputStreams(IMeshPrimitives * mesh, IVertexArray * vao);
	virtual void GetInputStreams(UINT * pOutStreams);
	virtual void GetProvideStreams(UINT * pOutStreams);
};

NS_END