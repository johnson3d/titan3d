using EngineNS.Bricks.Terrain.Grass;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class TtTerrainModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public UTerrainNode TerrainNode;
        public TtPatch Patch;
        public int Dimension = 64;
        public bool IsWater = false;

        public void ActiveRVTs()
        {
            TerrainNode.Terrain.HeightmapRVT.ActiveRVT(Patch.Level.HeightMapSRV);
            TerrainNode.Terrain.HeightmapRVT.ActiveRVT(Patch.Level.WaterHMapSRV);
            TerrainNode.Terrain.NormalmapRVT.ActiveRVT(Patch.Level.NormalMapSRV);
            TerrainNode.Terrain.MaterialIdRVT.ActiveRVT(Patch.Level.MaterialIdMapSRV);
        }

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
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            return (NxRHI.FShaderCode*)0;
        }
        public string GetUniqueText()
        {
            return "";
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
                EPixelShaderInput.PST_SpecialData,
            };
        }
        public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {

        }
        public class UMdfShaderBinder : Graphics.Pipeline.UCoreShaderBinder.UShaderResourceIndexer
        {
            public void Init(NxRHI.TtShaderEffect effect)
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
            public NxRHI.TtEffectBinder TextureSlotBuffer;
            public NxRHI.TtEffectBinder HeightMapTexture;
            public NxRHI.TtEffectBinder Samp_HeightMapTexture;
            public NxRHI.TtEffectBinder HeightMapTextureArray;
            public NxRHI.TtEffectBinder NormalMapTexture;
            public NxRHI.TtEffectBinder MaterialIdMapTexture;
            public NxRHI.TtEffectBinder Samp_NormalMapTexture;
            public NxRHI.TtEffectBinder MaterialIdTexture;
            public NxRHI.TtEffectBinder Samp_MaterialIdTexture;
            public NxRHI.TtEffectBinder DiffuseTextureArray;
            public NxRHI.TtEffectBinder Samp_DiffuseTextureArray;
            public NxRHI.TtEffectBinder NormalTextureArray;
            public NxRHI.TtEffectBinder Samp_NormalTextureArray;
            public NxRHI.TtEffectBinder cbPerPatch;
            public NxRHI.TtEffectBinder cbPerTerrain;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeOnDrawCall = Profiler.TimeScopeManager.GetTimeScope(typeof(UTerrainMdfQueue), nameof(OnDrawCall));
        public unsafe void OnDrawCall(TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            bool bUseRVT = TtEngine.Instance.Config.Feature_UseRVT;
            using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
            {
                var effectBinder = drawcall.Effect.mBindIndexer as UMdfShaderBinder;
                if (effectBinder == null)
                {
                    effectBinder = new UMdfShaderBinder();
                    effectBinder.Init(drawcall.Effect.ShaderEffect);
                    drawcall.Effect.mBindIndexer = effectBinder;
                }

                var shaderProg = drawcall.mCoreObject.GetGraphicsEffect();

                if (bUseRVT)
                {
                    var slotBufferSRV = TerrainNode.Terrain.HeightmapRVT.TextureSlotBuffer.Srv;
                    drawcall.BindSRV(effectBinder.TextureSlotBuffer, slotBufferSRV);

                    var texture = TerrainNode.Terrain.HeightmapRVT.TextureSlotAllocator.TextureArraySRV;
                    drawcall.BindSRV(effectBinder.HeightMapTexture, texture);

                    texture = TerrainNode.Terrain.NormalmapRVT.TextureSlotAllocator.TextureArraySRV;
                    drawcall.BindSRV(effectBinder.NormalMapTexture, texture);

                    texture = TerrainNode.Terrain.MaterialIdRVT.TextureSlotAllocator.TextureArraySRV;
                    drawcall.BindSRV(effectBinder.MaterialIdTexture, texture);

                    if (TerrainNode.TerrainCBuffer == null)
                    {
                        var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
                        coreBinder.CBPerTerrain.UpdateFieldVar(shaderProg, "cbPerTerrain");
                        TerrainNode.TerrainCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.CBPerTerrain.Binder.mCoreObject);
                    }

                    drawcall.BindCBuffer(effectBinder.cbPerTerrain, TerrainNode.TerrainCBuffer);
                }
                else
                {
                    var pat = Patch;
                    if (IsWater)
                    {
                        drawcall.BindSRV(effectBinder.HeightMapTexture, pat.Level.WaterHMapSRV);
                    }
                    else
                    {
                        drawcall.BindSRV(effectBinder.HeightMapTexture, pat.Level.HeightMapSRV);
                    }
                    drawcall.BindSRV(effectBinder.NormalMapTexture, pat.Level.NormalMapSRV);
                    drawcall.BindSRV(effectBinder.MaterialIdTexture, Patch.Level.MaterialIdMapSRV);

                    pat.SureCBuffer(drawcall.mCoreObject.GetGraphicsEffect(), ref pat.PatchCBuffer);
                    if (effectBinder.cbPerPatch != null)
                    {
                        var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
                        var terrain = pat.Level.GetTerrainNode();

                        pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.StartPosition, in pat.StartPosition);
                        pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.CurrentLOD, pat.CurrentLOD);                        
                        
                        //pat.TexUVOffset.X = (Patch.XInLevel * 64.0f) / 1024.0f;
                        //pat.TexUVOffset.Y = (Patch.ZInLevel * 64.0f) / 1024.0f;

                        //pat.TexUVOffset.X = (Patch.XInLevel * pat.Level.GetTerrainNode().PatchSize) / pat.Level.GetTerrainNode().LevelSize;
                        //pat.TexUVOffset.Y = (Patch.ZInLevel * pat.Level.GetTerrainNode().PatchSize) / pat.Level.GetTerrainNode().LevelSize;

                        pat.TexUVOffset.X = ((float)Patch.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                        pat.TexUVOffset.Y = ((float)Patch.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);

                        pat.PatchCBuffer.SetValue(coreBinder.CBPerTerrainPatch.TexUVOffset, in pat.TexUVOffset);

                        drawcall.BindCBuffer(effectBinder.cbPerPatch, pat.PatchCBuffer);
                    }
                    drawcall.BindCBuffer(effectBinder.cbPerTerrain, TerrainNode.TerrainCBuffer);
                }
                
                drawcall.BindSampler(effectBinder.Samp_HeightMapTexture, policy.ClampState);// TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);                
                drawcall.BindSampler(effectBinder.Samp_NormalMapTexture, policy.ClampState);// TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState.mCoreObject);
                drawcall.BindSampler(effectBinder.Samp_MaterialIdTexture, policy.ClampPointState);

                var srv = TerrainNode.TerrainMaterialIdManager.DiffuseTextureArraySRV;
                drawcall.BindSRV(effectBinder.DiffuseTextureArray, srv);
                drawcall.BindSampler(effectBinder.Samp_DiffuseTextureArray, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

                srv = TerrainNode.TerrainMaterialIdManager.NormalTextureArraySRV;
                drawcall.BindSRV(effectBinder.NormalTextureArray, srv);
                drawcall.BindSampler(effectBinder.Samp_NormalTextureArray, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);
            }
        }
        public static void SetInstanceData(Graphics.Mesh.TtMesh mesh, Bricks.Terrain.CDLOD.UTerrainMdfQueue mdfQueue, ref Graphics.Mesh.Modifier.FVSInstanceData instance)
        {
            var cb =  mesh.PerMeshCBuffer;
            var matrix = cb.GetMatrix(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.WorldMatrix);

            instance.Position = matrix.Translation;
            instance.Scale = matrix.Scale;// mdfQueue.Patch.StartPosition;
            instance.Quat = matrix.Rotation;
            instance.HitProxyId = cb.GetValue<uint>(TtEngine.Instance.GfxDevice.CoreShaderBinder.CBPerMesh.HitProxyId);

            var patch = mdfQueue.Patch;
            if(mdfQueue.TerrainModifier.IsWater)
                instance.UserData.X = patch.Level.GetWaterHeightmapRVT().UniqueTexID;
            else
                instance.UserData.X = patch.Level.GetHeightmapRVT().UniqueTexID;
            instance.UserData.Y = patch.Level.GetNormalmapRVT().UniqueTexID;
            instance.UserData.Z = patch.Level.GetMaterialIdRVT().UniqueTexID; 
            instance.UserData.W = (uint)mdfQueue.Patch.CurrentLOD;

            instance.UserData2.X = CoreSDK.AsUInt32(patch.StartPosition.X);
            instance.UserData2.Y = CoreSDK.AsUInt32(patch.StartPosition.Y);
            instance.UserData2.Z = CoreSDK.AsUInt32(patch.StartPosition.Z);

            Vector2 TexUVOffset;
            TexUVOffset.X = ((float)patch.XInLevel / (float)patch.Level.GetTerrainNode().PatchSide);
            TexUVOffset.Y = ((float)patch.ZInLevel / (float)patch.Level.GetTerrainNode().PatchSide);
            instance.UserData2.W = CoreSDK.AsUInt32(TexUVOffset.X);
            instance.Scale_Pad = CoreSDK.AsUInt32(TexUVOffset.Y);
            //instance.Quat.X = mdfQueue.Patch.TexUVOffset.X;
            //instance.Quat.Y = mdfQueue.Patch.TexUVOffset.Y;
            //instance.PointLightIndices.X = CoreSDK.AsUInt32(mdfQueue.Patch.TexUVOffset.X);
            //instance.PointLightIndices.Y = CoreSDK.AsUInt32(mdfQueue.Patch.TexUVOffset.Y);
        }
    }
    public class UTerrainMdfQueue : Graphics.Pipeline.Shader.TtMdfQueue1<TtTerrainModifier>
    {
        public TtPatch Patch
        {
            get
            {
                return TerrainModifier.Patch;
            }
        }
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            TerrainModifier.Patch = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.Patch;
            TerrainModifier.TerrainNode = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.TerrainNode;
            TerrainModifier.Dimension = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.Dimension;
            TerrainModifier.IsWater = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.IsWater;
        }
        public TtTerrainModifier TerrainModifier
        {
            get => this.Modifiers[0] as TtTerrainModifier;
        }
    }
    public class UTerrainInstanceMdfQueue : Graphics.Pipeline.Shader.TtMdfQueue2<TtTerrainModifier, Graphics.Mesh.Modifier.TtInstanceModifier>
    {
        public override void CopyFrom(TtMdfQueueBase mdf)
        {
            base.CopyFrom(mdf);
            TerrainModifier.Patch = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.Patch;
            TerrainModifier.TerrainNode = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.TerrainNode;
            TerrainModifier.Dimension = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.Dimension;
            TerrainModifier.IsWater = (mdf as UTerrainInstanceMdfQueue).TerrainModifier.IsWater;
        }
        public TtTerrainModifier TerrainModifier
        {
            get => this.Modifiers[0] as TtTerrainModifier;
        }
        public Graphics.Mesh.Modifier.TtInstanceModifier InstanceModifier
        {
            get => this.Modifiers[1] as Graphics.Mesh.Modifier.TtInstanceModifier;
        }
    }

}
