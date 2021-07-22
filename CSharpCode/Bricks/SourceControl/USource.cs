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
    public class USource
    {
        public virtual async System.Threading.Tasks.Task<USourceOpResult> Pull(URepository reps)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }
        public virtual async System.Threading.Tasks.Task<USourceOpResult> Add(URepository reps)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }
        public virtual async System.Threading.Tasks.Task<USourceOpResult> Delete(URepository reps, bool keepLocal)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }

        public virtual async System.Threading.Tasks.Task<USourceOpResult> Commit(URepository reps)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }
        public virtual async System.Threading.Tasks.Task<USourceOpResult> Push(URepository reps)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }

        public virtual async System.Threading.Tasks.Task<USourceOpResult> GetHistory(URepository reps, List<UVersion> versions)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }
        public virtual async System.Threading.Tasks.Task<USourceOpResult> SetVersion(URepository reps, UVersion ver)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return new USourceOpResult();
        }
    }
}
