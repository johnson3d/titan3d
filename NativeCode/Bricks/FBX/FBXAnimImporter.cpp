#include "FBXAnimImporter.h"
#include "FbxDataConverter.h"


namespace AssetImportAndExport
{
	namespace FBX
	{

		FBXAnimImporter::FBXAnimImporter(FBXImporter* hostFBXImporter, UINT animIndex)
		{
			mHostFBXImporter = hostFBXImporter;
			mAnimIndex = animIndex;
		}

		FBXAnimImporter::~FBXAnimImporter()
		{
			mHostFBXImporter = nullptr;
		}

		EFBXImportResult FBXAnimImporter::Process(const FBXFileImportDesc* fileImportOption, const FBXAnimImportDesc* animImportOption)
		{
			if (!animImportOption->Imported)
				return FIR_Cancel;
			auto node = animImportOption->GetFBXNode();
			auto animStack = animImportOption->GetFBXAnimStack();
			
			//if (animImportOption->AnimationType == FOT_Skeleton || animImportOption->AnimationType == FOT_Null)
			//{
			//	//skeleton animation
			//	auto animStackName = FbxDataConverter::ConvertToStdString(animStack->GetName());
			//	auto span = animStack->GetLocalTimeSpan();
			//	auto start = span.GetStart().GetSecondDouble();
			//	auto duration = span.GetDuration().GetSecondDouble();
			//	auto end = span.GetStop().GetSecondDouble();
			//	GfxSkeletonAnimationElement* skeletonElement = new GfxSkeletonAnimationElement();
			//	skeletonElement->SetAnimationElementType(AET_Skeleton);
			//	//ignore root preRotation & postRotation
			//	GetBoneAnimationCurveRecursion(node, skeletonElement, mAssetImportOption->Scale * fileImportOption->mScaleFactor, true);
			//	GfxAnimationElementDesc* desc = new GfxAnimationElementDesc();
			//	desc->SetName(animStackName.c_str());
			//	skeletonElement->SetAnimationElementDesc(desc);
			//	mElements.insert(std::make_pair(desc->NameHash, skeletonElement));
			//}
			//else
			//{
			//	GfxAnimationElement* animElement = new GfxAnimationElement();
			//	animElement->SetAnimationElementType(AnimationElementType::AET_Default);
			//	GetPropertyAnimationCurveRecursion(node, animElement, mAssetImportOption->Scale * fileImportOption->mScaleFactor, false);
			//}
		}

	}
}