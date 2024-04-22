#pragma once
#include "EmbreeCommon.h"
#include "xnd/vfxxnd.h"

namespace EngineNS::DistanceField
{
	struct DistanceFieldConfig
	{
		// One voxel border around object for handling gradient
		INT32 MeshDistanceFieldObjectBorder = 1;
		INT32 UniqueDataBrickSize = 7;
		// Trade off between SDF memory and number of steps required to find intersection
		INT32 BandSizeInVoxels = 4;

		INT32 MaxPerMeshResolution = 256;
		INT32 MaxIndirectionDimension = 1024;

		float fDefaultVoxelDensity = 20.0f;

		INT32 NumMips = 3;
	};

	/** Distance field data payload and output of the mesh build process. */
	class FDistanceFieldVolumeData
	{
	public:

		/** Local space bounding box of the distance field volume. */
		v3dxBox3 LocalSpaceMeshBounds;

		/** Whether most of the triangles in the mesh used a two-sided material. */
		bool bMostlyTwoSided;

		bool bAsyncBuilding;

		UINT64 Id;

		// For stats
		VNameString AssetName;

		FDistanceFieldVolumeData();

		UINT64 GetId() const { return Id; }

		bool LoadXnd(XndHolder* xnd);
		void Save2Xnd(XndNode* node);
	};

	void GenerateSignedDistanceFieldVolumeData(
		VNameString MeshName,
		NxRHI::FMeshDataProvider& meshProvider,
		DistanceFieldConfig sdfConfig,
		float DistanceFieldResolutionScale,
		bool bGenerateAsIfTwoSided,
		FDistanceFieldVolumeData& OutData);

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
	);

}
