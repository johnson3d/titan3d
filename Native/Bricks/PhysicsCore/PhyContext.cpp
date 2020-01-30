#include "PhyContext.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyMaterial.h"
#include "PhyShape.h"

#ifdef PLATFORM_IOS
	#include <malloc/malloc.h>
#else
	#include <stdlib.h>
	#include <malloc.h>
#endif

#define new VNEW

using namespace physx;

NS_BEGIN

RTTI_IMPL(EngineNS::PhyContext, EngineNS::PhyEntity);

class PxVictoryAllocator : public physx::PxAllocatorCallback
{
	void* allocate(size_t size, const char*, const char*, int)
	{
#if defined PLATFORM_DROID
		//return _mm_malloc(size, 16);
		return memalign(16, size);
		//return memalign(16, size);
#elif defined PLATFORM_IOS
		return malloc_zone_memalign(malloc_default_zone(), 16, size);
#else // PLATFORM_WINDOW
		return _aligned_malloc(size, 16);
#endif
	}

	void deallocate(void* ptr)
	{
#if defined PLATFORM_DROID || defined PLATFORM_IOS
		//return _mm_free(ptr);
		return free(ptr);
#else // PLATFORM_WINDOW
		return _aligned_free(ptr);
#endif

	}
};

class PxVictoryErrorCallback : public physx::PxErrorCallback
{
public:
	PxVictoryErrorCallback()
	{

	}
	~PxVictoryErrorCallback()
	{

	}

	virtual void reportError(physx::PxErrorCode::Enum code, const char* message, const char* file, int line)
	{
		const char* errorCode = NULL;

		switch (code)
		{
		case physx::PxErrorCode::eINVALID_PARAMETER:
			errorCode = "invalid parameter";
			break;
		case physx::PxErrorCode::eINVALID_OPERATION:
			errorCode = "invalid operation";
			break;
		case physx::PxErrorCode::eOUT_OF_MEMORY:
			errorCode = "out of memory";
			break;
		case physx::PxErrorCode::eDEBUG_INFO:
			errorCode = "info";
			break;
		case physx::PxErrorCode::eDEBUG_WARNING:
			errorCode = "warning";
			break;
		default:
			errorCode = "unknown error";
			break;
		}

		printf("%s (%d) :", file, line);
		printf("%s", errorCode);
		printf(" : %s\n", message);
	}
};

PxVictoryAllocator gDefaultAllocatorCallback;
PxVictoryErrorCallback gDefaultErrorCallback;

PhyContext::PhyContext()
{
	mFoundation = nullptr;
	mPvd = nullptr;
	mContext = nullptr;
	mCooking = nullptr;
	EntityType = Phy_Context;
}

