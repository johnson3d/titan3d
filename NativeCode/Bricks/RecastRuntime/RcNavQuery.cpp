#include "../../NextRHI/NxGeomMesh.h"
#include "RcNavQuery.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::RcNavQuery);

static const float STEP_SIZE = 0.5f;
static const float SLOP = 0.01f;

template<class T> inline T dtMin(T a, T b) { return a < b ? a : b; }

inline void dtVcopy(float* dest, const float* a)
{
	dest[0] = a[0];
	dest[1] = a[1];
	dest[2] = a[2];
}

inline bool inRange(const float* v1, const float* v2, const float r, const float h)
{
	const float dx = v2[0] - v1[0];
	const float dy = v2[1] - v1[1];
	const float dz = v2[2] - v1[2];
	return (dx*dx + dz * dz) < r*r && fabsf(dy) < h;
}

inline void dtVsub(float* dest, const float* v1, const float* v2)
{
	dest[0] = v1[0] - v2[0];
	dest[1] = v1[1] - v2[1];
	dest[2] = v1[2] - v2[2];
}

inline float dtVdot(const float* v1, const float* v2)
{
	return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
}

inline float dtMathSqrtf(float x)
{
	return sqrtf(x);
}

inline void dtVmad(float* dest, const float* v1, const float* v2, const float s)
{
	dest[0] = v1[0] + v2[0] * s;
	dest[1] = v1[1] + v2[1] * s;
	dest[2] = v1[2] + v2[2] * s;
}

static int fixupCorridor(dtPolyRef* path, const int npath, const int maxPath,
	const dtPolyRef* visited, const int nvisited)
{
	int furthestPath = -1;
	int furthestVisited = -1;

	// Find furthest common polygon.
	for (int i = npath - 1; i >= 0; --i)
	{
		bool found = false;
		for (int j = nvisited - 1; j >= 0; --j)
		{
			if (path[i] == visited[j])
			{
				furthestPath = i;
				furthestVisited = j;
				found = true;
			}
		}
		if (found)
			break;
	}

	// If no intersection found just return current path. 
	if (furthestPath == -1 || furthestVisited == -1)
		return npath;

	// Concatenate paths.	

	// Adjust beginning of the buffer to include the visited.
	const int req = nvisited - furthestVisited;
	const int orig = rcMin(furthestPath + 1, npath);
	int size = rcMax(0, npath - orig);
	if (req + size > maxPath)
		size = maxPath - req;
	if (size)
		memmove(path + req, path + orig, size * sizeof(dtPolyRef));

	// Store visited
	for (int i = 0; i < req; ++i)
		path[i] = visited[(nvisited - 1) - i];

	return req + size;
}


// This function checks if the path has a small U-turn, that is,
// a polygon further in the path is adjacent to the first polygon
// in the path. If that happens, a shortcut is taken.
// This can happen if the target (T) location is at tile boundary,
// and we're (S) approaching it parallel to the tile edge.
// The choice at the vertex can be arbitrary, 
//  +---+---+
//  |:::|:::|
//  +-S-+-T-+
//  |:::|   | <-- the step can end up in here, resulting U-turn path.
//  +---+---+
static int fixupShortcuts(dtPolyRef* path, int npath, dtNavMeshQuery* navQuery)
{
	if (npath < 3)
		return npath;

	// Get connected polygons
	static const int maxNeis = 16;
	dtPolyRef neis[maxNeis];
	int nneis = 0;

	const dtMeshTile* tile = 0;
	const dtPoly* poly = 0;
	if (dtStatusFailed(navQuery->getAttachedNavMesh()->getTileAndPolyByRef(path[0], &tile, &poly)))
		return npath;

	for (unsigned int k = poly->firstLink; k != DT_NULL_LINK; k = tile->links[k].next)
	{
		const dtLink* link = &tile->links[k];
		if (link->ref != 0)
		{
			if (nneis < maxNeis)
				neis[nneis++] = link->ref;
		}
	}

	// If any of the neighbour polygons is within the next few polygons
	// in the path, short cut to that polygon directly.
	static const int maxLookAhead = 6;
	int cut = 0;
	for (int i = dtMin(maxLookAhead, npath) - 1; i > 1 && cut == 0; i--) {
		for (int j = 0; j < nneis; j++)
		{
			if (path[i] == neis[j]) {
				cut = i;
				break;
			}
		}
	}
	if (cut > 1)
	{
		int offset = cut - 1;
		npath -= offset;
		for (int i = 1; i < npath; i++)
			path[i] = path[i + offset];
	}

	return npath;
}

