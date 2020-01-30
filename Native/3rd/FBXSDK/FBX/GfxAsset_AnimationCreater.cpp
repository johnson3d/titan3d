#include "GfxAsset_AnimationCreater.h"
#include "../../Bricks/Animation/AnimElement/GfxSkeletonAnimationElement.h"
#include "../../Bricks/Animation/AnimCurve/GfxBoneCurve.h"
#include "FbxDataConverter.h"
#include "../../Bricks/Animation/AnimElement/GfxAnimationElement.h"
#include "GfxFileImportOption.h"
#include "../../Bricks/Animation/AnimCurve/GfxFloatCurve.h"
#include "../../Bricks/Animation/AnimCurve/GfxMotionStateCurve.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAsset_AnimationCreater, EngineNS::VIUnknown);

#define BHOTRoot "BHOT_Root"
#define BHOTChild "BHOT_Child"
#define BHOTProperty "BHOT_Property"

GfxAsset_AnimationCreater::GfxAsset_AnimationCreater()
{

}

GfxAsset_AnimationCreater::~GfxAsset_AnimationCreater()
{
}

void GfxAsset_AnimationCreater::Process(IRenderContext* rc, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxFBXManager* manager)
{
	auto option = GetAnimationImportOption();
	if (!option->IsImport)
		return;
	OnImportMessageDumping(AMT_Import, 0, "Processing");
	auto node = option->FBXNode;
	auto animStack = option->AnimStack;
	if (option->AnimationType == IAT_Skeleton || option->AnimationType == IAT_Null)
	{
		auto animStackName = FbxDataConverter::ConvertToStdString(animStack->GetName());
		auto span = animStack->GetLocalTimeSpan();
		auto start = span.GetStart().GetSecondDouble();
		auto duration = span.GetDuration().GetSecondDouble();
		auto end = span.GetStop().GetSecondDouble();
		GfxSkeletonAnimationElement* skeletonElement = new GfxSkeletonAnimationElement();
		skeletonElement->SetAnimationElementType(AET_Skeleton);
		//ignore root preRotation & postRotation
		GetBoneAnimationCurveRecursion(node, skeletonElement, mAssetImportOption->Scale * fileImportOption->mScaleFactor, true);
		GfxAnimationElementDesc* desc = new GfxAnimationElementDesc();
		desc->SetName(animStackName.c_str());
		skeletonElement->SetAnimationElementDesc(desc);
		mElements.insert(std::make_pair(desc->NameHash, skeletonElement));
	}
	else
	{
		GfxAnimationElement* animElement = new GfxAnimationElement();
		animElement->SetAnimationElementType(AnimationElementType::AET_Default);
		GetPropertyAnimationCurveRecursion(node, animElement, mAssetImportOption->Scale * fileImportOption->mScaleFactor, false);
	}
	OnImportMessageDumping(AMT_Import, 0, "ProcessDone", 1.0f);
}

GfxAnimationElement* GfxAsset_AnimationCreater::GetElement(UINT index)
{
	auto it = mElements.begin();
	int i = 0;
	for (it, i = 0; it != mElements.end(); it++, i++)
	{
		if (i == index)
			return (*it).second;

	}
	return  NULL;
}

void FillFloatCurve(FbxAnimCurve* curve, GfxCurveTpl<float>* floatCurve, float scale)
{
	if (curve == nullptr)
		return;
	CurveKeyTpl<float> key;
	int KeyCount = curve->KeyGetCount();
	for (int i = 0; i < KeyCount; ++i)
	{
		key.Time = (float)curve->KeyGetTime(i).GetSecondDouble();
		key.Value = (float)curve->KeyGetValue(i) * scale;
		floatCurve->AddKeyBackFast(key);
	}
	for (int j = 0; j < (int)floatCurve->GetKeyCount() - 1; ++j)
	{
		CurveKeyTpl<float> key0, key1;
		key0.Time = floatCurve->GetKey(j).Time;
		key0.Value = floatCurve->GetKey(j).Value;
		key1.Time = floatCurve->GetKey(j + 1).Time;
		key1.Value = floatCurve->GetKey(j + 1).Value;
		float out;
		out = (key1.Value - key0.Value) / (key1.Time - key0.Time);
		floatCurve->GetKey(j).OutSlope = out;
		floatCurve->GetKey(j + 1).InSlope = out;
	}
}

