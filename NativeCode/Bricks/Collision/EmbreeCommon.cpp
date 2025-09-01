#include "EmbreeCommon.h"
#include "perlin/perlin.h"

#if defined(PLATFORM_WIN)
#pragma comment(lib,"embree3.lib")
#pragma comment(lib,"tbb.lib")
#endif

#define  new VNEW

NS_BEGIN

class FEmbreePointQueryContext : public RTCPointQueryContext
{
public:
	RTCGeometry MeshGeometry;
	INT32 NumTriangles;
	FEmbreeScene* Scene = nullptr;
};

void EmbreeFilterFunc(const struct RTCFilterFunctionNArguments* args)
{
	FEmbreeGeometry* EmbreeGeometry = (FEmbreeGeometry*)args->geometryUserPtr;
	
	FEmbreeIntersectionContext& IntersectionContext = *static_cast<FEmbreeIntersectionContext*>(args->context);
	
	const RTCHit& EmbreeHit = *(RTCHit*)args->hit;
	if (IntersectionContext.SkipPrimId != RTC_INVALID_GEOMETRY_ID && IntersectionContext.SkipPrimId == EmbreeHit.primID)
	{
		// Ignore hit in order to continue tracing
		args->valid[0] = 0;
	}
}

void EmbreeErrorFunc(void* userPtr, RTCError code, const char* str)
{
	VFX_LTRACE(ELTT_Error, "Embree error: %s Code=%u", str, (UINT)code);
}

FEmbreeGeometry::~FEmbreeGeometry()
{
	if (InternalGeometry)
	{
		rtcReleaseGeometry(InternalGeometry);
		InternalGeometry = nullptr;
	}
}

FEmbreeScene* FEmbreeGeometry::AsTemplateScene(EmbreeManager* device)
{
	if (TemplateScene == nullptr)
	{
		TemplateScene = MakeWeakRef(device->CreateScene());
		TemplateScene->AttachGeometry(this);
		TemplateScene->CommitScene();
	}
	return TemplateScene;
}

void FEmbreeGeometry::SetGeometryTransform(const v3dxMatrix4& matrix)
{
	rtcSetGeometryTransform(InternalGeometry, 0, RTC_FORMAT_FLOAT4X4_COLUMN_MAJOR, &matrix);
}

FEmbreeGeometryInstance::~FEmbreeGeometryInstance()
{
	if (InternalGeometry)
	{
		rtcReleaseGeometry(InternalGeometry);
		InternalGeometry = nullptr;
	}
}

FEmbreeScene::~FEmbreeScene()
{
	if (EmbreeScene)
	{
		rtcReleaseScene(EmbreeScene);
		EmbreeScene = nullptr;
	}
}

void FEmbreeScene::AttachGeometry(FEmbreeGeometry* Geometry)
{
	rtcAttachGeometryByID(EmbreeScene, Geometry->InternalGeometry, Geometry->GeomID);
}

void FEmbreeScene::DetachGeometry(unsigned int geomID)
{
	rtcDetachGeometry(EmbreeScene, geomID);
}

void FEmbreeScene::AttachGeometryInstance(FEmbreeGeometryInstance* Geometry)
{
	rtcAttachGeometryByID(EmbreeScene, Geometry->InternalGeometry, Geometry->GeomID);
}

void FEmbreeScene::CommitScene()
{
	rtcCommitScene(EmbreeScene);
}

FEmbreeGeometry* FEmbreeScene::FindGeometry(unsigned int geomID)
{
	auto geom = rtcGetGeometry(EmbreeScene, geomID);
	if (geom == nullptr)
		return nullptr;
	return (FEmbreeGeometry*)rtcGetGeometryUserData(geom);
}

bool EmbreePointQueryFunction2(RTCPointQueryFunctionArguments* args)
{
	return true;
}

