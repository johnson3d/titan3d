using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace MaterialEditor.ResourceInfos
{
    class MaterialInstanceResourceCreateData : IResourceCreateData
    {
        [Browsable(false)]
        public EngineNS.RName.enRNameType RNameType { get; set; }
        [Browsable(false)]
        public string ResourceName { get; set; }
        [Browsable(false)]
        public ICustomCreateDialog HostDialog { get; set; }
        [Browsable(false)]
        public string Description { get; set; }
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Material)]
        public EngineNS.RName ParentMaterial { get; set; }
    }


    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance, ResourceExts = new string[] { ".instmtl" })]
    public class MaterialInstanceResourceInfo : EditorCommon.Resources.ResourceInfo, EditorCommon.Resources.IResourceInfoEditor, EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName => "材质实例";

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 79, 57));
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if(mResourceIcon==null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/MaterialInstanceConstant_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        }

        public string CreateMenuPath => "Materials & Textures/Material Instance";

        public bool IsBaseResource => true;

        public string EditorTypeName => "MaterialInstanceEditor";
        [EngineNS.Rtti.MetaData]
        public RName ParentMaterialRName
        {
            get;
            set;
        }

        public async System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string Absfolder, string rootFolder, EditorCommon.Resources.IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var data = createData as MaterialInstanceResourceCreateData;

            var result = new MaterialInstanceResourceInfo();

            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(Absfolder.TrimEnd('/') + "/" + data.ResourceName, rootFolder) + EngineNS.CEngineDesc.MaterialInstanceExtension;
            result.ResourceName = RName.GetRName(reName, data.RNameType);
            result.ParentMaterialRName = data.ParentMaterial;
            result.ReferenceRNameList.Add(data.ParentMaterial);
            var mtl = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(EngineNS.CEngine.Instance.RenderContext, data.ParentMaterial);
            var mtlInst = EngineNS.CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(
                EngineNS.CEngine.Instance.RenderContext,
                mtl, result.ResourceName);

            if(mtlInst==null)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Editor", $"MaterialInstance create error");
            }

            foreach(var i in mtl.ParamList)
            {
                mtlInst.SetParam(i);
            }

            mtlInst.SaveMaterialInstance();

            return result;
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            var result = new MaterialInstanceResourceCreateData();
            result.ResourceName = EditorCommon.Program.GetValidName(absFolder, "GfxMaterialInstance", CEngineDesc.MaterialInstanceExtension);
            return result;
        }

        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "GfxMaterialInstance", CEngineDesc.MaterialInstanceExtension);
        }
        public class MaterialInstanceEditProperty : INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            protected EngineNS.Graphics.CGfxMaterialInstance mMaterialInstance;
            public class SRVRName
            {
                public class ShaderVarPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
                {
                    public override string GetName(object arg)
                    {
                        var elem = arg as Element;
                        return elem.ShowName;
                    }
                    public override Type GetUIType(object arg)
                    {
                        return typeof(EngineNS.RName);
                    }
                    public override object GetValue(object arg)
                    {
                        var elem = arg as Element;
                        return elem.VarObject.mMaterialInstance.GetSRVName(elem.VarObject.Index);
                    }
                    public override void SetValue(object arg, object val)
                    {
                        var elem = arg as Element;

                        var value = val as EngineNS.RName;
                        var texture = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, value);
                        elem.VarObject.mMaterialInstance.SetSRV(elem.VarObject.Index, texture);
                    }
                }


                public UInt32 Index;
                public EngineNS.Graphics.CGfxMaterialInstance mMaterialInstance;
                public class Element
                {
                    public string ShowName;
                    public SRVRName VarObject;
                }
                [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Texture)]
                [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(ShaderVarPropertyUIProvider))]
                public object Name
                {
                    get
                    {
                        var elem = new Element();
                        elem.VarObject = this;
                        elem.ShowName = mMaterialInstance?.GetSRVShaderName(Index, true);
                        return elem;
                    }
                }
            }

            public class VarRName
            {
                public class ShaderVarPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
                {
                    public override string GetName(object arg)
                    {
                        var elem = arg as Element;
                        return elem.ShowName;
                    }
                    public override Type GetUIType(object arg)
                    {
                        var elem = arg as Element;
                        return elem.Type;
                    }
                    public override object GetValue(object arg)
                    {
                        var elem = arg as Element;
                        if(elem.Type == typeof(float))
                        {
                            float value = 0.0f;
                            elem.VarObject.mMaterialInstance.GetVarValue(elem.VarObject.Index, 0, ref value);
                            return value;
                        }
                        else if(elem.Type == typeof(Vector2))
                        {
                            var value = new Vector2();
                            elem.VarObject.mMaterialInstance.GetVarValue(elem.VarObject.Index, 0, ref value);
                            return value;
                        }
                        else if(elem.Type == typeof(Vector3))
                        {
                            var value = new Vector3();
                            elem.VarObject.mMaterialInstance.GetVarValue(elem.VarObject.Index, 0, ref value);
                            return value;
                        }
                        else if (elem.Type == typeof(Vector4))
                        {
                            var value = new Vector4();
                            elem.VarObject.mMaterialInstance.GetVarValue(elem.VarObject.Index, 0, ref value);
                            return value;
                        }
                        else if(elem.Type == typeof(EngineNS.Color))
                        {
                            var value = new Vector4();
                            elem.VarObject.mMaterialInstance.GetVarValue(elem.VarObject.Index, 0, ref value);
                            var retVal = new EngineNS.Color();
                            retVal.R = (byte)(value.X * 255);
                            retVal.G = (byte)(value.Y * 255);
                            retVal.B = (byte)(value.Z * 255);
                            retVal.A = (byte)(value.W * 255);
                            return retVal;
                        }
                        return null;
                    }
                    public override void SetValue(object arg, object val)
                    {
                        var elem = arg as Element;
                        if(elem.Type == typeof(float))
                        {
                            float value = (float)val;
                            elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        }
                        else if(elem.Type == typeof(Vector2))
                        {
                            var value = (Vector2)val;
                            elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        }
                        else if(elem.Type == typeof(Vector3))
                        {
                            var value = (Vector3)val;
                            elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        }
                        else if (elem.Type == typeof(Vector4))
                        {
                            var value = (Vector4)val;
                            elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        }
                        else if(elem.Type == typeof(EngineNS.Color))
                        {
                            var cl = (EngineNS.Color)val;
                            var value = new Vector4(cl.R / 255.0f, cl.G / 255.0f, cl.B / 255.0f, cl.A / 255.0f);
                            elem.VarObject.mMaterialInstance.SetVarValue(elem.VarObject.Index, 0, ref value);
                        }
                    }
                }

                public UInt32 Index;
                public EngineNS.Graphics.CGfxMaterialInstance mMaterialInstance;
                public class Element
                {
                    public System.Type Type;
                    public string ShowName;
                    public VarRName VarObject;
                }
                [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(ShaderVarPropertyUIProvider))]
                public object Name
                {
                    get
                    {
                        var elem = new Element();
                        elem.Type = typeof(Vector4);
                        elem.VarObject = this;
                        elem.ShowName = mMaterialInstance?.GetVarName(Index, true);
                        EngineNS.Graphics.CGfxVar varDesc = new EngineNS.Graphics.CGfxVar();
                        mMaterialInstance?.GetVarDesc(Index, ref varDesc);
                        var param = mMaterialInstance.Material.GetParam(mMaterialInstance.GetVarName(Index, false));
                        switch (varDesc.Type)
                        {
                            case EShaderVarType.SVT_Float1:
                                elem.Type = typeof(float);
                                break;
                            case EShaderVarType.SVT_Float2:
                                elem.Type = typeof(Vector2);
                                break;
                            case EShaderVarType.SVT_Float3:
                                elem.Type = typeof(Vector3);
                                break;
                            case EShaderVarType.SVT_Float4:
                                {
                                    if(param != null && param.EditorType == "Color")
                                        elem.Type = typeof(EngineNS.Color);
                                    else
                                        elem.Type = typeof(Vector4);
                                }
                                break;
                            //case EShaderVarType.SVT_Matrix4x4:
                            //    elem.Type = typeof(EngineNS.Matrix);
                            //    break;
                            default:
                                throw new InvalidOperationException();
                        }
                        return elem;
                    }
                }
            }
            public void SetMaterialInstance(EngineNS.Graphics.CGfxMaterialInstance mtl)
            {
                mMaterialInstance = mtl;

                mSRVPrimitives = new List<SRVRName>();
                for (UInt32 i = 0; i < mMaterialInstance.SRVNumber; i++)
                {
                    var item = new SRVRName();
                    item.Index = i;
                    item.mMaterialInstance = mMaterialInstance;
                    mSRVPrimitives.Add(item);
                }

                mShaderVars = new List<VarRName>();
                for (UInt32 i = 0; i < mMaterialInstance.VarNumber; i++)
                {
                    var item = new VarRName();
                    item.Index = i;
                    item.mMaterialInstance = mMaterialInstance;
                    mShaderVars.Add(item);
                }
            }

            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Material)]
            [DisplayName("材质")]
            public RName MaterialSourceName
            {
                get { return mMaterialInstance?.MaterialName; }
                set
                {
                    var noUse = mHostCtrl.OnResetPreviewMaterialParentMaterial(this, value);
                }
            }
            List<SRVRName> mSRVPrimitives;
            public List<SRVRName> Textures
            {
                get { return mSRVPrimitives; }
            }
            List<VarRName> mShaderVars;
            public List<VarRName> ShaderVars
            {
                get { return mShaderVars; }
            }
            [Browsable(false)]
            public EngineNS.Graphics.CGfxMaterialInstance MaterialInstance
            {
                get
                {
                    return mMaterialInstance;
                }
            }
            public EngineNS.Graphics.View.ERenderLayer RenderLayer
            {
                get
                {
                    return mMaterialInstance.mRenderLayer;
                }
                set
                {
                    mMaterialInstance.mRenderLayer = value;
                }
            }

            MaterialInstanceEditorControl mHostCtrl;
            public MaterialInstanceEditProperty(MaterialInstanceEditorControl ctrl)
            {
                mHostCtrl = ctrl;
            }
            public MaterialInstanceEditProperty()
            {

            }
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            var context = new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this);
            await EditorCommon.Program.OpenEditor(context);
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + CEngineDesc.MaterialInstanceExtension, SearchOption.TopDirectoryOnly);
            if (files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的材质实例文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new MaterialInstanceResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance;

            return retValue;
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address);
            var snapShotFile = ResourceName.Address + EditorCommon.Program.SnapshotExt;
            EngineNS.CEngine.Instance.FileManager.DeleteFile(snapShotFile);
            return true;
        }
         
        protected override async Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }

        bool mIsGenerationSnapshot = false;
        public override async Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
        {
            if (mIsGenerationSnapshot)
                return null;
            var snapShotFile = ResourceName.Address + ".snap";
            if(!forceCreate)
            {
                var imgSource = await EditorCommon.ImageInit.GetImage(snapShotFile);
                if (imgSource != null)
                {
                    return imgSource;
                }
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

            var mtlInst = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, ResourceName);
            var SyncOK = await mCurMesh.SetMaterialInstanceAsync(rc, 0, mtlInst, null);
            if (SyncOK == true)
            {
                var snapShorter = new EditorCommon.SnapshotProcess.SnapshotCreator();//EngineNS.Editor.SnapshotCreator();//
                SyncOK = await snapShorter.InitEnviroment();
                if (SyncOK == false)
                {
                    mIsGenerationSnapshot = false;
                    return null;
                }

                var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mCurMesh);

                mCurMesh.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
                snapShorter.World.AddActor(actor);
                snapShorter.World.GetScene(EngineNS.RName.GetRName("SnapshorCreator")).AddActor(actor);

                actor.Placement.Location = new EngineNS.Vector3(0, 0, 0);

                //var nouse = snapShorter.SaveToFile(snapShotFile);
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

            // 复制
            var duplicateMenuItem = new MenuItem()
            {
                Name = "MatInsResInfo_Duplicate",
                Header = "复制",
                Style = menuItemStyle,
            };
            duplicateMenuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                var dir = ResourceName.GetDirectory();
                var inputWin = new InputWindow.InputWindow();
                inputWin.Title = $"复制{ResourceName.PureName()}";
                inputWin.Description = "输入材质实例名称";
                inputWin.Value = EditorCommon.Program.GetValidName(dir, ResourceName.PureName(), CEngineDesc.MaterialInstanceExtension);
                if (inputWin.ShowDialog((value, cultureInfo) =>
                {
                    if (value == null)
                        return new ValidationResult(false, "内容不合法");
                    return ResourceNameAvailable(dir, value.ToString());
                }) == false)
                    return;

                var rc = EngineNS.CEngine.Instance.RenderContext;
                // materialInstance 
                var mat = await EngineNS.CEngine.Instance.MaterialManager.GetMaterialAsync(rc, this.ParentMaterialRName);
                if(mat == null)
                {
                    EditorCommon.MessageBox.Show($"没找到材质模板{this.ParentMaterialRName.Address}");
                    return;
                }
                var srcMatIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, ResourceName);
                var copyedRName = EngineNS.RName.GetRName(EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(dir + inputWin.Value + CEngineDesc.MaterialInstanceExtension, ResourceName.GetRootFolder()), ResourceName.RNameType);
                var copyedMatIns = EngineNS.CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(rc, mat, copyedRName);
                srcMatIns.SetDataToMaterialInstance(copyedMatIns);
                copyedMatIns.SaveMaterialInstance();
                // resourceInfo
                var copyedRInfo = new MaterialInstanceResourceInfo();
                copyedRInfo.ResourceName = copyedRName;
                await this.CopyResource(copyedRInfo);
                await copyedRInfo.Save();
                // snapshot
                EngineNS.CEngine.Instance.FileManager.CopyFile(this.ResourceName.Address + EditorCommon.Program.SnapshotExt, copyedRName.Address + EditorCommon.Program.SnapshotExt, true);
                await copyedRInfo.InitializeContextMenu();
                ParentBrowser.AddResourceInfo(copyedRInfo);
                ParentBrowser.SelectResourceInfo(copyedRInfo);
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(duplicateMenuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_Edit_Duplicate_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(duplicateMenuItem);

            // 删除
            var menuItem = new MenuItem()
            {
                Name = "MatInsResInfo_Delete",
                Header = "删除",
                Style = menuItemStyle,
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
                EngineNS.CEngine.Instance.MaterialInstanceManager.Materials.Remove(ResourceName);
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);


            return true;
        }

        protected override async Task CopyResource(ResourceInfo copyTo)
        {
            await base.CopyResource(copyTo);
            var cpInfo = copyTo as MaterialInstanceResourceInfo;
            cpInfo.ParentMaterialRName = ParentMaterialRName;
        }

        protected override async Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            if(referencedResInfo.ResourceType == EngineNS.Editor.Editor_RNameTypeAttribute.Material)
            {
                ReferenceRNameList.Remove(ParentMaterialRName);
                if(newRName != null)
                {
                    ParentMaterialRName = referencedResInfo.ResourceName;
                    ReferenceRNameList.Add(ParentMaterialRName);
                }
                await Save();

                var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
                EngineNS.CEngine.Instance.MaterialInstanceManager.RemoveMaterialFromDic(ResourceName);
                matIns.OnlySetMaterialName(referencedResInfo.ResourceName);
                matIns.SaveMaterialInstance();
            }
            else if(referencedResInfo.ResourceType == EngineNS.Editor.Editor_RNameTypeAttribute.Texture)
            {
                ReferenceRNameList.Remove(oldRName);
                if(newRName != null)
                    ReferenceRNameList.Add(referencedResInfo.ResourceName);
                await Save();

                var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
                for(uint i=0; i<matIns.SRVNumber; i++)
                {
                    var srvName = matIns.GetSRVName(i);
                    if(srvName == oldRName)
                    {
                        var texture = CEngine.Instance.TextureManager.GetShaderRView(CEngine.Instance.RenderContext, referencedResInfo.ResourceName);
                        matIns.SetSRV(i, texture);
                    }
                }
                matIns.SaveMaterialInstance();
            }
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }

        public override async Task Save(bool withSnapshot = false)
        {
            var matIns = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
            RefreshReferenceRNames(matIns);
            // 刷新资源引用表
            await EngineNS.CEngine.Instance.GameEditorInstance.RefreshResourceInfoReferenceDictionary(this);
            await base.Save(withSnapshot);
        }
        public void RefreshReferenceRNames(EngineNS.Graphics.CGfxMaterialInstance matIns)
        {
            // 刷新资源引用
            ReferenceRNameList.Clear();
            ReferenceRNameList.Add(matIns.MaterialName);
            for (UInt32 i = 0; i < matIns.SRVNumber; i++)
            {
                var rName = matIns.GetSRVName(i);
                ReferenceRNameList.Add(rName);
            }
        }
        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            var matIns = new EngineNS.Graphics.CGfxMaterialInstance();
            await matIns.PureLoadMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, data.RNameMapper.Name);
            data.RNameMapper.ResObject = matIns;
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var newFile = data.GetTargetAbsFileName();
            var matIns = data.ResObject as EngineNS.Graphics.CGfxMaterialInstance;
            matIns.SaveMaterialInstance(newFile);
            RefreshReferenceRNames(matIns);

            return true;
        }

    }
}
