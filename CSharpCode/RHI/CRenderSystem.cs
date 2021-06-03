using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public class CRenderSystem : AuxPtrType<IRenderSystem>
    {
        public static CRenderSystem CreateRenderSystem(ERHIType rhi, ref IRenderSystemDesc desc)
        {
            var result = new CRenderSystem();
            unsafe
            {
                fixed (IRenderSystemDesc* pDesc = &desc)
                {
                    result.mCoreObject = new IRenderSystem(IRenderSystem.CreateRenderSystem(rhi, pDesc));
                }
            }
            return result;
        }
        public uint NumOfContext
        {
            get
            {
                return mCoreObject.GetContextNumber();
            }
        }
        public CRenderContext CreateContext(ref EngineNS.IRenderContextDesc desc)
        {
            var result = new CRenderContext();
            unsafe
            {
                fixed (IRenderContextDesc* pDesc = &desc)
                {
                    result.mCoreObject = new IRenderContext(mCoreObject.CreateContext(pDesc));
                }
            }
            return result;
        }
        public bool GetContextDesc(UInt32 index, ref EngineNS.IRenderContextDesc desc)
        {
            unsafe
            {
                fixed (IRenderContextDesc* pDesc = &desc)
                {
                    return mCoreObject.GetContextDesc(index, pDesc)!=0;
                }
            }
        }
    }
}
