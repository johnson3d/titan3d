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

namespace MacrossEnumEditor
{
    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.MacrossEnum, ResourceExts = new string[] { ".macross_enum" })]
    public class MacrossEnumResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceInfoCreateEmpty
    {
        #region IResourceCreateData
        public class MacrossEnumCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public string ResourceName { get; set; }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            [Browsable(false)]
            public string Description { get; set; }
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.MacrossEnum)]
            public RName EnumName
            {
                get;
                set;
            } = RName.EmptyName;
            public RName.enRNameType RNameType { get; set; }
        }

        #endregion

        #region Interface

        //public static readonly string MacrossLinkExtension = ".link";
        public override string ResourceTypeName => EngineNS.Editor.Editor_RNameTypeAttribute.MacrossEnum;

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 31, 72));

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Enum.png", UriKind.Absolute));

        public string EditorTypeName => "MacrossEnumEditor";

        public string CreateMenuPath => "Macross/Macross Enum";

        public bool IsBaseResource => false;

        protected Type mBaseType = null;
        public Type BaseType
        {
            get
            {
                if (mBaseType == null)
                {
                    if (BaseTypeIsMacross)
                    {
                        var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;//EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Common, EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll", "", true);
                        mBaseType = assembly.GetType(BaseTypeSaveName);
                    }
                    else
                    {
                        mBaseType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(BaseTypeSaveName);
                    }
                }
                return mBaseType;
            }
        }
        [EngineNS.Rtti.MetaData]
        public string BaseTypeSaveName
        {
            get;
            set;
        }
        [EngineNS.Rtti.MetaData]
        public bool BaseTypeIsMacross
        {
            get;
            set;
        } = false;
        [EngineNS.Rtti.MetaData]
        public int Version
        {
            get;
            set;
        } = 0;
        [EngineNS.Rtti.MetaData]
        public Guid Id
        {
            get;
            set;
        } = Guid.NewGuid();

        public static async Task<MacrossEnumResourceInfo> GetBaseMacrossResourceInfo(Type baseType)
        {
            var macrossDirs = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, baseType.Name + EngineNS.CEngineDesc.MacrossEnumExtension + ".rinfo", System.IO.SearchOption.AllDirectories);
            if (macrossDirs.Count > 0)
            {
                var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(macrossDirs[0], null) as MacrossEnumResourceInfo;
                return resInfo;
            }
            return null;
        }

        protected override async Task<ResourceInfo> CreateResourceInfoFromResourceOverride(EngineNS.RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new MacrossEnumResourceInfo();
            return retValue;
        }

        //public ICustomCreateDialog GetCustomCreateDialogWindow()
        //{
        //    var retVal = new CreateMacrossWindow();
        //    retVal.HostResourceInfo = this;
        //    return retVal;
        //}

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return new MacrossEnumCreateData();
        }

        public virtual string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "MacrossEnum", EngineNS.CEngineDesc.MacrossEnumExtension);
        }

        public async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        public ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            // 判断资源名称是否合法
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }

            var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(EngineNS.CEngine.Instance.FileManager.ProjectContent, name + EngineNS.CEngineDesc.MacrossEnumExtension, SearchOption.AllDirectories);
            if (dirs.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的Macross文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override async Task<bool> MoveToFolderOverride(string absFolder, EngineNS.RName currentResourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }

        public override async Task Save(bool withSnapshot = false)
        {
            Version++;

            if (BaseTypeIsMacross && ReferenceRNameList.Count == 0)
            {
                var macrossDirs = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, BaseType.Name + EngineNS.CEngineDesc.MacrossEnumExtension + ".rinfo", System.IO.SearchOption.AllDirectories);
                if (macrossDirs.Count > 0)
                {
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(macrossDirs[0], null);
                    if (resInfo != null)
                    {
                        ReferenceRNameList.Add(resInfo.ResourceName);
                    }
                }
            }

            await base.Save(withSnapshot);
        }

        ValidationResult IResourceInfoCreateEmpty.ResourceNameAvailable(string absFolder, string name)
        {
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + CEngineDesc.MacrossEnumExtension, SearchOption.TopDirectoryOnly);
            if (files.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的模型文件!");
            }

            return new ValidationResult(true, null);
        }

        protected override Task<bool> RenameOverride(string absFolder, string newName)
        {
            throw new NotImplementedException();
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, RName oldRName)
        {
            throw new NotImplementedException();
        }

        public async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var result = new MacrossEnumResourceInfo();

            var data = createData as MacrossEnumCreateData;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + data.ResourceName, EngineNS.CEngine.Instance.FileManager.ProjectContent);
            reName += EngineNS.CEngineDesc.MacrossEnumExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);
            return result;
        }
        #endregion

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var tagFile = data.GetTargetAbsFileName();
            var srcFile = data.GetSourceAbsFileName();
            if(!string.Equals(tagFile, srcFile, StringComparison.OrdinalIgnoreCase))
            {
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcFile + ".cs", tagFile + ".cs", true);
                EngineNS.CEngine.Instance.FileManager.CopyFile(srcFile + ".xml", tagFile + ".xml", true);
            }

            return true;
        }

        protected override async Task<bool> InitializeContextMenuOverride(ContextMenu contextMenu)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

      
            var menuItemStyle = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
       

            var menuItem = new MenuItem()
            {
                Header = "删除",
                Style = menuItemStyle,
                Name = "MenuItem_SceneResourceInfo_Delete"
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);

            return true;
        }

        protected override async System.Threading.Tasks.Task<bool> DeleteResourceOverride()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address + ".rinfo");
            if (EngineNS.CEngine.Instance.FileManager.FileExists(ResourceName.Address + ".xml"))
            {
                EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address + ".xml");
            }

            if (EngineNS.CEngine.Instance.FileManager.FileExists(ResourceName.Address + ".cs"))
            {
                EngineNS.CEngine.Instance.FileManager.DeleteFile(ResourceName.Address + ".cs");
            }

            return true;
        }

    }
}
