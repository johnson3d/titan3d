using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public partial class TtNativeWindow
    {
        // Android 没有 kernel32 的目录级原生库搜索：Core.Android 由 APK 的 lib 目录随包安装，
        // 因此只需要把 DllImport 名映射到实际文件名，交给系统的动态链接器加载。
        private static string NativeDirectory;
        private static bool NativeResolverInstalled;

        public static int SetDllDirectoryA(string path)
        {
            NativeDirectory = path;
            if (!NativeResolverInstalled)
            {
                NativeResolverInstalled = true;
                NativeLibrary.SetDllImportResolver(typeof(TtNativeWindow).Assembly, ResolveNativeLibrary);
            }
            return 1;
        }
        public static IntPtr LoadLibraryA(string path)
        {
            var fullPath = (NativeDirectory != null && !System.IO.Path.IsPathRooted(path)) ? System.IO.Path.Combine(NativeDirectory, path) : path;
            return NativeLibrary.TryLoad(fullPath, out var handle) ? handle : IntPtr.Zero;
        }
        public static int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, int uType)
        {
            // 移动端没有阻塞式消息框；写标准错误进入 logcat。
            Console.Error.WriteLine($"[{lpCaption}] {lpText}");
            return 1;
        }
        private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != CoreSDK.CoreModule)
                return IntPtr.Zero;
            // Android 的 dlopen 不补 lib 前缀和 .so 后缀，必须先试完整文件名。
            var candidates = new[] { "libCore.Android.so", "Core.Android" };
            foreach (var candidate in candidates)
            {
                if (NativeLibrary.TryLoad(candidate, out var handle))
                    return handle;
            }
            Console.Error.WriteLine($"[TitanAndroid] failed to load {libraryName} (tried {string.Join(", ", candidates)})");
            return IntPtr.Zero;
        }
    }
}
