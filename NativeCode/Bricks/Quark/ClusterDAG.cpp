#include "ClusterDAG.h"
#include "GraphPartitioner.h"
#include "DisjointSet.h"
#include "../../../3rd/native/metis/5.1.0/include/metis.h"
#include <algorithm>
#include <cstdio>

NS_BEGIN

FClusterDAG::~FClusterDAG()
{
	for (auto* Cluster : Clusters)
	{
		delete Cluster;
	}
	Clusters.clear();
}

UINT FClusterDAG::AddMesh(
	const std::vector<v3dxVector3>& Verts,
	const std::vector<v3dxVector3>& Normals,
	const std::vector<float>& Tangents,
	const std::vector<float>& UVs,
	const std::vector<UINT>& Indexes,
	const std::vector<INT32>& MaterialIndexes)
{
	UINT NumTris = (UINT)(Indexes.size() / 3);
	if (NumTris == 0)
		return 0;

	// Step 1: Build adjacency for the entire mesh
	// Use edge hashing to find triangle adjacency
	FAdjacency Adjacency((INT32)Indexes.size());
	FEdgeHash EdgeHash((INT32)Indexes.size());

	for (INT32 EdgeIndex = 0; EdgeIndex < (INT32)Indexes.size(); EdgeIndex++)
	{
		Adjacency.Direct[EdgeIndex] = -1;

		EdgeHash.ForAllMatching(EdgeIndex, true,
			[&Verts, &Indexes](INT32 CornerIndex, v3dxVector3& result)
			{
				if ((UINT)CornerIndex < Indexes.size() && Indexes[CornerIndex] < Verts.size())
				{
					result = Verts[Indexes[CornerIndex]];
					return true;
				}
				return false;
			},
			[&Adjacency](INT32 EdgeIndex0, INT32 OtherEdgeIndex)
			{
				Adjacency.Link(EdgeIndex0, OtherEdgeIndex);
			});
	}

	// Step 2: Partition triangles into clusters using METIS
	FGraphPartitioner Partitioner(NumTris);

	// Build disjoint set for locality links
	FDisjointSet DisjointSet(NumTris);
	for (INT32 EdgeIndex = 0; EdgeIndex < (INT32)Indexes.size(); EdgeIndex++)
	{
		Adjacency.ForAll(EdgeIndex,
			[&DisjointSet](INT32 EdgeIndex0, INT32 EdgeIndex1)
			{
				if (EdgeIndex0 > EdgeIndex1)
					DisjointSet.UnionSequential(EdgeIndex0 / 3, EdgeIndex1 / 3);
			});
	}

	// Compute bounds for locality links
	v3dxBox3 MeshBounds;
	for (const auto& V : Verts)
	{
		MeshBounds += V;
	}

	// Build locality links
	auto GetCenter = [&Verts, &Indexes](UINT TriIndex, v3dxVector3& Center)
	{
		UINT Idx0 = TriIndex * 3 + 0;
		UINT Idx1 = TriIndex * 3 + 1;
		UINT Idx2 = TriIndex * 3 + 2;
		if (Idx2 >= Indexes.size())
			return false;
		if (Indexes[Idx0] >= Verts.size() || Indexes[Idx1] >= Verts.size() || Indexes[Idx2] >= Verts.size())
			return false;
		Center = Verts[Indexes[Idx0]];
		Center += Verts[Indexes[Idx1]];
		Center += Verts[Indexes[Idx2]];
		Center *= (1.0f / 3.0f);
		return true;
	};

	std::vector<INT32> EmptyGroupIndexes;
	Partitioner.BuildLocalityLinks(DisjointSet, MeshBounds, EmptyGroupIndexes, GetCenter);

	// Build graph for partitioning
	auto Graph = Partitioner.NewGraph(NumTris * 3);
	for (UINT i = 0; i < NumTris; i++)
	{
		Graph->AdjacencyOffset[i] = (INT32)Graph->Adjacency.size();

		UINT TriIndex = Partitioner.Indexes[i];
		for (int k = 0; k < 3; k++)
		{
			Adjacency.ForAll(3 * TriIndex + k,
				[&Partitioner, Graph](INT32 EdgeIndex, INT32 AdjIndex)
				{
					Partitioner.AddAdjacency(Graph, AdjIndex / 3, 4 * 65);
				});
		}
		Partitioner.AddLocalityLinks(Graph, TriIndex, 1);
	}
	Graph->AdjacencyOffset[NumTris] = (INT32)Graph->Adjacency.size();

	// Partition into clusters of ~ClusterSize triangles
	Partitioner.PartitionStrict(Graph, ClusterSize - 4, ClusterSize, false);

	// Step 3: Create cluster objects from partition result
	UINT ClusterStartIndex = (UINT)Clusters.size();
	for (const auto& Range : Partitioner.Ranges)
	{
		QuarkCluster* NewCluster = new QuarkCluster(
			Verts, Normals, Tangents, UVs, Indexes, MaterialIndexes,
			Range.Begin, Range.End,
			Partitioner, Adjacency);
		NewCluster->MipLevel = 0;
		NewCluster->LODError = 0.0f; // Leaf clusters have zero error
		Clusters.push_back(NewCluster);
	}

	// Step 4: Build adjacency map between clusters
	std::vector<UINT> NewClusterIndices;
	for (UINT i = ClusterStartIndex; i < (UINT)Clusters.size(); i++)
	{
		NewClusterIndices.push_back(i);
	}
	BuildClusterAdjacency(NewClusterIndices);

	// Record mip level start
	if (MipLevelStart.empty())
	{
		MipLevelStart.push_back(ClusterStartIndex);
	}
	NumMipLevels = 1;

	return (UINT)Clusters.size() - ClusterStartIndex;
}

