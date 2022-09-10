#pragma once
#include "../NxShader.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullGpuDevice;
	class NullShader : public IShader
	{
	public:
		static bool CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader);

		NullShader();
		~NullShader();
		bool Init(NullGpuDevice* device, FShaderDesc* desc);
		bool Reflect(FShaderDesc* desc);
	public:
	};
}

NS_END