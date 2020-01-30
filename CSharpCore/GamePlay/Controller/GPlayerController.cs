using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay.Controller
{
    [Rtti.MetaClassAttribute]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(GPlayerControllerInitializer), "玩家控制器", "Controller", "PlayerController")]
    [Editor.Editor_ComponentClassIconAttribute("icon/mcplayercontroller_64.txpic", RName.enRNameType.Editor)]
    public class GPlayerController : GControllerBase
    {
        [Rtti.MetaClass]
        public class GPlayerControllerInitializer : GComponentInitializer
        {

        }
        public GPlayerController()
        {
            Initializer = new GPlayerControllerInitializer();
        }

        Vector3 mInput = new Vector3();
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float VerticalInput
        {
            get => mInput.X;
            set => mInput.X = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float HorizontalInput
        {
            get => mInput.Z;
            set => mInput.Z = value;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float RollInput
        {
            get => mInput.Y;
            set => mInput.Y = value;
        }
        public override void Tick(GPlacementComponent placement)
        {
            base.Tick(placement);
            //var movementCom = Host.GetComponent<GMovementComponent>();
            //var cameraCom = Host.GetComponentRecursion<Camera.CameraComponent>();
            //if (cameraCom == null)
            //    return;
            //var dir = cameraCom.CameraDirection;
            //dir.Y = 0;
            //dir.Normalize();
            //var forward = mInput.X * dir;
            //var right = mInput.Z * (Vector3.Cross(Vector3.UnitY, dir));
            //var VelDir = forward + right;// - movementCom.Velocity;
            //if (mInput == Vector3.Zero)
            //{
            //    if (movementCom != null)
            //    {
            //        if (movementCom.ControllerInput != Vector3.Zero)
            //            movementCom.ControllerInput = Vector3.Zero;
            //    }
            //}
            //else
            //{
            //    if (movementCom != null)
            //    {
            //        movementCom.ControllerInput = VelDir;
            //        var orientation = movementCom.Velocity;
            //        orientation.Normalize();
            //        orientation.Y = 0;
            //        if (orientation != Vector3.Zero)
            //            movementCom.DesireOrientation = orientation;
            //    }
            //}
            McPlayerController?.OnTick(this);
            //mInput = Vector3.Zero;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.Editor_RNameMacrossType(typeof(McPlayerController))]
        public override RName ComponentMacross
        {
            get { return base.ComponentMacross; }
            set
            {
                base.ComponentMacross = value;
                if (McPlayerController != null)
                {
                    McPlayerController.HostComp = this;
                }
            }
        }
        McPlayerController McPlayerController
        {
            get
            {
                return mMcCompGetter?.CastGet<McPlayerController>(OnlyForGame);
            }
        }
    }
    [Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcplayercontroller_64.txpic", RName.enRNameType.Editor)]
    public class McPlayerController : McComponent
    {
        
    }
}
