using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore
{
    [Flags]
    public enum EPhyRigidBodyFlag : UInt32
    {
        eKINEMATIC = (1 << 0),      //!< Enable kinematic mode for the body.
        eUSE_KINEMATIC_TARGET_FOR_SCENE_QUERIES = (1 << 1),
        eENABLE_CCD = (1 << 2),     //!< Enable CCD for the body.
        eENABLE_CCD_FRICTION = (1 << 3),
        eENABLE_POSE_INTEGRATION_PREVIEW = (1 << 4),
        eENABLE_SPECULATIVE_CCD = (1 << 5),
        eENABLE_CCD_MAX_CONTACT_IMPULSE = (1 << 6)
    }
    [Flags]
    public enum EPhyActorFlag : UInt32
    {
        eVISUALIZATION = (1 << 0),
        eDISABLE_GRAVITY = (1 << 1),
        eSEND_SLEEP_NOTIFIES = (1 << 2),
        eDISABLE_SIMULATION = (1 << 3)

    }
    public class CPhyActor : CPhyEntity, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public CPhyActor(NativePointer self, EPhyActorType type) : base(self)
        {
            mPhyType = type;
        }
        ~CPhyActor()
        {

        }
        protected EPhyActorType mPhyType;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EPhyActorType PhyType
        {
            get { return mPhyType; }
            set
            {
                if (PCInitializer != null)
                    PCInitializer.ActorType = value;

                mPhyType = value;

            }
        }
        protected CPhyScene mScene;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        [Browsable(false)]
        public CPhyScene Scene
        {
            get { return mScene; }
            set { mScene = value; }
        }
        public HashSet<CPhyShape> Shapes
        {
            get;
            set;
        } = new HashSet<CPhyShape>();

        
        public EngineNS.Bricks.PhysicsCore.GPhysicsComponent.GPhysicsComponentInitializer PCInitializer
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public GamePlay.Actor.GActor HostActor
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public GamePlay.Component.IPlaceable HostPlaceable
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Browsable(false)]
        public CollisionComponent.IPhysicsEventCallback HostComponent
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Position
        {
            get
            {
                return SDK_PhyActor_GetPosition(CoreObject);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Quaternion Rotation
        {
            get
            {
                return SDK_PhyActor_GetRotation(CoreObject);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool SetPose2Physics(ref PhyTransform transform, bool autowake = true)
        {
            unsafe
            {
                fixed (PhyTransform* p = &transform)
                {
                    return SDK_PhyActor_SetPose2Physics(CoreObject, p, vBOOL.FromBoolean(autowake));
                }
            }
        }

        public void SetPose2PhysicsFrom(Vector3 location, Quaternion rotation)
        {
            var phyTrans = EngineNS.Bricks.PhysicsCore.PhyTransform.CreateTransform(location, rotation);
            SetPose2Physics(ref phyTrans);
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void AddToScene(CPhyScene scene)
        {
            if (scene == mScene)
                return;

            if (mScene != null)
            {
                mScene.Actors.Remove(this);
            }
            mScene = scene;
            if (mScene != null)
            {
                mScene.Actors.Add(this);
            }

            if (scene != null)
                SDK_PhyActor_AddToScene(CoreObject, scene.CoreObject);
            else
                SDK_PhyActor_AddToScene(CoreObject, CPhyScene.GetEmptyNativePointer());
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value)
        {
            if (mPhyType != EPhyActorType.PAT_Dynamic)
                return false;
            return SDK_PhyActor_SetRigidBodyFlag(CoreObject, (UInt32)flag, vBOOL.FromBoolean(value));
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool SetActorFlag(EPhyActorFlag flag, bool value)
        {
            if (mPhyType != EPhyActorType.PAT_Dynamic)
                return false;
            return SDK_PhyActor_SetActorFlag(CoreObject, (UInt32)flag, vBOOL.FromBoolean(value));
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void AttachParticle(ref Particle.CGfxParticle p)
        {
            p.WeakReferenceTag = this;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public static CPhyActor GetPhyActorFromParticle(ref Particle.CGfxParticle p)
        {
            return p.WeakReferenceTag as CPhyActor;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void SDK_PhyActor_AddToScene(NativePointer self, CPhyScene.NativePointer scene);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static Vector3 SDK_PhyActor_GetPosition(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static Quaternion SDK_PhyActor_GetRotation(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static unsafe vBOOL SDK_PhyActor_SetPose2Physics(NativePointer self, PhyTransform* transform, vBOOL autowake);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static vBOOL SDK_PhyActor_SetRigidBodyFlag(NativePointer self, UInt32 flag, vBOOL value);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static vBOOL SDK_PhyActor_SetActorFlag(NativePointer self, UInt32 flag, vBOOL value);
        #endregion
    }
}
