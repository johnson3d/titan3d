using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using EngineNS.Bricks.StateMachine.Macross.StateTransition;

namespace EngineNS.Bricks.StateMachine.Macross.CompoundState
{
    [GraphElement(typeof(TtGraphElement_TimedCompoundStateEntry))]
    public class TtTimedCompoundStateEntryClassDescription : IDescription
    {
        [Rtti.Meta]
        [Browsable(false)]
        public List<TtTimedStateTransitionClassDescription> Transitions { get; set; } = new();
        [Rtti.Meta]
        [Browsable(false)]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get=> Parent.Name; set { } }
        [Browsable(false)]
        public IDescription Parent { get; set; } = null;

        public bool AddTransition(TtTimedStateTransitionClassDescription transition)
        {
            Transitions.Add(transition);
            transition.Parent = this;
            return true;
        }

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

        public bool RemoveTransition(TtTimedStateTransitionClassDescription transition)
        {
            Transitions.Remove(transition);
            transition.Parent = null;
            return true;
        }
    }
}
