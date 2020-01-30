#include "PhyShape.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma GCC diagnostic ignored "-Wunused-variable"
#endif

NS_BEGIN

RTTI_IMPL(EngineNS::PhyShape, EngineNS::PhyEntity);

PhyShape::PhyShape()
{
	mShape = nullptr;
	EntityType = Phy_Shape;
	mTrianglesRemap = nullptr;
	mTrianglesRemapNumber = 0;
}

PhyShape::~PhyShape()
{
	Cleanup();
}

void PhyShape::Cleanup()
{
	mTrianglesRemapNumber = 0;
	Safe_DeleteArray(mTrianglesRemap);
	if (mShape != nullptr)
	{
		//unbind this from pxActor's user data
		mShape->userData = nullptr;
		physx::PxScene* scene = nullptr;
		auto actor = mActor.GetPtr();
		if (actor != nullptr)
		{
			auto ps = actor->mScene.GetPtr();
			if (ps != nullptr)
				scene = ps->mScene;
		}
		if (scene != nullptr)
		{
			physx::PxSceneWriteLock writeLock(*scene);
			mShape->release();
		}
		else
		{
			mShape->release();
		}
		mShape = nullptr;
	}
}

void PhyShape::BindPhysX()
{
	ASSERT(mShape);
	mShape->userData = this;
}

vBOOL PhyShape::AddToActor(PhyActor* actor, const physx::PxTransform* relativePose)
{
	auto prev = mActor.GetPtr();
	if (prev == actor)
		return TRUE;
	if (prev != nullptr)
	{
		prev->DetachShape(this, true);
	}

	mActor.FromObject(actor);
	if (actor == nullptr)
	{
		return FALSE;
	}

	return actor->AttachShape(this, relativePose);
}

void PhyShape::RemoveFromActor()
{
	auto prev = mActor.GetPtr();
	if (prev != nullptr)
	{
		prev->DetachShape(this, true);
	}

	mActor.FromObject(nullptr);
}

void PhyShape::SetLocalPose(const physx::PxTransform* relativePose)
{
	if (mActor.IsValid() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		mShape->setLocalPose(*relativePose);
	}
	else
	{
		mShape->setLocalPose(*relativePose);
	}
}

void PhyShape::GetLocalPose(physx::PxTransform* relativePose)
{
	if (mActor.IsValid() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneReadLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		*relativePose = mShape->getLocalPose();
	}
	else
	{
		*relativePose = mShape->getLocalPose();
	}
}

void PhyShape::SetQueryFilterData(physx::PxFilterData* filterData)
{
	physx::PxFilterData data;
	data = *filterData;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		mShape->setQueryFilterData(data);
	}
	else
	{
		mShape->setQueryFilterData(data);
	}
}

void PhyShape::SetFlag(EPhysShapeFlag flag, vBOOL value)
{
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		mShape->setFlag((physx::PxShapeFlag::Enum)flag, value);
	}
	else
	{
		mShape->setFlag((physx::PxShapeFlag::Enum)flag, value);
	}
}

vBOOL PhyShape::HaveFlag(EPhysShapeFlag flag)
{
	physx::PxShapeFlags flags = mShape->getFlags();
	if (flags & ((physx::PxShapeFlag::Enum)flag))
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}
}

vBOOL PhyShape::IfGetBox(float* w, float* h, float* l)
{
	if (mType != PST_Box)
		return FALSE;

	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxBoxGeometry geom;
		if (false == mShape->getBoxGeometry(geom))
			return FALSE;
		*w = geom.halfExtents.x * 2.0f;
		*h = geom.halfExtents.y * 2.0f;
		*l = geom.halfExtents.z * 2.0f;
	}
	else
	{
		physx::PxBoxGeometry geom;
		if (false == mShape->getBoxGeometry(geom))
			return FALSE;
		*w = geom.halfExtents.x * 2.0f;
		*h = geom.halfExtents.y * 2.0f;
		*l = geom.halfExtents.z * 2.0f;
	}
	return TRUE;
}

