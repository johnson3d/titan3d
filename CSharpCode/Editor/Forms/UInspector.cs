using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UInspector : IRootForm, EGui.IPanel
    {
        public UInspector()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get => mPropertyGrid.DockId; set => mPropertyGrid.DockId = value; }
        public ImGuiCond_ DockCond { get => mPropertyGrid.DockCond; set => mPropertyGrid.DockCond = value; }
        public bool ShowReadOnly { get; set; } = true;
        protected EGui.Controls.PropertyGrid.PropertyGrid mPropertyGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid PropertyGrid { get => mPropertyGrid; }
        
        public void Cleanup()
        {
            mPropertyGrid.Cleanup();
        }

        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            return await mPropertyGrid.Initialize();
        }
        
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(EGui.UIProxy.DockProxy.MainFormDockClass.ClassId, ImGuiCond_.ImGuiCond_Appearing);
            ImGuiAPI.SetNextWindowClass(EGui.UIProxy.DockProxy.MainFormDockClass);
            mPropertyGrid.OnDraw(ShowReadOnly, true, false);
        }
    }
}
