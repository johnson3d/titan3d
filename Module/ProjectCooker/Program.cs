using EngineNS;
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

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, 1);

            Console.WriteLine("Hello Titan");
            var extCmd = UCookCommand.FindArgument(args, "ExtraCmd=");
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
            
            var cfgFile = UCookCommand.FindArgument(args, "CookCfg=");
            //var cfgFile = @"F:\titan3d\content\EngineConfigForCook.cfg";
            //EngineNS.UEngine.UGfxDeviceType = typeof(EngineNS.Graphics.Pipeline.UGfxDeviceConsole);
            var task = EngineNS.UEngine.StartEngine(new EngineNS.UEngine(args), cfgFile);

            var cmd = UCookCommand.FindArgument(args, "ExeCmd=");
            Action action = async () =>
            {
                switch (cmd)
                {
                    case "SaveAsLastest":
                        {
                            //ExeCmd=SaveAsLastest AssetType=Scene+Mesh CookCfg=$(SolutionDir)content\EngineConfigForCook.cfg 
                            var exe = new Command.USaveAsLastest();
                            await exe.ExecuteCommand(args);
                        }
                        break;
                    case "RenameAsset":
                        {
                            var exe = new Command.URenameAsset();
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
                            var exe = new Command.UStartDS();
                            await exe.ExecuteCommand(args);
                        }
                        return;
                    case "LoadPlugin":
                        {
                            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("SourceGit");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartRobot":
                        {
                            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("ClientRobot");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartRootServer":
                        {
                            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("RootServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartLoginServer":
                        {
                            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("LoginServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartLevelServer":
                        {
                            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("LevelServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "StartGateServer":
                        {
                            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("GateServer");
                            if (serverPlugin != null)
                            {
                                serverPlugin.SureLoad();
                            }
                        }
                        return;
                    case "BuildSerializer":
                        {
                            //ExeCmd=BuildSerializer DS_Port=5555 CookCfg=$(SolutionDir)content\EngineConfigForCook.cfg Serializer_Path=$(SolutionDir)codegen\Serializer\Engine 
                            var exe = new Command.UBuildSerializer();
                            await exe.ExecuteCommand(args);
                        }
                        break;
                }
                EngineNS.UEngine.Instance.PostQuitMessage();
            };

            bool isExcuteAction = false;
            while (true)
            {
                if (EngineNS.UEngine.Instance.Tick() == false)
                    break;

                if (isExcuteAction == false && task.IsCompleted)
                {
                    isExcuteAction = true;
                    action();
                }
            }

            EngineNS.UEngine.Instance.FinalCleanup();
        }
    }
}
