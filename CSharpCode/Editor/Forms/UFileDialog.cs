using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UFileDialog
    {
        public ImGui.ImGuiFileDialog mFileDialog = ImGui.ImGuiFileDialog.CreateInstance();
        //public UFileDialog()
        //{
        //    //mFileDialog.SetCurrentPath()
        //}
        ~UFileDialog()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            mFileDialog.Dispose();
        }
    }
}

namespace EngineNS.Editor
{
    public partial class TtEditor
    {
        public Forms.UFileDialog FileDialog { get; set; } = new Forms.UFileDialog();
        //public EGui.Controls.UContentBrowser RNamePopupContentBrowser = new EGui.Controls.UContentBrowser()
        //{
        //    DrawInWindow = false,
        //    CreateNewAssets = false,
        //    ItemSelectedAction = (asset) =>
        //    {
        //        ImGuiAPI.CloseCurrentPopup();
        //    },
        //};
        public static EGui.Controls.TtContentBrowser NewPopupContentBrowser()
        {
            var result = new EGui.Controls.TtContentBrowser()
            {
                DrawInWindow = false,
                CreateNewAssets = false,
                ItemSelectedAction = (asset) =>
                {
                    ImGuiAPI.CloseCurrentPopup();
                },
            };
            _ = result.Initialize();
            return result;
        }
    }
}

