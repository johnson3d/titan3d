#include "EmbreeCommon.h"
#include "perlin/perlin.h"

#if defined(PLATFORM_WIN)
	#pragma comment(lib,"embree4.lib")
	//#pragma comment(lib,"tbb.lib")
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
	TemplateScene = nullptr;
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
		//TemplateScene->AttachGeometry(this);//TemplateScene don't reference this geometry,otherwise they will reference each other
		rtcAttachGeometryByID(TemplateScene->EmbreeScene, InternalGeometry, GeomID);
		TemplateScene->CommitScene();
	}
	return TemplateScene;
}

void FEmbreeGeometry::SetGeometryTransform(const v3dxMatrix4& matrix)
{
	rtcSetGeometryTransform(InternalGeometry, 0, RTC_FORMAT_FLOAT4X4_COLUMN_MAJOR, &matrix);
	rtcCommitGeometry(InternalGeometry);
}

FEmbreeGeometryInstance::~FEmbreeGeometryInstance()
{
	if (InternalGeometry)
	{
		rtcReleaseGeometry(InternalGeometry);
		InternalGeometry = nullptr;
	}
	TemplateGeometry = nullptr;
}

FEmbreeScene::~FEmbreeScene()
{
	RemoveAllGeometries();
	RemoveAllGeometryInstances();
	if (EmbreeScene)
	{
		rtcReleaseScene(EmbreeScene);
		EmbreeScene = nullptr;
	}
}

void FEmbreeScene::AttachGeometry(FEmbreeGeometry* Geometry)
{
	rtcAttachGeometryByID(EmbreeScene, Geometry->InternalGeometry, Geometry->GeomID);
	Geometries[Geometry->GeomID] = Geometry;
}

void FEmbreeScene::DetachGeometry(unsigned int geomID)
{
	rtcDetachGeometry(EmbreeScene, geomID);
	auto iter = Geometries.find(geomID);
	if (iter != Geometries.end())
	{
		Geometries.erase(iter);
	}
	rtcCommitScene(EmbreeScene);
}

void FEmbreeScene::AttachGeometryInstance(FEmbreeGeometryInstance* Geometry)
{
	rtcAttachGeometryByID(EmbreeScene, Geometry->InternalGeometry, Geometry->GeomID);
	GeometryInstances[Geometry->GeomID] = Geometry;
	rtcCommitScene(EmbreeScene);
}

void FEmbreeScene::DetachGeometryInstance(unsigned int geomID)
{
	rtcDetachGeometry(EmbreeScene, geomID);
	auto iter = GeometryInstances.find(geomID);
	if (iter != GeometryInstances.end())
	{
		GeometryInstances.erase(iter);
	}
	rtcCommitScene(EmbreeScene);
}

void FEmbreeScene::RemoveAllGeometries()
{
	if (Geometries.size() > 0)
	{
		for (auto& i : Geometries)
		{
			rtcDetachGeometry(EmbreeScene, i.second->GeomID);
		}
		Geometries.clear();
		rtcCommitScene(EmbreeScene);
	}
}

void FEmbreeScene::RemoveAllGeometryInstances()
{
	if (GeometryInstances.size() > 0)
	{
		for (auto& i : GeometryInstances)
		{
			rtcDetachGeometry(EmbreeScene, i.second->GeomID);
		}
		GeometryInstances.clear();
		rtcCommitScene(EmbreeScene);
	}
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

bool FEmbreeScene::EmbreeRayTrace(v3dxVector3 StartPosition, v3dxVector3 RayDirection, float minDist, float maxDist, FHitResult& OutHit)
{
	FEmbreeRay EmbreeRay;

	EmbreeRay.ray.org_x = StartPosition.X;
	EmbreeRay.ray.org_y = StartPosition.Y;
	EmbreeRay.ray.org_z = StartPosition.Z;
	EmbreeRay.ray.dir_x = RayDirection.X;
	EmbreeRay.ray.dir_y = RayDirection.Y;
	EmbreeRay.ray.dir_z = RayDirection.Z;
	EmbreeRay.ray.tnear = minDist;
	EmbreeRay.ray.tfar = maxDist;

	FEmbreeIntersectionContext EmbreeContext;
	rtcInitRayQueryContext(&EmbreeContext);
	RTCIntersectArguments args;
	rtcInitIntersectArguments(&args);
	args.context = &EmbreeContext;
	rtcIntersect1(EmbreeScene, &EmbreeRay, &args);

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
	for (auto& i : meshProvider->mAtoms)
	{
		if(i[0].PrimitiveType != NxRHI::EPT_TriangleList)
		{
			VFX_LTRACE(ELTT_Warning, "EmbreeManager::CreateGeometry only support triangle mesh, mesh=%s", meshName.c_str());
			return nullptr;
		}
	}
	UINT NumVertices = meshProvider->GetVertexNumber();
	UINT NumTriangles = meshProvider->GetPrimitiveNumber();
	UINT NumIndices = NumTriangles * 3;

	//const INT32 NumBufferVerts = 1; // Reserve extra space at the end of the array, as embree has an internal bug where they read and discard 4 bytes off the end of the array
	FEmbreeGeometry* result = new FEmbreeGeometry();
	result->MeshProvider = meshProvider;

	RTCGeometry rtcGeometry = rtcNewGeometry(EmbreeDevice, RTC_GEOMETRY_TYPE_TRIANGLE);
	result->InternalGeometry = rtcGeometry;

	auto pPos = (v3dxVector3*)meshProvider->GetStream(NxRHI::VST_Position)->GetData();
	rtcSetSharedGeometryBuffer(rtcGeometry, RTC_BUFFER_TYPE_VERTEX, 0, RTC_FORMAT_FLOAT3, pPos, 0, sizeof(v3dxVector3), NumVertices);
	if (meshProvider->IsIndex32)
	{
		rtcSetSharedGeometryBuffer(rtcGeometry, RTC_BUFFER_TYPE_INDEX, 0, RTC_FORMAT_UINT3, meshProvider->IndexBuffer->GetData(), 0, sizeof(UINT) * 3, NumTriangles);
	}
	else
	{
		result->IndexBuffer32.resize(NumIndices);
		auto pIndex16 = (USHORT*)meshProvider->IndexBuffer->GetData();
		for (int i = 0; i < (int)NumIndices; i++)
		{
			result->IndexBuffer32[i] = pIndex16[i];
		}
		rtcSetSharedGeometryBuffer(rtcGeometry, RTC_BUFFER_TYPE_INDEX, 0, RTC_FORMAT_UINT3, result->IndexBuffer32.data(), 0, sizeof(UINT) * 3, NumTriangles);
	}
	
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
	rtcCommitGeometry(result->InternalGeometry);

	result->GeomID = GeomIDAllocator++;
	return result;
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

NS_END
