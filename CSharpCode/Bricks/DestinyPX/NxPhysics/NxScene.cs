using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxPhysics
{
    public class TtScene : AuxPtrType<EngineNS.NxPhysics.NxScene>
    {
        public List<TtRigidBody> mRigidBodies = new List<TtRigidBody>();
        public TtScene(EngineNS.NxPhysics.NxScene ptr)
        {
            mCoreObject = ptr;
        }
        public void AddActor(TtActor actor)
        {
            if (actor is TtRigidBody)
            {
                if (mRigidBodies.Contains(actor))
                    return;
                mRigidBodies.Add(actor as TtRigidBody);
            }
            
            mCoreObject.AddActor(actor.NativeActor);
        }
        public unsafe void Simulate(NxReal elapsed)
        {
            mCoreObject.Simulate(elapsed);
        }
    }
}
