#include "DistanceField.h"

namespace EngineNS::DistanceField
{

	std::atomic<UINT64> NextDistanceFieldVolumeDataId{ 1 };

	FDistanceFieldVolumeData::FDistanceFieldVolumeData() :
		bMostlyTwoSided(false),
		bAsyncBuilding(false)
	{
		Id = NextDistanceFieldVolumeDataId++;
	}

	bool FDistanceFieldVolumeData::LoadXnd(EngineNS::XndHolder* xnd)
	{
		return true;
	}
	void FDistanceFieldVolumeData::Save2Xnd(EngineNS::XndNode* node)
	{
	}

	void GenerateSignedDistanceFieldVolumeData(
		VNameString MeshName,
		NxRHI::FMeshDataProvider &meshProvider,
		DistanceFieldConfig sdfConfig,
		float DistanceFieldResolutionScale,
		bool bGenerateAsIfTwoSided,
		FDistanceFieldVolumeData& OutData)
	{
		//if (DistanceFieldResolutionScale <= 0)
		//	return;

		//FEmbreeScene EmbreeScene;
		//SetupEmbreeScene(MeshName, meshProvider, DistanceFieldResolutionScale, EmbreeScene);

		//// Whether to use an Embree Point Query to compute the closest unsigned distance.  Rays will only be traced to determine backfaces visible for sign.
		//const bool bUsePointQuery = true;

		//std::vector<v3dxVector3> SampleDirections;
		//{
		//	const INT32 NumVoxelDistanceSamples = bUsePointQuery ? 49 : 576;
		//	GenerateStratifiedUniformHemisphereSamples(NumVoxelDistanceSamples, SampleDirections);
		//	std::vector<v3dxVector3> OtherHemisphereSamples;
		//	GenerateStratifiedUniformHemisphereSamples(NumVoxelDistanceSamples, OtherHemisphereSamples);

		//	for (INT32 i = 0; i < OtherHemisphereSamples.size(); i++)
		//	{
		//		v3dxVector3 Sample = OtherHemisphereSamples[i];
		//		Sample.Z *= -1.0f;
		//		SampleDirections.push_back(Sample);
		//	}
		//}

		//const INT32 PerMeshMax = sdfConfig.MaxPerMeshResolution;

		//// Meshes with explicit artist-specified scale can go higher
		//const INT32 MaxNumBlocksOneDim = std::min(Math::ICeil((DistanceFieldResolutionScale <= 1 ? PerMeshMax / 2 : PerMeshMax) / sdfConfig.UniqueDataBrickSize), sdfConfig.MaxIndirectionDimension - 1);

		//const float VoxelDensity = sdfConfig.fDefaultVoxelDensity;

		//const float NumVoxelsPerLocalSpaceUnit = VoxelDensity * DistanceFieldResolutionScale;
		//v3dxBox3 LocalSpaceMeshBounds;
		//meshProvider.GetAABB(&LocalSpaceMeshBounds);
		//// Make sure the mesh bounding box has positive extents to handle planes
		//{
		//	auto MeshBoundsCenter = LocalSpaceMeshBounds.GetCenter();
		//	v3dxVector3 MeshBoundsExtent = LocalSpaceMeshBounds.GetExtend();
		//	MeshBoundsExtent.makeCeil(v3dxVector3::UNIT_SCALE);
		//	LocalSpaceMeshBounds.Set(MeshBoundsCenter - MeshBoundsExtent, MeshBoundsCenter + MeshBoundsExtent);
		//}

		//// We sample on voxel corners and use central differencing for gradients, so a box mesh using two-sided materials whose vertices lie on LocalSpaceMeshBounds produces a zero gradient on intersection
		//// Expand the mesh bounds by a fraction of a voxel to allow room for a pullback on the hit location for computing the gradient.
		//// Only expand for two sided meshes as this adds significant Mesh SDF tracing cost
		//if (EmbreeScene.bMostlyTwoSided)
		//{
		//	// TODO
		//}

		//// The tracing shader uses a Volume space that is normalized by the maximum extent, to keep Volume space within [-1, 1], we must match that behavior when encoding
		//const float LocalToVolumeScale = 1.0f / LocalSpaceMeshBounds.GetExtend().getMax();

		//const v3dxVector3 DesiredDimensions = v3dxVector3(LocalSpaceMeshBounds.GetSize() * v3dxVector3::UNIT_SCALE*(NumVoxelsPerLocalSpaceUnit / (float)sdfConfig.UniqueDataBrickSize));
		//const v3dxVector3 Mip0IndirectionDimensions = v3dxVector3(
		//	std::clamp(Math::IFloor(DesiredDimensions.X), 1, MaxNumBlocksOneDim),
		//	std::clamp(Math::IFloor(DesiredDimensions.Y), 1, MaxNumBlocksOneDim),
		//	std::clamp(Math::IFloor(DesiredDimensions.Z), 1, MaxNumBlocksOneDim));

		//std::vector<UINT8> StreamableMipData;

		//for (INT32 MipIndex = 0; MipIndex < sdfConfig.NumMips; MipIndex++)
		//{
		//	const v3dxVector3 IndirectionDimensions = v3dxVector3(
		//		Math::ICeil(Mip0IndirectionDimensions.X / (1 << MipIndex)),
		//		Math::ICeil(Mip0IndirectionDimensions.Y / (1 << MipIndex)),
		//		Math::ICeil(Mip0IndirectionDimensions.Z / (1 << MipIndex)));

		//	// Expand to guarantee one voxel border for gradient reconstruction using bilinear filtering
		//	const v3dxVector3 TexelObjectSpaceSize = (v3dxVector3::UNIT_SCALE*LocalSpaceMeshBounds.GetSize()) / (IndirectionDimensions * sdfConfig.UniqueDataBrickSize - v3dxVector3::UNIT_SCALE*(2 * sdfConfig.MeshDistanceFieldObjectBorder));
		//	v3dxBox3 DistanceFieldVolumeBounds = LocalSpaceMeshBounds;
		//	DistanceFieldVolumeBounds.MergeVertex(TexelObjectSpaceSize);

		//	const v3dxVector3 IndirectionVoxelSize = v3dxVector3::UNIT_SCALE*DistanceFieldVolumeBounds.GetSize() / IndirectionDimensions;
		//	const float IndirectionVoxelRadius = IndirectionVoxelSize.getLength();

		//	const v3dxVector3 VolumeSpaceDistanceFieldVoxelSize = IndirectionVoxelSize * LocalToVolumeScale / (v3dxVector3::UNIT_SCALE*(sdfConfig.UniqueDataBrickSize));
		//	const float MaxDistanceForEncoding = VolumeSpaceDistanceFieldVoxelSize.getLength() * sdfConfig.BandSizeInVoxels;
		//	const float LocalSpaceTraceDistance = MaxDistanceForEncoding / LocalToVolumeScale;
		//	const v3dxVector2 DistanceFieldToVolumeScaleBias(2.0f * MaxDistanceForEncoding, -MaxDistanceForEncoding);


		//}

	}

	void GenerateSignedDistanceFiledBrick(
		const FEmbreeScene& InEmbreeScene,
		const std::vector<v3dxVector3>* InSampleDirections,
		float InLocalSpaceTraceDistance,
		v3dxBox3 InVolumeBounds,
		float InLocalToVolumeScale,
		v3dxVector2 InDistanceFieldToVolumeScaleBias,
		v3dxVector2 InBrickCoordinate,
		v3dxVector2 InIndirectionSize,
		bool bInUsePointQuery
	)
	{
		
	}

}