vBOOL PhyShape::IfGetSphere(float* radius)
{
	if (mType != PST_Sphere)
		return FALSE;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxSphereGeometry geom;
		if (false == mShape->getSphereGeometry(geom))
			return FALSE;
		*radius = geom.radius;
	}
	else
	{
		physx::PxSphereGeometry geom;
		if (false == mShape->getSphereGeometry(geom))
			return FALSE;
		*radius = geom.radius;
	}
	return TRUE;
}

vBOOL PhyShape::IfGetCapsule(float* radius, float* halfHeight)
{
	if (mType != PST_Capsule)
		return FALSE;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxCapsuleGeometry geom;
		if (false == mShape->getCapsuleGeometry(geom))
			return FALSE;
		*radius = geom.radius;
		*halfHeight = geom.halfHeight;
	}
	else
	{
		physx::PxCapsuleGeometry geom;
		if (false == mShape->getCapsuleGeometry(geom))
			return FALSE;
		*radius = geom.radius;
		*halfHeight = geom.halfHeight;
	}
	return TRUE;
}

vBOOL PhyShape::IfGetScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot)
{
	if (mType != PST_TriangleMesh)
		return FALSE;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return FALSE;
		*((physx::PxVec3*)scale) = geom.scale.scale;
		*((physx::PxQuat*)scaleRot) = geom.scale.rotation;
	}
	else
	{
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return FALSE;
		*((physx::PxVec3*)scale) = geom.scale.scale;
		*((physx::PxQuat*)scaleRot) = geom.scale.rotation;
	}
	return TRUE;
}

GfxMeshPrimitives* PhyShape::IfGetTriMesh(IRenderContext* rc)
{
	if (mType != PST_TriangleMesh)
		return nullptr;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return nullptr;

		auto result = new GfxMeshPrimitives();
		result->Init(rc, "", 1);
		UINT nVert = geom.triangleMesh->getNbVertices();
		UINT nTri = geom.triangleMesh->getNbTriangles();
		auto pVert = geom.triangleMesh->getVertices();

		IVertexBufferDesc vbDesc;
		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)pVert;
		AutoRef<IVertexBuffer> vbPos = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Position, vbPos);

		std::vector<v3dxVector3> normals;
		auto ibflags = geom.triangleMesh->getTriangleMeshFlags();
		if (ibflags == physx::PxTriangleMeshFlag::e16_BIT_INDICES)
		{
			IIndexBufferDesc ibDesc;
			ibDesc.ByteWidth = sizeof(USHORT) * nTri * 3;
			ibDesc.Type = IBT_Int16;
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			GfxMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);
		}
		else
		{
			IIndexBufferDesc ibDesc;
			ibDesc.ByteWidth = sizeof(UINT) * nTri * 3;
			ibDesc.Type = IBT_Int32;
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			GfxMeshPrimitives::CalcNormals32(normals, (const v3dxVector3*)pVert, nVert, (UINT*)ibDesc.InitData, nTri);
		}

		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)&normals[0];
		AutoRef<IVertexBuffer> vbNor = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Normal, vbNor);

		DrawPrimitiveDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtomLOD(0, &desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		result->GetResourceState()->SetKeepValid(TRUE);

		result->GetGeomtryMesh()->SetIsDirty(TRUE);

		return result;
	}
	else
	{
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return nullptr;

		auto result = new GfxMeshPrimitives();
		result->Init(rc, "", 1);
		UINT nVert = geom.triangleMesh->getNbVertices();
		UINT nTri = geom.triangleMesh->getNbTriangles();
		auto pVert = geom.triangleMesh->getVertices();

		IVertexBufferDesc vbDesc;
		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)pVert;
		AutoRef<IVertexBuffer> vbPos = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Position, vbPos);

		std::vector<v3dxVector3> normals;
		auto ibflags = geom.triangleMesh->getTriangleMeshFlags();
		if (ibflags == physx::PxTriangleMeshFlag::e16_BIT_INDICES)
		{
			IIndexBufferDesc ibDesc;
			ibDesc.ByteWidth = sizeof(USHORT) * nTri * 3;
			ibDesc.Type = IBT_Int16;
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			GfxMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);
		}
		else
		{
			IIndexBufferDesc ibDesc;
			ibDesc.ByteWidth = sizeof(UINT) * nTri * 3;
			ibDesc.Type = IBT_Int32;
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			GfxMeshPrimitives::CalcNormals32(normals, (const v3dxVector3*)pVert, nVert, (UINT*)ibDesc.InitData, nTri);
		}

		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)&normals[0];
		AutoRef<IVertexBuffer> vbNor = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Normal, vbNor);

		DrawPrimitiveDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtomLOD(0, &desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		result->GetResourceState()->SetKeepValid(TRUE);

		result->GetGeomtryMesh()->SetIsDirty(TRUE);

		return result;
	}
}

