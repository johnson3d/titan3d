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
using EditorCommon.CodeGenerateSystem;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = "AnimationClip", ResourceExts = new string[] { ".anim" })]
    public class AnimationClipResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu,
                                       EditorCommon.Resources.IResourceInfoPreviewForEditor,
                                       EditorCommon.CodeGenerateSystem.INodeListAttribute,
                                       EditorCommon.Resources.IResourceInfoDragToViewport,
                                       EditorCommon.Resources.IResourceInfoCreateActor,
                                       EditorCommon.Resources.IResourceInfoCreateComponent
    {

        public INodeConstructionParams CSParam { get; set; }
        public Type NodeType { get; set; }
        public string BindingFile { get => ""; set { } }
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return "AnimationClip"; }
        }
        public string SkeletonAsset
        {
            get => GetElementProperty(ElementPropertyType.EPT_Skeleton);
            set { }
        }
        [EngineNS.Rtti.MetaData]
        public List<ElementProperty> ElementProperties { get; set; } = new List<ElementProperty>();
        public string GetElementProperty(ElementPropertyType elementPropertyType)
        {
            for (int i = 0; i < ElementProperties.Count; ++i)
            {
                if (ElementProperties[i].ElementPropertyType == elementPropertyType)
                    return ElementProperties[i].Value;
            }
            return "";
        }
        [EngineNS.Rtti.MetaData]
        public string PreViewMesh
        {
            get;
            set;
        }
        [EngineNS.Rtti.MetaData]
        public int TrackCount
        {
            get;
            set;
        }
        public List<Controls.Animation.NotifyTrack> mNotifyTrackMap = new List<Controls.Animation.NotifyTrack>();
        [EngineNS.Rtti.MetaData]
        public List<Controls.Animation.NotifyTrack> NotifyTrackMap //notify 所在的轨道
        {
            get => mNotifyTrackMap;
            set => mNotifyTrackMap = value;
        }

        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if (mResourceIcon == null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/AnimSequence_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        }

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(72, 122, 154));

        public string EditorTypeName => "AnimationClipEditor";



        public void ForceReload()
        {
            //CCore.Graphics.Texture.ForceReloadTexture(RelativeResourceFileName);
        }
        public string[] GetFileSystemWatcherAttentionExtensions()
        {
            return new string[0];
        }
        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            {
                if (System.IO.File.Exists(ResourceName.Address))
                    System.IO.File.Delete(ResourceName.Address);
                if (System.IO.File.Exists(ResourceName.Address + CEngineDesc.AnimationClipNotifyExtension))
                    System.IO.File.Delete(ResourceName.Address + CEngineDesc.AnimationClipNotifyExtension);
            }
            EngineNS.CEngine.Instance.SkeletonActionManager.RemoveSkeletonAction(ResourceName);
            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return false;
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

            mIsGenerationSnapshot = true;
            var rc = CEngine.Instance.RenderContext;
            if(PreViewMesh == "" || PreViewMesh == "null")
                await EditorCommon.Utility.PreviewHelper.GetPreviewMeshBySkeleton(this);
            var meshName = RName.GetRName(PreViewMesh);
            EngineNS.Graphics.Mesh.CGfxMesh mCurMesh = null;
            var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(1);
            await CEngine.Instance.EventPoster.Post(async () =>
            {
                mCurMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, meshName/*, EditorCommon.SnapshotProcess.SnapshotCreator.GetShadingEnv()*/);
                smp.Release();
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            await smp.Await();
            //var mCurMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, meshName/*, EditorCommon.SnapshotProcess.SnapshotCreator.GetShadingEnv()*/);
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

            var clip = await AnimationClip.Create(ResourceName);
            var skinModifier = mCurMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            var animationCom = new EngineNS.GamePlay.Component.GAnimationComponent(RName.GetRName(this.SkeletonAsset));
            animationCom.Animation = clip;
            actor.AddComponent(animationCom);
            clip.Bind(animationCom.Pose);

            snapShorter.World.AddActor(actor);
            snapShorter.World.GetScene(RName.GetRName("SnapshorCreator")).AddActor(actor);
            snapShorter.FocusActor = actor;
            actor.Placement.Location = new Vector3(0, 0, 0);

            await snapShorter.SaveToFile(snapShotFile, 1000, 8);
            mIsGenerationSnapshot = false;

            return await EditorCommon.ImageInit.GetImage(snapShotFile);
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        protected override void UpdateToolTipOverride()
        {
        }

        public List<EditorCommon.Resources.ResourceFolderContextMenuItem> GetMenuItems(EditorCommon.Resources.IFolderItem folderItem)
        {
            var retItems = new List<EditorCommon.Resources.ResourceFolderContextMenuItem>();

            return retItems;
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new AnimationClipResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip;

            return retValue;
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
                Name = "Edit",
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
                Name = "Show_in_Explorer",
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

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
        #region DragDropToViewport

        EditorCommon.ViewPort.PreviewActorContainer mPreviewActor = null;
        public async System.Threading.Tasks.Task OnDragEnterViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            else
            {
                mPreviewActor = new EditorCommon.ViewPort.PreviewActorContainer();
                mPreviewActor.mPreviewActor = await CreateActor();
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
            var dropActor = await CreateActor();
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
        #region ICreateActor
        public async Task<EngineNS.GamePlay.Actor.GActor> CreateActor()
        {
            await EditorCommon.Utility.PreviewHelper.GetPreviewMeshBySkeleton(this);
            var actor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName(PreViewMesh));
            var animComp = await CreateComponent(actor);
            actor.SpecialName = ResourceName.PureName();
            actor.AddComponent(animComp);
            return actor;
        }
        #endregion DragDropToViewport
        #endregion ICreateActor
        #region ICreateComponent
        public async Task<EngineNS.GamePlay.Component.GComponent> CreateComponent(EngineNS.GamePlay.Component.IComponentContainer componentContainer)
        {
            EngineNS.GamePlay.Actor.GActor hostActor = null;
            EngineNS.GamePlay.Component.IComponentContainer hostContainer = null;
            if (componentContainer is EngineNS.GamePlay.Actor.GActor)
            {
                hostActor = componentContainer as EngineNS.GamePlay.Actor.GActor;
                hostContainer = componentContainer;
            }
            else if (componentContainer is EngineNS.GamePlay.Component.GComponent)
            {
                hostActor = (componentContainer as EngineNS.GamePlay.Component.GComponent).Host;
                hostContainer = componentContainer;
            }
            var rc = CEngine.Instance.RenderContext;
            var comp = new EngineNS.GamePlay.Component.GAnimationComponent();
            var init = new EngineNS.GamePlay.Component.GAnimationComponent.GAnimationComponentInitializer();
            await comp.SetInitializer(rc, hostActor, hostContainer, init);
            comp.SpecialName = ResourceName.PureName();
            comp.AnimationName = ResourceName;
            return comp;
        }

        #endregion ICreateComponent

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            data.RNameMapper.ResObject = await AnimationClip.Create(data.RNameMapper.Name);
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var ac = data.ResObject as AnimationClip;
            var tagFile = data.GetTargetAbsFileName();
            ac.Save(tagFile);

            return true;
        }

    }
}
