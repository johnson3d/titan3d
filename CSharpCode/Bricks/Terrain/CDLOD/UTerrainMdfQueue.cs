using EngineNS.Bricks.Terrain.Grass;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class TtTerrainModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public void Dispose()
        {

        }
        public string ModifierNameVS { get => "DoTerrainModifierVS"; }
        public string ModifierNamePS { get => null; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/Bricks/Terrain/TerrainCDLOD.cginc", RName.ERNameType.Engine);
            }
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_LightMap,
            };
        }
        public void Initialize(Graphics.Mesh.UMaterialMesh materialMesh)
        {

        }
    }
    public class UTerrainMdfQueue : Graphics.Pipeline.Shader.TtMdfQueue1<TtTerrainModifier>
    {
        public TtPatch Patch
        {
            get
            {
                return this.MdfDatas as TtPatch;
            }
        }
        public int Dimension = 64;
        public bool IsWater = false;
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            Dimension = (mdf as UTerrainMdfQueue).Dimension;
            IsWater = (mdf as UTerrainMdfQueue).IsWater;
        }
        public override Hash160 GetHash()
        {
            string CodeString = IO.TtFileManager.ReadAllText(RName.GetRName("shaders/Bricks/Terrain/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address);
            mMdfQueueHash = Hash160.CreateHash160(CodeString);
            return mMdfQueueHash;
        }
        public class UMdfShaderBinder : Graphics.Pipeline.UCoreShaderBinder.UShaderResourceIndexer
        {
            public void Init(NxRHI.UShaderEffect effect)
            {
                UpdateBindResouce(effect);
                TextureSlotBuffer = effect.FindBinder("TextureSlotBuffer");
                HeightMapTexture = effect.FindBinder("HeightMapTexture");
                Samp_HeightMapTexture = effect.FindBinder("Samp_HeightMapTexture");
                HeightMapTextureArray = effect.FindBinder("HeightMapTextureArray");
                NormalMapTexture = effect.FindBinder("NormalMapTexture");
                Samp_NormalMapTexture = effect.FindBinder("Samp_NormalMapTexture");
                MaterialIdTexture = effect.FindBinder("MaterialIdTexture");
                Samp_MaterialIdTexture = effect.FindBinder("Samp_MaterialIdTexture");
                DiffuseTextureArray = effect.FindBinder("DiffuseTextureArray");
                Samp_DiffuseTextureArray = effect.FindBinder("Samp_DiffuseTextureArray");
                NormalTextureArray = effect.FindBinder("NormalTextureArray");
                Samp_NormalTextureArray = effect.FindBinder("Samp_NormalTextureArray");
                cbPerPatch = effect.FindBinder("cbPerPatch");
                cbPerTerrain = effect.FindBinder("cbPerTerrain");
            }
            public NxRHI.UEffectBinder TextureSlotBuffer;
            public NxRHI.UEffectBinder HeightMapTexture;
            public NxRHI.UEffectBinder Samp_HeightMapTexture;
            public NxRHI.UEffectBinder HeightMapTextureArray;
            public NxRHI.UEffectBinder NormalMapTexture;
            public NxRHI.UEffectBinder Samp_NormalMapTexture;
            public NxRHI.UEffectBinder MaterialIdTexture;
            public NxRHI.UEffectBinder Samp_MaterialIdTexture;
            public NxRHI.UEffectBinder DiffuseTextureArray;
            public NxRHI.UEffectBinder Samp_DiffuseTextureArray;
            public NxRHI.UEffectBinder NormalTextureArray;
            public NxRHI.UEffectBinder Samp_NormalTextureArray;
            public NxRHI.UEffectBinder cbPerPatch;
            public NxRHI.UEffectBinder cbPerTerrain;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeOnDrawCall = Profiler.TimeScopeManager.GetTimeScope(typeof(UTerrainMdfQueue), nameof(OnDrawCall));
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            bool EnableTerrainMeshBatch = UEngine.Instance.Config.Feature_UseRVT;
            //var cullingNode = policy.FindFirstNode<Graphics.Pipeline.TtCullingNode>();
            //if (cullingNode != null)
            //{
            //    EnableTerrainMeshBatch = cullingNode.EnableTerrainMeshBatch;
            //    EnableTerrainMeshBatch = false;
            //}
            using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
            {
                base.OnDrawCall(cmd, shadingType, drawcall, policy, atom);

                var effectBinder = drawcall.Effect.mBindIndexer as UMdfShaderBinder;
                if (effectBinder == null)
                {
                    effectBinder = new UMdfShaderBinder();
                    effectBinder.Init(drawcall.Effect.ShaderEffect);
                    drawcall.Effect.mBindIndexer = effectBinder;
                }

                var pat = Patch;

                SureCBuffer(drawcall.mCoreObject.GetGraphicsEffect());

                var shaderProg = drawcall.mCoreObject.GetGraphicsEffect();

                if (EnableTerrainMeshBatch)
                {
                    var slotBufferSRV = pat.TerrainNode.Terrain.HeightmapRVT.TextureSlotBuffer.Srv;
                    drawcall.BindSRV(effectBinder.HeightMapTexture, slotBufferSRV);
                }

                if (IsWater)
                {
                    drawcall.BindSRV(effectBinder.HeightMapTexture, pat.Level.WaterHMapSRV);
                }
                else
                {
                    if (EnableTerrainMeshBatch)
                    {
                        var texture = pat.TerrainNode.Terrain.HeightmapRVT.TextureSlotAllocator.TextureArraySRV;
                        drawcall.BindSRV(effectBinder.HeightMapTexture, texture);
                    }
                    else
                    {
                        drawcall.BindSRV(effectBinder.HeightMapTexture, pat.Level.HeightMapSRV);
                    }
                }
                drawcall.BindSampler(effectBinder.Samp_HeightMapTexture, policy.ClampState);// UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

                //drawcall.BindSRV(effectBinder.HeightMapTextureArray, pat.Level.GetTerrainNode().RVTextureArray?.TexArraySRV);

                if (/*true || */EnableTerrainMeshBatch)
                {
                    pat.TerrainNode.Terrain.NormalmapRVT.ActiveRVT(pat.Level.NormalMapSRV);
                    var texture = pat.TerrainNode.Terrain.NormalmapRVT.TextureSlotAllocator.TextureArraySRV;
                    drawcall.BindSRV(effectBinder.NormalMapTexture, texture);
                }
                else
                {
                    drawcall.BindSRV(effectBinder.NormalMapTexture, pat.Level.NormalMapSRV);
                }
                
                drawcall.BindSampler(effectBinder.Samp_NormalMapTexture, policy.ClampState);// UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

                drawcall.BindSRV(effectBinder.MaterialIdTexture, pat.Level.MaterialIdMapSRV);
                drawcall.BindSampler(effectBinder.Samp_MaterialIdTexture, policy.ClampPointState);

                {
                    var srv = pat.Level.GetTerrainNode().TerrainMaterialIdManager.DiffuseTextureArraySRV;
                    drawcall.BindSRV(effectBinder.DiffuseTextureArray, srv);
                    drawcall.BindSampler(effectBinder.Samp_DiffuseTextureArray, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
                }

                {
                    var srv = pat.Level.GetTerrainNode().TerrainMaterialIdManager.NormalTextureArraySRV;
                    drawcall.BindSRV(effectBinder.NormalTextureArray, srv);
                    drawcall.BindSampler(effectBinder.Samp_NormalTextureArray, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
                }

                if (effectBinder.cbPerPatch != null)
                {
                    var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
                    pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.StartPosition, in pat.StartPosition);

                    pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.CurrentLOD, pat.CurrentLOD);

                    var terrain = pat.Level.GetTerrainNode();
                    pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.EyeCenter, terrain.EyeLocalCenter - pat.StartPosition);

                    //pat.TexUVOffset.X = (Patch.XInLevel * 64.0f) / 1024.0f;
                    //pat.TexUVOffset.Y = (Patch.ZInLevel * 64.0f) / 1024.0f;

                    //pat.TexUVOffset.X = (Patch.XInLevel * pat.Level.GetTerrainNode().PatchSize) / pat.Level.GetTerrainNode().LevelSize;
                    //pat.TexUVOffset.Y = (Patch.ZInLevel * pat.Level.GetTerrainNode().PatchSize) / pat.Level.GetTerrainNode().LevelSize;

                    pat.TexUVOffset.X = ((float)Patch.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                    pat.TexUVOffset.Y = ((float)Patch.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);

                    pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.TexUVOffset, in pat.TexUVOffset);

                    drawcall.BindCBuffer(effectBinder.cbPerPatch, pat.PatchCBuffer);
                }
                drawcall.BindCBuffer(effectBinder.cbPerTerrain, pat.Level.Level.Node.TerrainCBuffer);
            }   
        }
        private void SureCBuffer(NxRHI.IGraphicsEffect shaderProg)
        {
            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            var pat = Patch;
            if (pat.PatchCBuffer == null)
            {
                coreBinder.CBPerTerrainPatch.UpdateFieldVar(shaderProg, "cbPerPatch");
                pat.PatchCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerTerrainPatch.Binder.mCoreObject);
            }
            if (pat.Level.Level.Node.TerrainCBuffer == null)
            {
                coreBinder.CBPerTerrain.UpdateFieldVar(shaderProg, "cbPerTerrain");
                pat.Level.Level.Node.TerrainCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerTerrain.Binder.mCoreObject);
            }
        }
    }
}
