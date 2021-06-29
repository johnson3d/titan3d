using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    //public unsafe partial struct IBlendStateDesc
    //{
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget0 { get => RenderTarget[0]; set => RenderTarget[0] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget1 { get => RenderTarget[1]; set => RenderTarget[1] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget2 { get => RenderTarget[2]; set => RenderTarget[2] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget3 { get => RenderTarget[3]; set => RenderTarget[3] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget4 { get => RenderTarget[4]; set => RenderTarget[4] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget5 { get => RenderTarget[5]; set => RenderTarget[5] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget6 { get => RenderTarget[6]; set => RenderTarget[6] = value; }
    //    public EngineNS.RenderTargetBlendDesc mRenderTarget7 { get => RenderTarget[7]; set => RenderTarget[7] = value; }
    //}
}

namespace EngineNS.RHI
{
    public class CBlendState : AuxPtrType<IBlendState>
    {
        public IBlendStateDesc Desc
        {
            get
            {
                IBlendStateDesc result = new IBlendStateDesc();
                mCoreObject.GetDesc(ref result);
                return result;
            }
        }
    }
}
