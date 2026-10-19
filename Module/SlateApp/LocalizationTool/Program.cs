using LocalizationTool;

class Program
{
    [STAThreadAttribute]
    static void Main(string[] args)
    {
        var root = EngineNS.TtEngine.FindArgument(args, "EngineRoot=");
        var nativeDllName = EngineNS.TtEngine.FindArgument(args, "NativeDLL=");
        if (string.IsNullOrWhiteSpace(nativeDllName))
            nativeDllName = "release";
        var NativeDLL = EngineNS.IO.TtFileManager.CombinePath(root, "binaries/" + nativeDllName);
        if (!EngineNS.IO.TtFileManager.FileExists(NativeDLL + "/Core.Window.dll"))
        {
            EngineNS.TtNativeWindow.MessageBoxA(0, $"{NativeDLL}: not found native dll, please compile Core.Window project", "Titan3D", 0);
            return;
        }
        EngineNS.TtNativeWindow.SetDllDirectoryA(NativeDLL);

        var cfg = EngineNS.TtEngine.FindArgument(args, "config=");
        if (cfg == null)
        {
            EngineNS.TtNativeWindow.MessageBoxA(0, "config is null", "Titan3D", 0);
        }
        Console.WriteLine($"Config={cfg}");

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
