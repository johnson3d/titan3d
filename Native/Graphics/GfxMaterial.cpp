#include "GfxMaterial.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxMaterial, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::GfxMaterialInstance, EngineNS::VIUnknown);

GfxMaterial::GfxMaterial()
{
}


GfxMaterial::~GfxMaterial()
{
	Cleanup();
}

void GfxMaterial::Cleanup()
{
	VIUnknown::Cleanup();
}

vBOOL GfxMaterial::Init(const char* name)
{
	return TRUE;
}
//GfxMaterialInstance==============================================================//
GfxMaterialInstance::GfxMaterialInstance()
{

}

GfxMaterialInstance::~GfxMaterialInstance()
{
	//Cleanup();
}

//void GfxMaterialInstance::Cleanup()
//{
//	//for (auto i : mVars)
//	//{
//	//	Safe_Delete(i);
//	//}
//	//mVars.clear();
//
//	//for (auto i : mSRViews)
//	//{
//	//	Safe_Delete(i);
//	//}
//	//mSRViews.clear();
//
//	mSamplerStateDescArray.clear();
//
//	//if(mMaterial!=nullptr)
//	//	mMaterial->Cleanup();
//}

//vBOOL GfxMaterialInstance::Init(GfxMaterial* material, const char* name)
//{
//	Cleanup();
//
//	//mMaterial.StrongRef(material);
//
//	return SetMaterial(material, false);
//	/*for (auto i : material->mVars)
//	{
//		auto p = new GfxVarValue();
//		p->Definition = i.second;
//		ASSERT(FindVar(p->Definition.Name.c_str()) == nullptr);
//		p->ValueArray.resize(i.second.Elements * GetShaderVarTypeSize(i.second.Type));
//
//		mVars.push_back(p);
//	}
//
//	return true;*/
//}

//vBOOL GfxMaterialInstance::SetMaterial(GfxMaterial* material, bool doClear)
//{
//	mMaterial.StrongRef(material);
//	mVersion = material->GetVersion();
//	mMaterialName = mMaterial->GetName();
//
//	if (doClear)
//	{
//		for (auto i : mVars)
//		{
//			Safe_Delete(i);
//		}
//		mVars.clear();
//
//		for (auto i : mSRViews)
//		{
//			Safe_Delete(i);
//		}
//		mSRViews.clear();
//
//		mSamplerStateDescArray.clear();
//	}
//
//	for (auto i : material->mVars)
//	{
//		auto p = FindVar(i.first.c_str());
//		if (p == nullptr)
//		{
//			p = new GfxVarValue();
//			p->Definition = i.second;
//			mVars.push_back(p);
//			p->ValueArray.resize(i.second.Elements * GetShaderVarTypeSize(i.second.Type));
//		}
//		else
//		{
//			if (p->Definition.Name != i.second.Name ||
//				p->Definition.Type != i.second.Type ||
//				p->Definition.Elements != i.second.Elements)
//			{
//				p->ValueArray.resize(i.second.Elements * GetShaderVarTypeSize(i.second.Type));
//			}
//		}
//	}
//
//	for (auto i : material->mShaderResourceViews)
//	{
//		auto srv = FindSRV(i.first.c_str());
//		if (srv == nullptr)
//		{
//			auto tmp = new SRV();
//			tmp->ShaderName = i.first.c_str();
//			mSRViews.push_back(tmp);
//		}
//
//		ISamplerStateDesc desc;
//		material->GetSamplerStateDesc(i.first.c_str(), &desc);
//		mSamplerStateDescArray.push_back(desc);
//	}
//	return TRUE;
//}

//void GfxMaterialInstance::SetRasterizerState(IRasterizerState* rs)
//{
//	mRasterizerState.StrongRef(rs);
//}
//
//void GfxMaterialInstance::SetDepthStencilState(IDepthStencilState* dss)
//{
//	mDepthStencilState.StrongRef(dss);
//}
//
//void GfxMaterialInstance::SetBlendState(IBlendState* bs)
//{
//	mBlendState.StrongRef(bs);
//}

//IRasterizerState* GfxMaterialInstance::GetRasterizerState()
//{
//	return mRasterizerState;
//}
//
//IDepthStencilState* GfxMaterialInstance::GetDepthStencilState()
//{
//	return mDepthStencilState;
//}

//const char* GfxMaterialInstance::GetVarName(UINT index) const
//{
//	if (index >= (UINT)mVars.size())
//		return nullptr;
//	return mVars[index]->Definition.Name.c_str();
//}

