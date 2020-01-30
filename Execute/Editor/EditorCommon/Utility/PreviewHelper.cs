using EditorCommon.Controls.ResourceBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Utility
{
    public class PreviewHelper
    {
        public static async System.Threading.Tasks.Task<Resources.ResourceInfo> SearchFirshResourceInfo(string resourceType, Func<Resources.ResourceInfo, bool> function)
        {
            if (string.IsNullOrEmpty(resourceType))
                return null;
            var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(1);
            Resources.ResourceInfo tempInfo = null;
            await EngineNS.CEngine.Instance.EventPoster.Post(async () =>
            {
                var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(resourceType);
                foreach (var ext in meta.ResourceExts)
                {
                    var files = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, "*" + ext, System.IO.SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var resInfoFile = file + EditorCommon.Program.ResourceInfoExt;
                        if (!EngineNS.CEngine.Instance.FileManager.FileExists(resInfoFile))
                            continue;
                        var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(resInfoFile, null);
                        if (info != null)
                        {
                            if (function?.Invoke(info) == true)
                            {
                                smp.Release();
                                tempInfo = info;
                            }
                        }
                    }
                }
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);

            await smp.Await();
            return tempInfo;
        }
        public static async System.Threading.Tasks.Task InitPreviewResources(ContentControl previewCtrl, string[] resourceTypes, ulong showSourceInDirSerialId, Func<Resources.ResourceInfo, bool> function)
        {
            List<string> resourceExts = new List<string>();
            for(int i = 0;i<resourceTypes.Length;++i)
            {
                if (string.IsNullOrEmpty(resourceTypes[i]))
                    continue;
                var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(resourceTypes[i]);
                if (meta == null)
                    continue;
                resourceExts.AddRange(meta.ResourceExts);
            }
            
            var showData = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData()
            {
                SearchSubFolder = true,
                FileExts = resourceExts.ToArray(),
            };
            showData.FolderDatas.Add(new ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                RootFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
            });
            if (function != null)
            {
                showData.CompareFuction = (info) =>
                {
                    if (info != null)
                    {
                        if (function?.Invoke(info) == true)
                            return true;

                    }
                    return false;
                };
            }
            await previewCtrl.ShowSourcesInDir(showSourceInDirSerialId, showData);
        }
        public static async System.Threading.Tasks.Task InitPreviewResources(ContentControl previewCtrl, string resourceType, ulong showSourceInDirSerialId, Func<Resources.ResourceInfo, bool> function)
        {
            if (string.IsNullOrEmpty(resourceType))
                return;
            var meta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData(resourceType);
            var showData = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData()
            {
                SearchSubFolder = true,
                FileExts = meta.ResourceExts,
            };
            showData.FolderDatas.Add(new ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
                RootFolder = EngineNS.CEngine.Instance.FileManager.ProjectContent,
            });
            if (function != null)
            {
                showData.CompareFuction = (info) =>
                {
                    if (info != null)
                    {
                        if (function?.Invoke(info) == true)
                            return true;

                    }
                    return false;
                };
            }
            await previewCtrl.ShowSourcesInDir(showSourceInDirSerialId, showData);
        }
        public static Dictionary<string, string> SkeletonAssetMeshDic = new Dictionary<string, string>();
        public static async System.Threading.Tasks.Task GetPreviewMeshBySkeleton(Resources.IResourceInfoPreviewForEditor rInfoForEditor)
        {
            if (!string.IsNullOrEmpty(rInfoForEditor.PreViewMesh) && rInfoForEditor.PreViewMesh != "null")
            {
                if (!SkeletonAssetMeshDic.ContainsKey(rInfoForEditor.SkeletonAsset))
                    SkeletonAssetMeshDic.Add(rInfoForEditor.SkeletonAsset, rInfoForEditor.PreViewMesh);
            }
            else
            {
                string preViewMesh = "";
                if (!string.IsNullOrEmpty(rInfoForEditor.SkeletonAsset) && rInfoForEditor.SkeletonAsset != "null")
                {
                    if (SkeletonAssetMeshDic.TryGetValue(rInfoForEditor.SkeletonAsset, out preViewMesh) == false)
                    {
                        var meshInfo = await EditorCommon.Utility.PreviewHelper.SearchFirshResourceInfo(EngineNS.Editor.Editor_RNameTypeAttribute.Mesh, (info) =>
                        {
                            var tempInfo = info as EditorCommon.ResourceInfos.MeshResourceInfo;
                            if (tempInfo.SkeletonAsset == rInfoForEditor.SkeletonAsset)
                                return true;
                            else
                                return false;
                        });
                        if (meshInfo != null)
                        {
                            rInfoForEditor.PreViewMesh = meshInfo.ResourceName.Name;
                            var rInfo = rInfoForEditor as Resources.ResourceInfo;
                            await rInfo.Save();
                            if (!SkeletonAssetMeshDic.ContainsKey(rInfoForEditor.SkeletonAsset))
                                SkeletonAssetMeshDic.Add(rInfoForEditor.SkeletonAsset, rInfoForEditor.PreViewMesh);
                        }
                    }
                    else
                    {
                        rInfoForEditor.PreViewMesh = preViewMesh;
                        var rInfo = rInfoForEditor as Resources.ResourceInfo;
                        await rInfo.Save();
                    }
                }
            }
        }
    }
}
