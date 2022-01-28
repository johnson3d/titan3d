#include "IShaderConductor.h"
#include "../../RHI/IShader.h"
#include "../../RHI/ShaderReflector.h"
#include "../../Base/io/vfxfile.h"
#include "../../Base/io/MemStream.h"
#include "../../Base/CoreSDK.h"
#include "../../../3rd/native/ShaderConductor/Include/ShaderConductor/SpirvPostProcess.hpp"

#if defined(PLATFORM_WIN)
#include "D3D11/D11PreHead.h"
#pragma comment(lib, "ShaderConductor.lib")
#endif

#define  new VNEW

#if defined(PLATFORM_WIN)
using namespace EngineNS;


struct UEngineInclude : public ID3DInclude
{
	IShaderConductor* ShaderConductor;
	std::map<LPCVOID, std::string> Data2FileMap;
	virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Open(D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID *ppData, UINT *pBytes)
	{
		std::string parent;
		auto iter = Data2FileMap.find(pParentData);
		if (iter != Data2FileMap.end())
		{
			parent = iter->second;
		}

		std::string root = parent + pFileName;
		if (strlen(pFileName) > 2 && pFileName[1] == ':')
		{
			//todo: not windows platform
			root = pFileName;
		}
		auto ar = ShaderConductor->GetShaderCodeStream((void*)root.c_str());
		if (ar == nullptr)
		{
			VFX_LTRACE(ELTT_Graphics, "Include %s failed", root.c_str());
			*ppData = nullptr;
			*pBytes = 0;
			return S_OK;
		}

		*ppData = ar->GetPointer();
		*pBytes = (UINT)ar->Tell();

		auto dir = GetPath(root);
		Data2FileMap.insert(std::make_pair(*ppData, dir));

		return S_OK;
	}
	virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Close(LPCVOID pData)
	{
		return S_OK;
	}
	static std::string GetPath(std::string file)
	{
		for (size_t i = 0; i < file.size(); i++)
		{
			if (file[i] == '\\')
				file[i] = '/';
		}
		size_t pos = std::string::npos;
		for (int i = (int)file.size() - 1; i >= 0; i--)
		{
			if (file[i] == '/')
			{
				pos = i;
				break;
			}
		}
		std::string result = file.substr(0, pos + 1);
		return result;
	}
};
#endif

#if defined(USE_VK)

#endif

NS_BEGIN

