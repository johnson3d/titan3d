#include "GfxAsset_SubMesh.h"
#include "GfxFBXManager.h"
#include "FbxDataConverter.h"
#include "../../Bricks/Animation/Pose/GfxAnimationPose.h"
#include "../../Bricks/Animation/Skeleton/GfxSkeleton.h"
#include "../../Graphics/Mesh/GfxSkinModifier.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "../../Graphics/Mesh/GfxMdfQueue.h"
#include <fbxsdk.h>
#include "GfxAssetImportOption.h"
#include "GfxFileImportOption.h"
#include "GfxAssetCreater.h"
#define new VNEW
NS_BEGIN
const int TRIANGLE_VERTEX_COUNT = 3;

// Four floats for every position.
const int VERTEX_STRIDE = 3;
// Three floats for every normal.
const int NORMAL_STRIDE = 3;
// Two floats for every UV.
const int UV_STRIDE = 2;

bool IsVectorValid(v3dxVector3& val)
{
	if (isnan(val.x) || isnan(val.y) || isnan(val.z))
	{
		return false;
	}
	if (val.isZeroLength())
	{
		return false;
	}
	return true;
}
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
FbxMatrix GetBindMatrix(FbxNode* boneNode)
{
	return FbxMatrix();
}
v3dxMatrix4 ComputeTotalMatrix(FbxNode* Node, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxMeshImportOption* option)
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
	FbxAMatrix Geometry;
	FbxVector4 Translation, Rotation, Scaling;
	Translation = Node->GetGeometricTranslation(FbxNode::eSourcePivot);
	Rotation = Node->GetGeometricRotation(FbxNode::eSourcePivot);
	Scaling = Node->GetGeometricScaling(FbxNode::eSourcePivot);
	Geometry.SetT(Translation * fileImportOption->mScaleFactor * option->Scale);
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

	auto scale = fileImportOption->mScaleFactor * option->Scale;
	FbxAMatrix scaleMatrix;
	FbxDouble3 scaleFactor(scale, scale, scale);

	scaleMatrix.SetS(scaleFactor);
	FbxAMatrix TotalMatrix;
	if (!option->TransformVertexToAbsolute)
	{
		TotalMatrix = Geometry * scaleMatrix;
	}
	else
	{
		TotalMatrix = Geometry * gMatrix * scaleMatrix;
	}
	result = FbxDataConverter::ConvertMatrix(TotalMatrix);
	return result;
}

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

GfxAsset_SubMesh::GfxAsset_SubMesh()
{
}

GfxAsset_SubMesh::~GfxAsset_SubMesh()
{
}

