using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Graph;

namespace EngineNS.Bricks.StateMachine.Macross.StateAttachment
{
    [TimedStateAttachmentContextMenu("AnimPlay", "Attachment\\AnimPlay", UDesignMacross.MacrossAnimEditorKeyword)]
    [GraphElement(typeof(TtGraphElement_AnimPlayAttachment))]
    public class TtAnimPlayAttachmentClassDescription : TtTimedStateAttachmentClassDescription
    {
        public override string Name { get; set; } = "AnimPlay";
        [Rtti.Meta]
        public RName AnimationClip;

    }
}
