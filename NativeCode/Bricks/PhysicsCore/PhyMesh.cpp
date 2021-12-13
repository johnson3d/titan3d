#include "PhyMesh.h"
#include "PhyContext.h"
#include "../../RHI/IRenderContext.h"
#include "../../RHI/Utility/IMeshPrimitives.h"
#include "../../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

PhyTriMesh::~PhyTriMesh()
{
	Cleanup();
}

void PhyTriMesh::Cleanup()
{
	if (mMesh != nullptr)
	{
		mMesh->release();
		mMesh = nullptr;
	}
}

bool PhyTriMesh::CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size)
{
	Cleanup();

	mCookedData.ReSize(0);
	mCookedData.PushData(cookedData, size);
	physx::PxDefaultMemoryInputData readBuffer((physx::PxU8*)cookedData, size);
	mMesh = ctx->mContext->createTriangleMesh(readBuffer);
	if (mMesh == nullptr)
		return false;

	return true;
}

IMeshDataProvider* PhyTriMesh::CreateMeshProvider()
{
	IMeshDataProvider* result = new IMeshDataProvider();
	bool isIndex16;
	if (mMesh->getTriangleMeshFlags() & physx::PxTriangleMeshFlag::Enum::e16_BIT_INDICES)
	{
		result->Init((1<<EVertexSteamType::VST_Position) | 
			(1<<EVertexSteamType::VST_Color), EIndexBufferType::IBT_Int16, 1);
		isIndex16 = true;
	}
	else
	{
		result->Init((1 << EVertexSteamType::VST_Position) |
			(1 << EVertexSteamType::VST_Color), EIndexBufferType::IBT_Int32, 1);
		isIndex16 = false;
	}

	int nbTri = (int)mMesh->getNbTriangles();
	auto pPos = (v3dxVector3*)mMesh->getVerticesForModification();
	int nbPos = (int)mMesh->getNbVertices();
	for (int i = 0; i < nbPos; i++)
	{
		result->AddVertex(&pPos[i], nullptr, nullptr, 0);
	}
	
	DrawPrimitiveDesc dpDesc;
	dpDesc.SetDefault();
	IBlobObject* pColorBlob = result->GetStream(EVertexSteamType::VST_Color);
	DWORD* pColor = (DWORD*)pColorBlob->GetData();
	/*USHORT maxMtlIdx = 0;
	for (int i = 0; i < nbTri; i++)
	{
		USHORT mtlIdx = mMesh->getTriangleMaterialIndex((UINT)i);
		if (mtlIdx == 0xFFFF)
		{
			mtlIdx = 0;
		}
		if (mtlIdx > maxMtlIdx)
		{
			maxMtlIdx = mtlIdx;
		}
	}*/
	DWORD mdlColors[] = {
		0xFFFF0000,
		0xFF00FF00,
		0xFF0000FF,
		0xFFFF00FF,
		0xFFFFFF00,
		0xFF00FFFF,
		0xFF800000,
		0xFF008000,
		0xFF000080,
		0xFF800080,
		0xFF808000,
		0xFF008080,
	};
	if(isIndex16)
	{
		auto pIndices = (USHORT*)mMesh->getTriangles();
		for (int i = 0; i < nbTri; i++)
		{
			USHORT mtlIdx = mMesh->getTriangleMaterialIndex((UINT)i);
			if (mtlIdx == 0xFFFF)
			{
				mtlIdx = 0;
			}
			auto clr = mdlColors[mtlIdx % (sizeof(mdlColors) / sizeof(DWORD))];
			pColor[pIndices[i * 3 + 0]] = clr;
			pColor[pIndices[i * 3 + 1]] = clr;
			pColor[pIndices[i * 3 + 2]] = clr;
			result->AddTriangle(pIndices[i * 3 + 0], pIndices[i * 3 + 1], pIndices[i * 3 + 2]);
		}
	}
	else
	{
		auto pIndices = (UINT*)mMesh->getTriangles();
		for (int i = 0; i < nbTri; i++)
		{
			USHORT mtlIdx = mMesh->getTriangleMaterialIndex((UINT)i);
			if (mtlIdx == 0xFFFF)
			{
				mtlIdx = 0;
			}
			auto clr = mdlColors[mtlIdx % (sizeof(mdlColors) / sizeof(DWORD))];
			pColor[pIndices[i * 3 + 0]] = clr;
			pColor[pIndices[i * 3 + 1]] = clr;
			pColor[pIndices[i * 3 + 2]] = clr;
			result->AddTriangle(pIndices[i * 3 + 0], pIndices[i * 3 + 1], pIndices[i * 3 + 2]);
		}
	}
	dpDesc.NumPrimitives = nbTri;
	result->PushAtomLOD(0, &dpDesc);
	auto bounds = mMesh->getLocalBounds();
	v3dxBox3 aabb;
	aabb.minbox = *(v3dxVector3*)&bounds.minimum;
	aabb.maxbox = *(v3dxVector3*)&bounds.maximum;
	result->SetAABB(&aabb);
	return result;
}

PhyConvexMesh::~PhyConvexMesh()
{
	if (mMesh != nullptr)
	{
		mMesh->release();
		mMesh = nullptr;
	}
	Safe_Release(mCookedData);
}

NS_END
