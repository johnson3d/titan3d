using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler.Trace
{
    public class TtAction
    {
        public ETraceChannel GetChannel()
        {
            return ETraceChannel.None;
        }
        public virtual void Write(IO.IWriter writer)
        {

        }
    }
    public class TtChannel
    {
        public ETraceChannel Channel { get; set; }
        public string Name { get; set; } = "";
        public List<TtAction> Actions = new List<TtAction>();
        public void Write(IO.IWriter writer)
        {
            writer.Write(Actions.Count);
            foreach (var act in Actions)
            {
                act.Write(writer);
            }
        }
    }
}
