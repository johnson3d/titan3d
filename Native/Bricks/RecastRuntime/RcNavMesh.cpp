#include "../../Graphics/GfxPreHead.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "RcNavMesh.h"
#include "RcNavQuery.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::RcNavMesh, EngineNS::VIUnknown);

RcNavMesh::RcNavMesh()
{
	mNavMesh = nullptr;
	mNavQuery = nullptr;
	mCrowd = nullptr;
}

RcNavMesh::~RcNavMesh()
{
	if (mNavMesh != nullptr)
	{
		dtFreeNavMesh(mNavMesh);
		mNavMesh = nullptr;
	}
}

//vBOOL RcNavMesh::Init(dtNavMesh* mesh)
//{
//	if (mNavMesh != nullptr)
//		dtFreeNavMesh(mNavMesh);
//	mNavMesh = mesh;
//
//	auto status = mNavQuery->init(mNavMesh, 2048);
//	if (dtStatusFailed(status))
//	{
//		//m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
//		return FALSE;
//	}
//	return TRUE;
//}

RcNavQuery* RcNavMesh::CreateQuery(int maxNodes)
{
	auto p = dtAllocNavMeshQuery();
	auto status = p->init(mNavMesh, maxNodes);
	if (dtStatusFailed(status))
	{
		//m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init Detour navmesh query");
		return nullptr;
	}
	return new RcNavQuery(p);
}

vBOOL RcNavMesh::LoadXnd(XNDNode* node)
{
	auto attr = node->GetAttrib("NavMesh");
	attr->BeginRead(__FILE__, __LINE__);
	NavMeshSetHeader header;
	size_t readLen = attr->Read(&header, sizeof(NavMeshSetHeader));
	if (readLen != sizeof(NavMeshSetHeader))
	{
		attr->EndRead();
		return FALSE;
	}
	if (header.magic != NAVMESHSET_MAGIC)
	{
		attr->EndRead();
		return FALSE;
	}
	if (header.version != NAVMESHSET_VERSION)
	{
		attr->EndRead();
		return FALSE;
	}

	dtNavMesh* mesh = dtAllocNavMesh();
	if (!mesh)
	{
		attr->EndRead();
		return FALSE;
	}
	dtStatus status = mesh->init(&header.params);
	if (dtStatusFailed(status))
	{
		attr->EndRead();
		return FALSE;
	}
	// Read tiles.
	for (int i = 0; i < header.numTiles; ++i)
	{
		NavMeshTileHeader tileHeader;
		readLen = attr->Read(&tileHeader, sizeof(tileHeader));
		if (readLen != sizeof(tileHeader))
		{
			attr->EndRead();
			return FALSE;
		}

		if (!tileHeader.tileRef || !tileHeader.dataSize)
			break;

		unsigned char* data = (unsigned char*)dtAlloc(tileHeader.dataSize, DT_ALLOC_PERM);
		if (!data) break;
		memset(data, 0, tileHeader.dataSize);
		readLen = attr->Read(data, tileHeader.dataSize);
		if (readLen != tileHeader.dataSize)
		{
			dtFree(data);
			attr->EndRead();
			return FALSE;
		}

		mesh->addTile(data, tileHeader.dataSize, DT_TILE_FREE_DATA, tileHeader.tileRef, 0);
	}
	attr->EndRead();

	if (mNavMesh != nullptr)
		dtFreeNavMesh(mNavMesh);
	mNavMesh = mesh;
	return TRUE;
}

void RcNavMesh::Save2Xnd(XNDNode* node)
{
	auto attr = node->AddAttrib("NavMesh");

	attr->BeginWrite();
	// Store header.
	NavMeshSetHeader header;
	header.magic = NAVMESHSET_MAGIC;
	header.version = NAVMESHSET_VERSION;
	header.numTiles = 0;
	for (int i = 0; i < mNavMesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = ((const dtNavMesh*)mNavMesh)->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) 
			continue;
		header.numTiles++;
	}
	memcpy(&header.params, mNavMesh->getParams(), sizeof(dtNavMeshParams));
	attr->Write(&header, sizeof(NavMeshSetHeader));

	// Store tiles.
	for (int i = 0; i < mNavMesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = ((const dtNavMesh*)mNavMesh)->getTile(i);
		if (!tile || !tile->header || !tile->dataSize) 
			continue;

		NavMeshTileHeader tileHeader;
		tileHeader.tileRef = mNavMesh->getTileRef(tile);
		tileHeader.dataSize = tile->dataSize;
		attr->Write(&tileHeader, sizeof(tileHeader));

		attr->Write(tile->data, tile->dataSize);
	}

	attr->EndWrite();
}

