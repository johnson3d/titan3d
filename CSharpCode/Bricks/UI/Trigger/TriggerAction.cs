using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Trigger;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Trigger
{
    public abstract class TtTriggerActionBase
    {
        protected bool mIsSealed = false;

        TtTriggerBase mHostTrigger;
        public TtTriggerBase HostTrigger => mHostTrigger;

        public abstract void Invoke(IBindableObject obj);
        public virtual void Seal(TtTriggerBase host)
        {
            if (mIsSealed)
                return;

            mHostTrigger = host;
        }
    }

    public class TtTriggerValueSetAction : TtTriggerActionBase
    {
        public override void Invoke(IBindableObject obj)
        {
            throw new NotImplementedException();
        }
    }
}
