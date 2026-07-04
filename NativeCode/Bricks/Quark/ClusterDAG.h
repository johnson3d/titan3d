#pragma once

#include "Cluster.h"
#include "GraphPartitioner.h"
#include "../../Math/v3dxSphere.h"
#include "../../Math/v3dxBox3.h"
#include <vector>
#include <map>
#include <functional>

NS_BEGIN

// Forward declarations
class QuarkCluster;
class FGraphPartitioner;

// A group of adjacent clusters that will be simplified together to produce parent clusters.
// Analogous to UE Nanite's FClusterGroup.
struct FClusterGroup
{
	// Child cluster indices in FClusterDAG::Clusters array
	std::vector<UINT> Children;

	// Parent cluster indices (generated after ReduceGroup)
	std::vector<UINT> Parents;

	// LOD bounding sphere: encompasses all children, used for runtime LOD projection
	v3dxSphere LODBounds;

	// The simplification error of the parent level
	// Guarantee: ParentLODError >= max(child.LODError) for all children
	float ParentLODError = 0.0f;

	// DAG level (0 = leaf groups, increases upward)
	int MipLevel = 0;

	// AABB bounds of the group (union of all child cluster bounds)
	v3dxBox3 Bounds;
};

// The complete Cluster DAG: stores all clusters across all LOD levels,
// all groups, and the hierarchy relationships.
class FClusterDAG
{
public:
	FClusterDAG(UINT InMinGroupSize = 8, UINT InMaxGroupSize = 32, UINT InClusterSize = 128)
		: MinGroupSize(InMinGroupSize), MaxGroupSize(InMaxGroupSize), ClusterSize(InClusterSize) {}
	~FClusterDAG();

	// === Build Pipeline ===

	// Step 1: Add the base mesh and perform initial clustering (Level 0)
	// Returns the number of clusters created at level 0
	UINT AddMesh(
		const std::vector<v3dxVector3>& Verts,
		const std::vector<UINT>& Indexes,
		const std::vector<INT32>& MaterialIndexes);

	// Step 2: Build the complete DAG hierarchy
	// Iteratively groups clusters and reduces until convergence
	void BuildDAG();

	// === Query ===

	UINT GetNumClusters() const { return (UINT)Clusters.size(); }
	UINT GetNumGroups() const { return (UINT)Groups.size(); }
	UINT GetNumMipLevels() const { return NumMipLevels; }

	// Get all cluster indices at a specific mip level
	std::vector<UINT> GetClustersAtLevel(int Level) const;

	// Print DAG statistics for debugging
	void PrintDAGInfo() const;

public:
	// All clusters across all levels (Level 0 = finest, Level N = coarsest)
	std::vector<QuarkCluster*> Clusters;

	// All groups (each group links children→parents across one level boundary)
	std::vector<FClusterGroup> Groups;

	// Number of mip levels in the hierarchy
	UINT NumMipLevels = 0;

	// Index ranges per mip level: MipLevelStart[level] = first cluster index at that level
	std::vector<UINT> MipLevelStart;

	// Configurable group size parameters
	UINT MinGroupSize = 8;
	UINT MaxGroupSize = 32;
	UINT ClusterSize = 128; // Max triangles per cluster (UE Nanite default: 128)

private:
	// Internal: Group clusters at the current frontier level
	// Returns groups formed from clusters at `Level`
	void GroupClusters(int Level, std::vector<FClusterGroup>& OutGroups);

	// Internal: Reduce a single group (merge → simplify → split → create parent clusters)
	// Returns indices of newly created parent clusters
	void ReduceGroup(FClusterGroup& Group);

	// Internal: Build adjacency between clusters (for grouping)
	void BuildClusterAdjacency(const std::vector<UINT>& ClusterIndices);

	// Internal: Compute group LODBounds (union bounding sphere of all children)
	void ComputeGroupBounds(FClusterGroup& Group);
};

NS_END
