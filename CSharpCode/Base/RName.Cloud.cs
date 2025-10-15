using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public partial class RName
    {
        public static async Thread.Async.TtTask<bool> UploadToCloud(string name)
        {
            return true;
        }
        public static async Thread.Async.TtTask<bool> DownloadFromCloud(string name)
        {
            return true;
        }
        public static async Thread.Async.TtTask<bool> DownloadAssetFiles(IO.IAssetMeta ameta)
        {
            var path = ameta.AssetName.ParentPath;
            foreach (var i in ameta.AssetFiles)
            {
                if (await DownloadFromCloud(TtFileManager.CombinePath(path, i))==false)
                    return false;
            }
            ameta.IsAssetFilesValid = true;
            return true;
        }
        public static bool SureCloudAMeta(RName rn)
        {
            if (rn.RNameType != ERNameType.Cloud)
                return false;

            lock (rn)
            {
                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                if (ameta != null)
                    return true;
                ameta.IsAssetFilesValid = false;
                var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cloud);
                var file = TtFileManager.CombinePath(root, rn.Name) + IAssetMeta.MetaExt;
                if (TtFileManager.FileExists(file)==false)
                {
                    var ok = DownloadFromCloud(file).GetResultUntilCompleted();
                    if (ok == false)
                    {
                        return false;
                    }
                }
                ameta = TtAssetMetaManager.LoadAMeta(root, rn.RNameType, file);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                DownloadAssetFiles(ameta).AddWaitTask();
            }
            return true;
        }
    }
}
