using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAnimationBlendSpaceComponentInitializer), "二维动作混合组件", "Animation", "BlendSpace2DComponent")]
    public class GAnimationBlendSpaceComponent : SkeletonAnimationComponent
    {
        [Rtti.MetaClass]
        public class GAnimationBlendSpaceComponentInitializer : GComponentInitializer
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
                var initializer = this.Initializer as GAnimationBlendSpaceComponentInitializer;
                if (initializer != null)
                    initializer.SkeletonAssetName = value;
                //InitializeAnimationPose();
            }
        }
        BlendSpace2D mAnimation = null;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public BlendSpace2D Animation
        {
            set
            {
                mAnimation = value;
                mAnimation.Pose = Pose;
            }
            get { return mAnimation; }
        }
        public Bricks.Animation.Binding.AnimationBindingPose BindingPose { get; set; }
        RName mAnimationName = RName.EmptyName;
        [DisplayName("Animation")]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationBlendSpace)]
        public RName AnimationName
        {
            get => mAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mAnimationName = value;
                var initializer = this.Initializer as GAnimationBlendSpaceComponentInitializer;
                initializer.AnimationName = value;
                var anim = BlendSpace2D.CreateSync(value);
                if (anim != null)
                {
                    Animation = anim;
                    //BindingPose = Clip.Generate(mHost);
                }
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Input
        {
            get
            {
                if (Animation == null)
                    return Vector3.Zero;
                return Animation.Input;
            }
            set
            {
                if (Animation == null)
                    return;
                Animation.Input = value;
            }

        }

        public GAnimationBlendSpaceComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GAnimationBlendSpaceComponentInitializer();
        }
        public GAnimationBlendSpaceComponent(RName skeletonAsset)
        {
            var initializer = new GAnimationBlendSpaceComponentInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GAnimationBlendSpaceComponentInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            LinkSkinModifier();
            AnimationName = init.AnimationName;
            return true;
        }
        void InitializeAnimationPose()
        {
            if (Pose == null)
                Pose = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, mSkeletonAssetName).CreateSkeletonPose();
        }
        ~GAnimationBlendSpaceComponent()
        {

        }
        public override void Tick(GPlacementComponent placement)
        {
            Animation?.Tick();
            Animation?.TickNofity(this);
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
                SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                Pose = skinModifier.AnimationPose;
            }
            var mutiMeshComp = actor.GetComponent<GMutiMeshComponent>();
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    Pose = skinModifier.AnimationPose;
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
                Pose = skinModifier.AnimationPose;
            }
            var mutiMeshComp = compContainer as GMutiMeshComponent;
            if (mutiMeshComp != null)
            {
                foreach (var subMesh in mutiMeshComp.Meshes)
                {
                    var skinModifier = subMesh.Value.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
                    Pose = skinModifier.AnimationPose;
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

