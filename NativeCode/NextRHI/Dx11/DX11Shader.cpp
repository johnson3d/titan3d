#include "DX11Shader.h"
#include "DX11GpuDevice.h"
#include "../NxRHIDefine.h"

#if defined(HasModule_GpuDump)
#include "../../Bricks/GpuDump/NvAftermath.h"
#endif

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

			//std::string incFile = pFileName;
			std::string root = parent + pFileName;
			if ((strlen(pFileName) > 2 && pFileName[1] == ':') || (pFileName[0] == '@'))
			{
				root = pFileName;
			}
			auto ar = Compiler->GetShaderCodeStream(root.c_str(), pFileName);
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

	bool DX11Shader::CompileShader(FShaderCompiler* compiler, FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader)
	{
		desc->FunctionName = entry;
		AutoRef<FShaderCode> ar = compiler->GetShaderCodeStream(shader, shader);
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
			dwShaderFlags |= D3DCOMPILE_SKIP_OPTIMIZATION;			
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
			
			auto pos = pErrorStr.find(": error X");
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
		desc->Dxbc.resize((UINT)pBlob->GetBufferSize());
		memcpy(&desc->Dxbc[0], pBlob->GetBufferPointer(), (UINT)pBlob->GetBufferSize());

		DX11Shader::Reflect(desc);

		return true;
	}

	DX11Shader::DX11Shader()
	{
		mComputeShader = nullptr;
	}
	DX11Shader::~DX11Shader()
	{
		switch (Desc->Type)
		{
		case SDT_ComputeShader:
		{
			Safe_Release(mComputeShader);
		}
		break;
		case SDT_VertexShader:
		{
			Safe_Release(mVertexShader);
		}
		break;
		case SDT_PixelShader:
		{
			Safe_Release(mPixelShader);
		}
		break;
		}
	}
	bool DX11Shader::Init(DX11GpuDevice* device, FShaderDesc* desc)
	{
		Desc = desc;
		if (Desc->Dxbc.size() == 0)
			return false;

#if defined(HasModule_GpuDump)
		GpuDump::NvAftermath::RegByteCode(desc->DebugName.c_str(), &Desc->Dxbc[0], (UINT)Desc->Dxbc.size());
#endif
		//Reflect(desc);
		Reflector = desc->DxbcReflector;

		switch (desc->Type)
		{
			case SDT_ComputeShader:
			{
				auto hr = device->mDevice->CreateComputeShader(&desc->Dxbc[0], desc->Dxbc.size(), NULL, &mComputeShader);
				if (FAILED(hr))
					return false;
			}
			break;
			case SDT_VertexShader:
			{
				auto hr = device->mDevice->CreateVertexShader(&desc->Dxbc[0], desc->Dxbc.size(), NULL, &mVertexShader);
				if (FAILED(hr))
					return false;
			}
			break;
			case SDT_PixelShader:
			{
				auto hr = device->mDevice->CreatePixelShader(&desc->Dxbc[0], desc->Dxbc.size(), NULL, &mPixelShader);
				if (FAILED(hr))
					return false;
			}
			break;
		}
		return true;
	}
	static EShaderVarType DxShaderVarType2VarType(D3D_SHADER_VARIABLE_TYPE type)
	{
		switch (type)
		{
			case D3D10_SVT_INT:
			{
				return EShaderVarType::SVT_Int;
			}
			break;
			case D3D10_SVT_FLOAT:
			{
				return EShaderVarType::SVT_Float;
			}
			break;
			case D3D10_SVT_TEXTURE:
			{
				return EShaderVarType::SVT_Texture;
			}
			break;
			case D3D10_SVT_SAMPLER:
			{
				return EShaderVarType::SVT_Sampler;
			}
			break;
			default:
				return EShaderVarType::SVT_Unknown;
		}
	}
	static EVertexStreamType GetStreamType(const D3D11_SIGNATURE_PARAMETER_DESC& desc)
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

	bool DX11Shader::Reflect(FShaderDesc* desc)
	{
		desc->DxbcReflector = MakeWeakRef(new IShaderReflector());
		auto Reflector = desc->DxbcReflector;
		ID3D11ShaderReflection* pReflection;
		auto hr = D3DReflect(&desc->Dxbc[0], desc->Dxbc.size(), IID_ID3D11ShaderReflection, (void**)&pReflection);
		if (FAILED(hr))
		{
			return false;
		}
		D3D11_SHADER_DESC shaderDesc;
		pReflection->GetDesc(&shaderDesc);

		desc->InputStreams.clear();
		for (UINT i = 0; i < shaderDesc.InputParameters; i++)
		{
			D3D11_SIGNATURE_PARAMETER_DESC iptDesc;
			if (pReflection->GetInputParameterDesc(i, &iptDesc) == S_OK)
			{
				auto type = GetStreamType(iptDesc);
				if (type != VST_Number)
					desc->InputStreams.push_back(type);
			}
		}
		for (UINT i = 0; i < shaderDesc.BoundResources; i++)
		{
			D3D11_SHADER_INPUT_BIND_DESC csibDesc;
			pReflection->GetResourceBindingDesc(i, &csibDesc);
			ID3D11ShaderReflectionVariable* pVari = pReflection->GetVariableByName(csibDesc.Name);
			D3D11_SHADER_VARIABLE_DESC varDesc;
			hr = pVari->GetDesc(&varDesc);
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
				ID3D11ShaderReflectionConstantBuffer* pCBuffer = pReflection->GetConstantBufferByName(csibDesc.Name);
				D3D11_SHADER_BUFFER_DESC cDesc;
				hr = pCBuffer->GetDesc(&cDesc);

				binder->Type = EShaderBindType::SBT_CBuffer;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->BindCount = csibDesc.BindCount;
				binder->Size = cDesc.Size;
				binder->Space = 0;

				for (UINT j = 0; j < cDesc.Variables; j++)
				{
					ID3D11ShaderReflectionVariable* pVari2 = pCBuffer->GetVariableByIndex(j);
					D3D11_SHADER_VARIABLE_DESC varDesc2;
					pVari2->GetDesc(&varDesc2);

					D3D11_SHADER_TYPE_DESC stDesc;
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
				binder->BindCount = csibDesc.BindCount;
				binder->IsStructuredBuffer = TRUE;

				Reflector->Uavs.push_back(binder);
			}
			break;
			case D3D_SIT_TBUFFER:
			{
				binder->Type = EShaderBindType::SBT_SRV;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->BindCount = csibDesc.BindCount;

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
				binder->BindCount = csibDesc.BindCount;
				binder->IsStructuredBuffer = TRUE;
				Reflector->Srvs.push_back(binder);
			}
			break;
			case D3D_SIT_TEXTURE:
			{
				binder->Type = EShaderBindType::SBT_SRV;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->BindCount = csibDesc.BindCount;
				Reflector->Srvs.push_back(binder);
			}
			break;
			case D3D_SIT_SAMPLER:
			{
				binder->Type = EShaderBindType::SBT_Sampler;
				binder->Name = csibDesc.Name;
				binder->Slot = csibDesc.BindPoint;
				binder->BindCount = csibDesc.BindCount;
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