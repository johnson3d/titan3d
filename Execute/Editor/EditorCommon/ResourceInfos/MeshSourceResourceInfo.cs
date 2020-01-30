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
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource, ResourceExts = new string[] { ".vms" })]
    public class MeshSourceResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource; }
        }
        [EngineNS.Rtti.MetaData]
        public string SkeletonAsset
        {
            get; set;
        }
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
        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(106, 45, 59));

        public string EditorTypeName => "MeshPrimitiveEditor";

        bool mIsGenerationSnapshot = false;
        public override async System.Threading.Tasks.Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
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
            var meshSource = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(rc, ResourceName, true);

            var curMesh = CEngine.Instance.MeshManager.CreateMesh(rc, meshSource/*, EditorCommon.SnapshotProcess.SnapshotCreator.GetShadingEnv()*/);
            if (curMesh == null)
            {
                mIsGenerationSnapshot = false;
                return null;
            }

            var mtl = EngineNS.CEngine.Instance.MaterialInstanceManager.DefaultMaterialInstance;
            for (UInt32 i = 0; i < curMesh.MtlMeshArray.Length; i++)
            {
                await curMesh.SetMaterialInstanceAsync(EngineNS.CEngine.Instance.RenderContext, i, mtl, null);
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

            var actor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(curMesh);
            actor.Placement.Location = new Vector3(0, -0.5f, 0);

            curMesh.PreUse(true);//就这个地方用，别的地方别乱用，效率不好
            snapShorter.World.AddActor(actor);
            snapShorter.World.GetScene(RName.GetRName("SnapshorCreator")).AddActor(actor);
            snapShorter.FocusActor = actor;
            await snapShorter.SaveToFile(snapShotFile, 1, curMesh.Editor_GetSnapshortFrameNumber());
            mIsGenerationSnapshot = false;

            return await EditorCommon.ImageInit.GetImage(snapShotFile);
        }

        public void ForceReload()
        {
            var rc = CEngine.Instance.RenderContext;
            CEngine.Instance.MeshPrimitivesManager.RefreshMeshPrimitives(rc, ResourceName);
            //CCore.Graphics.Texture.ForceReloadTexture(RelativeResourceFileName);
        }
        public string[] GetFileSystemWatcherAttentionExtensions()
        {
            return new string[] { ".vms" };
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            CEngine.Instance.MeshPrimitivesManager.RemoveMeshPimitives(ResourceName);
            if (System.IO.File.Exists(ResourceName.Address))
                System.IO.File.Delete(ResourceName.Address);
            if (System.IO.File.Exists(ResourceName.Address + EditorCommon.Program.SnapshotExt))
                System.IO.File.Delete(ResourceName.Address + EditorCommon.Program.SnapshotExt);

            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new InvalidOperationException();
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
            //ParentBrowser?.OpenEditor(new object[] { "TextureSourceEditor", this });
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
            var retValue = new MeshSourceResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource;

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

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            // MeshSource不引用其他资源，直接拷贝
            var newAbsFileName = data.GetTargetAbsFileName();
            var srcAbsFileName = data.GetSourceAbsFileName();
            if(!string.Equals(newAbsFileName, srcAbsFileName, StringComparison.OrdinalIgnoreCase))
            {
                // vms
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcAbsFileName, newAbsFileName, true);
            }
            return true;
        }

    }
}
