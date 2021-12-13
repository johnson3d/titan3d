#pragma once
#include "FBX.h"
#include "../../RHI/PreHead.h"
#include "fbxsdk.h"
#include "../../RHI/RHI.h"
#include "FBXImporter.h"
#include "../../Math/v3dxColor4.h"
#include "../../Graphics/Mesh/Modifier/ISkinModifier.h"

using namespace fbxsdk;

namespace AssetImportAndExport
{
	namespace FBX
	{
		enum TR_ENUM(SV_EnumNoFlags)
			FBXAnimElementType
		{
			AET_Default,
			AET_Bone,
		};

		enum TR_ENUM(SV_EnumNoFlags)
			FBXCurvePropertyType
		{
			Custom = 0,
			Position_X,
			Position_Y,
			Position_Z,
			Rotation_X,
			Rotation_Y,
			Rotation_Z,
			Scale_X,
			Scale_Y,
			Scale_Z,
			
		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXAnimElementDesc
		{
		public:
			FBXAnimElementType  Type = AET_Default;
			VNameString			Name;
			UINT				NameHash = 0;
			VNameString			Parent;
			UINT				ParentHash = 0;
		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXKAnimKeyFrame
		{
		public:
			float Time = 0.f;
			float Value = 0.f;
			float InSlope = 1.f;
			float OutSlope = 1.f;
		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXAnimCurveDesc
		{
			VNameString Name;
			UINT NameHash = 0;
			FBXCurvePropertyType Type = FBXCurvePropertyType::Custom;
		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXAnimCurve
		{
		public:
			FBXAnimCurveDesc Desc;
			std::vector<FBXKAnimKeyFrame> Keys;
		public:
			int GetKeyFrameNum() const { return (int)Keys.size(); }
			FBXKAnimKeyFrame* GetKeyFrame(int index);

		};

		struct TR_CLASS(SV_LayoutStruct = 8)
			FBXAnimElement
		{
		public:
			FBXAnimElementDesc Desc;
			std::vector<FBXAnimCurve> PropertyCurves;
		public:
			int GetCurveNum() const { return (int)PropertyCurves.size(); }
			FBXAnimCurve* GetPropertyCurve(VNameString propertyName);
			FBXAnimCurve* GetPropertyCurve(int index) ;
			FBXAnimCurve* GetPropertyCurve(FBXCurvePropertyType propertyType);
		};

		class TR_CLASS(SV_Dispose = delete self)
		FBXAnimImporter
		{
		public:
			FBXAnimImporter(FBXImporter * hostFBXImporter, UINT animIndex);
			~FBXAnimImporter();
		public:
			EFBXImportResult Process();
			int GetAnimElementsNum() const { return (int)AnimElements.size(); }
			FBXAnimElement* GetAnimElement(int index);
		protected:
			void ExtractBoneAnimationCurveRecursion(FbxNode* node, float scale, const FBXAnimElement* parentElement, std::vector< FBXAnimElement>& outAnimElements);
			void ExtractPropertyAnimationCurveRecursion(FbxNode * node, float scale, std::vector< FBXAnimElement>&outAnimElements);
			void MakeTransformAnimElement(FbxNode* node,float scale, FBXAnimElement& outAnimElement);
			void MakeAnimCurveKeyFrames(FbxAnimCurve* curve, float scale, FBXAnimCurve& outAnimCurve);
			const FBXAnimImportDesc* GetAnimImportDesc() const;
			bool CheckRootNode(FbxNode* node);
		protected:
			UINT mAnimIndex = -1;
			FBXImporter* mHostFBXImporter;
			bool HasProcessed = false;
			std::vector<FBXAnimElement> AnimElements;
		};

	}
}

