using EngineNS.Profiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
        static readonly object GitLocker = new object();

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

            return RunGit(file, "add", "--", GetGitPathSpec(file));
        }
        public override Bricks.SourceControl.TtSourceOpResult AddDirectory(string dir)
        {
            if (IO.TtFileManager.DirectoryExists(dir) == false)
                return new Bricks.SourceControl.TtSourceOpResult(-1);

            return RunGit(dir, "add", "--", GetGitPathSpec(dir));
        }
        public override Bricks.SourceControl.TtSourceOpResult RemoveFile(string file, bool delLocal = true)
        {
            return delLocal ?
                RunGit(file, "rm", "-f", "--ignore-unmatch", "--", GetGitPathSpec(file, true)) :
                RunGit(file, "rm", "--cached", "-f", "--ignore-unmatch", "--", GetGitPathSpec(file, true));
        }
        public override Bricks.SourceControl.TtSourceOpResult RemoveDirectory(string dir, bool delLocal = true)
        {
            return delLocal ?
                RunGit(dir, "rm", "-r", "-f", "--ignore-unmatch", "--", GetGitPathSpec(dir, true)) :
                RunGit(dir, "rm", "-r", "--cached", "-f", "--ignore-unmatch", "--", GetGitPathSpec(dir, true));
        }

        Bricks.SourceControl.TtSourceOpResult RunGit(string path, params string[] args)
        {
            try
            {
                var repoRoot = FindRepositoryRoot(path);
                if (string.IsNullOrEmpty(repoRoot))
                    return new Bricks.SourceControl.TtSourceOpResult(-1);

                lock (GitLocker)
                {
                    var processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = @"git.exe";
                    processStartInfo.WorkingDirectory = repoRoot;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.CreateNoWindow = true;
                    foreach (var arg in args)
                    {
                        processStartInfo.ArgumentList.Add(arg);
                    }

                    using var process = new Process();
                    process.StartInfo = processStartInfo;
                    process.Start();
                    var stdoutTask = process.StandardOutput.ReadToEndAsync();
                    var stderrTask = process.StandardError.ReadToEndAsync();
                    if (!process.WaitForExit(30000))
                    {
                        try
                        {
                            process.Kill(true);
                        }
                        catch
                        {
                        }
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(ELogTag.Warning, $"git {string.Join(" ", args)} timed out");
                        return new Bricks.SourceControl.TtSourceOpResult(-1);
                    }
                    var stdout = stdoutTask.GetAwaiter().GetResult();
                    var stderr = stderrTask.GetAwaiter().GetResult();

                    if (process.ExitCode != 0)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(ELogTag.Warning, $"git {string.Join(" ", args)} failed:{stdout}{stderr}");
                        return new Bricks.SourceControl.TtSourceOpResult(process.ExitCode);
                    }

                    if (!string.IsNullOrWhiteSpace(stderr))
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(ELogTag.Info, $"git {string.Join(" ", args)} returned:{stdout}{stderr}");

                    return new Bricks.SourceControl.TtSourceOpResult(0);
                }
            }
            catch (Exception e)
            {
                Profiler.Log.WriteException(e);
                return new Bricks.SourceControl.TtSourceOpResult(-1);
            }
        }

        static string GetGitPathSpec(string path, bool ignoreCase = false)
        {
            var repoRoot = FindRepositoryRoot(path);
            if (string.IsNullOrEmpty(repoRoot))
                return path;

            var fullPath = Path.GetFullPath(path);
            var relativePath = Path.GetRelativePath(repoRoot, fullPath);
            var gitPath = relativePath.Replace('\\', '/');
            return ignoreCase ? $":(icase){gitPath}" : gitPath;
        }

        static string FindRepositoryRoot(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var fullPath = Path.GetFullPath(path);
            var dir = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath);
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(Path.Combine(dir, ".git")) || File.Exists(Path.Combine(dir, ".git")))
                    return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return string.Empty;
        }
    }
}
