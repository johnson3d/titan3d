using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtExecutionPinDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "";
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if (hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
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
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtExecutionLineDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "ExecutionLine";
        /// <summary>
        /// ExecutePinId
        /// </summary>
        [Rtti.Meta]
        public Guid FromId { get; set; } = Guid.Empty;
        /// <summary>
        /// ExecutePinId
        /// </summary>
        [Rtti.Meta]
        public Guid ToId { get; set; } = Guid.Empty; 
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            if (hostObject is IDescription parentDescription)
            {
                Parent = parentDescription;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
