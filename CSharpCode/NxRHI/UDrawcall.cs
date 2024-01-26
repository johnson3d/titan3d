using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.NxRHI
{
    public partial class UGraphicDraw : AuxPtrType<NxRHI.IGraphicDraw>
    {
        public object TagObject = null;
        public override void Dispose()
        {
            TagObject = null;
            if (IsDisposed == false)
                TtStatistic.Instance.GraphicsDrawcall--;
            base.Dispose();
        }
        public uint DrawInstance
        {
            get { return mCoreObject.DrawInstance; }
            set { mCoreObject.DrawInstance = (ushort)value; }
        }
        public byte MeshAtom
        {
            get { return mCoreObject.MeshAtom; }
            set { mCoreObject.MeshAtom = value; }
        }
        public byte MeshLOD
        {
            get { return mCoreObject.MeshLOD; }
            set { mCoreObject.MeshLOD = value; }
        }

        public IGraphicsEffect GraphicsEffect
        {
            get
            {
                return mCoreObject.GetGraphicsEffect();
            }
        }
        public void Commit(ICommandList cmdlist)
        {
            mCoreObject.NativeSuper.Commit(cmdlist, false);
        }
        public void Commit(UCommandList cmdlist)
        {
            mCoreObject.NativeSuper.Commit(cmdlist.mCoreObject, false);
        }
        public FEffectBinder FindBinder(string name)
        {
            return mCoreObject.FindBinder(name);
        }
        
        public void BindPipeline(UGpuPipeline pipeline)
        {
            mCoreObject.BindPipeline(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, pipeline.mCoreObject);
        }
        public bool BindCBuffer(VNameString name, UCbView buffer)
        {
            return mCoreObject.BindResource(name, buffer.mCoreObject.NativeSuper);
        }
        public void BindCBuffer(FEffectBinder binder, UCbView buffer)
        {
            if (binder.IsValidPointer == false || buffer == null)
                return;
            mCoreObject.BindResource(binder, buffer.mCoreObject.NativeSuper);
        }
        public void BindCBuffer(UEffectBinder binder, UCbView buffer)
        {
            if (binder == null || buffer == null)
                return;
            BindCBuffer(binder.mCoreObject, buffer);
        }
        public bool BindSRV(VNameString name, USrView srv)
        {
            if (srv == null)
                return false;
            return mCoreObject.BindResource(name, srv.mCoreObject.NativeSuper);
        }
        public void BindSRV(FEffectBinder binder, USrView srv)
        {
            if (binder.IsValidPointer == false || srv == null)
                return;
            mCoreObject.BindResource(binder, srv.mCoreObject.NativeSuper);
        }
        public void BindSRV(NxRHI.UEffectBinder binder, USrView srv)
        {
            if (binder == null || srv == null)
                return;
            BindSRV(binder.mCoreObject, srv);
        }
        public bool BindUAV(VNameString name, UUaView uav)
        {
            return mCoreObject.BindResource(name, uav.mCoreObject.NativeSuper);
        }
        public void BindUAV(FEffectBinder binder, UUaView uav)
        {
            if (binder.IsValidPointer == false || uav == null)
                return;
            mCoreObject.BindResource(binder, uav.mCoreObject.NativeSuper);
        }
        public void BindUAV(UEffectBinder binder, UUaView uav)
        {
            if (binder == null || uav == null)
                return;
            BindUAV(binder.mCoreObject, uav);
        }
        public bool BindSampler(VNameString name, USampler sampler)
        {
            if (sampler == null)
                return false;
            return mCoreObject.BindResource(name, sampler.mCoreObject.NativeSuper);
        }
        public void BindSampler(FEffectBinder binder, USampler sampler)
        {
            if (binder.IsValidPointer == false || sampler == null)
                return;
            mCoreObject.BindResource(binder, sampler.mCoreObject.NativeSuper);
        }
        public void BindSampler(UEffectBinder binder, USampler sampler)
        {
            if (binder == null || sampler == null)
                return;
            BindSampler(binder.mCoreObject, sampler);
        }
        public void BindGeomMesh(UGeomMesh mesh)
        {
            mCoreObject.BindGeomMesh(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, mesh.mCoreObject);
        }
        public void BindGeomMesh(FGeomMesh mesh)
        {
            mCoreObject.BindGeomMesh(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, mesh);
        }
        public void BindAttachVertexArray(UVertexArray va)
        {
            mCoreObject.BindAttachVertexArray(va.mCoreObject);
        }
        public void BindIndirectDrawArgsBuffer(UBuffer buffer, uint offset)
        {
            mCoreObject.BindIndirectDrawArgsBuffer(buffer.mCoreObject, offset);
        }
        public void SetDebugName(string name)
        {
            mCoreObject.NativeSuper.SetDebugName(name);
        }
    }
    public class UComputeDraw : AuxPtrType<NxRHI.IComputeDraw>
    {
        public object TagObject = null;
        public override void Dispose()
        {
            TagObject = null;
            if (IsDisposed == false)
                TtStatistic.Instance.ComputeDrawcall--;
            base.Dispose();
        }
        public void Commit(ICommandList cmdlist)
        {
            mCoreObject.NativeSuper.Commit(cmdlist, false);
        }
        public void Commit(UCommandList cmdlist)
        {
            mCoreObject.NativeSuper.Commit(cmdlist.mCoreObject, false);
        }
        public void SetComputeEffect(UComputeEffect effect)
        {
            mCoreObject.SetComputeEffect(effect.mCoreObject);
        }
        public void SetDispatch(uint x, uint y, uint z)
        {
            mCoreObject.SetDispatch(x, y, z);
        }
        public void BindIndirectDispatchArgsBuffer(UBuffer buffer)
        {
            if (buffer == null)
                return;
            if (buffer != null)
                mCoreObject.BindIndirectDispatchArgsBuffer(buffer.mCoreObject);
        }
        public FShaderBinder FindBinder(EShaderBindType type, string name)
        {
            return mCoreObject.FindBinder(type, name);
        }
        public void BindCBuffer(FShaderBinder binder, UCbView resource)
        {
            if (resource == null || binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindCBuffer(string name, UCbView resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_CBuffer, name);
            if (binder.IsValidPointer)
                BindCBuffer(binder, resource);
        }
        public void BindCBuffer(string name, ref UCbView resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_CBuffer, name);
            if (binder.IsValidPointer == false)
                return;
            if (resource == null)
            {
                resource = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            }
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindSrv(FShaderBinder binder, USrView resource)
        {
            if (resource == null || binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindSrv(string name, USrView resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_SRV, name);
            if (binder.IsValidPointer)
                BindSrv(binder, resource);
        }
        public void BindUav(FShaderBinder binder, UUaView resource)
        {
            if (resource == null || binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindUav(string name, UUaView resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_UAV, name);
            if (binder.IsValidPointer)
                BindUav(binder, resource);
        }
        public void BindSampler(FShaderBinder binder, USampler resource)
        {
            if (resource == null || binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindSampler(string name, USampler resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_Sampler, name);
            if (binder.IsValidPointer)
                BindSampler(binder, resource);
        }
        public void SetDebugName(string name)
        {
            mCoreObject.NativeSuper.SetDebugName(name);
        }
    }
    public class UCopyDraw : AuxPtrType<NxRHI.ICopyDraw>
    {
        public override void Dispose()
        {
            if (IsDisposed == false)
                TtStatistic.Instance.TransferDrawcall--;
            base.Dispose();
        }
        public void Commit(ICommandList cmdlist)
        {
            mCoreObject.NativeSuper.Commit(cmdlist, false);
        }
        public void Commit(UCommandList cmdlist)
        {
            mCoreObject.NativeSuper.Commit(cmdlist.mCoreObject, false);
        }
        public ECopyDrawMode Mode
        {
            get
            {
                return mCoreObject.Mode;
            }
            set
            {
                mCoreObject.Mode = value;
            }
        }
        public uint SrcSubResource
        {
            get => mCoreObject.SrcSubResource;
            set => mCoreObject.SrcSubResource = value;
        }
        public uint DestSubResource
        {
            get => mCoreObject.DestSubResource;
            set => mCoreObject.DestSubResource = value;
        }
        public uint DstX
        {
            get => mCoreObject.DstX;
            set => mCoreObject.DstX = value;
        }
        public uint DstY
        {
            get => mCoreObject.DstY;
            set => mCoreObject.DstY = value;
        }
        public uint DstZ
        {
            get => mCoreObject.DstZ;
            set => mCoreObject.DstZ = value;
        }
        public ref FSubResourceFootPrint FootPrint
        {
            get
            {
                unsafe
                {
                    return ref *mCoreObject.GetFootPrint();
                }
            }
        }
        public void BindSrc(UGpuResource res)
        {
            var bf = res as UBuffer;
            if (bf != null)
            {
                mCoreObject.BindBufferSrc(bf.mCoreObject);
            }
            else
            {
                var tex = res as UTexture;
                if (tex != null)
                {
                    mCoreObject.BindTextureSrc(tex.mCoreObject);
                }
            }
        }
        public void BindDest(UGpuResource res)
        {
            var bf = res as UBuffer;
            if (bf != null)
            {
                mCoreObject.BindBufferDest(bf.mCoreObject);
            }
            else
            {
                var tex = res as UTexture;
                if (tex != null)
                {
                    mCoreObject.BindTextureDest(tex.mCoreObject);
                }
            }
        }
        public void BindBufferSrc(UBuffer res)
        {
            mCoreObject.BindBufferSrc(res.mCoreObject);
        }
        public void BindTextureSrc(UTexture res)
        {
            mCoreObject.BindTextureSrc(res.mCoreObject);
        }
        public void BindTextureSrc(ITexture res)
        {
            mCoreObject.BindTextureSrc(res);
        }
        public void BindBufferDest(UBuffer res)
        {
            mCoreObject.BindBufferDest(res.mCoreObject);
        }
        public void BindTextureDest(UTexture res)
        {
            mCoreObject.BindTextureDest(res.mCoreObject);
        }
        public void BindTextureDest(ITexture res)
        {
            mCoreObject.BindTextureDest(res);
        }
        public void SetDebugName(string name)
        {
            mCoreObject.NativeSuper.SetDebugName(name);
        }
    }

    public class TtActionDraw : AuxPtrType<NxRHI.IActionDraw>
    {
        public override void Dispose()
        {
            if (IsDisposed == false)
                TtStatistic.Instance.ActionDrawcall--;
            base.Dispose();
        }
    }
}
