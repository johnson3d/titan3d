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
#if PWindow
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
#endif

            WeakReference wr = Main_Impl(args);
            while (wr.IsAlive)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            while(EngineNS.RHI.CShaderResourceView.NumOfInstance>0)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }
        static bool Run = true;
        static WeakReference Main_Impl(string[] args)
        {
            EngineNS.UEngine.StartEngine(new EngineNS.UEngine());
            EngineNS.UEngine.Instance.Config.CookGLSL = true;

            while (true)
            {
                if (EngineNS.UEngine.Instance.Tick() == false)
                    break;
            }

            var wr = new WeakReference(EngineNS.UEngine.Instance);
            EngineNS.UEngine.Instance.FinalCleanup();
            return wr;
        }
    }
}
