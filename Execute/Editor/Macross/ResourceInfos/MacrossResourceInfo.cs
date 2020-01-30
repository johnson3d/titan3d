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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Resources;
using EngineNS;

namespace Macross.ResourceInfos
{
    public class ResourceCreateData : IResourceCreateData
    {
        [Browsable(false)]
        public EngineNS.RName.enRNameType RNameType { get; set; }
        public string ResourceName { get; set; }
        public virtual ICustomCreateDialog HostDialog { get; set; }
        [Browsable(false)]
        public string Description { get; set; }
        public virtual Type ClassType { get; set; }
        public EngineNS.ECSType CSType;
        public bool IsMacrossType;
    }

    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross, ResourceExts = new string[] { ".macross" })]
    public class MacrossResourceInfo : EditorCommon.Resources.ResourceInfo,
                                       EditorCommon.Resources.IResourceInfoEditor,
                                       EditorCommon.Resources.IResourceInfoCreateEmpty,
                                       EditorCommon.Resources.IResourceInfoCustomCreateDialog
    {
        public static readonly string MacrossLinkExtension = ".link";
        //public static readonly string MacrossCodeExtension = ".cs";

        public override string ResourceTypeName => EngineNS.Editor.Editor_RNameTypeAttribute.Macross;

        public override Brush ResourceTypeBrush => new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 31, 72));

        public override ImageSource ResourceIcon
        {
            get
            {
                if(BaseType != null)
                {
                    var atts = BaseType.GetCustomAttributes(typeof(EngineNS.Editor.Editor_MacrossClassIconAttribute), true);
                    if(atts.Length > 0)
                    {
                        var att = atts[0] as EngineNS.Editor.Editor_MacrossClassIconAttribute;
                        if(!string.IsNullOrEmpty(att.IconRNameStr))
                        {
                            var rName = EngineNS.RName.GetRName(att.IconRNameStr, att.IconRNameType);
                            var imgs = EditorCommon.ImageInit.SyncGetImage(rName.Address);
                            return imgs[0];
                        }
                    }
                }
                return new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Blueprint_64x.png", UriKind.Absolute));
            }
        }

        public virtual string EditorTypeName => "MacrossEditor";

        public virtual string CreateMenuPath => "Macross/Macross Class";

        public virtual bool IsBaseResource => true;

