#pragma once
#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector3.h"

NS_BEGIN

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
	void OptimizeVertexCache(unsigned int* destination, const unsigned int* indices, UINT index_count, UINT vertex_count);
	void OptimizeVertexCacheStrip(unsigned int* destination, const unsigned int* indices, UINT index_count, UINT vertex_count);
	void OptimizeMeshlet(unsigned int* meshlet_vertices, unsigned char* meshlet_triangles, UINT triangle_count, UINT vertex_count);
};

NS_END
