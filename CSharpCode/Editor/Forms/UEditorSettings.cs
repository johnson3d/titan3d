using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UEditorSettings : Graphics.Pipeline.IRootForm
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
        public void Cleanup() { }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;

        public EGui.Controls.PropertyGrid.PropertyGrid SettingsPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);

            Vector2 size = Vector2.Zero;
            var fileDlg = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            if (ImGuiAPI.Begin("EditorSettings", null, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.Button("Save"))
                {
                    fileDlg.OpenModal("ChooseConfigKey", "Choose Config", ".cfg", UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Game));
                }
                SettingsPropGrid.OnDraw(true, false, false);
            }
            ImGuiAPI.End();
            if (fileDlg.DisplayDialog("ChooseConfigKey"))
            {
                if (fileDlg.IsOk() == true)
                {
                    var sltFile = fileDlg.GetFilePathName();

                    IO.FileManager.SaveObjectToXml(sltFile, Config);
                }
                fileDlg.CloseDialog();
            }
        }
    }
}
