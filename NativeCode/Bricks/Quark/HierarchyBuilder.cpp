#include "HierarchyBuilder.h"
#include <algorithm>
#include <cstdio>
#include <cstring>

NS_BEGIN

// ============================================================================
// FHierarchyBuilder
// ============================================================================

void FHierarchyBuilder::Build(const FClusterDAG& DAG, std::vector<FPackedHierarchyNode>& OutNodes)
{
	OutNodes.clear();

	if (DAG.Clusters.empty())
		return;

	// Collect all clusters (we build BVH over ALL clusters, not just one level)
	std::vector<UINT> AllClusterIndices;
	for (UINT i = 0; i < (UINT)DAG.Clusters.size(); i++)
	{
		AllClusterIndices.push_back(i);
	}

	BuildRecursive(DAG, AllClusterIndices, OutNodes);

	printf("[HierarchyBuilder] Built %u BVH nodes for %u clusters\n",
		(UINT)OutNodes.size(), (UINT)DAG.Clusters.size());
}

UINT FHierarchyBuilder::BuildRecursive(
	const FClusterDAG& DAG,
	const std::vector<UINT>& ClusterIndices,
	std::vector<FPackedHierarchyNode>& OutNodes)
{
	if (ClusterIndices.empty())
		return ~0u;

	// If small enough, create a leaf BVH node pointing directly to clusters
	if (ClusterIndices.size() <= QUARK_MAX_BVH_NODE_FANOUT)
	{
		FPackedHierarchyNode Node;
		memset(&Node, 0, sizeof(Node));
		Node.NumChildren = (UINT)ClusterIndices.size();

		for (UINT i = 0; i < Node.NumChildren; i++)
		{
			UINT CIdx = ClusterIndices[i];
			QuarkCluster* C = DAG.Clusters[CIdx];

			Node.ChildLODBounds[i][0] = C->LODBounds.getCenter().X;
			Node.ChildLODBounds[i][1] = C->LODBounds.getCenter().Y;
			Node.ChildLODBounds[i][2] = C->LODBounds.getCenter().Z;
			Node.ChildLODBounds[i][3] = C->LODBounds.getRadius();

			v3dxVector3 Center = (C->Bounds.Min() + C->Bounds.Max()) * 0.5f;
			v3dxVector3 Extent = (C->Bounds.Max() - C->Bounds.Min()) * 0.5f;
			Node.ChildBoxCenter[i][0] = Center.X;
			Node.ChildBoxCenter[i][1] = Center.Y;
			Node.ChildBoxCenter[i][2] = Center.Z;
			Node.ChildBoxExtent[i][0] = Extent.X;
			Node.ChildBoxExtent[i][1] = Extent.Y;
			Node.ChildBoxExtent[i][2] = Extent.Z;

			Node.ChildMaxParentLODError[i] = C->LODError;
			Node.ChildMinLODError[i] = C->LODError;
			Node.ChildStartReference[i] = CIdx;
			Node.ChildCount[i] = 1;
			Node.ChildFlags[i] = 1; // bLeaf = true (points to cluster)
		}

		UINT NodeIdx = (UINT)OutNodes.size();
		OutNodes.push_back(Node);
		return NodeIdx;
	}

	// Otherwise, spatially partition into up to 8 children and recurse
	std::vector<std::vector<UINT>> Partitions;
	SpatialPartition(DAG, ClusterIndices, Partitions);

	// Reserve a slot for this node
	UINT ThisNodeIdx = (UINT)OutNodes.size();
	OutNodes.push_back(FPackedHierarchyNode());

	// Recursively build children
	FPackedHierarchyNode& Node = OutNodes[ThisNodeIdx];
	memset(&Node, 0, sizeof(Node));
	Node.NumChildren = (UINT)Partitions.size();

	for (UINT i = 0; i < Node.NumChildren; i++)
	{
		UINT ChildNodeIdx = BuildRecursive(DAG, Partitions[i], OutNodes);

		// Compute bounds for this partition
		v3dxBox3 ChildBounds;
		v3dxVector3 ChildCenter(0, 0, 0);
		float ChildMaxRadius = 0.0f;
		float MaxError = 0.0f;
		float MinError = FLT_MAX;
		UINT ChildCount = 0;

		for (UINT CIdx : Partitions[i])
		{
			QuarkCluster* C = DAG.Clusters[CIdx];
			ChildBounds.MergeVertex(C->Bounds.minbox);
			ChildBounds.MergeVertex(C->Bounds.maxbox);
			ChildCenter += C->SphereBounds.getCenter();
			MaxError = std::max(MaxError, C->LODError);
			MinError = std::min(MinError, C->LODError);
			ChildCount++;
		}
		if (ChildCount > 0)
			ChildCenter /= (float)ChildCount;

		for (UINT CIdx : Partitions[i])
		{
			QuarkCluster* C = DAG.Clusters[CIdx];
			float Dist = (C->SphereBounds.getCenter() - ChildCenter).getLength() + C->SphereBounds.getRadius();
			ChildMaxRadius = std::max(ChildMaxRadius, Dist);
		}

		Node.ChildLODBounds[i][0] = ChildCenter.X;
		Node.ChildLODBounds[i][1] = ChildCenter.Y;
		Node.ChildLODBounds[i][2] = ChildCenter.Z;
		Node.ChildLODBounds[i][3] = ChildMaxRadius;

		v3dxVector3 BoxCenter = (ChildBounds.Min() + ChildBounds.Max()) * 0.5f;
		v3dxVector3 BoxExtent = (ChildBounds.Max() - ChildBounds.Min()) * 0.5f;
		Node.ChildBoxCenter[i][0] = BoxCenter.X;
		Node.ChildBoxCenter[i][1] = BoxCenter.Y;
		Node.ChildBoxCenter[i][2] = BoxCenter.Z;
		Node.ChildBoxExtent[i][0] = BoxExtent.X;
		Node.ChildBoxExtent[i][1] = BoxExtent.Y;
		Node.ChildBoxExtent[i][2] = BoxExtent.Z;

		Node.ChildMaxParentLODError[i] = MaxError;
		Node.ChildMinLODError[i] = MinError;
		Node.ChildStartReference[i] = ChildNodeIdx;
		Node.ChildCount[i] = (UINT)Partitions[i].size();
		Node.ChildFlags[i] = (Partitions[i].size() <= QUARK_MAX_BVH_NODE_FANOUT) ? 0 : 0;
		// Flag=0 means it points to another BVH node
	}

	return ThisNodeIdx;
}

