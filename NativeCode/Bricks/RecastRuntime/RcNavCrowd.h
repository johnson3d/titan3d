#pragma once

#include "DetourObstacleAvoidance.h"

#include "RcNavMesh.h"
#include "RcNavQuery.h"

NS_BEGIN

class TR_CLASS()
	RcNavCrowd : public VIUnknown
{
private:
	static const int AGENT_MAX_TRAIL = 64;
	static const int MAX_AGENTS = 128;

	struct AgentTrail
	{
		float trail[AGENT_MAX_TRAIL * 3];
		int htrail;
	};

private:
	bool m_run;
	float m_targetPos[3];

	dtCrowd* m_crowd;
	dtNavMeshQuery* m_navquery;
	dtPolyRef m_targetRef;

	dtCrowdAgentDebugInfo m_agentDebug;
	dtObstacleAvoidanceDebugData* m_vod;

	AgentTrail m_trails[MAX_AGENTS];

public:
	inline bool isRunning() const { return m_run; }
	inline void setRunning(const bool s) { m_run = s; }

public:
	ENGINE_RTTI(RcNavCrowd);
	RcNavCrowd();
	~RcNavCrowd();

	bool			Init(RcNavQuery* navquery, RcNavMesh* nav, float radius);
	int				AddAgent(const v3dxVector3* pos, float radius, float height, int flags, float m_obstacleAvoidanceType, float m_separationWeight);
	void			RemoveAgent(const int idx);
	void			SetMoveTargetAll(float p);
	void			SetMoveTarget(const int id, const v3dxVector3* pos);
	void			UpdateTick(const float dt);

	v3dxVector3		GetPosition(const int idx);
};

NS_END