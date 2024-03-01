#pragma once
#include "../../NextRHI/NxGeomMesh.h"

#include "../RecastRuntime/RcNavMesh.h"

#include "Recast.h"
#include "RecastDebugDraw.h"
#include "DetourDebugDraw.h"
#include "DetourNavMeshQuery.h"
#include "DetourCrowd.h"

#include "SampleInterfaces.h"
#include "DetourNavMeshBuilder.h"

#include "RecastCommon.h"


NS_BEGIN

class InputGeom;
class TR_CLASS()
	RecastBuilder : public IWeakReference
{
public:
	ENGINE_RTTI(RecastBuilder);
protected:
	AutoRef<InputGeom> m_geom;
	//class dtNavMeshQuery* m_navQuery;
	//class dtCrowd* m_crowd;

	float mCellSize;
	float mCellHeight;
	float mAgentHeight;
	float mAgentRadius;
	float mAgentMaxClimb;
	float mAgentMaxSlope;
	float mRegionMinSize;
	float mRegionMergeSize;
	float mEdgeMaxLen;
	float mEdgeMaxError;
	float mVertsPerPoly;
	float mDetailSampleDist;
	float mDetailSampleMaxError;
	int mPartitionType;

	bool mFilterLowHangingObstacles;
	bool mFilterLedgeSpans;
	bool mFilterWalkableLowHeightSpans;

	v3dxVector3			mMinBox;
	v3dxVector3			mMaxBox;
	BuildContext* m_ctx;
public:
	bool IsEmptyBoundingBox()
	{
		if (mMinBox.X > mMaxBox.X || mMinBox.Y > mMaxBox.Y || mMinBox.Z > mMaxBox.Z)
			return true;
		return false;
	}
public:
	RecastBuilder();
	virtual ~RecastBuilder();
	
	void setContext(BuildContext* ctx) { m_ctx = ctx; }
	
	virtual float getAgentRadius() { return mAgentRadius; }
	virtual float getAgentHeight() { return mAgentHeight; }
	virtual float getAgentClimb() { return mAgentMaxClimb; }
	
	void updateToolStates(const float dt);
	void resetToolStates();
	void renderToolStates();
	void renderOverlayToolStates(double* proj, double* model, int* view);

	void resetCommonSettings();
	void handleCommonSettings();

	virtual void SetInputGeom(InputGeom* geom);
	virtual RcNavMesh* BuildNavi() = 0;
};

NS_END
