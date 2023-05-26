using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Template
{
    public abstract class TtUITemplate
    {
        protected abstract void CheckTemplateParentValid(TtUIElement templateParent);

        bool mSealed = false;
        public void Seal()
        {
            if (mSealed)
                return;

            ProcessTemplateBeforeSeal();

            mSealed = true;
        }

        protected virtual void ProcessTemplateBeforeSeal()
        {

        }
    }
}
