using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Animation
{
    /// <summary>
    /// 动画系统的日志分类
    /// </summary>
    public class TtAnimationCategory : Profiler.TtLogCategory
    {
        public override string ToString()
        {
            return "Animation";
        }
    }
    public partial class TtAnimationModule : TtModule<TtEngine>
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