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
    public class CBlendStateDesc : IO.BaseSerializer
    {
        public IBlendStateDesc Desc;
        [Rtti.Meta]
        public int AlphaToCoverageEnable { get => Desc.AlphaToCoverageEnable; set => Desc.AlphaToCoverageEnable = value; }
        [Rtti.Meta]
        public int IndependentBlendEnable { get => Desc.IndependentBlendEnable; set => Desc.IndependentBlendEnable = value; }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget0 
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[0];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[0] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget1
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[1];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[1] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget2
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[2];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[2] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget3
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[3];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[3] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget4
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[4];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[4] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget5
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[5];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[5] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget6
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[6];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[6] = value;
                }
            }
        }
        [Rtti.Meta]
        public EngineNS.RenderTargetBlendDesc RenderTarget7
        {
            get
            {
                unsafe
                {
                    return Desc.RenderTarget[7];
                }
            }
            set
            {
                unsafe
                {
                    Desc.RenderTarget[7] = value;
                }
            }
        }
    }
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
