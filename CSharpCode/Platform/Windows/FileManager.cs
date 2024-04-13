using System;
using System.Collections.Generic;

namespace EngineNS.IO
{
    partial class TtFileManager
    {
        partial void InitDirectory(string[] args)
        {
            var mBin = System.IO.Directory.GetCurrentDirectory();//AppDomain.CurrentDomain.BaseDirectory;
            var root = GetBaseDirectory(mBin, 1);

            //SetRoot(ERootDir.Root, root);

            var cfg = UEngine.FindArgument(args, "publish=");
            if (cfg == "true")
            {
                SetRoot(ERootDir.Execute, root + "publishbin");
            }
            else
            {
                SetRoot(ERootDir.Execute, root + "binaries");// AppDomain.CurrentDomain.BaseDirectory);
            }
            SetRoot(ERootDir.Engine, root + "enginecontent");
            SetRoot(ERootDir.Game, root + "content");
            SetRoot(ERootDir.PluginContent, root + "plugincontent");
            SetRoot(ERootDir.Editor, root + "editorcontent");
            SetRoot(ERootDir.Cache, root + "cache");
            SetRoot(ERootDir.Plugin, root + $"binaries/Plugins/");
            SetRoot(ERootDir.EngineSource, root);
            SetRoot(ERootDir.GameSource, root);
        }
    }
    public partial class UOpenFileDialog
    {
        partial void ShowDialogImpl(ref int result)
        {
            //var fileDialog = new System.Windows.Forms.OpenFileDialog();
            //fileDialog.Multiselect = true;
            //fileDialog.Title = this.Title;
            //fileDialog.Filter = this.Filter;
            //fileDialog.InitialDirectory = this.InitialDirectory;
            //fileDialog.Multiselect = this.Multiselect;
            //fileDialog.ShowReadOnly = this.ShowReadOnly;
            //result = (int)fileDialog.ShowDialog();
            //mFileName = fileDialog.FileName;
            //mFileNames = fileDialog.FileNames;
        }
    }
}
