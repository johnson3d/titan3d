using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public class TtStatistic
    {
        public static readonly TtStatistic Instance = new TtStatistic();

        public Thread.TtAtomic_Int GraphicsDrawcall = new Thread.TtAtomic_Int();
        public Thread.TtAtomic_Int ComputeDrawcall = new Thread.TtAtomic_Int();
        public Thread.TtAtomic_Int TransferDrawcall = new Thread.TtAtomic_Int();
        public Thread.TtAtomic_Int ActionDrawcall = new Thread.TtAtomic_Int();
        public int NativeGraphicsDrawcall
        {
            get
            {
                return IGraphicDraw.GetNumOfInstance();
            }
        }
        public int NativeComputeDrawcall
        {
            get
            {
                return IComputeDraw.GetNumOfInstance();
            }
        }
        public int NativeTransferDrawcall
        {
            get
            {
                return ICopyDraw.GetNumOfInstance();
            }
        }
        public NxRHI.URenderCmdQueue.UQueueStat RenderCmdQueue
        {
            get
            {
                return UEngine.Instance.GfxDevice.RenderCmdQueue.GetStat();
            }
        }
    }
}
