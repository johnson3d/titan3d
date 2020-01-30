using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics
{
    public class CGfxGpuFetchManager
    {
        class FetchState
        {
            public CTexture2D Texture2D;
            public int TickFrame = 0;
            public FOnGpuFinished OnGpuFinished;
        }
        List<FetchState> WaitGpuFinishResources = new List<FetchState>();
        public delegate void FOnGpuFinished(CTexture2D srv);
        public void RegFetchTexture2D(CTexture2D tex, FOnGpuFinished cb)
        {
            for(int i = 0; i < WaitGpuFinishResources.Count; i++)
            {
                if (WaitGpuFinishResources[i].Texture2D == tex)
                    return;
            }

            var state = new FetchState();
            state.Texture2D = tex;
            state.OnGpuFinished = cb;
            state.TickFrame = 4;
            WaitGpuFinishResources.Add(state);
        }
        public void Cleanup()
        {
            //for (int i = 0; i < WaitGpuFinishResources.Count; i++)
            //{

            //}
            WaitGpuFinishResources.Clear();
        }
        public void TickSync()
        {
            var t1 = EngineNS.Support.Time.HighPrecision_GetTickCount();
            for (int i = 0; i < WaitGpuFinishResources.Count; i++)
            {
                var res = WaitGpuFinishResources[i];
                res.TickFrame--;
                if(res.TickFrame<=0)
                {
                    res.OnGpuFinished(res.Texture2D);
                    WaitGpuFinishResources.RemoveAt(i);
                    i--;
                    var t2 = EngineNS.Support.Time.HighPrecision_GetTickCount();
                    if(t2-t1>5000)
                    {
                        break;
                    }
                }
            }
        }
    }
}
