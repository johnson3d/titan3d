using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.Rtti;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.DesignMacross.Design.ConnectingLine
{
    public class TtPinDescription : IDescription
    {
        public IDescription Parent { get; set; }
        [Rtti.Meta("")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Rtti.Meta("")]
        public string Name { get; set; } = "";
        [Rtti.Meta("")]
        public TtTypeDesc TypeDesc { get; set; } = null;
        [Rtti.Meta("")]
        public bool CanMutilLink { get; set; } = false;
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

    public class TtLineDescriptionElementStyle : TtGraphElementStyle
    {
        public float Thickness = TtDesignMacrossGraphStyles.LineNormalThickness;
        
    }

    [GraphElementStyle(typeof(TtLineDescriptionElementStyle))]
    public class TtLineDescription : IDescription
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
