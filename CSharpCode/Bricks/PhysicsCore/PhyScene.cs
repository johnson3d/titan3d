using EngineNS.Bricks.Terrain.CDLOD;
using EngineNS.GamePlay.Scene;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS
{
    partial struct PhyEntity
    {
        public unsafe void BindObject(Bricks.PhysicsCore.TtPhyScene scene, object obj)
        {
            scene?.mCoreObject.LockWrite();
            var gchandle = System.Runtime.InteropServices.GCHandle.Alloc(obj, System.Runtime.InteropServices.GCHandleType.Weak);
            mCSharpHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(gchandle).ToPointer();
            scene?.mCoreObject.UnlockWrite();
        }
        public unsafe void UnbindObject(Bricks.PhysicsCore.TtPhyScene scene)
        {
            if (mCSharpHandle == IntPtr.Zero.ToPointer())
                return;
            scene?.mCoreObject.LockWrite();
            var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)mCSharpHandle);
            mCSharpHandle = IntPtr.Zero.ToPointer();
            gchandle.Free();
            scene?.mCoreObject.UnlockWrite();
        }
        public unsafe object GetCSharpHandle()
        {
            if (mCSharpHandle == IntPtr.Zero.ToPointer())
                return null;
            var gchandle = System.Runtime.InteropServices.GCHandle.FromIntPtr((IntPtr)mCSharpHandle);
            return gchandle.Target;
        }
    }
}

namespace EngineNS.Bricks.PhysicsCore
{
    public class TtPhySceneDesc : AuxPtrType<PhySceneDesc>
    {
        public TtPhySceneDesc(PhySceneDesc self)
        {
            mCoreObject = self;
        }
    }
    public class TtPhyScene : AuxPtrType<PhyScene>
    {
        public TtPhyScene(PhyScene self)
        {
            mCoreObject = self;
        }
        public bool IsPxFetchingPose { get; protected set; }
        private static Profiler.TimeScope mScopeTickSimulate;
        private static Profiler.TimeScope ScopeTickSimulate
        {
            get
            {
                if (mScopeTickSimulate == null)
                    mScopeTickSimulate = new Profiler.TimeScope(typeof(TtPhyScene), "Tick.Simulate");
                return mScopeTickSimulate;
            }
        }
        private static Profiler.TimeScope mScopeTickUpdateActor;
        private static Profiler.TimeScope ScopeTickUpdateActor
        {
            get
            {
                if (mScopeTickUpdateActor == null)
                    mScopeTickUpdateActor = new Profiler.TimeScope(typeof(TtPhyScene), "Tick.UpdateActor");
                return mScopeTickUpdateActor;
            }
        } 
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
            using (new Profiler.TimeScopeHelper(ScopeTickSimulate))
            {
                mCoreObject.LockWrite();
                for (int i = 0; i < count; i++)
                {
                    mCoreObject.Simulate(StepTime, scratchMemBlock, scratchMemBlockSize, true);
                    mCoreObject.FetchResults(true, &errorState);
                }
                if (fm > 0)
                {
                    mCoreObject.Simulate(fm, scratchMemBlock, scratchMemBlockSize, true);
                    mCoreObject.FetchResults(true, &errorState);
                }
                mCoreObject.UnlockWrite();
            }

            //todo: Execute on logic thread
            using (new Profiler.TimeScopeHelper(ScopeTickUpdateActor))
            {
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

                            var csActor = TtPhyActor.GetActor(actor);
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
        }
        public TtPhyController CreateBoxController(in PhyBoxControllerDesc desc)
        {
            var self = mCoreObject.CreateBoxController(desc);
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyController(this, self);
        }
        public TtPhyController CreateCapsuleController(in PhyCapsuleControllerDesc desc)
        {
            var self = mCoreObject.CreateCapsuleController(desc);
            if (self.IsValidPointer == false)
                return null;
            return new TtPhyController(this, self);
        }        
    }

