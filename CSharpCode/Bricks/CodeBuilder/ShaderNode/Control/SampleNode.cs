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
        [Rtti.Meta("")]
        [Category("Option")]
        public string TextureVarName { get; set; }
        RName mAssetName;
        [Rtti.Meta("")]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                return mAssetName;
            }
            set
            {
                mAssetName = value;
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());
                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta("")]
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

            Var.Texture2D.PreviewDraw(ref CmdParameters, mSlateEffect, TextureSRV, in prevStart, in prevEnd, cmdlist);
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
        [Rtti.Meta("")]
        [Category("Option")]
        public string TextureVarName { get; set; }

        RName mAssetName;
        [Rtti.Meta("")]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                return mAssetName;
            }
            set
            {
                mAssetName = value;
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());

                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta("")]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            Var.Texture2D.PreviewDraw(ref CmdParameters, mSlateEffect, TextureSRV, in prevStart, in prevEnd, cmdlist);
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
        [Rtti.Meta("")]
        [Category("Option")]
        public string TextureVarName { get; set; }
        RName mAssetName;
        [Rtti.Meta("")]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                return mAssetName;
            }
            set
            {
                mAssetName = value;
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());

                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta("")]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler { get => mSampler; set => mSampler = value; }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null || mSlateEffect == null)
                return;

            Var.Texture2D.PreviewDraw(ref CmdParameters, mSlateEffect, TextureSRV, in prevStart, in prevEnd, cmdlist);
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
        [Rtti.Meta("")]
        [Category("Option")]
        public string TextureVarName { get; set; }
        RName mAssetName;
        [Rtti.Meta("")]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                return mAssetName;
            }
            set
            {
                mAssetName = value;
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);

                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());
                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta("")]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler
        {
            get => mSampler;
            set => mSampler = value;
        }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            Var.Texture2D.PreviewDraw(ref CmdParameters, mSlateEffect, TextureSRV, in prevStart, in prevEnd, cmdlist);
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
        [Rtti.Meta("")]
        [Category("Option")]
        public string TextureVarName { get; set; }
        RName mAssetName;
        [Rtti.Meta("")]
        [Category("Option")]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName AssetName
        {
            get
            {
                return mAssetName;
            }
            set
            {
                mAssetName = value;
                if (value == null)
                {
                    TextureSRV = null;
                    return;
                }
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                    mSlateEffect = await TtEngine.Instance.GfxDevice.EffectManager.GetGraphicEffect(
                        await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>(),
                        TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial, new Graphics.Mesh.TtMdfStaticMesh());
                };
                exec();
            }
        }
        TtEffect mSlateEffect;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        NxRHI.FSamplerDesc mSampler;
        [Rtti.Meta("")]
        [Category("Option")]
        public NxRHI.FSamplerDesc Sampler
        {
            get => mSampler;
            set => mSampler = value;
        }
        private NxRHI.TtSrView TextureSRV;
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            Var.Texture2D.PreviewDraw(ref CmdParameters, mSlateEffect, TextureSRV, in prevStart, in prevEnd, cmdlist);
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