GfxMeshPrimitives* PhyShape::IfGetConvexMesh(IRenderContext* rc)
{
	if (mType != PST_Convex)
		return nullptr;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxConvexMeshGeometry geom;
		if (false == mShape->getConvexMeshGeometry(geom))
			return nullptr;

		auto result = new GfxMeshPrimitives();
		UINT nVert = geom.convexMesh->getNbVertices();
		auto pVert = geom.convexMesh->getVertices();

		IVertexBufferDesc vbDesc;
		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)pVert;
		AutoRef<IVertexBuffer> vbPos = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Position, vbPos);

		std::vector<v3dxVector3> normals;
		const BYTE* indexBuffer = geom.convexMesh->getIndexBuffer();

		std::vector<USHORT> indices;
		auto nbPolygons = geom.convexMesh->getNbPolygons();
		physx::PxU32 offset = 0;
		for (physx::PxU32 i = 0; i < nbPolygons; i++)
		{
			physx::PxHullPolygon face;
			bool status = geom.convexMesh->getPolygonData(i, face);
			PX_ASSERT(status);

			/*const physx::PxU8* faceIndices = indexBuffer + face.mIndexBase;
			for (physx::PxU32 j = 0; j < face.mNbVerts; j++)
			{
				vertices[offset + j] = pVert[faceIndices[j]];
				normals[offset + j] = PxVec3(face.mPlane[0], face.mPlane[1], face.mPlane[2]);
			}*/

			for (physx::PxU32 j = 2; j < face.mNbVerts; j++)
			{
				indices.push_back(physx::PxU16(offset));
				indices.push_back(physx::PxU16(offset + j));
				indices.push_back(physx::PxU16(offset + j - 1));
			}

			offset += face.mNbVerts;
		}

		UINT nTri = (UINT)indices.size() / 3;

		IIndexBufferDesc ibDesc;
		ibDesc.ByteWidth = sizeof(USHORT) * nTri * 3;
		ibDesc.Type = IBT_Int16;
		ibDesc.InitData = &indices[0];
		AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
		result->GetGeomtryMesh()->BindIndexBuffer(ib);

		GfxMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);

		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)&normals[0];
		AutoRef<IVertexBuffer> vbNor = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Normal, vbNor);

		DrawPrimitiveDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtomLOD(0, &desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		result->GetResourceState()->SetKeepValid(TRUE);

		return result;
	}
	else
	{
		physx::PxConvexMeshGeometry geom;
		if (false == mShape->getConvexMeshGeometry(geom))
			return nullptr;

		auto result = new GfxMeshPrimitives();
		UINT nVert = geom.convexMesh->getNbVertices();
		auto pVert = geom.convexMesh->getVertices();

		IVertexBufferDesc vbDesc;
		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)pVert;
		AutoRef<IVertexBuffer> vbPos = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Position, vbPos);

		std::vector<v3dxVector3> normals;
		const BYTE* indexBuffer = geom.convexMesh->getIndexBuffer();

		std::vector<USHORT> indices;
		auto nbPolygons = geom.convexMesh->getNbPolygons();
		physx::PxU32 offset = 0;
		for (physx::PxU32 i = 0; i < nbPolygons; i++)
		{
			physx::PxHullPolygon face;
			bool status = geom.convexMesh->getPolygonData(i, face);
			PX_ASSERT(status);

			/*const physx::PxU8* faceIndices = indexBuffer + face.mIndexBase;
			for (physx::PxU32 j = 0; j < face.mNbVerts; j++)
			{
				vertices[offset + j] = pVert[faceIndices[j]];
				normals[offset + j] = PxVec3(face.mPlane[0], face.mPlane[1], face.mPlane[2]);
			}*/

			for (physx::PxU32 j = 2; j < face.mNbVerts; j++)
			{
				indices.push_back(physx::PxU16(offset));
				indices.push_back(physx::PxU16(offset + j));
				indices.push_back(physx::PxU16(offset + j - 1));
			}

			offset += face.mNbVerts;
		}

		UINT nTri = (UINT)indices.size() / 3;

		IIndexBufferDesc ibDesc;
		ibDesc.ByteWidth = sizeof(USHORT) * nTri * 3;
		ibDesc.Type = IBT_Int16;
		ibDesc.InitData = &indices[0];
		AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
		result->GetGeomtryMesh()->BindIndexBuffer(ib);

		GfxMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);

		vbDesc.ByteWidth = sizeof(v3dxVector3) * nVert;
		vbDesc.Stride = sizeof(v3dxVector3);
		vbDesc.InitData = (void*)&normals[0];
		AutoRef<IVertexBuffer> vbNor = rc->CreateVertexBuffer(&vbDesc);
		result->GetGeomtryMesh()->BindVertexBuffer(VST_Normal, vbNor);

		DrawPrimitiveDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtomLOD(0, &desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		result->GetResourceState()->SetKeepValid(TRUE);

		return result;
	}
}

