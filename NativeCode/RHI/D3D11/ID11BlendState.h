#pragma once

#include "../IBlendState.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11BlendState : public IBlendState
{
public:
	ID11BlendState();
	~ID11BlendState();
public:
	ID3D11BlendState*		mState;
public:
	bool Init(ID11RenderContext* rc, const IBlendStateDesc* desc);
};

NS_END