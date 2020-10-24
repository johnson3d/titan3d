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

template <>
struct Type2TypeConverter<v3dxVector3>
{
	typedef v3dVector3_t		TarType;
};

extern "C"
{
	Cpp2CS1(EngineNS, RecastBuilder, SetInputGeom);
	Cpp2CS0(EngineNS, RecastBuilder, BuildNavi);

	Cpp2CS0(EngineNS, RecastBuilder, GetCellSize);
	Cpp2CS1(EngineNS, RecastBuilder, SetCellSize);
	Cpp2CS0(EngineNS, RecastBuilder, GetCellHeight);
	Cpp2CS1(EngineNS, RecastBuilder, SetCellHeight);
	Cpp2CS0(EngineNS, RecastBuilder, GetAgentHeight);
	Cpp2CS1(EngineNS, RecastBuilder, SetAgentHeight);
	Cpp2CS0(EngineNS, RecastBuilder, GetAgentRadius);
	Cpp2CS1(EngineNS, RecastBuilder, SetAgentRadius);
	Cpp2CS0(EngineNS, RecastBuilder, GetAgentMaxClimb);
	Cpp2CS1(EngineNS, RecastBuilder, SetAgentMaxClimb);
	Cpp2CS0(EngineNS, RecastBuilder, GetAgentMaxSlope);
	Cpp2CS1(EngineNS, RecastBuilder, SetAgentMaxSlope);
	Cpp2CS0(EngineNS, RecastBuilder, GetRegionMinSize);
	Cpp2CS1(EngineNS, RecastBuilder, SetRegionMinSize);
	Cpp2CS0(EngineNS, RecastBuilder, GetRegionMergeSize);
	Cpp2CS1(EngineNS, RecastBuilder, SetRegionMergeSize);
	Cpp2CS0(EngineNS, RecastBuilder, GetEdgeMaxLen);
	Cpp2CS1(EngineNS, RecastBuilder, SetEdgeMaxLen);
	Cpp2CS0(EngineNS, RecastBuilder, GetEdgeMaxError);
	Cpp2CS1(EngineNS, RecastBuilder, SetEdgeMaxError);
	Cpp2CS0(EngineNS, RecastBuilder, GetVertsPerPoly);
	Cpp2CS1(EngineNS, RecastBuilder, SetVertsPerPoly);
	Cpp2CS0(EngineNS, RecastBuilder, GetDetailSampleDist);
	Cpp2CS1(EngineNS, RecastBuilder, SetDetailSampleDist);
	Cpp2CS0(EngineNS, RecastBuilder, GetDetailSampleMaxError);
	Cpp2CS1(EngineNS, RecastBuilder, SetDetailSampleMaxError);
	Cpp2CS0(EngineNS, RecastBuilder, GetPartitionType);
	Cpp2CS1(EngineNS, RecastBuilder, SetPartitionType);

	Cpp2CS0(EngineNS, RecastBuilder, GetMinBox);
	Cpp2CS1(EngineNS, RecastBuilder, SetMinBox);
	Cpp2CS0(EngineNS, RecastBuilder, GetMaxBox);
	Cpp2CS1(EngineNS, RecastBuilder, SetMaxBox);
}