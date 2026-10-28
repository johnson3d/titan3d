using System.Text;

namespace EngineNS.Editor.AndroidBuild
{
    // adb 部署相关命令行。复用 TtAndroidProcessLauncher 跑，日志面板透传。
    public static class TtAndroidDeployCommand
    {
        public static string AdbInstall(string adbPath, string apkPath)
        {
            var builder = new StringBuilder();
            builder.Append('"').Append(adbPath).Append('"');
            builder.Append(" install -r \"").Append(apkPath).Append('"');
            return builder.ToString();
        }

        public static string AdbDevices(string adbPath)
        {
            var builder = new StringBuilder();
            builder.Append('"').Append(adbPath).Append('"');
            builder.Append(" devices");
            return builder.ToString();
        }
    }
}
