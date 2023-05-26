using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        internal virtual Template.TtUITemplate TemplateInternal => null;

        public bool ApplyTemplate()
        {
            return false;
        }
    }
}
