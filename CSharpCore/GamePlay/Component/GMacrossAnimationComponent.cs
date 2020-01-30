using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GMacrossAnimationComponenInitializer), "动画宏图组件", "Animation Component")]
    public class GMacrossAnimationComponent : GComponent
    {
        [Rtti.MetaClass]
        public class GMacrossAnimationComponenInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public RName SkeletonAssetName
            {
                get; set;
            }

        }
        protected RName mSkeletonAssetName = RName.EmptyName;
        public RName SkeletonAssetName
        {
            get => mSkeletonAssetName;
            set
            {
                if (value == null || value == RName.EmptyName)
                    return;
                mSkeletonAssetName = value;
                var initializer = this.Initializer as GMacrossAnimationComponenInitializer;
                initializer.SkeletonAssetName = value;
                InitializeAnimationPose();
            }
        }
        protected CGfxAnimationPoseProxy mAnimationPoseProxy = new CGfxAnimationPoseProxy();
        public CGfxAnimationPoseProxy AnimationPoseProxy { get => mAnimationPoseProxy; set => mAnimationPoseProxy = value; }
        Dictionary<string, CGfxCachedAnimationPose> mCachedAnimPose = new Dictionary<string, CGfxCachedAnimationPose>();
        public Dictionary<string, CGfxCachedAnimationPose> CachedAnimPoses
        {
            get => mCachedAnimPose;
            set => mCachedAnimPose = value;
        }
        public CGfxCachedAnimationPose CreateCachedAnimPose(string name)
        {
            var cachedPose = new CGfxCachedAnimationPose(mAnimationPoseProxy.Pose.Clone());
            AddCachedAnimPose(name, cachedPose);
            return cachedPose;
        }
        public void AddCachedAnimPose(string name, CGfxCachedAnimationPose pose)
        {
            if (!mCachedAnimPose.ContainsKey(name))
            {
                mCachedAnimPose.Add(name, pose);
                mTickComponents.Add(pose);
            }
        }
        public void RemoveCachedAnimPose(string name)
        {
            if (mCachedAnimPose.ContainsKey(name))
            {
                mTickComponents.Remove(mCachedAnimPose[name]);
                mCachedAnimPose.Remove(name);
            }
        }

        public CGfxCachedAnimationPose GetCachedAnimPose(string name)
        {
            CGfxCachedAnimationPose temp = null;
            mCachedAnimPose.TryGetValue(name, out temp);
            return temp;
        }

        public List<Bricks.Animation.SkeletonControl.CGfxSkeletonControl> skeletonControls = new List<Bricks.Animation.SkeletonControl.CGfxSkeletonControl>();

        public GMacrossAnimationComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GMacrossAnimationComponenInitializer();
        }
        public GMacrossAnimationComponent(RName skeletonAsset)
        {
            var initializer = new GMacrossAnimationComponenInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, Actor.GActor host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GMacrossAnimationComponenInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            return true;
        }
        void InitializeAnimationPose()
        {
            mAnimationPoseProxy.Pose = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, mSkeletonAssetName).BoneTab.Clone();
        }

        protected Dictionary<RName, AnimationStateMachine> mStateMachineList = new Dictionary<RName, AnimationStateMachine>(new RName.EqualityComparer());
        public Dictionary<RName, AnimationStateMachine> StateMachineList
        {
            get => mStateMachineList;
            private set => mStateMachineList = value;
        }
        public AnimationStateMachine CreateStateMachine(string name)
        {
            AnimationStateMachine stateMachine = new AnimationStateMachine(name);
            //stateMachine.AnimationPose = this.AnimationPose;
            this.AddTickComponent(stateMachine);
            stateMachine.OnStatePreChange += StateMachine_OnStatePreChange;
            return stateMachine;
        }

        private void StateMachine_OnStatePreChange(object sender, StateMachine.StateChangeEventArgs e)
        {
            var machine = sender as AnimationStateMachine;
            machine.AnimationPoseProxy.Pose.Shadowed();
        }

        //[Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void Init()
        {

        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(GMacrossAnimationComponent), nameof(Tick));
        public override Profiler.TimeScope GetTickTimeScope()
        {
            return ScopeTick;
        }
        List<ILogicTick> mTickComponents = new List<ILogicTick>();
        public void AddTickComponent(ILogicTick tickCom)
        {
            if (!mTickComponents.Contains(tickCom))
            {
                mTickComponents.Add(tickCom);
            }
        }
        public void RemoveTickComponent(ILogicTick tickCom)
        {
            if (mTickComponents.Contains(tickCom))
            {
                mTickComponents.Remove(tickCom);
            }
        }
        Vector3 lastRootMotion = Vector3.Zero;
        Vector3 lastPos = Vector3.Zero;
        bool startRootMotion = false;
        public override void Tick(GPlacementComponent placement)
        {
            OnAnimationUpdate(placement);
            for (int i = 0; i < mTickComponents.Count; ++i)
            {
                mTickComponents[i].TickLogic();
            }
            var phy = Host.GetComponent<Bricks.PhysicsCore.GPhyControllerComponent>();
            if (phy != null)
            {
                var displayment = mAnimationPoseProxy.Pose.ExtractRootMotion();
                if (displayment != Vector3.Zero && startRootMotion == false)
                {
                    lastPos = phy.Controller.FootPosition;
                    startRootMotion = true;
                }
                if (startRootMotion)
                {
                    if (displayment == Vector3.Zero)
                    {
                        startRootMotion = false;
                        lastPos = Vector3.Zero;
                    }
                }
                if (startRootMotion)
                {
                    displayment.Y = 0;
                    var dis = Vector3.TransformCoordinate((displayment), Matrix.RotationQuaternion(placement.Rotation));
                    dis = lastPos + dis;
                    var delta = dis - phy.Controller.FootPosition;
                    phy.OnTryMove(placement, ref delta, 0.001f, CEngine.Instance.EngineElapseTimeSecond);
                    //phy.Controller.FootPosition = lastPos + dis;
                }
            }
            base.Tick(placement);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnAnimationUpdate(GPlacementComponent placemen)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float GetAnimationSequenceRemainingTime(string stateMachineName, string stateName, string animName)
        {
            for (int i = 0; i < mTickComponents.Count; ++i)
            {
                var tickCom = mTickComponents[i];
                if (tickCom is AnimationStateMachine)
                {
                    var machine = tickCom as AnimationStateMachine;
                    if (machine.Name.Name == stateMachineName.ToLower())
                    {
                        if (machine.CurrentState.Name.Name == stateName.ToLower())
                        {
                            for (int j = 0; j < machine.CurrentState.TickComponents.Count; ++j)
                            {
                                var stateTickCom = machine.CurrentState.TickComponents[j];
                                if (stateTickCom is CGfxAnimationSequence)
                                {
                                    var anim = stateTickCom as CGfxAnimationSequence;
                                    if (anim.Name.PureName().ToLower() == animName.ToLower())
                                    {
                                        return anim.RemainingTime;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }
        public bool PlayAnimationSequence(RName animation)
        {
            var anim = new CGfxAnimationSequence(animation);
            anim.IsLoop = false;
            anim.AnimationPoseProxy = AnimationPoseProxy;
            anim.OnBeginPlay += Anim_OnBeginPlay;
            anim.OnEndPlay += Anim_OnEndPlay;
            for (int i = 0; i < mTickComponents.Count; ++i)
            {
                mTickComponents[i].EnableTick = false;
            }
            AddTickComponent(anim);
            return true;
        }
        private void Anim_OnBeginPlay(object sender, EventArgs e)
        {

        }
        private void Anim_OnEndPlay(object sender, EventArgs e)
        {
            var anim = sender as CGfxAnimationSequence;
            anim.OnBeginPlay -= Anim_OnBeginPlay;
            anim.OnEndPlay -= Anim_OnEndPlay;
            RemoveTickComponent(anim);
            for (int i = 0; i < mTickComponents.Count; ++i)
            {
                mTickComponents[i].EnableTick = true;
            }
        }
        public override void OnReInitComponent()
        {
            var meshComp = mHost.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            if (meshComp != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                try
                {
                    Init();
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Error, "Macross", $"GMacrossAnimationComponent.Init Exception");
                    Profiler.Log.WriteException(ex);
                }
                skinModifier.AnimationPoseProxy = AnimationPoseProxy;
            }
            var mutiMeshComp = mHost.GetComponent<EngineNS.GamePlay.Component.GMutiMeshComponent>();
            if (mutiMeshComp != null)
            {
                using (var it = mutiMeshComp.Meshes.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var subMesh = it.Current.Value;
                        var skinModifier = subMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                        SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                        break;
                    }
                };
                Init();
                using (var it = mutiMeshComp.Meshes.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var subMesh = it.Current.Value;
                        var skinModifier = subMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                        skinModifier.AnimationPoseProxy = AnimationPoseProxy;
                    }
                }
            }
            base.OnReInitComponent();
        }
        public override void OnAdded()
        {
            OnReInitComponent();
            base.OnAdded();
        }
        protected override void OnAddedComponent(GComponent addedComponent)
        {
            if (addedComponent is GMeshComponent)
            {
                var meshComp = addedComponent as GMeshComponent;
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                Init();
                skinModifier.AnimationPoseProxy = AnimationPoseProxy;
            }
            else if (addedComponent is GMutiMeshComponent)
            {
                var mutiMeshComp = addedComponent as GMutiMeshComponent;
                var it = mutiMeshComp.Meshes.GetEnumerator();
                while (it.MoveNext())
                {
                    var subMesh = it.Current.Value;
                    var skinModifier = subMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                    break;
                }
                it.Dispose();
                Init();
                it = mutiMeshComp.Meshes.GetEnumerator();
                while (it.MoveNext())
                {
                    var subMesh = it.Current.Value;
                    var skinModifier = subMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    skinModifier.AnimationPoseProxy = AnimationPoseProxy;
                }
                it.Dispose();
            }
        }
        public override void OnAddedScene()
        {

        }
        public override void OnRemoveScene()
        {

        }
    }
}
