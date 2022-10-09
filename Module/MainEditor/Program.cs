using System;
using System.Runtime.InteropServices;
using System.Threading;

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
        [STAThreadAttribute]
        static void Main(string[] args)
        {
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

            WeakReference wr = Main_Impl(args);
            while (wr.IsAlive)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            int GCTimes = 0;
            while (EngineNS.NxRHI.USrView.NumOfInstance > 0)
            {
                if (GCTimes >= 20)
                {
                    Console.WriteLine($"CSV.NumOfInstance = {EngineNS.NxRHI.USrView.NumOfInstance}/{EngineNS.NxRHI.USrView.NumOfGCHandle}");
                    System.Diagnostics.Trace.WriteLine($"CSV.NumOfInstance = {EngineNS.NxRHI.USrView.NumOfInstance}/{EngineNS.NxRHI.USrView.NumOfGCHandle}");
                    //Thread.Sleep(1000 * 30);
                    break;
                }
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                GCTimes++;
            }

            if (consoleWriter != null)
            {
                consoleWriter.Close();
                ostrm.Close();
            }
        }
        static WeakReference Main_Impl(string[] args)
        {
            int IsProfiling = 0;
            var ev1 = Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING");
            if (ev1 != null)
            {
                IsProfiling = int.Parse(ev1);
            }
            var cfg = FindArgument(args, "config=");
            Console.WriteLine($"Config={cfg}");

            bool bRenderDoc = false;
            var useRenderdoc = FindArgument(args, "use_renderdoc=");
            if (useRenderdoc != null && useRenderdoc == "true")
            {
                bRenderDoc = true;
            }

            var task = EngineNS.UEngine.StartEngine(new EngineNS.UEngine(), cfg, bRenderDoc);

            while (true)
            {
                if (EngineNS.UEngine.Instance.Tick() == false)
                {
                    break;
                }
                //System.GC.Collect();
                if (IsProfiling == 1)
                {
                    ClrString clrStr = new ClrString();
                    int num = 0;
                    var ok = ClrLogger.PopLogInfo(ref clrStr);
                    while (ok)
                    {
                        if (clrStr.mType == EClrLogStringType.ObjectAlloc)
                        {
                            num++;
                            unsafe
                            {
                                EngineNS.CoreSDK.Print2Console((sbyte*)&clrStr.m_mString, true);
                            }
                        }
                        ok = ClrLogger.PopLogInfo(ref clrStr);
                    }
                }
            }
            
            var wr = new WeakReference(EngineNS.UEngine.Instance);
            EngineNS.UEngine.Instance.FinalCleanup();
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
