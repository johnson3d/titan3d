using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDataPinDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; } = "";
        [Rtti.Meta]
        public UTypeDesc TypeDesc { get; set; } = UTypeDesc.TypeOf<bool>();
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
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtDataInPinDescription : TtDataPinDescription
    {
        
    }
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtDataOutPinDescription : TtDataPinDescription
    {
        
    }
    [GraphElement(typeof(TtGraphElement_DataLine))]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDataLineDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta]
        public string Name { get; set; }
        /// <summary>
        /// DataPinId
        /// </summary>
        [Rtti.Meta] 
        public Guid FromId { get; set; } = Guid.Empty;
        /// <summary>
        /// DataPinId
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
