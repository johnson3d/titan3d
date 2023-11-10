using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Base.Graph;
using EngineNS.DesignMacross.Base.Outline;
using EngineNS.DesignMacross.TimedStateMachine.CompoundState;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;

namespace EngineNS.DesignMacross.TimedStateMachine.CompoundState
{
    [GraphElement(typeof(TtGraphElement_TimedCompoundStateHub))]
    public class TtTimedCompoundStateHubClassDescription : IDescription
    {
        [Browsable(false)]
        public IDescription Parent { get; set; }
        [Rtti.Meta]
        [Browsable(false)]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Browsable(false)]
        public string Name { get=> TimedCompoundStateClassDescription.Name + "_Hub"; set { } }
        [Rtti.Meta]
        [Browsable(false)]
        public Guid TimedCompoundStateClassDescriptionId { get; set; } = Guid.Empty;
        private TtTimedCompoundStateClassDescription mTimedCompoundStateClassDescription = null;
        [Browsable(false)]
        public TtTimedCompoundStateClassDescription TimedCompoundStateClassDescription 
        {
            get
            {
                if(mTimedCompoundStateClassDescription == null)
                {
                    if (Parent is TtTimedCompoundStateClassDescription parentCompoundStateClassDescription)
                    {
                        mTimedCompoundStateClassDescription = parentCompoundStateClassDescription.StateMachineClassDescription.CompoundStates.Find((candidate) => { return candidate.Id == TimedCompoundStateClassDescriptionId; });
                        Debug.Assert(mTimedCompoundStateClassDescription != null);
                    }
                }

                return mTimedCompoundStateClassDescription;
            }
            set
            {
                mTimedCompoundStateClassDescription = value;
            }
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

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {

        }
        #endregion ISerializer
    }
}
