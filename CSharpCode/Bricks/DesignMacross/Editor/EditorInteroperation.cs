using EngineNS.Animation.Macross;
using EngineNS.Animation.Macross.BlendTree;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Outline;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace EngineNS.DesignMacross.Editor
{
    public class TtEditorInteroperation
    {
        public TtOutlineEditPanel OutlineEditPanel { get; set; } = null;
        public TtGraphEditPanel GraphEditPanel { get; set; } = null;
        public EGui.Controls.PropertyGrid.TtPropertyGrid PGMember = null;
        public Preview.TtAnimationPreviewPanel AnimationPreviewPanel { get; set; } = null;

        public void OpenGraph(IDescription description, FOutlineElementRenderingContext context)
        {
            GraphEditPanel.EditGraph(description);
            if(description is IAnimMacrossClassDescription animMacrossClassDescription)
            {
                AnimationPreviewPanel.DesignMacross = context.DesignMacrossAssetName;
                AnimationPreviewPanel.IsShow = true;
            }
            else
            {
                AnimationPreviewPanel.IsShow = false;
            }
        }
    }
}
