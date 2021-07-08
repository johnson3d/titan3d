using EngineNS.Graphics.Pipeline;
using EngineNS.RHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class UMdfTerrainMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UMdfTerrainMesh()
        {
            UpdateShaderCode();
        }
        public float GridSize = 1.0f;
        public float HeightStep = 0.01f;
        public RHI.CShaderResourceView HeightMapRSV { get; set; }
        public RHI.CConstantBuffer TerrainCBuffer;
        private static string CodeString;
        public override Hash160 GetHash()
        {
            if (CodeString == null)
            {
                CodeString = IO.FileManager.ReadAllText(RName.GetRName("shaders/shadingenv/mobile/heightmap.cginc", RName.ERNameType.Engine).Address);
            }
            mMdfQueueHash = Hash160.CreateHash160(CodeString);
            return mMdfQueueHash;
        }
        protected override void UpdateShaderCode()
        {
            {
                var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

                codeBuilder.AddLine("#ifndef _UMdfTerrainMesh_INC_");
                codeBuilder.AddLine("#define _UMdfTerrainMesh_INC_");
                codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/shadingenv/mobile/heightmap.cginc", RName.ERNameType.Engine).Address}\"");

                //codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
                //codeBuilder.PushBrackets();
                //{
                //    codeBuilder.AddLine("DoTerrainModifierVS(output, input);");
                //}
                //codeBuilder.PopBrackets();

                //codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");

                codeBuilder.AddLine("#endif");
                SourceCode = new IO.CMemStreamWriter();
                SourceCode.SetText(codeBuilder.ClassCode);
            }
        }
        public unsafe override void OnDrawCall(IRenderPolicy.EShadingType shadingType, CDrawCall drawcall, IRenderPolicy policy, UMesh mesh)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh);
            if (drawcall.TagObject == null)
            {
                //这里可以缓存Effec里面各种VarIndex，不过这里只有两个需要每次都查找，而地形对象又不会太多，那就别缓存了
            }
            var shaderProg = drawcall.Effect.ShaderProgram;
            var index = shaderProg.mCoreObject.GetTextureBindSlotIndex("HeightMapTexture");
            if (index != uint.MaxValue)
                drawcall.mCoreObject.BindSRVAll(index, HeightMapRSV.mCoreObject);

            var cbIndex = shaderProg.mCoreObject.FindCBuffer("cbPerTerrain");
            if (cbIndex != uint.MaxValue)
            {
                if (TerrainCBuffer == null)
                {
                    TerrainCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(shaderProg, cbIndex);

                    var varIndex = TerrainCBuffer.mCoreObject.FindVar("GridSize");
                    TerrainCBuffer.SetValue(varIndex, ref GridSize);

                    varIndex = TerrainCBuffer.mCoreObject.FindVar("HeightStep");
                    TerrainCBuffer.SetValue(varIndex, ref HeightStep);

                    varIndex = TerrainCBuffer.mCoreObject.FindVar("UVStep");
                    Vector2 uvStep = new Vector2(1.0f / 128.0f);
                    TerrainCBuffer.SetValue(varIndex, ref uvStep);

                    varIndex = TerrainCBuffer.mCoreObject.FindVar("StartPosition");
                    Vector3 startPosition = new Vector3(0);
                    TerrainCBuffer.SetValue(varIndex, ref startPosition);
                }
                drawcall.mCoreObject.BindCBufferAll(cbIndex, TerrainCBuffer.mCoreObject);
            }
        }

        public override void CopyFrom(Pipeline.Shader.UMdfQueue mdf)
        {
            var curMdf = mdf as UMdfTerrainMesh;
            if (curMdf != null)
            {
                GridSize = curMdf.GridSize;
                HeightStep = curMdf.HeightStep;
                HeightMapRSV = curMdf.HeightMapRSV;
                TerrainCBuffer = curMdf.TerrainCBuffer;
            }
        }
    }
    public class UMdfTerrainMesh_NoShadow : UMdfTerrainMesh
    {
        protected override void UpdateShaderCode()
        {
            {
                var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

                codeBuilder.AddLine("#ifndef _UMdfTerrainMesh_INC_");
                codeBuilder.AddLine("#define _UMdfTerrainMesh_INC_");
                codeBuilder.AddLine($"#include \"{RName.GetRName("shaders/shadingenv/mobile/heightmap.cginc", RName.ERNameType.Engine).Address}\"");

                //codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
                //codeBuilder.PushBrackets();
                //{
                //    codeBuilder.AddLine("DoTerrainModifierVS(output, input);");
                //}
                //codeBuilder.PopBrackets();

                //codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");

                codeBuilder.AddLine("#endif");
                codeBuilder.AddLine("#undef ENV_DISABLE_SHADOW");
                codeBuilder.AddLine("#define ENV_DISABLE_SHADOW 1");

                SourceCode = new IO.CMemStreamWriter();
                SourceCode.SetText(codeBuilder.ClassCode);
            }
        }
    }
}
