#pragma once
#include "../NxShader.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12Shader : public IShader
	{
	public:
		static bool CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion);

		DX12Shader();
		~DX12Shader();
		bool Init(DX12GpuDevice* device, FShaderDesc* desc);
		static bool Reflect(FShaderDesc* desc, ID3D12ShaderReflection* pReflection);
	public:
		
	};
}

NS_END