void FEmbreeScene::EmbreePointQuery(v3dxVector3 VoxelPosition, float LocalSpaceTraceDistance, bool& bOutNeedTracyRays, float& OutClosestDistance)
{
	RTCPointQuery PointQuery;
	PointQuery.x = VoxelPosition.X;
	PointQuery.y = VoxelPosition.Y;
	PointQuery.z = VoxelPosition.Z;
	PointQuery.time = 0;
	PointQuery.radius = LocalSpaceTraceDistance;

	FEmbreePointQueryContext QueryContext;
	rtcInitPointQueryContext(&QueryContext);
	QueryContext.Scene = this;
	float ClosestUnsignedDistanceSq = (LocalSpaceTraceDistance * 2.0f) * (LocalSpaceTraceDistance * 2.0f);
	rtcPointQuery(EmbreeScene, &PointQuery, &QueryContext, EmbreePointQueryFunction2, &ClosestUnsignedDistanceSq);

	OutClosestDistance = Math::Sqrt(ClosestUnsignedDistanceSq);
	bOutNeedTracyRays = OutClosestDistance <= LocalSpaceTraceDistance;
}

bool FEmbreeScene::EmbreeRayTrace(v3dxVector3 StartPosition, v3dxVector3 RayDirection, FHitResult& OutHit)
{
	FEmbreeRay EmbreeRay;

	EmbreeRay.ray.org_x = StartPosition.X;
	EmbreeRay.ray.org_y = StartPosition.Y;
	EmbreeRay.ray.org_z = StartPosition.Z;
	EmbreeRay.ray.dir_x = RayDirection.X;
	EmbreeRay.ray.dir_y = RayDirection.Y;
	EmbreeRay.ray.dir_z = RayDirection.Z;
	EmbreeRay.ray.tnear = 0;
	EmbreeRay.ray.tfar = 1.0f;

	FEmbreeIntersectionContext EmbreeContext;
	rtcInitIntersectContext(&EmbreeContext);
	rtcIntersect1(EmbreeScene, &EmbreeContext, &EmbreeRay);

	OutHit.SetDefault();
	if (EmbreeRay.hit.primID != RTC_INVALID_GEOMETRY_ID)
	{
		OutHit.HitDistance = EmbreeRay.ray.tfar;
		OutHit.HitNormal = EmbreeRay.GetHitNormal();
		OutHit.U = EmbreeRay.hit.u;
		OutHit.V = EmbreeRay.hit.v;
		if (EmbreeRay.hit.instID[0] != RTC_INVALID_GEOMETRY_ID)
		{
			OutHit.PrimID = EmbreeRay.hit.instID[0];
			OutHit.Geometry = this->FindGeometry(OutHit.PrimID);
		}
		else
		{
			OutHit.PrimID = EmbreeRay.hit.primID;
			OutHit.Geometry = this->FindGeometry(OutHit.PrimID);
		}
		return true;
	}
	return false;
}

EmbreeManager::~EmbreeManager()
{
	if (EmbreeDevice)
	{
		rtcReleaseDevice(EmbreeDevice);
		EmbreeDevice = nullptr;
	}
}

bool EmbreeManager::Initialize()
{
	EmbreeDevice = rtcNewDevice(nullptr);
	rtcSetDeviceErrorFunction(EmbreeDevice, EmbreeErrorFunc, nullptr);
	RTCError ReturnErrorNewDevice = rtcGetDeviceError(EmbreeDevice);
	if (ReturnErrorNewDevice != RTC_ERROR_NONE)
	{
		return false;
	}
	return true;
}

FEmbreeScene* EmbreeManager::CreateScene()
{
	auto EmbreeScene = rtcNewScene(EmbreeDevice);
	rtcSetSceneFlags(EmbreeScene, RTC_SCENE_FLAG_NONE);
	RTCError ReturnErrorNewScene = rtcGetDeviceError(EmbreeDevice);
	if (ReturnErrorNewScene != RTC_ERROR_NONE)
	{
		return nullptr;
	}
	auto result = new FEmbreeScene();
	result->EmbreeScene = EmbreeScene;
	result->EmbreeDevice = EmbreeDevice;
	return result;
}

