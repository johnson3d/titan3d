using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using EngineNS.Bricks.Animation.PoseControl.Blend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAnimationSelectPoseComponentInitializer), "动作选择组件", "Animation", "SelectPoseComponent")]
    public class GAnimationSelectPoseComponent : SkeletonAnimationComponent
    {
        [Rtti.MetaClass]
        public class GAnimationSelectPoseComponentInitializer : GComponentInitializer
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
                var initializer = this.Initializer as GAnimationSelectPoseComponentInitializer;
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
        RName mFirstAnimationName = RName.EmptyName;
        [DisplayName("FirstAnimation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName FirstAnimationName
        {
            get => mFirstAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mFirstAnimationName = value;
                var initializer = this.Initializer as GAnimationSelectPoseComponentInitializer;
                initializer.AnimationName = value;
                FirstClip = AnimationClip.CreateSync(value);
                mFirstPose = Pose.Clone();
                FirstBindingPose = FirstClip.Bind(mFirstPose);
                mSelectPose.Add(0, new PoseItemForBlend(mFirstPose, 0.1f));
            }
        }
        CGfxSkeletonPose mFirstPose;
        [Browsable(false)]
        public AnimationClip FirstClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose FirstBindingPose { get; set; }




        RName mSecondAnimationName = RName.EmptyName;
        [DisplayName("SecondAnimation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName SecondAnimationName
        {
            get => mSecondAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mSecondAnimationName = value;
                var initializer = this.Initializer as GAnimationSelectPoseComponentInitializer;
                initializer.AnimationName = value;
                SecondClip = AnimationClip.CreateSync(value);
                mSecondPose = Pose.Clone();
                SecondBindingPose = SecondClip.Bind(mSecondPose);
                mSelectPose.Add(1, new PoseItemForBlend(mSecondPose, 0.1f));
            }
        }
        CGfxSkeletonPose mSecondPose;
        [Browsable(false)]
        public AnimationClip SecondClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose SecondBindingPose { get; set; }



        RName mThirdAnimationName = RName.EmptyName;
        [DisplayName("ThirdAnimation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName ThirdAnimationName
        {
            get => mThirdAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mThirdAnimationName = value;
                var initializer = this.Initializer as GAnimationSelectPoseComponentInitializer;
                initializer.AnimationName = value;
                ThirdClip = AnimationClip.CreateSync(value);
                mThirdPose = Pose.Clone();
                ThirdBindingPose = ThirdClip.Bind(mThirdPose);
                mSelectPose.Add(2, new PoseItemForBlend(mThirdPose, 0.1f));
            }
        }
        CGfxSkeletonPose mThirdPose;
        [Browsable(false)]
        public AnimationClip ThirdClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose ThirdBindingPose { get; set; }
        public GAnimationSelectPoseComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GAnimationSelectPoseComponentInitializer();
        }
        public GAnimationSelectPoseComponent(RName skeletonAsset)
        {
            var initializer = new GAnimationSelectPoseComponentInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GAnimationSelectPoseComponentInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            LinkSkinModifier();
            ThirdAnimationName = init.AnimationName;

            mSelectPose.EvaluateSelectedFunc = () => { return Index; };
            return true;
        }
        ~GAnimationSelectPoseComponent()
        {

        }
        public enum PoseSwitch
        {
            Base,
            Ref,
            Add,
            Default,
        }
        public int Index { get; set; } = 0;
        Bricks.Animation.PoseControl.Blend.SelectPoseByInt mSelectPose = new Bricks.Animation.PoseControl.Blend.SelectPoseByInt();
        public PoseSwitch Switches { get; set; } = PoseSwitch.Default;
        public override void Tick(GPlacementComponent placement)
        {
            ThirdClip?.Tick();
            SecondClip?.Tick();
            FirstClip?.Tick();
            ThirdClip?.TickNofity(this);
            SecondClip?.TickNofity(this);
            FirstClip?.TickNofity(this);
            mSelectPose.Tick();
            Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(Pose, mSelectPose.OutPose);
            //if (SecondClip != null && ThirdClip != null && FirstClip != null)
            //{
            //    switch (Switches)
            //    {
            //        case PoseSwitch.Base:
            //            {
            //                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(AnimationPoseProxy.Pose, mFirstPose);
            //                //AnimationPoseProxy.Pose.BlendWithTargetPose(mBasePose, 1);
            //            }
            //            break;
            //        case PoseSwitch.Ref:
            //            {
            //                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(AnimationPoseProxy.Pose, mSecondPose);
            //                //AnimationPoseProxy.Pose.BlendWithTargetPose(mAdditiveRefrencePose, 1);
            //            }
            //            break;
            //        case PoseSwitch.Add:
            //            {
            //                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(AnimationPoseProxy.Pose, mThirdPose);
            //                //AnimationPoseProxy.Pose.BlendWithTargetPose(mAdditivePose, 1);
            //            }
            //            break;
            //        case PoseSwitch.Default:
            //            {
            //                AdditivePose.Tick();
            //                //Bricks.Animation.Runtime.CGfxAnimationRuntime.AdditivePose(AnimationPoseProxy.Pose, mBasePose, pose, 1);
            //                Bricks.Animation.Runtime.CGfxAnimationRuntime.CopyPose(AnimationPoseProxy.Pose, AdditivePose.OutPose);
            //            }
            //            break;
            //    }


            //}
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
                    mSelectPose.OutPose = Pose.Clone();
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
                        mSelectPose.OutPose = Pose.Clone();
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
                    mSelectPose.OutPose = Pose.Clone();
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
                        mSelectPose.OutPose = Pose.Clone();
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
