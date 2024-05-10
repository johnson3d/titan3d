using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    public partial class TtAnimationModule : UModule<UEngine>
    {

    }
}
namespace EngineNS
{
    partial class UEngine
    {
        public EngineNS.Animation.TtAnimationModule AnimationModule { get; } = new Animation.TtAnimationModule();
    }
}