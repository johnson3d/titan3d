using System;
using System.Collections.Generic;

namespace CSharpCodeTools
{
    class Program
    {
        static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                    return i;
            }
            return null;
        }
        static void Main(string[] args)
        {
            var file = args[0].Replace('\\', '/');
            var idx = file.LastIndexOf('/');
            var dir = file.Substring(0, idx);
            var cfg = System.IO.File.ReadAllText(file);
            var segs = cfg.Split("\r\n");

            bool workRpc = true;
            bool workProp = true;
            bool workCs2Cpp = true;
            var text = FindArgument(args, "mode=");
            if (text != null)
            {
                workRpc = false;
                workProp = false;
                workCs2Cpp = false;
                text = text.Substring("mode=".Length);
                var modes = text.Split('+');
                foreach(var i in modes)
                {
                    switch(i)
                    {
                        case "Rpc":
                            workRpc = true;
                            break;
                        case "Prop":
                            workProp = true;
                            break;
                        case "Cs2Cpp":
                            workCs2Cpp = true;
                            break;
                    }
                }
            }

            text = FindArgument(segs, "Include=");
            List<string> includes = new List<string>();
            {
                var text1 = text.Substring("Include=".Length);
                var inc = text1.Split(',');
                Console.WriteLine("Include:");
                foreach (var i in inc)
                {
                    includes.Add(dir + "/" + i);
                    Console.WriteLine(dir + "/" + i);
                }
            }
            text = FindArgument(segs, "Exclude=");
            text = text.Replace("\r", "");
            if (!text.StartsWith("Exclude="))
            {
                return;
            }
            List<string> excludes = new List<string>();
            {
                var text1 = text.Substring("Exclude=".Length);
                var inc = text1.Split(',');
                Console.WriteLine("Exclude:");
                foreach (var i in inc)
                {
                    excludes.Add(dir + "/" + i);
                    Console.WriteLine(dir + "/" + i);
                }
            }

            text = FindArgument(segs, "Target=");
            if (!text.StartsWith("Target="))
            {
                return;
            }
            
            if (workRpc)
            {
                string target = dir + "/" + text.Substring("Target=".Length);
                Console.WriteLine($"Target={target}");

                Console.WriteLine("Rpc:GatherClass");
                URpcCodeManager.Instance.GatherCodeFiles(includes, excludes);
                URpcCodeManager.Instance.GatherClass();
                Console.WriteLine("Rpc:WriteCode");
                URpcCodeManager.Instance.WriteCode(target);
                URpcCodeManager.Instance.MakeSharedProjectCSharp(target + "/", "EngineRPC.projitems");
                Console.WriteLine("Rpc:Finished");
            }

            text = FindArgument(segs, "Property_Target=");
            if (!text.StartsWith("Property_Target="))
            {
                return;
            }
            if (workProp)
            {
                string property_target = dir + "/" + text.Substring("Property_Target=".Length);

                PropertyGen.UPropertyCodeManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("Property:GatherClass");
                PropertyGen.UPropertyCodeManager.Instance.GatherClass();
                Console.WriteLine("Property:WriteCode");
                PropertyGen.UPropertyCodeManager.Instance.WriteCode(property_target);
                PropertyGen.UPropertyCodeManager.Instance.MakeSharedProjectCSharp(property_target + "/", "EngineProperty.projitems");
                Console.WriteLine("Property:Finished");
            }

            if (workCs2Cpp)
            {
                text = FindArgument(segs, "Pch=");
                if (text.StartsWith("Pch="))
                {
                    Cs2Cpp.UCs2CppManager.Instance.Pch = dir + "/" + text.Substring("Pch=".Length);
                }
                text = FindArgument(segs, "Cs2Cpp_Target=");
                if (!text.StartsWith("Cs2Cpp_Target="))
                {
                    return;
                }
                string cs2cpp_target = dir + "/" + text.Substring("Cs2Cpp_Target=".Length);

                Cs2Cpp.UCs2CppManager.Instance.GatherCodeFiles(includes, excludes);
                Console.WriteLine("Cs2Cpp:GatherClass");
                Cs2Cpp.UCs2CppManager.Instance.GatherClass();
                Console.WriteLine("Cs2Cpp:WriteCode");
                Cs2Cpp.UCs2CppManager.Instance.WriteCode(cs2cpp_target);
                Cs2Cpp.UCs2CppManager.Instance.MakeSharedProjectCSharp(cs2cpp_target + "/cs/", "Cs2Cpp.projitems");
                Cs2Cpp.UCs2CppManager.Instance.MakeSharedProjectCpp(cs2cpp_target + "/cpp/", "Cs2Cpp.vcxitems");
                Console.WriteLine("Cs2Cpp:Finished");
            }
        }
    }
}
