using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMesh
    {
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
        public Pipeline.Shader.UMdfQueue MdfQueue { get; private set; }
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
                        SetWorldMatrix(in Matrix.Identity);
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
        public class UAtom
        {
            public uint MaterialSerialId;
            public Pipeline.Shader.UMaterial Material;
            public class ViewDrawCalls
            {
                internal int State = 0;
                public WeakReference<Pipeline.UGraphicsBuffers.UTargetViewIdentifier> TargetView;
                public NxRHI.UGraphicDraw[] DrawCalls;
                //不同的View上可以有不同的渲染策略，同一个模型，可以渲染在不同视口上，比如装备预览的策略可以和GameView不一样
                public Pipeline.URenderPolicy Policy;                
            }
            public List<ViewDrawCalls> TargetViews;
            
            private async System.Threading.Tasks.Task BuildDrawCall(ViewDrawCalls vdc, UMesh mesh, int atom, Pipeline.URenderPolicy policy,
                Pipeline.URenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
            {
                if (atom >= mesh.MaterialMesh.Materials.Length)
                    return;
                var device = UEngine.Instance.GfxDevice;
                NxRHI.UGraphicDraw[] drawCalls = vdc.DrawCalls;
                for (Pipeline.URenderPolicy.EShadingType i = Pipeline.URenderPolicy.EShadingType.BasePass;
                    i < Pipeline.URenderPolicy.EShadingType.Count; i++)
                {
                    Graphics.Pipeline.Shader.UShadingEnv shading = null;
                    try
                    {
                        shading = policy.GetPassShading(i, mesh, atom, node);
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Mesh", "policy.GetPassShading except");
                        continue;
                    }
                    if (shading != null)
                    {
                        var effect = await UEngine.Instance.GfxDevice.EffectManager.GetEffect(shading, Material.ParentMaterial, mesh.MdfQueue);
                        if (effect == null)
                            continue;
                        var drawcall = UEngine.Instance.GfxDevice.RenderContext.CreateGraphicDraw();// (shading, Material.ParentMaterial, mesh.MdfQueue);
                        drawcall.MeshAtom = (byte)atom;
                        drawcall.MeshLOD = 0;
                        drawcall.BindShaderEffect(effect);
                        drawcall.BindGeomMesh(mesh.MaterialMesh.Mesh.mCoreObject.GetGeomtryMesh());
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
                            if (mesh.PerMeshCBuffer != null)
                            {
                                drawcall.BindCBuffer(effect.BindIndexer.cbPerMesh, mesh.PerMeshCBuffer);
                            }
                            if (Material != null)
                            {
                                if (Material.PerMaterialCBuffer == null)
                                {
                                    if (Material.CreateCBuffer(effect))
                                        Material.UpdateUniformVars(Material.PerMaterialCBuffer, Material.PerMaterialCBuffer.ShaderBinder);
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
                        drawCalls[(int)i] = drawcall;
                    }
                }

                vdc.State = 1;
            }
            internal void ResetDrawCalls()
            {
                TargetViews?.Clear();
                TargetViews = null;
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
            public unsafe virtual NxRHI.UGraphicDraw GetDrawCall(Pipeline.UGraphicsBuffers targetView, UMesh mesh, int atom, Pipeline.URenderPolicy policy,
                Pipeline.URenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
            {
                if (Material == null)
                    return null;
                if (Material != mesh.MaterialMesh.Materials[atom] || 
                    Material.SerialId != MaterialSerialId)
                {
                    Material = mesh.MaterialMesh.Materials[atom];
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

                switch (drawCalls.State)
                {
                    case 1:
                        break;
                    case 0:
                        {
                            drawCalls.State = -1;
                            var task = BuildDrawCall(drawCalls, mesh, atom, policy, shadingType, node);
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

                if (drawCalls.Policy != policy)
                {
                    Material = mesh.MaterialMesh.Materials[atom];
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    return null;
                }

                var result = drawCalls.DrawCalls[(int)shadingType];
                if (result == null)
                {
                    Material = mesh.MaterialMesh.Materials[atom];
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    return null;
                }

                //检查shading切换参数
                if (result.IsPermutationChanged())
                {
                    Material = mesh.MaterialMesh.Materials[atom];
                    MaterialSerialId = Material.SerialId;
                    ResetDrawCalls();
                    return null;
                }
                //result.CheckPermutation(Material.ParentMaterial, mesh.MdfQueue);
                //检查材质参数被修改
                //if (result.CheckMaterialParameters(Material))
                //{
                //    if (result.Effect.CBPerMeshIndex != 0xFFFFFFFF)
                //    {
                //        result.mCoreObject.BindCBufferAll(result.Effect.CBPerMeshIndex, mesh.PerMeshCBuffer.mCoreObject);
                //    }
                //}
                
                policy.OnDrawCall(shadingType, result, mesh, atom);
                return result;
            }
        }
        public UAtom[] Atoms;
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
            var mdf = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.UMdfQueue;
            if (mdf == null)
                return false;

            mdf.CopyFrom(MdfQueue);
            MdfQueue = mdf;

            for (int i = 0; i < Atoms.Length; i++)
            {
                Atoms[i].ResetDrawCalls();
            }
            return true;
        }
        public bool Initialize(UMaterialMesh materialMesh, Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDesc.TypeOf(typeof(UAtom));
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(UAtom)) && atomType.IsSubclassOf(typeof(UAtom)) == false)
                return false;
            MaterialMesh = materialMesh;

            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.UMdfQueue;
            if (MdfQueue == null)
                return false;

            Atoms = new UAtom[MaterialMesh.Materials.Length];
            for (int i = 0; i < Atoms.Length; i++)
            {
                Atoms[i] = Rtti.UTypeDescManager.CreateInstance(atomType) as UAtom;
                Atoms[i].Material = MaterialMesh.Materials[i];
            }

            return true;
        }
        public async System.Threading.Tasks.Task<bool> Initialize(RName materialMesh, Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDesc.TypeOf(typeof(UAtom));
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(UAtom)) && atomType.IsSubclassOf(typeof(UAtom)) == false)
                return false;
            MaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(materialMesh);
            if (MaterialMesh == null)
                return false;

            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.UMdfQueue;
            if (MdfQueue == null)
                return false;

            Atoms = new UAtom[MaterialMesh.Materials.Length];
            for (int i = 0; i < Atoms.Length; i++)
            {
                Atoms[i] = Rtti.UTypeDescManager.CreateInstance(atomType) as UAtom;
                Atoms[i].Material = MaterialMesh.Materials[i];
            }

            return true;
        }
        public async System.Threading.Tasks.Task<bool> Initialize(RName meshSource, Pipeline.Shader.UMaterial[] materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDesc.TypeOf(typeof(UAtom));
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(UAtom)) && atomType.IsSubclassOf(typeof(UAtom)) == false)
                return false;

            MaterialMesh = new UMaterialMesh();
            MaterialMesh.Mesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(meshSource);

            if (MaterialMesh.Materials.Length != materials.Length)
                return false;

            for (int i = 0; i < materials.Length; i++)
            {
                MaterialMesh.Materials[i] = materials[i];
            }

            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.UMdfQueue;
            if (MdfQueue == null)
                return false;

            Atoms = new UAtom[MaterialMesh.Materials.Length];
            for (int i = 0; i < Atoms.Length; i++)
            {
                Atoms[i] = Rtti.UTypeDescManager.CreateInstance(atomType) as UAtom;
                Atoms[i].Material = MaterialMesh.Materials[i];
            }
            return true;
        }

        public bool Initialize(UMeshPrimitives mesh, Pipeline.Shader.UMaterial[] materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDescGetter<UAtom>.TypeDesc;
            if (atomType != Rtti.UTypeDescGetter<UAtom>.TypeDesc && atomType.IsSubclassOf(typeof(UAtom)) == false)
                return false;

            MaterialMesh = new UMaterialMesh();
            MaterialMesh.Mesh = mesh;

            if (MaterialMesh.Materials.Length > materials.Length)
                return false;

            for (int i = 0; i < MaterialMesh.Materials.Length; i++)
            {
                MaterialMesh.Materials[i] = materials[i];
            }

            MdfQueue = Rtti.UTypeDescManager.CreateInstance(mdfQueueType) as Pipeline.Shader.UMdfQueue;
            if (MdfQueue == null)
                return false;

            Atoms = new UAtom[MaterialMesh.Materials.Length];
            for (int i = 0; i < Atoms.Length; i++)
            {
                Atoms[i] = Rtti.UTypeDescManager.CreateInstance(atomType) as UAtom;
                Atoms[i].Material = MaterialMesh.Materials[i];
            }
            return true;
        }
        //获得DrawCall的入口
        //参数是渲染targetView
        //渲染原子Id
        //渲染策略policy
        //本次渲染的Shading模式
        public NxRHI.UGraphicDraw GetDrawCall(Pipeline.UGraphicsBuffers targetView, int atom, Pipeline.URenderPolicy policy, 
            Pipeline.URenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
        {
            if (atom >= Atoms.Length)
                return null;

            if (this.MaterialMesh.Mesh.mCoreObject.IsValidPointer == false)
                return null;

            return Atoms[atom].GetDrawCall(targetView, this, atom, policy, shadingType, node);
        }
        public void UpdateCameraOffset(GamePlay.UWorld world)
        {
            if (PerMeshCBuffer == null)
                return;
            var tm = PerMeshCBuffer.GetValue<Matrix>(UEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);
            var realPos = WorldLocation - world.CameraOffset;
            tm.Translation = realPos.ToSingleVector3();
            this.SetWorldMatrix(in tm);
        }
        private DVector3 WorldLocation = DVector3.Zero;
        public void SetWorldTransform(in FTransform transform, GamePlay.UWorld world, bool isNoScale)
        {
            WorldLocation = transform.Position;
            if (world != null)
            {
                if (isNoScale == false)
                    this.SetWorldMatrix(transform.ToMatrixWithScale(in world.mCameraOffset));
                else
                    this.SetWorldMatrix(transform.ToMatrixNoScale(in world.mCameraOffset));
            }
            else
            {
                if (isNoScale == false)
                    this.SetWorldMatrix(transform.ToMatrixWithScale(in transform.mPosition));
                else
                    this.SetWorldMatrix(transform.ToMatrixNoScale(in transform.mPosition));
            }
        }
        private void SetWorldMatrix(in Matrix tm)
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
