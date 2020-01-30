#include "GfxAssetImportOption.h"
#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAssetImportOption, EngineNS::VIUnknown);
GfxAssetImportOption::GfxAssetImportOption()
{
	Name = "";
	Hash = 0;
	IsImport = TRUE;
	Type = IAT_Default;
	Scale = 1.0f;
}

GfxAssetImportOption::~GfxAssetImportOption()
{
}



RTTI_IMPL(EngineNS::GfxMeshImportOption, EngineNS::GfxAssetImportOption);
GfxMeshImportOption::GfxMeshImportOption()
{
	Type = IAT_Mesh;
	AsCollision = FALSE;
	AsLocalSpace = FALSE;
	HaveSkin = FALSE;
	AsStaticMesh = FALSE;
	Scale = 1.0f;
	RenderAtom = 0;
	TransformVertexToAbsolute = FALSE;
	BakePivotInVertex = TRUE;
	ReCalculateTangent = TRUE;
}
GfxMeshImportOption::~GfxMeshImportOption()
{

}



RTTI_IMPL(EngineNS::GfxAnimationImportOption, EngineNS::GfxAssetImportOption);
GfxAnimationImportOption::GfxAnimationImportOption()
{

} 

GfxAnimationImportOption::~GfxAnimationImportOption()
{

}
RTTI_IMPL(EngineNS::GfxSkeletonAnimationImportOption, EngineNS::GfxAnimationImportOption);
GfxSkeletonAnimationImportOption::GfxSkeletonAnimationImportOption()
{

}

GfxSkeletonAnimationImportOption::~GfxSkeletonAnimationImportOption()
{

}



NS_END
using namespace EngineNS;


extern "C"
{
	///////////////////////////////////////////////////////////////////
	//GfxObjectImportOption

	CSharpAPI1(EngineNS, GfxAssetImportOption, SetName, const char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxAssetImportOption, GetName);
	CSharpAPI1(EngineNS, GfxAssetImportOption, SetAbsSavePath, const char*);
	CSharpReturnAPI0(const char*, EngineNS, GfxAssetImportOption, GetAbsSavePath);
	CSharpAPI1(EngineNS, GfxAssetImportOption, SetHash, UINT);
	CSharpReturnAPI0(UINT, EngineNS, GfxAssetImportOption, GetHash);
	CSharpAPI1(EngineNS, GfxAssetImportOption, SetScale, float);
	CSharpReturnAPI0(float, EngineNS, GfxAssetImportOption, GetScale);
	CSharpAPI1(EngineNS, GfxAssetImportOption, SetIsImport, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxAssetImportOption, GetIsImport);
	CSharpAPI1(EngineNS, GfxAssetImportOption, SetAssetType, ImportAssetType);
	CSharpReturnAPI0(ImportAssetType, EngineNS, GfxAssetImportOption, GetAssetType);


	///////////////////////////////////////////////////////////////////
	//GfxMeshImportOption
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetReCalculateTangent, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxMeshImportOption, GetReCalculateTangent);
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetAsCollision, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxMeshImportOption, GetAsCollision);
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetAsLocalSpace, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxMeshImportOption, GetAsLocalSpace);
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetHaveSkin, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxMeshImportOption, GetHaveSkin);
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetAsStaticMesh, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxMeshImportOption, GetAsStaticMesh);
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetRenderAtom, UINT);
	CSharpReturnAPI0(UINT, EngineNS, GfxMeshImportOption, GetRenderAtom);
	CSharpAPI1(EngineNS, GfxMeshImportOption, SetTransformVertexToAbsolute, vBOOL);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxMeshImportOption, GetTransformVertexToAbsolute);

	///////////////////////////////////////////////////////////////////
	//GfxAnimationImportOption
	CSharpAPI1(EngineNS, GfxAnimationImportOption, SetAnimationType, ImportAssetType);
	CSharpReturnAPI0(ImportAssetType, EngineNS, GfxAnimationImportOption, GetAnimationType);
	CSharpAPI1(EngineNS, GfxAnimationImportOption, SetDuration, float);
	CSharpReturnAPI0(float, EngineNS, GfxAnimationImportOption, GetDuration);
	CSharpAPI1(EngineNS, GfxAnimationImportOption, SetSampleRate, float);
	CSharpReturnAPI0(float, EngineNS, GfxAnimationImportOption, GetSampleRate);

}