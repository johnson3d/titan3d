using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EngineNS.CodeCompiler
{
    public class ProjectGenerator
    {
        public static bool GenerateSharedProject(string[] codeFiles, string projectFile)
        {
            var xml = new XmlDocument();
            var nsUrl = "http://schemas.microsoft.com/developer/msbuild/2003";
            var root = xml.CreateElement("Project", nsUrl);
            xml.AppendChild(root);

            var xmldec = xml.CreateXmlDeclaration("1.0", "utf-8", null);
            xml.InsertBefore(xmldec, root);

            var proGroup = xml.CreateElement("PropertyGroup", nsUrl);
            var msBuildProjElem = xml.CreateElement("MSBuildAllProjects", nsUrl);
            msBuildProjElem.InnerText = "$(MSBuildAllProjects);$(MSBuildThisFileFullPath)";
            proGroup.AppendChild(msBuildProjElem);
            var hasSharedItemsElem = xml.CreateElement("HasSharedItems", nsUrl);
            hasSharedItemsElem.InnerText = "true";
            proGroup.AppendChild(hasSharedItemsElem);
            var guidElem = xml.CreateElement("SharedGUID", nsUrl);
            guidElem.InnerText = "181355b8-b0ae-47c5-b395-f57033faf3c8";
            proGroup.AppendChild(guidElem);
            root.AppendChild(proGroup);

            proGroup = xml.CreateElement("PropertyGroup", nsUrl);
            proGroup.SetAttribute("Label", "Configuration");
            var rootNameSpace = xml.CreateElement("Import_RootNamespace", nsUrl);
            rootNameSpace.InnerText = "MacrossGenCSharp";
            proGroup.AppendChild(rootNameSpace);
            root.AppendChild(proGroup);

            var itemGroup = xml.CreateElement("ItemGroup", nsUrl);
            var projFolder = IO.FileManager.GetParentPathName(projectFile);
            foreach (var file in codeFiles)
            {
                var relFile = EngineNS.IO.FileManager.GetRelativePath(projFolder, file);
                var node = xml.CreateElement("Compile", nsUrl);
                node.SetAttribute("Include", $"$(MSBuildThisFileDirectory){relFile}");
                itemGroup.AppendChild(node);
            }
            root.AppendChild(itemGroup);

            xml.Save(projectFile);

            return true;
        }
    }
}
