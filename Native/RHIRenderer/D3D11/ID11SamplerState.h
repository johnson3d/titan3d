#pragma once
#include "../ISamplerState.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class ID11SamplerState : public ISamplerState
{
public:
	ID11SamplerState();
	~ID11SamplerState();

public:
	D3D11_SAMPLER_DESC				mD11Desc;
	ID3D11SamplerState*				mSampler;
public:
	bool Init(ID11RenderContext* rc, const ISamplerStateDesc* desc);
};

NS_END