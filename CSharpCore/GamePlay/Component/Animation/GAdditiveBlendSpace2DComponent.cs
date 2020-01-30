using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAdditiveBlendSpace2DComponentInitializer), "二维附加动作混合组件", "Animation", "AdditiveBlendSpace2DComponent")]
    public class GAdditiveBlendSpace2DComponent : SkeletonAnimationComponent
    {
        [Rtti.MetaClass]
        public class GAdditiveBlendSpace2DComponentInitializer : GComponentInitializer
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
                var initializer = this.Initializer as GAdditiveBlendSpace2DComponentInitializer;
                if (initializer != null)
                    initializer.SkeletonAssetName = value;
                //InitializeAnimationPose();
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
                BaseClip = AnimationClip.CreateSync(value);
                if (BaseClip == null)
                    return;
                mBasePose = Pose.Clone();
                BaseBindingPose = BaseClip.Bind(mBasePose);
                if (mAnimation != null)
                    mAnimation.BasePose = mBasePose;
            }
        }
        CGfxSkeletonPose mBasePose;
        [Browsable(false)]
        public AnimationClip BaseClip { get; set; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose BaseBindingPose { get; set; }
        AdditiveBlendSpace2D mAnimation = null;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public AdditiveBlendSpace2D Animation
        {
            set
            {
                mAnimation = value;
                mAnimation.Pose = Pose;
                if (mBasePose != null)
                    mAnimation.BasePose = mBasePose;
            }
            get { return mAnimation; }
        }
        public Bricks.Animation.Binding.AnimationBindingPose BindingPose { get; set; }
        RName mAnimationName = RName.EmptyName;
        [DisplayName("Animation")]
        [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationAdditiveBlendSpace1D)]
        public RName AnimationName
        {
            get => mAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mAnimationName = value;
                var initializer = this.Initializer as GAdditiveBlendSpace2DComponentInitializer;
                initializer.AnimationName = value;
                var anim = AdditiveBlendSpace2D.CreateSync(value);
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

        public GAdditiveBlendSpace2DComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GAdditiveBlendSpace2DComponentInitializer();
        }
        public GAdditiveBlendSpace2DComponent(RName skeletonAsset)
        {
            var initializer = new GAdditiveBlendSpace2DComponentInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GAdditiveBlendSpace2DComponentInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            LinkSkinModifier();
            AnimationName = init.AnimationName;
            return true;
        }
        ~GAdditiveBlendSpace2DComponent()
        {

        }
        public override void Tick(GPlacementComponent placement)
        {
            BaseClip?.Tick();
            Animation?.Tick();
            BaseClip?.TickNofity(this);
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

