#include "NvPhyShape.h"
#include "NvPhyScene.h"
#include "NvPhyActor.h"
#include "NvPhyMaterial.h"
#include "../../../NextRHI/NxRHI.h"

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma GCC diagnostic ignored "-Wunused-variable"
#endif

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::NvPhyShape);

using namespace NxRHI;

NvPhyShape::NvPhyShape()
{
	mShape = nullptr;
	EntityType = Phy_Shape;
	mTrianglesRemap = nullptr;
	mTrianglesRemapNumber = 0;
}

NvPhyShape::~NvPhyShape()
{
	Cleanup();
}

void NvPhyShape::Cleanup()
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
			auto ps = (NvPhyScene*)actor->mScene.GetPtr();
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

void NvPhyShape::BindPhysX()
{
	ASSERT(mShape);
	mShape->userData = this;
}

vBOOL NvPhyShape::AddToActor(PhyActor* actor, const physx::PxTransform* relativePose)
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

	return ((NvPhyActor*)actor)->AttachShape(this, relativePose);
}

void NvPhyShape::RemoveFromActor()
{
	auto prev = mActor.GetPtr();
	if (prev != nullptr)
	{
		prev->DetachShape(this, true);
	}

	mActor.FromObject(nullptr);
}

void NvPhyShape::SetLocalPose(const physx::PxTransform* relativePose)
{
	if (mActor.IsValid() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		mShape->setLocalPose(*relativePose);
	}
	else
	{
		mShape->setLocalPose(*relativePose);
	}
}

void NvPhyShape::GetLocalPose(physx::PxTransform* relativePose)
{
	if (mActor.IsValid() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneReadLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		*relativePose = mShape->getLocalPose();
	}
	else
	{
		*relativePose = mShape->getLocalPose();
	}
}

void NvPhyShape::SetQueryFilterData(const FPhyFilterData* filterData)
{
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		mShape->setQueryFilterData(*(physx::PxFilterData*)filterData);
	}
	else
	{
		mShape->setQueryFilterData(*(physx::PxFilterData*)filterData);
	}

}void NvPhyShape::SetSimulationFilterData(const FPhyFilterData* filterData)
{
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		mShape->setSimulationFilterData(*(physx::PxFilterData*)filterData);
	}
	else
	{
		mShape->setSimulationFilterData(*(physx::PxFilterData*)filterData);
	}
}


void NvPhyShape::SetFlag(EPhysShapeFlag flag, bool value)
{
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		mShape->setFlag((physx::PxShapeFlag::Enum)flag, value);
	}
	else
	{
		mShape->setFlag((physx::PxShapeFlag::Enum)flag, value);
	}
}

bool NvPhyShape::HaveFlag(EPhysShapeFlag flag)
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

void NvPhyShape::GetMaterials(PhyMaterial** materials, int count)
{
	count = mShape->getNbMaterials();
	std::vector<physx::PxMaterial*> pxMtls;
	pxMtls.resize(count);
	mShape->getMaterials(&pxMtls[0], count * sizeof(physx::PxMaterial*), 0);
}

void NvPhyShape::SetMaterials(PhyMaterial** materials, int count)
{
	std::vector<physx::PxMaterial*> pxMtls;
	for (int i = 0; i < count; i++)
	{
		pxMtls.push_back(((NvPhyMaterial*)materials[i])->mMaterial);
	}
	mShape->setMaterials(&pxMtls[0], (USHORT)count);
}

bool NvPhyShape::IfGetBox(v3dxVector3* halfExtent)
{
	if (mType != PST_Box)
		return false;

	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		physx::PxBoxGeometry geom;
		if (false == mShape->getBoxGeometry(geom))
			return false;
		halfExtent->X = geom.halfExtents.x;
		halfExtent->Y = geom.halfExtents.y;
		halfExtent->Z = geom.halfExtents.z;
	}
	else
	{
		physx::PxBoxGeometry geom;
		if (false == mShape->getBoxGeometry(geom))
			return false;
		halfExtent->X = geom.halfExtents.x;
		halfExtent->Y = geom.halfExtents.y;
		halfExtent->Z = geom.halfExtents.z;
	}
	return true;
}

bool NvPhyShape::IfSetBox(const v3dxVector3* halfExtent)
{
	if (mType != PST_Box)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxBoxGeometry geom;
		geom.halfExtents.x = halfExtent->X;
		geom.halfExtents.y = halfExtent->Y;
		geom.halfExtents.z = halfExtent->Z;

		mShape->setGeometry(geom);

		return true;
	}
	return false;
}