void CalculateCurveTangent(GfxCurveTpl<float>* floatCurve)
{
	for (int j = 0; j < (int)floatCurve->GetKeyCount() - 1; ++j)
	{
		CurveKeyTpl<float> key0, key1;
		key0.Time = floatCurve->GetKey(j).Time;
		key0.Value = floatCurve->GetKey(j).Value;
		key1.Time = floatCurve->GetKey(j + 1).Time;
		key1.Value = floatCurve->GetKey(j + 1).Value;
		float out;
		out = (key1.Value - key0.Value) / (key1.Time - key0.Time);
		floatCurve->GetKey(j).OutSlope = out;
		floatCurve->GetKey(j + 1).InSlope = out;
	}
}

FbxAMatrix CalculateLRM(FbxNode* pNode, FbxTime time, bool prepost = true)
{
	FbxAMatrix lPreRotationM, lRotationM, lPostRotationM, lLRM;
	auto lRotation = pNode->LclRotation.EvaluateValue(time);
	auto lPreRotation = pNode->PreRotation.EvaluateValue(time);
	auto lPostRotation = pNode->PostRotation.EvaluateValue(time);
	lRotationM.SetR(lRotation);
	lPreRotationM.SetR(lPreRotation);
	lPostRotationM.SetR(lPostRotation);
	if (prepost)
		lLRM = lPreRotationM * lRotationM * lPostRotationM;
	else
		lLRM = lRotationM;
	return lLRM;
}

