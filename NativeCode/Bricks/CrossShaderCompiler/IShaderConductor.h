#pragma once
#include "../../Base/IUnknown.h"
#include "../../NextRHI/NxShader.h"

#include "../../../3rd/native/ShaderConductor/Include/ShaderConductor/ShaderConductor.hpp"

NS_BEGIN

class MemStreamWriter;

class TR_CLASS()
	IShaderConductor : public IWeakReference
{
public:
	static IShaderConductor* GetInstance();	
	IShaderConductor()
	{

	}
	bool CompileShader(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* shader, const char* entry, NxRHI::EShaderType type, const char* sm,
				const NxRHI::IShaderDefinitions* defines, bool bDebugShader, NxRHI::EShaderLanguage sl, bool debugShader, const char* extHlslVersion);
private:
	bool CompileHLSL(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* hlsl, const char* entry, NxRHI::EShaderType type, std::string sm,
				const NxRHI::IShaderDefinitions* defines, NxRHI::EShaderLanguage sl, bool debugShader, const char* extHlslVersion);
};

NS_END
