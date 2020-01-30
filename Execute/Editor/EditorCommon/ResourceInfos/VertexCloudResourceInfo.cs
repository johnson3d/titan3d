using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.VertexCloud, ResourceExts = new string[] { ".vtc" })]
    public class VertexCloudResourceInfo : EditorCommon.Resources.ResourceInfo, EditorCommon.Resources.IResourceInfoDragDrop, EditorCommon.Resources.IResourceInfoEditor, EditorCommon.Resources.IResourceInfoForceReload, EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        public class VertexCloudResourceInfoData : IResourceCreateData, INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion
            public bool ChangedName = false;
            private string mResourceName;
            [Browsable(false)]
            public EngineNS.RName.enRNameType RNameType { get; set; }
            [Browsable(false)]
            public string ResourceName
            {
                get { return mResourceName; }
                set
                {
                    mResourceName = value;
                    ChangedName = true;
                    OnPropertyChanged("ResourceName");
                }
            }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            private RName mGeomMesh = RName.EmptyName;
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MeshSource)]
            public RName GeomMesh
            {
                get { return mGeomMesh; }
                set
                {
                    mGeomMesh = value;
                    if(ChangedName==false)
                        ResourceName = value.PureName();
                }
            }

            public enum DensityType
            {
                [Description("1")]
                Density1 = 1,//1
                [Description("1.5")]
                Density2 = 2,//1.5
                [Description("2")]
                Density3 = 3,//2
                [Description("3")]
                Density4 = 4,//3
            }
            public DensityType Density
            {
                get;
                set;
            } = DensityType.Density1;

            public string Description { get; set; }
        }
        public override string ResourceTypeName => "VertexCloud";

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(86, 58, 106));
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if(mResourceIcon==null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/AtmosphericFog_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        }

        public string CreateMenuPath => "Mesh/VertexCloud";

        public bool IsBaseResource => false;

        public string EditorTypeName => throw new NotImplementedException();

        public async System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string Absfolder, string rootFolder, EditorCommon.Resources.IResourceCreateData createData)
        {
            var result = new VertexCloudResourceInfo();

            var mcd = createData as VertexCloudResourceInfoData;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(Absfolder + "/" + mcd.ResourceName, rootFolder) + CEngineDesc.VertexCloudExtension;
            result.ResourceName = RName.GetRName(reName, mcd.RNameType);

            var mesh = EngineNS.CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(EngineNS.CEngine.Instance.RenderContext, mcd.GeomMesh);
            if (mesh == null)
                return null;

            var density = 1.0f;
            if (mcd.Density == VertexCloudResourceInfoData.DensityType.Density2)
            {
                density = 1.5f;
            }
            else if (mcd.Density == VertexCloudResourceInfoData.DensityType.Density3)
            {
                density = 2f;
            }
            else if (mcd.Density == VertexCloudResourceInfoData.DensityType.Density4)
            {
                density = 3f;
            }

            var vcobj = await EngineNS.Graphics.Mesh.CGfxVertexCloud.CookFromMesh(EngineNS.CEngine.Instance.RenderContext, mesh, density);
            vcobj.SaveVertexCloud(result.ResourceName);

            return result;
        }

        public bool DragEnter(DragEventArgs e)
        {
            //throw new NotImplementedException();
            return true;
        }

        public void DragLeave(DragEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void DragOver(DragEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void Drop(DragEventArgs e)
        {
            //throw new NotImplementedException();
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
            var result = new VertexCloudResourceInfoData();
            result.ResourceName = EditorCommon.Program.GetValidName(absFolder, "GfxVertexCloud", EngineNS.CEngineDesc.VertexCloudExtension);
            result.ChangedName = false;
            return result;
        }

        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "GfxVertexCloud", EngineNS.CEngineDesc.VertexCloudExtension);
        }

        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //throw new NotImplementedException();
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + EngineNS.CEngineDesc.VertexCloudExtension, SearchOption.TopDirectoryOnly);
            if (files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的 vtc 文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new VertexCloudResourceInfo();

            result.ResourceName = resourceName;

            return result;
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
        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            {
                if (System.IO.File.Exists(ResourceName.Address))
                    System.IO.File.Delete(ResourceName.Address);

                EngineNS.CEngine.Instance.VertexCoudManager.UnmanageVertexCloud(ResourceName);
            }
            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
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
            var newAbsFileName = data.GetTargetAbsFileName();
            var srcAbsFileName = data.GetSourceAbsFileName();
            if (!string.Equals(newAbsFileName, srcAbsFileName, StringComparison.OrdinalIgnoreCase))
            {
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcAbsFileName, newAbsFileName, true);
            }
            return true;
        }

    }
}
