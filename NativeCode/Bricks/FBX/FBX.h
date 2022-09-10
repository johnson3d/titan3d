#pragma once
#include "../../NextRHI/NxRHI.h"
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

	class FBXImporter;
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