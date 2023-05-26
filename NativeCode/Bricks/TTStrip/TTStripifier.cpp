#include "TTStripifier.h"
#include "TTCluster.h"

namespace TTStrip
{
	bool IsDegenerate(UINT v0, UINT v1, UINT v2)
	{
		if (v0 == v1)
			return true;
		else if (v0 == v2)
			return true;
		else if (v1 == v2)
			return true;
		else
			return false;
	}

	// Make sure v0 < v1.
	void EnforceIndicesOrder(UINT& v0, UINT& v1)
	{
		if (v0 > v1)
		{
			UINT t = v0;
			v0 = v1;
			v1 = t;
		}
	}

	FStripifier::~FStripifier()
	{
		// clean all edges.
		for (auto& e : HalfEdges)
		{
			while (e != nullptr)
			{
				auto t = e;
				e = e->Next;
				delete t;
			}
		}
	}

	bool CheckValidIndicesForFace(const UINT* indices)
	{
		if (indices[0] == indices[1] || indices[0] == indices[2] || indices[1] == indices[2])
			return false;
		else
			return true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	// Build acceleration structure for stripifier, such as faces, edges.
	// also generate normals for faces.
	////////////////////////////////////////////////////////////////////////////////////////////////////
	void FStripifier::Preprocess(const std::vector<Float3>& Vertices, const std::vector<UINT>& Indices)
	{
		// Build Face and edges
		ASSERT(Indices.size() % 3 == 0);
		Faces.reserve(Indices.size() / 3);

		HalfEdges.resize(Vertices.size());
		for (auto& e : HalfEdges)
		{
			e = nullptr;
		}

		UINT OriginalFaceCount = (UINT)Indices.size() / 3;
		for (UINT f = 0; f < OriginalFaceCount; f++)
		{
			if (!CheckValidIndicesForFace(&Indices[f * 3]))
				continue;

			UINT FaceId = (UINT)Faces.size();
			Faces.resize(Faces.size() + 1);
			FFace& Face = Faces.back();

			Face.VertIdx[0] = Indices[f * 3];
			Face.VertIdx[1] = Indices[f * 3 + 1];
			Face.VertIdx[2] = Indices[f * 3 + 2];

			Float3 p0 = Vertices[Face.VertIdx[0]];
			Float3 p1 = Vertices[Face.VertIdx[1]];
			Float3 p2 = Vertices[Face.VertIdx[2]];
			Float3 n = (p1 - p0).Cross(p2 - p0);
			Face.Normal = n.Normalize();

			const UINT* v = &Face.VertIdx[0];

			if (IsDegenerate(v[0], v[1], v[2]))
				continue;

			Face.Edges[0] = AddEdge(v[0], v[1], FaceId, HalfEdges);
			Face.Edges[1] = AddEdge(v[1], v[2], FaceId, HalfEdges);
			Face.Edges[2] = AddEdge(v[2], v[0], FaceId, HalfEdges);
		}

		FaceUsedFlags.resize(Faces.size());
		for (auto& flag : FaceUsedFlags)
		{
			flag.ResetUsed();
		}

	}


	////////////////////////////////////////////////////////////////////////////////////////////////////
	// TODO: Can optimize it by save neighbor face on the halfedge.
	////////////////////////////////////////////////////////////////////////////////////////////////////
	std::vector<UINT> FStripifier::FindNeighbors(UINT FaceId, const std::vector<FUseFlag>& LocalUseFlags) const
	{
		const FFace& Face = Faces[FaceId];
		std::vector<UINT> output;
		for (UINT i = 0; i < 3; i++)
		{
			auto Neighbor = Face.Edges[i]->NeighborFace;
			if (Neighbor != -1 && !FaceUsedFlags[Neighbor].GetUsed() && !LocalUseFlags[Neighbor].GetUsed())
				output.push_back(Face.Edges[i]->NeighborFace);
		}
		return output;
	}

	void FStripifier::BuildSingleCluster()
	{
		std::unique_ptr<FCluster> PlaneProxyCluster = std::make_unique<FCluster>(Faces, (UINT)HalfEdges.size(), kStripSize, kStripNormalAngleThreshold,
			kSelectFullStripExperimentFaceCountWeight, kSelectConnectedStripExperimentAngleWeight, kMaxStartFaces);
		for (UINT f = 0; f < Faces.size(); f++)
		{
			FFace& Face = Faces[f];
			PlaneProxyCluster->AddFace(Face);
		}
		Clusters.emplace_back(std::move(PlaneProxyCluster));
	}

	// TODO: Jobify it for every cluster.
	void FStripifier::BuildStrips()
	{
		StripifiedStripLists.clear();

		UINT c = 0;
		for (auto& Cluster : Clusters)
		{
			const FStripList& result = Cluster->BuildStrips();

			StripifiedStripLists.insert(StripifiedStripLists.begin(), result.StripLists.begin(), result.StripLists.end());
			c++;
		}
	}

	void FStripifier::BuildTriangleStripId()
	{
		TriangleStripIds.resize(Faces.size());
		UINT StripId = 0;
		for (std::vector<UINT>& Strip : StripifiedStripLists)
		{
			for (UINT i = 0; i < (UINT)Strip.size() - 2; i++)
			{
				// degenerated triangle
				if (Strip[i + 2] == Strip[i + 1] || Strip[i + 2] == Strip[i] || Strip[i + 1] == Strip[i])
				{
					continue;
				}

				if (Strip[i] == UINT_MAX || Strip[i + 1] == UINT_MAX || Strip[i + 2] == UINT_MAX)
					continue;

				UINT face;
				if (i % 2 == 0)
					face = FindFace(Strip[i], Strip[i + 1], HalfEdges);
				else
					face = FindFace(Strip[i + 1], Strip[i], HalfEdges);

				TriangleStripIds[face] = StripId;
			}
			StripId++;
		}
	}

	void FStripifier::PostProcess()
	{
		BuildTriangleStripId();

		if (!MergeFinalStrips)
		{
			for (std::vector<UINT>& SrcStrip : StripifiedStripLists)
			{
				std::vector<UINT> DstStrip;
				UINT DividedStripCount = (UINT)(SrcStrip.size() + kStripSize - 1) / kStripSize;

				// This is wrong, we need to add the last 2 vertices from previous strip to the beginning of the next strip.
				for (UINT iStrip = 0; iStrip < DividedStripCount; iStrip++)
				{
					UINT CopyCount = std::min< UINT>(kStripSize, (UINT)SrcStrip.size() - iStrip * kStripSize);
					DstStrip.insert(DstStrip.begin(), SrcStrip.begin() + iStrip * kStripSize, SrcStrip.begin() + iStrip * kStripSize + CopyCount);

					PostProcessedStripLists.emplace_back(std::move(DstStrip));
				}

			}
		}
		else
		{
			std::vector<UINT> DstStrip;

			UINT SrcStripListIndex = 0;
			UINT SrcStripListVertexIndex = 0;

			while (SrcStripListIndex < StripifiedStripLists.size())
			{
				std::vector<UINT>& SrcStrip = StripifiedStripLists[SrcStripListIndex];

				int NeedDuplicateVertices = 0;
				int BackwardSrcStripVertices = 0;

				// Start a new SrcStrip, it could copy to the beginning of a DstStrip, or the middle of a DstStrip, if the previous Src strip didn't fill the full DstStrip.
				// New SrcStrip, New or Middle of DstStrip.
				if (SrcStripListVertexIndex == 0)
				{
					// If we are filling leftover of DstStrip, it means the previous DstStrip is less than the full size of New Strip, we will use start new DstStrip to fill the left over. 
					ASSERT(SrcStripListVertexIndex == 0);

					if (DstStrip.size() > 0)
					{
						if (DstStrip.size() % 2 == 0)
							NeedDuplicateVertices = 2;
						else
							NeedDuplicateVertices = 3;
					}

					BackwardSrcStripVertices = 0;

					int CurrentRemainingSpaceInNewStrip = kStripSize - (UINT)DstStrip.size();
					// New SrcStrip, New DstStrip
					if (CurrentRemainingSpaceInNewStrip - NeedDuplicateVertices < 3)
					{
						PostProcessedStripLists.push_back(DstStrip);
						DstStrip.clear();

						NeedDuplicateVertices = 0;
					}

					// At this moment, DstStrip has enough space to fill at least one triangle + NeedDuplicateVertices.
					if (NeedDuplicateVertices >= 2)
					{
						DstStrip.push_back(DstStrip.back());
						DstStrip.push_back(SrcStrip[SrcStripListVertexIndex]);
						if (NeedDuplicateVertices == 3)
							DstStrip.push_back(SrcStrip[SrcStripListVertexIndex]);
					}

					// Fill DstStrip with New SrcStrip.
					UINT LeftSpaceInDstStrip = kStripSize - (UINT)DstStrip.size();
					UINT FillVertices = std::min<UINT>(LeftSpaceInDstStrip, (UINT)SrcStrip.size());
					for (UINT i = 0; i < FillVertices; i++)
					{
						DstStrip.push_back(SrcStrip[i]);
					}
					// Increase SrcStripListVertexIndex
					SrcStripListVertexIndex += FillVertices;
				}
				else // Continue in the middle of SrcStrip, it must mean the space of previous DstStrip is smaller than SrcStrip. So we start a new DstStrip to continue fill the SrcStrip. 
					 // New DstStrip, Middle of SrcStrip.
				{
					PostProcessedStripLists.push_back(DstStrip);
					DstStrip.clear();

					if (SrcStripListVertexIndex % 2 == 0)
						NeedDuplicateVertices = 0;
					else
						NeedDuplicateVertices = 1;

					BackwardSrcStripVertices = 2;

					if (NeedDuplicateVertices)
						DstStrip.push_back(SrcStrip[SrcStripListVertexIndex - 2]);

					DstStrip.push_back(SrcStrip[SrcStripListVertexIndex - 2]);
					DstStrip.push_back(SrcStrip[SrcStripListVertexIndex - 1]);

					UINT LeftSpaceInDstStrip = kStripSize - (UINT)DstStrip.size();
					UINT FillVertices = std::min<UINT>(LeftSpaceInDstStrip, (UINT)SrcStrip.size() - SrcStripListVertexIndex);
					for (UINT i = 0; i < FillVertices; i++)
					{
						DstStrip.push_back(SrcStrip[SrcStripListVertexIndex + i]);
					}
					SrcStripListVertexIndex += FillVertices;

				}

				// Update SrcStripListIndex
				if (SrcStripListVertexIndex == SrcStrip.size())
				{
					SrcStripListIndex++;
					SrcStripListVertexIndex = 0;
				}

			}

			// Fill result for last one.
			if (DstStrip.size())
			{
				for (UINT i = (UINT)DstStrip.size(); i < (UINT)kStripSize; i++)
				{
					DstStrip.push_back(DstStrip.back());
				}
				PostProcessedStripLists.push_back(DstStrip);
			}
		}
		// Fill all Strips to kStripSize size.
		if (FillDummyVerticesToStrip)
		{
			for (auto& Strip : PostProcessedStripLists)
			{
				for (UINT i = (UINT)Strip.size(); i < (UINT)kStripSize; i++)
					Strip.push_back(Strip.back());
			}
		}
	}

	std::vector<std::vector<UINT>>& FStripifier::Stripify(const std::vector<Float3>& Vertices, const std::vector<UINT>& Indices)
	{
		// Preprocess to Build Information
		Preprocess(Vertices, Indices);

		BuildSingleCluster();

		// Stripify
		BuildStrips();

		PostProcess();

		return PostProcessedStripLists;
	}

}
