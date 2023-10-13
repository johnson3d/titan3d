using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.TimedStateMachine.StateAttachment.Scrpit;
using System;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine.StateAttachment
{
    [TimedStateAttachmentContextMenu("Script", "Attachment\\Script", UMacross.MacrossEditorKeyword)]
    [Graph(typeof(TtGraph_TimedStateScriptAttachment))]
    [GraphElement(typeof(TtGraphElement_TimedStateScriptAttachment))]
    public class TtTimedStateScriptAttachmentClassDescription : ITimedStateAttachmentClassDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Script";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public TtDelegateEventDescription OnBeginDesc { get; set; } = new TtDelegateEventDescription{ Name = "OnBegin" };
        public TtDelegateEventDescription OnTickDesc;
        public TtDelegateEventDescription OnEndDesc;


        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
