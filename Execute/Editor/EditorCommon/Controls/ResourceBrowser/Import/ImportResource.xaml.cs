using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EditorCommon.Controls.ResourceBrowser
{
    /// <summary>
    /// ImportResource.xaml 的交互逻辑
    /// </summary>
    public partial class ImportResource : ResourceLibrary.WindowBase, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        //[ImportMany(AllowRecomposition = true)]
        internal IEnumerable<EditorCommon.Resources.ResourceInfoMetaData> mResourceInfoProcessers = null;
        ObservableCollection<EditorCommon.Resources.ResourceInfo> mCurrentResources = new ObservableCollection<EditorCommon.Resources.ResourceInfo>();

        //EditorCommon.Resources.ResourceInfoMetaData mTechProcesser;
        //EditorCommon.Resources.ResourceInfoMetaData mMeshTemplateProcesser;
        //EditorCommon.Resources.ResourceInfoMetaData mTextureProcesser;
        //EditorCommon.Resources.ResourceInfoMetaData mVmsProcesser;

        IContentControlHost mParentWindow = null;

        private List<string> mConfigFileList;
        public List<string> ConfigFileList
        {
            get { return mConfigFileList; }
            set { mConfigFileList = value; }
        }
        Dictionary<string, string> mResourcesCurrentFolder = new Dictionary<string, string>();

        string mCurrentAbsFolder = null;
        public string CurrentAbsFolder
        {
            get { return mCurrentAbsFolder; }
            set
            {
                mCurrentAbsFolder = value;
                VmsSavePath = mCurrentAbsFolder;
                TextureSavePath = mCurrentAbsFolder;
                TechSavePath = mCurrentAbsFolder;
                MtlSavePath = mCurrentAbsFolder;
                OtherSavePath = mCurrentAbsFolder;

                EngineNS.CEngine.Instance.EventPoster.RunOn(()=>
                {
                    resourcesSaveTextBox.Text = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(mCurrentAbsFolder);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Main);
            }
        }
        string mVmsSavePath = null;
        public string VmsSavePath
        {
            get { return mVmsSavePath; }
            set { mVmsSavePath = value; }
        }
        string mTextureSavePath = null;
        public string TextureSavePath
        {
            get { return mTextureSavePath; }
            set
            {
                mTextureSavePath = value;
                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //    textureSaveTextBox.Text = EngineNS.CEngine.Instance.FileManager.Instance._GetRelativePathFromAbsPath(mTextureSavePath);
                //}));
            }
        }
        string mTechSavePath = null;
        public string TechSavePath
        {
            get { return mTechSavePath; }
            set
            {
                mTechSavePath = value;
                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //    techSaveTextBox.Text = EngineNS.CEngine.Instance.FileManager.Instance._GetRelativePathFromAbsPath(mTechSavePath); ;
                //}));
            }
        }
        string mMtlSavePath = null;
        public string MtlSavePath
        {
            get { return mMtlSavePath; }
            set
            {
                mMtlSavePath = value;
                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //    mtlSaveTextBox.Text = EngineNS.CEngine.Instance.FileManager.Instance._GetRelativePathFromAbsPath(mMtlSavePath); ;
                //}));
            }
        }
        string mOtherSavePath = null;
        public string OtherSavePath
        {
            get { return mOtherSavePath; }
            set
            {
                mOtherSavePath = value;
                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //    otherSaveTextBox.Text = EngineNS.CEngine.Instance.FileManager.Instance._GetRelativePathFromAbsPath(mOtherSavePath); ;
                //}));
            }
        }
        private MeshImportConfig mMeshConfig = new MeshImportConfig();
        public ImportResource(IContentControlHost parentWindow, IEnumerable<EditorCommon.Resources.ResourceInfoMetaData> processers)
        {
            InitializeComponent();
            mParentWindow = parentWindow;
            mResourceInfoProcessers = processers;
            foreach (var processer in mResourceInfoProcessers)
            {
                //switch (processer.ResourceInfoTypeStr)
                //{
                //    case EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource: mVmsProcesser = processer; break;
                //    case "MeshTemplate": mMeshTemplateProcesser = processer; break;
                //    case "Texture": mTextureProcesser = processer; break;
                //    case "Technique": mTechProcesser = processer; break;
                //}

            }
            //var template = this.TryFindResource("MaterialTypeSetControl") as DataTemplate;
            //WPG.Program.RegisterDataTemplate("MaterialTypeSetControl", template);

        }
        public async System.Threading.Tasks.Task StartLoadResource()
        {
            await AsynchronousLoadResources();
            //完事了做点啥！
        }
        Dictionary<string, PropertyGrid.MaterialTypeSetControl> mMaterialMatch = new Dictionary<string, PropertyGrid.MaterialTypeSetControl>();
        Dictionary<string, Guid> mCachedTechIDName = new Dictionary<string, Guid>();
        private async System.Threading.Tasks.Task AsynchronousLoadResources()
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                mMeshConfig.BatchLoad(mConfigFileList.ToArray());
                for (int i = 0; i < mMeshConfig.MeshConfigs.Count; ++i)
                {
                    var cof = mMeshConfig.MeshConfigs[i];
                    var filePath = mConfigFileList[i].Substring(0, mConfigFileList[i].LastIndexOf('\\'));
                    mResourcesCurrentFolder[cof.VMSPath] = filePath + @"\\" + cof.MeshName + EngineNS.CEngineDesc.MeshSourceExtension;
                    mResourcesCurrentFolder[cof.SocketPath] = filePath + @"\\" + cof.MeshName + EngineNS.CEngineDesc.MeshSourceExtension + EngineNS.CEngineDesc.MeshSocketExtension;
                }

                return true;
            });

            foreach (var config in mMeshConfig.MeshConfigs)
            {
                if (!mMaterialMatch.ContainsKey(config.MaterialTemplateType))
                {
                    PropertyGrid.MaterialTypeSetControl matSetCtl = new PropertyGrid.MaterialTypeSetControl(mResourceInfoProcessers);
                    matSetCtl.MaterialTypeNameTextBlock.Text = config.MaterialTemplateType;
                    mMaterialMatch.Add(config.MaterialTemplateType, matSetCtl);
                    materialsStackPanel.Children.Add(matSetCtl);
                }
            }
        }

        public async System.Threading.Tasks.Task CopyFile(string srcFile, string tagFile)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var fileInfo = new System.IO.FileInfo(srcFile);
            if (fileInfo == null)
            {
                EditorCommon.MessageBox.Show("文件" + srcFile + "不存在，请确认路径是否正确", "警告", this);
                return;
            }
            EditorCommon.MessageBox.enMessageBoxResult messageBoxResult = EditorCommon.MessageBox.enMessageBoxResult.None;
            if (System.IO.File.Exists(tagFile))
            {
                switch (messageBoxResult)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
                        System.IO.File.Copy(srcFile, tagFile, true);
                        break;
                    case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
                        return;
                    default:
                        {
                            messageBoxResult = EditorCommon.MessageBox.Show("文件" + fileInfo.Name + "已存在，是否覆盖", "警告", EditorCommon.MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll, this);
                            switch (messageBoxResult)
                            {
                                case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                                    System.IO.File.Copy(srcFile, tagFile, true);
                                    break;
                                case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
                                    System.IO.File.Copy(srcFile, tagFile, true);
                                    break;
                                case EditorCommon.MessageBox.enMessageBoxResult.No:
                                case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
                                    return;
                            }

                        }

                        break;
                }
            }
            else
                System.IO.File.Copy(srcFile, tagFile);
        }

        //double ProcessTotal = 0;
        //double ProcessCount = 0;

        Dictionary<string, string> mCachedFileName = new Dictionary<string, string>();
        private string GetTextureInResources(string textureName)
        {
            if (mCachedFileName.ContainsKey(textureName))
                return mCachedFileName[textureName];
            else
            {
                var fileName = ExistsInResources(EngineNS.CEngine.Instance.FileManager.Root + "/resources", textureName);
                if (fileName != "")
                {
                    mCachedFileName[textureName] = fileName;
                    return fileName;
                }
                return "";
            }
        }
        private string ExistsInResources(string directory, string textureName)
        {
            System.IO.DirectoryInfo dirInfo = new DirectoryInfo(directory);
            var files = dirInfo.GetFiles();
            foreach (var file in files)
            {
                if (file.Name == textureName)
                    return file.FullName;
            }
            foreach (var dInfo in dirInfo.GetDirectories())
            {
                var fileName = ExistsInResources(dInfo.FullName, textureName);
                if (fileName != "")
                    return fileName;
            }
            return "";
        }
        private List<string> mResourcesAlreadyInEngine = new List<string>();
        private async System.Threading.Tasks.Task ImportMeshConfig(MeshImportConfig importConfig, List<EngineNS.RName> typeMatResNameMatch)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //importStateTextBlock.Visibility = Visibility.Visible;
            //importMessageBlock.Visibility = Visibility.Visible;
            //importProgressBar.Visibility = Visibility.Visible;

            //List<MeshConfig> validConfig = new List<MeshConfig>();
            //var picFiles = new List<string>();
            //await EngineNS.CEngine.Instance.EventPoster.PostAsync(() =>
            //{
            //    foreach (var config in importConfig.MeshConfigs)
            //    {
            //        validConfig.Add(config);
            //    }
            //    #region 无效资源剔除
            //    List<MeshConfig> invalidConfig = new List<MeshConfig>();
            //    foreach (var meshCfg in validConfig)
            //    {
            //        List<TextureMaterial> invalidMaterial = new List<TextureMaterial>();
            //        Dictionary<string, string> tempTexturesAbsPath = new Dictionary<string, string>();
            //        foreach (var texMaterial in meshCfg.Materials)
            //        {
            //            foreach (var pair in texMaterial.TexturesAbsPath)
            //            {
            //                var file = pair.Value;

            //                System.IO.FileInfo fileInfo = null;
            //                if (!System.IO.File.Exists(file))
            //                {
            //                    var fileName = GetTextureInResources(file.Substring(file.LastIndexOf('\\') + 1));
            //                    if (fileName != "")
            //                        tempTexturesAbsPath[pair.Key] = fileName;
            //                    else
            //                    {
            //                        //EditorCommon.MessageBox.Show("文件" + file + "不存在，请确认路径是否正确", "警告", EditorCommon.MessageBox.enMessageBoxButton.OK, this);
            //                        //continue;
            //                        var noUse = OutputInfo("警告: " + "文件" + file + "不存在，与其相关的资源文件将不生成,请确认路径后重新导入!\n", Brushes.Yellow);
            //                        if (!invalidMaterial.Contains(texMaterial))
            //                            invalidMaterial.Add(texMaterial);
            //                        if (!invalidConfig.Contains(meshCfg))
            //                            invalidConfig.Add(meshCfg);
            //                    }
            //                }
            //                else
            //                {
            //                    fileInfo = new System.IO.FileInfo(file);
            //                    if (fileInfo != null && !mTextureProcesser.ResourceExts.Contains(fileInfo.Extension.ToLower()))
            //                    {
            //                        //EditorCommon.MessageBox.Show("不支持"+file + "格式，将停止本次导出，请检查文件是否正确", "警告", EditorCommon.MessageBox.enMessageBoxButton.OK, this);
            //                        var noUse = OutputInfo("警告: " + fileInfo.Name + "不支持的图片格式，与其相关的资源文件将不生成,请检查后从新导入!\n", Brushes.Yellow);
            //                        if (!invalidMaterial.Contains(texMaterial))
            //                            invalidMaterial.Add(texMaterial);
            //                        if (!invalidConfig.Contains(meshCfg))
            //                            invalidConfig.Add(meshCfg);
            //                    }
            //                }

            //            }
            //            foreach (var pair in tempTexturesAbsPath)
            //            {
            //                texMaterial.TexturesAbsPath[pair.Key] = pair.Value;
            //                if (!mResourcesAlreadyInEngine.Contains(pair.Value))
            //                    mResourcesAlreadyInEngine.Add(pair.Value);
            //            }
            //            //foreach(var tMat in invalidMaterial)
            //            //{
            //            //    meshCfg.Materials.Remove(tMat);
            //            //    this.Dispatcher.Invoke(new Action(() =>
            //            //    {
            //            //        errorTextBox.Text +="材质实例生成失败，不支持的图片格式，与其相关的资源文件将不生成,请检查后从新导入!\n";
            //            //    }));
            //            //}
            //        }
            //    }
            //    foreach (var config in invalidConfig)
            //    {
            //        validConfig.Remove(config);
            //        var noUse = OutputInfo("失败: " + config.MeshName + "相关资源批量生成失败，某些贴图资源无效，请检查文件是否正确!\n", Brushes.Red);
            //    }

            //    #endregion

            //    #region 资源进度分配
            //    //所有甚至的路径倒要编程保存后的路径，
            //    var vmsFiles = new List<string>();
            //    var positions = new List<EngineNS.Vector3>();

            //    foreach (var meshCfg in validConfig)
            //    {
            //        vmsFiles.Add(meshCfg.VMSPath);
            //        positions.Add(meshCfg.Position);
            //        ProcessTotal += meshCfg.Materials.Count * 2 + 2;
            //        foreach (var texMaterial in meshCfg.Materials)
            //        {
            //            foreach (var pair in texMaterial.TexturesAbsPath)
            //            {
            //                if (!picFiles.Contains(pair.Value))
            //                    picFiles.Add(pair.Value);
            //            }
            //        }
            //    }
            //    ProcessTotal += vmsFiles.Count;
            //    ProcessTotal += picFiles.Count;
            //    #endregion

            //    //导入vms
            //    ImportVMSResources(vmsFiles.ToArray(), mVmsSavePath, positions);
            //    ImportTextureResources(picFiles.ToArray(), mTextureSavePath);
            //    return true;
            //});
            //List<EditorCommon.Resources.ResourceInfo> resourceCreateSnap = new List<EditorCommon.Resources.ResourceInfo>();
            //var smp = new EngineNS.Thread.ASyncSemaphore(validConfig.Count);
            //foreach (var meshCfg in validConfig)
            //{
            //    var noUse = EngineNS.CEngine.Instance.EventPoster.PostAsync(async () =>
            //    {
            //        if (System.IO.File.Exists(meshCfg.SocketPath))
            //        {
            //            var socketFileInfo = new System.IO.FileInfo(meshCfg.SocketPath);
            //            //复制socket
            //            CopyFile(meshCfg.SocketPath, mVmsSavePath + @"\\" + socketFileInfo.Name);
            //        }
            //        else
            //        {
            //            var path = mResourcesCurrentFolder[meshCfg.SocketPath].Replace(@"\\", @"\");
            //            if (System.IO.File.Exists(path))
            //            {
            //                var socketFileInfo = new System.IO.FileInfo(path);
            //                //复制socket
            //                CopyFile(path, mVmsSavePath + "/" + socketFileInfo.Name);
            //            }
            //        }
            //        //查找现有材质实例，是否在某个材质实例里面存有改贴图
            //        //新建材质实例 并保留实例的GUID
            //        var resourceName = meshCfg.MeshName;
            //        //材质模板从用户输入获得、
            //        var hostMaterialName = EngineNS.CEngine.Instance.GameInstance.Desc.DefaultMaterialName;
            //        foreach(var resName in typeMatResNameMatch)
            //        {
            //            if (resName.Name == meshCfg.MaterialTemplateType)
            //                hostMaterialName = resName;
            //        }
            //        List<Guid> techs = new List<Guid>();
            //        //var absFileName = CurrentAbsFolder + "/" + id.ToString() + CSUtility.Support.IFileConfig.MaterialTechniqueExtension;
            //        //var relFile = EngineNS.CEngine.Instance.FileManager.Instance._GetRelativePathFromAbsPath(absFileName, EngineNS.CEngine.Instance.FileManager.Instance.Root + CSUtility.Support.IFileConfig.DefaultResourceDirectory);
            //        int techCount = 0;
            //        foreach (var textureMaterial in meshCfg.Materials)
            //        {
            //            if (mCachedTechIDName.ContainsKey(textureMaterial.MaterialName))
            //            {
            //                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //                {
            //                    importMessageBlock.Text = "材质实例" + textureMaterial.MaterialName + "已存在，直接使用";
            //                });
            //                techs.Add(mCachedTechIDName[textureMaterial.MaterialName]);
            //                techCount++;
            //                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //                {
            //                    ProcessCount++;
            //                    importProgressBar.Value = ProcessCount / ProcessTotal * 100;
            //                });
            //                continue;
            //            }
            //            Guid techGUID = Guid.Empty;
            //            var techResCreateInfo = mTechProcesser.ResInfo as EditorCommon.Resources.IResourceTechInfoImportCreate;
            //            if (techResCreateInfo == null)
            //                return false;
            //            foreach (var pair in textureMaterial.TexturesAbsPath)
            //            {
            //                var fileInfo = new System.IO.FileInfo(pair.Value);
            //                if (!picFiles.Contains(pair.Value))
            //                    picFiles.Add(pair.Value);
            //                string relPath = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(mTextureSavePath + "/" + fileInfo.Name);
            //                if (mResourcesAlreadyInEngine.Contains(pair.Value))
            //                {
            //                    relPath = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(pair.Value);
            //                }
            //                if (!textureMaterial.TexturesRelPath.Contains(new KeyValuePair<string, string>(pair.Key, relPath)))
            //                    textureMaterial.TexturesRelPath.Add(pair.Key, relPath);
            //            }
            //            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //            {
            //                importMessageBlock.Text = "创建材质实例" + textureMaterial.MaterialName;
            //            });
            //            var techInfo = techResCreateInfo.CreateResourceFormImportFile(hostMaterialName, mTechSavePath, textureMaterial.MaterialName, textureMaterial.TexturesRelPath);
            //            techs.Add(techInfo.Id);
            //            mCachedTechIDName[textureMaterial.MaterialName] = techInfo.Id;
            //            //info.Save(false);
            //            techCount++;
            //            resourceCreateSnap.Add(techInfo);
            //            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //            {
            //                ProcessCount++;
            //                importProgressBar.Value = ProcessCount / ProcessTotal * 100;
            //            });
            //        }
            //        string relativePath = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(mVmsSavePath + "/" + resourceName + ".vms");
            //        //新建模型模板
            //        System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Guid>> initPart = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Guid>>();
            //        initPart.Add(relativePath, techs);
            //        var resCreateInfo = mMeshTemplateProcesser.ResInfo as EditorCommon.Resources.IResourceMtlInfoImportCreate;
            //        if (resCreateInfo == null)
            //            return false;
            //        string templateName = resourceName + "MeshTemplate";
            //        EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //        {
            //            importMessageBlock.Text = "创建模型模板" + resourceName + "MeshTemplate";
            //        });
            //        await SolveDuplicationTemplatename(templateName);

            //        var info = await resCreateInfo.CreateResourceFormImportFile(hostMaterialName, mMtlSavePath, templateName, initPart);
            //        //info.Save(false);
            //        resourceCreateSnap.Add(info);
            //        EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //        {
            //            ProcessCount++;
            //            importProgressBar.Value = ProcessCount / ProcessTotal * 100;
            //        });
            //        smp.Release();
            //        return true;
            //    });
            //    await EngineNS.CEngine.Instance.EventPoster.AwaitSemaphore(smp, EngineNS.Thread.EAsyncContinueType.Sync);
            //}

            //foreach (var snap in resourceCreateSnap)
            //{
            //    importMessageBlock.Text = "生成缩略图" + snap.Name;
            //    ProcessCount++;
            //    importProgressBar.Value = ProcessCount / ProcessTotal * 100;
            //    //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //    //{
            //    //    EditorCommon.VersionControl.VersionControlManager.Instance.Add((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    //    {
            //    //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //    //        {
            //    //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{snap.ResourceType}{Name} {snap.AbsResourceFileName + EditorCommon.Program.SnapshotExt}使用版本控制添加失败!");
            //    //        }
            //    //    }, snap.AbsResourceFileName + EditorCommon.Program.SnapshotExt, $"AutoCommit {snap.ResourceType}{Name}缩略图");
            //    //}
            //}

            //importStateTextBlock.Text = "导入完成";
            //importMessageBlock.Text = "";
            //ProcessCount++;
            //importProgressBar.Value = 100;
            //EditorCommon.MessageBox.Show("资源导入完毕", "消息", EditorCommon.MessageBox.enMessageBoxButton.OK, this);
            ////完事了做点啥！
            //await mParentWindow.ShowSourcesInDir(mCurrentAbsFolder);
            ////this.Close();
            //importButton.IsEnabled = true;
            //canncelButton.IsEnabled = false;
        }
        //private System.Threading.Tasks.Task ImportVMSResources(string[] resourceFiles, string absFolder, List<EngineNS.Vector3> positions)
        //{
        //    var count = resourceFiles.Length;
        //    EditorCommon.MessageBox.enMessageBoxResult messageBoxResult = EditorCommon.MessageBox.enMessageBoxResult.None;
        //    float idx = 0;
        //    for (int i = 0; i < resourceFiles.Length; ++i)
        //    {
        //        string file = resourceFiles[i];
        //        if (!System.IO.File.Exists(file))
        //        {
        //            var path = mResourcesCurrentFolder[file].Replace(@"\\", @"\");
        //            if (System.IO.File.Exists(path))
        //                file = path;
        //            else
        //            {
        //                EditorCommon.MessageBox.Show("文件" + file + "不存在，请确认路径是否正确", "警告", this);
        //                continue;
        //            }
        //        }
        //        var fileInfo = new System.IO.FileInfo(file);
        //        if (fileInfo == null)
        //        {
        //            EditorCommon.MessageBox.Show("文件" + file + "不存在，请确认路径是否正确", "警告", this);
        //            continue;
        //        }
        //        var tagFile = absFolder + "/" + fileInfo.Name;
        //        EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
        //        {
        //            importMessageBlock.Text = "导入文件" + fileInfo.Name;
        //        });
        //        var ext = fileInfo.Extension.ToLower();
        //        EditorCommon.Resources.ResourceInfo resInfo = null;
        //        var resCreateInfo = mVmsProcesser.ResInfo as EditorCommon.Resources.IResourceMeshInfoImportCreate;
        //        if (resCreateInfo == null)
        //            return;
        //        resInfo = resCreateInfo.CreateResourceFromImportFile(tagFile, positions[i]);
        //        if (resInfo == null)
        //        {
        //            EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "ImportResource", $"导入文件{file}失败，目标类型不支持");
        //            return;
        //        }
        //        //var meshInfo = resInfo as MeshSourceEditor.MeshSourceResourceInfo;
        //        //resInfo.Save();

        //        if (System.IO.File.Exists(tagFile))
        //        {
        //            switch (messageBoxResult)
        //            {
        //                case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
        //                    System.IO.File.Copy(file, tagFile, true);
        //                    break;
        //                case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
        //                    return;
        //                default:
        //                    {
        //                        messageBoxResult = EditorCommon.MessageBox.Show("文件" + fileInfo.Name + "已存在，是否覆盖", "警告", EditorCommon.MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll, this);
        //                        switch (messageBoxResult)
        //                        {
        //                            case EditorCommon.MessageBox.enMessageBoxResult.Yes:
        //                                System.IO.File.Copy(file, tagFile, true);
        //                                break;
        //                            case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
        //                                System.IO.File.Copy(file, tagFile, true);
        //                                break;
        //                            case EditorCommon.MessageBox.enMessageBoxResult.No:
        //                            case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
        //                                return;
        //                        }

        //                    }

        //                    break;
        //            }
        //        }
        //        else
        //            System.IO.File.Copy(file, tagFile);

        //        idx += 1;
        //        ProcessPercent = idx / count;
        //        EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
        //        {
        //            ProcessCount++;
        //            importProgressBar.Value = ProcessCount / ProcessTotal * 100;
        //        });
        //        // System.Threading.Thread.Sleep(1);
        //    }
        //}
        //private void ImportTextureResources(string[] resourceFiles, string absFolder)
        //{
        //    var count = resourceFiles.Length;
        //    EditorCommon.MessageBox.enMessageBoxResult messageBoxResult = EditorCommon.MessageBox.enMessageBoxResult.None;
        //    float idx = 0;
        //    foreach (var file in resourceFiles)
        //    {
        //        if (!mResourcesAlreadyInEngine.Contains(file))
        //        {
        //            var fileInfo = new System.IO.FileInfo(file);
        //            if (fileInfo == null)
        //            {
        //                EditorCommon.MessageBox.Show("文件" + file + "不存在，请确认路径是否正确", "警告", EditorCommon.MessageBox.enMessageBoxButton.OK, this);
        //                continue;
        //            }
        //            var tagFile = absFolder + "/" + fileInfo.Name;
        //            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
        //            {
        //                importMessageBlock.Text = "导入文件" + fileInfo.Name;
        //            });
        //            var ext = fileInfo.Extension.ToLower();
        //            EditorCommon.Resources.ResourceInfo resInfo = null;
        //            resInfo = mTextureProcesser.ResInfo.CreateResourceInfoFromResource(tagFile);
        //            if (resInfo == null)
        //            {
        //                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "ImportResource", $"导入文件{file}失败，目标类型不支持");
        //                return;
        //            }
        //            //var meshInfo = resInfo as MeshSourceEditor.MeshSourceResourceInfo;
        //            resInfo.Save();

        //            if (System.IO.File.Exists(tagFile))
        //            {
        //                switch (messageBoxResult)
        //                {
        //                    case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
        //                        System.IO.File.Copy(file, tagFile, true);
        //                        break;
        //                    case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
        //                        return;
        //                    default:
        //                        {
        //                            messageBoxResult = EditorCommon.MessageBox.Show("文件" + fileInfo.Name + "已存在，是否覆盖", "警告", EditorCommon.MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll, this);
        //                            switch (messageBoxResult)
        //                            {
        //                                case EditorCommon.MessageBox.enMessageBoxResult.Yes:
        //                                    System.IO.File.Copy(file, tagFile, true);
        //                                    break;
        //                                case EditorCommon.MessageBox.enMessageBoxResult.YesAll:
        //                                    System.IO.File.Copy(file, tagFile, true);
        //                                    break;
        //                                case EditorCommon.MessageBox.enMessageBoxResult.No:
        //                                case EditorCommon.MessageBox.enMessageBoxResult.NoAll:
        //                                    return;
        //                            }

        //                        }

        //                        break;
        //                }
        //            }
        //            else
        //                System.IO.File.Copy(file, tagFile);
        //        }
        //        idx += 1;
        //        ProcessPercent = idx / count;
        //        EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
        //        {
        //            ProcessCount++;
        //            importProgressBar.Value = ProcessCount / ProcessTotal * 100;
        //        });
        //        // System.Threading.Thread.Sleep(1);
        //    }
        //}

        //private async System.Threading.Tasks.Task SolveDuplicationTemplatename(string templateName)
        //{
        //    var files = Directory.GetFiles(mMtlSavePath, "*.rinfo");
        //    List<string> duplicationNameList = new List<string>();
        //    bool isHaveDuplicationTemplatename = false;
        //    foreach (var file in files)
        //    {
        //        var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file, null);
        //        if (resInfo.ResourceType == "MeshTemplate")
        //        {
        //            if (resInfo.ResourceName.PureName() == templateName)
        //            {
        //                duplicationNameList.Add(file);
        //                isHaveDuplicationTemplatename = true;
        //            }
        //        }
        //    }
        //    if (isHaveDuplicationTemplatename)
        //    {
        //        var messageBoxResult = EditorCommon.MessageBox.Show("已存在同名文件" + templateName + "！ 是否删除同名文件", "警告", EditorCommon.MessageBox.enMessageBoxButton.YesNo, this);
        //        switch (messageBoxResult)
        //        {
        //            case EditorCommon.MessageBox.enMessageBoxResult.Yes:
        //                {
        //                    foreach (var duplicationName in duplicationNameList)
        //                    {
        //                        System.IO.File.Delete(duplicationName);
        //                        var templateFile = duplicationName.Substring(0, duplicationName.LastIndexOf('.'));
        //                        if (System.IO.File.Exists(templateFile))
        //                            System.IO.File.Delete(templateFile);
        //                    }
        //                }
        //                break;
        //            case EditorCommon.MessageBox.enMessageBoxResult.No:
        //                return;
        //        }
        //    }

        //}

        #region 长时间处理

        Visibility mProcessingVisible = Visibility.Collapsed;
        public Visibility ProcessingVisible
        {
            get { return mProcessingVisible; }
            set
            {
                mProcessingVisible = value;
                OnPropertyChanged("ProcessingVisible");
            }
        }

        string mProcessingInfo = "";
        public string ProcessingInfo
        {
            get { return mProcessingInfo; }
            set
            {
                mProcessingInfo = value;
                OnPropertyChanged("ProcessingInfo");
            }
        }

        float mProcessPercent = 0;
        public float ProcessPercent
        {
            get { return mProcessPercent; }
            set
            {
                mProcessPercent = value;
                OnPropertyChanged("ProcessPercent");
            }
        }

        //System.Threading.Thread mProcessThread;
        //Action mProcessAction;
        //Action mProcessFinishAction;
        //void StartProcessThread()
        //{
        //    mProcessThread = new System.Threading.Thread(new System.Threading.ThreadStart(DoProcess));
        //    mProcessThread.Name = "资源导入长时间处理操作线程";
        //    mProcessThread.IsBackground = true;
        //    mProcessThread.Start();
        //}

        //public void StartProcess(Action doAction)
        //{
        //    ProcessingVisible = Visibility.Visible;
        //    mProcessAction = doAction;
        //    if (mProcessThread == null)
        //        StartProcessThread();
        //}

        //void DoProcess()
        //{
        //    if (mProcessAction == null)
        //        return;

        //    mProcessAction.Invoke();

        //    ProcessingVisible = Visibility.Collapsed;
        //    mProcessThread = null;

        //    mProcessFinishAction.Invoke();
        //}

        //public void EndProcesssThread()
        //{
        //    if (mProcessThread != null)
        //        mProcessThread.Abort();
        //}

        #endregion

        private void importButton_Click(object sender, RoutedEventArgs e)
        {
            //if ("" != textureSaveTextBox.Text)
            //    TextureSavePath = textureSaveTextBox.Text;
            //if ("" != techSaveTextBox.Text)
            //    TechSavePath = techSaveTextBox.Text;
            //if ("" != mtlSaveTextBox.Text)
            //    MtlSavePath = mtlSaveTextBox.Text;
            //if ("" != otherSaveTextBox.Text)
            //    OtherSavePath = otherSaveTextBox.Text;
            foreach(var rNameData in EngineNS.CEngine.Instance.FileManager.RNameManager)
            {
                if(rNameData.Key.ContainsString(EngineNS.CEngineDesc.MaterialInstanceExtension))
                {
                    var fileName = rNameData.Value.Name;
                    if (!mCachedTechIDName.ContainsKey(fileName))
                    {
                        var strGuid = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(fileName, false);
                        Guid id = EngineNS.Rtti.RttiHelper.GuidTryParse(strGuid);
                        if (id != Guid.Empty)
                        {
                            mCachedTechIDName[fileName] = id;
                        }
                    }
                }
            }
            List<EngineNS.RName> mTypeResNameMatch = new List<EngineNS.RName>();
            foreach (var pair in mMaterialMatch)
            {
                if (pair.Value.CurrentMaterialInfo == null)
                    continue;
                mTypeResNameMatch.Add(pair.Value.CurrentMaterialInfo.ResourceName);
            }
            if (mMaterialMatch.Count != mTypeResNameMatch.Count)
            {
                EditorCommon.MessageBox.Show("模型模板不能为空", "消息", EditorCommon.MessageBox.enMessageBoxButton.OK, this);
                return;
            }
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                importButton.IsEnabled = false;
                canncelButton.IsEnabled = true;
                errorTextBox.Document.Blocks.Clear();
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
            var noUse = ImportMeshConfig(mMeshConfig, mTypeResNameMatch);
        }
        string mInfoStr = "";
        public async Task OutputInfo(string info, Brush brush)
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(()=>
            {
                Paragraph p = new Paragraph()
                {
                    Margin = new Thickness(0)
                };
                Span span = new Span(new Run(System.DateTime.Now.ToString() + ": "))
                {
                    Foreground = Brushes.LightGray
                };
                p.Inlines.Add(span);
                span = new Span(new Run(info))
                {
                    Foreground = brush
                };
                p.Inlines.Add(span);
                errorTextBox.Document.Blocks.Add(p);

                mInfoStr += System.DateTime.Now.ToString() + ": " + info + "\r\n";

                errorTextBox.ScrollToEnd();
                return true;
            },EngineNS.Thread.Async.EAsyncTarget.Main);
        }
        private void canncelButton_Click(object sender, RoutedEventArgs e)
        {
            //var result = EditorCommon.MessageBox.Show("确定取消导入吗？", "警告", EditorCommon.MessageBox.enMessageBoxButton.OKCancel, this);
            //if (result == EditorCommon.MessageBox.enMessageBoxResult.OK)
            //{
            //    EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //    {
            //        importButton.IsEnabled = true;
            //        importStateTextBlock.Visibility = Visibility.Collapsed;
            //        importMessageBlock.Visibility = Visibility.Collapsed;
            //        importProgressBar.Visibility = Visibility.Collapsed;
            //        importProgressBar.Value = 0;
            //        this.Close();
            //    });
            //}
            //else
            //{

            //}
        }
    }
}