bool NvPhyShape::IfGetSphere(float* radius)
{
	if (mType != PST_Sphere)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
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

bool NvPhyShape::IfSetSphere(float radius)
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

bool NvPhyShape::IfGetCapsule(float* radius, float* halfHeight)
{
	if (mType != PST_Capsule)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
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

bool NvPhyShape::IfSetCapsule(float radius, float halfHeight)
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

bool NvPhyShape::IfGetTriMeshScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot)
{
	if (mType != PST_TriangleMesh)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
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

bool NvPhyShape::IfSetTriMeshScaling(const v3dxVector3* scale, const v3dxQuaternion* scaleRot)
{
	if (mType != PST_TriangleMesh)
		return false;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>() != nullptr)
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

FMeshPrimitives* NvPhyShape::IfGetTriMesh(IGpuDevice* rc)
{
	if (mType != PST_TriangleMesh)
		return nullptr;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return nullptr;

		auto result = new FMeshPrimitives();
		result->Init(rc, "", 1);
		UINT nVert = geom.triangleMesh->getNbVertices();
		UINT nTri = geom.triangleMesh->getNbTriangles();
		auto pVert = geom.triangleMesh->getVertices();

		FVbvDesc vbvDesc;
		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)pVert;
		auto vbPos = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Position, vbPos);

		std::vector<v3dxVector3> normals;
		auto ibflags = geom.triangleMesh->getTriangleMeshFlags();
		if (ibflags == physx::PxTriangleMeshFlag::e16_BIT_INDICES)
		{
			result->GetGeomtryMesh()->IsIndex32 = false;
			FBufferDesc ibDesc;
			ibDesc.SetDefault();
			ibDesc.Size = sizeof(USHORT) * nTri * 3;
			ibDesc.StructureStride = sizeof(USHORT);
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			auto ibBuffer = MakeWeakRef(rc->CreateBuffer(&ibDesc));
			FIbvDesc ibvDesc;
			ibvDesc.Stride = ibDesc.StructureStride;
			ibvDesc.Size = ibDesc.Size;
			auto ib = MakeWeakRef(rc->CreateIBV(ibBuffer, &ibvDesc));
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			FMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);
		}
		else
		{
			result->GetGeomtryMesh()->IsIndex32 = true;
			FBufferDesc ibDesc;
			ibDesc.Size = sizeof(UINT) * nTri * 3;
			ibDesc.StructureStride = sizeof(UINT);
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			auto ibBuffer = MakeWeakRef(rc->CreateBuffer(&ibDesc));
			FIbvDesc ibvDesc;
			ibvDesc.Stride = ibDesc.StructureStride;
			ibvDesc.Size = ibDesc.Size;
			auto ib = MakeWeakRef(rc->CreateIBV(ibBuffer, &ibvDesc));
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			FMeshPrimitives::CalcNormals32(normals, (const v3dxVector3*)pVert, nVert, (UINT*)ibDesc.InitData, nTri);
		}

		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)&normals[0];
		auto vbNor = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Normal, vbNor);

		FMeshAtomDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtom(0, desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		
		return result;
	}
	else
	{
		physx::PxTriangleMeshGeometry geom;
		if (false == mShape->getTriangleMeshGeometry(geom))
			return nullptr;

		auto result = new FMeshPrimitives();
		result->Init(rc, "", 1);
		UINT nVert = geom.triangleMesh->getNbVertices();
		UINT nTri = geom.triangleMesh->getNbTriangles();
		auto pVert = geom.triangleMesh->getVertices();

		FVbvDesc vbvDesc;
		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)pVert;
		auto vbPos = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Position, vbPos);

		std::vector<v3dxVector3> normals;
		auto ibflags = geom.triangleMesh->getTriangleMeshFlags();
		if (ibflags == physx::PxTriangleMeshFlag::e16_BIT_INDICES)
		{
			result->GetGeomtryMesh()->IsIndex32 = false;
			FBufferDesc ibDesc;
			ibDesc.Size = sizeof(USHORT) * nTri * 3;
			ibDesc.StructureStride = sizeof(USHORT);
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			auto ibBuffer = MakeWeakRef(rc->CreateBuffer(&ibDesc));
			FIbvDesc ibvDesc;
			ibvDesc.Stride = ibDesc.StructureStride;
			ibvDesc.Size = ibDesc.Size;
			auto ib = MakeWeakRef(rc->CreateIBV(ibBuffer, &ibvDesc));
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			FMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);
		}
		else
		{
			result->GetGeomtryMesh()->IsIndex32 = true;
			FBufferDesc ibDesc;
			ibDesc.Size = sizeof(UINT) * nTri * 3;
			ibDesc.StructureStride = sizeof(USHORT);
			ibDesc.InitData = (void*)geom.triangleMesh->getTriangles();
			auto ibBuffer = MakeWeakRef(rc->CreateBuffer(&ibDesc));
			FIbvDesc ibvDesc;
			ibvDesc.Stride = ibDesc.StructureStride;
			ibvDesc.Size = ibDesc.Size;
			auto ib = MakeWeakRef(rc->CreateIBV(ibBuffer, &ibvDesc));
			result->GetGeomtryMesh()->BindIndexBuffer(ib);

			FMeshPrimitives::CalcNormals32(normals, (const v3dxVector3*)pVert, nVert, (UINT*)ibDesc.InitData, nTri);
		}

		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)&normals[0];
		auto vbNor = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Normal, vbNor);

		FMeshAtomDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtom(0, desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		//result->GetResourceState()->SetKeepValid(TRUE);

		//result->GetGeomtryMesh()->mIsDirty = TRUE;

		return result;
	}
}

