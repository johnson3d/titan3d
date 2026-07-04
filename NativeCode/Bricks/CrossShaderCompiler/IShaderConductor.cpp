#include "IShaderConductor.h"
#include "../../NextRHI/NxRHI.h"
#include "../../Base/io/vfxfile.h"
#include "../../Base/io/MemStream.h"
#include "../../Base/CoreSDK.h"
//#include "../../../3rd/native/ShaderConductor/Include/ShaderConductor/SpirvPostProcess.hpp"

#if defined(PLATFORM_WIN)
#include "../../NextRHI/Dx11/DX11PreHead.h"
#include "../../NextRHI/Dx12/DX12Shader.h"
#include "../../NextRHI/Vulkan/VKShader.h"
#pragma comment(lib, "ShaderConductor.lib")
#endif

#define  new VNEW

using namespace EngineNS;
using namespace EngineNS::NxRHI;

#if defined(PLATFORM_WIN)

struct UEngineInclude : public ID3DInclude
{
	NxRHI::FShaderCompiler* ShaderConductor;
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
		auto ar = ShaderConductor->GetShaderCodeStream(root.c_str(), pFileName);
		if (ar == nullptr)
		{
			VFX_LTRACE(ELTT_Graphics, "Include %s failed", root.c_str());
			*ppData = nullptr;
			*pBytes = 0;
			return S_OK;
		}

