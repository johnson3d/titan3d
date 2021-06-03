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

	virtual void SetDebugName(const char* name) override;
public:
	ID3D11PixelShader*				mShader;
public:
	bool Init(ID11RenderContext* rc, const IShaderDesc* desc);
};

NS_END