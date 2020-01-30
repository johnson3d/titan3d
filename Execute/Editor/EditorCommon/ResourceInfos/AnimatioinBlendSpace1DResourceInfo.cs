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

namespace ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = "AnimatioinBlendSpace1D", ResourceExts = new string[] { ".vanimbs1d" })]
    public class AnimatioinBlendSpace1DResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu,
                                       EditorCommon.Resources.IResourceReference
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return "AnimatioinBlendSpace1D"; }
        }

        public override ImageSource ResourceIcon
        {
            get;
        } = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/AssetIcons/AnimBlueprint_64x.png", UriKind.Absolute));

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 80, 64));

        public string EditorTypeName => "AnimationBlendSpace1DEditor";


        public void ForceReload()
        {
            //CCore.Graphics.Texture.ForceReloadTexture(RelativeResourceFileName);
        }

        protected override bool DeleteResourceOverride()
        {
            {
                if (System.IO.File.Exists(ResourceName.Address))
                    System.IO.File.Delete(ResourceName.Address);
            }
            return true;
        }

        protected override bool MoveToFolderOverride(string absFolder, EngineNS.RName resourceName, Action MoveInfoAction)
        {
            return true;
        }
        public void OpenEditor()
        {
            EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        protected override void UpdateToolTipOverride()
        {
        }

        #region Reference
        protected void DoChangeReferencesRef(string absSrcFile, string absTagFile)
        {
            var refInfos = GetReferences();
            var relSrcFile = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absSrcFile);
            var relTagFile = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absTagFile);
            foreach (var info in refInfos)
            {
                var resRef = info as EditorCommon.Resources.IResourceReference;
                if (resRef == null)
                    continue;

                resRef.ChangeDependency(this, relSrcFile, relTagFile);
            }
        }
        public List<EditorCommon.Resources.ResourceInfo> GetReferences()
        {
            var retList = new List<EditorCommon.Resources.ResourceInfo>();

            return retList;
        }
        public bool IsDependencyWith(EditorCommon.Resources.ResourceInfo info)
        {
            switch (info.ResourceTypeName)
            {
                default:
                    return false;
            }
        }
        public void ChangeDependency(EditorCommon.Resources.ResourceInfo info, object src, object tag)
        {

        }

        #endregion

        public List<EditorCommon.Resources.ResourceFolderContextMenuItem> GetMenuItems(EditorCommon.Resources.IFolderItem folderItem)
        {
            var retItems = new List<EditorCommon.Resources.ResourceFolderContextMenuItem>();

            return retItems;
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new AnimatioinBlendSpace1DResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.AnimatioinBlendSpace1D;

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
            // <MenuItem Header="Add Feature or Content Pack..." menu:MenuAssist.Icon="/ResourceLibrary;component/UEIcon/Icons/icon_file_saveall_40x.png" Style="{DynamicResource {ComponentResourceKey ResourceId=MenuItem_Default, TypeInTargetAssembly={x:Type res:CustomResources}}}"/>
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
            var menuItem = new MenuItem()
            {
                Header = "Edit...",
                Style = menuItemStyle,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                OpenEditor();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/Edit/icon_Edit_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "Rename",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/Icon_Asset_Rename_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "Duplicate",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/Edit/icon_Edit_Duplicate_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "Save",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/icon_levels_Save_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "Delete",
                Style = menuItemStyle,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/icon_delete_16px.png", UriKind.Absolute)));
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
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/icon_toolbar_genericfinder_512px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            return true;
        }
    }
}
