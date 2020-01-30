using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EditorCommon.Assets;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using EngineNS.GamePlay.Component.AI;
using Macross;
using Macross.ResourceInfos;

namespace McBehaviorTreeEditor.ResourceInfos
{
    public class McBTResourceCreateData : ResourceCreateData
    {
        public RName CenterDataTypeName { get; set; } = RName.EmptyName;
    }

    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.McBehaviorTree, ResourceExts = new string[] { ".macross" })]
    public class McBehaviorTreeResourceInfo : Macross.ResourceInfos.MacrossResourceInfo, 
                                       EditorCommon.Resources.IResourceInfoEditor, 
                                       EditorCommon.Resources.IResourceInfoCreateEmpty,
                                       EditorCommon.Resources.IResourceInfoCustomCreateDialog,
                                       EditorCommon.Resources.IResourceInfoCreateComponent
    {
        [EngineNS.Rtti.MetaData]
        public RName CenterDataTypeName { get; set; } = RName.EmptyName;
        [EngineNS.Rtti.MetaData]
        public string PreViewMesh { get; set; } = "";
        public override string ResourceTypeName => "McBehaviorTree";
        public override string EditorTypeName => "McBehaviorTreeEditor";
        public override string CreateMenuPath => "Macross/McBehaviorTree Class";
        public override bool IsBaseResource => false;
        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/BehaviorTree_64x.png", UriKind.Absolute));
        public override async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var result = new McBehaviorTreeResourceInfo();

            var data = createData as McBTResourceCreateData;
            result.CenterDataTypeName = data.CenterDataTypeName;
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
            result.ResourceType = EngineNS.Editor.Editor_RNameTypeAttribute.Macross;

            // 创建时走一遍编译，保证当前Macross能够取到this类型
            var csType = ECSType.Client;
            var codeGenerator = new CodeGenerator();
            var ctrl = new McBTMacrossLinkControl();
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
        public override ICustomCreateDialog GetCustomCreateDialogWindow()
        {
            var retVal = new CreateMcBehaviorTreeWindow();
            retVal.HostResourceInfo = this;
            return retVal;
        }
        public override async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }
        #region ICreateComponent
        public async Task<GComponent> CreateComponent(IComponentContainer componentContainer)
        {
            GActor hostActor = null;
            IComponentContainer hostContainer = null;
            if (componentContainer is GActor)
            {
                hostActor = componentContainer as GActor;
                hostContainer = componentContainer;
            }
            else if (componentContainer is GComponent)
            {
                hostActor = (componentContainer as GComponent).Host;
                hostContainer = componentContainer;
            }
            var rc = CEngine.Instance.RenderContext;
            var comp = new GBehaviorTreeComponent();
            var init = new GBehaviorTreeComponentInitializer();
            await comp.SetInitializer(rc, hostActor, hostContainer, init);
            comp.SpecialName = ResourceName.PureName();
            comp.ComponentMacross = ResourceName;
            return comp;
        }

        #endregion ICreateComponent

        public override async Task<bool> AssetsOption_LoadResourceOverride(AssetsPakage.LoadResourceData data)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        public override Task<bool> AssetsOption_SaveResourceOverride(AssetsPakage.SaveResourceData data)
        {
            throw new InvalidOperationException();
        }
    }
}