//UINT GfxMaterialInstance::FindVarIndex(const char* name)
//{
//	for (UINT i = 0; i < (UINT)mVars.size(); i++)
//	{
//		auto cmp = strcmp(mVars[i]->Definition.Name.c_str(), name);
//		if (cmp == 0)
//			return i;
//	}
//	return -1;
//}

//GfxVarValue* GfxMaterialInstance::FindVar(const char* name)
//{
//	for (auto i = mVars.begin(); i != mVars.end(); i++)
//	{
//		auto cmp = strcmp((*i)->Definition.Name.c_str(), name);
//		if (cmp == 0)//这里莫名其妙std::string ==就不行了！
//		{
//			return *i;
//		}
//	}
//	return nullptr;
//}

//GfxMaterialInstance::SRV* GfxMaterialInstance::FindSRV(const char* name)
//{
//	for (auto i = mSRViews.begin(); i != mSRViews.end(); i++)
//	{
//		auto cmp = strcmp((*i)->ShaderName.c_str(), name);
//		if (cmp == 0)//这里莫名其妙std::string ==就不行了！
//		{
//			return *i;
//		}
//	}
//	return nullptr; 
//}

//UINT GfxMaterialInstance::FindSRVIndex(const char* name)
//{
//	for (UINT i = 0; i < (UINT)mSRViews.size(); i++)
//	{
//		auto cmp = strcmp(mSRViews[i]->ShaderName.c_str(), name);
//		if (cmp == 0)
//			return i;
//	}
//	return -1;
//}

//void GfxMaterialInstance::SetSRV(UINT index, IShaderResourceView* srv)
//{
//	if (srv)
//		srv->AddRef();
//
//	Safe_Release(mSRViews[index]->RSView);
//	mSRViews[index]->RSView = srv;
//	if (srv)
//		mSRViews[index]->RName = srv->GetName();
//	else
//		mSRViews[index]->RName = "";
//}
//vBOOL GfxMaterialInstance::AddSRV(const char* name)
//{
//	auto srv = FindSRV(name);
//	if(srv == nullptr)
//	{
//		auto tmp = new SRV();
//		tmp->ShaderName = name;
//		mSRViews.push_back(tmp);
//	}
//	return TRUE;
//}
//vBOOL GfxMaterialInstance::RemoveSRV(UINT index)
//{
//	if (index >= (UINT)mSRViews.size())
//		return FALSE;
//	auto i = mSRViews[index];
//	mSRViews.erase(mSRViews.begin() + index);
//	Safe_Delete(i);
//	return TRUE;
//}

//void GfxMaterialInstance::SetSamplerStateDesc(UINT index, const ISamplerStateDesc *desc)
//{
//	if (index >= mSamplerStateDescArray.size())
//		return;
//	mSamplerStateDescArray[index] = *desc;
//}
//void GfxMaterialInstance::GetSamplerStateDesc(UINT index, ISamplerStateDesc *desc)
//{
//	if (index >= mSamplerStateDescArray.size())
//		return;
//	*desc = mSamplerStateDescArray[index];
//}

//vBOOL GfxMaterialInstance::GetVarValue(UINT index, UINT elementIndex, GfxVar* definition, BYTE* pValue)
//{
//	if (index >= (UINT)mVars.size())
//		return FALSE;
//
//	auto i = mVars[index];
//
//	if (definition != nullptr)
//	{
//		definition->Type = i->Definition.Type;
//		definition->Elements = i->Definition.Elements;
//	}
//
//	if (elementIndex >= i->Definition.Elements)
//		return FALSE;
//
//	if (pValue != nullptr)
//	{
//		auto size = GetShaderVarTypeSize(i->Definition.Type);
//		auto p = &i->ValueArray[0] + size * elementIndex;
//		memcpy(pValue, p, size);
//	}
//
//	return TRUE;
//}

//vBOOL GfxMaterialInstance::SetVarValue(UINT index, UINT elementIndex, const BYTE* pValue)
//{
//	if (index >= (UINT)mVars.size())
//		return FALSE;
//
//	auto i = mVars[index];
//
//	if (elementIndex >= i->Definition.Elements)
//		return FALSE;
//
//	if (pValue != nullptr)
//	{
//		auto size = GetShaderVarTypeSize(i->Definition.Type);
//		auto p = &i->ValueArray[0] + size * elementIndex;
//		memcpy(p, pValue, size);
//	}
//
//	return TRUE;
//}
//vBOOL GfxMaterialInstance::AddVar(const char* name, UINT type, UINT elements)
//{
//	auto sVar = FindVar(name);
//	if(sVar == nullptr)
//	{
//		auto p = new GfxVarValue();
//		p->Definition.Name = name;
//		p->Definition.Type = (EShaderVarType)type;
//		p->Definition.Elements = elements;
//		p->ValueArray.resize(p->Definition.Elements * GetShaderVarTypeSize(p->Definition.Type));
//		mVars.push_back(p);
//	}
//	return TRUE;
//}
//vBOOL GfxMaterialInstance::RemoveVar(UINT index)
//{
//	if (index >= (UINT)mVars.size())
//		return FALSE;
//	
//	auto i = mVars[index];
//	mVars.erase(mVars.begin() + index);
//	Safe_Delete(i);
//	return TRUE;
//}

