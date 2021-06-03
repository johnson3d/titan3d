#pragma once
#include "../IRasterizerState.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11RasterizerState : public IRasterizerState
{
public:
	ID11RasterizerState();
	~ID11RasterizerState();
public:
	D3D11_RASTERIZER_DESC			mD11Desc;
	ID3D11RasterizerState*			mState;
public:
	bool Init(ID11RenderContext* rc, const IRasterizerStateDesc* desc);
};

NS_END