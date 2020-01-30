#include "IShader.h"
#include "IConstantBuffer.h"
#include "ShaderReflector.h"
#include "IRenderSystem.h"
#include "../Core/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::IShaderDefinitions, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::IShaderDesc, EngineNS::VIUnknown);

IShaderDesc::~IShaderDesc()
{
	Safe_Release(Reflector);
}

UINT IShaderDesc::GetCBufferNum() 
{
	if (Reflector == nullptr)
		return 0;
	return (UINT)Reflector->mCBDescArray.size();
}

UINT IShaderDesc::GetSRVNum() 
{
	if (Reflector == nullptr)
		return 0;
	return (UINT)Reflector->mTexBindInfoArray.size();
}

UINT IShaderDesc::GetSamplerNum() 
{
	if (Reflector == nullptr)
		return 0;
	return (UINT)Reflector->mSamplerBindInfoArray.size();
}

vBOOL IShaderDesc::GetCBufferDesc(UINT index, IConstantBufferDesc* info)
{
	if (Reflector == nullptr)
		return FALSE;
	if (index >= Reflector->mCBDescArray.size())
		return FALSE;
	const auto& cb = Reflector->mCBDescArray[index];

	cb.CopyBaseData(info);

	return TRUE;
}

vBOOL IShaderDesc::GetSRVDesc(UINT index, TextureBindInfo* info)
{
	if (Reflector == nullptr)
		return FALSE;
	if (index >= Reflector->mTexBindInfoArray.size())
		return FALSE;
	const auto& cb = Reflector->mTexBindInfoArray[index];
	info->Type = cb.Type;
	info->BindCount = cb.BindCount;
	info->PSBindPoint = cb.PSBindPoint;
	info->VSBindPoint = cb.VSBindPoint;
	info->CSBindPoint = cb.CSBindPoint;

	return TRUE;
}

vBOOL IShaderDesc::GetSamplerDesc(UINT index, SamplerBindInfo* info)
{
	if (Reflector == nullptr)
		return FALSE;
	if (index >= Reflector->mCBDescArray.size())
		return FALSE;
	const auto& cb = Reflector->mSamplerBindInfoArray[index];
	info->Type = cb.Type;
	info->BindCount = cb.BindCount;
	info->PSBindPoint = cb.PSBindPoint;
	info->VSBindPoint = cb.VSBindPoint;
	info->CSBindPoint = cb.CSBindPoint;

	return TRUE;
}

UINT IShaderDesc::FindCBufferDesc(const char* name) 
{
	if (Reflector == nullptr)
		return 0;

	for (UINT i = 0; i < (UINT)Reflector->mCBDescArray.size(); i++)
	{
		if (Reflector->mCBDescArray[i].Name == name)
			return i;
	}
	return -1;
}

UINT IShaderDesc::FindSRVDesc(const char* name) 
{
	if (Reflector == nullptr)
		return 0;

	for (UINT i = 0; i < (UINT)Reflector->mTexBindInfoArray.size(); i++)
	{
		if (Reflector->mTexBindInfoArray[i].Name == name)
			return i;
	}
	return -1;
}

UINT IShaderDesc::FindSamplerDesc(const char* name) 
{
	if (Reflector == nullptr)
		return 0;

	for (UINT i = 0; i < (UINT)Reflector->mSamplerBindInfoArray.size(); i++)
	{
		if (Reflector->mSamplerBindInfoArray[i].Name == name)
			return i;
	}
	return -1;
}

