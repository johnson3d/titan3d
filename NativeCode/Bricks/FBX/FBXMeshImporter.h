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
		struct MeshVertex
		{
			v3dxVector3 Postion;
			v3dxVector3 Normal;
			v3dxVector3 Tangent;
			v3dVector4_t TangentChirality;
			v3dxVector3 BinNormal;
			v3dxColor4 Color;
			v3dxVector2 UV;
			v3dxVector2 UV2;

			bool IsTangentValid;
			bool IsNormalValid;
			bool IsBinNormalValid;
		};
		struct TriangleFace
		{
			//AssetVertex* Vertexs[3];
			UINT Indices[3];
			v3dxVector3 FaceTangent;
			v3dxVector3 FaceNormal;
			v3dxVector3 FaceBiNormal;

			bool IsVertexTBNValid;
		};
		/** Helper struct for building acceleration structures. */
		struct IndexAndZ
		{
			float Z;
			int Index;
			const v3dxVector3* OriginalVector;
			/** Default constructor. */
			IndexAndZ() {}

			/** Initialization constructor. */
			IndexAndZ(int InIndex, v3dxVector3 V)
			{
				Z = 0.30f * V.x + 0.33f * V.y + 0.37f * V.z;
				Index = InIndex;
				OriginalVector = &V;
			}
		};

		/** Sorting function for vertex Z/index pairs. */
		struct CompareIndexAndZ
		{
			FORCEINLINE bool operator()(IndexAndZ const& A, IndexAndZ const& B) const { return A.Z < B.Z; }
		};
		/**
		* Returns true if the specified points are about equal
		*/
		inline bool PointsEqual(const v3dxVector3& V1, const v3dxVector3& V2, float ComparisonThreshold)
		{
			if (Math::Abs(V1.x - V2.x) > ComparisonThreshold
				|| Math::Abs(V1.y - V2.y) > ComparisonThreshold
				|| Math::Abs(V1.z - V2.z) > ComparisonThreshold)
			{
				return false;
			}
			return true;
		}
		enum { INDEX_NONE = -1 };
		struct OverlappingVertexs
		{
			OverlappingVertexs() {}

			OverlappingVertexs(const std::vector<v3dxVector3>& InVertices, const std::vector<UINT>& InIndices, float ComparisonThreshold);

			/* Resets, pre-allocates memory, marks all indices as not overlapping in preperation for calls to Add() */
			void Init(int NumIndices);

			/* Add overlapping indices pair */
			void Add(int Key, int Value);

			/* Sorts arrays, converts sets to arrays for sorting and to allow simple iterating code, prevents additional adding */
			void FinishAdding();

			/* Estimate memory allocated */
			//UINT GetAllocatedSize(void) const;

			/**
			* @return array of sorted overlapping indices including input 'Key', empty array for indices that have no overlaps.
			*/
			const std::vector<int>& FindIfOverlapping(int Key) const
			{
				int ContainerIndex = IndexBelongsTo[Key];
				return (ContainerIndex != INDEX_NONE) ? Arrays[ContainerIndex] : EmptyArray;
			}

			std::vector< std::vector <int> > Arrays;
		private:
			std::vector<int> IndexBelongsTo;
			std::vector< std::vector<int> > Sets;
			std::vector<int> EmptyArray;
			bool bFinishedAdding = false;
		};

		class BoneCluster
		{
			public:
				IBone* Bone;
				FbxCluster* FBXCluster;
		};


		class TR_CLASS(SV_Dispose = delete self)
		FBXMeshImporter
		{
			public:
				FBXMeshImporter(FBXImporter* hostFBXImporter, UINT meshIndex);
				~FBXMeshImporter();
			public:
				
				EngineNS::IMeshPrimitives* GetMeshPrimitives() 
				{ 
					if (HasProcessed)
					{
						return mMeshPrimitives;
					}
					return nullptr;
				};
				EngineNS::IPartialSkeleton* GetPartialSkeleton()
				{
					if (HasProcessed)
					{
						return mPartialSkeleton;
					}
					return nullptr;
				};
				EFBXImportResult Process(EngineNS::IRenderContext* rc);
				//v3dxMatrix4 ComputeTotalMatrix(FbxNode* Node, FbxScene* scene);
				void RecursionCalculateBone(FbxNode* boneNode, const std::vector<BoneCluster>& boneClusters, EngineNS::IPartialSkeleton* skeleton);
			protected:
				EngineNS::IMeshPrimitives* mMeshPrimitives;
				EngineNS::IPartialSkeleton* mPartialSkeleton;
				//EngineNS::ISkinModifier* mSkinModifier;
			protected:
				UINT mMeshIndex = -1;
				FBXImporter* mHostFBXImporter;
				bool HasProcessed = false;
		};

		class SingleMaterialMesh
		{
		public:
			EFBXImportResult Process(const FBXFileImportDesc* fileImportOption, const FBXMeshImportDesc* meshImportOption);
			std::vector <TriangleFace> Faces;
			std::vector<MeshVertex> Vertexs;
			std::vector<UINT32> VertexIndices;
			bool IsIndex32() { return IndexCount() > 65535 ? true : false; }
			int IndexCount() { return (int)VertexIndices.size(); }
			int VertexCount() { return (int)Vertexs.size(); }
			int PolyCount;
			std::vector<std::vector<UINT>> VertexSkinIndex;
			std::vector<std::vector<float>> VertexSkinWeight;
			std::vector<int> PolyIndices;
			void MergeTo(SingleMaterialMesh* submesh);
		};

		template<class T>
		T GetLayerElementValue(FbxLayerElementTemplate<T>* layerElement, int index)
		{
			auto lMappingMode = layerElement->GetMappingMode();
			int tempIndex = index;
			if (layerElement->GetReferenceMode() == FbxLayerElement::eIndexToDirect)
			{
				tempIndex = layerElement->GetIndexArray().GetAt(index);
			}
			return layerElement->GetDirectArray().GetAt(tempIndex);

		}
	}
}

