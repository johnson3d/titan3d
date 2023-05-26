using EngineNS.Bricks.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.DesignMacross.TimedStateMachine
{
    public class StateMachineContextMenuAttribute : ContextMenuAttribute
    {
        public StateMachineContextMenuAttribute(string filterStrings, string menuPaths, params string[] keyStrings) : base(filterStrings, menuPaths, keyStrings)
        {
        }
    }
}
