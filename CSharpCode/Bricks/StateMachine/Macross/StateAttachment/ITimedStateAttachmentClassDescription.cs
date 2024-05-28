using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Design;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
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
        public TtTimedStateScriptMethodDescription TickMethodDescription { get; set; } = null;
        [Rtti.Meta]
        public TtTimedStateScriptMethodDescription InitMethodDescription { get; set; } = null;
    }
}
