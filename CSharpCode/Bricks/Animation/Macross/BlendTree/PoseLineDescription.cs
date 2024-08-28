using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Design.ConnectingLine;
using EngineNS.Rtti;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.Animation.Macross.BlendTree
{
    public class TtPosePinDescription : TtDataPinDescription
    {

    }

    [GraphElement(typeof(TtGraphElement_PosePin))]
    public class TtPoseInPinDescription : TtPosePinDescription
    {
        
    }

    [GraphElement(typeof(TtGraphElement_PosePin))]
    public class TtPoseOutPinDescription : TtPosePinDescription
    {
        
    }
    [GraphElement(typeof(TtGraphElement_PoseLine))]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtPoseLineDescription : IDescription
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
