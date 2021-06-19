#include "IShaderConductor.h"
#include "../../RHI/IShader.h"
#include "../../RHI/ShaderReflector.h"
#include "../../Base/io/vfxfile.h"
#include "../../Base/io/MemStream.h"

#if defined(PLATFORM_WIN)
#include "D3D11/D11PreHead.h"
#pragma comment(lib, "ShaderConductor.lib")
#endif

#define  new VNEW

#if defined(PLATFORM_WIN)
using namespace EngineNS;
inline EShaderVarType D11ShaderVarType2DX(D3D_SHADER_VARIABLE_TYPE type, UINT col, UINT row)
{
	switch (type)
	{
	case D3D_SVT_VOID:
	{
		return SVT_Struct;
	}
	case D3D_SVT_FLOAT:
	{
		if (row == 1)
		{
			switch (col)
			{
			case 1:
				return SVT_Float1;
			case 2:
				return SVT_Float2;
			case 3:
				return SVT_Float3;
			case 4:
				return SVT_Float4;
			}
		}
		else if (row == 3 && col == 3)
		{
			return SVT_Matrix3x3;
		}
		else if (row == 4 && col == 4)
		{
			return SVT_Matrix4x4;
		}
		break;
	}
	case D3D_SVT_INT:
	{
		switch (col)
		{
		case 1:
			return SVT_Int1;
		case 2:
			return SVT_Int2;
		case 3:
			return SVT_Int3;
		case 4:
			return SVT_Int4;
		}
		break;
	}
	default:
		break;
	}
	return SVT_Unknown;
}
extern "C" bool ReflectShader(IShaderDesc* desc)
{
	if (desc->Reflector == nullptr)
	{
		desc->Reflector = new ShaderReflector();
	}
	else
	{
		desc->Reflector->mCBDescArray.clear();
		desc->Reflector->mTexBindInfoArray.clear();
		desc->Reflector->mSamplerBindInfoArray.clear();
	}
	auto Reflector = desc->Reflector;
	ID3D11ShaderReflection* pReflection;
	auto hr = D3DReflect(&desc->GetCodes()[0], desc->GetCodes().size(), IID_ID3D11ShaderReflection, (void**)&pReflection);
	if (FAILED(hr))
	{
		return false;
	}
	D3D11_SHADER_DESC shaderDesc;
	pReflection->GetDesc(&shaderDesc);

	for (UINT i = 0; i < shaderDesc.InputParameters; i++)
	{
		D3D11_SIGNATURE_PARAMETER_DESC desc;
		pReflection->GetInputParameterDesc(i, &desc);
		pReflection->GetInputParameterDesc(i, &desc);
	}

#define DescSetBindPoint(type, dest, src) do{\
		switch (type) \
		{\
				case EST_VertexShader:\
					dest.VSBindPoint = src.BindPoint;\
					break;\
				case EST_PixelShader:\
					dest.PSBindPoint = src.BindPoint;\
					break;\
				case EST_ComputeShader:\
					dest.CSBindPoint = src.BindPoint;\
					break;\
		}\
		dest.BindCount = src.BindCount;\
	}\
	while(0);

	for (UINT i = 0; i < shaderDesc.BoundResources; i++)
	{
		D3D11_SHADER_INPUT_BIND_DESC csibDesc;
		pReflection->GetResourceBindingDesc(i, &csibDesc);
		ID3D11ShaderReflectionVariable* pVari = pReflection->GetVariableByName(csibDesc.Name);
		D3D11_SHADER_VARIABLE_DESC varDesc;
		auto hr = pVari->GetDesc(&varDesc);
		if (SUCCEEDED(hr))
		{
			if ((varDesc.uFlags & D3D10_SVF_USED) == 0)
			{
				continue;
			}
		}
		switch (csibDesc.Type)
		{
		case D3D_SIT_CBUFFER:
		case D3D_SIT_UAV_RWTYPED:
		case D3D_SIT_UAV_RWSTRUCTURED:
		case D3D_SIT_BYTEADDRESS:
		case D3D_SIT_UAV_RWBYTEADDRESS:
		case D3D_SIT_UAV_APPEND_STRUCTURED:
		case D3D_SIT_UAV_CONSUME_STRUCTURED:
		case D3D_SIT_UAV_RWSTRUCTURED_WITH_COUNTER:
		{
			ID3D11ShaderReflectionConstantBuffer* pCBuffer = pReflection->GetConstantBufferByName(csibDesc.Name);
			D3D11_SHADER_BUFFER_DESC Desc;
			auto hr = pCBuffer->GetDesc(&Desc);

			IConstantBufferDesc tcbDesc;
			tcbDesc.Type = (ECBufferRhiType)csibDesc.Type;
			tcbDesc.Name = csibDesc.Name;// Desc.Name;
			DescSetBindPoint(desc->ShaderType, tcbDesc, csibDesc);
			if (hr == S_OK)
			{
				tcbDesc.Size = Desc.Size;
				for (UINT j = 0; j < Desc.Variables; j++)
				{
					ID3D11ShaderReflectionVariable* pVari = pCBuffer->GetVariableByIndex(j);
					D3D11_SHADER_VARIABLE_DESC varDesc;
					pVari->GetDesc(&varDesc);

					D3D11_SHADER_TYPE_DESC stDesc;
					pVari->GetType()->GetDesc(&stDesc);

					ConstantVarDesc tcvDesc;
					tcvDesc.Name = varDesc.Name;
					tcvDesc.Type = D11ShaderVarType2DX(stDesc.Type, stDesc.Columns, stDesc.Rows);
					tcvDesc.Offset = varDesc.StartOffset;
					tcvDesc.Size = varDesc.Size;
					if (stDesc.Elements == 0)
						tcvDesc.Elements = 1;
					else
						tcvDesc.Elements = stDesc.Elements;

					tcbDesc.Vars.push_back(tcvDesc);
				}
			}
			else
			{
				tcbDesc.Size = 0;
			}

			Reflector->mCBDescArray.push_back(tcbDesc);
		}
		break;
		case D3D_SIT_TBUFFER:
		case D3D_SIT_STRUCTURED:
		{
			TSBindInfo tmp;
			tmp.Type = (ECBufferRhiType)csibDesc.Type;
			tmp.Name = csibDesc.Name;
			DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

			Reflector->mTexBindInfoArray.push_back(tmp);
		}
		break;
		case D3D_SIT_TEXTURE:
		{
			TSBindInfo tmp;
			tmp.Type = (ECBufferRhiType)csibDesc.Type;
			tmp.Name = csibDesc.Name;
			DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

			Reflector->mTexBindInfoArray.push_back(tmp);
		}
		break;
		case D3D_SIT_SAMPLER:
		{
			TSBindInfo tmp;
			tmp.Type = (ECBufferRhiType)csibDesc.Type;
			tmp.Name = csibDesc.Name;
			DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

			Reflector->mSamplerBindInfoArray.push_back(tmp);
		}
		break;
		default:
			break;
		}
	}
	return true;
}
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

		*ppData = ar->GetDataPointer();
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

