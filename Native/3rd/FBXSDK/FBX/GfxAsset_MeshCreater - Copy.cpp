#include "GfxAsset_MeshCreater.h"
#include "GfxFBXManager.h"
#include "FbxDataConverter.h"
#include "../../Bricks/Animation/Pose/GfxAnimationPose.h"
#include "../../Bricks/Animation/Skeleton/GfxSkeleton.h"
#include "../../Graphics/Mesh/GfxSkinModifier.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "../../Graphics/Mesh/GfxMdfQueue.h"

#define new VNEW
NS_BEGIN
RTTI_IMPL(EngineNS::GfxAsset_MeshCreater, EngineNS::GfxAssetCreater);

OverlappingVertexs::OverlappingVertexs(const std::vector<v3dxVector3>& InVertices, const std::vector<UINT>& InIndices, float ComparisonThreshold)
{
	const int NumWedges = (int)InIndices.size();

	// Create a list of vertex Z/index pairs
	std::vector<IndexAndZ> VertIndexAndZ;
	//VertIndexAnd.res(NumWedges);
	for (int WedgeIndex = 0; WedgeIndex < NumWedges; WedgeIndex++)
	{
		VertIndexAndZ.push_back(IndexAndZ(WedgeIndex, InVertices[InIndices[WedgeIndex]]));
	}

	// Sort the vertices by z value
	sort(VertIndexAndZ.begin(), VertIndexAndZ.end(), CompareIndexAndZ());
	//VertIndexAndZ.sort(FCompareIndexAndZ());

	Init(NumWedges);

	// Search for duplicates, quickly!
	for (int i = 0; i < VertIndexAndZ.size(); i++)
	{
		// only need to search forward, since we add pairs both ways
		for (int j = i + 1; j < VertIndexAndZ.size(); j++)
		{
			if (Math::Abs(VertIndexAndZ[j].Z - VertIndexAndZ[i].Z) > ComparisonThreshold)
				break; // can't be any more dups

			const v3dxVector3& PositionA = InVertices[InIndices[VertIndexAndZ[i].Index]];
			const v3dxVector3& PositionB = InVertices[InIndices[VertIndexAndZ[j].Index]];

			if (PointsEqual(PositionA, PositionB, ComparisonThreshold))
			{
				Add(VertIndexAndZ[i].Index, VertIndexAndZ[j].Index);
			}
		}
	}

	FinishAdding();
}

void OverlappingVertexs::Init(int NumIndices)
{
	Arrays.clear();
	Sets.clear();
	bFinishedAdding = false;

	IndexBelongsTo.clear();
	for (int i = 0; i < NumIndices; ++i)
	{
		IndexBelongsTo.push_back(INDEX_NONE);
	}
}

void OverlappingVertexs::Add(int Key, int Value)
{
	if (Key == Value)
		return;
	//check(bFinishedAdding == false);

	int ContainerIndex = IndexBelongsTo[Key];
	if (ContainerIndex == INDEX_NONE)
	{
		ContainerIndex = (int)Arrays.size();
		std::vector<int> Container;
		Container.push_back(Key);
		Container.push_back(Value);
		Arrays.push_back(Container);
		IndexBelongsTo[Key] = ContainerIndex;
		IndexBelongsTo[Value] = ContainerIndex;
	}
	else
	{
		IndexBelongsTo[Value] = ContainerIndex;

		std::vector<int>& ArrayContainer = Arrays[ContainerIndex];
		if (ArrayContainer.size() == 1)
		{
			// Container is a set
			//Sets[ArrayContainer.Last()].Add(Value);
		}
		else
		{
			// Container is an array
			bool find = false;
			for (int i = 0; i < ArrayContainer.size(); ++i)
			{
				if (ArrayContainer[i] == Value)
				{
					find = true;
					break;
				}
			}
			if (!find)
				ArrayContainer.push_back(Value);
			//// Change container into set when one vertex is shared by large number of triangles
			//if (ArrayContainer.Num() > 12)
			//{
			//	int SetIndex = Sets.Num();
			//	TSet<int>& Set = Sets.AddDefaulted_GetRef();
			//	Set.Append(ArrayContainer);

			//	// Having one element means we are using a set
			//	// An array will never have just 1 element normally because we add them as pairs
			//	ArrayContainer.Reset(1);
			//	ArrayContainer.Add(SetIndex);
			//}
		}
	}
}

void OverlappingVertexs::FinishAdding()
{
	//check(bFinishedAdding == false);

	for (std::vector<int>& Array : Arrays)
	{
		//// Turn sets back into arrays for easier iteration code
		//// Also reduces peak memory later in the import process
		//if (Array.Num() == 1)
		//{
		//	TSet<int>& Set = Sets[Array.Last()];
		//	Array.Reset(Set.Num());
		//	for (int i : Set)
		//	{
		//		Array.Add(i);
		//	}
		//}

		// Sort arrays now to avoid sort multiple times
		sort(Array.begin(), Array.end());
	}


	bFinishedAdding = true;
}



GfxAsset_MeshCreater::GfxAsset_MeshCreater()
{
}


GfxAsset_MeshCreater::~GfxAsset_MeshCreater()
{
}