void FClusterDAG::BuildDAG()
{
	int CurrentLevel = 0;

	while (true)
	{
		// Get clusters at current level
		std::vector<UINT> LevelClusters = GetClustersAtLevel(CurrentLevel);

		// Stop if we have very few clusters (converged)
		if (LevelClusters.size() <= MaxGroupSize)
		{
			// Create one final group with all remaining clusters
			if (LevelClusters.size() > 1)
			{
				FClusterGroup FinalGroup;
				FinalGroup.Children = LevelClusters;
				FinalGroup.MipLevel = CurrentLevel;
				ComputeGroupBounds(FinalGroup);
				ReduceGroup(FinalGroup);
				Groups.push_back(FinalGroup);
				NumMipLevels = CurrentLevel + 2;
				MipLevelStart.push_back((UINT)Clusters.size() - (UINT)FinalGroup.Parents.size());
			}
			break;
		}

		// Group clusters at this level
		std::vector<FClusterGroup> LevelGroups;
		GroupClusters(CurrentLevel, LevelGroups);

		if (LevelGroups.empty())
			break;

		// Record the start of next mip level
		UINT NextLevelStart = (UINT)Clusters.size();

		// Reduce each group to produce parent clusters
		for (auto& Group : LevelGroups)
		{
			ReduceGroup(Group);
			Groups.push_back(Group);
		}

		// Build adjacency for newly created parent clusters
		std::vector<UINT> ParentIndices;
		for (UINT i = NextLevelStart; i < (UINT)Clusters.size(); i++)
		{
			ParentIndices.push_back(i);
		}
		if (!ParentIndices.empty())
		{
			BuildClusterAdjacency(ParentIndices);
		}

		MipLevelStart.push_back(NextLevelStart);
		CurrentLevel++;
		NumMipLevels = CurrentLevel + 1;

		VFX_LTRACE(ELTT_info, "[ClusterDAG] Level %d: %u clusters -> %u parent clusters\n",
			CurrentLevel - 1, (UINT)LevelClusters.size(), (UINT)ParentIndices.size());
	}
}

