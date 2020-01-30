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
	//int								BoneNameHashID;		// 骨头名字的HASH ID，用这个来代替m_BoneName做判断
	//UINT							IndexInFullSkeleton;
	//std::vector<UINT>				ChildBones;

	v3dxMatrix4						InitMatrix;
	v3dxMatrix4						InvInitMatrix;

	// 渲染骨头的数据
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
	CSharpReturnAPI0(const char*, EngineNS, GfxBoneDesc, GetName);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetName, const char*);
	CSharpReturnAPI0(UINT, EngineNS, GfxBoneDesc, GetNameHash);
	CSharpReturnAPI0(const char*, EngineNS, GfxBoneDesc, GetParent);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetParent, const char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxBoneDesc, GetGrantParent);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetGrantParent, const char*);
	CSharpReturnAPI0(UINT, EngineNS, GfxBoneDesc, GetParentHash);
	CSharpReturnAPI0(float, EngineNS, GfxBoneDesc, GetGrantWeight);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetGrantWeight, float);
	CSharpAPI1(EngineNS, GfxBoneDesc, GetBindMatrix, v3dxMatrix4*);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetBindMatrix, v3dxMatrix4*);
	CSharpAPI1(EngineNS, GfxBoneDesc, GetBindInvInitMatrix, v3dxMatrix4*);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetBindInvInitMatrix, v3dxMatrix4*);
	CSharpAPI1(EngineNS, GfxBoneDesc, GetInvPos, v3dxVector3*);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetInvPos, v3dxVector3*);
	CSharpAPI1(EngineNS, GfxBoneDesc, GetInvScale, v3dxVector3*);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetInvScale, v3dxVector3*);
	CSharpAPI1(EngineNS, GfxBoneDesc, GetInvQuat, v3dxQuaternion*);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetInvQuat, v3dxQuaternion*);
	CSharpReturnAPI0(BoneType, EngineNS, GfxBoneDesc, GetBoneType);
	CSharpAPI1(EngineNS, GfxBoneDesc, SetBoneType, BoneType);

	CSharpReturnAPI0(GfxBoneDesc*, EngineNS, GfxBone, GetBoneDesc);
	CSharpAPI1(EngineNS, GfxBone, SetBoneDesc, GfxBoneDesc*);

	CSharpReturnAPI0(USHORT, EngineNS, GfxBone, GetIndexInTable);
	CSharpAPI1(EngineNS, GfxBone, SetIndexInTable, USHORT);
	CSharpReturnAPI0(UINT, EngineNS, GfxBone, GetChildNumber);
	CSharpReturnAPI1(USHORT, EngineNS, GfxBone, GetChild, UINT);
	CSharpAPI1(EngineNS, GfxBone, AddChild, UINT);
	CSharpAPI0(EngineNS, GfxBone, ClearChildren);
}

