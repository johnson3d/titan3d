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
using EditorCommon.Controls.ResourceBrowser;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = "AnimationBlendSpace", ResourceExts = new string[] { ".vanimbs" })]
    public class AnimationBlendSpaceResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu,
                                       EditorCommon.Resources.IResourceInfoCreateEmpty,
                                       EditorCommon.Resources.IResourceInfoPreviewForEditor,
                                       EditorCommon.CodeGenerateSystem.INodeListAttribute
    {
        public class AnimationBlendSpaceCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public EngineNS.RName.enRNameType RNameType { get; set; }
            [Browsable(false)]
            public string ResourceName { get; set; }
            ICustomCreateDialog mHostDialog;
            [Browsable(false)]
            public ICustomCreateDialog HostDialog
            {
                get => mHostDialog;
                set
                {
                    mHostDialog = value;
                    var createDialog = mHostDialog as CreateResDialog;
                    if (createDialog == null)
                        return;
                    if (mSkeletonAsset == RName.EmptyName)
                    {
                        createDialog.OKButtonEnable = false;
                    }
                    else
                    {
                        createDialog.OKButtonEnable = true;
                    }
                }
            }
            RName mSkeletonAsset = RName.EmptyName;
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Skeleton)]
            public RName SkeletonAsset
            {
                get => mSkeletonAsset;
                set
                {
                    mSkeletonAsset = value;
                    var createDialog = HostDialog as CreateResDialog;
                    if (createDialog == null)
                        return;
                    if (value == RName.EmptyName)
                    {
                        createDialog.OKButtonEnable = false;
                    }
                    else
                    {
                        createDialog.OKButtonEnable = true;
                    }
                }
            }
            [Browsable(false)]
            public string Description { get; set; }
        }
        public INodeConstructionParams CSParam { get; set; }
        public Type NodeType { get; set; }
        public string BindingFile { get => ""; set { } }
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return "AnimatioinBlendSpace"; }
        }
        [EngineNS.Rtti.MetaData]
        public string SkeletonAsset
        {
            get; set;
        }
        [EngineNS.Rtti.MetaData]
        public string PreViewMesh
        {
            get; set;
        }
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if (mResourceIcon == null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/BlendSpace_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        }

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(72, 122, 154));

        public string EditorTypeName => "AnimationBlendSpaceEditor";



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
            var retValue = new AnimationBlendSpaceResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.AnimationBlendSpace;

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

        #region ResourceCreateData
        public string CreateMenuPath => "Animation/AnimationBlendSpace";

        public bool IsBaseResource => false;
        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "AnimationBlendSpace", CEngineDesc.AnimationBlendSpaceExtension);
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + CEngineDesc.AnimationBlendSpaceExtension, SearchOption.TopDirectoryOnly);
            if (files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的模型文件!");
            }

            return new ValidationResult(true, null);
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            var result = new AnimationBlendSpaceCreateData();
            result.ResourceName = EditorCommon.Program.GetValidName(absFolder, "AnimationBlendSpace", CEngineDesc.AnimationBlendSpaceExtension);
            result.SkeletonAsset = RName.EmptyName;
            return result;
        }

        public async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new AnimationBlendSpaceResourceInfo();

            var mcd = createData as AnimationBlendSpaceCreateData;
            if (EngineNS.CEngine.Instance.FileManager.GetFileExtension(mcd.ResourceName) != EngineNS.CEngineDesc.AnimationBlendSpaceExtension)
            {
                mcd.ResourceName = mcd.ResourceName + EngineNS.CEngineDesc.AnimationBlendSpaceExtension;
            }
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + mcd.ResourceName, rootFolder);
            result.ResourceName = RName.GetRName(reName, mcd.RNameType);
            result.SkeletonAsset = mcd.SkeletonAsset.Name;
            BlendSpace2D bs = new BlendSpace2D();
            bs.Name = result.ResourceName;
            bs.SkeletonAsset = mcd.SkeletonAsset;
            bs.Save();
            return result;
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

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            data.RNameMapper.ResObject = await BlendSpace2D.Create(data.RNameMapper.Name);
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var b2d = data.ResObject as BlendSpace2D;
            var tagFile = data.GetTargetAbsFileName();
            b2d.Save(tagFile);

            return true;
        }

    }
}
