#pragma once
#include "../IComputeShader.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11ComputeShader : public IComputeShader
{
public:
	ID11ComputeShader();
	~ID11ComputeShader();

public:
	ID3D11ComputeShader*		mShader;
public:
	bool Init(ID11RenderContext* rc, const IShaderDesc* desc);
};

NS_END