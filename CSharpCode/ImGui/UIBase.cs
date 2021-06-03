using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui
{
    public interface IPanel
    {
        bool Visible { get; set; }
        void OnDraw();
        uint DockId { get; set; }
        ImGuiCond_ DockCond { get; set; }
    }
    public class UIFormBase
    {
        public bool Visible { get; set; } = true;
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
            Visible = visible;
            Name = name;
            ElementType = Rtti.UTypeDesc.TypeOf(type);
            LayoutIndex = 0;
            LayoutOffset = 0;
        }
        public bool Visible;
        public string Name;
        public Rtti.UTypeDesc ElementType;
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
