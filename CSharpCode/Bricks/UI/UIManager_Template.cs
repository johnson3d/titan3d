using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Template;
using EngineNS.UI.Trigger;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIManager
    {
        public static Dictionary<UTypeDesc, TtUITemplate> SystemDefaultTemplates = new Dictionary<UTypeDesc, TtUITemplate>();
        Dictionary<UTypeDesc, TtUITemplate> mDefaultTemplates = new Dictionary<UTypeDesc, TtUITemplate>();
        Dictionary<string, TtUITemplate> mTemplates = new Dictionary<string, TtUITemplate>();
        public void ClearTemplates()
        {
            SystemDefaultTemplates.Clear();
            mDefaultTemplates.Clear();
            mTemplates.Clear();
        }
        public TtUITemplate GetDefaultTemplate(UTypeDesc type)
        {
            if (mDefaultTemplates.TryGetValue(type, out var defaultTemplate))
                return defaultTemplate;
            if (SystemDefaultTemplates.TryGetValue(type, out var sysDefTemplate))
                return sysDefTemplate;
            return null;
        }
    }
}
