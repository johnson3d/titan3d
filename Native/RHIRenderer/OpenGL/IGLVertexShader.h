#pragma once
#include "../IVertexShader.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLVertexShader : public IVertexShader
{
public:
	IGLVertexShader();
	~IGLVertexShader();
	virtual void Cleanup() override;
public:
	std::shared_ptr<GLSdk::GLBufferId>	mShader;
public:
	bool Init(IGLRenderContext* rc, const IShaderDesc* desc);
};

NS_END