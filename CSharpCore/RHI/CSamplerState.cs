using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace EngineNS
{
    public enum ESamplerFilter
    {
        SPF_MIN_MAG_MIP_POINT = 0,
        SPF_MIN_MAG_POINT_MIP_LINEAR = 0x1,
        SPF_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x4,
        SPF_MIN_POINT_MAG_MIP_LINEAR = 0x5,
        SPF_MIN_LINEAR_MAG_MIP_POINT = 0x10,
        SPF_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x11,
        SPF_MIN_MAG_LINEAR_MIP_POINT = 0x14,
        SPF_MIN_MAG_MIP_LINEAR = 0x15,
        SPF_ANISOTROPIC = 0x55,
        SPF_COMPARISON_MIN_MAG_MIP_POINT = 0x80,
        SPF_COMPARISON_MIN_MAG_POINT_MIP_LINEAR = 0x81,
        SPF_COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x84,
        SPF_COMPARISON_MIN_POINT_MAG_MIP_LINEAR = 0x85,
        SPF_COMPARISON_MIN_LINEAR_MAG_MIP_POINT = 0x90,
        SPF_COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x91,
        SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT = 0x94,
        SPF_COMPARISON_MIN_MAG_MIP_LINEAR = 0x95,
        SPF_COMPARISON_ANISOTROPIC = 0xd5,
        SPF_MINIMUM_MIN_MAG_MIP_POINT = 0x100,
        SPF_MINIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x101,
        SPF_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x104,
        SPF_MINIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x105,
        SPF_MINIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x110,
        SPF_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x111,
        SPF_MINIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x114,
        SPF_MINIMUM_MIN_MAG_MIP_LINEAR = 0x115,
        SPF_MINIMUM_ANISOTROPIC = 0x155,
        SPF_MAXIMUM_MIN_MAG_MIP_POINT = 0x180,
        SPF_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x181,
        SPF_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x184,
        SPF_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x185,
        SPF_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x190,
        SPF_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x191,
        SPF_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x194,
        SPF_MAXIMUM_MIN_MAG_MIP_LINEAR = 0x195,
        SPF_MAXIMUM_ANISOTROPIC = 0x1d5
    }
    public enum EAddressMode
    {
        ADM_WRAP = 1,
        ADM_MIRROR,
        ADM_CLAMP,
        ADM_BORDER,
        ADM_MIRROR_ONCE,
    }
    public enum EComparisionMode
    {
        CMP_NEVER = 1,
        CMP_LESS = 2,
        CMP_EQUAL = 3,
        CMP_LESS_EQUAL = 4,
        CMP_GREATER = 5,
        CMP_NOT_EQUAL = 6,
        CMP_GREATER_EQUAL = 7,
        CMP_ALWAYS = 8
    }
    public struct CSamplerStateDesc
    {
        public void SetDefault()
        {
            Filter = ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
            CmpMode = EComparisionMode.CMP_NEVER;
            AddressU = EAddressMode.ADM_WRAP;
            AddressV = EAddressMode.ADM_WRAP;
            AddressW = EAddressMode.ADM_WRAP;
            MaxAnisotropy = 0;
            MipLODBias = 0;
            BorderColor = new Color4(0, 0, 0, 0);
            MinLOD = 0;
            MaxLOD = 3.402823466e+38f;
        }
        public override string ToString()
        {
            var props = this.GetType().GetProperties();
            string result = "CSamplerStateDesc:\n";
            foreach(var i in props)
            {
                result += i.Name + " = " + i.GetValue(this).ToString() + "\n";
            }
            return result;
        }

        private ESamplerFilter mFilter;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public ESamplerFilter Filter { get => mFilter; set => mFilter = value; }

        private EComparisionMode mCmpMode;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public EComparisionMode CmpMode { get => mCmpMode; set => mCmpMode = value; }

        private EAddressMode mAddressU;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public EAddressMode AddressU { get => mAddressU; set => mAddressU = value; }

        private EAddressMode mAddressV;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public EAddressMode AddressV { get => mAddressV; set => mAddressV = value; }

        private EAddressMode mAddressW;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public EAddressMode AddressW { get => mAddressW; set => mAddressW = value; }

        private UInt32 mMaxAnisotropy;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public uint MaxAnisotropy { get => mMaxAnisotropy; set => mMaxAnisotropy = value; }

        private float mMipLODBias;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public float MipLODBias { get => mMipLODBias; set => mMipLODBias = value; }

        private Color4 mBorderColor;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public Color4 BorderColor { get => mBorderColor; set => mBorderColor = value; }

        private float mMinLOD;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public float MinLOD { get => mMinLOD; set => mMinLOD = value; }

        private float mMaxLOD;
        [Editor.Editor_ShowInPropertyGridAttribute]
        public float MaxLOD { get => mMaxLOD; set => mMaxLOD = value; }

    }
    public class CSamplerState : AuxCoreObject<CSamplerState.NativePointer>
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

        public CSamplerState(NativePointer self)
        {
            mCoreObject = self;
        }

        #region SDK

        #endregion
    }
    public class CSamplerStateManager
    {
        public Dictionary<Hash64, CSamplerState> States
        {
            get;
        } = new Dictionary<Hash64, CSamplerState>(new Hash64.EqualityComparer());
        public CSamplerState GetSamplerState(CRenderContext rc, CSamplerStateDesc desc)
        {
            Hash64 hash = new Hash64();
            unsafe
            {
                Hash64.CalcHash64(&hash, (byte*)&desc, sizeof(CSamplerStateDesc));
            }
            CSamplerState state;
            if (States.TryGetValue(hash, out state) == false)
            {
                state = rc.CreateSamplerState(desc);
                if (state == null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Graphics", $"SamplerState create faild=>{desc.ToString()}");
                    desc.SetDefault();
                    state = rc.CreateSamplerState(desc);
                }
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
