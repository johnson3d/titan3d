#include "PhyContext.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyMaterial.h"
#include "PhyShape.h"
#include "PhyMesh.h"
#include "PhyHeightfield.h"

#ifdef PLATFORM_IOS
	#include <malloc/malloc.h>
#else
	#include <stdlib.h>
	#include <malloc.h>
#endif

#define new VNEW

using namespace physx;

NS_BEGIN

using namespace NxRHI;

ENGINE_RTTI_IMPL(EngineNS::PhyContext);

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
	mFoundation = PxCreateFoundation(PX_PHYSICS_VERSION, gDefaultAllocatorCallback, gDefaultErrorCallback);
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

	if (featureFlags & PhyFeatureFlag::Articulations)
		PxRegisterArticulations(*mContext);
	if (featureFlags & PhyFeatureFlag::HeightFields)
		PxRegisterHeightFields(*mContext);
	/*if (featureFlags & PhyFeatureFlag::Cloth)
		PxRegisterCloth(*mContext);
	if (featureFlags & PhyFeatureFlag::Particles)
		PxRegisterParticles(*mContext);*/

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

PhyActor* PhyContext::CreateActor(EPhyActorType type, const v3dxVector3* p, const v3dxQuaternion* q)
{
	physx::PxTransform trf;
	trf.p.x = p->x;
	trf.p.y = p->y;
	trf.p.z = p->z;

	trf.q.x = q->x;
	trf.q.y = q->y;
	trf.q.z = q->z;
	trf.q.w = q->w;

	return CreateActor(type, &trf);
}

