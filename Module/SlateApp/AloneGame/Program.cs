using NPOI.SS.Formula.Functions;
using System.Reflection;
using AloneGame;
using static Org.BouncyCastle.Math.EC.ECCurve;

class Program
{
    
    [STAThreadAttribute]
    static void Main(string[] args)
    {
        var root = EngineNS.TtEngine.FindArgument(args, "EngineRoot=");
        var NativeDLL = EngineNS.IO.TtFileManager.CombinePath(root, "binaries/release");
        EngineNS.TtNativeWindow.SetDllDirectoryA(NativeDLL);

        var cfg = EngineNS.TtEngine.FindArgument(args, "config=");
        if (cfg == null)
        {
            EngineNS.TtNativeWindow.MessageBoxA(0, "config is null", "Titan3D", 0);
        }
        Console.WriteLine($"Config={cfg}");

        var forceRenderDoc = EngineNS.TtEngine.FindArgument(args, "ForceRenderDoc=");
        if (forceRenderDoc == "1" || string.Equals(forceRenderDoc, "true", StringComparison.OrdinalIgnoreCase))
        {
            var renderDoc = EngineNS.IO.TtFileManager.CombinePath(root, "3rd/native/renderdoc/bin/renderdoc.dll");
            EngineNS.IRenderDocTool.GetInstance().InitRenderDoc(renderDoc);
        }

        var task = EngineNS.TtEngine.StartEngine(new EngineNS.TtEngine(args), cfg, false);

        while (true)
        {
            if (EngineNS.TtEngine.Instance.Tick() == false)
            {
                break;
            }
        }

        EngineNS.TtEngine.Instance.FinalCleanup();
    }
}
