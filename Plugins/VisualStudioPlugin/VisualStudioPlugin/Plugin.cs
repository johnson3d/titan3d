using Assimp;
using EnvDTE;
using EnvDTE100;
using MathNet.Numerics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class TtVisualStudioPluginAssemblyDesc : TtAssemblyDesc
        {
            public TtVisualStudioPluginAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:VisualStudioPlugin AssemblyDesc Created");
            }
            ~TtVisualStudioPluginAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:VisualStudioPlugin AssemblyDesc Destroyed");
            }
            public override string Name { get => "VisualStudioPlugin"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static TtVisualStudioPluginAssemblyDesc AssmblyDesc = new TtVisualStudioPluginAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.DataCopyer
{
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static TtVisualStudioPlugin mPluginObject = new TtVisualStudioPlugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public partial class TtVisualStudioPlugin : EngineNS.Bricks.DevIDE.TtDevIDEPlugin
    {
        internal static class NativeMethods
        {
            private const string OLEAUT32 = "oleaut32.dll";
            private const string OLE32 = "ole32.dll";

            [DllImport(OLE32, PreserveSig = false)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

            [DllImport(OLEAUT32, PreserveSig = false)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);

            [DllImport(OLE32)]
            public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

            [DllImport(OLE32)]
            public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);
        }

        public static object GetVisualStudio()
        {
            // 优先枚举运行对象表(ROT)：与 VS 版本无关，可匹配任意正在运行的 VisualStudio.DTE 实例
            // （包含 Insiders/Preview，如 18.x），避免 ProgID 猜测导致的 CO_E_CLASSSTRING。
            var fromRot = GetVisualStudioFromRot();
            if (fromRot != null)
                return fromRot;

            // 回退：按已知 ProgID 逐个尝试；某个版本未安装(CLSIDFromProgID 抛 CO_E_CLASSSTRING)时
            // 跳过继续，而不是中断整个枚举。
            string[] progIds = { "VisualStudio.DTE.18.0", "VisualStudio.DTE.17.0", "VisualStudio.DTE.16.0", "VisualStudio.DTE.15.0" };
            foreach (var id in progIds)
            {
                Guid clsid;
                try
                {
                    NativeMethods.CLSIDFromProgID(id, out clsid);
                }
                catch
                {
                    continue; // ProgID 未注册（该版本 VS 未安装）
                }

                try
                {
                    NativeMethods.GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
                    if (obj != null)
                        return obj;
                }
                catch
                {
                    continue; // 该版本无运行实例(MK_E_UNAVAILABLE)
                }
            }

            return null;
        }

        // 遍历 ROT，返回第一个显示名包含 "VisualStudio.DTE" 的运行对象。
        static object GetVisualStudioFromRot()
        {
            IRunningObjectTable rot = null;
            IEnumMoniker enumMoniker = null;
            IBindCtx bindCtx = null;
            try
            {
                if (NativeMethods.GetRunningObjectTable(0, out rot) != 0 || rot == null)
                    return null;
                rot.EnumRunning(out enumMoniker);
                if (enumMoniker == null)
                    return null;
                if (NativeMethods.CreateBindCtx(0, out bindCtx) != 0 || bindCtx == null)
                    return null;

                enumMoniker.Reset();
                var monikers = new IMoniker[1];
                while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
                {
                    var moniker = monikers[0];
                    if (moniker == null)
                        continue;
                    try
                    {
                        moniker.GetDisplayName(bindCtx, null, out var displayName);
                        if (string.IsNullOrEmpty(displayName))
                            continue;
                        // 形如 "!VisualStudio.DTE.18.0:12345"
                        if (displayName.IndexOf("VisualStudio.DTE", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (rot.GetObject(moniker, out var obj) == 0 && obj != null)
                                return obj;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(moniker);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (bindCtx != null) Marshal.ReleaseComObject(bindCtx);
                if (enumMoniker != null) Marshal.ReleaseComObject(enumMoniker);
                if (rot != null) Marshal.ReleaseComObject(rot);
            }
            return null;
        }
        public override bool OpenFileAtLine(string filePath, int lineNumber)
        {
            try
            {
                // 获取当前运行的Visual Studio实例
                DTE dte = (DTE)GetVisualStudio();
                if (dte==null)
                    return false;

                // 规范化为 Windows 路径分隔符，避免正斜杠导致 VS 打不开文件。
                filePath = filePath?.Replace('/', '\\');

                // 打开文件
                var window = dte.ItemOperations.OpenFile(filePath, EnvDTE.Constants.vsViewKindTextView);

                // 获取文本文档对象
                var textDoc = (EnvDTE.TextDocument)window.Document.Object("TextDocument");

                // 移动到指定行
                textDoc.Selection.MoveToLineAndOffset(lineNumber, 1);
                textDoc.Selection.SelectLine();
                return true;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return false;
            }
        }

        // 使用外部轻量编辑器打开并跳转到指定行：优先 VSCode（code --goto file:line），
        // 未安装 VSCode 时回退到 notepad（仅打开文件，不支持跳行）。
        public override bool OpenFileAtLineExternal(string filePath, int lineNumber)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            filePath = filePath.Replace('/', '\\');
            try
            {
                var codeExe = FindVSCodeExecutable();
                if (string.IsNullOrEmpty(codeExe) == false)
                {
                    // Code.exe / code.cmd 均支持 --goto "file:line"；用 ShellExecute 以兼容 .cmd shim。
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = codeExe,
                        Arguments = $"--goto \"{filePath}:{lineNumber}\"",
                        UseShellExecute = true,
                    });
                    return true;
                }

                // 回退：notepad 仅能打开文件，无法定位到行。
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{filePath}\"",
                    UseShellExecute = true,
                });
                return true;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return false;
            }
        }

        // 定位 VSCode 可执行文件：先查常见安装路径（用户/系统/Insiders），再查 PATH 上的 code.cmd/code.exe。
        static string FindVSCodeExecutable()
        {
            var candidates = new List<string>();
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (string.IsNullOrEmpty(local) == false)
            {
                candidates.Add(Path.Combine(local, "Programs\\Microsoft VS Code\\Code.exe"));
                candidates.Add(Path.Combine(local, "Programs\\Microsoft VS Code Insiders\\Code - Insiders.exe"));
            }
            if (string.IsNullOrEmpty(pf) == false)
                candidates.Add(Path.Combine(pf, "Microsoft VS Code\\Code.exe"));
            if (string.IsNullOrEmpty(pfx86) == false)
                candidates.Add(Path.Combine(pfx86, "Microsoft VS Code\\Code.exe"));

            foreach (var c in candidates)
            {
                if (File.Exists(c))
                    return c;
            }

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv) == false)
            {
                foreach (var dir in pathEnv.Split(Path.PathSeparator))
                {
                    try
                    {
                        var d = dir.Trim();
                        if (d.Length == 0)
                            continue;
                        var cmd = Path.Combine(d, "code.cmd");
                        if (File.Exists(cmd))
                            return cmd;
                        var exe = Path.Combine(d, "code.exe");
                        if (File.Exists(exe))
                            return exe;
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }
    }
}