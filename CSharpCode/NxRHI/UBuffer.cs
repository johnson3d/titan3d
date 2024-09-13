﻿using System;
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
    public interface TtGpuResource : IDisposable
    {
        IGpuBufferData GetGpuBufferDataPointer();
        unsafe void Map(uint subRes, FMappedSubResource* mapped, bool forRead);
        unsafe void Umap(uint subRes);
        void TransitionTo(ICommandList cmd, EGpuResourceState state);
        void TransitionTo(UCommandList cmd, EGpuResourceState state);
        unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint);
        void SetDebugName(string name);
        EGpuResourceState GpuState
        {
            get;
        }
        NxRHI.IBuffer CreateReadable(int subRes, EngineNS.NxRHI.ICopyDraw cpDraw);
    }
    public class TtBuffer : AuxPtrType<NxRHI.IBuffer>, TtGpuResource
    {
        public TtBuffer()
        {

        }
        public TtBuffer(IBuffer ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.NativeSuper.AddRef();
        }
        public void SetDebugName(string name)
        {
            mCoreObject.SetDebugName(name);
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
        public void FlushDirty(NxRHI.ICommandList cmd, bool clear = false)
        {
            mCoreObject.FlushDirty(cmd, clear);
        }
        public unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint)
        {
            mCoreObject.NativeSuper.UpdateGpuData(cmd, subRes, pData, footPrint);
        }
        public unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint offset, void* pData, uint size, uint subRes = 0)
        {
            mCoreObject.NativeSuper.UpdateGpuDataSimple(cmd, offset, pData, size, subRes);
        }
        public unsafe void UpdateGpuData(uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint)
        {
            mCoreObject.NativeSuper.UpdateGpuData(subRes, pData, footPrint);
        }
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size, uint subRes = 0)
        {
            mCoreObject.NativeSuper.UpdateGpuDataSimple(offset, pData, size, subRes);
        }
        public bool FetchGpuData(uint index, EngineNS.IBlobObject blob)
        {
            return mCoreObject.FetchGpuData(index, blob);
        }
        public NxRHI.IBuffer CreateReadable(int subRes, EngineNS.NxRHI.ICopyDraw cpDraw)
        {
            return mCoreObject.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, subRes, cpDraw);
        }
        public void TransitionTo(ICommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd, state);
        }
        public void TransitionTo(UCommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd.mCoreObject, state);
        }
        public EGpuResourceState GpuState
        {
            get => mCoreObject.NativeSuper.GpuState;
        }
    }
    public class TtTransientBuffer : AuxPtrType<NxRHI.FTransientBuffer>
    {
        public TtTransientBuffer()
        {
            mCoreObject = FTransientBuffer.CreateInstance();
        }
        public void Initialize(uint size, EngineNS.NxRHI.EBufferType type = EBufferType.BFT_Vertex | EBufferType.BFT_Index,
            EngineNS.NxRHI.EGpuUsage usage = EGpuUsage.USAGE_DEFAULT, EngineNS.NxRHI.ECpuAccess cpuAccess = (ECpuAccess)0)
        {
            mCoreObject.Initialize(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, size, type, usage, cpuAccess);
        }
        public uint Alloc(uint size, bool bGrow)
        {
            return mCoreObject.Alloc(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, size, bGrow);
        }
        public TtVbView AllocVBV(uint stride, uint size, bool bGrow)
        {
            var ptr = mCoreObject.AllocVBV(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, stride, size, bGrow);
            return new TtVbView(ptr);
        }
        public TtIbView AllocIBV(uint stride, uint size, bool bGrow)
        {
            var ptr = mCoreObject.AllocIBV(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, stride, size, bGrow);
            return new TtIbView(ptr);
        }
        public void Reset()
        {
            mCoreObject.Reset();
        }
        public IBuffer GetBuffer()
        {
            return mCoreObject.GetBuffer();
        }
    }
    public class TtTexture : AuxPtrType<NxRHI.ITexture>, TtGpuResource
    {
        //public static UTexture CreateTexture2D(EPixelFormat format, uint w, uint h)
        //{
        //    var texDesc = new FTextureDesc();
        //    texDesc.SetDefault();
        //    texDesc.Width = w;
        //    texDesc.Height = h;
        //    texDesc.Format = format;
        //    return TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
        //}
        public TtTexture(ITexture ptr)
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
        public unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint)
        {
            mCoreObject.NativeSuper.UpdateGpuData(cmd, subRes, pData, footPrint);
        }
        public unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint offset, void* pData, uint size, uint subRes = 0)
        {
            mCoreObject.NativeSuper.UpdateGpuDataSimple(cmd, offset, pData, size, subRes);
        }
        public bool FetchGpuData(uint index, EngineNS.IBlobObject blob)
        {
            return mCoreObject.FetchGpuData(index, blob);
        }
        public NxRHI.IBuffer CreateReadable(int subRes, EngineNS.NxRHI.ICopyDraw cpDraw)
        {
            return mCoreObject.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, subRes, cpDraw);
        }
        public void TransitionTo(ICommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd, state);
        }
        public void TransitionTo(UCommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd.mCoreObject, state);
        }
        public unsafe TtGpuResource CreateBufferData(uint mipIndex, ECpuAccess cpuAccess, ref EngineNS.NxRHI.FSubResourceFootPrint outFootPrint)
        {
            var device = TtEngine.Instance.GfxDevice.RenderContext;
            var ptr = mCoreObject.CreateBufferData(device.mCoreObject, mipIndex, cpuAccess, ref outFootPrint);
            if (!ptr.IsValidPointer)
                return null;
            if (device.RhiType == ERhiType.RHI_D3D11)
            {
                var result = new TtTexture(new ITexture(ptr.CppPointer));
                ptr.Release();
                return result;
            }
            else
            {
                var result = new TtBuffer(new IBuffer(ptr.CppPointer));
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
    public class TtCbView : AuxPtrType<NxRHI.ICbView>
    {
        public void SetDebugName(string name)
        {
            mCoreObject.NativeSuper.SetDebugName(name);
        }
        public void FlushDirty(NxRHI.ICommandList cmd, bool clear = false)
        {
            mCoreObject.FlushDirty(cmd, clear);
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
        public enum EUpdateMode
        {
            Auto = 0,
            Immediately,
        }
        public void SetValue<T>(FShaderVarDesc binder, in T v, bool bFlush = true, EUpdateMode mode = EUpdateMode.Auto) where T : unmanaged
        {
            if (binder.IsValidPointer == false)
                return;
            unsafe
            {
                fixed (T* p = &v)
                {
                    switch (mode)
                    {
                        case EUpdateMode.Auto:
                            mCoreObject.SetValue(binder, p, sizeof(T), bFlush, TtEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
                            break;
                        case EUpdateMode.Immediately:
                            mCoreObject.SetValue(binder, p, sizeof(T), bFlush, new FCbvUpdater());
                            break;
                    }
                }
            }
        }
        public void SetValue<T>(FShaderVarDesc binder, int elemIndex, in T v, bool bFlush = true, EUpdateMode mode = EUpdateMode.Auto) where T : unmanaged
        {
            unsafe
            {
                fixed (T* p = &v)
                {
                    switch (mode)
                    {
                        case EUpdateMode.Auto:
                            mCoreObject.SetArrrayValue(binder, elemIndex, p, sizeof(T), bFlush, TtEngine.Instance.GfxDevice.CbvUpdater.mCoreObject);
                            break;
                        case EUpdateMode.Immediately:
                            mCoreObject.SetArrrayValue(binder, elemIndex, p, sizeof(T), bFlush, new FCbvUpdater());
                            break;
                    }
                }
            }
        }
        public bool SetValue<T>(string name, in T v, bool bFlush = true, EUpdateMode mode = EUpdateMode.Auto) where T : unmanaged
        {
            if (ShaderBinder.IsValidPointer == false)
                return false;
            var binder = ShaderBinder.FindField(name);
            if (binder.IsValidPointer == false)
                return false;
            SetValue<T>(binder, v, bFlush, mode);
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
        public void SetMatrix(FShaderVarDesc binder, int elem, in Matrix value, bool transpose = true, EUpdateMode mode = EUpdateMode.Auto)
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
        public void SetMatrix(FShaderVarDesc binder, in Matrix value, bool transpose = true, EUpdateMode mode = EUpdateMode.Auto)
        {
            if (transpose == false)
            {
                SetValue(binder, 0, value);
            }
            else
            {
                var tm = Matrix.Transpose(in value);
                SetValue(binder, 0, tm);
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

        public class TrCbcUpdater : AuxPtrType<FCbvUpdater>
        {
            public TrCbcUpdater()
            {
                mCoreObject = FCbvUpdater.CreateInstance();
            }
            public void UpdateCBVs()
            {
                mCoreObject.UpdateCBVs();
            }
        }
    }
    public class TtVbView : AuxPtrType<NxRHI.IVbView>
    {
        public TtVbView()
        {

        }
        public TtVbView(IVbView ptr)
        {
            mCoreObject = ptr;
        }
        public unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint offset, void* pData, uint size)
        {
            mCoreObject.UpdateGpuData(cmd, offset, pData, size);
        }
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size)
        {
            mCoreObject.UpdateGpuData(offset, pData, size);
        }
    }
    public class TtIbView : AuxPtrType<NxRHI.IIbView>
    {
        public TtIbView()
        {

        }
        public TtIbView(IIbView ptr)
        {
            mCoreObject = ptr;
        }
        public unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint offset, void* pData, uint size)
        {
            mCoreObject.UpdateGpuData(cmd, offset, pData, size);
        }
        public unsafe void UpdateGpuData(uint offset, void* pData, uint size)
        {
            mCoreObject.UpdateGpuData(offset, pData, size);
        }
    }
    public class TtUaView : AuxPtrType<NxRHI.IUaView>
    {
    }
}
