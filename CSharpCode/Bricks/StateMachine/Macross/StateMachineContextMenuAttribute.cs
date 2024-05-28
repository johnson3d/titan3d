using EngineNS.Bricks.CodeBuilder;

namespace EngineNS.Bricks.StateMachine.Macross
{
    public class StateMachineContextMenuAttribute : ContextMenuAttribute
    {
        public StateMachineContextMenuAttribute(string filterStrings, string menuPaths, params string[] keyStrings) : base(filterStrings, menuPaths, keyStrings)
        {
        }
    }
}
