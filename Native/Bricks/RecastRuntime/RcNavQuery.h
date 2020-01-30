#pragma once

#include "RcNavMesh.h"

NS_BEGIN

class RcNavQuery : public VIUnknown
{
public:
	enum SamplePolyAreas
	{
		SAMPLE_POLYAREA_GROUND,
		SAMPLE_POLYAREA_WATER,
		SAMPLE_POLYAREA_ROAD,
		SAMPLE_POLYAREA_DOOR,
		SAMPLE_POLYAREA_GRASS,
		SAMPLE_POLYAREA_JUMP,
	};
	enum SamplePolyFlags
	{
		SAMPLE_POLYFLAGS_WALK = 0x01,		// Ability to walk (ground, grass, road)
		SAMPLE_POLYFLAGS_SWIM = 0x02,		// Ability to swim (water).
		SAMPLE_POLYFLAGS_DOOR = 0x04,		// Ability to move through doors.
		SAMPLE_POLYFLAGS_JUMP = 0x08,		// Ability to jump.
		SAMPLE_POLYFLAGS_DISABLED = 0x10,		// Disabled polygon
		SAMPLE_POLYFLAGS_ALL = 0xffff	// All abilities.
	};

private:
	static const int MAX_POLYS = 256;
	static const int MAX_SMOOTH = 2048;

protected:
	float			mPolyPickExt[3];
	float			mIterPos[3];
	float			mTargetPos[3];
	float			mSmoothPath[MAX_SMOOTH * 3];

	float			mStraightPath[MAX_POLYS * 3];
	unsigned char	mStraightPathFlags[MAX_POLYS];
	int				mNStraightPath;
	int				mStraightPathOptions;

	int				mNSmoothPath;
	int				mNPolys;
	int				mTempNPolys;

	dtNavMeshQuery*	mNavQuery;
	dtCrowd*		mCrowd;

	dtQueryFilter	mFilter;
	dtPolyRef		mStartRef;
	dtPolyRef		mEndRef;
	dtPolyRef		mPolys[MAX_POLYS];
	dtPolyRef		mTempPolys[MAX_POLYS];
	dtPolyRef		mStraightPathPolys[MAX_POLYS];
	//dtPolyRef		m_parent[MAX_POLYS];
public:
	RTTI_DEF(RcNavQuery, 0xba8238cd5c2c71de, true);
	RcNavQuery();
	RcNavQuery(dtNavMeshQuery* query);
	~RcNavQuery();

	inline unsigned short GetIncludeFlags()
	{
		return mFilter.getIncludeFlags();
	}
	inline void SetIncludeFlags(const unsigned short flags)
	{
		mFilter.setIncludeFlags(flags);
	}

	inline unsigned short GetExcludeFlags()
	{
		return mFilter.getExcludeFlags();
	}
	inline void SetExcludeFlags(const unsigned short flags)
	{
		mFilter.setExcludeFlags(flags);
	}

	inline dtNavMeshQuery* GetNavMeshQuery() { return mNavQuery; };

	vBOOL	GetHeight(const v3dxVector3* pos, const v3dxVector3* pickext, float* h);
	vBOOL	FindStraightPath(const v3dxVector3* start, const v3dxVector3* end, IBlobObject* blob);
	void	ClosestPointOnPoly(const v3dxVector3* start, const v3dxVector3* end);
	void	AutoRecalcFollowPaths(const dtNavMesh* navmesh);
	void	AutoRecalcStraightPaths(const v3dxVector3* start, const v3dxVector3* end);
};

NS_END