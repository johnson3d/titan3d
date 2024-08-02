using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.SourceControl
{
    public struct UVersion
    {
        public ulong VersionId;
    }
    public struct USourceOpResult
    {
        public USourceOpResult(int hr)
        {
            HResult = hr;
            Info = "NotImplemented";
        }
        public int HResult;
        public string Info;
    }
    public abstract class USource : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public abstract USourceOpResult AddFile(string file);
        public abstract USourceOpResult AddDirectory(string dir);
        public abstract USourceOpResult RemoveFile(string file, bool delLocal = true);
        //public virtual async System.Threading.Tasks.Task<USourceOpResult> Pull(URepository reps)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}
        //public virtual async System.Threading.Tasks.Task<USourceOpResult> Add(URepository reps)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}
        //public virtual async System.Threading.Tasks.Task<USourceOpResult> Delete(URepository reps, bool keepLocal)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}

        //public virtual async System.Threading.Tasks.Task<USourceOpResult> Commit(URepository reps)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}
        //public virtual async System.Threading.Tasks.Task<USourceOpResult> Push(URepository reps)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}

        //public virtual async System.Threading.Tasks.Task<USourceOpResult> GetHistory(URepository reps, List<UVersion> versions)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}
        //public virtual async System.Threading.Tasks.Task<USourceOpResult> SetVersion(URepository reps, UVersion ver)
        //{
        //    await Thread.TtAsyncDummyClass.DummyFunc();
        //    return new USourceOpResult();
        //}
    }

    public class USourceControlModule : UModule<UEngine>
    {
        public USource Source { get; private set; } = null;
        public List<string> PreAddFiles = new List<string>();
        public override int GetOrder()
        {
            return 2;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var serverPlugin = UEngine.Instance.PluginModuleManager.GetPluginModule("SourceGit");
            if (serverPlugin != null)
                Source = serverPlugin.GetPluginObject<USource>();

            if (Source != null)
            {
                if (PreAddFiles.Count > 0)
                {
                    lock (PreAddFiles)
                    {
                        foreach (var i in PreAddFiles)
                        {
                            Source.AddFile(i);
                        }
                        PreAddFiles.Clear();
                    }
                }
            }
            return true;
        }
        public void AddFile(string file, bool bWaitFile = false)
        {
            if (bWaitFile)
            {
                while (IO.TtFileManager.FileExists(file) == false)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            if (Source != null)
            {
                Source.AddFile(file);
            }
            else
            {
                lock (PreAddFiles)
                {
                    PreAddFiles.Add(file);
                }
            }
        }
        public void RemoveFile(string file, bool delLocal = true)
        {
            if (Source != null)
            {
                Source.RemoveFile(file, delLocal);
            }
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.SourceControl.USourceControlModule SourceControlModule { get; } = new Bricks.SourceControl.USourceControlModule();
    }
}

