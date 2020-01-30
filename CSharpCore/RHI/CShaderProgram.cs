using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum ECBufferRhiType
    {
        SIT_CBUFFER = 0,
        SIT_TBUFFER = (SIT_CBUFFER + 1),
        SIT_TEXTURE = (SIT_TBUFFER + 1),
        SIT_SAMPLER = (SIT_TEXTURE + 1),
        SIT_UAV_RWTYPED = (SIT_SAMPLER + 1),
        SIT_STRUCTURED = (SIT_UAV_RWTYPED + 1),
        SIT_UAV_RWSTRUCTURED = (SIT_STRUCTURED + 1),
        SIT_BYTEADDRESS = (SIT_UAV_RWSTRUCTURED + 1),
        SIT_UAV_RWBYTEADDRESS = (SIT_BYTEADDRESS + 1),
        SIT_UAV_APPEND_STRUCTURED = (SIT_UAV_RWBYTEADDRESS + 1),
        SIT_UAV_CONSUME_STRUCTURED = (SIT_UAV_APPEND_STRUCTURED + 1),
        SIT_UAV_RWSTRUCTURED_WITH_COUNTER = (SIT_UAV_CONSUME_STRUCTURED + 1),
    }
    public struct CConstantBufferDesc
    {
        public ECBufferRhiType Type;
        public UInt32 Size;
        public UInt32 VSBindPoint;
        public UInt32 PSBindPoint;
        public UInt32 CSBindPoint;
        public UInt32 BindCount;
        public UInt32 CPUAccess;
    }
    public struct CTextureBindInfo
    {
        public ECBufferRhiType Type;
        public UInt32 VSBindPoint;
        public UInt32 PSBindPoint;
        public UInt32 CSBindPoint;
        public UInt32 BindCount;
    }
    public struct CSamplerBindInfo
    {
        public ECBufferRhiType Type;
        public UInt32 VSBindPoint;
        public UInt32 PSBindPoint;
        public UInt32 CSBindPoint;
        public UInt32 BindCount;
    }
    public struct CShaderProgramDesc
    {

    };
    public class CShaderProgram : AuxCoreObject<CShaderProgram.NativePointer>
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
       // private GCHandle WeakHandle;
        public CShaderProgram(NativePointer self)
        {
            mCoreObject = self;
            //WeakHandle = GCHandle.Alloc(this, GCHandleType.WeakTrackResurrection);
            //var userData = System.Runtime.InteropServices.GCHandle.ToIntPtr(WeakHandle);
            //SDK_IShaderProgram_SetUserData(CoreObject, userData);
        }
        ~CShaderProgram()
        {
            //SDK_IShaderProgram_SetUserData(CoreObject, IntPtr.Zero);
            //WeakHandle.Free();
        }
        public static CShaderProgram GetShaderProgramFormPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(ptr);
            return handle.Target as CShaderProgram;
        }
        public override void Cleanup()
        {
            if (mVertexShader != null)
            {
                mVertexShader.Cleanup();
                mVertexShader = null;
            }
            if (mPixelShader != null)
            {
                mPixelShader.Cleanup();
                mPixelShader = null;
            }
            if (mInputLayout != null)
            {
                mInputLayout.Cleanup();
                mInputLayout = null;
            }
            base.Cleanup();
        }
        CVertexShader mVertexShader = null;
        public CVertexShader VertexShader
        {
            get { return mVertexShader; }
            set
            {
                mVertexShader = value;
                if (value == null)
                {
                    CVertexShader.NativePointer tmp;
                    tmp.Pointer = IntPtr.Zero;
                    SDK_IShaderProgram_BindVertexShader(CoreObject, tmp);
                }
                else
                    SDK_IShaderProgram_BindVertexShader(CoreObject, value.CoreObject);
            }
        }
        CPixelShader mPixelShader = null;
        public CPixelShader PixelShader
        {
            get { return mPixelShader; }
            set
            {
                mPixelShader = value;
                if (value == null)
                {
                    CPixelShader.NativePointer tmp;
                    tmp.Pointer = IntPtr.Zero;
                    SDK_IShaderProgram_BindPixelShader(CoreObject, tmp);
                }
                else
                    SDK_IShaderProgram_BindPixelShader(CoreObject, value.CoreObject);
            }
        }

        CInputLayout mInputLayout = null;
        public CInputLayout InputLayout
        {
            get { return mInputLayout; }
            set
            {
                mInputLayout = value;
                if (value == null)
                {
                    CInputLayout.NativePointer tmp;
                    tmp.Pointer = IntPtr.Zero;
                    SDK_IShaderProgram_BindInputLayout(CoreObject, tmp);
                }
                else
                    SDK_IShaderProgram_BindInputLayout(CoreObject, value.CoreObject);
            }
        }
        public bool LinkShaders(CRenderContext rc)
        {
            return SDK_IShaderProgram_LinkShaders(CoreObject, rc.CoreObject);
        }

        public UInt32 CBufferNumber
        {
            get
            {
                return SDK_IShaderProgram_GetCBufferNumber(CoreObject);
            }
        }
        public UInt32 FindCBuffer(string name)
        {
            return SDK_IShaderProgram_FindCBuffer(CoreObject, name);
        }
        public bool GetCBufferDesc(UInt32 index, ref CConstantBufferDesc desc)
        {
            unsafe
            {
                fixed (CConstantBufferDesc* p = &desc)
                {
                    return (bool)SDK_IShaderProgram_GetCBufferDesc(CoreObject, index, p);
                }
            }
        }

        public UInt32 ShaderResourceNumber
        {
            get
            {
                return SDK_IShaderProgram_GetShaderResourceNumber(CoreObject);
            }
        }
        public bool FindTextureBindInfo(Graphics.CGfxMaterialInstance mtl, string name, ref CTextureBindInfo desc)
        {
            if (mtl != null)
            {
                //name = Graphics.CGfxMaterialManager.GetValidShaderVarName(name, mtl.Material.GetHash64().ToString());
                name = Graphics.CGfxMaterialManager.GetValidShaderVarName(name, mtl.Material.Hash64String);
            }

            var index = SDK_IShaderProgram_GetTextureBindSlotIndex(CoreObject, name);
            if ((int)index < 0)
                return false;
            GetTextureBindDesc(index, ref desc);
            return true;
        }
        public UInt32 FindTextureIndexPS(Graphics.CGfxMaterialInstance mtl, string name)
        {
            CTextureBindInfo desc = new CTextureBindInfo();
            if (FindTextureBindInfo(mtl, name, ref desc) == false)
                return UInt32.MaxValue;
            return desc.PSBindPoint;
        }
        public bool GetTextureBindDesc(UInt32 index, ref CTextureBindInfo desc)
        {
            unsafe
            {
                fixed (CTextureBindInfo* p = &desc)
                {
                    return (bool)SDK_IShaderProgram_GetSRBindDesc(CoreObject, index, p);
                }
            }
        }

        public UInt32 SamplerNumber
        {
            get
            {
                return SDK_IShaderProgram_GetSamplerNumber(CoreObject);
            }
        }
        public bool FindSamplerBindInfo(Graphics.CGfxMaterialInstance mtl, string name, ref CSamplerBindInfo desc)
        {
            if(mtl!=null)
                name = Graphics.CGfxMaterialManager.GetValidShaderVarName(name, mtl.Material.GetHash64().ToString());
            var index = SDK_IShaderProgram_GetSamplerBindSlotIndex(CoreObject, name);
            if ((int)index < 0)
                return false;
            GetSamplerBindInfo(index, ref desc);
            return true;
        }
        public bool FindSamplerBindInfoByShaderName(Graphics.CGfxMaterialInstance mtl, string name, ref CSamplerBindInfo desc)
        {
            if (mtl != null)
                name = "Samp_" + name;
            var index = SDK_IShaderProgram_GetSamplerBindSlotIndex(CoreObject, name);
            if ((int)index < 0)
                return false;
            GetSamplerBindInfo(index, ref desc);
            return true;
        }
        public bool GetSamplerBindInfo(UInt32 Index, ref CSamplerBindInfo desc)
        {
            unsafe
            {
                fixed (CSamplerBindInfo* p = &desc)
                {
                    return (bool)SDK_IShaderProgram_GetSampBindDesc(CoreObject, Index, p);
                }
            }
        }

        #region SDK
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static IntPtr SDK_IShaderProgram_GetUserData(NativePointer self);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //public extern static void SDK_IShaderProgram_SetUserData(NativePointer self, IntPtr userData);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderProgram_BindVertexShader(NativePointer self, CVertexShader.NativePointer vs);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderProgram_BindPixelShader(NativePointer self, CPixelShader.NativePointer ps);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderProgram_BindInputLayout(NativePointer self, CInputLayout.NativePointer layout);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static vBOOL SDK_IShaderProgram_LinkShaders(NativePointer self, CRenderContext.NativePointer rc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderProgram_FindCBuffer(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderProgram_GetCBufferNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IShaderProgram_GetCBufferDesc(NativePointer self, UInt32 index, CConstantBufferDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderProgram_GetShaderResourceNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderProgram_GetTextureBindSlotIndex(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IShaderProgram_GetSRBindDesc(NativePointer self, UInt32 Index, CTextureBindInfo* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderProgram_GetSamplerNumber(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderProgram_GetSamplerBindSlotIndex(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe vBOOL SDK_IShaderProgram_GetSampBindDesc(NativePointer self, UInt32 Index, CSamplerBindInfo* desc);
        #endregion
    }
}
