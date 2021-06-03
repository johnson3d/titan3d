#include "IBlendState.h"

NS_BEGIN

IBlendState::IBlendState()
{
}


IBlendState::~IBlendState()
{
}

void IBlendState::SetDesc(const IBlendStateDesc* desc)
{
	//mDesc.AlphaToCoverageEnable = desc.AlphaToCoverageEnable;
	//mDesc.IndependentBlendEnable = desc.IndependentBlendEnable;
	//mDesc.RenderTarget = desc.RenderTarget;
	mDesc = *desc;
}

NS_END

