#pragma once
#include "FBX.h"
#include "../../RHI/PreHead.h"
#include "fbxsdk.h"
#include "../../RHI/RHI.h"
#include "FBXImporter.h"
#include "../../Math/v3dxColor4.h"
#include "../Animation/Skeleton/IPartialSkeleton.h"
#include "../../Graphics/Mesh/Modifier/ISkinModifier.h"

namespace AssetImportAndExport
{
	namespace FBX
	{
		enum FBXAnimElementType
		{
			AET_Default,
			AET_Bone,
		};

		struct FBXAnimElementDesc
		{
		public:
			FBXAnimElementType  Type;
			VNameString			Name;
			UINT				NameHash;
			VNameString			Parent;
			UINT				ParentHash;
		};

		struct FBXKeyFrame
		{
		public:
			float Time;
			float Value;
			float InSlope;
			float OutSlope;
		};

		struct FBXAnimCurveDesc
		{
			VNameString Name;
			UINT NameHash;
		};

		struct FBXAnimCurve
		{
		public:
			FBXAnimCurveDesc Desc;
			std::vector<FBXKeyFrame> Keys;
		};

		struct FBXAnimElement
		{
		public:
			FBXAnimElementDesc Desc;
			std::vector<FBXAnimCurve> PropertyCurves;
		};

		class TR_CLASS(SV_Dispose = delete self)
		FBXAnimImporter
		{
		public:
			FBXAnimImporter(FBXImporter * hostFBXImporter, UINT animIndex);
			~FBXAnimImporter();
		public:
			EFBXImportResult Process(const FBXFileImportDesc * fileImportOption, const FBXAnimImportDesc * animImportOption);
		protected:
			UINT mAnimIndex = -1;
			FBXImporter* mHostFBXImporter;
			bool HasProcessed = false;
		};

	}
}

