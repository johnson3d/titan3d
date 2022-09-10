#include "NullShader.h"
#include "NullGpuDevice.h"
#include "../NxRHIDefine.h"

#define new VNEW

NS_BEGIN
namespace NxRHI
{
	bool NullShader::CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader)
	{
		AutoRef<FShaderCode> ar = compiler->GetShaderCodeStream(shader);
		if (ar == nullptr)
			return false;

		return true;
	}

	NullShader::NullShader()
	{
	}
	NullShader::~NullShader()
	{
	}
	bool NullShader::Init(NullGpuDevice* device, FShaderDesc* desc)
	{
		Desc = desc;
		Reflect(desc);

		return true;
	}

	bool NullShader::Reflect(FShaderDesc* desc)
	{
		return true;
	}
}
NS_END