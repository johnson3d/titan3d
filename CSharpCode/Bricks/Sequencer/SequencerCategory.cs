using System;

namespace EngineNS.Sequencer
{
    /// <summary>Sequencer 的日志分类</summary>
    public class TtSequencerCategory : Profiler.TtLogCategory
    {
        public override string ToString()
        {
            return "Sequencer";
        }
    }
}