FEmbreeGeometry* EmbreeManager::CreateGeometry(VNameString meshName, NxRHI::FMeshDataProvider* meshProvider)
{
	UINT NumVertices = meshProvider->GetVertexNumber();
	UINT NumTriangles = meshProvider->GetPrimitiveNumber();
	UINT NumIndices = NumTriangles * 3;

	// 2, calculate valid triangles
	std::vector<INT32> FilteredTriangles;
	FilteredTriangles.reserve(NumTriangles);
	auto pPos = (v3dxVector3*)meshProvider->GetStream(NxRHI::VST_Position)->GetData();
	for (UINT triangleIndex = 0; triangleIndex < NumTriangles; ++triangleIndex)
	{
		UINT i0, i1, i2;
		if (meshProvider->GetTriangle(triangleIndex, &i0, &i1, &i2))
		{
			v3dxVector3 v0 = pPos[i0];
			v3dxVector3 v1 = pPos[i1];
			v3dxVector3 v2 = pPos[i2];

			//no filter for now, as we need to keep the same vertex index for embree
			/*v3dxVector3 triangleNormal = (v0 - v2).crossProduct(v1 - v2);
			bool bDegenerateTriangle = triangleNormal.getLengthSq() < SMALL_EPSILON;
			if (!bDegenerateTriangle)
				FilteredTriangles.push_back(triangleIndex);*/
		}
	}

	// 3, fill RTCGeometry(vertices,indices,triangleDesc) in embreeScene
	const INT32 NumBufferVerts = 1; // Reserve extra space at the end of the array, as embree has an internal bug where they read and discard 4 bytes off the end of the array
	FEmbreeGeometry* result = new FEmbreeGeometry();
	result->MeshProvider = meshProvider;
	result->VertexArray.resize(NumVertices + NumBufferVerts);

	const auto NumFilteredIndices = FilteredTriangles.size() * 3;

	result->IndexArray.resize(NumFilteredIndices);

	v3dxVector3* EmbreeVertices = result->VertexArray.data();
	UINT* EmbreeIndices = result->IndexArray.data();
	
	for (INT32 FilteredTriangleIndex = 0; FilteredTriangleIndex < FilteredTriangles.size(); FilteredTriangleIndex++)
	{
		UINT I0, I1, I2;
		v3dxVector3 V0, V1, V2;

		const INT32 TriangleIndex = FilteredTriangles[FilteredTriangleIndex];
		if (meshProvider->GetTriangle(TriangleIndex, &I0, &I1, &I2))
		{
			V0 = pPos[I0];
			V1 = pPos[I1];
			V2 = pPos[I2];
		}

		// TODO calculate bTriangleIsTwoSided from materials
		bool bTriangleIsTwoSided = false;

		EmbreeIndices[FilteredTriangleIndex * 3 + 0] = I0;
		EmbreeIndices[FilteredTriangleIndex * 3 + 1] = I1;
		EmbreeIndices[FilteredTriangleIndex * 3 + 2] = I2;

		EmbreeVertices[I0] = V0;
		EmbreeVertices[I1] = V1;
		EmbreeVertices[I2] = V2;
	}

	RTCGeometry rtcGeometry = rtcNewGeometry(EmbreeDevice, RTC_GEOMETRY_TYPE_TRIANGLE);
	result->InternalGeometry = rtcGeometry;

	rtcSetSharedGeometryBuffer(rtcGeometry, RTC_BUFFER_TYPE_VERTEX, 0, RTC_FORMAT_FLOAT3, EmbreeVertices, 0, sizeof(v3dxVector3), NumVertices);
	rtcSetSharedGeometryBuffer(rtcGeometry, RTC_BUFFER_TYPE_INDEX, 0, RTC_FORMAT_UINT3, EmbreeIndices, 0, sizeof(UINT) * 3, FilteredTriangles.size());

	rtcSetGeometryUserData(rtcGeometry, result);
	rtcSetGeometryIntersectFilterFunction(rtcGeometry, EmbreeFilterFunc);

	rtcCommitGeometry(rtcGeometry);

	result->GeomID = GeomIDAllocator++;
	return result;
}

