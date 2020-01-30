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
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.PhyHeightFieldGeom, ResourceExts = new string[] { ".PhyHeightFieldGeom" })]
    public class PhyHeightFieldGeomResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return EngineNS.Editor.Editor_RNameTypeAttribute.PhyHeightFieldGeom; }
        }
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("ShapeType")]
        [EngineNS.Rtti.MetaData]
        public EngineNS.Bricks.PhysicsCore.EPhysShapeType ShapeType { get; set; } = EngineNS.Bricks.PhysicsCore.EPhysShapeType.PST_HeightField;
        public override ImageSource ResourceIcon
        {
            get;
        } = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Cube_64x.png", UriKind.Absolute));

        public override Brush ResourceTypeBrush
        {
            get
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(116,60,90));

            }
        }

        public string EditorTypeName => throw new NotImplementedException();

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
            }
            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new InvalidOperationException();
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            //ParentBrowser?.OpenEditor(new object[] { "TextureSourceEditor", this });
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
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
            var retValue = new PhyHeightFieldGeomResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.PhyHeightFieldGeom;
            retValue.ShapeType = EngineNS.Bricks.PhysicsCore.EPhysShapeType.PST_HeightField;
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
            throw new InvalidOperationException();
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new InvalidOperationException();
        }

    }
}
