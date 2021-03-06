﻿using System;
using System.Collections.Generic;

namespace THeaderTools
{
    class Program
    {
        static void Main(string[] args)
        {
            var proj = FindArgument(args, "vcxproj=");
            if (proj == null)
            {
                Console.WriteLine("must input vcxproj");
                return;
            }
            proj = proj.Substring("vcxproj=".Length);
            proj = proj.Replace('\\', '/');

            var path = proj.Substring(0, proj.LastIndexOf("/")+1);

            var genDir = FindArgument(args, "gen_dir=");
            genDir = genDir.Substring("gen_dir=".Length);

            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(proj);
            var root = myXmlDoc.LastChild;

            System.Xml.XmlNode sharedNode = null;
            foreach (System.Xml.XmlNode node in root.ChildNodes)
            {
                if(node.Name== "ImportGroup" && node.Attributes["Label"]!=null && node.Attributes["Label"].Value=="Shared")
                {
                    sharedNode = node;
                    break;
                }
            }
            if (sharedNode == null)
            {
                Console.WriteLine($"no shared projects in {proj}");
                return;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (System.Xml.XmlNode node in sharedNode.ChildNodes)
            {
                if (node.Name != "Import")
                    continue;
                var attr = node.Attributes["Project"];
                if (attr == null)
                    continue;

                var sharedProjFile = attr.Value.Replace('\\', '/');
                sharedProjFile = path + sharedProjFile;

                bool error;
                sharedProjFile = NormalizePath(sharedProjFile, out error);
                if (error)
                {
                    Console.WriteLine($"header file {sharedProjFile} is error path");
                }

                System.Xml.XmlDocument sharedProjXmlDoc = new System.Xml.XmlDocument();
                sharedProjXmlDoc.Load(sharedProjFile);
                var spjPath = sharedProjFile.Substring(0, sharedProjFile.LastIndexOf("/") + 1);
                //var spjRoot = sharedProjXmlDoc.SelectSingleNode("Project");
                var spjRoot = sharedProjXmlDoc.LastChild;
                foreach (System.Xml.XmlNode sn in spjRoot.ChildNodes)
                {
                    if (sn.Name == "ItemGroup")
                    {
                        if(CollectInclude(sn, spjPath, headers))
                        {
                            break;
                        }
                    }
                }
            }
            if (null != FindArgument(args, "gen_csharp_internal"))
                CodeGenerator.Instance.GenInternalClass = true;
            bool bGenPInvoke = false;
            if (null != FindArgument(args, "gen_cpp_pinvoke"))
                bGenPInvoke = true;
            bool bGenCSharp = false;
            if (FindArgument(args, "gen_csharpwrapper") != null)
            {
                bGenPInvoke = true;
                bGenCSharp = true;
            }
            string moduleNS = FindArgument(args, "module_name=");
            if(moduleNS!=null)
            {//example: module_name=\"libCore.so\"
                moduleNS = moduleNS.Substring("module_name=".Length);
                CodeGenerator.Instance.GenImportModuleNC = moduleNS;
            }
            string pch = FindArgument(args, "pch=");
            if (pch != null)
            {//example: module_name=\"libCore.so\"
                pch = pch.Substring("pch=".Length);
                CodeGenerator.Instance.IncludePCH = pch;
            }
            string type2type = FindArgument(args, "type2type=");
            if (type2type != null)
            {//example: module_name=\"libCore.so\"
                type2type = type2type.Substring("type2type=".Length);
                CodeGenerator.Instance.LoadType2TypeMapper(type2type);
            }
            Console.WriteLine("THeaderTools.exe ScanHeader ...");
            CodeGenerator codeManager = CodeGenerator.Instance;
            codeManager.Reset(genDir);
            foreach (var i in headers)
            {
                var headScanner = new CppHeaderScanner();
                headScanner.ScanHeader(i.Value, codeManager.ClassCollector, codeManager.EnumCollector, codeManager.CBCollector);
            }
            Console.WriteLine("THeaderTools.exe GenCode ...");
            codeManager.GenCode(genDir, bGenPInvoke);
            if (bGenCSharp)
            {
                codeManager.GenCodeCSharp(genDir);
            }

            Console.WriteLine("THeaderTools.exe GenSharedProject ...");
            codeManager.MakeSharedProjectCpp();
        }
        static bool CollectInclude(System.Xml.XmlNode sn, string spjPath, Dictionary<string,string> headers)
        {
            bool bFinded = false;
            foreach (System.Xml.XmlNode i in sn.ChildNodes)
            {
                if (i.Name == "ClInclude")
                {
                    var attr = i.Attributes["Include"];
                    if (attr != null)
                    {
                        if (attr.Value.EndsWith("TypeUtility.h"))
                            continue;
                        var hf = attr.Value.Replace("$(MSBuildThisFileDirectory)", spjPath);
                        bool error;
                        hf = NormalizePath(hf, out error);
                        if(error)
                        {
                            Console.WriteLine($"header file {hf} is error path");
                        }
                        headers[hf] = hf;
                        bFinded = true;
                    }
                }
            }
            return bFinded;
        }
        static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                    return i;
            }
            return null;
        }
        static string NormalizePath(string path, out bool error)
        {
            error = false;

            path = path.Replace("\\", "/");

            path = path.Replace("../", "$/");

            path = path.Replace("./", "");

            //path = path.ToLower();

            int UpDirLength = "$/".Length;
            int startPos = path.LastIndexOf("$/");
            while (startPos >= 0)
            {
                int rmvNum = 1;
                var head = path.Substring(0, startPos);
                var tail = path.Substring(startPos + UpDirLength);
                while (head.Length > UpDirLength && head.EndsWith("$/"))
                {
                    rmvNum++;
                    head = head.Substring(0, head.Length - "$/".Length);
                }
                if (head.EndsWith('/'))
                    head = head.Substring(0, head.Length - 1);
                int discardPos = -1;
                for (int i = 0; i < rmvNum; i++)
                {
                    discardPos = head.LastIndexOf("/");
                    if (discardPos < 0)
                    {
                        error = true;
                        return null;
                    }
                    else
                    {
                        head = head.Substring(0, discardPos);
                    }
                }
                path = head + '/' + tail;

                startPos = path.LastIndexOf("$/");
            }

            return path;
        }
    }
}