static bool getSteerTarget(dtNavMeshQuery* navQuery, const float* startPos, const float* endPos,
	const float minTargetDist,
	const dtPolyRef* path, const int pathSize,
	float* steerPos, unsigned char& steerPosFlag, dtPolyRef& steerPosRef,
	float* outPoints = 0, int* outPointCount = 0)
{
	// Find steer target.
	static const int MAX_STEER_POINTS = 3;
	float steerPath[MAX_STEER_POINTS * 3];
	unsigned char steerPathFlags[MAX_STEER_POINTS];
	dtPolyRef steerPathPolys[MAX_STEER_POINTS];
	int nsteerPath = 0;
	navQuery->findStraightPath(startPos, endPos, path, pathSize,
		steerPath, steerPathFlags, steerPathPolys, &nsteerPath, MAX_STEER_POINTS);
	if (!nsteerPath)
		return false;

	if (outPoints && outPointCount)
	{
		*outPointCount = nsteerPath;
		for (int i = 0; i < nsteerPath; ++i)
			dtVcopy(&outPoints[i * 3], &steerPath[i * 3]);
	}


	// Find vertex far enough to steer to.
	int ns = 0;
	while (ns < nsteerPath)
	{
		// Stop at Off-Mesh link or when point is further than slop away.
		if ((steerPathFlags[ns] & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ||
			!inRange(&steerPath[ns * 3], startPos, minTargetDist, 1000.0f))
			break;
		ns++;
	}
	// Failed to find good point to steer to.
	if (ns >= nsteerPath)
		return false;

	dtVcopy(steerPos, &steerPath[ns * 3]);
	steerPos[1] = startPos[1];
	steerPosFlag = steerPathFlags[ns];
	steerPosRef = steerPathPolys[ns];

	return true;
}

RcNavQuery::RcNavQuery()
{
	mNavQuery = nullptr;
	mCrowd = nullptr;

	mNSmoothPath = 0;

	mStartRef = 0;
	mEndRef = 0;

	mNPolys = 0;
	mTempNPolys = 0;
	mNStraightPath = 0;
	mStraightPathOptions = 0;

	mIterPos[0] = 0.0f;
	mIterPos[1] = 0.0f;
	mIterPos[2] = 0.0f;

	mTargetPos[0] = 0.0f;
	mTargetPos[1] = 0.0f;
	mTargetPos[2] = 0.0f;

	mPolyPickExt[0] = 2;
	mPolyPickExt[1] = 4;
	mPolyPickExt[2] = 2;

	mFilter.setIncludeFlags(SAMPLE_POLYFLAGS_ALL ^ SAMPLE_POLYFLAGS_DISABLED);
	mFilter.setExcludeFlags(0);
}

RcNavQuery::RcNavQuery(dtNavMeshQuery* query)
{
	mNavQuery = query;
	mCrowd = nullptr;

	mNSmoothPath = 0;

	mStartRef = 0;
	mEndRef = 0;

	mNPolys = 0;
	mTempNPolys = 0;
	mNStraightPath = 0;
	mStraightPathOptions = 0;

	mIterPos[0] = 0.0f;
	mIterPos[1] = 0.0f;
	mIterPos[2] = 0.0f;

	mTargetPos[0] = 0.0f;
	mTargetPos[1] = 0.0f;
	mTargetPos[2] = 0.0f;

	mPolyPickExt[0] = 2;
	mPolyPickExt[1] = 4;
	mPolyPickExt[2] = 2;

	mFilter.setIncludeFlags(SAMPLE_POLYFLAGS_ALL ^ SAMPLE_POLYFLAGS_DISABLED);
	mFilter.setExcludeFlags(0);
}

RcNavQuery::~RcNavQuery()
{
	if (mNavQuery != nullptr)
	{
		dtFreeNavMeshQuery(mNavQuery);
		mNavQuery = nullptr;
	}
}

bool RcNavQuery::GetHeight(const v3dxVector3* pos, const v3dxVector3* pickext, float* h)
{
	float fpos[3] = { pos->X, pos->Y, pos->Z };
	dtQueryFilter filter;
	dtPolyRef polyref;
	float fpickext[3] = {pickext->X, pickext->Y, pickext->Z};
	mNavQuery->findNearestPoly(fpos, fpickext, &filter, &polyref, 0);
	if (polyref == 0)
	{
		VFX_LTRACE(ELTT_Warning, "RcNavQuery GetHeight polyref is 0");
		return false;
	}

	mNavQuery->getPolyHeight(polyref, fpos, h);
	return true;
}

bool RcNavQuery::FindStraightPath(const v3dxVector3* start, const v3dxVector3* end, IBlobObject* blob)
{
	float startpos[3] = { start->X, start->Y, start->Z };
	mNavQuery->findNearestPoly(startpos, mPolyPickExt, &mFilter, &mStartRef, 0);
	if (mStartRef == 0)
	{
		VFX_LTRACE(ELTT_Warning, "FindPath start polyref is 0");
		return false;
	}
	
	float endpos[3] = { end->X, end->Y, end->Z };
	mNavQuery->findNearestPoly(endpos, mPolyPickExt, &mFilter, &mEndRef, 0);
	if (mEndRef == 0)
	{
		VFX_LTRACE(ELTT_Warning, "FindPath end polyref is 0");
		return false;
	}
	
	mNavQuery->findPath(mStartRef, mEndRef, startpos, endpos, &mFilter, mPolys, &mNPolys, MAX_POLYS);
	//ClosestPointOnPoly(start, end);

	AutoRecalcStraightPaths(start, end);

	blob->PushData((BYTE*)&mStraightPath[0], mNStraightPath * sizeof(float) * 3);
	
	return mNPolys != 0;
}

void RcNavQuery::ClosestPointOnPoly(const v3dxVector3* start, const v3dxVector3* end)
{
	memcpy(mTempPolys, mPolys, sizeof(dtPolyRef)*mNPolys);
	mTempNPolys = mNPolys;
	float startpos[3] = { start->X, start->Y, start->Z };
	mNavQuery->closestPointOnPoly(mStartRef, startpos, mIterPos, 0);
	float endpos[3] = { end->X, end->Y, end->Z };
	mNavQuery->closestPointOnPoly(mTempPolys[mTempNPolys - 1], endpos, mTargetPos, 0);

	mNSmoothPath = 0;

	dtVcopy(&mSmoothPath[mNSmoothPath * 3], mIterPos);
	//mNSmoothPath++;
}

void RcNavQuery::AutoRecalcFollowPaths(const dtNavMesh* navmesh)
{
	mNSmoothPath++;
	while (mTempNPolys && mNSmoothPath < MAX_SMOOTH)
	{
		// Find location to steer towards.
		float steerPos[3];
		unsigned char steerPosFlag;
		dtPolyRef steerPosRef;

		if (!getSteerTarget(mNavQuery, mIterPos, mTargetPos, SLOP,
			mPolys, mTempNPolys, steerPos, steerPosFlag, steerPosRef))
			break;

		bool endOfPath = (steerPosFlag & DT_STRAIGHTPATH_END) ? true : false;
		bool offMeshConnection = (steerPosFlag & DT_STRAIGHTPATH_OFFMESH_CONNECTION) ? true : false;

		// Find movement delta.
		float delta[3];
		dtVsub(delta, steerPos, mIterPos);
		float len = dtMathSqrtf(dtVdot(delta, delta));
		// If the steer target is end of path or off-mesh link, do not move past the location.
		if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
			len = 1;
		else
			len = STEP_SIZE / len;
		float moveTgt[3];
		dtVmad(moveTgt, mIterPos, delta, len);

		// Move
		float result[3];
		dtPolyRef visited[16];
		int nvisited = 0;
		mNavQuery->moveAlongSurface(mPolys[0], mIterPos, moveTgt, &mFilter,
			result, visited, &nvisited, 16);

		mTempNPolys = fixupCorridor(mPolys, mTempNPolys, MAX_POLYS, visited, nvisited);
		mTempNPolys = fixupShortcuts(mPolys, mTempNPolys, mNavQuery);

		float h = 0;
		mNavQuery->getPolyHeight(mPolys[0], result, &h);
		result[1] = h;
		dtVcopy(mIterPos, result);

		// Handle end of path and off-mesh links when close enough.
		if (endOfPath && inRange(mIterPos, steerPos, SLOP, 1.0f))
		{
			// Reached end of path.
			dtVcopy(mIterPos, mTargetPos);
			if (mNSmoothPath < MAX_SMOOTH)
			{
				dtVcopy(&mSmoothPath[mNSmoothPath * 3], mIterPos);
				mNSmoothPath++;
			}
			break;
		}
		else if (offMeshConnection && inRange(mIterPos, steerPos, SLOP, 1.0f))
		{
			// Reached off-mesh connection.
			float startPos[3], endPos[3];

			// Advance the path up to and over the off-mesh connection.
			dtPolyRef prevRef = 0, polyRef = mPolys[0];
			int npos = 0;
			while (npos < mTempNPolys && polyRef != steerPosRef)
			{
				prevRef = polyRef;
				polyRef = mPolys[npos];
				npos++;
			}
			for (int i = npos; i < mTempNPolys; ++i)
				mPolys[i - npos] = mPolys[i];
			mTempNPolys -= npos;

			// Handle the connection.
			if (navmesh != nullptr)
			{
				dtStatus status = navmesh->getOffMeshConnectionPolyEndPoints(prevRef, polyRef, startPos, endPos);
				if (dtStatusSucceed(status))
				{
					if (mNSmoothPath < MAX_SMOOTH)
					{
						dtVcopy(&mSmoothPath[mNSmoothPath * 3], startPos);
						mNSmoothPath++;
						// Hack to make the dotted path not visible during off-mesh connection.
						if (mNSmoothPath & 1)
						{
							dtVcopy(&mSmoothPath[mNSmoothPath * 3], startPos);
							mNSmoothPath++;
						}
					}
					// Move position at the other side of the off-mesh link.
					dtVcopy(mIterPos, endPos);
					float eh = 0.0f;
					mNavQuery->getPolyHeight(mPolys[0], mIterPos, &eh);
					mIterPos[1] = eh;
				}
			}
		}

		// Store results.
		if (mNSmoothPath < MAX_SMOOTH)
		{
			dtVcopy(&mSmoothPath[mNSmoothPath * 3], mIterPos);
			mNSmoothPath++;
		}
	}
}

void RcNavQuery::AutoRecalcStraightPaths(const v3dxVector3* start, const v3dxVector3* end)
{
	if (mStartRef == 0 || mEndRef == 0)
	{
		mNPolys = 0;
		mNStraightPath = 0;
		return;
	}

	float epos[3];

	
	float endpos[3] = { end->X, end->Y, end->Z };
	dtVcopy(epos, endpos);
	if (mPolys[mNPolys - 1] != mEndRef)
		mNavQuery->closestPointOnPoly(mPolys[mNPolys - 1], endpos, epos, 0);

	float startpos[3] = { start->X, start->Y, start->Z };
	mNavQuery->findStraightPath(startpos, epos, mPolys, mNPolys,
		mStraightPath, mStraightPathFlags,
		mStraightPathPolys, &mNStraightPath, MAX_POLYS, mStraightPathOptions);
}

NS_END
