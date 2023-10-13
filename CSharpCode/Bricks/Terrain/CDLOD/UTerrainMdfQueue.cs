using EngineNS.Bricks.Terrain.Grass;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainMdfQueue : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UPatch Patch
        {
            get
            {
                return this.MdfDatas as UPatch;
            }
        }
        public int Dimension = 64;
        public bool IsWater = false;
        public UTerrainMdfQueue()
        {
            UpdateShaderCode();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_LightMap,
            };
        }
        public override void CopyFrom(UMdfQueue mdf)
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
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine("#ifndef _UTerrainMdfQueue_CDLOD_INC_", ref sourceCode);
            codeBuilder.AddLine("#define _UTerrainMdfQueue_CDLOD_INC_", ref sourceCode);
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/Bricks/Terrain/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address}\"", ref sourceCode);

            codeBuilder.AddLine("#endif", ref sourceCode);
            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = sourceCode;
        }
        public class UMdfShaderBinder : Graphics.Pipeline.UCoreShaderBinder.UShaderResourceIndexer
        {
            public void Init(NxRHI.UShaderEffect effect)
            {
                UpdateBindResouce(effect);
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
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh, int atom)
        {
            using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
            {
                base.OnDrawCall(cmd, shadingType, drawcall, policy, mesh, atom);

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
                //var index = shaderProg.FindBinder("HeightMapTexture");
                if (effectBinder.HeightMapTexture != null)
                {
                    if (IsWater)
                        drawcall.BindSRV(effectBinder.HeightMapTexture.mCoreObject, pat.Level.WaterHMapSRV);
                    else
                        drawcall.BindSRV(effectBinder.HeightMapTexture.mCoreObject, pat.Level.HeightMapSRV);
                }
                //index = shaderProg.FindBinder("Samp_HeightMapTexture");
                if (effectBinder.Samp_HeightMapTexture != null)
                    drawcall.BindSampler(effectBinder.Samp_HeightMapTexture.mCoreObject, policy.ClampState);// UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

                //index = shaderProg.FindBinder("HeightMapTextureArray");
                if (effectBinder.HeightMapTextureArray != null)
                    drawcall.BindSRV(effectBinder.HeightMapTextureArray.mCoreObject, pat.Level.GetTerrainNode().RVTextureArray.TexArraySRV);

                //index = shaderProg.FindBinder("ArrayTextures[1]");
                //if (index.IsValidPointer)
                //    drawcall.BindSRV(index, pat.Level.HeightMapSRV);

                //index = shaderProg.FindBinder("NormalMapTexture");
                if (effectBinder.NormalMapTexture != null)
                    drawcall.BindSRV(effectBinder.NormalMapTexture.mCoreObject, pat.Level.NormalMapSRV);
                //index = shaderProg.FindBinder("Samp_NormalMapTexture");
                if (effectBinder.Samp_NormalMapTexture != null)
                    drawcall.BindSampler(effectBinder.Samp_NormalMapTexture.mCoreObject, policy.ClampState);// UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

                //index = shaderProg.FindBinder("MaterialIdTexture");
                if (effectBinder.MaterialIdTexture != null)
                    drawcall.BindSRV(effectBinder.MaterialIdTexture.mCoreObject, pat.Level.MaterialIdMapSRV);
                //index = shaderProg.FindBinder("Samp_MaterialIdTexture");
                if (effectBinder.Samp_MaterialIdTexture != null)
                    drawcall.BindSampler(effectBinder.Samp_MaterialIdTexture.mCoreObject, policy.ClampPointState);

                //var index = shaderProg.FindBinder("DiffuseTextureArray");
                if (effectBinder.DiffuseTextureArray != null)
                {
                    var srv = pat.Level.GetTerrainNode().TerrainMaterialIdManager.DiffuseTextureArraySRV;
                    if (srv != null)
                        drawcall.BindSRV(effectBinder.DiffuseTextureArray.mCoreObject, srv);
                }
                //index = shaderProg.FindBinder("Samp_DiffuseTextureArray");
                if (effectBinder.Samp_DiffuseTextureArray != null)
                    drawcall.BindSampler(effectBinder.Samp_DiffuseTextureArray.mCoreObject, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                //var index = shaderProg.FindBinder("NormalTextureArray");
                if (effectBinder.NormalTextureArray != null)
                {
                    var srv = pat.Level.GetTerrainNode().TerrainMaterialIdManager.NormalTextureArraySRV;
                    if (srv != null)
                        drawcall.BindSRV(effectBinder.NormalTextureArray.mCoreObject, srv);
                }
                //index = shaderProg.FindBinder("Samp_NormalTextureArray");
                if (effectBinder.Samp_NormalTextureArray != null)
                    drawcall.BindSampler(effectBinder.Samp_NormalTextureArray.mCoreObject, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                if (effectBinder.cbPerPatch != null)
                {
                    //var cbIndex = shaderProg.FindBinder("cbPerPatch");
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

                    drawcall.BindCBuffer(effectBinder.cbPerPatch.mCoreObject, pat.PatchCBuffer);
                }
                //var cbIndex = shaderProg.FindBinder("cbPerTerrain");
                if (effectBinder.cbPerTerrain != null)
                {
                    drawcall.BindCBuffer(effectBinder.cbPerTerrain.mCoreObject, pat.Level.Level.Node.TerrainCBuffer);
                }
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
        public override Rtti.UTypeDesc GetPermutation(List<string> features)
        {
            if (features.Contains("UMdf_NoShadow"))
            {
                return Rtti.UTypeDescGetter<UTerrainMdfQueuePermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc;
            }
            else
            {
                return Rtti.UTypeDescGetter<UTerrainMdfQueuePermutation<Graphics.Pipeline.Shader.UMdf_Shadow>>.TypeDesc;
            }
        }
    }

    public class UTerrainMdfQueuePermutation<PermutationType> : UTerrainMdfQueue
    {
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine("#ifndef _UTerrainMdfQueue_CDLOD_INC_", ref sourceCode);
            codeBuilder.AddLine("#define _UTerrainMdfQueue_CDLOD_INC_", ref sourceCode);
            codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/Bricks/Terrain/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address}\"", ref sourceCode);

            codeBuilder.AddLine("#endif", ref sourceCode);

            if (typeof(PermutationType).Name == "UMdf_NoShadow")
            {
                codeBuilder.AddLine("#define DISABLE_SHADOW_MDFQUEUE 1", ref sourceCode);
            }
            else if (typeof(PermutationType).Name == "UMdf_Shadow")
            {
                
            }

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = sourceCode;
        }
    }
}
