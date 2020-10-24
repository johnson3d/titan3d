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

	Cpp2CS1(EngineNS, GfxAssetImportOption, SetName);
	Cpp2CS0(EngineNS, GfxAssetImportOption, GetName);
	Cpp2CS1(EngineNS, GfxAssetImportOption, SetAbsSavePath);
	Cpp2CS0(EngineNS, GfxAssetImportOption, GetAbsSavePath);
	Cpp2CS1(EngineNS, GfxAssetImportOption, SetHash);
	Cpp2CS0(EngineNS, GfxAssetImportOption, GetHash);
	Cpp2CS1(EngineNS, GfxAssetImportOption, SetScale);
	Cpp2CS0(EngineNS, GfxAssetImportOption, GetScale);
	Cpp2CS1(EngineNS, GfxAssetImportOption, SetIsImport);
	Cpp2CS0(EngineNS, GfxAssetImportOption, GetIsImport);
	Cpp2CS1(EngineNS, GfxAssetImportOption, SetAssetType);
	Cpp2CS0(EngineNS, GfxAssetImportOption, GetAssetType);


	///////////////////////////////////////////////////////////////////
	//GfxMeshImportOption
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetReCalculateTangent);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetReCalculateTangent);
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetAsCollision);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetAsCollision);
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetAsLocalSpace);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetAsLocalSpace);
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetHaveSkin);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetHaveSkin);
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetAsStaticMesh);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetAsStaticMesh);
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetRenderAtom);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetRenderAtom);
	Cpp2CS1(EngineNS, GfxMeshImportOption, SetTransformVertexToAbsolute);
	Cpp2CS0(EngineNS, GfxMeshImportOption, GetTransformVertexToAbsolute);

	///////////////////////////////////////////////////////////////////
	//GfxAnimationImportOption
	Cpp2CS1(EngineNS, GfxAnimationImportOption, SetAnimationType);
	Cpp2CS0(EngineNS, GfxAnimationImportOption, GetAnimationType);
	Cpp2CS1(EngineNS, GfxAnimationImportOption, SetDuration);
	Cpp2CS0(EngineNS, GfxAnimationImportOption, GetDuration);
	Cpp2CS1(EngineNS, GfxAnimationImportOption, SetSampleRate);
	Cpp2CS0(EngineNS, GfxAnimationImportOption, GetSampleRate);

}