#include "GfxShadingEnv.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxShadingEnv, EngineNS::VIUnknown)

GfxShadingEnv::GfxShadingEnv()
{
	//mVersion = 0;
}


GfxShadingEnv::~GfxShadingEnv()
{
	Cleanup();
}

void GfxShadingEnv::Cleanup()
{
	//for (auto i : mVars)
	//{
	//	Safe_Delete(i.second);
	//}
	//mVars.clear();
	//for (auto i : mSRViews)
	//{
	//	Safe_Delete(i.second);
	//}
	//mSRViews.clear();
}

//Hash64 GfxShadingEnv::GetHash64()
//{
//	auto hashStr = VStringA_FormatV("%s", mShaderName.c_str());
//	return HashHelper::CalcHash64(hashStr.c_str(), (int)hashStr.length());
//}

//vBOOL GfxShadingEnv::Init(const char* name, const char* shader)
//{
//	mName = name;
//	mShaderName = shader;
//
//	return TRUE;
//}

//const char* GfxShadingEnv::GetShaderName() const
//{
//	return mShaderName.c_str();
//}

//void GfxShadingEnv::Save2Xnd(XNDNode* node)
//{
//	auto attr = node->AddAttrib("Desc");
//	attr->BeginWrite();
//	attr->Write(mVersion);
//	attr->WriteStringAsRName(mName);
//	attr->WriteText(mShaderName.c_str());
//	attr->EndWrite();
//
//	attr = node->AddAttrib("Vars");
//	attr->BeginWrite();
//	auto count = (UINT)mVars.size();
//	attr->Write(&count, sizeof(UINT));
//	for (auto i : mVars)
//	{
//		attr->WriteText(i.second->Definition.Name.c_str());
//		auto type = (UINT)i.second->Definition.Type;
//		attr->Write(&type, sizeof(UINT));
//		attr->Write(&i.second->Definition.Elements, sizeof(UINT));
//		ASSERT(i.second->ValueArray.size() == i.second->Definition.Elements * GetShaderVarTypeSize(i.second->Definition.Type));
//		attr->Write(&i.second->ValueArray[0], i.second->ValueArray.size());
//	}
//	attr->EndWrite();
//
//	attr = node->AddAttrib("SRV");
//	attr->BeginWrite();
//	count = (UINT)mSRViews.size();
//	attr->Write(&count, sizeof(UINT));
//	for (auto i : mSRViews)
//	{
//		attr->WriteText(i.second->ShaderName.c_str());
//		attr->WriteStringAsRName(i.second->RName);
//	}
//	attr->EndWrite();
//}

//vBOOL GfxShadingEnv::LoadXnd(XNDNode* node)
//{
//	auto attr = node->GetAttrib("Desc");
//	attr->BeginRead(__FILE__, __LINE__);
//	attr->Read(mVersion);
//	attr->ReadStringAsRName(mName);
//	attr->ReadText(mShaderName);
//	attr->EndRead();
//
//	attr = node->GetAttrib("Vars");
//	attr->BeginRead(__FILE__, __LINE__);
//	attr->Read(&mVersion, sizeof(UINT));
//	UINT count = 0;
//	attr->Read(&count, sizeof(UINT));
//	for (UINT i = 0; i < count; i++)
//	{
//		auto p = new GfxVarValue();
//		attr->ReadText(p->Definition.Name);
//		attr->Read(&p->Definition.Type, sizeof(UINT));
//		attr->Read(&p->Definition.Elements, sizeof(UINT));
//		p->ValueArray.resize(p->Definition.Elements * GetShaderVarTypeSize(p->Definition.Type));
//		attr->Read(&p->ValueArray[0], p->ValueArray.size());
//	}
//	attr->EndRead();
//
//	attr = node->GetAttrib("SRV");
//	if (attr != nullptr)
//	{
//		attr->BeginRead(__FILE__, __LINE__);
//		UINT count;
//		attr->Read(count);
//		for (UINT i = 0; i < count; i++)
//		{
//			auto tmp = new SRV();
//			attr->ReadText(tmp->ShaderName);
//			attr->ReadStringAsRName(tmp->RName);
//			mSRViews.insert(std::make_pair(tmp->ShaderName, tmp));
//		}
//		attr->EndRead();
//	}
//	
//	return true;
//}

