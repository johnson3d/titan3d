using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EngineNS.Graphics.Mesh
{
    
    public class CGfxMtlMesh : AuxCoreObject<CGfxMtlMesh.NativePointer>
    {
        //For editor..
        public bool Visible
        {
            get;
            set;
        } = true;

        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }


        public UInt32 MtlInstVersion;
        //public CShaderProgram MeshPassShaderProgram;
        public CGfxMesh mRootSceneMesh;
        public UInt32 mAtomIndex;

        //public bool mNeedToRefreshCBOnly = false;
        private CPass[] mPrebuildPassArray;

        
        public CPass[] PrebuildPassArray
        {
            get
            {
                return mPrebuildPassArray;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CPass GetPass(PrebuildPassIndex index)
        {
            if (mPrebuildPassArray[(int)index] == null)
                return null;
            return mPrebuildPassArray[(int)index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CPass GetFirstValidPass()
        {
            for (int index = 0; index < (int)PrebuildPassIndex.PPI_Num; index++)
            {
                if (mPrebuildPassArray[(int)index] == null)
                    continue;
                return mPrebuildPassArray[(int)index];
            }
            return null;
        }
        public void RefreshAllPassEffect(CRenderContext rc)
        {
            for (EngineNS.PrebuildPassIndex i = 0; i < EngineNS.PrebuildPassIndex.PPI_Num; i++)
            {
                RefreshPassEffect(rc, i);
            }
        }
        public void RefreshPassEffect(CRenderContext RHICtx, PrebuildPassIndex ppi)
        {
            var refPass = GetPass(ppi);
            if (refPass == null)
            {
                return;
            }

            refPass.Effect.Editor_RefreshEffect(RHICtx, false);
            refPass.PreUse();
        }

        public void GetSamplerBinder(CRenderContext RHICtx, CShaderProgram shader, CShaderSamplers sampler)
        {
            if (shader==null || MtlInst.Material.GetSamplerStateDescs().Count <= 0)
                return;
            using (var it = MtlInst.Material.GetSamplerStateDescs().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var name = it.Current.Key;
                    var samplerDesc = it.Current.Value;
                    CSamplerBindInfo info = new CSamplerBindInfo();
                    if (shader.FindSamplerBindInfoByShaderName(MtlInst, name, ref info))
                    {
                        CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, samplerDesc);
                        sampler.PSBindSampler(info.PSBindPoint, SamplerState);
                    }
                }
            }
        }

        public CDrawPrimitiveDesc mDPDesc = new CDrawPrimitiveDesc();

        public CShaderSamplers mSamplerBinder_HitProxy = null;
        public CShaderSamplers GetSamplerBinder_HitProxy(CRenderContext RHICtx, CShaderProgram shader)
        {
            if (shader == null)
                return null;

            if (mSamplerBinder_HitProxy == null)
            {
                mSamplerBinder_HitProxy = new CShaderSamplers();
                //var SamplerDesc = new CSamplerStateDesc();
                //SamplerDesc.SetDefault();
                //for (UInt32 i = 0; i < shader.SamplerNumber; i++)
                //{
                //    CSamplerBindInfo info = new CSamplerBindInfo();
                //    shader.GetSamplerBindInfo(i, ref info);
                //    MtlInst.GetSamplerStateDesc(i, ref SamplerDesc);
                //    CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, SamplerDesc);
                //    mSamplerBinder_HitProxy.PSBindSampler(info.PSBindPoint, SamplerState);
                //}
                using (var it = MtlInst.Material.GetSamplerStateDescs().GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var name = it.Current.Key;
                        var samplerDesc = it.Current.Value;
                        CSamplerBindInfo info = new CSamplerBindInfo();
                        if (shader.FindSamplerBindInfoByShaderName(MtlInst, name, ref info))
                        {
                            CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, samplerDesc);
                            mSamplerBinder_HitProxy.PSBindSampler(info.PSBindPoint, SamplerState);
                        }
                        else
                        {
                            var defaultSamplerDesc = new CSamplerStateDesc();
                            defaultSamplerDesc.SetDefault();
                            CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, defaultSamplerDesc);
                            mSamplerBinder_HitProxy.PSBindSampler(info.PSBindPoint, SamplerState);
                        }
                    }
                }
            }

            return mSamplerBinder_HitProxy;
        }

        public CShaderSamplers mSamplerBinder_PickedEditor = null;
        public CShaderSamplers GetSamplerBinder_PickedEditor(CRenderContext RHICtx, CShaderProgram shader)
        {
            if (shader == null)
            {
                return null;
            }
            
            if (mSamplerBinder_PickedEditor == null)
            {
                mSamplerBinder_PickedEditor = new CShaderSamplers();
                using (var it = MtlInst.Material.GetSamplerStateDescs().GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var name = it.Current.Key;
                        var samplerDesc = it.Current.Value;
                        CSamplerBindInfo info = new CSamplerBindInfo();
                        if (shader.FindSamplerBindInfoByShaderName(MtlInst, name, ref info))
                        {
                            CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, samplerDesc);
                            mSamplerBinder_PickedEditor.PSBindSampler(info.PSBindPoint, SamplerState);
                        }
                        else
                        {
                            var defaultSamplerDesc = new CSamplerStateDesc();
                            defaultSamplerDesc.SetDefault();
                            CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, defaultSamplerDesc);
                            mSamplerBinder_PickedEditor.PSBindSampler(info.PSBindPoint, SamplerState);
                        }
                    }
                }
            }
            return mSamplerBinder_PickedEditor;
        }


        public CShaderSamplers mSamplerBinder_Shadow = null;
        public CShaderSamplers GetSamplerBinder_Shadow(CRenderContext RHICtx, CShaderProgram shader)
        {
            if (shader == null)
                return null;

            if (mSamplerBinder_Shadow == null)
            {
                mSamplerBinder_Shadow = new CShaderSamplers();
                //var SamplerDesc = new CSamplerStateDesc();
                //SamplerDesc.SetDefault();
                //for (UInt32 i = 0; i < shader.SamplerNumber; i++)
                //{
                //    CSamplerBindInfo info = new CSamplerBindInfo();
                //    shader.GetSamplerBindInfo(i, ref info);
                //    MtlInst.GetSamplerStateDesc(i, ref SamplerDesc);
                //    CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, SamplerDesc);
                //    mSamplerBinder_Shadow.PSBindSampler(info.PSBindPoint, SamplerState);
                //}
                using (var it = MtlInst.Material.GetSamplerStateDescs().GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        var name = it.Current.Key;
                        var samplerDesc = it.Current.Value;
                        CSamplerBindInfo info = new CSamplerBindInfo();
                        if (shader.FindSamplerBindInfoByShaderName(MtlInst, name, ref info))
                        {
                            CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, samplerDesc);
                            mSamplerBinder_Shadow.PSBindSampler(info.PSBindPoint, SamplerState);
                        }
                        else
                        {
                            var defaultSamplerDesc = new CSamplerStateDesc();
                            defaultSamplerDesc.SetDefault();
                            CSamplerState SamplerState = CEngine.Instance.SamplerStateManager.GetSamplerState(RHICtx, defaultSamplerDesc);
                            mSamplerBinder_Shadow.PSBindSampler(info.PSBindPoint, SamplerState);
                        }
                    }
                }
            }

            return mSamplerBinder_Shadow;
        }
        
        public CGfxMtlMesh()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMaterialPrimitive");
        }

        public static Profiler.TimeScope ScopeSetValue2CBuffer = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxMtlMesh), nameof(PassData2ConstBufferPerTick));
        public UInt32 MeshVarVersion = UInt32.MaxValue;
        private void PassData2ConstBufferPerTick(CGfxCamera camera)
        {
            if (CBuffer == null)
            {
                return;
            }

            if (MeshVarVersion != mRootSceneMesh.mMeshVars.Version)
            {
                //mCBuffer.SetValue(mID_ActorId, mRootSceneMesh.mMeshVars.ActorIdColor, 0);
                //mCBuffer.SetValue(mID_HitProxyId, mRootSceneMesh.mMeshVars.HitProxyColor, 0);

                MeshVarVersion = mRootSceneMesh.mMeshVars.Version;
            }
        }

        public static Profiler.TimeScope ScopeUpdateCBuffer = Profiler.TimeScopeManager.GetTimeScope(typeof(CGfxMtlMesh), nameof(UpdatePerMtlMeshCBuffer));
        public void UpdatePerMtlMeshCBuffer(CRenderContext RHICtx, CGfxCamera camera)
        {            
            var refPrebuildPass = GetFirstValidPass();
            if (refPrebuildPass == null || refPrebuildPass.Effect == null)
            {
                return;
            }

            //ScopeUpdateCBuffer.Begin();
            var cbIndex = refPrebuildPass.Effect.CacheData.PerInstanceId;// effect.ShaderProgram.FindCBuffer("cbPerInstance");
            if ((int)cbIndex >= 0)
            {
                PassData2ConstBufferPerTick(camera);
                //MtlInst.SetCBufferVars(mCBuffer);
            }
            //ScopeUpdateCBuffer.End();
        }

        private void PassData2ConstBufferPerTickForShadow(CGfxCamera camera)
        {
            if (CBuffer == null)
            {
                return;
            }
        }

        public void UpdatePerMtlMeshCBufferForShadow(CRenderContext RHICtx, CGfxCamera camera)
        {
            var refPrebuildPass = GetFirstValidPass();
            if (refPrebuildPass == null || refPrebuildPass.Effect == null)
            {
                return;
            }

            var cbIndex = refPrebuildPass.Effect.CacheData.PerInstanceId;// effect.ShaderProgram.FindCBuffer("cbPerInstance");
            if ((int)cbIndex >= 0)
            {
                //else
                //{
                //    //this code below is for what???????? 
                //    var cbDesc = new CConstantBufferDesc();
                //    if (refPrebuildPass.Effect.ShaderProgram.GetCBufferDesc(cbIndex, ref cbDesc))
                //    {
                //        if (mCBuffer.Size != cbDesc.Size)
                //        {
                //            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Material", $"{refPrebuildPass.Effect.Desc.MtlShaderPatch.Name} CBuffer size is invalid, recreate it");
                //            mCBuffer?.Cleanup();
                //            mCBuffer = RHICtx.CreateConstantBuffer(refPrebuildPass.Effect.ShaderProgram, cbIndex);
                //            MeshVarVersion = UInt32.MaxValue;
                //        }
                //    }
                //}

                PassData2ConstBufferPerTickForShadow(camera);

                //MtlInst.SetCBufferVars(mCBuffer);
            }
        }

        public bool Init(CRenderContext RHICtx, CGfxMesh SceneMesh, UInt32 index, CGfxMaterialInstance mtl_inst, CGfxShadingEnv[] EnvShaderArray, bool preUseEffect)
        {
            if (EnvShaderArray == null)
            {
                EnvShaderArray = CEngine.Instance.PrebuildPassData.DefaultShadingEnvs;
            }

            MtlInst = mtl_inst;
            mRootSceneMesh = SceneMesh;
            mAtomIndex = index;

            CGfxEffect[] effects = new CGfxEffect[EnvShaderArray.Length];
            for (int ppi = 0; ppi < (int)PrebuildPassIndex.PPI_Num; ppi++)
            {
                if (EnvShaderArray[ppi] == null)
                    continue;

                var effect = GetEffectAsync(RHICtx, SceneMesh, true, EnvShaderArray[ppi].EnvCode);
                effects[ppi] = effect;
            }

            return InitByEffects(RHICtx, SceneMesh, index, mtl_inst, effects, EnvShaderArray, preUseEffect);
        }
        public bool InitByEffects(CRenderContext RHICtx, CGfxMesh SceneMesh, UInt32 index, CGfxMaterialInstance mtl_inst, CGfxEffect[] effects, CGfxShadingEnv[] EnvShaderArray, bool preUseEffect)
        {
            MtlInst = mtl_inst;
            mRootSceneMesh = SceneMesh;
            mAtomIndex = index;

            if (mPrebuildPassArray == null)
            {
                mPrebuildPassArray = new CPass[(int)PrebuildPassIndex.PPI_Num];
            }

            for (int ppi = 0; ppi < (int)PrebuildPassIndex.PPI_Num; ppi++)
            {
                var effect = effects[ppi];
                if (effect == null)
                {
                    continue;
                }

                CPass refPass = null;
                if (mPrebuildPassArray[ppi] != null)
                {
                    refPass = mPrebuildPassArray[ppi];
                }
                else
                {
                    refPass = RHICtx.CreatePass();
                }

                refPass.InitPass(RHICtx, effect, EnvShaderArray[ppi], this, index);
                refPass.PassIndex = (PrebuildPassIndex)ppi;

                //如果Effect已经加载完成，那么设置Pass状态
                //否则就要等渲染的时候Pass.PreUse来间接触发了
                //
                if(refPass.Effect.IsValid)
                {
                    FillPassData(RHICtx, refPass, effect, (PrebuildPassIndex)ppi, true);
                }
                else
                {
                    if(preUseEffect)
                    {
                        effect.PreUse((successed) =>
                        {
                            if (successed == false)
                            {
                                return;
                            }
                            FillPassData(RHICtx, refPass, effect, (PrebuildPassIndex)ppi);
                        });
                    }
                }
                //这里一旦PreUse了，那就把所有shader都在初始化的时候就加载了
                //effect.PreUse((successed) =>
                //{
                //    if(successed==false)
                //    {
                //        return;
                //    }
                //    FillPassData(RHICtx, refPass, effect, (PrebuildPassIndex)ppi);
                //});

                mPrebuildPassArray[ppi] = refPass;
            }
            
            return true;
        }
        public void FillPassData(CRenderContext RHICtx,
            CPass refPass, 
            CGfxEffect effect, 
            PrebuildPassIndex ppi,
            bool flushVars = false)
        {
            refPass.Effect = effect;
            if (effect.CacheData.PerInstanceId != UInt32.MaxValue)
            {
                ReCreateCBuffer(RHICtx, effect, flushVars);
            }
            refPass.BindCBuffer(effect.ShaderProgram, effect.CacheData.PerInstanceId, CBuffer);
            MtlInst.BindTextures(refPass.ShaderResources, effect.ShaderProgram);
            refPass.BindShaderTextures(this);
            var tempSampler = new CShaderSamplers();
            this.GetSamplerBinder(RHICtx, effect.ShaderProgram, tempSampler);
            refPass.ShaderSamplerBinder = tempSampler;

            var PrebuildPassData = CEngine.Instance.PrebuildPassData;

            if (ppi == PrebuildPassIndex.PPI_HitProxy ||
                ppi == PrebuildPassIndex.PPI_SceneCapture)
            {
                switch (MtlInst.mRenderLayer)
                {
                    case View.ERenderLayer.RL_Opaque:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mOpaqueRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mOpaqueDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_Translucent:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mTranslucentRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mTranslucentDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_CustomOpaque:
                    case View.ERenderLayer.RL_CustomTranslucent:
                    case View.ERenderLayer.RL_Gizmos:
                    case View.ERenderLayer.RL_Sky:
                        {
                            refPass.RenderPipeline.RasterizerState = MtlInst.CustomRasterizerState;
                            refPass.RenderPipeline.DepthStencilState = MtlInst.CustomDepthStencilState;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                }
            }
            else if (ppi == PrebuildPassIndex.PPI_Snapshot)
            {
                switch (MtlInst.mRenderLayer)
                {
                    case View.ERenderLayer.RL_Opaque:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mOpaqueRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mOpaqueDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_Translucent:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mTranslucentRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mTranslucentDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mSnapshotBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_CustomOpaque:
                    case View.ERenderLayer.RL_CustomTranslucent:
                    case View.ERenderLayer.RL_Gizmos:
                    case View.ERenderLayer.RL_Sky:
                        {
                            refPass.RenderPipeline.RasterizerState = MtlInst.CustomRasterizerState;
                            refPass.RenderPipeline.DepthStencilState = MtlInst.CustomDepthStencilState;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mSnapshotBlendStat;
                        }
                        break;
                }
            }
            else if (ppi == PrebuildPassIndex.PPI_PickedEditor)
            {
                switch (MtlInst.mRenderLayer)
                {
                    case View.ERenderLayer.RL_Opaque:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mOpaqueRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mOpaqueDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_Translucent:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mTranslucentRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mTranslucentDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_CustomOpaque:
                    case View.ERenderLayer.RL_CustomTranslucent:
                    case View.ERenderLayer.RL_Gizmos:
                    case View.ERenderLayer.RL_Sky:
                        {
                            refPass.RenderPipeline.RasterizerState = MtlInst.CustomRasterizerState;
                            refPass.RenderPipeline.DepthStencilState = MtlInst.CustomDepthStencilState;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                }
            }
            else if(ppi == PrebuildPassIndex.PPI_SSM)
            {
                refPass.RenderPipeline.RasterizerState = MtlInst.mShadowRasterState;
                refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mOpaqueDSStat;
                refPass.RenderPipeline.BlendState = PrebuildPassData.mShadowBlendStat;
            }
            else
            {
                switch (MtlInst.mRenderLayer)
                {
                    case View.ERenderLayer.RL_Shadow:
                        {
                            refPass.RenderPipeline.RasterizerState = MtlInst.mShadowRasterState;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mOpaqueDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mShadowBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_Opaque:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mOpaqueRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mOpaqueDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_Translucent:
                        {
                            refPass.RenderPipeline.RasterizerState = PrebuildPassData.mTranslucentRasterStat;
                            refPass.RenderPipeline.DepthStencilState = PrebuildPassData.mTranslucentDSStat;
                            refPass.RenderPipeline.BlendState = PrebuildPassData.mTranslucentBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_CustomOpaque:
                    case View.ERenderLayer.RL_CustomTranslucent:
                        {
                            refPass.RenderPipeline.RasterizerState = MtlInst.CustomRasterizerState;
                            refPass.RenderPipeline.DepthStencilState = MtlInst.CustomDepthStencilState;
                            refPass.RenderPipeline.BlendState = MtlInst.CustomBlendState;// PrebuildPassData.mOpaqueBlendStat;
                        }
                        break;
                    case View.ERenderLayer.RL_Gizmos:
                    case View.ERenderLayer.RL_Sky:
                        {
                            refPass.RenderPipeline.RasterizerState = MtlInst.CustomRasterizerState;
                            refPass.RenderPipeline.DepthStencilState = MtlInst.CustomDepthStencilState;
                            refPass.RenderPipeline.BlendState = MtlInst.CustomBlendState;
                        }
                        break;
                }
            }
        }
        internal CGfxEffectDesc GetEffectDesc(CGfxMesh mesh, GfxEnvShaderCode ShaderEnv)
        {
            var result = CGfxEffectDesc.CreateDesc(MtlInst.Material, mesh.MdfQueue, ShaderEnv);//, mShaderDefinition);
            return result;
        }
        public CGfxEffect GetEffectAsync(CRenderContext RHICtx, CGfxMesh mesh, bool tryLoad, GfxEnvShaderCode ShaderEnv)
        {
            var mEffectDesc = GetEffectDesc(mesh, ShaderEnv);

            return CEngine.Instance.EffectManager.GetEffect(RHICtx, mEffectDesc);
        }
        public CGfxEffect TryGetEffect(CRenderContext RHICtx, CGfxMesh mesh, bool tryLoad, GfxEnvShaderCode ShaderEnv)
        {
            var result = CGfxEffectDesc.CreateDesc(MtlInst.Material, mesh.MdfQueue, ShaderEnv);//, mShaderDefinition);
            var hash64 = result.GetHash64();
            return CEngine.Instance.EffectManager.TryGetEffect(ref hash64);
        }
        private void UpdateMaterialCBuffer(CRenderContext rc, CGfxEffect effect)
        {
            var cbIndex = effect.CacheData.PerInstanceId;// effect.ShaderProgram.FindCBuffer("cbPerInstance");
            if ((int)cbIndex >= 0)
            {
                PassData2ConstBufferPerTick(null);

                MtlInst.SetCBufferVars(CBuffer);
            }
        }
        protected CGfxMaterialInstance mMtlInst;
        
        [ReadOnly(true)]
        public CGfxMaterialInstance MtlInst
        {
            get { return mMtlInst; }
            protected set
            {
                mMtlInst = value;
                if (value != null)
                {
                    SDK_GfxMaterialPrimitive_SetMaterial(CoreObject, value.CoreObject);
                }
            }
        }
        private CConstantBuffer mCBuffer;
        [Browsable(false)]
        public CConstantBuffer CBuffer
        {
            get
            {
                return mCBuffer;
            }
        }
        public void ReCreateCBuffer(CRenderContext rc, CGfxEffect effect, bool FlushVars)
        {
            if (effect.CacheData.PerInstanceId != UInt32.MaxValue)
            {
                bool recreateCB = false;
                if (mCBuffer != null)
                {
                    var desc = new CConstantBufferDesc();
                    effect.ShaderProgram.GetCBufferDesc(effect.CacheData.PerInstanceId, ref desc);
                    if(mCBuffer.Size != desc.Size)
                    {
                        recreateCB = true;
                    }
                    else
                    {
                        recreateCB = !mCBuffer.IsSameVars(effect.ShaderProgram, effect.CacheData.PerInstanceId);
                    }
                }
                else
                {
                    recreateCB = true;
                }
                if (recreateCB)
                {
                    mCBuffer = rc.CreateConstantBuffer(effect.ShaderProgram, effect.CacheData.PerInstanceId);
                    MtlInst.SetCBufferVars(CBuffer);
                }
                else if (FlushVars)
                {
                    MtlInst.SetCBufferVars(CBuffer);
                }
            }
        }
        [Browsable(false)]
        public Dictionary<UInt32, CShaderResourceView> Textures
        {
            get;
        } = new Dictionary<uint, CShaderResourceView>();
        public void SetTexutre(UInt32 bindPoint, CShaderResourceView texture)
        {
            Textures[bindPoint] = texture;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMaterialPrimitive_SetMaterial(NativePointer self, CGfxMaterialInstance.NativePointer material);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxMaterialInstance.NativePointer SDK_GfxMaterialPrimitive_GetMaterial(NativePointer self);
        #endregion
    }
}
