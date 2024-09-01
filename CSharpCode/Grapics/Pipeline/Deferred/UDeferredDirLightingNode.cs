using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Pipeline.Common;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.NxRHI;
using Microsoft.CodeAnalysis.Host;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public partial class UDeferredDirLightingShading : Shader.TtGraphicsShadingEnv
    {
        #region Permutation
        public UPermutationItem DisableAO
        {
            get;
            set;
        }
        public UPermutationItem DisablePointLights
        {
            get;
            set;
        }
        public UPermutationItem DisableShadow
        {
            get;
            set;
        }
        public UPermutationItem DisableSunshaft
        {
            get;
            set;
        }
        public UPermutationItem DisableBloom
        {
            get;
            set;
        }
        public UPermutationItem DisableHdr
        {
            get;
            set;
        }
        #endregion
        public UDeferredDirLightingShading()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredDirLighting.cginc", RName.ERNameType.Engine);

            this.BeginPermutaion();

            DisableAO = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_AO", (int)Shader.EPermutation_Bool.BitWidth);
            DisablePointLights = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_POINTLIGHTS", (int)Shader.EPermutation_Bool.BitWidth);
            DisableShadow = this.PushPermutation<Shader.EPermutation_Bool>("DISABLE_SHADOW_ALL", (int)Shader.EPermutation_Bool.BitWidth);
            DisableSunshaft = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_SUNSHAFT", (int)Shader.EPermutation_Bool.BitWidth);
            DisableBloom = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_BLOOM", (int)Shader.EPermutation_Bool.BitWidth);
            DisableHdr = this.PushPermutation<Shader.EPermutation_Bool>("ENV_DISABLE_HDR", (int)Shader.EPermutation_Bool.BitWidth);

            DisableAO.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisableShadow.SetValue((int)Shader.EPermutation_Bool.FalseValue);
            DisablePointLights.SetValue((int)Shader.EPermutation_Bool.FalseValue);

            DisableSunshaft.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableBloom.SetValue((int)Shader.EPermutation_Bool.TrueValue);
            DisableHdr.SetValue((int)Shader.EPermutation_Bool.TrueValue);

            this.UpdatePermutation();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { 
                NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,
            };
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_LightMap,
            };
        }
        public unsafe override void OnBuildDrawCall(TtRenderPolicy policy, NxRHI.UGraphicDraw drawcall)
        {
        }
        public unsafe override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, TtRenderPolicy policy, Mesh.TtMesh.TtAtom atom)
        {
            base.OnDrawCall(cmd, drawcall, policy, atom);

            var dirLightingNode = drawcall.TagObject as UDeferredDirLightingNode;

            var index = drawcall.FindBinder("cbPerGpuScene");
            if (index.IsValidPointer)
            {
                //drawcall.mCoreObject.BindShaderCBuffer(index, Manager.GetGpuSceneNode().PerGpuSceneCBuffer.mCoreObject);
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.GpuScenePinIn);
                drawcall.BindCBuffer(index, attachBuffer.Cbv);
            }

            #region MRT
            index = drawcall.FindBinder("GBufferRT0");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt0PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT0");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("GBufferRT1");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt1PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT1");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT2");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt2PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT2");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("GBufferRT3");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.Rt3PinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GBufferRT3");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.DepthStencilPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);
            #endregion

            #region shadow
            index = drawcall.FindBinder("GShadowMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.ShadowMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GShadowMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            #endregion

            #region effect
            index = drawcall.FindBinder("gEnvMap");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.EnvMapPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_gEnvMap");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);

            index = drawcall.FindBinder("gPreIntegratedGF");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, TtEngine.Instance.GetPreIntegratedDFSrv(cmd));
            }
            index = drawcall.FindBinder("Samp_gPreIntegratedGF");
            if (index.IsValidPointer)
            {
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            }

            index = drawcall.FindBinder("GVignette");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.VignettePinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_GVignette");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.LinearClampState);
            #endregion

            #region MultiLights
            index = drawcall.FindBinder("TilingBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.TileScreenPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }

            index = drawcall.FindBinder("GpuScene_PointLights");
            if (index.IsValidPointer)
            {
                var attachBuffer = dirLightingNode.GetAttachBuffer(dirLightingNode.PointLightsPinIn);
                if (attachBuffer.Srv != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            #endregion

            index = drawcall.FindBinder("cbPerCamera");
            if (index.IsValidPointer)
            {
                drawcall.BindCBuffer(index, policy.DefaultCamera.PerCameraCBuffer);
            }
        }
        public void SetDisableShadow(bool value)
        {
            DisableShadow.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableAO(bool value)
        {
            DisableAO.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableSunShaft(bool value)
        {
            DisableSunshaft.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableBloom(bool value)
        {
            DisableBloom.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisableHDR(bool value)
        {
            DisableHdr.SetValue(value);
            UpdatePermutation();
        }
        public void SetDisablePointLights(bool value)
        {
            DisablePointLights.SetValue(value);
            UpdatePermutation();
        }
    }
    [Bricks.CodeBuilder.ContextMenu("DirLighting", "Deferred\\DirLighting", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public partial class UDeferredDirLightingNode : Common.USceenSpaceNode
    {
        public TtRenderGraphPin Rt0PinIn = TtRenderGraphPin.CreateInput("MRT0");
        public TtRenderGraphPin Rt1PinIn = TtRenderGraphPin.CreateInput("MRT1");
        public TtRenderGraphPin Rt2PinIn = TtRenderGraphPin.CreateInput("MRT2");
        public TtRenderGraphPin Rt3PinIn = TtRenderGraphPin.CreateInputOutput("MRT3");
        public TtRenderGraphPin DepthStencilPinIn = TtRenderGraphPin.CreateInputOutput("DepthStencil");
        
        public TtRenderGraphPin ShadowMapPinIn = TtRenderGraphPin.CreateInput("ShadowMap");
        public TtRenderGraphPin EnvMapPinIn = TtRenderGraphPin.CreateInput("EnvMap");
        public TtRenderGraphPin VignettePinIn = TtRenderGraphPin.CreateInput("Vignette");
        public TtRenderGraphPin TileScreenPinIn = TtRenderGraphPin.CreateInput("TileScreen");
        public TtRenderGraphPin GpuScenePinIn = TtRenderGraphPin.CreateInput("GpuScene");
        public TtRenderGraphPin PointLightsPinIn = TtRenderGraphPin.CreateInput("PointLights");

        public UDeferredDirLightingNode()
        {
            Name = "UDeferredDirLightingNode";
        }
        public override void InitNodePins()
        {
            ResultPinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
            base.InitNodePins();

            AddInput(Rt0PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Rt1PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(Rt2PinIn, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(Rt3PinIn, NxRHI.EBufferType.BFT_SRV);
            //Rt3PinIn.IsAllowInputNull = true;
            AddInputOutput(DepthStencilPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(ShadowMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(EnvMapPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(VignettePinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(TileScreenPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(PointLightsPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(GpuScenePinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            base.FrameBuild(policy);
        }
        public UDeferredDirLightingShading mBasePassShading;
        public override TtGraphicsShadingEnv GetPassShading(TtMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<UDeferredDirLightingShading>();
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(UDeferredDirLightingNode), nameof(TickLogic));
                return mScopeTick;
            }
        } 
        public override void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                GBuffers?.SetViewportCBuffer(world, policy);
                base.TickLogic(world, policy, bClear);
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public void Dispose()
        {
            unsafe
            {
                if(PreIntegratedDFData != IntPtr.Zero)
                    CoreSDK.Free(PreIntegratedDFData.ToPointer());
            }

            PreIntegratedDFTexture?.Dispose();
            PreIntegratedDFTexture = null;
            PreIntegratedDFSrv?.Dispose();
            PreIntegratedDFSrv = null;
        }

        uint ReverseBits(uint Bits)
        {
            Bits = (Bits << 16) | (Bits >> 16);
            Bits = ((Bits & 0x00ff00ff) << 8) | ((Bits & 0xff00ff00) >> 8);
            Bits = ((Bits & 0x0f0f0f0f) << 4) | ((Bits & 0xf0f0f0f0) >> 4);
            Bits = ((Bits & 0x33333333) << 2) | ((Bits & 0xcccccccc) >> 2);
            Bits = ((Bits & 0x55555555) << 1) | ((Bits & 0xaaaaaaaa) >> 1);
            return Bits;
        }
        IntPtr PreIntegratedDFData;
        void InitPreIntegratedDF()
        {
            unsafe
            {
                int Y = 32;
                int X = 128;
                uint BufferSize = (uint)X * (uint)Y * (uint)sizeof(UInt16) * 2;
                var pAddr = (UInt16*)CoreSDK.Alloc(BufferSize, "PreIntegratedDFTexture", 0);
                for (int y = 0; y < Y; y++)
                {
                    float Roughness = (float)(y + 0.5f) / Y;
                    float m = Roughness * Roughness;
                    float m2 = m * m;

                    for (int x = 0; x < X; x++)
                    {
                        float NoV = (float)(x + 0.5f) / X;

                        Vector3 V;
                        V.X = MathHelper.Sqrt(1.0f - NoV * NoV);    // sin
                        V.Y = 0.0f;
                        V.Z = NoV;                              // cos

                        float A = 0.0f;
                        float B = 0.0f;
                        float C = 0.0f;

                        const uint NumSamples = 128;
                        for (uint i = 0; i < NumSamples; i++)
                        {
                            float E1 = (float)i / NumSamples;
                            double v1 = ReverseBits(i);
                            double v2 = 4294967296;
                            float E2 = (float)(v1 / v2);

                            {
                                float Phi = 2.0f * MathHelper.PI * E1;
                                float CosPhi = MathHelper.Cos(Phi);
                                float SinPhi = MathHelper.Sin(Phi);
                                float CosTheta = MathHelper.Sqrt((1.0f - E2) / (1.0f + (m2 - 1.0f) * E2));
                                float SinTheta = MathHelper.Sqrt(1.0f - CosTheta * CosTheta);

                                Vector3 H = new Vector3(SinTheta * MathHelper.Cos(Phi), SinTheta * MathHelper.Sin(Phi), CosTheta);
                                Vector3 L = 2.0f * Vector3.Dot(V, H) * H - V;

                                float NoL = MathHelper.Max(L.Z, 0.0f);
                                float NoH = MathHelper.Max(H.Z, 0.0f);
                                float VoH = MathHelper.Max(Vector3.Dot(V, H), 0.0f);

                                if (NoL > 0.0f)
                                {
                                    float Vis_SmithV = NoL * (NoV * (1 - m) + m);
                                    float Vis_SmithL = NoV * (NoL * (1 - m) + m);
                                    float Vis = 0.5f / (Vis_SmithV + Vis_SmithL);

                                    float NoL_Vis_PDF = NoL * Vis * (4.0f * VoH / NoH);
                                    float Fc = 1.0f - VoH;

                                    Fc *= (Fc * Fc) * (Fc * Fc);
                                    A += NoL_Vis_PDF * (1.0f - Fc);
                                    B += NoL_Vis_PDF * Fc;
                                }
                            }

                            {
                                float Phi = 2.0f * MathHelper.PI * E1;
                                float CosPhi = MathHelper.Cos(Phi);
                                float SinPhi = MathHelper.Sin(Phi);
                                float CosTheta = MathHelper.Sqrt(E2);
                                float SinTheta = MathHelper.Sqrt(1.0f - CosTheta * CosTheta);

                                Vector3 L = new Vector3(SinTheta * MathHelper.Cos(Phi), SinTheta * MathHelper.Sin(Phi), CosTheta);
                                Vector3 H = (V + L);
                                H.Normalize();

                                float NoL = MathHelper.Max(L.Z, 0.0f);
                                float NoH = MathHelper.Max(H.Z, 0.0f);
                                float VoH = MathHelper.Max(Vector3.Dot(V, H), 0.0f);

                                float FD90 = 0.5f + 2.0f * VoH * VoH * Roughness;
                                float FdV = 1.0f + (FD90 - 1.0f) * MathHelper.Pow(1.0f - NoV, 5);
                                float FdL = 1.0f + (FD90 - 1.0f) * MathHelper.Pow(1.0f - NoL, 5);
                                C += FdV * FdL;// * ( 1.0f - 0.3333f * Roughness );
                            }
                        }
                        A /= NumSamples;
                        B /= NumSamples;
                        C /= NumSamples;

                        //if (Desc.Format == PF_G16R16)
                        {
                            pAddr[(x + y * X) * 2] = (UInt16)(MathHelper.Clamp(A, 0.0f, 1.0f) * 65535.0f + 0.5f);
                            pAddr[(x + y * X) * 2 + 1] = (UInt16)(MathHelper.Clamp(B, 0.0f, 1.0f) * 65535.0f + 0.5f);
                        }
                    }
                }
                PreIntegratedDFData = (IntPtr)pAddr;
            }
        }

        public TtSrView GetPreIntegratedDFSrv(NxRHI.ICommandList cmd)
        {
            if (PreIntegratedDFTexture == null || PreIntegratedDFSrv == null)
            {
                var desc = new NxRHI.FTextureDesc();
                desc.SetDefault();
                desc.Format = EPixelFormat.PXF_R16G16_UNORM;
                desc.Width = 128;
                desc.Height = 32;

                PreIntegratedDFTexture = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetTexture2DArray();
                srvDesc.Format = desc.Format;
                srvDesc.Texture2DArray.ArraySize = desc.ArraySize;
                srvDesc.Texture2DArray.FirstArraySlice = 0;
                srvDesc.Texture2DArray.MipLevels = desc.MipLevels;
                srvDesc.Texture2DArray.MostDetailedMip = 0;
                PreIntegratedDFSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(PreIntegratedDFTexture, in srvDesc);

                var fp = new NxRHI.FSubResourceFootPrint();
                fp.SetDefault();
                fp.Format = PreIntegratedDFTexture.mCoreObject.Desc.Format;
                fp.Width = PreIntegratedDFTexture.mCoreObject.Desc.Width;
                fp.Height = PreIntegratedDFTexture.mCoreObject.Desc.Height;
                fp.Depth = 1;
                fp.RowPitch = (uint)fp.Width * sizeof(UInt16) * 2;
                uint BufferSize = 128 * 32 * (uint)sizeof(UInt16) * 2;
                fp.TotalSize = BufferSize;

                unsafe
                {
                    PreIntegratedDFTexture.UpdateGpuData(cmd, 0, PreIntegratedDFData.ToPointer(), &fp);
                }
            }

            return PreIntegratedDFSrv;
        }

        UTexture PreIntegratedDFTexture = null;
        TtSrView PreIntegratedDFSrv = null;
    }
}
