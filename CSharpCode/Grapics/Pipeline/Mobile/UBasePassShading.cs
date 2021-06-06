using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UBasePassShading : Shader.UShadingEnv
    {
        public UBasePassShading()
        {
            var disable_AO = new MacroDefine();//0
            disable_AO.Name = "ENV_DISABLE_AO";
            disable_AO.Values.Add("0");
            disable_AO.Values.Add("1");
            MacroDefines.Add(disable_AO);

            var disable_PointLights = new MacroDefine();//1
            disable_PointLights.Name = "ENV_DISABLE_POINTLIGHTS";
            disable_PointLights.Values.Add("0");
            disable_PointLights.Values.Add("1");
            MacroDefines.Add(disable_PointLights);

            var disable_Shadow = new MacroDefine();//2
            disable_Shadow.Name = "ENV_DISABLE_SHADOW";
            disable_Shadow.Values.Add("0");
            disable_Shadow.Values.Add("1");
            MacroDefines.Add(disable_Shadow);

            var mode_editor = new MacroDefine();//3
            mode_editor.Name = "MODE_EDITOR";
            mode_editor.Values.Add("0");
            mode_editor.Values.Add("1");
            MacroDefines.Add(mode_editor);

            UpdatePermutationBitMask();

            mMacroValues.Add("0");//disable_AO = 0
            mMacroValues.Add("0");//disalbe_PointLights = 0
            mMacroValues.Add("0");//disalbe_Shadow = 0
            mMacroValues.Add("1");//mode_editor = 0
            
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableAO(bool value)
        {
            mMacroValues[0] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisablePointLights(bool value)
        {
            mMacroValues[1] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        public void SetDisableShadow(bool value)
        {
            mMacroValues[2] = value ? "1" : "0";
            UpdatePermutation(mMacroValues);
        }
        List<string> mMacroValues = new List<string>();
        public RHI.CConstantBuffer PerShadingCBuffer;
        public override bool IsValidPermutation(Shader.UMaterial mtl, uint permutation)
        {
            var ao_value = GetDefineValue(permutation, "ENV_DISABLE_AO");
            var pointlights_value = GetDefineValue(permutation, "ENV_DISABLE_POINTLIGHTS");
            if (mtl.LightingMode != Shader.UMaterial.ELightingMode.Stand)
            {
                if(ao_value == "1" || pointlights_value == "1")
                    return false;
            }
            return true;
        }
        public class VarIndexer : RHI.CShaderProgram.IShaderVarIndexer
        {
            [RHI.CShaderProgram.ShaderVar(VarType = "CBuffer")]
            public uint cbPerShadingEnv;
            [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
            public uint gEnvMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
            public uint gShadowMap;
            [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
            public int gEnvMapMaxMipLevel;
            [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
            public int gEyeEnvMapMaxMipLevel;
        }

        protected virtual VarIndexer GetVarIndexer(RHI.CDrawCall drawcall)
        {
            VarIndexer indexer = (VarIndexer)drawcall.Effect.TagObject;
            if (drawcall.Effect.TagObject == null)
            {
                indexer = new VarIndexer();
                indexer.UpdateIndex(drawcall.Effect.ShaderProgram);
                drawcall.Effect.TagObject = indexer;
            }
            return indexer;
        }
        public unsafe override void OnBuildDrawCall(RHI.CDrawCall drawcall)
        {
            var indexer = GetVarIndexer(drawcall);
            if (indexer.cbPerShadingEnv != 0xFFFFFFFF && PerShadingCBuffer == null)
            {
                PerShadingCBuffer = UEngine.Instance.GfxDevice.RenderContext.CreateConstantBuffer(drawcall.Effect.ShaderProgram, indexer.cbPerShadingEnv);
                float EnvMapMaxMipLevel = 1.0f;
                PerShadingCBuffer.SetValue(indexer.gEnvMapMaxMipLevel, ref EnvMapMaxMipLevel);
                float EyeEnvMapMaxMipLevel = 1.0f;
                PerShadingCBuffer.SetValue(indexer.gEyeEnvMapMaxMipLevel, ref EyeEnvMapMaxMipLevel);
            }
            if (PerShadingCBuffer != null)
                drawcall.mCoreObject.BindCBufferAll(indexer.cbPerShadingEnv, PerShadingCBuffer.mCoreObject);
        }
        public unsafe virtual void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, UMobileFSPolicy policy, Mesh.UMesh mesh)
        {
            var indexer = GetVarIndexer(drawcall);

            drawcall.mCoreObject.BindSRVAll(indexer.gEnvMap, policy.EnvMapSRV.mCoreObject);

            if (PerShadingCBuffer != null)
            {
                float gEnvMapMaxMipLevel = policy.EnvMapSRV.PicDesc.MipLevel - 1;
                PerShadingCBuffer.SetValue(indexer.gEnvMapMaxMipLevel, ref gEnvMapMaxMipLevel);
            }
        }
    }
    public class UBasePassOpaque : UBasePassShading
    {
        public UBasePassOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/MobileOpaque.cginc", RName.ERNameType.Engine);
        }
    }

    //public class UBasePassTerrain : UBasePassShading
    //{
    //    public UBasePassTerrain()
    //    {
    //        CodeName = RName.GetRName("shaders/ShadingEnv/Mobile/heightmap.cginc", RName.ERNameType.Engine);
    //    }
    //    public class UTerrainAttachment : Mesh.UMesh.UMeshAttachment
    //    {
    //        public override UShadingEnv GetPassShading(IRenderPolicy.EShadingType type, UMesh mesh)
    //        {
    //            switch (type)
    //            {
    //                case Pipeline.IRenderPolicy.EShadingType.BasePass:
    //                    return UEngine.Instance.ShadingEnvManager.GetShadingEnv<Pipeline.Mobile.UBasePassTerrain>();
    //            }
    //            return null;
    //        }
    //        public RHI.CShaderResourceView HeightMapRSV { get; set; }
    //    }
    //    public class TerrainVarIndexer : UBasePassShading.VarIndexer
    //    {
    //        [RHI.CShaderProgram.ShaderVar(VarType = "Texture")]
    //        public uint HeightMapTexture;
    //        [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
    //        public int StartPosition;
    //        [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
    //        public int GridSize;
    //        [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
    //        public int HeightStep;
    //        [RHI.CShaderProgram.ShaderVar(VarType = "Var", CBuffer = "cbPerShadingEnv")]
    //        public int UVStep;
    //    }
    //    protected override VarIndexer GetVarIndexer(RHI.CDrawCall drawcall)
    //    {
    //        VarIndexer indexer = (VarIndexer)drawcall.Effect.TagObject;
    //        if (drawcall.Effect.TagObject == null)
    //        {
    //            indexer = new TerrainVarIndexer();
    //            indexer.UpdateIndex(drawcall.Effect.ShaderProgram);
    //            drawcall.Effect.TagObject = indexer;
    //        }
    //        return indexer;
    //    }
    //    public static float GridSize = 1.0f;
    //    public static float HeightStep = 0.01f;
    //    public override void OnBuildDrawCall(RHI.CDrawCall drawcall)
    //    {
    //        base.OnBuildDrawCall(drawcall);
    //        var indexer = GetVarIndexer(drawcall) as TerrainVarIndexer;
    //        if (indexer.cbPerShadingEnv != 0xFFFFFFFF && PerShadingCBuffer != null)
    //        {
    //            PerShadingCBuffer.SetValue(indexer.GridSize, ref GridSize);
    //            PerShadingCBuffer.SetValue(indexer.HeightStep, ref HeightStep);
    //            Vector2 uvStep = new Vector2(1.0f / 128.0f); 
    //            PerShadingCBuffer.SetValue(indexer.UVStep, ref uvStep);
    //            Vector3 startPosition = new Vector3(0);
    //            PerShadingCBuffer.SetValue(indexer.StartPosition, ref startPosition);
    //        }
    //    }
    //    public unsafe override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, UMobileFSPolicy policy, Mesh.UMesh mesh)
    //    {
    //        base.OnDrawCall(shadingType, drawcall, policy, mesh);
    //        var indexer = GetVarIndexer(drawcall) as TerrainVarIndexer;

    //        var terrainAttachement = mesh.Tag as UTerrainAttachment;

    //        if (terrainAttachement.HeightMapRSV != null)
    //            drawcall.mCoreObject.BindSRVAll(indexer.HeightMapTexture, terrainAttachement.HeightMapRSV.mCoreObject);
    //    }
    //}
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_ShadingEnv
    {
        public void UnitTestEntrance()
        {
            var env = UEngine.Instance.ShadingEnvManager.GetShadingEnv<Graphics.Pipeline.Mobile.UBasePassOpaque>();
            uint permuationId;
            var values = new List<string>();
            values.Add("1");
            values.Add("1");
            values.Add("0");
            values.Add("0");
            env.GetPermutation(values, out permuationId);
            UnitTestManager.TAssert(permuationId == 3, "");

            values.Clear();
            values.Add("0");
            values.Add("1");
            values.Add("0");
            values.Add("0");
            env.GetPermutation(values, out permuationId);
            UnitTestManager.TAssert(permuationId == 2, "");
        }
    }
}
