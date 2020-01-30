#include "TileMeshBuilder.h"
#include "InputGeom.h"

#define new VNEW

#ifdef WIN32
#	define snprintf _snprintf
#endif

NS_BEGIN

RTTI_IMPL(EngineNS::TileMeshBuilder, EngineNS::RecastBuilder);

inline unsigned int nextPow2(unsigned int v)
{
	v--;
	v |= v >> 1;
	v |= v >> 2;
	v |= v >> 4;
	v |= v >> 8;
	v |= v >> 16;
	v++;
	return v;
}

inline unsigned int ilog2(unsigned int v)
{
	unsigned int r;
	unsigned int shift;
	r = (v > 0xffff) << 4; v >>= r;
	shift = (v > 0xff) << 3; v >>= shift; r |= shift;
	shift = (v > 0xf) << 2; v >>= shift; r |= shift;
	shift = (v > 0x3) << 1; v >>= shift; r |= shift;
	r |= (v >> 1);
	return r;
}

TileMeshBuilder::TileMeshBuilder() :
	m_keepInterResults(false),
	m_buildAll(true),
	m_totalBuildTimeMs(0),
	m_triareas(0),
	m_solid(0),
	m_chf(0),
	m_cset(0),
	m_pmesh(0),
	m_dmesh(0),
	m_drawMode(DRAWMODE_NAVMESH),
	m_maxTiles(0),
	m_maxPolysPerTile(0),
	mTileSize(32),
	m_tileBuildTime(0),
	m_tileMemUsage(0),
	m_tileTriCount(0)
{
	resetCommonSettings();
	memset(m_lastBuiltTileBmin, 0, sizeof(m_lastBuiltTileBmin));
	memset(m_lastBuiltTileBmax, 0, sizeof(m_lastBuiltTileBmax));
}

TileMeshBuilder::~TileMeshBuilder()
{
	cleanup();
}

void TileMeshBuilder::cleanup()
{
	delete [] m_triareas;
	m_triareas = 0;
	rcFreeHeightField(m_solid);
	m_solid = 0;
	rcFreeCompactHeightfield(m_chf);
	m_chf = 0;
	rcFreeContourSet(m_cset);
	m_cset = 0;
	rcFreePolyMesh(m_pmesh);
	m_pmesh = 0;
	rcFreePolyMeshDetail(m_dmesh);
	m_dmesh = 0;
}
extern BuildContext GRCContext;
RcNavMesh* TileMeshBuilder::BuildNavi()
{	
	m_ctx = &GRCContext;
	
	auto m_navMesh = dtAllocNavMesh();
	if (!m_navMesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not allocate navmesh.");
		return false;
	}

	RcNavMesh* navmesh = new RcNavMesh(m_navMesh);
	{
		int gw = 0, gh = 0;
		const float* bmin = m_geom->getNavMeshBoundsMin();
		const float* bmax = m_geom->getNavMeshBoundsMax();
		if (IsEmptyBoundingBox() == false)
		{
			bmin = (const float*)&mMinBox;
			bmax = (const float*)&mMaxBox;
		}
		rcCalcGridSize(bmin, bmax, mCellSize, &gw, &gh);
		const int ts = (int)mTileSize;
		const int tw = (gw + ts - 1) / ts;
		const int th = (gh + ts - 1) / ts;
		navmesh->SetTilesWidth( tw );
		navmesh->SetTilesHeight( th );

		// Max tiles and max polys affect how the tile IDs are caculated.
		// There are 22 bits available for identifying a tile and a polygon.
		int tileBits = rcMin((int)ilog2(nextPow2(tw*th)), 14);
		if (tileBits > 14) 
			tileBits = 14;
		int polyBits = 22 - tileBits;
		m_maxTiles = 1 << tileBits;
		m_maxPolysPerTile = 1 << polyBits;
	}


	dtNavMeshParams params;
	rcVcopy(params.orig, m_geom->getNavMeshBoundsMin());
	params.tileWidth = mTileSize*mCellSize;
	params.tileHeight = mTileSize*mCellSize;
	params.maxTiles = m_maxTiles;
	params.maxPolys = m_maxPolysPerTile;
	
	dtStatus status;
	
	status = m_navMesh->init(&params);
	if (dtStatusFailed(status))
	{
		m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Could not init navmesh.");
		return false;
	}
		
	if (m_buildAll)
		buildAllTiles(m_navMesh);
	
	return navmesh;
}

