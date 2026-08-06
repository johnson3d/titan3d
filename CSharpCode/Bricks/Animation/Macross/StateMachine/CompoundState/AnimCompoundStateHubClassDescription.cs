using EngineNS.Animation.Macross;
using EngineNS.Bricks.StateMachine.Macross.CompoundState;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EngineNS.Bricks.Animation.Macross.StateMachine.CompoundState
{
    [GraphElement(typeof(TtGraphElement_TimedCompoundStateHub))]
    public class TtAnimCompoundStateHubClassDescription : TtTimedCompoundStateHubClassDescription, IAnimMacrossClassDescription
    {

    }
}
