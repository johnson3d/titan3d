using EngineNS.UI.Bind;
using EngineNS.UI.Trigger;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        List<TtTriggerActionBase> mDeferredActionss = new List<TtTriggerActionBase>();
        List<TtTriggerBase> mDefferedTriggers = new List<TtTriggerBase>();

        void InovkeDeferredActions()
        {
            for(int i=0; i<mDeferredActionss.Count; i++)
            {
                mDeferredActionss[i].Invoke(this);
            }
        }

        public void DeferActions(List<TtTriggerActionBase> actions)
        {
            mDeferredActionss.AddRange(actions);
        }

        public void AddTrigger(TtTriggerBase trigger)
        {
            if(TemplateInternal != null && !HasTemplateGeneratedSubTree)
            {
                mDefferedTriggers.Add(trigger);
            }
            else
            {
                trigger.Seal(this, TemplateInternal);
            }
        }
    }
}
