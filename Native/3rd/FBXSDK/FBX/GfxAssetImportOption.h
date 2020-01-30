#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include <fbxsdk.h>
NS_BEGIN
enum ImportAssetType
{
	IAT_Unknown,
	IAT_Null,
	IAT_Marker,
	IAT_Skeleton,
	IAT_Mesh,
	IAT_Nurbs,
	IAT_Patch,
	IAT_Camera,
	IAT_CameraStereo,
	IAT_CameraSwitcher,
	IAT_Light,
	IAT_OpticalReference,
	IAT_OpticalMarker,
	IAT_NurbsCurve,
	IAT_TrimNurbsSurface,
	IAT_Boundary,
	IAT_NurbsSurface,
	IAT_Shape,
	IAT_LODGroup,
	IAT_SubDiv,
	IAT_CachedEffect,
	IAT_Line,
	IAT_Animation,
	IAT_Default,
};
class GfxAssetImportOption : public VIUnknown
{
public:
	RTTI_DEF(GfxAssetImportOption, 0xdb495a985d106c73, true);
	GfxAssetImportOption();
	~GfxAssetImportOption();
public:
	void SetName(const char* name) { Name = std::string(name); }
	const char* GetName() { return Name.c_str(); }
	void SetAbsSavePath(const char* absSavePath) { AbsSavePath = std::string(absSavePath); }
	const char* GetAbsSavePath() { return AbsSavePath.c_str(); }
	void SetHash(UINT hash) { Hash = hash; }
	UINT GetHash() { return Hash; }
	void SetScale(float scale) { Scale = scale; }
	float GetScale() { return Scale; }
	void SetIsImport(vBOOL isImport) { IsImport = isImport; }
	vBOOL GetIsImport() { return IsImport; }
	void SetAssetType(ImportAssetType type) { Type = type; }
	ImportAssetType GetAssetType() { return Type; }
public:
	std::string Name;
	std::string AbsSavePath;
	UINT Hash;
	vBOOL IsImport;
	ImportAssetType Type;
	FbxNode* FBXNode;
	float Scale;
};

class GfxMeshImportOption :public GfxAssetImportOption
{
public:
	RTTI_DEF(GfxMeshImportOption, 0xde1926ae5cf77d22, true);
	GfxMeshImportOption();
	~GfxMeshImportOption();
public:
	vBOOL GetReCalculateTangent() { return ReCalculateTangent; }
	void SetReCalculateTangent(vBOOL reCalculateTangent) { ReCalculateTangent = reCalculateTangent; }
	vBOOL GetAsCollision() { return AsCollision; }
	void SetAsCollision(vBOOL asCollision) { AsCollision = asCollision; }
	vBOOL GetAsLocalSpace() { return AsLocalSpace; }
	void SetAsLocalSpace(vBOOL asLocalSpace) { AsLocalSpace = asLocalSpace; }
	vBOOL GetHaveSkin() { return HaveSkin; }
	void SetHaveSkin(vBOOL haveSkin) { HaveSkin = haveSkin; }
	vBOOL GetAsStaticMesh() { return AsStaticMesh; }
	void SetAsStaticMesh(vBOOL asStaticMesh) { AsStaticMesh = asStaticMesh; }
	void SetRenderAtom(UINT renderAtom) { RenderAtom = renderAtom; }
	UINT GetRenderAtom() { return RenderAtom; }
	vBOOL GetTransformVertexToAbsolute() { return TransformVertexToAbsolute; }
	void SetTransformVertexToAbsolute(vBOOL transformVertexToAbsolute) { TransformVertexToAbsolute = transformVertexToAbsolute; }
public:
	vBOOL ReCalculateTangent;
	vBOOL AsCollision;
	vBOOL AsLocalSpace;
	vBOOL HaveSkin;
	vBOOL AsStaticMesh;
	UINT RenderAtom;
	vBOOL TransformVertexToAbsolute;
	vBOOL BakePivotInVertex;
};
class GfxAnimationImportOption :public GfxAssetImportOption
{
public:
	RTTI_DEF(GfxAnimationImportOption, 0x244a470e5cf77d2b, true);
	GfxAnimationImportOption();
	~GfxAnimationImportOption();
	void SetAnimationType(ImportAssetType type) { AnimationType = type; }
	ImportAssetType GetAnimationType() { return AnimationType; }
public:
	FbxAnimStack* AnimStack;
	FbxAnimLayer* AnimLayer;
	ImportAssetType AnimationType;
public:
	void SetDuration(float duration) { Duration = duration; }
	float GetDuration() { return Duration; }
	void SetSampleRate(float sampleRate) { SampleRate = sampleRate; }
	float GetSampleRate() { return SampleRate; }
public:
	float Duration;
	float SampleRate;
};
class GfxSkeletonAnimationImportOption :public GfxAnimationImportOption
{
public:
	RTTI_DEF(GfxSkeletonAnimationImportOption, 0x38c7b3065d1c5dd1, true);
	GfxSkeletonAnimationImportOption();
	~GfxSkeletonAnimationImportOption();
public:

};

NS_END
