using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.UIEditor.Elements
{
    public class InputText : ControlBase
    {
        public string TextValue { get; set; }
        public float WidgetWidth { get; set; } = -1;
        public Vector4 TextColor { get; set; } = new Vector4(1, 1, 1, 1);
        public int MaxInputChars { get; set; } = 256;
        public ImGuiInputTextFlags_ InputTextFlags { get; set; } = ImGuiInputTextFlags_.ImGuiInputTextFlags_None;
        public DataBinder Binder { get; set; } = null;
        public unsafe override void GenCode(Form form)
        {
            if (WidgetWidth > 0)
            {
                form.AddLine($"ImGuiAPI.SetNextItemWidth({WidgetWidth});");
            }
            form.AddLine($"var tmpColor = new {typeof(Vector4).FullName}({TextColor.X}, {TextColor.Y}, {TextColor.Z}, {TextColor.W});");
            if (Binder != null)
            {
                //var buffer = new BigStackBuffer_PtrType(512, true);
                //var pSrcString = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(valName);
                //CoreSDK.StrCpy(buffer.GetBuffer(), pSrcString.ToPointer());
                //ImGuiAPI.InputText($"##{Name}", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                //if (CoreSDK.StrCmp(buffer.GetBuffer(), pSrcString) != 0)
                //{
                //    System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)buffer.GetBuffer());
                //}
                //System.Runtime.InteropServices.Marshal.FreeHGlobal(pSrcString);                
                //buffer.DestroyMe();

                string valName;
                var realType = Binder.GenPreCode(form, typeof(string), out valName);

                form.AddLine($"var buffer = new {typeof(BigStackBuffer_PtrType).FullName}({MaxInputChars}, true);");
                form.AddLine($"var pSrcString = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi({valName});");
                form.AddLine($"CoreSDK.StrCpy(buffer.GetBuffer(), pSrcString.ToPointer());");
                form.AddLine($"var flags = ({typeof(ImGuiInputTextFlags_).FullName})({System.Convert.ToUInt32(InputTextFlags)});//{InputTextFlags}");
                form.AddLine($"ImGuiAPI.InputText(\"##{Name}\", buffer.GetBuffer(), (uint)buffer.GetSize(), flags, null, (void*)0);");
                form.AddLine($"if (CoreSDK.StrCmp(buffer.GetBuffer(), pSrcString.ToPointer()) != 0)");
                form.PushBrackets();
                form.AddLine($"{valName} = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)buffer.GetBuffer());");
                form.PopBrackets();
                form.AddLine($"System.Runtime.InteropServices.Marshal.FreeHGlobal(pSrcString);");
                form.AddLine($"buffer.DestroyMe();");

                Binder.GenPostCode(form, typeof(string), realType, valName);
            }
            else
            {
                form.AddLine($"var buffer = new {typeof(BigStackBuffer_PtrType).FullName}({MaxInputChars}, true);");
                form.AddLine($"CoreSDK.CopyString2Ansi(buffer, \"{TextValue}\");");
                form.AddLine($"var flags = ({typeof(ImGuiInputTextFlags_).FullName})({System.Convert.ToUInt32(InputTextFlags)});//{InputTextFlags}");
                form.AddLine($"ImGuiAPI.InputText(\"##{Name}\", buffer.GetBuffer(), (uint)buffer.GetSize(), flags, null, (void*)0);");
                form.AddLine($"buffer.DestroyMe();");
            }
        }
    }
}
