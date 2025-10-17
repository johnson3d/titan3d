using EngineNS.Bricks.Terrain.CDLOD;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Deferred;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.NxRHI
{
    public partial class TtGraphicDraw
    {
        public byte SubMesh;
        public void SetSourceAtom(Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {
            SubMesh = (byte)atom.SubMesh.MeshIndex;
            MeshAtom = (byte)atom.AtomIndex;
            MeshLOD = 0;
        }
    }
}

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Graphics.Mesh.TtMesh@EngineCore", "EngineNS.Graphics.Mesh.TtMesh" })]
    public partial class TtRenderMesh : IDisposable
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
        public Graphics.Pipeline.Shader.TtGraphicsShadingEnv UserShading = null;
        public GamePlay.Scene.TtNode HostNode { get; set; }
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
        public TtMaterialMesh MaterialMesh { get; private set; }
        public uint MaterialMeshSerialId { get; private set; }
        private void CheckVersionUpdated()
        {
            if (MaterialMesh.SerialId == MaterialMeshSerialId)
                return;
            MaterialMesh.SerialId = MaterialMeshSerialId;

            Initialize(MaterialMesh,
                Rtti.TtTypeDesc.TypeOf(MdfQueue.GetType()),
                Rtti.TtTypeDesc.TypeOf(mSubMeshes[0].Atoms[0].GetType()));
        }

        public Pipeline.Shader.TtMdfQueueBase MdfQueue { get; private set; }
        NxRHI.TtCbView mPerMeshCBuffer;
        protected System.Action OnAfterCBufferCreated;

        [Flags]
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EObjectFlags_2Bit")]
        public enum EObjectFlags_2Bit : uint
        {
            AcceptShadow = 1,
            UnLight = (1 << 1),
        }
        EObjectFlags_2Bit ObjectFlags_2Bit = 0;
        public bool IsAcceptShadow
        {
            get
            {
                return (ObjectFlags_2Bit & EObjectFlags_2Bit.AcceptShadow) != 0;
            }
            set
            {
                if (value)
                {
                    ObjectFlags_2Bit |= EObjectFlags_2Bit.AcceptShadow;
                }
                else
                {
                    ObjectFlags_2Bit &= (~EObjectFlags_2Bit.AcceptShadow);
                }
                PerMeshCBuffer?.SetValue(Graphics.Pipeline.TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.ObjectFLags_2Bit, in ObjectFlags_2Bit);
            }
        }
        public bool IsUnlit
        {
            get
            {
                return (ObjectFlags_2Bit & EObjectFlags_2Bit.UnLight) != 0;
            }
            set
            {
                if (value)
                {
                    ObjectFlags_2Bit |= EObjectFlags_2Bit.UnLight;
                }
                else
                {
                    ObjectFlags_2Bit &= (~EObjectFlags_2Bit.UnLight);
                }
                PerMeshCBuffer.SetValue(Graphics.Pipeline.TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.ObjectFLags_2Bit, in ObjectFlags_2Bit);
            }
        }
        public NxRHI.TtCbView PerMeshCBuffer 
        {
            get
            {
                if (mPerMeshCBuffer == null)
                {
                    var binder = NxRHI.TtShader.TtCommonShaderResourceIndexer.Instance.cbPerMesh;
                    mPerMeshCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
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
        public void UnsafeSetPerMeshCBuffer(NxRHI.TtCbView cbv)
        {
            mPerMeshCBuffer = cbv;
        }
        public bool IsDrawHitproxy = false;
        public delegate void FOnBuildDrawcall(NxRHI.TtGraphicDraw drawcall);
        public FOnBuildDrawcall OnBuildDrawcall = null;
        public object Tag { get; set; }
        [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Graphics.Mesh.TtMesh.TtAtom@EngineCore", "EngineNS.Graphics.Mesh.TtMesh.TtAtom" })]
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
            public Pipeline.Shader.TtMaterial Material;
            public Pipeline.Shader.TtMaterial GetMeshMaterial()
            {
                if (SubMesh == null)
                    return null;
                var mtlMesh = MaterialMesh;
                if (mtlMesh.SubMeshes.Count <= SubMesh.MeshIndex)
                    return null;
                if (AtomIndex >= mtlMesh.SubMeshes[SubMesh.MeshIndex].Materials.Count)
                    return null;
                return mtlMesh.SubMeshes[SubMesh.MeshIndex].Materials[AtomIndex];
            }            
            public TtMaterialMesh MaterialMesh 
            {
                get => SubMesh.Mesh.MaterialMesh;
            }
            public TtMeshPrimitives MeshPrimitives
            {
                get
                {
                    if (MaterialMesh.SubMeshes.Count <= SubMesh.MeshIndex)
                        return null;
                    return MaterialMesh.SubMeshes[(int)SubMesh.MeshIndex].Mesh;
                }
            }
            public unsafe uint DrawPrimitiveNum
            {
                get
                {
                    var atom = MeshPrimitives.mCoreObject.GetAtom((uint)AtomIndex, 0);
                    return atom->NumPrimitives;
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
            public Graphics.Pipeline.Shader.TtGraphicsShadingEnv UserShading
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
                        DrawCalls.Dispose();
                        DrawCalls = null;
                    }
                }
                internal int State = 0;
                public WeakReference<Pipeline.TtGraphicsBuffers.TtTargetViewIdentifier> TargetView;
                public NxRHI.TtGraphicDraw DrawCalls;
                //不同的View上可以有不同的渲染策略，同一个模型，可以渲染在不同视口上，比如装备预览的策略可以和GameView不一样
                public Pipeline.TtRenderPolicy Policy;                
            }
            public List<ViewDrawCalls> TargetViews;

            private async Thread.Async.TtTask BuildDrawCall(ViewDrawCalls vdc, Pipeline.TtGraphicsBuffers targetView, Pipeline.TtRenderPolicy policy,
                Pipeline.TtRenderGraphNode node)
            {
                vdc.State = -1;
                var device = TtEngine.Instance.GfxDevice;
                Graphics.Pipeline.Shader.TtGraphicsShadingEnv shading = null;
                try
                {
                    shading = node.GetPassShading(this);
                    if (shading != null)
                    {
                        var effect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(shading, Material.ParentMaterial, this.MdfQueue);
                        if (effect == null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Error, $"GetEffect({shading},{Material.ParentMaterial},{this.MdfQueue}) failed");
                            return;
                        }
                        var drawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateGraphicDraw();// (shading, Material.ParentMaterial, mesh.MdfQueue);
                        drawcall.TagObject = node;
                        drawcall.SetSourceAtom(this);
                        drawcall.BindShaderEffect(effect);
                        drawcall.BindGeomMesh(MeshPrimitives.mCoreObject.GetGeomtryMesh());
                        drawcall.BindPipeline(Material.Pipeline);
                        drawcall.BindGBuffer(policy.DefaultCamera, targetView);
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
                        {
                            var binder = drawcall.FindBinder("DefaultSampLinear");
                            if (binder.IsValidPointer)
                            {
                                drawcall.BindSampler(binder, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
                            }
                        }
                        #endregion

                        #region CBuffer
                        unsafe
                        {
                            if (TtEngine.Instance.GfxDevice.PerFrameCBuffer != null)
                            {
                                drawcall.BindCBV(effect.BindIndexer.cbPerFrame, TtEngine.Instance.GfxDevice.PerFrameCBuffer);
                            }
                            if (this.SubMesh.Mesh.PerMeshCBuffer != null)
                            {
                                drawcall.BindCBV(effect.BindIndexer.cbPerMesh, this.SubMesh.Mesh.PerMeshCBuffer);
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
                                    drawcall.BindCBV(effect.BindIndexer.cbPerMaterial, Material.PerMaterialCBuffer);
                                }
                            }
                        }
                        #endregion

                        MdfQueue.OnBuildDrawCall(policy, drawcall, this);
                        shading.OnBuildDrawCall(policy, drawcall);

                        if (this.SubMesh.Mesh.OnBuildDrawcall!=null)
                        {
                            this.SubMesh.Mesh.OnBuildDrawcall(drawcall);
                        }

                        vdc.DrawCalls = drawcall;
                        vdc.State = 1;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Assert(false);
                    Profiler.Log.WriteException(ex);
                    Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, "policy.GetPassShading except");
                }
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
            private ViewDrawCalls GetOrCreateDrawCalls(Pipeline.TtGraphicsBuffers.TtTargetViewIdentifier id, Pipeline.TtRenderPolicy policy)
            {
                ViewDrawCalls drawCalls = null;
                //查找或者缓存TargetView
                for (int i = 0; i < TargetViews.Count; i++)
                {
                    Pipeline.TtGraphicsBuffers.TtTargetViewIdentifier identifier;
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
                    drawCalls.TargetView = new WeakReference<Pipeline.TtGraphicsBuffers.TtTargetViewIdentifier>(id);
                    drawCalls.Policy = policy;
                    TargetViews.Add(drawCalls);
                }
                return drawCalls;
            }
            [ThreadStatic]
            private static Profiler.TimeScope mScopeOnDrawCall;
            private static Profiler.TimeScope ScopeOnDrawCall
            {
                get
                {
                    if (mScopeOnDrawCall == null)
                        mScopeOnDrawCall = new Profiler.TimeScope(typeof(TtAtom), "OnDrawCall");
                    return mScopeOnDrawCall;
                }
            } 
            public unsafe virtual NxRHI.TtGraphicDraw GetDrawCall(NxRHI.ICommandList cmd, Pipeline.TtGraphicsBuffers targetView, Pipeline.TtRenderPolicy policy,
                Pipeline.TtRenderGraphNode node, bool bForce = false)
            {
                if (Material == null)
                    return null;

                bForce |= policy.IsSyncBuildDrawcall;

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
                var result = drawCalls.DrawCalls;
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
                    drawCalls.State = 0;
                }

                switch (drawCalls.State)
                {
                    case 1:
                        break;
                    case 0:
                        {
                            var task = BuildDrawCall(drawCalls, targetView, policy, node);
                            if (bForce)
                            {
                                task.WaitCompleted();
                            }
                            if (task.IsCompleted)
                            {
                                task.Dispose();
                            }
                            else
                            {
                                task.AddWaitTask(null);
                                return null;
                            }
                        }
                        break;
                    case -1:
                    default:
                        return null;
                }

                result = drawCalls.DrawCalls;
                if (result == null)
                {
                    //如果需要这个Pass，那么BuildDrawCall中的policy.GetPassShading就应该能提供shading
                    System.Diagnostics.Debug.Assert(false);
                }
                //检查shading切换参数
                if (result.IsPermutationChanged())
                {
                    Material = this.GetMeshMaterial();
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    return null;
                }

                result.TagObject = node;
                
                var shading = node.GetPassShading(this);
                using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
                {
                    shading.OnDrawCall(cmd, result, policy, this);
                    this.MdfQueue.OnDrawCall(cmd, result, policy, this);
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
            public TtRenderMesh Mesh;
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
        [Rtti.Meta("")]
        public Pipeline.Shader.TtMaterial GetMaterial(uint subMesh, uint atom)
        {
            if (subMesh >= SubMeshes.Count)
                return null;
            if (atom >= SubMeshes[(int)subMesh].Atoms.Count)
                return null;
            return SubMeshes[(int)subMesh].Atoms[(int)atom].Material;
        }
        [ReadOnly(true)]
        public string MdfQueueType
        {
            get
            {
                return Rtti.TtTypeDesc.TypeStr(this.MdfQueue.GetType());
            }
            set
            {
                SetMdfQueueType(Rtti.TtTypeDesc.TypeOf(value));
            }
        }
        internal bool SetMdfQueueType(Rtti.TtTypeDesc mdfQueueType)
        {
            var mdf = Rtti.TtTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
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
        public bool Initialize(TtMaterialMesh materialMesh, Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.TtTypeDescGetter<TtAtom>.TypeDesc;
            if (atomType != Rtti.TtTypeDescGetter<TtAtom>.TypeDesc && atomType.IsSubclassOf(typeof(TtAtom)) == false)
                return false;
            
            MdfQueue = Rtti.TtTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
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
                    sbMesh.Atoms[i] = Rtti.TtTypeDescManager.CreateInstance(atomType) as TtAtom;
                    sbMesh.Atoms[i].SubMesh = sbMesh;
                    sbMesh.Atoms[i].AtomIndex = i;
                    sbMesh.Atoms[i].Material = MaterialMesh.SubMeshes[j].Materials[i];
                }
                SubMeshes.Add(sbMesh);
            }

            return true;
        }
        public async Thread.Async.TtTask<bool> Initialize(RName materialMesh, Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            MaterialMesh = await materialMesh.GetAsset<Graphics.Mesh.TtMaterialMesh>();
            if (MaterialMesh == null)
                return false;

            return Initialize(MaterialMesh, mdfQueueType, atomType);
        }
        public async Thread.Async.TtTask<bool> Initialize(List<RName> meshSource, List<List<Pipeline.Shader.TtMaterial>> materials,
            Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.TtTypeDesc.TypeOf(typeof(TtAtom));
            if (atomType != Rtti.TtTypeDesc.TypeOf(typeof(TtAtom)) && atomType.IsSubclassOf(typeof(TtAtom)) == false)
                return false;

            var mesh = new List<TtMeshPrimitives>();
            foreach(var i in meshSource)
            {
                var tm = await i.GetAsset<Graphics.Mesh.TtMeshPrimitives>();
                mesh.Add(tm);
            }
            if (false == UpdateMesh(mesh, materials, atomType))
                return false;

            MdfQueue = Rtti.TtTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
            if (MdfQueue == null)
                return false;
            MdfQueue.Initialize(MaterialMesh);
            return true;
        }

        public bool Initialize(List<TtMeshPrimitives> mesh, List<List<Pipeline.Shader.TtMaterial>> materials,
            Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            if (false == UpdateMesh(mesh, materials, atomType))
                return false;

            MdfQueue = Rtti.TtTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.TtMdfQueueBase;
            if (MdfQueue == null)
                return false;
            MdfQueue.Initialize(MaterialMesh);
            return true; 
        }
        public bool Initialize(TtMeshPrimitives mesh, Pipeline.Shader.TtMaterial[] mats,
            Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            var materials = ListExtra.CreateList(mats);
            return Initialize(new List<TtMeshPrimitives>(){ mesh }, 
                new List<List<Pipeline.Shader.TtMaterial>>() { materials }, 
                mdfQueueType, atomType);
        }
        public bool Initialize(TtMeshPrimitives mesh, List<Pipeline.Shader.TtMaterial> materials,
            Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            return Initialize(new List<TtMeshPrimitives>() { mesh },
                new List<List<Pipeline.Shader.TtMaterial>>() { materials },
                mdfQueueType, atomType);
        }
        public async Thread.Async.TtTask<bool> Initialize(RName meshName, List<Pipeline.Shader.TtMaterial> materials,
            Rtti.TtTypeDesc mdfQueueType, Rtti.TtTypeDesc atomType = null)
        {
            var mesh = await meshName.GetAsset<Graphics.Mesh.TtMeshPrimitives>();
            return Initialize(new List<TtMeshPrimitives>() { mesh },
                new List<List<Pipeline.Shader.TtMaterial>>() { materials },
                mdfQueueType, atomType);
        }
        public bool UpdateMaterial(Pipeline.Shader.TtMaterial material, int subMesh = -1, Rtti.TtTypeDesc atomType = null)
        {
            if(subMesh >= 0 && subMesh < MaterialMesh.SubMeshes.Count)
            {
                var sbMesh = MaterialMesh.SubMeshes[subMesh];
                return UpdateMesh(subMesh, sbMesh.Mesh, material, atomType);
            }
            else
            {
                for (int i = 0; i < MaterialMesh.SubMeshes.Count; i++)
                {
                    if (!UpdateMaterial(material, i, atomType))
                        return false;
                }
            }
            return true;
        }
        public bool UpdateMaterial(List<Pipeline.Shader.TtMaterial> materials, int subMesh = -1, Rtti.TtTypeDesc atomType = null)
        {
            if(subMesh >= 0 && subMesh < MaterialMesh.SubMeshes.Count)
            {
                var sbMesh = MaterialMesh.SubMeshes[subMesh];
                return UpdateMesh(subMesh, sbMesh.Mesh, materials, atomType);
            }
            else
            {
                for (int i = 0; i < MaterialMesh.SubMeshes.Count; i++)
                {
                    if (!UpdateMaterial(materials, i, atomType))
                        return false;
                }
            }
            return true;
        }
        public bool UpdateMesh(int subMesh, TtMeshPrimitives mesh, Pipeline.Shader.TtMaterial material, Rtti.TtTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.TtTypeDescGetter<TtAtom>.TypeDesc;

            var sbMesh = MaterialMesh.SubMeshes[subMesh];
            sbMesh.Mesh = mesh;
            for (int i = 0; i < sbMesh.Materials.Count; i++)
            {
                sbMesh.Materials[i] = material;
            }

            MeshAtomUpdate(subMesh, atomType);
            return true;
        }
        public bool UpdateMesh(int subMesh, TtMeshPrimitives mesh, List<Pipeline.Shader.TtMaterial> materials, Rtti.TtTypeDesc atomType = null)
        {
            if (materials.Count == 0)
                return false;
            if (atomType == null)
                atomType = Rtti.TtTypeDescGetter<TtAtom>.TypeDesc;

            if (subMesh >= MaterialMesh.SubMeshes.Count)
                return false;

            var sbMesh = MaterialMesh.SubMeshes[subMesh];
            sbMesh.Mesh = mesh;
            //if (sbMesh.Materials.Count > materials.Count)
            //    return false;
            for (int i = 0; i < sbMesh.Materials.Count; i++)
            {
                sbMesh.Materials[i] = materials[Math.Min(i, materials.Count - 1)];
            }

            MeshAtomUpdate(subMesh, atomType);
            return true;
        }
        void MeshAtomUpdate(int subMesh, Rtti.TtTypeDesc atomType)
        {
            var sbMesh = MaterialMesh.SubMeshes[subMesh];
            var tarMesh = this.SubMeshes[subMesh];
            System.Diagnostics.Debug.Assert(tarMesh.MeshIndex == subMesh);
            System.Diagnostics.Debug.Assert(sbMesh.Materials.Count > 0);
            if (tarMesh.Atoms == null || tarMesh.Atoms.Count != sbMesh.Materials.Count)
            {
                tarMesh.Atoms.Resize(sbMesh.Materials.Count);
            }
            for (int i = 0; i < tarMesh.Atoms.Count; i++)
            {
                if (tarMesh.Atoms[i] == null || tarMesh.Atoms[i].GetType() != atomType.SystemType)
                {
                    tarMesh.Atoms[i] = Rtti.TtTypeDescManager.CreateInstance(atomType) as TtAtom;
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
        }
        public bool UpdateMesh(List<TtMeshPrimitives> mesh, List<List<Pipeline.Shader.TtMaterial>> materials, Rtti.TtTypeDesc atomType = null)
        {
            if (mesh.Count != materials.Count)
                return false;
            if (MaterialMesh == null || MaterialMesh.SubMeshes.Count != mesh.Count)
            {
                MaterialMesh = new TtMaterialMesh();
                if (false == MaterialMesh.Initialize(mesh, materials))
                    return false;
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
        public void UpdateCameraOffset(GamePlay.TtWorld world)
        {
            if (PerMeshCBuffer == null || world == null)
                return;
            var tm = PerMeshCBuffer.GetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrix);
            var realPos = WorldLocation - world.CameraOffset;
            tm.Translation = realPos.ToSingleVector3();
            this.DirectSetWorldMatrix(in tm);
        }
        private DVector3 WorldLocation
        {
            get => mTransform.Position;
        }
        public DBoundingBox WorldAABB
        {
            get
            {
                DBoundingBox result;
                var src = new DBoundingBox(in MaterialMesh.AABB);
                DBoundingBox.Transform(in src, in mTransform, out result);
                return result;
            }
        }
        public FTransform mTransform = FTransform.Identity;
        public void SetWorldTransform(in FTransform transform, GamePlay.TtWorld world, bool isNoScale)
        {
            mTransform = transform;
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
                    this.DirectSetWorldMatrix(transform.ToMatrixWithScale(in DVector3.Zero));
                else
                    this.DirectSetWorldMatrix(transform.ToMatrixNoScale(in DVector3.Zero));
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
                    PerMeshCBuffer.SetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrix, in savedTM);
                    var inv = Matrix.Invert(in savedTM);
                    PerMeshCBuffer.SetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrixInverse, in inv);
                };
                return;
            }   
            PerMeshCBuffer.SetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrix, in tm);
            var inv = Matrix.Invert(in tm);
            PerMeshCBuffer.SetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrixInverse, in inv);
        }
        public Matrix GetWorldMatrix()
        {
            if (PerMeshCBuffer == null)
                return Matrix.Identity;
            return PerMeshCBuffer.GetMatrix(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrix);
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

                    PerMeshCBuffer.SetValue(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.HitProxyId, in savedValue);
                };
                return;
            }
            PerMeshCBuffer.SetValue(TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.HitProxyId, in value);
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Mesh
{
	partial class TtRenderMesh
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetMaterial_899003873 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Graphics.Mesh.TtRenderMesh->Pipeline.Shader.TtMaterial GetMaterial(uint subMesh, uint atom)");
		public unsafe Pipeline.Shader.TtMaterial macross_GetMaterial (string nodeName, uint subMesh, uint atom) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":subMesh", subMesh);
					stackframe.SetWatchVariable(nodeName + ":atom", atom);
				}
			}
			var _return_value = GetMaterial(subMesh, atom);
			macross_break_GetMaterial_899003873.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross