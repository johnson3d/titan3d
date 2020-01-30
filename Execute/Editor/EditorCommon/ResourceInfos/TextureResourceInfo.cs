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

namespace EditorCommon.Resources
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Texture, ResourceExts = new string[] {".txpic", ".png", ".jpg", ".bmp", ".dds" })]
    public class TextureResourceInfo : EditorCommon.Resources.ResourceInfo, 
                                       EditorCommon.Resources.IResourceInfoForceReload,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceFolderContextMenu,
                                       EditorCommon.Resources.IResourceReference
    {
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName
        {
            get { return "贴图"; }
        }

        public override ImageSource ResourceIcon
        {
            get;
        } = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/AssetIcons/Texture2D_64x.png", UriKind.Absolute));

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(192, 64, 64));

        string mDimensions = "0X0";
        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("像素尺寸")]
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
        public PixelFormat PixelFormat
        {
            get { return mPixelFormat; }
            set
            {
                mPixelFormat = value;
                OnPropertyChanged("PixelFormat");
            }
        }

        public string EditorTypeName => throw new NotImplementedException();

        public override async System.Threading.Tasks.Task<ImageSource> GetSnapshotImage(bool forceCreate)
        {
            var fileName = AbsInfoFileName.Replace(EditorCommon.Resources.ResourceInfo.ExtString, "");
            fileName = EngineNS.CEngine.Instance.FileManager._GetAbsPathFromRelativePath(fileName);
            // 缩略图背景不该透明
            //var image = await EditorCommon.ImageInit.GetImage(fileName, System.Drawing.Color.Empty) as BitmapSource;
            var image = await EditorCommon.ImageInit.GetImage(fileName);

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

            Snapshot = image;
            return Snapshot;
        }
        
        public void ForceReload()
        {
            //CCore.Graphics.Texture.ForceReloadTexture(RelativeResourceFileName);
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            if (System.IO.File.Exists(AbsResourceFileName))
            //                System.IO.File.Delete(AbsResourceFileName);
            //            string textureImageInfoFileName = AbsResourceFileName + CCore.Graphics.TextureImageInfo.Suffix;
            //            if (System.IO.File.Exists(textureImageInfoFileName))
            //                System.IO.File.Delete(textureImageInfoFileName);

            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{ResourceTypeName}{Name} {AbsResourceFileName}使用版本控制删除失败!");
            //        }
            //        else
            //        {
            //            EditorCommon.VersionControl.VersionControlManager.Instance.Delete((EditorCommon.VersionControl.VersionControlCommandResult resultDelete) =>
            //            {
            //                if (resultDelete.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //                {
            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"{ResourceTypeName}{Name} {AbsResourceFileName}使用版本控制删除失败!");
            //                }
            //            }, AbsResourceFileName, $"AutoCommit 删除{ResourceTypeName}{Name}");
            //        }
            //    }, AbsResourceFileName);
            //}
            //else
            {
                if (System.IO.File.Exists(ResourceName.Address))
                    System.IO.File.Delete(ResourceName.Address);
                //string textureImageInfoFileName = AbsResourceFileName + CCore.Graphics.TextureImageInfo.Suffix;
                //if (System.IO.File.Exists(textureImageInfoFileName))
                //    System.IO.File.Delete(textureImageInfoFileName);
            }
            return true;
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            //if (EditorCommon.VersionControl.VersionControlManager.Instance.Enable)
            //{
            //    EditorCommon.VersionControl.VersionControlManager.Instance.Update((EditorCommon.VersionControl.VersionControlCommandResult result) =>
            //    {
            //        if (result.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //        {
            //            EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:{ResourceTypeName}{Name}移动到目录{absFolder}失败!");
            //        }
            //        else
            //        {
            //            EditorCommon.VersionControl.VersionControlManager.Instance.Move((EditorCommon.VersionControl.VersionControlCommandResult resultMove) =>
            //            {
            //                if (resultMove.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //                {
            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:{ResourceTypeName}{Name}移动到目录{absFolder}失败!");
            //                }
            //            }, absResourceFile, absFolder + ResourceFileName, $"AutoCommit {ResourceTypeName}{Name}从{EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absResourceFile)}移动到{EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + ResourceFileName)}");
            //            EditorCommon.VersionControl.VersionControlManager.Instance.Move((EditorCommon.VersionControl.VersionControlCommandResult resultMove) =>
            //            {
            //                if (resultMove.Result != EditorCommon.VersionControl.EProcessResult.Success)
            //                {
            //                    EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:{ResourceTypeName}{Name}移动到目录{absFolder}失败!");
            //                }
            //            }, absResourceFile + CCore.Graphics.TextureImageInfo.Suffix, absFolder + ResourceFileName + CCore.Graphics.TextureImageInfo.Suffix, $"AutoCommit {ResourceTypeName}{Name}从{EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absResourceFile)}移动到{EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + ResourceFileName)}");

            //            DoChangeReferencesRef(absResourceFile, absFolder + ResourceFileName);
            //            MoveInfoAction?.Invoke();
            //        }
            //    }, absResourceFile);

            //}
            //else
            //{
            //    try
            //    {
            //        if (System.IO.File.Exists(absFolder + ResourceFileName))
            //        {
            //            EditorCommon.MessageBox.enMessageBoxResult messageBoxResult = EditorCommon.MessageBox.enMessageBoxResult.None;
            //            messageBoxResult = EditorCommon.MessageBox.Show("该文件以存在,是否覆盖", "警告", EditorCommon.MessageBox.enMessageBoxButton.YesNo);
            //            switch (messageBoxResult)
            //            {
            //                case EditorCommon.MessageBox.enMessageBoxResult.Yes:
            //                    {
            //                        System.IO.File.Delete(absFolder + ResourceFileName);
            //                        System.IO.File.Move(absResourceFile, absFolder + ResourceFileName);
            //                        if(System.IO.File.Exists(absResourceFile + CCore.Graphics.TextureImageInfo.Suffix))
            //                        {
            //                            if (System.IO.File.Exists(absFolder + ResourceFileName + CCore.Graphics.TextureImageInfo.Suffix))
            //                            {
            //                                System.IO.File.Delete(absFolder + ResourceFileName + CCore.Graphics.TextureImageInfo.Suffix);
            //                            }
            //                            System.IO.File.Move(absResourceFile + CCore.Graphics.TextureImageInfo.Suffix, absFolder + ResourceFileName + CCore.Graphics.TextureImageInfo.Suffix);
            //                        }
            //                        DoChangeReferencesRef(absResourceFile, absFolder + ResourceFileName);
            //                        System.IO.File.Delete(absResourceFile + EditorCommon.Program.ResourceInfoExt);
            //                        //MoveInfoAction?.Invoke();
            //                    }
            //                    break;
            //                case EditorCommon.MessageBox.enMessageBoxResult.No:

            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            System.IO.File.Move(absResourceFile, absFolder + ResourceFileName);
            //            if (System.IO.File.Exists(absResourceFile + CCore.Graphics.TextureImageInfo.Suffix))
            //                System.IO.File.Move(absResourceFile + CCore.Graphics.TextureImageInfo.Suffix, absFolder + ResourceFileName + CCore.Graphics.TextureImageInfo.Suffix);
            //            DoChangeReferencesRef(absResourceFile, absFolder + ResourceFileName);
            //            MoveInfoAction?.Invoke();
            //        }
            //    }
            //    catch (UnauthorizedAccessException)
            //    {
            //        EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:资源{Name}移动到目录{absFolder}失败，没有权限!");
            //        return false;
            //    }
            //    catch (PathTooLongException)
            //    {
            //        EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:资源{Name}移动到目录{absFolder}失败，路径太长!");
            //    }
            //    catch (Exception)
            //    {
            //        EditorCommon.MessageReport.Instance.ReportMessage(EditorCommon.MessageReport.enMessageType.Error, $"资源浏览器:资源{Name}移动到目录{absFolder}失败，路径太长!");
            //    }
            //}

            return true;
        }
        public async System.Threading.Tasks.Task OpenEditor()
        {
            //ParentBrowser?.OpenEditor(new object[] { "TextureSourceEditor", this });
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
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
            //var folder = EngineNS.CEngine.Instance.FileManager._GetAbsPathFromRelativePath(EngineNS.CEngine.Instance.FileManager.Content);
            //if (!System.IO.Directory.Exists(folder))
            //    return retList;

            //var techMeta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData("Technique");
            //var matMeta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData("Material");
            //var uvMeta = EditorCommon.Resources.ResourceInfoManager.Instance.GetResourceInfoMetaData("UVAnim");
            //var extArray = new string[techMeta.ResourceExts.Length + matMeta.ResourceExts.Length + uvMeta.ResourceExts.Length];
            //int idx = 0;
            //for (int i=0; i<techMeta.ResourceExts.Length; i++)
            //{
            //    extArray[idx] = techMeta.ResourceExts[i];
            //    idx++;
            //}
            //for (int i = 0; i < matMeta.ResourceExts.Length; i++)
            //{
            //    extArray[idx] = matMeta.ResourceExts[i];
            //    idx++;
            //}
            //for(int i=0; i<uvMeta.ResourceExts.Length; i++)
            //{
            //    extArray[idx] = uvMeta.ResourceExts[i];
            //    idx++;
            //}
            //foreach (var ext in extArray)
            //{
            //    foreach (var file in System.IO.Directory.GetFiles(folder, "*" + ext, SearchOption.AllDirectories))
            //    {
            //        var info = EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(file + EditorCommon.Program.ResourceInfoExt, null);
            //        var resRef = info as EditorCommon.Resources.IResourceReference;
            //        if (resRef == null)
            //            continue;

            //        if (resRef.IsDependencyWith(this))
            //        {
            //            retList.Add(info);
            //        }
            //    }
            //}

            return retList;
        }
        public bool IsDependencyWith(EditorCommon.Resources.ResourceInfo info)
        {
            switch(info.ResourceTypeName)
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
            var retValue = new TextureResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Texture;

            var image = await EditorCommon.ImageInit.GetImage(resourceName.Address) as BitmapSource;
            if(image != null)
            {
                retValue.Dimensions = image.PixelWidth + "X" + image.PixelHeight;
                retValue.PixelFormat = image.Format;
            }

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
            // <MenuItem Header="Add Feature or Content Pack..." menu:MenuAssist.Icon="/ResourceLibrary;component/UEIcon/Icons/icon_file_saveall_40x.png" Style="{DynamicResource {ComponentResourceKey ResourceId=MenuItem_Default, TypeInTargetAssembly={x:Type res:CustomResources}}}"/>
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
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
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
        public override bool IsExists(string file)
        {
            if (true == System.IO.File.Exists(file))
                return true;

            var pos = file.LastIndexOf('.');
            file = file.Substring(0, pos);
            file += EngineNS.CEngineDesc.TextureExtension;

            return System.IO.File.Exists(file);
        }
        public override void DoImport(string file, string target, bool overwrite = false)
        {
            var rawData = EditorCommon.ImageInit.ReadRawData(file);
            //var pngData = EditorCommon.ImageInit.ConverImage2PNG(file);
            //if (pngData == null)
            //{
            //    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Resource", $"Texture {file} is not found");
            //    return;
            //}

            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
            var attr = xnd.Node.AddAttrib("Desc");

            attr.Version = 1;
            attr.BeginWrite();
            attr.Write(file);
            //这里最好导出的时候有参数，确定是否sRGB
            var txDesc = new EngineNS.CTxPicDesc();
            txDesc.SetDefault();
            unsafe
            {
                attr.Write((IntPtr)(&txDesc), sizeof(EngineNS.CTxPicDesc));
            }
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("RawImage");
            attr.BeginWrite();
            attr.Write(rawData);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("PNG");
            attr.BeginWrite();
            var pngBlob = new EngineNS.Support.CBlobObject();
            EngineNS.CShaderResourceView.ConvertImage(EngineNS.CEngine.Instance.RenderContext, rawData, EIMAGE_FILE_FORMAT.PNG, pngBlob);
            var pngData = pngBlob.ToBytes();
            attr.Write(pngData);
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("JPG");
            attr.BeginWrite();
            var jpgBlob = new EngineNS.Support.CBlobObject();
            EngineNS.CShaderResourceView.ConvertImage(EngineNS.CEngine.Instance.RenderContext, rawData, EIMAGE_FILE_FORMAT.JPG, jpgBlob);
            attr.Write(jpgBlob.ToBytes());
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("DDS");
            attr.BeginWrite();
            var ddsBlob = new EngineNS.Support.CBlobObject();
            EngineNS.CShaderResourceView.ConvertImage(EngineNS.CEngine.Instance.RenderContext, rawData, EIMAGE_FILE_FORMAT.DDS, ddsBlob);
            attr.Write(ddsBlob.ToBytes());
            attr.EndWrite();

            attr = xnd.Node.AddAttrib("ETC2");
            attr.BeginWrite();
            //EngineNS.CShaderResourceView.SaveETC2(file, attr, 0, true);
            EngineNS.CShaderResourceView.SaveETC2_Png(pngData, attr, 0, true);
            attr.EndWrite();

            var pos = target.LastIndexOf('.');
            target = target.Substring(0, pos);
            target += EngineNS.CEngineDesc.TextureExtension;
            EngineNS.IO.XndHolder.SaveXND(target, xnd);

            pos = this.ResourceName.Name.LastIndexOf('.');
            var newRName = this.ResourceName.Name.Substring(0, pos);
            newRName += EngineNS.CEngineDesc.TextureExtension;
            this.ResourceName = RName.GetRName(newRName);
        }
    }
}