vBOOL PhyContext::Init(UINT32 featureFlags)
{
	mFoundation = PxCreateFoundation(PX_FOUNDATION_VERSION, gDefaultAllocatorCallback, gDefaultErrorCallback);
	if (mFoundation == NULL)
	{
		VFX_LTRACE(ELTT_Physics, "PxCreateFoundation(Ver = %d) failed!\r\n", PX_PHYSICS_VERSION);
		return FALSE;
	}

#if defined PLATFORM_WIN
	physx::PxCudaContextManagerDesc cudaContextManagerDesc;
	auto m_cudaContextManager = PxCreateCudaContextManager(*mFoundation, cudaContextManagerDesc);
	if (m_cudaContextManager)
	{
		if (!m_cudaContextManager->contextIsValid())
		{
			m_cudaContextManager->release();
			m_cudaContextManager = NULL;
		}
	}
	if (m_cudaContextManager) {
		VFX_LTRACE(ELTT_Physics, "Device Name: %s\n", m_cudaContextManager->getDeviceName());
		VFX_LTRACE(ELTT_Physics, "Driver Version: %d\n", m_cudaContextManager->getDriverVersion());
		VFX_LTRACE(ELTT_Physics, "Total Bytes: %d\n", (int)m_cudaContextManager->getDeviceTotalMemBytes());
		VFX_LTRACE(ELTT_Physics, "core count: %d\n", m_cudaContextManager->getMultiprocessorCount());
	}
	//if (NULL == m_physics->getPvdConnectionManager())
	//	return false;
	// setup connection parameters
	const char*     pvd_host_ip = "127.0.0.1";  // IP of the PC which is running PVD
	int             port = 5425;         // TCP port to connect to, where PVD is listening
	unsigned int    timeout = 100;          // timeout in milliseconds to wait for PVD to respond,
											// consoles and remote PCs need a higher timeout.
											///*PxVisualDebuggerConnectionFlags connectionFlags = */PxVisualDebuggerExt::getAllConnectionFlags();
											//PxVisualDebuggerConnectionFlags theConnectionFlags(PxVisualDebuggerConnectionFlag::eDEBUG | PxVisualDebuggerConnectionFlag::ePROFILE | PxVisualDebuggerConnectionFlag::eMEMORY);
											//// and now try to connect
											//m_physics->getVisualDebugger()->setVisualDebuggerFlag(PxVisualDebuggerFlag::eTRANSMIT_CONSTRAINTS, true);
											//m_physics->getVisualDebugger()->setVisualDebuggerFlag(PxVisualDebuggerFlag::eTRANSMIT_CONTACTS, true);
											//m_physics->getVisualDebugger()->setVisualDebuggerFlag(PxVisualDebuggerFlag::eTRANSMIT_SCENEQUERIES, true);
											//theConnection = PxVisualDebuggerExt::createConnection(m_physics->getPvdConnectionManager(),
											//	pvd_host_ip, port, timeout, theConnectionFlags);

	mPvd = PxCreatePvd(*mFoundation);
	PxPvdTransport* transport = PxDefaultPvdSocketTransportCreate(pvd_host_ip, port, timeout);
	mPvd->connect(*transport, PxPvdInstrumentationFlag::eDEBUG);
#endif

	physx::PxTolerancesScale scale;
	//创建基本包含基本物理特性的物理组件
	mContext = PxCreateBasePhysics(PX_PHYSICS_VERSION, *mFoundation, scale, false, mPvd);
	if (!mContext)
	{
		VFX_LTRACE(ELTT_Physics, "PxCreatePhysics(Ver = %d) failed!\r\n", PX_PHYSICS_VERSION);
		return FALSE;
	}
	if (!PxInitExtensions(*mContext, mPvd))
	{
		VFX_LTRACE(ELTT_Physics, "PxInitExtensions(Ver = %d) failed!\r\n", PX_PHYSICS_VERSION);
		return FALSE;
	}
	PxCookingParams params(scale);
	params.meshWeldTolerance = 0.001f;
	// version 3.3 params.meshPreprocessParams = PxMeshPreprocessingFlags(PxMeshPreprocessingFlag::eWELD_VERTICES | PxMeshPreprocessingFlag:: PxMeshPreprocessingFlag::eREMOVE_UNREFERENCED_VERTICES | PxMeshPreprocessingFlag::eREMOVE_DUPLICATED_TRIANGLES);
	params.meshPreprocessParams = PxMeshPreprocessingFlags(PxMeshPreprocessingFlag::eWELD_VERTICES);
	mCooking = PxCreateCooking(PX_PHYSICS_VERSION, *mFoundation, params);
	if (!mCooking)
	{
		VFX_LTRACE(ELTT_Physics, "PxCreateCooking(Ver = %d) failed!\r\n", PX_PHYSICS_VERSION);
		return FALSE;
	}

	//注册需要的物理特性
	if (featureFlags & PhyFeatureFlag::Articulations)
		PxRegisterArticulations(*mContext);
	if (featureFlags & PhyFeatureFlag::HeightFields)
		PxRegisterHeightFields(*mContext);
	if (featureFlags & PhyFeatureFlag::Cloth)
		PxRegisterCloth(*mContext);
	if (featureFlags & PhyFeatureFlag::Particles)
		PxRegisterParticles(*mContext);

	return TRUE;
}

PhySceneDesc* PhyContext::CreateSceneDesc()
{
	auto ret = new PhySceneDesc();
	ret->mDesc = new physx::PxSceneDesc(mContext->getTolerancesScale());
	ret->Init();
	return ret;
}

