using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UEditorSettings : IRootForm
    {
        public UEditorSettings()
        {
            UEngine.RootFormManager.RegRootForm(this);
        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await SettingsPropGrid.Initialize();

            SettingsPropGrid.Target = this;
            return true;
        }
        
        public UEngineConfig Config
        {
            get
            {
                return UEngine.Instance.Config;
            }
            set
            {
                UEngine.Instance.Config = value;
            }
        }
        public void Dispose() { }
        bool mVisible = true;
        public bool Visible 
        { 
            get => mVisible; 
            set => mVisible = value; 
        }
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public EGui.Controls.PropertyGrid.PropertyGrid SettingsPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);

            Vector2 size = Vector2.Zero;
            var fileDlg = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            if (EGui.UIProxy.DockProxy.BeginMainForm("EditorSettings", this, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.Button("Save"))
                {
                    fileDlg.OpenModal("ChooseConfigKey", "Choose Config", ".cfg", UEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game));
                }
                SettingsPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndMainForm();
            if (fileDlg.DisplayDialog("ChooseConfigKey"))
            {
                if (fileDlg.IsOk() == true)
                {
                    var sltFile = fileDlg.GetFilePathName();

                    Config.SaveConfig(sltFile);
                }
                fileDlg.CloseDialog();
            }
        }
    }
}
