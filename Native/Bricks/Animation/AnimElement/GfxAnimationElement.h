#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../AnimCurve/GfxICurve.h"
NS_BEGIN
enum AnimationElementType
{
	AET_Default,
	AET_Bone,
	AET_Skeleton,
};
class GfxAnimationElementDesc :public VIUnknown
{
	RTTI_DEF(GfxAnimationElementDesc, 0x225f83605d1dc426, true);
public:
	std::string			Name;
	UINT				NameHash;
	std::string			Path; //GActor:actor1/bool:IsVisable
	std::string			Parent;
	UINT				ParentHash;
	std::string			GrantParent;
	UINT				GrantParentHash;
	void SetPath(const char* name) { Path = name;}
	const char* GetPath() { return Path.c_str(); }
	void SetName(const char* name)
	{
		Name = name;
		NameHash = HashHelper::APHash(name);
	}
	const char* GetName()
	{
		return Name.c_str();
	}
	void SetParent(const char* name)
	{
		Parent = name;
		if (Parent == "")
		{
			ParentHash = -1;
			return;
		}
		ParentHash = HashHelper::APHash(name);
	}
	const char* GetParent()
	{
		return Parent.c_str();
	}
	UINT GetNameHash() { return NameHash; }
	UINT GetParentHash() { return ParentHash; };
	UINT GetGrantParentHash() { return GrantParentHash; }
	void SetGrantParent(const char* name)
	{
		GrantParent = name;
		if (GrantParent == "")
		{
			GrantParentHash = -1;
			return;
		}
		GrantParentHash = HashHelper::APHash(name);
	}
	const char* GetGrantParent()
	{
		return GrantParent.c_str();
	}
};
class GfxAnimationElement :public VIUnknown
{
	RTTI_DEF(GfxAnimationElement, 0x5801af0d5cf0b2a6, true);
public:
	GfxAnimationElement() {}
	GfxAnimationElement(AnimationElementType type);
	~GfxAnimationElement();
public:
	void SetCurve(GfxICurve* curve) { mCurve = curve; }
	GfxICurve* GetCurve() { return mCurve; }
	void SetAnimationElementType(AnimationElementType type) { mAnimationElementType = type; }
	AnimationElementType GetAnimationElementType() { return mAnimationElementType; }
	void SetAnimationElementDesc(GfxAnimationElementDesc* desc) { mAnimationElementDesc = desc; }
	GfxAnimationElementDesc* GetAnimationElementDesc() { return mAnimationElementDesc; }

protected:
	AnimationElementType		mAnimationElementType;
	GfxAnimationElementDesc*	mAnimationElementDesc;
	GfxICurve*			mCurve;
};

NS_END