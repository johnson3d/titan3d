using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace EditorCommon.ResourceInfos
{
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Font, ResourceExts = new string[] { ".ttf" })]
    public class FontResourceInfo : ResourceInfo,
                                    IResourceInfoForceReload

    {
        #region resourceInfo

        [EditorCommon.Resources.ResourceToolTipAttribute]
        [DisplayName("类型")]
        public override string ResourceTypeName => "字体";

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 48, 48));
        ImageSource mResourceIcon;
        public override ImageSource ResourceIcon
        {
            get
            {
                if (mResourceIcon == null)
                {
                    mResourceIcon = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Font_64x.png", UriKind.Absolute));
                }
                return mResourceIcon;
            }
        } 

        protected override Task<bool> DeleteResourceOverride()
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            throw new NotImplementedException();
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, RName newRName, RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IResourceInfoForceReload
        public void ForceReload()
        {

        }
        public string[] GetFileSystemWatcherAttentionExtensions()
        {
            return new string[] { ".ttf" };
        }

        #endregion

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            // 字体不引用任何资源，直接拷贝
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var newFile = data.GetTargetAbsFileName();
            var srcFile = data.GetSourceAbsFileName();
            if(!string.Equals(newFile, srcFile, StringComparison.OrdinalIgnoreCase))
            {
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcFile, newFile, true);
            }
            return true;
        }

    }
}
