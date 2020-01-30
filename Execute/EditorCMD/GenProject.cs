using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace EditorCMD
{
    public class CompileConfig
    {
        public string Name;
        public List<string> Definitions = new List<string>();
        public List<string> IncludeDirs = new List<string>();
        public List<string> LinkDirs = new List<string>();
        public List<string> LinkLibs = new List<string>();

        public void Merge(CompileConfig cfg)
        {
            foreach(var i in cfg.Definitions)
            {
                if (Definitions.Contains(i))
                    continue;
                Definitions.Add(i);
            }
            foreach (var i in cfg.IncludeDirs)
            {
                if (IncludeDirs.Contains(i))
                    continue;
                IncludeDirs.Add(i);
            }
            foreach (var i in cfg.LinkDirs)
            {
                if (LinkDirs.Contains(i))
                    continue;
                LinkDirs.Add(i);
            }
            foreach (var i in cfg.LinkLibs)
            {
                if (LinkLibs.Contains(i))
                    continue;
                LinkLibs.Add(i);
            }
        }
    }

    class NativeModule
    {
        public string Name;
        public Dictionary<string, CompileConfig> Configs = new Dictionary<string, CompileConfig>();
        
        public bool LoadModuleInfo(string dir, string file)
        {
            Name = file;
            Configs.Clear();
            var xml = EngineNS.IO.XmlHolder.LoadXML(dir + file);
            foreach(var i in xml.RootNode.GetNodes())
            {
                var attr = i.FindAttrib("Name");
                if(attr==null)
                {
                    continue;
                }
                
                var cfg = new CompileConfig();
                Configs.Add(attr.Value, cfg);

                var node = i.FindNode("Defines");
                if (node != null)
                {
                    foreach (var j in node.GetNodes())
                    {
                        cfg.Definitions.Add(j.Name);
                    }
                }

                node = i.FindNode("Includes");
                if (node != null)
                {
                    foreach (var j in node.GetNodes())
                    {
                        attr = j.FindAttrib("Dir");
                        if (attr == null)
                            continue;
                        
                        cfg.IncludeDirs.Add(attr.Value.ToLower());
                    }
                }
                node = i.FindNode("Links");
                if (node != null)
                {
                    attr = node.FindAttrib("Libs");
                    if(attr!=null)
                    {
                        var segs = attr.Value.Split(';');
                        foreach(var j in segs)
                        {
                            if (string.IsNullOrEmpty(j))
                                continue;
                            if(cfg.LinkLibs.Contains(j.ToLower())==false)
                            {
                                cfg.LinkLibs.Add(j.ToLower());
                            }
                        }
                    }
                    foreach (var j in node.GetNodes())
                    {
                        attr = j.FindAttrib("Dir");
                        if (attr == null)
                            continue;

                        cfg.LinkDirs.Add(attr.Value.ToLower());
                    }
                }
            }
            return true;
        }
        public CompileConfig FindConfig(string name)
        {
            CompileConfig result;
            if (Configs.TryGetValue(name, out result))
                return result;
            return null;
        }
    }

    enum ENativePlatformType
    {
        Windows,
        Android,
        IOS,
    }

    class NativePlatform
    {
        public string ProjectFile;
        public List<string> Definitions = new List<string>();
        public Dictionary<string, CompileConfig> Configs = new Dictionary<string, CompileConfig>();
        public CompileConfig CreateConfig(string name)
        {
            var result = new CompileConfig();
            foreach(var i in Definitions)
            {
                if(result.Definitions.Contains(i)==false)
                {
                    result.Definitions.Add(i);
                }
            }
            result.Name = name;
            Configs[name] = result;
            return result;
        }

        public bool LoadInfo(EngineNS.IO.XmlNode node)
        {
            var attr = node.FindAttrib("ProjFile");
            if (attr == null)
                return false;
            ProjectFile = attr.Value;

            Definitions.Clear();
            foreach (var i in node.GetNodes())
            {
                if (Definitions.Contains(i.Name)==false)
                {
                    Definitions.Add(i.Name);
                }
            }
            return true;
        }
    }
    class GenProject
    {
        public readonly static string[] WindowsProjectConfigList = new string[]
                    {
                        "Debug|x64",
                        "Release|x64",
                    };
        public readonly static string[] AndroidProjectConfigList = new string[]
                    {
                        "Debug|ARM",
                        "Release|ARM",
                        "Debug|ARM64",
                        "Release|ARM64",
                    };
        public readonly static string ProjToRoot = "../../";
        public static GenProject Instance = new GenProject();
        public string ProjectDirectory = null;
        public void Command(string[] args)
        {
            var proFile = args[1];
            
            var xml = EngineNS.IO.XmlHolder.LoadXML(proFile);

            string ProjectDirectory = EngineNS.CEngine.Instance.FileManager.GetPathFromFullName(proFile);

            if (xml.RootNode.Name != "NativeProject")
            {
                System.Console.WriteLine($"{proFile} is not a native project");
                return;
            }

            var modules = xml.RootNode.FindNode("Modules");
            if (modules == null)
            {
                System.Console.WriteLine($"{proFile} don't have modules");
                return;
            }

            var moduleList = new List<NativeModule>();
            foreach(var i in modules.GetNodes())
            {
                var module = i.FindAttrib("Module");
                if(module==null || System.IO.File.Exists(ProjectDirectory + module.Value)==false)
                {
                    System.Console.WriteLine($"[{module.Name}] is not found");
                    continue;
                }

                var t = new NativeModule();
                if (t.LoadModuleInfo(ProjectDirectory, module.Value))
                {
                    moduleList.Add(t);
                }
                else
                {
                    System.Console.WriteLine($"[{module.Name}] load failed");
                }
            }

            var winNode = GetPlatform("Windows", xml);
            if (winNode != null)
            {
                var pltf = new NativePlatform();
                if (pltf.LoadInfo(winNode) == false)
                {
                    System.Console.WriteLine($"Windows Platform project load failed");
                }
                else
                {
                    
                    foreach(var i in WindowsProjectConfigList)
                    {
                        var cfg = pltf.CreateConfig(i);
                        foreach (var j in moduleList)
                        {
                            var cfg_module = j.FindConfig(i);
                            if(cfg_module!=null)
                                cfg.Merge(cfg_module);
                        }
                    }
                }

                VCProject winProj = new VCProject();
                winProj.PlatformType = ENativePlatformType.Windows;
                winProj.LoadProject(ProjectDirectory, "Execute/Core.Windows/Core.Windows.vcxproj", WindowsProjectConfigList);
            }

            var droidNode = GetPlatform("Android", xml);
            if (droidNode != null)
            {
                var pltf = new NativePlatform();
                if (pltf.LoadInfo(droidNode) == false)
                {
                    System.Console.WriteLine($"Android Platform project load failed");
                }
                else
                {

                    foreach (var i in AndroidProjectConfigList)
                    {
                        var cfg = pltf.CreateConfig(i);
                        foreach (var j in moduleList)
                        {
                            var cfg_module = j.FindConfig(i);
                            if (cfg_module != null)
                                cfg.Merge(cfg_module);
                        }
                    }
                }

                VCProject droidProj = new VCProject();
                droidProj.PlatformType = ENativePlatformType.Android;
                droidProj.LoadProject(ProjectDirectory, "Execute/Core.Droid/Core.Droid.vcxproj", AndroidProjectConfigList);

                //VCProject iosProj = new VCProject();
                //iosProj.LoadProject(ProjectDirectory, "Execute/Core.IOS/Core.IOS.vcxproj");
            }
        }
        private EngineNS.IO.XmlNode GetPlatform(string name, EngineNS.IO.XmlHolder xml)
        {
            var node = xml.RootNode.FindNode("Platform");
            if (node == null)
                return null;
            return node.FindNode(name);
        }
    }

    class VCProject
    {
        public ENativePlatformType PlatformType = ENativePlatformType.Windows;
        public List<NativeModule> Modules
        {
            get;
        } = new List<NativeModule>();
        public NativeModule FindModule(string name)
        {
            foreach(var i in Modules)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public NativePlatform PlatformProject
        {
            get;
        } = new NativePlatform();
        public EngineNS.IO.XmlHolder LoadProject(string ProjectDirectory, string projFileName, string[] cfgList)
        {
            var xml = EngineNS.IO.XmlHolder.LoadXML(ProjectDirectory + projFileName);
            var moduleNames = GetModules(xml.RootNode);
            foreach(var i in moduleNames)
            {
                var tmp = new NativeModule();
                tmp.Name = i;
                Modules.Add(tmp);
            }
            foreach (var i in cfgList)
            {
                var cfg = new CompileConfig();
                cfg.Name = i;
                GetCompileConfig(xml.RootNode, cfg);
                PlatformProject.Configs.Add(i, cfg);
            }   
            return xml;
        }
        private List<string> GetModules(EngineNS.IO.XmlNode node)
        {
            List<string> result = new List<string>();
            node.FindNodes("ImportGroup", (EngineNS.IO.XmlNode nd, ref bool bCancel) => 
            {
                var attr = nd.FindAttrib("Label");
                if (attr != null)
                {
                    if(attr.Value == "Shared")
                    {
                        bCancel = true;
                        foreach(var i in nd.GetNodes())
                        {
                            if("Import" == i.Name)
                            {
                                var projAttr = i.FindAttrib("Project");
                                if(projAttr!=null)
                                {
                                    var strPath = projAttr.Value.Substring(GenProject.ProjToRoot.Length);
                                    strPath = strPath.Replace(".vcxitems", ".nmodule");
                                    strPath = strPath.Replace('\\', '/');
                                    result.Add(strPath);
                                }
                            }
                        }
                        return true;
                    }
                }
                return false;
            });

            return result;
        }
        private void GetCompileConfig(EngineNS.IO.XmlNode node, CompileConfig cfg)
        {
            var condiName = $"'$(Configuration)|$(Platform)'=='{cfg.Name}'";
            node.FindNodes("ItemDefinitionGroup", (EngineNS.IO.XmlNode nd, ref bool bCancel) =>
            {
                var attr = nd.FindAttrib("Condition");
                if (attr != null)
                {
                    if (attr.Value == condiName)
                    {
                        this.FillClCompile(nd.FindNode("ClCompile"), cfg);
                        this.FillLink(nd.FindNode("Link"), cfg);
                        bCancel = true;
                        return true;
                    }
                }
                return false;
            });
        }
        private void FillClCompile(EngineNS.IO.XmlNode node, CompileConfig cfg)
        {
            var inc = node.FindNode("AdditionalIncludeDirectories");
            if(inc!=null)
            {
                var segs = inc.Value.Trim().Split(';');
                foreach(var i in segs)
                {
                    var dir = i.Substring(GenProject.ProjToRoot.Length).Replace('\\', '/');
                    if(cfg.IncludeDirs.Contains(dir)==false)
                    {
                        cfg.IncludeDirs.Add(dir);
                    }
                }
            }
            var defines = node.FindNode("PreprocessorDefinitions");
            if (defines != null)
            {
                var segs = defines.Value.Trim().Split(';');
                foreach (var i in segs)
                {
                    if (cfg.Definitions.Contains(i) == false)
                    {
                        cfg.Definitions.Add(i);
                    }
                }
            }
        }
        private void FillLink(EngineNS.IO.XmlNode node, CompileConfig cfg)
        {
            var libDir = node.FindNode("AdditionalLibraryDirectories");
            if (libDir != null)
            {
                var segs = libDir.Value.Trim().Split(';');
                foreach (var i in segs)
                {
                    var dir = i.Substring(GenProject.ProjToRoot.Length).Replace('\\', '/');
                    if (cfg.LinkDirs.Contains(dir) == false)
                    {
                        cfg.LinkDirs.Add(dir);
                    }
                }
            }
            string libsName = "AdditionalDependencies";
            switch(PlatformType)
            {
                case ENativePlatformType.Windows:
                    libsName = "AdditionalDependencies";
                    break;
                case ENativePlatformType.Android:
                    libsName = "LibraryDependencies";
                    break;
            }
            var libs = node.FindNode(libsName);
            if (libs != null)
            {
                var segs = libs.Value.Trim().Split(';');
                foreach (var i in segs)
                {
                    if (cfg.LinkLibs.Contains(i) == false)
                    {
                        cfg.LinkLibs.Add(i);
                    }
                }
            }
        }
    }
}