void TileMeshBuilder::buildTile(const float* pos, dtNavMesh* m_navMesh)
{
	if (!m_geom) return;
	if (!m_navMesh) return;
		
	const float* bmin = m_geom->getNavMeshBoundsMin();
	const float* bmax = m_geom->getNavMeshBoundsMax();
	
	const float ts = mTileSize*mCellSize;
	const int tx = (int)((pos[0] - bmin[0]) / ts);
	const int ty = (int)((pos[2] - bmin[2]) / ts);
	
	m_lastBuiltTileBmin[0] = bmin[0] + tx*ts;
	m_lastBuiltTileBmin[1] = bmin[1];
	m_lastBuiltTileBmin[2] = bmin[2] + ty*ts;
	
	m_lastBuiltTileBmax[0] = bmin[0] + (tx+1)*ts;
	m_lastBuiltTileBmax[1] = bmax[1];
	m_lastBuiltTileBmax[2] = bmin[2] + (ty+1)*ts;
	
	m_ctx->resetLog();
	
	int dataSize = 0;
	unsigned char* data = buildTileMesh(tx, ty, m_lastBuiltTileBmin, m_lastBuiltTileBmax, dataSize);

	// Remove any previous data (navmesh owns and deletes the data).
	m_navMesh->removeTile(m_navMesh->getTileRefAt(tx,ty,0),0,0);

	// Add tile, or leave the location empty.
	if (data)
	{
		// Let the navmesh own the data.
		dtStatus status = m_navMesh->addTile(data,dataSize,DT_TILE_FREE_DATA,0,0);
		if (dtStatusFailed(status))
			dtFree(data);
	}
	
	m_ctx->dumpLog("Build Tile (%d,%d):", tx,ty);
}

void TileMeshBuilder::getTilePos(const float* pos, int& tx, int& ty)
{
	if (!m_geom) return;
	
	const float* bmin = m_geom->getNavMeshBoundsMin();
	
	const float ts = mTileSize*mCellSize;
	tx = (int)((pos[0] - bmin[0]) / ts);
	ty = (int)((pos[2] - bmin[2]) / ts);
}

v3dxVector3 TileMeshBuilder::CorrectPosition(v3dxVector3 pos)
{
	if (!m_geom) return pos;

	const float* bmin = m_geom->getNavMeshBoundsMin();

	v3dxVector3 cpos;
	const float ts = mTileSize * mCellSize;
	int tx = (int)((pos[0] - bmin[0]) / ts);
	int ty = (int)((pos[2] - bmin[2]) / ts);
	cpos.x = (float)tx * ts + bmin[0];
	cpos.z = (float)ty * ts + bmin[2];

	cpos.y = pos.y;
	return cpos;
}

void TileMeshBuilder::removeTile(const float* pos, dtNavMesh* m_navMesh)
{
	if (!m_geom) return;
	if (!m_navMesh) return;
	
	const float* bmin = m_geom->getNavMeshBoundsMin();
	const float* bmax = m_geom->getNavMeshBoundsMax();

	const float ts = mTileSize*mCellSize;
	const int tx = (int)((pos[0] - bmin[0]) / ts);
	const int ty = (int)((pos[2] - bmin[2]) / ts);
	
	m_lastBuiltTileBmin[0] = bmin[0] + tx*ts;
	m_lastBuiltTileBmin[1] = bmin[1];
	m_lastBuiltTileBmin[2] = bmin[2] + ty*ts;
	
	m_lastBuiltTileBmax[0] = bmin[0] + (tx+1)*ts;
	m_lastBuiltTileBmax[1] = bmax[1];
	m_lastBuiltTileBmax[2] = bmin[2] + (ty+1)*ts;
	
	m_navMesh->removeTile(m_navMesh->getTileRefAt(tx,ty,0),0,0);
}

