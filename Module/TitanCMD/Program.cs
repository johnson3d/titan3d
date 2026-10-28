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

        static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
            finally
            {
                if (TtEngine.Instance != null)
                {
                    TtEngine.Instance.FinalCleanup();
                }
            }
        }

        static async System.Threading.Tasks.Task<int> ExecuteCommandAsync(string cmd, string[] args)
        {
            switch (cmd)
            {
                case "SaveAsLastest":
                    return await new Command.TtSaveAsLastest().ExecuteCommand(args);
                case "RunTests":
                    return await new Command.TtRunTests().ExecuteCommand(args);
                case "MakeSln":
                    return 0;
                case "StartDS":
#if PWindow
                    return await new Command.TtStartDS().ExecuteCommand(args);
#else
                    Console.WriteLine("StartDS is not supported on this platform (TcpServer/HPSocket closure missing).");
                    return -1;
#endif
                case "LoadPlugin":
                    TtEngine.Instance.PluginModuleManager.GetPluginModule("SourceGit")?.SureLoad();
                    return 0;
                case "StartRobot":
                    TtEngine.Instance.PluginModuleManager.GetPluginModule("ClientRobot")?.SureLoad();
                    return 0;
                case "StartRootServer":
                    TtEngine.Instance.PluginModuleManager.GetPluginModule("RootServer")?.SureLoad();
                    return 0;
                case "StartLoginServer":
                    TtEngine.Instance.PluginModuleManager.GetPluginModule("LoginServer")?.SureLoad();
                    return 0;
                case "StartLevelServer":
                    TtEngine.Instance.PluginModuleManager.GetPluginModule("LevelServer")?.SureLoad();
                    return 0;
                case "StartGateServer":
                    TtEngine.Instance.PluginModuleManager.GetPluginModule("GateServer")?.SureLoad();
                    return 0;
                case "macross-patch":
                    return await new Command.TtMacrossPatch().ExecuteCommand(args);
                default:
                    Console.Error.WriteLine($"Unknown ExeCmd: {cmd ?? "<null>"}");
                    return 2;
            }
        }

        static int Run(string[] args)
        {
            IsRun = true;
            var mBin = System.IO.Directory.GetCurrentDirectory();
            var requestedNativeConfig = TtCookCommand.FindArgument(args, "NativeDLL=");
            var nativeConfig = TtCommandExecutionPolicy.ResolveNativeConfiguration(requestedNativeConfig, null);
            Console.WriteLine($"NativeDLL={nativeConfig}");
            EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/{nativeConfig}");

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
                nativeConfig = TtCommandExecutionPolicy.ResolveNativeConfiguration(requestedNativeConfig, Config.NativeDll);
                Console.WriteLine($"NativeDLL={nativeConfig}");
                EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/{nativeConfig}");

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("AssetType="))
                    {
                        args[i] = "AssetType=" + Config.CookAssetType;
                        break;
                    }
                }
            }

#if PWindow
            var handle = GetConsoleWindow();
            ShowWindow(handle, 1);
#endif

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
            if (!TtCommandExecutionPolicy.IsEngineConfigurationAvailable(cfgFile))
            {
                Console.Error.WriteLine($"Engine configuration does not exist: {cfgFile}");
                return 1;
            }
            //var cfgFile = @"F:\titan3d\content\EngineConfigForCook.cfg";
            //EngineNS.UEngine.UGfxDeviceType = typeof(EngineNS.Graphics.Pipeline.UGfxDeviceConsole);
            var task = EngineNS.TtEngine.StartEngine(new EngineNS.TtEngine(args), cfgFile, false);

            var cmd = TtCookCommand.FindArgument(args, "ExeCmd=");
            var isServiceCommand = TtCommandExecutionPolicy.IsServiceCommand(cmd);
            System.Threading.Tasks.Task<int> commandTask = null;
            var commandStarted = false;
            var exitCode = 0;
            while (IsRun)
            {
                if (EngineNS.TtEngine.Instance.Tick() == false)
                {
                    if (!commandStarted || (!isServiceCommand && commandTask != null))
                        return 1;
                    break;
                }

                if (!commandStarted && task.IsCompleted)
                {
                    if (!task.GetAwaiter().GetResult())
                        return 1;

                    commandStarted = true;
                    commandTask = ExecuteCommandAsync(cmd, args);
                }

                if (commandTask != null && commandTask.IsCompleted)
                {
                    exitCode = commandTask.GetAwaiter().GetResult();
                    commandTask = null;
                    if (!isServiceCommand)
                    {
                        EngineNS.TtEngine.Instance.PostQuitMessage();
                        IsRun = false;
                    }
                }
            }

            return exitCode;
        }
    }
}
