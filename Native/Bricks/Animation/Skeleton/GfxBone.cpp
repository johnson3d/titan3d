#include "GfxBone.h"
#include "../GfxBoneAnim.h"

#define new VNEW

NS_BEGIN
RTTI_IMPL(EngineNS::GfxBoneDesc, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::GfxBone, EngineNS::VIUnknown);

struct GfxBoneSharedData : VIUnknown {
	GfxBoneSharedData()
	{

	}
	~GfxBoneSharedData()
	{

	}
	bool LoadXnd(XNDAttrib* attr);

	std::string						Name;
	//UINT							Index;
	UINT							ParentIndex;
	//int								BoneNameHashID;
	//UINT							IndexInFullSkeleton;
	//std::vector<UINT>				ChildBones;

	v3dxMatrix4						InitMatrix;
	v3dxMatrix4						InvInitMatrix;

	// ��Ⱦ��ͷ������
	std::vector<v3dxVector3>		Verts;
	std::vector<SHORT>				Indices;
	v3dxVector3						BoxDimension;
};

bool GfxBoneSharedData::LoadXnd(XNDAttrib* attr)
{
	attr->ReadText(Name);
	UINT							Index;
	attr->Read(Index);
	attr->Read(ParentIndex);
	int								BoneNameHashID;
	attr->Read(BoneNameHashID);
	UINT							IndexInFullSkeleton;
	attr->Read(IndexInFullSkeleton);
	attr->Read(InitMatrix);
	attr->Read(InvInitMatrix);
	attr->Read(BoxDimension);

	UINT n;
	attr->Read(&n, sizeof(n));
	Verts.resize(n);
	if (n > 0)
		attr->Read(&Verts[0], n * sizeof(v3dxVector3));
	attr->Read(&n, sizeof(n));
	Indices.resize(n);
	if (n > 0)
		attr->Read(&Indices[0], n * sizeof(SHORT));

	INT nBone;
	attr->Read(nBone);

	std::vector<UINT>				ChildBones;
	ChildBones.resize(nBone);
	if (nBone > 0)
		attr->Read(&ChildBones[0], sizeof(UINT)*nBone);

	vBOOL bHasPhy;
	attr->Read(&bHasPhy, sizeof(vBOOL));
	return true;
}

void GfxBoneDesc::SetName(const char* name)
{
	Name = name;
	NameHash = Name2Hash(name);
}

void GfxBoneDesc::SetParent(const char* name)
{
	Parent = name;
	if (Parent == "")
	{
		ParentHash = -1;
		return;
	}
	ParentHash = Name2Hash(name);
}

void EngineNS::GfxBoneDesc::SetGrantParent(const char* name)
{
	GrantParent = name;
	if (GrantParent == "")
	{
		GrantParentHash = -1;
		return;
	}
	GrantParentHash = Name2Hash(name);
}

GfxBone::GfxBone()
{
	IndexInTable = -1;
}
GfxBone::GfxBone(GfxBoneDesc* desc)
{
	mSharedData.StrongRef(desc);
	IndexInTable = -1;
}

GfxBone::~GfxBone()
{

}

NS_END

using namespace EngineNS;


extern "C"
{
	Cpp2CS0(EngineNS, GfxBoneDesc, GetName);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetName);
	Cpp2CS0(EngineNS, GfxBoneDesc, GetNameHash);
	Cpp2CS0(EngineNS, GfxBoneDesc, GetParent);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetParent);
	Cpp2CS0(EngineNS, GfxBoneDesc, GetGrantParent);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetGrantParent);
	Cpp2CS0(EngineNS, GfxBoneDesc, GetParentHash);
	Cpp2CS0(EngineNS, GfxBoneDesc, GetGrantWeight);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetGrantWeight);
	Cpp2CS1(EngineNS, GfxBoneDesc, GetBindMatrix);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetBindMatrix);
	Cpp2CS1(EngineNS, GfxBoneDesc, GetBindInvInitMatrix);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetBindInvInitMatrix);
	Cpp2CS1(EngineNS, GfxBoneDesc, GetInvPos);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetInvPos);
	Cpp2CS1(EngineNS, GfxBoneDesc, GetInvScale);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetInvScale);
	Cpp2CS1(EngineNS, GfxBoneDesc, GetInvQuat);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetInvQuat);
	Cpp2CS0(EngineNS, GfxBoneDesc, GetBoneType);
	Cpp2CS1(EngineNS, GfxBoneDesc, SetBoneType);

	Cpp2CS0(EngineNS, GfxBone, GetBoneDesc);
	Cpp2CS1(EngineNS, GfxBone, SetBoneDesc);

	Cpp2CS0(EngineNS, GfxBone, GetIndexInTable);
	Cpp2CS1(EngineNS, GfxBone, SetIndexInTable);
	Cpp2CS0(EngineNS, GfxBone, GetChildNumber);
	Cpp2CS1(EngineNS, GfxBone, GetChild);
	Cpp2CS1(EngineNS, GfxBone, AddChild);
	Cpp2CS0(EngineNS, GfxBone, ClearChildren);
}

