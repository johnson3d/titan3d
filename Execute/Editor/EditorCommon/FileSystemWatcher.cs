using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon
{
    public class FileSystemWatcherProcess
    {
        static bool mEnable = false;
        public static bool Enable
        {
            get => mEnable;
            set
            {
                mEnable = false;
                if(mResourceFilesWatcher != null)
                    mResourceFilesWatcher.EnableRaisingEvents = value;
            }
        }

        static FileSystemWatcher mResourceFilesWatcher;
        public static void InitializeFileSystemWatcher()
        {
            if (mResourceFilesWatcher == null)
            {
                mResourceFilesWatcher = new FileSystemWatcher(EngineNS.CEngine.Instance.FileManager.ProjectContent.Replace("/", "\\"));
                mResourceFilesWatcher.EnableRaisingEvents = true;
                mResourceFilesWatcher.IncludeSubdirectories = true;
                mResourceFilesWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                mResourceFilesWatcher.Renamed += (object sender, RenamedEventArgs e) =>
                {
                    OnFileSystemWatcherEventRise(sender, e);
                };
                mResourceFilesWatcher.Created += (object sender, FileSystemEventArgs e) =>
                {
                    OnFileSystemWatcherEventRise(sender, e);
                };
                mResourceFilesWatcher.Deleted += (object sender, FileSystemEventArgs e) =>
                {
                    OnFileSystemWatcherEventRise(sender, e);
                };
                mResourceFilesWatcher.Changed += (object sender, FileSystemEventArgs e) =>
                {
                    OnFileSystemWatcherEventRise(sender, e);
                };
            }
        }

        static void OnFileSystemWatcherEventRise(object sender, FileSystemEventArgs e)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                string absFileName = "";
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        //case WatcherChangeTypes.Created:
                        {
                            absFileName = e.FullPath.Replace("\\", "/");
                        }
                        break;
                    case WatcherChangeTypes.Renamed:
                        {
                            var re = e as System.IO.RenamedEventArgs;
                            absFileName = re.FullPath.Replace("\\", "/");
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(absFileName))
                {
                    if (EngineNS.CEngine.Instance.FileManager.FileExists(absFileName))
                    {
                        absFileName = absFileName.Replace("//", "/");
                        var path = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(absFileName);
                        if (path.Length > 0 && path[path.Length - 1] == '/')
                            path = path.Remove(path.Length - 1);

                        var file = absFileName + EditorCommon.Program.ResourceInfoExt;
                        if (!System.IO.File.Exists(file))
                        {
                            file = EngineNS.CEngine.Instance.FileManager.RemoveExtension(absFileName) + EngineNS.CEngineDesc.TextureExtension + EditorCommon.Program.ResourceInfoExt;
                            if (!System.IO.File.Exists(file))
                                return false;
                        }

                        var rName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(absFileName);
                        var result = await EditorCommon.Controls.ResourceBrowser.BrowserControl.OnResourceChanged(rName);
                        if (!result)
                        {
                            var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null, true);
                            if (resInfo != null)
                            {
                                await EngineNS.CEngine.Instance.GameEditorInstance.RefreshResourceInfoReferenceDictionary(resInfo);
                                var frlRes = resInfo as EditorCommon.Resources.IResourceInfoForceReload;
                                if (frlRes != null)
                                {
                                    var exts = frlRes.GetFileSystemWatcherAttentionExtensions();
                                    var fileExt = rName.GetExtension(true);
                                    foreach (var ext in exts)
                                    {
                                        if (string.Equals(ext, fileExt, StringComparison.OrdinalIgnoreCase))
                                        {
                                            frlRes?.ForceReload();
                                            await resInfo.GetSnapshotImage(true);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }
    }
}
