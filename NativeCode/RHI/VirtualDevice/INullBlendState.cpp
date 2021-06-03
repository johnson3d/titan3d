#include "INullBlendState.h"

#define new VNEW

NS_BEGIN

INullBlendState::INullBlendState()
{
}


INullBlendState::~INullBlendState()
{
}

bool INullBlendState::Init(const IBlendStateDesc* desc)
{
	mDesc = *desc;
	return true;
}

NS_END