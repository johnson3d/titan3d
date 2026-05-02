#include "IMeshSimplify.h"
#include "Simplify.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"
#include "meshoptimizer.h"

#include <CGAL/Exact_predicates_inexact_constructions_kernel.h>
#include <CGAL/Delaunay_triangulation_3.h>
#include <CGAL/point_generators_3.h>
#include <CGAL/Triangulation_vertex_base_with_info_3.h>

#pragma comment(lib,"gmp.lib")

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

UINT IMeshOptimizer::BuildMeshlets(IBlobObject* meshlets, IBlobObject* meshletMaterials, IBlobObject* meshlet_vertices, IBlobObject* meshlet_triangles,
	NxRHI::FMeshDataProvider* mesh,
	UINT max_vertices, UINT max_triangles, float cone_weight)
{
	UINT result = 0;
	mesh->ConvertToIndex32();
	auto pIndices = mesh->IndexBuffer->GetDataPtr<UINT>();
	auto vb = mesh->mVertexBuffers[NxRHI::VST_Position];
	//这里其实需要分材质Build
	for (UINT i = 0; i < mesh->GetAtomNumber(); i++)
	{
		auto pAtom = mesh->GetAtom(i, 0);
		std::vector<UINT> indices;
		for (UINT j = 0; j < pAtom->NumPrimitives; j++)
		{
			auto a = pIndices[pAtom->StartIndex + j * 3 + 0];
			auto b = pIndices[pAtom->StartIndex + j * 3 + 1];
			auto c = pIndices[pAtom->StartIndex + j * 3 + 2];
			indices.push_back(a);
			indices.push_back(b);
			indices.push_back(c);
		}

		auto max_meshlets = (mesh->GetPrimitiveNumber() / max_triangles) * 10;
		std::vector<meshopt_Meshlet> t_meshlets;
		t_meshlets.resize(max_meshlets);
		std::vector<UINT> t_meshlet_vertices;
		t_meshlet_vertices.resize(mesh->GetVertexNumber());
		std::vector<BYTE> t_meshlet_triangles;
		t_meshlet_triangles.resize(max_triangles * max_meshlets);
		auto numOfMeshlets = (UINT)meshopt_buildMeshlets(&t_meshlets[0], &t_meshlet_vertices[0], &t_meshlet_triangles[0],
			indices.data(), pAtom->NumPrimitives * 3,
			vb->GetDataPtr<float>(), mesh->VertexNumber, sizeof(v3dxVector3),
			max_vertices, max_triangles, cone_weight);

		for (UINT i = 0; i < numOfMeshlets; i++)
		{
			BYTE mtl = (BYTE)i;
			meshletMaterials->PushData(&mtl, sizeof(BYTE));
		}

		meshlets->PushData(t_meshlets.data(), numOfMeshlets * sizeof(meshopt_Meshlet));
		meshlet_vertices->PushData(t_meshlet_vertices.data(), (UINT)t_meshlet_vertices.size() * sizeof(UINT));
		meshlet_triangles->PushData(t_meshlet_triangles.data(), (UINT)t_meshlet_triangles.size() * sizeof(BYTE));
		result += numOfMeshlets;
		
		//temp code
		break;
	}
		
	return result;
}

typedef CGAL::Exact_predicates_inexact_constructions_kernel K;

typedef CGAL::Triangulation_vertex_base_with_info_3<int, K> Vb;
typedef CGAL::Triangulation_cell_base_3<K> Cb;
typedef CGAL::Triangulation_data_structure_3<Vb, Cb> Tds;

// 定义Delaunay三角剖分类型，带int的info 
typedef CGAL::Delaunay_triangulation_3<K, Tds> Delaunay;
//typedef CGAL::Delaunay_triangulation_3<K> Delaunay;
// 定义点类型
typedef K::Point_3 Point_3;
// 定义四面体（单元）类型
typedef Delaunay::Cell_handle Cell_handle;
typedef Delaunay::Vertex_handle Vertex_handle;

bool VPointCloud::BuildTetrahedron(v3dxVector3* positions, int num, IBlobObject* outTraahedrons)
{
	// 创建一个随机点云作为示例
	std::vector<std::pair<Point_3, int>> points;
	for (int i = 0; i < num; ++i) 
	{
		Point_3 t(positions[i].X, positions[i].Y, positions[i].Z);
		points.push_back({ t, i});
	}

	// 创建Delaunay三角剖分
	Delaunay dt(points.begin(), points.end());

	// 验证三角剖分是否有效
	if (dt.is_valid() == false)
		return false;

	// 输出基本信息
	std::cout << "Number of vertices: " << dt.number_of_vertices() << std::endl;
	std::cout << "Number of finite cells (tetrahedra): " << dt.number_of_finite_cells() << std::endl;
	std::cout << "Number of infinite cells: " << dt.number_of_cells() - dt.number_of_finite_cells() << std::endl;

	outTraahedrons->ReSize(0);
	outTraahedrons->PushData((UINT)dt.number_of_finite_cells());
	// 迭代遍历所有有限四面体（单元）
	for (Cell_handle cell : dt.finite_cell_handles()) 
	{	
		// 每个四面体有4个顶点
		for (int i = 0; i < 4; ++i) 
		{
			Vertex_handle vh = cell->vertex(i);
			// 确保顶点有效（无限顶点不存在）
			if (dt.is_infinite(vh)) 
			{
				outTraahedrons->PushData((int)-1);
			}
			else 
			{
				auto index = vh->info();
				ASSERT(index < num);
				outTraahedrons->PushData((int)index);
			}
		}
	}
	return true;
}

NS_END
