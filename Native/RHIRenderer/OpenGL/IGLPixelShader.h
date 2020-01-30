#pragma once
#include "../IPixelShader.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLPixelShader : public IPixelShader
{
public:
	IGLPixelShader();
	~IGLPixelShader();

	virtual void Cleanup() override;
public:
	std::shared_ptr<GLSdk::GLBufferId>		mShader;
public:
	bool Init(IGLRenderContext* rc, const IShaderDesc* desc);
};

NS_END