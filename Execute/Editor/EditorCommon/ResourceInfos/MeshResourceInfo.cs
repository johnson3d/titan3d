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
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Mesh, ResourceExts = new string[] { ".gms" })]
    public class MeshResourceInfo : EditorCommon.Resources.ResourceInfo, 
                                    EditorCommon.Resources.IResourceInfoDragDrop, 
                                    EditorCommon.Resources.IResourceInfoEditor, 
                                    EditorCommon.Resources.IResourceInfoForceReload, 
                                    EditorCommon.Resources.IResourceInfoCreateEmpty,
                                    EditorCommon.Resources.IResourceInfoDragToViewport,
                                    EditorCommon.Resources.IResourceInfoCreateActor,
                                    EditorCommon.Resources.IResourceInfoCreateComponent
    {
        public class MeshCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public EngineNS.RName.enRNameType RNameType { get; set; }
            [Browsable(false)]
            public string ResourceName { get; set; }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource)]
            public RName GeomMesh
            {
                get;
                set;
            } = RName.EmptyName;
			[EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.ShadingEnv)]
            public RName ShadingEnv
            {
                get;
                set;
            }
            [Browsable(false)]
            public string Description { get; set; }
        }
        [EngineNS.Rtti.MetaData]
        public string SkeletonAsset
        {
            get; set;
        }
        public override string ResourceTypeName
        {
            get { return EngineNS.Editor.Editor_RNameTypeAttribute.Mesh; }
        }
        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(170, 88, 59));
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if(mResourceIcon==null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/StaticMesh_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        }

        public string CreateMenuPath => "Mesh/Mesh";

        public bool IsBaseResource => true;

        public string EditorTypeName => "MeshEditor";

        public async System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string Absfolder, string rootFolder, EditorCommon.Resources.IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new MeshResourceInfo();
            
            var mcd = createData as MeshCreateData;
            if(EngineNS.CEngine.Instance.FileManager.GetFileExtension(mcd.ResourceName)!=EngineNS.CEngineDesc.MeshExtension)
            {
                mcd.ResourceName = mcd.ResourceName + EngineNS.CEngineDesc.MeshExtension;
            }
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(Absfolder + "/" + mcd.ResourceName, rootFolder);
            result.ResourceName = RName.GetRName(reName, mcd.RNameType);
            var vmsInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(mcd.GeomMesh.Address + ".rinfo",null) as MeshSourceResourceInfo;
            result.SkeletonAsset = vmsInfo.SkeletonAsset;
            RName meshPrimitives = mcd.GeomMesh;
            //var shadingEnv = CEngine.Instance.ShadingEnvManager.GetGfxShadingEnv(mcd.ShadingEnv);
            var mCurMesh = EngineNS.CEngine.Instance.MeshManager.NewMesh(
                EngineNS.CEngine.Instance.RenderContext,
                result.ResourceName, meshPrimitives/*, shadingEnv*/);
            var mtl = EngineNS.CEngine.Instance.MaterialInstanceManager.DefaultMaterialInstance;
            for (UInt32 i = 0; i < mCurMesh.MtlMeshArray.Length; i++)
            {
                await mCurMesh.SetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, i, mtl, null);
            }
            mCurMesh.SaveMesh();

            return result;
        }
        public bool DragEnter(DragEventArgs e)
        {
            return false;
        }

        public void DragLeave(DragEventArgs e)
        {

        }

        public void DragOver(DragEventArgs e)
        {

        }

        public void Drop(DragEventArgs e)
        {

        }

        public void ForceReload()
        {
            //throw new NotImplementedException();
        }
        public string[] GetFileSystemWatcherAttentionExtensions()
        {
            return new string[0];
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            var result = new MeshCreateData();
            result.ResourceName = EditorCommon.Program.GetValidName(absFolder, "GfxMesh", CEngineDesc.MeshExtension);
            result.GeomMesh = RName.GetRName("Mesh/box.vms");
            result.ShadingEnv = RName.GetRName("ShadingEnv/fsbase.senv");
            return result;
        }

        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "GfxMesh", CEngineDesc.MeshExtension);
        }
        public class MtlRNameContainer : EngineNS.Editor.Editor_RNameTypeObjectBind
        {
            [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(MaterialInstancePropertyUIProvider))]
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MaterialInstance, true)]
            public MtlRName Name
            {
                get;
                set;
            } = new MtlRName();


            public void invoke(object param)
            {
                EngineNS.Graphics.Mesh.CGfxMtlMesh mtlmesh =  Name.mMesh.MtlMeshArray[Name.Index];
                if (mtlmesh != null)
                    mtlmesh.Visible = (bool)param;
            }
        }
        public class MtlRName
        {
            public int Index;
            public EngineNS.Graphics.Mesh.CGfxMesh mMesh;

            RName mName;
            public RName Name
            {
                get
                {
                    // 不能直接返回mMesh?.MtlMeshArray[Index]?.MtlInst?.Name，Set的时候异步了，这里界面直接取值会取到旧的值
                    if (mName == null)
                    {
                        mName = mMesh?.MtlMeshArray[Index]?.MtlInst?.Name;
                    }
                    return mName;
                }
                set
                {
                    mName = value;
                    Action action = async () =>
                    {
                        if(value == null)
                        {
                            await mMesh.SetMaterialInstanceAsync(CEngine.Instance.RenderContext, (UInt32)Index, CEngine.Instance.MaterialInstanceManager.DefaultMaterialInstance, null);
                        }
                        else
                        {
                            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(CEngine.Instance.RenderContext, value);
                            await mMesh.SetMaterialInstanceAsync(CEngine.Instance.RenderContext, (UInt32)Index, mtl, null);
                        }
                    };
                    action();
                }
            }

            bool mVisible = true;
            public bool Visible
            {
                get
                {
                    return mVisible;
                }
                set
                {
                    mVisible = value;
                }
            }
        }
        public class MaterialInstancePropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
        {
            public override string GetName(object arg)
            {
                var elem = arg as MtlRName;
                return "Material" + (elem.Index+1).ToString();
            }
            public override Type GetUIType(object arg)
            {
                var elem = arg as MtlRName;
                return elem.Name.GetType();
            }
            public override object GetValue(object arg)
            {
                var elem = arg as MtlRName;
                return elem.Name;
            }
            public override void SetValue(object arg, object val)
            {
                var elem = arg as MtlRName;
                elem.Name = val as RName;
            }
        }
        public class MeshEditProperty
        {
            protected EngineNS.Graphics.Mesh.CGfxMesh mMesh;
           
            public void SetMesh(EngineNS.Graphics.Mesh.CGfxMesh mesh)
            {
                mMesh = mesh;

                mMaterialPrimitives = new List<MtlRNameContainer>();
                for(int i=0;i< mesh.MtlMeshArray.Length;i++)
                {
                    var item = new MtlRNameContainer();
                    item.Name.Index = i;
                    item.Name.mMesh= mesh;
                    mMaterialPrimitives.Add(item);
                }
            }
            [DisplayName("MeshPrimitive")]
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource)]
            public RName MeshPrimitiveName
            {
                get { return mMesh?.MeshPrimitiveName; }
            }
            List<MtlRNameContainer> mMaterialPrimitives;
            [EditorCommon.Editor_PropertyGridSortTypeAttribute]
            [DisplayName("MaterialList")]
            public List<MtlRNameContainer> mMtlMeshArray
            {
                get { return mMaterialPrimitives; }
            }
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
            return;
            //var context = new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this);
            
            //var showValue = new MeshEditProperty();
            //var mesh = CEngine.Instance.MeshManager.GetMeshOrigion(CEngine.Instance.RenderContext, this.ResourceName/*, CEngine.Instance.ShadingEnvManager.DefaultShadingEnv*/);
            //showValue.SetMesh(mesh);
            //context.PropertyShowValue = showValue;
            //EditorCommon.Program.OpenEditor(context);

            //context.SaveAction = () =>
            //{
            //    mesh.SaveMesh();
            //};
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + CEngineDesc.MeshExtension, SearchOption.TopDirectoryOnly);
            if (files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的模型文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new MeshResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Mesh;

            return retValue;
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            CEngine.Instance.MeshManager.RemoveMesh(ResourceName);
            if (System.IO.File.Exists(ResourceName.Address))
                System.IO.File.Delete(ResourceName.Address);
            if (System.IO.File.Exists(ResourceName.Address + EditorCommon.Program.SnapshotExt))
                System.IO.File.Delete(ResourceName.Address + EditorCommon.Program.SnapshotExt);

            return true;
        }

        protected override async Task<bool> InitializeContextMenuOverride(ContextMenu contextMenu)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var textSeparatorStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as System.Windows.Style;
            contextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
            {
                Text = "Common",
                Style = textSeparatorStyle,
            });
            // <MenuItem Header="Add Feature or Content Pack..." menu:MenuAssist.Icon="/ResourceLibrary;component/Icons/Icons/icon_file_saveall_40x.png" Style="{DynamicResource {ComponentResourceKey ResourceId=MenuItem_Default, TypeInTargetAssembly={x:Type res:CustomResources}}}"/>
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
            var menuItem = new MenuItem()
            {
                Name = "UI_Menu_Edit",
                Header = "Edit...",
                Style = menuItemStyle,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                var noUsed = OpenEditor();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Edit/icon_Edit_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Name = "Rename",
                Header = "Rename",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Icon_Asset_Rename_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Name = "Duplicate",
                Header = "Duplicate",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Edit/icon_Edit_Duplicate_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Name = "Save",
                Header = "Save",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_levels_Save_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Name = "Delete",
                Header = "Delete",
                Style = menuItemStyle,
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator()
            {
                Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuSeparatorStyle")) as System.Windows.Style,
            });
            contextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
            {
                Text = "Common",
                Style = textSeparatorStyle,
            });
            menuItem = new MenuItem()
            {
                Name = "UI_Menu_Show_in_Explorer",
                Header = "Show in Explorer",
                Style = menuItemStyle,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + ResourceName.Address.Replace("/", "\\"));
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_toolbar_genericfinder_512px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

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
            if (mIsGenerationSnapshot == true)
                return null;
            var snapShotFile = ResourceName.Address + EditorCommon.Program.SnapshotExt;
            if (forceCreate == false)
            {
                var imgSource = await EditorCommon.ImageInit.GetImage(snapShotFile);
                if (imgSource != null)
                    return imgSource;
            }

            if (mIsGenerationSnapshot == true)
                return null;

            mIsGenerationSnapshot = true;
            var rc = CEngine.Instance.RenderContext;
            var mCurMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, ResourceName/*, EditorCommon.SnapshotProcess.SnapshotCreator.GetShadingEnv()*/);
            if (mCurMesh == null)
            {
                mIsGenerationSnapshot = false;
                return null;
            }

            var snapShorter = new EditorCommon.SnapshotProcess.SnapshotCreator();//EngineNS.Editor.SnapshotCreator();//
            snapShorter.SkyName = EngineNS.RName.GetRName("Mesh/sky.gms");
            snapShorter.FloorName = EngineNS.RName.GetRName(@"editor/floor.gms");
            var eye = new EngineNS.Vector3();
            eye.SetValue(1.6f, 1.5f, -3.6f);
            var at = new EngineNS.Vector3();
            at.SetValue(0.0f, 0.0f, 0.0f);
            var up = new EngineNS.Vector3();
            up.SetValue(0.0f, 1.0f, 0.0f);
            await snapShorter.InitEnviroment();
            snapShorter.Camera.LookAtLH(eye, at, up);

            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(mCurMesh);

            mCurMesh.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
            snapShorter.World.AddActor(actor);
            snapShorter.World.GetScene(RName.GetRName("SnapshorCreator")).AddActor(actor);
            snapShorter.FocusActor = actor;
            actor.Placement.Location = new Vector3(0, 0, 0);

            await snapShorter.SaveToFile(snapShotFile, 1, mCurMesh.Editor_GetSnapshortFrameNumber());
            mIsGenerationSnapshot = false;

            return await EditorCommon.ImageInit.GetImage(snapShotFile);
        }

        #region Viewport

        EditorCommon.ViewPort.PreviewActorContainer mPreviewActor = null;
        public async System.Threading.Tasks.Task OnDragEnterViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if(mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            else
            {
                mPreviewActor = new EditorCommon.ViewPort.PreviewActorContainer();
                mPreviewActor.mPreviewActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(this.ResourceName);
                var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
                var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
                mPreviewActor.mPreviewActor.Placement.Location = pos;
                mPreviewActor.ReleaseWaitContext();
            }
            mPreviewActor.mPreviewActor.Tag = new Controls.Outliner.InvisibleInOutliner();
            viewport.AddActor(mPreviewActor.mPreviewActor);
        }
        public async System.Threading.Tasks.Task OnDragLeaveViewport(ViewPort.ViewPortControl viewport, EventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            
            viewport.World.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
            viewport.World.DefaultScene.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
        }
        public async System.Threading.Tasks.Task OnDragOverViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }

            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            mPreviewActor.mPreviewActor.Placement.Location = pos;
        }
        public async System.Threading.Tasks.Task OnDragDropViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            var dropActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(this.ResourceName);
            dropActor.SpecialName = EngineNS.GamePlay.SceneGraph.GSceneGraph.GeneratorActorSpecialNameInEditor(this.ResourceName.PureName(), viewport.World);
            EngineNS.CEngine.Instance.HitProxyManager.MapActor(dropActor);
            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            dropActor.Placement.Location = pos;
            var selActors = new List<ViewPort.ViewPortControl.SelectActorData>(viewport.GetSelectedActors());

            var redoAction = new Action<object>((obj) =>
            {
                viewport.World.AddActor(dropActor);
                viewport.World.DefaultScene.AddActor(dropActor);
                viewport.SelectActor(dropActor);
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand("WorldEditOperation", null, redoAction, null, (obj) =>
            {
                viewport.World.RemoveActor(dropActor.ActorId);
                viewport.World.DefaultScene.RemoveActor(dropActor.ActorId);
                dropActor.Selected = false;
                viewport.SelectActors(selActors.ToArray());
            }, "添加对象");

            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            viewport.World.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
            viewport.World.DefaultScene.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ICreateActor
        public async Task<GActor> CreateActor()
        {
            var actor =  await GActor.NewMeshActorAsync(ResourceName);
            actor.SpecialName = ResourceName.PureName();
            return actor;
        }

        #endregion ICreateActor
        #region ICreateComponent
        public async Task<GComponent> CreateComponent(IComponentContainer componentContainer)
        {
            GActor hostActor = null;
            IComponentContainer hostContainer = null;
            if (componentContainer is GActor)
            {
                hostActor = componentContainer as GActor;
                hostContainer = componentContainer;
            }
            else if (componentContainer is GComponent)
            {
                hostActor = (componentContainer as GComponent).Host;
                hostContainer = componentContainer;
            }
            var rc = CEngine.Instance.RenderContext;
            var comp = new GMeshComponent();
            var init = new GMeshComponent.GMeshComponentInitializer();
            init.MeshName = ResourceName;
            await comp.SetInitializer(rc, hostActor, hostContainer, init);
            comp.SpecialName = ResourceName.PureName();
            return comp;
        }

        #endregion ICreateComponent

        public override async Task Save(bool withSnapshot = false)
        {
            var mesh = await EngineNS.CEngine.Instance.MeshManager.CreateMeshAsync(EngineNS.CEngine.Instance.RenderContext, ResourceName);
            RefreshReferenceRNames(mesh);
            // 刷新资源引用表
            await EngineNS.CEngine.Instance.GameEditorInstance.RefreshResourceInfoReferenceDictionary(this);

            await base.Save(withSnapshot);
        }
        public void RefreshReferenceRNames(EngineNS.Graphics.Mesh.CGfxMesh mesh)
        {
            // 刷新引用
            ReferenceRNameList.Clear();
            ReferenceRNameList.Add(mesh.MeshPrimitiveName);
            foreach (var matName in mesh.MaterialNames)
            {
                ReferenceRNameList.Add(matName);
            }
        }
        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            var mesh = new EngineNS.Graphics.Mesh.CGfxMesh();
            await mesh.LoadMeshAsync(EngineNS.CEngine.Instance.RenderContext, data.RNameMapper.Name);
            data.RNameMapper.ResObject = mesh;
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            if (data.ResObject == null)
                return false;

            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var mesh = data.ResObject as EngineNS.Graphics.Mesh.CGfxMesh;
            var file = data.GetTargetAbsFileName();
            mesh.SaveMesh(file);
            RefreshReferenceRNames(mesh);

            return true;
        }

    }
}