FEmbreeGeometryInstance* EmbreeManager::CreateGeometryInstance(FEmbreeGeometry* geometry)
{
	auto result = new FEmbreeGeometryInstance();
	result->TemplateGeometry = geometry;
	result->InternalGeometry = rtcNewGeometry(EmbreeDevice, RTC_GEOMETRY_TYPE_INSTANCE);
	rtcSetGeometryInstancedScene(result->InternalGeometry, geometry->AsTemplateScene(this)->EmbreeScene);
	rtcSetGeometryUserData(result->InternalGeometry, result);
	rtcSetGeometryIntersectFilterFunction(result->InternalGeometry, EmbreeFilterFunc);
	result->GeomID = GeomIDAllocator++;
	return result;
}

void EmbreeManager::SetupEmbreeScene(VNameString meshName, NxRHI::FMeshDataProvider& meshProvider, float DistanceFieldResolutionScale, FEmbreeScene& embreeScene)
{
	//NxRHI::FMeshDataProvider meshProvider;
	//if (meshProvider.LoadFromMeshPrimitive(&meshPrimitive, NxRHI::EVertexStreamType::VST_FullMask) == false)
	//	return;

	UINT NumVertices = meshProvider.GetVertexNumber();
	UINT NumTriangles = meshProvider.GetPrimitiveNumber();
	UINT NumIndices = NumTriangles*3;
	embreeScene.NumIndices = NumTriangles;

	// 1, init environment 
	embreeScene.EmbreeDevice = rtcNewDevice(nullptr);
	rtcSetDeviceErrorFunction(embreeScene.EmbreeDevice, EmbreeErrorFunc, nullptr);
	RTCError ReturnErrorNewDevice = rtcGetDeviceError(embreeScene.EmbreeDevice);
	if (ReturnErrorNewDevice != RTC_ERROR_NONE)
	{
		VFX_LTRACE(ELTT_Warning, "GenerateSignedDistanceFieldVolumeData failed for %s. Embree rtcNewDevice failed. Code: %d", meshName.c_str(), (INT32)ReturnErrorNewDevice);
		return;
	}

	embreeScene.EmbreeScene = rtcNewScene(embreeScene.EmbreeDevice);
	rtcSetSceneFlags(embreeScene.EmbreeScene, RTC_SCENE_FLAG_NONE);
	RTCError ReturnErrorNewScene = rtcGetDeviceError(embreeScene.EmbreeDevice);
	if (ReturnErrorNewScene != RTC_ERROR_NONE)
	{
		VFX_LTRACE(ELTT_Warning, "GenerateSignedDistanceFieldVolumeData failed for %s. Embree rtcNewScene failed. Code: %d", meshName.c_str(), (INT32)ReturnErrorNewScene);
		rtcReleaseDevice(embreeScene.EmbreeDevice);
		return;
	}

	// 2, calculate valid triangles
	std::vector<INT32> FilteredTriangles;
	FilteredTriangles.reserve(NumTriangles);
	auto pPos = (v3dxVector3*)meshProvider.GetStream(NxRHI::VST_Position)->GetData();
	for (UINT triangleIndex = 0; triangleIndex < NumTriangles; ++triangleIndex)
	{
		UINT i0, i1, i2;
		if (meshProvider.GetTriangle(triangleIndex, &i0, &i1, &i2))
		{
			v3dxVector3 v0 = pPos[i0];
			v3dxVector3 v1 = pPos[i1];
			v3dxVector3 v2 = pPos[i2];

			v3dxVector3 triangleNormal = (v0 - v2).crossProduct(v1 - v2);
			bool bDegenerateTriangle = triangleNormal.getLengthSq() < SMALL_EPSILON;
			if (!bDegenerateTriangle)
				FilteredTriangles.push_back(triangleIndex);
		}
	}

	// 3, fill RTCGeometry(vertices,indices,triangleDesc) in embreeScene
	const INT32 NumBufferVerts = 1; // Reserve extra space at the end of the array, as embree has an internal bug where they read and discard 4 bytes off the end of the array
	embreeScene.Geometry.VertexArray.resize(NumVertices + NumBufferVerts);

	const auto NumFilteredIndices = FilteredTriangles.size() * 3;

	embreeScene.Geometry.IndexArray.resize(NumFilteredIndices);

	v3dxVector3* EmbreeVertices = embreeScene.Geometry.VertexArray.data();
	UINT* EmbreeIndices = embreeScene.Geometry.IndexArray.data();
	
	for (INT32 FilteredTriangleIndex = 0; FilteredTriangleIndex < FilteredTriangles.size(); FilteredTriangleIndex++)
	{
		UINT I0, I1, I2;
		v3dxVector3 V0, V1, V2;

		const INT32 TriangleIndex = FilteredTriangles[FilteredTriangleIndex];
		if (meshProvider.GetTriangle(TriangleIndex, &I0, &I1, &I2))
		{
			V0 = pPos[I0];
			V1 = pPos[I1];
			V2 = pPos[I2];
		}

		// TODO calculate bTriangleIsTwoSided from materials
		bool bTriangleIsTwoSided = false;

		EmbreeIndices[FilteredTriangleIndex * 3 + 0] = I0;
		EmbreeIndices[FilteredTriangleIndex * 3 + 1] = I1;
		EmbreeIndices[FilteredTriangleIndex * 3 + 2] = I2;

		EmbreeVertices[I0] = V0;
		EmbreeVertices[I1] = V1;
		EmbreeVertices[I2] = V2;
	}

	RTCGeometry Geometry = rtcNewGeometry(embreeScene.EmbreeDevice, RTC_GEOMETRY_TYPE_TRIANGLE);
	embreeScene.Geometry.InternalGeometry = Geometry;

	rtcSetSharedGeometryBuffer(Geometry, RTC_BUFFER_TYPE_VERTEX, 0, RTC_FORMAT_FLOAT3, EmbreeVertices, 0, sizeof(v3dxVector3), NumVertices);
	rtcSetSharedGeometryBuffer(Geometry, RTC_BUFFER_TYPE_INDEX, 0, RTC_FORMAT_UINT3, EmbreeIndices, 0, sizeof(UINT) * 3, FilteredTriangles.size());

	rtcSetGeometryUserData(Geometry, &embreeScene.Geometry);
	rtcSetGeometryIntersectFilterFunction(Geometry, EmbreeFilterFunc);

	rtcCommitGeometry(Geometry);
	rtcAttachGeometry(embreeScene.EmbreeScene, Geometry);
	rtcReleaseGeometry(Geometry);

	rtcCommitScene(embreeScene.EmbreeScene);

	RTCError ReturnError = rtcGetDeviceError(embreeScene.EmbreeDevice);
	if (ReturnError != RTC_ERROR_NONE)
	{
		VFX_LTRACE(ELTT_Warning, "GenerateSignedDistanceFieldVolumeData failed for %s. Embree rtcCommitScene failed. Code: %d", meshName.c_str(), (INT32)ReturnError);
		return;
	}
}

