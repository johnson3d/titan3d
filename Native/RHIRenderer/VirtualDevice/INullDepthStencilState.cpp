#include "INullDepthStencilState.h"

#define new VNEW

NS_BEGIN

INullDepthStencilState::INullDepthStencilState()
{
}


INullDepthStencilState::~INullDepthStencilState()
{
	
}

bool INullDepthStencilState::Init(const IDepthStencilStateDesc* desc)
{
	mDesc = *desc;
	return true;
}

NS_END

