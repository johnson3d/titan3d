using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.Generic;
using EditorCommon.Resources;
using EngineNS;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Text;

namespace TextureSourceEditor
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Texture, ResourceExts = new string[] {".txpic", ".psd", ".png", ".jpg", ".bmp", ".dds", ".tga" })]
    public class TextureResourceInfo : EditorCommon.Resources.ResourceInfo, 
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return "贴图"; }
        }

        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if (mResourceIcon == null)
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Texture2D_64x.png", UriKind.Absolute));
                return mResourceIcon;
            }
        } 

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(38, 38, 38));


        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("路径")]
        public string DisplayPath
        {
            get
            {
                return ResourceName.RNameType + ":  " + ResourceName.Name;
            }
        }

        string mDimensions = "0X0";
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("像素尺寸")]
        [EngineNS.Rtti.MetaData]
        public string Dimensions
        {
            get { return mDimensions; }
            set
            {
                mDimensions = value;
                OnPropertyChanged("Dimensions");
            }
        }

        int mPixelWidth = 0;
        int mPixelHeight = 0;

        PixelFormat mPixelFormat;
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("像素格式")]
        [EngineNS.Rtti.MetaData]
        public PixelFormat PixelFormat
        {
            get { return mPixelFormat; }
            set
            {
                mPixelFormat = value;
                OnPropertyChanged("PixelFormat");
            }
        }

        string mOrigionFile;
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("导入文件")]
        [EngineNS.Rtti.MetaData]
        public string OrigionFile
        {
            get => mOrigionFile;
            set
            {
                mOrigionFile = value;
                OnPropertyChanged("OrigionFile");
            }
        }

        string mLinkedFile;
        [EngineNS.Rtti.MetaData]
        public string LinkedFile
        {
            get => mLinkedFile;
            set
            {
                mLinkedFile = value;
                OnPropertyChanged("LinkedFile");
            }
        }

        [EngineNS.Rtti.MetaData]
        public EditorCommon.TextureDesc mCurrentDesc = new EditorCommon.TextureDesc();

        public string EditorTypeName => "TextureSourceEditor";

        public override async System.Threading.Tasks.Task<ImageSource[]> GetSnapshotImage(bool forceCreate)
        {
            //var fileName = AbsInfoFileName.Replace(EditorCommon.Resources.ResourceInfo.ExtString, "");
            //fileName = EngineNS.CEngine.Instance.FileManager._GetAbsPathFromRelativePath(fileName);
            // 缩略图背景不该透明
            //var image = await EditorCommon.ImageInit.GetImage(fileName, System.Drawing.Color.Empty) as BitmapSource;
            var image = await EditorCommon.ImageInit.GetImage(ResourceName.Address);

            //if (image != null)
            //{
            //    Dimensions = image.PixelWidth + "X" + image.PixelHeight;
            //    PixelFormat = image.Format;
            //    mPixelWidth = image.PixelWidth;
            //    mPixelHeight = image.PixelHeight;
            //    if (image.PixelWidth > 1024 || image.PixelHeight > 1024)
            //    {
            //        if (mResDic == null)
            //        {
            //            mResDic = new ResourceDictionary();
            //            mResDic.Source = new Uri("/ResourcesBrowser;component/Themes/Generic.xaml", UriKind.Relative);
            //        }
            //        BorderBrush = EditorCommon.Program.TryFindResource("HeavyResourceBrush", mResDic) as Brush;
            //    }
            //}

            if(image!=null)
                Snapshot = image[0];
            return image;
        }

        public string[] GetFileSystemWatcherAttentionExtensions()
        {
            return new string[] { ".psd", ".png", ".jpg", ".bmp", ".dds", ".tga" };
        }
        public void ForceReload()
        {
            var noUse = ForceReloadProcess();
        }
        async Task ForceReloadProcess()
        {
            var srcFile = ResourceName.GetDirectory() + LinkedFile;
            EditorCommon.TextureDesc desc = new EditorCommon.TextureDesc();
            await GetTextureDesc(ResourceName.Address, desc);


            SaveTXPicFromFilename(ResourceName.Address, ref desc.PicDesc, srcFile);
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var rv = EngineNS.CEngine.Instance.TextureManager.GetShaderRView(rc, ResourceName);
            rv.RefreshResource();
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (EngineNS.CEngine.Instance.FileManager.FileExists(ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address);
            var orgFile = ResourceName.GetDirectory() + LinkedFile;
            if (EngineNS.CEngine.Instance.FileManager.FileExists(orgFile))
                EngineNS.CEngine.Instance.FileManager.DeleteFile(orgFile);
            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.FileManager.MoveFile(ResourceName.Address, absFolder + currentResourceName.PureName(true));
            
            return true;
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        public int GetWidth()
        {
            return System.Convert.ToInt32(mDimensions.Split('X')[0]);
        }

        public int GetHeight()
        {
            return System.Convert.ToInt32(mDimensions.Split('X')[1]);
        }
        
        //static ResourceDictionary mResDic = null;

        protected override void UpdateToolTipOverride()
        {
            // 重资源警告
            if(System.Math.Min(mPixelHeight, mPixelWidth) > 1024)
            {
                var tb = new TextBlock()
                {
                    // 这里要写成配置，资源大小警告配置
                    Text = "警告：图片资源大小超过1024，会对手机上运行时性能产生影响!",
                    Foreground = Brushes.Yellow,
                };
                mResToolTipPanel.Children.Add(tb);
            }

            // 版本控制
            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    switch (mVersionControlState)
            //    {
            //        case EditorCommon.VersionControl.EStatus.Conflict:
            //            {
            //                EditorCommon.VersionControl.VersionControlManager.Instance.Info((result) =>
            //                {
            //                    var la = "Last Changed Author:";
            //                    var idx = result.OutputString.IndexOf(la);
            //                    if (idx < 0)
            //                        return;
            //                    var idxLast = result.OutputString.IndexOf("\n", idx);
            //                    var lastAuthorStr = result.OutputString.Substring(idx + la.Length, idxLast - idx - la.Length);
            //                    la = "Last Changed Rev:";
            //                    idx = result.OutputString.IndexOf(la);
            //                    if (idx < 0)
            //                        return;
            //                    idxLast = result.OutputString.IndexOf("\n", idx);
            //                    var lastRevStr = result.OutputString.Substring(idx + la.Length, idxLast - idx - la.Length);
            //                    la = "Last Changed Date:";
            //                    idx = result.OutputString.IndexOf(la);
            //                    if (idx < 0)
            //                        return;
            //                    idxLast = result.OutputString.IndexOf("\n", idx);
            //                    var lastDateStr = result.OutputString.Substring(idx + la.Length, idxLast - idx - la.Length);

            //                    EditorCommon.Program.MainDispatcher.Invoke(() =>
            //                    {
            //                        if (mResDic == null)
            //                        {
            //                            mResDic = new ResourceDictionary();
            //                            mResDic.Source = new Uri("/ResourcesBrowser;component/Themes/Generic.xaml", UriKind.Relative);
            //                        }

            //                        var tb = new TextBlock()
            //                        {
            //                            Text = "版本控制冲突:\r\n" + "最后上传:" + lastAuthorStr + "最后版本:" + lastRevStr + "最后上传日期:" + lastDateStr,
            //                            Foreground = Brushes.Red,
            //                            Style = EditorCommon.Program.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default"), mResDic) as Style
            //                        };
            //                        mResToolTipPanel.Children.Add(tb);
            //                    });
            //                }, AbsResourceFileName);
            //            }
            //            break;
            //    }
            //}
        }

        public List<EditorCommon.Resources.ResourceFolderContextMenuItem> GetMenuItems(EditorCommon.Resources.IFolderItem folderItem)
        {
            var retItems = new List<EditorCommon.Resources.ResourceFolderContextMenuItem>();

            //foreach(var prop in typeof(CCore.Graphics.TextureImageInfo).GetProperties())
            //{
            //    var atts = prop.GetCustomAttributes(typeof(CSUtility.Editor.Editor_MultiSetInResourceFolderContextMenuAttribute), false);
            //    if (atts.Length <= 0)
            //        continue;

            //    string disName = prop.Name;
            //    atts = prop.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false);
            //    if (atts.Length > 0)
            //        disName = ((System.ComponentModel.DisplayNameAttribute)(atts[0])).DisplayName;

            //    if(prop.PropertyType == typeof(bool))
            //    {
            //        var menuItem = new EditorCommon.Resources.ResourceFolderContextMenuItem();
            //        menuItem.Header = "批量设置" + disName;
            //        menuItem.ClickAction = () =>
            //        {
            //            if (!System.IO.Directory.Exists(folderItem.AbsolutePath))
            //                return;

            //            var resInfoAtts = typeof(TextureResourceInfo).GetCustomAttributes(typeof(EditorCommon.Resources.ResourceInfoAttribute), false);
            //            if (resInfoAtts.Length == 0)
            //                return;

            //            SearchOption sop = SearchOption.TopDirectoryOnly;
            //            if (EditorCommon.MessageBox.Show("是否设置子文件夹？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
            //                sop = SearchOption.AllDirectories;

            //            var att = resInfoAtts[0] as EditorCommon.Resources.ResourceInfoAttribute;
            //            foreach(var ext in att.ResourceExts)
            //            {
            //                foreach(var file in System.IO.Directory.GetFiles(folderItem.AbsolutePath, "*" + ext, sop))
            //                {
            //                    // 跳过缩略图
            //                    if (file.EndsWith(EditorCommon.Program.SnapshotExt))
            //                        continue;

            //                    var imgInfo = new CCore.Graphics.TextureImageInfo();

            //                    var imgInfoFile = file + CCore.Graphics.TextureImageInfo.Suffix;
            //                    if(System.IO.File.Exists(imgInfoFile))
            //                    {
            //                        imgInfo.Load(file);
            //                        //imgInfo.Load(imgInfoFile);
            //                    }

            //                    prop.SetValue(imgInfo, true);
            //                    imgInfo.Save(file);
            //                    //imgInfo.Save(imgInfoFile);
            //                }
            //            }
            //        };
            //        retItems.Add(menuItem);

            //        menuItem = new EditorCommon.Resources.ResourceFolderContextMenuItem();
            //        menuItem.Header = "批量取消" + disName;
            //        menuItem.ClickAction = () =>
            //        {
            //            if (!System.IO.Directory.Exists(folderItem.AbsolutePath))
            //                return;

            //            var resInfoAtts = typeof(TextureResourceInfo).GetCustomAttributes(typeof(EditorCommon.Resources.ResourceInfoAttribute), false);
            //            if (resInfoAtts.Length == 0)
            //                return;

            //            SearchOption sop = SearchOption.TopDirectoryOnly;
            //            if (EditorCommon.MessageBox.Show("是否设置子文件夹？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
            //                sop = SearchOption.AllDirectories;

            //            var att = resInfoAtts[0] as EditorCommon.Resources.ResourceInfoAttribute;
            //            foreach (var ext in att.ResourceExts)
            //            {
            //                foreach (var file in System.IO.Directory.GetFiles(folderItem.AbsolutePath, "*" + ext, sop))
            //                {
            //                    // 跳过缩略图
            //                    if (file.EndsWith(EditorCommon.Program.SnapshotExt))
            //                        continue;

            //                    var imgInfo = new CCore.Graphics.TextureImageInfo();

            //                    var imgInfoFile = file + CCore.Graphics.TextureImageInfo.Suffix;
            //                    if (System.IO.File.Exists(imgInfoFile))
            //                    {
            //                        imgInfo.Load(file);
            //                    }

            //                    prop.SetValue(imgInfo, false);
            //                    imgInfo.Save(file);
            //                }
            //            }
            //        };
            //        retItems.Add(menuItem);
            //    }
            //}

            return retItems;
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new TextureResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Texture;

            //EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Info, "ResourceImport", "添加贴图文件" + resourceName.Address);

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
                Header = "编辑...",
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
                Header = "重命名",
                Style = menuItemStyle,
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await Rename();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Icon_Asset_Rename_16x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "复制",
                Style = menuItemStyle,
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Edit/icon_Edit_Duplicate_40x.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            //menuItem = new MenuItem()
            //{
            //    Header = "Save",
            //    Style = menuItemStyle,
            //};
            //ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_levels_Save_40x.png", UriKind.Absolute)));
            //contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Header = "删除",
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
        public override bool IsExists(string file)
        {
            if (true == System.IO.File.Exists(file))
                return true;

            var pos = file.LastIndexOf('.');
            file = file.Substring(0, pos);
            file += EngineNS.CEngineDesc.TextureExtension;

            return System.IO.File.Exists(file);
        }

        public static async Task<bool> GetTextureDesc(string absFile, EditorCommon.TextureDesc desc)
        {
            var xnd = await EngineNS.IO.XndHolder.LoadXND(absFile);
            if (xnd == null)
                return false;

            var attr = xnd.Node.FindAttrib("Desc");
            if (attr == null)
                return false;

            attr.BeginRead();
            switch(attr.Version)
            {
                case 1:
                    {
                        string ori;
                        attr.Read(out ori);
                        unsafe
                        {
                            fixed (EngineNS.CTxPicDesc* descPin = &(desc.PicDesc))
                            {
                                attr.Read((IntPtr)descPin, sizeof(EngineNS.CTxPicDesc));
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        attr.Read(out desc.PicDesc.sRGB);
                    }
                    break;
                case 3:
                    {
                        unsafe
                        {
                            fixed (EngineNS.CTxPicDesc* descPin = &(desc.PicDesc))
                            {
                                attr.Read((IntPtr)descPin, sizeof(EngineNS.CTxPicDesc));
                            }
                        }
                    }
                    break;
            }
            attr.EndRead();

            var rawAttr = xnd.Node.FindAttrib("PNG");
            if (rawAttr != null)
            { 
                rawAttr.BeginRead();
                rawAttr.Read(out desc.RawData, (int)rawAttr.Length);
                rawAttr.EndRead();
            }

            xnd.Node.TryReleaseHolder();

            return true;
        }

        public void SaveTXPicFromFilename(string absFile, ref EngineNS.CTxPicDesc txDesc, string filename, EditorCommon.TextureDesc desc = null)
        {
            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();

            if (desc == null)
            {
                desc = new EditorCommon.TextureDesc();
            }
            EngineNS.BitmapProc.SaveTxPic(xnd, ref txDesc, filename, desc.ETCFormat, desc.MipMapLevel);

            EngineNS.IO.XndHolder.SaveXND(absFile, xnd);
            xnd.Node.TryReleaseHolder();
        }

        public static void SaveTXPicToFile(string absFile, ref EngineNS.CTxPicDesc txDesc, byte[] rawData, EditorCommon.TextureDesc desc = null)
        {
            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();

            var stream = new System.IO.MemoryStream(rawData);
            EngineNS.BitmapProc.SaveTxPic(xnd, ref txDesc, new System.Drawing.Bitmap(stream), desc.ETCFormat, desc.MipMapLevel);

            EngineNS.IO.XndHolder.SaveXND(absFile, xnd);
            xnd.Node.TryReleaseHolder();
        }
 
        public override async Task DoImport(string file, string target, bool overwrite = false)
        {
            var copyedFile = this.ResourceName.GetDirectory() + EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(file);
            LinkedFile = EngineNS.CEngine.Instance.FileManager.GetPureFileFromFullName(copyedFile);
            OrigionFile = file;

            var pos = target.LastIndexOf('.');
            target = target.Substring(0, pos);
            target += EngineNS.CEngineDesc.TextureExtension;

            //这里最好导出的时候有参数，确定是否sRGB
            var txDesc = new EngineNS.CTxPicDesc();
            txDesc.SetDefault();


            SaveTXPicFromFilename(target, ref txDesc, file);

            pos = this.ResourceName.Name.LastIndexOf('.');
            var newRName = this.ResourceName.Name.Substring(0, pos);
            newRName += EngineNS.CEngineDesc.TextureExtension;
            this.ResourceName = RName.GetRName(newRName, this.ResourceName.RNameType);

            // 复制原始文件
            EngineNS.CEngine.Instance.FileManager.CopyFile(file, copyedFile, true);

            await UpdateFileInfo();
        }
        async Task UpdateFileInfo()
        {
            var imgs = await EditorCommon.ImageInit.GetImage(ResourceName.Address);
            var image = imgs[0] as BitmapSource;
            if (image != null)
            {
                Dimensions = image.PixelWidth + "X" + image.PixelHeight;
                PixelFormat = image.Format;
            }
        }

        protected override async Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }

        protected override async Task<bool> RenameOverride(string absFolder, string newName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.FileManager.MoveFile(ResourceName.Address, absFolder + newName + EngineNS.CEngineDesc.TextureExtension);
            return true;
        }

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            // Texture不引用其他资源，所以这里直接拷贝
            var newAbsFileName = data.GetTargetAbsFileName();
            var srcAbsFileName = data.GetSourceAbsFileName();
            if (!string.Equals(newAbsFileName, srcAbsFileName, StringComparison.OrdinalIgnoreCase))
            {
                // txpic
                //EngineNS.CEngine.Instance.FileManager.CopyFile(data.RName.Address, newAbsFileName, true);
                // png、jpg...
                var meta = ResourceInfoManager.Instance.GetResourceInfoMetaData(EngineNS.Editor.Editor_RNameTypeAttribute.Texture);
                var newAbsFileNameWithoutExt = EngineNS.CEngine.Instance.FileManager.RemoveExtension(newAbsFileName);
                var oldAbsFileNameWithoutExt = EngineNS.CEngine.Instance.FileManager.RemoveExtension(srcAbsFileName);
                foreach (var ext in meta.ResourceExts)
                {
                    EngineNS.CEngine.Instance.FileManager.CopyFile(oldAbsFileNameWithoutExt + ext, newAbsFileNameWithoutExt + ext, true);
                }
            }
            return true;
        }

    }
}