void FClusterDAG::GroupClusters(int Level, std::vector<FClusterGroup>& OutGroups)
{
	std::vector<UINT> LevelClusters = GetClustersAtLevel(Level);
	UINT NumClustersAtLevel = (UINT)LevelClusters.size();

	if (NumClustersAtLevel <= 1)
		return;

	// Build a cluster-to-cluster adjacency graph for METIS partitioning
	// Map from cluster global index -> local index (0..N-1)
	std::map<UINT, UINT> ClusterToLocal;
	for (UINT i = 0; i < NumClustersAtLevel; i++)
	{
		ClusterToLocal[LevelClusters[i]] = i;
	}

	// Build CSR adjacency for METIS
	std::vector<INT32> AdjOffset(NumClustersAtLevel + 1);
	std::vector<INT32> AdjList;
	std::vector<INT32> AdjWeight;

	for (UINT i = 0; i < NumClustersAtLevel; i++)
	{
		AdjOffset[i] = (INT32)AdjList.size();
		UINT ClusterIdx = LevelClusters[i];
		QuarkCluster* Cluster = Clusters[ClusterIdx];

		for (auto& Pair : Cluster->AdjacentClusters)
		{
			UINT AdjClusterIdx = Pair.first;
			UINT SharedEdges = Pair.second;

			auto It = ClusterToLocal.find(AdjClusterIdx);
			if (It != ClusterToLocal.end())
			{
				AdjList.push_back((INT32)It->second);
				AdjWeight.push_back((INT32)SharedEdges);
			}
		}
	}
	AdjOffset[NumClustersAtLevel] = (INT32)AdjList.size();

	// Target number of groups
	UINT TargetGroupSize = (MinGroupSize + MaxGroupSize) / 2;
	INT32 NumParts = std::max(2, (INT32)(NumClustersAtLevel / TargetGroupSize));

	if (NumParts <= 1)
	{
		// All in one group
		FClusterGroup Group;
		Group.Children = LevelClusters;
		Group.MipLevel = Level;
		ComputeGroupBounds(Group);
		OutGroups.push_back(Group);
		return;
	}

	// Run METIS partitioning
	std::vector<INT32> PartitionIDs(NumClustersAtLevel);
	INT32 NumConstraints = 1;
	INT32 EdgesCut = 0;
	INT32 N = (INT32)NumClustersAtLevel;

	INT32 Options[METIS_NOPTIONS];
	METIS_SetDefaultOptions(Options);
	Options[METIS_OPTION_UFACTOR] = 200; // Allow some imbalance

	int Result = METIS_OK;
	if (AdjList.size() > 0)
	{
		Result = METIS_PartGraphRecursive(
			&N,
			&NumConstraints,
			AdjOffset.data(),
			AdjList.data(),
			NULL,           // vertex weights
			NULL,           // vertex sizes
			AdjWeight.data(),
			&NumParts,
			NULL,           // target partition weights
			NULL,           // imbalance tolerance
			Options,
			&EdgesCut,
			PartitionIDs.data());
	}
	else
	{
		// No adjacency - fallback: sequential grouping
		for (UINT i = 0; i < NumClustersAtLevel; i++)
		{
			PartitionIDs[i] = (INT32)(i / TargetGroupSize);
		}
		NumParts = (INT32)((NumClustersAtLevel + TargetGroupSize - 1) / TargetGroupSize);
	}

	if (Result != METIS_OK)
	{
		// Fallback: sequential grouping
		for (UINT i = 0; i < NumClustersAtLevel; i++)
		{
			PartitionIDs[i] = (INT32)(i / TargetGroupSize);
		}
		NumParts = (INT32)((NumClustersAtLevel + TargetGroupSize - 1) / TargetGroupSize);
	}

	// Collect clusters into groups by partition ID
	std::vector<std::vector<UINT>> PartitionClusters(NumParts);
	for (UINT i = 0; i < NumClustersAtLevel; i++)
	{
		INT32 PartID = PartitionIDs[i];
		if (PartID >= 0 && PartID < NumParts)
		{
			PartitionClusters[PartID].push_back(LevelClusters[i]);
		}
	}

	// Create groups from partitions
	for (INT32 p = 0; p < NumParts; p++)
	{
		if (PartitionClusters[p].empty())
			continue;

		FClusterGroup Group;
		Group.Children = PartitionClusters[p];
		Group.MipLevel = Level;
		ComputeGroupBounds(Group);
		OutGroups.push_back(Group);
	}
}