NS_BEGIN

StructImpl(FGetShaderCodeStream)

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

typedef void (WINAPI *FOnShaderTranslated)(IShaderDesc* shaderDesc);

FOnShaderTranslated GOnShaderTranslated = nullptr;

bool IShaderConductor::CompileShader(IShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm,
			const IShaderDefinitions* defines, bool bDebugShader, bool bDxbc, bool bGlsl, bool bMetal)
{
	if (bDxbc)
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
		auto hr = D3DCompile(ar->GetDataPointer(), ar->Tell(), shader, pMacros, engineInc, entry, shaderTarget.c_str(), dwShaderFlags, 0, &pBlob, &pError);
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

		ReflectShader(desc);
#endif
	}

	if (bGlsl)
	{
		CompileHLSL(desc, shader, entry, type, sm, defines, bGlsl, bMetal);
	}

	if (desc->Reflector == nullptr)
	{
		desc->Reflector = new ShaderReflector();
	}
	return true;
}

bool IShaderConductor::CompileHLSL(IShaderDesc* desc, const char* hlsl, const char* entry, EShaderType type, std::string sm,
	const IShaderDefinitions* defines, bool hasGLSL, bool hasMetal)
{
#if defined(PLATFORM_WIN)
	MemStreamWriter* ar = GetShaderCodeStream((void*)hlsl);

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
	src.source = (const char*)ar->GetDataPointer();
	src.entryPoint = entry;
	src.stage = stage;
	src.fileName = hlsl;
	src.loadIncludeCallback = [=](const char* includeName)->ShaderConductor::Blob*
	{
		MemStreamWriter* ar_inc = GetShaderCodeStream((void*)includeName);
		
		if (ar_inc != nullptr)
			return ShaderConductor::CreateBlob(ar_inc->GetDataPointer(), static_cast<uint32_t>(ar_inc->Tell()));
		else
			return ShaderConductor::CreateBlob(nullptr, static_cast<uint32_t>(0));
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
#else
	return false;
#endif
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
