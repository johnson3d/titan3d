using EngineNS.Bricks.Terrain.CDLOD;
using EngineNS.Graphics.Pipeline.Deferred;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.NxRHI
{
    public partial class UGraphicDraw
    {
        public byte SubMesh;
        public void SetSourceAtom(Graphics.Mesh.TtMesh.TtAtom atom)
        {
            SubMesh = (byte)atom.SubMesh.MeshIndex;
            MeshAtom = (byte)atom.AtomIndex;
            MeshLOD = 0;
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    public class TtMesh : IDisposable
    {
        public void Dispose()
        {
            foreach (var i in SubMeshes)
            {
                i.Dispose();
            }
            SubMeshes.Clear();

            MdfQueue?.Dispose();
            MdfQueue = null;
            CoreSDK.DisposeObject(ref mPerMeshCBuffer);
        }
        public Graphics.Pipeline.Shader.UGraphicsShadingEnv UserShading = null;
        public GamePlay.Scene.UNode HostNode { get; set; }
        bool mIsCastShadow = false;
        public bool IsCastShadow 
        { 
            get
            {
                if (HostNode != null)
                    return HostNode.IsCastShadow;
                return mIsCastShadow;
            }
            set
            {
                mIsCastShadow = value;
            }
        }
        public UMaterialMesh MaterialMesh { get; private set; }
        public uint MaterialMeshSerialId { get; private set; }
        private void CheckVersionUpdated()
        {
            if (MaterialMesh.SerialId == MaterialMeshSerialId)
                return;
            MaterialMesh.SerialId = MaterialMeshSerialId;

            Initialize(MaterialMesh,
                Rtti.UTypeDesc.TypeOf(MdfQueue.GetType()),
                Rtti.UTypeDesc.TypeOf(mSubMeshes[0].Atoms[0].GetType()));
        }

        public Pipeline.Shader.TtMdfQueueBase MdfQueue { get; private set; }
        NxRHI.UCbView mPerMeshCBuffer;
        protected System.Action OnAfterCBufferCreated;

        int ObjectFlags_2Bit = 0;
        public bool IsAcceptShadow
        {
            get
            {
                return (ObjectFlags_2Bit & 1) != 0;
            }
            set
            {
                if (value)
                {
                    ObjectFlags_2Bit |= 1;
                }
                else
                {
                    ObjectFlags_2Bit &= (~1);
                }
                PerMeshCBuffer?.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.ObjectFLags_2Bit, in ObjectFlags_2Bit);
            }
        }
        public bool IsUnlit
        {
            get
            {
                return (ObjectFlags_2Bit & (2)) != 0;
            }
            set
            {
                if (value)
                {
                    ObjectFlags_2Bit |= (2);
                }
                else
                {
                    ObjectFlags_2Bit &= (~(2));
                }
                PerMeshCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.ObjectFLags_2Bit, in ObjectFlags_2Bit);
            }
        }
        public NxRHI.UCbView PerMeshCBuffer 
        {
            get
            {
                if (mPerMeshCBuffer == null)
                {
                    var binder = UEngine.Instance.GfxDevice.CoreShaderBinder.CBufferCreator.cbPerMesh;
                    mPerMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
                    if (mPerMeshCBuffer == null)
                        return null;
                    if (OnAfterCBufferCreated != null)
                    {
                        OnAfterCBufferCreated();
                        OnAfterCBufferCreated = null;
                    }
                    else
                    {
                        DirectSetWorldMatrix(in Matrix.Identity);
                    }
                }
                return mPerMeshCBuffer;
            }
        }
        public class UMeshAttachment
        {
            
        }
        public bool IsDrawHitproxy = false;
        public UMeshAttachment Tag { get; set; }
        public class TtAtom : IDisposable
        {
            public void Dispose()
            {
                if (TargetViews != null)
                {
                    foreach (var i in TargetViews)
                    {
                        i.Dispose();
                    }
                    TargetViews.Clear();
                    TargetViews = null;
                }
                SubMesh = null;
                Material = null;
            }
            public TtSubMesh SubMesh;
            public int AtomIndex;

            public uint MaterialSerialId;
            public Pipeline.Shader.UMaterial Material;
            public Pipeline.Shader.UMaterial GetMeshMaterial()
            {
                if (SubMesh == null)
                    return null;
                var mtlMesh = MaterialMesh;
                if (mtlMesh.SubMeshes.Count <= SubMesh.MeshIndex)
                    return null;
                return mtlMesh.SubMeshes[SubMesh.MeshIndex].Materials[AtomIndex];
            }            
            public UMaterialMesh MaterialMesh 
            {
                get => SubMesh.Mesh.MaterialMesh;
            }
            public UMeshPrimitives MeshPrimitives
            {
                get
                {
                    if (MaterialMesh.SubMeshes.Count <= SubMesh.MeshIndex)
                        return null;
                    return MaterialMesh.SubMeshes[(int)SubMesh.MeshIndex].Mesh;
                }
            }
            public unsafe NxRHI.FMeshAtomDesc* GetMeshAtomDesc(uint lod)
            {
                return MeshPrimitives.mCoreObject.GetAtom((uint)AtomIndex, lod);
            }
            public VIUnknown GetAtomExtData()
            {
                return MeshPrimitives.mCoreObject.GetAtomExtData((uint)AtomIndex);
            }
            public Graphics.Pipeline.Shader.UGraphicsShadingEnv UserShading
            {
                get => SubMesh.Mesh.UserShading;
            }
            public Pipeline.Shader.TtMdfQueueBase MdfQueue { get => SubMesh.Mesh.MdfQueue; }
            public class ViewDrawCalls : IDisposable
            {
                public void Dispose()
                {
                    if (DrawCalls != null)
                    {
                        foreach (var i in DrawCalls)
                        {
                            if (i == null)
                                continue;
                            i.Dispose();
                        }
                        DrawCalls = null;
                    }
                }
                internal int State = 0;
                public WeakReference<Pipeline.UGraphicsBuffers.UTargetViewIdentifier> TargetView;
                public NxRHI.UGraphicDraw[] DrawCalls;
                //不同的View上可以有不同的渲染策略，同一个模型，可以渲染在不同视口上，比如装备预览的策略可以和GameView不一样
                public Pipeline.URenderPolicy Policy;                
            }
            public List<ViewDrawCalls> TargetViews;
            
            private async Thread.Async.TtTask BuildDrawCall(ViewDrawCalls vdc, Pipeline.URenderPolicy policy,
                Pipeline.URenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
            {
                var device = UEngine.Instance.GfxDevice;
                NxRHI.UGraphicDraw[] drawCalls = vdc.DrawCalls;
                for (Pipeline.URenderPolicy.EShadingType i = Pipeline.URenderPolicy.EShadingType.BasePass;
                    i < Pipeline.URenderPolicy.EShadingType.Count; i++)
                {
                    Graphics.Pipeline.Shader.UGraphicsShadingEnv shading = null;
                    try
                    {
                        shading = policy.GetPassShading(i, this, node);
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Mesh", "policy.GetPassShading except");
                        continue;
                    }
                    if (shading != null)
                    {
                        var effect = await UEngine.Instance.GfxDevice.EffectManager.GetEffect(shading, Material.ParentMaterial, this.MdfQueue);
                        if (effect == null)
                            continue;
                        var drawcall = UEngine.Instance.GfxDevice.RenderContext.CreateGraphicDraw();// (shading, Material.ParentMaterial, mesh.MdfQueue);
                        drawcall.SetSourceAtom(this);
                        drawcall.BindShaderEffect(effect);
                        drawcall.BindGeomMesh(this.MeshPrimitives.mCoreObject.GetGeomtryMesh());
                        drawcall.BindPipeline(Material.Pipeline);
                        drawcall.PermutationId = shading.mCurrentPermutationId;
                        

                        #region Textures
                        for (int j = 0; j < Material.NumOfSRV; j++)
                        {
                            var srv = await Material.GetSRV(j);
                            if (srv == null)
                                continue;
                            var varName = Material.GetNameOfSRV(j);                            
                            unsafe
                            {
                                var binder = drawcall.FindBinder(varName);
                                if (binder.IsValidPointer)
                                    drawcall.BindSRV(binder, srv);
                            }
                        }
                        #endregion

                        #region Samplers
                        for (int j = 0; j < Material.NumOfSampler; j++)
                        {
                            var varName = Material.GetNameOfSampler(j);
                            var sampler = Material.GetSampler(j);
                            var binder = drawcall.FindBinder(varName);
                            if (binder.IsValidPointer)
                            {
                                drawcall.BindSampler(binder, sampler);
                            }
                        }
                        #endregion

                        #region CBuffer
                        unsafe
                        {
                            var coreBinder = device.CoreShaderBinder;
                            if (UEngine.Instance.GfxDevice.PerFrameCBuffer != null)
                            {
                                drawcall.BindCBuffer(effect.BindIndexer.cbPerFrame, UEngine.Instance.GfxDevice.PerFrameCBuffer);
                            }
                            if (this.SubMesh.Mesh.PerMeshCBuffer != null)
                            {
                                drawcall.BindCBuffer(effect.BindIndexer.cbPerMesh, this.SubMesh.Mesh.PerMeshCBuffer);
                            }
                            if (Material != null)
                            {
                                if (Material.PerMaterialCBuffer == null)
                                {
                                    if (Material.CreateCBuffer(effect))
                                        Material.UpdateCBufferVars(Material.PerMaterialCBuffer, Material.PerMaterialCBuffer.ShaderBinder);
                                }
                                if (Material.PerMaterialCBuffer != null)
                                {   
                                    drawcall.BindCBuffer(effect.BindIndexer.cbPerMaterial, Material.PerMaterialCBuffer);
                                }
                            }
                        }
                        #endregion

                        shading.OnBuildDrawCall(policy, drawcall);
                        //drawcall.MaterialSerialId = Material.SerialId;
                        if (drawCalls[(int)i] != null)
                        {
                            drawCalls[(int)i].Dispose();
                        }
                        drawCalls[(int)i] = drawcall;
                    }
                }

                vdc.State = 1;
            }
            internal void ResetDrawCalls()
            {
                if (TargetViews != null)
                {
                    foreach(var i in TargetViews)
                    {
                        i.Dispose();
                    }
                    TargetViews.Clear();
                    TargetViews = null;
                }
            }            
            private ViewDrawCalls GetOrCreateDrawCalls(Pipeline.UGraphicsBuffers.UTargetViewIdentifier id, Pipeline.URenderPolicy policy)
            {
                ViewDrawCalls drawCalls = null;
                //查找或者缓存TargetView
                for (int i = 0; i < TargetViews.Count; i++)
                {
                    Pipeline.UGraphicsBuffers.UTargetViewIdentifier identifier;
                    if (TargetViews[i].TargetView.TryGetTarget(out identifier) == false)
                    {//多开的窗口已经关闭
                        TargetViews[i].Dispose();
                        TargetViews.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else if (identifier == id)
                    {
                        drawCalls = TargetViews[i];
                        break;
                    }
                }
                if (drawCalls == null)
                {
                    drawCalls = new ViewDrawCalls();
                    drawCalls.TargetView = new WeakReference<Pipeline.UGraphicsBuffers.UTargetViewIdentifier>(id);
                    drawCalls.DrawCalls = new NxRHI.UGraphicDraw[(int)Pipeline.URenderPolicy.EShadingType.Count];
                    drawCalls.Policy = policy;
                    TargetViews.Add(drawCalls);
                }
                return drawCalls;
            }
            [ThreadStatic]
            private static Profiler.TimeScope ScopeOnDrawCall = Profiler.TimeScopeManager.GetTimeScope(typeof(TtAtom), "OnDrawCall");
            public unsafe virtual NxRHI.UGraphicDraw GetDrawCall(NxRHI.ICommandList cmd, Pipeline.UGraphicsBuffers targetView, Pipeline.URenderPolicy policy,
                Pipeline.URenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
            {
                if (Material == null)
                    return null;

                var meshMaterial = this.GetMeshMaterial();
                if (Material != meshMaterial ||
                    (Material!=null && Material.SerialId != MaterialSerialId))
                {
                    Material = meshMaterial;
                    if (Material == null)
                        return null;
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                }
                //每个TargetView都要对应一个DrawCall数组                
                if (TargetViews == null)
                {
                    TargetViews = new List<ViewDrawCalls>();
                }
                ViewDrawCalls drawCalls = GetOrCreateDrawCalls(targetView.TargetViewIdentifier, policy);
                if (drawCalls.Policy != policy)
                {
                    Material = this.GetMeshMaterial();
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    if (TargetViews == null)
                    {
                        TargetViews = new List<ViewDrawCalls>();
                    }
                    drawCalls = GetOrCreateDrawCalls(targetView.TargetViewIdentifier, policy);
                }
                var result = drawCalls.DrawCalls[(int)shadingType];
                if (drawCalls.State == 1 && result == null)
                {
                    Material = this.GetMeshMaterial();
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    if (TargetViews == null)
                    {
                        TargetViews = new List<ViewDrawCalls>();
                    }
                    drawCalls = GetOrCreateDrawCalls(targetView.TargetViewIdentifier, policy);
                }

                switch (drawCalls.State)
                {
                    case 1:
                        break;
                    case 0:
                        {
                            drawCalls.State = -1;
                            var task = BuildDrawCall(drawCalls, policy, shadingType, node);
                            if (task.IsCompleted == false)
                            {
                                return null;
                            }
                            else
                            {
                                drawCalls.State = 1;
                                break;
                            }
                        }
                    case -1:
                    default:
                        {
                            return null;
                        }
                }

                result = drawCalls.DrawCalls[(int)shadingType];
                //检查shading切换参数
                if (result.IsPermutationChanged())
                {
                    Material = this.GetMeshMaterial();
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    return null;
                }

                result.TagObject = node;
                //result.CheckPermutation(Material.ParentMaterial, mesh.MdfQueue);
                //检查材质参数被修改
                //if (result.CheckMaterialParameters(Material))
                //{
                //    if (result.Effect.CBPerMeshIndex != 0xFFFFFFFF)
                //    {
                //        result.mCoreObject.BindCBufferAll(result.Effect.CBPerMeshIndex, mesh.PerMeshCBuffer.mCoreObject);
                //    }
                //}

                using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
                {
                    policy.OnDrawCall(cmd, shadingType, result, this);
                }
                return result;
            }
        }
        public class TtSubMesh : IDisposable
        {
            public void Dispose()
            {
                if (Atoms != null)
                {
                    foreach (var i in Atoms)
                    {
                        if (i != null)
                            i.Dispose();
                    }
                    Atoms = null;
                }
            }
            public TtMesh Mesh;
            public int MeshIndex;
            public List<TtAtom> Atoms = new List<TtAtom>();
        }
        List<TtSubMesh> mSubMeshes = new List<TtSubMesh>();
        public List<TtSubMesh> SubMeshes 
        { 
            get
            {
                CheckVersionUpdated();
                return mSubMeshes;
            }
        }
        [ReadOnly(true)]
        public string MdfQueueType
        {
            get
            {
                return Rtti.UTypeDesc.TypeStr(this.MdfQueue.GetType());
            }
            set
            {
                SetMdfQueueType(Rtti.UTypeDesc.TypeOf(value));
            }
        }
        internal bool SetMdfQueueType(Rtti.UTypeDesc mdfQueueType)
        {
            var mdf = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
            if (mdf == null)
                return false;
            MdfQueue.Initialize(MaterialMesh);

            mdf.CopyFrom(MdfQueue);
            MdfQueue = mdf;

            foreach (var i in SubMeshes)
            {
                foreach (var j in i.Atoms)
                {
                    j.ResetDrawCalls();
                }
            }
            return true;
        }
        public bool Initialize(UMaterialMesh materialMesh, Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDesc.TypeOf(typeof(TtAtom));
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(TtAtom)) && atomType.IsSubclassOf(typeof(TtAtom)) == false)
                return false;
            
            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
            if (MdfQueue == null)
                return false;
            MdfQueue.Initialize(materialMesh);

            MaterialMesh = materialMesh;
            MaterialMeshSerialId = MaterialMesh.SerialId;
            for (int j = 0; j < MaterialMesh.SubMeshes.Count; j++)
            {
                var sbMesh = new TtSubMesh();
                sbMesh.Mesh = this;
                sbMesh.MeshIndex = j;
                sbMesh.Atoms.Resize(MaterialMesh.SubMeshes[j].Materials.Count);
                for (int i = 0; i < sbMesh.Atoms.Count; i++)
                {
                    sbMesh.Atoms[i] = Rtti.UTypeDescManager.CreateInstance(atomType) as TtAtom;
                    sbMesh.Atoms[i].SubMesh = sbMesh;
                    sbMesh.Atoms[i].AtomIndex = i;
                    sbMesh.Atoms[i].Material = MaterialMesh.SubMeshes[j].Materials[i];
                }
                SubMeshes.Add(sbMesh);
            }

            return true;
        }
        public async Thread.Async.TtTask<bool> Initialize(RName materialMesh, Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            MaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(materialMesh);
            if (MaterialMesh == null)
                return false;

            return Initialize(MaterialMesh, mdfQueueType, atomType);
        }
        public async Thread.Async.TtTask<bool> Initialize(List<RName> meshSource, List<List<Pipeline.Shader.UMaterial>> materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDesc.TypeOf(typeof(TtAtom));
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(TtAtom)) && atomType.IsSubclassOf(typeof(TtAtom)) == false)
                return false;

            var mesh = new List<UMeshPrimitives>();
            foreach(var i in meshSource)
            {
                var tm = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(i);
                mesh.Add(tm);
            }
            if (false == UpdateMesh(mesh, materials, atomType))
                return false;

            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
            if (MdfQueue == null)
                return false;
            MdfQueue.Initialize(MaterialMesh);
            return true;
        }

        public bool Initialize(List<UMeshPrimitives> mesh, List<List<Pipeline.Shader.UMaterial>> materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (false == UpdateMesh(mesh, materials, atomType))
                return false;

            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
            if (MdfQueue == null)
                return false;
            MdfQueue.Initialize(MaterialMesh);
            return true; 
        }
        public bool Initialize(UMeshPrimitives mesh, Pipeline.Shader.UMaterial[] mats,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            var materials = ListExtra.CreateList(mats);
            return Initialize(new List<UMeshPrimitives>(){ mesh }, 
                new List<List<Pipeline.Shader.UMaterial>>() { materials }, 
                mdfQueueType, atomType);
        }
        public bool Initialize(UMeshPrimitives mesh, List<Pipeline.Shader.UMaterial> materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            return Initialize(new List<UMeshPrimitives>() { mesh },
                new List<List<Pipeline.Shader.UMaterial>>() { materials },
                mdfQueueType, atomType);
        }
        public async Thread.Async.TtTask<bool> Initialize(RName meshName, List<Pipeline.Shader.UMaterial> materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            var mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(meshName);
            return Initialize(new List<UMeshPrimitives>() { mesh },
                new List<List<Pipeline.Shader.UMaterial>>() { materials },
                mdfQueueType, atomType);
        }
        public bool UpdateMesh(int subMesh, UMeshPrimitives mesh, List<Pipeline.Shader.UMaterial> materials, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDescGetter<TtAtom>.TypeDesc;

            var sbMesh = MaterialMesh.SubMeshes[subMesh];
            sbMesh.Mesh = mesh;
            if (sbMesh.Materials.Count > materials.Count)
                return false;
            for (int i = 0; i < sbMesh.Materials.Count; i++)
            {
                sbMesh.Materials[i] = materials[i];
            }

            var tarMesh = this.SubMeshes[subMesh];
            System.Diagnostics.Debug.Assert(tarMesh.MeshIndex == subMesh);
            if (tarMesh.Atoms == null || tarMesh.Atoms.Count != sbMesh.Materials.Count)
            {
                tarMesh.Atoms.Resize(sbMesh.Materials.Count);
            }
            for (int i = 0; i < tarMesh.Atoms.Count; i++)
            {
                if (tarMesh.Atoms[i] == null || tarMesh.Atoms[i].GetType() != atomType.SystemType)
                {
                    tarMesh.Atoms[i] = Rtti.UTypeDescManager.CreateInstance(atomType) as TtAtom;
                    tarMesh.Atoms[i].SubMesh = tarMesh;
                    tarMesh.Atoms[i].AtomIndex = i;
                    if (tarMesh.Atoms[i].Material != sbMesh.Materials[i])
                    {
                        tarMesh.Atoms[i].Material = sbMesh.Materials[i];
                        tarMesh.Atoms[i].MaterialSerialId = sbMesh.Materials[i].SerialId - 1;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Assert(tarMesh.Atoms[i].SubMesh == tarMesh);
                    System.Diagnostics.Debug.Assert(tarMesh.Atoms[i].AtomIndex == i);
                    if (tarMesh.Atoms[i].Material != sbMesh.Materials[i])
                    {
                        tarMesh.Atoms[i].Material = sbMesh.Materials[i];
                        tarMesh.Atoms[i].MaterialSerialId = sbMesh.Materials[i].SerialId - 1;
                    }
                }
            }
            return true;
        }
        public bool UpdateMesh(List<UMeshPrimitives> mesh, List<List<Pipeline.Shader.UMaterial>> materials, Rtti.UTypeDesc atomType = null)
        {
            if (mesh.Count != materials.Count)
                return false;
            if (MaterialMesh == null || MaterialMesh.SubMeshes.Count != mesh.Count)
            {
                MaterialMesh = new UMaterialMesh();
                MaterialMesh.Initialize(mesh, materials);
                MaterialMeshSerialId = MaterialMesh.SerialId;

                SubMeshes.Clear();
                for (int i = 0; i < mesh.Count; i++)
                {
                    var sbMesh = new TtSubMesh();
                    sbMesh.Mesh = this;
                    sbMesh.MeshIndex = i;
                    SubMeshes.Add(sbMesh);
                }
            }
            
            for (int i = 0; i < mesh.Count; i++)
            {
                if (false == UpdateMesh(i, mesh[i], materials[i], atomType))
                    return false;
            }

            return true;
        }
        public void UpdateCameraOffset(GamePlay.UWorld world)
        {
            if (PerMeshCBuffer == null || world == null)
                return;
            var tm = PerMeshCBuffer.GetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
            var realPos = WorldLocation - world.CameraOffset;
            tm.Translation = realPos.ToSingleVector3();
            this.DirectSetWorldMatrix(in tm);
        }
        private DVector3 WorldLocation = DVector3.Zero;
        public void SetWorldTransform(in FTransform transform, GamePlay.UWorld world, bool isNoScale)
        {
            WorldLocation = transform.Position;
            if (world != null)
            {
                if (isNoScale == false)
                    this.DirectSetWorldMatrix(transform.ToMatrixWithScale(in world.mCameraOffset));
                else
                    this.DirectSetWorldMatrix(transform.ToMatrixNoScale(in world.mCameraOffset));
            }
            else
            {
                if (isNoScale == false)
                    this.DirectSetWorldMatrix(transform.ToMatrixWithScale(in transform.mPosition));
                else
                    this.DirectSetWorldMatrix(transform.ToMatrixNoScale(in transform.mPosition));
            }
        }
        public void DirectSetWorldMatrix(in Matrix tm)
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedTM = tm;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();
                    PerMeshCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix, in savedTM);
                    var inv = Matrix.Invert(in savedTM);
                    PerMeshCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrixInverse, in inv);
                };
                return;
            }   
            PerMeshCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix, in tm);
            var inv = Matrix.Invert(in tm);
            PerMeshCBuffer.SetMatrix(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrixInverse, in inv);
        }
        public void SetValue<T>(NxRHI.FShaderVarDesc index, in T value, int elem = 0) where T : unmanaged
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedValue = value;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();

                    PerMeshCBuffer.SetValue(index, elem, in savedValue);
                };
                return;
            }
            PerMeshCBuffer.SetValue(index, elem, in value);
        }
        public void SetHitproxy(in Vector4 value)
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedValue = value;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();

                    PerMeshCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.HitProxyId, in savedValue);
                };
                return;
            }
            PerMeshCBuffer.SetValue(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.HitProxyId, in value);
        }
    }
}
