using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    public class TtNavCrowd : AuxPtrType<RcNavCrowd>
    {
        public TtNavCrowd()
        {
            mCoreObject = RcNavCrowd.CreateInstance();
        }
        public bool Init(TtNavQuery navquery, TtNavMesh nav, float radius)
        {
            return mCoreObject.Init(navquery.mCoreObject, nav.mCoreObject, radius);
        }
        public int AddAgent(in Vector3 pos, float radius, float height, int flags, float m_obstacleAvoidanceType, float m_separationWeight)
        {
            return mCoreObject.AddAgent(pos, radius, height, flags, m_obstacleAvoidanceType, m_separationWeight);
        }
        public void RemoveAgent(int idx)
        {
            mCoreObject.RemoveAgent(idx);
        }
        public void SetMoveTargetAll(float p)
        {
            mCoreObject.SetMoveTargetAll(p);
        }
        public void SetMoveTarget(int id, in Vector3 pos)
        {
            mCoreObject.SetMoveTarget(id, in pos);
        }
        public void UpdateTick(float dt)
        {
            mCoreObject.UpdateTick(dt);
        }
        public Vector3 GetPosition(int idx)
        {
            return mCoreObject.GetPosition(idx);
        }
    }
}
