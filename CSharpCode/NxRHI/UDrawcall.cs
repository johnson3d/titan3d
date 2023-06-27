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
            if (binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, buffer.mCoreObject.NativeSuper);
        }
        public void BindCBuffer(UEffectBinder binder, UCbView buffer)
        {
            if (binder == null)
                return;
            mCoreObject.BindResource(binder.mCoreObject, buffer.mCoreObject.NativeSuper);
        }
        public bool BindSRV(VNameString name, USrView srv)
        {
            return mCoreObject.BindResource(name, srv.mCoreObject.NativeSuper);
        }
        public void BindSRV(FEffectBinder binder, USrView srv)
        {
            if (binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, srv.mCoreObject.NativeSuper);
        }
        public void BindSRV(NxRHI.UEffectBinder binder, USrView srv)
        {
            if (binder == null)
                return;
            mCoreObject.BindResource(binder.mCoreObject, srv.mCoreObject.NativeSuper);
        }
        public bool BindUAV(VNameString name, UUaView uav)
        {
            return mCoreObject.BindResource(name, uav.mCoreObject.NativeSuper);
        }
        public void BindUAV(FEffectBinder binder, UUaView uav)
        {
            if (binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, uav.mCoreObject.NativeSuper);
        }
        public bool BindSampler(VNameString name, USampler sampler)
        {
            return mCoreObject.BindResource(name, sampler.mCoreObject.NativeSuper);
        }
        public void BindSampler(FEffectBinder binder, USampler sampler)
        {
            if (binder.IsValidPointer == false)
                return;
            mCoreObject.BindResource(binder, sampler.mCoreObject.NativeSuper);
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
            if (buffer != null)
                mCoreObject.BindIndirectDispatchArgsBuffer(buffer.mCoreObject);
        }
        public FShaderBinder FindBinder(EShaderBindType type, string name)
        {
            return mCoreObject.FindBinder(type, name);
        }
        public void BindCBuffer(FShaderBinder binder, UCbView resource)
        {
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindCBuffer(string name, UCbView resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_CBuffer, name);
            if (binder.IsValidPointer)
                BindCBuffer(binder, resource);
        }
        public void BindSrv(FShaderBinder binder, USrView resource)
        {
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
            mCoreObject.BindResource(binder, resource.mCoreObject.NativeSuper);
        }
        public void BindSampler(string name, USampler resource)
        {
            var binder = mCoreObject.FindBinder(EShaderBindType.SBT_Sampler, name);
            if (binder.IsValidPointer)
                BindSampler(binder, resource);
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
        public void BindBufferDest(UBuffer res)
        {
            mCoreObject.BindBufferDest(res.mCoreObject);
        }
        public void BindTextureDest(UTexture res)
        {
            mCoreObject.BindTextureDest(res.mCoreObject);
        }
    }
}
