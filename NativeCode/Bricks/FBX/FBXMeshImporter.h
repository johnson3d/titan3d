#pragma once
#include "FBX.h"
#include "fbxsdk.h"
#include "../../NextRHI/NxRHI.h"
#include "FBXImporter.h"
#include "../../Math/v3dxColor4.h"
#include "../../Graphics/Mesh/Modifier/ISkinModifier.h"
#include "FBXPartialSkeleton.h"

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
				Z = 0.30f * V.X + 0.33f * V.Y + 0.37f * V.Z;
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
			if (Math::Abs(V1.X - V2.X) > ComparisonThreshold
				|| Math::Abs(V1.Y - V2.Y) > ComparisonThreshold
				|| Math::Abs(V1.Z - V2.Z) > ComparisonThreshold)
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
				FBXBoneDesc Bone;
				FbxCluster* FBXCluster = nullptr;
		};


		class TR_CLASS(SV_Dispose = delete self)
		FBXMeshImporter
		{
			public:
				FBXMeshImporter(FBXImporter* hostFBXImporter, UINT meshIndex);
				~FBXMeshImporter();
			public:
				
				NxRHI::FMeshPrimitives* GetMeshPrimitives() 
				{ 
					if (HasProcessed)
					{
						return mMeshPrimitives;
					}
					return nullptr;
				};
				FBXSkeletonDesc* GetSkeletonDesc()
				{
					if (HasProcessed)
					{
						return mSkeletonDesc;
					}
					return nullptr;
				};
				EFBXImportResult Process(NxRHI::IGpuDevice* cmd);
				//v3dxMatrix4 ComputeTotalMatrix(FbxNode* Node, FbxScene* scene);
				void RecursionCalculateBone(FbxNode* boneNode, FBXSkeletonDesc* skeleton);
			protected:
				NxRHI::FMeshPrimitives* mMeshPrimitives = nullptr;
				FBXSkeletonDesc* mSkeletonDesc = nullptr;
				//EngineNS::ISkinModifier* mSkinModifier;
			protected:
				UINT mMeshIndex = -1;
				FBXImporter* mHostFBXImporter = nullptr;
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
			int PolyCount = 0;
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

