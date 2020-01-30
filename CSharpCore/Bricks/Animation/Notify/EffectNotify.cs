using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.Animation.Notify
{
    public class EffectNotify : CGfxNotify
    {
        [Rtti.MetaData]
        [Editor.Editor_RNameMacrossType(typeof(Particle.McParticleEffector))]
        public RName Particle { get; set; } = RName.EmptyName;
        [Rtti.MetaData]
        public Vector3 Position { get; set; }
        [Rtti.MetaData]
        public Vector3 Rotation { get; set; }
        [Rtti.MetaData]
        public Vector3 Scale { get; set; }
        [Rtti.MetaData]
        public bool WorldSpace { get; set; } = false;
        [Rtti.MetaData]
        public string SocketName { get; set; } = "";
        [Browsable(false)]
        public Bricks.Animation.Skeleton.IScocket ParentSocket { get; set; } = null;
        public override void TickLogic(GComponent component)
        {
            base.TickLogic(component);
        }
        public override bool Notify(GComponent component, long beforeTime, long afterTime)
        {
            var skeletonCom = component as SkeletonAnimationComponent;
            var socket = skeletonCom.Pose.FindBonePose(SocketName);
            var mat = socket.MeshSpaceMatrix;
            //var hostContainer = mHostContainer as GMeshComponent;
            //var skin = hostContainer?.SceneMesh?.MdfQueue.FindModifier<Graphics.Mesh.CGfxSkinModifier>();
            //if (skin == null || skin.MeshSpaceAnimPose == null)
            //    return;
            //var bone = skin.MeshSpaceAnimPose.FindBonePose(value);
            //if (bone == null)
            //    return;
            //ParentSocket = bone;
            //UpdateSocketMatrix(ParentSocket.MeshSpaceMatrix);
            return base.Notify(component, beforeTime, afterTime);
        }
    }
}
