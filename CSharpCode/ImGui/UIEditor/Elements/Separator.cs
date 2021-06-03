using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.UIEditor.Elements
{
    public class Separator : ControlBase
    {
        public override void GenCode(Form form)
        {
            form.AddLine($"ImGuiAPI.Separator();");
        }
    }
}
