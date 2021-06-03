#pragma once
#include "../IComputeShader.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;

class IGLShaderReflector
{
public:
	static std::shared_ptr<GLSdk::GLBufferId> Reflect(IGLRenderContext* rc, const std::shared_ptr<GLSdk::GLBufferId>& shaderId, IShader* pShader);

	static void ReflectProgram(IGLRenderContext* rc, const std::shared_ptr<GLSdk::GLBufferId>& program, EShaderType shaderType, ShaderReflector* pReflector);
};

NS_END