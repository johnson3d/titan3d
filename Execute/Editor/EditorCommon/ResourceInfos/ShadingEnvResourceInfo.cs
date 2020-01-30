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
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.ShadingEnv, ResourceExts = new string[] { ".senv" })]
    public class ShadingEnvResourceInfo : EditorCommon.Resources.ResourceInfo, EditorCommon.Resources.IResourceInfoDragDrop, EditorCommon.Resources.IResourceInfoEditor, EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        public class ShadingEnvCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public string ResourceName { get; set; }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.ShaderCode)]
            public RName Shader
            {
                get;
                set;
            } = RName.GetRName("Shaders/FSBase.shadingenv");
        }
        public override string ResourceTypeName => "Shading Env";

        public override Brush ResourceTypeBrush
        {
            get;
        } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 175, 175));
        public override ImageSource ResourceIcon
        {
            get;
        } = new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/UEIcon/Icons/AssetIcons/MaterialFunction_64x.png", UriKind.Absolute));

        public string CreateMenuPath => "Materials & Textures/Shading Env";

        public bool IsBaseResource => false;

        public string EditorTypeName => throw new NotImplementedException();

        public async System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string Absfolder, EditorCommon.Resources.IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new ShadingEnvResourceInfo();

            var mcd = createData as ShadingEnvCreateData;
            if (EngineNS.CEngine.Instance.FileManager.GetFileExtension(mcd.ResourceName) != EngineNS.CEngineDesc.ShadingEnvExtension)
            {
                mcd.ResourceName = mcd.ResourceName + EngineNS.CEngineDesc.ShadingEnvExtension;
            }

            var ipWin = createData as InputWindow.InputWindow;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(Absfolder + "/" + mcd.ResourceName, EngineNS.CEngine.Instance.FileManager.Content);
            result.ResourceName = RName.GetRName(reName);
            var senv = EngineNS.CEngine.Instance.ShadingEnvManager.NewGfxShadingEnv(
                typeof(EngineNS.Graphics.CGfxShadingEnv),
                RName.GetRName(reName), mcd.Shader);

            senv.SaveShadingEnv();

            return result;
        }

        public bool DragEnter(DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void DragLeave(DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void DragOver(DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Drop(DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            var result = new ShadingEnvCreateData();
            result.ResourceName = EditorCommon.Program.GetValidName(absFolder, "GfxShadingEnv", CEngineDesc.ShadingEnvExtension);
            return result;
        }

        public string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "GfxShadingEnv", CEngineDesc.ShadingEnvExtension);
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
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + CEngineDesc.ShadingEnvExtension, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return new ValidationResult(false, "已包含同名的ShadingEnv文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override async System.Threading.Tasks.Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new ShadingEnvResourceInfo();

            result.ResourceName = resourceName;

            return result;
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, RName currentResourceName)
        {
            throw new NotImplementedException();
        }

        protected override Task OnReferencedRNameChnagedOverride(ResourceInfo referencedResInfo, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }
    }
}
