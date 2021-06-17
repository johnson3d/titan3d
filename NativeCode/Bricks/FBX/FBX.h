#pragma once
#include "../../RHI/PreHead.h"
#include "fbxsdk.h"
#include "../../Base/CoreSDK.h"
#include "../../Base/String/vfxstring.h"

using namespace EngineNS;

namespace AssetImportAndExport::FBX
{
	enum TR_ENUM(SV_EnumNoFlags)
		EFBXObjectType
	{
		FOT_Unknown,
			FOT_Null,
			FOT_Marker,
			FOT_Skeleton,
			FOT_Mesh,
			FOT_Nurbs,
			FOT_Patch,
			FOT_Camera,
			FOT_CameraStereo,
			FOT_CameraSwitcher,
			FOT_Light,
			FOT_OpticalReference,
			FOT_OpticalMarker,
			FOT_NurbsCurve,
			FOT_TrimNurbsSurface,
			FOT_Boundary,
			FOT_NurbsSurface,
			FOT_Shape,
			FOT_LODGroup,
			FOT_SubDiv,
			FOT_CachedEffect,
			FOT_Line,
			FOT_Animation,
			FOT_Default,
	};

	enum TR_ENUM(SV_EnumNoFlags)
		SystemUnit
	{
		SU_mm,
			SU_dm,
			SU_cm,
			SU_m,
			SU_km,
			SU_Inch,
			SU_Foot,
			SU_Mile,
			SU_Yard,
			SU_Custom,
	};


	struct TR_CLASS(SV_LayoutStruct = 8)
		FBXObjectImportDesc
	{
		friend class FBXImporter;
	public:
		FBXObjectImportDesc()
		{
			Name = nullptr;
			Type = FOT_Unknown;
			Scale = 1.f;
			Imported = true;
		}
		~FBXObjectImportDesc()
		{
			FBXNode = nullptr;
		}
	public:
		VNameString Name;
		EFBXObjectType Type;
		float Scale;
		bool Imported;
	public:
		FbxNode* GetFBXNode() const { return FBXNode; }
	protected:
		FbxNode* FBXNode;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FBXMeshImportDesc :public FBXObjectImportDesc
	{
		friend class FBXImporter;
	public:
		bool ReCalculateTangent;
		bool AsCollision;
		bool AsLocalSpace;
		bool HaveSkin;
		bool AsStaticMesh;
		UINT RenderAtom;
		bool TransformVertexToAbsolute;
		bool BakePivotInVertex;
	};


	struct TR_CLASS(SV_LayoutStruct = 8)
	FBXAnimImportDesc :public FBXObjectImportDesc
	{
		friend class FBXImporter;
	public:
		float Duration;
		float SampleRate;
		EFBXObjectType AnimationType;
	public:
		const FbxAnimStack* GetFBXAnimStack() const { return AnimStack; }
		const FbxAnimLayer* GetFBXAnimLayer() const { return AnimLayer; }
	private:
		FbxAnimStack* AnimStack;
		FbxAnimLayer* AnimLayer;
	};

	struct TR_CLASS(SV_Dispose = delete self)
		FBXFileImportDesc
	{
		friend class FBXImporter;
	public:
		FBXFileImportDesc()
		{
			FileName = nullptr;
			Creater = nullptr;
			FileSystemUnit = SU_cm;
			ConvertSceneUnit = false;
			ScaleFactor = 1.f;
			MeshNum = 0;
			Meshes = nullptr;
		};
		~FBXFileImportDesc()
		{
			for (UINT i = 0; i < MeshNum; ++i)
			{
				Safe_Delete<FBXMeshImportDesc>(Meshes[i]);
			}
			Safe_DeleteArray<FBXMeshImportDesc*>(Meshes);
			for (UINT i = 0; i < MeshNum; ++i)
			{
				Safe_Delete<FBXAnimImportDesc>(Anims[i]);
			}
			Safe_DeleteArray<FBXAnimImportDesc*>(Anims);
		}
	public:
		VNameString FileName;
		VNameString Creater;
		SystemUnit FileSystemUnit;
		bool ConvertSceneUnit;
		float ScaleFactor;
		UINT MeshNum;
		UINT AnimNum;
	protected:
		//FBXObjectImportDesc** Objects;
		FBXMeshImportDesc** Meshes;
		FBXAnimImportDesc** Anims;
	};
	
	class TR_CLASS(SV_Dispose = delete self)
	FBXFactory
	{
	public:
		FBXFactory();
		~FBXFactory();
	public:
		FBXImporter* CreateImporter();
	protected:
		void InitializeSdkManager();
		void DestroySdkObjects();
		fbxsdk::FbxManager* mFBXSdkManager;
	};

}