using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    //public unsafe partial struct IRasterizerStateDesc
    //{
    //    public EngineNS.EFillMode mFillMode { get => FillMode; set => FillMode = value; }
    //    public EngineNS.ECullMode mCullMode { get => CullMode; set => CullMode = value; }
    //    public int mFrontCounterClockwise { get => mPointer->FrontCounterClockwise; set => mPointer->FrontCounterClockwise = value; }
    //    public int mDepthBias { get => DepthBias; set => DepthBias = value; }
    //    public float mDepthBiasClamp { get => DepthBiasClamp; set => DepthBiasClamp = value; }
    //    public float mSlopeScaledDepthBias { get => SlopeScaledDepthBias; set => SlopeScaledDepthBias = value; }
    //    public int mDepthClipEnable { get => mPointer->DepthClipEnable; set => mPointer->DepthClipEnable = value; }
    //    public int mScissorEnable { get => mPointer->ScissorEnable; set => mPointer->ScissorEnable = value; }
    //    public int mMultisampleEnable { get => mPointer->MultisampleEnable; set => mPointer->MultisampleEnable = value; }
    //    public int mAntialiasedLineEnable { get => mPointer->AntialiasedLineEnable; set => mPointer->AntialiasedLineEnable = value; }
    //}
}

namespace EngineNS.RHI
{
    public class CRasterizerState : AuxPtrType<IRasterizerState>
    {
        public IRasterizerStateDesc Desc
        {
            get
            {
                var result = new IRasterizerStateDesc();
                mCoreObject.GetDesc(ref result);
                return result;
            }
        }
    }
}