void FClusterDAG::ReduceGroup(FClusterGroup& Group)
{
	if (Group.Children.empty())
		return;

	// Step 1: Merge all children into one large cluster
	std::vector<QuarkCluster*> MergeList;
	for (UINT ChildIdx : Group.Children)
	{
		MergeList.push_back(Clusters[ChildIdx]);
	}
	QuarkCluster* MergedCluster = new QuarkCluster(MergeList);
	MergedCluster->MipLevel = Group.MipLevel + 1;

	// Step 2: Simplify to ~50% triangle count
	UINT TargetTris = std::max(ClusterSize, MergedCluster->NumTris / 2);
	float SimplifyError = MergedCluster->Simplify(TargetTris);

	// Step 3: Compute ParentLODError = max(SimplifyError, max child LODError)
	float MaxChildError = 0.0f;
	for (UINT ChildIdx : Group.Children)
	{
		MaxChildError = std::max(MaxChildError, Clusters[ChildIdx]->LODError);
	}
	Group.ParentLODError = std::max(SimplifyError, MaxChildError);

	// Step 4: Split the simplified merged cluster back into <=ClusterSize tri clusters
	if (MergedCluster->NumTris <= ClusterSize)
	{
		// Already small enough, just use it as-is
		MergedCluster->LODError = Group.ParentLODError;
		MergedCluster->LODBounds = Group.LODBounds;
		MergedCluster->GeneratingGroupIndex = (UINT)Groups.size();

		UINT NewIdx = (UINT)Clusters.size();
		Clusters.push_back(MergedCluster);
		Group.Parents.push_back(NewIdx);
	}
	else
	{
		// Re-partition the merged cluster
		FAdjacency Adjacency = MergedCluster->BuildAdjacency();
		FGraphPartitioner Partitioner(MergedCluster->NumTris);
		MergedCluster->Split(Partitioner, Adjacency, ClusterSize);

		// Create parent clusters from partition ranges
		for (const auto& Range : Partitioner.Ranges)
		{
			QuarkCluster* ParentCluster = new QuarkCluster(
				*MergedCluster, Range.Begin, Range.End, Partitioner, Adjacency);
			ParentCluster->MipLevel = Group.MipLevel + 1;
			ParentCluster->LODError = Group.ParentLODError;
			ParentCluster->LODBounds = Group.LODBounds;
			ParentCluster->GeneratingGroupIndex = (UINT)Groups.size();

			UINT NewIdx = (UINT)Clusters.size();
			Clusters.push_back(ParentCluster);
			Group.Parents.push_back(NewIdx);
		}

		// Delete the intermediate merged cluster
		delete MergedCluster;
	}

	// Step 5: Propagate LODBounds to parent clusters
	for (UINT ParentIdx : Group.Parents)
	{
		Clusters[ParentIdx]->LODBounds = Group.LODBounds;
	}
}

void FClusterDAG::BuildClusterAdjacency(const std::vector<UINT>& ClusterIndices)
{
	// Build a position-based hash to find shared edges between clusters
	// Two clusters are adjacent if they share vertex positions on their boundaries

	struct FVertexKey
	{
		v3dxVector3 Position;
		UINT ClusterIndex;
	};

	// Collect all external edge vertices from each cluster
	std::map<UINT, std::vector<v3dxVector3>> ClusterBoundaryVerts;
	for (UINT ClusterIdx : ClusterIndices)
	{
		QuarkCluster* Cluster = Clusters[ClusterIdx];
		std::vector<v3dxVector3> BoundaryVerts;

		for (UINT i = 0; i < (UINT)Cluster->ExternalEdges.size(); i++)
		{
			if (Cluster->ExternalEdges[i] != 0)
			{
				UINT VertIdx = Cluster->Indexes[i];
				if (VertIdx < Cluster->NumVerts)
				{
					BoundaryVerts.push_back(Cluster->GetPosition(VertIdx));
				}
			}
		}
		ClusterBoundaryVerts[ClusterIdx] = BoundaryVerts;
	}

	// For each pair of clusters, check if they share boundary vertices
	for (UINT i = 0; i < (UINT)ClusterIndices.size(); i++)
	{
		UINT ClusterA = ClusterIndices[i];
		const auto& VertsA = ClusterBoundaryVerts[ClusterA];

		for (UINT j = i + 1; j < (UINT)ClusterIndices.size(); j++)
		{
			UINT ClusterB = ClusterIndices[j];
			const auto& VertsB = ClusterBoundaryVerts[ClusterB];

			// Count shared vertices (using position comparison)
			UINT SharedCount = 0;
			for (const auto& VA : VertsA)
			{
				for (const auto& VB : VertsB)
				{
					if (VA == VB)
					{
						SharedCount++;
						if (SharedCount >= 2) // At least one shared edge
							goto FoundAdjacency;
					}
				}
			}
			continue;

		FoundAdjacency:
			// Mark as adjacent
			Clusters[ClusterA]->AdjacentClusters[ClusterB] = SharedCount;
			Clusters[ClusterB]->AdjacentClusters[ClusterA] = SharedCount;
		}
	}
}

