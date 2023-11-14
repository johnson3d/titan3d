using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    public class TimedStateAttachmentContextMenuAttribute : ContextMenuAttribute
    {
        public TimedStateAttachmentContextMenuAttribute(string filterStrings, string menuPaths, params string[] keyStrings) : base(filterStrings, menuPaths, keyStrings)
        {
        }
    }
    public class TtTimedStateAttachmentClassDescription : TtDesignableVariableDescription
    {
        public override string Name { get; set; } = "Script";
        [Rtti.Meta]
        public TtMethodDescription TickMethodDescription { get; set; } = null;
    }
}
