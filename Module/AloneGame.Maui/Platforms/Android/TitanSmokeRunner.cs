using AloneGame.Maui.Services;
using EngineNS;

namespace AloneGame.Maui;

// 冒烟初始化：安装内容 -> 创建引擎 -> PreInitEngine -> 校验 DataCopyer 哈希。
// 全程单次执行（Activity 重建复用同一个 Task），不阻塞 UI 线程。
public static class TitanSmokeRunner
{
    private static readonly object Gate = new();
    private static Task<string>? Running;
    private static TtEngine? Engine;

    public static Task<string> Start(string filesDir)
    {
        lock (Gate)
        {
            return Running ??= Task.Run(async () =>
            {
                try
                {
                    var root = TitanContentInstaller.Install(filesDir,
                        path => Android.App.Application.Context.Assets!.Open(path));
                    Engine = new TtEngine(new[] { "EngineRoot=" + root });
                    var ok = await TtEngine.StartEngine(Engine,
                        root + "content/config/engineconfig_android_smoke.jscfg", false);
                    if (ok)
                    {
                        var calculated = EngineNS.Bricks.DataCopyer.TtDataCopyer.CalcVersionHash().ToString();
                        var loaded = Engine.DataCopyer.GetVersionHash().ToString();
                        using var stream = Android.App.Application.Context.Assets!.Open("titan/datacopyer.hash");
                        using var reader = new StreamReader(stream);
                        var expected = reader.ReadToEnd().Trim();
                        Android.Util.Log.Info("TitanSmoke", $"VersionHash generated={expected} runtime={calculated} plugin={loaded}");
                        if (calculated != expected || loaded != expected) throw new InvalidDataException("DataCopyer 哈希不一致");
                    }
                    Android.Util.Log.Info("TitanSmoke", $"PreInitEngine result={ok}");
                    return $"PreInitEngine result={ok}";
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("TitanSmoke", ex.ToString());
                    Android.Util.Log.Info("TitanSmoke", "PreInitEngine result=False");
                    return ex.Message;
                }
            });
        }
    }
}
