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
        public override void CopyFrom(UMdfQueue mdf)
        {
            Dimension = (mdf as UTerrainMdfQueue).Dimension;
            IsWater = (mdf as UTerrainMdfQueue).IsWater;
        }
        public override Hash160 GetHash()
        {
            string CodeString = IO.FileManager.ReadAllText(RName.GetRName("shaders/Bricks/Terrain/TerrainCDLOD.cginc", RName.ERNameType.Engine).Address);
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
        public unsafe override void OnDrawCall(Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);
            
            var pat = Patch;
            
            SureCBuffer(drawcall.mCoreObject.GetShaderEffect());

            var shaderProg = drawcall.mCoreObject.GetShaderEffect();
            var index = shaderProg.FindBinder("HeightMapTexture");
            if (index.IsValidPointer)
            {
                if (IsWater)
                    drawcall.BindSRV(index, pat.Level.WaterHMapSRV);
                else
                    drawcall.BindSRV(index, pat.Level.HeightMapSRV);
            }
            index = shaderProg.FindBinder("Samp_HeightMapTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, policy.ClampState);// UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = shaderProg.FindBinder("HeightMapTextureArray");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, pat.Level.GetTerrainNode().RVTextureArray.TexArraySRV);

            index = shaderProg.FindBinder("ArrayTextures[1]");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, pat.Level.HeightMapSRV);

            index = shaderProg.FindBinder("NormalMapTexture");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, pat.Level.NormalMapSRV);
            index = shaderProg.FindBinder("Samp_NormalMapTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, policy.ClampState);// UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);

            index = shaderProg.FindBinder("MaterialIdTexture");
            if (index.IsValidPointer)
                drawcall.BindSRV(index, pat.Level.MaterialIdMapSRV);
            index = shaderProg.FindBinder("Samp_MaterialIdTexture");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, policy.ClampPointState);

            index = shaderProg.FindBinder("DiffuseTextureArray");
            if (index.IsValidPointer)
            {
                var srv = pat.Level.GetTerrainNode().TerrainMaterialIdManager.DiffuseTextureArraySRV;
                if (srv != null)
                    drawcall.BindSRV(index, srv);
            }
            index = shaderProg.FindBinder("Samp_DiffuseTextureArray");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = shaderProg.FindBinder("NormalTextureArray");
            if (index.IsValidPointer)
            {
                var srv = pat.Level.GetTerrainNode().TerrainMaterialIdManager.NormalTextureArraySRV;
                if (srv != null)
                    drawcall.BindSRV(index, srv);
            }
            index = shaderProg.FindBinder("Samp_NormalTextureArray");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, UEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            var cbIndex = shaderProg.FindBinder("cbPerPatch");
            if (cbIndex.IsValidPointer)
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

                drawcall.BindCBuffer(cbIndex, pat.PatchCBuffer);
            }
            cbIndex = shaderProg.FindBinder("cbPerTerrain");
            if (cbIndex.IsValidPointer)
            {
                drawcall.BindCBuffer(cbIndex, pat.Level.Level.Node.TerrainCBuffer);
            }
        }
        private void SureCBuffer(NxRHI.IShaderEffect shaderProg)
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
