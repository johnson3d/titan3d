#pragma once
#include "embree3/rtcore.h"
#include "embree3/rtcore_ray.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"
#include "../../Math/v3dxVector3.h"

NS_BEGIN

struct FEmbreeGeometryInstance;
class FEmbreeScene;
class EmbreeManager;

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
	RTCGeometry InternalGeometry = nullptr;
	unsigned int GeomID = 0;
	~FEmbreeGeometry();

	virtual void SetGeometryTransform(const v3dxMatrix4& matrix);

	FEmbreeScene* AsTemplateScene(EmbreeManager* device);
	AutoRef<FEmbreeScene> TemplateScene;

	virtual std::vector<v3dxVector3>& GetVertexArray() {
		return VertexArray;
	}
	virtual std::vector<UINT> GetIndexArray() {
		return IndexArray;
	}
	virtual std::vector<FEmbreeTriangleDesc>& GetTriangleDescs() {
		return TriangleDescs;
	}
};

struct TR_CLASS()
	FEmbreeGeometryInstance : public FEmbreeGeometry
{
	~FEmbreeGeometryInstance();
	AutoPtr<FEmbreeGeometry> TemplateGeometry;

	virtual std::vector<v3dxVector3>& GetVertexArray() override {
		return TemplateGeometry->GetVertexArray();
	}
	virtual std::vector<UINT> GetIndexArray() override {
		return TemplateGeometry->GetIndexArray();
	}
	virtual std::vector<FEmbreeTriangleDesc>& GetTriangleDescs() override {
		return TemplateGeometry->GetTriangleDescs();
	}
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
	{
}
~FEmbreeScene();

INT32 NumIndices = 0;
bool bMostlyTwoSided = false;

// Embree
RTCDevice EmbreeDevice = nullptr;
RTCScene EmbreeScene = nullptr;
TR_MEMBER(SV_NoBind)
FEmbreeGeometry Geometry;
void AttachGeometry(FEmbreeGeometry* Geometry);
void DetachGeometry(unsigned int geomID);
void AttachGeometryInstance(FEmbreeGeometryInstance* Geometry);
void CommitScene();
FEmbreeGeometry* FindGeometry(unsigned int geomID);
void EmbreePointQuery(v3dxVector3 VoxelPosition, float LocalSpaceTraceDistance, bool& bOutNeedTracyRays, float& OutClosestDistance);
void EmbreeRayTrace(v3dxVector3 StartPosition, v3dxVector3 RayDirection, bool& bOutHit, bool& bOutHitTwoSided, v3dxVector3& OutHitNormal, float& OutTFar);
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
	{
	}

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
	RTCDevice EmbreeDevice = nullptr;
	UINT GeomIDAllocator = 0;
	EmbreeManager() {}
	~EmbreeManager();
	bool Initialize();
	FEmbreeScene* CreateScene();
	FEmbreeGeometry* CreateGeometry(VNameString meshName, NxRHI::FMeshDataProvider& meshProvider);
	FEmbreeGeometryInstance* CreateGeometryInstance(FEmbreeGeometry* geometry);

	void SetupEmbreeScene(VNameString meshName, NxRHI::FMeshDataProvider& meshProvider, float DistanceFieldResolutionScale, FEmbreeScene& embreeScene);

	void DeleteEmbreeScene(FEmbreeScene& embreeScene);

	void EmbreePointQuery(FEmbreeScene& embreeScene, v3dxVector3 VoxelPosition, float LocalSpaceTraceDistance, bool& bOutNeedTracyRays, float& OutClosestDistance);
	void EmbreeRayTrace(FEmbreeScene& embreeScene, v3dxVector3 StartPosition, v3dxVector3 RayDirection, bool& bOutHit, bool& bOutHitTwoSided, v3dxVector3& OutHitNormal, float& OutTFar);
};


NS_END