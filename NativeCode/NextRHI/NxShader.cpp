#include "NxShader.h"

#if defined(HasModule_Dx11)
#include "Dx11/DX11Shader.h"
#endif

#if defined(HasModule_Dx12)
#include "Dx12/DX12Shader.h"
#endif

#if defined(HasModule_Vulkan)
#include "Vulkan/VKShader.h"
#endif

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	const char* FShaderCode::GetSourceCode() const
	{
		return SourceCode.c_str();
	}
	void FShaderBinder::SaveXnd(XndAttribute* pAttr)
	{
		pAttr->Write(Name);
		pAttr->Write(Type);
		pAttr->Write(Space);
		pAttr->Write(Slot);
		pAttr->Write(Size);
		pAttr->Write(IsStructuredBuffer);
		pAttr->Write(DescriptorIndex);
		pAttr->Write((UINT)Fields.size());
		for (auto& i : Fields)
		{
			pAttr->Write(i->Name);
			pAttr->Write(i->Type);
			pAttr->Write(i->Columns);
			pAttr->Write(i->Elements);
			pAttr->Write(i->Offset);
			pAttr->Write(i->Size);
		}
	}
	bool FShaderBinder::LoadXnd(IGpuDevice* device, XndAttribute* pAttr)
	{
		pAttr->Read(Name);
		pAttr->Read(Type);
		pAttr->Read(Space);
		pAttr->Read(Slot);
		pAttr->Read(Size);
		pAttr->Read(IsStructuredBuffer);
		pAttr->Read(DescriptorIndex);
		UINT count = 0;
		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto fld = MakeWeakRef(new FShaderVarDesc());
			pAttr->Read(fld->Name);
			pAttr->Read(fld->Type);
			pAttr->Read(fld->Columns);
			pAttr->Read(fld->Elements);
			pAttr->Read(fld->Offset);
			pAttr->Read(fld->Size);
			Fields.push_back(fld);
		}
		return true;
	}
	const FShaderVarDesc* FShaderBinder::FindField(const char* name) const
	{
		for (const auto& i : Fields)
		{
			if (i->Name == name)
				return i;
		}
		return nullptr;
	}
	void IShaderReflector::SaveXnd(XndAttribute* pAttr)
	{		
		pAttr->Write((UINT)CBuffers.size());
		for (auto& i : CBuffers)
		{
			i->SaveXnd(pAttr);
		}

		pAttr->Write((UINT)Uavs.size());
		for (auto& i : Uavs)
		{
			i->SaveXnd(pAttr);
		}

		pAttr->Write((UINT)Srvs.size());
		for (auto& i : Srvs)
		{
			i->SaveXnd(pAttr);
		}

		pAttr->Write((UINT)Samplers.size());
		for (auto& i : Samplers)
		{
			i->SaveXnd(pAttr);
		}
	}
	bool IShaderReflector::LoadXnd(IGpuDevice* device, XndAttribute* pAttr)
	{
		UINT count = 0;

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder());
			tmp->LoadXnd(device, pAttr);
			CBuffers.push_back(tmp);
		}

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder());
			tmp->LoadXnd(device, pAttr);
			Uavs.push_back(tmp);
		}

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder());
			tmp->LoadXnd(device, pAttr);
			Srvs.push_back(tmp);
		}

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder());
			tmp->LoadXnd(device, pAttr);
			Samplers.push_back(tmp);
		}

		return true;
	}
	const FShaderBinder* IShaderReflector::FindBinder(EShaderBindType type, const char* name) const
	{
		return FindBinder(type, VNameString(name));
	}
	const FShaderBinder* IShaderReflector::FindBinder(EShaderBindType type, VNameString name) const
	{
		switch (type)
		{
		case EShaderBindType::SBT_CBuffer:
		{
			for (const auto& i : CBuffers)
			{
				if (i->Name == name)
					return i;
			}
		}
		break;
		case EShaderBindType::SBT_SRV:
		{
			for (const auto& i : Srvs)
			{
				if (i->Name == name)
					return i;
			}
		}
		break;
		case EShaderBindType::SBT_UAV:
		{
			for (const auto& i : Uavs)
			{
				if (i->Name == name)
					return i;
			}
		}
		break;
		case EShaderBindType::SBT_Sampler:
		{
			for (const auto& i : Samplers)
			{
				if (i->Name == name)
					return i;
			}
		}
		break;
		default:
			break;
		}
		return nullptr;
	}

	void IShaderDefinitions::ClearDefines()
	{
		Definitions.clear();
	}

	void IShaderDefinitions::AddDefine(const char* name, const char* value)
	{
		for (auto i = Definitions.begin(); i != Definitions.end(); i++)
		{
			if (i->Name == name)
			{
				i->Definition = value;
				return;
			}
		}
		FMacroDefine tmp(name, value);
		Definitions.push_back(tmp);
	}

	void IShaderDefinitions::RemoveDefine(const char* name)
	{
		for (auto i = Definitions.begin(); i != Definitions.end(); i++)
		{
			if (i->Name == name)
			{
				Definitions.erase(i);
				return;
			}
		}
	}

	void IShaderDefinitions::MergeDefinitions(IShaderDefinitions* def)
	{
		for (auto i : def->Definitions)
		{
			AddDefine(i.Name.c_str(), i.Definition.c_str());
		}
	}

	const FMacroDefine* IShaderDefinitions::FindDefine(const char* name) const
	{
		for (auto i = Definitions.begin(); i != Definitions.end(); i++)
		{
			if (i->Name == name)
			{
				return &(*i);
			}
		}
		return nullptr;
	}
	void FShaderDesc::SaveXnd(XndNode* node)
	{
		//node->SetName(FunctionName.c_str());
		{
			auto pAttr = node->GetOrAddAttribute("ShaderDesc", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(Type);
			pAttr->Write(FunctionName);
			pAttr->EndWrite();
		}

		if (Dxbc.size() > 0)
		{
			auto pAttr = node->GetOrAddAttribute("DXBC", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(&Dxbc[0], (UINT)Dxbc.size());
			pAttr->EndWrite();

			if (DxbcReflector != nullptr)
			{
				pAttr = node->GetOrAddAttribute("DxbcReflector", 0, 0);
				pAttr->BeginWrite();
				DxbcReflector->SaveXnd(pAttr);
				pAttr->EndWrite();
			}
		}
		if (DxIL.size() > 0)
		{
			auto pAttr = node->GetOrAddAttribute("DXIL", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(&DxIL[0], (UINT)DxIL.size());
			pAttr->EndWrite();

			if (DxILReflector != nullptr)
			{
				pAttr = node->GetOrAddAttribute("DxilReflector", 0, 0);
				pAttr->BeginWrite();
				DxILReflector->SaveXnd(pAttr);
				pAttr->EndWrite();
			}
		}
		if (SpirV.size() > 0)
		{
			auto pAttr = node->GetOrAddAttribute("SPIRV", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(&SpirV[0], (UINT)SpirV.size());
			pAttr->EndWrite();

			if (SpirvReflector != nullptr)
			{
				pAttr = node->GetOrAddAttribute("SpirvReflector", 0, 0);
				pAttr->BeginWrite();
				SpirvReflector->SaveXnd(pAttr);
				pAttr->EndWrite();
			}
		}
		if (Es300Code.size() > 0)
		{
			auto pAttr = node->GetOrAddAttribute("GLES", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(Es300Code);
			pAttr->EndWrite();
		}
		if (MetalCode.size() > 0)
		{
			auto pAttr = node->GetOrAddAttribute("METAL", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(MetalCode);
			pAttr->EndWrite();
		}
	}
	bool FShaderDesc::LoadXnd(IGpuDevice* device, XndNode* node)
	{
		//FunctionName = node->GetName();
		{
			auto pAttr = node->FindFirstAttribute("ShaderDesc");
			pAttr->BeginRead();
			pAttr->Read(Type);
			pAttr->Read(FunctionName);
			pAttr->EndRead();
		}
		switch (device->Desc.RhiType)
		{
			case ERhiType::RHI_D3D11:
			{
				auto pAttr = node->FindFirstAttribute("DXBC");
				if (pAttr != nullptr)
				{
					pAttr->BeginRead();
					Dxbc.resize((size_t)pAttr->GetReaderLength());
					pAttr->Read(&Dxbc[0], (UINT)Dxbc.size());
					pAttr->EndRead();

					pAttr = node->FindFirstAttribute("DxbcReflector");
					if (pAttr != nullptr)
					{
						DxbcReflector = MakeWeakRef(new IShaderReflector());
						pAttr->BeginRead();
						DxbcReflector->LoadXnd(device, pAttr);
						pAttr->EndRead();
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			break;
			case ERhiType::RHI_D3D12:
			{
				auto pAttr = node->FindFirstAttribute("DXIL");
				if (pAttr != nullptr)
				{
					pAttr->BeginRead();
					DxIL.resize((size_t)pAttr->GetReaderLength());
					pAttr->Read(&DxIL[0], (UINT)DxIL.size());
					pAttr->EndRead();

					pAttr = node->FindFirstAttribute("DxilReflector");
					if (pAttr != nullptr)
					{
						DxILReflector = MakeWeakRef(new IShaderReflector());
						pAttr->BeginRead();
						DxILReflector->LoadXnd(device, pAttr);
						pAttr->EndRead();
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			break;
			case ERhiType::RHI_VK:
			{
				auto pAttr = node->FindFirstAttribute("SPIRV");
				if (pAttr != nullptr)
				{
					pAttr->BeginRead();
					SpirV.resize((size_t)pAttr->GetReaderLength());
					pAttr->Read(&SpirV[0], (UINT)SpirV.size());
					pAttr->EndRead();

					pAttr = node->FindFirstAttribute("SpirvReflector");
					if (pAttr != nullptr)
					{
						SpirvReflector = MakeWeakRef(new IShaderReflector());
						pAttr->BeginRead();
						SpirvReflector->LoadXnd(device, pAttr);
						pAttr->EndRead();
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			break;
			case ERhiType::RHI_GL:
			{
				auto pAttr = node->FindFirstAttribute("GLES");
				if (pAttr != nullptr)
				{
					pAttr->BeginRead();
					pAttr->Read(Es300Code);
					pAttr->EndRead();
				}
				else
				{
					return false;
				}
			}
			break;
			case ERhiType::RHI_Metal:
			{
				auto pAttr = node->FindFirstAttribute("METAL");
				if (pAttr != nullptr)
				{
					pAttr->BeginRead();
					pAttr->Read(MetalCode);
					pAttr->EndRead();
				}
				else
				{
					return false;
				}
			}
			break;
			default:
				break;
		}
		
		return true;
	}
	const AutoRef<IShader>& FShaderDesc::GetOrCreateShader(IGpuDevice* pDevice)
	{
		if (Shader == nullptr)
		{
			AutoRef<FShaderDesc> pDesc(this);
			Shader = MakeWeakRef(pDevice->CreateShader(pDesc));
		}
		return Shader;
	}

	FShaderCode* FShaderCompiler::GetShaderCodeStream(const char* name)
	{
		if (GetShaderCodeStreamPtr == nullptr)
			return nullptr;
		return GetShaderCodeStreamPtr(name);
	}
	bool FShaderCompiler::CompileShader(FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader)
	{
		if (sl == EShaderLanguage::SL_DXBC)
		{
#if defined(HasModule_Dx11)
			return DX11Shader::CompileShader(this, desc, shader, entry, type, sm, defines, sl, bDebugShader);
#else
			ASSERT(false);
			return false;
#endif
		}
		if (sl == EShaderLanguage::SL_DXIL)
		{
#if defined(HasModule_Dx12)
			return DX12Shader::CompileShader(this, desc, shader, entry, type, sm, defines, sl, bDebugShader);
#else
			ASSERT(false);
			return false;
#endif
		}
		if (sl == EShaderLanguage::SL_SPIRV)
		{
#if defined(HasModule_Vulkan)
			return VKShader::CompileShader(this, desc, shader, entry, type, sm, defines, sl, bDebugShader);
#else
			ASSERT(false);
			return false;
#endif
		}
		return false;
	}
}

NS_END