void EmbreeManager::DeleteEmbreeScene(FEmbreeScene& embreeScene)
{
	rtcReleaseScene(embreeScene.EmbreeScene);
	rtcReleaseDevice(embreeScene.EmbreeDevice);
}

bool EmbreePointQueryFunction(RTCPointQueryFunctionArguments* args)
{
	const FEmbreePointQueryContext* Context = (const FEmbreePointQueryContext*)args->context;

	assert(args->userPtr);
	float& ClosestDistanceSq = *(float*)(args->userPtr);

	const INT32 TriangleIndex = args->primID;
	assert(TriangleIndex < Context->NumTriangles);

	const v3dxVector3* VertexBuffer = (const v3dxVector3*)rtcGetGeometryBufferData(Context->MeshGeometry, RTC_BUFFER_TYPE_VERTEX, 0);
	const UINT* IndexBuffer = (const UINT*)rtcGetGeometryBufferData(Context->MeshGeometry, RTC_BUFFER_TYPE_INDEX, 0);

	const UINT I0 = IndexBuffer[TriangleIndex * 3 + 0];
	const UINT I1 = IndexBuffer[TriangleIndex * 3 + 1];
	const UINT I2 = IndexBuffer[TriangleIndex * 3 + 2];

	const v3dxVector3 V0 = VertexBuffer[I0];
	const v3dxVector3 V1 = VertexBuffer[I1];
	const v3dxVector3 V2 = VertexBuffer[I2];

	v3dxVector3 QueryPosition(args->query->x, args->query->y, args->query->z);
	v3dxVector3 ClosestPoint; 
	ClosestPointOnTriangleToPoint(&ClosestPoint, &QueryPosition, &V0, &V1, &V2);
	const float QueryDistanceSq = (ClosestPoint - QueryPosition).getLengthSq();

	if (QueryDistanceSq < ClosestDistanceSq)
	{
		ClosestDistanceSq = QueryDistanceSq;

		bool bShrinkQuery = true;

		if (bShrinkQuery)
		{
			args->query->radius = Math::Sqrt(ClosestDistanceSq);
			// Return true to indicate that the query radius has shrunk
			return true;
		}
	}

	// Return false to indicate that the query radius hasn't changed
	return false;
}
void EmbreeManager::EmbreePointQuery(FEmbreeScene& embreeScene, v3dxVector3 VoxelPosition, float LocalSpaceTraceDistance, bool& bOutNeedTracyRays, float& OutClosestDistance)
{
	RTCPointQuery PointQuery;
	PointQuery.x = VoxelPosition.X;
	PointQuery.y = VoxelPosition.Y;
	PointQuery.z = VoxelPosition.Z;
	PointQuery.time = 0;
	PointQuery.radius = LocalSpaceTraceDistance;

	FEmbreePointQueryContext QueryContext;
	rtcInitPointQueryContext(&QueryContext);
	QueryContext.MeshGeometry = embreeScene.Geometry.InternalGeometry;
	QueryContext.NumTriangles = (INT32)embreeScene.Geometry.IndexArray.size() / 3;
	float ClosestUnsignedDistanceSq = (LocalSpaceTraceDistance * 2.0f) * (LocalSpaceTraceDistance * 2.0f);
	rtcPointQuery(embreeScene.EmbreeScene, &PointQuery, &QueryContext, EmbreePointQueryFunction, &ClosestUnsignedDistanceSq);

	OutClosestDistance = Math::Sqrt(ClosestUnsignedDistanceSq);
	bOutNeedTracyRays = OutClosestDistance <= LocalSpaceTraceDistance;
}

