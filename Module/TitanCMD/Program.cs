using EngineNS;
using EngineNS.EGui.UIProxy;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProjectCooker
{
    class Program
    {
#if PWindow
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

        static bool IsRun = true;
        static void Main(string[] args)
        {
            var mBin = System.IO.Directory.GetCurrentDirectory();

            var jsCode = EngineNS.IO.TtFileManager.ReadAllText(mBin + "/../cache/config/engine.jscfg");
            EngineNS.TtEngineConfig Config = null;
            if (jsCode != null)
            {
                Config = EngineNS.IO.TtFileManager.LoadObjectFromJson<TtEngineConfig>(jsCode);
            }
            else
            {
                EngineNS.IO.TtFileManager.WriteAllText(mBin + "/../cache/config/engine.jscfg", "{\"NativeDll\": \"release\"}");
            }

            if (Config!=null)
            {
                Console.WriteLine($"NativeDLL={Config.NativeDll}");
                EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/{Config.NativeDll}");

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("AssetType="))
                    {
                        args[i] = "AssetType=" + Config.CookAssetType;
                        break;
                    }
                }
            }
            else
            {
                var cfg = TtCookCommand.FindArgument(args, "NativeDLL=");
                if (cfg != null && cfg == "debug")
                {
                    Console.WriteLine($"NativeDLL=debug");
                    EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/debug");
                }
                else
                {
                    Console.WriteLine($"NativeDLL=release");
                    EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/release");
                }
            }

            var handle = GetConsoleWindow();
            ShowWindow(handle, 1);

            Console.WriteLine("Hello Titan");
            foreach(var arg in args)
            {
                Console.WriteLine(arg);
            }
            var extCmd = TtCookCommand.FindArgument(args, "ExtraCmd=");
            if (extCmd != null)
            {
                List<string> argList = new List<string>(args);
                Console.WriteLine($"Please input extra command: {extCmd}");
                var cmdNum = System.Convert.ToInt32(extCmd);
                for (int i = 0; i < cmdNum; i++)
                {
                    var tmpCmd = Console.ReadLine();
                    argList.Add(tmpCmd);
                }
                args = argList.ToArray();
            }
            
            var cfgFile = TtCookCommand.FindArgument(args, "CookCfg=");
            //var cfgFile = @"F:\titan3d\content\EngineConfigForCook.cfg";
            //EngineNS.UEngine.UGfxDeviceType = typeof(EngineNS.Graphics.Pipeline.UGfxDeviceConsole);
            var task = EngineNS.TtEngine.StartEngine(new EngineNS.TtEngine(args), cfgFile, false);

            var cmd = TtCookCommand.FindArgument(args, "ExeCmd=");
            Action action = async () =>
            {
                switch (cmd)
                {
                    case "SaveAsLastest":
                        {
                            //ExeCmd=SaveAsLastest AssetType=Scene+Mesh CookCfg=$(SolutionDir)content\EngineConfigForCook.cfg 
                            var exe = new Command.TtSaveAsLastest();
                            await exe.ExecuteCommand(args);
                        }
                        break;
                    case "MakeSln":
                        {
                            //using (var sln = new net.r_eg.MvsSln.Sln("e:/Titan3d/EngineAll.sln", net.r_eg.MvsSln.SlnItems.All & ~net.r_eg.MvsSln.SlnItems.ProjectDependencies))
                            //{
                            //    foreach(var i in sln.Result.ProjectItems)
                            //    {
                            //        Console.WriteLine(i.fullPath);
                            //    }
                            //}
                        }
                        break;
                    case "StartDS":
                        {
                            var exe = new Command.TtStartDS();
                            await exe.ExecuteCommand(args);
                        }
                        return;
                    case "LoadPlugin":
                        {
                            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("SourceGit");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartRobot":
                        {
                            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("ClientRobot");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartRootServer":
                        {
                            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("RootServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartLoginServer":
                        {
                            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("LoginServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartLevelServer":
                        {
                            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("LevelServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartGateServer":
                        {
                            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("GateServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                }
                EngineNS.TtEngine.Instance.PostQuitMessage();
                IsRun = false;
            };

            bool isExcuteAction = false;
            while (IsRun)
            {
                if (EngineNS.TtEngine.Instance.Tick() == false)
                    break;

                if (isExcuteAction == false && task.IsCompleted)
                {
                    isExcuteAction = true;
                    action();
                }
            }

            EngineNS.TtEngine.Instance.FinalCleanup();
        }
    }
}
