#include "PhyShape.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyMaterial.h"
#include "../../RHI/IRenderContext.h"
#include "../../RHI/Utility/IMeshPrimitives.h"

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

void PhyShape::SetQueryFilterData(const PhyFilterData* filterData)
{
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		mShape->setQueryFilterData(*(physx::PxFilterData*)filterData);
	}
	else
	{
		mShape->setQueryFilterData(*(physx::PxFilterData*)filterData);
	}
}

void PhyShape::SetFlag(EPhysShapeFlag flag, bool value)
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

bool PhyShape::HaveFlag(EPhysShapeFlag flag)
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

void PhyShape::GetMaterials(PhyMaterial** materials, int count)
{
	count = mShape->getNbMaterials();
	std::vector<physx::PxMaterial*> pxMtls;
	pxMtls.resize(count);
	mShape->getMaterials(&pxMtls[0], count * sizeof(physx::PxMaterial*), 0);
}

void PhyShape::SetMaterials(PhyMaterial** materials, int count)
{
	std::vector<physx::PxMaterial*> pxMtls;
	for (int i = 0; i < count; i++)
	{
		pxMtls.push_back(materials[i]->mMaterial);
	}
	mShape->setMaterials(&pxMtls[0], (USHORT)count);
}

bool PhyShape::IfGetBox(v3dxVector3* halfExtent)
{
	if (mType != PST_Box)
		return false;

	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxBoxGeometry geom;
		if (false == mShape->getBoxGeometry(geom))
			return false;
		halfExtent->x = geom.halfExtents.x;
		halfExtent->y = geom.halfExtents.y;
		halfExtent->z = geom.halfExtents.z;
	}
	else
	{
		physx::PxBoxGeometry geom;
		if (false == mShape->getBoxGeometry(geom))
			return false;
		halfExtent->x = geom.halfExtents.x;
		halfExtent->y = geom.halfExtents.y;
		halfExtent->z = geom.halfExtents.z;
	}
	return true;
}

bool PhyShape::IfSetBox(const v3dxVector3* halfExtent)
{
	if (mType != PST_Box)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxBoxGeometry geom;
		geom.halfExtents.x = halfExtent->x;
		geom.halfExtents.y = halfExtent->y;
		geom.halfExtents.z = halfExtent->z;

		mShape->setGeometry(geom);

		return true;
	}
	return false;
}

bool PhyShape::IfGetSphere(float* radius)
{
	if (mType != PST_Sphere)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxSphereGeometry geom;
		if (false == mShape->getSphereGeometry(geom))
			return false;
		*radius = geom.radius;
	}
	else
	{
		physx::PxSphereGeometry geom;
		if (false == mShape->getSphereGeometry(geom))
			return false;
		*radius = geom.radius;
	}
	return true;
}

bool PhyShape::IfSetSphere(float radius)
{
	if (mType != PST_Sphere)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSphereGeometry geom;
		geom.radius = radius;

		mShape->setGeometry(geom);
		return true;
	}
	return false;
}

bool PhyShape::IfGetCapsule(float* radius, float* halfHeight)
{
	if (mType != PST_Capsule)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxCapsuleGeometry geom;
		if (false == mShape->getCapsuleGeometry(geom))
			return false;
		*radius = geom.radius;
		*halfHeight = geom.halfHeight;
	}
	else
	{
		physx::PxCapsuleGeometry geom;
		if (false == mShape->getCapsuleGeometry(geom))
			return false;
		*radius = geom.radius;
		*halfHeight = geom.halfHeight;
	}
	return true;
}

bool PhyShape::IfSetCapsule(float radius, float halfHeight)
{
	if (mType != PST_Capsule)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxCapsuleGeometry geom;
		geom.radius = radius;
		geom.halfHeight = halfHeight;

		mShape->setGeometry(geom);
		return true;
	}
	return false;
}

bool PhyShape::IfGetTriMeshScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot)
{
	if (mType != PST_TriangleMesh)
		return false;
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

bool PhyShape::IfSetTriMeshScaling(const v3dxVector3* scale, const v3dxQuaternion* scaleRot)
{
	if (mType != PST_TriangleMesh)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return false;
		geom.scale.scale = *((physx::PxVec3*)scale);
		geom.scale.rotation = *((physx::PxQuat*)scaleRot);

		mShape->setGeometry(geom);
		return true;
	}
	return false;
}

IMeshPrimitives* PhyShape::IfGetTriMesh(IRenderContext* rc)
{
	if (mType != PST_TriangleMesh)
		return nullptr;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return nullptr;

		auto result = new IMeshPrimitives();
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

			IMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);
		}
		else
		{
			IIndexBufferDesc ibDesc;
			ibDesc.ByteWidth = sizeof(UINT) * nTri * 3;
			ibDesc.Type = IBT_Int32;
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			IMeshPrimitives::CalcNormals32(normals, (const v3dxVector3*)pVert, nVert, (UINT*)ibDesc.InitData, nTri);
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
		
		result->GetGeomtryMesh()->mIsDirty = TRUE;

		return result;
	}
	else
	{
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return nullptr;

		auto result = new IMeshPrimitives();
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

			IMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);
		}
		else
		{
			IIndexBufferDesc ibDesc;
			ibDesc.ByteWidth = sizeof(UINT) * nTri * 3;
			ibDesc.Type = IBT_Int32;
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&ibDesc);
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			IMeshPrimitives::CalcNormals32(normals, (const v3dxVector3*)pVert, nVert, (UINT*)ibDesc.InitData, nTri);
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
		//result->GetResourceState()->SetKeepValid(TRUE);

		result->GetGeomtryMesh()->mIsDirty = TRUE;

		return result;
	}
}

IMeshPrimitives* PhyShape::IfGetConvexMesh(IRenderContext* rc)
{
	if (mType != PST_Convex)
		return nullptr;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetPtr()->mScene);
		physx::PxConvexMeshGeometry geom;
		if (false == mShape->getConvexMeshGeometry(geom))
			return nullptr;

		auto result = new IMeshPrimitives();
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

		IMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);

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
		//result->GetResourceState()->SetKeepValid(TRUE);

		return result;
	}
	else
	{
		physx::PxConvexMeshGeometry geom;
		if (false == mShape->getConvexMeshGeometry(geom))
			return nullptr;

		auto result = new IMeshPrimitives();
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

		IMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);

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
		//result->GetResourceState()->SetKeepValid(TRUE);

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
