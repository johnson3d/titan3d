using Assimp;
using EnvDTE;
using EnvDTE100;
using MathNet.Numerics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            public static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

            [DllImport(OLE32, PreserveSig = false)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

            [DllImport(OLEAUT32, PreserveSig = false)]
            [SuppressUnmanagedCodeSecurity]
            public static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
        }

        public static object GetVisualStudio()
        {
            Guid clsid;
            string[] progId = { "VisualStudio.DTE.17.0", "VisualStudio.DTE.16.0", "VisualStudio.DTE.15.0" };

            foreach (var id in progId)
            {
                try
                {
                    NativeMethods.CLSIDFromProgIDEx(id, out clsid);
                }
                catch
                {
                    NativeMethods.CLSIDFromProgID(id, out clsid);
                }

                NativeMethods.GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
                if (obj!=null)
                {
                    // 返回第一个找到的Visual Studio实例
                    return obj;
                }
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
    }
}