void FClusterDAG::ComputeGroupBounds(FClusterGroup& Group)
{
	if (Group.Children.empty())
		return;

	// Compute union AABB
	Group.Bounds = v3dxBox3();
	v3dxVector3 Centroid(0, 0, 0);
	UINT TotalVerts = 0;

	for (UINT ChildIdx : Group.Children)
	{
		QuarkCluster* Cluster = Clusters[ChildIdx];
		Group.Bounds.MergeVertex(Cluster->Bounds.minbox);
		Group.Bounds.MergeVertex(Cluster->Bounds.maxbox);
		Centroid += Cluster->SphereBounds.getCenter();
		TotalVerts++;
	}

	if (TotalVerts > 0)
		Centroid /= (float)TotalVerts;

	// Compute bounding sphere: find max distance from centroid to any child sphere edge
	float MaxRadius = 0.0f;
	for (UINT ChildIdx : Group.Children)
	{
		QuarkCluster* Cluster = Clusters[ChildIdx];
		float Dist = (Cluster->SphereBounds.getCenter() - Centroid).getLength() + Cluster->SphereBounds.getRadius();
		MaxRadius = std::max(MaxRadius, Dist);
	}

	Group.LODBounds = v3dxSphere(Centroid, MaxRadius);
}

std::vector<UINT> FClusterDAG::GetClustersAtLevel(int Level) const
{
	std::vector<UINT> Result;
	for (UINT i = 0; i < (UINT)Clusters.size(); i++)
	{
		if (Clusters[i]->MipLevel == Level)
		{
			Result.push_back(i);
		}
	}
	return Result;
}

void FClusterDAG::PrintDAGInfo() const
{
	VFX_LTRACE(ELTT_info, "=== ClusterDAG Info ===\n");
	VFX_LTRACE(ELTT_info, "Total Clusters: %u\n", (UINT)Clusters.size());
	VFX_LTRACE(ELTT_info, "Total Groups: %u\n", (UINT)Groups.size());
	VFX_LTRACE(ELTT_info, "Mip Levels: %u\n", NumMipLevels);

	for (UINT Level = 0; Level < NumMipLevels; Level++)
	{
		std::vector<UINT> LevelClusters = GetClustersAtLevel(Level);

		float MinError = FLT_MAX, MaxError = 0.0f;
		UINT TotalTris = 0;
		for (UINT ClusterIdx : LevelClusters)
		{
			QuarkCluster* C = Clusters[ClusterIdx];
			MinError = std::min(MinError, C->LODError);
			MaxError = std::max(MaxError, C->LODError);
			TotalTris += C->NumTris;
		}
		if (LevelClusters.empty()) MinError = 0.0f;

		VFX_LTRACE(ELTT_info, "  Level %u: %u clusters, %u triangles, LODError [%.4f - %.4f]\n",
			Level, (UINT)LevelClusters.size(), TotalTris, MinError, MaxError);
	}
	VFX_LTRACE(ELTT_info, "=======================\n");
}