    public class TtPhySceneMember : IMemberTickable
    {
        private GamePlay.Scene.TtScene HostScene;
        private TtPhyScene mPxScene;
        public TtPhyScene PxScene
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
        EngineNS.PhySceneDesc.FDelegate_FonContact mOnContackCallBack;
        EngineNS.PhySceneDesc.FDelegate_FonTrigger mOnTriggerCallBack;
        private TtPhySceneDesc mPxceneDesc = null;
        public async System.Threading.Tasks.Task<bool> Initialize(object host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var scene = host as GamePlay.Scene.TtScene;
            if (scene == null)
                return false;
            HostScene = scene;

            var pc = TtEngine.Instance.PhyModule.PhyContext;
            if (pc == null)
                return false;

            mPxceneDesc = pc.CreateSceneDesc();
            mPxceneDesc.mCoreObject.SetFlags(EPhySceneFlag.eENABLE_ACTIVE_ACTORS);
            var gravity = new Vector3(0, -9.8f, 0);
            mPxceneDesc.mCoreObject.SetGravity(in gravity);
            unsafe
            {
                mOnContackCallBack = new PhySceneDesc.FDelegate_FonContact(OnContact);
                mPxceneDesc.mCoreObject.SetOnContact(mOnContackCallBack);
                mOnTriggerCallBack = new PhySceneDesc.FDelegate_FonTrigger(OnTrigger);
                mPxceneDesc.mCoreObject.SetOnTrigger(mOnTriggerCallBack);
            }
            //desc.mCoreObject.SetOnTrigger()
            mPxScene = pc.CreateScene(mPxceneDesc);

            return true;
        }
        public unsafe void OnContact(void* arg0, FPhyContactPairHeader* arg1, FPhyContactPair* arg2,uint arg3)
        {
            var phyActor1 = new EngineNS.PhyActor(arg1->actors[0]);
            var actor1 = TtPhyActor.GetActor(phyActor1);
            var phyActor2 = new EngineNS.PhyActor(arg1->actors[1]);
            var actor2 = TtPhyActor.GetActor(phyActor2);

            if (actor1 != null && actor2 != null)
            {
                var task = TtEngine.Instance.EventPoster.CreatePostTask();
                task.UserArguments.Obj1 = actor1;
                task.UserArguments.Obj2 = actor2;
                task.PostAction = static (state) =>
                {
                    var actor1 = state.UserArguments.Obj1 as TtPhyActor;
                    var actor2 = state.UserArguments.Obj2 as TtPhyActor;
                    actor1.RigidBodyNode.OnContact(actor1.TagNode, actor2.TagNode);
                    actor2.RigidBodyNode.OnContact(actor2.TagNode, actor1.TagNode);
                    return true;
                };
                TtEngine.Instance.EventPoster.PostTask(Thread.Async.EAsyncTarget.Logic, task);

                //actor1.RigidBodyNode.OnContact(actor1.TagNode, actor2.TagNode);
                //actor2.RigidBodyNode.OnContact(actor2.TagNode, actor1.TagNode);
            }
        }
        public unsafe void OnTrigger(void* arg0, FPhyTriggerPair* arg1, uint arg2)
        {
            var triggerActor = TtPhyActor.GetActor(new EngineNS.PhyActor(arg1->triggerActor));
            if (triggerActor == null)
                return;

            var otherController = TtPhyController.GetPhyController(new PhyController(arg1->otherActor));
            if (otherController != null)
            {
                if (EPhyPairFlag.eNOTIFY_TOUCH_FOUND == (arg1->status & EPhyPairFlag.eNOTIFY_TOUCH_FOUND))
                {
                    var task = TtEngine.Instance.EventPoster.CreatePostTask();
                    task.UserArguments.Obj1 = triggerActor;
                    task.UserArguments.Obj2 = otherController;
                    task.PostAction = static (state) =>
                    {
                        var triggerActor = state.UserArguments.Obj1 as TtPhyActor;
                        var otherController = state.UserArguments.Obj2 as TtPhyController;
                        triggerActor.RigidBodyNode.OnBeginTrigger(triggerActor.TagNode, otherController.TagNode);
                        return true;
                    };
                    TtEngine.Instance.EventPoster.PostTask(Thread.Async.EAsyncTarget.Logic, task);
                    //triggerActor.RigidBodyNode.OnBeginTrigger(triggerActor.TagNode, otherController.TagNode);
                    //otherActor.RigidBodyNode.OnBeginTrigger(otherActor.TagNode, triggerActor.TagNode);
                    return;
                }

                if (EPhyPairFlag.eNOTIFY_TOUCH_LOST == (arg1->status & EPhyPairFlag.eNOTIFY_TOUCH_LOST))
                {
                    var task = TtEngine.Instance.EventPoster.CreatePostTask();
                    task.UserArguments.Obj1 = triggerActor;
                    task.UserArguments.Obj2 = otherController;
                    task.PostAction = static (state) =>
                    {
                        var triggerActor = state.UserArguments.Obj1 as TtPhyActor;
                        var otherController = state.UserArguments.Obj2 as TtPhyController;
                        triggerActor.RigidBodyNode.OnBeginTrigger(triggerActor.TagNode, otherController.TagNode);
                        return true;
                    };
                    TtEngine.Instance.EventPoster.PostTask(Thread.Async.EAsyncTarget.Logic, task);
                    //triggerActor.RigidBodyNode.OnEndTrigger(triggerActor.TagNode, otherController.TagNode);
                    //otherActor.RigidBodyNode.OnEndTrigger(otherActor.TagNode, triggerActor.TagNode);
                    return;
                }
            }

            var otherActor = TtPhyActor.GetActor(new EngineNS.PhyActor(arg1->otherActor));
            if (otherActor != null)
            {
                if (EPhyPairFlag.eNOTIFY_TOUCH_FOUND == (arg1->status & EPhyPairFlag.eNOTIFY_TOUCH_FOUND))
                {
                    var task = TtEngine.Instance.EventPoster.CreatePostTask();
                    task.UserArguments.Obj1 = triggerActor;
                    task.UserArguments.Obj2 = otherActor;
                    task.PostAction = static (state) =>
                    {
                        var triggerActor = state.UserArguments.Obj1 as TtPhyActor;
                        var otherActor = state.UserArguments.Obj2 as TtPhyActor;
                        triggerActor.RigidBodyNode.OnBeginTrigger(triggerActor.TagNode, otherActor.TagNode);
                        otherActor.RigidBodyNode.OnBeginTrigger(otherActor.TagNode, triggerActor.TagNode);
                        return true;
                    };
                    TtEngine.Instance.EventPoster.PostTask(Thread.Async.EAsyncTarget.Logic, task);

                    //triggerActor.RigidBodyNode.OnBeginTrigger(triggerActor.TagNode, otherActor.TagNode);
                    //otherActor.RigidBodyNode.OnBeginTrigger(otherActor.TagNode, triggerActor.TagNode);
                    return;
                }

                if (EPhyPairFlag.eNOTIFY_TOUCH_LOST == (arg1->status & EPhyPairFlag.eNOTIFY_TOUCH_LOST))
                {
                    var task = TtEngine.Instance.EventPoster.CreatePostTask();
                    task.UserArguments.Obj1 = triggerActor;
                    task.UserArguments.Obj2 = otherActor;
                    task.PostAction = static (state) =>
                    {
                        var triggerActor = state.UserArguments.Obj1 as TtPhyActor;
                        var otherActor = state.UserArguments.Obj2 as TtPhyActor;
                        triggerActor.RigidBodyNode.OnEndTrigger(triggerActor.TagNode, otherActor.TagNode);
                        otherActor.RigidBodyNode.OnEndTrigger(otherActor.TagNode, triggerActor.TagNode);
                        return true;
                    };
                    TtEngine.Instance.EventPoster.PostTask(Thread.Async.EAsyncTarget.Logic, task);

                    //triggerActor.RigidBodyNode.OnEndTrigger(triggerActor.TagNode, otherActor.TagNode);
                    //otherActor.RigidBodyNode.OnEndTrigger(otherActor.TagNode, triggerActor.TagNode);
                    return;
                }
            }
        }
        public void Cleanup(object host)
        {
            mPxScene?.Dispose();
            mPxScene = null;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtPhySceneMember), nameof(TickLogic));
                return mScopeTick;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeWaitPx;
        private static Profiler.TimeScope ScopeWaitPx
        {
            get
            {
                if (mScopeWaitPx == null)
                    mScopeWaitPx = new Profiler.TimeScope(typeof(TtPhySceneMember), nameof(TickLogic) + ".WaitPX");
                return mScopeWaitPx;
            }
        }
        System.Threading.AutoResetEvent PxSceneTickEndEvent = new System.Threading.AutoResetEvent(false);
        private float TickLogic_ellapse;
        public void TickLogic(object host, float ellapse)
        {
            if (!HostScene.World.IsGameWorld)
                return;

            if (TtEngine.Instance.Config.UsePhysxMT)
            {
                TickLogic_ellapse = ellapse;
                TtEngine.Instance.EventPoster.RunOn(static (state) =>
                {
                    var scene = state.UserArguments.Obj0 as TtPhySceneMember;
                    scene.TickPxScene(scene.TickLogic_ellapse);
                    return true;
                }, Thread.Async.EAsyncTarget.Physics, this, PxSceneTickEndEvent);
                //PxSceneTickEndEvent?.WaitOne();

                //var hostNode = host as UNode;
                //hostNode.GetWorld().RegAfterTickAction(() =>
                TtEngine.Instance.RegAfterTickAction(() =>
                {
                    using (new Profiler.TimeScopeHelper(ScopeWaitPx))
                    {
                        PxSceneTickEndEvent?.WaitOne();
                    }
                });
            }
            else
            {
                TickPxScene(ellapse);
            }
        }
        private void TickPxScene(float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                try
                {
                    PxScene?.Tick(ellapse * 0.001f);
                }
                catch(Exception ex)
                {
                    return;
                }
            }
        }
        public void OnHostNotify(object host, in FHostNotify notify)
        {

        }
    }
}

namespace EngineNS.GamePlay.Scene
{
    public partial class TtScene
    {
        public Bricks.PhysicsCore.TtPhySceneMember PxSceneMB { get; } = new Bricks.PhysicsCore.TtPhySceneMember();
    }
}