//void GfxShadingEnv::AddVar(const char* name, EShaderVarType type, UINT elements)
//{
//	auto i = mVars.find(name);
//	if (i == mVars.end())
//	{
//		GfxVarValue* tmp = new GfxVarValue();
//		tmp->Definition.Name = name;
//		tmp->Definition.Type = type;
//		tmp->Definition.Elements = elements;
//		auto size = GetShaderVarTypeSize(tmp->Definition.Type) * elements;
//		tmp->ValueArray.resize(size);
//		mVars.insert(std::make_pair(name, tmp));
//	}
//	else
//	{
//		if (i->second->Definition.Type == type && i->second->Definition.Elements == elements)
//			return;
//		i->second->Definition.Type = type;
//		i->second->Definition.Elements = elements;
//		auto size = GetShaderVarTypeSize(i->second->Definition.Type) * elements;
//		i->second->ValueArray.resize(size);
//	}
//	mVersion++;
//}

//void GfxShadingEnv::RemoveVar(const char* name)
//{
//	auto i = mVars.find(name);
//	if (i != mVars.end())
//	{
//		Safe_Delete(i->second);
//		mVars.erase(name);
//	}
//	mVersion++;
//}

//void GfxShadingEnv::AddSRV(const char* name)
//{
//	auto i = mSRViews.find(name);
//	if (i == mSRViews.end())
//	{
//		auto tmp = new SRV();
//		tmp->ShaderName = name;
//		mSRViews.insert(std::make_pair(name, tmp));
//	}
//}

//void GfxShadingEnv::RemoveSRV(const char* name)
//{
//	auto i = mSRViews.find(name);
//	if (i != mSRViews.end())
//	{
//		Safe_Delete(i->second);
//		mSRViews.erase(i);
//	}
//}

//void GfxShadingEnv::SetSRV(const char* name, IShaderResourceView* srv)
//{
//	auto i = mSRViews.find(name);
//	if (i != mSRViews.end())
//	{
//		if (srv)
//			srv->AddRef();
//		Safe_Release(i->second->RSView);
//		i->second->RSView = srv;
//		if (srv)
//		{
//			i->second->RName = srv->GetName();
//		}
//		else
//		{
//			i->second->RName = "";
//		}
//	}
//}

//IShaderResourceView* GfxShadingEnv::GetSRV(const char* name)
//{
//	auto i = mSRViews.find(name);
//	if (i != mSRViews.end())
//	{
//		return i->second->RSView;
//	}
//	return nullptr;
//}

NS_END

using namespace EngineNS;

extern "C"
{
	////CSharpReturnAPI2(vBOOL, EngineNS, GfxShadingEnv, Init, const char*, const char*);
	////CSharpReturnAPI0(UINT, EngineNS, GfxShadingEnv, GetVersion);
	////CSharpReturnAPI0(const char*, EngineNS, GfxShadingEnv, GetName);
	////CSharpReturnAPI0(const char*, EngineNS, GfxShadingEnv, GetShaderName);

	////CSharpAPI3(EngineNS, GfxShadingEnv, AddVar, const char*, EShaderVarType, UINT);
	////CSharpAPI1(EngineNS, GfxShadingEnv, RemoveVar, const char*);
	////CSharpAPI1(EngineNS, GfxShadingEnv, AddSRV, const char*);
	////CSharpAPI1(EngineNS, GfxShadingEnv, RemoveSRV, const char*);
	////CSharpAPI2(EngineNS, GfxShadingEnv, SetSRV, const char*, IShaderResourceView*);
	////CSharpReturnAPI1(IShaderResourceView*, EngineNS, GfxShadingEnv, GetSRV, const char*);

	////CSharpAPI1(EngineNS, GfxShadingEnv, Save2Xnd, XNDNode*);
	////CSharpReturnAPI1(vBOOL, EngineNS, GfxShadingEnv, LoadXnd, XNDNode* );
}