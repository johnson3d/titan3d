#pragma once
#include "../../../Graphics/Mesh/GfxModifier.h"
#include "../AnimationDataTypes.h"
NS_BEGIN
enum BoneType
{
	Bone,
	Socket,
};
class GfxBoneDesc : public VIUnknown
{
public:
	RTTI_DEF(GfxBoneDesc, 0x9318fb745d006795, true);
	GfxBoneDesc()
	{
		NameHash = 0;
		ParentHash = 0;
		GrantParentHash = 0;
		GrantWeight = 0.0f;
		Type = Bone;
	}
	std::string			Name;
	UINT				NameHash;
	std::string			Parent;
	UINT				ParentHash;
	std::string			GrantParent;
	UINT				GrantParentHash;
	float				GrantWeight;
	MotionConstraint	MotionConstraint;
	BoneType			Type;


	v3dxMatrix4			InitMatrix;
	v3dxMatrix4			InvInitMatrix;
	v3dxVector3			InvPos;
	v3dxVector3			InvScale;
	v3dxQuaternion		InvQuat;

	const char* GetName() const {
		return Name.c_str();
	}
	void SetName(const char* name);
	UINT GetNameHash() const{
		return NameHash;
	}
	const char* GetParent() const {
		return Parent.c_str();
	}
	void SetParent(const char* name);
	UINT GetParentHash() const {
		return ParentHash;
	}
	const char* GetGrantParent() const {
		return GrantParent.c_str();
	}
	void SetGrantParent(const char* name);
	UINT GetGrantParentHash() const {
		return GrantParentHash;
	}
	float GetGrantWeight()const {
		return GrantWeight;
	}
	void SetGrantWeight(float weight) {
		GrantWeight = weight;
	}
	BoneType GetBoneType()const { return Type; }
	void SetBoneType(BoneType type) { Type = type; }
	void GetBindMatrix(v3dxMatrix4* mat) const {
		*mat = InitMatrix;
	}
	void SetBindMatrix(v3dxMatrix4* mat) {
		InitMatrix = *mat;
		auto invMat = InitMatrix.inverse();
		SetBindInvInitMatrix(&invMat);
	}
	void GetBindInvInitMatrix(v3dxMatrix4* mat) const {
		*mat = InvInitMatrix;
	}
	void SetBindInvInitMatrix(v3dxMatrix4* mat) {
		InvInitMatrix = *mat;
		InvInitMatrix.Decompose(InvScale, InvPos, InvQuat);
	}
	void GetInvPos(v3dxVector3* pos)
	{
		*pos = InvPos;
	}
	void SetInvPos(v3dxVector3* pos)
	{
		InvPos = *pos;
	}
	void GetInvScale(v3dxVector3* scale)
	{
		*scale = InvScale;
	}
	void SetInvScale(v3dxVector3* scale)
	{
		InvScale = *scale;
	}
	void GetInvQuat(v3dxQuaternion* quat)
	{
		*quat = InvQuat;
	}
	void SetInvQuat(v3dxQuaternion* quat)
	{
		InvQuat = *quat;
	}
	inline static int Name2Hash(const char* name) {
		return HashHelper::APHash(name);
	}
};

class GfxBoneAnim;
class GfxBoneTable;
class GfxAnimationPose;

class GfxBone : public VIUnknown
{
public:
	RTTI_DEF(GfxBone, 0xbd316f935cff1a92, true);
	GfxBone();
	GfxBone(GfxBoneDesc* desc);
	~GfxBone();

	AutoRef<GfxBoneDesc>		mSharedData;
	AutoRef<GfxBoneAnim>		BoneAnim;
	
	USHORT						Parent;
	UINT						IndexInTable;
	std::vector<USHORT>			Children;
	std::vector<USHORT>			GrantChildren;
	
	USHORT GetIndexInTable() const {
		return (USHORT)IndexInTable;
	}
	void SetIndexInTable(USHORT index) {
		IndexInTable = index; 
	}
	UINT GetChildNumber() const {
		return (UINT)Children.size();
	}
	USHORT GetChild(UINT i) const{
		if (i > (UINT)Children.size())
			return -1;
		return Children[i];
	}
	void AddChild(USHORT childIndex){
		Children.push_back(childIndex);
	 }
	void ClearChildren()
	{
		Children.clear();
	}
	//void LinkBone(GfxBoneTable* table);
	void LinkBone(GfxAnimationPose* table);
	void CalculateCharacterSpaceTransfromRecursively(GfxAnimationPose* table);

	GfxBoneDesc* GetBoneDesc() {
		return mSharedData;
	}
	void SetBoneDesc(GfxBoneDesc* desc) {
		mSharedData.StrongRef(desc);
	}
	
};

NS_END