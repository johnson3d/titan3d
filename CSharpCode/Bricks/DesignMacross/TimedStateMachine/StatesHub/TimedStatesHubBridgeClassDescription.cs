using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Description;
using EngineNS.DesignMacross.Graph;
using EngineNS.DesignMacross.Outline;
using EngineNS.DesignMacross.TimedStateMachine.StatesHub;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    [GraphElement(typeof(TtGraphElement_TimedStatesHubBridge))]
    public class TtTimedStatesHubBridgeClassDescription : IDescription
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "TimedStatesHubBridge";

        public TtTimedStatesHubClassDescription TimedStatesHubClassDescription { get; set; } = null;

    }
}
