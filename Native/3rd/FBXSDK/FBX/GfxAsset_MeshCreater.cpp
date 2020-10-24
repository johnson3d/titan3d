#include "GfxAsset_MeshCreater.h"
#include "GfxFBXManager.h"
#include "FbxDataConverter.h"
#include "../../Bricks/Animation/Pose/GfxAnimationPose.h"
#include "../../Bricks/Animation/Skeleton/GfxSkeleton.h"
#include "../../Graphics/Mesh/GfxSkinModifier.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "../../Graphics/Mesh/GfxMdfQueue.h"
#include "GfxAsset_SubMesh.h"
#include "GfxFileImportOption.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAsset_MeshCreater, EngineNS::GfxAssetCreater);


GfxAsset_MeshCreater::GfxAsset_MeshCreater()
{
}


GfxAsset_MeshCreater::~GfxAsset_MeshCreater()
{
}
bool IsSkeletonNode(FbxNode* boneNode)
{
	if (boneNode == NULL)
		return false;
	auto  name = FbxDataConverter::ConvertToStdString(boneNode->GetName());
	FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
	if (attr == NULL)
		return false;
	FbxNodeAttribute::EType attType = attr->GetAttributeType();
	if (attType == FbxNodeAttribute::eSkeleton || attType == FbxNodeAttribute::eNull)
		return true;
	else
		return false;
}
bool IsFbxRootNode(FbxNode* boneNode)
{
	if (boneNode == NULL)
		return true;
	auto  name = FbxDataConverter::ConvertToStdString(boneNode->GetName());
	FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
	if (attr == NULL && name == "RootNode")
		return true;
	else
		return false;
}
FbxNode* GetSkeletonRootNode(FbxNode* boneNode)
{
	auto  name = FbxDataConverter::ConvertToStdString(boneNode->GetName());
	FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
	FbxNodeAttribute::EType attType = attr->GetAttributeType();
	if (attType == FbxNodeAttribute::eSkeleton || attType == FbxNodeAttribute::eNull)
	{
		FbxSkeleton* fbxBone = (FbxSkeleton*)attr;
		if (fbxBone->GetSkeletonType() == FbxSkeleton::eRoot)
		{
			auto parent = boneNode->GetParent();
			if (parent != nullptr && parent->GetNodeAttribute() != nullptr)
			{
				auto parentAttType = parent->GetNodeAttribute()->GetAttributeType();
				if (attType != FbxNodeAttribute::eSkeleton && attType != FbxNodeAttribute::eNull)
				{
					return boneNode;
				}
			}
		}
	}
	if (!IsSkeletonNode(boneNode->GetParent()) || IsFbxRootNode(boneNode->GetParent()))
	{
		return boneNode;
	}
	return GetSkeletonRootNode(boneNode->GetParent());
}

void GfxAsset_MeshCreater::RecursionCalculateBone(FbxNode* boneNode, GfxSkeleton* skeleton, std::vector<BoneCluster> boneClusters)
{
	if (boneNode == NULL)
		return;
	auto boneName = FbxDataConverter::ConvertToStdString(boneNode->GetName());
	auto r = boneNode->LclRotation.Get();
	auto t = boneNode->LclTranslation.Get();
	auto s = boneNode->LclScaling.Get();

	//CalculateGlobalTransform(boneNode);
	auto bone = skeleton->FindBone(boneName.c_str());
	if (bone)
	{
		for (int i = 0; i < boneNode->GetChildCount(); ++i)
		{
			RecursionCalculateBone(boneNode->GetChild(i), skeleton, boneClusters);
		}
	}
	else
	{
		FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
		if (!attr)
			return;
		FbxNodeAttribute::EType attType = attr->GetAttributeType();
		if (attType != FbxNodeAttribute::eSkeleton && attType != FbxNodeAttribute::eNull)
			return;
		FbxSkeleton* fbxBone = (FbxSkeleton*)attr;

		GfxBoneDesc* boneDesc = new GfxBoneDesc();
		boneDesc->SetName(boneName.c_str());
		auto mat = boneNode->EvaluateGlobalTransform();
		auto scaleT = mat.GetT() * mAssetImportOption->Scale;
		mat.SetT(scaleT);
		//boneDesc->SetBindMatrix(&FbxDataConverter::ConvertMatrix(mat));
		auto v3dMat = v3dxMatrix4::IDENTITY;
		boneDesc->SetBindMatrix(&v3dMat);
		//if (fbxBone->GetSkeletonType() != FbxSkeleton::eRoot)
		{
			FbxNode* parentNode = boneNode->GetParent();
			if (parentNode != NULL)
			{
				FbxNodeAttribute* parentAttr = parentNode->GetNodeAttribute();
				if (parentAttr != NULL)
				{
					if (parentAttr->GetAttributeType() == FbxNodeAttribute::eSkeleton || parentAttr->GetAttributeType() == FbxNodeAttribute::eNull)
					{
						auto parentNodeName = FbxDataConverter::ConvertToStdString(parentNode->GetName());
						boneDesc->SetParent(parentNodeName.c_str());
					}
				}
			}
		}
		auto newBone = skeleton->NewBone(boneDesc);
		for (int i = 0; i < boneNode->GetChildCount(); ++i)
		{
			RecursionCalculateBone(boneNode->GetChild(i), skeleton, boneClusters);
		}
	}

}
void ReCalculateBoneBindMatrix(GfxBone* child, GfxBone* parent, GfxSkeleton* skeleton)
{
	if (parent != NULL)
	{
		//child->mSharedData->InitMatrix = child->mSharedData->InitMatrix * parent->mSharedData->InitMatrix;
	}
	for (UINT i = 0; i < child->GetChildNumber(); ++i)
	{
		ReCalculateBoneBindMatrix(skeleton->GetBone(child->GetChild(i)), child, skeleton);
	}
}