void FHierarchyBuilder::SpatialPartition(
	const FClusterDAG& DAG,
	const std::vector<UINT>& ClusterIndices,
	std::vector<std::vector<UINT>>& OutPartitions)
{
	UINT NumClusters = (UINT)ClusterIndices.size();

	// Compute overall bounds
	v3dxBox3 Bounds;
	for (UINT CIdx : ClusterIndices)
	{
		Bounds.MergeVertex(DAG.Clusters[CIdx]->Bounds.minbox);
		Bounds.MergeVertex(DAG.Clusters[CIdx]->Bounds.maxbox);
	}

	v3dxVector3 BoundsSize = Bounds.Max() - Bounds.Min();

	// Find the longest axis
	int SplitAxis = 0;
	if (BoundsSize.Y > BoundsSize.X && BoundsSize.Y > BoundsSize.Z)
		SplitAxis = 1;
	else if (BoundsSize.Z > BoundsSize.X && BoundsSize.Z > BoundsSize.Y)
		SplitAxis = 2;

	// Sort clusters along the split axis by their centroid
	std::vector<std::pair<float, UINT>> SortedClusters;
	SortedClusters.reserve(NumClusters);
	for (UINT CIdx : ClusterIndices)
	{
		v3dxVector3 Center = DAG.Clusters[CIdx]->SphereBounds.getCenter();
		float Key = (SplitAxis == 0) ? Center.X : ((SplitAxis == 1) ? Center.Y : Center.Z);
		SortedClusters.push_back({ Key, CIdx });
	}
	std::sort(SortedClusters.begin(), SortedClusters.end());

	// Divide into up to QUARK_MAX_BVH_NODE_FANOUT equal-sized groups
	UINT NumPartitions = std::min((UINT)QUARK_MAX_BVH_NODE_FANOUT,
		std::max(2u, NumClusters / QUARK_MAX_BVH_NODE_FANOUT));

	OutPartitions.resize(NumPartitions);
	UINT PerPartition = (NumClusters + NumPartitions - 1) / NumPartitions;

	for (UINT i = 0; i < NumClusters; i++)
	{
		UINT PartIdx = std::min(i / PerPartition, NumPartitions - 1);
		OutPartitions[PartIdx].push_back(SortedClusters[i].second);
	}

	// Remove empty partitions
	OutPartitions.erase(
		std::remove_if(OutPartitions.begin(), OutPartitions.end(),
			[](const std::vector<UINT>& v) { return v.empty(); }),
		OutPartitions.end());
}

