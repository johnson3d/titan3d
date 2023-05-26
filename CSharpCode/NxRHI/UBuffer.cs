using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public unsafe partial struct FBuffer_SRV
    {
        const int UnionOffset = 0;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public uint FirstElement;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public uint ElementOffset;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset + 4)]
        public uint NumElements;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset + 4)]
        public uint ElementWidth;
    }
    partial struct FSrvDesc
    {
        const int UnionOffset = 8;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FBuffer_SRV Buffer;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex1D_SRV Texture1D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex1D_Array_SRV Texture1DArray;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2D_SRV Texture2D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2D_Array_SRV Texture2DArray;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2DMS_SRV Texture2DMS;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2DMS_Array_SRV Texture2DMSArray;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex3D_SRV Texture3D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTexCube_SRV TextureCube;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTexCube_Array_SRV TextureCubeArray;
    }
    partial struct FUavDesc
    {
        const int UnionOffset = 8;
        //union
        //{
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FBufferUAV Buffer;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex1DUAV Texture1D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex1DArrayUAV Texture1DArray;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2DUAV Texture2D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2DArrayUAV Texture2DArray;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex3DUAV Texture3D;
        //};
    }
    partial struct FRtvDesc
    {
        const int UnionOffset = 24;//64bit
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FBufferRTV Buffer;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex1DRTV Texture1D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex1DArrayRTV Texture1DArray;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2DRTV Texture2D;
        [System.Runtime.InteropServices.FieldOffset(UnionOffset)]
        public FTex2DArrayRTV Texture2DArray;
    }
    public interface UGpuResource : IDisposable
    {
        IGpuBufferData GetGpuBufferDataPointer();
        unsafe void Map(uint subRes, FMappedSubResource* mapped, bool forRead);
        unsafe void Umap(uint subRes);
        void TransitionTo(ICommandList cmd, EGpuResourceState state);
        void TransitionTo(UCommandList cmd, EGpuResourceState state);
        unsafe void UpdateGpuData(uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint);
        void SetDebugName(string name);
        EGpuResourceState GpuState
        {
            get;
        }
    }
    public class UBuffer : AuxPtrType<NxRHI.IBuffer>, UGpuResource
    {
        public UBuffer()
        {

        }
        public UBuffer(IBuffer ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.NativeSuper.AddRef();
        }
        public IGpuBufferData GetGpuBufferDataPointer()
        {
            return mCoreObject.NativeSuper;
        }
        public unsafe void Map(uint subRes, FMappedSubResource* mapped, bool forRead)
        {
            mCoreObject.Map(subRes, mapped, forRead);
        }
        public unsafe void Umap(uint subRes)
        {
            mCoreObject.Unmap(subRes);
        }
        public void FlushDirty(bool clear = false)
        {
            mCoreObject.FlushDirty(clear);
        }
        public unsafe void UpdateGpuData(uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint)
        {
            mCoreObject.NativeSuper.UpdateGpuData(subRes, pData, footPrint);
        }
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size, uint subRes = 0)
        {
            mCoreObject.NativeSuper.UpdateGpuDataSimple(offset, pData, size, subRes);
        }
        public void TransitionTo(ICommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd, state);
        }
        public void TransitionTo(UCommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd.mCoreObject, state);
        }
        public void SetDebugName(string name)
        {
            mCoreObject.SetDebugName(name);
        }
        public EGpuResourceState GpuState
        {
            get => mCoreObject.NativeSuper.GpuState;
        }
    }
    public class UTexture : AuxPtrType<NxRHI.ITexture>, UGpuResource
    {
        //public static UTexture CreateTexture2D(EPixelFormat format, uint w, uint h)
        //{
        //    var texDesc = new FTextureDesc();
        //    texDesc.SetDefault();
        //    texDesc.Width = w;
        //    texDesc.Height = h;
        //    texDesc.Format = format;
        //    return UEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
        //}
        public UTexture(ITexture ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.NativeSuper.AddRef();
        }
        public IGpuBufferData GetGpuBufferDataPointer()
        {
            return mCoreObject.NativeSuper;
        }
        public unsafe void Map(uint subRes, FMappedSubResource* mapped, bool forRead)
        {
            mCoreObject.Map(subRes, mapped, forRead);
        }
        public unsafe void Umap(uint subRes)
        {
            mCoreObject.Unmap(subRes);
        }
        public unsafe void UpdateGpuData(uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint)
        {
            mCoreObject.NativeSuper.UpdateGpuData(subRes, pData, footPrint);
        }
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size, uint subRes = 0)
        {
            mCoreObject.NativeSuper.UpdateGpuDataSimple(offset, pData, size, subRes);
        }
        public void TransitionTo(ICommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd, state);
        }
        public void TransitionTo(UCommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd.mCoreObject, state);
        }
        public unsafe UGpuResource CreateBufferData(uint mipIndex, ECpuAccess cpuAccess, ref EngineNS.NxRHI.FSubResourceFootPrint outFootPrint)
        {
            var device = UEngine.Instance.GfxDevice.RenderContext;
            var ptr = mCoreObject.CreateBufferData(device.mCoreObject, mipIndex, cpuAccess, ref outFootPrint);
            if (!ptr.IsValidPointer)
                return null;
            if (device.RhiType == ERhiType.RHI_D3D11)
            {
                var result = new UTexture(new ITexture(ptr.CppPointer));
                ptr.Release();
                return result;
            }
            else
            {
                var result = new UBuffer(new IBuffer(ptr.CppPointer));
                ptr.Release();
                return result;
            }
        }
        public void SetDebugName(string name)
        {
            mCoreObject.SetDebugName(name);
        }
        public EGpuResourceState GpuState
        {
            get => mCoreObject.NativeSuper.GpuState;
        }
    }
    public class UCbView : AuxPtrType<NxRHI.ICbView>
    {
        public void SetDebugName(string name)
        {

        }
        public void FlushDirty(bool clear = false)
        {
            mCoreObject.FlushDirty(clear);
        }
        public FShaderBinder ShaderBinder
        {
            get
            {
                return mCoreObject.GetShaderBinder();
            }
        }
        public void SetValue<T>(FShaderVarDesc binder, in T v) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &v)
                {
                    mCoreObject.SetValue(binder, p, sizeof(T));
                }
            }
        }
        public void SetValue<T>(FShaderVarDesc binder, int elemIndex, in T v) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &v)
                {
                    mCoreObject.SetArrrayValue(binder, elemIndex, p, sizeof(T));
                }
            }
        }
        public bool SetValue<T>(string name, in T v) where T : unmanaged
        {
            if (ShaderBinder.IsValidPointer == false)
                return false;
            var binder = ShaderBinder.FindField(name);
            if (binder.IsValidPointer == false)
                return false;
            SetValue<T>(binder, v);
            return true;
        }
        public unsafe ref T GetValue<T>(FShaderVarDesc binder) where T : unmanaged
        {
            return ref *(T*)mCoreObject.GetVarPtrToWrite(binder, (uint)sizeof(T));
        }
        public unsafe ref T GetValue<T>(FShaderVarDesc binder, int elem) where T : unmanaged
        {
            return ref *((T*)mCoreObject.GetVarPtrToWrite(binder, (uint)sizeof(T)) + elem);
        }
        public void SetMatrix(FShaderVarDesc binder, in Matrix value, int elem = 0, bool transpose = true)
        {
            if (transpose == false)
            {
                SetValue(binder, elem, value);
            }
            else
            {
                var tm = Matrix.Transpose(in value);
                SetValue(binder, elem, tm);
            }
        }
        public Matrix GetMatrix(FShaderVarDesc binder, int elem = 0, bool transpose = true)
        {
            if (transpose == false)
            {
                return GetValue<Matrix>(binder, elem);
            }
            else
            {
                var tm = GetValue<Matrix>(binder, elem);
                tm.Transpose();
                return tm;
            }
        }
    }
    public class UVbView : AuxPtrType<NxRHI.IVbView>
    {
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size)
        {
            mCoreObject.UpdateGpuData(offset, pData, size);
        }
    }
    public class UIbView : AuxPtrType<NxRHI.IIbView>
    {
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size)
        {
            mCoreObject.UpdateGpuData(offset, pData, size);
        }
    }
    public class UUaView : AuxPtrType<NxRHI.IUaView>
    {
    }
}
