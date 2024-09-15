using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    public class KeyValueCreator
    {
        public string CtrlName;
        public object KeyData;
        public object ValueData;
        public PropertyGrid PGKeyData = new PropertyGrid();
        public PropertyGrid PGValueData = new PropertyGrid();
        public EGui.Controls.UTypeSelector KeyTypeSlt = new EGui.Controls.UTypeSelector();
        public EGui.Controls.UTypeSelector ValueTypeSlt = new EGui.Controls.UTypeSelector();
        public bool CreateFinished = false;
        public unsafe void OnDraw(string ctrlId)
        {
            if (ImGuiAPI.BeginPopup(ctrlId, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                if (ImGuiAPI.CollapsingHeader("Key", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    KeyTypeSlt.CtrlId = "KeyTypeSlt";
                    var saved = KeyTypeSlt.SelectedType;
                    KeyTypeSlt.OnDraw(-1, 6);
                    if (KeyTypeSlt.SelectedType != saved)
                    {
                        KeyData = Rtti.TtTypeDescManager.CreateInstance(KeyTypeSlt.SelectedType.SystemType);
                    }

                    ImGuiAPI.Separator();

                    if (ImGuiAPI.TreeNode("Settings"))
                    {
                        PGKeyData.Target = KeyData;
                        PGKeyData.OnDraw(false, false, false);
                        ImGuiAPI.TreePop();
                    }
                }

                if (ImGuiAPI.CollapsingHeader("Value", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    ValueTypeSlt.CtrlId = "ValueTypeSlt";
                    var saved = ValueTypeSlt.SelectedType;
                    ValueTypeSlt.OnDraw(-1, 6);
                    if (ValueTypeSlt.SelectedType != saved)
                    {
                        ValueData = Rtti.TtTypeDescManager.CreateInstance(ValueTypeSlt.SelectedType.SystemType);
                    }

                    ImGuiAPI.Separator();

                    if (ImGuiAPI.TreeNode("Settings"))
                    {
                        PGValueData.Target = ValueData;
                        PGValueData.OnDraw(false, false, false);
                        ImGuiAPI.TreePop();
                    }
                }

                ImGuiAPI.Separator();
                var sz = new Vector2(-1, 0);
                if (ImGuiAPI.Button("AddItem", in sz))
                {
                    if (KeyData != null && ValueData != null)
                    {
                        CreateFinished = true;
                        ImGuiAPI.CloseCurrentPopup();
                    }
                }
                ImGuiAPI.EndPopup();
            }
        }
    }
}
