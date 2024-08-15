using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    public partial class TtAnimationModule : UModule<TtEngine>
    {

    }
}
namespace EngineNS
{
    partial class TtEngine
    {
        public EngineNS.Animation.TtAnimationModule AnimationModule { get; } = new Animation.TtAnimationModule();
    }
}