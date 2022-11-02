using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class InputText
    {
        public unsafe static void OnDraw(string label, ref string text, ImGuiInputTextFlags_ flags = ImGuiInputTextFlags_.ImGuiInputTextFlags_None, ImGuiAPI.FDelegate_ImGuiInputTextCallback callback = null, object user_Data = null)
        {
            var len = string.IsNullOrEmpty(text) ? 256 : text.Length + 256;
            using (var buffer = BigStackBuffer.CreateInstance(len))
            {
                buffer.SetText(text);
                ImGuiAPI.PushID(label);
                ImGuiAPI.Text(label);
                ImGuiAPI.SameLine(0, 0);
                if (user_Data == null)
                    ImGuiAPI.InputText("", buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, (void*)0);
                else
                    ImGuiAPI.InputText("", buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, (void*)0);
                ImGuiAPI.PopID();
                text = buffer.AsText();
            }
        }
    }
}