void EmbreeManager::EmbreeRayTrace(FEmbreeScene& embreeScene, v3dxVector3 StartPosition, v3dxVector3 RayDirection, bool& bOutHit, bool& bOutHitTwoSided, v3dxVector3 &OutHitNormal, float &OutTFar)
{
	FEmbreeRay EmbreeRay;

	EmbreeRay.ray.org_x = StartPosition.X;
	EmbreeRay.ray.org_y = StartPosition.Y;
	EmbreeRay.ray.org_z = StartPosition.Z;
	EmbreeRay.ray.dir_x = RayDirection.X;
	EmbreeRay.ray.dir_y = RayDirection.Y;
	EmbreeRay.ray.dir_z = RayDirection.Z;
	EmbreeRay.ray.tnear = 0;
	EmbreeRay.ray.tfar = 1.0f;

	FEmbreeIntersectionContext EmbreeContext;
	rtcInitIntersectContext(&EmbreeContext);
	rtcIntersect1(embreeScene.EmbreeScene, &EmbreeContext, &EmbreeRay);

	bOutHit = false;
	bOutHitTwoSided = false;
	if (EmbreeRay.hit.geomID != RTC_INVALID_GEOMETRY_ID && EmbreeRay.hit.primID != RTC_INVALID_GEOMETRY_ID)
	{
		bOutHit = true;
		OutHitNormal = EmbreeRay.GetHitNormal();
		bOutHitTwoSided = EmbreeContext.IsHitTwoSided();
		OutTFar = EmbreeRay.ray.tfar;
	}
}

NS_END