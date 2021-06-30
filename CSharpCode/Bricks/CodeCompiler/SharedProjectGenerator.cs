using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EngineNS.CodeCompiler
{
    [Rtti.Meta]
    public partial class ProjectConfig
    {
        public enum enProjectType
        {
            DefaultGame,
            DefaultEditor,
            Custom,
        }
        [Rtti.Meta]
        public enProjectType ProjectType;
        [Rtti.Meta]
        public Guid ProjectGuid;
        [Rtti.Meta]
        public string ProjectFile;
        [Rtti.Meta]
        public System.Version MinVSVersion;
        [Rtti.Meta]
        public List<string> ReferenceProjects;
        [Rtti.Meta]
        public List<string> CodeFiles;
    }

    public class ProjectGenerator
    {
        public static bool GenerateSharedProject(ProjectConfig config)
        {
            var projFolder = IO.FileManager.GetParentPathName(config.ProjectFile);
            var projName = IO.FileManager.GetPureName(config.ProjectFile);
            var version = "1.0";
            var encode = "utf-8";
            var nsUrl = "http://schemas.microsoft.com/developer/msbuild/2003";
            // shproj
            {
                var xml = new XmlDocument();
                var xmldec = xml.CreateXmlDeclaration(version, encode, null);
                var root = xml.CreateElement("Project", nsUrl);
                xml.AppendChild(root);

                xml.InsertBefore(xmldec, root);

                var projGroup = xml.CreateElement("PropertyGroup", nsUrl);
                var projGuid = xml.CreateElement("ProjectGuid", nsUrl);
                projGuid.InnerText = config.ProjectGuid.ToString();
                projGroup.AppendChild(projGuid);
                var minVSVersion = xml.CreateElement("MinimumVisualStudioVersion", nsUrl);
                minVSVersion.InnerText = "14.0";
                projGroup.AppendChild(minVSVersion);
                root.AppendChild(projGroup);

                var import = xml.CreateElement("Import", nsUrl);
                import.SetAttribute("Project", @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props");
                import.SetAttribute("Condition", @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')");
                root.AppendChild(import);

                import = xml.CreateElement("Import", nsUrl);
                import.SetAttribute("Project", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.Common.Default.props");
                root.AppendChild(import);

                import = xml.CreateElement("Import", nsUrl);
                import.SetAttribute("Project", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.Common.props");
                root.AppendChild(import);

                import = xml.CreateElement("Import", nsUrl);
                import.SetAttribute("Project", $"{projName}.projitems");
                import.SetAttribute("Label", "Shared");
                root.AppendChild(import);

                import = xml.CreateElement("Import", nsUrl);
                import.SetAttribute("Project", @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\CodeSharing\Microsoft.CodeSharing.CSharp.targets");
                root.AppendChild(import);

                xml.Save(config.ProjectFile);
            }

            // projitems
            {
                var xml = new XmlDocument();
                var xmldec = xml.CreateXmlDeclaration(version, encode, null);
                var root = xml.CreateElement("Project", nsUrl);
                xml.AppendChild(root);

                xml.InsertBefore(xmldec, root);

                var proGroup = xml.CreateElement("PropertyGroup", nsUrl);
                var msBuildProjElem = xml.CreateElement("MSBuildAllProjects", nsUrl);
                msBuildProjElem.InnerText = "$(MSBuildAllProjects);$(MSBuildThisFileFullPath)";
                proGroup.AppendChild(msBuildProjElem);
                var hasSharedItemsElem = xml.CreateElement("HasSharedItems", nsUrl);
                hasSharedItemsElem.InnerText = "true";
                proGroup.AppendChild(hasSharedItemsElem);
                var guidElem = xml.CreateElement("SharedGUID", nsUrl);
                guidElem.InnerText = config.ProjectGuid.ToString();
                proGroup.AppendChild(guidElem);
                root.AppendChild(proGroup);

                proGroup = xml.CreateElement("PropertyGroup", nsUrl);
                proGroup.SetAttribute("Label", "Configuration");
                var rootNameSpace = xml.CreateElement("Import_RootNamespace", nsUrl);
                rootNameSpace.InnerText = "MacrossGenCSharp";
                proGroup.AppendChild(rootNameSpace);
                root.AppendChild(proGroup);

                var itemGroup = xml.CreateElement("ItemGroup", nsUrl);
                foreach (var file in config.CodeFiles)
                {
                    var relFile = EngineNS.IO.FileManager.GetRelativePath(projFolder, file);
                    var node = xml.CreateElement("Compile", nsUrl);
                    node.SetAttribute("Include", $"$(MSBuildThisFileDirectory){relFile}");
                    itemGroup.AppendChild(node);
                }
                root.AppendChild(itemGroup);

                var projItemFileName = IO.FileManager.RemoveExtName(config.ProjectFile) + ".projitems";
                xml.Save(projItemFileName);
            }

            return true;
        }
    }
}