void EngineNS::GfxAsset_AnimationCreater::CreateBoneAnimationCurves(FbxNode* node, GfxSkeletonAnimationElement* skeletonElement, float scale, bool isRootNode)
{
	auto att = node->GetNodeAttribute();
	if (att == nullptr)
		return;
	auto layer = GetAnimationImportOption()->AnimLayer;
	if ((att->GetAttributeType() == FbxNodeAttribute::eSkeleton || att->GetAttributeType() == FbxNodeAttribute::eNull) && IsHaveAnimCurve(node, layer))
	{
		//calculate curve
		{
			GfxAnimationElement* element = new GfxAnimationElement();
			element->SetAnimationElementType(AET_Bone);
			GfxAnimationElementDesc* desc = new GfxAnimationElementDesc();
			auto nodeName = FbxDataConverter::ConvertToStdString(node->GetName());
			desc->SetName(nodeName.c_str());
			GfxBoneCurve* boneCurve = new GfxBoneCurve();
			element->SetCurve(boneCurve);
			element->SetAnimationElementDesc(desc);
			skeletonElement->AddElement(element);
			//Pos
			{
				auto fbxXCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
				auto fbxYCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
				auto fbxZCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
				if (fbxXCurve || fbxYCurve || fbxZCurve)
				{
					GfxVector3Curve* posCurve = new GfxVector3Curve();
					if (fbxXCurve)
					{
						posCurve->mXCurve = new GfxCurveTpl<float>();
						posCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
						posCurve->mXCurve->SetPreInfinity(WrapMode_Clamp);
						FillFloatCurve(fbxXCurve, posCurve->mXCurve, scale);
					}
					// Y Z swap
					if (fbxZCurve)
					{
						posCurve->mYCurve = new GfxCurveTpl<float>();
						posCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
						posCurve->mYCurve->SetPreInfinity(WrapMode_Clamp);
						FillFloatCurve(fbxZCurve, posCurve->mYCurve, scale);
					}
					if (fbxYCurve)
					{
						posCurve->mZCurve = new GfxCurveTpl<float>();
						posCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
						posCurve->mZCurve->SetPreInfinity(WrapMode_Clamp);
						FillFloatCurve(fbxYCurve, posCurve->mZCurve, scale);
					}
					boneCurve->mPosCurve.StrongRef(posCurve);
					Safe_Release(posCurve);
				}

				FullAnimTime fullAnimTime;
				int count = 0;
				auto tXCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
				if (tXCurve)
				{
					count = tXCurve->KeyGetCount();
					for (int i = 0; i < count; ++i)
					{
						fullAnimTime.Add(tXCurve->KeyGetTime(i));
					}
				}
				auto tYCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
				if (tYCurve)
				{
					count = tYCurve->KeyGetCount();
					for (int i = 0; i < count; ++i)
					{
						fullAnimTime.Add(tYCurve->KeyGetTime(i));
					}
				}
				auto tZCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
				if (tZCurve)
				{
					count = tZCurve->KeyGetCount();
					for (int i = 0; i < count; ++i)
					{
						fullAnimTime.Add(tZCurve->KeyGetTime(i));
					}
				}
				GfxMotionStateCurve* msCurve = new GfxMotionStateCurve();
				msCurve->mPosCurve = new GfxVector3Curve();
				msCurve->mPosCurve->mXCurve = new GfxCurveTpl<float>();
				msCurve->mPosCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
				msCurve->mPosCurve->mXCurve->SetPreInfinity(WrapMode_Clamp);
				msCurve->mPosCurve->mYCurve = new GfxCurveTpl<float>();
				msCurve->mPosCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
				msCurve->mPosCurve->mYCurve->SetPreInfinity(WrapMode_Clamp);
				msCurve->mPosCurve->mZCurve = new GfxCurveTpl<float>();
				msCurve->mPosCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
				msCurve->mPosCurve->mZCurve->SetPreInfinity(WrapMode_Clamp);
				msCurve->mVelocityCurve = new GfxVector3Curve();
				msCurve->mVelocityCurve->mXCurve = new GfxCurveTpl<float>();
				msCurve->mVelocityCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
				msCurve->mVelocityCurve->mXCurve->SetPreInfinity(WrapMode_Clamp);
				msCurve->mVelocityCurve->mYCurve = new GfxCurveTpl<float>();
				msCurve->mVelocityCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
				msCurve->mVelocityCurve->mYCurve->SetPreInfinity(WrapMode_Clamp);
				msCurve->mVelocityCurve->mZCurve = new GfxCurveTpl<float>();
				msCurve->mVelocityCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
				msCurve->mVelocityCurve->mZCurve->SetPreInfinity(WrapMode_Clamp);
				CurveKeyTpl<v3dxVector3> currentKey;
				for (int i = 0; i < (int)fullAnimTime.FullTime.size()-1; ++i)
				{
					auto first =FbxDataConverter::ConvertPos(node->EvaluateGlobalTransform(fullAnimTime.FullTime[i]).GetT()) * scale;
					auto next = FbxDataConverter::ConvertPos(node->EvaluateGlobalTransform(fullAnimTime.FullTime[i+1]).GetT() * scale);
					auto vel = (next - first) / (float)(fullAnimTime.FullTime[i + 1] - fullAnimTime.FullTime[i]).GetSecondDouble();
					
					CurveKeyTpl<float> key;
					key.Time = (float)fullAnimTime.FullTime[i].GetSecondDouble();
					key.Value = first.x;
					msCurve->mPosCurve->mXCurve->AddKeyBackFast(key);
					key.Value = first.y;
					msCurve->mPosCurve->mYCurve->AddKeyBackFast(key);
					key.Value = first.z;
					msCurve->mPosCurve->mZCurve->AddKeyBackFast(key);
					key.Value = vel.x;
					msCurve->mVelocityCurve->mXCurve->AddKeyBackFast(key);
					key.Value = vel.y;
					msCurve->mVelocityCurve->mYCurve->AddKeyBackFast(key);
					key.Value = vel.z;
					msCurve->mVelocityCurve->mZCurve->AddKeyBackFast(key);
				}
				CalculateCurveTangent(msCurve->mPosCurve->mXCurve);
				CalculateCurveTangent(msCurve->mPosCurve->mYCurve);
				CalculateCurveTangent(msCurve->mPosCurve->mZCurve);
				CalculateCurveTangent(msCurve->mVelocityCurve->mXCurve);
				CalculateCurveTangent(msCurve->mVelocityCurve->mYCurve);
				CalculateCurveTangent(msCurve->mVelocityCurve->mZCurve);
				boneCurve->mMotionStateCurve.StrongRef(msCurve);
				Safe_Release(msCurve);
			}
			//scale
			{
				auto fbxScaleXCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
				auto fbxScaleYCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
				auto fbxScaleZCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
				if (fbxScaleXCurve || fbxScaleYCurve || fbxScaleZCurve)
				{
					GfxVector3Curve* scaleCurve = new GfxVector3Curve();
					if (fbxScaleXCurve)
					{
						scaleCurve->mXCurve = new GfxCurveTpl<float>();
						scaleCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
						scaleCurve->mXCurve->SetPreInfinity(WrapMode_Clamp);
						FillFloatCurve(fbxScaleXCurve, scaleCurve->mXCurve, scale);
					}
					// Y Z swap
					if (fbxScaleZCurve)
					{
						scaleCurve->mYCurve = new GfxCurveTpl<float>();
						scaleCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
						scaleCurve->mYCurve->SetPreInfinity(WrapMode_Clamp);
						FillFloatCurve(fbxScaleZCurve, scaleCurve->mYCurve, scale);
					}
					if (fbxScaleYCurve)
					{
						scaleCurve->mZCurve = new GfxCurveTpl<float>();
						scaleCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
						scaleCurve->mZCurve->SetPreInfinity(WrapMode_Clamp);
						FillFloatCurve(fbxScaleYCurve, scaleCurve->mZCurve, scale);
					}
					boneCurve->mScaleCurve.StrongRef(scaleCurve);
					Safe_Release(scaleCurve);
				}
			}
			//Rotate
			{
				FullAnimTime fullAnimTime;
				int count = 0;
				auto rXCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
				if (rXCurve)
				{
					count = rXCurve->KeyGetCount();
					for (int i = 0; i < count; ++i)
					{
						fullAnimTime.Add(rXCurve->KeyGetTime(i));
					}
				}
				auto rYCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
				if (rYCurve)
				{
					count = rYCurve->KeyGetCount();
					for (int i = 0; i < count; ++i)
					{
						fullAnimTime.Add(rYCurve->KeyGetTime(i));
					}
				}
				auto rZCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
				if (rZCurve)
				{
					count = rZCurve->KeyGetCount();
					for (int i = 0; i < count; ++i)
					{
						fullAnimTime.Add(rZCurve->KeyGetTime(i));
					}
				}

				GfxQuaternionCurve* rotCurve = new GfxQuaternionCurve();
				rotCurve->mCurve.SetPostInfinity(WrapMode_Clamp);
				rotCurve->mCurve.SetPreInfinity(WrapMode_Clamp);
				CurveKeyTpl<v3dxQuaternion> currentRotKey;
				for (int i = 0; i < fullAnimTime.FullTime.size(); ++i)
				{
					//auto value = node->LclRotation.EvaluateValue(fullAnimTime.FullTime[i]);
					auto r = CalculateLRM(node, fullAnimTime.FullTime[i], !isRootNode).GetR();
					auto fbxQuat = CalculateLRM(node, fullAnimTime.FullTime[i], !isRootNode).GetQ();
					auto quat = FbxDataConverter::ConvertQuat(fbxQuat);
					currentRotKey.Time = (float)fullAnimTime.FullTime[i].GetSecondDouble();
					currentRotKey.Value = quat;
					rotCurve->mCurve.AddKey(currentRotKey);
				}
				for (int j = 0; j < (int)rotCurve->GetKeyCount() - 1; ++j)
				{
					CurveKeyTpl<v3dxQuaternion>& key0 = rotCurve->GetKey(j);
					CurveKeyTpl<v3dxQuaternion>& key1 = rotCurve->GetKey(j + 1);
					if (key0.Value.dot(key1.Value) < 0)
					{
						key1.Value = -key1.Value;
					}
				}
				for (int j = 0; j < (int)rotCurve->GetKeyCount() - 1; ++j)
				{
					CurveKeyTpl<v3dxQuaternion>& key0 = rotCurve->GetKey(j);
					CurveKeyTpl<v3dxQuaternion>& key1 = rotCurve->GetKey(j + 1);
					v3dxVector3 lV, rV;
					v3dxYawPitchRollQuaternionRotation(key0.Value, &lV);
					v3dxYawPitchRollQuaternionRotation(key1.Value, &rV);
					float lTime = key0.Time;
					float rTime = key1.Time;
					v3dxVector3 lVDelta, rVDelta;
					v3dxVec3Lerp(&lVDelta, &lV, &rV, 0.001F);
					v3dxVec3Lerp(&rVDelta, &lV, &rV, 0.999F);
					v3dxQuaternion lR, rR;

					v3dxQuaternionSlerp(&lR, &key0.Value, &key1.Value, 0.001f);
					v3dxQuaternionSlerp(&rR, &key0.Value, &key1.Value, 0.999f);
					/*if (lR.dot(key0.Value) < 0.0F)
						lR = -lR;
					if (rR.dot(key1.Value) < 0.0F)
						rR = -rR;*/
					key0.OutSlope = (lR - key0.Value) * 1000 / (rTime - lTime);
					key1.InSlope = (key1.Value - rR) * 1000 / (rTime - lTime);
					if (key0.OutSlope.x < -10 || key0.OutSlope.x >10)
					{
						int s = 0;
					}
				}
				boneCurve->mRotationCurve.StrongRef(rotCurve);
				Safe_Release(rotCurve);
			}

			//MotionData
			{

			}
		}
	}
}