GfxMeshPrimitives* RcNavMesh::CreateRenderMesh(IRenderContext* rc)
{
	std::vector<v3dxVector3> posData;
	std::vector<v3dxVector3> norData;
	std::vector<v3dxVector2> uvData;
	std::vector<int> ibData;
	for (int i = 0; i < mNavMesh->getMaxTiles(); ++i)
	{
		const dtMeshTile* tile = ((const dtNavMesh*)mNavMesh)->getTile(i);
		if (!tile || !tile->header || !tile->dataSize)
			continue;
		//auto pos = (v3dxVector3*)tile->detailVerts;
		//auto ib = (char*)tile->detailTris;
		//int startVert = (int)posData.size();
		for (int j = 0; j < tile->header->polyCount; j++)
		{
			const dtPoly* p = &tile->polys[j];
			if (p->getType() == DT_POLYTYPE_OFFMESH_CONNECTION)	// Skip off-mesh links.
				continue;

			const dtPolyDetail* pd = &tile->detailMeshes[j];
			
			for (int k = 0; k < pd->triCount; ++k)
			{
				const unsigned char* t = &tile->detailTris[(pd->triBase + k) * 4];
				v3dxVector3 nor;
				auto va = *(v3dxVector3*)&tile->verts[p->verts[t[0]] * 3];
				auto vb = *(v3dxVector3*)&tile->verts[p->verts[t[1]] * 3];
				auto vc = *(v3dxVector3*)&tile->verts[p->verts[t[2]] * 3];
				
				auto v1 = vb - va;
				auto v2 = vc - va;

				if (v1.getLength() == 0)
				{
					v1 = v3dxVector3::UNIT_Y;
				}
				if (v2.getLength() == 0)
				{
					v2 = v3dxVector3::UNIT_Y;
				}
				v3dxVec3Cross(&nor, &v1, &v2);
				nor.normalize();
				for (int n = 0; n < 3; ++n)
				{
					v3dxVector2 uv;
					uv.x = float(n)/3;
					uv.y = 1 - float(n) / 3;
					uvData.push_back(uv);
					norData.push_back(nor);
					if (t[n] < p->vertCount)
					{
						auto vt = (v3dxVector3*)&tile->verts[p->verts[t[n]] * 3];
						posData.push_back(*vt);
					}
					else
					{
						auto vt = (v3dxVector3*)&tile->detailVerts[(pd->vertBase + t[n] - p->vertCount) * 3];
						posData.push_back(*vt);
					}
				}
			}
		}
		/*for (int j = 0; j < tile->header->vertCount; j++)
		{
			v3dxVector3 vt = pos[tile->detailMeshes->vertBase + j];
			posData.push_back(vt);
		}
		for (int j = 0; j < tile->detailMeshes->triCount; j++)
		{
			int A = startVert + ib[j * 3 + 0];
			int B = startVert + ib[j * 3 + 1];
			int C = startVert + ib[j * 3 + 2];
			ibData.push_back(A);
			ibData.push_back(B);
			ibData.push_back(C);
		}*/
	}
	if (posData.size() == 0)
		return nullptr;
	
	GfxMeshPrimitives* result = new GfxMeshPrimitives();
	result->Init(rc, "", 1);

	IGeometryMesh* mesh = result->GetGeomtryMesh();

	IVertexBufferDesc vbDesc;
	vbDesc.ByteWidth = sizeof(v3dxVector3) * (UINT)posData.size();
	vbDesc.CPUAccess = 0;
	vbDesc.InitData = &posData[0];
	vbDesc.Stride = sizeof(v3dxVector3);
	AutoRef<IVertexBuffer> vbPos = rc->CreateVertexBuffer(&vbDesc);
	mesh->BindVertexBuffer(VST_Position, vbPos);

	vbDesc.ByteWidth = sizeof(v3dxVector3) * (UINT)norData.size();
	vbDesc.CPUAccess = 0;
	vbDesc.InitData = &norData[0];
	vbDesc.Stride = sizeof(v3dxVector3);
	AutoRef<IVertexBuffer> vbNor = rc->CreateVertexBuffer(&vbDesc);
	mesh->BindVertexBuffer(VST_Normal, vbNor);

	vbDesc.ByteWidth = sizeof(v3dxVector2) * (UINT)uvData.size();
	vbDesc.CPUAccess = 0;
	vbDesc.InitData = &uvData[0];
	vbDesc.Stride = sizeof(v3dxVector2);
	AutoRef<IVertexBuffer> vbUV = rc->CreateVertexBuffer(&vbDesc);
	mesh->BindVertexBuffer(VST_UV, vbUV);

	DrawPrimitiveDesc dpDesc;
	dpDesc.StartIndex = 0xFFFFFFFF;
	dpDesc.NumPrimitives = (UINT)posData.size() / 3;
	result->PushAtomLOD(0, &dpDesc);
	
	/*IIndexBufferDesc ibDesc;
	ibDesc.ByteWidth = sizeof(int) * (UINT)ibData.size();
	ibDesc.CPUAccess = 0;
	ibDesc.InitData = &ibData[0];
	ibDesc.Type = IBT_Int32;
	auto indexBuffer = rc->CreateIndexBuffer(&ibDesc);
	mesh->BindIndexBuffer(indexBuffer);*/

	result->GetResourceState()->SetStreamState(SS_Valid);
	mesh->SetIsDirty(TRUE);
	
	return result;
}

