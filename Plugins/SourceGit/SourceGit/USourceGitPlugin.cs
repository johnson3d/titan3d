using EngineNS.Profiler;
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
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
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

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = @"git.exe";
                processStartInfo.Arguments = $"add {file}";
                processStartInfo.RedirectStandardOutput = true;
                System.Diagnostics.Process result = new System.Diagnostics.Process();
                result.StartInfo = processStartInfo;
                result.Start();
                var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                result.WaitForExit();
                //result.WaitForExitAsync(timeoutSignal.Token).Wait();
                var q = new System.Text.StringBuilder();
                while (!result.HasExited)
                {
                    q.Append(result.StandardOutput.ReadToEnd());
                }
                string r = q.ToString();

                if (r == "")
                {

                }
                else
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(ELogTag.Warning, $"git add {file} returned:{r}");
                }

                //Action action = async () =>
                //{
                //    try
                //    {
                //        var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                //        await result.WaitForExitAsync(timeoutSignal.Token);

                //        var q = new System.Text.StringBuilder();
                //        while (!result.HasExited)
                //        {
                //            q.Append(result.StandardOutput.ReadToEnd());
                //        }
                //        string r = q.ToString();

                //        if (r == "")
                //        {

                //        }
                //        else
                //        {

                //        }
                //    }
                //    catch (Exception actionEx) 
                //    {
                //        Profiler.Log.WriteException(actionEx);
                //        Profiler.Log.WriteLine<Profiler.TtIOCategory>(ELogTag.Warning, $"git add {file} failed:{actionEx.Message}");
                //    }
                //};
                //action();

                return new Bricks.SourceControl.TtSourceOpResult(0);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return new Bricks.SourceControl.TtSourceOpResult(-1);
            }
        }
        public override Bricks.SourceControl.TtSourceOpResult AddDirectory(string dir)
        {
            if (IO.TtFileManager.DirectoryExists(dir) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = @"git.exe";
                processStartInfo.Arguments = $"add {dir}";
                processStartInfo.RedirectStandardOutput = true;
                System.Diagnostics.Process result = new System.Diagnostics.Process();
                result.StartInfo = processStartInfo;
                result.Start();
                Action action = async () =>
                {
                    var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await result.WaitForExitAsync(timeoutSignal.Token);

                    var q = new System.Text.StringBuilder();
                    while (!result.HasExited)
                    {
                        q.Append(result.StandardOutput.ReadToEnd());
                    }
                    string r = q.ToString();

                    if (r == "")
                    {

                    }
                    else
                    {

                    }
                };
                action();
                return new Bricks.SourceControl.TtSourceOpResult(0);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(ELogTag.Warning, $"git add {dir} failed:{ex.Message}");
                return new Bricks.SourceControl.TtSourceOpResult(-1);
            }
        }
        public override Bricks.SourceControl.TtSourceOpResult RemoveFile(string file, bool delLocal = true)
        {
            if (IO.TtFileManager.FileExists(file) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = @"git.exe";
                string arg_delLocal = delLocal ? "" : " --cached";
                processStartInfo.Arguments = $"rm{arg_delLocal} {file}";
                processStartInfo.RedirectStandardOutput = true;
                System.Diagnostics.Process result = new System.Diagnostics.Process();
                result.StartInfo = processStartInfo;
                result.Start();
                Action action = async () =>
                {
                    var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await result.WaitForExitAsync(timeoutSignal.Token);

                    var q = new System.Text.StringBuilder();
                    while (!result.HasExited)
                    {
                        q.Append(result.StandardOutput.ReadToEnd());
                    }
                    string r = q.ToString();

                    if (r == "")
                    {

                    }
                    else
                    {

                    }
                };
                action();
                return new Bricks.SourceControl.TtSourceOpResult(0);
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
                return new Bricks.SourceControl.TtSourceOpResult(-1);
            }
        }
        public override Bricks.SourceControl.TtSourceOpResult RemoveDirectory(string dir, bool delLocal = true)
        {
            if (IO.TtFileManager.FileExists(dir) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = @"git.exe";
                string arg_delLocal = delLocal ? "" : " -r --cached";
                processStartInfo.Arguments = $"rm{arg_delLocal} {dir}";
                processStartInfo.RedirectStandardOutput = true;
                System.Diagnostics.Process result = new System.Diagnostics.Process();
                result.StartInfo = processStartInfo;
                result.Start();
                Action action = async () =>
                {
                    var timeoutSignal = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await result.WaitForExitAsync(timeoutSignal.Token);

                    var q = new System.Text.StringBuilder();
                    while (!result.HasExited)
                    {
                        q.Append(result.StandardOutput.ReadToEnd());
                    }
                    string r = q.ToString();

                    if (r == "")
                    {

                    }
                    else
                    {

                    }
                };
                action();
                return new Bricks.SourceControl.TtSourceOpResult(0);
            }
            catch (Exception e)
            {
                Profiler.Log.WriteException(e);
                return new Bricks.SourceControl.TtSourceOpResult(-1);
            }
        }
    }
}