void GfxAsset_AnimationCreater::GetBoneAnimationCurveRecursion(FbxNode* node, GfxSkeletonAnimationElement* skeletonElement, float scale, bool isRootNode)
{
	CreateBoneAnimationCurves(node, skeletonElement, scale, isRootNode);
	for (int i = 0; i < node->GetChildCount(); ++i)
	{
		GetBoneAnimationCurveRecursion(node->GetChild(i), skeletonElement, scale, false);
	}

}

void EngineNS::GfxAsset_AnimationCreater::GetPropertyAnimationCurveRecursion(FbxNode* node, GfxAnimationElement* propertyElement, float scale, bool isRootNode)
{
	auto nodeAtt = node->GetNodeAttribute();
	auto layer = GetAnimationImportOption()->AnimLayer;
	auto hierarchyNodeName = GetHierarchyNodeName(node);
	FbxProperty lProperty = node->GetFirstProperty();
	FbxString animCurveName;
	while (lProperty.IsValid())
	{
		auto curveNode = lProperty.GetCurveNode(layer);
		if (curveNode)
		{
			animCurveName = lProperty.GetName();
			GfxAnimationElement* element = new GfxAnimationElement();
			GfxAnimationElementDesc* desc = new GfxAnimationElementDesc();
			if (lProperty == node->LclTranslation)
			{
				auto propertyName = hierarchyNodeName + "/" + std::string(BHOTProperty) + ":" + "Placement/Location";
				desc->SetName("Positon");
				desc->SetPath(propertyName.c_str());
				element->SetAnimationElementDesc(desc);
				element->SetAnimationElementType(AnimationElementType::AET_Default);
				element->SetCurve(GetNodeLclTranslationCurve(node, scale));
			}
			else if (lProperty == node->LclRotation)
			{
				auto propertyName = hierarchyNodeName + "/" + std::string(BHOTProperty) + ":" + "Placement/Rotation";
				desc->SetName("Rotation");
				desc->SetPath(propertyName.c_str());
				element->SetAnimationElementDesc(desc);
				element->SetAnimationElementType(AnimationElementType::AET_Default);
				element->SetCurve(GetNodeLclRotationCurve(node, scale));
			}
			else if (lProperty == node->LclScaling)
			{
				auto propertyName = hierarchyNodeName + "/" + std::string(BHOTProperty) + ":" + "Placement/Scale";
				desc->SetName("Scale");
				desc->SetPath(propertyName.c_str());
				element->SetAnimationElementDesc(desc);
				element->SetAnimationElementType(AnimationElementType::AET_Default);
				element->SetCurve(GetNodeLclScaleCurve(node, scale));
			}
			else
			{
				auto propertyName = hierarchyNodeName + "/" + std::string(lProperty.GetName());
				desc->SetName(std::string(lProperty.GetName()).c_str());
				desc->SetPath(propertyName.c_str());
				element->SetAnimationElementDesc(desc);
				element->SetAnimationElementType(AnimationElementType::AET_Default);
				auto curve = lProperty.GetCurve(layer);
				GfxFloatCurve* floatCurve = new GfxFloatCurve();;
				FillFloatCurve(curve, &floatCurve->mCurve, scale);
				element->SetCurve(floatCurve);
				//element->SetCurve(GetNodeLclRotationCurve(node, scale));
			}
			mElements.insert(std::make_pair(desc->NameHash, element));
		}
		lProperty = node->GetNextProperty(lProperty);
	}
	lProperty = nodeAtt->GetFirstProperty();
	while (lProperty.IsValid())
	{
		auto curveNode = lProperty.GetCurveNode(layer);
		if (curveNode)
		{
			GfxAnimationElement* element = new GfxAnimationElement();
			GfxAnimationElementDesc* desc = new GfxAnimationElementDesc();
			animCurveName = lProperty.GetName();
			auto propertyName = hierarchyNodeName + "/" + std::string(lProperty.GetName());
			desc->SetName(propertyName.c_str());
			element->SetAnimationElementDesc(desc);
			element->SetAnimationElementType(AnimationElementType::AET_Default);
			auto curve = lProperty.GetCurve(layer);
			GfxFloatCurve* floatCurve = new GfxFloatCurve();;
			FillFloatCurve(curve, &floatCurve->mCurve, scale);
			element->SetCurve(floatCurve);
			mElements.insert(std::make_pair(desc->NameHash, element));
		}
		lProperty = nodeAtt->GetNextProperty(lProperty);
	}

}