void GfxAsset_MeshCreater::Process(IRenderContext* rc, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxFBXManager* manager)
{
	if (!mAssetImportOption->IsImport)
		return;
	auto meshImportOption = (GfxMeshImportOption*)mAssetImportOption;


	FbxNode* node = mAssetImportOption->FBXNode;
	auto att = node->GetNodeAttribute();
	if (att->GetAttributeType() != IAT_Mesh)
		return;
	auto mesh = (FbxMesh*)att;
	OnImportMessageDumping(AMT_Import, 0, "Processing", 0);

	// Must do this before triangulating the mesh due to an FBX bug in TriangulateMeshAdvance
	// Convert mesh, NURBS and patch into triangle mesh
	FbxGeometryConverter GeometryConverter(manager->GetSDKManager());
	int LayerSmoothingCount = mesh->GetLayerCount(FbxLayerElement::eSmoothing);
	for (int i = 0; i < LayerSmoothingCount; i++)
	{
		FbxLayerElementSmoothing const* SmoothingInfo = mesh->GetLayer(0)->GetSmoothing();
		if (SmoothingInfo && SmoothingInfo->GetMappingMode() != FbxLayerElement::eByPolygon)
		{
			GeometryConverter.ComputePolygonSmoothingFromEdgeSmoothing(mesh, i);
		}
	}
	GeometryConverter.Triangulate(scene, /*replace*/true);


	auto controlPointsCount = mesh->GetControlPointsCount();
	auto controlPoints = mesh->GetControlPoints();
	auto polyCount = mesh->GetPolygonCount();
	//auto polyVertexCount = mesh->GetPolygonVertexCount();

	//split submesh by material
	std::vector<std::vector<int>*> subMeshIndices;
	auto nodeMatCount = node->GetMaterialCount();

	auto elementMatCount = mesh->GetElementMaterialCount();
	if (elementMatCount == 0 || nodeMatCount == 0)
	{
		subMeshIndices.push_back(new std::vector<int>());
		for (int i = 0; i < polyCount; ++i)
		{
			subMeshIndices[0]->push_back(i);
		}
	}
	else {
		for (int i = 0; i < nodeMatCount; ++i)
		{
			subMeshIndices.push_back(new std::vector<int>());
		}
		for (int i = 0; i < elementMatCount; ++i)
		{
			auto mat = node->GetMaterial(i);
			auto elementMat = mesh->GetElementMaterial(i);
			auto indices = elementMat->GetIndexArray();
			auto mode = elementMat->GetMappingMode();
			if (mode == FbxLayerElement::eByPolygon)
			{
				auto  count = indices.GetCount();
				int matId = 0;
				for (int j = 0; j < polyCount; ++j)
				{
					auto lMatId = indices.GetAt(j);
					subMeshIndices[lMatId]->push_back(j);
					auto material = node->GetMaterial(lMatId);
				}
			}
			else
			{
				int matId = 0;
				for (int j = 0; j < polyCount; ++j)
				{
					auto lMatId = indices.GetAt(0);
					subMeshIndices[lMatId]->push_back(j);
					auto material = node->GetMaterial(lMatId);
				}
			}
		}
	}
	for (int i = 0; i < (int)subMeshIndices.size(); ++i)
	{
		for (int j = i; j < (int)subMeshIndices.size(); ++j)
		{
			if (subMeshIndices[i]->size() < subMeshIndices[j]->size())
			{
				auto iIndices = subMeshIndices[i];
				subMeshIndices[i] = subMeshIndices[j];
				subMeshIndices[j] = iIndices;
			}
		}
	}
	GfxAsset_SubMesh* subMesh = new GfxAsset_SubMesh[subMeshIndices.size()];
	for (int i = 0; i < subMeshIndices.size(); ++i)
	{
		subMesh[i].PolyIndices = *subMeshIndices[i];
		subMesh[i].PolyCount = (int)subMeshIndices[i]->size();
		subMesh[i].AssetCreater = this;
		subMesh[i].Process(rc, scene, fileImportOption, meshImportOption, manager);
	}
	std::vector<DrawPrimitiveDesc> descs;
	UINT startIndex = 0;
	for (int i = 0; i < subMeshIndices.size(); ++i)
	{
		DrawPrimitiveDesc desc;
		desc.PrimitiveType = EPT_TriangleList;
		desc.BaseVertexIndex = 0;
		desc.StartIndex = startIndex;
		desc.NumInstances = 1;
		desc.NumPrimitives = subMesh[i].PolyCount;
		startIndex += desc.NumPrimitives * 3;
		descs.push_back(desc);
	}
	GfxAsset_SubMesh& biggestMesh = subMesh[0];
	for (int i = 1; i < subMeshIndices.size(); ++i)
	{
		subMesh[i].MergeTo(&biggestMesh);
	}



	auto renderVertexCount = biggestMesh.VertexCount();
	auto& vertexSkinIndex = biggestMesh.VertexSkinIndex;
	auto& vertexSkinWeight = biggestMesh.VertexSkinWeight;
	BYTE* skinIndexsStream = new BYTE[4 * renderVertexCount];
	float* skinWeightsStream = new float[4 * renderVertexCount];

	if (meshImportOption->HaveSkin)
	{
		//build skeleton
		std::vector < BoneCluster> BoneClusters;
		mFullSkeleton = new GfxSkeleton();
		GfxSkeleton* meshSubSkeleton = new GfxSkeleton();
		{
			auto skinDeformerCount = mesh->GetDeformerCount(FbxDeformer::EDeformerType::eSkin);
			if (skinDeformerCount > 0)
			{
				for (int i = 0; i < skinDeformerCount; ++i)
				{
					FbxSkin* skin = (FbxSkin*)mesh->GetDeformer(i, FbxDeformer::EDeformerType::eSkin);
					auto clusterCount = skin->GetClusterCount();
					for (int clusterIndex = 0; clusterIndex < clusterCount; ++clusterIndex)
					{
						bool illegalScale = false;
						FbxCluster* cluster = skin->GetCluster(clusterIndex);
						FbxNode* link = cluster->GetLink();
						auto boneName = FbxDataConverter::ConvertToStdString(link->GetName());
						auto lclR = link->LclRotation.Get();
						auto lclT = link->LclTranslation.Get();
						auto lclS = link->LclScaling.Get();
						FbxAMatrix linkTransformMatrix, transformMatrix;
						linkTransformMatrix = cluster->GetTransformLinkMatrix(linkTransformMatrix);
						auto linkrot = linkTransformMatrix.GetR();
						auto linktran = linkTransformMatrix.GetT();
						auto linkscale = linkTransformMatrix.GetS();
						if (Math::Abs((float)linkscale[0] - 1) > SMALL_EPSILON || Math::Abs((float)linkscale[1] - 1) > SMALL_EPSILON || Math::Abs((float)linkscale[2] - 1) > SMALL_EPSILON)
						{
							illegalScale = true;
						}
						transformMatrix = cluster->GetTransformMatrix(transformMatrix);
						auto rot = transformMatrix.GetR();
						auto tran = transformMatrix.GetT();
						auto scale = transformMatrix.GetS();
						if (Math::Abs((float)scale[0] - 1) > SMALL_EPSILON || Math::Abs((float)scale[1] - 1) > SMALL_EPSILON || Math::Abs((float)scale[2] - 1) > SMALL_EPSILON)
						{
							illegalScale = true;
						}
						if (illegalScale)
						{
							std::string name(boneName);
							std::string message("Bone has illegal Scale");
							OnImportMessageDumping(AMT_Error, 0, (name + ":" + message).c_str(), 0.0f);
						}

						FbxNodeAttribute* attr = link->GetNodeAttribute();
						FbxNodeAttribute::EType attType = attr->GetAttributeType();
						if (attType != FbxNodeAttribute::eSkeleton && attType != FbxNodeAttribute::eNull)
							continue;
						FbxSkeleton* fbxBone = (FbxSkeleton*)attr;
						GfxBoneDesc* boneDesc = new GfxBoneDesc();
						boneDesc->SetName(boneName.c_str());
						auto mat = transformMatrix.Inverse() * linkTransformMatrix;
						auto scaleT = mat.GetT() * mAssetImportOption->Scale * fileImportOption->mScaleFactor;
						mat.SetT(scaleT);

						auto v3dMat = FbxDataConverter::ConvertMatrix(mat);
						boneDesc->SetBindMatrix(&v3dMat);
						//if (fbxBone->GetSkeletonType() != FbxSkeleton::eRoot)
						{
							FbxNode* parentNode = link->GetParent();
							if (parentNode != NULL)
							{
								FbxNodeAttribute* parentAttr = parentNode->GetNodeAttribute();
								if (parentAttr != NULL)
								{
									if (parentAttr->GetAttributeType() == FbxNodeAttribute::eSkeleton || parentAttr->GetAttributeType() == FbxNodeAttribute::eNull)
									{
										auto parentNodeName = FbxDataConverter::ConvertToStdString(parentNode->GetName());
										boneDesc->SetParent(parentNodeName.c_str());
									}
								}
							}
						}
						auto newBone = mFullSkeleton->NewBone(boneDesc);
						BoneCluster boneCluster;
						boneCluster.Bone = newBone;
						boneCluster.FBXCluster = cluster;
						BoneClusters.push_back(boneCluster);
						if (fbxBone->GetSkeletonType() == FbxSkeleton::eRoot)
						{
							mFullSkeleton->SetRoot(boneName.c_str());
						}
						auto newMeshBone = meshSubSkeleton->NewBone(boneDesc);
						if (fbxBone->GetSkeletonType() == FbxSkeleton::eRoot)
						{
							meshSubSkeleton->SetRoot(boneName.c_str());
						}
					}
					if (clusterCount > 0)
					{
						FbxCluster* cluster = skin->GetCluster(0);
						FbxNode* link = cluster->GetLink();
						auto root = GetSkeletonRootNode(link);
						RecursionCalculateBone(root, mFullSkeleton, BoneClusters);
						auto rootName = FbxDataConverter::ConvertToStdString(root->GetName());
						mFullSkeleton->SetRoot(rootName.c_str());
					}

				}

				mFullSkeleton->GenerateHierarchy();
				ReCalculateBoneBindMatrix(mFullSkeleton->GetRoot(), NULL, mFullSkeleton);
			}
		}

		//build skinModifier

		auto skinModifier = new GfxSkinModifier();
		skinModifier->SetSkeleton(meshSubSkeleton);
		mMeshPrimitives->GetMdfQueue()->AddModifier(skinModifier);
		for (int i = 0; i < renderVertexCount; ++i)
		{
			auto size = vertexSkinIndex[i].size();
			for (int j = 0; j < size; ++j)
			{
				for (int k = j + 1; k < size; ++k)
				{
					if (vertexSkinWeight[i][j] < vertexSkinWeight[i][k])
					{
						auto temp = vertexSkinWeight[i][j];
						vertexSkinWeight[i][j] = vertexSkinWeight[i][k];
						vertexSkinWeight[i][k] = temp;
						auto tempIndex = vertexSkinIndex[i][j];
						vertexSkinIndex[i][j] = vertexSkinIndex[i][k];
						vertexSkinIndex[i][k] = tempIndex;
					}
				}
			}
		}
		//build skin stream
		for (int i = 0; i < renderVertexCount; ++i)
		{
			auto size = vertexSkinIndex[i].size();
			float totalWeight = 0.0f;
			if (size > 4)
				size = 4;
			for (int j = 0; j < size; ++j)
			{
				totalWeight += vertexSkinWeight[i][j];
			}
			for (int j = 0; j < 4; ++j)
			{
				if (j < size)
				{
					skinIndexsStream[i * 4 + j] = vertexSkinIndex[i][j];
					skinWeightsStream[i * 4 + j] = vertexSkinWeight[i][j] / totalWeight;
				}
				else
				{
					skinIndexsStream[i * 4 + j] = 0;
					skinWeightsStream[i * 4 + j] = 0;
				}
			}
		}
	}

	OnImportMessageDumping(AMT_Import, 0, "Build Stream", 0.5f);
	//build stream
	auto polyVertexCount = biggestMesh.IndexCount();
	auto& renderVertexs = biggestMesh.Vertexs;
	v3dxVector3* posStream = new v3dxVector3[renderVertexCount];
	v3dxVector3* normalStream = new v3dxVector3[renderVertexCount];
	v3dVector4_t* tangentStream = new v3dVector4_t[renderVertexCount];
	v3dxVector2* uvStream = new v3dxVector2[renderVertexCount];
	v3dxVector2* lightMapStream = new v3dxVector2[renderVertexCount];
	DWORD* vertexColorStream = new DWORD[renderVertexCount];
	UINT16* renderIndex16 = NULL;
	UINT* renderIndex32 = NULL;
	bool isIndex32 = false;


	//copy
	{
		if (biggestMesh.IsIndex32())
		{
			isIndex32 = true;
			renderIndex32 = biggestMesh.VertexIndices.data();
		}
		else
		{
			renderIndex16 = new UINT16[polyVertexCount];
			for (int i = 0; i < polyVertexCount; ++i)
			{
				renderIndex16[i] = (UINT16)biggestMesh.VertexIndices[i];
			}
		}
		for (int i = 0; i < renderVertexCount; ++i)
		{
			posStream[i] = renderVertexs[i].Postion;
			normalStream[i] = renderVertexs[i].Normal;
			tangentStream[i] = renderVertexs[i].TangentChirality;
			vertexColorStream[i] = renderVertexs[i].Color.getABGR();
			uvStream[i] = renderVertexs[i].UV;
			lightMapStream[i] = renderVertexs[i].UV2;
		}
	}
	bool hasVertexColor = true;
	mesh->GetElementVertexColorCount();
	if (mesh->GetElementVertexColorCount() > 0)
	{
		auto lVertexColorMappingMode = mesh->GetElementVertexColor(0)->GetMappingMode();
		if (lVertexColorMappingMode == FbxGeometryElement::eNone)
		{
			//Don't have VertexColor
			hasVertexColor = false;
		}

	}
	else
	{
		//Don't have VertexColor
		hasVertexColor = false;
	}
	//set stream
	{

		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_Position, posStream, (UINT)((UINT)sizeof(v3dxVector3) * renderVertexCount), sizeof(v3dxVector3), 0);
		if (hasVertexColor)
			mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_Color, vertexColorStream, (UINT)((UINT)sizeof(DWORD) * renderVertexCount), sizeof(DWORD), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_Normal, normalStream, (UINT)((UINT)sizeof(v3dxVector3) * renderVertexCount), sizeof(v3dxVector3), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_UV, uvStream, (UINT)((UINT)sizeof(v3dxVector2) * renderVertexCount), sizeof(v3dxVector2), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_LightMap, lightMapStream, (UINT)((UINT)sizeof(v3dxVector2) * renderVertexCount), sizeof(v3dxVector2), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_Tangent, tangentStream, (UINT)((UINT)sizeof(v3dVector4_t) * renderVertexCount), sizeof(v3dVector4_t), 0);
		if (isIndex32)
		{
			mMeshPrimitives->SetGeomtryMeshIndex(rc, renderIndex32, polyVertexCount * sizeof(UINT), EIndexBufferType::IBT_Int32, 0);
		}
		else
		{
			mMeshPrimitives->SetGeomtryMeshIndex(rc, renderIndex16, polyVertexCount * sizeof(UINT16), EIndexBufferType::IBT_Int16, 0);
		}
		if (meshImportOption->HaveSkin)
		{
			mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_SkinIndex, skinIndexsStream, 4 * renderVertexCount * sizeof(BYTE), 4 * sizeof(BYTE), 0);
			mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_SkinWeight, skinWeightsStream, 4 * renderVertexCount * sizeof(float), 4 * sizeof(float), 0);
		}
	}

	//build aabb
	{
		v3dxBox3 aabb;
		for (int i = 0; i < renderVertexCount; i++)
		{
			aabb.OptimalVertex(posStream[i]);
		}
		mMeshPrimitives->SetAABB(aabb);
	}

	//delete stream
	{
		delete[] posStream;
		delete[] normalStream;
		delete[] tangentStream;
		delete[] uvStream;
		delete[] lightMapStream;
		if (renderIndex16)
		{
			delete[] renderIndex16;
		}
		if (renderIndex32)
		{
			// renderIndex32 is vertexIndex
		}
		//delete[] pControlPoints;
		//delete[] pAssetVertexs;
	}

	//final
	for (int i = 0; i < descs.size(); ++i)
	{
		mMeshPrimitives->PushAtomLOD(i, &descs[i]);
	}
	OnImportMessageDumping(AMT_Import, 0, "ProcessDone", 1.0f);
}





NS_END

using namespace EngineNS;
extern "C"
{
	Cpp2CS1(EngineNS, GfxAsset_MeshCreater, SetMeshPrimitives);
	Cpp2CS0(EngineNS, GfxAsset_MeshCreater, GetFullSkeleton);
}