#include "GfxMesh.h"
#include "GfxMeshPrimitives.h"
#include "GfxMdfQueue.h"
#include "GfxModifier.h"
#include "GfxMaterialPrimitive.h"
#include "../GfxMaterial.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxMesh, EngineNS::VIUnknown);

GfxMesh::GfxMesh()
{
}

GfxMesh::~GfxMesh()
{
	/*for (auto i : mMaterials)
	{
		Safe_Release(i);
	}
	mMaterials.clear();*/
}

vBOOL GfxMesh::Init(const char* name, GfxMeshPrimitives* geom, GfxMdfQueue* mdf)
{
	//mName = name;

	mGeoms.StrongRef(geom);
	//mGeomName = mGeoms->GetName();

	mMdfQueue.StrongRef(mdf);

	//mMaterialNames.clear();
	//mMaterialNames.resize(mGeoms->GetAtomNumber());
	/*for (auto i : mMaterials)
	{
		Safe_Release(i);
	}
	mMaterials.clear();
	mMaterials.resize(mGeoms->GetAtomNumber());*/
	return TRUE;
}

void GfxMesh::SetMeshPrimitives(GfxMeshPrimitives* geom)
{
	mGeoms.StrongRef(geom);
	//mGeomName = mGeoms->GetName();
}

void GfxMesh::SetGfxMdfQueue(GfxMdfQueue* mdf)
{
	mMdfQueue.StrongRef(mdf);
}

//vBOOL GfxMesh::SetMaterial(UINT index, GfxMaterialPrimitive* material)
//{
//	if (mMaterialNames.size() != mGeoms->GetAtomNumber())
//	{
//		mMaterialNames.resize(mGeoms->GetAtomNumber());
//	}
//	if (index >= mMaterialNames.size())
//	{
//		return FALSE;
//	}
//	/*if (material != nullptr)
//	{
//		material->AddRef();
//	}
//	Safe_Release(mMaterials[index]);
//	mMaterials[index] = material;*/
//	if (material != nullptr && material->GetMaterial() != nullptr)
//	{
//		mMaterialNames[index] = material->GetMaterial()->GetName();
//	}
//	else
//	{
//		mMaterialNames[index] = "";
//	}
//	return TRUE;
//}

//void GfxMesh::Save2Xnd(XNDNode* node)
//{
//	auto attr = node->AddAttrib("Desc");
//	attr->BeginWrite();
//	attr->WriteStringAsRName(mName);
//	attr->WriteStringAsRName(mGeomName);
//
//	UINT count = (UINT)mMaterialNames.size();
//	attr->Write(count);
//	for (UINT i = 0; i < count; i++)
//	{
//		/*if (mMaterials[i] == nullptr)
//		{
//			attr->WriteStringAsRName(RName());
//			continue;
//		}
//		auto mtlName = mMaterials[i]->GetMaterial()->GetRName();
//		attr->WriteStringAsRName(mtlName);*/
//		attr->WriteStringAsRName(mMaterialNames[i]);
//	}
//	attr->EndWrite();
//	/*count = mMdfQueue->GetMdfNumber();
//	if (count > 0)
//	{
//		auto mdfQueue = node->AddNode("MdfQueue", 0, 0);
//		for (UINT i = 0; i < count; i++)
//		{
//			auto modifier = mMdfQueue->GetModifier(i);
//			auto mdfNode = mdfQueue->AddNode(modifier->GetName(), 0, 0);
//			modifier->Save2Xnd(mdfNode);
//		}
//	}*/
//}

//vBOOL GfxMesh::LoadXnd(XNDNode* node)
//{
//	auto attr = node->GetAttrib("Desc");
//	if (attr == nullptr)
//		return FALSE;
//
//	attr->BeginRead(__FILE__, __LINE__);
//	attr->ReadStringAsRName(mName);
//	attr->ReadStringAsRName(mGeomName);
//
//	UINT count = 0;
//	attr->Read(count);
//	mMaterialNames.resize(count);
//	for (UINT i = 0; i < count; i++)
//	{
//		attr->ReadStringAsRName(mMaterialNames[i]);
//	}
//	attr->EndRead();
//
//	return TRUE;
//}

//const char* GfxMesh::GetName() const
//{
//	return mName.Name.c_str();
//}
//
//void EngineNS::GfxMesh::SetName(const char* name)
//{
//	mName = name;
//}

//const char* GfxMesh::GetGeomName() const
//{
//	return mGeomName.Name.c_str();
//}

UINT GfxMesh::GetAtomNumber() const
{
	return mGeoms->GetAtomNumber();
}

//const char* GfxMesh::GetMaterailName(UINT index) const
//{
//	if (index > (UINT)mMaterialNames.size())
//		return "";
//	return mMaterialNames[index].Name.c_str();
//}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI3(vBOOL, EngineNS, GfxMesh, Init, const char*, GfxMeshPrimitives*, GfxMdfQueue*);
	////CSharpReturnAPI2(vBOOL, EngineNS, GfxMesh, SetMaterial, UINT, GfxMaterialPrimitive*);
	CSharpAPI1(EngineNS, GfxMesh, SetMeshPrimitives, GfxMeshPrimitives*);
	CSharpAPI1(EngineNS, GfxMesh, SetGfxMdfQueue, GfxMdfQueue*);

	////CSharpAPI1(EngineNS, GfxMesh, Save2Xnd, XNDNode*);
	////CSharpReturnAPI1(vBOOL, EngineNS, GfxMesh, LoadXnd, XNDNode*);

	////CSharpAPI1(EngineNS, GfxMesh, SetName, const char*);
	////CSharpReturnAPI0(const char*, EngineNS, GfxMesh, GetName);
	////CSharpReturnAPI0(const char*, EngineNS, GfxMesh, GetGeomName);
	CSharpReturnAPI0(UINT, EngineNS, GfxMesh, GetAtomNumber);
	////CSharpReturnAPI1(const char*, EngineNS, GfxMesh, GetMaterailName, UINT);
}