// ============================================================================
// QuarkGeometryAsset
// ============================================================================

void QuarkGeometryAsset::BuildFromDAG(const FClusterDAG& DAG)
{
	NumClusters = (UINT)DAG.Clusters.size();
	NumMipLevels = DAG.NumMipLevels;
	PackedClusters.resize(NumClusters);

	// Pack all cluster geometry into contiguous buffers
	UINT GlobalVertexOffset = 0;
	UINT GlobalIndexOffset = 0;

	MeshBounds = v3dxBox3();

	for (UINT i = 0; i < NumClusters; i++)
	{
		QuarkCluster* C = DAG.Clusters[i];
		FPackedCluster& PC = PackedClusters[i];

		// LOD bounds
		PC.LODBounds[0] = C->LODBounds.getCenter().X;
		PC.LODBounds[1] = C->LODBounds.getCenter().Y;
		PC.LODBounds[2] = C->LODBounds.getCenter().Z;
		PC.LODBounds[3] = C->LODBounds.getRadius();

		// AABB
		v3dxVector3 Center = (C->Bounds.Min() + C->Bounds.Max()) * 0.5f;
		v3dxVector3 Extent = (C->Bounds.Max() - C->Bounds.Min()) * 0.5f;
		PC.BoxCenter[0] = Center.X;
		PC.BoxCenter[1] = Center.Y;
		PC.BoxCenter[2] = Center.Z;
		PC.BoxExtent[0] = Extent.X;
		PC.BoxExtent[1] = Extent.Y;
		PC.BoxExtent[2] = Extent.Z;

		// LOD metadata
		PC.LODError = C->LODError;
		PC.EdgeLength = C->EdgeLength;

		// Geometry offsets
		PC.VertexStart = GlobalVertexOffset;
		PC.VertexCount = C->NumVerts;
		PC.IndexStart = GlobalIndexOffset;
		PC.IndexCount = C->NumTris * 3;

		// Material & level
		PC.MaterialIndex = (!C->MaterialIndexes.empty()) ? (UINT)C->MaterialIndexes[0] : 0;
		PC.MipLevel = (UINT)C->MipLevel;
		PC.Flags = (C->MipLevel == 0) ? 1 : 0; // bit0 = leaf
		PC.Padding = 0;

		// Pack vertex positions
		for (UINT v = 0; v < C->NumVerts; v++)
		{
			v3dxVector3 Pos = C->GetPosition(v);
			PackedPositions.push_back(Pos.X);
			PackedPositions.push_back(Pos.Y);
			PackedPositions.push_back(Pos.Z);
		}

		// Pack indices (cluster-local)
		for (UINT idx = 0; idx < C->NumTris * 3; idx++)
		{
			PackedIndices.push_back(C->Indexes[idx]);
		}

		GlobalVertexOffset += C->NumVerts;
		GlobalIndexOffset += C->NumTris * 3;

		// Accumulate mesh bounds
		MeshBounds.MergeVertex(C->Bounds.minbox);
		MeshBounds.MergeVertex(C->Bounds.maxbox);
	}

	// Build BVH hierarchy
	FHierarchyBuilder::Build(DAG, HierarchyNodes);
	NumHierarchyNodes = (UINT)HierarchyNodes.size();

	// Find root LOD error
	RootLODError = 0.0f;
	for (UINT i = 0; i < NumClusters; i++)
	{
		RootLODError = std::max(RootLODError, DAG.Clusters[i]->LODError);
	}

	printf("[QuarkGeometryAsset] Packed: %u clusters, %u verts, %u indices, %u BVH nodes\n",
		NumClusters, GetTotalVertexCount(), GetTotalIndexCount(), NumHierarchyNodes);
}

