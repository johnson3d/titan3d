using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UInspector : IRootForm, EGui.IPanel
    {
        public UInspector()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get => mPropertyGrid.DockId; set => mPropertyGrid.DockId = value; }
        public ImGuiCond_ DockCond { get => mPropertyGrid.DockCond; set => mPropertyGrid.DockCond = value; }
        public bool ShowReadOnly { get; set; } = true;
        protected EGui.Controls.PropertyGrid.PropertyGrid mPropertyGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid PropertyGrid { get => mPropertyGrid; }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            mPropertyGrid.OnDraw(ShowReadOnly, true, false);
        }
    }
}