void FClusterDAG::GetExportSizes(UINT& outGroupCount, UINT& outChildrenTotal,
	UINT& outParentsTotal, UINT& outClusterCount, UINT& outRootGroupCount) const
{
	outGroupCount = (UINT)Groups.size();
	outClusterCount = (UINT)Clusters.size();

	UINT childrenTotal = 0;
	UINT parentsTotal = 0;
	UINT rootGroupCount = 0;

	int maxGroupLevel = 0;
	for (const auto& G : Groups)
	{
		childrenTotal += (UINT)G.Children.size();
		parentsTotal += (UINT)G.Parents.size();
		if (G.MipLevel > maxGroupLevel)
			maxGroupLevel = G.MipLevel;
	}

	// Root groups = groups at the highest MipLevel
	for (const auto& G : Groups)
	{
		if (G.MipLevel == maxGroupLevel)
			rootGroupCount++;
	}

	outChildrenTotal = childrenTotal;
	outParentsTotal = parentsTotal;
	outRootGroupCount = rootGroupCount;
}

UINT FClusterDAG::ExportGroupsForGPU(
	void* outGroupsRaw, UINT maxGroups,
	UINT* outChildrenIndices, UINT maxChildren,
	UINT* outParentsIndices, UINT maxParents,
	UINT* outClusterGroupMap, UINT maxClusters,
	UINT* outRootGroupIndices, UINT maxRootGroups,
	UINT& outChildrenTotal, UINT& outParentsTotal, UINT& outRootGroupCount) const
{
	FClusterGroupExport* outGroups = (FClusterGroupExport*)outGroupsRaw;
	UINT numGroups = (UINT)Groups.size();
	if (numGroups == 0 || outGroups == nullptr)
		return 0;

	// Find max group level (root level)
	int maxGroupLevel = 0;
	for (const auto& G : Groups)
	{
		if (G.MipLevel > maxGroupLevel)
			maxGroupLevel = G.MipLevel;
	}

	// Flatten children and parents into contiguous arrays
	UINT childrenOffset = 0;
	UINT parentsOffset = 0;
	UINT rootCount = 0;

	for (UINT g = 0; g < numGroups && g < maxGroups; g++)
	{
		const FClusterGroup& group = Groups[g];
		FClusterGroupExport& exp = outGroups[g];

		exp.LODBoundsCenter = group.LODBounds.getCenter();
		exp.LODBoundsRadius = group.LODBounds.getRadius();
		exp.ParentLODError = group.ParentLODError;
		exp.MipLevel = group.MipLevel;
		exp.ChildrenStart = (int)childrenOffset;
		exp.ChildrenCount = (int)group.Children.size();
		exp.ParentsStart = (int)parentsOffset;
		exp.ParentsCount = (int)group.Parents.size();
		exp.Padding0 = 0;
		exp.Padding1 = 0;

		// Copy children indices
		for (UINT c = 0; c < (UINT)group.Children.size() && childrenOffset < maxChildren; c++)
		{
			outChildrenIndices[childrenOffset] = group.Children[c];
			childrenOffset++;
		}

		// Copy parents indices
		for (UINT p = 0; p < (UINT)group.Parents.size() && parentsOffset < maxParents; p++)
		{
			outParentsIndices[parentsOffset] = group.Parents[p];
			parentsOffset++;
		}

		// Collect root groups (highest level)
		if (group.MipLevel == maxGroupLevel && rootCount < maxRootGroups)
		{
			outRootGroupIndices[rootCount] = g;
			rootCount++;
		}
	}

	// Build ClusterGroupMap: for each cluster, find its generating group
	// GeneratingGroupIndex on each cluster points to the group that created it
	if (outClusterGroupMap != nullptr)
	{
		UINT numClusters = (UINT)Clusters.size();
		for (UINT i = 0; i < numClusters && i < maxClusters; i++)
		{
			UINT genGroup = Clusters[i]->GeneratingGroupIndex;
			// Level 0 clusters have no generating group (they ARE the leaf)
			// Use ~0u as sentinel for "no group"
			outClusterGroupMap[i] = (genGroup < numGroups) ? genGroup : ~0u;
		}
	}

	outChildrenTotal = childrenOffset;
	outParentsTotal = parentsOffset;
	outRootGroupCount = rootCount;

	return numGroups;
}

NS_END
