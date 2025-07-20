#pragma once
#include "../VKPreHead.h"

NS_BEGIN

class FSpirvProcessor
{
public:
	static void GlslToSpirv(const char* glsl, NxRHI::EShaderType shaderStage, std::vector<unsigned char>& data);
};

NS_END