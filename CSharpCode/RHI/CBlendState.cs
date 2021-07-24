using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct IBlendStateDesc
    {
        const int SizeOfBlendDesc = 32;//sizeof(EngineNS.RenderTargetBlendDesc);
        [System.Runtime.InteropServices.FieldOffset(8)]
        public EngineNS.RenderTargetBlendDesc m_RenderTarget0;
        [System.Runtime.InteropServices.FieldOffset(8 + SizeOfBlendDesc * 1)]
        public EngineNS.RenderTargetBlendDesc m_RenderTarget1;
        [System.Runtime.InteropServices.FieldOffset(8 + SizeOfBlendDesc * 2)]
        public EngineNS.RenderTargetBlendDesc m_RenderTarget2;

        public EngineNS.RenderTargetBlendDesc RenderTarget0 
        {
            get
            {
                unsafe
                {
                    fixed (IBlendStateDesc* mPointer = &this)
                    {
                        return mPointer->m_RenderTarget0;
                    }
                }
            }
            set
            {
                unsafe
                {
                    fixed (IBlendStateDesc* mPointer = &this)
                    {
                        mPointer->m_RenderTarget0 = value;
                    }
                }
            }
        }
        public EngineNS.RenderTargetBlendDesc RenderTarget1
        {
            get
            {
                unsafe
                {
                    fixed (IBlendStateDesc* mPointer = &this)
                    {
                        return mPointer->m_RenderTarget1;
                    }
                }
            }
            set
            {
                unsafe
                {
                    fixed (IBlendStateDesc* mPointer = &this)
                    {
                        mPointer->m_RenderTarget1 = value;
                    }
                }
            }
        }
        public EngineNS.RenderTargetBlendDesc RenderTarget2
        {
            get
            {
                unsafe
                {
                    fixed (IBlendStateDesc* mPointer = &this)
                    {
                        return mPointer->m_RenderTarget2;
                    }
                }
            }
            set
            {
                unsafe
                {
                    fixed (IBlendStateDesc* mPointer = &this)
                    {
                        mPointer->m_RenderTarget2 = value;
                    }
                }
            }
        }
    }
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
