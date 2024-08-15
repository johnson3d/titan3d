using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EngineNS.Bricks.CodeBuilder
{
    [Rtti.Meta]
    public partial class UMacrossConfig
    {
        public List<string> GameReferenceAssemblies = new List<string>();
        public List<CodeCompiler.ProjectConfig> GenProjects = new List<CodeCompiler.ProjectConfig>();
    }

    public class UMacrossManager : UModule<TtEngine>
    {
        public override int GetOrder()
        {
            return 10;
        }
        bool mNeedRegenGameProject = false;

        public UMacrossConfig Config { get; set; }

        public override async Task<bool> Initialize(TtEngine host)
        {
            var cfgFile = host.FileManager.GetRoot(IO.TtFileManager.ERootDir.Editor) + "MacrossConfig.cfg";
            Config = IO.TtFileManager.LoadXmlToObject<UMacrossConfig>(cfgFile);
            if(Config == null)
            {
                Config = new UMacrossConfig();
                Config.GameReferenceAssemblies = new List<string>()
                {
                    IO.TtFileManager.GetRelativePath(
                        IO.TtFileManager.GetBaseDirectory(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameProject),
                        TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Execute)) + "Engine.Window.dll",
                };
                Config.GenProjects = new List<CodeCompiler.ProjectConfig>()
                {
                    new CodeCompiler.ProjectConfig()
                    {
                        ProjectType = CodeCompiler.ProjectConfig.enProjectType.DefaultGame,
                        ProjectFile = IO.TtFileManager.GetRelativePath(
                            IO.TtFileManager.GetBaseDirectory(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameProject),
                            TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game)) + "MacrossGenCSharp.shproj",
                        ProjectGuid = Guid.NewGuid(),
                        MinVSVersion = new Version(14, 0),
                    }
                };
            }

            return await base.Initialize(host);
        }

        public void GenerateProjects()
        {
            GenerateSharedProjects();
            GenerateGameProject();
        }

        void GenerateSharedProjects()
        {
            // 根据配置生成多个共享工程
            var folder = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Game);
            var codeFiles = new HashSet<string>(IO.TtFileManager.GetFiles(folder, "*.cs"));
            for(int i=0; i<Config.GenProjects.Count; ++i)
            {
                var projConfig = Config.GenProjects[i];
                switch(projConfig.ProjectType)
                {
                    case CodeCompiler.ProjectConfig.enProjectType.DefaultGame:
                        {
                            var projFile = projConfig.ProjectFile;
                            var config = new CodeCompiler.ProjectConfig();
                            projConfig.CodeFiles = new List<string>(codeFiles);
                            CodeCompiler.ProjectGenerator.GenerateSharedProject(projConfig);
                        }
                        break;
                    case CodeCompiler.ProjectConfig.enProjectType.Custom:
                        {
                            CodeCompiler.ProjectGenerator.GenerateSharedProject(projConfig);
                        }
                        break;
                    default:
                        throw new InvalidOperationException("未实现");
            }
        }
    }

        void GenerateGameProject()
        {
            if (!mNeedRegenGameProject)
                return;

            var projFile = TtEngine.Instance.EditorInstance.Config.GameProject;
            var projFolder = IO.TtFileManager.GetParentPathName(projFile);
            var outputPath = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Execute), projFile);
            string nsUrl = null;// "http://schemas.microsoft.com/developer/msbuild/2003";

            var xml = new XmlDocument();
            var root = xml.CreateElement("Project", nsUrl);
            root.SetAttribute("Sdk", "Microsoft.NET.Sdk");
            xml.AppendChild(root);

            var projGroup = xml.CreateElement("PropertyGroup", nsUrl);
            projGroup.SetAttribute("Condition", "'$(Configuration)|$(Platform)'=='Debug|AnyCPU'");
            var outPathElem = xml.CreateElement("OutputPath", nsUrl);
            outPathElem.SetAttribute("OutputPath", outputPath);
            projGroup.AppendChild(outPathElem);
            root.AppendChild(projGroup);

            projGroup = xml.CreateElement("PropertyGroup", nsUrl);
            projGroup.SetAttribute("Condition", "'$(Configuration)|$(Platform)'=='Release|AnyCPU'");
            outPathElem = xml.CreateElement("OutputPath", nsUrl);
            outPathElem.SetAttribute("OutputPath", outputPath);
            projGroup.AppendChild(outPathElem);
            root.AppendChild(projGroup);

            var itemGroup = xml.CreateElement("ItemGroup", nsUrl);
            //var projRefElem = xml.CreateElement("ProjectReference", nsUrl);
            //projRefElem.SetAttribute("Include", @"..\Engine.Window\Engine.Window.csproj");
            //itemsGroup.AppendChild(projRefElem);
            for(int i=0; i<Config.GameReferenceAssemblies.Count; ++i)
            {
                var refElem = xml.CreateElement("Reference", nsUrl);
                var hintPath = xml.CreateElement("", nsUrl);
                hintPath.InnerText = Config.GameReferenceAssemblies[i];
                refElem.AppendChild(hintPath);
                itemGroup.AppendChild(refElem);
            }
            root.AppendChild(itemGroup);
        }

        public void ClearGameProjectTemplateBuildFiles()
        {
            var projFile = TtEngine.Instance.EditorInstance.Config.GameProject;
            var projFolder = IO.TtFileManager.GetParentPathName(projFile);
            var objFolder = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + projFolder.TrimEnd('/') + "/obj";
            if(IO.TtFileManager.DirectoryExists(objFolder))
            {
                IO.TtFileManager.DeleteDirectory(objFolder);
            }
        }

        public void CompileCode()
        {

        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        public Bricks.CodeBuilder.UMacrossManager MacrossManager { get; } = new Bricks.CodeBuilder.UMacrossManager();
    }
}