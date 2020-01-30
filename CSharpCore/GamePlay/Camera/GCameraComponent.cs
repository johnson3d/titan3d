using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay.Component;
using EngineNS.Graphics;
using EngineNS.GamePlay.Camera.ControlStratety;
using EngineNS.GamePlay.Camera.Modifier;

namespace EngineNS.GamePlay.Camera
{
    public enum ControlStrategyType
    {
        Follow,
        Free,
        FollowAndLookAt,
        Custom,
    }
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GCameraComponent.GCameraComponentInitializer), "摄像机组件", "Camera", "GCameraComponent")]
    [Editor.Editor_ComponentClassIconAttribute("icon/cameraactor_64x.txpic", RName.enRNameType.Editor)]
    public class GCameraComponent : GamePlay.Component.GComponent, IPlaceable
    {
        #region Initializer
        [Rtti.MetaClass]
        public class GCameraComponentInitializer : GamePlay.Component.GComponent.GComponentInitializer
        {
            [Rtti.MetaData]
            public ControlStrategyType ControlStrategyType { get; set; } = ControlStrategyType.Follow;
            [Rtti.MetaData]
            public RName StrategyName { get; set; } = RName.EmptyName;
            [Rtti.MetaData]
            public GPlacementComponent.GPlacementComponentInitializer PlacementInitializer { get; set; } = new GPlacementComponent.GPlacementComponentInitializer();
            [Rtti.MetaData]
            public bool InheritRotation { get; set; } = true;
            [Rtti.MetaData]
            public float Fov { get; set; } = MathHelper.V_PI / 2.25f;
        }
        [Browsable(false)]
        private GCameraComponentInitializer CameraComponentInitializer
        {
            get
            {
                return this.Initializer as GCameraComponentInitializer;
            }
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            var result = await base.SetInitializer(rc, host, hostContainer, v);
            await Placement.SetInitializer(rc, host, null, CameraComponentInitializer.PlacementInitializer);
            Placement.PlaceableHost = this;
            ChangingControlStartety();
            if (result == false)
                return false;
            return true;
        }
        #endregion Initializer
        #region Strategy
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(GCameraControlStrategy))]
        [Editor.Editor_PackData()]
        [Category("Control")]
        public virtual RName Strategy
        {
            get
            {
                if (this.Initializer == null)
                    return null;
                return CameraComponentInitializer.StrategyName;
            }
            set
            {
                if (Initializer == null)
                    return;
                if (CameraComponentInitializer.StrategyName == value && mStrategyGetter != null)
                    return;

                mStrategyGetter?.Get()?.OnUnsetMacross(this);
                if (mStrategyGetter != null)
                {
                    var getter = mStrategyGetter.Get(OnlyForGame);
                    if (getter != null)
                        getter.HostComp = null;
                }
                CameraComponentInitializer.StrategyName = value;
                mStrategyGetter = CEngine.Instance.MacrossDataManager.NewObjectGetter<GCameraControlStrategy>(value);
                if (mStrategyGetter != null)
                {
                    var getter = mStrategyGetter.Get(OnlyForGame);
                    if (getter != null)
                        getter.HostComp = this;
                }
                mStrategyGetter?.Get()?.OnSetMacross(this);
                OnPropertyChanged("Strategy");
            }
        }
        protected Macross.MacrossGetter<GCameraControlStrategy> mStrategyGetter;
        [Browsable(false)]
        public Macross.MacrossGetter<GCameraControlStrategy> StrategyGetter
        {
            get { return mStrategyGetter; }
        }
        #endregion Strategy
        #region IPlaceable
        public GPlacementComponent Placement
        {
            get;
            set;
        } = new GPlacementComponent();
        public void OnPlacementChanged(GPlacementComponent placement)
        {

        }
        #endregion IPlaceable
        #region CameraProperties
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 Direction
        {
            get
            {
                if (Camera != null)
                    return Camera.Direction;
                return Vector3.UnitX;
            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public Vector3 Position
        {
            get => Placement.Location;
            set => Placement.Location = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public virtual Vector3 Rotation
        {
            get
            {
                return Placement.Rotation.ToEuler() * MathHelper.Rad2Deg;
            }
            set
            {
                Placement.Rotation = Quaternion.FromEuler(value * MathHelper.Deg2Rad);
            }
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public GCamera Camera
        {
            get;
            set;
        }
        #endregion CameraProperties
        #region properties
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("Control")]
        public ControlStrategyType ControlStrategyType
        {
            get => CameraComponentInitializer.ControlStrategyType;
            set
            {
                CameraComponentInitializer.ControlStrategyType = value;
                ChangingControlStartety();
            }
        }
        void ChangingControlStartety()
        {
            if (ControlStrategyType == ControlStrategyType.Follow)
            {
                var follow = new GFollowStrategy();
                follow.HostCameraComponent = this;
                mStrategyGetter = new Macross.MacrossGetter<GCameraControlStrategy>(RName.EmptyName, follow);
            }
            else if (ControlStrategyType == ControlStrategyType.Free)
            {

            }
            else if (ControlStrategyType == ControlStrategyType.FollowAndLookAt)
            {
                var follow = new GFollowAndLookAtStrategy();
                follow.LookAtObject = this.Host;
                follow.HostCameraComponent = this;
                follow.LookAtOffset = new Vector3(-0.7f, 1.6f, 0);
                mStrategyGetter = new Macross.MacrossGetter<GCameraControlStrategy>(RName.EmptyName, follow);
            }
            else if (ControlStrategyType == ControlStrategyType.Custom)
            {

            }
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Category("Control")]
        public float Fov
        {
            get => CameraComponentInitializer.Fov;
            set
            {
                CameraComponentInitializer.Fov = value;
                Camera.Fov = value;
            }
        }
        List<GCameraModifier> mModifiers = new List<GCameraModifier>();
        #endregion properties
        public GCameraComponent()
        {
            Initializer = new GCameraComponentInitializer();
            Camera = new GCamera();
            Camera.Init(CEngine.Instance.RenderContext, true);
            TickBeforeCheckVisible = true;
        }

        public void SetCamera(Graphics.CGfxCamera camera)
        {
            //Camera = camera;
        }
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            if (Camera == null)
                return;
            mStrategyGetter.Get(OnlyForGame)?.Perform(Camera);
            for (int i = 0; i < mModifiers.Count; ++i)
            {
                if (mModifiers[i].Status == ModifierStatus.Invalid)
                {
                    mModifiers[i].StartExecution(Camera);
                }
                var result = mModifiers[i].Perform(Camera);
                if (result == ModifierStatus.Success)
                {
                    mModifiers[i].StopExecution(Camera);
                    mModifiers.RemoveAt(i);
                    i--;
                }
            }
        }
        public virtual void LookAt(Vector3 lookAt)
        {
            var dir = lookAt - Position;
            var rotation = Quaternion.GetQuaternion(-Vector3.UnitZ, dir);
            Placement.Rotation = rotation;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void PlayCameraModifier([Editor.Editor_RNameMacrossType(typeof(GCameraModifier))] RName modifier)
        {
            var getter = CEngine.Instance.MacrossDataManager.NewObjectGetter<GCameraModifier>(modifier);
            var theModidier = getter.Get(OnlyForGame);
            if (theModidier != null)
                mModifiers.Add(theModidier);
        }

        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void ZoomTo(float value, float time)
        {
            var modifier = new GFovCameraModifier();
            modifier.TargetFov = value;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationX(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.XOscillation.Frequency = frequency;
            modifier.XOscillation.Amplitude = amplitude;
            modifier.XOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationY(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.YOscillation.Frequency = frequency;
            modifier.YOscillation.Amplitude = amplitude;
            modifier.YOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationZ(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.ZOscillation.Frequency = frequency;
            modifier.ZOscillation.Amplitude = amplitude;
            modifier.ZOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationPitch(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.PitchOscillation.Frequency = frequency;
            modifier.PitchOscillation.Amplitude = amplitude;
            modifier.PitchOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationYaw(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.YawOscillation.Frequency = frequency;
            modifier.YawOscillation.Amplitude = amplitude;
            modifier.YawOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationRoll(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.RollOscillation.Frequency = frequency;
            modifier.RollOscillation.Amplitude = amplitude;
            modifier.RollOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void OscillationFov(float frequency, float amplitude, float offset, float time)
        {
            var modifier = new GCameraShake();
            modifier.FovOscillation.Frequency = frequency;
            modifier.FovOscillation.Amplitude = amplitude;
            modifier.FovOscillation.Offset = offset;
            modifier.Duration = time;
            mModifiers.Add(modifier);
        }
    }
}
