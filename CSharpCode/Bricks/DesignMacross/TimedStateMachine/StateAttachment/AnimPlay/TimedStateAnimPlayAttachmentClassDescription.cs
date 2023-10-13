using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.TimedStateMachine.StateAttachment.Scrpit;
using System;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine.StateAttachment
{
    [TimedStateAttachmentContextMenu("AnimPlay", "Attachment\\AnimPlay", UMacross.MacrossEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_TimedStateAnimPlayAttachment))]
    public class TtTimedStateAnimPlayAttachmentClassDescription : ITimedStateAttachmentClassDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "AnimPlay";
        [Rtti.Meta]
        public Vector2 Location { get; set; }

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
