using EngineNS.Animation.SceneNode;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Camera
{
    [Bricks.CodeBuilder.ContextMenu("CameraSpringArm", "CameraSpringArm", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtCameraSpringArm.TtCameraSpringArmData), DefaultNamePrefix = "CameraSpringArm")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtCameraSpringArm : TtLightWeightNodeBase, ICameraControlNode
    {
        public class TtCameraSpringArmData : TtNodeData
        {
            [Rtti.Meta]
            public DVector3 TargetOffset { get; set; } = DVector3.Zero;
            [Rtti.Meta]
            public float ArmLength { get; set; } = 3.0f;
            [Rtti.Meta]
            public float ProbeSize
            {
                get;
                set;
            } = 0.01f;
            [Rtti.Meta]
            public float SpringDamping { get; set; } = 0.2f;
            [Rtti.Meta]
            public bool DoCollisionTest { get; set; } = true;
        }
        [Category("Option")]
        public DVector3 TargetOffset
        {
            get => (NodeData as TtCameraSpringArmData).TargetOffset;
            set => (NodeData as TtCameraSpringArmData).TargetOffset = value;
        }
        [Category("Option")]
        public float ArmLength
        {
            get => (NodeData as TtCameraSpringArmData).ArmLength;
            set => (NodeData as TtCameraSpringArmData).ArmLength = value;
        }
        [Category("Option")]
        public float ProbeSize
        {
            get => (NodeData as TtCameraSpringArmData).ProbeSize;
            set => (NodeData as TtCameraSpringArmData).ProbeSize = value;
        }
        [Category("Option")]
        public float SpringDamping
        {
            get => (NodeData as TtCameraSpringArmData).SpringDamping;
            set => (NodeData as TtCameraSpringArmData).SpringDamping = value;
        }
        [Category("Option")]
        public bool DoCollisionTest
        {
            get => (NodeData as TtCameraSpringArmData).DoCollisionTest;
            set => (NodeData as TtCameraSpringArmData).DoCollisionTest = value;
        }

        public TtGamePlayCamera Camera
        {
            get
            {
                if (Children.Count == 0)
                    return null;
                //for now just allow a camera in children
                System.Diagnostics.Debug.Assert(Children.Count == 1);
                return Children[0] as TtGamePlayCamera;
            }
        }

        private Quaternion Rotation = Quaternion.Identity;
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtCameraSpringArm), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override bool OnTickLogic(TtWorld world, TtRenderPolicy policy)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                var position = Placement.AbsTransform.Position + Placement.Quat * DVector3.Backward * ArmLength;
                var lookAt = Placement.AbsTransform.Position + TargetOffset;

                Camera?.LookAtLH(position, lookAt, Vector3.Up);
                return base.OnTickLogic(world, policy);
            }   
        }
        #region ICameraControlNode
        public void AddDelta(in FRotator delta)
        {

            Placement.Quat = Quaternion.FromEuler(Placement.Quat.ToEuler() + delta);
            //Placement.Quat = Quaternion.RotationAxis(Vector3.Left, delta.X) * Quaternion.RotationAxis(Vector3.Up, delta.Y) * Placement.Quat;


        }
        public void AddYaw(float delta)
        {
            //Placement.Quat = Quaternion.FromEuler(new Vector3(0, delta * 0.01f, 0)) * Placement.Quat;
        }

        public void AddPitch(float delta)
        {
           Placement.Quat = Quaternion.FromEuler(new FRotator(delta * 0.01f, 0, 0)) * Placement.Quat;
        }

        public void AddRoll(float delta)
        {
            throw new NotImplementedException();
        }
        #endregion ICameraControlNode
    }
}

