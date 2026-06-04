using BCnEncoder.Shared;
using EngineNS.Bricks.VXGI;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Shader;
using EngineNS.Graphics.Pipeline.Shadow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static EngineNS.Graphics.Pipeline.Shader.TtMaterial;

namespace EngineNS.Graphics.Pipeline.Deferred
{
    public class TtDeferredOpaque : Shader.TtGraphicsShadingEnv
    {
        public TtDeferredOpaque()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredOpaque.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_Normal,
                NxRHI.EVertexStreamType.VST_Tangent,
                NxRHI.EVertexStreamType.VST_UV,
                NxRHI.EVertexStreamType.VST_Color};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
                EPixelShaderInput.PST_Custom1,
                EPixelShaderInput.PST_Custom2,
            };
        }
    }
    public class TtDeferredOpaqueMeshlets : Shader.TtGraphicsShadingEnv
    {
        public TtDeferredOpaqueMeshlets()
        {
            CodeName = RName.GetRName("shaders/ShadingEnv/Deferred/DeferredOpaqueMeshlets.cginc", RName.ERNameType.Engine);
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,};
        }
        public override EPixelShaderInput[] GetPSNeedInputs()
        {
            return new EPixelShaderInput[] {
                EPixelShaderInput.PST_Position,
                EPixelShaderInput.PST_Normal,
                EPixelShaderInput.PST_UV,
                EPixelShaderInput.PST_Color,
                EPixelShaderInput.PST_Custom1,
                EPixelShaderInput.PST_Custom2,
            };
        }
    }
    [Bricks.CodeBuilder.ContextMenu("BassPass", "Deferred\\BassPass", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Deferred.UDeferredBasePassNode@EngineCore", "EngineNS.Graphics.Pipeline.Deferred.UDeferredBasePassNode" })]
    public class TtDeferredBasePassNode : Common.TtBasePassNode
    {
        //MRT中存储的数据格式设计，共128bits
        // Pure semantic GBuffer data definition — how these fields are packed into MRT
        // is entirely handled by EncodeGBuffer/DecodeGBuffer in shader code.
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FGBufferDataBase")]
        public struct FGBufferDataBase
        {
            #region Helpers
            static Vector2 OctEncode(Vector3 n)
            {
                var an = new Vector3(MathF.Abs(n.X), MathF.Abs(n.Y), MathF.Abs(n.Z));
                float invSum = 1.0f / (an.X + an.Y + an.Z);
                float ox = n.X * invSum;
                float oy = n.Y * invSum;
                if (n.Z < 0.0f)
                {
                    float tmpX = (1.0f - MathF.Abs(oy)) * (ox >= 0.0f ? 1.0f : -1.0f);
                    float tmpY = (1.0f - MathF.Abs(ox)) * (oy >= 0.0f ? 1.0f : -1.0f);
                    ox = tmpX;
                    oy = tmpY;
                }
                return new Vector2(ox * 0.5f + 0.5f, oy * 0.5f + 0.5f);
            }
            static Vector3 OctDecode(Vector2 enc)
            {
                float fx = enc.X * 2.0f - 1.0f;
                float fy = enc.Y * 2.0f - 1.0f;
                float nz = 1.0f - MathF.Abs(fx) - MathF.Abs(fy);
                float t = Math.Clamp(-nz, 0.0f, 1.0f);
                float nx = fx + (fx >= 0.0f ? -t : t);
                float ny = fy + (fy >= 0.0f ? -t : t);
                var v = new Vector3(nx, ny, nz);
                v.Normalize();
                return v;
            }
            static Vector3 EncodeNormalXYZ(Vector3 n) => n * 0.5f + new Vector3(0.5f);
            static Vector3 DecodeNormalXYZ(Vector3 enc) => enc * 2.0f - new Vector3(1.0f);
            #endregion

            #region ShadingMode
            public int GetShadingMode()
            {
                return (mRenderFlags_10Bit & (int)ERenderFlags.ShadingModeMask) >> (int)ERenderFlags.ShadingModeMaskShift;
            }
            public bool IsHair() => GetShadingMode() == (int)EShadingMode.Hair;
            public bool IsSubsurface() => GetShadingMode() == (int)EShadingMode.Subsurface;
            #endregion

            #region MRT Encode / Decode
            public void EncodeGBuffer(out Vector4 rt0/*rgba8*/, out Vector4 rt1/*rgb10a2*/, out Vector4 rt2/*rgba8*/, out Vector4 rt3/*rgb10a2*/)
            {
                //rt0 = Vector4.Zero;
                //rt1 = Vector4.Zero;
                //rt2 = Vector4.Zero;
                //rt3 = Vector4.Zero;

                rt0.X = mMtlColorRaw.X;
                rt0.Y = mMtlColorRaw.Y;
                rt0.Z = mMtlColorRaw.Z;

                // RenderFlags → rt3.b (10-bit integer normalized to [0,1])
                rt3.Z = mRenderFlags_10Bit / 1023.0f;

                bool useOctNormal = TtEngine.Instance.GfxDevice.Config.UseOctahedronNormal;

                if (IsHair())
                {
                    // Hair: normal oct-encoded into rt0.a + rt2.r, tangent into rt1
                    var normalOct = OctEncode(mWorldNormal);
                    rt0.W = normalOct.X;
                    rt2.X = normalOct.Y;

                    if (!useOctNormal)
                    {
                        var encTangent = EncodeNormalXYZ(mWorldTangent);
                        rt1.X = encTangent.X;
                        rt1.Y = encTangent.Y;
                        rt1.Z = encTangent.Z;
                    }
                    else
                    {
                        var tangentOct = OctEncode(mWorldTangent);
                        rt1.X = tangentOct.X;
                        rt1.Y = tangentOct.Y;
                        rt1.Z = 0;
                    }
                }
                else
                {
                    // rt0.a: per-ShadingMode scalar
                    if (IsSubsurface())
                        rt0.W = Math.Clamp(mSubsurfaceProfileIndex / 255.0f, 0.0f, 1.0f);
                    else
                        rt0.W = mSpecOcclusion;

                    if (!useOctNormal)
                    {
                        var encNormal = EncodeNormalXYZ(mWorldNormal);
                        rt1.X = encNormal.X;
                        rt1.Y = encNormal.Y;
                        rt1.Z = encNormal.Z;
                    }
                    else
                    {
                        var normalOct = OctEncode(mWorldNormal);
                        rt1.X = normalOct.X;
                        rt1.Y = normalOct.Y;
                        rt1.Z = 0;
                    }

                    rt2.X = mMetallicity;
                }

                rt1.W = mMask;
                rt2.Y = mSpecular;
                rt2.Z = mRoughness;
                rt2.W = mAO;

                // Motion vector (direct pass-through, matching shader non-MOTIONVECTOR_SCALAR path)
                rt3.X = mMotionVector.X;
                rt3.Y = -mMotionVector.Y;
                rt3.W = Math.Clamp(mOpacity, 0.0f, 1.0f);
            }

            public void DecodeGBuffer(Vector4 rt0, Vector4 rt1, Vector4 rt2, Vector4 rt3)
            {
                mMtlColorRaw = new Vector3(rt0.X, rt0.Y, rt0.Z);

                // RenderFlags from rt3.b
                mRenderFlags_10Bit = (int)(rt3.Z * 1023.0f + 0.5f);

                bool useOctNormal = TtEngine.Instance.GfxDevice.Config.UseOctahedronNormal;
                Vector3 decodedDir;
                if (!useOctNormal)
                    decodedDir = DecodeNormalXYZ(new Vector3(rt1.X, rt1.Y, rt1.Z));
                else
                    decodedDir = OctDecode(new Vector2(rt1.X, rt1.Y));

                if (IsHair())
                {
                    mWorldTangent = decodedDir;
                    mWorldNormal = OctDecode(new Vector2(rt0.W, rt2.X));
                    mMetallicity = 0;
                    mSubsurfaceProfileIndex = 0;
                    mSpecOcclusion = 0;
                }
                else
                {
                    mWorldNormal = decodedDir;
                    mWorldTangent = Vector3.Zero;
                    mMetallicity = rt2.X;

                    if (IsSubsurface())
                    {
                        mSubsurfaceProfileIndex = (float)(int)(rt0.W * 255.0f + 0.5f);
                        mSpecOcclusion = 0;
                    }
                    else
                    {
                        mSubsurfaceProfileIndex = 0;
                        mSpecOcclusion = rt0.W;
                    }
                }

                mMask = rt1.W;
                mSpecular = rt2.Y;
                mRoughness = rt2.Z;
                mAO = rt2.W;

                // Motion vector (direct pass-through)
                mMotionVector = new Vector2(rt3.X, rt3.Y);
                mOpacity = rt3.W;
            }
            #endregion

            public Vector3 mMtlColorRaw;
            public float mSubsurfaceProfileIndex; // SSS profile index (0~255 integer, normalized in encode)
            public float mSpecOcclusion;           // specular occlusion for PBR shading mode

            public Vector3 mWorldNormal;
            public Vector3 mWorldTangent;
            public int mRenderFlags_10Bit;
            public float mMask;

            public float mMetallicity;
            // F0 = lerp(AbsSpecular, Albedo, Metallic)
            public float mSpecular;
            public float mAO;
            public float mRoughness;

            public Vector2 mMotionVector;
            public uint mCustomData_10Bit;
            public float mOpacity;
        }
        public TtRenderGraphPin VisiblesPinIn = TtRenderGraphPin.CreateInput("Visibles", NxRHI.EBufferType.BFT_NONE);
        public TtRenderGraphPin GpuCullPinIn = TtRenderGraphPin.CreateInput("GpuCull", NxRHI.EBufferType.BFT_NONE);
        public TtRenderGraphPin Rt0PinOut = TtRenderGraphPin.CreateInputOutput("MRT0", true, EPixelFormat.PXF_R8G8B8A8_UNORM, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);//rgb - metallicty
        public TtRenderGraphPin Rt1PinOut = TtRenderGraphPin.CreateInputOutput("MRT1", true, EPixelFormat.PXF_R10G10B10A2_UNORM, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);//normal - Flags
        public TtRenderGraphPin Rt2PinOut = TtRenderGraphPin.CreateInputOutput("MRT2", true, EPixelFormat.PXF_R8G8B8A8_UNORM, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);//Roughness,Emissive,Specular,unused
        public TtRenderGraphPin Rt3PinOut = TtRenderGraphPin.CreateInputOutput("MRT3", true, EPixelFormat.PXF_R10G10B10A2_UNORM, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);//motionXY,RenderFlag10Bits,custom.g
        public TtRenderGraphPin DepthStencilPinOut = TtRenderGraphPin.CreateInputOutput("DepthStencil", true, EPixelFormat.PXF_D24_UNORM_S8_UINT, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);

        public TtCpuCullingNode CpuCullNode = null;
        public TtGpuCullingNode GpuCullNode = null;
        [Category("Option")]
        [Rtti.Meta("")]
        public bool ClearMRT
        {
            get;
            set;
        } = true;
        [Category("Option")]
        [Rtti.Meta("")]
        public bool EnableMeshlets
        {
            get;
            set;
        } = false;
        public TtDeferredBasePassNode()
        {
            Name = "UDeferredBasePassNode";
        }
        public override void InitNodePins()
        {
            AddInput(VisiblesPinIn);
            AddInput(GpuCullPinIn);
            GpuCullPinIn.IsAllowInputNull = true;
            AddInputOutput(Rt0PinOut);
            AddInputOutput(Rt1PinOut);
            AddInputOutput(Rt2PinOut);
            AddInputOutput(Rt3PinOut);
            AddInputOutput(DepthStencilPinOut);

            Rt0PinOut.IsAllowInputNull = true;
            Rt1PinOut.IsAllowInputNull = true;
            Rt2PinOut.IsAllowInputNull = true;
            Rt3PinOut.IsAllowInputNull = true;
            DepthStencilPinOut.IsAllowInputNull = true;
        }
        public override void OnResize(TtRenderPolicy policy, float x, float y)
        {
            if (GBuffers != null)
            {
                GBuffers.SetSize(x, y);
            }

            Rt0PinOut.Attachement.Height = (uint)y;
            Rt0PinOut.Attachement.Width = (uint)x;
        }
        public override void FrameBuild(TtRenderPolicy policy)
        {
            
        }
        
        public TtDeferredOpaque mOpaqueShading;
        public TtDeferredOpaqueMeshlets mMeshletsOpaqueShading;
        public NxRHI.TtRenderPass RenderPass;

        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            
            CreateGBuffers(policy, Rt0PinOut.Attachement.Format);
            
            mOpaqueShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtDeferredOpaque>();
            mMeshletsOpaqueShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtDeferredOpaqueMeshlets>();

            var linker = VisiblesPinIn.FindInLinker();
            if (linker != null)
            {
                CpuCullNode = linker.OutPin.HostNode as TtCpuCullingNode;
            }
            System.Diagnostics.Debug.Assert(CpuCullNode != null);
            linker = GpuCullPinIn.FindInLinker();
            if (linker != null)
            {
                GpuCullNode = linker.OutPin.HostNode as TtGpuCullingNode;
            }
        }
        public virtual unsafe TtGraphicsBuffers CreateGBuffers(TtRenderPolicy policy, EPixelFormat format)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var PassDesc = new NxRHI.FRenderPassDesc();
            PassDesc.NumOfMRT = 4;
            PassDesc.AttachmentMRTs[0].Format = format;
            PassDesc.AttachmentMRTs[0].Samples = 1;
            PassDesc.AttachmentMRTs[0].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[0].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[1].Format = Rt1PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[1].Samples = 1;
            PassDesc.AttachmentMRTs[1].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[1].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[2].Format = Rt2PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[2].Samples = 1;
            PassDesc.AttachmentMRTs[2].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[2].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.AttachmentMRTs[3].Format = Rt3PinOut.Attachement.Format;
            PassDesc.AttachmentMRTs[3].Samples = 1;
            PassDesc.AttachmentMRTs[3].LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.AttachmentMRTs[3].StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.Format = DepthStencilPinOut.Attachement.Format;
            PassDesc.m_AttachmentDepthStencil.Samples = 1;
            PassDesc.m_AttachmentDepthStencil.LoadAction = ClearMRT ? NxRHI.EFrameBufferLoadAction.LoadActionClear : NxRHI.EFrameBufferLoadAction.LoadActionDontCare;
            PassDesc.m_AttachmentDepthStencil.StoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            PassDesc.m_AttachmentDepthStencil.StencilLoadAction = NxRHI.EFrameBufferLoadAction.LoadActionClear;
            PassDesc.m_AttachmentDepthStencil.StencilStoreAction = NxRHI.EFrameBufferStoreAction.StoreActionStore;
            //PassDesc.mFBClearColorRT0 = new Color4f(1, 0, 0, 0);
            //PassDesc.mDepthClearValue = 1.0f;                
            //PassDesc.mStencilClearValue = 0u;
            RenderPass = TtEngine.Instance.GfxDevice.RenderPassManager.GetPipelineState<NxRHI.FRenderPassDesc>(rc, in PassDesc);

            GBuffers.Initialize(policy, RenderPass);
            GBuffers.SetRenderTarget(policy, 0, Rt0PinOut);
            GBuffers.SetRenderTarget(policy, 1, Rt1PinOut);
            GBuffers.SetRenderTarget(policy, 2, Rt2PinOut);
            GBuffers.SetRenderTarget(policy, 3, Rt3PinOut);
            GBuffers.SetDepthStencil(policy, DepthStencilPinOut);
            GBuffers.TargetViewIdentifier = policy.DefaultCamera.TargetViewIdentifier;

            return GBuffers;
        }
        public override void Dispose()
        {
            GBuffers?.Dispose();
            GBuffers = null;

            base.Dispose();
        }
        public override Shader.TtGraphicsShadingEnv GetPassShading(Mesh.TtRenderMesh.TtAtom atom)
        {
            if (EnableMeshlets && TtEngine.Instance.GfxDevice.RenderContext.DeviceCaps.IsSupportMeshShader && atom.MeshPrimitives.Meshlets != null)
            {
                return mMeshletsOpaqueShading;
            }
            else
            {
                return mOpaqueShading;
            }
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public bool EnableHDR { get; set; }
        public override void BeforeTick(TtRenderPolicy policy)
        {
            //if (DepthStencilPinOut.FindInLinker() == null)
            //{
            //    DepthStencilPinOut.Attachement.Format = EPixelFormat.PXF_D24_UNORM_S8_UINT;
            //    DepthStencilPinOut.IsAutoResize = true;
            //}

            if (EnableHDR == false)
            {
                if (Rt0PinOut.Attachement.Format != EPixelFormat.PXF_R8G8B8A8_UNORM)
                {
                    Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R8G8B8A8_UNORM);
                }
            }
            else
            {
                if (Rt0PinOut.Attachement.Format != EPixelFormat.PXF_R16G16B16A16_FLOAT)
                {
                    Rt0PinOut.Attachement.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                    this.CreateGBuffers(policy, EPixelFormat.PXF_R16G16B16A16_FLOAT);
                }
            }
        }

        [ThreadStatic]
        private static Profiler.TimeScope mScopePushGpuDraw;
        private static Profiler.TimeScope ScopePushGpuDraw
        {
            get
            {
                if (mScopePushGpuDraw == null)
                    mScopePushGpuDraw = new Profiler.TimeScope(typeof(TtDeferredBasePassNode), "PushGpuDraw");
                return mScopePushGpuDraw;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeFlushDraw;
        private static Profiler.TimeScope ScopeFlushDraw
        {
            get
            {
                if (mScopeFlushDraw == null)
                    mScopeFlushDraw = new Profiler.TimeScope(typeof(TtDeferredBasePassNode), "FlushDraw");
                return mScopeFlushDraw;
            }
        }
        NxRHI.TtCmdRecorder mBasePassRecorder = new NxRHI.TtCmdRecorder();
        NxRHI.TtCmdRecorder mBackgroundPassRecorder = new NxRHI.TtCmdRecorder();
        public unsafe override void Tick(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            mBasePassRecorder.ResetGpuDraws();
            mBackgroundPassRecorder.ResetGpuDraws();

            using (new NxRHI.TtCmdListScope(cmdlist, "DefferredBassPass"))
            {
                using (new Profiler.TimeScopeHelper(ScopePushGpuDraw))
                {
                    if (GpuCullNode != null)
                    {
                        GpuCullNode.Commit(policy, cmdlist, GBuffers);
                    }

                    var visibleMeshes = CpuCullNode.VisParameter.VisibleMeshes;
                    var camera = policy.DefaultCamera;//CpuCullNode.VisParameter.CullCamera;
                                                      //todo:ParrallelFor
                    foreach (var i in visibleMeshes)
                    {
                        if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                            continue;
                        foreach (var j in i.Mesh.SubMeshes)
                        {
                            foreach (var k in j.Atoms)
                            {
                                if (k == null || k.Material == null)
                                    continue;

                                var layer = k.Material.RenderLayer;
                                if (layer == ERenderLayer.RL_Background)
                                {
                                    var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                    if (drawcall != null)
                                    {
                                        //drawcall.BindGBuffer(camera, GBuffers);
                                        mBackgroundPassRecorder.PushGpuDraw(drawcall);
                                    }
                                }
                                else if (layer == ERenderLayer.RL_Opaque)
                                {
                                    if (i.DrawMode == FVisibleMesh.EDrawMode.Instance)
                                        continue;

                                    var drawcall = k.GetDrawCall(cmdlist.mCoreObject, GBuffers, policy, this);
                                    if (drawcall != null)
                                    {
                                        //drawcall.BindGBuffer(camera, GBuffers);
                                        mBasePassRecorder.PushGpuDraw(drawcall);
                                    }
                                }
                            }
                        }
                    }
                }

                var passClears = new NxRHI.FRenderPassClears();
                passClears.SetDefault();
                passClears.SetClearColor(0, new Color4f(1, 0, 0, 0));
                passClears.SetClearColor(1, new Color4f(1, 0, 0, 0));
                passClears.SetClearColor(2, new Color4f(1, 0, 0, 0));
                if (Rt3PinOut.Attachement.Format == EPixelFormat.PXF_R16G16_FLOAT)
                    passClears.SetClearColor(3, new Color4f(0, 0, 0, 0));
                else
                    passClears.SetClearColor(3, new Color4f(0, 0.5f, 0.5f, 0));

                GBuffers.BuildFrameBuffers(policy);

                using (new Profiler.TimeScopeHelper(ScopeFlushDraw))
                {
                    cmdlist.SetViewport(in GBuffers.Viewport);
                    var scissor = new NxRHI.FScissorRect();
                    scissor.MinX = 0;
                    scissor.MinY = 0;
                    scissor.MaxX = (int)GBuffers.Viewport.Width;
                    scissor.MaxY = (int)GBuffers.Viewport.Height;
                    cmdlist.SetScissor(in scissor);
                    cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, "RL_Background");
                    cmdlist.AppendDraws(mBackgroundPassRecorder);
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();

                    cmdlist.SetViewport(in GBuffers.Viewport);
                    cmdlist.SetScissor(in scissor);
                    passClears.ClearFlags = (NxRHI.ERenderPassClearFlags)0;
                    cmdlist.BeginPass(GBuffers.FrameBuffers, in passClears, "RL_Opaque");
                    cmdlist.AppendDraws(mBasePassRecorder);
                    cmdlist.FlushDraws();
                    cmdlist.EndPass();
                }
            }

            policy.CommitCommandList(cmdlist, "DefferredBassPass");

            mBasePassRecorder.ResetGpuDraws();
            mBackgroundPassRecorder.ResetGpuDraws();
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
        }
        
    }
}
