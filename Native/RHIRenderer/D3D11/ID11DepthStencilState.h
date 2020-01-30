#pragma once
#include "../IDepthStencilState.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11DepthStencilState : public IDepthStencilState
{
public:
	ID11DepthStencilState();
	~ID11DepthStencilState();
public:
	ID3D11DepthStencilState*		mState;
public:
	bool Init(ID11RenderContext* rc, const IDepthStencilStateDesc* desc);
};

NS_END