using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EDepthWriteMask : int
    {
        DSWM_ZERO = 0,
        DSWM_ALL = 1
    }
    public enum EStencilOp : int
    {
        STOP_KEEP = 1,
        STOP_ZERO = 2,
        STOP_REPLACE = 3,
        STOP_INCR_SAT = 4,
        STOP_DECR_SAT = 5,
        STOP_INVERT = 6,
        STOP_INCR = 7,
        STOP_DECR = 8
    }
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct StencilOpDesc
    {
        [Editor.Editor_ShowInPropertyGrid]
        public EStencilOp StencilFailOp;
        [Editor.Editor_ShowInPropertyGrid]
        public EStencilOp StencilDepthFailOp;
        [Editor.Editor_ShowInPropertyGrid]
        public EStencilOp StencilPassOp;
        [Editor.Editor_ShowInPropertyGrid]
        public EComparisionMode StencilFunc;
    };
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct CDepthStencilStateDesc
    {
        public void InitForOpacity()
        {
            DepthEnable = 1;
            DepthWriteMask = EDepthWriteMask.DSWM_ALL;
            DepthFunc = EComparisionMode.CMP_LESS_EQUAL;
            StencilEnable = 0;
            StencilReadMask = 0xFF;
            StencilWriteMask = 0xFF;

            FrontFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            FrontFace.StencilFailOp = EStencilOp.STOP_KEEP;
            FrontFace.StencilFunc = EComparisionMode.CMP_NEVER;

            BackFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            BackFace.StencilFailOp = EStencilOp.STOP_KEEP;
            BackFace.StencilFunc = EComparisionMode.CMP_NEVER;

            StencilRef = 0;
        }

        public void InitForTranslucency()
        {
            DepthEnable = 1;
            DepthWriteMask = EDepthWriteMask.DSWM_ZERO;
            DepthFunc = EComparisionMode.CMP_LESS_EQUAL;
            StencilEnable = 0;
            StencilReadMask = 0xFF;
            StencilWriteMask = 0xFF;

            FrontFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            FrontFace.StencilFailOp = EStencilOp.STOP_KEEP;
            FrontFace.StencilFunc = EComparisionMode.CMP_NEVER;

            BackFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            BackFace.StencilFailOp = EStencilOp.STOP_KEEP;
            BackFace.StencilFunc = EComparisionMode.CMP_NEVER;

            StencilRef = 0;
        }
        
        public void InitForCustomLayers()
        {
            DepthEnable = 1;
            DepthWriteMask = EDepthWriteMask.DSWM_ALL;
            DepthFunc = EComparisionMode.CMP_LESS_EQUAL;
            StencilEnable = 0;
            StencilReadMask = 0xFF;
            StencilWriteMask = 0xFF;

            FrontFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            FrontFace.StencilFailOp = EStencilOp.STOP_KEEP;
            FrontFace.StencilFunc = EComparisionMode.CMP_NEVER;

            BackFace.StencilDepthFailOp = EStencilOp.STOP_KEEP;
            BackFace.StencilFailOp = EStencilOp.STOP_KEEP;
            BackFace.StencilFunc = EComparisionMode.CMP_NEVER;

            StencilRef = 0;
        }
        

        [Editor.Editor_ShowInPropertyGrid]
        public Int32 DepthEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public EDepthWriteMask DepthWriteMask;
        [Editor.Editor_ShowInPropertyGrid]
        public EComparisionMode DepthFunc;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 StencilEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public byte StencilReadMask;
        [Editor.Editor_ShowInPropertyGrid]
        public byte StencilWriteMask;
        [Editor.Editor_ShowInPropertyGrid]
        public StencilOpDesc FrontFace;
        [Editor.Editor_ShowInPropertyGrid]
        public StencilOpDesc BackFace;
        [Editor.Editor_ShowInPropertyGrid]
        public UInt32 StencilRef;
    }
    public class CDepthStencilState : AuxCoreObject<CDepthStencilState.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }

        public CDepthStencilState(NativePointer self)
        {
            mCoreObject = self;
        }

        public CDepthStencilStateDesc Desc
        {
            get
            {
                CDepthStencilStateDesc desc;
                unsafe
                {
                    SDK_IDepthStencilState_GetDesc(CoreObject, &desc);
                }
                return desc;
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IDepthStencilState_GetDesc(NativePointer self, CDepthStencilStateDesc* desc);
        #endregion
    }
    public class CDepthStencilStateManager
    {
        public Dictionary<Hash64, CDepthStencilState> States
        {
            get;
        } = new Dictionary<Hash64, CDepthStencilState>(new Hash64.EqualityComparer());
        public CDepthStencilState GetDepthStencilState(CRenderContext rc, CDepthStencilStateDesc desc)
        {
            Hash64 hash = new Hash64();
            unsafe
            {
                Hash64.CalcHash64(&hash, (byte*)&desc, sizeof(CDepthStencilStateDesc));
            }
            CDepthStencilState state;
            if (States.TryGetValue(hash, out state) == false)
            {
                state = rc.CreateDepthStencilState(desc);
                States.Add(hash, state);
            }
            return state;
        }
        public void Cleanup()
        {
            foreach(var i in States)
            {
                i.Value.Core_Release(true);
            }
            States.Clear();
        }
    }
}
