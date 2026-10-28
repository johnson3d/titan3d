using System;
using System.Collections.Generic;

namespace CppWeaving
{
    public enum TargetPlatform
    {
        Windows,
        Mac,
        Android,
    }

    public class ClangProfile
    {
        public TargetPlatform Platform = TargetPlatform.Windows;
        public string SysRoot;
        public string ResourceDir;
    }

    public static class ClangArgs
    {
        public static List<string> Build(ClangProfile profile, IEnumerable<string> includes, IEnumerable<string> macros)
        {
            var args = new List<string>();
            args.Add("-std=c++17");
            args.Add("-xc++");
            args.Add("-Wno-pragma-once-outside-header");
            args.Add("-Wno-address-of-temporary");
            if (profile.Platform == TargetPlatform.Mac)
            {
                // 与 Core.Mac 的 AppleClang 一致：Apple Silicon + libc++，SDK 与内建头由 xcrun 提供
                args.Add("-target");
                args.Add("arm64-apple-macosx11.0");
                args.Add("-stdlib=libc++");
                if (!string.IsNullOrEmpty(profile.SysRoot))
                {
                    args.Add("-isysroot");
                    args.Add(profile.SysRoot);
                }
                if (!string.IsNullOrEmpty(profile.ResourceDir))
                {
                    args.Add("-resource-dir");
                    args.Add(profile.ResourceDir);
                }
            }
            else if (profile.Platform == TargetPlatform.Android)
            {
                // 与 NDK r27 的 clang 一致：arm64-v8a + API 28，sysroot 与内建头由工具链提供
                args.Add("--target=aarch64-linux-android28");
                if (!string.IsNullOrEmpty(profile.SysRoot))
                {
                    args.Add("--sysroot=" + profile.SysRoot);
                }
                if (!string.IsNullOrEmpty(profile.ResourceDir))
                {
                    args.Add("-resource-dir");
                    args.Add(profile.ResourceDir);
                }
            }
            else
            {
                args.Add("-m64");
                args.Add("-target");
                args.Add("x86_64-pc-windows-msvc");
                args.Add("-fms-extensions");
                args.Add("-fms-compatibility");
                args.Add("-D_M_X64=1");
                args.Add("-D__EMSCRIPTEN__");
            }
            foreach (var i in includes)
            {
                args.Add($"-I{i}");
            }
            foreach (var i in macros)
            {
                args.Add($"-D{i}");
            }
            return args;
        }

        // vcxproj 里只对 MSVC 宿主有意义的 include：MSBuild 宏、pthread-win32、ATL/MFC
        public static bool IsHostOnlyInclude(TargetPlatform platform, string include)
        {
            if (platform == TargetPlatform.Windows)
                return false;
            if (include.Contains("$("))
                return include == "$(WindowsSDK_IncludePath)" || include == "$(VC_IncludePath)";
            var dir = include.Replace('\\', '/').TrimEnd('/');
            return dir.EndsWith("/pthread/include", StringComparison.OrdinalIgnoreCase)
                || dir.EndsWith("/atlmfc/include", StringComparison.OrdinalIgnoreCase);
        }

        public static TargetPlatform ParsePlatform(string text)
        {
            if (string.IsNullOrEmpty(text))
                return OperatingSystem.IsMacOS() ? TargetPlatform.Mac : TargetPlatform.Windows;
            if (Enum.TryParse<TargetPlatform>(text, true, out var platform) && Enum.IsDefined(platform))
                return platform;
            throw new ArgumentException($"unknown Platform={text}, expected Windows, Mac or Android");
        }
    }
}
