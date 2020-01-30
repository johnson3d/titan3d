#include "CscShaderConductor.h"
//#include "../../3rd/ShaderConductor/Include/ShaderConductor/Native.h"

#pragma comment(lib, "ShaderConductor.lib")

#define  new VNEW

NS_BEGIN

CscShaderConductor* CscShaderConductor::GetInstance()
{
	static CscShaderConductor obj;
	return &obj;
}

//std::string 

//std::string DefaultLoadCallback(const std::string& includeName)
//{
//	const char* shaderRoot = "E:/Engine/Content/Shaders/";
//	std::string full = includeName;
//	for (size_t i = 0; i < full.size(); i++)
//	{
//		if (full[i] == '\\')
//			full[i] = '/';
//	}
//	auto pos = full.find_first_of(shaderRoot);
//	if (pos != std::string::npos)
//	{
//		full.substr(pos);
//	}
//}

typedef void (WINAPI *FOnShaderTranslated)(IShaderDesc* shaderDesc);

FOnShaderTranslated GOnShaderTranslated = nullptr;

bool CscShaderConductor::CompileHLSL(IShaderDesc* desc, std::string incRoot, std::string hlsl, LPCSTR entry, std::string sm, 
	const IShaderDefinitions* defines, CscShaderConductor::Includer* inc, bool hasGLSL, bool hasMetal)
{
	ViseFile io;
	if (io.Open(hlsl.c_str(), ViseFile::modeRead) == FALSE)
	{
		return false;
	}
	std::string code;
	{
		int len = (int)io.GetLength();
		char* buffer = new char[len +1];
		buffer[len] = '\0';
		io.Read(buffer, len);
		code = buffer;
		delete[] buffer;
	}
	io.Close();

	incRoot = hlsl;
	ShaderConductor::ShaderStage stage = ShaderConductor::ShaderStage::VertexShader;

	std::string essl_version = "310";
	auto pShaderModel = defines->FindDefine("ShaderModel");
	if (pShaderModel != nullptr)
	{
		if (pShaderModel->Definition == "3")
		{
			essl_version = "300";
		}
		else if (pShaderModel->Definition == "4")
		{
			essl_version = "310";
		}
		else if (pShaderModel->Definition == "5")
		{
			essl_version = "320";
		}
	}
	
	if (sm == "vs_5_0")
	{
		stage = ShaderConductor::ShaderStage::VertexShader;
	}
	else if (sm == "ps_5_0")
	{
		stage = ShaderConductor::ShaderStage::PixelShader;
	}
	else if (sm == "cs_5_0")
	{
		stage = ShaderConductor::ShaderStage::ComputeShader;
	}

	ShaderConductor::Compiler::SourceDesc src;
	src.source = code.c_str();
	src.entryPoint = entry;
	src.stage = stage;
	src.fileName = incRoot.c_str();
	src.loadIncludeCallback = [=](const char* includeName)->ShaderConductor::Blob*
	{
		std::vector<char> ret;
		if (strstr(includeName, "cbPerInstance.var") != nullptr)
		{
			ret.resize(inc->cbPerInstance_var.size());
			if (inc->cbPerInstance_var.size() > 0)
			{
				memcpy(&ret[0], &inc->cbPerInstance_var[0], ret.size());
			}
		}
		else if (strstr(includeName, "dummy.gen") != nullptr)
		{
			ret.resize(inc->dummy_gen.size());
			if (inc->dummy_gen.size() > 0)
			{
				memcpy(&ret[0], &inc->dummy_gen[0], ret.size());
			}
		}
		else
		{
			std::ifstream includeFile(includeName, std::ios_base::in);
			if (includeFile)
			{
				includeFile.seekg(0, std::ios::end);
				ret.resize(includeFile.tellg());
				includeFile.seekg(0, std::ios::beg);
				includeFile.read(ret.data(), ret.size());
				ret.resize(includeFile.gcount());
			}
			else
			{
				throw std::runtime_error(std::string("COULDN'T load included file ") + includeName + ".");
			}
		}
		return ShaderConductor::CreateBlob(ret.data(), static_cast<uint32_t>(ret.size()));
	};

	ShaderConductor::MacroDefine defs[32];
	src.numDefines = (int)defines->Definitions.size();
	src.defines = defs;
	for (size_t i=0; i<defines->Definitions.size(); i++)
	{
		defs[i].name = defines->Definitions[i].Name.c_str();
		defs[i].value = defines->Definitions[i].Definition.c_str();
	}

	bool DxCompileBugFixed = true;
	if(DxCompileBugFixed == false)
	{
		src.numDefines += 6;
		int HalfStartIndex = (int)defines->Definitions.size();
		defs[HalfStartIndex + 0].name = "half";
		defs[HalfStartIndex + 0].value = "float";
		defs[HalfStartIndex + 1].name = "half2";
		defs[HalfStartIndex + 1].value = "float2";
		defs[HalfStartIndex + 2].name = "half3";
		defs[HalfStartIndex + 2].value = "float3";
		defs[HalfStartIndex + 3].name = "half4";
		defs[HalfStartIndex + 3].value = "float4";
		defs[HalfStartIndex + 4].name = "half4x4";
		defs[HalfStartIndex + 4].value = "float4x4"; 
		defs[HalfStartIndex + 5].name = "half3x3";
		defs[HalfStartIndex + 5].value = "float3x3";
	}

	ShaderConductor::Compiler::Options opt;
	opt.packMatricesInRowMajor = false;
	opt.enable16bitTypes = true;
	opt.shaderModel = { 6,2 };
	opt.shiftAllTexturesBindings = 0;
	opt.shiftAllSamplersBindings = 0;
	opt.shiftAllCBuffersBindings = 0;
	opt.shiftAllUABuffersBindings = 0;

	std::vector<ShaderConductor::Compiler::TargetDesc> dest;
	std::vector < ShaderConductor::Compiler::ResultDesc> result;
	if (hasGLSL)
	{
		ShaderConductor::Compiler::TargetDesc tmp;
		tmp.language = ShaderConductor::ShadingLanguage::Essl;
		tmp.version = essl_version.c_str();
		tmp.asModule = false;
		dest.push_back(tmp);

		ShaderConductor::Compiler::ResultDesc tmp2;
		result.push_back(tmp2);
	}
	if (hasMetal)
	{
		ShaderConductor::Compiler::TargetDesc tmp;
		tmp.language = ShaderConductor::ShadingLanguage::Msl_iOS;
		tmp.version = nullptr;
		tmp.asModule = false;
		dest.push_back(tmp);

		ShaderConductor::Compiler::ResultDesc tmp2;
		result.push_back(tmp2);
	}

	ShaderConductor::Compiler::Compile(src, opt, &dest[0], (uint32_t)dest.size(), &result[0]);

	for (size_t i = 0; i < result.size(); i++)
	{
		auto& tmp = result[i];
		if (tmp.hasError == false && tmp.errorWarningMsg != nullptr)
		{
			auto s0 = std::string((char*)tmp.errorWarningMsg->Data(), tmp.errorWarningMsg->Size());
			if (s0 !=
				"warning: macro 'MDFQUEUE_FUNCTION' contains embedded newline; text after the newline is ignored\n")
			{
				VFX_LTRACE(ELTT_Graphics, "CrossPlatform Shader Warning:%s\r\n", s0.c_str());
			}
		}
		if (tmp.hasError)
		{
			if (tmp.errorWarningMsg)
			{
				auto s0 = std::string((char*)tmp.errorWarningMsg->Data(), tmp.errorWarningMsg->Size());
				VFX_LTRACE(ELTT_Graphics, "CrossPlatform Shader Error:%s\r\n", s0.c_str());
			}
			ShaderConductor::DestroyBlob(tmp.target);
			return false;
		}
		else
		{
			if(dest[i].language == ShaderConductor::ShadingLanguage::Essl)
				desc->Es300Code = std::string((char*)result[0].target->Data(), result[0].target->Size());
			else if (dest[i].language == ShaderConductor::ShadingLanguage::Msl_iOS)
				desc->MetalCode = std::string((char*)result[1].target->Data(), result[1].target->Size());
		}
		ShaderConductor::DestroyBlob(tmp.target);
	}

	if (GOnShaderTranslated != nullptr)
	{
		GOnShaderTranslated(desc);
	}

	return true;
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API void SDK_CscShaderConductor_SetTranslateCB(FOnShaderTranslated func)
	{
		GOnShaderTranslated = func;
	}
}
