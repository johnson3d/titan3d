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
    public interface TtGpuResource : IDisposable
    {
        IGpuBufferData GetGpuBufferDataPointer();
        unsafe void Map(uint subRes, FMappedSubResource* mapped, bool forRead);
        unsafe void Umap(uint subRes);
        void TransitionTo(ICommandList cmd, EGpuResourceState state);
        void TransitionTo(TtCommandList cmd, EGpuResourceState state);
        unsafe void UpdateGpuData(NxRHI.ICommandList cmd, uint subRes, void* pData, EngineNS.NxRHI.FSubResourceFootPrint* footPrint);
        void SetDebugName(string name);
        EGpuResourceState GpuState
        {
            get;
        }
        bool IsAutoTransition
        {
            get;
            set;
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
        public bool IsDirty
        {
            get => mCoreObject.IsDirty();
        }
        public void MarkDirty()
        {
            mCoreObject.MarkDirty();
        }
        public void FlushDirty(NxRHI.ICommandList cmd, bool clear = false)
        {
            mCoreObject.FlushDirty(cmd, clear);
        }
        public void FlushDirty(bool clear = false)
        {
            mCoreObject.FlushDirty(clear);
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
            return mCoreObject.FetchGpuData(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, index, blob);
        }
        /// <summary>
        /// 异步版 <see cref="FetchGpuData(uint, EngineNS.IBlobObject)"/>。
        ///
        /// 实现委托给 <see cref="TtAsyncReadback.RunReadbackAsync"/>, 走低层 readback 路径,
        /// 不阻塞调用线程也不阻塞渲染线程: <c>CreateReadable</c> 在调用线程做 (纯 CPU record),
        /// fence wait + FetchGpuData 在 EventPoster TPools 工作线程做, 完成后通过 semaphore
        /// 唤醒调用线程。详细说明见 <c>Documents/Coding/CodingGuidelines.md §1.5.3.1</c> 和
        /// <c>§1.6 (EventPoster)</c>。
        /// </summary>
        /// <param name="subRes">subresource 索引, 同步版同名参数。</param>
        /// <param name="blob">调用方分配的 blob, await 返回 true 时数据已就绪。</param>
        /// <returns>readback 是否成功。</returns>
        public Thread.Async.TtTask<bool> AsyncFetchGpuData(uint subRes, EngineNS.IBlobObject blob)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            return TtAsyncReadback.RunReadbackAsync(
                cpDraw => mCoreObject.CreateReadable(rc.mCoreObject, (int)subRes, cpDraw),
                blob,
                "TtBuffer.AsyncFetch");
        }
        public NxRHI.IBuffer CreateReadable(int subRes, EngineNS.NxRHI.ICopyDraw cpDraw)
        {
            return mCoreObject.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, subRes, cpDraw);
        }
        public void TransitionTo(ICommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd, state);
        }
        public void TransitionTo(TtCommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd.mCoreObject, state);
        }
        public EGpuResourceState GpuState
        {
            get => mCoreObject.NativeSuper.GpuState;
        }
        public bool IsAutoTransition
        {
            get => mCoreObject.NativeSuper.IsAutoTransition;
            set
            {
                var super = mCoreObject.NativeSuper;
                super.IsAutoTransition = value;
            }
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
            return mCoreObject.FetchGpuData(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, index, blob);
        }
        /// <summary>
        /// 异步版 <see cref="FetchGpuData(uint, EngineNS.IBlobObject)"/>。
        ///
        /// 与 <see cref="TtBuffer.AsyncFetchGpuData(uint, EngineNS.IBlobObject)"/> 完全对称,
        /// 实现委托给 <see cref="TtAsyncReadback.RunReadbackAsync"/>, 走同一条低层 readback 路径:
        /// <c>CreateReadable</c> 在调用线程做, fence wait + FetchGpuData 在 EventPoster TPools
        /// 工作线程做, 完成后通过 semaphore 唤醒调用线程, 不阻塞调用线程也不阻塞渲染线程。
        /// 详细说明见 <c>Documents/Coding/CodingGuidelines.md §1.5.3.1</c> 和 <c>§1.6 (EventPoster)</c>。
        /// </summary>
        /// <param name="subRes">subresource 索引, 同步版同名参数 (mip × array slice 平铺索引)。</param>
        /// <param name="blob">调用方分配的 blob, await 返回 true 时数据已就绪。</param>
        /// <returns>readback 是否成功。</returns>
        public Thread.Async.TtTask<bool> AsyncFetchGpuData(uint subRes, EngineNS.IBlobObject blob)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            return TtAsyncReadback.RunReadbackAsync(
                cpDraw => mCoreObject.CreateReadable(rc.mCoreObject, (int)subRes, cpDraw),
                blob,
                "TtTexture.AsyncFetch");
        }
        public NxRHI.IBuffer CreateReadable(int subRes, EngineNS.NxRHI.ICopyDraw cpDraw)
        {
            return mCoreObject.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, subRes, cpDraw);
        }
        public void TransitionTo(ICommandList cmd, EGpuResourceState state)
        {
            mCoreObject.NativeSuper.TransitionTo(cmd, state);
        }
        public void TransitionTo(TtCommandList cmd, EGpuResourceState state)
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
        public bool IsAutoTransition
        {
            get => mCoreObject.NativeSuper.IsAutoTransition;
            set
            {
                var super = mCoreObject.NativeSuper;
                super.IsAutoTransition = value;
            }
        }
        public bool GetFootprint(ref EngineNS.NxRHI.FSubResourceFootPrint fp, ref ulong rowSize, ref ulong totalSize, uint subRes = 0, ulong offset = 0)
        {
            return mCoreObject.GetFootprint(ref fp, ref rowSize, ref totalSize, subRes, offset);
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
        public bool IsDirty
        {
            get => mCoreObject.IsDirty();
        }
        public void MarkDirty()
        {
            mCoreObject.MarkDirty();
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
            if (binder.IsValidPointer == false)
                return;
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
        public unsafe bool SetValue<T>(string name, in T v, bool bFlush = true, EUpdateMode mode = EUpdateMode.Auto) where T : unmanaged
        {
            if (ShaderBinder.IsValidPointer == false)
                return false;
            var binder = ShaderBinder.FindField(name);
            if (binder.IsValidPointer == false)
                return false;
            if (binder.Size < sizeof(T))
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
        public void SetMatrix(string name, int elem, in Matrix value, bool transpose = true, EUpdateMode mode = EUpdateMode.Auto)
        {
            var binder = ShaderBinder.FindField(name);
            if (binder.IsValidPointer)
                SetMatrix(binder, elem, value, transpose, mode);
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
            [ThreadStatic]
            private static Profiler.TimeScope mScopeUpdateCBVs;
            private static Profiler.TimeScope ScopeUpdateCBVs
            {
                get
                {
                    if (mScopeUpdateCBVs == null)
                        mScopeUpdateCBVs = new Profiler.TimeScope(typeof(TrCbcUpdater), nameof(UpdateCBVs));
                    return mScopeUpdateCBVs;
                }
            }
            public void UpdateCBVs()
            {
                using (new Profiler.TimeScopeHelper(ScopeUpdateCBVs))
                {
                    mCoreObject.UpdateCBVs();
                }   
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

    // -------------------------------------------------------------------------
    // TtBindless: bindless descriptor table 的托管包装. 一份 TtBindless 实例对应
    // 一个 HLSL 端的 unbounded array 声明 (Texture2D<float4> Foo[] DX_AUTOBIND;
    // 或 SamplerState Bar[] DX_AUTOBIND;), 通过 SetSrv/SetCbv/SetUav/SetSampler
    // 把任意数量 (上限 IBindless::MaxBindless = 4096) 的资源映射到 shader 端可
    // NonUniformResourceIndex 索引访问的 slot.
    //
    // 使用规范见 documents/coding/CodingGuidelines.md §1.4. 关键约束:
    //   - 实例必须由 drawcall.CreateBindless(name) 创建 (name 必须与 HLSL 端
    //     unbounded array 变量名一致), 创建时机和其他 BindXxx 一样必须在
    //     ShadingEnv.OnDrawCall 阶段完成 (与 §1.2 保持一致).
    //   - 同一个 TtBindless 实例可跨帧/跨 drawcall 复用 (它内部会维护 fingerprint
    //     做差量重绑, 不需要每帧重建).
    //   - 持有 TtBindless 的代码必须确保被引用的 underlying resource (texture /
    //     buffer / sampler) 在 GPU 用完之前不被 Dispose, 否则会出现 "GPU 用着
    //     已释放的 descriptor" 的随机崩溃, 见 §1.4 反例.
    // -------------------------------------------------------------------------
    public class TtBindless : AuxPtrType<NxRHI.IBindless>
    {
        public TtBindless(NxRHI.IBindless ptr)
        {
            mCoreObject = ptr;
        }

        // bindless 数组的全局上限, HLSL 端 BindlessTextures[idx] 的 idx 必须 < 此值.
        // 来自 NativeCode/NextRHI/NxBuffer.h: IBindless::MaxBindless.
        public static uint MaxBindless => NxRHI.IBindless.GetMaxBindless();

        public EShaderBindType BindType
        {
            get => mCoreObject.mBindType;
        }

        // 此 bindless 表的实际 slot 数量 (由 effect 反射决定, 通常 == 数组实际声明
        // 的元素数; 若 HLSL 端是 unbounded array, 则等于 effect 编译时探到的最大
        // 索引 + 1). 上层在 SetXxx 前应保证 index < ResourceCount.
        public uint ResourceCount
        {
            get => mCoreObject.GetResourceCount();
        }

        // null resource 是否参与重绑.
        // - false (默认): BindResources() 会跳过 null slot, 适合"只关心已填充 slot"
        //   的常见场景 (例如材质表里只有部分 slot 有效).
        // - true: BindResources() 也会把 null slot 显式绑成 NullHeap, 适合需要
        //   "明确清空 slot, 让 shader 索引到时拿到 null 数据" 的极少数场景.
        public bool IsCopyNull
        {
            get => mCoreObject.mIsCopyNull;
            set => mCoreObject.mIsCopyNull = value;
        }

        public bool SetSrv(uint index, TtSrView res)
        {
            return mCoreObject.SetResource(index, res.mCoreObject.NativeSuper);
        }
        public bool SetCbv(uint index, TtCbView res)
        {
            return mCoreObject.SetResource(index, res.mCoreObject.NativeSuper);
        }
        public bool SetUav(uint index, TtUaView res)
        {
            return mCoreObject.SetResource(index, res.mCoreObject.NativeSuper);
        }
        // bindless sampler 表. HLSL 端: SamplerState Foo[] DX_AUTOBIND;
        public bool SetSampler(uint index, TtSampler res)
        {
            return mCoreObject.SetResource(index, res.mCoreObject.NativeSuper);
        }

        // 把指定 slot 设为空 (底层会绑成 NullHeap). 用途:
        //   - 卸载场景 / 切换关卡时清空所有 slot, 防止 GPU 残留指向已 Dispose 资源
        //     的 descriptor.
        //   - 动态重分配 slot 时, 把旧 slot 显式释放, 避免被 fingerprint 缓存认为
        //     "没变" 而保留旧绑定.
        // 返回 false 表示 index 越界 (>= ResourceCount).
        public bool ClearSlot(uint index)
        {
            return mCoreObject.SetResource(index, new IGpuResource());
        }

        // 全量重绑 (跳过 null slot, 除非 IsCopyNull = true). 正常情况下不需要手动
        // 调用, 引擎会在 drawcall commit 时自动通过 fingerprint 增量重绑. 仅在
        // 资源池整体重建 (例如重新加载场景的所有材质) 后, 需要强制让 GPU 端 heap
        // 与 CPU 端 mResources 重新一致时才用.
        public void RebindAll()
        {
            mCoreObject.BindResources();
        }
    }
}
