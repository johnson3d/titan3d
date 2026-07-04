#pragma once

#include "ClusterDAG.h"
#include "../../Math/v3dxSphere.h"
#include "../../Math/v3dxBox3.h"
#include <vector>

NS_BEGIN

// Max children per BVH node (8-way tree like UE Nanite)
static const UINT QUARK_MAX_BVH_NODE_FANOUT = 8;

// Packed cluster data for GPU upload
struct FPackedCluster
{
	// LOD bounding sphere (xyz=center, w=radius)
	float LODBounds[4];      // v3dxSphere as float4

	// AABB
	float BoxCenter[3];
	float BoxExtent[3];

	// LOD metadata
	float LODError;           // Simplification error of this cluster
	float EdgeLength;         // Max edge length (for SW/HW raster selection)

	// Geometry data offsets into global vertex/index buffers
	UINT VertexStart;
	UINT VertexCount;
	UINT IndexStart;
	UINT IndexCount;

	// Material and level info
	UINT MaterialIndex;
	UINT MipLevel;
	UINT Flags;              // bit0: bLeaf, bit1: bStreamingLeaf, etc.
	UINT Padding;
};

// A single BVH node with up to 8 children (either other nodes or clusters)
struct FPackedHierarchyNode
{
	// Per-child LOD bounding sphere (for LOD projection)
	float ChildLODBounds[QUARK_MAX_BVH_NODE_FANOUT][4];  // [childIdx][xyzr]

	// Per-child AABB (for frustum/HZB culling)
	float ChildBoxCenter[QUARK_MAX_BVH_NODE_FANOUT][3];
	float ChildBoxExtent[QUARK_MAX_BVH_NODE_FANOUT][3];

	// Per-child LOD error range
	float ChildMaxParentLODError[QUARK_MAX_BVH_NODE_FANOUT]; // Error of the group that generated this child
	float ChildMinLODError[QUARK_MAX_BVH_NODE_FANOUT];       // Min LODError in subtree (for early-out)

	// Per-child reference: index into either HierarchyNodes or PackedClusters
	UINT ChildStartReference[QUARK_MAX_BVH_NODE_FANOUT]; // Start index
	UINT ChildCount[QUARK_MAX_BVH_NODE_FANOUT];          // Number of sub-items

	// Per-child flags
	UINT ChildFlags[QUARK_MAX_BVH_NODE_FANOUT]; // bit0: bLeaf (points to clusters, not nodes)

	// Number of valid children in this node (1..8)
	UINT NumChildren;
	UINT Padding[3];
};

// The complete packed geometry asset ready for GPU upload
struct QuarkGeometryAsset
{
	// === Cluster Geometry Data ===
	std::vector<float> PackedPositions;     // float3 per vertex, tightly packed
	std::vector<float> PackedNormals;       // float3 per vertex (placeholder for future)
	std::vector<float> PackedUVs;           // float2 per vertex (placeholder for future)
	std::vector<UINT>  PackedIndices;       // cluster-local triangle indices

	// === Cluster Metadata ===
	std::vector<FPackedCluster> PackedClusters;

	// === Hierarchy ===
	std::vector<FPackedHierarchyNode> HierarchyNodes;

	// === Meta info ===
	UINT NumClusters = 0;
	UINT NumHierarchyNodes = 0;
	UINT NumMipLevels = 0;
	float RootLODError = 0.0f;            // Error at the root (coarsest level)
	v3dxBox3 MeshBounds;                   // World-space bounds of the full mesh

	// === Build from DAG ===
	void BuildFromDAG(const FClusterDAG& DAG);

	// === Serialization ===
	bool SaveToFile(const char* FilePath) const;
	bool LoadFromFile(const char* FilePath);

	// === Query ===
	UINT GetTotalVertexCount() const { return (UINT)(PackedPositions.size() / 3); }
	UINT GetTotalIndexCount() const { return (UINT)PackedIndices.size(); }
};

// Hierarchy builder: constructs BVH from the flat cluster list + DAG groups
class FHierarchyBuilder
{
public:
	// Build BVH from the DAG
	// Returns the packed hierarchy nodes
	static void Build(const FClusterDAG& DAG, std::vector<FPackedHierarchyNode>& OutNodes);

private:
	struct FBuildNode
	{
		std::vector<UINT> ClusterIndices;  // Cluster indices in this subtree (leaf level)
		v3dxBox3 Bounds;
		v3dxSphere LODBounds;
		float MaxLODError = 0.0f;
		float MinLODError = FLT_MAX;
		bool bIsLeaf = false;
	};

	// Recursively build BVH top-down
	static UINT BuildRecursive(
		const FClusterDAG& DAG,
		const std::vector<UINT>& ClusterIndices,
		std::vector<FPackedHierarchyNode>& OutNodes);

	// Partition cluster indices into up to 8 spatial groups
	static void SpatialPartition(
		const FClusterDAG& DAG,
		const std::vector<UINT>& ClusterIndices,
		std::vector<std::vector<UINT>>& OutPartitions);
};

NS_END
