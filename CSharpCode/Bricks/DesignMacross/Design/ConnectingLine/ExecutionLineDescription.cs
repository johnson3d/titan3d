using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtExecutionPinDescription : TtPinDescription
    {
      
    }
    [GraphElement(typeof(TtGraphElement_ExecutionPin))]
    public class TtExecutionInPinDescription : TtExecutionPinDescription
    {
        
    }
    [GraphElement(typeof(TtGraphElement_ExecutionPin))]
    public class TtExecutionOutPinDescription : TtExecutionPinDescription
    {
        
    }

    [GraphElement(typeof(TtGraphElement_ExecutionLine))]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtExecutionLineDescription : TtLineDescription
    {

    }
}