std::string GfxAsset_AnimationCreater::GetHierarchyNodeName(FbxNode* node)
{
	std::string hierarchyNodeName = FbxDataConverter::ConvertToStdString(node->GetName());
	while (FbxDataConverter::ConvertToStdString(node->GetParent()->GetName()) != std::string("RootNode"))
	{
		std::string nodeName =std::string(BHOTChild)+ ":" + FbxDataConverter::ConvertToStdString(node->GetName());
		hierarchyNodeName = nodeName + std::string("/") + hierarchyNodeName;
		node = node->GetParent();
	}
	return std::string(BHOTRoot) + ":" + hierarchyNodeName;
}

GfxQuaternionCurve* GfxAsset_AnimationCreater::GetNodeLclRotationCurve(FbxNode* node, float scale)
{
	auto layer = GetAnimationImportOption()->AnimLayer;
	FullAnimTime fullAnimTime;
	//Rotate
	int count = 0;
	auto rXCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
	auto rYCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
	auto rZCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
	if (!rXCurve && !rYCurve && !rZCurve)
		return nullptr;
	if (rXCurve)
	{
		count = rXCurve->KeyGetCount();
		for (int i = 0; i < count; ++i)
		{
			fullAnimTime.Add(rXCurve->KeyGetTime(i));
		}
	}
	if (rYCurve)
	{
		count = rYCurve->KeyGetCount();
		for (int i = 0; i < count; ++i)
		{
			fullAnimTime.Add(rYCurve->KeyGetTime(i));
		}
	}
	if (rZCurve)
	{
		count = rZCurve->KeyGetCount();
		for (int i = 0; i < count; ++i)
		{
			fullAnimTime.Add(rZCurve->KeyGetTime(i));
		}
	}

	GfxQuaternionCurve* rotCurve = new GfxQuaternionCurve();
	rotCurve->mCurve.SetPostInfinity(WrapMode_Clamp);
	rotCurve->mCurve.SetPreInfinity(WrapMode_Clamp);
	CurveKeyTpl<v3dxQuaternion> currentRotKey;
	for (int i = 0; i < fullAnimTime.FullTime.size(); ++i)
	{
		//auto value = node->LclRotation.EvaluateValue(fullAnimTime.FullTime[i]);
		auto r = CalculateLRM(node, fullAnimTime.FullTime[i], false).GetR();
		auto fbxQuat = CalculateLRM(node, fullAnimTime.FullTime[i], false).GetQ();
		auto quat = FbxDataConverter::ConvertQuat(fbxQuat);
		currentRotKey.Time = (float)fullAnimTime.FullTime[i].GetSecondDouble();
		currentRotKey.Value = quat;
		rotCurve->mCurve.AddKey(currentRotKey);
	}
	for (int j = 0; j < (int)rotCurve->GetKeyCount() - 1; ++j)
	{
		CurveKeyTpl<v3dxQuaternion>& key0 = rotCurve->GetKey(j);
		CurveKeyTpl<v3dxQuaternion>& key1 = rotCurve->GetKey(j + 1);
		if (key0.Value.dot(key1.Value) < 0)
		{
			key1.Value = -key1.Value;
		}
	}
	for (int j = 0; j < (int)rotCurve->GetKeyCount() - 1; ++j)
	{
		CurveKeyTpl<v3dxQuaternion>& key0 = rotCurve->GetKey(j);
		CurveKeyTpl<v3dxQuaternion>& key1 = rotCurve->GetKey(j + 1);
		v3dxVector3 lV, rV;
		v3dxYawPitchRollQuaternionRotation(key0.Value, &lV);
		v3dxYawPitchRollQuaternionRotation(key1.Value, &rV);
		float lTime = key0.Time;
		float rTime = key1.Time;
		v3dxVector3 lVDelta, rVDelta;
		v3dxVec3Lerp(&lVDelta, &lV, &rV, 0.001F);
		v3dxVec3Lerp(&rVDelta, &lV, &rV, 0.999F);
		v3dxQuaternion lR, rR;

		v3dxQuaternionSlerp(&lR, &key0.Value, &key1.Value, 0.001f);
		v3dxQuaternionSlerp(&rR, &key0.Value, &key1.Value, 0.999f);
		/*if (lR.dot(key0.Value) < 0.0F)
			lR = -lR;
		if (rR.dot(key1.Value) < 0.0F)
			rR = -rR;*/
		key0.OutSlope = (lR - key0.Value) * 1000 / (rTime - lTime);
		key1.InSlope = (key1.Value - rR) * 1000 / (rTime - lTime);
		if (key0.OutSlope.x < -10 || key0.OutSlope.x >10)
		{
			int s = 0;
		}
	}
	return rotCurve;

}

