#pragma once
#include "../NxShader.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GpuDevice;
	class DX11Shader : public IShader
	{
	public:
		static bool CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader);

		DX11Shader();
		~DX11Shader();
		bool Init(DX11GpuDevice* device, FShaderDesc* desc);
		static bool Reflect(FShaderDesc* desc);
	public:
		union {
			ID3D11ComputeShader* mComputeShader;
			ID3D11VertexShader* mVertexShader;
			ID3D11PixelShader* mPixelShader;
		};
	};
}

NS_END