void TileMeshBuilder::buildAllTiles(dtNavMesh* m_navMesh)
{
	if (!m_geom) return;
	if (!m_navMesh) return;
	
	const float* bmin = m_geom->getNavMeshBoundsMin();
	const float* bmax = m_geom->getNavMeshBoundsMax();
	int gw = 0, gh = 0;
	rcCalcGridSize(bmin, bmax, mCellSize, &gw, &gh);
	const int ts = (int)mTileSize;
	const int tw = (gw + ts-1) / ts;
	const int th = (gh + ts-1) / ts;
	const float tcs = mTileSize*mCellSize;

	
	// Start the build process.
	m_ctx->startTimer(RC_TIMER_TEMP);

	if (m_geom->WalkDatas.size() == 0)
	{
		//Calc All Grid TODO..
		for (int y = 0; y < th; ++y)
		{
			for (int x = 0; x < tw; ++x)
			{
				m_lastBuiltTileBmin[0] = bmin[0] + x * tcs;
				m_lastBuiltTileBmin[1] = bmin[1];
				m_lastBuiltTileBmin[2] = bmin[2] + y * tcs;

				m_lastBuiltTileBmax[0] = bmin[0] + (x + 1)*tcs;
				m_lastBuiltTileBmax[1] = bmax[1];
				m_lastBuiltTileBmax[2] = bmin[2] + (y + 1)*tcs;

				int dataSize = 0;
				unsigned char* data = buildTileMesh(x, y, m_lastBuiltTileBmin, m_lastBuiltTileBmax, dataSize);
				if (data)
				{
					// Remove any previous data (navmesh owns and deletes the data).
					m_navMesh->removeTile(m_navMesh->getTileRefAt(x, y, 0), 0, 0);
					// Let the navmesh own the data.
					dtStatus status = m_navMesh->addTile(data, dataSize, DT_TILE_FREE_DATA, 0, 0);
					if (dtStatusFailed(status))
						dtFree(data);
				}
			}
		}
	}
	else
	{
		float data[3];
		for (int i = 0; i < m_geom->WalkDatas.size(); i += 2)
		{
			data[0] = m_geom->WalkDatas[i].x;
			data[1] = m_geom->WalkDatas[i].y;
			data[2] = m_geom->WalkDatas[i].z;

			rcCalcGridSize(bmin, data, mCellSize, &gw, &gh);
			int minx = (gw + ts - 1) / ts;
			int miny = (gh + ts - 1) / ts;

			data[0] = m_geom->WalkDatas[i + 1].x;
			data[1] = m_geom->WalkDatas[i + 1].y;
			data[2] = m_geom->WalkDatas[i + 1].z;

			rcCalcGridSize(bmin, data, mCellSize, &gw, &gh);
			int maxx = (gw + ts - 1) / ts;
			int maxy = (gh + ts - 1) / ts;

			for (int y = miny; y < maxy; ++y)
			{
				for (int x = minx; x < maxx; ++x)
				{
					m_lastBuiltTileBmin[0] = bmin[0] + x * tcs;
					m_lastBuiltTileBmin[1] = bmin[1];
					m_lastBuiltTileBmin[2] = bmin[2] + y * tcs;

					m_lastBuiltTileBmax[0] = bmin[0] + (x + 1)*tcs;
					m_lastBuiltTileBmax[1] = bmax[1];
					m_lastBuiltTileBmax[2] = bmin[2] + (y + 1)*tcs;

					int dataSize = 0;
					unsigned char* data = buildTileMesh(x, y, m_lastBuiltTileBmin, m_lastBuiltTileBmax, dataSize);
					if (data)
					{
						// Remove any previous data (navmesh owns and deletes the data).
						m_navMesh->removeTile(m_navMesh->getTileRefAt(x, y, 0), 0, 0);
						// Let the navmesh own the data.
						dtStatus status = m_navMesh->addTile(data, dataSize, DT_TILE_FREE_DATA, 0, 0);
						if (dtStatusFailed(status))
							dtFree(data);
					}
				}
			}
		}
	}
	
	
	// Start the build process.	
	m_ctx->stopTimer(RC_TIMER_TEMP);

	m_totalBuildTimeMs = m_ctx->getAccumulatedTime(RC_TIMER_TEMP)/1000.0f;
	
}

void TileMeshBuilder::removeAllTiles(dtNavMesh* m_navMesh)
{
	if (!m_geom || !m_navMesh)
		return;

	const float* bmin = m_geom->getNavMeshBoundsMin();
	const float* bmax = m_geom->getNavMeshBoundsMax();
	int gw = 0, gh = 0;
	rcCalcGridSize(bmin, bmax, mCellSize, &gw, &gh);
	const int ts = (int)mTileSize;
	const int tw = (gw + ts-1) / ts;
	const int th = (gh + ts-1) / ts;
	
	for (int y = 0; y < th; ++y)
		for (int x = 0; x < tw; ++x)
			m_navMesh->removeTile(m_navMesh->getTileRefAt(x,y,0),0,0);
}


