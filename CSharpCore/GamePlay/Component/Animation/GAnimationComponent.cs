using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Pose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GAnimationComponentInitializer), "动作组件", "Animation", "AnimationComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/animsequence_64x.txpic", RName.enRNameType.Editor)]
    public class GAnimationComponent : SkeletonAnimationComponent
    {
        [Rtti.MetaClass]
        public class GAnimationComponentInitializer : GComponentInitializer
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
                var initializer = this.Initializer as GAnimationComponentInitializer;
                if (initializer != null)
                    initializer.SkeletonAssetName = value;
                //InitializeAnimationPose();
            }
        }
        AnimationClip mClip = null;
        [Browsable(false)]
        public AnimationClip Animation { get => mClip; set => mClip = value; }
        [Browsable(false)]
        public Bricks.Animation.Binding.AnimationBindingPose BindingPose { get; set; }
        public float PlayRate
        {
            get
            {
                if (mClip != null)
                    return mClip.PlayRate;
                return 0;
            }
            set
            {
                if (mClip != null)
                    mClip.PlayRate = value;
            }
        }
        public bool Pause
        {
            get
            {
                if (mClip != null)
                    return mClip.Pause;
                return false;
            }
            set
            {
                if (mClip != null)
                    mClip.Pause = value;
            }
        }
        RName mAnimationName = RName.EmptyName;
        [DisplayName("Animation")]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName AnimationName
        {
            get => mAnimationName;
            set
            {
                if (value == RName.EmptyName || value == null)
                    return;
                mAnimationName = value;
                var initializer = this.Initializer as GAnimationComponentInitializer;
                initializer.AnimationName = value;
                Animation = AnimationClip.CreateSync(value);
                if(Animation!= null)
                    BindingPose = Animation.Bind(Pose);
            }
        }
        public GAnimationComponent()
        {
            OnlyForGame = false;
            this.Initializer = new GAnimationComponentInitializer();
        }
        public GAnimationComponent(RName skeletonAsset)
        {
            var initializer = new GAnimationComponentInitializer();
            SkeletonAssetName = skeletonAsset;
            initializer.SkeletonAssetName = skeletonAsset;
            this.Initializer = initializer;
        }
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;
            await base.SetInitializer(rc, host, hostContainer, v);
            var init = v as GAnimationComponentInitializer;
            if (init == null)
                return false;
            SkeletonAssetName = init.SkeletonAssetName;
            LinkSkinModifier();
            AnimationName = init.AnimationName;
            return true;
        }
        ~GAnimationComponent()
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
                if (skinModifier != null)
                {
                    SkeletonAssetName = RName.GetRName(skinModifier.SkeletonAsset);
                    Pose = skinModifier.AnimationPose;
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
