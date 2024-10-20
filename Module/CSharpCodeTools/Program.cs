using System;
using System.Collections.Generic;

namespace CSharpCodeTools
{
    class Program
    {
        public static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                {
                    return i.Substring(startWith.Length);
                }
            }
            return null;
        }
        public static string[] GetArguments(string[] args, string startWith, char split = '+')
        {
            var types = FindArgument(args, startWith);
            if (types != null)
            {
                return types.Split(split);
            }
            return null;
        }
        static void Main(string[] args)
        {
            string DefNewLine = "\r\n";
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Xbox:
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                    DefNewLine = "\r\n";
                    break;
                case PlatformID.Unix:
                    DefNewLine = "\n";
                    break;
                case PlatformID.MacOSX:
                    DefNewLine = "\r";
                    break;
                case PlatformID.Other:
                    DefNewLine = "\n";
                    break;
            }
            var file = args[0].Replace('\\', '/');
            var idx = file.LastIndexOf('/');
            var dir = file.Substring(0, idx);
            var cfg = System.IO.File.ReadAllText(file);
            var segs = cfg.Split(DefNewLine);

            bool workRpc = true;
            bool workAutoSync = true;
            bool workCs2Cpp = true;
            bool workMacross = true;
            var modes = GetArguments(args, "mode=");
            if (modes != null)
            {
                workRpc = false;
                workAutoSync = false;
                workCs2Cpp = false;
                workMacross = false;
                foreach(var i in modes)
                {
                    switch(i)
                    {
                        case "Rpc":
                            workRpc = true;
                            break;
                        case "AutoSync":
                            workAutoSync = true;
                            break;
                        case "Cs2Cpp":
                            workCs2Cpp = true;
                            break;
                        case "Macross":
                            workMacross = true;
                            break;
                    }
                }
            }

            var text = FindArgument(segs, "Include=");
            List<string> includes = new List<string>();
            {
                var inc = text.Split(',');
                Console.WriteLine("Include:");
                foreach (var i in inc)
                {
                    includes.Add(dir + "/" + i);
                    Console.WriteLine(dir + "/" + i);
                }
            }
            text = FindArgument(segs, "Exclude=");
            text = text.Replace("\r", "");            
            List<string> excludes = new List<string>();
            {
                var inc = text.Split(',');
                Console.WriteLine("Exclude:");
                foreach (var i in inc)
                {
                    excludes.Add(dir + "/" + i);
                    Console.WriteLine(dir + "/" + i);
                }
            }

            text = FindArgument(segs, "Target=");
            if (text == null)
            {
                return;
            }
            
            if (workRpc)
            {
                Console.WriteLine("CSharp build event: Rpc");
                string target = dir + "/" + text;
                Console.WriteLine($"Target={target}");
                URpcCodeManager.Instance.GatherCodeFiles(includes, excludes);
                URpcCodeManager.Instance.GatherRpcClass(target);

                //Console.WriteLine("Rpc:GatherClass");
                //URpcCodeManager.Instance.GatherCodeFiles(includes, excludes);
                //URpcCodeManager.Instance.GatherClass();
                //Console.WriteLine("Rpc:WriteCode");
                //URpcCodeManager.Instance.WriteCode(target);
                URpcCodeManager.Instance.MakeSharedProjectCSharp(target + "/", "EngineRPC.projitems");
                Console.WriteLine("Rpc:Finished");
            }

            text = FindArgument(segs, "Property_Target=");
            if (text==null)
            {
                return;
            }
            if (workAutoSync)
            {
                Console.WriteLine("CSharp build event: AutoSync");
                string property_target = dir + "/" + text;

                PropertyGen.UPropertyCodeManager.Instance.GatherCodeFiles(includes, excludes);
                PropertyGen.UPropertyCodeManager.Instance.GatherAutoSyncClass(property_target);

                //PropertyGen.UPropertyCodeManager.Instance.GatherCodeFiles(includes, excludes);
                //Console.WriteLine("Property:GatherClass");
                //PropertyGen.UPropertyCodeManager.Instance.GatherClass();
                //Console.WriteLine("Property:WriteCode");
                //PropertyGen.UPropertyCodeManager.Instance.WriteCode(property_target);
                //PropertyGen.UPropertyCodeManager.Instance.MakeSharedProjectCSharp(property_target + "/", "EngineProperty.projitems");
                //Console.WriteLine("Property:Finished");
            }

            if (workCs2Cpp)
            {
                Console.WriteLine("CSharp build event: Cs2Cpp");

                text = FindArgument(segs, "Pch=");
                if (text!=null)
                {
                    Cs2Cpp.UCs2CppManager.Instance.Pch = dir + "/" + text;
                }
                text = FindArgument(segs, "Cs2Cpp_Target=");
                if (text==null)
                {
                    return;
                }
                string cs2cpp_target = dir + "/" + text;

                Cs2Cpp.UCs2CppManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("Cs2Cpp:GatherClass");
                Cs2Cpp.UCs2CppManager.Instance.GatherClass();
                Console.WriteLine("Cs2Cpp:WriteCode");
                Cs2Cpp.UCs2CppManager.Instance.WriteCode(cs2cpp_target);
                Cs2Cpp.UCs2CppManager.Instance.MakeSharedProjectCSharp(cs2cpp_target + "/cs/", "Cs2Cpp.projitems");
                Cs2Cpp.UCs2CppManager.Instance.MakeSharedProjectCpp(cs2cpp_target + "/cpp/", "Cs2Cpp.vcxitems");
                Console.WriteLine("Cs2Cpp:Finished");
            }

            if (workMacross)
            {
                text = FindArgument(segs, "Macross_Target=");
                if (text==null)
                {
                    return;
                }
                string macross_target = dir + "/" + text;

                Macross.UMacrossClassManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("Macross:GatherClass");
                Macross.UMacrossClassManager.Instance.GatherClass();
                Console.WriteLine("Macross:WriteCode");
                Macross.UMacrossClassManager.Instance.WriteCode(macross_target);

                Macross.UMacrossContextMenuManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("MacrossContextMenu:GatherClass");
                Macross.UMacrossContextMenuManager.Instance.GatherClass();
                Console.WriteLine("MacrossContextMenu:WriteCode");
                Macross.UMacrossContextMenuManager.Instance.WriteCode(macross_target);
                Macross.UMacrossClassManager.Instance.WritedFiles.Add($"{macross_target}/MacrossContextMenu.macross.cs".ToLower());
                Macross.UMacrossClassManager.Instance.MakeSharedProjectCSharp(macross_target + "/", "EngineMacross.projitems");
                Console.WriteLine("Macross:Finished");
            }
        }
    }
}