GfxVector3Curve* GfxAsset_AnimationCreater::GetNodeLclTranslationCurve(FbxNode* node, float scale)
{
	//Pos
	auto layer = GetAnimationImportOption()->AnimLayer;
	auto fbxXCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
	auto fbxYCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
	auto fbxZCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
	if (fbxXCurve || fbxYCurve || fbxZCurve)
	{
		GfxVector3Curve* posCurve = new GfxVector3Curve();
		if (fbxXCurve)
		{
			posCurve->mXCurve = new GfxCurveTpl<float>();
			posCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
			posCurve->mXCurve->SetPreInfinity(WrapMode_Clamp);
			FillFloatCurve(fbxXCurve, posCurve->mXCurve, scale);
		}
		// Y Z swap
		if (fbxZCurve)
		{
			posCurve->mYCurve = new GfxCurveTpl<float>();
			posCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
			posCurve->mYCurve->SetPreInfinity(WrapMode_Clamp);
			FillFloatCurve(fbxZCurve, posCurve->mYCurve, scale);
		}
		if (fbxYCurve)
		{
			posCurve->mZCurve = new GfxCurveTpl<float>();
			posCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
			posCurve->mZCurve->SetPreInfinity(WrapMode_Clamp);
			FillFloatCurve(fbxYCurve, posCurve->mZCurve, scale);
		}
		return posCurve;
	}
	return NULL;
}