PhyActor* PhyContext::CreateActor(EPhyActorType type, const physx::PxTransform* pose)
{
	auto ret = new PhyActor();

	ret->mActorType = type;
	switch (type)
	{
	case PAT_Dynamic:
		{
			auto rigidDyn = mContext->createRigidDynamic(*pose);
			ret->mActor = rigidDyn;
			auto cfc = rigidDyn->getMinCCDAdvanceCoefficient();
			rigidDyn->setMinCCDAdvanceCoefficient(cfc);
		}
		break;
	case PAT_Static:
		{
			auto rigidStatic = mContext->createRigidStatic(*pose);
			ret->mActor = rigidStatic;
		}
		break;
	default:
		break;
	}

	if (ret->mActor == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	//ret->mActor->setActorFlag(PxActorFlag::eVISUALIZATION, true);
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
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeBox(PhyMaterial* material, const v3dxVector3* halfExtent)
{
	auto ret = new PhyShape();
	ret->mType = PST_Box;	
	ret->mShape = mContext->createShape(physx::PxBoxGeometry(halfExtent->x, halfExtent->y, halfExtent->z), *material->mMaterial, true,
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

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
	ret->mShape = mContext->createShape(physx::PxSphereGeometry(radius), *material->mMaterial, true,
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE | physx::PxShapeFlag::eVISUALIZATION);

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
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}
	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeConvex(PhyMaterial* material, PhyConvexMesh* mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot)
{
	auto convexMesh = mesh->mMesh;

	physx::PxMeshScale scaling(*(const physx::PxVec3*)scale, *(const physx::PxQuat*)scaleRot);
	auto ret = new PhyShape();
	ret->mType = PST_Convex;
	ret->mShape = mContext->createShape(physx::PxConvexMeshGeometry(convexMesh, scaling), *material->mMaterial, true,
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeTriMesh(PhyMaterial** material, int NumOfMtl, PhyTriMesh* mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot)
{
	physx::PxMeshScale scaling(*(const physx::PxVec3*)scale, *(const physx::PxQuat*)scaleRot);
	
	auto ret = new PhyShape();
	ret->mTrianglesRemapNumber = mesh->mMesh->getNbTriangles();
	ret->mTrianglesRemap = new uint32_t[ret->mTrianglesRemapNumber];
	memcpy(ret->mTrianglesRemap , mesh->mMesh->getTrianglesRemap(), sizeof(uint32_t) * ret->mTrianglesRemapNumber);
	//ret->mTriangleMesh = triangleMesh;

	ret->mType = PST_TriangleMesh;
	std::vector<PxMaterial*> pxMtls;
	for (int i = 0; i < NumOfMtl; i++)
	{
		pxMtls.push_back(material[i]->mMaterial);
	}
	ret->mShape = mContext->createShape(physx::PxTriangleMeshGeometry(mesh->mMesh, scaling), &pxMtls[0], NumOfMtl, true,
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);

	//physx::PxTriangleMeshGeometry geom;
	//ret->mShape->getTriangleMeshGeometry(geom);

	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	ret->BindPhysX();
	return ret;
}

PhyShape* PhyContext::CreateShapeHeightfield(PhyMaterial** material, int NumOfMtl, PhyHeightfield* heightfield, float heightScale, const v3dxVector3* scale)
{
	auto ret = new PhyShape();
	PxHeightFieldGeometry hfGeom;//(heightfield->mHeightField, PxMeshGeometryFlags(), scale->x, scale->y, scale->z);
	hfGeom.heightFieldFlags = PxMeshGeometryFlag::eDOUBLE_SIDED;
	hfGeom.heightField = heightfield->mHeightField;
	hfGeom.heightScale = heightScale;
	hfGeom.rowScale = 1.0f;
	hfGeom.columnScale = 1.0f;
	std::vector<PxMaterial*> pxMtls;
	for (int i = 0; i < NumOfMtl; i++)
	{
		pxMtls.push_back(material[i]->mMaterial);
	}
	ret->mShape = mContext->createShape(hfGeom, &pxMtls[0], NumOfMtl, true, 
		physx::PxShapeFlag::eVISUALIZATION | physx::PxShapeFlag::eSIMULATION_SHAPE | physx::PxShapeFlag::eSCENE_QUERY_SHAPE);
	ret->mType = PST_HeightField;
	if (ret->mShape == nullptr)
	{
		ret->Release();
		return nullptr;
	}

	ret->BindPhysX();
	return ret;
}

PhyConvexMesh* PhyContext::CookConvexMesh(FMeshDataProvider* mesh)
{
	auto blobVB = mesh->GetStream(VST_Position);
	
	physx::PxConvexMeshDesc convexDesc;
	convexDesc.points.count = blobVB->GetSize() / sizeof(v3dxVector3);
	convexDesc.points.stride = sizeof(v3dxVector3);
	convexDesc.points.data = blobVB->GetData();
	convexDesc.flags |= physx::PxConvexFlag::eCOMPUTE_CONVEX;
	//convexDesc.flags |= physx::PxConvexFlag::eINFLATE_CONVEX; //physx 3.4 flags
	physx::PxDefaultMemoryOutputStream m_memoryStream;
	physx::PxConvexMeshCookingResult::Enum cookOk;
	if (mCooking->cookConvexMesh(convexDesc, m_memoryStream, &cookOk) == 0)
		return nullptr;

	physx::PxDefaultMemoryInputData readBuffer(m_memoryStream.getData(), m_memoryStream.getSize());
	auto convexMesh = mContext->createConvexMesh(readBuffer);
	if (convexMesh == nullptr)
		return nullptr;
	PhyConvexMesh* result = new PhyConvexMesh();
	result->mMesh = convexMesh;

	return result;
}

PhyTriMesh* PhyContext::CookTriMesh(FMeshDataProvider* mesh, IBlobObject* uvblob, IBlobObject* faceblob, IBlobObject* posblob)
{
	auto blobVB = mesh->GetStream(VST_Position);

	auto blobIB = mesh->GetIndices();

	physx::PxTriangleMeshDesc  meshDesc;

	std::vector<USHORT> mtlTabs;
	if(mesh->GetAtomNumber()>1)
	{
		for (UINT i = 0; i < mesh->GetAtomNumber(); i++)
		{
			const FMeshAtomDesc* dpDesc = mesh->GetAtom(i, 0);
			for (UINT j = 0; j < dpDesc->NumPrimitives; j++)
			{
				mtlTabs.push_back((USHORT)i);
			}
		}
		meshDesc.materialIndices.stride = sizeof(USHORT);
		meshDesc.materialIndices.data = &mtlTabs[0];
	}
	meshDesc.points.data = blobVB->GetData();
	meshDesc.points.count = blobVB->GetSize() / sizeof(v3dxVector3);
	meshDesc.points.stride = sizeof(v3dxVector3);

	//meshDesc.flags |= physx::PxMeshFlag::eFLIPNORMALS;
	if (mesh->IsIndex32)
	{
		meshDesc.triangles.stride = sizeof(physx::PxU32) * 3;
		meshDesc.triangles.count = blobIB->GetSize() / meshDesc.triangles.stride;
	}
	else
	{
		meshDesc.triangles.stride = sizeof(physx::PxU16) * 3;
		meshDesc.flags |= physx::PxMeshFlag::e16_BIT_INDICES;
		meshDesc.triangles.count = blobIB->GetSize() / meshDesc.triangles.stride;
	}
	meshDesc.triangles.data = blobIB->GetData();

	physx::PxDefaultMemoryOutputStream m_memoryStream;
	bool status = mCooking->cookTriangleMesh(meshDesc, m_memoryStream);
	if (!status)
		return nullptr;

	physx::PxDefaultMemoryInputData readBuffer(m_memoryStream.getData(), m_memoryStream.getSize());
	auto triMesh = mContext->createTriangleMesh(readBuffer);
	if (triMesh == nullptr)
		return nullptr;

	PhyTriMesh* result = new PhyTriMesh();
	result->mMesh = triMesh;
	result->mCookedData.ReSize(0);
	result->mCookedData.PushData(m_memoryStream.getData(), m_memoryStream.getSize());

	if (posblob != nullptr)
	{
		posblob->PushData(blobVB->GetData(), blobVB->GetSize());
	}

	//UV
	if (uvblob != nullptr)
	{
		auto blobVB = mesh->GetStream(VST_UV);
		uvblob->PushData(blobVB->GetData(), blobVB->GetSize());
	}

	//Face
	if (faceblob != nullptr)
	{
		auto blobVB = mesh->GetIndices();
		faceblob->PushData(blobVB->GetData(), blobVB->GetSize());
	}

	return result;
}

PhyHeightfield* PhyContext::CookHeightfield(int nbColumns, int nbRows, PhyHeightFieldSample* pData, float convexEdgeThreshold, bool bNoBoundaryEdge)
{
	physx::PxHeightFieldDesc desc;
	desc.format = PxHeightFieldFormat::eS16_TM;
	desc.nbColumns = nbColumns;
	desc.nbRows = nbRows;
	desc.convexEdgeThreshold = convexEdgeThreshold;
	desc.samples.data = pData;
	desc.samples.stride = sizeof(PhyHeightFieldSample);// dataStride;
	if (bNoBoundaryEdge)
	{
		desc.flags.set(physx::PxHeightFieldFlag::Enum::eNO_BOUNDARY_EDGES);
	}
	physx::PxDefaultMemoryOutputStream m_memoryStream;
	mCooking->cookHeightField(desc, m_memoryStream);
	physx::PxDefaultMemoryInputData readBuffer(m_memoryStream.getData(), m_memoryStream.getSize());
	auto hflds = mContext->createHeightField(readBuffer);
	if (hflds == nullptr)
		return nullptr;

	PhyHeightfield* result = new PhyHeightfield();
	result->mHeightField = hflds;
	return result;
}

NS_END
