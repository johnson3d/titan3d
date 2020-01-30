using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EngineNS
{
    public enum EBlend
    {
        BLD_ZERO = 1,
        BLD_ONE = 2,
        BLD_SRC_COLOR = 3,
        BLD_INV_SRC_COLOR = 4,
        BLD_SRC_ALPHA = 5,
        BLD_INV_SRC_ALPHA = 6,
        BLD_DEST_ALPHA = 7,
        BLD_INV_DEST_ALPHA = 8,
        BLD_DEST_COLOR = 9,
        BLD_INV_DEST_COLOR = 10,
        BLD_SRC_ALPHA_SAT = 11,
        BLD_BLEND_FACTOR = 14,
        BLD_INV_BLEND_FACTOR = 15,
        BLD_SRC1_COLOR = 16,
        BLD_INV_SRC1_COLOR = 17,
        BLD_SRC1_ALPHA = 18,
        BLD_INV_SRC1_ALPHA = 19
    }
    public enum EBlendOp
    {
        BLDOP_ADD = 1,
        BLDOP_SUBTRACT = 2,
        BLDOP_REV_SUBTRACT = 3,
        BLDOP_MIN = 4,
        BLDOP_MAX = 5
    }

    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct RenderTargetBlendDesc
    {
        public void Disable()
        {
            BlendEnable = 0;
            SrcBlend = EBlend.BLD_ONE;
            DestBlend = EBlend.BLD_ZERO;
            BlendOp = EBlendOp.BLDOP_ADD;
            SrcBlendAlpha = EBlend.BLD_ZERO;
            DestBlendAlpha = EBlend.BLD_ONE;
            BlendOpAlpha = EBlendOp.BLDOP_ADD;
            RenderTargetWriteMask = 0x0F;
        }

        public void InitForOpacity()
        {
            BlendEnable = 0;
            SrcBlend = EBlend.BLD_ONE;
            DestBlend = EBlend.BLD_ZERO;
            BlendOp = EBlendOp.BLDOP_ADD;
            SrcBlendAlpha = EBlend.BLD_ZERO;
            DestBlendAlpha = EBlend.BLD_ONE;
            BlendOpAlpha = EBlendOp.BLDOP_ADD;
            RenderTargetWriteMask = 0x0F;
        }

        public void InitForTranslucency()
        {
            BlendEnable = 1;
            SrcBlend = EBlend.BLD_SRC_ALPHA;
            DestBlend = EBlend.BLD_INV_SRC_ALPHA;
            BlendOp = EBlendOp.BLDOP_ADD;
            SrcBlendAlpha = EBlend.BLD_ZERO;
            DestBlendAlpha = EBlend.BLD_ONE;
            BlendOpAlpha = EBlendOp.BLDOP_ADD;
            RenderTargetWriteMask = 0x0F;
        }

        public void InitForCustomLayers()
        {
            BlendEnable = 0;
            SrcBlend = EBlend.BLD_SRC_ALPHA;
            DestBlend = EBlend.BLD_INV_SRC_ALPHA;
            BlendOp = EBlendOp.BLDOP_ADD;
            SrcBlendAlpha = EBlend.BLD_ZERO;
            DestBlendAlpha = EBlend.BLD_ONE;
            BlendOpAlpha = EBlendOp.BLDOP_ADD;
            RenderTargetWriteMask = 0x0F;
        }

        public void InitForSnapshot()
        {
            BlendEnable = 1;
            SrcBlend = EBlend.BLD_ONE;
            DestBlend = EBlend.BLD_ZERO;
            BlendOp = EBlendOp.BLDOP_ADD;
            SrcBlendAlpha = EBlend.BLD_ONE;
            DestBlendAlpha = EBlend.BLD_ZERO;
            BlendOpAlpha = EBlendOp.BLDOP_ADD;
            RenderTargetWriteMask = 0x0F;
        }

        public void InitForShadow()
        {
            BlendEnable = 0;
            SrcBlend = EBlend.BLD_ONE;
            DestBlend = EBlend.BLD_ZERO;
            BlendOp = EBlendOp.BLDOP_ADD;
            SrcBlendAlpha = EBlend.BLD_ZERO;
            DestBlendAlpha = EBlend.BLD_ONE;
            BlendOpAlpha = EBlendOp.BLDOP_ADD;
            RenderTargetWriteMask = 0x00;
        }

        public enum EBlendType
        {
            DisableBlend,
            AlphaBlend,
            AddColor,
            SubColor,
        }

        public void SetBlendType(EBlendType value)
        {
            switch (value)
            {
                case EBlendType.DisableBlend:
                    Disable();
                    break;
                case EBlendType.AlphaBlend:
                    InitForTranslucency();
                    break;
                case EBlendType.AddColor:
                    {
                        BlendEnable = 1;
                        SrcBlend = EBlend.BLD_SRC_ALPHA;

                        DestBlend = EBlend.BLD_ONE;
                        BlendOp = EBlendOp.BLDOP_ADD;
                        SrcBlendAlpha = EBlend.BLD_ZERO;
                        DestBlendAlpha = EBlend.BLD_ONE;
                        BlendOpAlpha = EBlendOp.BLDOP_ADD;
                        RenderTargetWriteMask = 0x0F;
                    }
                    break;
                case EBlendType.SubColor:
                    {
                        BlendEnable = 1;
                        SrcBlend = EBlend.BLD_SRC_ALPHA;
                        DestBlend = EBlend.BLD_ONE;
                        BlendOp = EBlendOp.BLDOP_SUBTRACT;
                        SrcBlendAlpha = EBlend.BLD_ZERO;
                        DestBlendAlpha = EBlend.BLD_ONE;
                        BlendOpAlpha = EBlendOp.BLDOP_ADD;
                        RenderTargetWriteMask = 0x0F;
                    }
                    break;
            }
        }

        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 BlendEnable;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public EBlend SrcBlend;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public EBlend DestBlend;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public EBlendOp BlendOp;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public EBlend SrcBlendAlpha;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public EBlend DestBlendAlpha;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public EBlendOp BlendOpAlpha;
        [Category("精确设置")]
        [Editor.Editor_NotifyMemberValueChanged]
        [Editor.Editor_ShowInPropertyGrid]
        public Byte RenderTargetWriteMask;
    }
    [System.ComponentModel.TypeConverterAttribute("System.ComponentModel.ExpandableObjectConverter")]
    public struct CBlendStateDesc
    {
        public void InitForOpacity()
        {
            AlphaToCoverageEnable = 0;
            IndependentBlendEnable = 0;
            RenderTarget0.InitForOpacity();
            RenderTarget1.Disable();
            RenderTarget2.Disable();
            RenderTarget3.Disable();
            RenderTarget4.Disable();
            RenderTarget5.Disable();
            RenderTarget6.Disable();
            RenderTarget7.Disable();
        }

        public void InitForTranslucency()
        {
            AlphaToCoverageEnable = 0;
            IndependentBlendEnable = 0;
            RenderTarget0.InitForTranslucency();
            RenderTarget1.Disable();
            RenderTarget2.Disable();
            RenderTarget3.Disable();
            RenderTarget4.Disable();
            RenderTarget5.Disable();
            RenderTarget6.Disable();
            RenderTarget7.Disable();
        }

        public void InitForCustomLayers()
        {
            AlphaToCoverageEnable = 0;
            IndependentBlendEnable = 0;
            RenderTarget0.InitForCustomLayers();
            RenderTarget1.Disable();
            RenderTarget2.Disable();
            RenderTarget3.Disable();
            RenderTarget4.Disable();
            RenderTarget5.Disable();
            RenderTarget6.Disable();
            RenderTarget7.Disable();
        }

        public void InitForSnapshot()
        {
            AlphaToCoverageEnable = 0;
            IndependentBlendEnable = 0;
            RenderTarget0.InitForSnapshot();
            RenderTarget1.Disable();
            RenderTarget2.Disable();
            RenderTarget3.Disable();
            RenderTarget4.Disable();
            RenderTarget5.Disable();
            RenderTarget6.Disable();
            RenderTarget7.Disable();
        }

        public void InitForShadow()
        {
            AlphaToCoverageEnable = 0;
            IndependentBlendEnable = 0;
            RenderTarget0.InitForShadow();
            RenderTarget1.Disable();
            RenderTarget2.Disable();
            RenderTarget3.Disable();
            RenderTarget4.Disable();
            RenderTarget5.Disable();
            RenderTarget6.Disable();
            RenderTarget7.Disable();
        }
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 AlphaToCoverageEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public Int32 IndependentBlendEnable;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget0;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget1;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget2;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget3;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget4;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget5;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget6;
        [Editor.Editor_ShowInPropertyGrid]
        public RenderTargetBlendDesc RenderTarget7;
    }
    public class CBlendState : AuxCoreObject<CBlendState.NativePointer>
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
        public CBlendState(CBlendState.NativePointer self)
        {
            mCoreObject = self;
        }
        public CBlendStateDesc Desc
        {
            get
            {
                var desc = new CBlendStateDesc();
                unsafe
                {
                    SDK_IBlendState_GetDesc(CoreObject, &desc);
                }
                return desc;
            }
            set
            {
                unsafe
                {
                    SDK_IBlendState_SetDesc(CoreObject, &value);
                }
            }
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IBlendState_GetDesc(CBlendState.NativePointer self, CBlendStateDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IBlendState_SetDesc(CBlendState.NativePointer self, CBlendStateDesc* desc);
        #endregion
    }

    public class CBlendStateManager
    {
        public Dictionary<Hash64, CBlendState> States
        {
            get;
        } = new Dictionary<Hash64, CBlendState>(new Hash64.EqualityComparer());
        public CBlendState GetBlendState(CRenderContext rc, CBlendStateDesc desc)
        {
            Hash64 hash = new Hash64();
            unsafe
            {
                Hash64.CalcHash64(&hash, (byte*)&desc, sizeof(CBlendStateDesc));
            }
            CBlendState state;
            if (States.TryGetValue(hash, out state) == false)
            {
                state = rc.CreateBlendState(desc);
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
