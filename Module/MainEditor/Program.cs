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

        static string TryReadNativeDllName(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath) || !System.IO.File.Exists(configPath))
                return null;

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(configPath));
                if (doc.RootElement.TryGetProperty("NativeDll", out var value) &&
                    value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    return value.GetString();
                }
            }
            catch
            {
            }

            return null;
        }

        static string ResolveConfigPath(string cfg, string binDir)
        {
            if (!string.IsNullOrWhiteSpace(cfg))
            {
                if (System.IO.Path.IsPathRooted(cfg))
                    return cfg;

                var directPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(binDir, cfg));
                if (System.IO.File.Exists(directPath))
                    return directPath;

                return System.IO.Path.GetFullPath(System.IO.Path.Combine(binDir, "..", cfg));
            }

            return System.IO.Path.GetFullPath(System.IO.Path.Combine(binDir, "..", "content", "engineconfigdx12.jscfg"));
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
            var nativeDllArg = FindArgument(args, "NativeDLL=");
            var configArg = ResolveConfigPath(FindArgument(args, "config="), mBin);
            var bootstrapNativeDll = nativeDllArg;
            if (string.IsNullOrWhiteSpace(bootstrapNativeDll))
            {
                var cacheConfig = mBin + "/../cache/config/engine.jscfg";
                bootstrapNativeDll = TryReadNativeDllName(cacheConfig);
                if (string.IsNullOrWhiteSpace(bootstrapNativeDll))
                    bootstrapNativeDll = TryReadNativeDllName(configArg);
            }
            if (string.IsNullOrWhiteSpace(bootstrapNativeDll))
                bootstrapNativeDll = "release";
            EngineNS.TtNativeWindow.SetDllDirectoryA($"{mBin}/{bootstrapNativeDll}");

            Console.WriteLine($"NativeDLL={bootstrapNativeDll}");
            var dllDir = $"{mBin}/{bootstrapNativeDll}";

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
            var cfg = ResolveConfigPath(FindArgument(args, "config="), System.IO.Directory.GetCurrentDirectory());
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
