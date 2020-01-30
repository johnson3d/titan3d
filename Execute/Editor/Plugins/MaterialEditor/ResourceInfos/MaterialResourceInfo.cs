using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;

namespace MaterialEditor.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Material, ResourceExts = new string[] { ".material" })]
    public class MaterialResourceInfo : EditorCommon.Resources.ResourceInfo, EditorCommon.Resources.IResourceInfoEditor, EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName => "材质";

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(114, 154, 75));
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if(mResourceIcon==null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Material_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        }

        public string CreateMenuPath => "Materials & Textures/Material";

        public bool IsBaseResource => true;
        
        public async System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string Absfolder, string rootFolder, EditorCommon.Resources.IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var result = new MaterialResourceInfo();

            var data = createData as EditorCommon.Resources.ResourceCreateDataBase;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(Absfolder + "/" + data.ResourceName, rootFolder);
            reName += EngineNS.CEngineDesc.MaterialExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);

            {
                // .var文件创建
                EngineNS.CEngine.Instance.FileManager.CreateFileAndFlush(result.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension);
                
                // .code文件创建
                string codeContent = "void DoVSMaterial(in PS_INPUT input, inout MTL_OUTPUT mtl)\r\n{\r\n}\r\n";
                codeContent += "void DoPSMaterial(in PS_INPUT input, inout MTL_OUTPUT mtl)\r\n{\r\n}\r\n";
                codeContent += "#ifndef DO_VS_MATERIAL\r\n";
                codeContent += "#define DO_VS_MATERIAL DoVSMaterial\r\n";
                codeContent += "#endif\r\n";
                codeContent += "#ifndef DO_PS_MATERIAL\r\n";
                codeContent += "#define DO_PS_MATERIAL DoPSMaterial\r\n";
                codeContent += "#endif\r\n";


                EngineNS.CEngine.Instance.FileManager.WriteAllTextASCII(result.ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, codeContent);
            }
            
            var mat = EngineNS.CEngine.Instance.MaterialManager.NewMaterial(result.ResourceName);
            mat.SaveMaterial();

            return result;
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return null;
        }

        public string EditorTypeName
        {
            get => "MaterialEditor";
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            // 判断资源名称是否合法
            if (EditorCommon.Program.IsValidRName(name)==false)
            {
                return new ValidationResult(false, "名称不合法!");
            }

            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + EngineNS.CEngineDesc.MaterialExtension, SearchOption.TopDirectoryOnly);
            if(files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的材质文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(EngineNS.RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new MaterialResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Material;

            return retValue;
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.EffectManager.UnRegEffect_Editor(ResourceName);

            // material
            EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address);
            // var
            var varFile = ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension;
            EngineNS.CEngine.Instance.FileManager.DeleteFile(varFile);
            // code
            var codeFile = ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension;
            EngineNS.CEngine.Instance.FileManager.DeleteFile(codeFile);
            // link
            var linkFile = ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension;
            EngineNS.CEngine.Instance.FileManager.DeleteFile(linkFile);
            // snapshot
            var snapShotFile = ResourceName.Address + EditorCommon.Program.SnapshotExt;
            EngineNS.CEngine.Instance.FileManager.DeleteFile(snapShotFile);

            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            var tagRName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(absFolder + currentResourceName.PureName(true));
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if(EngineNS.CEngine.Instance.FileManager.FileExists(tagRName.Address))
            {
                if (EditorCommon.MessageBox.Show($"文件夹{absFolder}已存在文件{currentResourceName.PureName()}，是否替换?") != EditorCommon.MessageBox.enMessageBoxResult.Yes)
                    return false;
            }
            // var
            EngineNS.CEngine.Instance.FileManager.MoveFile(currentResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, tagRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension);
            // code
            EngineNS.CEngine.Instance.FileManager.MoveFile(currentResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, tagRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension);
            // link
            if(EngineNS.CEngine.Instance.FileManager.FileExists(currentResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension))
                EngineNS.CEngine.Instance.FileManager.MoveFile(currentResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension, tagRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension);
            // snapshot
            if(EngineNS.CEngine.Instance.FileManager.FileExists(currentResourceName.Address + EditorCommon.Program.SnapshotExt))
                EngineNS.CEngine.Instance.FileManager.MoveFile(currentResourceName.Address + EditorCommon.Program.SnapshotExt, tagRName.Address + EditorCommon.Program.SnapshotExt);
            // material
            EngineNS.CEngine.Instance.FileManager.MoveFile(currentResourceName.Address, tagRName.Address);
            var mat = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, tagRName, true);
            mat.Name = tagRName;
            mat.SaveMaterial();

            // 刷新所有材质实例
            // 刷新所有打开的材质实例编辑器
            //var matInsFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.Content, "*" + EngineNS.CEngineDesc.MaterialInstanceExtension, System.IO.SearchOption.AllDirectories);
            //List<string> refreshedMatIns = new List<string>();
            //foreach (var ctrlData in EditorCommon.PluginAssist.Process.ControlsDic)
            //{
            //    if (ctrlData.Key.GetExtension() == EngineNS.CEngineDesc.MaterialInstanceExtension.TrimStart('.'))
            //    {
            //        var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, ctrlData.Key);
            //        if (matIns.Material.Name == currentResourceName)
            //        {
            //            EngineNS.CEngine.Instance.MaterialInstanceManager.RemoveMaterialFromDic(ctrlData.Key);
            //            var matInsEditor = ctrlData.Value.Content as MaterialInstanceEditorControl;
            //            var insResInfo = matInsEditor.CurrentContext.ResInfo as MaterialInstanceResourceInfo;
            //            insResInfo.ParentMaterialRName = tagRName;
            //            await insResInfo.Save();

            //            matIns.OnlySetMaterialName(tagRName);
            //            refreshedMatIns.Add(tagRName.Address);
            //            matIns.SaveMaterialInstance();
            //        }
            //    }
            //}
            //foreach(var file in matInsFiles)
            //{
            //    var relFile = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(file, EngineNS.CEngine.Instance.FileManager.Content);
            //    var rName = EngineNS.CEngine.Instance.FileManager.GetRName(relFile);
            //    if (refreshedMatIns.Contains(rName.Address))
            //        continue;

            //    var rInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromResource(rName.Address) as ResourceInfos.MaterialInstanceResourceInfo;
            //    await rInfo.Load(rName.Address + EditorCommon.Program.ResourceInfoExt);
            //    if (rInfo.ParentMaterialRName == currentResourceName)
            //    {
            //        rInfo.ParentMaterialRName = tagRName;
            //        await rInfo.Save();

            //        var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, rName);
            //        EngineNS.CEngine.Instance.MaterialInstanceManager.RemoveMaterialFromDic(rName);
            //        matIns.OnlySetMaterialName(tagRName);
            //        //matIns.Editor_SetMaterial(mat);
            //        matIns.SaveMaterialInstance();
            //    }
            //}

            return true;
        }

        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "GfxMaterial", EngineNS.CEngineDesc.MaterialExtension);
        }

        bool mIsGenerationSnapshot = false;
        public override async Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
        {
            if (mIsGenerationSnapshot)
                return null;
            var snapShotFile = ResourceName.Address + ".snap";
            if (!forceCreate)
            {
                var imgSource = await EditorCommon.ImageInit.GetImage(snapShotFile);
                if (imgSource != null)
                    return imgSource;
            }

            mIsGenerationSnapshot = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var meshSource = EngineNS.CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, 
                    EngineNS.CEngine.Instance.FileManager.GetRName("Meshes/sphere.vms", EngineNS.RName.enRNameType.Editor), true);
            var mCurMesh = EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, meshSource/*, EditorCommon.SnapshotProcess.SnapshotCreator.GetShadingEnv()*/);
            if (mCurMesh == null)
            {
                mIsGenerationSnapshot = false;
                return null;
            }

            var mtl = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, ResourceName);
            if (mtl == null)
            {
                mIsGenerationSnapshot = false;
                return null;
            }
            var mtlInst = EngineNS.CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(rc, mtl);
            foreach (var i in mtl.ParamList)
            {
                mtlInst.SetParam(i);
            }
            var SyncOK =  await mCurMesh.SetMaterialInstanceAsync(rc, 0, mtlInst, null);
            if (SyncOK == true)
            {
                var snapShorter = new EditorCommon.SnapshotProcess.SnapshotCreator();//EngineNS.Editor.SnapshotCreator();//
                var IsReady = await snapShorter.InitEnviroment();
                if (IsReady == false)
                {
                    mIsGenerationSnapshot = false;
                    return null;
                }

                var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mCurMesh);

                mCurMesh.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
                snapShorter.World.AddActor(actor);
                snapShorter.World.GetScene(EngineNS.RName.GetRName("SnapshorCreator")).AddActor(actor);

                actor.Placement.Location = new EngineNS.Vector3(0, 0, 0);

                //await snapShorter.SaveToFile(snapShotFile);
                actor.PreUse(true);
                await snapShorter.SaveToFile(snapShotFile, 1000, 8);
            }
            mIsGenerationSnapshot = false;
            return await EditorCommon.ImageInit.GetImage(snapShotFile);
        }

        protected override async Task<bool> InitializeContextMenuOverride(ContextMenu contextMenu)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var textSeparatorStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as System.Windows.Style;
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
            contextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
            {
                Text = "Common",
                Style = textSeparatorStyle,
            });

            var ciMenuItem = new MenuItem()
            {
                Name = "MatResInfo_CreateMatIns",
                Header = "创建材质实例",
                Style = menuItemStyle,
            };
            ciMenuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                var resInfo = new MaterialInstanceResourceInfo();

                var win = new EditorCommon.Controls.ResourceBrowser.CreateResDialog();
                win.Title = $"创建材质实例";
                win.ResourceName = EditorCommon.Program.GetValidName(ResourceName.GetDirectory(), ResourceName.PureName() + "_ins", EngineNS.CEngineDesc.MaterialInstanceExtension);
                var data = new MaterialInstanceResourceCreateData()
                {
                    ResourceName = win.ResourceName,
                    ParentMaterial = this.ResourceName,
                };
                win.ResCreateData = data;
                data.HostDialog = win;
                if (win.ShowDialog((value, cultureInfo) =>
                {
                    if(value == null)
                        return new ValidationResult(false, "内容不合法");
                    return resInfo.ResourceNameAvailable(ResourceName.GetDirectory(), value.ToString());
                }) == false)
                    return;

                var createData = win.GetCreateData();
                createData.RNameType = ResourceName.RNameType;
                var resourceInfo = await resInfo.CreateEmptyResource(ResourceName.GetDirectory(), ResourceName.GetRootFolder(), createData);
                var pro = resourceInfo.GetType().GetProperty("ResourceType");
                pro?.SetValue(resourceInfo, EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance);
                resourceInfo.ParentBrowser = this.ParentBrowser;
                await resourceInfo.Save(true);
                this.ParentBrowser.AddResourceInfo(resourceInfo);
                this.ParentBrowser.SelectResourceInfo(resourceInfo);
                await resourceInfo.InitializeContextMenu();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(ciMenuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/MaterialInstanceConstant_64x.png", UriKind.Absolute)));
            contextMenu.Items.Add(ciMenuItem);

            var duplicateMenuItem = new MenuItem()
            {
                Name = "MatResInfo_Duplicate",
                Header = $"复制",
                Style = menuItemStyle,
            };
            duplicateMenuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                var dir = ResourceName.GetDirectory();
                var inputWin = new InputWindow.InputWindow();
                inputWin.Title = $"复制{ResourceName.PureName()}";
                inputWin.Description = "输入材质名称";
                inputWin.Value = EditorCommon.Program.GetValidName(dir, ResourceName.PureName(), EngineNS.CEngineDesc.MaterialExtension);
                if (inputWin.ShowDialog((value, cultureInfo) =>
                {
                    if (value == null)
                        return new ValidationResult(false, "内容不合法");
                    return ResourceNameAvailable(dir, value.ToString());
                }) == false)
                    return;

                var rc = EngineNS.CEngine.Instance.RenderContext;
                var tagRName = EngineNS.RName.EditorOnly_GetRNameFromAbsFile(dir + inputWin.Value + EngineNS.CEngineDesc.MaterialExtension);
                // mat
                var mat = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, ResourceName);
                var matCopy = EngineNS.CEngine.Instance.MaterialManager.NewMaterial(tagRName);
                mat.CopyTo(matCopy);
                matCopy.SaveMaterial();
                // link
                var xndHolder = await EngineNS.IO.XndHolder.LoadXND(ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension);
                if(xndHolder != null)
                {
                    var container = new CodeGenerateSystem.Base.NodesContainer();
                    await container.Load(xndHolder.Node);
                    xndHolder.Node.TryReleaseHolder();

                    Controls.MaterialControl matCtrl = null;
                    foreach (var node in container.OrigionNodeControls)
                    {
                        if(node is Controls.MaterialControl)
                        {
                            node.Id = Guid.NewGuid();
                            matCtrl = node as Controls.MaterialControl;
                        }
                    }

                    System.IO.TextWriter codeFile, varFile;
                    CodeGenerator.GenerateCode(container, matCtrl, out codeFile, out varFile);

                    // var
                    System.IO.File.WriteAllText(tagRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, varFile.ToString());
                    // code
                    System.IO.File.WriteAllText(tagRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, codeFile.ToString());
                    // link
                    var tagHolder = EngineNS.IO.XndHolder.NewXNDHolder();
                    container.Save(tagHolder.Node);
                    EngineNS.IO.XndHolder.SaveXND(tagRName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension, tagHolder);
                }
                // resInfo
                var copyedRInfo = new MaterialResourceInfo();
                copyedRInfo.ResourceName = tagRName;
                await CopyResource(copyedRInfo);
                await copyedRInfo.Save();
                // snapshot
                EngineNS.CEngine.Instance.FileManager.CopyFile(ResourceName.Address + EditorCommon.Program.SnapshotExt, tagRName.Address + EditorCommon.Program.SnapshotExt, true);
                await copyedRInfo.InitializeContextMenu();
                ParentBrowser.AddResourceInfo(copyedRInfo);
                ParentBrowser.SelectResourceInfo(copyedRInfo);
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(duplicateMenuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_Edit_Duplicate_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(duplicateMenuItem);

            var menuItem = new MenuItem()
            {
                Name = "MatResInfo_Delete",
                Header = "删除",
                Style = menuItemStyle,
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
                EngineNS.CEngine.Instance.MaterialManager.Materials.Remove(ResourceName);
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            return true;
        }
        
        protected override async Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            if(referencedResInfo.ResourceType == EngineNS.Editor.Editor_RNameTypeAttribute.Texture)
            {
                ReferenceRNameList.Remove(oldRName);
                ReferenceRNameList.Add(referencedResInfo.ResourceName);
                await Save();

                // mat
                bool bFinded = false;
                var mat = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
                foreach(var param in mat.ParamList)
                {
                    if(param.VarType == EngineNS.EShaderVarType.SVT_Texture && param.TextureRName.Equals(oldRName))
                    {
                        param.SetValueStr(referencedResInfo.ResourceName);
                        bFinded = true;
                    }
                }
                mat.SaveMaterial();

                // link
                if (bFinded)
                {
                    var xndHolder = await EngineNS.IO.XndHolder.LoadXND(ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension);
                    if(xndHolder != null)
                    {
                        var container = new CodeGenerateSystem.Base.NodesContainer();
                        await container.Load(xndHolder.Node);
                        xndHolder.Node.TryReleaseHolder();

                        foreach(var node in container.CtrlNodeList)
                        {
                            if(node is Controls.TextureControl)
                            {
                                var tc = node as Controls.TextureControl;
                                if(tc.TextureRName == oldRName)
                                    tc.TextureRName = referencedResInfo.ResourceName;
                            }
                        }

                        var saveHolder = EngineNS.IO.XndHolder.NewXNDHolder();
                        container.Save(saveHolder.Node);
                        EngineNS.IO.XndHolder.SaveXND(ResourceName.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension, saveHolder);
                    }
                }
            }
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }

        public override async Task Save(bool withSnapshot = false)
        {
            var mat = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
            RefreshReferenceRNames(mat);
            // 刷新资源引用表
            await EngineNS.CEngine.Instance.GameEditorInstance.RefreshResourceInfoReferenceDictionary(this);
            await base.Save(withSnapshot);
        }

        class MatAssetsObject
        {
            public EngineNS.Graphics.CGfxMaterial Material;
            [EngineNS.Bricks.SandBox.NotProcessObjectReferenceAttribute]
            public CodeGenerateSystem.Controls.NodesContainerControl NodesCtrl;
        }
        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            // material
            var assetsObj = new MatAssetsObject();
            assetsObj.Material = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(EngineNS.CEngine.Instance.RenderContext, data.RNameMapper.Name);
            var nodesCtrl = new CodeGenerateSystem.Controls.NodesContainerControl();
            nodesCtrl.CSType = EngineNS.ECSType.Client;
            var xndHolder = await EngineNS.IO.XndHolder.LoadXND(data.RNameMapper.Name.Address + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension);
            if (xndHolder != null)
            {
                await nodesCtrl.Load(xndHolder.Node);
                xndHolder.Node?.TryReleaseHolder();
            }
            assetsObj.NodesCtrl = nodesCtrl;
            data.RNameMapper.ResObject = assetsObj;

            return true;
        }
        public void RefreshReferenceRNames(EngineNS.Graphics.CGfxMaterial mat)
        {
            // 资源引用刷新
            ReferenceRNameList.Clear();
            foreach (var param in mat.ParamList)
            {
                switch (param.VarType)
                {
                    case EngineNS.EShaderVarType.SVT_Texture:
                    case EngineNS.EShaderVarType.SVT_Sampler:
                        {
                            ReferenceRNameList.Add(param.TextureRName);
                        }
                        break;
                }
            }
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var matAsset = data.ResObject as MatAssetsObject;
            var newFile = data.GetTargetAbsFileName();
            var srcFile = data.GetSourceAbsFileName();

            var mat = matAsset.Material;
            mat.SaveMaterial(newFile);

            // link
            var nodesControl = matAsset.NodesCtrl;
            var saveXnd = EngineNS.IO.XndHolder.NewXNDHolder();
            nodesControl.Save(saveXnd.Node);
            EngineNS.IO.XndHolder.SaveXND(newFile + EngineNS.Graphics.CGfxMaterial.ShaderLinkExtension, saveXnd);

            RefreshReferenceRNames(mat);

            if(!string.Equals(newFile, srcFile, StringComparison.OrdinalIgnoreCase))
            {
                // var
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcFile + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, 
                                                               newFile + EngineNS.Graphics.CGfxMaterial.ShaderDefineExtension, true);
                // code
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcFile + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension,
                                                               newFile + EngineNS.Graphics.CGfxMaterial.ShaderIncludeExtension, true);
            }

            return true;
        }

    }
}
