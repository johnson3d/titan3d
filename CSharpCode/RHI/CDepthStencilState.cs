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