void GfxAsset_SubMesh::Process(IRenderContext* rc, FbxScene* scene, GfxFileImportOption* fileImportOption, GfxMeshImportOption* meshImportOption, GfxFBXManager* manager)
{
	if (!meshImportOption->IsImport)
		return;
	auto GeometryConverter = new FbxGeometryConverter(manager->GetSDKManager());
	FbxNode* node = meshImportOption->FBXNode;
	auto att = node->GetNodeAttribute();
	if (att->GetAttributeType() != IAT_Mesh)
		return;
	auto mesh = (FbxMesh*)att;
	auto controlPointsCount = mesh->GetControlPointsCount();
	auto controlPoints = mesh->GetControlPoints();
	auto polyCount = PolyIndices.size();
	//auto polyVertexCount = mesh->GetPolygonVertexCount();
	auto polyVertexCount = polyCount * 3;

	v3dxMatrix4 TotalMatrix;
	v3dxMatrix4 TotalMatrixForNormal;
	TotalMatrix = ComputeTotalMatrix(node, scene, fileImportOption, meshImportOption);
	TotalMatrixForNormal = TotalMatrix;//.inverse();
	TotalMatrixForNormal.setTrans(v3dxVector3::ZERO);
	//TotalMatrixForNormal.transPose();
	//v3dxVector3* pControlPoints = new v3dxVector3[controlPointsCount];
	AssetVertex* pAssetVertexs = new AssetVertex[polyVertexCount];
	AssetCreater->OnImportMessageDumping(AMT_Import, 1, "build vertex for per poly");
	bool needCalculateTangent = false;
	//build vertex for per poly
	{
		//Position 
		{
			int vertexCount = 0;
			for (int i = 0; i < polyCount; ++i)
			{
				for (int j = 0; j < VERTEX_STRIDE; j++)
				{
					auto polyIndex = PolyIndices[i];
					int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
					auto pos = FbxDataConverter::ConvertPos(controlPoints[ctrlPointIndex]);
					pos = pos * TotalMatrix;
					AssetVertex aVertex;
					aVertex.Postion = pos /** meshImportOption->Scale*/;
					aVertex.Normal = v3dxVector3::ZERO;
					aVertex.Tangent = v3dxVector3::ZERO;
					aVertex.BinNormal = v3dxVector3::ZERO;
					aVertex.Color = v3dxColor4::fromArgb(0, 0, 0, 0);
					aVertex.IsNormalValid = FALSE;
					aVertex.IsTangentValid = FALSE;
					aVertex.IsBinNormalValid = FALSE;

					pAssetVertexs[vertexCount] = aVertex;
					vertexCount++;
				}
			}

		}
		//VertexColor
		{
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
			if (hasVertexColor)
			{
				auto lVertexColorMappingMode = mesh->GetElementVertexColor(0)->GetMappingMode();
				const FbxGeometryElementVertexColor * pVertexColorElement = mesh->GetElementVertexColor(0);;
				FbxColor currentVertexColor;
				v3dxColor4 vertexColor;
				if (lVertexColorMappingMode == FbxGeometryElement::eByControlPoint)
				{
					//v3dxColor4* pControlPointVertexColors = new v3dxColor4[controlPointsCount];
					//for (int i = 0; i < controlPointsCount; ++i)
					//{
					//	int vertexColorIndex = i;
					//	if (pVertexColorElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
					//	{
					//		vertexColorIndex = pVertexColorElement->GetIndexArray().GetAt(i);
					//	}
					//	currentVertexColor = pVertexColorElement->GetDirectArray().GetAt(vertexColorIndex);
					//	vertexColor = FbxDataConverter::ConvertColor(currentVertexColor);
					//	pControlPointVertexColors[i] = vertexColor;
					//}
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							auto polyIndex = PolyIndices[i];
							int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
							currentVertexColor = GetLayerElementValue<FbxColor>((FbxLayerElementTemplate<FbxColor>*)pVertexColorElement, ctrlPointIndex);
							vertexColor = FbxDataConverter::ConvertColor(currentVertexColor);
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							vertex.Color = vertexColor;
							vertexCount++;
						}
					}
					//delete[] pControlPointVertexColors;
				}
				else
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							auto polyIndex = PolyIndices[i];
							auto vertexIndex = polyIndex * VERTEX_STRIDE + j;
							/*int colorIndex = polyIndex;
							if (pVertexColorElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
							{
								colorIndex = pVertexColorElement->GetIndexArray().GetAt(polyIndex);
							}
							currentVertexColor = pVertexColorElement->GetDirectArray().GetAt(colorIndex);*/
							currentVertexColor = GetLayerElementValue<FbxColor>((FbxLayerElementTemplate<FbxColor>*)pVertexColorElement, vertexIndex);
							vertexColor = FbxDataConverter::ConvertColor(currentVertexColor);
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							vertex.Color = vertexColor;
							vertexCount++;
						}
					}
				}

			}
			else
			{
				AssetCreater->OnImportMessageDumping(AMT_Warning, 0, "Don't have VertexColor!");
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
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							auto polyIndex = PolyIndices[i];
							int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
							currentNormal = GetLayerElementValue<FbxVector4>((FbxLayerElementTemplate<FbxVector4>*)pNormalElement, ctrlPointIndex);
							normal = FbxDataConverter::ConvertDir(currentNormal);
							normal = normal * TotalMatrixForNormal;
							normal.normalize();
							if (!IsVectorValid(normal))
							{
								vertex.IsNormalValid = FALSE;
							}
							else
							{
								vertex.Normal = normal;
								vertex.IsNormalValid = TRUE;
							}
							vertexCount++;
						}
					}
				}
				else
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							auto polyIndex = PolyIndices[i];
							int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
							bool result = mesh->GetPolygonVertexNormal(polyIndex, j, currentNormal);
							normal = FbxDataConverter::ConvertDir(currentNormal);
							normal = normal * TotalMatrixForNormal;
							normal.normalize();
							if (!IsVectorValid(normal))
							{
								vertex.IsNormalValid = FALSE;
							}
							else
							{
								vertex.Normal = normal;
								vertex.IsNormalValid = TRUE;
							}
							vertexCount++;
						}
					}
				}

			}
			else
			{
				AssetCreater->OnImportMessageDumping(AMT_Warning, 0, "Don't have Normal!");
			}
		}

		//Tangent
		if (!meshImportOption->ReCalculateTangent)
		{
			bool hasTangent = true;
			if (mesh->GetElementTangentCount() > 0)
			{
				auto lTangentMappingMode = mesh->GetElementTangent(0)->GetMappingMode();
				if (lTangentMappingMode == FbxGeometryElement::eNone)
				{
					//Don't have Tangent
					hasTangent = false;
					needCalculateTangent = true;
				}
			}
			else
			{
				//Don't have Tangent
				hasTangent = false;
				needCalculateTangent = true;
			}
			if (hasTangent)
			{
				auto lTangentMappingMode = mesh->GetElementTangent(0)->GetMappingMode();
				const FbxGeometryElementTangent * pTangentElement = mesh->GetElementTangent(0);;
				FbxVector4 currentTangent;
				v3dxVector3 tangent;
				if (lTangentMappingMode == FbxGeometryElement::eByControlPoint)
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							auto polyIndex = PolyIndices[i];
							int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
							currentTangent = GetLayerElementValue<FbxVector4>((FbxLayerElementTemplate<FbxVector4>*)pTangentElement, ctrlPointIndex);
							tangent = FbxDataConverter::ConvertDir(currentTangent);
							tangent = tangent * TotalMatrixForNormal;
							tangent.normalize();
							if (tangent == vertex.Normal)
							{
								ASSERT(FALSE);
							}
							if (!IsVectorValid(tangent))
							{
								vertex.IsTangentValid = FALSE;
							}
							else if (tangent.dotProduct(vertex.Normal) == 1 || tangent.dotProduct(vertex.Normal) == -1)
							{
								vertex.IsTangentValid = FALSE;
							}
							else
							{
								vertex.Tangent = tangent;
								vertex.IsTangentValid = TRUE;
							}
							vertexCount++;
						}
					}
					//delete[] pControlPointTangents;
				}
				else
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							auto polyIndex = PolyIndices[i];
							auto vertexIndex = polyIndex * VERTEX_STRIDE + j;
							currentTangent = GetLayerElementValue<FbxVector4>((FbxLayerElementTemplate<FbxVector4>*)pTangentElement, vertexIndex);
							tangent = FbxDataConverter::ConvertDir(currentTangent);
							tangent = tangent * TotalMatrixForNormal;
							tangent.normalize();

							if (!IsVectorValid(tangent))
							{
								vertex.IsTangentValid = FALSE;
							}
							else if (tangent.dotProduct(vertex.Normal) == 1 || tangent.dotProduct(vertex.Normal) == -1)
							{
								vertex.IsTangentValid = FALSE;
							}
							else
							{
								vertex.Tangent = tangent;
								vertex.IsTangentValid = TRUE;
							}
							vertexCount++;
						}
					}
				}

			}
			else
			{
				AssetCreater->OnImportMessageDumping(AMT_Warning, 0, "Don't have Tangent,Will be Calculate!", 0.0f);
			}
		}

		//BinTangent or BinNormal
		if (!meshImportOption->ReCalculateTangent)
		{
			bool hasBinNormal = true;
			if (mesh->GetElementBinormalCount() > 0)
			{
				auto lBinNormalMappingMode = mesh->GetElementBinormal(0)->GetMappingMode();
				if (lBinNormalMappingMode == FbxGeometryElement::eNone)
				{
					//Don't have Tangent
					hasBinNormal = false;
					needCalculateTangent = true;
				}
			}
			else
			{
				//Don't have Tangent
				hasBinNormal = false;
				needCalculateTangent = true;
			}
			if (hasBinNormal)
			{
				auto lBinNormalMappingMode = mesh->GetElementBinormal(0)->GetMappingMode();
				const FbxGeometryElementBinormal * pBinNormalElement = mesh->GetElementBinormal(0);;
				FbxVector4 currentBinNormal;
				v3dxVector3 binNormal;
				if (lBinNormalMappingMode == FbxGeometryElement::eByControlPoint)
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							auto polyIndex = PolyIndices[i];
							int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
							currentBinNormal = GetLayerElementValue<FbxVector4>((FbxLayerElementTemplate<FbxVector4>*)pBinNormalElement, ctrlPointIndex);
							binNormal = FbxDataConverter::ConvertDir(currentBinNormal);
							binNormal = binNormal * TotalMatrixForNormal;
							binNormal.normalize();
							if (!IsVectorValid(binNormal))
							{
								vertex.IsBinNormalValid = FALSE;
							}
							else
							{
								vertex.BinNormal = binNormal;
								vertex.IsBinNormalValid = TRUE;
							}
							vertexCount++;
						}
					}
				}
				else
				{
					int vertexCount = 0;
					for (int i = 0; i < polyCount; ++i)
					{
						for (int j = 0; j < VERTEX_STRIDE; j++)
						{
							AssetVertex& vertex = pAssetVertexs[vertexCount];
							auto polyIndex = PolyIndices[i];
							auto vertexIndex = polyIndex * VERTEX_STRIDE + j;
							currentBinNormal = GetLayerElementValue<FbxVector4>((FbxLayerElementTemplate<FbxVector4>*)pBinNormalElement, vertexIndex);
							binNormal = FbxDataConverter::ConvertDir(currentBinNormal);
							binNormal = binNormal * TotalMatrixForNormal;
							binNormal.normalize();

							if (!IsVectorValid(binNormal))
							{
								vertex.IsBinNormalValid = FALSE;
							}
							else
							{
								vertex.BinNormal = binNormal;
								vertex.IsBinNormalValid = TRUE;
							}
							vertexCount++;
						}
					}
				}

			}
			else
			{
				AssetCreater->OnImportMessageDumping(AMT_Warning, 0, "Don't have BinNormal,Will be Calculate!");
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
				//Don't have UV
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
					const FbxGeometryElementUV * pUVElement = mesh->GetElementUV(uvIndex);
					FbxVector2 currentUV;
					v3dxVector2 UV;
					if (lUVMappingMode == FbxGeometryElement::eByControlPoint)
					{
						/*v3dxVector2* pControlPointUVs = new v3dxVector2[controlPointsCount];
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
						}*/
						int vertexCount = 0;
						for (int i = 0; i < polyCount; ++i)
						{
							for (int j = 0; j < VERTEX_STRIDE; j++)
							{
								auto polyIndex = PolyIndices[i];
								int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
								currentUV = GetLayerElementValue<FbxVector2>((FbxLayerElementTemplate<FbxVector2>*)pUVElement, ctrlPointIndex);
								UV = FbxDataConverter::ConvertUV(currentUV);
								AssetVertex& vertex = pAssetVertexs[vertexCount];
								if (uvIndex == 0)
									vertex.UV = UV;
								else
									vertex.UV2 = UV;
								vertexCount++;
							}
						}
						//delete[] pControlPointUVs;

					}
					else if (lUVMappingMode == FbxGeometryElement::eByPolygonVertex)
					{
						int vertexCount = 0;
						for (int i = 0; i < polyCount; ++i)
						{
							for (int j = 0; j < VERTEX_STRIDE; j++)
							{
								bool lUnmappedUV;
								auto polyIndex = PolyIndices[i];
								bool result = mesh->GetPolygonVertexUV(polyIndex, j, lUVName, currentUV, lUnmappedUV);
								UV = FbxDataConverter::ConvertUV(currentUV);
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
			else
			{
				AssetCreater->OnImportMessageDumping(AMT_Error, 0, "Don't have UV!");
			}
		}
	}

	//ControlIndicesMapPolyIndices
	std::map<int, std::vector<int>> ControlIndicesMapPolyIndices;
	{
		int vertexCount = 0;
		for (int i = 0; i < polyCount; ++i)
		{
			for (int j = 0; j < VERTEX_STRIDE; j++)
			{
				auto polyIndex = PolyIndices[i];
				int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
				ControlIndicesMapPolyIndices[ctrlPointIndex].push_back(vertexCount);
				++vertexCount;
			}
		}
	}

	AssetCreater->OnImportMessageDumping(AMT_Import, 1, "build overlaps for accelerate");
	//build overlaps for accelerate
	OverlappingVertexs overlappingVertexs;
	{
		overlappingVertexs.Init((int)polyVertexCount);
		float ComparisonThreshold = 0.00001f;
		std::vector<IndexAndZ> VertIndexAndZ;
		VertIndexAndZ.resize(polyVertexCount);
		int vertexCount = 0;
		for (int i = 0; i < polyCount; ++i)
		{
			for (int j = 0; j < VERTEX_STRIDE; j++)
			{
				//auto polyIndex = PolyIndices[i];
				//int ctrlPointIndex = mesh->GetPolygonVertex(polyIndex, j);
				VertIndexAndZ[vertexCount] = (IndexAndZ(vertexCount, pAssetVertexs[vertexCount].Postion));
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
	AssetCreater->OnImportMessageDumping(AMT_Import, 1, "vertex optimize");
	std::vector<int> remapIndex;
	VertexIndices.resize(polyVertexCount);
	//VertexIndex = new UINT32[polyVertexCount];
	{
		int theVertexIndex = 0;
		for (int i = 0; i < polyCount; ++i)
		{
			Faces.push_back(TriangleFace());
		}
		for (int i = 0; i < polyCount; ++i)
		{
			TriangleFace& face = Faces[i];
			face.IsVertexTBNValid = TRUE;
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
					VertexIndices[i * 3 + 2 - j] = (UINT)Vertexs.size();
					remapIndex.push_back(VertexIndices[i * 3 + 2 - j]);
					Vertexs.push_back(vertex);
					face.Indices[j] = VertexIndices[i * 3 + 2 - j];
				}
				else
				{
					VertexIndices[i * 3 + 2 - j] = remapIndex[findSameVertexIndex];
					remapIndex.push_back(remapIndex[findSameVertexIndex]);
					face.Indices[j] = remapIndex[findSameVertexIndex];
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

	for (int i = 0; i < polyCount; ++i)
	{
		for (int j = 0; j < VERTEX_STRIDE; ++j)
		{
			if (!Vertexs[Faces[i].Indices[j]].IsNormalValid)
			{
				Faces[i].IsVertexTBNValid = FALSE;
				break;
			}
			if (!Vertexs[Faces[i].Indices[j]].IsTangentValid)
			{
				Faces[i].IsVertexTBNValid = FALSE;
				break;
			}
			if (!Vertexs[Faces[i].Indices[j]].IsBinNormalValid)
			{
				Faces[i].IsVertexTBNValid = FALSE;
				break;
			}
		}
	}

	AssetCreater->OnImportMessageDumping(AMT_Import, 1, "CalculateTangent");
	//CalculateTangent
	{
		bool forceCalculateTangent = false;
		if (needCalculateTangent || meshImportOption->ReCalculateTangent)
		{
			forceCalculateTangent = true;
		}
		//face NBT
		for (int i = 0; i < Faces.size(); ++i)
		{
			auto& face = Faces[i];

			v3dxVector3 faceTangent;
			v3dxVector3 faceBin;
			v3dxVector3 faceNormal;

			v3dxVector3 pos[3];
			v3dxVector2 uv[3];

			pos[0] = Vertexs[Faces[i].Indices[2]].Postion;
			pos[1] = Vertexs[Faces[i].Indices[1]].Postion;
			pos[2] = Vertexs[Faces[i].Indices[0]].Postion;
			uv[0] = Vertexs[Faces[i].Indices[2]].UV;
			uv[1] = Vertexs[Faces[i].Indices[1]].UV;
			uv[2] = Vertexs[Faces[i].Indices[0]].UV;
			v3dxVector3  e1 = pos[1] - pos[0];
			v3dxVector3  e2 = pos[2] - pos[0];
			v3dxVector2 u1 = { uv[1].x - uv[0].x , uv[1].y - uv[0].y };
			v3dxVector2 u2 = { uv[2].x - uv[0].x , uv[2].y - uv[0].y };
			faceNormal = (e1 ^ e2);
			if (!IsVectorValid(faceNormal))
			{
				AssetCreater->OnImportMessageDumping(AMT_Warning, 1, "InValid Triangle");
				face.FaceBiNormal = v3dxVector3::UNIT_MINUS_X;
				face.FaceTangent = v3dxVector3::UNIT_MINUS_Y;
				face.FaceNormal = v3dxVector3::UNIT_MINUS_Z;
			}
			else
			{
				faceNormal.normalize();
				float det = (u1.x * u2.y - u2.x * u1.y);
				if (det == 0.0f)
				{
					if (!e1.isZeroLength())
					{
						faceTangent = e1;
					}
					else if (!e2.isZeroLength())
					{
						faceTangent = e2;
					}
					else
					{
						ASSERT(false);
					}
					faceBin = faceNormal.crossProduct(faceTangent);
				}
				else
				{
					float a = u2.y / det;
					float b = -u1.y / det;
					float c = -u2.x / det;
					float d = u1.x / det;
					faceTangent = (e1*a + e2 * b);
					faceBin = (e1*c + e2 * d);
				}
				face.FaceBiNormal = faceBin;
				face.FaceTangent = faceTangent;
				face.FaceNormal = faceNormal;
				if (!IsVectorValid(faceBin))
				{
					ASSERT(FALSE);
				}
				if (!IsVectorValid(faceTangent))
				{
					ASSERT(FALSE);
				}
				if (!IsVectorValid(faceNormal))
				{
					ASSERT(FALSE);
				}
			}
			if (face.IsVertexTBNValid && !forceCalculateTangent)
				continue;
			for (int k = 0; k < VERTEX_STRIDE; ++k)
			{
				if (!Vertexs[Faces[i].Indices[k]].IsNormalValid)
				{
					Vertexs[Faces[i].Indices[k]].Normal += face.FaceNormal;
					if (Vertexs[Faces[i].Indices[k]].Normal == v3dxVector3::ZERO)
						Vertexs[Faces[i].Indices[k]].Normal -= face.FaceNormal;
				}
				if (!Vertexs[Faces[i].Indices[k]].IsTangentValid || forceCalculateTangent)
				{
					Vertexs[Faces[i].Indices[k]].Tangent += face.FaceTangent;
					if (Vertexs[Faces[i].Indices[k]].Tangent == v3dxVector3::ZERO)
						Vertexs[Faces[i].Indices[k]].Tangent -= face.FaceTangent;
				}
				if (!Vertexs[Faces[i].Indices[k]].IsBinNormalValid || forceCalculateTangent)
				{
					Vertexs[Faces[i].Indices[k]].BinNormal += face.FaceBiNormal;
					if (Vertexs[Faces[i].Indices[k]].BinNormal == v3dxVector3::ZERO)
						Vertexs[Faces[i].Indices[k]].BinNormal -= face.FaceBiNormal;
				}
				if (Vertexs[Faces[i].Indices[k]].Normal == Vertexs[Faces[i].Indices[k]].Tangent)
				{
					Vertexs[Faces[i].Indices[k]].IsNormalValid = FALSE;
					Vertexs[Faces[i].Indices[k]].Normal = face.FaceNormal;
				}
				if (Vertexs[Faces[i].Indices[k]].Normal == -Vertexs[Faces[i].Indices[k]].Tangent)
				{
					Vertexs[Faces[i].Indices[k]].IsNormalValid = FALSE;
					Vertexs[Faces[i].Indices[k]].Normal = face.FaceNormal;
				}
			}
			face.IsVertexTBNValid = TRUE;
		}
		for (int i = 0; i < Vertexs.size(); ++i)
		{
			if (!Vertexs[i].IsNormalValid)
			{
				if (!IsVectorValid(Vertexs[i].Normal))
				{
					//ASSERT(FALSE);
					Vertexs[i].Normal = v3dxVector3::UNIT_Y;
				}
				Vertexs[i].Normal.normalize();
				Vertexs[i].IsNormalValid = TRUE;
			}
			if (!Vertexs[i].IsTangentValid)
			{
				if (!IsVectorValid(Vertexs[i].Tangent))
				{
					//ASSERT(FALSE);
					Vertexs[i].Tangent = v3dxVector3::UNIT_X;
				}
				Vertexs[i].Tangent.normalize();
				Vertexs[i].IsTangentValid = TRUE;
			}
			if (!Vertexs[i].IsBinNormalValid)
			{
				if (!IsVectorValid(Vertexs[i].BinNormal))
				{
					//ASSERT(FALSE);
					Vertexs[i].BinNormal = v3dxVector3::UNIT_Z;
				}
				Vertexs[i].BinNormal.normalize();
				Vertexs[i].IsBinNormalValid = TRUE;
			}
			if (Vertexs[i].Normal == Vertexs[i].Tangent)
			{
				Vertexs[i].Tangent = Vertexs[i].Normal.crossProduct(Vertexs[i].BinNormal);
				//ASSERT(FALSE);
			}
			if (Vertexs[i].Normal == -Vertexs[i].Tangent)
			{
				Vertexs[i].Tangent = Vertexs[i].Normal.crossProduct(Vertexs[i].BinNormal);
				//ASSERT(FALSE);
			}
		}
		for (int i = 0; i < Vertexs.size(); ++i)
		{
			if (!IsVectorValid(Vertexs[i].Normal))
			{
				ASSERT(FALSE);
			}
			if (!IsVectorValid(Vertexs[i].Tangent))
			{
				ASSERT(FALSE);
			}
			if (!IsVectorValid(Vertexs[i].BinNormal))
			{
				ASSERT(FALSE);
			}
		}
		//VertexT TangentChirality
		for (int i = 0; i < polyCount; ++i)
		{
			auto& face = Faces[i];
			for (int j = 0; j < VERTEX_STRIDE; ++j)
			{
				AssetVertex& vertex = Vertexs[Faces[i].Indices[j]];
				auto& vertexTC = vertex.TangentChirality;
				auto& vertexN = vertex.Normal;
				auto& vertexT = vertex.Tangent;
				auto& vertexB = vertex.BinNormal;
				if (forceCalculateTangent)
				{
					auto tempVertexT = (vertexT - vertexN * (vertexN.dotProduct(vertexT)));
					tempVertexT.normalize();
					if (!IsVectorValid(tempVertexT))
					{
						ASSERT(FALSE);
					}
					vertexT = tempVertexT;
				}
				if (!IsVectorValid(vertexB))
				{
					//vertexB = face.FaceBiNormal;??
					vertexB = vertexN.crossProduct(vertexT);
				}
				if (!IsVectorValid(vertexT) || !IsVectorValid(vertexB))
				{
					ASSERT(FALSE);
				}

				vertexTC.x = vertexT.x;
				vertexTC.y = vertexT.y;
				vertexTC.z = vertexT.z;
				vertexTC.w = ((vertexN.crossProduct(vertexT)).dotProduct(vertexB) < 0.0f) ? -1.0f : 1.0f;
			}
		}
	}

	if (meshImportOption->HaveSkin)
	{
		AssetCreater->OnImportMessageDumping(AMT_Import, 1, "build Skin");
		//build vertexSkinIndex
		std::vector < BoneCluster> BoneClusters;
		int renderVertexCount = (int)Vertexs.size();
		//std::vector<std::vector<UINT>> vertexSkinIndex;
		//std::vector<std::vector<float>> vertexSkinWeight;
		{
			VertexSkinIndex.resize(renderVertexCount);
			VertexSkinWeight.resize(renderVertexCount);
			/*for (int i = 0; i < renderVertexCount; ++i)
			{
				VertexSkinIndex.push_back(std::vector<UINT>());
				VertexSkinWeight.push_back(std::vector<float>());
			}*/
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
						auto boneName = FbxDataConverter::ConvertToStdString(link->GetName());
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
								for (int skinIndexIt = 0; skinIndexIt < VertexSkinIndex[remapIndex[polyVertexIndex]].size(); ++skinIndexIt)
								{
									if (VertexSkinIndex[remapIndex[polyVertexIndex]][skinIndexIt] == clusterIndex)
									{
										isExist = true;
										break;
									}
								}
								if (!isExist)
								{
									if (VertexSkinIndex[remapIndex[polyVertexIndex]].size() > 4)
									{
										AssetCreater->OnImportMessageDumping(AMT_Warning, 0, "SkinIndex > 4!");
									}
									VertexSkinIndex[remapIndex[polyVertexIndex]].push_back(clusterIndex);
									VertexSkinWeight[remapIndex[polyVertexIndex]].push_back((float)weight);
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < renderVertexCount; ++i)
			{
				if (VertexSkinIndex[i].size() == 0)
				{
					ASSERT(FALSE);
				}
			}
		}
	}
	//IndexCount = polyVertexCount;
	//VertexCount = renderVertexCount;
}

void GfxAsset_SubMesh::MergeTo(GfxAsset_SubMesh* submesh)
{
	int totalVerticesCount = VertexCount() + submesh->VertexCount();
	int indexOffset = submesh->VertexCount();
	int srcIndexCount = (int)submesh->VertexIndices.size();
	std::copy(Vertexs.begin(), Vertexs.end(), std::back_inserter(submesh->Vertexs));
	std::copy(VertexIndices.begin(), VertexIndices.end(), std::back_inserter(submesh->VertexIndices));
	for (int i = srcIndexCount; i < (int)submesh->VertexIndices.size(); ++i)
	{
		//submesh->VertexIndices.push_back(VertexIndices[i] + indexOffset);
		submesh->VertexIndices[i] += indexOffset;
	}
	std::copy(VertexSkinIndex.begin(), VertexSkinIndex.end(), std::back_inserter(submesh->VertexSkinIndex));
	std::copy(VertexSkinWeight.begin(), VertexSkinWeight.end(), std::back_inserter(submesh->VertexSkinWeight));
	//for (int i = 0; i < VertexSkinIndex.size(); ++i)
	//{
	//	submesh->VertexSkinIndex.push_back(VertexSkinIndex[i]);
	//	submesh->VertexSkinWeight.push_back(VertexSkinWeight[i]);
	//}
}
NS_END


