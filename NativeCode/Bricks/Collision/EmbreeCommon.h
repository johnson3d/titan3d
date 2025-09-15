#pragma once
#include "rtcore.h"
#include "rtcore_ray.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"
#include "../../Math/v3dxVector3.h"

NS_BEGIN

struct FEmbreeGeometryInstance;
class FEmbreeScene;
class EmbreeManager;

struct TR_CLASS()
	FEmbreeGeometry : public VIUnknown
{
	AutoRef<NxRHI::FMeshDataProvider> MeshProvider;
	RTCGeometry InternalGeometry = nullptr;
	std::vector<UINT> IndexBuffer32;
	unsigned int GeomID = 0;
	~FEmbreeGeometry();

	virtual void SetGeometryTransform(const v3dxMatrix4& matrix);

	FEmbreeScene* AsTemplateScene(EmbreeManager* device);
	AutoRef<FEmbreeScene> TemplateScene;

	virtual NxRHI::FMeshDataProvider* GetMeshProvider() {
		return MeshProvider;
	}
};

struct TR_CLASS()
	FEmbreeGeometryInstance : public FEmbreeGeometry
{
	~FEmbreeGeometryInstance();
	AutoRef<FEmbreeGeometry> TemplateGeometry;
	
	virtual NxRHI::FMeshDataProvider* GetMeshProvider() override{
		return TemplateGeometry->GetMeshProvider();
	}
};

struct TR_CLASS(SV_LayoutStruct = 8)
	FHitResult
{
	v3dxVector3 HitNormal;
	float HitDistance = 0; // distance from ray origin to hit

	float U = 0;             // barycentric u coordinate of hit
	float V = 0;             // barycentric v coordinate of hit

	UINT PrimID = -1; // primitive ID
	FEmbreeGeometry* Geometry = nullptr; // geometry that was hit
	FEmbreeGeometry* GetGeometry() const { return Geometry; }
	void SetDefault()
	{
		HitDistance = 0;

		U = 0;
		V = 0;

		PrimID = -1;
		Geometry = nullptr;
	}
};

class TR_CLASS()
	FEmbreeScene : public VIUnknown
{
public:
	FEmbreeScene() :
		EmbreeDevice(nullptr),
		EmbreeScene(nullptr)
	{
	}
	~FEmbreeScene();

	// Embree
	RTCDevice EmbreeDevice = nullptr;
	RTCScene EmbreeScene = nullptr;
	std::map<UINT, AutoRef<FEmbreeGeometry>> Geometries;
	std::map<UINT, AutoRef<FEmbreeGeometryInstance>> GeometryInstances;

	void RemoveAllGeometries();
	void RemoveAllGeometryInstances();
	void AttachGeometry(FEmbreeGeometry* Geometry);
	void DetachGeometry(unsigned int geomID);
	void AttachGeometryInstance(FEmbreeGeometryInstance* Geometry);
	void DetachGeometryInstance(unsigned int geomID);
	void CommitScene();
	FEmbreeGeometry* FindGeometry(unsigned int geomID);
	void EmbreePointQuery(v3dxVector3 VoxelPosition, float LocalSpaceTraceDistance, bool& bOutNeedTracyRays, float& OutClosestDistance);
	bool EmbreeRayTrace(v3dxVector3 StartPosition, v3dxVector3 RayDirection, float minDist, float maxDist, FHitResult& OutHit);
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


struct FEmbreeIntersectionContext : public RTCRayQueryContext
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
	FEmbreeGeometry* CreateGeometry(VNameString meshName, NxRHI::FMeshDataProvider* meshProvider);
	FEmbreeGeometryInstance* CreateGeometryInstance(FEmbreeGeometry* geometry);
};


NS_END