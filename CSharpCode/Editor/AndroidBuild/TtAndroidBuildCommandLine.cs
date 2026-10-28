using System.Text;

namespace EngineNS.Editor.AndroidBuild
{
    // 把 TtAndroidBuildRequest 拼成外部进程命令行。引号内路径由 Process 直接整串传给 powershell/cmd。
    public static class TtAndroidBuildCommandLine
    {
        public static string Check(TtAndroidBuildRequest request, string stage)
        {
            var builder = new StringBuilder();
            builder.Append('"').Append(request.PowerShell).Append('"');
            builder.Append(" -NoProfile -NonInteractive -ExecutionPolicy Bypass -File ");
            AppendQuotedPath(builder, request.RepositoryRoot, "BuildScript/check/check-android-toolchain.ps1");
            builder.Append(" -Stage ").Append(stage);
            var config = request.GetToolchainConfigFullPath();
            if (!string.IsNullOrEmpty(config))
            {
                builder.Append(" -ConfigPath \"").Append(config).Append('"');
            }
            return builder.ToString();
        }

        public static string Cook(TtAndroidBuildRequest request)
        {
            var builder = new StringBuilder();
            builder.Append('"').Append(TtAndroidRepository.ToFullPath(request.RepositoryRoot, request.TitanCmd)).Append('"');
            builder.Append(" ExeCmd=SaveAsLastest");
            builder.Append(" CookCfg=\"").Append(TtAndroidRepository.ToFullPath(request.RepositoryRoot, request.CookConfig)).Append('"');
            if (!string.IsNullOrWhiteSpace(request.CookBlacklist))
            {
                builder.Append(" Blacklist=\"").Append(request.CookBlacklist).Append('"');
            }
            return builder.ToString();
        }

        public static string Apk(TtAndroidBuildRequest request)
        {
            var builder = new StringBuilder();
            builder.Append('"').Append(request.PowerShell).Append('"');
            builder.Append(" -NoProfile -NonInteractive -ExecutionPolicy Bypass -File ");
            AppendQuotedPath(builder, request.RepositoryRoot, "BuildScript/android/build-android.ps1");
            builder.Append(" -Configuration ").Append(request.Configuration).Append(" -Stage Apk");
            var config = request.GetToolchainConfigFullPath();
            if (!string.IsNullOrEmpty(config))
            {
                builder.Append(" -ToolchainConfig \"").Append(config).Append('"');
            }
            return builder.ToString();
        }

        private static void AppendQuotedPath(StringBuilder builder, string root, string relative)
        {
            builder.Append('"').Append(TtAndroidRepository.ToFullPath(root, relative)).Append('"');
        }
    }
}