void IShaderDesc::Save2Xnd(XNDNode* node, DWORD platforms)
{
	UINT len = 0;
	if (platforms&PLTF_Windows)
	{
		auto dxbc = node->AddAttrib("DXBC");
		dxbc->BeginWrite();
		len = (UINT)Codes.size();
		dxbc->Write(&len, sizeof(UINT));
		dxbc->Write(&Codes[0], len);
		dxbc->EndWrite();
	}

	if (platforms&PLTF_Android)
	{
		auto gles = node->AddAttrib("ES300");
		gles->BeginWrite();
		len = (UINT)Es300Code.size();
		gles->Write(&len, sizeof(UINT));
		gles->Write(&Es300Code[0], len);
		gles->EndWrite();
	}

	if (platforms&PLTF_AppleIOS)
	{
		auto metal = node->AddAttrib("METAL");
		metal->BeginWrite();
		len = (UINT)MetalCode.size();
		metal->Write(&len, sizeof(UINT));
		metal->Write(&MetalCode[0], len);
		metal->EndWrite();
	}

	if (Reflector != nullptr)
	{
		auto rflct = node->AddAttrib("Reflector");
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
			rflct->WriteText(tex->Name.c_str());
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
			rflct->WriteText(samp->Name.c_str());
		}
		rflct->EndWrite();
	}
}

vBOOL IShaderDesc::LoadXnd(XNDNode* node)
{
	UINT len = 0;
	if (IRenderSystem::Instance->mRHIType == RHT_D3D11)
	{
		auto dxbc = node->GetAttrib("DXBC");
		if (dxbc == nullptr)
			return FALSE;

		dxbc->BeginRead(__FILE__, __LINE__);
		dxbc->Read(&len, sizeof(UINT));
		Codes.resize(len);
		dxbc->Read(&Codes[0], len);
		dxbc->EndRead();
	}

	if (IRenderSystem::Instance->mRHIType == RHT_OGL)
	{
		auto gles = node->GetAttrib("ES300");
		if (gles == nullptr)
			return FALSE;

		gles->BeginRead(__FILE__, __LINE__);
		gles->Read(&len, sizeof(UINT));
		Es300Code.resize(len + 1);
		gles->Read(&Es300Code[0], len);
		//Es300Code[len] = '\'
		gles->EndRead();
	}

	if (IRenderSystem::Instance->mRHIType == RHIType_Metal)
	{
		auto metal = node->GetAttrib("METAL");
		if (metal == nullptr)
			return FALSE;

		metal->BeginRead(__FILE__, __LINE__);
		metal->Read(&len, sizeof(UINT));
		MetalCode.resize(len + 1);
		metal->Read(&MetalCode[0], len);
		metal->EndRead();
	}

	auto rflct = node->GetAttrib("Reflector");
	if (rflct != nullptr)
	{
		Reflector = new ShaderReflector();

		rflct->BeginRead(__FILE__, __LINE__);
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
			rflct->ReadText(tex->Name);
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
			rflct->ReadText(samp->Name);
		}
		rflct->EndRead();
	}

	return TRUE;
}

IShader::IShader()
{
}


IShader::~IShader()
{
}

UINT32 IShader::GetConstantBufferNumber() const
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return 0;
	return (UINT32)mDesc->Reflector->mCBDescArray.size();
}

bool IShader::GetConstantBufferDesc(UINT32 Index, IConstantBufferDesc* desc)
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return false;
	if (Index >= mDesc->Reflector->mCBDescArray.size())
		return false;
	*desc = mDesc->Reflector->mCBDescArray[Index];
	return true;
}

const IConstantBufferDesc* IShader::FindCBufferDesc(const char* name)
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return nullptr;
	for (size_t i=0;i<mDesc->Reflector->mCBDescArray.size();i++)
	{
		if (mDesc->Reflector->mCBDescArray[i].Name == name)
			return &mDesc->Reflector->mCBDescArray[i];
	}
	return nullptr;
}

UINT IShader::GetShaderResourceNumber() const
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return 0;
	return (UINT)mDesc->Reflector->mTexBindInfoArray.size();
}

bool IShader::GetShaderResourceBindInfo(UINT Index, TextureBindInfo* info) const
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return false;
	if (Index >= mDesc->Reflector->mTexBindInfoArray.size())
		return false;
	*info = mDesc->Reflector->mTexBindInfoArray[Index];
	return true;
}

