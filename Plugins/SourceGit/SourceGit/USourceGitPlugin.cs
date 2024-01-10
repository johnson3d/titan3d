using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class USourceGitAssemblyDesc : UAssemblyDesc
        {
            public USourceGitAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:SourceGit AssemblyDesc Created");
            }
            ~USourceGitAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:SourceGit AssemblyDesc Destroyed");
            }
            public override string Name { get => "RpcCaller"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static USourceGitAssemblyDesc AssmblyDesc = new USourceGitAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.SourceGit
{
    public class UPluginLoader
    {
        public static URpcCallerPlugin mPluginObject = new URpcCallerPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class URpcCallerPlugin : Bricks.SourceControl.USource
    {
        public override void OnLoadedPlugin()
        {
            //AddFile("F:/TitanEngine/TestGit.txt");
            //AddFile("F:/TitanEngine/TestGit1.txt");
        }
        public override void OnUnloadPlugin()
        {
            
        }
        public override Bricks.SourceControl.USourceOpResult AddFile(string file)
        {
            if (IO.TtFileManager.FileExists(file) == false)
                return new Bricks.SourceControl.USourceOpResult(-1);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"git.exe";
            processStartInfo.Arguments = $"add {file}";

            System.Diagnostics.Process? result = null;
            try
            {
                result = System.Diagnostics.Process.Start(processStartInfo);
                var hr = new Bricks.SourceControl.USourceOpResult(0);
                if (result != null && result.StandardOutput != null)
                {
                    hr.Info = result.StandardOutput.ReadToEnd();
                }
                return hr;
            }
            catch (Exception)
            {
                return new Bricks.SourceControl.USourceOpResult(-2);
            }
        }
        public override Bricks.SourceControl.USourceOpResult AddDirectory(string dir)
        {
            return new Bricks.SourceControl.USourceOpResult(0);
        }
        public override Bricks.SourceControl.USourceOpResult RemoveFile(string file, bool delLocal = true)
        {
            if (IO.TtFileManager.FileExists(file) == false)
                return new Bricks.SourceControl.USourceOpResult(-1);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"git.exe";
            processStartInfo.Arguments = $"rm {file}";

            System.Diagnostics.Process? result = null;
            try
            {
                result = System.Diagnostics.Process.Start(processStartInfo);
                var hr = new Bricks.SourceControl.USourceOpResult(0);
                if (result != null && result.StandardOutput != null)
                {
                    hr.Info = result.StandardOutput.ReadToEnd();
                }

                if (delLocal)
                    System.IO.File.Delete(file);
                return hr;
            }
            catch (Exception)
            {
                return new Bricks.SourceControl.USourceOpResult(-2);
            }
        }
    }
}
