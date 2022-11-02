using EngineNS.Bricks.Terrain.CDLOD;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class UPhySceneDesc : AuxPtrType<PhySceneDesc>
    {
        public UPhySceneDesc(PhySceneDesc self)
        {
            mCoreObject = self;
        }
    }
    public class UPhyScene : AuxPtrType<PhyScene>
    {
        public UPhyScene(PhyScene self)
        {
            mCoreObject = self;
        }
        public bool IsPxFetchingPose { get; protected set; }
        public unsafe void Tick(float elapsedSecond)
        {
            var elapse = elapsedSecond;
            if (elapse > 1.0F || elapse <= 0.0F)
                return;
            uint scratchMemBlockSize = 0;
            void* scratchMemBlock = (void*)0;
            uint errorState = 0;
            //这里要考虑elapse过久，分多次tick处理
            const float StepTime = 1 / 20.0f;
            int count = (int)(elapse / StepTime);
            float fm = elapse % StepTime;
            for (int i = 0; i < count; i++)
            {
                mCoreObject.Simulate(StepTime, scratchMemBlock, scratchMemBlockSize, true);
                mCoreObject.FetchResults(false, &errorState);
            }
            if (fm > 0)
            {
                mCoreObject.Simulate(fm, scratchMemBlock, scratchMemBlockSize, true);
                mCoreObject.FetchResults(false, &errorState);
            }
            uint activeActorCount = 0;
            try
            {
                mCoreObject.LockRead();
                IsPxFetchingPose = true;
                var actors = mCoreObject.UpdateActorTransforms(ref activeActorCount);
                for (uint i = 0; i < activeActorCount; ++i)
                {
                    var actor = mCoreObject.GetActor(actors, i);
                    if (actor.IsValidPointer)
                    {
                        actor.UpdateTransform();

                        var csActor = UPhyActor.GetActor(actor);
                        if (csActor != null && csActor.TagNode != null)
                        {
                            csActor.TagNode.Placement.Position = csActor.mCoreObject.mPosition.AsDVector();
                            csActor.TagNode.Placement.Quat = csActor.mCoreObject.mRotation;
                        }
                    }
                }
            }
            finally
            {
                IsPxFetchingPose = false;
                mCoreObject.UnlockRead();
            }
        }
        public UPhyController CreateBoxController(in PhyBoxControllerDesc desc)
        {
            var self = mCoreObject.CreateBoxController(desc);
            if (self.IsValidPointer == false)
                return null;
            return new UPhyController(self);
        }
        public UPhyController CreateCapsuleController(in PhyCapsuleControllerDesc desc)
        {
            var self = mCoreObject.CreateCapsuleController(desc);
            if (self.IsValidPointer == false)
                return null;
            return new UPhyController(self);
        }        
    }

    public class UPhySceneMember : IMemberTickable
    {
        private GamePlay.Scene.UScene HostScene;
        private UPhyScene mPxScene;
        public UPhyScene PxScene
        {
            get
            {
                if (mPxScene == null)
                {
                    var task = Initialize(HostScene);
                }
                return mPxScene;
            }
        }
        public async System.Threading.Tasks.Task<bool> Initialize(object host)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var scene = host as GamePlay.Scene.UScene;
            if (scene == null)
                return false;
            HostScene = scene;

            var pc = UEngine.Instance.PhyModule.PhyContext;
            if (pc == null)
                return false;
            
            var desc = pc.CreateSceneDesc();
            desc.mCoreObject.SetFlags(PhySceneFlag.eENABLE_ACTIVE_ACTORS);
            var gravity = new Vector3(0, -9.8f, 0);
            desc.mCoreObject.SetGravity(in gravity);
            //desc.mCoreObject.SetOnTrigger()
            mPxScene = pc.CreateScene(desc);
            return true;
        }
        public void Cleanup(object host)
        {
            mPxScene?.Dispose();
            mPxScene = null;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UPhySceneMember), nameof(TickLogic));
        public void TickLogic(object host, int ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                PxScene?.Tick(((float)ellapse) * 0.001f);
            }   
        }
        public void TickRender(object host, int ellapse)
        {

        }
        public void TickSync(object host, int ellapse)
        {

        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    public partial class UScene
    {
        public Bricks.PhysicsCore.UPhySceneMember PxSceneMB { get; } = new Bricks.PhysicsCore.UPhySceneMember();
    }
}