unsigned char* TileMeshBuilder::buildTileMesh(const int tx, const int ty, const float* bmin, const float* bmax, int& dataSize)
{
	if (!m_geom || !m_geom->getMesh() || !m_geom->getChunkyMesh())
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Input mesh is not specified.");
		return 0;
	}
	
	m_tileMemUsage = 0;
	m_tileBuildTime = 0;
	
	cleanup();
	
	const float* verts = m_geom->getMesh()->getVerts();
	const int nverts = m_geom->getMesh()->getVertCount();
	const int ntris = m_geom->getMesh()->getTriCount();
	const rcChunkyTriMesh* chunkyMesh = m_geom->getChunkyMesh();
		
	// Init build configuration from GUI
	memset(&m_cfg, 0, sizeof(m_cfg));
	m_cfg.cs = mCellSize;
	m_cfg.ch = mCellHeight;
	m_cfg.walkableSlopeAngle = mAgentMaxSlope;
	m_cfg.walkableHeight = (int)ceilf(mAgentHeight / m_cfg.ch);
	m_cfg.walkableClimb = (int)floorf(mAgentMaxClimb / m_cfg.ch);
	m_cfg.walkableRadius = (int)ceilf(mAgentRadius / m_cfg.cs);
	m_cfg.maxEdgeLen = (int)(mEdgeMaxLen / mCellSize);
	m_cfg.maxSimplificationError = mEdgeMaxError;
	m_cfg.minRegionArea = (int)rcSqr(mRegionMinSize);		// Note: area = size*size
	m_cfg.mergeRegionArea = (int)rcSqr(mRegionMergeSize);	// Note: area = size*size
	m_cfg.maxVertsPerPoly = (int)mVertsPerPoly;
	m_cfg.tileSize = (int)mTileSize;
	m_cfg.borderSize = m_cfg.walkableRadius + 3; // Reserve enough padding.
	m_cfg.width = m_cfg.tileSize + m_cfg.borderSize*2;
	m_cfg.height = m_cfg.tileSize + m_cfg.borderSize*2;
	m_cfg.detailSampleDist = mDetailSampleDist < 0.9f ? 0 : mCellSize * mDetailSampleDist;
	m_cfg.detailSampleMaxError = mCellHeight * mDetailSampleMaxError;
	
	// Expand the heighfield bounding box by border size to find the extents of geometry we need to build this tile.
	//
	// This is done in order to make sure that the navmesh tiles connect correctly at the borders,
	// and the obstacles close to the border work correctly with the dilation process.
	// No polygons (or contours) will be created on the border area.
	//
	// IMPORTANT!
	//
	//   :''''''''':
	//   : +-----+ :
	//   : |     | :
	//   : |     |<--- tile to build
	//   : |     | :  
	//   : +-----+ :<-- geometry needed
	//   :.........:
	//
	// You should use this bounding box to query your input geometry.
	//
	// For example if you build a navmesh for terrain, and want the navmesh tiles to match the terrain tile size
	// you will need to pass in data from neighbour terrain tiles too! In a simple case, just pass in all the 8 neighbours,
	// or use the bounding box below to only pass in a sliver of each of the 8 neighbours.
	rcVcopy(m_cfg.bmin, bmin);
	rcVcopy(m_cfg.bmax, bmax);
	m_cfg.bmin[0] -= m_cfg.borderSize*m_cfg.cs;
	m_cfg.bmin[2] -= m_cfg.borderSize*m_cfg.cs;
	m_cfg.bmax[0] += m_cfg.borderSize*m_cfg.cs;
	m_cfg.bmax[2] += m_cfg.borderSize*m_cfg.cs;
	
	// Reset build times gathering.
	m_ctx->resetTimers();
	
	// Start the build process.
	m_ctx->startTimer(RC_TIMER_TOTAL);
	
	m_ctx->log(RC_LOG_PROGRESS, "Building navigation:");
	m_ctx->log(RC_LOG_PROGRESS, " - %d x %d cells", m_cfg.width, m_cfg.height);
	m_ctx->log(RC_LOG_PROGRESS, " - %.1fK verts, %.1fK tris", nverts/1000.0f, ntris/1000.0f);
	
	// Allocate voxel heightfield where we rasterize our input data to.
	m_solid = rcAllocHeightfield();
	if (!m_solid)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'solid'.");
		return 0;
	}
	if (!rcCreateHeightfield(m_ctx, *m_solid, m_cfg.width, m_cfg.height, m_cfg.bmin, m_cfg.bmax, m_cfg.cs, m_cfg.ch))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create solid heightfield.");
		return 0;
	}
	
	// Allocate array that can hold triangle flags.
	// If you have multiple meshes you need to process, allocate
	// and array which can hold the max number of triangles you need to process.
	m_triareas = new unsigned char[chunkyMesh->maxTrisPerChunk];
	if (!m_triareas)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'm_triareas' (%d).", chunkyMesh->maxTrisPerChunk);
		return 0;
	}
	
	float tbmin[2], tbmax[2];
	tbmin[0] = m_cfg.bmin[0];
	tbmin[1] = m_cfg.bmin[2];
	tbmax[0] = m_cfg.bmax[0];
	tbmax[1] = m_cfg.bmax[2];
	int cid[512];// TODO: Make grow when returning too many items.
	const int ncid = rcGetChunksOverlappingRect(chunkyMesh, tbmin, tbmax, cid, 512);
	if (!ncid)
		return 0;
	
	m_tileTriCount = 0;
	
	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = chunkyMesh->nodes[cid[i]];
		const int* ctris = &chunkyMesh->tris[node.i*3];
		const int nctris = node.n;
		
		m_tileTriCount += nctris;
		
		memset(m_triareas, 0, nctris*sizeof(unsigned char));
		rcMarkWalkableTriangles(m_ctx, m_cfg.walkableSlopeAngle,
								verts, nverts, ctris, nctris, m_triareas);
		
		if (!rcRasterizeTriangles(m_ctx, verts, nverts, ctris, m_triareas, nctris, *m_solid, m_cfg.walkableClimb))
			return 0;
	}
	
	if (!m_keepInterResults)
	{
		delete [] m_triareas;
		m_triareas = 0;
	}
	
	// Once all geometry is rasterized, we do initial pass of filtering to
	// remove unwanted overhangs caused by the conservative rasterization
	// as well as filter spans where the character cannot possibly stand.
	if (mFilterLowHangingObstacles)
		rcFilterLowHangingWalkableObstacles(m_ctx, m_cfg.walkableClimb, *m_solid);
	if (mFilterLedgeSpans)
		rcFilterLedgeSpans(m_ctx, m_cfg.walkableHeight, m_cfg.walkableClimb, *m_solid);
	if (mFilterWalkableLowHeightSpans)
		rcFilterWalkableLowHeightSpans(m_ctx, m_cfg.walkableHeight, *m_solid);
	
	// Compact the heightfield so that it is faster to handle from now on.
	// This will result more cache coherent data as well as the neighbours
	// between walkable cells will be calculated.
	m_chf = rcAllocCompactHeightfield();
	if (!m_chf)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'chf'.");
		return 0;
	}
	if (!rcBuildCompactHeightfield(m_ctx, m_cfg.walkableHeight, m_cfg.walkableClimb, *m_solid, *m_chf))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build compact data.");
		return 0;
	}
	
	if (!m_keepInterResults)
	{
		rcFreeHeightField(m_solid);
		m_solid = 0;
	}

	// Erode the walkable area by agent radius.
	if (!rcErodeWalkableArea(m_ctx, m_cfg.walkableRadius, *m_chf))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not erode.");
		return 0;
	}

	// (Optional) Mark areas.
	const ConvexVolume* vols = m_geom->getConvexVolumes();
	for (int i  = 0; i < m_geom->getConvexVolumeCount(); ++i)
		rcMarkConvexPolyArea(m_ctx, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned char)vols[i].area, *m_chf);
	
	//TODO..
	if (m_geom->getConvexVolumeCount() > 0)
	{
		for (int i = 0; i < m_chf->spanCount; i++)
		{
			if (m_chf->areas[i] == RC_WALKABLE_AREA)
				m_chf->areas[i] = RC_NULL_AREA;
			else if (m_chf->areas[i] == 64)
				m_chf->areas[i] = RC_WALKABLE_AREA;
		}
	}
	// Partition the heightfield so that we can use simple algorithm later to triangulate the walkable areas.
	// There are 3 martitioning methods, each with some pros and cons:
	// 1) Watershed partitioning
	//   - the classic Recast partitioning
	//   - creates the nicest tessellation
	//   - usually slowest
	//   - partitions the heightfield into nice regions without holes or overlaps
	//   - the are some corner cases where this method creates produces holes and overlaps
	//      - holes may appear when a small obstacles is close to large open area (triangulation can handle this)
	//      - overlaps may occur if you have narrow spiral corridors (i.e stairs), this make triangulation to fail
	//   * generally the best choice if you precompute the nacmesh, use this if you have large open areas
	// 2) Monotone partioning
	//   - fastest
	//   - partitions the heightfield into regions without holes and overlaps (guaranteed)
	//   - creates long thin polygons, which sometimes causes paths with detours
	//   * use this if you want fast navmesh generation
	// 3) Layer partitoining
	//   - quite fast
	//   - partitions the heighfield into non-overlapping regions
	//   - relies on the triangulation code to cope with holes (thus slower than monotone partitioning)
	//   - produces better triangles than monotone partitioning
	//   - does not have the corner cases of watershed partitioning
	//   - can be slow and create a bit ugly tessellation (still better than monotone)
	//     if you have large open areas with small obstacles (not a problem if you use tiles)
	//   * good choice to use for tiled navmesh with medium and small sized tiles
	
	if (mPartitionType == SAMPLE_PARTITION_WATERSHED)
	{
		// Prepare for region partitioning, by calculating distance field along the walkable surface.
		if (!rcBuildDistanceField(m_ctx, *m_chf))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build distance field.");
			return 0;
		}
		
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildRegions(m_ctx, *m_chf, m_cfg.borderSize, m_cfg.minRegionArea, m_cfg.mergeRegionArea))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build watershed regions.");
			return 0;
		}
	}
	else if (mPartitionType == SAMPLE_PARTITION_MONOTONE)
	{
		// Partition the walkable surface into simple regions without holes.
		// Monotone partitioning does not need distancefield.
		if (!rcBuildRegionsMonotone(m_ctx, *m_chf, m_cfg.borderSize, m_cfg.minRegionArea, m_cfg.mergeRegionArea))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build monotone regions.");
			return 0;
		}
	}
	else // SAMPLE_PARTITION_LAYERS
	{
		// Partition the walkable surface into simple regions without holes.
		if (!rcBuildLayerRegions(m_ctx, *m_chf, m_cfg.borderSize, m_cfg.minRegionArea))
		{
			m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not build layer regions.");
			return 0;
		}
	}
	 	
	// Create contours.
	m_cset = rcAllocContourSet();
	if (!m_cset)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'cset'.");
		return 0;
	}
	if (!rcBuildContours(m_ctx, *m_chf, m_cfg.maxSimplificationError, m_cfg.maxEdgeLen, *m_cset))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not create contours.");
		return 0;
	}
	
	if (m_cset->nconts == 0)
	{
		return 0;
	}
	
	// Build polygon navmesh from the contours.
	m_pmesh = rcAllocPolyMesh();
	if (!m_pmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'pmesh'.");
		return 0;
	}
	if (!rcBuildPolyMesh(m_ctx, *m_cset, m_cfg.maxVertsPerPoly, *m_pmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could not triangulate contours.");
		return 0;
	}
	
	// Build detail mesh.
	m_dmesh = rcAllocPolyMeshDetail();
	if (!m_dmesh)
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Out of memory 'dmesh'.");
		return 0;
	}
	
	if (!rcBuildPolyMeshDetail(m_ctx, *m_pmesh, *m_chf,
							   m_cfg.detailSampleDist, m_cfg.detailSampleMaxError,
							   *m_dmesh))
	{
		m_ctx->log(RC_LOG_ERROR, "buildNavigation: Could build polymesh detail.");
		return 0;
	}
	
	if (!m_keepInterResults)
	{
		rcFreeCompactHeightfield(m_chf);
		m_chf = 0;
		rcFreeContourSet(m_cset);
		m_cset = 0;
	}
	
	unsigned char* navData = 0;
	int navDataSize = 0;
	if (m_cfg.maxVertsPerPoly <= DT_VERTS_PER_POLYGON)
	{
		if (m_pmesh->nverts >= 0xffff)
		{
			// The vertex indices are ushorts, and cannot point to more than 0xffff vertices.
			m_ctx->log(RC_LOG_ERROR, "Too many vertices per tile %d (max: %d).", m_pmesh->nverts, 0xffff);
			return 0;
		}
		
		//// Update poly flags from areas.
		//for (int i = 0; i < m_pmesh->npolys; ++i)
		//{
		//	if (m_pmesh->areas[i] == RC_WALKABLE_AREA)
		//		m_pmesh->areas[i] = SAMPLE_POLYAREA_GROUND;
		//	
		//	if (m_pmesh->areas[i] == SAMPLE_POLYAREA_GROUND)
		//	{
		//		m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK;
		//	}
		//	else
		//	{
		//		m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK | (int)::pow(2, m_pmesh->areas[i]);
		//	}
		//}

		// Update poly flags from areas.
		for (int i = 0; i < m_pmesh->npolys; ++i)
		{
			if (m_pmesh->areas[i] == RC_WALKABLE_AREA)
				m_pmesh->areas[i] = SAMPLE_POLYAREA_GROUND;

			if (m_pmesh->areas[i] == SAMPLE_POLYAREA_GROUND ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_GRASS ||
				m_pmesh->areas[i] == SAMPLE_POLYAREA_ROAD)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_WATER)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_SWIM;
			}
			else if (m_pmesh->areas[i] == SAMPLE_POLYAREA_DOOR)
			{
				m_pmesh->flags[i] = SAMPLE_POLYFLAGS_WALK | SAMPLE_POLYFLAGS_DOOR;
			}
		}
		
		dtNavMeshCreateParams params;
		memset(&params, 0, sizeof(params));
		params.verts = m_pmesh->verts;
		params.vertCount = m_pmesh->nverts;
		params.polys = m_pmesh->polys;
		params.polyAreas = m_pmesh->areas;
		params.polyFlags = m_pmesh->flags;
		params.polyCount = m_pmesh->npolys;
		params.nvp = m_pmesh->nvp;
		params.detailMeshes = m_dmesh->meshes;
		params.detailVerts = m_dmesh->verts;
		params.detailVertsCount = m_dmesh->nverts;
		params.detailTris = m_dmesh->tris;
		params.detailTriCount = m_dmesh->ntris;
		params.offMeshConVerts = m_geom->getOffMeshConnectionVerts();
		params.offMeshConRad = m_geom->getOffMeshConnectionRads();
		params.offMeshConDir = m_geom->getOffMeshConnectionDirs();
		params.offMeshConAreas = m_geom->getOffMeshConnectionAreas();
		params.offMeshConFlags = m_geom->getOffMeshConnectionFlags();
		params.offMeshConUserID = m_geom->getOffMeshConnectionId();
		params.offMeshConCount = m_geom->getOffMeshConnectionCount();
		params.walkableHeight = mAgentHeight;
		params.walkableRadius = mAgentRadius;
		params.walkableClimb = mAgentMaxClimb;
		params.tileX = tx;
		params.tileY = ty;
		params.tileLayer = 0;
		rcVcopy(params.bmin, m_pmesh->bmin);
		rcVcopy(params.bmax, m_pmesh->bmax);
		params.cs = m_cfg.cs;
		params.ch = m_cfg.ch;
		params.buildBvTree = true;
		
		if (!dtCreateNavMeshData(&params, &navData, &navDataSize))
		{
			m_ctx->log(RC_LOG_ERROR, "Could not build Detour navmesh.");
			return 0;
		}		
	}
	m_tileMemUsage = navDataSize/1024.0f;
	
	m_ctx->stopTimer(RC_TIMER_TOTAL);
	
	// Show performance stats.
	m_ctx->log(RC_LOG_PROGRESS, ">> Polymesh: %d vertices  %d polygons", m_pmesh->nverts, m_pmesh->npolys);
	
	m_tileBuildTime = m_ctx->getAccumulatedTime(RC_TIMER_TOTAL)/1000.0f;

	dataSize = navDataSize;
	return navData;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, TileMeshBuilder, SetTileSize, float);
	CSharpReturnAPI0(float, EngineNS, TileMeshBuilder, GetTileSize);
	CSharpReturnAPI1(v3dVector3_t, EngineNS, TileMeshBuilder, CorrectPosition, v3dxVector3);
}