PhyScene* PhyContext::CreateScene(const PhySceneDesc* desc)
{
	auto ret = new PhyScene();
	ret->mScene = mContext->createScene(*desc->mDesc);
#if defined(_DEBUG) && defined(PLATFORM_WIN)
	ret->mScene->getScenePvdClient()->setScenePvdFlags(PxPvdSceneFlag::eTRANSMIT_CONSTRAINTS | PxPvdSceneFlag::eTRANSMIT_SCENEQUERIES | PxPvdSceneFlag::eTRANSMIT_CONTACTS);
#endif
	if (ret->mScene == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyActor* PhyContext::CreateActor(EPhyActorType type, const physx::PxTransform* pose)
{
	auto ret = new PhyActor();

	ret->mActorType = type;
	switch (type)
	{
	case PAT_Dynamic:
		ret->mActor = mContext->createRigidDynamic(*pose);
		break;
	case PAT_Static:
		ret->mActor = mContext->createRigidStatic(*pose);
		break;
	default:
		break;
	}

	if (ret->mActor == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	ret->mActor->setActorFlag(PxActorFlag::eVISUALIZATION, true);
	ret->BindPhysX();
	ret->mPosition = *((v3dxVector3*)&pose->p);
	ret->mRotation = *((v3dxQuaternion*)&pose->q);
	return ret;
}

PhyMaterial* PhyContext::CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
{
	auto ret = new PhyMaterial();

	ret->mMaterial = mContext->createMaterial(staticFriction, dynamicFriction, restitution);

	if (ret->mMaterial == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapePlane(PhyMaterial* material)
{
	auto ret = new PhyShape();
	ret->mType = PST_Plane;
	ret->mShape = mContext->createShape(physx::PxPlaneGeometry(), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeBox(PhyMaterial* material, float width, float height, float length)
{
	auto ret = new PhyShape();
	ret->mType = PST_Box;
	ret->mShape = mContext->createShape(physx::PxBoxGeometry(width / 2.0f, height / 2.0f, length / 2.0f), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeSphere(PhyMaterial* material, float radius)
{
	auto ret = new PhyShape();
	ret->mType = PST_Sphere;
#if defined(_DEBUG) && defined(PLATFORM_WIN)
	ret->mShape = mContext->createShape(physx::PxSphereGeometry(radius), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE | physx::PxShapeFlag::eVISUALIZATION);
#else
	ret->mShape = mContext->createShape(physx::PxSphereGeometry(radius), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);
#endif

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeCapsule(PhyMaterial* material, float radius, float halfHeight)
{
	auto ret = new PhyShape();
	ret->mType = PST_Capsule;
	ret->mShape = mContext->createShape(physx::PxCapsuleGeometry(radius, halfHeight), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeConvex(PhyMaterial* material, IBlobObject* blob, const v3dxVector3* scale, const v3dxQuaternion* scaleRot)
{
	physx::PxDefaultMemoryInputData stream((physx::PxU8*)blob->GetData(), blob->GetSize());
	auto convexMesh = mContext->createConvexMesh(stream);
	if (convexMesh == nullptr)
		return NULL;

	physx::PxMeshScale scaling(*(const physx::PxVec3*)scale, *(const physx::PxQuat*)scaleRot);
	auto ret = new PhyShape();
	ret->mType = PST_Convex;
	ret->mShape = mContext->createShape(physx::PxConvexMeshGeometry(convexMesh, scaling), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	convexMesh->release();

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeTriMesh(PhyMaterial* material, IBlobObject* blob, const v3dxVector3* scale, const v3dxQuaternion* scaleRot)
{
	physx::PxMeshScale scaling(*(const physx::PxVec3*)scale, *(const physx::PxQuat*)scaleRot);
	physx::PxDefaultMemoryInputData readBuffer((physx::PxU8*)blob->GetData(), blob->GetSize());
	physx::PxTriangleMesh* triangleMesh = mContext->createTriangleMesh(readBuffer);

	auto ret = new PhyShape();
	ret->mTrianglesRemapNumber = triangleMesh->getNbTriangles();
	ret->mTrianglesRemap = new uint32_t[ret->mTrianglesRemapNumber];
	memcpy(ret->mTrianglesRemap , triangleMesh->getTrianglesRemap(), sizeof(uint32_t) * ret->mTrianglesRemapNumber);
	//ret->mTriangleMesh = triangleMesh;

	ret->mType = PST_TriangleMesh;
	ret->mShape = mContext->createShape(physx::PxTriangleMeshGeometry(triangleMesh, scaling), *material->mMaterial, true,
		physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	triangleMesh->release();

	physx::PxTriangleMeshGeometry geom;
	ret->mShape->getTriangleMeshGeometry(geom);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	ret->BindPhysX();
	return ret;
}

vBOOL PhyContext::CookConvexMesh(IRenderContext* rc, IGeometryMesh* mesh, IBlobObject* blob)
{
	auto posVB = mesh->GetVertexBuffer(VST_Position);
	IBlobObject blobVB;
	posVB->GetBufferData(rc, &blobVB);

	physx::PxConvexMeshDesc convexDesc;
	convexDesc.points.count = blobVB.GetSize() / sizeof(v3dxVector3);
	convexDesc.points.stride = sizeof(v3dxVector3);
	convexDesc.points.data = blobVB.GetData();
	convexDesc.flags |= physx::PxConvexFlag::eCOMPUTE_CONVEX;
	convexDesc.flags |= physx::PxConvexFlag::eINFLATE_CONVEX;
	physx::PxConvexMeshCookingResult::Enum result;
	physx::PxDefaultMemoryOutputStream m_memoryStream;
	if (mCooking->cookConvexMesh(convexDesc, m_memoryStream, &result) == 0)
		return FALSE;

	blob->ReSize(m_memoryStream.getSize());
	memcpy(&blob->mDatas[0], m_memoryStream.getData(), m_memoryStream.getSize());

	return TRUE;
}

vBOOL PhyContext::CookTriMesh(IRenderContext* rc, IGeometryMesh* mesh, IBlobObject* blob, IBlobObject* uvblob, IBlobObject* faceblob, IBlobObject* posblob)
{
	//pos && index
	{
		auto posVB = mesh->GetVertexBuffer(VST_Position);
		IBlobObject blobVB;
		posVB->GetBufferData(rc, &blobVB);

		IBlobObject blobIB;
		auto ib = mesh->GetIndexBuffer();
		ib->GetBufferData(rc, &blobIB);

		posblob->PushData(blobVB.GetData(), blobVB.GetSize());

		physx::PxTriangleMeshDesc  meshDesc;
		meshDesc.points.data = blobVB.GetData();
		meshDesc.points.count = blobVB.GetSize() / sizeof(v3dxVector3);
		meshDesc.points.stride = sizeof(v3dxVector3);

		//meshDesc.flags |= physx::PxMeshFlag::eFLIPNORMALS;
		if (mesh->GetIndexBuffer()->mDesc.Type == IBT_Int32)
		{
			meshDesc.triangles.stride = sizeof(physx::PxU32) * 3;
			meshDesc.triangles.count = blobIB.GetSize() / meshDesc.triangles.stride;
		}
		else {
			meshDesc.triangles.stride = sizeof(physx::PxU16) * 3;
			meshDesc.flags |= physx::PxMeshFlag::e16_BIT_INDICES;
			meshDesc.triangles.count = blobIB.GetSize() / meshDesc.triangles.stride;
		}
		meshDesc.triangles.data = blobIB.GetData();

		physx::PxDefaultMemoryOutputStream m_memoryStream;
		bool status = mCooking->cookTriangleMesh(meshDesc, m_memoryStream);
		if (!status)
			return FALSE;

		blob->ReSize(m_memoryStream.getSize());
		memcpy(&blob->mDatas[0], m_memoryStream.getData(), m_memoryStream.getSize());
	}
	

	//UV
	{
		auto uvVB = mesh->GetVertexBuffer(VST_UV);
		IBlobObject blobVB;
		uvVB->GetBufferData(rc, &blobVB);
		uvblob->PushData(blobVB.GetData(), blobVB.GetSize());
	}

	//Face
	{
		auto IB = mesh->GetIndexBuffer();
		IBlobObject blobVB;
		IB->GetBufferData(rc, &blobVB);
		faceblob->PushData(blobVB.GetData(), blobVB.GetSize());
	}

	return TRUE;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI1(vBOOL, EngineNS, PhyContext, Init, UINT32);
	CSharpReturnAPI0(PhySceneDesc*, EngineNS, PhyContext, CreateSceneDesc);
	CSharpReturnAPI2(PhyActor*, EngineNS, PhyContext, CreateActor, EPhyActorType, const physx::PxTransform*);
	CSharpReturnAPI1(PhyScene*, EngineNS, PhyContext, CreateScene, const PhySceneDesc*);
	CSharpReturnAPI3(PhyMaterial*, EngineNS, PhyContext, CreateMaterial, float, float, float);

	CSharpReturnAPI1(PhyShape*, EngineNS, PhyContext, CreateShapePlane, PhyMaterial*);
	CSharpReturnAPI4(PhyShape*, EngineNS, PhyContext, CreateShapeBox, PhyMaterial*, float, float, float);
	CSharpReturnAPI2(PhyShape*, EngineNS, PhyContext, CreateShapeSphere, PhyMaterial*, float);
	CSharpReturnAPI3(PhyShape*, EngineNS, PhyContext, CreateShapeCapsule, PhyMaterial*, float, float);
	CSharpReturnAPI4(PhyShape*, EngineNS, PhyContext, CreateShapeConvex, PhyMaterial*, IBlobObject*, const v3dxVector3*, const v3dxQuaternion*);
	CSharpReturnAPI4(PhyShape*, EngineNS, PhyContext, CreateShapeTriMesh, PhyMaterial*, IBlobObject*, const v3dxVector3*, const v3dxQuaternion*);

	CSharpReturnAPI3(vBOOL, EngineNS, PhyContext, CookConvexMesh, IRenderContext*, IGeometryMesh*, IBlobObject*);
	CSharpReturnAPI6(vBOOL, EngineNS, PhyContext, CookTriMesh, IRenderContext*, IGeometryMesh*, IBlobObject*, IBlobObject*, IBlobObject*, IBlobObject*);
}