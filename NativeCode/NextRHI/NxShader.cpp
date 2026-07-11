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
		pAttr->Write(BindCount);
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
		pAttr->Read(BindCount);
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
			const_cast<FShaderBinder*>(i.GetPtr())->SaveXnd(pAttr);
		}

		pAttr->Write((UINT)Uavs.size());
		for (auto& i : Uavs)
		{
			const_cast<FShaderBinder*>(i.GetPtr())->SaveXnd(pAttr);
		}

		pAttr->Write((UINT)Srvs.size());
		for (auto& i : Srvs)
		{
			const_cast<FShaderBinder*>(i.GetPtr())->SaveXnd(pAttr);
		}

		pAttr->Write((UINT)Samplers.size());
		for (auto& i : Samplers)
		{
			const_cast<FShaderBinder*>(i.GetPtr())->SaveXnd(pAttr);
		}
	}
	bool IShaderReflector::LoadXnd(IGpuDevice* device, XndAttribute* pAttr, EShaderType stage)
	{
		UINT count = 0;

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder(stage));
			tmp->LoadXnd(device, pAttr);
			CBuffers.push_back(tmp);
		}

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder(stage));
			tmp->LoadXnd(device, pAttr);
			Uavs.push_back(tmp);
		}

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder(stage));
			tmp->LoadXnd(device, pAttr);
			Srvs.push_back(tmp);
		}

		pAttr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tmp = MakeWeakRef(new FShaderBinder(stage));
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
		case EShaderBindType::SBT_CBV:
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
		{
			auto pAttr = node->GetOrAddAttribute("ShaderDesc", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(Type);
			pAttr->Write(Language);
			pAttr->Write(DebugName);
			pAttr->Write(FunctionName);
			pAttr->EndWrite();
		}

		if (RhiData.size() > 0)
		{
			auto pAttr = node->GetOrAddAttribute("RhiData", 0, 0);
			pAttr->BeginWrite();
			pAttr->Write(&RhiData[0], (UINT)RhiData.size());
			pAttr->EndWrite();

			if (Reflector != nullptr)
			{
				pAttr = node->GetOrAddAttribute("Reflector", 0, 0);
				pAttr->BeginWrite();
				Reflector->SaveXnd(pAttr);
				pAttr->EndWrite();
			}
		}
	}
	bool FShaderDesc::LoadXnd(IGpuDevice* device, XndNode* node)
	{
		{
			auto pAttr = node->FindFirstAttribute("ShaderDesc");
			if (pAttr == nullptr)
				return false;
			pAttr->BeginRead();
			pAttr->Read(Type);
			pAttr->Read(Language);
			pAttr->Read(DebugName);
			pAttr->Read(FunctionName);
			pAttr->EndRead();
		}

		auto pAttr = node->FindFirstAttribute("RhiData");
		if (pAttr != nullptr)
		{
			pAttr->BeginRead();
			RhiData.resize((size_t)pAttr->GetReaderLength());
			if (RhiData.size() > 0)
				pAttr->Read(&RhiData[0], (UINT)RhiData.size());
			pAttr->EndRead();

			pAttr = node->FindFirstAttribute("Reflector");
			if (pAttr != nullptr)
			{
				Reflector = MakeWeakRef(new IShaderReflector());
				pAttr->BeginRead();
				Reflector->LoadXnd(device, pAttr, this->Type);
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

		return true;
	}
	const AutoRef<IShader>& FShaderDesc::GetOrCreateShader(IGpuDevice* pDevice)
	{
		if (Shader == nullptr)
		{
			AutoRef<FShaderDesc> pDesc(this);
			Shader = MakeWeakRef(pDevice->CreateShader(pDesc, __FILE__, __LINE__));
		}
		return Shader;
	}

	FShaderCode* FShaderCompiler::GetShaderCodeStream(const char* name, const char* oriName)
	{
		if (GetShaderCodeStreamPtr == nullptr)
			return nullptr;
		return GetShaderCodeStreamPtr(name, oriName);
	}
	bool FShaderCompiler::CompileShader(FShaderDesc* desc, const char* shader, const char* entry, EShaderType type, const char* sm, const IShaderDefinitions* defines, EShaderLanguage sl, bool bDebugShader, const char* extHlslVersion, const char* dxcArgs, IBlobObject* output, bool asModule)
	{
		if (sl == EShaderLanguage::SL_DXBC)
		{
#if defined(HasModule_Dx11)
			return DX11Shader::CompileShader(this, desc, shader, entry, type, sm, defines, sl, bDebugShader, output);
#else
			ASSERT(false);
			return false;
#endif
		}
		if (sl == EShaderLanguage::SL_DXIL)
		{
#if defined(HasModule_Dx12)
			return DX12Shader::CompileShader(this, desc, shader, entry, type, sm, defines, sl, bDebugShader, extHlslVersion, dxcArgs, output, asModule);
#else
			ASSERT(false);
			return false;
#endif
		}
		if (sl == EShaderLanguage::SL_SPIRV)
		{
#if defined(HasModule_Vulkan)
			return VKShader::CompileShader(this, desc, shader, entry, type, sm, defines, sl, bDebugShader, extHlslVersion, dxcArgs, output, asModule);
#else
			ASSERT(false);
			return false;
#endif
		}
		return false;
	}
}

NS_END