using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace EngineNS
{
    public partial class RName
    {
        public static async Thread.Async.TtTask<bool> UploadToCloud(string name, string destinationPath)
        {
            return true;
        }
        public static async Thread.Async.TtTask<bool> DownloadFromCloud(string name, string destinationPath)
        {
            var httpClient = new System.Net.Http.HttpClient();
            try
            {
                using var response = await httpClient.GetAsync(TtFileManager.CombinePath(TtEngine.Instance.FileManager.CloudUrlBase, name), HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return false;
                }
                await using var fileStream = new System.IO.FileStream(destinationPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
                await response.Content.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return false;
            }

            return true;
        }
        private async Thread.Async.TtTask DownloadFileWithProgressAsync(string fileUrl, string destinationPath, IProgress<double> progress)
        {
            var _httpClient = new System.Net.Http.HttpClient();
            using var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var receivedBytes = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new System.IO.FileStream(destinationPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                receivedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    progress?.Report((double)receivedBytes / totalBytes);
                }
            }
        }

        public static async Thread.Async.TtTask<bool> DownloadAssetFiles(IO.IAssetMeta ameta)
        {
            var path = ameta.AssetName.ParentPath;
            var absPath = ameta.AssetName.AbsParentPath;
            foreach (var i in ameta.AssetFiles)
            {
                if (await DownloadFromCloud(TtFileManager.CombinePath(path, i.Path), TtFileManager.CombinePath(absPath, i.Path))==false)
                    return false;
            }
            ameta.AssetStatus = IAssetMeta.EAssetStatus.Valid;
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
                
                var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cloud);
                var file = TtFileManager.CombinePath(root, rn.Name) + IAssetMeta.MetaExt;
                if (TtFileManager.FileExists(file)==false)
                {
                    var ok = DownloadFromCloud(TtFileManager.CombinePath(rn.Name, IAssetMeta.MetaExt), file).GetResultUntilCompleted();
                    if (ok == false)
                    {
                        return false;
                    }
                }
                ameta = TtAssetMetaManager.LoadAMeta(root, rn.RNameType, file);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                TtEngine.Instance.FileManager.CloudAssetManager.AddDownloadAsset(ameta);
            }
            return true;
        }
        public static void CheckAssetState(IO.IAssetMeta meta)
        {
            if (meta.AssetName.RNameType!=ERNameType.Cloud)
            {
                return;
            }
            var absPath = meta.AssetName.AbsParentPath;
            meta.AssetStatus = IAssetMeta.EAssetStatus.Valid;
            foreach (var i in meta.AssetFiles)
            {
                if (i.Path==null)
                {
                    continue;
                }
                var hash = i.CalcFileHash(TtFileManager.CombinePath(absPath, i.Path));
                if (hash == null || hash!=i.Hash)
                {
                    TtEngine.Instance.FileManager.CloudAssetManager.AddDownloadAsset(meta);
                    break;
                }
            }
        }
    }
    public class TtCloudAssetManager
    {
        private HashSet<IO.IAssetMeta> DownAssets { get; } = new HashSet<IO.IAssetMeta>();
        public void AddDownloadAsset(IO.IAssetMeta rn)
        {
            lock (DownAssets)
            {
                if (rn.AssetStatus == IAssetMeta.EAssetStatus.PullingFiles)
                    return;
                if (DownAssets.Contains(rn))
                    return;
                rn.AssetStatus = IAssetMeta.EAssetStatus.PullingFiles;
                DownAssets.Add(rn);
            }
        }
        public void Download()
        {
            lock (DownAssets)
            {
                foreach (var i in DownAssets)
                {
                    RName.DownloadAssetFiles(i).AddWaitTask((task) =>
                    {
                        //lock (DownAssets)
                        //{
                        //    DownAssets.Remove(i);
                        //}
                    });
                }
                DownAssets.Clear();
            }   
        }
    }
}

namespace EngineNS.IO
{
    partial class TtFileManager
    {
        public TtCloudAssetManager CloudAssetManager { get; } = new TtCloudAssetManager();
    }
}
