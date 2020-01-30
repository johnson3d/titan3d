#pragma once
#include "../IVertexShader.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11VertexShader : public IVertexShader
{
public:
	ID11VertexShader();
	~ID11VertexShader();

public:
	ID3D11VertexShader*			mShader;
public:
	bool Init(ID11RenderContext* rc, const IShaderDesc* desc);
};

NS_END