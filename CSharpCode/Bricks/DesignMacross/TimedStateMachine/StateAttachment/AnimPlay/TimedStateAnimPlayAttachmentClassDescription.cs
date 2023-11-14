using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.TimedStateMachine.StateAttachment.Scrpit;
using System;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine.StateAttachment
{
    [TimedStateAttachmentContextMenu("AnimPlay", "Attachment\\AnimPlay", UDesignMacross.MacrossEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_TimedStateAnimPlayAttachment))]
    public class TtTimedStateAnimPlayAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {
        public override string Name { get; set; } = "AnimPlay";
        [Rtti.Meta]
        public RName AnimationClip;

    }
}