		*ppData = ar->GetSourceCode();
		*pBytes = ar->GetSize();

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

bool IShaderConductor::CompileShader(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* shader, const char* entry, NxRHI::EShaderType type, const char* sm,
			const NxRHI::IShaderDefinitions* defines, bool bDebugShader, NxRHI::EShaderLanguage sl, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule)
{
	if (sl == NxRHI::EShaderLanguage::SL_DXBC)
	{
#if defined(PLATFORM_WIN)
		auto ar = compiler->GetShaderCodeStream(shader, shader);
		if (ar == nullptr)
			return false;

		std::vector<D3D_SHADER_MACRO> d11macros;
		if (defines != nullptr)
		{
			D3D_SHADER_MACRO tmp;
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
		case EShaderType::SDT_VertexShader:
			shaderTarget = "vs_";
			break;
		case EShaderType::SDT_PixelShader:
			shaderTarget = "ps_";
			break;
		case EShaderType::SDT_ComputeShader:
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
		engineInc->ShaderConductor = compiler;
		engineInc->Data2FileMap.insert(std::make_pair(nullptr, UEngineInclude::GetPath(shader)));
		auto hr = D3DCompile(ar->GetSourceCode(), ar->GetSize(), shader, pMacros, engineInc, entry, shaderTarget.c_str(), dwShaderFlags, 0, &pBlob, &pError);
		if (pError != NULL)
		{
			std::string pErrorStr = (char*)pError->GetBufferPointer();
			output->PushData(pErrorStr.c_str(), (UINT)pErrorStr.length());
			//VFX_LTRACE(ELTT_Graphics, (char*)pError->GetBufferPointer());
			pError->Release();
		}
		if (FAILED(hr))
		{
			ASSERT(false);
			return false;
		}

		desc->SetRhiData((BYTE*)pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());

#endif
	}

	if (sl == NxRHI::EShaderLanguage::SL_DXIL ||
		sl == NxRHI::EShaderLanguage::SL_SPIRV ||
		sl == NxRHI::EShaderLanguage::SL_GLSL ||
		sl == NxRHI::EShaderLanguage::SL_METAL)
	{
		auto ret = CompileHLSL(compiler, desc, shader, entry, type, sm, defines, sl, bDebugShader, extHlslVersion, dxcArgs, output, asModule);
		if (sl == NxRHI::EShaderLanguage::SL_DXIL)
			return ret;
		//Spirv is not ready for all shaders
		//ASSERT(ret);
		//return ret;
		return ret;
	}

	return true;
}

bool IShaderConductor::CompileHLSL(NxRHI::FShaderCompiler* compiler, NxRHI::FShaderDesc* desc, const char* hlsl, const char* entry, NxRHI::EShaderType type, std::string sm,
	const NxRHI::IShaderDefinitions* defines, NxRHI::EShaderLanguage sl, bool debugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule)
{
#if defined(PLATFORM_WIN)
	auto ar = compiler->GetShaderCodeStream(hlsl, hlsl);
	std::string codeText((const char*)ar->GetSourceCode(), (size_t)ar->GetSize());
	
	const int spirvShiftStride = 4 * typeMaxBinding;
	int spirvShiftBase = 0;
	ShaderConductor::ShaderStage stage = ShaderConductor::ShaderStage::VertexShader;
	switch (type)
	{
	case NxRHI::EShaderType::SDT_VertexShader:
		stage = ShaderConductor::ShaderStage::VertexShader;
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_PixelShader:
		stage = ShaderConductor::ShaderStage::PixelShader;
		spirvShiftBase = 1 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_ComputeShader:
		stage = ShaderConductor::ShaderStage::ComputeShader;
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_AmplificationShader:
		stage = ShaderConductor::ShaderStage::AmplificationShader;
		spirvShiftBase = 2 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_MeshShader:
		stage = ShaderConductor::ShaderStage::MeshShader;
		spirvShiftBase = 0 * spirvShiftStride;
		break;
	case NxRHI::EShaderType::SDT_RayTracing:
		spirvShiftBase = 0 * spirvShiftStride;
		//stage = ShaderConductor::ShaderStage:;
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

	auto add_DxcArgs = [](std::string& OutArgs, const char* arg)->void
	{
		if (OutArgs.length() == 0)
		{
			OutArgs += arg;
		}
		else
		{
			OutArgs += ' ';
			OutArgs += arg;
		}
	};

	std::string finalDxcArgs;
	if (dxcArgs != nullptr)
	{
		finalDxcArgs = dxcArgs;
		/*if (finalDxcArgs[finalDxcArgs.size() - 1] != ' ')
		{
			finalDxcArgs += ' ';
		}*/
	}

	ShaderConductor::Compiler::SourceDesc src{};
	src.source = codeText.c_str();
	src.entryPoint = entry;
	src.hlslExtVersion = extHlslVersion;
	//src.dxcArgString = dxcArgs;
	src.stage = stage;
	src.fileName = hlsl;
	src.loadIncludeCallback = [=](const char* includeName)->ShaderConductor::Blob
	{
		auto inc = std::string(includeName);
		auto pos = inc.find_first_of('@');
		if (pos != std::string::npos)
		{
			inc = inc.substr(pos);
		}
		auto ar_inc = compiler->GetShaderCodeStream(inc.c_str(), inc.c_str());
		
		if (ar_inc != nullptr)
			return ShaderConductor::Blob(ar_inc->GetSourceCode(), static_cast<uint32_t>(ar_inc->GetSize()));
		else
			return ShaderConductor::Blob(nullptr, static_cast<uint32_t>(0));
	};

	std::vector<ShaderConductor::MacroDefine> defs;
	if (defines != nullptr)
	{
		defs.resize(defines->Definitions.size());
		for (size_t i = 0; i < defines->Definitions.size(); i++)
		{
			defs[i].name = defines->Definitions[i].Name.c_str();
			defs[i].value = defines->Definitions[i].Definition.c_str();
		}
		src.numDefines = (int)defs.size();
		src.defines = &defs[0];
	}
	else
	{
		src.numDefines = 0;
		src.defines = nullptr;
	}

	ShaderConductor::Compiler::Options opt;
	opt.enableDebugInfo = debugShader;
	opt.disableOptimizations = debugShader;
	opt.packMatricesInRowMajor = false;
	opt.enable16bitTypes = true;
	auto _pos = sm.find_first_of('_');
	auto f1 = sm.substr(0, _pos);
	auto f2 = sm.substr(_pos + 1);
	opt.shaderModel.major_ver = atoi(f1.c_str());
	opt.shaderModel.minor_ver = atoi(f2.c_str());
	opt.shiftAllCBuffersBindings = spirvShiftBase + typeMaxBinding * 0;
	//opt.shiftAllCBuffersSet = 1;
	opt.shiftAllTexturesBindings = spirvShiftBase + typeMaxBinding * 1;
	//opt.shiftAllTexturesSet = 1;
	opt.shiftAllSamplersBindings = spirvShiftBase + typeMaxBinding * 2;
	//opt.shiftAllSamplersSet = 1;
	opt.shiftAllUABuffersBindings = spirvShiftBase + typeMaxBinding * 3;
	//opt.shiftAllUABuffersSet = 1;
	opt.needReflection = true;

	std::vector<ShaderConductor::Compiler::TargetDesc> dest;
	std::vector<ShaderConductor::Compiler::ResultDesc> result;

	ShaderConductor::Compiler::TargetDesc tmp{};
	ShaderConductor::Compiler::ResultDesc tmp2{};

	tmp.asModule = asModule;
	
	if (debugShader)
	{
		add_DxcArgs(finalDxcArgs, "-Qembed_debug");
	}

	switch (sl)
	{
		case NxRHI::EShaderLanguage::SL_DXBC:
			break;
		case NxRHI::EShaderLanguage::SL_DXIL:
			tmp.version = sm.c_str();// "6_2";
			tmp.language = ShaderConductor::ShadingLanguage::Dxil;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case NxRHI::EShaderLanguage::SL_GLSL:
			if (opt.shaderModel.major_ver >= 6)
			{
				add_DxcArgs(finalDxcArgs, "-fspv-target-env=vulkan1.2");
			}
			tmp.version = essl_version.c_str();
			tmp.language = ShaderConductor::ShadingLanguage::Essl;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case NxRHI::EShaderLanguage::SL_SPIRV:
			if (opt.shaderModel.major_ver >= 6)
			{
				add_DxcArgs(finalDxcArgs, "-fspv-target-env=vulkan1.2");
			}
			tmp.language = ShaderConductor::ShadingLanguage::SpirV;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		case NxRHI::EShaderLanguage::SL_METAL:
			if (opt.shaderModel.major_ver >= 6)
			{
				add_DxcArgs(finalDxcArgs, "-fspv-target-env=vulkan1.2");
			}
			tmp.language = ShaderConductor::ShadingLanguage::Msl_iOS;
			dest.push_back(tmp);
			result.push_back(tmp2);
			break;
		default:
			break;
	}
	
	src.dxcArgString = finalDxcArgs.c_str();
	ShaderConductor::Compiler::Compile(src, opt, &dest[0], (uint32_t)dest.size(), &result[0]);

	for (size_t i = 0; i < result.size(); i++)
	{
		auto& tmp = result[i];
		if (tmp.errorWarningMsg.Size() != 0)
		{
			output->PushData((char*)tmp.errorWarningMsg.Data(), tmp.errorWarningMsg.Size());
		}
		if (tmp.hasError == false)
		{
			if (dest[i].language == ShaderConductor::ShadingLanguage::Dxil)
			{
				auto& finalResult = result[i];
				size_t sz = finalResult.target.Size();
				desc->RhiData.resize(sz);
				memcpy(&desc->RhiData[0], (char*)finalResult.target.Data(), finalResult.target.Size());
				desc->Language = EShaderLanguage::SL_DXIL;

#if defined(HasModule_Dx12)
				if (asModule)
				{
					auto pDx12Reflection = (ID3D12LibraryReflection*)finalResult.reflection.GetD3D12LibraryReflection();
					if (pDx12Reflection != nullptr)
					{
						DX12Shader::Reflect(desc, pDx12Reflection);
					}
				}
				else 
				{
					auto pDx12Reflection = (ID3D12ShaderReflection*)finalResult.reflection.GetD3D12ShaderReflection();
					if (pDx12Reflection != nullptr)
					{
						DX12Shader::Reflect(desc, pDx12Reflection);
					}
				}
				
#endif
			}
			else if(dest[i].language == ShaderConductor::ShadingLanguage::Essl)
			{
				auto& data = result[i].target;
				desc->RhiData.resize(data.Size());
				memcpy(&desc->RhiData[0], (char*)data.Data(), data.Size());
				desc->Language = EShaderLanguage::SL_GLSL;
			}
			else if (dest[i].language == ShaderConductor::ShadingLanguage::Msl_iOS)
			{
				auto& data = result[i].target;
				desc->RhiData.resize(data.Size());
				memcpy(&desc->RhiData[0], (char*)data.Data(), data.Size());
				desc->Language = EShaderLanguage::SL_METAL;
			}
			else if (dest[i].language == ShaderConductor::ShadingLanguage::SpirV)
			{
				auto& finalResult = result[i];
				size_t sz = finalResult.target.Size();
				desc->RhiData.resize(sz);
				memcpy(&desc->RhiData[0], (char*)finalResult.target.Data(), finalResult.target.Size());
				desc->Language = EShaderLanguage::SL_SPIRV;
#if defined(HasModule_Vulkan)
				VKShader::Reflect(desc);
#endif
			}
		}
		else
		{
			return false;
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

//bool CompileRayTracingLibrary(IDxcBlob** ppDXILBlob) 
//{
//	try {
//		// 初始化DXC
//		CComPtr<IDxcUtils> pUtils;
//		CComPtr<IDxcCompiler3> pCompiler;
//		DxcCreateInstance(CLSID_DxcUtils, IID_PPV_ARGS(&pUtils));
//		DxcCreateInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&pCompiler));
//
//		// 加载HLSL源码
//		CComPtr<IDxcBlobEncoding> pSource;
//		pUtils->LoadFile(L"RayTracing.hlsl", nullptr, &pSource);
//
//		// 配置编译参数
//		std::vector<LPCWSTR> args = {
//			L"-T", L"lib_6_6",
//			L"-E", L"RayGen",
//			L"-Zi", L"-Qembed_debug",
//			DXC_ARG_WARNINGS_ARE_ERRORS
//		};
//
//		// 执行编译
//		DxcBuffer sourceBuffer = {
//			pSource->GetBufferPointer(),
//			pSource->GetBufferSize(),
//			DXC_CP_UTF8
//		};
//
//		CComPtr<IDxcResult> pResult;
//		pCompiler->Compile(&sourceBuffer, args.data(), (UINT)args.size(),
//			nullptr, IID_PPV_ARGS(&pResult));
//
//		// 错误处理
//		CComPtr<IDxcBlobUtf8> pErrors;
//		pResult->GetOutput(DXC_OUT_ERRORS, IID_PPV_ARGS(&pErrors), nullptr);
//		if (pErrors && pErrors->GetStringLength() > 0) {
//			OutputDebugStringA(pErrors->GetStringPointer());
//		}
//
//		HRESULT status;
//		pResult->GetStatus(&status);
//		if (FAILED(status)) return false;
//
//		// 提取DXIL
//		return SUCCEEDED(pResult->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(ppDXILBlob), nullptr));
//	}
//	catch (...) {
//		return false;
//	}
//}

bool IShaderConductor::CompileDXR(NxRHI::FShaderCompiler* compiler, IBlobObject* lib, const char* sm, const NxRHI::IShaderDefinitions* defines, bool bDebugShader, NxRHI::EShaderLanguage sl)
{
	return false;
}

NS_END