int RcNavMesh::GetTilesCount()
{
	return mNavMesh->getMaxTiles();
}

vBOOL RcNavMesh::CheckVaildAt(int tileindex, int layer)
{
	if (mNavMesh == nullptr)
		return FALSE;

	const dtMeshTile* tile = (const dtMeshTile*)(((const dtNavMesh*)mNavMesh)->getTile(tileindex));
	return tile != nullptr && tile->header != nullptr && tile->dataSize != 0 && tile->header->polyCount > layer;
}

v3dxVector3 RcNavMesh::GetPositionAt(int tileindex, int layer)
{
	if (CheckVaildAt(tileindex, layer) == false)
		return v3dxVector3();

	const dtMeshTile* tile = (const dtMeshTile*)(((const dtNavMesh*)mNavMesh)->getTile(tileindex));

	float tricount = 0.0f;
	v3dxVector3 pos(0.0f, 0.0f, 0.0f);

	const dtPoly* p = &tile->polys[layer];
	if (p->getType() == DT_POLYTYPE_OFFMESH_CONNECTION)	// Skip off-mesh links.
		return v3dxVector3();

	const dtPolyDetail* pd = &tile->detailMeshes[layer];

	for (int k = 0; k < pd->triCount; ++k)
	{
		const unsigned char* t = &tile->detailTris[(pd->triBase + k) * 4];
		v3dxVector3 va = *(v3dxVector3*)&tile->verts[p->verts[t[0]] * 3];
		pos += va;

		v3dxVector3 vb = *(v3dxVector3*)&tile->verts[p->verts[t[1]] * 3];
		pos += vb;

		v3dxVector3 vc = *(v3dxVector3*)&tile->verts[p->verts[t[2]] * 3];
		pos += vc;

		tricount++;
	}

	return pos / (tricount * 3.0f);
}

v3dxVector3 RcNavMesh::GetBoundBoxMinAt(int tileindex)
{
	if (CheckVaildAt(tileindex, 0) == false)
		return v3dxVector3();

	const dtMeshTile* mtile = (const dtMeshTile*)(((const dtNavMesh*)mNavMesh)->getTile(tileindex));
	return v3dxVector3(mtile->bvTree->bmin[0], mtile->bvTree->bmin[1], mtile->bvTree->bmin[2]);
}

v3dxVector3 RcNavMesh::GetBoundBoxMaxAt(int tileindex)
{
	if (CheckVaildAt(tileindex, 0) == false)
		return v3dxVector3();

	const dtMeshTile* mtile = (const dtMeshTile*)(((const dtNavMesh*)mNavMesh)->getTile(tileindex));
	return v3dxVector3(mtile->bvTree->bmax[0], mtile->bvTree->bmax[1], mtile->bvTree->bmax[2]);
}

NS_END

using namespace EngineNS;

template <>
struct Type2TypeConverter<v3dxVector3>
{
	typedef v3dVector3_t		TarType;
};

extern "C"
{
	Cpp2CS1(EngineNS, RcNavMesh, CreateQuery);
	Cpp2CS1(EngineNS, RcNavMesh, LoadXnd);
	Cpp2CS1(EngineNS, RcNavMesh, Save2Xnd);

	Cpp2CS1(EngineNS, RcNavMesh, CreateRenderMesh);

	Cpp2CS0(EngineNS, RcNavMesh, GetTilesWidth);
	Cpp2CS0(EngineNS, RcNavMesh, GetTilesHeight);
	Cpp2CS0(EngineNS, RcNavMesh, GetTilesCount);
	Cpp2CS2(EngineNS, RcNavMesh, CheckVaildAt);
	Cpp2CS2(EngineNS, RcNavMesh, GetPositionAt);
	Cpp2CS1(EngineNS, RcNavMesh, GetBoundBoxMinAt);
	Cpp2CS1(EngineNS, RcNavMesh, GetBoundBoxMaxAt);
}