const TextureBindInfo* IShader::FindTextureBindInfo(const char* name)
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return nullptr;
	for (size_t i = 0; i < mDesc->Reflector->mTexBindInfoArray.size(); i++)
	{
		if (mDesc->Reflector->mTexBindInfoArray[i].Name == name)
			return &mDesc->Reflector->mTexBindInfoArray[i];
	}
	return nullptr;
}

UINT IShader::GetSamplerNumber() const
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return 0;
	return (UINT)mDesc->Reflector->mSamplerBindInfoArray.size();
}

bool IShader::GetSamplerBindInfo(UINT Index, SamplerBindInfo* info) const
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return false;
	if (Index >= mDesc->Reflector->mTexBindInfoArray.size())
		return false;
	*info = mDesc->Reflector->mSamplerBindInfoArray[Index];
	return true;
}

const SamplerBindInfo* IShader::FindSamplerBindInfo(const char* name)
{
	if (mDesc == nullptr || mDesc->Reflector == nullptr)
		return nullptr;
	for (size_t i = 0; i < mDesc->Reflector->mSamplerBindInfoArray.size(); i++)
	{
		if (mDesc->Reflector->mSamplerBindInfoArray[i].Name == name)
			return &mDesc->Reflector->mSamplerBindInfoArray[i];
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
	MacroDefine tmp;
	tmp.Name = name;
	tmp.Definition = value;
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

using namespace EngineNS;
extern "C"
{
	CSharpAPI0(EngineNS, IShaderDefinitions, ClearDefines);
	CSharpAPI1(EngineNS, IShaderDefinitions, RemoveDefine, const char*);
	CSharpAPI2(EngineNS, IShaderDefinitions, AddDefine, const char*, const char*);
	CSharpAPI1(EngineNS, IShaderDefinitions, MergeDefinitions, IShaderDefinitions*);
	CSharpAPI1(EngineNS, IShaderDesc, SetGLCode, const char*);
	CSharpAPI1(EngineNS, IShaderDesc, SetMetalCode, const char*);
	CSharpReturnAPI0(const char*, EngineNS, IShaderDesc, GetGLCode);
	CSharpReturnAPI0(const char*, EngineNS, IShaderDesc, GetMetalCode);
	CSharpAPI1(EngineNS, IShaderDesc, SetShaderType, EShaderType);
	CSharpReturnAPI0(EShaderType, EngineNS, IShaderDesc, GetShaderType);
	CSharpAPI2(EngineNS, IShaderDesc, Save2Xnd, XNDNode*, DWORD);
	CSharpReturnAPI1(vBOOL, EngineNS, IShaderDesc, LoadXnd, XNDNode*);

	CSharpReturnAPI0(UINT, EngineNS, IShaderDesc, GetCBufferNum);
	CSharpReturnAPI0(UINT, EngineNS, IShaderDesc, GetSRVNum);
	CSharpReturnAPI0(UINT, EngineNS, IShaderDesc, GetSamplerNum);
	CSharpReturnAPI2(vBOOL, EngineNS, IShaderDesc, GetCBufferDesc, UINT, IConstantBufferDesc*);
	CSharpReturnAPI2(vBOOL, EngineNS, IShaderDesc, GetSRVDesc, UINT, TextureBindInfo*);
	CSharpReturnAPI2(vBOOL, EngineNS, IShaderDesc, GetSamplerDesc, UINT, SamplerBindInfo*);
	CSharpReturnAPI1(UINT, EngineNS, IShaderDesc, FindCBufferDesc, const char*);
	CSharpReturnAPI1(UINT, EngineNS, IShaderDesc, FindSRVDesc, const char*);
	CSharpReturnAPI1(UINT, EngineNS, IShaderDesc, FindSamplerDesc, const char*);
}