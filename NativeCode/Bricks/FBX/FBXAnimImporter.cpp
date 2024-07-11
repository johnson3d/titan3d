#include "FBXAnimImporter.h"
#include "FBXDataConverter.h"
#include "FBXUtils.h"
#include "fbxsdk/core/fbxproperty.h"

using namespace fbxsdk;

namespace AssetImportAndExport
{
	namespace FBX
	{
		bool CheckBoneNode(FbxNode* node)
		{
			auto att = node->GetNodeAttribute();
			if (att->GetAttributeType() == FbxNodeAttribute::EType::eSkeleton || att->GetAttributeType() == FbxNodeAttribute::EType::eNull)
			{
				return true;
			}
			return false;
		}

		FBXAnimCurve* FBXAnimElement::GetPropertyCurve(int index)
		{
			if (index > PropertyCurves.size())
				return nullptr;
			return &PropertyCurves[index];
		}

		FBXAnimCurve* FBXAnimElement::GetPropertyCurve(FBXCurvePropertyType propertyType)
		{
			for (int i = 0; i < (int)PropertyCurves.size(); ++i)
			{
				if (PropertyCurves[i].Desc.Type == propertyType)
				{
					return &PropertyCurves[i];
				}
			}
			return nullptr;
		}

		AssetImportAndExport::FBX::FBXAnimCurve* FBXAnimElement::GetPropertyCurve(VNameString propertyName)
		{
			for (int i = 0; i < (int)PropertyCurves.size(); ++i)
			{
				if (PropertyCurves[i].Desc.Name == propertyName)
				{
					return &PropertyCurves[i];
				}
			}
			return nullptr;

		}

		FBXKAnimKeyFrame* FBXAnimCurve::GetKeyFrame(int index)
		{
			if (index > Keys.size())
				return nullptr;
			return &Keys[index];
		}



		FBXAnimImporter::FBXAnimImporter(FBXImporter* hostFBXImporter, UINT animIndex)
		{
			mHostFBXImporter = hostFBXImporter;
			mAnimIndex = animIndex;
		}

		FBXAnimImporter::~FBXAnimImporter()
		{
			mHostFBXImporter = nullptr;
		}
		const FBXAnimImportDesc* FBXAnimImporter::GetAnimImportDesc() const
		{
			if (mHostFBXImporter)
			{
				return mHostFBXImporter->GetFBXAnimDesc(mAnimIndex);
			}
			else
			{
				return nullptr;
			}
		}
		EFBXImportResult FBXAnimImporter::Process()
		{
			auto animImportOption = GetAnimImportDesc();
			ASSERT(animImportOption);
			if (!animImportOption->Imported)
				return EFBXImportResult::FIR_Cancel;
			auto fileImportOption = mHostFBXImporter->GetFileImportDesc();
			auto node = animImportOption->GetFBXNode();
			auto animStack = animImportOption->GetFBXAnimStack();

			if (animImportOption->AnimationType ==  EFBXAnimType::AT_Skeleton)
			{
				ExtractBoneAnimationCurveRecursion(node, fileImportOption->ScaleFactor * animImportOption->Scale, nullptr, AnimElements);
			}
			else if(animImportOption->AnimationType == EFBXAnimType::AT_Property)
			{
				ExtractPropertyAnimationCurveRecursion(node, fileImportOption->ScaleFactor * animImportOption->Scale, AnimElements);
			}
			else
			{
				ASSERT(false);
			}
			return EFBXImportResult::FIR_Sucess;
		}

		FBXAnimElement* FBXAnimImporter::GetAnimElement(int index)
		{
			if (index > AnimElements.size())
			{
				return nullptr;
			}
			return &AnimElements[index];
		}

		void FBXAnimImporter::ExtractBoneAnimationCurveRecursion(FbxNode* node, float scale, const FBXAnimElement* parentElement, std::vector< FBXAnimElement>& outAnimElements)
		{
			FBXAnimElement animElement;
			if (CheckBoneNode(node))
			{
				MakeTransformAnimElement(node, scale, animElement);
				if (parentElement)
				{
					animElement.Desc.Parent = parentElement->Desc.Name;
					animElement.Desc.ParentHash = parentElement->Desc.NameHash;
				}
				outAnimElements.push_back(animElement);
				for (int i = 0; i < node->GetChildCount(); ++i)
				{
					ExtractBoneAnimationCurveRecursion(node->GetChild(i), scale, &animElement, outAnimElements);
				}
			}
		}

