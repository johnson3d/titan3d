#pragma once
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector3.h"

NS_BEGIN

struct IBlobObject;
namespace NxRHI
{
	class FMeshDataProvider;
}

struct TR_CLASS(SV_LayoutStruct = 8)
	IMeshTriangle
{
	int A;
	int B;
	int C;
	int Material;
};

class TR_CLASS(SV_Dispose = delete self)
IMeshSimplify
{
public:
	static void SimplifyMesh(const v3dxVector3* posArray, int numOfPos, const IMeshTriangle* Indices, int numOfTri,
						v3dxVector3* outPosArray, int* outNumOfPos, IMeshTriangle* outIndices, int* outNumOfTri,
						int target_count, double agressiveness = 7, bool verbose = false);
};

class TR_CLASS(SV_Dispose = delete self)
	IMeshOptimizer
{
public:
	static void OptimizeVertexCache(unsigned int* destination, const unsigned int* indices, UINT index_count, UINT vertex_count);
	static void OptimizeVertexCacheStrip(unsigned int* destination, const unsigned int* indices, UINT index_count, UINT vertex_count);
	static void OptimizeMeshlet(unsigned int* meshlet_vertices, unsigned char* meshlet_triangles, UINT triangle_count, UINT vertex_count);

	static UINT BuildMeshlets(IBlobObject* meshlets, IBlobObject* meshletMaterials, IBlobObject* meshlet_vertices, IBlobObject* meshlet_triangles,
		NxRHI::FMeshDataProvider* mesh,
		UINT max_vertices, UINT max_triangles, float cone_weight = 0);
};

NS_END
