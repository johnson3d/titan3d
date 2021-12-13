#include "IShader.h"
#include "IConstantBuffer.h"
#include "ShaderReflector.h"
#include "IRenderSystem.h"
#include "../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::IShaderDefinitions, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::IShaderDesc, EngineNS::VIUnknown);

IShaderDesc::IShaderDesc()
{
	ShaderType = EST_UnknownShader;
	Reflector = new ShaderReflector();
}

IShaderDesc::IShaderDesc(EShaderType type)
{
	ShaderType = type;
	Reflector = new ShaderReflector();
}

IShaderDesc::~IShaderDesc()
{
	Safe_Release(Reflector);
}

void IShaderDesc::Save2Xnd(XndNode* node, DWORD platforms)
{
	UINT len = 0;
	if (/*platforms & PLTF_Windows && */Codes.size() > 0)
	{
		auto dxbc = node->GetOrAddAttribute("DXBC", 0, 0);
		dxbc->BeginWrite();
		len = (UINT)Codes.size();
		dxbc->Write(&len, sizeof(UINT));
		dxbc->Write(&Codes[0], len);
		dxbc->EndWrite();
	}

	if (/*platforms & PLTF_Android && */Es300Code.size() > 0)
	{
		auto gles = node->GetOrAddAttribute("ES300", 0, 0);
		gles->BeginWrite();
		len = (UINT)Es300Code.size();
		gles->Write(&len, sizeof(UINT));
		gles->Write(&Es300Code[0], len);
		gles->EndWrite();
	}

	if (/*platforms&PLTF_AppleIOS && */MetalCode.size() > 0)
	{
		auto metal = node->GetOrAddAttribute("METAL", 0, 0);
		metal->BeginWrite();
		len = (UINT)MetalCode.size();
		metal->Write(&len, sizeof(UINT));
		metal->Write(&MetalCode[0], len);
		metal->EndWrite();
	}

	if (SpirV.size() > 0)
	{
		auto metal = node->GetOrAddAttribute("SPIRV", 0, 0);
		metal->BeginWrite();
		len = (UINT)SpirV.size();
		metal->Write(&len, sizeof(UINT));
		metal->Write(&SpirV[0], len * sizeof(UINT));
		metal->EndWrite();
	}

	/*if (Reflector != nullptr)
	{
		auto rflct = node->GetOrAddAttribute("Reflector", 0, 0);
		rflct->BeginWrite();
		rflct->Write((UINT)Reflector->mCBDescArray.size());
		for (size_t i = 0; i < Reflector->mCBDescArray.size(); i++)
		{
			auto cb = &Reflector->mCBDescArray[i];
			cb->Save2Xnd(rflct);
		}

		rflct->Write((UINT)Reflector->mTexBindInfoArray.size());
		for (size_t i = 0; i < Reflector->mTexBindInfoArray.size(); i++)
		{
			auto tex = &Reflector->mTexBindInfoArray[i];
			rflct->Write(tex->Type);
			rflct->Write(tex->VSBindPoint);
			rflct->Write(tex->PSBindPoint);
			rflct->Write(tex->CSBindPoint);
			rflct->Write(tex->BindCount);
			rflct->Write(tex->Name);
		}

		rflct->Write((UINT)Reflector->mSamplerBindInfoArray.size());
		for (size_t i = 0; i < Reflector->mSamplerBindInfoArray.size(); i++)
		{
			auto samp = &Reflector->mSamplerBindInfoArray[i];
			rflct->Write(samp->Type);
			rflct->Write(samp->VSBindPoint);
			rflct->Write(samp->PSBindPoint);
			rflct->Write(samp->CSBindPoint);
			rflct->Write(samp->BindCount);
			rflct->Write(samp->Name);
		}
		rflct->EndWrite();
	}*/
}

vBOOL IShaderDesc::LoadXnd(XndNode* node)
{
	UINT len = 0;
	if (IRenderSystem::Instance->mRHIType == RHT_D3D11)
	{
		auto dxbc = node->TryGetAttribute("DXBC");
		if (dxbc == nullptr)
			return FALSE;

		dxbc->BeginRead();
		dxbc->Read(&len, sizeof(UINT));
		Codes.resize(len);
		dxbc->Read(&Codes[0], len);
		dxbc->EndRead();
	}

	if (IRenderSystem::Instance->mRHIType == RHT_OGL || IRenderSystem::Instance->mRHIType == RHT_VULKAN)
	{
		auto gles = node->TryGetAttribute("ES300");
		if (gles == nullptr)
			return FALSE;

		gles->BeginRead();
		gles->Read(&len, sizeof(UINT));
		Es300Code.resize(len + 1);
		gles->Read(&Es300Code[0], len);
		//Es300Code[len] = '\'
		gles->EndRead();
	}

	if (IRenderSystem::Instance->mRHIType == RHT_VULKAN)
	{
		auto spirvIR = node->TryGetAttribute("SPIRV");
		if (spirvIR == nullptr)
			return FALSE;

		spirvIR->BeginRead();
		spirvIR->Read(&len, sizeof(UINT));
		SpirV.resize(len);
		spirvIR->Read(&SpirV[0], len * sizeof(UINT));
		spirvIR->EndRead();
	}

	if (IRenderSystem::Instance->mRHIType == RHIType_Metal)
	{
		auto metal = node->TryGetAttribute("METAL");
		if (metal == nullptr)
			return FALSE;

		metal->BeginRead();
		metal->Read(&len, sizeof(UINT));
		MetalCode.resize(len + 1);
		metal->Read(&MetalCode[0], len);
		metal->EndRead();
	}

	/*auto rflct = node->TryGetAttribute("Reflector");
	if (rflct != nullptr)
	{
		Reflector = new ShaderReflector();

		rflct->BeginRead();
		UINT count = 0;
		rflct->Read(count);
		Reflector->mCBDescArray.resize(count);
		for (UINT i = 0; i < count; i++)
		{
			auto cb = &Reflector->mCBDescArray[i];
			cb->LoadXnd(rflct);
		}

		rflct->Read(count);
		Reflector->mTexBindInfoArray.resize(count);
		for (UINT i = 0; i < count; i++)
		{
			auto tex = &Reflector->mTexBindInfoArray[i];
			rflct->Read(tex->Type);
			rflct->Read(tex->VSBindPoint);
			rflct->Read(tex->PSBindPoint);
			rflct->Read(tex->CSBindPoint);
			rflct->Read(tex->BindCount);
			rflct->Read(tex->Name);
		}

		rflct->Read(count);
		Reflector->mSamplerBindInfoArray.resize(count);
		for (UINT i = 0; i < count; i++)
		{
			auto samp = &Reflector->mSamplerBindInfoArray[i];
			rflct->Read(samp->Type);
			rflct->Read(samp->VSBindPoint);
			rflct->Read(samp->PSBindPoint);
			rflct->Read(samp->CSBindPoint);
			rflct->Read(samp->BindCount);
			rflct->Read(samp->Name);
		}
		rflct->EndRead();
	}*/

	return TRUE;
}

IShader::IShader()
{
}


IShader::~IShader()
{
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
	MacroDefine tmp(name, value);
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

const MacroDefine* IShaderDefinitions::FindDefine(const char* name) const
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
NS_END
