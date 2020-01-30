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

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Prefab, ResourceExts = new string[] { ".prefab" })]
    public class PrefabResourceInfo : EditorCommon.Resources.ResourceInfo,
                                    EditorCommon.Resources.IResourceInfoDragDrop,
                                    EditorCommon.Resources.IResourceInfoEditor,
                                    EditorCommon.Resources.IResourceInfoForceReload,
                                    EditorCommon.Resources.IResourceInfoDragToViewport,
                                    EditorCommon.Resources.IResourceInfoCreateActor
    {
        public override string ResourceTypeName
        {
            get { return EngineNS.Editor.Editor_RNameTypeAttribute.Prefab; }
        }
        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(58, 94, 154));
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if(mResourceIcon==null)
                {
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Prefab_64x.png", UriKind.Absolute));
                }
                return mResourceIcon;
            }
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

        public string EditorTypeName => "PrefabEditor";
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

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new PrefabResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Prefab;

            return retValue;
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            {
                CEngine.Instance.PrefabManager.Remove(ResourceName);
                if (System.IO.File.Exists(ResourceName.Address))
                    System.IO.File.Delete(ResourceName.Address);
            }
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
                Header = "Rename",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Icon_Asset_Rename_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "Duplicate",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Edit/icon_Edit_Duplicate_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "Save",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_levels_Save_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
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
        //public override async Task<ImageSource> GetSnapshotImage(bool forceCreate)
        //{
        //    var snapShotFile = ResourceName.Address + EditorCommon.Program.SnapshotExt;
        //    var imgSource = await EditorCommon.ImageInit.GetImage(snapShotFile);
        //    if (imgSource != null)
        //        return imgSource;

        //    var rc = CEngine.Instance.RenderContext;
        //    var mCurMesh = await CEngine.Instance.MeshManager.CreateMeshAsync(rc, ResourceName/*, EditorCommon.SnapshotProcess.SnapshotCreator.GetShadingEnv()*/);
        //    if (mCurMesh == null)
        //        return null;

        //    var snapShorter = new EditorCommon.SnapshotProcess.SnapshotCreator();//EngineNS.Editor.SnapshotCreator();//
        //    await snapShorter.InitEnviroment();

        //    var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActor(mCurMesh);

        //    mCurMesh.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
        //    snapShorter.World.AddActor(actor);
        //    snapShorter.World.Scenes[RName.GetRName("SnapshorCreator")].AddActor(actor);

        //    actor.Placement.Location = new Vector3(0, 0, 0);

        //    var noused = snapShorter.SaveToFile(snapShotFile);

        //    return await EditorCommon.ImageInit.GetImage(snapShotFile);
        //}

        #region Viewport
        public void MapPrefabActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            EngineNS.CEngine.Instance.HitProxyManager.MapActor(actor);
            List<EngineNS.GamePlay.Actor.GActor> Children = actor.GetChildrenUnsafe();
            if (Children.Count > 0)
            {
                foreach (var i in Children)
                {
                    MapPrefabActor(i);
                }
            }
        }

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
                mPreviewActor.mPreviewActor = await EngineNS.GamePlay.Actor.GActor.NewPrefabActorAsync(this.ResourceName);
                var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
                var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
                mPreviewActor.mPreviewActor.Placement.Location = pos;
                mPreviewActor.ReleaseWaitContext();
            }
            mPreviewActor.mPreviewActor.Tag = new Controls.Outliner.InvisibleInOutliner();
            viewport.World.AddEditorActor(mPreviewActor.mPreviewActor);
            //foreach (var actor in mPreviewActor.mPreviewActor.Children)
            //{
            //    actor.SetParent(mPreviewActor.mPreviewActor);
            //    viewport.World.AddActor(actor);
            //    viewport.World.DefaultScene.AddActor(actor);
            //}
        }
        public async System.Threading.Tasks.Task OnDragLeaveViewport(ViewPort.ViewPortControl viewport, EventArgs e)
        {
            if (mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }

            viewport.World.RemoveEditorActor(mPreviewActor.mPreviewActor.ActorId);

            //for (int i = mPreviewActor.mPreviewActor.Children.Count - 1; i >= 0; i--)
            //{
            //    EngineNS.GamePlay.Actor.GActor actor = mPreviewActor.mPreviewActor.Children[i];
            //    actor.Parent.SetParent(null);
            //    viewport.World.RemoveActor(actor.ActorId);
            //    viewport.World.DefaultScene.RemoveActor(actor.ActorId);
            //}
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
            var dropActor = await EngineNS.GamePlay.Actor.GActor.NewPrefabActorAsync(this.ResourceName);
            string sname = this.ResourceName.GetFileName();
            //dropActor.SpecialName = sname;
            MapPrefabActor(dropActor);
            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            dropActor.Placement.Location = pos;
            var selActors = new List<ViewPort.ViewPortControl.SelectActorData>(viewport.GetSelectedActors());

            var redoAction = new Action<object>((obj) =>
            {
                viewport.AddActor(dropActor);
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand("WorldEditOperation", null, redoAction, null, (obj) =>
            {
                viewport.World.RemoveActor(dropActor.ActorId);
                viewport.World.DefaultScene.RemoveActor(dropActor.ActorId);

                //for (int i = dropActor.Children.Count - 1; i >= 0; i--)
                //{
                //    EngineNS.GamePlay.Actor.GActor actor = dropActor.Children[i];
                //    actor.Parent.SetParent(null);
                //    viewport.World.RemoveActor(actor.ActorId);
                //    viewport.World.DefaultScene.RemoveActor(actor.ActorId);
                //}


                dropActor.Selected = false;
                viewport.SelectActors(selActors.ToArray());
            }, "添加对象");

            if (mPreviewActor == null || mPreviewActor.mPreviewActor == null)
            {
                return;
            }

            await mPreviewActor.AwaitLoad();
            viewport.World.RemoveEditorActor(mPreviewActor.mPreviewActor.ActorId);

            //if (mPreviewActor.mPreviewActor.Children.Count > 0)
            //{
            //    for (int i = mPreviewActor.mPreviewActor.Children.Count - 1; i >= 0; i --)
            //    {
            //        EngineNS.GamePlay.Actor.GActor actor = mPreviewActor.mPreviewActor.Children[i];
            //        actor.Parent.SetParent(null);
            //        viewport.World.RemoveActor(actor.ActorId);
            //        viewport.World.DefaultScene.RemoveActor(actor.ActorId);
            //    }
            //}
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
        public async Task<EngineNS.GamePlay.Actor.GActor> CreateActor()
        {
            var actor = await EngineNS.CEngine.Instance.PrefabManager.GetPrefab(CEngine.Instance.RenderContext,ResourceName, true);
            actor.SpecialName = ResourceName.PureName();
            return actor;
        }

        #endregion ICreateActor


        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            data.RNameMapper.ResObject = await EngineNS.CEngine.Instance.PrefabManager.GetPrefab(EngineNS.CEngine.Instance.RenderContext, data.RNameMapper.Name, true);
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var prefab = data.ResObject as EngineNS.GamePlay.Actor.GPrefab;
            var newFile = data.GetTargetAbsFileName();
            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
            prefab.Save2Xnd(xnd.Node);
            EngineNS.IO.XndHolder.SaveXND(newFile, xnd);

            await RefreshReferenceRNames(prefab);
            await base.Save(false);

            return true;
        }
        public async System.Threading.Tasks.Task RefreshReferenceRNames(EngineNS.GamePlay.Actor.GPrefab prefab)
        {
            // 刷新资源引用
            ReferenceRNameList.Clear();

            HashSet<RName> refObjectHashSet = new HashSet<RName>();
            await EngineNS.GamePlay.Actor.GActor.AnalysisActorRefResource(prefab, refObjectHashSet, null);

            foreach(var i in refObjectHashSet)
            {
                if (i.Name.EndsWith(".shadingenv"))
                    continue;
                ReferenceRNameList.Add(i);
            }
        }
    }
}
