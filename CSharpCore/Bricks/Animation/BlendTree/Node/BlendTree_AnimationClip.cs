using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.BlendTree.Node
{
    public class BlendTree_AnimationClip : IBlendTree
    {
        public async static Task<BlendTree_AnimationClip> Create(RName name)
        {
            var bs = new BlendTree_AnimationClip();
            bs.Clip = await Animation.AnimNode.AnimationClip.Create(name);
            return bs;
        }
        public static BlendTree_AnimationClip CreateSync(RName name)
        {
            var bs = new BlendTree_AnimationClip();
            bs.Clip = Animation.AnimNode.AnimationClip.CreateSync(name);
            return bs;
        }
        protected Pose.CGfxSkeletonPose mOutPose = null;
        public Pose.CGfxSkeletonPose OutPose
        {
            get => mOutPose;
            set
            {
                mOutPose = value;
                if (mClip != null)
                    mClip.Bind(mOutPose);
            }
        }
        AnimNode.AnimationClip mClip = null;
        public AnimNode.AnimationClip Clip
        {
            get =>mClip;
            set
            {
                mClip = value;
            }
        }
        public ClipWarpMode WarpMode { get; set; } = ClipWarpMode.Loop;

        public void InitializePose(Pose.CGfxSkeletonPose pose)
        {
            OutPose = pose.Clone();
        }
        public void Evaluate(float timeInSecond)
        {
            Clip.Evaluate(timeInSecond, WarpMode);
        }
        public void Notifying(GamePlay.Component.GComponent component)
        {
            Clip.TickNofity(component);
        }
    }
}
