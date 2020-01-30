using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAnimationAdditiveComponentInitializer), "附加动作组件", "Animation", "AdditiveComponent")]
    public class GAnimationAdditiveComponent : SkeletonAnimationComponent
    {
        [Rtti.MetaClass]
        public class GAnimationAdditiveComponentInitializer : GComponentInitializer
        {
            [Rtti.MetaData]
            public RName SkeletonAssetName
            {
                get; set;
            }
            [Rtti.MetaData]
            public RName AnimationName
            {
                get; set;
            }
        }
        protected RName mSkeletonAssetName = RName.EmptyName;
        [Browsable(false)]
        public RName SkeletonAssetName
        {
            get => mSkeletonAssetName;
            set
            {
                if (value == null || value == RName.EmptyName)
                    return;
                mSkeletonAssetName = value;
                var initializer = this.Initializer as GAnimationAdditiveComponentInitializer;
                if (initializer != null)
                    initializer.SkeletonAssetName = value;
                //InitializeAnimationPose();
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
        RName mBaseAnimationName = RName.EmptyName;
        [DisplayName("BaseAnimation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName BaseAnimationName
        {
            get => mBaseAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mBaseAnimationName = value;
                var initializer = this.Initializer as GAnimationAdditiveComponentInitializer;
                initializer.AnimationName = value;
                BaseClip = AnimationClip.CreateSync(value);
                mBasePose = Pose.Clone();
                BaseBindingPose = BaseClip.Bind(mBasePose);
                AdditivePose.BasePose = mBasePose;
            }
        }
        CGfxSkeletonPose mBasePose;
        [Browsable(false)]
        public AnimationClip BaseClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose BaseBindingPose { get; set; }




        RName mAdditiveRefrenceAnimationName = RName.EmptyName;
        [DisplayName("AdditiveRefrenceAnimation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName AdditiveRefrenceAnimationName
        {
            get => mAdditiveRefrenceAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mAdditiveRefrenceAnimationName = value;
                var initializer = this.Initializer as GAnimationAdditiveComponentInitializer;
                initializer.AnimationName = value;
                AdditiveReferenceClip = AnimationClip.CreateSync(value);
                mAdditiveReferencePose = Pose.Clone();
                AdditiveReferenceBindingPose = AdditiveReferenceClip.Bind(mAdditiveReferencePose);
                AdditivePose.ReferencePose = mAdditiveReferencePose;
            }
        }
        CGfxSkeletonPose mAdditiveReferencePose;
        [Browsable(false)]
        public AnimationClip AdditiveReferenceClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose AdditiveReferenceBindingPose { get; set; }



        RName mAdditiveAnimationName = RName.EmptyName;
        [DisplayName("AdditiveAnimation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName AdditiveAnimationName
        {
            get => mAdditiveAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mAdditiveAnimationName = value;
                var initializer = this.Initializer as GAnimationAdditiveComponentInitializer;
                initializer.AnimationName = value;
                AdditiveClip = AnimationClip.CreateSync(value);
                mAdditivePose = Pose.Clone();
                AdditiveBindingPose = AdditiveClip.Bind(mAdditivePose);
                AdditivePose.AddPose = mAdditivePose;
            }
        }
        CGfxSkeletonPose mAdditivePose;
        [Browsable(false)]
        public AnimationClip AdditiveClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose AdditiveBindingPose { get; set; }
        public GAnimationAdditiveComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GAnimationAdditiveComponentInitializer();
        }
        public GAnimationAdditiveComponent(RName skeletonAsset)
        {
            var initializer = new GAnimationAdditiveComponentInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GAnimationAdditiveComponentInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            LinkSkinModifier();
            AdditiveAnimationName = init.AnimationName;
            return true;
        }
        ~GAnimationAdditiveComponent()
        {

        }
        public enum PoseSwitch
        {
            Base,
            Ref,
            Add,
            Default,
        }
        Bricks.Animation.PoseControl.Blend.AdditivePose AdditivePose = new Bricks.Animation.PoseControl.Blend.AdditivePose();
        public PoseSwitch Switches { get; set; } = PoseSwitch.Default;
        public override void Tick(GPlacementComponent placement)
        {
            if (AdditiveReferenceClip != null && AdditiveClip != null)
            {
                if (mBasePose == null)
                    mBasePose = Pose.Clone();
                AdditiveClip?.Tick();
                AdditiveReferenceClip?.Tick();
                BaseClip?.Tick();
                switch (Switches)
                {
                    case PoseSwitch.Base:
                        {
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, mBasePose);
                        }
                        break;
                    case PoseSwitch.Ref:
                        {
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, mAdditiveReferencePose);
                        }
                        break;
                    case PoseSwitch.Add:
                        {
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(mAdditivePose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(mAdditivePose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, mAdditivePose);
                        }
                        break;
                    case PoseSwitch.Default:
                        {
                            var addPose = mAdditivePose.Clone();
                            var refPose = mAdditiveReferencePose.Clone();
                            var basePose = mBasePose.Clone();
                            var tempPose = mBasePose.Clone();
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(addPose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(refPose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.MinusPose(tempPose, refPose, addPose);
                            //Bricks.Animation.Runtime.CGfxAnimationRuntime.ZeroTransition(tempPose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToMeshSpace(basePose);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.AddPose(Pose, basePose, tempPose, 1);
                            Bricks.Animation.Runtime.CGfxAnimationRuntime.ConvertRotationToLocalSpace(Pose);
                        }
                        break;
                }
                AdditiveClip?.TickNofity(this);
                AdditiveReferenceClip?.TickNofity(this);
                BaseClip?.TickNofity(this);

            }
            base.Tick(placement);
        }
        public override void OnAdded()
        {
            LinkSkinModifier();
            base.OnAdded();
        }
        bool IsSkinModifierLinked = false;
        void LinkSkinModifier()
        {
            if (IsSkinModifierLinked)
                return;
            if (mHostContainer != Host)
            {
                LinkSkinModifier(mHostContainer);
            }
            else
            {
                LinkSkinModifier(Host);
            }
            IsSkinModifierLinked = true;
        }
        void LinkSkinModifier(Actor.GActor actor)
        {
            var meshComp = actor.GetComponent<GMeshComponent>();
            if (meshComp != null && meshComp.SceneMesh != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                if (skinModifier != null)
                {
                    SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                    Pose = skinModifier.AnimationPose;
                    AdditivePose.OutPose = Pose.Clone();
                }
            }
            var mutiMeshComp = actor.GetComponent<GMutiMeshComponent>();
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    if (skinModifier != null)
                    {
                        Pose = skinModifier.AnimationPose;
                        AdditivePose.OutPose = Pose.Clone();
                    }
                }
            }

        }
        void LinkSkinModifier(IComponentContainer compContainer)
        {
            var meshComp = compContainer as GMeshComponent;
            if (meshComp != null && meshComp.SceneMesh != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                if (skinModifier != null)
                {
                    SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                    Pose = skinModifier.AnimationPose;
                    AdditivePose.OutPose = Pose.Clone();
                }
            }
            var mutiMeshComp = compContainer as GMutiMeshComponent;
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    if (skinModifier != null)
                    {
                        Pose = skinModifier.AnimationPose;
                        AdditivePose.OutPose = Pose.Clone();
                    }
                }
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
