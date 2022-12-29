#include "../../NextRHI/NxGeomMesh.h"
#include "RcNavCrowd.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::RcNavCrowd);

/// Performs a vector copy.
///  @param[out]	dest	The result. [(x, y, z)]
///  @param[in]		a		The vector to copy. [(x, y, z)]
inline void dtVcopy(float* dest, const float* a)
{
	dest[0] = a[0];
	dest[1] = a[1];
	dest[2] = a[2];
}

RcNavCrowd::RcNavCrowd( ) : m_run(true), m_targetRef(0)
{

}

RcNavCrowd::~RcNavCrowd()
{
	dtFreeCrowd(m_crowd);
}

bool RcNavCrowd::Init(RcNavQuery* navquery, RcNavMesh* nav, float radius)
{
	if (!navquery || !navquery->GetNavMeshQuery() || !nav || !nav->GetNavMesh())
		return false;

	memset(m_trails, 0, sizeof(m_trails));

	m_vod = dtAllocObstacleAvoidanceDebugData();
	if (m_vod->init(2048) == false)
		return false;

	memset(&m_agentDebug, 0, sizeof(m_agentDebug));
	m_agentDebug.idx = -1;
	m_agentDebug.vod = m_vod;

	m_crowd = dtAllocCrowd();
	m_navquery = navquery->GetNavMeshQuery();
	if (m_crowd->init(MAX_AGENTS, radius, nav->GetNavMesh()) == false)
		return false;

	// Make polygons with 'disabled' flag invalid.
	m_crowd->getEditableFilter(0)->setExcludeFlags(0x10);

	// Setup local avoidance params to different qualities.
	dtObstacleAvoidanceParams params;
	// Use mostly default settings, copy from dtCrowd.
	memcpy(&params, m_crowd->getObstacleAvoidanceParams(0), sizeof(dtObstacleAvoidanceParams));

	// Low (11)
	params.velBias = 0.5f;
	params.adaptiveDivs = 5;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 1;
	m_crowd->setObstacleAvoidanceParams(0, &params);

	// Medium (22)
	params.velBias = 0.5f;
	params.adaptiveDivs = 5;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 2;
	m_crowd->setObstacleAvoidanceParams(1, &params);

	// Good (45)
	params.velBias = 0.5f;
	params.adaptiveDivs = 7;
	params.adaptiveRings = 2;
	params.adaptiveDepth = 3;
	m_crowd->setObstacleAvoidanceParams(2, &params);

	// High (66)
	params.velBias = 0.5f;
	params.adaptiveDivs = 7;
	params.adaptiveRings = 3;
	params.adaptiveDepth = 3;

	m_crowd->setObstacleAvoidanceParams(3, &params);

	return true;
}

int RcNavCrowd::AddAgent(const v3dxVector3* pos, float radius, float height, int flags, float m_obstacleAvoidanceType, float m_separationWeight)
{
	dtCrowdAgentParams ap;
	memset(&ap, 0, sizeof(ap));
	ap.radius = radius;
	ap.height = height;
	ap.maxAcceleration = 8.0f;
	ap.maxSpeed = 3.5f;
	ap.collisionQueryRange = ap.radius * 12.0f;
	ap.pathOptimizationRange = ap.radius * 30.0f;
	ap.updateFlags |= flags;
	//if (m_toolParams.m_anticipateTurns)
	//	ap.updateFlags |= DT_CROWD_ANTICIPATE_TURNS;
	//if (m_toolParams.m_optimizeVis)
	//	ap.updateFlags |= DT_CROWD_OPTIMIZE_VIS;
	//if (m_toolParams.m_optimizeTopo)
	//	ap.updateFlags |= DT_CROWD_OPTIMIZE_TOPO;
	//if (m_toolParams.m_obstacleAvoidance)
	//	ap.updateFlags |= DT_CROWD_OBSTACLE_AVOIDANCE;
	//if (m_toolParams.m_separation)
	//	ap.updateFlags |= DT_CROWD_SEPARATION;
	ap.obstacleAvoidanceType = (unsigned char)m_obstacleAvoidanceType;
	ap.separationWeight = m_separationWeight;

	float p[3] = { pos->x, pos->y, pos->z };
	int idx = m_crowd->addAgent(p, &ap);
	if (idx != -1)
	{
		//if (m_targetRef)
		//	m_crowd->requestMoveTarget(idx, m_targetRef, m_targetPos);

		// Init trail
		AgentTrail* trail = &m_trails[idx];
		for (int i = 0; i < AGENT_MAX_TRAIL; ++i)
			dtVcopy(&trail->trail[i * 3], p);
		trail->htrail = 0;
	}

	return idx;
}

