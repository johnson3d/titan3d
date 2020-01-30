using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public class CShaderResources : AuxCoreObject<CShaderResources.NativePointer>
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

        public CShaderResources()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::IShaderResources");
        }

        public CShaderResources(NativePointer self)
        {
            mCoreObject = self;
            Core_AddRef();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Core_Release(true);
        }

        public UInt32 VSResourceNum
        {
            get
            {
                return SDK_IShaderResources_VSResourceNum(CoreObject);
            }
        }
        public UInt32 PSResourceNum
        {
            get
            {
                return SDK_IShaderResources_PSResourceNum(CoreObject);
            }
        }
        public void VSBindTexture(UInt32 slot, CShaderResourceView tex)
        {
            if (tex == null)
                return;
            SDK_IShaderResources_VSBindTexture(CoreObject, (byte)slot, tex.CoreObject);
        }
        public void PSBindTexture(UInt32 slot, CShaderResourceView tex)
        {
            if (tex == null)
                return;
            SDK_IShaderResources_PSBindTexture(CoreObject, (byte)slot, tex.CoreObject);
        }
        internal bool IsUserControlTexture(UInt32 slot)
        {
            if (UserControlTextures == null)
                return false;
            return UserControlTextures.ContainsKey(slot);
        }
        internal void SetUserControlTexture(UInt32 slot, bool userControl, bool reset = true)
        {
            if (userControl)
            {
                if (UserControlTextures == null)
                {
                    UserControlTextures = new Dictionary<uint, uint>();
                }
                if (reset)
                    UserControlTextures.Clear();
                UserControlTextures[slot] = slot;
            }
            else
            {
                UserControlTextures.Remove(slot);
                if (UserControlTextures.Count == 0)
                    UserControlTextures = null;
            }
        }
        private Dictionary<UInt32, UInt32> UserControlTextures = null;
        public void PSBindTexturePointer(UInt32 slot, CShaderResourceView.NativePointer texPtr)
        {
            SDK_IShaderResources_PSBindTexture(CoreObject, (byte)slot, texPtr);
        }
        public CShaderResourceView.NativePointer GetBindTextureVS(byte slot)
        {
            return SDK_IShaderResources_GetBindTextureVS(CoreObject, slot);
        }
        public CShaderResourceView.NativePointer GetBindTexturePS(byte slot)
        {
            return SDK_IShaderResources_GetBindTexturePS(CoreObject, slot);
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderResources_VSBindTexture(NativePointer self, byte slot, CShaderResourceView.NativePointer tex);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IShaderResources_PSBindTexture(NativePointer self, byte slot, CShaderResourceView.NativePointer tex);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderResources_VSResourceNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IShaderResources_PSResourceNum(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderResourceView.NativePointer SDK_IShaderResources_GetBindTextureVS(NativePointer self, byte slot);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderResourceView.NativePointer SDK_IShaderResources_GetBindTexturePS(NativePointer self, byte slot);
        #endregion
    }
}