// Simple binary serialization format:
// [Header: 4 uint32 - magic, numClusters, numHierarchyNodes, numMipLevels]
// [float RootLODError]
// [MeshBounds: 6 floats]
// [PackedPositions: count + data]
// [PackedIndices: count + data]
// [PackedClusters: numClusters * sizeof(FPackedCluster)]
// [HierarchyNodes: numHierarchyNodes * sizeof(FPackedHierarchyNode)]

static const UINT QUARK_ASSET_MAGIC = 0x51475041; // "QGPA" - Quark Geometry Packed Asset

bool QuarkGeometryAsset::SaveToFile(const char* FilePath) const
{
	FILE* f = fopen(FilePath, "wb");
	if (!f) return false;

	// Header
	fwrite(&QUARK_ASSET_MAGIC, sizeof(UINT), 1, f);
	fwrite(&NumClusters, sizeof(UINT), 1, f);
	fwrite(&NumHierarchyNodes, sizeof(UINT), 1, f);
	fwrite(&NumMipLevels, sizeof(UINT), 1, f);
	fwrite(&RootLODError, sizeof(float), 1, f);

	// Mesh bounds
	v3dxVector3 BMin = MeshBounds.Min();
	v3dxVector3 BMax = MeshBounds.Max();
	fwrite(&BMin, sizeof(v3dxVector3), 1, f);
	fwrite(&BMax, sizeof(v3dxVector3), 1, f);

	// Packed positions
	UINT PosCount = (UINT)PackedPositions.size();
	fwrite(&PosCount, sizeof(UINT), 1, f);
	if (PosCount > 0)
		fwrite(PackedPositions.data(), sizeof(float), PosCount, f);

	// Packed indices
	UINT IdxCount = (UINT)PackedIndices.size();
	fwrite(&IdxCount, sizeof(UINT), 1, f);
	if (IdxCount > 0)
		fwrite(PackedIndices.data(), sizeof(UINT), IdxCount, f);

	// Packed clusters
	if (NumClusters > 0)
		fwrite(PackedClusters.data(), sizeof(FPackedCluster), NumClusters, f);

	// Hierarchy nodes
	if (NumHierarchyNodes > 0)
		fwrite(HierarchyNodes.data(), sizeof(FPackedHierarchyNode), NumHierarchyNodes, f);

	fclose(f);
	return true;
}

bool QuarkGeometryAsset::LoadFromFile(const char* FilePath)
{
	FILE* f = fopen(FilePath, "rb");
	if (!f) return false;

	// Header
	UINT Magic = 0;
	fread(&Magic, sizeof(UINT), 1, f);
	if (Magic != QUARK_ASSET_MAGIC)
	{
		fclose(f);
		return false;
	}

	fread(&NumClusters, sizeof(UINT), 1, f);
	fread(&NumHierarchyNodes, sizeof(UINT), 1, f);
	fread(&NumMipLevels, sizeof(UINT), 1, f);
	fread(&RootLODError, sizeof(float), 1, f);

	// Mesh bounds
	v3dxVector3 BMin, BMax;
	fread(&BMin, sizeof(v3dxVector3), 1, f);
	fread(&BMax, sizeof(v3dxVector3), 1, f);
	MeshBounds = v3dxBox3(BMin, BMax);

	// Packed positions
	UINT PosCount = 0;
	fread(&PosCount, sizeof(UINT), 1, f);
	PackedPositions.resize(PosCount);
	if (PosCount > 0)
		fread(PackedPositions.data(), sizeof(float), PosCount, f);

	// Packed indices
	UINT IdxCount = 0;
	fread(&IdxCount, sizeof(UINT), 1, f);
	PackedIndices.resize(IdxCount);
	if (IdxCount > 0)
		fread(PackedIndices.data(), sizeof(UINT), IdxCount, f);

	// Packed clusters
	PackedClusters.resize(NumClusters);
	if (NumClusters > 0)
		fread(PackedClusters.data(), sizeof(FPackedCluster), NumClusters, f);

	// Hierarchy nodes
	HierarchyNodes.resize(NumHierarchyNodes);
	if (NumHierarchyNodes > 0)
		fread(HierarchyNodes.data(), sizeof(FPackedHierarchyNode), NumHierarchyNodes, f);

	fclose(f);
	return true;
}

NS_END
