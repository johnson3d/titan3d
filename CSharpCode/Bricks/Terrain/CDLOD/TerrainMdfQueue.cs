using EngineNS.Bricks.Terrain.Grass;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class TtTerrainModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public TtTerrainNode TerrainNode;
        public TtPatch Patch;
        public int Dimension = 64;
        public bool IsWater = false;

        public void ActiveRVTs()
        {
            Patch.ActiveRVTs();
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
                EPixelShaderInput.PST_ExtraUV,
                EPixelShaderInput.PST_SpecialData,
            };
        }
        public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {

        }
        public class TtMdfShaderBinder : NxRHI.TtShader.AuxShaderBinderIndexer<TtMdfShaderBinder>
        {
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder TextureSlotBuffer;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder HeightMapTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_HeightMapTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder HeightMapTextureArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder NormalMapTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder MaterialIdMapTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_NormalMapTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder MaterialIdTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_MaterialIdTexture;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder DiffuseTextureArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_DiffuseTextureArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSrView))]
            public NxRHI.TtEffectBinder NormalTextureArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtSampler))]
            public NxRHI.TtEffectBinder Samp_NormalTextureArray;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerPatch;
            [NxRHI.TtShader.TtShaderVar(VarType = typeof(NxRHI.TtBuffer))]
            public NxRHI.TtEffectBinder cbPerTerrain;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeOnDrawCall;
        private static Profiler.TimeScope ScopeOnDrawCall
        {
            get
            {
                if (mScopeOnDrawCall == null)
                    mScopeOnDrawCall = new Profiler.TimeScope(typeof(UTerrainMdfQueue), nameof(OnDrawCall));
                return mScopeOnDrawCall;
            }
        }
        public void OnBuildDrawCall(Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtGraphicDraw drawcall, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {

        }
        public unsafe void OnDrawCall(TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {
            bool bUseRVT = TtEngine.Instance.Config.Feature_UseRVT;
            using (new Profiler.TimeScopeHelper(ScopeOnDrawCall))
            {
                var effectBinder = drawcall.Effect.GetTypedBindIndexer<TtMdfShaderBinder>();
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
                        var coreBinder = Graphics.Pipeline.TtCoreShaderBinder.TtPerTerrainCBufferVarIndexer.Instance;
                        coreBinder.UpdateFieldVar(shaderProg, "cbPerTerrain");
                        TerrainNode.TerrainCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(coreBinder.Binder.mCoreObject);
                    }

                    drawcall.BindCBV(effectBinder.cbPerTerrain, TerrainNode.TerrainCBuffer);

                    // rpolicy 里没有 TtGpuCullingNode 时, 地形走的是非 instancing 的 UTerrainMdfQueue,
                    // 此时 shader 端 GetInstanceData 命中 SysFunctionDefImpl.cginc 的默认实现 ——
                    // 它只填 Position/Quat/Scale, UserData/UserData2 恒为 0, 于是 TerrainCDLOD.cginc
                    // 里六个 Get* 全部归零, 地形被渲染成一块位于原点的平板。
                    // 所以这条路径改由 cbPerPatch 传参, 并置 UsePatchRVTParams = 1 通知 shader。
                    if ((mdfQueue1 is UTerrainInstanceMdfQueue) == false && effectBinder.cbPerPatch != null)
                    {
                        var pat = Patch;
                        pat.SureCBuffer(shaderProg, ref pat.PatchCBuffer);

                        var patchBinder = Graphics.Pipeline.TtCoreShaderBinder.TtPerTerrainPatchCBufferVarIndexer.Instance;
                        pat.PatchCBuffer.SetValue(patchBinder.StartPosition, in pat.StartPosition);
                        pat.PatchCBuffer.SetValue(patchBinder.CurrentLOD, pat.CurrentLOD);

                        pat.TexUVOffset.X = ((float)pat.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                        pat.TexUVOffset.Y = ((float)pat.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                        pat.PatchCBuffer.SetValue(patchBinder.TexUVOffset, in pat.TexUVOffset);

                        // 三个 TexID 与 SetInstanceData 里塞进 UserData 的是同一批值
                        uint heightTexID;
                        if (IsWater && pat.Level.WaterHMap != null)
                            heightTexID = pat.Level.GetWaterHeightmapRVT().UniqueTexID;
                        else
                            heightTexID = pat.Level.GetHeightmapRVT().UniqueTexID;

                        pat.PatchCBuffer.SetValue(patchBinder.UsePatchRVTParams, (uint)1);
                        pat.PatchCBuffer.SetValue(patchBinder.HeightMapTexID, heightTexID);
                        pat.PatchCBuffer.SetValue(patchBinder.NormalMapTexID, pat.Level.GetNormalmapRVT().UniqueTexID);
                        pat.PatchCBuffer.SetValue(patchBinder.MaterialIdTexID, pat.Level.GetMaterialIdRVT().UniqueTexID);

                        drawcall.BindCBV(effectBinder.cbPerPatch, pat.PatchCBuffer);
                    }
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
                        var coreBinder = Graphics.Pipeline.TtCoreShaderBinder.TtPerTerrainPatchCBufferVarIndexer.Instance;
                        var terrain = pat.Level.GetTerrainNode();

                        pat.PatchCBuffer.SetValue(coreBinder.StartPosition, in pat.StartPosition);
                        pat.PatchCBuffer.SetValue(coreBinder.CurrentLOD, pat.CurrentLOD);                        
                        
                        //pat.TexUVOffset.X = (Patch.XInLevel * 64.0f) / 1024.0f;
                        //pat.TexUVOffset.Y = (Patch.ZInLevel * 64.0f) / 1024.0f;

                        //pat.TexUVOffset.X = (Patch.XInLevel * pat.Level.GetTerrainNode().PatchSize) / pat.Level.GetTerrainNode().LevelSize;
                        //pat.TexUVOffset.Y = (Patch.ZInLevel * pat.Level.GetTerrainNode().PatchSize) / pat.Level.GetTerrainNode().LevelSize;

                        pat.TexUVOffset.X = ((float)Patch.XInLevel / (float)pat.Level.GetTerrainNode().PatchSide);
                        pat.TexUVOffset.Y = ((float)Patch.ZInLevel / (float)pat.Level.GetTerrainNode().PatchSide);

                        pat.PatchCBuffer.SetValue(coreBinder.TexUVOffset, in pat.TexUVOffset);

                        // 非 RVT 路径本来就从 cbPerPatch 取参数, shader 端不看这个开关,
                        // 但仍显式写 0, 避免与 RVT 路径复用同一份 PatchCBuffer 时留下残留态。
                        pat.PatchCBuffer.SetValue(coreBinder.UsePatchRVTParams, (uint)0);

                        drawcall.BindCBV(effectBinder.cbPerPatch, pat.PatchCBuffer);
                    }
                    drawcall.BindCBV(effectBinder.cbPerTerrain, TerrainNode.TerrainCBuffer);
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
        public static void SetInstanceData(Graphics.Mesh.TtRenderMesh mesh, Bricks.Terrain.CDLOD.UTerrainMdfQueue mdfQueue, ref Graphics.Pipeline.Shader.FVSInstanceData instance)
        {
            var cb =  mesh.PerMeshCBuffer;
            var matrix = cb.GetMatrix(Graphics.Pipeline.TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.WorldMatrix);

            instance.Position = matrix.Translation;
            instance.Scale = matrix.Scale;// mdfQueue.Patch.StartPosition;
            instance.Quat = matrix.Rotation;
            instance.HitProxyId = cb.GetValue<uint>(Graphics.Pipeline.TtCoreShaderBinder.TtPerMeshCBufferVarIndexer.Instance.HitProxyId);

            var patch = mdfQueue.Patch;
            if (mdfQueue.TerrainModifier.IsWater && patch.Level.WaterHMap!=null)
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