//void GfxMaterialInstance::Save2Xnd(XNDNode* node)
//{
//	if (mMaterial == nullptr)
//		return;
//
////	mVersion++;
//
//	auto attr = node->AddAttrib("Vars");
//	attr->BeginWrite();
////	attr->Write(&mVersion, sizeof(UINT));
//	auto count = (UINT)mVars.size();
//	attr->Write(&count, sizeof(UINT));
//	for (auto i : mVars)
//	{
//		attr->WriteText(i->Definition.Name.c_str());
//		auto type = (UINT)i->Definition.Type;
//		attr->Write(&type, sizeof(UINT));
//		attr->Write(&i->Definition.Elements, sizeof(UINT));
//		if (i->Definition.Elements == 0)
//			i->Definition.Elements = 1;
//		ASSERT(i->ValueArray.size() == i->Definition.Elements * GetShaderVarTypeSize(i->Definition.Type));
//		attr->Write(&i->ValueArray[0], i->ValueArray.size());
//	}
//	attr->EndWrite();
//
//	attr = node->AddAttrib("RSV");
//	attr->BeginWrite();
//	count = (UINT)mSRViews.size();
//	attr->Write(&count, sizeof(UINT));
//	for (auto i : mSRViews)
//	{
//		attr->WriteText(i->ShaderName.c_str());
//		attr->WriteStringAsRName(i->RName);
//	}
//	attr->EndWrite();
//
//	if (mRasterizerState != nullptr)
//	{
//		attr = node->AddAttrib("RasterizerState");
//		attr->BeginWrite();
//		attr->Write(mRasterizerState->mDesc);
//		attr->EndWrite();
//	}
//
//	if (mDepthStencilState != nullptr)
//	{
//		attr = node->AddAttrib("DepthStencilState");
//		attr->BeginWrite();
//		IDepthStencilStateDesc desc;
//		mDepthStencilState->GetDesc(&desc);
//		attr->Write(desc);
//		attr->EndWrite();
//	}
//
//	if (mBlendState != nullptr)
//	{
//		attr = node->AddAttrib("BlendState");
//		attr->BeginWrite();
//		IBlendStateDesc desc;
//		mBlendState->GetDesc(&desc);
//		attr->Write(desc);
//		attr->EndWrite();
//	}
//}
//
//vBOOL GfxMaterialInstance::LoadXnd(XNDNode* node)
//{
//	mName.Name = node->GetName().c_str();
//	auto attr = node->GetAttrib("Vars");
//	attr->BeginRead(__FILE__, __LINE__);
//	attr->Read(&mVersion, sizeof(UINT));
//	
//	attr->ReadStringAsRName(mMaterialName);
//	UINT count = 0;
//	attr->Read(&count, sizeof(UINT));
//	for (auto i : mVars)
//	{
//		Safe_Delete(i);
//	}
//	mVars.clear();
//	for (UINT i = 0; i<count; i++)
//	{
//		auto p = new GfxVarValue();
//		attr->ReadText(p->Definition.Name);
//		attr->Read(&p->Definition.Type, sizeof(UINT));
//		attr->Read(&p->Definition.Elements, sizeof(UINT));
//		p->ValueArray.resize(p->Definition.Elements * GetShaderVarTypeSize(p->Definition.Type));
//		attr->Read(&p->ValueArray[0], p->ValueArray.size());
//		ASSERT(nullptr == FindVar(p->Definition.Name.c_str()));
//		mVars.push_back(p);
//	}
//	attr->EndRead();
//
//	for (auto i : mSRViews)
//	{
//		Safe_Delete(i);
//	}
//	mSRViews.clear();
//	attr = node->GetAttrib("RSV");
//	if (attr != nullptr)
//	{
//		attr->BeginRead(__FILE__, __LINE__);
//		attr->Read(&count, sizeof(UINT));
//		for (UINT i = 0; i < count; i++)
//		{
//			auto tmp = new SRV();
//			attr->ReadText(tmp->ShaderName);
//			attr->ReadStringAsRName(tmp->RName);
//			ASSERT(nullptr == FindSRV(tmp->ShaderName.c_str()));
//			mSRViews.push_back(tmp);
//		}
//		attr->EndRead();
//	}
//
//	attr = node->GetAttrib("RasterizerState");
//	if (attr!=nullptr)
//	{
//		attr->BeginRead(__FILE__, __LINE__);
//		attr->Read(mRSDesc);
//		attr->EndRead();
//	}
//	attr = node->GetAttrib("DepthStencilState");
//	if (attr != nullptr)
//	{
//		attr->BeginRead(__FILE__, __LINE__);
//		attr->Read(mDSDesc);
//		attr->EndRead();
//	}
//	attr = node->GetAttrib("BlendState");
//	if (attr != nullptr)
//	{
//		attr->BeginRead(__FILE__, __LINE__);
//		attr->Read(mBLDDesc);
//		attr->EndRead();
//	}
//	return TRUE;
//}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI1(vBOOL, EngineNS, GfxMaterial, Init, const char*);

	////CSharpReturnAPI2(vBOOL, EngineNS, GfxMaterialInstance, Init, GfxMaterial*, const char*);
	////CSharpReturnAPI2(vBOOL, EngineNS, GfxMaterialInstance, SetMaterial, GfxMaterial*, bool);
	//CSharpAPI1(EngineNS, GfxMaterialInstance, GetRSDesc, IRasterizerStateDesc*);
	//CSharpAPI1(EngineNS, GfxMaterialInstance, GetDSDesc, IDepthStencilStateDesc*);
	//CSharpAPI1(EngineNS, GfxMaterialInstance, GetBLDDesc, IBlendStateDesc*);
	//////CSharpAPI1(EngineNS, GfxMaterialInstance, SetRasterizerState, IRasterizerState*);
	//////CSharpAPI1(EngineNS, GfxMaterialInstance, SetDepthStencilState, IDepthStencilState*);
	//////CSharpAPI1(EngineNS, GfxMaterialInstance, SetBlendState, IBlendState*);
	//CSharpReturnAPI0(IRasterizerState*, EngineNS, GfxMaterialInstance, GetRasterizerState);
	//CSharpReturnAPI0(IDepthStencilState*, EngineNS, GfxMaterialInstance, GetDepthStencilState);
	////CSharpReturnAPI0(UINT, EngineNS, GfxMaterialInstance, GetVarNumber);
	//CSharpReturnAPI1(const char*, EngineNS, GfxMaterialInstance, GetVarName, UINT);
	//CSharpReturnAPI1(UINT, EngineNS, GfxMaterialInstance, FindVarIndex, const char*);
	////CSharpReturnAPI4(vBOOL, EngineNS, GfxMaterialInstance, GetVarValue, UINT, UINT, GfxVar*, BYTE*);
	////CSharpReturnAPI3(vBOOL, EngineNS, GfxMaterialInstance, SetVarValue, UINT, UINT, const BYTE*);
	////CSharpReturnAPI1(UINT, EngineNS, GfxMaterialInstance, FindSRVIndex, const char*);
	////CSharpReturnAPI0(UINT, EngineNS, GfxMaterialInstance, GetSRVNumber);
	////CSharpAPI2(EngineNS, GfxMaterialInstance, SetSRV, UINT, IShaderResourceView*);
	////CSharpReturnAPI1(IShaderResourceView*, EngineNS, GfxMaterialInstance, GetSRV, UINT);
	//CSharpReturnAPI1(const char*, EngineNS, GfxMaterialInstance, GetSRVShaderName, UINT);
	////CSharpReturnAPI1(const char*, EngineNS, GfxMaterialInstance, GetSRVName, UINT);
	////CSharpAPI2(EngineNS, GfxMaterialInstance, SetSamplerStateDesc, UINT, ISamplerStateDesc*);
	////CSharpAPI2(EngineNS, GfxMaterialInstance, GetSamplerStateDesc, UINT, ISamplerStateDesc*);

	////CSharpReturnAPI3(vBOOL, EngineNS, GfxMaterialInstance, AddVar, const char*, UINT, UINT);
	////CSharpReturnAPI1(vBOOL, EngineNS, GfxMaterialInstance, RemoveVar, UINT);
	////CSharpReturnAPI1(vBOOL, EngineNS, GfxMaterialInstance, AddSRV, const char*);
	////CSharpReturnAPI1(vBOOL, EngineNS, GfxMaterialInstance, RemoveSRV, UINT);

	////CSharpAPI1(EngineNS, GfxMaterialInstance, Save2Xnd, XNDNode*);
	////CSharpReturnAPI1(vBOOL, EngineNS, GfxMaterialInstance, LoadXnd, XNDNode*);
}