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
using Macross.ResourceInfos;

namespace McLogicAnimationEditor.ResourceInfos
{
    public class McLAResourceCreateData : ResourceCreateData
    {
        public string SkeletonAsset { get; set; } = "";
        public RName CenterDataTypeName { get; set; } = RName.EmptyName;
        public Type CenterDataType { get; set; } = null;
        public string CenterDataTypeString { get; set; } = "";
    }

    [EngineNS.Rtti.MetaClass]
    [EditorCommon.Resources.ResourceInfoAttribute(ResourceInfoType = EngineNS.Editor.Editor_RNameTypeAttribute.McLogicAnim, ResourceExts = new string[] { ".macross" })]
    public class McLogicAnimationResourceInfo : Macross.ResourceInfos.MacrossResourceInfo, 
                                       EditorCommon.Resources.IResourceInfoEditor, 
                                       EditorCommon.Resources.IResourceInfoCreateEmpty,
                                       EditorCommon.Resources.IResourceInfoCustomCreateDialog,
                                       IResourceInfoPreviewForEditor,
                                       EditorCommon.Resources.IResourceInfoCreateComponent
    {
        [EngineNS.Rtti.MetaData]
        public string SkeletonAsset { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public string PreViewMesh { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public RName CenterDataTypeName { get; set; } = RName.EmptyName;
        public override string ResourceTypeName => "McLogicAnimation";
        public override string EditorTypeName => "McLogicAnimationEditor";
        public override string CreateMenuPath => "Macross/McLogicAnimation Class";
        public override bool IsBaseResource => false;
        public override ImageSource ResourceIcon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/AnimBlueprint_64x.png", UriKind.Absolute));
        public override async Task<ResourceInfo> CreateEmptyResource(string absFolder, string rootFolder, IResourceCreateData createData)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var result = new McLogicAnimationResourceInfo();

            var data = createData as McLAResourceCreateData;
            result.SkeletonAsset = data.SkeletonAsset;
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
            var ctrl = new MCLAMacrossLinkControl();
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
            var retVal = new CreateMcLogicAnimationWindow();
            retVal.HostResourceInfo = this;
            return retVal;
        }
        public override async System.Threading.Tasks.Task OpenEditor()
        {
            await EditorCommon.Program.OpenEditor(new EditorCommon.Resources.ResourceEditorContext(EditorTypeName, this));
        }
        #region ICreateComponent
        public async Task<EngineNS.GamePlay.Component.GComponent> CreateComponent(EngineNS.GamePlay.Component.IComponentContainer componentContainer)
        {
            EngineNS.GamePlay.Actor.GActor hostActor = null;
            EngineNS.GamePlay.Component.IComponentContainer hostContainer = null;
            if (componentContainer is EngineNS.GamePlay.Actor.GActor)
            {
                hostActor = componentContainer as EngineNS.GamePlay.Actor.GActor;
                hostContainer = componentContainer;
            }
            else if (componentContainer is EngineNS.GamePlay.Component.GComponent)
            {
                hostActor = (componentContainer as EngineNS.GamePlay.Component.GComponent).Host;
                hostContainer = componentContainer;
            }
            var rc = CEngine.Instance.RenderContext;
            var comp = new EngineNS.GamePlay.Component.GLogicAnimationComponent();
            var init = new EngineNS.GamePlay.Component.GLogicAnimationComponentInitializer();
            await comp.SetInitializer(rc, hostActor, hostContainer, init);
            comp.SpecialName = ResourceName.PureName();
            comp.ComponentMacross= ResourceName;
            return comp;
        }

        #endregion ICreateComponent

        public override Task<bool> AssetsOption_LoadResourceOverride(EditorCommon.Assets.AssetsPakage.LoadResourceData data)
        {
            throw new InvalidOperationException();
        }
        public override Task<bool> AssetsOption_SaveResourceOverride(EditorCommon.Assets.AssetsPakage.SaveResourceData data)
        {
            throw new InvalidOperationException();
        }
    }
}
