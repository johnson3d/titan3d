using System.Collections.Generic;
using System.IO;

namespace EngineNS.Editor.AndroidBuild
{
    // 面板收集到的一次构建请求：仓库根 + 配置 + 可选 cook。不执行任何东西，只承载数据与校验。
    public sealed class TtAndroidBuildRequest
    {
        public string RepositoryRoot = string.Empty;
        public string Configuration = "Debug";
        public string ToolchainConfig = string.Empty;
        public bool CookFirst;
        public string CookConfig = "content/config/engineconfig_windows_cook.jscfg";
        public string CookBlacklist = string.Empty;
        public string TitanCmd = "binaries/net8.0/TitanCMD.exe";
        public string PowerShell = "powershell.exe";

        // 空值返回空串：命令行层遇到空串就不传参，让脚本使用自己的默认位置。
        public string GetToolchainConfigFullPath()
        {
            if (string.IsNullOrEmpty(ToolchainConfig))
                return string.Empty;
            return TtAndroidRepository.ToFullPath(RepositoryRoot, ToolchainConfig);
        }

        // 返回中文错误列表；空列表表示可以发起构建。
        public List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(RepositoryRoot) || !Directory.Exists(RepositoryRoot))
                errors.Add("仓库根不存在，请先定位包含 BuildScript/android/build-android.ps1 的目录。");
            if (Configuration != "Debug" && Configuration != "Release")
            {
                errors.Add("配置只允许 Debug 或 Release。");
            }
            else if (CookFirst && errors.Count == 0)
            {
                // cook 相关只在仓库根有效时才检查，避免重复报错。
                var cook = TtAndroidRepository.ToFullPath(RepositoryRoot, CookConfig);
                if (!File.Exists(cook))
                    errors.Add("cook 配置不存在：" + cook);
                if (!File.Exists(TtAndroidRepository.ToFullPath(RepositoryRoot, TitanCmd)))
                    errors.Add("找不到 TitanCMD.exe，请先完成托管构建：" + TtAndroidRepository.ToFullPath(RepositoryRoot, TitanCmd));
            }
            return errors;
        }
    }
}
