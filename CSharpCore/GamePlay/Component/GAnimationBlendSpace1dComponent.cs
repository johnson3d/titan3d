using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAnimationBlendSpace1DComponentInitializer), "动画组件", "Animation Component")]
    public class GAnimationBlendSpace1DComponent : GComponent
    {
        [Rtti.MetaClass]
        public class GAnimationBlendSpace1DComponentInitializer : GComponentInitializer
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
                var initializer = this.Initializer as GAnimationBlendSpace1DComponentInitializer;
                if (initializer != null)
                    initializer.SkeletonAssetName = value;
                //InitializeAnimationPose();
            }
        }
        protected CGfxAnimationPoseProxy mAnimationPoseProxy = new CGfxAnimationPoseProxy();
        [Browsable(false)]
        public CGfxAnimationPoseProxy AnimationPoseProxy { get => mAnimationPoseProxy; set => mAnimationPoseProxy = value; }
        CGfxAnimationBlendSpace1D mAnimation = null;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public CGfxAnimationBlendSpace1D Animation
        {
            set
            {
                mAnimation = value;
                mAnimation.AnimationPoseProxy = mAnimationPoseProxy;
            }
            get { return mAnimation; }
        }
        AnimationClip mClip = null;
        public AnimationClip Clip { get => mClip; set => mClip = value; }
        public Bricks.Animation.Binding.AnimationBindingPose BindingPose { get; set; }
        RName mAnimationName = RName.EmptyName;
        [DisplayName("Animation")]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.AnimatioinBlendSpace1D)]
        public RName AnimationName
        {
            get => mAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mAnimationName = value;
                var initializer = this.Initializer as GAnimationBlendSpace1DComponentInitializer;
                initializer.AnimationName = value;
                var anim = new CGfxAnimationBlendSpace1D();
                if (anim.Init(EngineNS.CEngine.Instance.RenderContext, value))
                {
                    Animation = anim;
                    //BindingPose = Clip.Generate(mHost);
                }
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Input
        {
            get
            {
                if (Animation == null)
                    return 0;
                return Animation.Input.X;
            }
            set
            {
                if (Animation == null)
                    return;
                Vector3 v = Vector3.Zero;
                v.X = value;
                Animation.Input = v;
            }

        }
     
        public GAnimationBlendSpace1DComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GAnimationBlendSpace1DComponentInitializer();
        }
        public GAnimationBlendSpace1DComponent(RName skeletonAsset)
        {
            var initializer = new GAnimationBlendSpace1DComponentInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, Actor.GActor host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GAnimationBlendSpace1DComponentInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            LinkSkinModifier();
            AnimationName = init.AnimationName;
            return true;
        }
        void InitializeAnimationPose()
        {
            if (mAnimationPoseProxy.IsNullPose)
                mAnimationPoseProxy.Pose = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, mSkeletonAssetName).BoneTab.Clone();
        }
        ~GAnimationBlendSpace1DComponent()
        {

        }
        public override void Tick(GPlacementComponent placement)
        {
            Animation?.TickLogic();
            if (BindingPose != null)
            {
                //Clip.Evaluate(EngineNS.CEngine.Instance.EngineElapseTimeSecond, BindingPose);
                //BindingPose.Flush();
                Clip?.Tick();
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
            if (mHostContainer != mHost)
            {
                LinkSkinModifier(mHostContainer);
            }
            else
            {
                LinkSkinModifier(mHost);
            }
            IsSkinModifierLinked = true;
        }
        void LinkSkinModifier(Actor.GActor actor)
        {
            var meshComp = actor.GetComponent<GMeshComponent>();
            if (meshComp != null && meshComp.SceneMesh != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                AnimationPoseProxy = skinModifier.AnimationPoseProxy;
            }
            var mutiMeshComp = actor.GetComponent<GMutiMeshComponent>();
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    AnimationPoseProxy = skinModifier.AnimationPoseProxy;
                }
            }

        }
        void LinkSkinModifier(IComponentContainer compContainer)
        {
            var meshComp = compContainer as GMeshComponent;
            if (meshComp != null && meshComp.SceneMesh != null)
            {
                var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                AnimationPoseProxy = skinModifier.AnimationPoseProxy;
            }
            var mutiMeshComp = compContainer as GMutiMeshComponent;
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    AnimationPoseProxy = skinModifier.AnimationPoseProxy;
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

