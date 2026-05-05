using Assimp;
using Assimp.Unmanaged;
using EngineNS;
using EngineNS.EGui.UIProxy;
using EngineNS.Graphics.Pipeline;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MainEditor
{
    class Program
    {
#if PWindow
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

        delegate IAsyncEnumerable<int> FOnPostTest(bool arg);
        static FOnPostTest FnOnTest = OnPostTest;
        public static async IAsyncEnumerable<int> OnPostTest(bool arg)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            for (int i = 0; i < 10; i++)
            {
                yield return i;
            }
            yield return -1;
        }
        public static async System.Threading.Tasks.Task Test1()
        {
            await foreach(var it in OnPostTest(true))
            {

            }
        }

        static bool WaitRedgate = false;
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            //TtNativeWindow.MessageBoxA(0, "Hello", "Hello", 0);
            //SampleA.Test();
            //var it = FnOnTest(true);
            //var itt = it.GetAsyncEnumerator();
            //while (itt.MoveNextAsync().Result == true)
            //{
            //    var rr = itt.Current;
            //    if (rr == -1)
            //        break;
            //}

            var mBin = System.IO.Directory.GetCurrentDirectory();
            
            EngineNS.TtEngineConfig Config = null;
            
            var jsCode = EngineNS.IO.TtFileManager.ReadAllText(mBin + "/../cache/config/engine.jscfg");
            if (jsCode != null)
            {
                if (Config==null)
                {
                    Config = EngineNS.IO.TtFileManager.LoadObjectFromJson<TtEngineConfig>(jsCode);
                }
                else
                {
                    EngineNS.IO.TtAdvancedJsonPartialUpdater.PartialUpdate<TtEngineConfig>(jsCode, Config, null);
                }
            }
            else
            {
                EngineNS.IO.TtJsonOptions options = new EngineNS.IO.TtJsonOptions();
                options.SaveProperties = new List<string>() { "NativeDll",
                            "UseRenderDoc",
                            "HasDebugLayer",
                            "IsGpuBaseValidation",
                            "IsDebugShader",
                            "IsGpuDred",
                            "IsAftermath"
                };
                
                Config = new TtEngineConfig();
                Config.SaveConfig(mBin + "/../cache/config/engine.jscfg", options);
                //EngineNS.IO.TtFileManager.WriteAllText(mBin + "/../cache/config/engine.jscfg", "{\"NativeDll\": \"release\"}");
                
                var cfg = FindArgument(args, "config=");
                if (cfg == null)
                {
                    TtNativeWindow.MessageBoxA(0, "config is null", "Titan3D", 0);
                }
                jsCode = EngineNS.IO.TtFileManager.ReadAllText(cfg);
                if (jsCode!=null)
                {
                    Config = EngineNS.IO.TtFileManager.LoadObjectFromJson<TtEngineConfig>(jsCode);
                }
            }

            string dllDir = "";
            if (Config != null)
            {
                Console.WriteLine($"NativeDLL={Config.NativeDll}");
                dllDir = $"{mBin}/{Config.NativeDll}";
            }
            else
            {
                var cfg = FindArgument(args, "NativeDLL=");
                if (cfg != null && cfg == "debug")
                {
                    Console.WriteLine($"NativeDLL=debug");
                    dllDir = $"{mBin}/debug";
                    
                }
                else
                {
                    Console.WriteLine($"NativeDLL=release");
                    dllDir = $"{mBin}/release";
                }
            }

            if (!TtFileManager.FileExists(dllDir + "/Core.Window.dll"))
            {
                TtNativeWindow.MessageBoxA(IntPtr.Zero, $"{dllDir}: not found native dll, please compile Core.Window project", "Error", 0);
                return;
            }
            EngineNS.TtNativeWindow.SetDllDirectoryA(dllDir);

            {
                var ev1 = Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING");
                Console.WriteLine($"CORECLR_ENABLE_PROFILING:{ev1}");
                var ev2 = Environment.GetEnvironmentVariable("CORECLR_PROFILER");
                Console.WriteLine($"CORECLR_PROFILER:{ev2}");
                var ev3 = Environment.GetEnvironmentVariable("CORECLR_PROFILER_PATH_64");
                Console.WriteLine($"CORECLR_PROFILER_PATH_64:{ev3}");
            }

            System.IO.StreamWriter consoleWriter = null;
            System.IO.FileStream ostrm = null;
#if PWindow
            //try
            //{
            //    ostrm = new System.IO.FileStream("./console.out", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            //    consoleWriter = new System.IO.StreamWriter(ostrm);
            //    System.Console.SetOut(consoleWriter);
            //}
            //catch
            //{

            //}
            
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
            //ShowWindow(handle, 1);
            //EngineNS.EigenUtility.TestJacobi();
#endif

            EngineNS.NxRHI.TtGpuSystem renderSys;
            EngineNS.NxRHI.TtGpuDevice gpuDevice;
            WeakReference wr = Main_Impl(args, out renderSys, out gpuDevice);
            
            //wait gc
            int iCollect = 0;
            while (wr.IsAlive)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                iCollect++;
                if (iCollect>=50)
                {
                    EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtCoreGategory>(EngineNS.Profiler.ELogTag.Warning, $"Main wait GC failed");
                    break;
                }
            }
            
            TtGfxDevice.DestroyRenderSystem(renderSys, gpuDevice);
            
            if (consoleWriter != null)
            {
                consoleWriter.Close();
                ostrm.Close();
            }

            while (WaitRedgate)
            {
                System.Threading.Thread.Sleep(1000);
            }

            CoreSDK.DumpNativeMemoryState("MainExit:", 0);
            //Open for MemoryProfiler
            //CoreSDK.MessageDialog("ExitApp");
        }
        static WeakReference Main_Impl(string[] args, out EngineNS.NxRHI.TtGpuSystem gpuSystem, out EngineNS.NxRHI.TtGpuDevice gpuDevice)
        {
            var cfg = FindArgument(args, "config=");
            if (cfg == null)
            {
                TtNativeWindow.MessageBoxA(0, "config is null", "Titan3D", 0);
            }
            Console.WriteLine($"Config={cfg}");

            bool bNativeMem = true;
            var nativMem = FindArgument(args, "NativeMem=");
            if (nativMem != null)
            {
                bNativeMem = int.Parse(nativMem) == 1 ? true : false;
            }
            Console.WriteLine($"Native Memory Profiler={bNativeMem}");

            var task = EngineNS.TtEngine.StartEngine(new EngineNS.TtEngine(args), cfg, bNativeMem);
            while (task.IsCompleted == false)
            {
                if (EngineNS.TtEngine.Instance.Tick() == false)
                {
                    break;
                }
            }
            if (task.IsCompleted == false)
            {
                throw new InvalidOperationException("Engine startup did not complete before the main loop exited.");
            }
            if (task.GetAwaiter().GetResult() == false)
            {
                throw new InvalidOperationException("Engine startup failed.");
            }
            
            while (true)
            {
                var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as EngineNS.Editor.TtMainEditorApplication;
                if (mainEditor != null)
                {
                    mainEditor.mClrProfiler.UpdateLogs();
                }
                if (EngineNS.TtEngine.Instance.Tick() == false)
                {
                    break;
                }
            }
            
            var wr = new WeakReference(EngineNS.TtEngine.Instance);
            gpuDevice = EngineNS.TtEngine.Instance.GfxDevice.RenderContext;
            gpuSystem = EngineNS.TtEngine.Instance.GfxDevice.RenderSystem;
            EngineNS.TtEngine.Instance.FinalCleanup();
            return wr;
        }
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
    }
}
