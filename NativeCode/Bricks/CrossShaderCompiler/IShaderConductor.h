#pragma once
#include "../../Base/IUnknown.h"
#include "../../RHI/IShader.h"

#include "../../../3rd/native/ShaderConductor/Include/ShaderConductor/ShaderConductor.hpp"

NS_BEGIN

class IShaderDesc;
class IShaderDefinitions;
class MemStreamWriter;

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl, SV_NameSpace = EngineNS)
typedef MemStreamWriter* (*FGetShaderCodeStream)(void* includeName);

VTypeHelperDefine(FGetShaderCodeStream, sizeof(void*));
StructBegin(FGetShaderCodeStream, EngineNS)
StructEnd(void)

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShaderConductor : public VIUnknown
{
public:
	FGetShaderCodeStream			GetShaderCodeStream;
public:
	static IShaderConductor* GetInstance();	
	TR_CONSTRUCTOR()
	IShaderConductor()
	{

	}
	TR_FUNCTION()
	void SetCodeStreamGetter(FGetShaderCodeStream fn)
	{
		GetShaderCodeStream = fn;
	}
	TR_FUNCTION()
	bool CompileShader(IShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm,
				const IShaderDefinitions* defines, bool bDebugShader, bool bDxbc, bool bGlsl, bool bMetal);

	bool CompileHLSL(IShaderDesc* desc, const char* hlsl, const char* entry, EShaderType type, std::string sm,
				const IShaderDefinitions* defines, bool hasGLSL, bool hasMetal);
};

NS_END
