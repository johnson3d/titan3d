using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.SourceControl
{
    public struct TtVersion
    {
        public ulong VersionId;
    }
    public struct TtSourceOpResult
    {
        public TtSourceOpResult(int hr)
        {
            HResult = hr;
            Info = "NotImplemented";
        }
        public int HResult;
        public string Info;
    }
    public abstract class TtSource : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public abstract TtSourceOpResult AddFile(string file);
        public abstract TtSourceOpResult AddDirectory(string dir);
        public abstract TtSourceOpResult RemoveFile(string file, bool delLocal = true);
        public abstract TtSourceOpResult RemoveDirectory(string dir, bool delLocal = true);
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

    public class TtSourceControlModule : TtModule<TtEngine>
    {
        public TtSource Source { get; private set; } = null;
        public List<string> PreAddFiles = new List<string>();
        public override int GetOrder()
        {
            return 2;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(TtEngine host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("SourceGit");
            if (serverPlugin != null)
                Source = serverPlugin.GetPluginObject<TtSource>();

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
    partial class TtEngine
    {
        public Bricks.SourceControl.TtSourceControlModule SourceControlModule { get; } = new Bricks.SourceControl.TtSourceControlModule();
    }
}

