#include "RecastBuilder.h"
#include "InputGeom.h"

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::RecastBuilder);

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
