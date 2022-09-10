#include "DX12Shader.h"
#include "DX12GpuDevice.h"
#include "../NxRHIDefine.h"
#include "../../Bricks/CrossShaderCompiler/IShaderConductor.h"

#define new VNEW

NS_BEGIN
namespace NxRHI
{
	struct UEngineInclude : public ID3DInclude
	{
		virtual ~UEngineInclude()
		{

		}
		FShaderCompiler* Compiler;
		std::map<LPCVOID, std::string> Data2FileMap;
		virtual COM_DECLSPEC_NOTHROW HRESULT STDMETHODCALLTYPE Open(D3D_INCLUDE_TYPE IncludeType, LPCSTR pFileName, LPCVOID pParentData, LPCVOID* ppData, UINT* pBytes)
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
			auto ar = Compiler->GetShaderCodeStream(root.c_str());
			if (ar == nullptr)
			{
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

	bool DX12Shader_CompileShader2(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader)
	{
		AutoRef<FShaderCode> ar = compiler->GetShaderCodeStream(shader);
		if (ar == nullptr)
			return false;

		((IShaderDefinitions*)defines)->AddDefine("RHI_TYPE", "RHI_DX11");

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

		D3D_SHADER_MACRO* pMacros = nullptr;
		if (d11macros.size() > 0)
			pMacros = &d11macros[0];

		std::string shaderTarget;
		switch (type)
		{
		case SDT_VertexShader:
			shaderTarget = "vs_";
			break;
		case SDT_PixelShader:
			shaderTarget = "ps_";
			break;
		case SDT_ComputeShader:
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
		auto engineInc = AutoPtr(new UEngineInclude());
		engineInc->Compiler = compiler;
		engineInc->Data2FileMap.insert(std::make_pair(nullptr, UEngineInclude::GetPath(shader)));
		auto hr = D3DCompile(ar->GetSourceCode(), ar->GetSize(), shader, pMacros, engineInc, entry, shaderTarget.c_str(), dwShaderFlags, 0, &pBlob, &pError);
		if (pError != NULL)
		{
			std::string pErrorStr = (char*)pError->GetBufferPointer();

			auto pos = pErrorStr.find("error X");
			if (pos != std::string::npos)
			{
				VFX_LTRACE(ELTT_Graphics, pErrorStr.c_str());
			}
			pError->Release();
		}
		if (FAILED(hr))
		{
			ASSERT(false);
			return false;
		}
		desc->DxIL.resize((UINT)pBlob->GetBufferSize());
		memcpy(&desc->DxIL[0], pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());

		ID3D12ShaderReflection* pReflection;
		hr = D3DReflect(&desc->DxIL[0], desc->DxIL.size(), IID_PPV_ARGS(&pReflection));
		if (FAILED(hr))
		{
			return false;
		}
		if (pReflection != nullptr)
		{
			DX12Shader::Reflect(desc, pReflection);
		}
		return true;
	}

	bool DX12Shader::CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader)
	{
		desc->FunctionName = entry;
		//return DX12Shader_CompileShader2(compiler, desc, shader, entry, type, "5_0", defines, sl, bDebugShader);
		return IShaderConductor::GetInstance()->CompileShader(compiler, desc, shader, entry, type, sm, defines, bDebugShader, sl);
	}
	
	DX12Shader::DX12Shader()
	{
		
	}
	DX12Shader::~DX12Shader()
	{
		
	}
	bool DX12Shader::Init(DX12GpuDevice* device, FShaderDesc* desc)
	{
		Desc = desc;
		if (Desc->DxIL.size() == 0)
			return false;

		//Reflect(desc);
		Reflector = desc->DxILReflector;

		return true;
	}
	static EShaderVarType DxShaderVarType2VarType(D3D_SHADER_VARIABLE_TYPE type)
	{
		switch (type)
		{
			case D3D_SVT_INT:
			{
				return EShaderVarType::SVT_Int;
			}
			break;
			case D3D_SVT_FLOAT:
			{
				return EShaderVarType::SVT_Float;
			}
			break;
			case D3D_SVT_TEXTURE:
			{
				return EShaderVarType::SVT_Texture;
			}
			break;
			case D3D_SVT_SAMPLER:
			{
				return EShaderVarType::SVT_Sampler;
			}
			break;
			default:
				return EShaderVarType::SVT_Unknown;
		}
	}
	static EVertexStreamType GetStreamType(const D3D12_SIGNATURE_PARAMETER_DESC& desc)
	{
		auto name = std::string(desc.SemanticName);
		if ((name == "SV_POSITION" || name == "POSITION") && desc.SemanticIndex == 0)
		{
			return VST_Position;
		}
		else if (name == "NORMAL" && desc.SemanticIndex == 0)
		{
			return VST_Normal;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 0)
		{
			return VST_Tangent;
		}
		else if (name == "COLOR" && desc.SemanticIndex == 0)
		{
			return VST_Color;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 1)
		{
			return VST_UV;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 2)
		{
			return VST_LightMap;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 3)
		{
			return VST_SkinIndex;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 4)
		{
			return VST_SkinWeight;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 5)
		{
			return VST_TerrainIndex;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 6)
		{
			return VST_TerrainGradient;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 7)
		{
			return VST_InstPos;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 8)
		{
			return VST_InstQuat;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 9)
		{
			return VST_InstScale;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 10)
		{
			return VST_F4_1;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 11)
		{
			return VST_F4_2;
		}
		else if (name == "TEXCOORD" && desc.SemanticIndex == 12)
		{
			return VST_F4_3;
		}
		return VST_Number;
	}

	bool DX12Shader::Reflect(FShaderDesc* desc, ID3D12ShaderReflection* pReflection)
	{
		desc->DxILReflector = MakeWeakRef(new IShaderReflector());

		auto Reflector = desc->DxILReflector;

		D3D12_SHADER_DESC shaderDesc;
		pReflection->GetDesc(&shaderDesc);

		desc->InputStreams.clear();
		for (UINT i = 0; i < shaderDesc.InputParameters; i++)
		{
			D3D12_SIGNATURE_PARAMETER_DESC iptDesc;
			if (pReflection->GetInputParameterDesc(i, &iptDesc) == S_OK)
			{
				auto type = GetStreamType(iptDesc);
				if (type != VST_Number)
					desc->InputStreams.push_back(type);
			}
		}
		for (UINT i = 0; i < shaderDesc.BoundResources; i++)
		{
			D3D12_SHADER_INPUT_BIND_DESC csibDesc;
			pReflection->GetResourceBindingDesc(i, &csibDesc);
			auto pVari = pReflection->GetVariableByName(csibDesc.Name);
			D3D12_SHADER_VARIABLE_DESC varDesc;
			auto hr = pVari->GetDesc(&varDesc);
			if (SUCCEEDED(hr))
			{
				if ((varDesc.uFlags & D3D10_SVF_USED) == 0)
				{
					continue;
				}
			}
			auto binder = MakeWeakRef(new FShaderBinder());
			binder->Space = desc->Type;
			switch (csibDesc.Type)
			{
			case D3D_SIT_CBUFFER:
			{
				auto pCBuffer = pReflection->GetConstantBufferByName(csibDesc.Name);
				D3D12_SHADER_BUFFER_DESC cDesc;
				hr = pCBuffer->GetDesc(&cDesc);

				binder->Type = EShaderBindType::SBT_CBuffer;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->Size = cDesc.Size;
				binder->Space = csibDesc.Space;

				for (UINT j = 0; j < cDesc.Variables; j++)
				{
					auto pVari2 = pCBuffer->GetVariableByIndex(j);
					D3D12_SHADER_VARIABLE_DESC varDesc2;
					pVari2->GetDesc(&varDesc2);

					D3D12_SHADER_TYPE_DESC stDesc;
					pVari2->GetType()->GetDesc(&stDesc);

					auto tcvDesc = MakeWeakRef(new FShaderVarDesc());
					tcvDesc->Name = varDesc2.Name;
					tcvDesc->Size = varDesc2.Size;
					
					tcvDesc->Type = DxShaderVarType2VarType(stDesc.Type);
					tcvDesc->Columns = (USHORT)stDesc.Columns;
					tcvDesc->Offset = varDesc2.StartOffset;
					if (stDesc.Elements == 0)
						tcvDesc->Elements = 1;
					else
						tcvDesc->Elements = (USHORT)stDesc.Elements;

					binder->Fields.push_back(tcvDesc);
				}

				Reflector->CBuffers.push_back(binder);
			}
			break;
			case D3D_SIT_UAV_RWTYPED:
			case D3D_SIT_UAV_RWSTRUCTURED:
			case D3D_SIT_UAV_RWBYTEADDRESS:
			case D3D_SIT_UAV_APPEND_STRUCTURED:
			case D3D_SIT_UAV_CONSUME_STRUCTURED:
			case D3D_SIT_UAV_RWSTRUCTURED_WITH_COUNTER:
			{
				binder->Type = EShaderBindType::SBT_UAV;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->Space = csibDesc.Space;
				binder->IsStructuredBuffer = TRUE;

				Reflector->Uavs.push_back(binder);
			}
			break;
			case D3D_SIT_TBUFFER:
			{
				binder->Type = EShaderBindType::SBT_SRV;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->Space = csibDesc.Space;
				
				Reflector->Srvs.push_back(binder);
				/*if (desc->ShaderType == EShaderType::EST_ComputeShader)
				{
					TBufferBindInfo tmp;
					tmp.Name = csibDesc.Name;
					DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

					mTBufferBindArray.push_back(tmp);
				}
				else*/
				{
					//ASSERT(false);
					/*ShaderRViewBindInfo tmp;
					tmp.Name = csibDesc.Name;
					tmp.BufferType = EGpuBufferType::GBT_TBufferBuffer;
					DescSetBindPoint(desc->ShaderType, tmp, csibDesc);

					mSrvBindArray.push_back(tmp);*/
				}
			}
			break;
			case D3D_SIT_BYTEADDRESS:
			case D3D_SIT_STRUCTURED:
			{
				binder->Type = EShaderBindType::SBT_SRV;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->Space = csibDesc.Space;
				binder->IsStructuredBuffer = TRUE;

				Reflector->Srvs.push_back(binder);
			}
			break;
			case D3D_SIT_TEXTURE:
			{
				binder->Type = EShaderBindType::SBT_SRV;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->Space = csibDesc.Space;

				Reflector->Srvs.push_back(binder);
			}
			break;
			case D3D_SIT_SAMPLER:
			{
				binder->Type = EShaderBindType::SBT_Sampler;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->Space = csibDesc.Space;

				Reflector->Samplers.push_back(binder);
			}
			break;
			default:
				break;
			}
		}
		return true;
	}
}
NS_END