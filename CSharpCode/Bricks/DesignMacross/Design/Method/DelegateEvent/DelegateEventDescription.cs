using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EngineNS.DesignMacross.Design
{
    [GraphElement(typeof(TtGraphElement_DelegateEvent))]
    public class TtDelegateEventDescription : IDescription
    {
        public TtExecutionPinDescription OutExecutionPin = new TtExecutionPinDescription();
        public List<TtDataPinDescription> DataPins { get; set; } = new();
        public List<TtExecutionLineDescription> ExecutionLines { get; set; } = new();
        public List<TtDataLineDescription> DataLines { get; set; } = new();

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Event";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
