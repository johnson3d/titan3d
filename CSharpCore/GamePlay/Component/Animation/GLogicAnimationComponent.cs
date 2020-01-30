using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.Bricks.Animation.AnimStateMachine;
using EngineNS.GamePlay.Actor;
using EngineNS.Bricks.FSM.TFSM;
using EngineNS.Bricks.Animation.Pose;

namespace EngineNS.GamePlay.Component
{
    [Rtti.MetaClass]
    public class GLogicAnimationComponentInitializer : GComponent.GComponentInitializer
    {
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GLogicAnimationComponentInitializer), "动作状态机组件", "Animation", "LogicAnimationComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/animmacross_64x.txpic", RName.enRNameType.Editor)]
    public class GLogicAnimationComponent : SkeletonAnimationComponent
    {
        public bool PostProcessing
        {
            get
            {
                if (McLogicAnimation == null)
                    return false;
                return McLogicAnimation.PostProcessing;
            }
            set
            {
                if (McLogicAnimation != null)
                    McLogicAnimation.PostProcessing = value;
            }
        }
        public float PlayRate
        {
            get
            {

                return 0;
            }
            set
            {

            }
        }
        public bool Pause
        {
            get
            {

                return false;
            }
            set
            {
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McLogicAnimationComponent))]
        public override RName ComponentMacross
        {
            get
            {
                return base.ComponentMacross;
            }
            set
            {
                base.ComponentMacross = value;
                if (McLogicAnimation != null)
                {
                    McLogicAnimation.HostComp = this;
                    if(CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                        McLogicAnimation?.Construct();
                }
            }
        }
        McLogicAnimationComponent McLogicAnimation
        {
            get
            {
                return mMcCompGetter?.CastGet<McLogicAnimationComponent>(false);
            }
        }
        [Browsable(false)]
        public GLogicAnimationComponentInitializer LogicAnimationComponentInitializer
        {
            get
            {
                var v = Initializer as GLogicAnimationComponentInitializer;
                if (v != null)
                    return v;
                return null;
            }
        }
        public GLogicAnimationComponent()
        {
            Initializer = new GLogicAnimationComponentInitializer();
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var mesh = host.GetComponent<GMeshComponent>();
            if (mesh == null)
                mesh = hostContainer as GMeshComponent;
            if (mesh != null)
            {
                var skinModifier = mesh.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                Pose = skinModifier.AnimationPose;
            }
            else
            {
                return false;
            }
            return await base.SetInitializer(rc, host, hostContainer, v);
        }
        public override Profiler.TimeScope GetTickTimeScope()
        {
            return ScopeTick;
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(GLogicAnimationComponent), nameof(Tick));
        public override void Tick(GPlacementComponent placement)
        {
            McLogicAnimation?.Tick(placement);
        }
        public override void OnActorLoaded(GActor actor)
        {
            base.OnActorLoaded(actor);
            McLogicAnimation?.Construct();
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter)]
    public class McLogicAnimationComponent : GamePlay.Component.McComponent
    {
        Dictionary<string, LogicAnimationStateMachine> mLogicAnimationStateMachineDic = new Dictionary<string, LogicAnimationStateMachine>();
        public McLogicAnimationComponent()
        {
        }
        public override void OnNewMacross()
        {
            
        }
        public override void OnActorLoaded(GComponent comp)
        {
            base.OnActorLoaded(comp);
            //Construct();
        }
        public void Construct()
        {
            var host = HostComp as GLogicAnimationComponent;
            mFinalPose = host.Pose;
            ConstructLAGraph();
            InitializePostProcessFunc?.Invoke(mPostProcessInPose);
        }
        public LogicAnimationStateMachine CreateLAStateMachine(string name)
        {
            if (mLogicAnimationStateMachineDic.ContainsKey(name))
                return mLogicAnimationStateMachineDic[name];
            var lasm = new LogicAnimationStateMachine("LATFSM_" + name);

            lasm.Pose = mFinalPose.Clone();
            mStateMachinePose = mFinalPose.Clone();
            mPostProcessInPose = mFinalPose.Clone();
            mTempPose = mFinalPose.Clone();
            mLogicAnimationStateMachineDic.Add(name, lasm);
            return lasm;
        }
        public LogicAnimationState CreateLAStateWithAnimClip(string name, RName animFilePath, string stateMachineName)
        {
            var laState = new LogicAnimationState(mLogicAnimationStateMachineDic[stateMachineName], name);
            laState.Pose = mFinalPose.Clone();
            laState.Animation = animFilePath;
            return laState;
        }
        public LogicAnimationState CreateLAStateWithAnimClip(string name, float duration, string animFilePath, string stateMachineName)
        {
            var laState = new LogicAnimationState(mLogicAnimationStateMachineDic[stateMachineName], name);
            laState.Duration = duration;
            laState.Pose = mFinalPose.Clone();
            laState.Animation = RName.GetRName(animFilePath);
            return laState;
        }
        public LATimedTransitionFunction CreateLATimedTransitionFunction(float start, float duration)
        {
            var lat = new LATimedTransitionFunction();
            lat.Start = (long)(start * 1000);
            lat.Duration = (long)(duration * 1000);
            return lat;
        }
        public LATimedTransitionFunction CreateLATimedTransitionFunction(float start, float duration, LogicAnimationState from, LogicAnimationState to)
        {
            var lat = new LATimedTransitionFunction();
            lat.Start = (long)(start * 1000);
            lat.Duration = (long)(duration * 1000);
            lat.From = from;
            lat.To = to;
            return lat;
        }
        public LATimedTransitionFunction CreateLATimedTransitionFunction(float start, float duration, LogicAnimationState from, LogicAnimationState to, bool performanceFirst, float fadeTime)
        {
            var lat = new LATimedTransitionFunction();
            lat.Start = (long)(start * 1000);
            lat.Duration = (long)(duration * 1000);
            lat.From = from;
            lat.To = to;
            lat.AnimCrossfading = new AnimCrossfading(performanceFirst,fadeTime);
            return lat;
        }
        public void SetDefaultState(LogicAnimationState state, string stateMachineName)
        {
            mLogicAnimationStateMachineDic[stateMachineName].SetCurrentStateImmediately(state);
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void ConstructLAGraph()
        {

        }
        CGfxSkeletonPose mFinalPose = null;
        CGfxSkeletonPose mStateMachinePose = null;
        CGfxSkeletonPose mPostProcessInPose = null;
        public bool PostProcessing { get; set; } = true;
        [Browsable(false)]
        public Action<CGfxSkeletonPose> InitializePostProcessFunc { get; set; } = null;
        CGfxSkeletonPose mTempPose = null;
        void ProcessStateMachines()
        {
            if (mStateMachinePose == null)
                return;
            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mTempPose, mStateMachinePose);
            using (var it = mLogicAnimationStateMachineDic.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    it.Current.Value.Tick();
                }
            }
            using (var it = mLogicAnimationStateMachineDic.GetEnumerator())
            {
                AnimLayerType lastLayerType = AnimLayerType.Default;
                while (it.MoveNext())
                {
                    if (it.Current.Value.LayerType == AnimLayerType.Additive)
                    {
                        if (lastLayerType == AnimLayerType.Default)
                        {
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(mTempPose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(mTempPose, mTempPose, it.Current.Value.Pose, 1);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(mTempPose);
                        }
                        else
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mTempPose, it.Current.Value.Pose);
                    }
                    if (it.Current.Value.LayerType == AnimLayerType.Default)
                    {
                        if (lastLayerType == AnimLayerType.Additive)
                        {
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(mTempPose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(mTempPose, mTempPose, it.Current.Value.Pose, 1);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(mTempPose);
                        }
                        else
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mTempPose, it.Current.Value.Pose);
                    }
                    lastLayerType = it.Current.Value.LayerType;
                }
            }
            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mStateMachinePose, mTempPose);
        }
        void ProcessNotify(GComponent component)
        {
            using (var it = mLogicAnimationStateMachineDic.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    it.Current.Value.TickNotify(component);
                }
            }
        }
        Bricks.Animation.BlendTree.IBlendTree mPostProcessBlendTreeRoot = null;
        [Browsable(false)]
        public Bricks.Animation.BlendTree.IBlendTree PostProcessBlendTreeRoot
        {
            get => mPostProcessBlendTreeRoot;
            set
            {
                mPostProcessBlendTreeRoot = value;
                mPostProcessBlendTreeRoot?.InitializePose(mPostProcessInPose);
            }
        }
        public void Tick(GPlacementComponent placement)
        {
            ProcessStateMachines();
            if (PostProcessing && PostProcessBlendTreeRoot != null)
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mPostProcessInPose, mStateMachinePose);
                PostProcessBlendTreeRoot.Evaluate(0);
                //PostProcess(ref mPostProcessInPose, ref mPostProcessOutPose);
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mFinalPose, PostProcessBlendTreeRoot.OutPose);
            }
            else
            {
                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(mFinalPose, mStateMachinePose);
            }
            ProcessNotify(this.HostComp);
            OnTick(HostComp);
        }
    }
}
