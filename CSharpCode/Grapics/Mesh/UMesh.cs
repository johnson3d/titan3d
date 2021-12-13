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
            get
            {
                if (mPerMeshCBuffer == null)
                {
                    var effect = UEngine.Instance.GfxDevice.EffectManager.DummyEffect;
                    mPerMeshCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(effect.ShaderProgram, effect.CBPerMeshIndex);
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
                public RHI.CDrawCall[] DrawCalls;
                //不同的View上可以有不同的渲染策略，同一个模型，可以渲染在不同视口上，比如装备预览的策略可以和GameView不一样
                public Pipeline.IRenderPolicy Policy;                
            }
            public List<ViewDrawCalls> TargetViews;
            
            private async System.Threading.Tasks.Task BuildDrawCall(ViewDrawCalls vdc, UMesh mesh, int atom, Pipeline.IRenderPolicy policy, Pipeline.IRenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
            {
                if (atom >= mesh.MaterialMesh.Materials.Length)
                    return;
                RHI.CDrawCall[] drawCalls = vdc.DrawCalls;
                for (Pipeline.IRenderPolicy.EShadingType i = Pipeline.IRenderPolicy.EShadingType.BasePass;
                    i < Pipeline.IRenderPolicy.EShadingType.Count; i++)
                {
                    var shading = policy.GetPassShading(i, mesh, atom, node);
                    if (shading != null)
                    {
                        var drawcall = await UEngine.Instance.GfxDevice.RenderContext.CreateDrawCall(shading, Material.ParentMaterial, mesh.MdfQueue);
                        if (drawcall == null || drawcall.Effect == null)
                            continue;
                        var reflector = drawcall.mCoreObject.GetReflector();
                        #region Textures
                        unsafe
                        {
                            drawcall.mCoreObject.BindGeometry(mesh.MaterialMesh.Mesh.mCoreObject, (uint)atom, 0);
                        }                
                        for (int j = 0; j < Material.NumOfSRV; j++)
                        {
                            var srv = await Material.GetSRV(j);
                            var varName = Material.GetNameOfSRV(j);                            
                            unsafe
                            {
                                IShaderBinder* bindInfo = reflector.GetShaderBinder(EShaderBindType.SBT_Srv, varName);
                                if (!CoreSDK.IsNullPointer(bindInfo) && srv != null)
                                {
                                    drawcall.mCoreObject.BindShaderSrv(bindInfo, srv.mCoreObject);
                                }
                            }
                        }
                        #endregion

                        #region Samplers
                        unsafe
                        {
                            for (int j = 0; j < Material.NumOfSampler; j++)
                            {
                                var varName = Material.GetNameOfSampler(j);
                                var sampler = Material.GetSampler(j);
                                IShaderBinder* bindInfo = reflector.GetShaderBinder(EShaderBindType.SBT_Sampler, varName);
                                if (!CoreSDK.IsNullPointer(bindInfo))
                                {
                                    drawcall.mCoreObject.BindShaderSampler(bindInfo, sampler.mCoreObject);
                                }
                                //else
                                //{
                                //    //System.Diagnostics.Debugger.Break();
                                //    System.Diagnostics.Debug.WriteLine($"Sampler not find: {varName}");
                                //}
                            }

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
                            
                            //var binder = drawcall.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Sampler, "gDefaultSamplerState");
                            //if (binder!=(IShaderBinder*)0)
                            //{
                            //    var SamplerState = UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState;
                            //    drawcall.mCoreObject.GetShaderSamplers().BindPS(binder->PSBindPoint, SamplerState.mCoreObject);
                            //}

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
                                drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerFrameIndex, UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject);
                            }
                            if (drawcall.Effect.CBPerMeshIndex != 0xFFFFFFFF)
                            {
                                drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerMeshIndex, mesh.PerMeshCBuffer.mCoreObject);
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
                                    drawcall.mCoreObject.BindShaderCBuffer(drawcall.Effect.CBPerMaterialIndex, Material.PerMaterialCBuffer.mCoreObject);
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
            private ViewDrawCalls GetOrCreateDrawCalls(Pipeline.UGraphicsBuffers.UTargetViewIdentifier id, Pipeline.IRenderPolicy policy)
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
                    drawCalls.DrawCalls = new RHI.CDrawCall[(int)Pipeline.IRenderPolicy.EShadingType.Count];
                    drawCalls.Policy = policy;
                    TargetViews.Add(drawCalls);
                }
                return drawCalls;
            }
            public unsafe virtual RHI.CDrawCall GetDrawCall(Pipeline.UGraphicsBuffers targetView, UMesh mesh, int atom, Pipeline.IRenderPolicy policy, Pipeline.IRenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
            {
                if (Material != mesh.MaterialMesh.Materials[atom] || 
                    Material.SerialId != MaterialSerialId)
                {
                    Material = mesh.MaterialMesh.Materials[atom];
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
        public RHI.CDrawCall GetDrawCall(Pipeline.UGraphicsBuffers targetView, int atom, Pipeline.IRenderPolicy policy, Pipeline.IRenderPolicy.EShadingType shadingType, Pipeline.Common.URenderGraphNode node)
        {
            if (atom >= Atoms.Length)
                return null;

            return Atoms[atom].GetDrawCall(targetView, this, atom, policy, shadingType, node);
        }
        public void UpdateCameraOffset(GamePlay.UWorld world)
        {
            if (PerMeshCBuffer == null)
                return;
            var tm = PerMeshCBuffer.GetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrix);
            var realPos = CameraOffset + tm.Translation - world.CameraOffset;
            tm.Translation = realPos.ToSingleVector3();
            CameraOffset = world.CameraOffset;
            this.SetWorldMatrix(in tm);
        }
        private DVector3 CameraOffset = DVector3.Zero;
        public void SetWorldTransform(in FTransform transform, GamePlay.UWorld world, bool isNoScale)
        {
            if (world != null)
            {
                CameraOffset = world.CameraOffset;
                if (isNoScale == false)
                    this.SetWorldMatrix(transform.ToMatrixWithScale(in world.mCameraOffset));
                else
                    this.SetWorldMatrix(transform.ToMatrixNoScale(in world.mCameraOffset));
            }
            else
            {
                CameraOffset = transform.Position;
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
                    PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrix, in savedTM);
                    var inv = Matrix.Invert(in savedTM);
                    PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrixInverse, in inv);
                };
                return;
            }   
            PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrix, in tm);
            var inv = Matrix.Invert(in tm);
            PerMeshCBuffer.SetMatrix(PerMeshCBuffer.PerMeshIndexer.WorldMatrixInverse, in inv);
        }
        public void SetValue<T>(int index, in T value, uint elem = 0) where T : unmanaged
        {
            if (PerMeshCBuffer == null)
            {
                var saved = OnAfterCBufferCreated;
                var savedValue = value;
                OnAfterCBufferCreated = () =>
                {
                    if (saved != null)
                        saved();

                    PerMeshCBuffer.SetValue(index, in savedValue, elem);
                };
                return;
            }
            PerMeshCBuffer.SetValue(index, in value, elem);
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

                    PerMeshCBuffer.SetValue(PerMeshCBuffer.PerMeshIndexer.HitProxyId, in savedValue, 0);
                };
                return;
            }
            PerMeshCBuffer.SetValue(PerMeshCBuffer.PerMeshIndexer.HitProxyId, in value, 0);
        }
    }
}
