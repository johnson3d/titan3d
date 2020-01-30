#pragma once
#include "../IPixelShader.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11PixelShader : public IPixelShader
{
public:
	ID11PixelShader();
	~ID11PixelShader();

public:
	ID3D11PixelShader*				mShader;
public:
	bool Init(ID11RenderContext* rc, const IShaderDesc* desc);
};

NS_END