using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Render;
using EngineNS.DesignMacross.Design.ConnectingLine;

namespace EngineNS.DesignMacross.Design.Expressions
{
    [ImGuiElementRender(typeof(TtGraphElementRender_ExpressionDescription))]
    public class TtGraphElement_BinaryLogicOperator : TtGraphElement_ExpressionDescription
    {
        public TtBinaryLogicOperatorDescription LogicOperatorDescription { get => Description as TtBinaryLogicOperatorDescription; }
        public TtGraphElement_BinaryLogicOperator(IDescription description, IGraphElementStyle style) : base(description, style)
        {
        }
    }
}
