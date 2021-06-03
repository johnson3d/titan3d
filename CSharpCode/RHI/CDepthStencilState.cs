using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    //public unsafe partial struct IDepthStencilStateDesc
    //{
    //    public int mDepthEnable { get => mPointer->DepthEnable; set => mPointer->DepthEnable = value; }
    //    public EngineNS.EDepthWriteMask mDepthWriteMask { get => DepthWriteMask; set => DepthWriteMask = value; }
    //    public EngineNS.EComparisionMode mDepthFunc { get => DepthFunc; set => DepthFunc = value; }
    //    public int mStencilEnable { get => mPointer->StencilEnable; set => mPointer->StencilEnable = value; }
    //    public byte mStencilReadMask { get => StencilReadMask; set => StencilReadMask = value; }
    //    public byte mStencilWriteMask { get => StencilWriteMask; set => StencilWriteMask = value; }
    //    public EngineNS.StencilOpDesc mFrontFace { get => FrontFace; set => FrontFace = value; }
    //    public EngineNS.StencilOpDesc mBackFace { get => BackFace; set => BackFace = value; }
    //    public UInt32 mStencilRef { get => StencilRef; set => StencilRef = value; }
    //}
}

namespace EngineNS.RHI
{
    public class CDepthStencilStateDesc : IO.BaseSerializer
    {
        public IDepthStencilStateDesc Desc;
        [Rtti.Meta]
        public int DepthEnable { get => Desc.DepthEnable; set => Desc.DepthEnable = value; }
        [Rtti.Meta]
        public EngineNS.EDepthWriteMask DepthWriteMask { get => Desc.DepthWriteMask; set => Desc.DepthWriteMask = value; }
        [Rtti.Meta]
        public EngineNS.EComparisionMode DepthFunc { get => Desc.DepthFunc; set => Desc.DepthFunc = value; }
        [Rtti.Meta]
        public int StencilEnable { get => Desc.StencilEnable; set => Desc.StencilEnable = value; }
        [Rtti.Meta]
        public byte StencilReadMask { get => Desc.StencilReadMask; set => Desc.StencilReadMask = value; }
        [Rtti.Meta]
        public byte StencilWriteMask { get => Desc.StencilWriteMask; set => Desc.StencilWriteMask = value; }
        [Rtti.Meta]
        public EngineNS.StencilOpDesc FrontFace { get => Desc.FrontFace; set => Desc.FrontFace = value; }
        [Rtti.Meta]
        public EngineNS.StencilOpDesc BackFace { get => Desc.BackFace; set => Desc.BackFace = value; }
        [Rtti.Meta]
        public UInt32 StencilRef { get => Desc.StencilRef; set => Desc.StencilRef = value; }
    }
    public class CDepthStencilState : AuxPtrType<IDepthStencilState>
    {
        public IDepthStencilStateDesc Desc
        {
            get
            {
                var result = new IDepthStencilStateDesc();
                mCoreObject.GetDesc(ref result);
                return result;
            }
        }
    }
}