#warning 更改名称需要重新生成dll文件

        protected Type mBaseType = null;
        [ResourceToolTipAttribute]
        [DisplayName("宏图基类")]
        public Type BaseType
        {
            get
            {
                if(mBaseType == null)
                {
                    if(BaseTypeIsMacross)
                    {
                        // todo: 通过BaseTypeSaveName区分服务器客户端
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
        public void ResetBaseType(Type baseType)
        {
            mBaseType = baseType;
            var assembly = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly;
            BaseTypeSaveName = EngineNS.Rtti.RttiHelper.GetTypeSaveString(baseType);
            if (assembly.GetType(BaseTypeSaveName) == baseType)
                BaseTypeIsMacross = true;
            else
                BaseTypeIsMacross = false;
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

        [EngineNS.Rtti.MetaClass]
        public class CustomFunctionData : EngineNS.IO.Serializer.Serializer
        {
            [EngineNS.Rtti.MetaData]
            public Guid Id { get; set; }
            [EngineNS.Rtti.MetaData]
            public CodeDomNode.CustomMethodInfo MethodInfo { get; set; }
        }
        [EngineNS.Rtti.MetaData]
        public List<CustomFunctionData> CustomFunctions_Client = new List<CustomFunctionData>();
        [EngineNS.Rtti.MetaData]
        public List<CustomFunctionData> CustomFunctions_Server = new List<CustomFunctionData>();
        bool mNotGenerateCode = false;
        [EngineNS.Rtti.MetaData]
        [ResourceToolTipAttribute]
        [DisplayName("不生成代码")]
        public bool NotGenerateCode
        {
            get => mNotGenerateCode;
            set
            {
                mNotGenerateCode = value;
                NotUse = mNotGenerateCode;

                if(!mIsLoading)
                {
                    if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(ResourceName.Address))
                    {
                        if (mNotGenerateCode)
                        {
                            foreach (var codeFile in EngineNS.CEngine.Instance.FileManager.GetFiles(ResourceName.Address, "*.cs"))
                            {
                                EngineNS.CEngine.Instance.FileManager.MoveFile(codeFile, codeFile + "code");
                            }
                        }
                        else
                        {
                            foreach (var codeFile in EngineNS.CEngine.Instance.FileManager.GetFiles(ResourceName.Address, "*.cscode"))
                            {
                                var file = EngineNS.CEngine.Instance.FileManager.RemoveExtension(codeFile);
                                EngineNS.CEngine.Instance.FileManager.MoveFile(codeFile, file + ".cs");
                            }
                        }

                    }
                    var noUse = BuildDll();
                }
                if (ResourceName != null)
                    TitleName = ResourceName.Name + (NotGenerateCode ? "(不生成代码)" : "");
                OnPropertyChanged("NotGenerateCode");
            }
        }

        async Task BuildDll()
        {
            await Save();
            var codeGenerator = new Macross.CodeGenerator();
            // 生成Collection文件
            await codeGenerator.GenerateAndSaveMacrossCollector(EngineNS.ECSType.Client);
            // 收集所有Macross文件，放入游戏共享工程中
            List<string> macrossfiles = codeGenerator.CollectionMacrossProjectFiles(EngineNS.ECSType.Client);
            codeGenerator.GenerateMacrossProject(macrossfiles.ToArray(), EngineNS.ECSType.Client);
            // 编译游戏dll
            EditorCommon.Program.BuildGameDll(true);
        }

        public override RName ResourceName
        {
            get => base.ResourceName;
            protected set
            {
                base.ResourceName = value;
                TitleName = base.ResourceName.Name + (NotGenerateCode ? "(不生成代码)" : "");
            }
        }

        string mTitleName = "";
        public string TitleName
        {
            get
            {
                if (string.IsNullOrEmpty(mTitleName))
                    mTitleName = ResourceName.Name + (NotGenerateCode ? "(不生成代码)" : "");
                return mTitleName;
            }
            set
            {
                mTitleName = value;
                OnPropertyChanged("TitleName");
            }
        }

        public static async Task<MacrossResourceInfo> GetBaseMacrossResourceInfo(MacrossResourceInfo info)
        {
            if (!info.BaseTypeIsMacross)
                return null;
            return await GetBaseMacrossResourceInfo(info.BaseType);
        }
        public static async Task<MacrossResourceInfo> GetBaseMacrossResourceInfo(Type baseType)
        {
            var macrossDirs = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, baseType.Name + EngineNS.CEngineDesc.MacrossExtension + ".rinfo", System.IO.SearchOption.AllDirectories);
            if (macrossDirs.Count > 0)
            {
                var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(macrossDirs[0], null) as MacrossResourceInfo;
                return resInfo;
            }
            return null;
        }

        public virtual async System.Threading.Tasks.Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var result = new MacrossResourceInfo();

            var data = createData as ResourceCreateData;
            var reName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(absFolder + "/" + data.ResourceName, rootFolder);
            reName += EngineNS.CEngineDesc.MacrossExtension;
            result.ResourceName = EngineNS.RName.GetRName(reName, data.RNameType);
            result.BaseTypeIsMacross = data.IsMacrossType;
            if (result.BaseTypeIsMacross)
            {
                result.BaseTypeSaveName = data.ClassType.FullName;
                var baseResInfo = await GetBaseMacrossResourceInfo(this);
                if (baseResInfo != null)
                    ReferenceRNameList.Add(baseResInfo.ResourceName);
            }
            else
                result.BaseTypeSaveName = EngineNS.Rtti.RttiHelper.GetTypeSaveString(data.ClassType);
            //var macData = new MacrossData(result.ResourceName);
            //macData.ParentType = data.ClassType;
            //macData.CSType = data.CSType;
            //macData.Id = Guid.NewGuid();
            //macData.Save();

            //var code = "namespace MacrossClasses\r\n";
            //code += "{\r\n";
            //code += $"\t// class name:{result.ResourceName.PureName()}\r\n";
            //code += $"\tpublic class Macross_{macData.Id.ToString().Replace("-", "")} : {macData.ParentType.FullName}\r\n";
            //code += "\t{\r\n";
            //code += "\t}\r\n";
            //code += "}\r\n";
            //using(var writer = System.IO.File.CreateText(result.ResourceName.Address + MacrossData.MacrossCodeExtension))
            //{
            //    writer.WriteLine(code);
            //}

            //result.mMacrossData = macData;
            result.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;

            // 创建时走一遍编译，保证当前Macross能够取到this类型
            var csType = ECSType.Client;
            var codeGenerator = new CodeGenerator();
            var ctrl = new MacrossLinkControl();
            ctrl.CurrentResourceInfo = result;
            ctrl.CSType = csType;
            var codeStr = await codeGenerator.GenerateCode(result, ctrl);
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(result.ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(result.ResourceName.Address);
            var codeFile = $"{result.ResourceName.Address}/{result.ResourceName.PureName()}_{csType.ToString()}.cs";
            using (var fs = new System.IO.FileStream(codeFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
            {
                fs.Write(System.Text.Encoding.Default.GetBytes(codeStr), 0, Encoding.Default.GetByteCount(codeStr));
            }
            await codeGenerator.GenerateAndSaveMacrossCollector(csType);
            var files = codeGenerator.CollectionMacrossProjectFiles(csType);
            codeGenerator.GenerateMacrossProject(files.ToArray(), csType);
            EditorCommon.Program.BuildGameDll(true);

            return result;
        }

        protected override async Task<ResourceInfo> CreateResourceInfoFromResourceOverride(RName resourceName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retValue = new MacrossResourceInfo();
            retValue.ResourceName = resourceName;
            retValue.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;
            return retValue;
        }

        public virtual ICustomCreateDialog GetCustomCreateDialogWindow()
        {
            var retVal = new CreateMacrossWindow();
            retVal.HostResourceInfo = this;
            return retVal;
        }

        public virtual IResourceCreateData GetResourceCreateData(string absFolder)
        {
            return new ResourceCreateData();
        }

        public virtual string GetValidName(string absFolder)
        {
            return EditorCommon.Program.GetValidName(absFolder, "Macross", EngineNS.CEngineDesc.MacrossExtension);
        }

        public virtual async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }

        public virtual ValidationResult ResourceNameAvailable(string absFolder, string name)
        {
            // 判断资源名称是否合法
            if (EditorCommon.Program.IsValidRName(name) == false)
            {
                return new ValidationResult(false, "名称不合法!");
            }

            //var dirs = EngineNS.CEngine.Instance.FileManager.GetDirectories(EngineNS.CEngine.Instance.FileManager.Content, name + EngineNS.CEngineDesc.MacrossExtension, SearchOption.AllDirectories);
            //if (dirs.Length > 0)
            //{
            //    return new ValidationResult(false, "已包含同名的Macross文件!");
            //}

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

            if(BaseTypeIsMacross)
            {
                var macrossDirs = EngineNS.CEngine.Instance.FileManager.GetFiles(EngineNS.CEngine.Instance.FileManager.ProjectContent, BaseType.Name + EngineNS.CEngineDesc.MacrossExtension + ".rinfo", System.IO.SearchOption.AllDirectories);
                if(macrossDirs.Count > 0)
                {
                   var filter = new Predicate<RName>(
                                      delegate (RName obj)
                                      {
                                          return obj.Equals(this);//TODO.. 待优化
                                      });
                    var resInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(macrossDirs[0], null);
                    if(resInfo != null && ReferenceRNameList.Exists(filter) == false)
                    {
                        ReferenceRNameList.Add(resInfo.ResourceName);
                    }
                }
            }

            await base.Save(withSnapshot);
        }

        protected override Task OnReferencedRNameChangedOverride(ResourceInfo referencedResInfo, EngineNS.RName newRName, EngineNS.RName oldRName)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> RenameOverride(string absFolder, string newName)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
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
                Name = "MCResInfo_ChangeDesc",
                Header = "更改说明",
                Style = menuItemStyle,
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                var inputWin = new InputWindow.InputWindow();
                inputWin.Description = "更改说明:";
                inputWin.Value = Description;
                if(inputWin.ShowDialog() == true)
                {
                    Description = (string)inputWin.Value;
                }
            };
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem()
            {
                Name = "MCResInfo_Delete",
                Header = "删除",
                Style = menuItemStyle,
            };
            menuItem.Click += async (object sender, RoutedEventArgs e) =>
            {
                await DeleteResource();
            };
            ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_delete_16px.png", UriKind.Absolute)));
            contextMenu.Items.Add(menuItem);
            menuItem = new MenuItem()
            {
                Name = "MCResInfo_NotGenCode",
                Header = "不生成代码",
                Style = menuItemStyle,
                IsCheckable = true,
            };
            menuItem.SetBinding(MenuItem.IsCheckedProperty, new Binding("NotGenerateCode") { Source = this, Mode = BindingMode.TwoWay });
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
                Name = "MCResInfo_ShowInExplorer",
                Header = "在浏览器中显示",
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
            string rinfo = ResourceName.Address + ".rinfo";
            if (EngineNS.CEngine.Instance.FileManager.FileExists(rinfo))
                EngineNS.CEngine.Instance.FileManager.DeleteFile(rinfo);

            if (EngineNS.CEngine.Instance.FileManager.DirectoryExists(ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.DeleteDirectory(ResourceName.Address, true);

            var noUse = BuildDll();

            return true;
        }

        class MCAssetsObject
        {
            public class PlatformAssetsLinks
            {
                class NodesCtrlData
                {
                    public NodesControlAssist CtrlAssist;
                    public string FileName;
                }
                List<NodesCtrlData> NodesCtrlList = new List<NodesCtrlData>();
                public MacrossLinkControlBase LinkCtrl = new MacrossLinkControlBase();

                public async Task LoadFiles(string curDir, ECSType csType)
                {
                    // data
                    var dataFile = $"{curDir}/data_{csType.ToString()}{EngineNS.CEngineDesc.MacrossExtension}";
                    LinkCtrl = new MacrossLinkControlBase();
                    await LinkCtrl.LoadData(dataFile);
                    // link
                    var clientLinkFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(curDir, $"_{csType.ToString()}.link", SearchOption.AllDirectories);
                    foreach (var file in clientLinkFiles)
                    {
                        var fileName = EngineNS.CEngine.Instance.FileManager._GetRelativePathFromAbsPath(file, curDir);

                        var linkAssist = new NodesControlAssist();
                        linkAssist.CSType = csType;
                        await linkAssist.Load(file, true);
                        NodesCtrlList.Add(new NodesCtrlData()
                        {
                            CtrlAssist = linkAssist,
                            FileName = fileName,
                        });
                    }
                }

                public void SaveFiles(string tagDir, ECSType csType)
                {
                    var newDataFile = $"{tagDir}/data_{csType.ToString()}{EngineNS.CEngineDesc.MacrossExtension}".ToLower();
                    LinkCtrl.SaveData(newDataFile);

                    foreach(var linkData in NodesCtrlList)
                    {
                        linkData.CtrlAssist.Save(tagDir + linkData.FileName);
                    }
                }
            }

            public PlatformAssetsLinks ClientLinks = new PlatformAssetsLinks();
            public PlatformAssetsLinks ServerLinks = new PlatformAssetsLinks();
        }
        public override async Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            var obj = new MCAssetsObject();
            await obj.ClientLinks.LoadFiles(data.RNameMapper.Name.Address, ECSType.Client);
            await obj.ServerLinks.LoadFiles(data.RNameMapper.Name.Address, ECSType.Server);

            data.RNameMapper.ResObject = obj;
            return true;
        }
        public override async Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var obj = data.ResObject as MCAssetsObject;

            var curDir = data.GetSourceAbsFileName();
            var tagDir = data.GetTargetAbsFileName();
            // client
            obj.ClientLinks.SaveFiles(tagDir, ECSType.Client);
            // server
            obj.ServerLinks.SaveFiles(tagDir, ECSType.Server);

            return true;
        }
    }
}
