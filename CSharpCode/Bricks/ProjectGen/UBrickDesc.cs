using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.ProjectGen
{
    public class UNugetDesc
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }
    public class UCompileConfig
    {
        public enum ECpuArch
        {
            AnyCPU,
            X86,
            X64,
            Arm32,
            Arm64,
        }
        public string GetCpuArchString()
        {
            switch (Arch)
            {
                case ECpuArch.AnyCPU:
                    return "AnyCPU";
                case ECpuArch.X86:
                    return "x86";
                case ECpuArch.X64:
                    return "arm32";
                case ECpuArch.Arm32:
                    return "x86";
                case ECpuArch.Arm64:
                    return "arm64";
                default:
                    return "AnyCPU";
            }
        }
        public string Name { get; set; }
        public string OutputDir { get; set; } = "../../binaries/";
        public ECpuArch Arch { get; set; } = ECpuArch.AnyCPU;
        public List<string> Defines { get; set; } = new List<string>();
    }
    public class UBrickDesc
    {
        public string Name { get; set; }
        public string Version { get; set; } = "1.0.0.0";
        public string Description { get; set; }
        public string License { get; set; }
        public string Path { get; set; }
        public string FullName
        {
            get
            {
                return $"{Path}/{Name}.brick";
            }
        }
        public EPlatformType SupportPlatforms { get; set; } = EPlatformType.PLTF_Windows | EPlatformType.PLTF_Android | EPlatformType.PLTF_AppleIOS;
        public bool IsServerRun { get; set; } = true;
        public bool AllowUnsafe { get; set; } = true;
        public List<UCompileConfig> Configs { get; set; } = new List<UCompileConfig>();
        public List<string> SharedProjects { get; set; } = new List<string>();
        public List<UNugetDesc> Nugets { get; set; } = new List<UNugetDesc>();
        public List<string> DllModules { get; set; } = new List<string>();

        public bool Checked = false;
    }

    public class UProjectDesc
    {
        public string Name { get; set; }
        public EPlatformType Platform { get; set; } = EPlatformType.PLTF_Windows;
        public List<UBrickDesc> Bricks { get; set; } = new List<UBrickDesc>();
        public List<UCompileConfig> Configs { get; set; } = new List<UCompileConfig>();
        public List<string> PrebuildEvents { get; set; } = new List<string>();
        public void Build()
        {
            Nugets.Clear();
            foreach (var i in Bricks)
            {
                foreach (var j in i.Nugets)
                {
                    bool hasNuget = false;
                    foreach (var k in Nugets)
                    {
                        if(k.Name == j.Name)
                        {
                            hasNuget = true;
                            break;
                        }
                    }
                    if (hasNuget == false)
                    {
                        Nugets.Add(j);
                    }
                }
                foreach (var j in i.DllModules)
                {
                    if (DllModules.Contains(j))
                        continue;
                    DllModules.Add(j);
                }
            }
        }
        public List<UNugetDesc> Nugets { get; set; } = new List<UNugetDesc>();
        public List<string> DllModules { get; set; } = new List<string>();
        public bool IsAllowUnsafe()
        {
            foreach (var i in Bricks)
            {
                if (i.AllowUnsafe)
                    return true;
            }
            return false;
        }

        public void SaveVSProject(string file)
        {
            //file + ".Window.csproj";
            //file + ".Console.csproj";

            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            var path = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource);
            var proj = path + "Module/Template.csproj";
            xml.Load(proj);
            var root = xml.LastChild;
            foreach (var i in Configs)
            {
                var group = xml.CreateElement($"PropertyGroup", xml.NamespaceURI);
                var attr = xml.CreateAttribute($"Condition");
                attr.Value = $"'$(Configuration)|$(Platform)'=='{i.Name}|{i.GetCpuArchString()}'";
                group.Attributes.Append(attr);
                {
                    var node = xml.CreateElement($"OutputPath", xml.NamespaceURI);
                    node.InnerText = i.OutputDir;
                    group.AppendChild(node);

                    node = xml.CreateElement($"AllowUnsafeBlocks", xml.NamespaceURI);
                    if (IsAllowUnsafe())
                        node.InnerText = "true";
                    else
                        node.InnerText = "false";
                    group.AppendChild(node);

                    node = xml.CreateElement($"PlatformTarget", xml.NamespaceURI);
                    node.InnerText = i.GetCpuArchString();
                    group.AppendChild(node);

                    node = xml.CreateElement($"DefineConstants", xml.NamespaceURI);
                    string defs = "";
                    foreach (var j in i.Defines)
                    {
                        defs += j + ";";
                    }
                    if (defs.EndsWith(";"))
                        defs = defs.Substring(0, defs.Length - 1);
                    node.InnerText = defs;
                    group.AppendChild(node);
                }
                root.AppendChild(group);
            }

            foreach (var i in Bricks)
            {
                var prop = xml.CreateElement($"Import", xml.NamespaceURI);
                var attr = xml.CreateAttribute($"Project");
                attr.Value = i.FullName;
                prop.Attributes.Append(attr);

                attr = xml.CreateAttribute($"DisplayName");
                attr.Value = "Shared";
                prop.Attributes.Append(attr);
                root.AppendChild(prop);
            }

            {
                var group = xml.CreateElement($"ItemGroup", xml.NamespaceURI);
                root.AppendChild(group);
                foreach (var i in Nugets)
                {
                    var prop = xml.CreateElement($"PackageReference", xml.NamespaceURI);
                    
                    var attr = xml.CreateAttribute($"Include");
                    attr.Value = i.Name;
                    prop.Attributes.Append(attr);

                    attr = xml.CreateAttribute($"Version");
                    attr.Value = i.Version;
                    prop.Attributes.Append(attr);

                    group.AppendChild(prop);
                }
            }

            {
                var group = xml.CreateElement($"ItemGroup", xml.NamespaceURI);
                root.AppendChild(group);
                foreach (var i in DllModules)
                {
                    var prop = xml.CreateElement($"Reference", xml.NamespaceURI);
                    var attr = xml.CreateAttribute($"Include");
                    attr.Value = i;
                    prop.Attributes.Append(attr);
                    group.AppendChild(prop);

                    var hintPath = xml.CreateElement($"HintPath", xml.NamespaceURI);
                    hintPath.InnerText = i;
                    prop.AppendChild(hintPath);
                }
            }
            var text = IO.TtFileManager.GetXmlText(xml);
        }
        public UBrickDesc FindBrick(string fullname)
        {
            foreach (var i in Bricks)
            {
                if (i.FullName == fullname)
                    return i;
            }
            return null;
        }
        public bool IsReferProj(string fullname)
        {
            foreach (var i in Bricks)
            {
                foreach (var j in i.SharedProjects)
                {
                    if (j == fullname)
                        return true;
                }
            }
            return false;
        }
        public void AddBrick(UBrickDesc brx, UBrickManager mgr)
        {
            if (FindBrick(brx.FullName) == null)
            {
                brx.Checked = true;
                Bricks.Add(brx);

                foreach (var j in brx.SharedProjects)
                {
                    var refBrx = mgr.FindBrick(j);
                    if (refBrx != null)
                        AddBrick(refBrx, mgr);
                }
            }
        }
        public void RemoveBrick(string fullname)
        {
            for (int i = 0; i < Bricks.Count; i++)
            {
                if (Bricks[i].FullName == fullname)
                {
                    Bricks[i].Checked = false;
                    Bricks.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
