using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Binding;
using EngineNS.Bricks.Animation.Notify;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.LogicAnim
{
    public class LAAnimClip
    {
        public async static Task<LAAnimClip> Create(RName name)
        {
            var clip =await AnimationClip.Create(name);
            if (clip != null)
            {
                return new LAAnimClip() { Clip = clip };
            }
            return null;
        }
        public static LAAnimClip CreateSync(RName name)
        {
            var clip = AnimationClip.CreateSync(name);
            if (clip != null)
            {
                return new LAAnimClip() { Clip = clip };
            }
            return null;
        }
        AnimationClip Clip { get; set; } = null;
        public AnimationBindingPose Bind(Pose.CGfxSkeletonPose pose)
        {
            return Clip?.Bind(pose);
        }
        public void StretchTime(float timeSecond)
        {
            Clip?.StretchTime(timeSecond);
        }
        internal void Seek(float timeInSecond)
        {
            Clip?.Seek(timeInSecond);
        }
        internal void TickNotify(GamePlay.Component.GComponent component)
        {
            Clip?.TickNofity(component);
        }
        public Pose.CGfxSkeletonPose OutPose
        {
            get => Clip?.BindingSkeletonPose;
        }
        public void AttachNotifyEvent(int index, NotifyHandle notifyHandle)
        {
            Clip?.AttachNotifyEvent(index,notifyHandle);
        }
    }
}
