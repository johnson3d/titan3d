#pragma once

#include "Recast.h"
#include "DetourNavMeshQuery.h"
#include "DetourCrowd.h"

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
class GfxMeshPrimitives;

class RcNavMesh : public VIUnknown
{
protected:
	dtNavMesh*		mNavMesh;
	dtNavMeshQuery* mNavQuery;
	dtCrowd*		mCrowd;

	int				mTilesWidth;
	int				mTilesHeight;

public:
	inline dtNavMesh* GetNavMesh() {return mNavMesh;}

public:
	RTTI_DEF(RcNavMesh, 0xe5912afa5c2c6f85, true);
	RcNavMesh(dtNavMesh* mesh)
	{
		mNavMesh = mesh;
	}
	RcNavMesh();
	~RcNavMesh();

	VDef_ReadWrite( int, TilesWidth, m );
	VDef_ReadWrite( int, TilesHeight, m );

	RcNavQuery* CreateQuery(int maxNodes);

	vBOOL LoadXnd(XNDNode* node);
	void Save2Xnd(XNDNode* node);

	GfxMeshPrimitives*	CreateRenderMesh(IRenderContext* rc);
	int					GetTilesCount();
	vBOOL				CheckVaildAt(int tileindex, int layer);
	v3dxVector3			GetPositionAt(int tileindex, int layer);
	v3dxVector3			GetBoundBoxMaxAt(int tileindex);
	v3dxVector3			GetBoundBoxMinAt(int tileindex);
};

NS_END