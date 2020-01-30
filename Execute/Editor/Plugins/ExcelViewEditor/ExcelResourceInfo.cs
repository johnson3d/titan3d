using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EngineNS.Bricks.ExcelTable;
using EditorCommon.Resources;
using EngineNS;

namespace ExcelViewEditor
{
    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Excel, ResourceExts = new string[] { ".xls" })]
    public class ExcelResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceInfoCreateEmpty,
                                       EditorCommon.Resources.IResourceInfoCustomCreateDialog
    {
        #region IResourceCreateData
        public class ExcelCreateData : IResourceCreateData
        {
            [Browsable(false)]
            public string ResourceName { get; set; }
            [Browsable(false)]
            public ICustomCreateDialog HostDialog { get; set; }
            [Browsable(false)]
            public string Description { get; set; }
            [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.Excel)]
            public string ExcelName
            {
                get;
                set;
            } = "";
            public RName MacrossName
            {
                get;
                set;
            }
        
            public RName.enRNameType RNameType { get; set; }
        }

        #endregion

        #region Interface

        //public static readonly string MacrossLinkExtension = ".link";
        public override string ResourceTypeName => EngineNS.Editor.Editor_RNameTypeAttribute.Excel;

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 70, 43));

        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/Xsl.png", UriKind.Absolute));

        public string EditorTypeName => "ExcelViewEditor";

        public string CreateMenuPath => "Excel/Excel";

        public bool IsBaseResource => false;

        protected Type mBaseType = null;
        public Type BaseType
        {
            get
            {
                if (mBaseType == null)
                {
                    if (BaseTypeIsExcel)
                    {
                        var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;// EngineNS.Rtti.RttiHelper.GetAssemblyFromDllFileName(EngineNS.ECSType.Common, EngineNS.CEngine.Instance.FileManager.Bin + "MacrossScript.dll", "", true);
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
        public bool BaseTypeIsExcel
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
        [EngineNS.Rtti.MetaData]
        public RName MacrossName
        {
            get;
            set;
        }

        //public static async Task<MacrossEnumResourceInfo> GetBaseMacrossResourceInfo(Type baseType)
        //{
        //    var macrossDirs = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.Content, baseType.Name + EngineNS.CEngineDesc.MacrossEnumExtension + ".rinfo", System.IO.SearchOption.AllDirectories);
        //    if (macrossDirs.Length > 0)
        //    {
        //        var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(macrossDirs[0], null) as MacrossEnumResourceInfo;
        //        return resInfo;
        //    }
        //    return null;
        //}

        protected override async Task<ResourceInfo> CreateResourceInfoFromResourceOverride(EngineNS.RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new ExcelResourceInfo();
            return retValue;
        }

        public ICustomCreateDialog GetCustomCreateDialogWindow()
        {
            var retVal = new CreateDataSetter();
            retVal.HostResourceInfo = this;
            return retVal;
        }

        public IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return new ExcelCreateData();
        }

        public virtual string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "Excel", EngineNS.CEngineDesc.ExcelExtension);
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

            var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(EngineNS.CEngine.Instance.FileManager.ProjectContent, name + EngineNS.CEngineDesc.ExcelExtension, SearchOption.AllDirectories);
            if (dirs.Count > 0)
            {
                return new ValidationResult(false, "已包含同名的Excel文件!");
            }

            return new ValidationResult(true, null);
        }

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

        public override async Task Save(bool withSnapshot = false)
        {
            Version++;

            if (BaseTypeIsExcel && ReferenceRNameList.Count == 0)
            {
                var excelDirs = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, BaseType.Name + EngineNS.CEngineDesc.ExcelExtension + ".rinfo", System.IO.SearchOption.AllDirectories);
                if (excelDirs.Count > 0)
                {
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(excelDirs[0], null);
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
            var files = EngineNS.CEngine.Instance.FileManager.GetFiles(absFolder, name + CEngineDesc.ExcelExtension, SearchOption.TopDirectoryOnly);
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
            var result = new ExcelResourceInfo();

            var data = createData as ExcelCreateData;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + (data.ExcelName.Equals("") ? data.ResourceName : data.ExcelName), EngineNS.CEngine.Instance.FileManager.ProjectContent);
            reName += EngineNS.CEngineDesc.ExcelExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);

            ExcelExporter newexport = new ExcelExporter();
            var macrossType = EngineNS.Macross.MacrossFactory.Instance.GetMacrossType(data.MacrossName);
            newexport.Init("", macrossType);
            List<Object> lst = new List<Object>();
            //lst.Add( Activator.CreateInstance(data._Type)); //必须要有默认构造函数
            
            newexport.Objects2Table(lst);
            newexport.Save(result.ResourceName.Address);
            result.MacrossName = data.MacrossName;// + ", " + data._Type.Assembly.FullName;
            return result;
        }
        #endregion

        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var dataSet = new EngineNS.Bricks.DataProvider.GDataSet();
            dataSet.LoadDataSet(data.ObjType, data.RNameMapper.Name, false);
            data.RNameMapper.ResObject = dataSet;
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            return true;
        }

        [EngineNS.Rtti.MetaData]
        public string TypeName
        {
            get;
            set;
        }
    }
}
