using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class UFileDialog
    {
        public ImGui.ImGuiFileDialog mFileDialog = ImGui.ImGuiFileDialog.CreateInstance();
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
    public partial class UEditor
    {
        public Forms.UFileDialog FileDialog { get; set; } = new Forms.UFileDialog();
    }
}

