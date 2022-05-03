using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.UIProxy
{
    public class InputText
    {
        public unsafe static void OnDraw(string label, ref string text, ImGuiInputTextFlags_ flags, ImGuiAPI.FDelegate_ImGuiInputTextCallback callback, void* user_Data)
        {
            var len = string.IsNullOrEmpty(text) ? 256 : text.Length + 256;
            using (var buffer = BigStackBuffer.CreateInstance(len))
            {
                buffer.SetText(text);
                ImGuiAPI.InputText(label, buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, user_Data);
                text = buffer.AsText();
            }
        }
    }
}