IShaderConductor* IShaderConductor::GetInstance()
{
	static IShaderConductor obj;
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

bool IShaderConductor::CompileShader(IShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm,
			const IShaderDefinitions* defines, bool bDebugShader, EShaderLanguage sl)
{
	if (sl == EShaderLanguage::SL_DXBC)
	{
#if defined(PLATFORM_WIN)
		MemStreamWriter* ar = GetShaderCodeStream((void*)shader);
		if (ar == nullptr)
			return false;

		std::vector<D3D10_SHADER_MACRO> d11macros;
		if (defines != nullptr)
		{
			D3D10_SHADER_MACRO tmp;
			for (size_t i = 0; i < defines->Definitions.size(); i++)
			{	
				tmp.Name = defines->Definitions[i].Name.c_str();
				tmp.Definition = defines->Definitions[i].Definition.c_str();
				d11macros.push_back(tmp);
			}
			tmp.Name = nullptr;
			tmp.Definition = nullptr;
			d11macros.push_back(tmp);
		}

		D3D10_SHADER_MACRO* pMacros = nullptr;
		if (d11macros.size() > 0)
			pMacros = &d11macros[0];

		std::string shaderTarget;
		switch (type)
		{
		case EngineNS::EST_UnknownShader:
			break;
		case EngineNS::EST_VertexShader:
			shaderTarget = "vs_";
			break;
		case EngineNS::EST_PixelShader:
			shaderTarget = "ps_";
			break;
		case EngineNS::EST_ComputeShader:
			shaderTarget = "cs_";
			break;
		default:
			break;
		}
		shaderTarget += sm;

		DWORD dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;

		if (bDebugShader)
		{
			dwShaderFlags |= D3DCOMPILE_DEBUG;
		}
		ID3DBlob* pBlob;
		ID3DBlob* pError;
		AutoPtr<UEngineInclude> engineInc(new UEngineInclude());
		engineInc->ShaderConductor = this;
		engineInc->Data2FileMap.insert(std::make_pair(nullptr, UEngineInclude::GetPath(shader)));
		auto hr = D3DCompile(ar->GetPointer(), ar->Tell(), shader, pMacros, engineInc, entry, shaderTarget.c_str(), dwShaderFlags, 0, &pBlob, &pError);
		if (pError != NULL)
		{
			VFX_LTRACE(ELTT_Graphics, (char*)pError->GetBufferPointer());
			pError->Release();
		}
		if (FAILED(hr))
		{
			ASSERT(false);
			return false;
		}

		desc->SetDXBC((BYTE*)pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());

#endif
	}

	if (sl == EShaderLanguage::SL_DXIL || 
		sl == EShaderLanguage::SL_SPIRV || 
		sl == EShaderLanguage::SL_GLSL ||
		sl == EShaderLanguage::SL_METAL)
	{
		CompileHLSL(desc, shader, entry, type, sm, defines, sl);
	}

	if (sl == EShaderLanguage::SL_SPIRV)
	{
		
	}
	return true;
}

struct MySpirvCallback : ShaderConductor::SpirvCallback
{
	EShaderType ShaderStage;
	virtual bool Remapping(const char* name, ShaderConductor::SpirvResourceType type, uint32_t set, uint32_t binding, 
					uint32_t& outSet, uint32_t& outBinding) override 
	{
		outSet = set;
		outBinding = binding;
		UINT baseBinding = 0;
		switch (ShaderStage)
		{
			case EngineNS::EST_UnknownShader:
				break;
			case EngineNS::EST_VertexShader:
				baseBinding = 0;
				break;
			case EngineNS::EST_PixelShader:
				baseBinding = 50;
				break;
			case EngineNS::EST_ComputeShader:
				break;
			default:
				break;
		}
		
		switch (type)
		{
			case ShaderConductor::SpirvResourceType::StageInput:
				baseBinding = 0;
				break;
			case ShaderConductor::SpirvResourceType::StageOutput:
				baseBinding = 0;
				break;
			case ShaderConductor::SpirvResourceType::UniformBuffer:
				break;
			case ShaderConductor::SpirvResourceType::SampledImage:
				break;
			case ShaderConductor::SpirvResourceType::SeparateImage:
				break;
			case ShaderConductor::SpirvResourceType::SeparateSampler:
				break;
			case ShaderConductor::SpirvResourceType::StorageBuffer:
				break;
			case ShaderConductor::SpirvResourceType::StorageImage:
				break;
			case ShaderConductor::SpirvResourceType::AtomicCounter:
				break;
			case ShaderConductor::SpirvResourceType::AccelerationStructures:
				break;
			default:
				break;
		}

		outSet = set;
		outBinding = baseBinding + binding;
		return true;
	}
};

bool IShaderConductor::CompileHLSL(IShaderDesc* desc, const char* hlsl, const char* entry, EShaderType type, std::string sm,
	const IShaderDefinitions* defines, EShaderLanguage sl)
{
#if defined(PLATFORM_WIN)
	MemStreamWriter* ar = GetShaderCodeStream((void*)hlsl);
	std::string codeText((const char*)ar->GetPointer(), (size_t)ar->Tell());
	
	ShaderConductor::ShaderStage stage = ShaderConductor::ShaderStage::VertexShader;
	switch (type)
	{
	case EngineNS::EST_UnknownShader:
		break;
	case EngineNS::EST_VertexShader:
		stage = ShaderConductor::ShaderStage::VertexShader;
		break;
	case EngineNS::EST_PixelShader:
		stage = ShaderConductor::ShaderStage::PixelShader;
		break;
	case EngineNS::EST_ComputeShader:
		stage = ShaderConductor::ShaderStage::ComputeShader;
		break;
	default:
		break;
	}

	std::string essl_version = "310";
	if (defines != nullptr)
	{
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
	}

	ShaderConductor::Compiler::SourceDesc src;
	src.source = codeText.c_str();
	src.entryPoint = entry;
	src.stage = stage;
	src.fileName = hlsl;
	src.loadIncludeCallback = [=](const char* includeName)->ShaderConductor::Blob
	{
		MemStreamWriter* ar_inc = GetShaderCodeStream((void*)includeName);
		
		if (ar_inc != nullptr)
			return ShaderConductor::Blob(ar_inc->GetPointer(), static_cast<uint32_t>(ar_inc->Tell()));
		else
			return ShaderConductor::Blob(nullptr, static_cast<uint32_t>(0));
	};

	ShaderConductor::MacroDefine defs[32];
	if (defines != nullptr)
	{
		src.numDefines = (int)defines->Definitions.size();
		src.defines = defs;
		for (size_t i = 0; i < defines->Definitions.size(); i++)
		{
			defs[i].name = defines->Definitions[i].Name.c_str();
			defs[i].value = defines->Definitions[i].Definition.c_str();
		}
	}
	else
	{
		src.numDefines = 0;
		src.defines = defs;
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

	ShaderConductor::Compiler::TargetDesc tmp;
	ShaderConductor::Compiler::ResultDesc tmp2;

	tmp.version = essl_version.c_str();
	tmp.asModule = false;
	
	switch (sl)
	{
		case EngineNS::SL_DXBC:
			break;
		case EngineNS::SL_DXIL:
			break;
		case EngineNS::SL_GLSL:
			tmp.language = ShaderConductor::ShadingLanguage::Essl;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case EngineNS::SL_SPIRV:
			tmp.language = ShaderConductor::ShadingLanguage::SpirV;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case EngineNS::SL_METAL:
			tmp.language = ShaderConductor::ShadingLanguage::Msl_iOS;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		default:
			break;
	}

	ShaderConductor::Compiler::Compile(src, opt, &dest[0], (uint32_t)dest.size(), &result[0]);

	for (size_t i = 0; i < result.size(); i++)
	{
		auto& tmp = result[i];
		if (tmp.hasError == false && tmp.errorWarningMsg.Size() != 0)
		{
			auto s0 = std::string((char*)tmp.errorWarningMsg.Data(), tmp.errorWarningMsg.Size());
			if (s0.find("Inc/FXAAMobile.cginc:") == std::string::npos)
			{
				VFX_LTRACE(ELTT_Graphics, "CrossPlatform Shader Warning:%s\r\n", s0.c_str());
			}
			/*if (s0 !=
				"warning: macro 'MDFQUEUE_FUNCTION' contains embedded newline; text after the newline is ignored\n")
			{
				
			}*/
		}
		if (tmp.hasError)
		{
			if (tmp.errorWarningMsg.Size() != 0)
			{
				auto s0 = std::string((char*)tmp.errorWarningMsg.Data(), tmp.errorWarningMsg.Size());
				VFX_LTRACE(ELTT_Graphics, "CrossPlatform Shader Error:%s\r\n", s0.c_str());
			}
			return false;
		}
		else
		{
			if(dest[i].language == ShaderConductor::ShadingLanguage::Essl)
			{
				desc->Es300Code = std::string((char*)result[i].target.Data(), result[i].target.Size());
			}
			else if (dest[i].language == ShaderConductor::ShadingLanguage::Msl_iOS)
			{
				desc->MetalCode = std::string((char*)result[i].target.Data(), result[i].target.Size());
			}
			else if (dest[i].language == ShaderConductor::ShadingLanguage::SpirV)
			{
				MySpirvCallback cb;
				cb.ShaderStage = type;
				//auto finalResult = ProcessSpirv(result[i], &cb);
				auto& finalResult = result[i];
				size_t sz = finalResult.target.Size() / sizeof(UINT);
				desc->SpirV.resize(sz);
				memcpy(&desc->SpirV[0], (char*)finalResult.target.Data(), finalResult.target.Size());
			}
		}
	}

	if (CoreSDK::OnShaderTranslated != nullptr)
	{
		CoreSDK::OnShaderTranslated(desc);
	}

	return true;
#else
	return false;
#endif
}

NS_END
