#include "RecastBuilder.h"
#include "InputGeom.h"

NS_BEGIN

RTTI_IMPL(EngineNS::RecastBuilder, EngineNS::VIUnknown);

RecastBuilder::RecastBuilder() :
	mFilterLowHangingObstacles(true),
	mFilterLedgeSpans(true),
	mFilterWalkableLowHeightSpans(true),
	m_ctx(0)
{
	resetCommonSettings();
}


RecastBuilder::~RecastBuilder()
{
	
}

void RecastBuilder::resetCommonSettings()
{
	mCellSize = 0.3f;
	mCellHeight = 0.2f;
	mAgentHeight = 2.0f;
	mAgentRadius = 0.6f;
	mAgentMaxClimb = 0.9f;
	mAgentMaxSlope = 45.0f;
	mRegionMinSize = 8;
	mRegionMergeSize = 20;
	mEdgeMaxLen = 12.0f;
	mEdgeMaxError = 1.3f;
	mVertsPerPoly = 6.0f;
	mDetailSampleDist = 6.0f;
	mDetailSampleMaxError = 1.0f;
	mPartitionType = SAMPLE_PARTITION_WATERSHED;

	mMinBox.setValue(1, 1, 1);
	mMaxBox.setValue(-1, -1, -1);
}

void RecastBuilder::SetInputGeom(InputGeom* geom)
{
	m_geom.StrongRef(geom);
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, RecastBuilder, SetInputGeom, InputGeom*);
	CSharpReturnAPI0(RcNavMesh*, EngineNS, RecastBuilder, BuildNavi);

	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetCellSize);
	CSharpAPI1(EngineNS, RecastBuilder, SetCellSize, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetCellHeight);
	CSharpAPI1(EngineNS, RecastBuilder, SetCellHeight, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetAgentHeight);
	CSharpAPI1(EngineNS, RecastBuilder, SetAgentHeight, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetAgentRadius);
	CSharpAPI1(EngineNS, RecastBuilder, SetAgentRadius, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetAgentMaxClimb);
	CSharpAPI1(EngineNS, RecastBuilder, SetAgentMaxClimb, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetAgentMaxSlope);
	CSharpAPI1(EngineNS, RecastBuilder, SetAgentMaxSlope, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetRegionMinSize);
	CSharpAPI1(EngineNS, RecastBuilder, SetRegionMinSize, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetRegionMergeSize);
	CSharpAPI1(EngineNS, RecastBuilder, SetRegionMergeSize, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetEdgeMaxLen);
	CSharpAPI1(EngineNS, RecastBuilder, SetEdgeMaxLen, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetEdgeMaxError);
	CSharpAPI1(EngineNS, RecastBuilder, SetEdgeMaxError, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetVertsPerPoly);
	CSharpAPI1(EngineNS, RecastBuilder, SetVertsPerPoly, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetDetailSampleDist);
	CSharpAPI1(EngineNS, RecastBuilder, SetDetailSampleDist, float);
	CSharpReturnAPI0(float, EngineNS, RecastBuilder, GetDetailSampleMaxError);
	CSharpAPI1(EngineNS, RecastBuilder, SetDetailSampleMaxError, float);
	CSharpReturnAPI0(int, EngineNS, RecastBuilder, GetPartitionType);
	CSharpAPI1(EngineNS, RecastBuilder, SetPartitionType, int);

	CSharpReturnAPI0(v3dVector3_t, EngineNS, RecastBuilder, GetMinBox);
	CSharpAPI1(EngineNS, RecastBuilder, SetMinBox, v3dxVector3);
	CSharpReturnAPI0(v3dVector3_t, EngineNS, RecastBuilder, GetMaxBox);
	CSharpAPI1(EngineNS, RecastBuilder, SetMaxBox, v3dxVector3);
}