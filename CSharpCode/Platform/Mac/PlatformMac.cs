using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EngineNS
{
    partial class TtEngine
    {
        public EPlatformType CurrentPlatform
        {
            get
            {
                return EPlatformType.PLTF_Mac;
            }
        }
    }

    public partial class TtNativeWindow
    {
        private static string NativeDirectory;
        private static bool NativeResolverInstalled;

        // 对齐 kernel32.SetDllDirectoryA：记录目录，CoreSDK.CoreModule 由 DllImportResolver 从该目录加载
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
            var fullPath = (NativeDirectory != null && !Path.IsPathRooted(path)) ? Path.Combine(NativeDirectory, path) : path;
            return NativeLibrary.TryLoad(fullPath, out var handle) ? handle : IntPtr.Zero;
        }
        public static int MessageBoxA(IntPtr hWnd, string lpText, string lpCaption, int uType)
        {
            Console.Error.WriteLine($"[{lpCaption}] {lpText}");
            return 1;
        }
        private static IntPtr ResolveNativeLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName != CoreSDK.CoreModule)
                return IntPtr.Zero;
            if (NativeDirectory == null)
            {
                Console.Error.WriteLine($"[TitanMac] native directory not initialized, skip {libraryName}");
                return IntPtr.Zero;
            }
            var handle = LoadLibraryA(libraryName);
            if (handle == IntPtr.Zero)
                Console.Error.WriteLine($"[TitanMac] failed to load {libraryName} from {NativeDirectory}");
            return handle;
        }
    }
}
