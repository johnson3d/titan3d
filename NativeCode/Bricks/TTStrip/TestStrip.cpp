#include "TTStrip.h"
#include "TTStripifier.h"
#include "SurfaceAreaHeuristic.h"

#include "../../Math/v3dxVector3.h"

#include <vector>


void BuildOccluders_TTStrip_Test(const std::vector<v3dxVector3>& Vertices, const std::vector<UInt16>& Indices, std::vector<std::vector<uint32_t>>& OutputIndices)
{
	if (Vertices.size() == 0 || Indices.size() == 0)
	{
		return;
	}

	Int32 MeshClusterStripAngleThreshold = 45;
	float AngleThresholdForStripPartition = (float)MeshClusterStripAngleThreshold;

	float kSelectConnectedStripExperimentAngleWeight = 1.2f;
	bool FillDummyVerticesStrips = false;
	bool MergeFinalStrips = false;

	Int32 GenerateClusterSize = 256;

	// Do Stripification.
	TTStrip::FStripifier Stripifier(GenerateClusterSize, AngleThresholdForStripPartition, AngleThresholdForStripPartition, kSelectConnectedStripExperimentAngleWeight, 0, 8, FillDummyVerticesStrips, MergeFinalStrips);

	std::vector<TTStrip::Float3> TVertices;
	std::vector<uint32_t> TIndices;

	TVertices.resize(Vertices.size());
	uint32_t counter = 0;


	for (const v3dxVector3& BuildVertex : Vertices)
	{
		TVertices[counter] = TTStrip::Float3{ BuildVertex[0], BuildVertex[1], BuildVertex[2]};
		counter++;

	}

	TIndices.resize(Indices.size());
	for (Int32 i = 0; i < Indices.size(); i += 3)
	{
		TIndices[i] = Indices[i];
		TIndices[i + 1] = Indices[i + 1];
		TIndices[i + 2] = Indices[i + 2];
	}

	// Do LQ Stripification.
	std::vector<std::vector<uint32_t>>* pOutputIndices = &Stripifier.Stripify(TVertices, TIndices);

	OutputIndices.resize(pOutputIndices->size());
	for (uint32_t i = 0; i < pOutputIndices->size(); i++)
	{
		OutputIndices[i].resize((*pOutputIndices)[i].size());

		memcpy(OutputIndices[i].data(), (*pOutputIndices)[i].data(), OutputIndices[i].size() * sizeof(uint32_t));
		//for (uint32_t j = 0; j < (*pOutputIndices)[i].size(); j++)
		//{
		//	OutputIndices[i][j] = (*pOutputIndices)[i][j];
		//}
	}
}

void BuildOccluders_Quad_Test(const std::vector<v3dxVector3>& Vertices, const std::vector<UInt16>& Indices)
{


	const auto& vertices = Vertices;

	std::vector<v3dxBox3> Aabbs;
	Aabbs.resize(Indices.size() / 3);
	for (Int32 quadIndex = 0; quadIndex < Indices.size() / 3; ++quadIndex)
	{
		v3dxBox3 aabb;
		aabb += vertices[Indices[3 * quadIndex + 0]];
		aabb += vertices[Indices[3 * quadIndex + 1]];
		aabb += vertices[Indices[3 * quadIndex + 2]];
		Aabbs.push_back(aabb);
	}

	auto batchAssignment = SurfaceAreaHeuristic::generateBatches(Aabbs, 128, 4);

	v3dxBox3 refAabb;
	for (auto v : vertices)
	{
		refAabb += v;
	}

	// Bake occluders
	for (const auto& batch : batchAssignment)
	{
		std::vector<v3dxVector3> batchVertices;

		for (auto quadIndex : batch)
		{
			batchVertices.push_back(vertices[Indices[quadIndex * 3 + 0]]);
			batchVertices.push_back(vertices[Indices[quadIndex * 3 + 1]]);
			batchVertices.push_back(vertices[Indices[quadIndex * 3 + 2]]);
		}

		//Bake batchVertices for occluder.. batchVertices is cluster..
	}
}
