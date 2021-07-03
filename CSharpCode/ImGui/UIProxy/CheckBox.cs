using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class CheckBox
    {
        public static bool DrawCheckBox(string name, ref bool value, bool readOnly)
        {
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding);
            var retValue = ImGuiAPI.Checkbox(name, ref value);
            ImGuiAPI.PopStyleVar(1);
            return retValue && !readOnly;
        }
        public static bool DrawCheckBoxTristate(string name, ref int value, bool readOnly)
        {
            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.Instance.PGCheckboxFramePadding);
            var retValue = ImGuiAPI.CheckBoxTristate(name, ref value);
            ImGuiAPI.PopStyleVar(1);
            return retValue && !readOnly;
        }
    }
}
