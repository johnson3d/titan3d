using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Assets
{
    public class RNameManager
    {
        public class RNameMapper
        {
            public int Index;               // 当前包唯一索引
            public string PathName;         // 当前包路径Key
            public EngineNS.RName Name;     // 资源RName
            public object ResObject;        // 加载出来的资源，用于分析该资源引用的资源
        }
        public Dictionary<string, RNameMapper> NameMapper = new Dictionary<string, RNameMapper>();
        public void ClearMapper()
        {
            NameMapper.Clear();
        }
        private string RNameToPathName(EngineNS.RName name)
        {
            return name.RNameType.ToString() + "|" + name.Name;
        }
        public RNameMapper GetRNameIndex(EngineNS.RName name)
        {
            if (string.IsNullOrEmpty(name.Name))
                return null;
            var path = RNameToPathName(name);
            RNameMapper mapper;
            if (NameMapper.TryGetValue(path, out mapper))
                return mapper;
            mapper = new RNameMapper();
            mapper.Index = NameMapper.Count;
            mapper.PathName = path;
            mapper.Name = name;
            NameMapper[path] = mapper;
            return mapper;
        }
    }
    public class AssetsPakage
    {
        RNameManager mNameManager = new RNameManager();
        Dictionary<EngineNS.RName, Resources.ResourceInfo> mResInfoDic = new Dictionary<EngineNS.RName, Resources.ResourceInfo>();
        class ShaderEnvData
        {
            public EngineNS.RName Name;
        }

        public class LoadResourceData
        {
            public System.Type ObjType;
            public RNameManager.RNameMapper RNameMapper;

            public string GetTargetAbsFileName()
            {
                return RNameMapper.Name.Address;
            }
        }
        public class SaveResourceData
        {
            public string TargetAbsFileName;
            public object ResObject;
            public string SrcAbsFileName;
            //public RNameManager.RNameMapper RNameMapper;

            public string GetTargetAbsFileName()
            {
                if (string.IsNullOrEmpty(TargetAbsFileName))
                    return SrcAbsFileName;
                else
                    return TargetAbsFileName;
                //if (string.IsNullOrEmpty(mTargetRootPath))
                //    return RNameMapper.Name.Address;
                //else
                //    return mTargetRootPath + RNameMapper.Name.GetNameWithRootFolder();
            }
            public string GetSourceAbsFileName()
            {
                return SrcAbsFileName;
            }
        }

        private async System.Threading.Tasks.Task LoadResource(LoadResourceData data)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var ext = data.RNameMapper.Name.GetExtension();
            if(string.Equals(ext, "shadingenv", StringComparison.OrdinalIgnoreCase))
            {
                var svData = new ShaderEnvData()
                {
                    Name = data.RNameMapper.Name,
                };
                data.RNameMapper.ResObject = svData;
            }
            else
            {
                Resources.ResourceInfo rInfo;
                if (!mResInfoDic.TryGetValue(data.RNameMapper.Name, out rInfo))
                {
                    rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(data.RNameMapper.Name.Address);
                    if (rInfo == null)
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "资源丢失", $"找不到资源{data.RNameMapper.Name.Address}");
                        return;
                    }
                    mResInfoDic.Add(data.RNameMapper.Name, rInfo);
                }
                await rInfo.AssetsOption_LoadResource(data);
            }
        }
        async Task OnVisitProperty(System.Reflection.PropertyInfo prop, object value)
        {
            if(prop.PropertyType == typeof(EngineNS.RName))
            {
                var name = value as EngineNS.RName;
                var rnm = mNameManager.GetRNameIndex(name);
                if(rnm != null)
                {
                    name.NameIndexInPakage = rnm.Index;
                    if (rnm.ResObject == null)//换成实际对象
                    {
                        var data = new LoadResourceData()
                        {
                            ObjType = prop.PropertyType,
                            RNameMapper = rnm,
                        };
                        await LoadResource(data);
                    }
                }
            }
        }
        async Task OnVisitMember(System.Reflection.FieldInfo member, object value)
        {
            if (member.FieldType == typeof(EngineNS.RName))
            {
                var name = value as EngineNS.RName;
                var rnm = mNameManager.GetRNameIndex(name);
                if(rnm != null)
                {
                    name.NameIndexInPakage = rnm.Index;
                    if (rnm.ResObject == null)//换成实际对象
                    {
                        var data = new LoadResourceData()
                        {
                            ObjType = member.FieldType,
                            RNameMapper = rnm,
                        };
                        await LoadResource(data);
                    }
                }
            }
        }
        async Task OnVisitContainer(object value)
        {
            if (value == null)
                return;
            if (value.GetType() == typeof(EngineNS.RName))
            {
                var name = value as EngineNS.RName;
                var rnm = mNameManager.GetRNameIndex(name);
                if(rnm != null)
                {
                    name.NameIndexInPakage = rnm.Index;
                    if (rnm.ResObject == null)//换成实际对象
                    {
                        var data = new LoadResourceData()
                        {
                            ObjType = value.GetType(),
                            RNameMapper = rnm,
                        };
                        await LoadResource(data);
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task RefreshAssets(List<Resources.ResourceInfo> resInfos, Action<float, string> progressReport)
        {
            if (resInfos.Count == 0)
                return;

            await CollectionResource(resInfos, 0.5f, progressReport);
            await SaveToPackage(EngineNS.CEngine.Instance.FileManager.Root, 0.5f, progressReport);
        }

        float mProgress = 0;
        public async System.Threading.Tasks.Task PackAssets(List<EngineNS.RName> res, string packageFile, Action<float, string> progressReport)
        {
            if (res.Count == 0)
                return;

            mProgress = 0;
            float delta = 1.0f / res.Count;
            List<Resources.ResourceInfo> rInfos = new List<Resources.ResourceInfo>(res.Count);
            foreach(var r in res)
            {
                progressReport?.Invoke(mProgress, $"正在加载资源信息 {r.Name}");

                var rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(r.Address);
                if (rInfo == null)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "资源丢失", $"找不到资源{r.Address}");
                    continue;
                }
                rInfos.Add(rInfo);
                mProgress += delta;
            }
            await PackAssets(rInfos, packageFile, progressReport);
        }
        public async System.Threading.Tasks.Task PackAssets(List<Resources.ResourceInfo> resInfos, string packageFile, Action<float, string> progressReport)
        {
            if (resInfos.Count == 0)
                return;

            mProgress = 0;
            EditorCommon.FileSystemWatcherProcess.Enable = false;

            if (EngineNS.CEngine.Instance.FileManager.FileExists(packageFile))
            {
                if (EditorCommon.MessageBox.Show($"存在同名文件，是否替换?", MessageBox.enMessageBoxButton.YesNo) == MessageBox.enMessageBoxResult.No)
                    return;
                EngineNS.CEngine.Instance.FileManager.DeleteFile(packageFile);
            }

            var rootPath = EngineNS.CEngine.Instance.FileManager.Root + Guid.NewGuid().ToString() + "/";
            //var files = EngineNS.CEngine.Instance.FileManager.GetFiles(rootPath);
            //var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(rootPath);
            //if(files.Length > 0 || dirs.Length > 0)
            //{
            //    if (EditorCommon.MessageBox.Show($"将会清空目录{rootPath}中的所有文件，是否继续?", MessageBox.enMessageBoxButton.YesNo) == MessageBox.enMessageBoxResult.No)
            //        return;
            //}
            if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(rootPath))
                EngineNS.CEngine.Instance.FileManager.DeleteDirectory(rootPath, true);
            EngineNS.CEngine.Instance.FileManager.CreateDirectory(rootPath);
            System.IO.File.SetAttributes(rootPath, System.IO.FileAttributes.Hidden);

            var rc = EngineNS.CEngine.Instance.RenderContext;
            mNameManager.ClearMapper();
            mResInfoDic.Clear();

            float totalProgress = 0.5f;
            await CollectionResource(resInfos, totalProgress, progressReport);

            string rootFolder;
            EngineNS.IO.Serializer.RNameSerializer.SaveUseIndex = true;
            totalProgress = 0.4f;
            rootFolder = await SaveToPackage(rootPath, totalProgress, progressReport);
            EngineNS.IO.Serializer.RNameSerializer.SaveUseIndex = false;

            // 计算相对根目录
            rootFolder = rootFolder.Replace(EngineNS.CEngine.Instance.FileManager.ProjectContent, "");
            // 存储RName表
            var xmlHolder = EngineNS.IO.XmlHolder.NewXMLHolder("RNameList", "");
            var rNameDataNode = xmlHolder.RootNode.AddNode("RNameData", "", xmlHolder);
            foreach (var mapper in mNameManager.NameMapper)
            {
                var data = rNameDataNode.AddNode("Data", "", xmlHolder);
                data.AddAttrib("Index", mapper.Value.Index.ToString());
                string name = mapper.Value.Name.Name;
                if (!string.IsNullOrEmpty(rootFolder))
                    name = mapper.Value.Name.Name.Replace(rootFolder, "");
                data.AddAttrib("Name", name);
                data.AddAttrib("RNameType", mapper.Value.Name.RNameType.ToString());
                data.AddAttrib("PackagePath", mapper.Value.Name.Name);
            }
            EngineNS.IO.XmlHolder.SaveXML(rootPath + "/RNames.xml", xmlHolder);

            // 压缩
            progressReport?.Invoke(mProgress, "正在压包...");
            System.IO.Compression.ZipFile.CreateFromDirectory(rootPath, packageFile);
            EngineNS.CEngine.Instance.FileManager.DeleteDirectory(rootPath, true);

            progressReport?.Invoke(1.0f, "完成!");

            EditorCommon.FileSystemWatcherProcess.Enable = true;
        }

        async Task CollectionResource(List<Resources.ResourceInfo> resInfos, float totalProgress, Action<float, string> progressReport)
        {
            float delta = totalProgress / resInfos.Count;

            var collector = new EngineNS.Bricks.SandBox.ObjectReferencesCollector();
            var collectorData = new EngineNS.Bricks.SandBox.ObjectReferencesCollector.CollectProcessData();
            for (int i = 0; i < resInfos.Count; i++)
            {
                Resources.ResourceInfo rInfo = resInfos[i];
                progressReport?.Invoke(mProgress, $"正在收集资源引用 {rInfo.ResourceName.Name}");
                var rName = rInfo.ResourceName;
                mResInfoDic.Add(rName, rInfo);
                //if (!resInfoDic.TryGetValue(rName, out rInfo))
                //{
                //    rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(rName.Address);
                //    if (rInfo == null)
                //    {
                //        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "资源丢失", $"找不到资源{rName.Address}");
                //        continue;
                //    }
                //    resInfoDic.Add(rName, rInfo);
                //}

                var rnm = mNameManager.GetRNameIndex(rName);
                if(rnm != null)
                {
                    var loadData = new EditorCommon.Assets.AssetsPakage.LoadResourceData()
                    {
                        ObjType = null,
                        RNameMapper = rnm,
                    };
                    await rInfo.AssetsOption_LoadResource(loadData);
                    rName.NameIndexInPakage = rnm.Index;

                    //收集res直接、间接用到的资源RName
                    if (rnm.ResObject != null)
                        await collector.CollectReferences(rnm.ResObject, OnVisitProperty, OnVisitMember, OnVisitContainer, collectorData);
                }

                mProgress += delta;
            }
        }
        async Task<string> SaveToPackage(string rootPath, float totalProgress, Action<float, string> progressReport)
        {
            string rootFolder = "";
            var delta = totalProgress / mNameManager.NameMapper.Count;
            using (var iter = mNameManager.NameMapper.Values.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    progressReport?.Invoke(mProgress, $"正在导出到包 {iter.Current.Name.Name}");

                    //if (iter.Current.ResObject == null)
                    //    continue;
                    if(iter.Current.ResObject is ShaderEnvData)
                    {
                        // ShaderEnv 直接拷贝
                        var tagFile = rootPath + iter.Current.Name.GetNameWithRootFolder();
                        EngineNS.CEngine.Instance.FileManager.CopyFile(iter.Current.Name.Address, tagFile, true);
                    }
                    else
                    {
                        Resources.ResourceInfo rInfo;
                        if (!mResInfoDic.TryGetValue(iter.Current.Name, out rInfo))
                        {
                            rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(iter.Current.Name.Address);
                            if (rInfo == null)
                            {
                                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "资源丢失", $"找不到资源{iter.Current.Name.Address}");
                                continue;
                            }
                            mResInfoDic.Add(iter.Current.Name, rInfo);
                        }

                        var saveData = new SaveResourceData()
                        {
                            TargetAbsFileName = rootPath + iter.Current.Name.GetNameWithRootFolder(),
                            SrcAbsFileName = iter.Current.Name.Address,
                            ResObject = iter.Current.ResObject,
                            //RNameMapper = iter.Current,
                        };
                        await rInfo.AssetsOption_SaveResource(saveData);

                        // 计算公共目录根
                        if (rInfo.ResourceName.RNameType == EngineNS.RName.enRNameType.Game)
                        {
                            var path = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(rInfo.ResourceName.Address);

                            if (rootFolder == "" || rootFolder.Contains(path))
                                rootFolder = path;
                            else
                            {
                                // 获取公共目录
                                var splitRootFolder = rootFolder.TrimEnd('/').Split('/');
                                var splitPath = path.TrimEnd('/').Split('/');
                                rootFolder = "";
                                var minLen = System.Math.Min(splitRootFolder.Length, splitPath.Length);
                                for(int i=0; i<minLen; i++)
                                {
                                    if (string.Equals(splitRootFolder[i], splitPath[i], StringComparison.OrdinalIgnoreCase))
                                    {
                                        rootFolder += splitRootFolder[i] + "/";
                                    }
                                    else if (i == 0)
                                    {
                                        throw new InvalidOperationException("没有找到公共父目录");
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                    }

                    mProgress += delta;
                }
            }
            return rootFolder;
        }

        class RNameData
        {
            public int Index;
            public string Name;
            public EngineNS.RName.enRNameType RNameType;
            public string PackagePath;
        }
        struct PackageResData
        {
            public Resources.ResourceInfo ResInfo;
            public RNameManager.RNameMapper RNM;
        }
        public async Task<List<Resources.ResourceInfo>> UnPackAssets(string packageAbsFile, string targetAbsPath)
        {
            List<Resources.ResourceInfo> retList = new List<Resources.ResourceInfo>(20);

            EditorCommon.FileSystemWatcherProcess.Enable = false;

            targetAbsPath = targetAbsPath.Replace("\\", "/").TrimEnd('/') + "/";
            mNameManager.ClearMapper();
            EngineNS.CEngine.Instance.FileManager.ClearPackageRNameCache();

            // unzip
            var tempPath = EngineNS.CEngine.Instance.FileManager.Root + Guid.NewGuid().ToString() + "/";
            if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(tempPath))
                EngineNS.CEngine.Instance.FileManager.DeleteDirectory(tempPath, true);
            EngineNS.CEngine.Instance.FileManager.CreateDirectory(tempPath);
            System.IO.File.SetAttributes(tempPath, System.IO.FileAttributes.Hidden);
            System.IO.Compression.ZipFile.ExtractToDirectory(packageAbsFile, tempPath);
            var srcContentTempPath = (tempPath + "Content/").Replace("\\", "/").ToLower();
            var srcEditorContentTempPath = (tempPath + "EditContent/").Replace("\\", "/").ToLower();
            var srcEngineContentTempPath = (tempPath + "EngineContent/").Replace("\\", "/").ToLower();
            MessageBox.enMessageBoxResult chooseResult = MessageBox.enMessageBoxResult.Cancel;
            // EditorContent和EngineContent的对象直接复制
            if(EngineNS.CEngine.Instance.FileManager.DirectoryExists(srcEditorContentTempPath))
            {
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(srcEditorContentTempPath, "*", System.IO.SearchOption.AllDirectories);
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[0].Replace("\\", "/").ToLower();
                    var tagFile = file.Replace(srcEditorContentTempPath, EngineNS.CEngine.Instance.FileManager.EditorContent);
                    switch (chooseResult)
                    {
                        case MessageBox.enMessageBoxResult.YesAll:
                            EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, true);
                            break;
                        case MessageBox.enMessageBoxResult.NoAll:
                            EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, false);
                            break;
                        default:
                            if (EngineNS.CEngine.Instance.FileManager.FileExists(tagFile))
                            {
                                chooseResult = EditorCommon.MessageBox.Show($"文件{tagFile}与包中文件重名，是否覆盖?", MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll);
                                switch (chooseResult)
                                {
                                    case MessageBox.enMessageBoxResult.Yes:
                                        EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, true);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            if(EngineNS.CEngine.Instance.FileManager.DirectoryExists(srcEngineContentTempPath))
            {
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(srcEngineContentTempPath, "*", System.IO.SearchOption.AllDirectories);
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[0].Replace("\\", "/").ToLower();
                    var tagFile = file.Replace(srcEngineContentTempPath, EngineNS.CEngine.Instance.FileManager.EngineContent);
                    switch (chooseResult)
                    {
                        case MessageBox.enMessageBoxResult.YesAll:
                            EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, true);
                            break;
                        case MessageBox.enMessageBoxResult.NoAll:
                            if(!EngineNS.CEngine.Instance.FileManager.FileExists(tagFile))
                                EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, false);
                            break;
                        default:
                            if (EngineNS.CEngine.Instance.FileManager.FileExists(tagFile))
                            {
                                chooseResult = EditorCommon.MessageBox.Show($"文件{tagFile}与包中文件重名，是否覆盖?", MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll);
                                switch (chooseResult)
                                {
                                    case MessageBox.enMessageBoxResult.Yes:
                                        EngineNS.CEngine.Instance.FileManager.CopyFile(file, tagFile, true);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            // Content中放入目标目录
            if(EngineNS.CEngine.Instance.FileManager.DirectoryExists(srcContentTempPath))
            {
                // 读取RName表
                var xmlHolder = EngineNS.IO.XmlHolder.LoadXML(tempPath + "RNames.xml");
                var rNameDataNode = xmlHolder.RootNode.FindNode("RNameData");
                Dictionary<int, RNameData> rNameDataDic = new Dictionary<int, RNameData>();
                Dictionary<string, RNameData> rNameDataPathDic = new Dictionary<string, RNameData>();
                if (rNameDataNode != null)
                {
                    var cNodes = rNameDataNode.GetNodes();
                    foreach (var cNode in cNodes)
                    {
                        var data = new RNameData();
                        var idxAtt = cNode.FindAttrib("Index");
                        if (idxAtt != null)
                        {
                            data.Index = System.Convert.ToInt32(idxAtt.Value);
                        }
                        var nameAtt = cNode.FindAttrib("Name");
                        if (nameAtt != null)
                        {
                            data.Name = nameAtt.Value;
                        }
                        var rtAtt = cNode.FindAttrib("RNameType");
                        if (rtAtt != null)
                        {
                            data.RNameType = EngineNS.Rtti.RttiHelper.EnumTryParse<EngineNS.RName.enRNameType>(rtAtt.Value);
                        }
                        var pathAtt = cNode.FindAttrib("PackagePath");
                        if(pathAtt != null)
                        {
                            data.PackagePath = pathAtt.Value;
                        }
                        rNameDataDic.Add(data.Index, data);
                        rNameDataPathDic.Add(data.PackagePath, data);
                    }
                }

                // RName填充，将RName中路径填充为Package目录
                var rNameDataDicEnum = rNameDataDic.GetEnumerator();
                while(rNameDataDicEnum.MoveNext())
                {
                    var rName = EngineNS.RName.GetRName(rNameDataDicEnum.Current.Key);
                    rName.RNameType = EngineNS.RName.enRNameType.Package;
                    rName.Name = tempPath + EngineNS.RName.GetRootFolderName(rNameDataDicEnum.Current.Value.RNameType) + rNameDataDicEnum.Current.Value.PackagePath;
                }

                // Shader目录直接拷贝
                if(EngineNS.CEngine.Instance.FileManager.DirectoryExists(srcContentTempPath + "shaders"))
                {
                    EngineNS.CEngine.Instance.FileManager.CopyDirectory(srcContentTempPath + "shaders", EngineNS.CEngine.Instance.FileManager.ProjectContent + "shaders");
                }

                var relPath = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(targetAbsPath, EngineNS.CEngine.Instance.FileManager.ProjectContent);
                // 全部加载，
                var files = EngineNS.CEngine.Instance.FileManager.GetFiles(srcContentTempPath, "*.rinfo", System.IO.SearchOption.AllDirectories);

                List<PackageResData> mLoadedResInfoDatas = new List<PackageResData>(files.Count);
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i].Replace("\\", "/");
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null);
                    var rnm = mNameManager.GetRNameIndex(resInfo.ResourceName);
                    if (rnm == null)
                        continue;

                    var loadData = new EditorCommon.Assets.AssetsPakage.LoadResourceData()
                    {
                        ObjType = null,
                        RNameMapper = rnm,
                    };
                    await resInfo.AssetsOption_LoadResource(loadData);

                    var pkResData = new PackageResData()
                    {
                        ResInfo = resInfo,
                        RNM = rnm,
                    };
                    mLoadedResInfoDatas.Add(pkResData);
                }
                // 填充rname，将RName中路径填充为Content目录，这里需要保证在之前全部加载完成，否则可能导致加载到目标目录的文件，而目标目录的文件是有可能还没导入
                foreach (var cache in EngineNS.CEngine.Instance.FileManager.PackageRNameCache.Values)
                {
                    RNameData data;
                    if (rNameDataDic.TryGetValue(cache.NameIndexInPakage, out data))
                    {
                        cache.RNameType = data.RNameType;
                        if (cache.RNameType == EngineNS.RName.enRNameType.Game)
                            cache.Name = relPath + data.Name;
                        else
                            cache.Name = data.Name;
                    }
                }
                // 全部存入目标文件夹
                foreach(var resInfoData in mLoadedResInfoDatas)
                {
                    var resInfo = resInfoData.ResInfo;
                    var tempTagPath = resInfo.ResourceName.Address.Replace(srcContentTempPath, "");
                    string tagFile = targetAbsPath + tempTagPath;
                    RNameData tempData;
                    if (rNameDataPathDic.TryGetValue(tempTagPath, out tempData))
                        tagFile = targetAbsPath + tempData.Name;
                    EngineNS.CEngine.Instance.FileManager.MergePackageRNameCacheToRNameManager();
                    var saveData = new SaveResourceData()
                    {
                        TargetAbsFileName = tagFile,
                        ResObject = resInfoData.RNM.ResObject,
                        SrcAbsFileName = resInfo.ResourceName.Address,
                    };
                    await resInfo.AssetsOption_SaveResource(saveData);

                    var tempResInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(tagFile + EditorCommon.Program.ResourceInfoExt, null);
                    await EngineNS.CEngine.Instance.GameEditorInstance.RefreshResourceInfoReferenceDictionary(tempResInfo);
                    retList.Add(tempResInfo);
                }
            }

            EngineNS.CEngine.Instance.FileManager.DeleteDirectory(tempPath, true);
            //EditorCommon.MessageBox.Show("导入完成");
            EditorCommon.FileSystemWatcherProcess.Enable = true;

            return retList;
        }
    }
}