GfxVector3Curve* GfxAsset_AnimationCreater::GetNodeLclScaleCurve(FbxNode* node, float scale)
{
	auto layer = GetAnimationImportOption()->AnimLayer;
	auto fbxScaleXCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
	auto fbxScaleYCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
	auto fbxScaleZCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
	if (fbxScaleXCurve || fbxScaleYCurve || fbxScaleZCurve)
	{
		GfxVector3Curve* scaleCurve = new GfxVector3Curve();
		if (fbxScaleXCurve)
		{
			scaleCurve->mXCurve = new GfxCurveTpl<float>();
			scaleCurve->mXCurve->SetPostInfinity(WrapMode_Clamp);
			scaleCurve->mXCurve->SetPreInfinity(WrapMode_Clamp);
			FillFloatCurve(fbxScaleXCurve, scaleCurve->mXCurve, scale);
		}
		// Y Z swap
		if (fbxScaleZCurve)
		{
			scaleCurve->mYCurve = new GfxCurveTpl<float>();
			scaleCurve->mYCurve->SetPostInfinity(WrapMode_Clamp);
			scaleCurve->mYCurve->SetPreInfinity(WrapMode_Clamp);
			FillFloatCurve(fbxScaleZCurve, scaleCurve->mYCurve, scale);
		}
		if (fbxScaleYCurve)
		{
			scaleCurve->mZCurve = new GfxCurveTpl<float>();
			scaleCurve->mZCurve->SetPostInfinity(WrapMode_Clamp);
			scaleCurve->mZCurve->SetPreInfinity(WrapMode_Clamp);
			FillFloatCurve(fbxScaleYCurve, scaleCurve->mZCurve, scale);
		}
		return scaleCurve;
	}
	return NULL;
}

AnimationElementType GfxAsset_AnimationCreater::GetElementType(UINT index)
{
	auto it = mElements.begin();
	int i = 0;
	for (it, i = 0; it != mElements.end(); it++, i++)
	{
		if (i == index)
			return (*it).second->GetAnimationElementType();

	}
	return  AET_Default;
}

bool GfxAsset_AnimationCreater::IsHaveAnimCurve(FbxNode* node, FbxAnimLayer* animLayer)
{
	FbxProperty lProperty = node->GetFirstProperty();
	while (lProperty.IsValid())
	{
		auto curveNode = lProperty.GetCurveNode(animLayer);
		if (curveNode)
			return true;
		lProperty = node->GetNextProperty(lProperty);
	}
	return false;
}

NS_END
using namespace EngineNS;
extern "C"
{
	CSharpReturnAPI0(UINT, EngineNS, GfxAsset_AnimationCreater, GetElementCount);
	CSharpReturnAPI1(AnimationElementType, EngineNS, GfxAsset_AnimationCreater, GetElementType, UINT);
	CSharpReturnAPI1(GfxAnimationElement*, EngineNS, GfxAsset_AnimationCreater, GetElement, UINT);
}