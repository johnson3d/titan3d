using EngineNS.Animation.RootMotion;
using EngineNS.GamePlay.Camera;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Movemnet
{
    [Bricks.CodeBuilder.ContextMenu("Movement", "Gameplay\\Movement", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtMovement.TtMovementData), DefaultNamePrefix = "Movement")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtMovement : Scene.TtLightWeightNodeBase
    {
        [Rtti.Meta("")]
        public class TtMovementData : TtNodeData
        {
            [Rtti.Meta("")]
            public bool EnableGravity { get; set; } = false;
            [Rtti.Meta("")]
            public Vector3 GravityAcceleration { get; set; } = Vector3.Down * 9.8f;
            [Rtti.Meta("")]
            public float Speed { get; set; } = 5;
            /// <summary>
            /// 动画根骨骼位移的采用方式。非Ignore时本节点会把该模式同步给动画节点,
            /// 并每帧消费其产出的位移。
            /// </summary>
            [Rtti.Meta("")]
            public ERootMotionMode RootMotionMode { get; set; } = ERootMotionMode.Ignore;
        }
        public TtMovementData MovementData { get=> NodeData as TtMovementData;}
        public float Speed { get=>MovementData.Speed; }
        public Vector3 LinearVelocity { get; private set; }
        [Category("Option")]
        public Vector3 DesiredLinearVelocity { get; set; } = Vector3.Zero;
        public void SetLinearVelocity(Vector3 linearVelocity)
        {
            DesiredLinearVelocity = linearVelocity;
        }
        public Vector3 AngularVelocity { get; private set; }

        private Vector3 SettedAngularVelocity = Vector3.Zero;
        public void SetAngularVelocity(Vector3 angularVelocity)
        {
            SettedAngularVelocity = angularVelocity;
        }
        [Category("Option")]
        public bool EnableGravity { get=>MovementData.EnableGravity; set=> MovementData.EnableGravity = value; }
        [Category("Option")]
        public Vector3 GravityAcceleration { get => MovementData.GravityAcceleration; set => MovementData.GravityAcceleration = value; }
        [Category("Option")]
        public ERootMotionMode RootMotionMode { get => MovementData.RootMotionMode; set => MovementData.RootMotionMode = value; }
        protected Vector3 GravityVelocity = Vector3.Zero;
        public float MaxGravitySpeed = 10;

        #region RootMotion
        IRootMotionSource mRootMotionSource = null;
        int mRootMotionSourceSearchFrame = -1;
        const int RootMotionSourceSearchInterval = 60;

        /// <summary>
        /// 在所属Actor子树下查找动画播放节点(一般挂在MeshNode下), 找到后缓存。
        /// 未找到时隔一段帧数重试, 避免每帧遍历节点树。
        /// </summary>
        protected IRootMotionSource GetRootMotionSource()
        {
            if (mRootMotionSource != null)
                return mRootMotionSource;
            if (Parent == null)
                return null;

            int frame = TtEngine.Instance.FrameCount;
            if (mRootMotionSourceSearchFrame >= 0 && frame - mRootMotionSourceSearchFrame < RootMotionSourceSearchInterval)
                return null;
            mRootMotionSourceSearchFrame = frame;

            IRootMotionSource found = null;
            Parent.IterateNodes((node, arg) =>
            {
                var source = node as IRootMotionSource;
                if (source == null)
                    return true;
                found = source;
                return false;
            }, null);
            mRootMotionSource = found;
            return mRootMotionSource;
        }

        /// <summary>
        /// 取出本帧的根骨骼位移(Actor空间), 并把当前模式同步给动画节点
        /// </summary>
        protected bool ConsumeRootMotion(out FTransform delta)
        {
            delta = FTransform.Identity;
            if (RootMotionMode == ERootMotionMode.Ignore)
                return false;

            var source = GetRootMotionSource();
            if (source == null)
                return false;

            if (source.RootMotionMode != RootMotionMode)
                source.RootMotionMode = RootMotionMode;
            return source.ConsumeRootMotion(out delta);
        }

        /// <summary>
        /// 把Actor空间的位移增量转为世界空间位移, 并把旋转增量应用到Placement。
        /// 返回需要叠加的世界空间位移。
        /// </summary>
        protected DVector3 ApplyRootMotionRotationAndGetTranslation(in FTransform delta)
        {
            var actorQuat = Parent.Placement.Quat;
            var worldTranslation = TtRootMotionUtil.ConvertDeltaToWorldTranslation(in delta, in actorQuat);
            // 引擎约定: q1 * q2 表示先旋q1再旋q2, 增量作为局部量要放在左侧
            if (!delta.Quat.IsIdentity)
            {
                var newQuat = delta.Quat * actorQuat;
                newQuat.Normalize();
                Parent.Placement.Quat = newQuat;
            }
            return worldTranslation;
        }
        #endregion RootMotion

        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtMovement>.Scope;
        }
        /// <summary>
        /// 本节点要消费动画节点本帧提交的 RootMotion, 所以必须排在动画求值之后。
        /// 取 ETickOrder.Movement 而不是缺省值, 是为了跟动画节点的 ETickOrder.Animation 拉开 ——
        /// 相同返回值的节点之间相对顺序是未定义的。
        /// </summary>
        public override int GetTickOrder()
        {
            return (int)ETickOrder.Movement;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            DVector3 posBeforeMove = Parent.Placement.AbsTransform.Position;
            UpdatePlacement(args.World, args.Policy);
            DVector3 posAfterMove = Parent.Placement.AbsTransform.Position;
            LinearVelocity = (posAfterMove - posBeforeMove).ToSingleVector3() / args.World.DeltaTimeSecond;
            return base.OnTickLogic(args);
        }

        protected Vector3 ConsumeSettedLinearVelocity()
        {
            var temp = DesiredLinearVelocity;
            DesiredLinearVelocity = Vector3.Zero;
            return temp;
        }
        protected Vector3 ConsumeSettedAngularVelocity()
        {
            var temp = SettedAngularVelocity;
            SettedAngularVelocity = Vector3.Zero;
            return temp;
        }
        protected virtual void UpdatePlacement(TtWorld world, TtRenderPolicy policy)
        {
            if (!world.IsGameWorld)
                return;

            var settedLinearVelocity = ConsumeSettedLinearVelocity();
            var settedAngularVelocity = ConsumeSettedAngularVelocity();
            var currentLinearVelocity = Vector3.Zero;
            var currentAngularVelocity = Vector3.Zero;
            if (EnableGravity)
            {
                GravityVelocity += GravityAcceleration * world.DeltaTimeSecond;
                if (GravityVelocity.Length() > MaxGravitySpeed)
                {
                    GravityVelocity = GravityAcceleration.NormalizeValue * MaxGravitySpeed;
                }
                currentLinearVelocity = settedLinearVelocity + GravityVelocity;
            }
            else
            {
                currentLinearVelocity = settedLinearVelocity;
            }
            currentAngularVelocity = settedAngularVelocity;

            FTransform rootMotionDelta;
            DVector3 rootMotionTranslation = DVector3.Zero;
            if (ConsumeRootMotion(out rootMotionDelta))
            {
                rootMotionTranslation = ApplyRootMotionRotationAndGetTranslation(in rootMotionDelta);
            }

            Parent.Placement.Position += (currentLinearVelocity * world.DeltaTimeSecond).AsDVector() + rootMotionTranslation;
            Parent.Placement.Quat = Parent.Placement.Quat * Quaternion.FromEuler(new FRotator(currentAngularVelocity) * world.DeltaTimeSecond);
        }
    }
}
