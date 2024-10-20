using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class USourceGitAssemblyDesc : TtAssemblyDesc
        {
            public USourceGitAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:SourceGit AssemblyDesc Created");
            }
            ~USourceGitAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:SourceGit AssemblyDesc Destroyed");
            }
            public override string Name { get => "RpcCaller"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static USourceGitAssemblyDesc AssmblyDesc = new USourceGitAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace EngineNS.Plugins.SourceGit
{
    public class TtPluginLoader
    {
        public static TtSourceGitPlugin mPluginObject = new TtSourceGitPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
    public class TtSourceGitPlugin : Bricks.SourceControl.TtSource
    {
        public override void OnLoadedPlugin()
        {
            //AddFile("F:/TitanEngine/TestGit.txt");
            //AddFile("F:/TitanEngine/TestGit1.txt");
        }
        public override void OnUnloadPlugin()
        {
            
        }
        public override Bricks.SourceControl.TtSourceOpResult AddFile(string file)
        {
            if (IO.TtFileManager.FileExists(file) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"git.exe";
            processStartInfo.Arguments = $"add {file}";
            processStartInfo.RedirectStandardOutput = true;
            System.Diagnostics.Process result = new System.Diagnostics.Process();
            result.StartInfo = processStartInfo;
            result.Start();
            result.WaitForExit();

            var q = new System.Text.StringBuilder();
            while (!result.HasExited)
            {
                q.Append(result.StandardOutput.ReadToEnd());
            }
            string r = q.ToString();
            if (r == "")
                return new Bricks.SourceControl.TtSourceOpResult(0);
            else
                return new Bricks.SourceControl.TtSourceOpResult(-2);

            //ProcessStartInfo processStartInfo = new ProcessStartInfo();
            //processStartInfo.FileName = @"git.exe";
            //processStartInfo.Arguments = $"add {file}";
            //processStartInfo.RedirectStandardOutput = true;

            //System.Diagnostics.Process? result = null;
            //try
            //{
            //    result = System.Diagnostics.Process.Start(processStartInfo);
                
            //    var hr = new Bricks.SourceControl.TtSourceOpResult(0);
            //    if (result != null)
            //    {
            //        while (result.HasExited == false)
            //        {
            //            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Info, result.StandardOutput.ReadToEnd());
            //        }
            //        return hr;
            //    }
            //    return new Bricks.SourceControl.TtSourceOpResult(-1);
            //}
            //catch (Exception)
            //{
            //    return new Bricks.SourceControl.TtSourceOpResult(-2);
            //}
        }
        public override Bricks.SourceControl.TtSourceOpResult AddDirectory(string dir)
        {
            if (IO.TtFileManager.DirectoryExists(dir) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"git.exe";
            processStartInfo.Arguments = $"add {dir}";
            processStartInfo.RedirectStandardOutput = true;
            System.Diagnostics.Process result = new System.Diagnostics.Process();
            result.StartInfo = processStartInfo;
            result.Start();
            result.WaitForExit();

            var q = new System.Text.StringBuilder();
            while (!result.HasExited)
            {
                q.Append(result.StandardOutput.ReadToEnd());
            }
            string r = q.ToString();
            if (r == "")
                return new Bricks.SourceControl.TtSourceOpResult(0);
            else
                return new Bricks.SourceControl.TtSourceOpResult(-2);
        }
        public override Bricks.SourceControl.TtSourceOpResult RemoveFile(string file, bool delLocal = true)
        {
            if (IO.TtFileManager.FileExists(file) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"git.exe";
            string arg_delLocal = delLocal ? "" : " --cached";
            processStartInfo.Arguments = $"rm{arg_delLocal} {file}";
            processStartInfo.RedirectStandardOutput = true;
            System.Diagnostics.Process result = new System.Diagnostics.Process();
            result.StartInfo = processStartInfo;
            result.Start();
            result.WaitForExit();

            var q = new System.Text.StringBuilder();
            while (!result.HasExited)
            {
                q.Append(result.StandardOutput.ReadToEnd());
            }
            string r = q.ToString();
            if (r == "")
                return new Bricks.SourceControl.TtSourceOpResult(0);
            else
                return new Bricks.SourceControl.TtSourceOpResult(-2);
            //System.Diagnostics.Process? result = null;
            //try
            //{
            //    result = System.Diagnostics.Process.Start(processStartInfo);
            //    var hr = new Bricks.SourceControl.TtSourceOpResult(0);
            //    if (result != null)
            //    {
            //        //if (result.StandardOutput != null)
            //        //    hr.Info = result.StandardOutput.ReadToEnd();
            //    }

            //    if (delLocal)
            //        System.IO.File.Delete(file);
            //    return hr;
            //}
            //catch (Exception)
            //{
            //    return new Bricks.SourceControl.TtSourceOpResult(-2);
            //}
        }
        public override Bricks.SourceControl.TtSourceOpResult RemoveDirectory(string dir, bool delLocal = true)
        {
            if (IO.TtFileManager.FileExists(dir) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"git.exe";
            string arg_delLocal = delLocal ? "" : " -r --cached";
            processStartInfo.Arguments = $"rm{arg_delLocal} {dir}";
            processStartInfo.RedirectStandardOutput = true;
            System.Diagnostics.Process result = new System.Diagnostics.Process();
            result.StartInfo = processStartInfo;
            result.Start();
            result.WaitForExit();

            var q = new System.Text.StringBuilder();
            while (!result.HasExited)
            {
                q.Append(result.StandardOutput.ReadToEnd());
            }
            string r = q.ToString();
            if (r == "")
                return new Bricks.SourceControl.TtSourceOpResult(0);
            else
                return new Bricks.SourceControl.TtSourceOpResult(-2);
        }
    }
}
