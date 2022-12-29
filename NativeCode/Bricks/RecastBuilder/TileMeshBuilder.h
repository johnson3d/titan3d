#pragma once

#include "RecastBuilder.h"

NS_BEGIN

class TR_CLASS()
	TileMeshBuilder : public RecastBuilder
{
public:
	ENGINE_RTTI(TileMeshBuilder);
protected:
	bool m_keepInterResults;
	bool m_buildAll;
	float m_totalBuildTimeMs;

	unsigned char* m_triareas;
	rcHeightfield* m_solid;
	rcCompactHeightfield* m_chf;
	rcContourSet* m_cset;
	rcPolyMesh* m_pmesh;
	rcPolyMeshDetail* m_dmesh;
	rcConfig m_cfg;	
	
	enum DrawMode
	{
		DRAWMODE_NAVMESH,
		DRAWMODE_NAVMESH_TRANS,
		DRAWMODE_NAVMESH_BVTREE,
		DRAWMODE_NAVMESH_NODES,
		DRAWMODE_NAVMESH_PORTALS,
		DRAWMODE_NAVMESH_INVIS,
		DRAWMODE_MESH,
		DRAWMODE_VOXELS,
		DRAWMODE_VOXELS_WALKABLE,
		DRAWMODE_COMPACT,
		DRAWMODE_COMPACT_DISTANCE,
		DRAWMODE_COMPACT_REGIONS,
		DRAWMODE_REGION_CONNECTIONS,
		DRAWMODE_RAW_CONTOURS,
		DRAWMODE_BOTH_CONTOURS,
		DRAWMODE_CONTOURS,
		DRAWMODE_POLYMESH,
		DRAWMODE_POLYMESH_DETAIL,		
		MAX_DRAWMODE
	};
		
	DrawMode m_drawMode;
	
	int m_maxTiles;
	int m_maxPolysPerTile;
	float mTileSize;
	
	float m_lastBuiltTileBmin[3];
	float m_lastBuiltTileBmax[3];
	float m_tileBuildTime;
	float m_tileMemUsage;
	int m_tileTriCount;

	unsigned char* buildTileMesh(const int tx, const int ty, const float* bmin, const float* bmax, int& dataSize);
	
	void cleanup();
public:
	TileMeshBuilder();
	virtual ~TileMeshBuilder();
	
	void getTilePos(const float* pos, int& tx, int& ty);
	v3dxVector3 CorrectPosition(v3dxVector3 pos);
	
	void buildTile(const float* pos, dtNavMesh* mesh);
	void removeTile(const float* pos, dtNavMesh* mesh);
	void buildAllTiles(dtNavMesh* mesh);
	void removeAllTiles(dtNavMesh* mesh);

	virtual RcNavMesh* BuildNavi() override;
};

NS_END