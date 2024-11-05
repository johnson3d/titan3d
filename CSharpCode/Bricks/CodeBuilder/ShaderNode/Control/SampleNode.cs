using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Control
{
    public class SampleLevel2DNode : CallNode
    {
        public SampleLevel2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"Texture_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~SampleLevel2DNode()
        {
            CoreSDK.DisposeObject(ref CmdParameters);
        }
        [Rtti.Meta]
        [Category("Option")]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());
                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler
        {
            get => mSampler;
            set => mSampler = value;
        }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            unsafe
            {
                if (CmdParameters == null)
                {
                    var rc = TtEngine.Instance.GfxDevice.RenderContext;

                    var iptDesc = new NxRHI.TtInputLayoutDesc();
                    unsafe
                    {
                        iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                        iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                        iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                        //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                    }
                    iptDesc.mCoreObject.SetShaderDesc(mSlateEffect.DescVS.mCoreObject);
                    var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                    mSlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                    var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                    var cbBinder = mSlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                    cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                    cmdParams.Drawcall.BindShaderEffect(mSlateEffect);
                    cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                    cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
                    cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                    cmdParams.IsNormalMap = 0;
                    if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                        cmdParams.IsNormalMap = 1;

                    CmdParameters = cmdParams;
                }

                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);
                cmdlist.AddImage((ulong)CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }

        }
        
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }

    public class Sample2DNode : CallNode
    {
        public Sample2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"Texture_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~Sample2DNode()
        {
            CoreSDK.DisposeObject(ref CmdParameters);
        }
        [Rtti.Meta]
        [Category("Option")]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());

                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            unsafe
            {
                if (CmdParameters == null)
                {
                    var rc = TtEngine.Instance.GfxDevice.RenderContext;

                    var iptDesc = new NxRHI.TtInputLayoutDesc();
                    unsafe
                    {
                        iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                        iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                        iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                        //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                    }
                    iptDesc.mCoreObject.SetShaderDesc(mSlateEffect.DescVS.mCoreObject);
                    var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                    mSlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                    var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                    var cbBinder = mSlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                    cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                    cmdParams.Drawcall.BindShaderEffect(mSlateEffect);
                    cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                    cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
                    cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                    cmdParams.IsNormalMap = 0;
                    if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                        cmdParams.IsNormalMap = 1;

                    CmdParameters = cmdParams;
                }

                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);
                cmdlist.AddImage((ulong)CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);

                // support preview A channel
                //var textPos = end - new Vector2(32, 32);
                //cmdlist.AddText(textPos, mShowA ? 0xFFFFFFFF : 0x00FF00FF, "A", null);
                //if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false) && ImGuiAPI.IsMouseHoveringRect(textPos, end, true))
                //{
                //    CmdParameters.ColorMask.W = mShowA ? 1 : 0;
                //    mShowA = !mShowA;
                //}
            }
        }
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                //var t method.FindParameter("texture");
                var texPin = data.GraphHostNode.FindPinIn("texture");
                if (data.NodeGraph.PinHasLinker(texPin))
                {
                    var retVal = new TtVariableReferenceExpression()
                    {
                        VariableName = "DefaultSampLinear"
                    };
                    return retVal;
                }
                else
                {
                    var retVal = new TtVariableReferenceExpression()
                    {
                        VariableName = "Samp_" + TextureVarName
                    };
                    return retVal;
                }
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var hostNodeStore = data.GraphHostNode;
            data.GraphHostNode = this;
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
            data.GraphHostNode = hostNodeStore;
        }
    }

    public class Sample2DBiasNode : CallNode
    {
        public Sample2DBiasNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"TextureBias_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~Sample2DBiasNode()
        {
            CoreSDK.DisposeObject(ref CmdParameters);
        }
        [Rtti.Meta]
        [Category("Option")]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());

                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            if (CmdParameters == null)
            {
                var rc = TtEngine.Instance.GfxDevice.RenderContext;

                var iptDesc = new NxRHI.TtInputLayoutDesc();
                unsafe
                {
                    iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                    iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                    iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                    //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                }
                iptDesc.mCoreObject.SetShaderDesc(mSlateEffect.DescVS.mCoreObject);
                var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                mSlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                var cbBinder = mSlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                cmdParams.Drawcall.BindShaderEffect(mSlateEffect);
                cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, TextureSRV);
                cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                cmdParams.IsNormalMap = 0;
                if (TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || TextureSRV.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                    cmdParams.IsNormalMap = 1;

                CmdParameters = cmdParams;
            }

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            cmdlist.AddImage((ulong)CmdParameters.GetHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
        }
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "bias")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "0"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }

    public class SampleArrayLevel2DNode : CallNode
    {
        public SampleArrayLevel2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"TextureArray_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~SampleArrayLevel2DNode()
        {
        }
        [Rtti.Meta]
        [Category("Option")]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage((ulong)TextureSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }

    public class SampleArray2DNode : CallNode
    {
        public SampleArray2DNode()
        {
            PrevSize = new Vector2(100, 100);
            TextureVarName = $"TextureArray_{(uint)NodeId.GetHashCode()}";

            mSampler.SetDefault();
        }
        ~SampleArray2DNode()
        {
        }
        [Rtti.Meta]
        [Category("Option")]
        public string TextureVarName { get; set; }
        [Rtti.Meta]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                if (TextureSRV == null)
                    return null;
                return TextureSRV.AssetName;
            }
            set
            {
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage((ulong)TextureSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        protected override TtExpressionBase GetNoneLinkedParameterExp(NodeGraph.PinIn pin, int argIdx, ref NodeGraph.BuildCodeStatementsData data)
        {
            var method = Method;
            if (method.Parameters[argIdx].Name == "texture")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "sampler")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "Samp_" + TextureVarName
                };
                return retVal;
            }
            else if (method.Parameters[argIdx].Name == "uv")
            {
                var retVal = new TtVariableReferenceExpression()
                {
                    VariableName = "input.vUV"
                };
                return retVal;
            }
            return base.GetNoneLinkedParameterExp(pin, argIdx, ref data);
        }
        public override void BuildStatements(NodePin pin, ref NodeGraph.BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            var texturePinIn = FindPinIn("texture");
            if (texturePinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameRNamePair();
                tmp.Name = TextureVarName;
                tmp.ShaderType = "Texture2D";
                if (material.FindSRV(tmp.Name) == null)
                {
                    tmp.Value = AssetName;
                    material.UsedSrView.Add(tmp);
                }
            }
            var samplerPinIn = FindPinIn("sampler");
            if (samplerPinIn.HasLinker() == false)
            {
                var tmp = new Graphics.Pipeline.Shader.TtMaterial.NameSamplerStateDescPair();
                tmp.Name = "Samp_" + TextureVarName;
                if (material.FindSampler(tmp.Name) == null)
                {
                    tmp.Value = Sampler;
                    material.UsedSamplerStates.Add(tmp);
                }
            }
            base.BuildStatements(pin, ref data);
        }
    }
}
