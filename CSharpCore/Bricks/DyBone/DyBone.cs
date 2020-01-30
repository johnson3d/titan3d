using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace EngineNS.Bricks.DyBone
{
    public class DyBone
    {
        [Description("How much the bones slowed down.")]
        public float Damping
        {
            get;
            set;
        } = 0.1f;
        [Description("How much the force applied to return each bone to original orientation.")]
        public float Elasticity
        {
            get;
            set;
        } = 0.1f;
        [Description("How much bone's original orientation are preserved.")]
        public float Stiffness
        {
            get;
            set;
        } = 0.1f;
        [Description("How much character's position change is ignored in physics simulation.")]
        public float Inert
        {
            get;
            set;
        } = 0;
        [Description("Each bone can be a sphere to collide with colliders. Radius describe sphere's size.")]
        public float Radius
        {
            get;
            set;
        } = 0;
        [Description("If End Length is not zero, an extra bone is generated at the end of transform hierarchy.")]
        public float EndLength
        {
            get;
            set;
        } = 0;
        [Description("If End Offset is not zero, an extra bone is generated at the end of transform hierarchy.")]
        public Vector3 EndOffset
        {
            get;
            set;
        } = Vector3.Zero;
        [Description("The force apply to bones. Partial force apply to character's initial pose is cancelled out.")]
        public Vector3 Gravity
        {
            get;
            set;
        } = Vector3.Zero;
        [Description("The force apply to bones.")]
        public Vector3 Force
        {
            get;
            set;
        } = Vector3.Zero;

        public enum EFreezeAxis
        {
            None, X, Y, Z
        }
        [Description("Constrain bones to move on specified plane.")]
        public EFreezeAxis FreezeAxis
        {
            get;
            set;
        } = EFreezeAxis.None;
        [Description("Disable physics simulation automatically if character is far from camera or player.")]
        public bool DistantDisable
        {
            get;
            set;
        } = false;

        public struct Particle
        {
            public void SetDefault()
            {
                ParentIndex = -1;
                Damping = 0;
                Elasticity = 0;
                Stiffness = 0;
                Inert = 0;
                Radius = 0;
                BoneLength = 0;

                Position = Vector3.Zero;
                PrevPosition = Vector3.Zero;
                EndOffset = Vector3.Zero;
                InitLocalPosition = Vector3.Zero;
                InitLocalRotation = Quaternion.Identity;
            }
            public int ParentIndex;
            public float Damping;
            public float Elasticity;
            public float Stiffness;
            public float Inert;
            public float Radius;
            public float BoneLength;

            public Vector3 Position;
            public Vector3 PrevPosition;
            public Vector3 EndOffset;
            public Vector3 InitLocalPosition;
            public Quaternion InitLocalRotation;
            public UInt64 PVS;
        }
        Support.NativeList<Particle> mParticles = new Support.NativeList<Particle>();

        public DyColliderSet mColliderSet = null;
        public void InitParticles()
        {
            mParticles.Clear();

            //回到C++做初始化
            //if (m_Root == null)
            //    return;

            //m_LocalGravity = m_Root.InverseTransformDirection(m_Gravity);
            //m_ObjectScale = Mathf.Abs(transform.lossyScale.x);
            //m_ObjectPrevPosition = transform.position;
            //m_ObjectMove = Vector3.zero;
            //m_BoneTotalLength = 0;
            //AppendParticles(m_Root, -1, 0);
            //UpdateParameters();
        }

        public void Tick()
        {
            //根据重力，施加的力，运动，弹性系数什么的计算粒子新位置
            //根据ColliderSet来检测碰撞约束和修改粒子位置
            //重复上面操作，看看精度够了退出
        }

        //把mParticles的位置数据刷回附加骨骼状态
    }
}
