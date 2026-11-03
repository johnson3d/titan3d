using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EngineNS.Editor.AndroidBuild
{
    // 从环境变量、默认安装位置、注册表与 Visual Studio 捆绑目录探测本机 Android 工具链。
    // 只做「发现」，不做下载与版本校验：版本门禁由 AndroidTools.ps1 统一把关。
    public static class TtAndroidToolchainLocator
    {
        // 与 AndroidToolchain.example.json / CMakeLists.txt 的仓库契约保持一致。
        // ndkMajor 只是缺省建议值：实际版本要求以 android-toolchain.json 的 requirements.ndkMajor 为准，
        // 面板可改，升级 NDK 时只动配置，不用改代码。
        public const int DefaultNdkMajor = 27;
        public const int RepoApiLevel = 28;
        // 兼容旧引用的别名。
        public const int RequiredNdkMajor = DefaultNdkMajor;

        // 缺件时的兜底安装命令：取不到仓库 SDK 版本带时才用它（优先用 GetWorkloadInstallCommand）。
        public const string MauiWorkloadInstallCommand = "dotnet workload install maui-android";

        // 生成面向仓库 SDK 版本带的 workload 安装命令：global.json 锁定 major.minor（如 10.0），
        // 从本机已装 SDK 里取该版本带的最高版本并附 --sdk-version。不带 --sdk-version 时 dotnet
        // 会按默认（最新）SDK 安装——本机存在 rc 预览版时会装错位置，构建时 NETSDK1147。
        public static string GetWorkloadInstallCommand(string? repoRoot)
        {
            var band = ReadGlobalJsonSdkBand(repoRoot);
            var version = band == null ? null : FindHighestInstalledSdk(band);
            return version == null
                ? MauiWorkloadInstallCommand
                : $"dotnet workload install maui-android --sdk-version {version}";
        }

        // 读 global.json 的 sdk.version，返回 major.minor 版本带（如 "10.0.100" → "10.0"）；缺失或非法返回 null。
        public static string? ReadGlobalJsonSdkBand(string? repoRoot)
        {
            if (string.IsNullOrWhiteSpace(repoRoot))
                return null;
            var path = System.IO.Path.Combine(repoRoot, "global.json");
            if (!System.IO.File.Exists(path))
                return null;
            try
            {
                var node = System.Text.Json.Nodes.JsonNode.Parse(System.IO.File.ReadAllText(path));
                var raw = node?["sdk"]?["version"]?.ToString();
                if (string.IsNullOrWhiteSpace(raw))
                    return null;
                var parts = raw.Split('.');
                if (parts.Length < 2 || !int.TryParse(parts[0], out _) || !int.TryParse(parts[1], out _))
                    return null;
                return $"{parts[0]}.{parts[1]}";
            }
            catch (Exception)
            {
                return null;
            }
        }

        // dotnet --list-sdks 输出每行形如 "10.0.403 [C:\...]"；返回版本带 <band>.x 里最高的已装版本，无匹配返回 null。
        public static string? FindHighestInstalledSdk(string band)
        {
            if (string.IsNullOrWhiteSpace(band))
                return null;
            string output;
            try
            {
                using var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "--list-sdks",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    },
                };
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);
            }
            catch (Exception)
            {
                return null;
            }
            Version? best = null;
            string? bestText = null;
            var prefix = band + ".";
            foreach (var line in output.Split('\n'))
            {
                var token = line.Trim().Split(' ')[0];
                if (!token.StartsWith(prefix, StringComparison.Ordinal))
                    continue;
                if (!Version.TryParse(token, out var version))
                    continue;
                if (best == null || version > best)
                {
                    best = version;
                    bestText = token;
                }
            }
            return bestText;
        }

        // 本机未找到时给面板一个「下载」按钮的目标页；不在清单里的 key 返回 false。
        public static bool TryGetDownloadUrl(string key, out string url)
        {
            switch (key)
            {
                case "androidSdk":
                    url = "https://developer.android.com/studio";
                    return true;
                case "ndk":
                    url = "https://developer.android.com/ndk/downloads";
                    return true;
                case "javaHome":
                    // .NET Android 工具链（XA0030）要求 JDK 21（17 亦可）；新版 Studio 捆绑的 JBR 是 25，会被拒绝。
                    url = "https://adoptium.net/temurin/releases/?version=21";
                    return true;
                default:
                    url = string.Empty;
                    return false;
            }
        }

        public sealed class Result
        {
            // key（ndk/androidSdk/cmake/ninja/dotnet/javaHome）→ 探测到的绝对路径。
            public readonly Dictionary<string, string> Values = new Dictionary<string, string>();
            // 未探测到的 key，面板据此提示用户安装。
            public readonly List<string> Missing = new List<string>();
        }

        public static Result Locate()
        {
            return Locate(DefaultNdkMajor);
        }

        // requiredNdkMajor 由调用方从 android-toolchain.json 的 requirements.ndkMajor 读入。
        public static Result Locate(int requiredNdkMajor)
        {
            var result = new Result();
            var sdk = FindAndroidSdk();
            if (sdk != null)
                result.Values["androidSdk"] = sdk;
            var ndk = FindNdk(sdk, requiredNdkMajor);
            if (ndk != null)
                result.Values["ndk"] = ndk;
            var cmake = FindCmake(sdk);
            if (cmake != null)
                result.Values["cmake"] = cmake;
            var ninja = FindNinja(sdk);
            if (ninja != null)
                result.Values["ninja"] = ninja;
            var dotnet = FindDotnet();
            if (dotnet != null)
                result.Values["dotnet"] = dotnet;
            var javaHome = FindJavaHome();
            if (javaHome != null)
                result.Values["javaHome"] = javaHome;
            foreach (var key in new[] { "ndk", "androidSdk", "cmake", "ninja", "dotnet", "javaHome" })
            {
                if (!result.Values.ContainsKey(key))
                    result.Missing.Add(key);
            }
            return result;
        }

        // ---- androidSdk ----

        private static string? FindAndroidSdk()
        {
            var candidates = new List<string>();
            foreach (var name in new[] { "ANDROID_HOME", "ANDROID_SDK_ROOT" })
            {
                var env = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(env))
                    candidates.Add(env);
            }
            // Android Studio 安装器写入 HKLM\SOFTWARE\Android Studio 的 SdkPath（用户自定义 SDK 位置时唯一可靠来源）。
            var fromRegistry = ReadAndroidStudioSdkPath();
            if (!string.IsNullOrWhiteSpace(fromRegistry))
                candidates.Add(fromRegistry);
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            candidates.Add(Path.Combine(localAppData, "Android", "Sdk"));
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Android", "android-sdk"));
            candidates.Add(@"C:\Android\Sdk");
            candidates.Add(@"D:\Android\Sdk");
            foreach (var candidate in candidates)
            {
                if (IsValidSdkRoot(candidate))
                    return Normalize(candidate);
            }
            return null;
        }

        // sdkmanager 布局的 SDK 根至少含这些子目录之一。
        public static bool IsValidSdkRoot(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return false;
            foreach (var sub in new[] { "platforms", "build-tools", "platform-tools", "cmdline-tools", "ndk" })
            {
                if (Directory.Exists(Path.Combine(path, sub)))
                    return true;
            }
            return false;
        }

        // ---- ndk：<sdk>\ndk\<version>，取满足 ndkMajor 的最高版本 ----

        private static string? FindNdk(string? sdk, int requiredNdkMajor)
        {
            if (string.IsNullOrWhiteSpace(sdk))
                return null;
            var ndkRoot = Path.Combine(sdk, "ndk");
            if (!Directory.Exists(ndkRoot))
                return null;
            return SelectHighestNdk(Directory.GetDirectories(ndkRoot), File.ReadAllText, requiredNdkMajor);
        }

        // 依据 source.properties 的 Pkg.Revision 选版本；不满足 requiredNdkMajor 的一律不选（脚本同样会拒）。
        public static string? SelectHighestNdk(IEnumerable<string> dirs, Func<string, string> readText, int requiredNdkMajor)
        {
            Version? best = null;
            string? bestPath = null;
            foreach (var dir in dirs)
            {
                var props = Path.Combine(dir, "source.properties");
                if (!File.Exists(props))
                    continue;
                var version = TryParseNdkRevision(readText(props));
                if (version == null || version.Major != requiredNdkMajor)
                    continue;
                if (best == null || version > best)
                {
                    best = version;
                    bestPath = Normalize(dir);
                }
            }
            return bestPath;
        }

        public static Version? TryParseNdkRevision(string? sourcePropertiesText)
        {
            if (string.IsNullOrEmpty(sourcePropertiesText))
                return null;
            foreach (var line in sourcePropertiesText.Replace("\r\n", "\n").Split('\n'))
            {
                var idx = line.IndexOf('=');
                if (idx <= 0)
                    continue;
                if (line.Substring(0, idx).Trim() != "Pkg.Revision")
                    continue;
                var raw = line.Substring(idx + 1).Trim();
                // Pkg.Revision 可能是 "27.2.12479018" 或带后缀 "27.2.12479018-rc1"，取前缀数字段。
                var dotParts = raw.Split('.');
                var numbers = new List<int>();
                foreach (var part in dotParts)
                {
                    var digits = new string(part.TakeWhile(char.IsDigit).ToArray());
                    if (digits.Length == 0 || !int.TryParse(digits, out var n))
                        break;
                    numbers.Add(n);
                }
                if (numbers.Count < 2)
                    return null;
                while (numbers.Count < 3)
                    numbers.Add(0);
                try
                {
                    return new Version(numbers[0], numbers[1], numbers[2]);
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            return null;
        }

        // ---- cmake / ninja：优先 SDK 内版本，其次独立安装，最后 VS 捆绑 ----

        private static string? FindCmake(string? sdk)
        {
            if (!string.IsNullOrWhiteSpace(sdk))
            {
                var cmakeRoot = Path.Combine(sdk, "cmake");
                if (Directory.Exists(cmakeRoot))
                {
                    var best = PickHighestVersionSubdir(cmakeRoot);
                    if (best != null)
                    {
                        var exe = Path.Combine(best, "bin", "cmake.exe");
                        if (File.Exists(exe))
                            return exe;
                    }
                }
            }
            return FindFirstFile(new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CMake", "bin", "cmake.exe"),
                Path.Combine(VsCommonExtensionsCMakeRoot(), "bin", "cmake.exe"),
            });
        }

        private static string? FindNinja(string? sdk)
        {
            if (!string.IsNullOrWhiteSpace(sdk))
            {
                var cmakeRoot = Path.Combine(sdk, "cmake");
                if (Directory.Exists(cmakeRoot))
                {
                    var best = PickHighestVersionSubdir(cmakeRoot);
                    if (best != null)
                    {
                        var exe = Path.Combine(best, "bin", "ninja.exe");
                        if (File.Exists(exe))
                            return exe;
                    }
                }
            }
            return FindFirstFile(new[]
            {
                Path.Combine(VsCommonExtensionsCMakeRoot(), "Ninja", "ninja.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CMake", "bin", "ninja.exe"),
            });
        }

        // VS 的 CMake/Ninja 捆绑在 <VS>\Common7\IDE\CommonExtensions\Microsoft\CMake 下；多版本 VS 取第一个存在的。
        private static string VsCommonExtensionsCMakeRoot()
        {
            foreach (var vsRoot in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) })
            {
                var vsRootDir = Path.Combine(vsRoot, "Microsoft Visual Studio");
                if (!Directory.Exists(vsRootDir))
                    continue;
                foreach (var editionDir in Directory.GetDirectories(vsRootDir))
                {
                    foreach (var idDir in Directory.GetDirectories(editionDir))
                    {
                        var root = Path.Combine(idDir, "Common7", "IDE", "CommonExtensions", "Microsoft", "CMake");
                        if (Directory.Exists(root))
                            return root;
                    }
                }
            }
            return string.Empty;
        }

        // <root>\<version>\ 目录名按 Version 排序取最大（Android SDK cmake 目录形如 3.22.1 / 4.0.2）。
        public static string? PickHighestVersionSubdir(string parent)
        {
            Version? best = null;
            string? bestPath = null;
            foreach (var dir in Directory.GetDirectories(parent))
            {
                var name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar));
                if (!Version.TryParse(name, out var version))
                    continue;
                if (best == null || version > best)
                {
                    best = version;
                    bestPath = dir;
                }
            }
            return bestPath;
        }

        // ---- dotnet：默认安装位置 + PATH ----

        private static string? FindDotnet()
        {
            var fromPath = FindOnPath("dotnet.exe");
            if (fromPath != null)
                return fromPath;
            return FindFirstFile(new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "dotnet", "dotnet.exe"),
            });
        }

        public static string? FindOnPath(string fileName)
        {
            return FindOnPath(fileName, Environment.GetEnvironmentVariable("Path"));
        }

        public static string? FindOnPath(string fileName, string? pathValue)
        {
            if (string.IsNullOrEmpty(pathValue))
                return null;
            foreach (var dir in pathValue.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(dir))
                    continue;
                var candidate = Path.Combine(dir.Trim().Trim('"'), fileName);
                if (File.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        // ---- javaHome：按 XA 兼容版本择优（21 > 17 > 其它）——

        // Xamarin.Android 工具链支持的 JDK 主版本：21 最优（XA0030 推荐），17 次之；
        // 新版 Android Studio 自带 JBR（25）会被 XA0030 拒绝，只在没有更好候选时才用。
        private static readonly int[] PreferredJavaMajors = { 21, 17 };

        // 读 <home>\release 文件的 JAVA_VERSION（形如 21.0.5），无法解析返回 -1。
        public static int ReadJavaMajorVersion(string path)
        {
            try
            {
                var release = Path.Combine(path, "release");
                if (!File.Exists(release))
                    return -1;
                foreach (var line in File.ReadAllLines(release))
                {
                    // release 文件可能是 "JAVA_VERSION=\"21.0.5\"" 带引号的键值对。
                    var match = System.Text.RegularExpressions.Regex.Match(
                        line, "^JAVA_VERSION=\\\"?([0-9]+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out var major))
                        return major;
                }
            }
            catch (Exception)
            {
                // 读不到版本信息不影响其它探测分支。
            }
            return -1;
        }

        // 给候选打分：21 优先，17 次之，已知会被 XA 拒绝的版本（如 25）降权，其余按原顺序。
        private static int JavaCandidateScore(string path)
        {
            var major = ReadJavaMajorVersion(path);
            for (var i = 0; i < PreferredJavaMajors.Length; i++)
            {
                if (major == PreferredJavaMajors[i])
                    return PreferredJavaMajors.Length - i;
            }
            return major switch
            {
                -1 => 0,
                >= 18 and < 21 => 1,
                _ => -1,
            };
        }

        private static string? FindJavaHome()
        {
            var candidates = new List<string>();
            var env = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (IsValidJavaHome(env))
                candidates.Add(Normalize(env!));
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            candidates.Add(Path.Combine(FindAndroidStudioPath(), "jbr"));
            foreach (var root in new[]
            {
                Path.Combine(programFiles, "Android", "Android Studio", "jbr"),
                Path.Combine(programFiles, "Java"),
                Path.Combine(programFiles, "Eclipse Adoptium"),
                Path.Combine(programFiles, "Microsoft"),
            })
            {
                if (!Directory.Exists(root))
                    continue;
                foreach (var dir in Directory.GetDirectories(root))
                {
                    if (IsValidJavaHome(dir))
                        candidates.Add(dir);
                }
            }
            string? best = null;
            var bestScore = int.MinValue;
            foreach (var candidate in candidates)
            {
                var score = JavaCandidateScore(candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }
            return best;
        }

        public static bool IsValidJavaHome(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            return File.Exists(Path.Combine(path, "bin", "java.exe"));
        }

        // Android Studio 安装位置：注册表 HKLM\SOFTWARE\Android Studio 的 Path 值 → 默认目录。
        private static string FindAndroidStudioPath()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Android Studio");
                    var value = key?.GetValue("Path") as string;
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
                catch (Exception)
                {
                    // 注册表不可读时静默退化到默认目录探测。
                }
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Android", "Android Studio");
        }

        // 注册表 HKLM\SOFTWARE\Android Studio 的 SdkPath 值（SDK Manager 配置后由安装器/Studio 写入）。
        private static string? ReadAndroidStudioSdkPath()
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Android Studio");
                    var value = key?.GetValue("SdkPath") as string;
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
                catch (Exception)
                {
                    // 注册表不可读时静默退化到默认目录探测。
                }
            }
            return null;
        }

        private static string? FindFirstFile(IEnumerable<string> candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        private static string Normalize(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return Path.GetFullPath(path).TrimEnd('\\');
        }
    }
}
