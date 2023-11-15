using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;

namespace EngineNS.DesignMacross.Editor
{

    public class TtOutlineEditPanel
    {
        public TtClassDescription ClassDesc { get; set; } = new TtClassDescription();
        public TtOutline Outline { get; set; } = new TtOutline();
        public void Initialize()
        {
            mRender = new TtOutlinePanelRender();
        }
        TtOutlinePanelRender mRender = null;
        public void Draw(FDesignMacrossEditorRenderingContext context)
        {
            if(Outline.Description == null)
            {
                Outline.Description = ClassDesc;
            }
            mRender.Draw(this, context);
        }
       
    }

    public class TtOutlinePanelRender
    {
        public void Draw(TtOutlineEditPanel classDeclarationPanel, FDesignMacrossEditorRenderingContext context)
        {
            var render = TtElementRenderDevice.CreateOutlineRender(classDeclarationPanel.Outline);
            var outlineContext = new FOutlineRenderingContext();
            outlineContext.CommandHistory = context.CommandHistory;
            outlineContext.EditorInteroperation = context.EditorInteroperation;
            render.Draw(classDeclarationPanel.Outline, ref outlineContext);
        }
    }
}
