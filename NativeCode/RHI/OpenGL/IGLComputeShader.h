#pragma once
#include "../IComputeShader.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;

class IGLComputeShader : public IComputeShader
{
public:
	IGLComputeShader();
	~IGLComputeShader();
	virtual void Cleanup() override;
public:
	std::shared_ptr<GLSdk::GLBufferId>	mProgram;
	std::shared_ptr<GLSdk::GLBufferId>	mShader;
public:
	bool Init(IGLRenderContext* rc, const IShaderDesc* desc);
};

NS_END