FMeshPrimitives* NvPhyShape::IfGetConvexMesh(IGpuDevice* rc)
{
	if (mType != PST_Convex)
		return nullptr;
	if (mActor.IsValid() && mActor.GetPtr() && mActor.GetPtr()->mScene.GetPtr() != nullptr)
	{
		physx::PxSceneWriteLock Lock(*mActor.GetPtr()->mScene.GetCastPtr<NvPhyScene>()->mScene);
		physx::PxConvexMeshGeometry geom;
		if (false == mShape->getConvexMeshGeometry(geom))
			return nullptr;

		auto result = new FMeshPrimitives();
		UINT nVert = geom.convexMesh->getNbVertices();
		auto pVert = geom.convexMesh->getVertices();

		FVbvDesc vbvDesc;
		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)pVert;
		auto vbPos = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Position, vbPos);

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

		FBufferDesc ibDesc;
		ibDesc.Size = sizeof(USHORT) * nTri * 3;
		ibDesc.StructureStride = sizeof(USHORT);
		ibDesc.InitData = &indices[0];
		auto ibBuffer = MakeWeakRef(rc->CreateBuffer(&ibDesc));
		FIbvDesc ibvDesc;
		ibvDesc.Stride = ibDesc.StructureStride;
		ibvDesc.Size = ibDesc.Size;
		auto ib = MakeWeakRef(rc->CreateIBV(ibBuffer, &ibvDesc));
		result->GetGeomtryMesh()->BindIndexBuffer(ib);

		FMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);

		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)&normals[0];
		auto vbNor = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Normal, vbNor);

		FMeshAtomDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtom(0, desc);

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

		auto result = new FMeshPrimitives();
		UINT nVert = geom.convexMesh->getNbVertices();
		auto pVert = geom.convexMesh->getVertices();

		FVbvDesc vbvDesc;
		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)pVert;
		auto vbPos = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Position, vbPos);

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

		FBufferDesc ibDesc;
		ibDesc.Size = sizeof(USHORT) * nTri * 3;
		ibDesc.StructureStride = sizeof(USHORT);
		ibDesc.InitData = &indices[0];
		auto ibBuffer = MakeWeakRef(rc->CreateBuffer(&ibDesc));
		FIbvDesc ibvDesc;
		ibvDesc.Stride = ibDesc.StructureStride;
		ibvDesc.Size = ibDesc.Size;
		auto ib = MakeWeakRef(rc->CreateIBV(ibBuffer, &ibvDesc));
		result->GetGeomtryMesh()->BindIndexBuffer(ib);

		FMeshPrimitives::CalcNormals16(normals, (const v3dxVector3*)pVert, nVert, (USHORT*)ibDesc.InitData, nTri);

		vbvDesc.Stride = sizeof(v3dxVector3);
		vbvDesc.Size = sizeof(v3dxVector3) * nVert;
		vbvDesc.InitData = (void*)&normals[0];
		auto vbNor = MakeWeakRef(rc->CreateVBV(nullptr, &vbvDesc));
		result->GetGeomtryMesh()->VertexArray->BindVB(VST_Normal, vbNor);

		FMeshAtomDesc desc;
		desc.NumPrimitives = nTri;
		result->PushAtom(0, desc);

		result->GetResourceState()->SetResourceSize((UINT)nVert * sizeof(v3dxVector3) * 2);
		result->GetResourceState()->SetStreamState(SS_Valid);
		//result->GetResourceState()->SetKeepValid(TRUE);

		return result;
	}
}

int NvPhyShape::GetTrianglesRemap(int index)
{
	if (mTrianglesRemap == nullptr)
		return -1;

	if (mTrianglesRemapNumber <= index)
		return -1;

	return mTrianglesRemap[index];
}
NS_END

