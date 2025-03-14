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
		static bool CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule);

		DX12Shader();
		~DX12Shader();
		bool Init(DX12GpuDevice* device, FShaderDesc* desc);
		static bool Reflect(FShaderDesc* desc, ID3D12ShaderReflection* pReflection);
		static bool Reflect(FShaderDesc* desc, ID3D12LibraryReflection* pReflection);
		static void CreateBinder(ID3D12ShaderReflectionConstantBuffer* pCBuffer, FShaderDesc* desc, D3D12_SHADER_INPUT_BIND_DESC& csibDesc);
		static void CreateCBufferFields(ID3D12ShaderReflectionConstantBuffer* pCBuffer, FShaderBinder* binder);
	public:
		
	};
}

NS_END