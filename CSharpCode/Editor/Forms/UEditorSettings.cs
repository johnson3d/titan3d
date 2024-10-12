using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class TtEditorSettings : IRootForm
    {
        public TtEditorSettings()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await SettingsPropGrid.Initialize();

            SettingsPropGrid.Target = Config;
            return true;
        }
        
        public TtEngineConfig Config
        {
            get
            {
                return TtEngine.Instance.Config;
            }
            set
            {
                TtEngine.Instance.Config = value;
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
            var fileDlg = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            var result = EGui.UIProxy.DockProxy.BeginMainForm("EditorSettings", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                DockId = ImGuiAPI.GetWindowDockID();
                if (ImGuiAPI.Button("Save"))
                {
                    fileDlg.OpenModal("ChooseConfigKey", "Choose Config", ".cfg", TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game));
                }
                SettingsPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
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
