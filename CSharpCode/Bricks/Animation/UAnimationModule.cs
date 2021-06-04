using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    public partial class UAnimationModule : UModule<UEngine>
    {

    }
}
namespace EngineNS
{
    partial class UEngine
    {
        public EngineNS.Animation.UAnimationModule AnimationModule { get; } = new Animation.UAnimationModule();
    }
}