const int TRIANGLE_VERTEX_COUNT = 3;

// Four floats for every position.
const int VERTEX_STRIDE = 3;
// Three floats for every normal.
const int NORMAL_STRIDE = 3;
// Two floats for every UV.
const int UV_STRIDE = 2;
FbxAMatrix CalculateGlobalTransform(FbxNode* pNode)
{
	FbxAMatrix lTranlationM, lScalingM, lScalingPivotM, lScalingOffsetM, lRotationOffsetM, lRotationPivotM, \
		lPreRotationM, lRotationM, lPostRotationM, lTransform;

	FbxAMatrix lParentGX, lGlobalT, lGlobalRS;

	if (!pNode)
	{
		lTransform.SetIdentity();
		return lTransform;
	}

	// Construct translation matrix
	FbxVector4 lTranslation = pNode->LclTranslation.Get();
	lTranlationM.SetT(lTranslation);

	// Construct rotation matrices
	FbxVector4 lRotation = pNode->LclRotation.Get();
	FbxVector4 lPreRotation = pNode->PreRotation.Get();
	FbxVector4 lPostRotation = pNode->PostRotation.Get();
	lRotationM.SetR(lRotation);
	//lPreRotationM.SetR(lPreRotation);
	//lPostRotationM.SetR(lPostRotation);
	lPreRotationM.SetIdentity();
	lPostRotationM.SetIdentity();

	// Construct scaling matrix
	FbxVector4 lScaling = pNode->LclScaling.Get();
	lScalingM.SetS(lScaling);

	// Construct offset and pivot matrices
	FbxVector4 lScalingOffset = pNode->ScalingOffset.Get();
	FbxVector4 lScalingPivot = pNode->ScalingPivot.Get();
	FbxVector4 lRotationOffset = pNode->RotationOffset.Get();
	FbxVector4 lRotationPivot = pNode->RotationPivot.Get();
	lScalingOffsetM.SetT(lScalingOffset);
	lScalingPivotM.SetT(lScalingPivot);
	lRotationOffsetM.SetT(lRotationOffset);
	lRotationPivotM.SetT(lRotationPivot);

	// Calculate the global transform matrix of the parent node
	FbxNode* lParentNode = pNode->GetParent();
	if (lParentNode)
	{
		lParentGX = CalculateGlobalTransform(lParentNode);
	}
	else
	{
		lParentGX.SetIdentity();
	}

	//Construct Global Rotation
	FbxAMatrix lLRM, lParentGRM;
	FbxVector4 lParentGR = lParentGX.GetR();
	lParentGRM.SetR(lParentGR);
	lLRM = lPreRotationM * lRotationM * lPostRotationM;

	//Construct Global Shear*Scaling
	//FBX SDK does not support shear, to patch this, we use:
	//Shear*Scaling = RotationMatrix.Inverse * TranslationMatrix.Inverse * WholeTranformMatrix
	FbxAMatrix lLSM, lParentGSM, lParentGRSM, lParentTM;
	FbxVector4 lParentGT = lParentGX.GetT();
	lParentTM.SetT(lParentGT);
	lParentGRSM = lParentTM.Inverse() * lParentGX;
	lParentGSM = lParentGRM.Inverse() * lParentGRSM;
	lLSM = lScalingM;

	//Do not consider translation now
	FbxTransform::EInheritType lInheritType = pNode->InheritType.Get();
	if (lInheritType == FbxTransform::eInheritRrSs)
	{
		lGlobalRS = lParentGRM * lLRM * lParentGSM * lLSM;
	}
	else if (lInheritType == FbxTransform::eInheritRSrs)
	{
		lGlobalRS = lParentGRM * lParentGSM * lLRM * lLSM;
	}
	else if (lInheritType == FbxTransform::eInheritRrs)
	{
		FbxAMatrix lParentLSM;
		FbxVector4 lParentLS = lParentNode->LclScaling.Get();
		lParentLSM.SetS(lParentLS);

		FbxAMatrix lParentGSM_noLocal = lParentGSM * lParentLSM.Inverse();
		lGlobalRS = lParentGRM * lLRM * lParentGSM_noLocal * lLSM;
	}
	else
	{
		FBXSDK_printf("error, unknown inherit type! \n");
	}

	// Construct translation matrix
	// Calculate the local transform matrix
	lTransform = lTranlationM * lRotationOffsetM * lRotationPivotM * lPreRotationM * lRotationM * lPostRotationM * lRotationPivotM.Inverse()\
		* lScalingOffsetM * lScalingPivotM * lScalingM * lScalingPivotM.Inverse();
	FbxVector4 lLocalTWithAllPivotAndOffsetInfo = lTransform.GetT();
	// Calculate global translation vector according to: 
	// GlobalTranslation = ParentGlobalTransform * LocalTranslationWithPivotAndOffsetInfo
	FbxVector4 lGlobalTranslation = lParentGX.MultT(lLocalTWithAllPivotAndOffsetInfo);
	lGlobalT.SetT(lGlobalTranslation);

	//Construct the whole global transform
	lTransform = lGlobalT * lGlobalRS;

	return lTransform;
}
int IsSameVertex(AssetVertex& vertexA, AssetVertex& vertexB)
{
	return (vertexA.Postion.Equals(vertexB.Postion) &&
		vertexA.Normal.Equals(vertexB.Normal) &&
		vertexA.UV.Equals(vertexB.UV));
}
bool IsSkeletonNode(FbxNode* boneNode)
{
	auto  name = boneNode->GetName();
	FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
	if (attr == NULL)
		return false;
	FbxNodeAttribute::EType attType = attr->GetAttributeType();
	if (attType == FbxNodeAttribute::eSkeleton)
		return true;
	else
		return false;
}
bool IsFbxRootNode(FbxNode* boneNode)
{
	auto  name = boneNode->GetName();
	FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
	if (attr == NULL && name == "RootNode")
		return true;
	else
		return false;
}
FbxNode* GetSkeletonRootNode(FbxNode* boneNode)
{
	auto  name = boneNode->GetName();
	FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
	FbxNodeAttribute::EType attType = attr->GetAttributeType();
	if (attType == FbxNodeAttribute::eSkeleton)
	{
		FbxSkeleton* fbxBone = (FbxSkeleton*)attr;
		if (fbxBone->GetSkeletonType() == FbxSkeleton::eRoot)
			return boneNode;
	}
	if (!IsSkeletonNode(boneNode->GetParent()) || IsFbxRootNode(boneNode->GetParent()))
	{
		return boneNode;
	}
	return GetSkeletonRootNode(boneNode->GetParent());
}
FbxMatrix GetBindMatrix(FbxNode* boneNode)
{
	return FbxMatrix();
}
//生成没在cluster里的骨骼
void GfxAsset_MeshCreater::RecursionCalculateBone(FbxNode* boneNode, GfxAnimationPose* boneTable, std::vector<BoneCluster> boneClusters)
{
	if (boneNode == NULL)
		return;
	auto boneName = boneNode->GetName();
	auto r = boneNode->LclRotation.Get();
	auto t = boneNode->LclTranslation.Get();
	auto s = boneNode->LclScaling.Get();

	CalculateGlobalTransform(boneNode);
	auto bone = boneTable->FindBone(boneName);
	if (bone)
	{
		for (int i = 0; i < boneNode->GetChildCount(); ++i)
		{
			RecursionCalculateBone(boneNode->GetChild(i), boneTable, boneClusters);
		}
	}
	else
	{
		FbxNodeAttribute* attr = boneNode->GetNodeAttribute();
		FbxNodeAttribute::EType attType = attr->GetAttributeType();
		if (attType != FbxNodeAttribute::eSkeleton)
			return;
		FbxSkeleton* fbxBone = (FbxSkeleton*)attr;

		GfxBoneDesc* boneDesc = new GfxBoneDesc();
		boneDesc->SetName(boneName);
		auto mat = boneNode->EvaluateGlobalTransform();
		auto scaleT = mat.GetT()* mAssetImportOption->Scale;
		mat.SetT(scaleT);
		//boneDesc->SetBindMatrix(&FbxDataConverter::ConvertMatrix(mat));
		auto v3dMat = v3dxMatrix4::IDENTITY;
		boneDesc->SetBindMatrix(&v3dMat);
		if (fbxBone->GetSkeletonType() != FbxSkeleton::eRoot)
		{
			FbxNode* parentNode = boneNode->GetParent();
			FbxNodeAttribute* parentAttr = parentNode->GetNodeAttribute();
			if (parentAttr != NULL && parentAttr->GetAttributeType() == FbxNodeAttribute::eSkeleton)
			{
				boneDesc->SetParent(parentNode->GetName());
			}
		}
		auto newBone = boneTable->NewBone(boneDesc);
		if (fbxBone->GetSkeletonType() == FbxSkeleton::eRoot)
		{
			boneTable->SetRoot(boneName);
			return;
		}
		else
		{
			for (int i = 0; i < boneNode->GetChildCount(); ++i)
			{
				RecursionCalculateBone(boneNode->GetChild(i), boneTable, boneClusters);
			}
		}
	}

}

