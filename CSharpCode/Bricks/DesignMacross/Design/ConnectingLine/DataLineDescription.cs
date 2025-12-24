using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    public class TtDataPinDescriptionElementStyle : TtGraphElementStyle
    {
        public bool BrowserVisible = false;
        public EGui.Controls.TtContentBrowser ContentBrowser = EngineNS.Editor.TtEditor.NewPopupContentBrowser();
        public string FilterExts;
        public Rtti.TtTypeDesc ShowType;
    }
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [GraphElementStyle(typeof(TtDataPinDescriptionElementStyle))]
    public class TtDataPinDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta("")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta("")]
        public string Name { get; set; } = "";
        [Rtti.Meta("")]
        public TtTypeDesc TypeDesc { get; set; } = null;
        public void UpdateData(ref FDescriptionUpdateContext updateContext)
        {

        }
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

        public void OnPropertyRead(object tagObject, string name, bool fromXml)
        {

        }
        public void OnPostRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyWrite(string prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
    [GraphElement(typeof(TtGraphElement_DataInPin))]
    public class TtDataInPinDescription : TtDataPinDescription
    {
        [Rtti.Meta("")]
        public string TypeVaule { get; set; } = null;
    }
    [GraphElement(typeof(TtGraphElement_DataPin))]
    public class TtDataOutPinDescription : TtDataPinDescription
    {
        
    }
    [GraphElement(typeof(TtGraphElement_DataLine))]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtDataLineDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta("")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta("")]
        public string Name { get; set; }
        /// <summary>
        /// DataPinId
        /// </summary>
        [Rtti.Meta("")] 
        public Guid FromId { get; set; } = Guid.Empty;
        /// <summary>
        /// DataPinId
        /// </summary>
        [Rtti.Meta("")] 
        public Guid ToId { get; set; } = Guid.Empty;
        public void UpdateData(ref FDescriptionUpdateContext updateContext)
        {

        }
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

        public void OnPropertyRead(object tagObject, string prop, bool fromXml)
        {

        }
        public void OnPostRead(object tagObject, object hostObject, bool fromXml) { }
        public void OnPropertyWrite(string prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
