using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = "Folder")]
    public class FolderResourceInfo : EditorCommon.Resources.ResourceInfo, EditorCommon.Resources.IResourceInfoEditor, Resources.IResourceInfoDragDrop, Resources.IFolderItem
    {
        public override string ResourceTypeName => "Folder";

        public override Brush ResourceTypeBrush => Brushes.Transparent;

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Folders/Folder_BaseMix_512x.png", UriKind.Absolute));

        public string EditorTypeName => "";

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }

        protected override async Task<EditorCommon.Resources.ResourceInfo> CreateResourceInfoFromResourceOverride(EngineNS.RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new FolderResourceInfo();
            retValue.ResourceName = resourceName;
            return retValue;
        }

        public override async System.Threading.Tasks.Task<bool> AsyncLoad(string absFileName)
        {
            mIsLoading = true;
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            mIsLoading = false;
            return true;
        }

        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (ParentBrowser == null)
                return;
            var data = new EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData();
            data.FolderDatas.Add(new Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
            {
                AbsFolder = ResourceName.Address,
                RootFolder = ResourceName.GetRootFolder(),
            });
            var noUse = ParentBrowser.ShowSourcesInDir(data);
        }

        #region IFolderItem

        EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData mFolderData;
        public EditorCommon.Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData FolderData
        {
            get
            {
                if(mFolderData == null)
                {
                    mFolderData = new Controls.ResourceBrowser.ContentControl.ShowSourcesInDirData.FolderData()
                    {
                        AbsFolder = ResourceName.Address,
                        RootFolder = ResourceName.GetRootFolder(),
                    };
                }
                return mFolderData;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public string PathName
        {
            get { return ResourceName.PureName(true); }
        }
        public void AddSubFolderItem(Resources.IFolderItem item)
        {
        }
        #endregion

        #region IResourceInfoDragDrop
        public bool DragEnter(DragEventArgs e)
        {
            var browser = ParentBrowser as Controls.ResourceBrowser.BrowserControl;
            if (browser == null)
                return false;

            return browser.FolderItem_OnDragEnter(this, e);
        }

        public void DragLeave(DragEventArgs e)
        {
            var browser = ParentBrowser as Controls.ResourceBrowser.BrowserControl;
            if (browser == null)
                return;

            browser.FolderItem_OnDragLeave(this, e);
        }

        public void DragOver(DragEventArgs e)
        {
            var browser = ParentBrowser as Controls.ResourceBrowser.BrowserControl;
            if (browser == null)
                return;

            browser.FolderItem_OnDragOver(this, e);
        }

        public void Drop(DragEventArgs e)
        {
            var browser = ParentBrowser as Controls.ResourceBrowser.BrowserControl;
            if (browser == null)
                return;

            browser.FolderItem_OnDrop(this, e);
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
