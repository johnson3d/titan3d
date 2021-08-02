using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMesh
    {
        public GamePlay.Scene.UNode HostNode;
        public UMaterialMesh MaterialMesh { get; private set; }
        public Pipeline.Shader.UMdfQueue MdfQueue { get; private set; }
        RHI.CConstantBuffer mPerMeshCBuffer;
        protected System.Action OnAfterCBufferCreated;
        public RHI.CConstantBuffer PerMeshCBuffer 
        {
            get => mPerMeshCBuffer;
            private set
            {
                mPerMeshCBuffer = value;
                if (value != null)
                {
                    if (OnAfterCBufferCreated != null)
                    {
                        OnAfterCBufferCreated();
                        OnAfterCBufferCreated = null;
                    }
                    else
                    {
                        SetWorldMatrix(ref Matrix.mIdentity);
                    }
                }
            }
        }
        public class UMeshAttachment
        {
            
        }
        public bool IsDrawHitproxy = false;
        public UMeshAttachment Tag { get; set; }
        public class UAtom
        {
            public uint SerialId;
            public Pipeline.Shader.UMaterial Material;
            public class ViewDrawCalls
            {
                public WeakReference<Pipeline.UGraphicsBuffers.UTargetViewIdentifier> TargetView;
                public Pipeline.IRenderPolicy Policy;
                public RHI.CDrawCall[] DrawCalls;

                //甚至我们可以放一个cbuffer在这里处理PerMesh&&PerView的变量: CameraPositionInModel
            }
            public void ResetDrawCalls()
            {
                TargetViews?.Clear();
                TargetViews = null;
                TaskBuildDrawCall = null;
            }
            public List<ViewDrawCalls> TargetViews;
            System.Threading.Tasks.Task TaskBuildDrawCall = null;
            public unsafe virtual RHI.CDrawCall GetDrawCall(Pipeline.UGraphicsBuffers targetView, UMesh mesh, int atom, Pipeline.IRenderPolicy policy, Pipeline.IRenderPolicy.EShadingType shadingType)
            {
                if (Material != mesh.MaterialMesh.Materials[atom] || Material.SerialId != SerialId)
                {
                    Material = mesh.MaterialMesh.Materials[atom];
                    SerialId = Material.SerialId;
                    ResetDrawCalls();
                }
                //每个TargetView都要对应一个DrawCall数组
                ViewDrawCalls drawCalls = null;
                if (TargetViews == null)
                {
                    TargetViews = new List<ViewDrawCalls>();
                    drawCalls = new ViewDrawCalls();
                    drawCalls.TargetView = new WeakReference<Pipeline.UGraphicsBuffers.UTargetViewIdentifier>(targetView.TargetViewIdentifier);
                    drawCalls.Policy = null;
                    drawCalls.DrawCalls = new RHI.CDrawCall[(int)Pipeline.IRenderPolicy.EShadingType.Count];
                    TargetViews.Add(drawCalls);
                }
                else
                {
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
                        else if (identifier == targetView.TargetViewIdentifier)
                        {
                            drawCalls = TargetViews[i];
                            break;
                        }
                    }
                    if (drawCalls == null)
                    {
                        drawCalls = new ViewDrawCalls();
                        drawCalls.TargetView = new WeakReference<Pipeline.UGraphicsBuffers.UTargetViewIdentifier>(targetView.TargetViewIdentifier);
                        drawCalls.Policy = null;
                        drawCalls.DrawCalls = new RHI.CDrawCall[(int)Pipeline.IRenderPolicy.EShadingType.Count];
                        TargetViews.Add(drawCalls);
                    }
                }
                System.Diagnostics.Debug.Assert(policy != null);
                if (drawCalls.Policy != policy)
                {//渲染策略改变
                    if (TaskBuildDrawCall == null)
                    {
                        TaskBuildDrawCall = BuildDrawCall(drawCalls.DrawCalls, mesh, atom, policy, shadingType);
                        drawCalls.Policy = policy;
                    }
                }

                if (TaskBuildDrawCall != null)
                {
                    if (TaskBuildDrawCall.IsCompleted)
                    {
                        TaskBuildDrawCall = null;
                    }
                    else
                        return null;
                }

                var result = drawCalls.DrawCalls[(int)shadingType];
                if (result == null)
                    return null;

                //检查shading切换参数或者材质HLSL被编辑器修改
                result.CheckPermutation(Material.ParentMaterial, mesh.MdfQueue);
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
            public async System.Threading.Tasks.Task BuildDrawCall(RHI.CDrawCall[] drawCalls, UMesh mesh, int atom, Pipeline.IRenderPolicy policy, Pipeline.IRenderPolicy.EShadingType shadingType)
            {
                if (atom >= mesh.MaterialMesh.Materials.Length)
                    return;
                for (Pipeline.IRenderPolicy.EShadingType i = Pipeline.IRenderPolicy.EShadingType.BasePass;
                    i < Pipeline.IRenderPolicy.EShadingType.Count; i++)
                {
                    var shading = policy.GetPassShading(i, mesh, atom);
                    if (shading != null)
                    {
                        var drawcall = await UEngine.Instance.GfxDevice.RenderContext.CreateDrawCall(shading, Material.ParentMaterial, mesh.MdfQueue);
                        if (drawcall == null || drawcall.Effect == null)
                            continue;
                        #region Textures
                        unsafe
                        {
                            drawcall.mCoreObject.BindGeometry(mesh.MaterialMesh.Mesh.mCoreObject, (uint)atom, 0);
                        }
                        var textures = new RHI.CShaderResources();
                        for (int j = 0; j < Material.NumOfSRV; j++)
                        {
                            var varName = Material.GetNameOfSRV(j);
                            var bindInfo = new TSBindInfo();
                            unsafe
                            {
                                drawcall.mCoreObject.GetSRVBindInfo(varName, ref bindInfo, sizeof(TSBindInfo));
                            }
                            var srv = await Material.GetSRV(j);
                            unsafe
                            {
                                if (bindInfo.PSBindPoint != 0xffffffff && srv != null)
                                {
                                    textures.mCoreObject.PSBindTexture(bindInfo.PSBindPoint, srv.mCoreObject);
                                }
                                if (bindInfo.VSBindPoint != 0xffffffff && srv != null)
                                {
                                    textures.mCoreObject.VSBindTexture(bindInfo.VSBindPoint, srv.mCoreObject);
                                }
                            }
                        }
                        unsafe
                        {
                            drawcall.mCoreObject.BindShaderResources(textures.mCoreObject);
                        }
                        #endregion

                        #region Samplers
                        unsafe
                        {
                            var samplers = new RHI.CShaderSamplers();
                            for (int j = 0; j < Material.NumOfSampler; j++)
                            {
                                var varName = Material.GetNameOfSampler(j);
                                var bindInfo = new TSBindInfo();

                                drawcall.mCoreObject.GetSamplerBindInfo(varName, ref bindInfo, sizeof(TSBindInfo));
                                if (bindInfo.PSBindPoint != 0xffffffff)
                                {
                                    samplers.mCoreObject.PSBindSampler(bindInfo.PSBindPoint, Material.GetSampler(j).mCoreObject);
                                }
                                if (bindInfo.VSBindPoint != 0xffffffff)
                                {
                                    samplers.mCoreObject.VSBindSampler(bindInfo.VSBindPoint, Material.GetSampler(j).mCoreObject);
                                }
                            }
                            drawcall.mCoreObject.BindShaderSamplers(samplers.mCoreObject);

                            // renwind modified: debug code set globalSamplerState
                            //var splDesc = new ISamplerStateDesc();
                            //splDesc.SetDefault();
                            //splDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                            //splDesc.AddressU = EAddressMode.ADM_WRAP;
                            //splDesc.AddressV = EAddressMode.ADM_WRAP;
                            //splDesc.AddressW = EAddressMode.ADM_WRAP;
                            //splDesc.MipLODBias = 0;
                            //splDesc.MaxAnisotropy = 0;
                            //splDesc.CmpMode = EComparisionMode.CMP_ALWAYS;
                            //var rc = UEngine.Instance.GfxDevice.RenderContext;
                            //var SamplerState = UEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(rc, ref splDesc);
                            //samplers.mCoreObject.PSBindSampler(0, SamplerState.mCoreObject);
                        }
                        #endregion

                        #region CBuffer
                        unsafe
                        {
                            var gpuProgram = drawcall.Effect.ShaderProgram;
                            if (drawcall.Effect.CBPerFrameIndex != 0xFFFFFFFF && UEngine.Instance.GfxDevice.PerFrameCBuffer != null)
                            {
                                drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerFrameIndex, UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject);
                            }
                            if (drawcall.Effect.CBPerMeshIndex != 0xFFFFFFFF)
                            {
                                if (mesh.PerMeshCBuffer == null)
                                {
                                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                                    mesh.PerMeshCBuffer = rc.CreateConstantBuffer(gpuProgram, drawcall.Effect.CBPerMeshIndex);
                                }
                                drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerMeshIndex, mesh.PerMeshCBuffer.mCoreObject);
                            }
                            if (drawcall.Effect.CBPerMaterialIndex != 0xFFFFFFFF)
                            {
                                if (Material != null)
                                {
                                    if (Material.PerMaterialCBuffer == null)
                                    {
                                        var rc = UEngine.Instance.GfxDevice.RenderContext;
                                        Material.PerMaterialCBuffer = rc.CreateConstantBuffer(gpuProgram, drawcall.Effect.CBPerMaterialIndex);
                                        Material.UpdateUniformVars(Material.PerMaterialCBuffer);
                                    }
                                    drawcall.mCoreObject.BindCBufferAll(drawcall.Effect.CBPerMaterialIndex, Material.PerMaterialCBuffer.mCoreObject);
                                }
                            }
                        }
                        #endregion

                        #region State
                        unsafe
                        {
                            var pipeline = new IRenderPipeline(drawcall.mCoreObject.GetPipeline());
                            if (Material.RasterizerState != null)
                                pipeline.BindRasterizerState(Material.RasterizerState.mCoreObject);
                            if (Material.DepthStencilState != null)
                                pipeline.BindDepthStencilState(Material.DepthStencilState.mCoreObject);
                            if (Material.BlendState != null)
                                pipeline.BindBlendState(Material.BlendState.mCoreObject);
                        }
                        #endregion

                        shading.OnBuildDrawCall(drawcall);
                        //drawcall.MaterialSerialId = Material.SerialId;
                        drawCalls[(int)i] = drawcall;
                    }
                }
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
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(UAtom)) && atomType.SystemType.IsSubclassOf(typeof(UAtom)) == false)
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
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(UAtom)) && atomType.SystemType.IsSubclassOf(typeof(UAtom)) == false)
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
            if (atomType != Rtti.UTypeDesc.TypeOf(typeof(UAtom)) && atomType.SystemType.IsSubclassOf(typeof(UAtom)) == false)
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

        public bool Initialize(CMeshPrimitives mesh, Pipeline.Shader.UMaterial[] materials,
            Rtti.UTypeDesc mdfQueueType, Rtti.UTypeDesc atomType = null)
        {
            if (atomType == null)
                atomType = Rtti.UTypeDescGetter<UAtom>.TypeDesc;
            if (atomType != Rtti.UTypeDescGetter<UAtom>.TypeDesc && atomType.SystemType.IsSubclassOf(typeof(UAtom)) == false)
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
        public RHI.CDrawCall GetDrawCall(Pipeline.UGraphicsBuffers targetView, int atom, Pipeline.IRenderPolicy policy, Pipeline.IRenderPolicy.EShadingType shadingType)
        {
            if (atom >= Atoms.Length)
                return null;

            return Atoms[atom].GetDrawCall(targetView, this, atom, policy, shadingType);
        }

        public void SetWorldMatrix(ref Matrix tm)
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedTM = tm;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();
                    PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrix, ref savedTM);
                    var inv = Matrix.Invert(ref savedTM);
                    PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrixInverse, ref inv);
                };
                return;
            }   
            PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrix, ref tm);
            var inv = Matrix.Invert(ref tm);
            PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrixInverse, ref inv);
        }
        public void SetValue<T>(int index, ref T value, uint elem = 0) where T : unmanaged
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedValue = value;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();

                    PerMeshCBuffer.SetValue(index, ref savedValue, elem);
                };
                return;
            }
            PerMeshCBuffer.SetValue(index, ref value, elem);
        }
        public void SetHitproxy(ref Vector4 value)
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedValue = value;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();

                    PerMeshCBuffer.SetValue(PerMeshCBuffer.PerMeshIndexer.HitProxyId, ref savedValue, 0);
                };
                return;
            }
            PerMeshCBuffer.SetValue(PerMeshCBuffer.PerMeshIndexer.HitProxyId, ref value, 0);
        }
    }
}
