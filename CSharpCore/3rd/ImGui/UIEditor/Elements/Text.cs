using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.UIEditor.Elements
{
    public class Text : ControlBase
    {
        public string TextValue { get; set; }
        public float WidgetWidth { get; set; } = -1;
        public Vector4 TextColor { get; set; } = new Vector4(1, 1, 1, 1);
        public DataBinder Binder { get; set; } = null;
        public override void GenCode(Form form)
        {
            //ImGuiAPI.SetNextItemWidth(WidgetWidth);
            //{
            //    var tmpColor = new Vector4(TextColor.X, TextColor.Y, TextColor.Z, TextColor.W);
            //    string textout = TextValue;
            //    if (this.BindObject != null)
            //    {
            //        this.BindObject as Binder.BindType.FullName;
            //    }
            //    ImGuiAPI.TextColored(ref tmpColor, $"{textout}");
            //}
            if (WidgetWidth > 0)
            {
                form.AddLine($"ImGuiAPI.SetNextItemWidth({WidgetWidth})");
            }
            form.AddLine($"var tmpColor = new {typeof(Vector4).FullName}({TextColor.X}, {TextColor.Y}, {TextColor.Z}, {TextColor.W});");
            if (Binder != null)
            {
                string valName;
                var realType = Binder.GenPreCode(form, typeof(string), out valName);
                form.AddLine($"ImGuiAPI.TextColored(ref tmpColor, {valName});");
                Binder.GenPostCode(form, typeof(string), realType, valName);
            }
            else
            {
                form.AddLine($"ImGuiAPI.TextColored(ref tmpColor, \"{TextValue}\");");
            }
        }
    }
}
