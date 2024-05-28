using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design;

namespace EngineNS.Bricks.StateMachine.Macross.StateTransition
{
    [ImGuiElementRender(typeof(TtGraph_MethodRender))]
    public class TtGraph_TimedStateTransition : TtGraph_Method
    {
        public TtGraph_TimedStateTransition(IDescription description) : base(description)
        {

        }
        public override void ConstructElements(ref FGraphRenderingContext context)
        {
            base.ConstructElements(ref context);
        }
        public override void AfterConstructElements(ref FGraphRenderingContext context)
        {
            base.AfterConstructElements(ref context);
        }
    }
}
