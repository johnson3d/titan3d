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
    public class CRasterizerStateDesc : IO.BaseSerializer
    {
        public IRasterizerStateDesc Desc;
        [Rtti.Meta]
        public EngineNS.EFillMode FillMode { get => Desc.FillMode; set => Desc.FillMode = value; }
        [Rtti.Meta]
        public EngineNS.ECullMode CullMode { get => Desc.CullMode; set => Desc.CullMode = value; }
        [Rtti.Meta]
        public int FrontCounterClockwise { get => Desc.FrontCounterClockwise; set => Desc.FrontCounterClockwise = value; }
        [Rtti.Meta]
        public int DepthBias { get => Desc.DepthBias; set => Desc.DepthBias = value; }
        [Rtti.Meta]
        public float DepthBiasClamp { get => Desc.DepthBiasClamp; set => Desc.DepthBiasClamp = value; }
        [Rtti.Meta]
        public float SlopeScaledDepthBias { get => Desc.SlopeScaledDepthBias; set => Desc.SlopeScaledDepthBias = value; }
        [Rtti.Meta]
        public int DepthClipEnable { get => Desc.DepthClipEnable; set => Desc.DepthClipEnable = value; }
        [Rtti.Meta]
        public int ScissorEnable { get => Desc.ScissorEnable; set => Desc.ScissorEnable = value; }
        [Rtti.Meta]
        public int MultisampleEnable { get => Desc.MultisampleEnable; set => Desc.MultisampleEnable = value; }
        [Rtti.Meta]
        public int AntialiasedLineEnable { get => Desc.AntialiasedLineEnable; set => Desc.AntialiasedLineEnable = value; }
    }
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
