using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.TimedStateMachine.StatesHub;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Reflection;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [GraphElement(typeof(TtGraphElement_TimedStatesHubBridge))]
    public class TtTimedStatesHubBridgeClassDescription : IDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "TimedStatesHubBridge";
        [Rtti.Meta]
        public Vector2 Location { get; set; }
        public TtTimedStatesHubClassDescription TimedStatesHubClassDescription { get; set; } = null;

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
