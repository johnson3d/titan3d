#pragma once
#include "IRenderResource.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IFence : public IRenderResource
{
public:
	virtual void Wait() = 0;
	virtual bool IsCompletion() = 0;
};

NS_END