void ReCalculateBoneBindMatrix(GfxBone* child, GfxBone* parent, GfxAnimationPose* boneTable)
{
	if (parent != NULL)
	{
		//child->mSharedData->InitMatrix = child->mSharedData->InitMatrix * parent->mSharedData->InitMatrix;
	}
	for (UINT i = 0; i < child->GetChildNumber(); ++i)
	{
		ReCalculateBoneBindMatrix(boneTable->GetBone(child->GetChild(i)), child, boneTable);
	}
}

void GfxAsset_MeshCreater::Process(IRenderContext* rc, FbxScene* scene)
{
	if (!mAssetImportOption->IsImport)
		return;
	auto GeometryConverter = new FbxGeometryConverter(GfxFBXManager::GetSDKManager());
	FbxNode* node = mAssetImportOption->FBXNode;
	auto att = node->GetNodeAttribute();
	if (att->GetAttributeType() != IAT_Mesh)
		return;
	auto mesh = (FbxMesh*)att;
	auto controlPointsCount = mesh->GetControlPointsCount();
	auto controlPoints = mesh->GetControlPoints();
	auto polyCount = mesh->GetPolygonCount();
	auto polyVertexCount = mesh->GetPolygonVertexCount();

	//sort poly by material
	std::vector<std::vector<int>> subMeshIndices;
	auto nodeMatCount = node->GetMaterialCount();
	for (int i = 0; i < nodeMatCount; ++i)
	{
		subMeshIndices.push_back(std::vector<int>());
	}
	auto elementMatCount = mesh->GetElementMaterialCount();
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
				subMeshIndices[lMatId].push_back(j);
				auto material = node->GetMaterial(lMatId);
			}
		}
		else
		{
			ASSERT(FALSE);
		}
	}

	GfxAsset_SubMesh* subMesh = new GfxAsset_SubMesh[]
	for (int i = 0; i < subMeshIndices.size(); ++i)
	{

	}



	// Construct the matrices for the conversion from right handed to left handed system
	v3dxMatrix4 TotalMatrix;
	v3dxMatrix4 TotalMatrixForNormal;
	TotalMatrix = ComputeTotalMatrix(node, scene);
	TotalMatrixForNormal = TotalMatrix.inverse();
	TotalMatrixForNormal.transPose();
	v3dxVector3* pControlPoints = new v3dxVector3[controlPointsCount];
	AssetVertex* pAssetVertexs = new AssetVertex[polyVertexCount];
	//build vertex for per poly
	{
		// Must do this before triangulating the mesh due to an FBX bug in TriangulateMeshAdvance
		int LayerSmoothingCount = mesh->GetLayerCount(FbxLayerElement::eSmoothing);
		for (int i = 0; i < LayerSmoothingCount; i++)
		{
			FbxLayerElementSmoothing const* SmoothingInfo = mesh->GetLayer(0)->GetSmoothing();
			if (SmoothingInfo && SmoothingInfo->GetMappingMode() != FbxLayerElement::eByPolygon)
			{
				GeometryConverter->ComputePolygonSmoothingFromEdgeSmoothing(mesh, i);
			}
		}
		//Position 
		{
			for (int i = 0; i < controlPointsCount; ++i)
			{
				//auto point = globalMatrix.MultT(controlPoints[i]);
				auto pos = FbxDataConverter::ConvertPos(controlPoints[i]);
				pos = pos * TotalMatrix;
				pControlPoints[i] = pos;
				//pControlPoints[i].x = (float)pos[0];
				//pControlPoints[i].y = (float)pos[1];
				//pControlPoints[i].z = (float)pos[2];
			}
			int vertexCount = 0;
			for (int i = 0; i < polyCount; ++i)
			{
				for (int j = 0; j < VERTEX_STRIDE; j++)
				{
					int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
					v3dxVector3* vertex = &pControlPoints[ctrlPointIndex];
					AssetVertex aVertex;
					aVertex.Postion = *vertex * mAssetImportOption->Scale;
					pAssetVertexs[vertexCount] = aVertex;
					vertexCount++;
				}
			}

		}
		//Normal
		{
			bool hasNormal = true;
			if (mesh->GetElementNormalCount() > 0)
			{
				auto lNormalMappingMode = mesh->GetElementNormal(0)->GetMappingMode();
				if (lNormalMappingMode == FbxGeometryElement::eNone)
				{
					//Don't have Normal
					hasNormal = false;
				}

			}
			else
			{
				//Don't have Normal
				hasNormal = false;
			}
			if (hasNormal)
			{
				auto lNormalMappingMode = mesh->GetElementNormal(0)->GetMappingMode();
				const FbxGeometryElementNormal * pNormalElement = mesh->GetElementNormal(0);;
				FbxVector4 currentNormal;
				v3dxVector3 normal;
				if (lNormalMappingMode == FbxGeometryElement::eByControlPoint)
				{
					v3dxVector3* pControlPointNormals = new v3dxVector3[controlPointsCount];
					for (int i = 0; i < controlPointsCount; ++i)
					{
						int normalIndex = i;
						if (pNormalElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
						{
							normalIndex = pNormalElement->GetIndexArray().GetAt(i);
						}
						currentNormal = pNormalElement->GetDirectArray().GetAt(normalIndex);
						normal = FbxDataConverter::ConvertDir(currentNormal);
						normal = normal * TotalMatrixForNormal;
						pControlPointNormals[i] = normal;
						//pControlPointNormals[i].x = (float)currentNormal[0];
						//pControlPointNormals[i].y = (float)currentNormal[1];
						//pControlPointNormals[i].z = (float)currentNormal[2];
					}
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
							normal = pControlPointNormals[ctrlPointIndex];
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							vertex.Normal = normal;
							vertexCount++;
						}
					}
					delete[] pControlPointNormals;
				}
				else
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
							bool result = mesh->GetPolygonVertexNormal(i, j, currentNormal);
							normal = FbxDataConverter::ConvertDir(currentNormal);
							normal = normal * TotalMatrixForNormal;
							//normal = ;
							/*normal.x = static_cast<float>(currentNormal[0]);
							normal.y = static_cast<float>(currentNormal[1]);
							normal.z = static_cast<float>(currentNormal[2]);*/
							normal.normalize();
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							vertex.Normal = normal;
							vertexCount++;
						}
					}
				}

			}

		}
		//UV
		{
			bool hasUV = true;
			if (mesh->GetElementUVCount() > 0)
			{
				auto lUVMappingMode = mesh->GetElementUV(0)->GetMappingMode();
				if (lUVMappingMode == FbxGeometryElement::eNone)
				{
					//Don't have UV
					hasUV = false;
				}

			}
			else
			{
				//Don't have Normal
				hasUV = false;
			}
			if (hasUV)
			{
				float * lUVs = NULL;
				FbxStringList lUVNames;
				mesh->GetUVSetNames(lUVNames);

				for (int uvIndex = 0; uvIndex < mesh->GetElementUVCount(); ++uvIndex)
				{
					const char * lUVName = lUVNames[uvIndex];
					auto lUVMappingMode = mesh->GetElementUV(uvIndex)->GetMappingMode();
					const FbxGeometryElementUV * lUVElement = mesh->GetElementUV(uvIndex);
					FbxVector2 currentUV;
					v3dxVector2 UV;
					if (lUVMappingMode == FbxGeometryElement::eByControlPoint)
					{
						v3dxVector2* pControlPointUVs = new v3dxVector2[controlPointsCount];
						for (int i = 0; i < controlPointsCount; ++i)
						{
							int UVIndex = i;
							if (lUVElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
							{
								UVIndex = lUVElement->GetIndexArray().GetAt(i);
							}
							currentUV = lUVElement->GetDirectArray().GetAt(UVIndex);
							pControlPointUVs[i].x = static_cast<float>(currentUV[0]);
							pControlPointUVs[i].y = 1.0f - static_cast<float>(currentUV[1]);
						}
						int vertexCount = 0;
						for (int i = 0; i < polyCount; ++i)
						{
							for (int j = 0; j < VERTEX_STRIDE; j++)
							{
								int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
								UV = pControlPointUVs[ctrlPointIndex];
								AssetVertex& vertex = pAssetVertexs[vertexCount];
								if (uvIndex == 0)
									vertex.UV = UV;
								else
									vertex.UV2 = UV;
								vertexCount++;
							}
						}
						delete[] pControlPointUVs;

					}
					else if (lUVMappingMode == FbxGeometryElement::eByPolygonVertex)
					{
						int vertexCount = 0;
						for (int i = 0; i < polyCount; ++i)
						{
							for (int j = 0; j < VERTEX_STRIDE; j++)
							{
								bool lUnmappedUV;
								bool result = mesh->GetPolygonVertexUV(i, j, lUVName, currentUV, lUnmappedUV);
								int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
								UV.x = static_cast<float>(currentUV[0]);
								UV.y = 1.0f - static_cast<float>(currentUV[1]);
								AssetVertex& vertex = pAssetVertexs[vertexCount];
								if (uvIndex == 0)
									vertex.UV = UV;
								else
									vertex.UV2 = UV;
								vertexCount++;
							}
						}
					}
				}
			}

		}
	}

	//ControlIndicesMapPolyIndices
	std::vector< std::vector <int> > ControlIndicesMapPolyIndices;
	{
		for (int i = 0; i < controlPointsCount; ++i)
		{
			ControlIndicesMapPolyIndices.push_back(std::vector<int>());
		}
		int vertexCount = 0;
		for (int i = 0; i < polyCount; ++i)
		{
			for (int j = 0; j < VERTEX_STRIDE; j++)
			{
				int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
				ControlIndicesMapPolyIndices[ctrlPointIndex].push_back(vertexCount);
				++vertexCount;
			}
		}
	}

	//build overlaps for accelerate
	OverlappingVertexs overlappingVertexs;
	{
		overlappingVertexs.Init(polyVertexCount);
		float ComparisonThreshold = 0.00001f;
		std::vector<IndexAndZ> VertIndexAndZ;
		int vertexCount = 0;
		for (int i = 0; i < polyCount; ++i)
		{
			for (int j = 0; j < VERTEX_STRIDE; j++)
			{
				int ctrlPointIndex = mesh->GetPolygonVertex(i, j);
				VertIndexAndZ.push_back(IndexAndZ(vertexCount, pControlPoints[ctrlPointIndex]));
				++vertexCount;
			}
		}
		sort(VertIndexAndZ.begin(), VertIndexAndZ.end(), CompareIndexAndZ());
		// Search for duplicates, quickly!
		for (int i = 0; i < VertIndexAndZ.size(); i++)
		{
			// only need to search forward, since we add pairs both ways
			for (int j = i + 1; j < VertIndexAndZ.size(); j++)
			{
				if (Math::Abs(VertIndexAndZ[j].Z - VertIndexAndZ[i].Z) > ComparisonThreshold)
					break; // can't be any more dups

				v3dxVector3& PositionA = (v3dxVector3)*(VertIndexAndZ[i].OriginalVector);
				v3dxVector3& PositionB = (v3dxVector3)*(VertIndexAndZ[j].OriginalVector);

				if (PositionA.Equals(PositionB, ComparisonThreshold))
				{
					overlappingVertexs.Add(VertIndexAndZ[i].Index, VertIndexAndZ[j].Index);
					overlappingVertexs.Add(VertIndexAndZ[j].Index, VertIndexAndZ[i].Index);
				}
			}
		}
		overlappingVertexs.FinishAdding();
	}

	//build indices
	std::vector<AssetVertex> renderVertexs;
	std::vector<int> remapIndex;
	UINT32* vertexIndex = new UINT32[polyVertexCount];
	{
		int theVertexIndex = 0;
		for (int i = 0; i < polyCount; ++i)
		{
			for (int j = 0; j < VERTEX_STRIDE; ++j)
			{
				AssetVertex& vertex = pAssetVertexs[i * 3 + j];
				const std::vector<int>& overlapVertexs = overlappingVertexs.FindIfOverlapping(theVertexIndex);
				//find overlaps before self
				int findSameVertexIndex = -1;
				for (int k = 0; k < overlapVertexs.size(); ++k)
				{
					int overlapIndex = overlapVertexs[k];
					//find self ,then no same
					if (theVertexIndex == overlapIndex)
						break;
					if (IsSameVertex(pAssetVertexs[overlapIndex], vertex))
					{
						findSameVertexIndex = overlapIndex;
						break;
					}
				}
				if (findSameVertexIndex == -1)
				{
					vertexIndex[i * 3 + 2 - j] = (UINT)renderVertexs.size();
					remapIndex.push_back(vertexIndex[i * 3 + 2 - j]);
					renderVertexs.push_back(vertex);
				}
				else
				{
					vertexIndex[i * 3 + 2 - j] = remapIndex[findSameVertexIndex];
					remapIndex.push_back(remapIndex[findSameVertexIndex]);
				}
				//remapIndex.push_back(theVertexIndex);
				//remapIndex[theVertexIndex] = remapIndex[overlapIndex];
				//vertexIndex[i * 3 + j] = remapIndex[overlapIndex];
				//int sameIndex = IsSameVertex(renderVertexs, vertex);
				//if (sameIndex == -1)
				//{
				//	vertexIndex[i * 3 + j] = (UINT)renderVertexs.size();
				//	renderVertexs.push_back(vertex);
				//	remapIndex[theVertexIndex] = renderVertexs.size();
				//}
				//else
				//{
				//	vertexIndex[i * 3 + j] = sameIndex;
				//}
				++theVertexIndex;
			}
		}
	}

	auto poseCount = scene->GetPoseCount();
	for (int i = 0; i < poseCount; ++i)
	{
		auto pose = scene->GetPose(i);
		auto isBindePose = pose->IsBindPose();
		for (int j = 0; j < pose->GetCount(); ++j)
		{
			auto node = pose->GetNode(j);
			auto name = pose->GetName();
			auto gM = node->EvaluateGlobalTransform();
			auto lM = node->EvaluateLocalTransform();
			auto matrix = pose->GetMatrix(j);
		}
	}

	//build vertexSkinIndex
	std::vector < BoneCluster> BoneClusters;
	int renderVertexCount = (int)renderVertexs.size();
	std::vector<std::vector<UINT>> vertexSkinIndex;
	std::vector<std::vector<float>> vertexSkinWeight;
	AutoRef<GfxAnimationPose> boneTable = new GfxAnimationPose();
	{
		for (int i = 0; i < renderVertexCount; ++i)
		{
			vertexSkinIndex.push_back(std::vector<UINT>());
			vertexSkinWeight.push_back(std::vector<float>());
		}
		auto skinDeformerCount = mesh->GetDeformerCount(FbxDeformer::EDeformerType::eSkin);
		if (skinDeformerCount > 0)
		{
			for (int i = 0; i < skinDeformerCount; ++i)
			{
				FbxSkin* skin = (FbxSkin*)mesh->GetDeformer(i, FbxDeformer::EDeformerType::eSkin);
				auto clusterCount = skin->GetClusterCount();
				for (int clusterIndex = 0; clusterIndex < clusterCount; ++clusterIndex)
				{
					FbxCluster* cluster = skin->GetCluster(clusterIndex);
					FbxNode* link = cluster->GetLink();
					auto boneName = link->GetName();
					auto lclR = link->LclRotation.Get();
					auto lclT = link->LclTranslation.Get();
					auto lclS = link->LclScaling.Get();
					FbxAMatrix linkTransformMatrix, transformMatrix;
					linkTransformMatrix = cluster->GetTransformLinkMatrix(linkTransformMatrix);
					auto linkrot = linkTransformMatrix.GetR();
					auto linktran = linkTransformMatrix.GetT();
					auto linkscale = linkTransformMatrix.GetS();
					transformMatrix = cluster->GetTransformMatrix(transformMatrix);
					auto rot = transformMatrix.GetR();
					auto tran = transformMatrix.GetT();
					auto scale = transformMatrix.GetS();
					int controlPointCount = cluster->GetControlPointIndicesCount();
					auto indices = cluster->GetControlPointIndices();
					auto weights = cluster->GetControlPointWeights();
					for (int ctrlPointIt = 0; ctrlPointIt < controlPointCount; ++ctrlPointIt)
					{
						auto polyVertexIndices = ControlIndicesMapPolyIndices[indices[ctrlPointIt]];
						auto weight = weights[ctrlPointIt];
						for (int polyVertexIndicesIt = 0; polyVertexIndicesIt < polyVertexIndices.size(); ++polyVertexIndicesIt)
						{
							auto polyVertexIndex = polyVertexIndices[polyVertexIndicesIt];
							bool isExist = false;
							for (int skinIndexIt = 0; skinIndexIt < vertexSkinIndex[remapIndex[polyVertexIndex]].size(); ++skinIndexIt)
							{
								if (vertexSkinIndex[remapIndex[polyVertexIndex]][skinIndexIt] == clusterIndex)
								{
									isExist = true;
									break;
								}
							}
							if (!isExist)
							{
								vertexSkinIndex[remapIndex[polyVertexIndex]].push_back(clusterIndex);
								vertexSkinWeight[remapIndex[polyVertexIndex]].push_back((float)weight);
							}
						}
					}

					FbxNodeAttribute* attr = link->GetNodeAttribute();
					FbxNodeAttribute::EType attType = attr->GetAttributeType();
					if (attType != FbxNodeAttribute::eSkeleton)
						continue;
					FbxSkeleton* fbxBone = (FbxSkeleton*)attr;

					GfxBoneDesc* boneDesc = new GfxBoneDesc();
					boneDesc->SetName(boneName);
					auto mat = transformMatrix.Inverse() * linkTransformMatrix;
					auto scaleT = mat.GetT()* mAssetImportOption->Scale;
					mat.SetT(scaleT);
					auto v3dMat = FbxDataConverter::ConvertMatrix(mat);
					boneDesc->SetBindMatrix(&v3dMat);
					if (fbxBone->GetSkeletonType() != FbxSkeleton::eRoot)
					{
						FbxNode* parentNode = link->GetParent();
						FbxNodeAttribute* parentAttr = parentNode->GetNodeAttribute();
						if (parentAttr != NULL && parentAttr->GetAttributeType() == FbxNodeAttribute::eSkeleton)
						{
							boneDesc->SetParent(parentNode->GetName());
						}
					}
					auto newBone = boneTable->NewBone(boneDesc);
					BoneCluster boneCluster;
					boneCluster.Bone = newBone;
					boneCluster.FBXCluster = cluster;
					BoneClusters.push_back(boneCluster);
					if (fbxBone->GetSkeletonType() == FbxSkeleton::eRoot)
					{
						boneTable->SetRoot(boneName);
					}
				}
				if (clusterCount > 0)
				{
					FbxCluster* cluster = skin->GetCluster(0);
					FbxNode* link = cluster->GetLink();
					auto root = GetSkeletonRootNode(link);
					RecursionCalculateBone(root, boneTable, BoneClusters);
					boneTable->SetRoot(root->GetName());
				}

			}

			boneTable->GenerateHierarchy();
			ReCalculateBoneBindMatrix(boneTable->GetRoot(), NULL, boneTable);
		}
	}

	GfxSkeleton* skeleton = new GfxSkeleton();
	skeleton->SetSkeletonBoneTable(boneTable);
	auto skinModifier = new GfxSkinModifier();
	skinModifier->SetSkeleton(skeleton);
	mMeshPrimitives->GetMdfQueue()->AddModifier(skinModifier);

	BYTE* skinIndexsStream = new BYTE[4 * renderVertexCount];
	float* skinWeightsStream = new float[4 * renderVertexCount];
	//build skin stream
	for (int i = 0; i < renderVertexCount; ++i)
	{
		auto size = vertexSkinIndex[i].size();
		for (int j = 0; j < 4; ++j)
		{
			if (j < size)
			{
				skinIndexsStream[i * 4 + j] = vertexSkinIndex[i][j];
				skinWeightsStream[i * 4 + j] = vertexSkinWeight[i][j];
			}
			else
			{
				skinIndexsStream[i * 4 + j] = 0;
				skinWeightsStream[i * 4 + j] = 0;
			}
		}
	}

	//build stream
	v3dxVector3* posStream = new v3dxVector3[renderVertexCount];
	v3dxVector3* normalStream = new v3dxVector3[renderVertexCount];
	v3dxVector3* tangentStream = new v3dxVector3[renderVertexCount];
	v3dxVector2* uvStream = new v3dxVector2[renderVertexCount];
	v3dxVector2* lightMapStream = new v3dxVector2[renderVertexCount];
	UINT16* renderIndex16 = NULL;
	UINT* renderIndex32 = NULL;
	bool isIndex32 = false;

	//copy
	{
		if (polyVertexCount > 65535)
		{
			isIndex32 = true;
			renderIndex32 = vertexIndex;
		}
		else
		{
			renderIndex16 = new UINT16[polyVertexCount];
			for (int i = 0; i < polyVertexCount; ++i)
			{
				renderIndex16[i] = (UINT16)vertexIndex[i];
			}
		}
		for (int i = 0; i < renderVertexCount; ++i)
		{
			posStream[i] = renderVertexs[i].Postion;
			normalStream[i] = renderVertexs[i].Normal;
			tangentStream[i] = renderVertexs[i].Tangent;
			uvStream[i] = renderVertexs[i].UV;
			lightMapStream[i] = renderVertexs[i].UV2;
		}
	}

	//set stream
	{

		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_Position, posStream, (UINT)((UINT) sizeof(v3dxVector3) *renderVertexCount), sizeof(v3dxVector3), 0);

		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_Normal, normalStream, (UINT)((UINT) sizeof(v3dxVector3) *renderVertexCount), sizeof(v3dxVector3), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_UV, uvStream, (UINT)((UINT) sizeof(v3dxVector2) * renderVertexCount), sizeof(v3dxVector2), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_LightMap, lightMapStream, (UINT)((UINT) sizeof(v3dxVector2) * renderVertexCount), sizeof(v3dxVector2), 0);
		if (isIndex32)
		{
			mMeshPrimitives->SetGeomtryMeshIndex(rc, renderIndex32, polyVertexCount * sizeof(UINT), EIndexBufferType::IBT_Int32, 0);
		}
		else
		{
			mMeshPrimitives->SetGeomtryMeshIndex(rc, renderIndex16, polyVertexCount * sizeof(UINT16), EIndexBufferType::IBT_Int16, 0);
		}

		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_SkinIndex, skinIndexsStream, 4 * renderVertexCount * sizeof(BYTE), 4 * sizeof(BYTE), 0);
		mMeshPrimitives->SetGeomtryMeshStream(rc, EVertexSteamType::VST_SkinWeight, skinWeightsStream, 4 * renderVertexCount * sizeof(float), 4 * sizeof(float), 0);
	}

	//build aabb
	{
		v3dxBox3 aabb;
		for (int i = 0; i < controlPointsCount; i++)
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
			delete[] vertexIndex;
		}
		if (renderIndex32)
		{
			// renderIndex32 is vertexIndex
			delete[] renderIndex32;
		}
		delete[] pControlPoints;
		delete[] pAssetVertexs;
	}

	//final
	DrawPrimitiveDesc desc;
	desc.PrimitiveType = EPT_TriangleList;
	desc.BaseVertexIndex = 0;
	desc.StartIndex = 0;
	desc.NumInstances = 1;
	desc.NumPrimitives = polyCount;
	mMeshPrimitives->PushAtomLOD(0, &desc);
}



