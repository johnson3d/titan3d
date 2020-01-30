using EngineNS.Bricks.AssetImpExp.Creater;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.AssetImpExp
{
    public delegate AssetImportMessageType AssetImportMessageHandle(object sender, string fileName, string assetName, AssetImportMessageType type, int level, string resourceDetial, float progress);
    public class AssetImpExpManager
    {
        ~AssetImpExpManager()
        {

        }
        public event AssetImportMessageHandle OnAssetImportMessageDumping;
        Dictionary<string, CGfxFileImportOption> mFileImportOptions = new Dictionary<string, CGfxFileImportOption>();
        Dictionary<string, FBX.CGfxFBXImporter> mFBXImporters = new Dictionary<string, FBX.CGfxFBXImporter>();
        string SingleFileName = "";
        CGfxAsset_File SingleAsset_File = null;
        //预读？
        public async System.Threading.Tasks.Task<CGfxFileImportOption> PreImport(string fileName, string absSavePath)
        {
            CGfxFileImportOption fileImportOption = new CGfxFileImportOption();
            var fBXImporter = new FBX.CGfxFBXImporter();
            var result = await fBXImporter.PreImport(fileName, fileImportOption);
            if (result)
            {
                mFileImportOptions.Add(fileName, fileImportOption);
                fileImportOption.BuildOptionsDictionary();
                mFBXImporters.Add(fileName, fBXImporter);
                fileImportOption.InitializeSavePath(absSavePath);
                return fileImportOption;
            }
            return null;
        }
        public bool IsCancle = false;
        public async System.Threading.Tasks.Task Import(string fileName)
        {
            SingleFileName = fileName;
            if (!mFileImportOptions.ContainsKey(fileName))
                return;
            SingleAsset_File = new CGfxAsset_File();
            SingleAsset_File.FileOption = mFileImportOptions[fileName];
            using (var it = SingleAsset_File.FileOption.ObjectOptions.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (IsCancle)
                        return;
                    var hash = it.Current.Key;
                    var assetOption = it.Current.Value;
                    if (!assetOption.IsImport)
                        continue;
                    var creater = CGfxAssetCreater.CreateAssetCreater(assetOption);
                    creater.OnCreaterAssetImportMessageDumping += AssetCreater_OnCreaterAssetImportMessageDumping;
                    if (!creater.CheckIfNeedImport())
                    {
                        creater.OnCreaterAssetImportMessageDumping -= AssetCreater_OnCreaterAssetImportMessageDumping;
                        continue;
                    }
                    creater.ImportPercent = 0;
                    SingleAsset_File.AddCreaters(hash, creater);
                }
            }
            await mFBXImporters[fileName].Import(SingleAsset_File);
            await SingleAsset_File.SaveAsset();
            mFBXImporters.Remove(fileName);
        }
        public float ImportPercent = 0;
        private AssetImportMessageType AssetCreater_OnCreaterAssetImportMessageDumping(object sender, AssetImportMessageType type, int level, string info, float percent)
        {
            var totalPercent = 0.0f;
            using (var it = SingleAsset_File.AssetCreaters.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var creater = it.Current.Value;
                    totalPercent += creater.ImportPercent;

                }
            }
            return (AssetImportMessageType)OnAssetImportMessageDumping?.Invoke(this, SingleAsset_File.FileOption.Name, SingleAsset_File.FileOption.Name, type, level, info, totalPercent / SingleAsset_File.AssetCreaters.Count);
        }

        //slow
        private async System.Threading.Tasks.Task<MutiFilesImportOption> PreImport(string[] fileNames, string absSavePath)
        {
            MutiFilesImportOption mutiFilesImportOption = new MutiFilesImportOption();
            for (int i = 0; i < fileNames.Length; i++)
            {
                CGfxFileImportOption fileImportOption = await PreImport(fileNames[i], absSavePath);
                if (fileImportOption != null)
                {
                    mutiFilesImportOption.FileImportOptions.Add(fileImportOption);
                }
            }
            return mutiFilesImportOption;
        }
        //slow
        private async System.Threading.Tasks.Task Import(string[] fileNames)
        {
            //使用通用AssetImportOption
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (IsCancle)
                    return;
                if (!mFileImportOptions.ContainsKey(fileNames[i]))
                    continue;
                var assetFile = new CGfxAsset_File();
                assetFile.FileOption = mFileImportOptions[fileNames[i]];
                using (var it = assetFile.FileOption.ObjectOptions.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        if (IsCancle)
                            return;
                        var hash = it.Current.Key;
                        var assetOption = it.Current.Value;
                        if (!assetOption.IsImport)
                            continue;
                        var creater = CGfxAssetCreater.CreateAssetCreater(assetOption);
                        assetFile.AddCreaters(hash, creater);
                    }
                }
                await mFBXImporters[fileNames[i]].Import(assetFile);
                await assetFile.SaveAsset();
            }

        }
        private AssetImportMessageType MutiFiles_OnCreaterAssetImportMessageDumping(object sender, AssetImportMessageType type, int level, string info, float percent)
        {
            var creater = sender as CGfxAssetCreater;
            return (AssetImportMessageType)OnAssetImportMessageDumping?.Invoke(this, creater.AssetImportOption.Name, creater.AssetImportOption.Name, type, level, info, 0);
        }
        public async System.Threading.Tasks.Task ImportMutiFiles(string[] fileNames, string absSavePath, MutiFilesImportOption mutiFilesImportOption)
        {
            var per = 1.0f / fileNames.Length;
            float currentPercent = 0;
            OnAssetImportMessageDumping?.Invoke(this, "", "", AssetImportMessageType.AMT_Import, 0, "ImportStart", 0);
            for (int i = 0; i < fileNames.Length; ++i)
            {
                if (IsCancle)
                    return;
                var fileName = fileNames[i];
                OnAssetImportMessageDumping?.Invoke(this, "", "", AssetImportMessageType.AMT_Import, 0, "ImportStart: " + fileName, currentPercent);
                CGfxFileImportOption fileImportOption = new CGfxFileImportOption();
                var fBXImporter = new FBX.CGfxFBXImporter();
                var result = await fBXImporter.PreImport(fileName, fileImportOption);
                if (result)
                {
                    mFileImportOptions.Add(fileName, fileImportOption);
                    fileImportOption.BuildOptionsDictionary();
                    mFBXImporters.Add(fileName, fBXImporter);
                    fileImportOption.InitializeSavePath(absSavePath);

                    fileImportOption.ImportMesh = mutiFilesImportOption.ImportMesh;
                    fileImportOption.ImportAnimation = mutiFilesImportOption.ImportAnimation;
                    fileImportOption.ConvertSceneUnit = mutiFilesImportOption.ConvertSceneUnit;
                    fileImportOption.Scale = mutiFilesImportOption.Scale;

                    var assetFile = new CGfxAsset_File();
                    assetFile.FileOption = mFileImportOptions[fileName];
                    using (var it = assetFile.FileOption.ObjectOptions.GetEnumerator())
                    {
                        while (it.MoveNext())
                        {
                            if (IsCancle)
                                return;
                            var hash = it.Current.Key;
                            var assetOption = it.Current.Value;
                            if (!assetOption.IsImport)
                                continue;
                            if(assetOption.AssetType== ImportAssetType.IAT_Mesh)
                            {
                                var meshOP = assetOption as CGfxMeshImportOption;
                                meshOP.Skeleton = mutiFilesImportOption.Skeleton;
                            }
                            if (assetOption.AssetType == ImportAssetType.IAT_Animation)
                            {
                                var animOP = assetOption as CGfxAnimationImportOption;
                                animOP.Skeleton = mutiFilesImportOption.Skeleton;
                            }
                            var creater = CGfxAssetCreater.CreateAssetCreater(assetOption);
                            creater.OnCreaterAssetImportMessageDumping += MutiFiles_OnCreaterAssetImportMessageDumping;
                            if(!creater.CheckIfNeedImport())
                            {
                                creater.OnCreaterAssetImportMessageDumping -= MutiFiles_OnCreaterAssetImportMessageDumping;
                                continue;
                            }
                            assetFile.AddCreaters(hash, creater);
                        }
                    }
                    await mFBXImporters[fileName].Import(assetFile);
                    await assetFile.SaveAsset();
                    currentPercent += per;
                    OnAssetImportMessageDumping?.Invoke(this, "", "", AssetImportMessageType.AMT_Import, 0, "ImportDone: " + fileName, currentPercent);
                    OnAssetImportMessageDumping?.Invoke(this, "", "", AssetImportMessageType.AMT_Save, 0, "ImportDone: " + fileName, 0.0f);
                }
            }
            //await CEngine.Instance.EventPoster.AwaitMTS_Foreach(fileNames.Length,async (idx, smp) =>
            //{
            //    var fileName = fileNames[idx];
            //    OnAssetImportMessageDumping?.Invoke(this, "", "", AssetImportMessageType.AMT_Import, "ImportStart: " + fileName, currentPercent);
            //    CGfxFileImportOption fileImportOption = new CGfxFileImportOption();
            //    var fBXImporter = new FBX.CGfxFBXImporter();
            //    var result = await fBXImporter.PreImport(fileName, fileImportOption);
            //    if (result)
            //    {
            //        mFileImportOptions.Add(fileName, fileImportOption);
            //        fileImportOption.BuildOptionsDictionary();
            //        mFBXImporters.Add(fileName, fBXImporter);
            //        fileImportOption.InitializeSavePath(absSavePath);

            //        fileImportOption.ImportMesh = mutiFilesImportOption.ImportMesh;
            //        fileImportOption.ImportAnimation = mutiFilesImportOption.ImportAnimation;
            //        fileImportOption.ConvertSceneUnit = mutiFilesImportOption.ConvertSceneUnit;
            //        fileImportOption.Scale = mutiFilesImportOption.Scale;

            //        var assetFile = new CGfxAsset_File();
            //        assetFile.FileOption = mFileImportOptions[fileName];
            //        using (var it = assetFile.FileOption.ObjectOptions.GetEnumerator())
            //        {
            //            while (it.MoveNext())
            //            {
            //                var hash = it.Current.Key;
            //                var assetOption = it.Current.Value;
            //                if (!assetOption.IsImport)
            //                    continue;
            //                var creater = CGfxAssetCreater.CreateAssetCreater(assetOption);
            //                assetFile.AddCreaters(hash, creater);
            //            }
            //        }
            //        await mFBXImporters[fileName].Import(assetFile);
            //        await assetFile.SaveAsset();
            //        currentPercent += per;
            //        OnAssetImportMessageDumping?.Invoke(this, "", "", AssetImportMessageType.AMT_Import, "ImportDone: " + fileName, currentPercent);
            //    }
            //});

        }
    }
}
