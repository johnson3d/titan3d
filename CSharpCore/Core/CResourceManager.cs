using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public interface IResource
    {
        CResourceState ResourceState
        {
            get;
        }
    }

    public class CResourceManager<T> where T : IResource
    {
        public Thread.Async.TSafeDictionary<RName, T> Resources
        {
            get;
        } = new Thread.Async.TSafeDictionary<RName, T>(new RName.EqualityComparer());
        public virtual T GetResource(CRenderContext rc, RName name, bool firstLoad = false)
        {
            return default(T);
        }
        public async virtual System.Threading.Tasks.Task<T> AwaitResource(CRenderContext rc, RName name)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return default(T);
        }
        public virtual void Tick()
        {
            TickStreaming(CEngine.Instance.RenderContext);
        }
        public Int64 ForgetTime
        {
            get;
            set;
        } = 1000 * 30;//30 seconds
        public Int64 ActiveTime
        {
            get;
            set;
        } = 100;
        public UInt64 TotalSize
        {
            get;
            set;
        }
        public bool PauseStreaming = false;
        public virtual void TickStreaming(CRenderContext rc)
        {
            if (PauseStreaming)
            {
                return;
            }
            UInt64 resSize = 0;
            var now = CEngine.Instance.EngineTime;
            var scaleForgetTime = (long)((float)ForgetTime * CEngine.Instance.EngineTimeScale);
            using (var iter = Resources.GetEnumerator())
            {
                while (true)
                {
                    try
                    {
                        bool go = iter.MoveNext();
                        if (go == false)
                            break;
                    }
                    catch
                    {
                        break;
                    }
                    var cur = iter.Current;
                    var res = cur.Value;
                    if (CanRemove(res))
                    {//外部无引用
                        Resources.Remove(cur.Key);
                        OnRemove(res);
                        break;
                    }

                    if (res.ResourceState.KeepValid == false && now - res.ResourceState.AccessTime > scaleForgetTime)
                    {
                        InvalidateResource(res);
                    }
                    else
                    {
                        if (res.ResourceState.StreamState == EStreamingState.SS_Invalid)
                        {
                            RestoreResource(res);
                            res.ResourceState.StreamState = EStreamingState.SS_Streaming;
                        }

                        OnStreamingTick(res);
                    }

                    resSize += cur.Value.ResourceState.ResourceSize;
                }
            }
            TotalSize = resSize;
        }
        protected virtual bool CanRemove(T res)
        {
            return false;
        }
        protected virtual void OnRemove(T res)
        {

        }
        protected virtual void InvalidateResource(T res)
        {

        }
        protected virtual void RestoreResource(T res)
        {

        }
        protected virtual void OnStreamingTick(T res)
        {

        }
    }
}
