#pragma once
#include "../../Base/IUnknown.h"
#include "../../NextRHI/NxShader.h"

#include "../../../3rd/native/ShaderConductor/Include/ShaderConductor/ShaderConductor.hpp"

NS_BEGIN

class MemStreamWriter;
struct IBlobObject;
class TR_CLASS()
	IShaderConductor : public IWeakRefObject
{
public:
	static IShaderConductor* GetInstance();	
	static const int typeMaxBinding = 50;
	IShaderConductor()
	{

	}
	bool CompileShader(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* shader, const char* entry, NxRHI::EShaderType type, const char* sm,
				const NxRHI::IShaderDefinitions* defines, bool bDebugShader, NxRHI::EShaderLanguage sl, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule);
	bool CompileDXR(NxRHI::FShaderCompiler* compiler, IBlobObject* lib, const char* sm = "lib_6_5", const NxRHI::IShaderDefinitions* defines = nullptr, bool bDebugShader = true, NxRHI::EShaderLanguage sl = NxRHI::EShaderLanguage::SL_DXIL);
private:
	bool CompileHLSL(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* hlsl, const char* entry, NxRHI::EShaderType type, std::string sm,
		const NxRHI::IShaderDefinitions* defines, NxRHI::EShaderLanguage sl, bool debugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule);
};

NS_END
