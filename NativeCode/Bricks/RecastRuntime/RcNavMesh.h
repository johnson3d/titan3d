#pragma once

#include "../../Base/xnd/vfxxnd.h"
#include "Recast.h"
#include "DetourNavMeshQuery.h"
#include "DetourCrowd.h"
#include "../../NextRHI/NxGeomMesh.h"

NS_BEGIN

struct NavMeshSetHeader
{
	int magic;
	int version;
	int numTiles;
	dtNavMeshParams params;
};

static const int NAVMESHSET_MAGIC = 'M' << 24 | 'S' << 16 | 'E' << 8 | 'T'; //'MSET';
static const int NAVMESHSET_VERSION = 1;

struct NavMeshTileHeader
{
	dtTileRef tileRef;
	int dataSize;
};

class RcNavQuery;

class TR_CLASS()
	RcNavMesh : public IWeakReference
{
protected:
	dtNavMesh*		mNavMesh;
	dtNavMeshQuery* mNavQuery;
	dtCrowd*		mCrowd;
public:
	int				mTilesWidth;
	int				mTilesHeight;

public:
	inline dtNavMesh* GetNavMesh() {
		return mNavMesh;
	}
public:
	ENGINE_RTTI(RcNavMesh);
	RcNavMesh(dtNavMesh* mesh)
	{
		mNavMesh = mesh;
	}
	RcNavMesh();
	~RcNavMesh();

	RcNavQuery* CreateQuery(int maxNodes);

	bool LoadXnd(XndNode* node);
	void Save2Xnd(XndNode* node);

	NxRHI::FMeshDataProvider*	CreateRenderMesh();
	int					GetTilesCount();
	bool				CheckVaildAt(int tileindex, int layer);
	v3dxVector3			GetPositionAt(int tileindex, int layer);
	v3dxVector3			GetBoundBoxMaxAt(int tileindex);
	v3dxVector3			GetBoundBoxMinAt(int tileindex);
};

NS_END