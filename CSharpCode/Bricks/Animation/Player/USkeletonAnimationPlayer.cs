using EngineNS.Animation.Animatable;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation.Player
{
    public class USkeletonAnimationPlayer : IAnimationPlayer
    {
        public float Time { get; set; } = 0;

        public Asset.UAnimationClip SkeletonAnimClip { get; set; } = null;
        public UAnimationPropertiesSetter AnimationPropertiesSetter { get; set; }

        public USkeletonAnimationPlayer()
        {

        }

        public void Update(float elapse)
        {
            System.Diagnostics.Debug.Assert(SkeletonAnimClip.Duration != 0.0f);

            Time += elapse;
            Time %= SkeletonAnimClip.Duration;
        }

        public void Evaluate()
        {
            //make command
            Pipeline.UPropertiesSettingCommand command = new Pipeline.UPropertiesSettingCommand();
            command.Context = new Pipeline.PropertiesSettingCommandContext(AnimationPropertiesSetter, Time);

            //if(IsImmediate)
            command.Excute();
            //else Insert to pipeline
            //AnimationPiple.CommandList.Add();
        }

        //DynamicInitialize will be refactoring in next version
        public void Binding(SkeletonAnimation.AnimatablePose.IAnimatableLimbPose pose)
        {
            System.Diagnostics.Debug.Assert(pose != null);

            var attrs = pose.GetType().GetCustomAttributes(typeof(IAnimatableClassBindingAttribute), true);
            UAnimationPropertiesSetter propertiesSetter = new UAnimationPropertiesSetter();
            if(attrs.Length == 0)
            {
                var binder = new CommonAnimatableBingAttribute();
                binder.Binding(pose, SkeletonAnimClip.AnimatedHierarchy, SkeletonAnimClip, ref propertiesSetter);
            }
            else
            {
                var binder = attrs[0] as IAnimatableClassBindingAttribute;
                binder.Binding(pose, SkeletonAnimClip.AnimatedHierarchy, SkeletonAnimClip, ref propertiesSetter);
            }
            AnimationPropertiesSetter = propertiesSetter;
        }
    }
}
