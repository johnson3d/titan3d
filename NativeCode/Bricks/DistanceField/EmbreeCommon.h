#pragma once
#include "embree3/rtcore.h"
#include "embree3/rtcore_ray.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"
#include "../../Math/v3dxVector3.h"

NS_BEGIN

struct FEmbreeTriangleDesc
{
	INT16 ElementIndex;

	bool IsTwoSided() const
	{
		// MaterialIndex on the build triangles was set to 1 if two-sided, or 0 if one-sided
		return ElementIndex == 1;
	}
};

struct TR_CLASS()
FEmbreeGeometry : public VIUnknown
{
	std::vector<UINT> IndexArray;
	std::vector<v3dxVector3> VertexArray;
	std::vector<FEmbreeTriangleDesc> TriangleDescs; // The material ID of each triangle.
	RTCGeometry InternalGeometry;
};

class TR_CLASS()
FEmbreeScene : public VIUnknown
{
public:
	FEmbreeScene() :
		NumIndices(0),
		bMostlyTwoSided(false),
		EmbreeDevice(nullptr),
		EmbreeScene(nullptr)
	{}

	INT32 NumIndices = 0;
	bool bMostlyTwoSided = false;

	// Embree
	RTCDevice EmbreeDevice = nullptr;
	RTCScene EmbreeScene = nullptr;
	FEmbreeGeometry Geometry;
};

class FEmbreeRay : public RTCRayHit
{
public:
	FEmbreeRay() :
		ElementIndex(-1)
	{
		hit.u = hit.v = 0;
		ray.time = 0;
		ray.mask = 0xFFFFFFFF;
		hit.geomID = RTC_INVALID_GEOMETRY_ID;
		hit.instID[0] = RTC_INVALID_GEOMETRY_ID;
		hit.primID = RTC_INVALID_GEOMETRY_ID;
	}

	v3dxVector3 GetHitNormal() const
	{
		return v3dxVector3(-hit.Ng_x, -hit.Ng_y, -hit.Ng_z).getNormal();
	}

	bool IsHitTwoSided() const
	{
		// MaterialIndex on the build triangles was set to 1 if two-sided, or 0 if one-sided
		return ElementIndex == 1;
	}

	// Additional Outputs.
	INT32 ElementIndex; // Material Index
};


struct FEmbreeIntersectionContext : public RTCIntersectContext
{
	FEmbreeIntersectionContext() :
		ElementIndex(-1)
	{}

	bool IsHitTwoSided() const
	{
		// MaterialIndex on the build triangles was set to 1 if two-sided, or 0 if one-sided
		return ElementIndex == 1;
	}

	// Hit against this primitive will be ignored
	INT32 SkipPrimId = RTC_INVALID_GEOMETRY_ID;

	// Additional Outputs.
	INT32 ElementIndex; // Material Index
};

class TR_CLASS()
EmbreeManager : public VIUnknown
{
public:
	EmbreeManager() {}

	void SetupEmbreeScene(VNameString meshName, NxRHI::FMeshDataProvider& meshProvider, float DistanceFieldResolutionScale, FEmbreeScene& embreeScene);

	void DeleteEmbreeScene(FEmbreeScene& embreeScene);

	void EmbreePointQuery(FEmbreeScene& embreeScene, v3dxVector3 VoxelPosition, float LocalSpaceTraceDistance, bool &bOutNeedTracyRays, float &OutClosestDistance);
	void EmbreeRayTrace(FEmbreeScene& embreeScene, v3dxVector3 StartPosition, v3dxVector3 RayDirection, bool& bOutHit, bool& bOutHitTwoSided, v3dxVector3& OutHitNormal, float& OutTFar);
};


NS_END