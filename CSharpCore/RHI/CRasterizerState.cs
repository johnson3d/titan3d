using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum EFillMode : int
    {
        FMD_WIREFRAME = 2,
        FMD_SOLID = 3
    };
    public enum ECullMode : int
    {
        CMD_NONE = 1,
        CMD_FRONT = 2,
        CMD_BACK = 3
    };
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct CRasterizerStateDesc
    {
        public void InitForOpaque()
        {
            FillMode = EFillMode.FMD_SOLID;
            CullMode = ECullMode.CMD_BACK;
            //CullMode = ECullMode.CMD_NONE;
            FrontCounterClockwise = 0;
            DepthBias = 0;
            DepthBiasClamp = 0;
            SlopeScaledDepthBias = 0;
            DepthClipEnable = 0;
            ScissorEnable = 0;
            MultisampleEnable = 0;
            AntialiasedLineEnable = 0;
        }
        
        public void InitForTranslucent()
        {
            FillMode = EFillMode.FMD_SOLID;
            CullMode = ECullMode.CMD_BACK;
            FrontCounterClockwise = 0;
            DepthBias = 0;
            DepthBiasClamp = 0;
            SlopeScaledDepthBias = 0;
            DepthClipEnable = 0;
            ScissorEnable = 0;
            MultisampleEnable = 0;
            AntialiasedLineEnable = 0;
        }

        public void InitForCustom()
        {
            FillMode = EFillMode.FMD_SOLID;
            CullMode = ECullMode.CMD_BACK;
            //CullMode = ECullMode.CMD_NONE;
            FrontCounterClockwise = 0;
            DepthBias = 0;
            DepthBiasClamp = 0;
            SlopeScaledDepthBias = 0;
            DepthClipEnable = 0;
            ScissorEnable = 0;
            MultisampleEnable = 0;
            AntialiasedLineEnable = 0;
            
        }

        public void InitForOutlineNPR()
        {
            FillMode = EFillMode.FMD_SOLID;
            CullMode = ECullMode.CMD_FRONT;
            FrontCounterClockwise = 0;
            DepthBias = 0;
            DepthBiasClamp = 0;
            SlopeScaledDepthBias = 0;
            DepthClipEnable = 0;
            ScissorEnable = 0;
            MultisampleEnable = 0;
            AntialiasedLineEnable = 0;
            
        }

        [Editor.Editor_ShowInPropertyGrid]
        public EFillMode FillMode;
        [Editor.Editor_ShowInPropertyGrid]
        public ECullMode CullMode;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 FrontCounterClockwise;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 DepthBias;
        [Editor.Editor_ShowInPropertyGrid]
        public float DepthBiasClamp;
        [Editor.Editor_ShowInPropertyGrid]
        public float SlopeScaledDepthBias;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 DepthClipEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 ScissorEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 MultisampleEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 AntialiasedLineEnable;
        
    }
    public class CRasterizerState : AuxCoreObject<CRasterizerState.NativePointer>
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

        public CRasterizerState(NativePointer self)
        {
            mCoreObject = self;
        }

        public CRasterizerStateDesc Desc
        {
            get
            {
                CRasterizerStateDesc desc;
                unsafe
                {
                    SDK_IRasterizerState_GetDesc(CoreObject, &desc);
                }
                return desc;
            }
        }

        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IRasterizerState_GetDesc(NativePointer self, CRasterizerStateDesc* desc);
        #endregion
    }

    public class CRasterizerStateManager
    {
        public Dictionary<Hash64, CRasterizerState> States
        {
            get;
        } = new Dictionary<Hash64, CRasterizerState>(new Hash64.EqualityComparer());
        public CRasterizerState GetRasterizerState(CRenderContext rc, CRasterizerStateDesc desc)
        {
            Hash64 hash = new Hash64();
            unsafe
            {
                Hash64.CalcHash64(&hash, (byte*)&desc, sizeof(CRasterizerStateDesc));
            }
            CRasterizerState state;
            if(States.TryGetValue(hash, out state)==false)
            {
                state = rc.CreateRasterizerState(desc);
                States.Add(hash, state);
            }
            return state;
        }
        public void Cleanup()
        {
            foreach (var i in States.Values)
            {
                i.Core_Release(true);
            }
            States.Clear();
        }
    }
}
