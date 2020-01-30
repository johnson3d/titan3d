using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics
{
    public class CGfxTextureManager
    {
        public Thread.Async.TSafeDictionary<RName, CShaderResourceView> Textures
        {
            get;
        } = new Thread.Async.TSafeDictionary<RName, CShaderResourceView>(new RName.EqualityComparer());
        public CShaderResourceView GetShaderRView(CRenderContext rc, RName name, bool firstLoad = false)
        {
            if (name == null)
                return null;
            CShaderResourceView result;
            if(false == Textures.TryGetValue(name, out result))
            {
                result = rc.LoadShaderResourceView(name);
                if (result == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Resource", $"Texture {name} load failed");
                    return null;
                }
                if(firstLoad)
                {
                    result.ResourceState.StreamState = EStreamingState.SS_Streaming;
                    if (false == result.RestoreResource())
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Texture", $"Texture {result} Restore Resource Failed");
                        result.ResourceState.StreamState = EStreamingState.SS_Invalid;
                    }
                    else
                    {
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Texture", $"Texture Name is empty");
                        result.ResourceState.StreamState = EStreamingState.SS_Valid;
                    }
                }
                result.Name = name;
                Textures.Add(name, result);
            }
            result.ResourceState.AccessTime = CEngine.Instance.EngineTime;

            return result.CloneTexture();
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
        public void Tick()
        {

        }
        private List<KeyValuePair<Thread.ASyncSemaphore, CShaderResourceView>> AwaitingRSV = new List<KeyValuePair<Thread.ASyncSemaphore, CShaderResourceView>>();
        public async System.Threading.Tasks.Task<CShaderResourceView> AwaitTextureValid(CRenderContext rc, RName name)
        {
            var result = GetShaderRView(rc, name, false);
            result.PreUse(false);
            if (result.ResourceState.StreamState == EStreamingState.SS_Valid)
                return result;

            var smp = Thread.ASyncSemaphore.CreateSemaphore(1);
            lock (AwaitingRSV)
            {
                AwaitingRSV.Add(new KeyValuePair<Thread.ASyncSemaphore, CShaderResourceView>(smp, result));
            }
            await smp.Await();
            return result;
        }
        public bool PauseStreaming = false;
        public int WaitStreamingCount = 0;
        public bool PauseKickResource = false;
        public void TickStreaming(CRenderContext rc)
        {
            if (PauseStreaming)
            {
                return;
            }
            int needStream = 0;
            UInt64 resSize = 0;
            var now = CEngine.Instance.EngineTime;
            var scaleForgetTime = (long)((float)ForgetTime * CEngine.Instance.EngineTimeScale);
            using (var iter = Textures.GetEnumerator())
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
                    KeyValuePair<RName, CShaderResourceView> cur = iter.Current;
                    var texture = cur.Value;
                    if (texture.Core_GetRef() == 1)
                    {//外部无引用
                        Textures.Remove(cur.Key);
                        texture.Cleanup();
                        break;
                    }

                    if (PauseKickResource==false && texture.ResourceState.KeepValid==false && now - texture.ResourceState.AccessTime > scaleForgetTime)
                    {
                        if (texture.ResourceState.StreamState == EStreamingState.SS_Valid)
                        {
                            texture.InvalidateResource();
                            texture.ResourceState.StreamState = EStreamingState.SS_Invalid;
                        }
                    }
                    else
                    {
                        if(texture.ResourceState.StreamState == EStreamingState.SS_Invalid)
                        {
                            texture.RestoreResource();
                            texture.ResourceState.StreamState = EStreamingState.SS_Streaming;
                        }
                        if (texture.TexSteaming.LoadedMipCount < texture.TexSteaming.FullMipCount)
                        {
                            texture.TexSteaming.LoadNextMip(rc, texture);
                        }
                        if(texture.TexSteaming.LoadedMipCount < texture.TexSteaming.FullMipCount)
                        {
                            needStream++;
                        }
                        else
                        {
                            if (texture.ResourceState.StreamState != EStreamingState.SS_Valid)
                            {//没有意义，方便调试而已
                                texture.ResourceState.StreamState = EStreamingState.SS_Valid;
                            }
                            lock (AwaitingRSV)
                            {
                                for (int i = 0; i < AwaitingRSV.Count; i++)
                                {
                                    if (AwaitingRSV[i].Value == texture)
                                    {
                                        AwaitingRSV[i].Key.Release();
                                        AwaitingRSV.RemoveAt(i);
                                        i--;
                                    }
                                    else
                                    {
                                        //防止塞入awaitingRsv的时候，正好加载完错过了Release的时机
                                        if (AwaitingRSV[i].Value.ResourceState.StreamState == EStreamingState.SS_Valid)
                                        {
                                            AwaitingRSV[i].Key.Release();
                                            AwaitingRSV.RemoveAt(i);
                                            i--;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    resSize += cur.Value.ResourceState.ResourceSize;
                }
            }
            TotalSize = resSize;
            WaitStreamingCount = needStream;
        }
    }
}
