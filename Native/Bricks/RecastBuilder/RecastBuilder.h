#pragma once
#include "../../Graphics/GfxPreHead.h"

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
class RecastBuilder : public VIUnknown
{
public:
	RTTI_DEF(RecastBuilder, 0xca929b075b99f43d, false);
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
	VDef_ReadWrite(float, CellSize, m);
	VDef_ReadWrite(float, CellHeight, m);
	VDef_ReadWrite(float, AgentHeight, m);
	VDef_ReadWrite(float, AgentRadius, m);
	VDef_ReadWrite(float, AgentMaxClimb, m);
	VDef_ReadWrite(float, AgentMaxSlope, m);
	VDef_ReadWrite(float, RegionMinSize, m);
	VDef_ReadWrite(float, RegionMergeSize, m);
	VDef_ReadWrite(float, EdgeMaxLen, m);
	VDef_ReadWrite(float, EdgeMaxError, m);
	VDef_ReadWrite(float, VertsPerPoly, m);
	VDef_ReadWrite(float, DetailSampleDist, m);
	VDef_ReadWrite(float, DetailSampleMaxError, m);
	VDef_ReadWrite(int, PartitionType, m);
	
	VDef_ReadWrite(v3dxVector3, MinBox, m);
	VDef_ReadWrite(v3dxVector3, MaxBox, m);

	bool IsEmptyBoundingBox()
	{
		if (mMinBox.x > mMaxBox.x || mMinBox.y > mMaxBox.y || mMinBox.z > mMaxBox.z)
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