void RcNavCrowd::RemoveAgent(const int idx)
{
	m_crowd->removeAgent(idx);

	if (idx == m_agentDebug.idx)
		m_agentDebug.idx = -1;
}

void RcNavCrowd::SetMoveTargetAll(float p)
{
	if (!m_crowd || !m_navquery) return;

	// Find nearest point on navmesh and set move request to that location.
	//dtNavMeshQuery* navquery = m_sample->getNavMeshQuery();
	const dtQueryFilter* filter = m_crowd->getFilter(0);
	const float* halfExtents = m_crowd->getQueryExtents();

	
	m_navquery->findNearestPoly(&p, halfExtents, filter, &m_targetRef, m_targetPos);

	if (m_agentDebug.idx != -1)
	{
		const dtCrowdAgent* ag = m_crowd->getAgent(m_agentDebug.idx);
		if (ag && ag->active)
			m_crowd->requestMoveTarget(m_agentDebug.idx, m_targetRef, m_targetPos);
	}
	else
	{
		for (int i = 0; i < m_crowd->getAgentCount(); ++i)
		{
			const dtCrowdAgent* ag = m_crowd->getAgent(i);
			if (!ag->active) continue;
			if (i > 0)
			{
				m_crowd->requestMoveTarget(i, m_targetRef, m_targetPos);
			}

		}
	}
}

void RcNavCrowd::SetMoveTarget(const int id, const v3dxVector3* pos)
{
	if (!m_crowd || !m_navquery) return;

	const dtCrowdAgent* ag = m_crowd->getAgent(id);
	if (!ag || !ag->active) return;

	// Find nearest point on navmesh and set move request to that location.
	//dtNavMeshQuery* navquery = m_sample->getNavMeshQuery();
	const dtQueryFilter* filter = m_crowd->getFilter(0);
	const float* halfExtents = m_crowd->getQueryExtents();

	float p[3] = { pos->x, pos->y, pos->z };
	m_navquery->findNearestPoly(p, halfExtents, filter, &m_targetRef, m_targetPos);

	m_crowd->requestMoveTarget(id, m_targetRef, m_targetPos);
}

void RcNavCrowd::UpdateTick(const float dt)
{
	if (!m_navquery || !m_crowd) return;

	//TimeVal startTime = getPerfTime();

	m_crowd->update(dt, &m_agentDebug);

	//TimeVal endTime = getPerfTime();

	// Update agent trails
	for (int i = 0; i < m_crowd->getAgentCount(); ++i)
	{
		const dtCrowdAgent* ag = m_crowd->getAgent(i);
		AgentTrail* trail = &m_trails[i];
		if (!ag->active)
			continue;
		// Update agent movement trail.
		trail->htrail = (trail->htrail + 1) % AGENT_MAX_TRAIL;
		dtVcopy(&trail->trail[trail->htrail * 3], ag->npos);
	}

	m_agentDebug.vod->normalizeSamples();

	//m_crowdSampleCount.addSample((float)crowd->getVelocitySampleCount());
	//m_crowdTotalTime.addSample(getPerfTimeUsec(endTime - startTime) / 1000.0f);
}

v3dxVector3 RcNavCrowd::GetPosition(const int idx)
{
	if (!m_crowd || !m_navquery) return v3dxVector3();

	const dtCrowdAgent* ag = m_crowd->getAgent(idx);
	if (!ag || !ag->active) return v3dxVector3();

	return v3dxVector3(ag->npos[0], ag->npos[1], ag->npos[2]);
}

NS_END

