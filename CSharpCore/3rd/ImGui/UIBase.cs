using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using EngineNS;

namespace CSharpCode
{
    public class UIFormBase
    {
        public CppBool Visible { get; set; } = CppBool.FromBoolean(true);
        public string Name { get; set; } = "Winform";
        public virtual object BindTarget { get { return null; } set { } }
        public virtual unsafe void DrawForm()
        {
            var bOpen = Visible;
            if (ImGuiAPI.Begin(Name, &bOpen, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                try
                {
                    OnDraw();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            ImGuiAPI.End();
        }
        public virtual unsafe void OnDraw()
        {

        }
    }
    public class UIElementData
    {
        public UIElementData(bool visible, string name, Type type)
        {
            Visible = CppBool.FromBoolean(visible);
            Name = name;
            ElementType = type;
            LayoutIndex = 0;
            LayoutOffset = 0;
        }
        public CppBool Visible;
        public string Name;
        public System.Type ElementType;
        public int LayoutIndex;
        public int LayoutOffset;
    }
    public class ColumnsElementData : UIElementData
    {
        public ColumnsElementData(bool visible, string name, Type type)
            : base(visible, name, type)
        {

        }
        public Dictionary<int, int> ColumnsOffset = new Dictionary<int, int>();
    }
}