v3dxMatrix4 GfxAsset_MeshCreater::ComputeTotalMatrix(FbxNode* Node, FbxScene* scene)
{
	v3dxMatrix4 result;
	auto rootNode = scene->GetRootNode();
	FbxAMatrix& axisTransform = rootNode->EvaluateGlobalTransform();
	//FbxVector4 rootTranslation, rootRotation, rootScaling;
	auto locTranslation = Node->LclTranslation.Get();
	auto locRotation = Node->LclRotation.Get();
	auto locScaling = Node->LclScaling.Get();

	auto gMatrix = CalculateGlobalTransform(Node);

	FbxAMatrix locMatrix;
	locMatrix.SetT(locTranslation);
	locMatrix.SetR(locRotation);
	locMatrix.SetS(locScaling);
	auto option = (GfxMeshImportOption*)mAssetImportOption;
	FbxAMatrix Geometry;
	FbxVector4 Translation, Rotation, Scaling;
	Translation = Node->GetGeometricTranslation(FbxNode::eSourcePivot);
	Rotation = Node->GetGeometricRotation(FbxNode::eSourcePivot);
	Scaling = Node->GetGeometricScaling(FbxNode::eSourcePivot);
	Geometry.SetT(Translation);
	Geometry.SetR(Rotation);
	Geometry.SetS(Scaling);
	Geometry = Geometry;
	//For Single Matrix situation, obtain transfrom matrix from eDESTINATION_SET, which include pivot offsets and pre/post rotations.
	//FbxAMatrix& GlobalTransform = scene->GetAnimationEvaluator()->GetNodeGlobalTransform(Node);
	FbxAMatrix& GlobalTransform = Node->EvaluateGlobalTransform();
	//We can bake the pivot only if we don't transform the vertex to the absolute position
	if (!option->TransformVertexToAbsolute)
	{
		if (option->BakePivotInVertex)
		{
			FbxAMatrix PivotGeometry;
			FbxVector4 RotationPivot = Node->GetRotationPivot(FbxNode::eSourcePivot);
			FbxVector4 FullPivot;
			FullPivot[0] = -RotationPivot[0];
			FullPivot[1] = -RotationPivot[1];
			FullPivot[2] = -RotationPivot[2];
			PivotGeometry.SetT(FullPivot);
			Geometry = Geometry * PivotGeometry;
		}
		else
		{
			//No Vertex transform and no bake pivot, it will be the mesh as-is.
			Geometry.SetIdentity();
		}
	}
	//We must always add the geometric transform. Only Max use the geometric transform which is an offset to the local transform of the node
	//FbxAMatrix TotalMatrix = option->TransformVertexToAbsolute ? GlobalTransform * Geometry : Geometry;
	FbxAMatrix TotalMatrix;
	if (!option->TransformVertexToAbsolute)
	{
		TotalMatrix = Geometry;
	}
	else
	{
		TotalMatrix = Geometry * gMatrix;
	}
	result = FbxDataConverter::ConvertMatrix(TotalMatrix);
	return result;
}


NS_END

using namespace EngineNS;
extern "C"
{
	CSharpAPI1(EngineNS, GfxAsset_MeshCreater, SetMeshPrimitives, GfxMeshPrimitives*);
}