		void FBXAnimImporter::ExtractPropertyAnimationCurveRecursion(FbxNode* node, float scale, std::vector< FBXAnimElement>& outAnimElements)
		{
			ASSERT(false);
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
		bool FBXAnimImporter::CheckRootNode(FbxNode* node)
		{
			if (node == nullptr)
				return false;
			if (node == GetAnimImportDesc()->GetFBXNode())
			{
				return true;
			}
			return false;
		}
		void FBXAnimImporter::MakeTransformAnimElement(FbxNode* node, float scale, FBXAnimElement& outAnimElement)
		{
			auto layer = GetAnimImportDesc()->GetFBXAnimLayer();
			if (Utils::IsHaveAnimCurve(node, layer))
			{
				FBXAnimElementDesc& desc = outAnimElement.Desc;
				desc.Name.SetString(FBXDataConverter::ConvertToStdString(node->GetName()).c_str());
				desc.NameHash = HashHelper::APHash(desc.Name.GetString().c_str());
				//Pos
				{
					auto fbxXCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
					auto fbxYCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
					auto fbxZCurve = node->LclTranslation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);

					if (fbxXCurve)
					{
						FBXAnimCurve curve;
						curve.Desc.Name = VNameString("Position_X");
						curve.Desc.NameHash = HashHelper::APHash(curve.Desc.Name.c_str());
						curve.Desc.Type = Position_X;
						MakeAnimCurveKeyFrames(fbxXCurve, scale, curve);
						outAnimElement.PropertyCurves.push_back(curve);
					}
					// Y Z swap
					if (fbxZCurve)
					{
						FBXAnimCurve curve;
						curve.Desc.Name = VNameString("Position_Y");
						curve.Desc.NameHash = HashHelper::APHash(curve.Desc.Name.c_str());
						curve.Desc.Type = Position_Y;
						MakeAnimCurveKeyFrames(fbxZCurve, scale, curve);
						outAnimElement.PropertyCurves.push_back(curve);
					}
					if (fbxYCurve)
					{
						FBXAnimCurve curve;
						curve.Desc.Name = VNameString("Position_Z");
						curve.Desc.NameHash = HashHelper::APHash(curve.Desc.Name.c_str());
						curve.Desc.Type = Position_Z;
						MakeAnimCurveKeyFrames(fbxYCurve, scale, curve);
						outAnimElement.PropertyCurves.push_back(curve);
					}
				}
				//scale
				{
					auto fbxScaleXCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
					auto fbxScaleYCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
					auto fbxScaleZCurve = node->LclScaling.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
					if (fbxScaleXCurve)
					{
						FBXAnimCurve curve;
						curve.Desc.Name = VNameString("Scale_X");
						curve.Desc.NameHash = HashHelper::APHash(curve.Desc.Name.c_str());
						curve.Desc.Type = Scale_X;
						MakeAnimCurveKeyFrames(fbxScaleXCurve, scale, curve);
						outAnimElement.PropertyCurves.push_back(curve);
					}
					// Y Z swap
					if (fbxScaleZCurve)
					{
						FBXAnimCurve curve;
						curve.Desc.Name = VNameString("Scale_Y");
						curve.Desc.NameHash = HashHelper::APHash(curve.Desc.Name.c_str());
						curve.Desc.Type = Scale_Y;
						MakeAnimCurveKeyFrames(fbxScaleZCurve, scale, curve);
						outAnimElement.PropertyCurves.push_back(curve);
					}
					if (fbxScaleYCurve)
					{
						FBXAnimCurve curve;
						curve.Desc.Name = VNameString("Scale_Z");
						curve.Desc.NameHash = HashHelper::APHash(curve.Desc.Name.c_str());
						curve.Desc.Type = Scale_Z;
						MakeAnimCurveKeyFrames(fbxScaleYCurve, scale, curve);
						outAnimElement.PropertyCurves.push_back(curve);
					}
				}
				//Rot
				{
					auto rXCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_X);
					auto rYCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Y);
					auto rZCurve = node->LclRotation.GetCurve(layer, FBXSDK_CURVENODE_COMPONENT_Z);
					if (rXCurve || rYCurve || rZCurve)
					{
						auto animDesc = GetAnimImportDesc();
						float delta = 1.f / animDesc->SampleRate;
						float time = 0;

						FBXAnimCurve xCurve, yCurve, zCurve;

						xCurve.Desc.Name = VNameString("Yaw");
						xCurve.Desc.NameHash = HashHelper::APHash(xCurve.Desc.Name.c_str());
						xCurve.Desc.Type = Rotation_X;
						yCurve.Desc.Name = VNameString("Pitch");
						yCurve.Desc.NameHash = HashHelper::APHash(yCurve.Desc.Name.c_str());
						yCurve.Desc.Type = Rotation_Y;
						zCurve.Desc.Name = VNameString("Roll");
						zCurve.Desc.NameHash = HashHelper::APHash(zCurve.Desc.Name.c_str());
						zCurve.Desc.Type = Rotation_Z;
						while (time <= animDesc->Duration)
						{
							fbxsdk::FbxTime fbxTime;
							fbxTime.SetSecondDouble(time);
							auto fbxQuat = CalculateLRM(node, fbxTime, !CheckRootNode(node)).GetQ();
							auto quat = FBXDataConverter::ConvertQuat(fbxQuat);
							v3dxRotator lV;
							v3dxYawPitchRollQuaternionRotation(&quat, &lV);

							FBXKAnimKeyFrame key;
							key.Time = time;

							key.Value = lV.Yaw;
							xCurve.Keys.push_back(key);
							key.Value = lV.Pitch;
							yCurve.Keys.push_back(key);
							key.Value = lV.Roll;
							zCurve.Keys.push_back(key);

							time += delta;
						}
						outAnimElement.PropertyCurves.push_back(xCurve);
						outAnimElement.PropertyCurves.push_back(yCurve);
						outAnimElement.PropertyCurves.push_back(zCurve);
					}
				}
			}
		}



		void FBXAnimImporter::MakeAnimCurveKeyFrames(FbxAnimCurve* curve, float scale, FBXAnimCurve& outAnimCurve)
		{
			if (curve == nullptr)
				return;
			int KeyCount = curve->KeyGetCount();
			for (int i = 0; i < KeyCount; ++i)
			{
				FBXKAnimKeyFrame key;
				key.Time = (float)curve->KeyGetTime(i).GetSecondDouble();
				key.Value = (float)curve->KeyGetValue(i) * scale;
				outAnimCurve.Keys.push_back(key);
			}
			for (int i = 0; i < KeyCount - 1; ++i)
			{
				FBXKAnimKeyFrame key0 = outAnimCurve.Keys[i];
				FBXKAnimKeyFrame key1 = outAnimCurve.Keys[i + 1];
				float out;
				out = (key1.Value - key0.Value) / (key1.Time - key0.Time);
				key0.OutSlope = out;
				key1.InSlope = out;
			}
		}



	}
}