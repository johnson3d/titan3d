using EngineNS;
using EngineNS.Bricks.AssetImpExp;
using EngineNS.Bricks.AssetImpExp.Creater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EditorCommon.Controls.ResourceBrowser.Import
{
    /// <summary>
    /// Interaction logic for ResourceImportControl.xaml
    /// </summary>
    public partial class ResourceImportControl : ResourceLibrary.WindowBase, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        IContentControlHost mParentWindow = null;
        string[] mFileList;
        RName mSkeletonAsset;
        //[EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Skeleton)]
        public RName SkeletonAsset
        {
            get => mSkeletonAsset;
            set
            {
                mSkeletonAsset = value;
                OnPropertyChanged("SkeletonAsset");
            }
        }

        public ContentControl.ShowSourcesInDirData.FolderData CurrentFolderData;
        AssetImpExpManager AssetImpExpManager = new AssetImpExpManager();
        bool mIsSingleFile = true;
        public ResourceImportControl(IContentControlHost parentWindow, string[] datas, ContentControl.ShowSourcesInDirData.FolderData folderData)
        {
            InitializeComponent();
            CurrentFolderData = folderData;
            //SkeletonAssetNameControl.RNameResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Skeleton;
            mParentWindow = parentWindow;
            mFileList = datas;
            //ImportPropertyGrid.IsEnabled = false;
            ImportButton.IsEnabled = false;
            //ImportAllButton.IsEnabled = false;
            Action preProcessAction = (async () =>
            {
                await PreProcess();
            });
            preProcessAction();
            mMessageDocument = mMessageWindow.MessageRichTextBox.Document;
            mMessageDocument.Blocks.Clear();
        }
        MutiFilesImportOption mMutiFilesImportOption = null;
        public async Task PreProcess()
        {
            if (mFileList.Length > 1)
                mIsSingleFile = false;
            AssetImpExpManager.OnAssetImportMessageDumping += AssetImpExpManager_OnAssetImportMessageDumping;
            if (mIsSingleFile)
            {
                FilePathLabel.Content = mFileList[0];
                var option = await AssetImpExpManager.PreImport(mFileList[0], CurrentFolderData.AbsFolder);
                ImportPropertyGrid.Instance = option;
            }
            else
            {
                FilePathLabel.Content = "MutiFiles";
                mMutiFilesImportOption = new MutiFilesImportOption();

                ImportPropertyGrid.Instance = mMutiFilesImportOption;
            }
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new System.EventHandler(OnTimedEvent);
            timer.Interval = System.TimeSpan.FromSeconds(0.8f);
            timer.Start();
            //ImportPropertyGrid.IsEnabled = true;
            ImportButton.IsEnabled = true;
            //ImportAllButton.IsEnabled = true;
        }
        bool mIsDeleteAll = false;
        bool mIsIgnoreAll = false;
        bool mAllOptionIsValid = false;
        //bool mHasMessage = false;
        private AssetImportMessageType AssetImpExpManager_OnAssetImportMessageDumping(object sender, string fileName, string assetName, AssetImportMessageType type, int level, string resourceDetial, float progress)
        {
            switch (type)
            {
                case AssetImportMessageType.AMT_Error:
                    {

                        var noUse = EngineNS.CEngine.Instance.EventPoster.Post(() =>
                        {
                            Paragraph paragraph = new Paragraph();
                            paragraph.Foreground = Brushes.Red;
                            paragraph.FontSize = 12;
                            paragraph.Margin = new System.Windows.Thickness(0);
                            paragraph.Inlines.Add("错误:" + resourceDetial);
                            mMessageDocument.Blocks.Add(paragraph);
                            mMessageWindow.Show();
                            return true;
                        }, EngineNS.Thread.Async.EAsyncTarget.Main);

                    }
                    break;
                case AssetImportMessageType.AMT_Warning:
                    {
                        if (mWarningMessageList.Contains(resourceDetial))
                            return AssetImportMessageType.AMT_Warning;
                        mWarningMessageList.Add(resourceDetial);
                        var noUse = EngineNS.CEngine.Instance.EventPoster.Post(() =>
                        {
                            Paragraph paragraph = new Paragraph();
                            paragraph.Foreground = Brushes.Yellow;
                            paragraph.FontSize = 12;
                            paragraph.Margin = new System.Windows.Thickness(0);
                            paragraph.Inlines.Add("警告:" + resourceDetial);
                            mMessageDocument.Blocks.Add(paragraph);
                            return true;
                        }, EngineNS.Thread.Async.EAsyncTarget.Main);
                    }
                    break;
                case AssetImportMessageType.AMT_Import:
                    {
                        if (level == 0)
                        {
                            var name = CEngine.Instance.FileManager.GetPureFileFromFullName(assetName, true);
                            var noUse = UpdateProcess(resourceDetial, progress * 100);
                        }
                        var no = EngineNS.CEngine.Instance.EventPoster.Post(() =>
                        {
                            Paragraph paragraph = new Paragraph();
                            paragraph.Foreground = Brushes.White;
                            paragraph.FontSize = 12;
                            paragraph.Margin = new System.Windows.Thickness(0);
                            paragraph.Inlines.Add("导入:" + resourceDetial);
                            mMessageDocument.Blocks.Add(paragraph);
                            return true;
                        }, EngineNS.Thread.Async.EAsyncTarget.Main);
                    }
                    break;
                case AssetImportMessageType.AMT_Save:
                    {
                        var parent = mParentWindow as ResourceBrowser.BrowserControl;
                        bool currentFolderIsShowing = false;
                        if (parent != null)
                        {
                            foreach (var fd in parent.CurrentFolderData.FolderDatas)
                            {
                                if (fd.IsEqual(CurrentFolderData))
                                {
                                    currentFolderIsShowing = true;
                                    break;
                                }
                            }
                        }
                        if (currentFolderIsShowing)
                        {
                            var data = new ContentControl.ShowSourcesInDirData()
                            {
                                ResetHistory = true,
                                ForceRefresh = true,
                            };
                            data.FolderDatas.Add(CurrentFolderData);
                            var noUse = mParentWindow.ShowSourcesInDir(data);
                        }
                    }
                    break;
                case AssetImportMessageType.AMT_FileExist:
                    {
                        if (mAllOptionIsValid)
                        {
                            if (mIsDeleteAll)
                            {
                                return AssetImportMessageType.AMT_DeleteOriginFile;
                            }
                            if (mIsIgnoreAll)
                            {
                                return AssetImportMessageType.AMT_IgnoreFile;
                            }
                        }
                        var result = EditorCommon.MessageBox.Show("文件" + assetName + "已存在,是否覆盖？", EditorCommon.MessageBox.enMessageBoxButton.Yes_YesAll_No_NoAll_Cancel);
                        if (result == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                        {
                            return AssetImportMessageType.AMT_DeleteOriginFile;
                        }
                        if (result == EditorCommon.MessageBox.enMessageBoxResult.No)
                        {
                            return AssetImportMessageType.AMT_IgnoreFile;
                        }
                        if (result == EditorCommon.MessageBox.enMessageBoxResult.Cancel)
                        {
                            return AssetImportMessageType.AMT_IgnoreFile;
                        }
                        if (result == EditorCommon.MessageBox.enMessageBoxResult.YesAll)
                        {
                            mAllOptionIsValid = true;
                            mIsDeleteAll = true;
                            return AssetImportMessageType.AMT_DeleteOriginFile;
                        }
                        if (result == EditorCommon.MessageBox.enMessageBoxResult.NoAll)
                        {
                            mAllOptionIsValid = true;
                            mIsIgnoreAll = true;
                            return AssetImportMessageType.AMT_IgnoreFile;
                        }
                    }
                    break;
            }
            return AssetImportMessageType.AMT_UnKnown;
        }

        int index = 1;
        private void OnTimedEvent(object sender, EventArgs e)
        {
            index++;
            if (index > 6)
                index = 1;
            string temp = "";
            for (int i = 0; i < index; ++i)
            {
                temp += ".";
            }
            ticktickTextBlock.Text = temp;
        }
        List<string> mWarningMessageList = new List<string>();
        FlowDocument mMessageDocument = new FlowDocument();
        ImportMessage mMessageWindow = new ImportMessage();
        private void MessageButton_Click(object sender, RoutedEventArgs e)
        {
            mMessageWindow.Show();
        }
        public async System.Threading.Tasks.Task UpdateProcess(string info, double progressChange)
        {
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                string showName = "";

                var strings = info.Split(':');
                var head = strings[0];
                //var tailIndex = strings[1].LastIndexOf('\\');
                //var tail = strings[1].Substring(tailIndex, strings[1].Length);
                if (info.Length > 100)
                {
                    showName = info.Substring(info.Length - 30);
                    showName = head + ":..." + showName;
                }
                else
                    showName = info;
                if (ProgressGrid.Visibility != Visibility.Visible)
                    ProgressGrid.Visibility = Visibility.Visible;
                ImportProgressTextBlock.Text = showName;
                ImportProgressTextBlock.ToolTip = info;
                ImportProgressBar.Value = progressChange;
                //TextBlock_Info.Text = info;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Main);
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportPropertyGrid.IsEnabled = false;
            ImportButton.IsEnabled = false;
            //ImportAllButton.IsEnabled = false;
            System.Action actions = async () =>
            {
                if (mIsSingleFile)
                    await AssetImpExpManager.Import(mFileList[0]);
                else
                {
                    await AssetImpExpManager.ImportMutiFiles(mFileList, CurrentFolderData.AbsFolder, mMutiFilesImportOption);
                }
                //do sth process
                await UpdateProcess("Import Done!", 1 * 100);
                this.Close();
                var parent = mParentWindow as ResourceBrowser.BrowserControl;
                bool currentFolderIsShowing = false;
                if (parent != null)
                {
                    foreach (var fd in parent.CurrentFolderData.FolderDatas)
                    {
                        if (IsCancle)
                            break;
                        if (fd.IsEqual(CurrentFolderData))
                        {
                            currentFolderIsShowing = true;
                            break;
                        }
                    }
                }
                if (currentFolderIsShowing)
                {
                    var data = new ContentControl.ShowSourcesInDirData()
                    {
                        ResetHistory = true,
                        ForceRefresh = true,
                    };
                    data.FolderDatas.Add(CurrentFolderData);
                    var noUse = mParentWindow.ShowSourcesInDir(data);
                }

            };
            actions();
        }
        bool IsCancle = false;
        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            IsCancle = true;
            AssetImpExpManager.IsCancle = true;
            ImportPropertyGrid.IsEnabled = true;
            mMessageWindow.Close();
            this.Close();
        }
        protected override void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            IsCancle = true;
            AssetImpExpManager.IsCancle = true;
            ImportPropertyGrid.IsEnabled = true;
            mMessageWindow.Close();
            this.Close();
        }
    }
}