int PhyShape::GetTrianglesRemap(int index)
{
	if (mTrianglesRemap == nullptr)
		return -1;

	if (mTrianglesRemapNumber <= index)
		return -1;

	return mTrianglesRemap[index];
}
NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(EPhysShapeType, EngineNS, PhyShape, GetType);
	CSharpAPI1(EngineNS, PhyShape, SetType, EPhysShapeType);
	CSharpAPI1(EngineNS, PhyShape, SetLocalPose, const physx::PxTransform*);
	CSharpAPI1(EngineNS, PhyShape, GetLocalPose, physx::PxTransform*);
	CSharpAPI2(EngineNS, PhyShape, SetFlag, EPhysShapeFlag, vBOOL);
	CSharpAPI1(EngineNS, PhyShape, SetQueryFilterData, physx::PxFilterData*);
	CSharpReturnAPI2(vBOOL, EngineNS, PhyShape, AddToActor, PhyActor*, const physx::PxTransform*);
	CSharpAPI0(EngineNS, PhyShape, RemoveFromActor);
	CSharpReturnAPI3(vBOOL, EngineNS, PhyShape, IfGetBox, float*, float*, float*);
	CSharpReturnAPI1(vBOOL, EngineNS, PhyShape, IfGetSphere, float*);
	CSharpReturnAPI2(vBOOL, EngineNS, PhyShape, IfGetCapsule, float*, float*);
	CSharpReturnAPI2(vBOOL, EngineNS, PhyShape, IfGetScaling, v3dxVector3*, v3dxQuaternion*);
	CSharpReturnAPI1(GfxMeshPrimitives*, EngineNS, PhyShape, IfGetTriMesh, IRenderContext*);
	CSharpReturnAPI1(GfxMeshPrimitives*, EngineNS, PhyShape, IfGetConvexMesh, IRenderContext*);

	CSharpReturnAPI1(int, EngineNS, PhyShape, GetTrianglesRemap, int);
}