#include "IMeshSimplify.h"
#include "Simplify.h"
#include "meshoptimizer.h"

#define new VNEW

NS_BEGIN

void IMeshSimplify::SimplifyMesh(const v3dxVector3* posArray, int numOfPos, const IMeshTriangle* Indices, int numOfTri,
			v3dxVector3* outPosArray, int* outNumOfPos, IMeshTriangle* outIndices, int* outNumOfTri,
			int target_count, double agressiveness, bool verbose)
{
	Simplify::vertices.clear();
	Simplify::triangles.clear();
	for (int i = 0; i < numOfPos; i++)
	{
		Simplify::Vertex v; 
		v.p = vec3f(posArray[i].X, posArray[i].Y, posArray[i].Z);
		Simplify::vertices.push_back(v);
	}
	for (int i = 0; i < numOfTri; i++)
	{
		Simplify::Triangle t;
		t.v[0] = Indices[i * 3].A;
		t.v[1] = Indices[i * 3].B;
		t.v[2] = Indices[i * 3].C;
		t.material = Indices[i * 3].Material;
		Simplify::triangles.push_back(t);
	}

	Simplify::simplify_mesh(target_count, agressiveness, verbose);

	*outNumOfPos = (int)Simplify::vertices.size();
	if (outPosArray != nullptr)
	{
		for (int i = 0; i < *outNumOfPos; i++)
		{
			outPosArray[i].setValue((float)Simplify::vertices[i].p.x, (float)Simplify::vertices[i].p.y, (float)Simplify::vertices[i].p.z);
		}
	}

	*outNumOfTri = (int)Simplify::triangles.size();
	if (outPosArray != nullptr)
	{
		for (int i = 0; i < *outNumOfTri; i++)
		{
			outIndices[i * 3].A = Simplify::triangles[i].v[0];
			outIndices[i * 3].B = Simplify::triangles[i].v[1];
			outIndices[i * 3].C = Simplify::triangles[i].v[2];
			outIndices[i * 3].Material = Simplify::triangles[i].material;
		}
	}

	Simplify::vertices.clear();
	Simplify::triangles.clear();
}

void IMeshOptimizer::OptimizeVertexCache(unsigned int* destination, const unsigned int* indices, UINT index_count, UINT vertex_count)
{
	meshopt_optimizeVertexCache(destination, indices, (size_t)index_count, (size_t)vertex_count);
}

void IMeshOptimizer::OptimizeVertexCacheStrip(unsigned int* destination, const unsigned int* indices, UINT index_count, UINT vertex_count)
{
	meshopt_optimizeVertexCacheStrip(destination, indices, (size_t)index_count, (size_t)vertex_count);
}

void IMeshOptimizer::OptimizeMeshlet(unsigned int* meshlet_vertices, unsigned char* meshlet_triangles, UINT triangle_count, UINT vertex_count)
{
	meshopt_optimizeMeshlet(meshlet_vertices, meshlet_triangles, (size_t)triangle_count, (size_t)vertex_count);
}

NS_END
