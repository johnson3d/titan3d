using EngineNS.Animation.Macross.BlendTree;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline.Deferred;
using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public class TtShaderCodeViewerAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
    {
        bool mPopupOpen = false;
        string mPopupId;
        EGui.TtCodeEditor mCodeEditor;
        string mCachedCode;

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            var code = info.Value as string ?? "";
            var preview = code.Length > 60 ? code.Substring(0, 60) + "..." : code;
            if (string.IsNullOrEmpty(preview))
                preview = "(empty)";

            var buttonSize = new Vector2(-1, 0);
            if (ImGuiAPI.Button($"{preview}##{info.Name}_btn", in buttonSize))
            {
                mPopupId = $"ShaderCode: {info.Name}##popup_{info.Name}";
                mCachedCode = code;
                if (mCodeEditor == null)
                {
                    mCodeEditor = new EGui.TtCodeEditor();
                    mCodeEditor.SetLanguage("HLSL");
                    mCodeEditor.SetReadOnly(true);
                }
                mCodeEditor.SetText(mCachedCode);
                mPopupOpen = true;
                ImGuiAPI.OpenPopup(mPopupId, ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }

            if (mPopupOpen)
            {
                var popupSize = new Vector2(900, 600);
                ImGuiAPI.SetNextWindowSize(in popupSize, ImGuiCond_.ImGuiCond_Appearing);
                if (ImGuiAPI.BeginPopupModal(mPopupId, ref mPopupOpen, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var contentSize = ImGuiAPI.GetContentRegionAvail();
                    var editorSize = new Vector2(contentSize.X, contentSize.Y - 30);
                    mCodeEditor.Render("##code_viewer", in editorSize, true);

                    var closeSize = new Vector2(120, 0);
                    if (ImGuiAPI.Button("Close", in closeSize))
                    {
                        mPopupOpen = false;
                        ImGuiAPI.CloseCurrentPopup();
                    }
                    ImGuiAPI.EndPopup();
                }
            }
            return false;
        }
    }

    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterialAMeta@EngineCore" })]
    public partial class TtMaterialAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtMaterial.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Material";
        }
        public override async Thread.Async.TtTask<IO.IAsset> LoadAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(GetAssetName());
        }
        public override async Thread.Async.TtTask<IO.IAsset> CreateAsset(params object[] args)
        {
            return await TtEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MaterialBoderColor;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "Mtl", null);
        //}
    }
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial@EngineCore" })]
    [TtMaterial.MaterialImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/Material")]
    [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtMaterial : IO.BaseSerializer, IO.IAsset, IShaderCodeProvider
    {
        public const string AssetExt = ".material";
        public string TypeExt { get => AssetExt; }
        public TtMaterial()
        {
            mPipelineDesc.SetDefault();
        }
        public bool IsEditingMaterial { get; set; }

        public override string ToString()
        {
            string result = $"Materai:{AssetName}\n";
            return result;
        }
        protected Hash160 mMaterialHash;
        [Rtti.Meta("")]
        [Browsable(false)]
        public virtual Hash160 MaterialHash
        {
            get
            {
                return mMaterialHash;
            }
            set
            {
                mMaterialHash = value;
            }
        }
        public Hash160 GetHash()
        {
            string result = DefineCode?.TextCode;
            result += SourceCode?.TextCode;
            mMaterialHash = Hash160.CreateHash160(result);
            return mMaterialHash;
        }
        internal uint mSerialId = 0;
        [Browsable(false)]
        public virtual uint SerialId
        {
            get => mSerialId;
            set
            {
                mSerialId = value;
                if (PerMaterialCBuffer != null)
                    this.UpdateCBufferVars(PerMaterialCBuffer, PerMaterialCBuffer.ShaderBinder);
            }
        }
        public class MaterialImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                await base.DoCreate(dir, type, ext);

                var material = (mAsset as TtMaterial);
                material.mPipelineDesc.SetDefault();
                material.UpdateShaderCode(true);
            }
        }
        #region IAsset
        public virtual IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMaterialAMeta();
            return result;
        }
        public virtual IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta, Bricks.CodeBuilder.ShaderNode.TtMaterialGraph MaterialGraph)
        {
            ameta.RefAssetRNames.Clear();
            foreach (var i in UsedSrView)
            {
                if (i.Value == null)
                    continue;
                ameta.AddReferenceAsset(i.Value);
            }
            foreach (var i in MaterialGraph.Nodes)
            {
                var f = i as Bricks.CodeBuilder.ShaderNode.TtShadeBaseNode;
                if (f == null)
                    continue;
                f.UpdateAMetaReferences(ameta, MaterialGraph);
            }
        }
        [Rtti.Meta("")]
        public virtual void SaveAssetTo(RName name)
        {
            var MaterialGraph = new Bricks.CodeBuilder.ShaderNode.TtMaterialGraph();
            var xml = IO.TtFileManager.LoadXmlFromString(this.GraphXMLString);
            if (xml != null)
            {
                var node = xml.LastChild as System.Xml.XmlElement;
                var thisTypeStr = node.GetAttribute("Type");
                var typeDesc = Rtti.TtTypeDesc.TypeOf(thisTypeStr);
                if (typeDesc == Rtti.TtTypeDescGetter<Bricks.CodeBuilder.ShaderNode.TtMaterialEditor>.TypeDesc)
                {
                    System.Diagnostics.Debug.Assert(false);
                    object pThis = new Bricks.CodeBuilder.ShaderNode.TtMaterialEditor();
                    IO.SerializerHelper.ReadObjectMetaFields(this, node, ref pThis, null);
                    MaterialGraph = (pThis as Bricks.CodeBuilder.ShaderNode.TtMaterialEditor).MaterialGraph;
                    {   
                        var xml2 = new System.Xml.XmlDocument();
                        var xmlRoot2 = xml2.CreateElement($"Root", xml2.NamespaceURI);
                        xml2.AppendChild(xmlRoot2);
                        IO.SerializerHelper.WriteObjectMetaFields(xml2, xmlRoot2, MaterialGraph);
                        var xmlText = IO.TtFileManager.GetXmlText(xml2);
                        this.GraphXMLString = xmlText;
                    }
                }
                else
                {
                    object pThis = MaterialGraph;
                    IO.SerializerHelper.ReadObjectMetaFields(this, node, ref pThis, null);
                }
            }
            
            string code = "";
            var MaterialOutput = MaterialGraph.FindFirstTypedNode<Bricks.CodeBuilder.ShaderNode.TtMaterialOutput>("Output", false);
            if (MaterialOutput == null)
            {
                MaterialOutput = Bricks.CodeBuilder.ShaderNode.TtMaterialOutput.NewNode(MaterialGraph);
                MaterialGraph.AddNode(MaterialOutput);
            }
            GenMateralGraphCode(ref code, this, new UHLSLCodeGenerator(), MaterialGraph, MaterialOutput);

            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta, MaterialGraph);
                ameta.SaveAMeta(this);
            }

            var typeStr = Rtti.TtTypeDesc.TypeOf(this.GetType()).TypeString;
            using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
            {
                using (var attr = xnd.NewAttribute("Material", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(this);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }

                xnd.SaveXnd(name.Address);
            }
            this.SerialId++;
            name.AMeta?.AddAssetFile(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        public static bool ReloadXnd(TtMaterial material, TtMaterialManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("Material");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(material, null);
                        material.UpdateShaderCode(false);
                        material.SerialId++;
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
            return true;
        }
        [Rtti.Meta("")]
        [RName.PGRName(ReadOnly = true)]
        [Category("Option")]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        [Browsable(false)]
        public virtual TtMaterial ParentMaterial
        {
            get { return this; }
            protected set { }
        }
        public virtual void GetDefines(List<KeyValuePair<string, string>> vars)
        {
        }
        public static TtMaterial LoadXnd(TtMaterialManager manager, IO.TtXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("Material");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out result, null);
                }
            }

            var material = result as TtMaterial;
            if (material != null)
            {
                material.UpdateShaderCode(false);
                return material;
            }
            return null;
        }
        public static void GenMateralGraphCode(ref string code, TtMaterial Material, UHLSLCodeGenerator mHLSLCodeGen, 
            Bricks.CodeBuilder.ShaderNode.TtMaterialGraph MaterialGraph, 
            Bricks.CodeBuilder.ShaderNode.TtMaterialOutput MaterialOutput)
        {
            foreach (var i in MaterialGraph.Nodes)
            {
                var f = i as Bricks.CodeBuilder.ShaderNode.Control.TtCallMaterialFunctionNode;
                if (f != null)
                {
                    f.MaterialFunction.WriteRefHLSLCode(ref code);
                }
            }

            Material.UsedSrView.Clear();
            Material.UsedUniformVars.Clear();
            Material.UsedSamplerStates.Clear();

            try
            {
                var MaterialClass = new TtClassDeclaration();

                var gen = mHLSLCodeGen.GetCodeObjectGen(Rtti.TtTypeDescGetter<TtMethodDeclaration>.TypeDesc);
                BuildCodeStatementsData data = new BuildCodeStatementsData()
                {
                    ClassDec = MaterialClass,
                    NodeGraph = MaterialGraph,
                    UserData = Material,
                    CodeGen = mHLSLCodeGen,
                };
                MaterialOutput.BuildStatements(null, ref data);
                var incGen = mHLSLCodeGen.GetCodeObjectGen(Rtti.TtTypeDescGetter<TtIncludeDeclaration>.TypeDesc);
                TtCodeGeneratorData genData = new TtCodeGeneratorData()
                {
                    Method = null,
                    CodeGen = mHLSLCodeGen,
                    UserData = Material,
                };
                Material.IncludeFiles.Clear();
                foreach (var i in MaterialClass.PreIncludeHeads)
                {
                    incGen.GenCodes(i, ref code, ref genData);
                    Material.IncludeFiles.Add(i.FilePath);
                }
                genData = new TtCodeGeneratorData()
                {
                    Method = MaterialOutput.VSFunction,
                    CodeGen = mHLSLCodeGen,
                    UserData = Material,
                };
                gen.GenCodes(MaterialOutput.VSFunction, ref code, ref genData);
                genData = new TtCodeGeneratorData()
                {
                    Method = MaterialOutput.PSFunction,
                    CodeGen = mHLSLCodeGen,
                    UserData = Material,
                };
                gen.GenCodes(MaterialOutput.PSFunction, ref code, ref genData);

                Material.HLSLCode = code;
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            Material.VSNeedStreams = MaterialOutput.GetVSNeedStreams();
            Material.PSNeedInputs = MaterialOutput.GetPSNeedInputs();

            if (Material.NormalMode == Graphics.Pipeline.Shader.TtMaterial.ENormalMode.NormalMap)
            {
                if (Material.VSNeedStreams.Contains(NxRHI.EVertexStreamType.VST_Normal) == false)
                    Material.VSNeedStreams.Add(NxRHI.EVertexStreamType.VST_Normal);
                if (Material.VSNeedStreams.Contains(NxRHI.EVertexStreamType.VST_Tangent) == false)
                    Material.VSNeedStreams.Add(NxRHI.EVertexStreamType.VST_Tangent);

                if (Material.PSNeedInputs.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal) == false)
                    Material.PSNeedInputs.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal);
                if (Material.PSNeedInputs.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent) == false)
                    Material.PSNeedInputs.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent);
            }
            else if (Material.NormalMode == Graphics.Pipeline.Shader.TtMaterial.ENormalMode.Normal)
            {
                if (Material.VSNeedStreams.Contains(NxRHI.EVertexStreamType.VST_Normal) == false)
                    Material.VSNeedStreams.Add(NxRHI.EVertexStreamType.VST_Normal);
                if (Material.PSNeedInputs.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal) == false)
                    Material.PSNeedInputs.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal);
            }
            Material.UpdateShaderCode(false);
        }
        [Flags]
        public enum InnerFlags : UInt32
        {
            None = 0,
            Is64bitVColorAlpha = 1 << 0,
        }
        [Rtti.Meta, Browsable(false)]
        public InnerFlags Flags { get; set; }

        #region Render Option
        [Category("Option")]
        public virtual bool Is64bitVColorAlpha
        { 
            get => (Flags & InnerFlags.Is64bitVColorAlpha) != 0;
            set
            {
                if (value)
                    Flags |= InnerFlags.Is64bitVColorAlpha;
                else
                    Flags &= ~InnerFlags.Is64bitVColorAlpha;
            }
        }
        [Editor.ShaderCompiler.TtShaderDefine(ShaderName = "ELightingMode")]
        public enum ELightingMode : uint
        {
            Stand = 0,
            Unlight,
            Skin,
            Transmit,
            Hair,
            Eye,
            Num,
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public virtual ELightingMode LightingMode
        {
            get;
            set;
        } = ELightingMode.Stand;
        public enum ENormalMode
        {
            Normal,
            NormalMap,
            NormalMapRGB,
            NormalNone,
        }
        ENormalMode mNormalMode = ENormalMode.NormalMap;
        [Rtti.Meta("")]
        [Category("Option")]
        public virtual ENormalMode NormalMode
        {
            get => mNormalMode;
            set
            {
                mNormalMode = value;
            }
        }
        protected ERenderLayer mRenderLayer = ERenderLayer.RL_Opaque;
        [Rtti.Meta("")]
        [Category("Option")]
        public virtual unsafe ERenderLayer RenderLayer
        {
            get => mRenderLayer;
            set
            {
                mRenderLayer = value;
                if (mRenderLayer == ERenderLayer.RL_Translucent ||
                    mRenderLayer == ERenderLayer.RL_PostTranslucent ||
                    mRenderLayer == ERenderLayer.RL_TranslucentGizmos)
                {
                    mPipelineDesc.m_Blend.RenderTarget[0].BlendEnable = 1;
                }
                else
                {
                    mPipelineDesc.m_Blend.RenderTarget[0].BlendEnable = 0;
                }
                SerialId++;
            }
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public virtual bool AlphaTest
        {
            get;
            set;
        } = true;

        [Flags]
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "ERenderFlags")]
        public enum ERenderFlags : uint
        {
            None = 0,
            DisableEnvColor = 1,
            // bit1~2: per-mesh object flags (merged into GBuffer at encode time)
            AcceptShadow = (1 << 1),
            UnLight = (1 << 2),
            // bit3~5 reserved for future flags
            // bit6~9: ShadingMode (4 bits, use EShadingMode enum)
            ShadingModeMask = 0x03C0, // (0xF << 6)
            ShadingModeMaskShift = 6,
        }

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EShadingMode")]
        public enum EShadingMode : uint
        {
            PBR = 0,
            Subsurface = 1,
            Hair = 2,
        }

        private const int ShadingModeBitOffset = 6;

        private ERenderFlags mRenderFlags = ERenderFlags.None;
        [Rtti.Meta("")]
        [Category("Option")]
        public virtual ERenderFlags RenderFlags { get => mRenderFlags; }
        [Category("Option")]
        public virtual bool DisableEnvColor
        {
            get
            {
                return (mRenderFlags & ERenderFlags.DisableEnvColor) != 0;
            }
            set
            {
                if (value)
                    mRenderFlags |= ERenderFlags.DisableEnvColor;
                else
                    mRenderFlags &= (~ERenderFlags.DisableEnvColor);
            }
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public virtual EShadingMode ShadingMode
        {
            get
            {
                return (EShadingMode)(((uint)mRenderFlags & (uint)ERenderFlags.ShadingModeMask) >> ShadingModeBitOffset);
            }
            set
            {
                mRenderFlags = (ERenderFlags)(((uint)mRenderFlags & ~(uint)ERenderFlags.ShadingModeMask)
                    | (((uint)value << ShadingModeBitOffset) & (uint)ERenderFlags.ShadingModeMask));
            }
        }
        #endregion

        [Rtti.Meta("")]
        public List<string> UserDefines { get; set; } = new List<string>();
        internal virtual void UpdateShaderCode(bool EmptyMaterial)
        {
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";
            codeBuilder.AddLine($"#ifndef _Material_H_", ref sourceCode);
            codeBuilder.AddLine($"#define _Material_H_", ref sourceCode);

            foreach (var i in this.UsedSrView)
            {
                codeBuilder.AddLine($"{i.ShaderType} {i.Name} DX_AUTOBIND;", ref sourceCode);
            }

            foreach (var i in this.UsedSamplerStates)
            {
                codeBuilder.AddLine($"SamplerState {i.Name} DX_AUTOBIND;", ref sourceCode);
            }

            //Defines.AddDefine("USE_VS_UV", "1");
            //Defines.AddDefine("USE_VS_Color", "1");
            foreach (var i in IncludeFiles)
            {
                var incCode = EngineNS.Editor.ShaderCompiler.TtHLSLCompiler.GetIncludeCode(i);
                if (incCode.IsValidPointer)
                {
                    var t = UniHash32.XXHash(incCode.GetSourceCode());
                    sourceCode += $"//{i}:{t}\r\n";
                }
            }

            if (EmptyMaterial)
            {
                this.HLSLCode = "void DO_VS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl)\n{\n}\n";
                this.HLSLCode += "void DO_PS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl)\n" +
                    "{\n" +
                        "mtl.mAlbedo = float3(0.5,0.5,0.5);\n" +
                        "mtl.mMetallic = 1.0f;\n" +
                        "mtl.mRough = 0.5f;\n" +
                        "mtl.mEmissive = float3(0.1,0.1,0.1);\n" +
                    "}\n";
                //codeBuilder.AddLine("void DO_PS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl)");
                //codeBuilder.PushBrackets();
                //codeBuilder.AddLine("mtl.mAlbedo = float3(0.5,0.5,0.5);");
                //codeBuilder.AddLine("mtl.mMetallic = 1.0f;");
                //codeBuilder.AddLine("mtl.mRough = 0.5f;");
                //codeBuilder.AddLine("mtl.mEmissive = float3(0.1,0.1,0.1);");
                //codeBuilder.PopBrackets();
                sourceCode += this.HLSLCode;
                //codeBuilder.AppendCode(this.HLSLCode, false, true);
            }
            else
            {
                sourceCode += this.HLSLCode;
                //codeBuilder.AppendCode(this.HLSLCode, false, true);
                if (sourceCode.IndexOf("DO_VS_MATERIAL_IMPL") < 0)
                {
                    codeBuilder.AddLine("void DO_VS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl)", ref sourceCode);
                    codeBuilder.PushSegment(ref sourceCode);
                    codeBuilder.PopSegment(ref sourceCode);
                }
            }

            codeBuilder.AddLine("#undef DO_VS_MATERIAL", ref sourceCode);
            codeBuilder.AddLine("#define DO_VS_MATERIAL DO_VS_MATERIAL_IMPL", ref sourceCode);
            codeBuilder.AddLine("#undef DO_PS_MATERIAL", ref sourceCode);
            codeBuilder.AddLine("#define DO_PS_MATERIAL DO_PS_MATERIAL_IMPL", ref sourceCode);

            if (Is64bitVColorAlpha)
                codeBuilder.AddLine("#define MTL_ID_64BITVCOLORALPHA 1", ref sourceCode);
            else
                codeBuilder.AddLine("#define MTL_ID_64BITVCOLORALPHA 0", ref sourceCode);

            codeBuilder.AddLine($"#define MTL_RENDERFLAGS {(uint)mRenderFlags}", ref sourceCode);

            codeBuilder.AddLine($"#define MTL_LightingMode {nameof(ELightingMode)}_{System.Enum.GetName(LightingMode)}", ref sourceCode);
            switch (NormalMode)
            {
                case ENormalMode.Normal:
                    codeBuilder.AddLine("#define MTL_NORMAL_MODE MTL_NORMAL", ref sourceCode);
                    break;
                case ENormalMode.NormalMap:
                    codeBuilder.AddLine("#define MTL_NORMAL_MODE MTL_NORMALMAP", ref sourceCode);
                    break;
                case ENormalMode.NormalMapRGB:
                    codeBuilder.AddLine("#define MTL_NORMAL_MODE MTL_NORMALMAP_RGB", ref sourceCode);
                    break;
                case ENormalMode.NormalNone:
                default:
                    codeBuilder.AddLine("#define MTL_NORMAL_MODE MTL_NORMALNONE", ref sourceCode);
                    break;
            }


            if (AlphaTest)
            {
                codeBuilder.AddLine("#define ALPHA_TEST", ref sourceCode);
            }

            if (UserDefines != null)
            {
                foreach (var i in UserDefines)
                {
                    codeBuilder.AddLine($"#define {i}", ref sourceCode);
                }
            }

            codeBuilder.AddLine("#endif//_Material_H_", ref sourceCode);
            if (SourceCode.TextCode.ToString() != sourceCode)
            {
                this.SerialId++;
                SourceCode.TextCode = sourceCode;
            }

            string uniformVarsCode = "";
            foreach (var i in this.UsedUniformVars)
            {
                if (i.VarType == "float4")
                {
                    uniformVarsCode += $"{i.VarType} {i.Name} = float4({i.Value});";
                }
                else if(i.VarType == "float3")
                {
                    uniformVarsCode += $"{i.VarType} {i.Name} = float3({i.Value});";
                }
                else if (i.VarType == "float2")
                {
                    uniformVarsCode += $"{i.VarType} {i.Name} = float2({i.Value});";
                }
                else if (i.VarType == "float1" || i.VarType == "float")
                {
                    uniformVarsCode += $"{i.VarType} {i.Name} = {i.Value};";
                }
            }

            if (DefineCode.TextCode != uniformVarsCode)
            {
                this.SerialId++;
                DefineCode.TextCode = uniformVarsCode;
            }

            mPerMaterialCBuffer = null;

            mMaterialHash = GetHash();
        }
        public NxRHI.TtShaderCode DefineCode { get; } = new NxRHI.TtShaderCode();
        public NxRHI.TtShaderCode SourceCode { get; } = new NxRHI.TtShaderCode();
        [TtShaderCodeViewer]
        [Category("Option")]
        public string DefineCodeText
        {
            get
            {
                return DefineCode.TextCode;
            }
        }
        [TtShaderCodeViewer]
        [Category("Option")]
        public string SourceCodeText
        {
            get
            {
                return SourceCode.TextCode;
            }
        }
        public NxRHI.TtShaderDefinitions Defines { get; } = new NxRHI.TtShaderDefinitions();

        public EngineNS.NxRHI.EVertexStreamType[] GetVSNeedStreams()
        {
            if(VSNeedStreams == null)
            {
                return new EngineNS.NxRHI.EVertexStreamType[] {
                    EngineNS.NxRHI.EVertexStreamType.VST_Position,
                    EngineNS.NxRHI.EVertexStreamType.VST_Normal,
                    EngineNS.NxRHI.EVertexStreamType.VST_UV,
                };
            }
            else
            {
                var result = new List<NxRHI.EVertexStreamType>(VSNeedStreams);
                if (mNormalMode == ENormalMode.NormalMap)
                {
                    if (result.Contains(NxRHI.EVertexStreamType.VST_Normal) == false)
                        result.Add(NxRHI.EVertexStreamType.VST_Normal);
                    if (result.Contains(NxRHI.EVertexStreamType.VST_Tangent) == false)
                        result.Add(NxRHI.EVertexStreamType.VST_Tangent);
                }
                else if (mNormalMode == ENormalMode.Normal)
                {
                    if (result.Contains(NxRHI.EVertexStreamType.VST_Normal) == false)
                        result.Add(NxRHI.EVertexStreamType.VST_Normal);
                }
                return result.ToArray();
            }
        }
        public EPixelShaderInput[] GetPSNeedInputs()
        {
            if (PSNeedInputs == null)
            {
                return new EPixelShaderInput[] {
                    EPixelShaderInput.PST_Position,
                    EPixelShaderInput.PST_Normal,
                    EPixelShaderInput.PST_Color,
                    EPixelShaderInput.PST_UV,
                    EPixelShaderInput.PST_WorldPos,
                    EPixelShaderInput.PST_Tangent,
                    EPixelShaderInput.PST_LightMap,
                    EPixelShaderInput.PST_Custom0,
                    EPixelShaderInput.PST_Custom1,
                    EPixelShaderInput.PST_Custom2,
                    EPixelShaderInput.PST_Custom3,
                    EPixelShaderInput.PST_Custom4,
                    EPixelShaderInput.PST_F4_1,
                    EPixelShaderInput.PST_F4_2,
                    EPixelShaderInput.PST_F4_3,
                    EPixelShaderInput.PST_SpecialData,
                    EPixelShaderInput.PST_InstanceID,
                 };
            }
            else
            {
                var result = new List<Graphics.Pipeline.Shader.EPixelShaderInput>(PSNeedInputs);
                if (mNormalMode == ENormalMode.NormalMap)
                {
                    if (result.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal) == false)
                        result.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal);
                    if (result.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent) == false)
                        result.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent);
                }
                else if (mNormalMode == ENormalMode.Normal)
                {
                    if (result.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal) == false)
                        result.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal);
                }
                return result.ToArray();
            }
        }

        #region Data

        #region Code&Graph
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        [Browsable(false)]
        public string GraphXMLString
        {
            get;
            set;
        }
        string mHLSLCode;
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        [Browsable(false)]
        public string HLSLCode
        {
            get => mHLSLCode;
            set
            {
                mHLSLCode = value;
                MaterialHash = GetHash();
            }
        }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        public List<string> IncludeFiles { get; set; } = new List<string>();
        [Rtti.Meta("")]
        public List<EngineNS.NxRHI.EVertexStreamType> VSNeedStreams
        {
            get;
            set;
        } = null;        
        [Rtti.Meta("")]
        public List<Graphics.Pipeline.Shader.EPixelShaderInput> PSNeedInputs 
        { 
            get; 
            set;
        } = null;
        #endregion
        #region Texture
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial.NameRNamePair@EngineCore" })]
        [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        public class NameRNamePair : IO.BaseSerializer
        {
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as TtMaterial;
            }
            TtMaterial HostMaterial;
            [Rtti.Meta("")]
            [Category("Option")]
            [ReadOnly(true)]
            public string Name { get; set; }
            RName mValue;
            [Rtti.Meta("")]
            [Category("Option")]
            [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
            public RName Value
            {
                get => mValue;
                set
                {
                    mValue = value;
                    if (HostMaterial != null)
                        HostMaterial.SerialId++;
                }
            }
            [Rtti.Meta("")]
            public string ShaderType { get; set; } = "Texture2D";
            /// <summary>
            /// 是否为动态 SRV（运行时由 TtDynamicSrvRegistry 提供，而非静态纹理资产）
            /// </summary>
            [Rtti.Meta("")]
            [Category("Option")]
            public bool IsDynamic { get; set; } = false;
            /// <summary>
            /// 动态 SRV 在 TtDynamicSrvRegistry 中的注册名（仅 IsDynamic=true 时有效）
            /// </summary>
            [Rtti.Meta("")]
            [Category("Option")]
            public string DynamicSrvName { get; set; }
            public NameRNamePair Clone(TtMaterial mtl)
            {
                var result = new NameRNamePair();
                result.HostMaterial = mtl;
                result.Name = Name;
                result.mValue = mValue;
                result.IsDynamic = IsDynamic;
                result.DynamicSrvName = DynamicSrvName;
                return result;
            }
            public object SrvObject { get; set; } = null;
        }
        protected List<NameRNamePair> mUsedSrView = new List<NameRNamePair>();
        [Rtti.Meta("",NameAlias = new string[] { "UsedRSView" })]
        [Category("Variable")]
        public List<NameRNamePair> UsedSrView { get => mUsedSrView; }
        public NameRNamePair FindSRV(string name)
        {
            foreach (var i in mUsedSrView)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public int NumOfSRV
        {
            get
            {
                return UsedSrView.Count;
            }
        }
        public string GetNameOfSRV(int index)
        {
            if (index < 0 || index >= UsedSrView.Count)
                return null;
            return UsedSrView[index]?.Name;
        }
        public virtual async Thread.Async.TtTask<NxRHI.TtSrView> GetSRV(int index)
        {
            var entry = UsedSrView[index];
            // 动态 SRV: 每帧从注册表实时查找（不缓存，因为 ping-pong 纹理会变）
            if (entry.IsDynamic)
                return TtEngine.Instance.GfxDevice.DynamicSrvRegistry.Find(entry.DynamicSrvName);
            var srv = entry.SrvObject as NxRHI.TtSrView;
            if (srv != null)
                return srv;
            if (entry.Value == null)
                return null;
            entry.SrvObject = await entry.Value.GetAsset<NxRHI.TtSrView>();
            return entry.SrvObject as NxRHI.TtSrView;
        }
        public NxRHI.TtSrView TryGetSRV(int index)
        {
            var entry = UsedSrView[index];
            // 动态 SRV: 每帧从注册表实时查找
            if (entry.IsDynamic)
                return TtEngine.Instance.GfxDevice.DynamicSrvRegistry.Find(entry.DynamicSrvName);
            var srv = entry.SrvObject as NxRHI.TtSrView;
            if (srv != null)
                return srv;
            entry.SrvObject = TtEngine.Instance.GfxDevice.TextureManager.TryGetTexture(entry.Value);
            return entry.SrvObject as NxRHI.TtSrView;
        }
        #endregion
        #region Sampler
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair@EngineCore" })]
        [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        public class NameSamplerStateDescPair : IO.BaseSerializer
        {
            public NameSamplerStateDescPair()
            {
                mValue.SetDefault();
            }
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as TtMaterial;
            }
            TtMaterial HostMaterial;
            [Rtti.Meta("")]
            [Category("Option")]
            [ReadOnly(true)]
            public string Name { get; set; }
            internal NxRHI.FSamplerDesc mValue;
            [Rtti.Meta("")]
            [Category("Option")]
            public NxRHI.FSamplerDesc Value
            {
                get => mValue;
                set
                {
                    mValue = value;
                    if (HostMaterial != null)
                        HostMaterial.SerialId++;
                }
            }
            public NameSamplerStateDescPair Clone(TtMaterial mtl)
            {
                var result = new NameSamplerStateDescPair();
                result.HostMaterial = mtl;
                result.Name = Name;
                result.mValue = mValue;
                return result;
            }
        }
        protected List<NameSamplerStateDescPair> mUsedSamplerStates = new List<NameSamplerStateDescPair>();
        [Rtti.Meta("")]
        [Category("Variable")]
        public List<NameSamplerStateDescPair> UsedSamplerStates { 
            get => mUsedSamplerStates; 
            set => mUsedSamplerStates = value; }
        public int NumOfSampler
        {
            get
            {
                return mUsedSamplerStates.Count;
            }
        }
        public string GetNameOfSampler(int index)
        {
            if (index < 0 || index >= mUsedSamplerStates.Count)
                return null;
            return mUsedSamplerStates[index].Name;
        }
        public NxRHI.TtSampler GetSampler(int index)
        {
            if (mUsedSamplerStates[index].mValue.m_AddressU == 0)
            {
                mUsedSamplerStates[index].mValue.SetDefault();
            }
            //if (mUsedSamplerStates[index].mValue.m_AddressV == 0)
            //{
            //    mUsedSamplerStates[index].mValue.m_AddressV = EAddressMode.ADM_WRAP;
            //}
            //if (mUsedSamplerStates[index].mValue.m_AddressW == 0)
            //{
            //    mUsedSamplerStates[index].mValue.m_AddressW = EAddressMode.ADM_WRAP;
            //}
            return TtEngine.Instance.GfxDevice.SamplerStateManager.GetPipelineState(TtEngine.Instance.GfxDevice.RenderContext, in mUsedSamplerStates[index].mValue);
        }
        public NameSamplerStateDescPair FindSampler(string name)
        {
            foreach (var i in mUsedSamplerStates)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        #endregion
        #region UniformVar
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial.NameValuePair@EngineCore" })]
        [EGui.Controls.PropertyGrid.TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        public class NameValuePair : IO.BaseSerializer
        {
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as TtMaterial;
            }
            TtMaterial HostMaterial;
            [Rtti.Meta("")]
            [Category("Option")]
            [ReadOnly(true)]
            public string VarType { get; set; }
            [Category("Option")]
            [ReadOnly(true)]
            [Rtti.Meta("")]
            public string Name { get; set; }
            string mValue;
            [Rtti.Meta("")]
            [Category("Option")]
            public string Value
            {
                get => mValue;
                set
                {
                    GamePlay.Action.UAction.OnChanged(HostMaterial, this, "Value", mValue, value);
                    mValue = value;
                    if (HostMaterial != null)
                        HostMaterial.SerialId++;
                }
            }
            public bool SetValue(in Color3f v)
            {
                if (VarType != "float3")
                    return false;
                Value = v.ToVector3().ToString();
                return true;
            }
            public bool SetValue(in Color4f v)
            {
                if (VarType != "float4")
                    return false;
                Value = v.ToVector4().ToString();
                return true;
            }
            public bool SetValue(in Vector4 v)
            {
                if (VarType != "float4")
                    return false;
                Value = v.ToString();
                return true;
            }
            public bool SetValue(in Vector3 v)
            {
                if (VarType != "float3")
                    return false;
                Value = v.ToString();
                return true;
            }
            public bool SetValue(in Vector2 v)
            {
                if (VarType != "float2")
                    return false;
                Value = v.ToString();
                return true;
            }
            public bool SetValue(float v)
            {
                if (VarType != "float")
                    return false;
                Value = v.ToString();
                return true;
            }
            public NameValuePair Clone(TtMaterial mtl)
            {
                var result = new NameValuePair();
                result.HostMaterial = mtl;
                result.VarType = VarType;
                result.Name = Name;
                result.mValue = mValue;
                return result;
            }
        }
        protected List<NameValuePair> mUsedUniformVars = new List<NameValuePair>();
        [Rtti.Meta("")]
        [Category("Variable")]
        public List<NameValuePair> UsedUniformVars { get => mUsedUniformVars; }

        /// <summary>
        /// The SubsurfaceProfile asset referenced by this material (if any).
        /// Set during material compilation from TtSubsurfaceProfileIdNode.
        /// Used at runtime to resolve the dynamic profile index in CBuffer.
        /// </summary>
        [Rtti.Meta("")]
        [Browsable(false)]
        public RName SubsurfaceProfileAsset { get; set; }
        public NameValuePair FindVar(string name)
        {
            foreach(var i in mUsedUniformVars)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public virtual int NumOfUniformVars
        {
            get
            {
                return UsedUniformVars.Count;
            }
        }
        public virtual string GetNameOfUniformVar(int index)
        {
            if (index < 0 || index >= UsedUniformVars.Count)
                return null;
            return UsedUniformVars[index]?.Name;
        }
        internal unsafe virtual void UpdateCBufferVars(NxRHI.TtCbView cBuffer, NxRHI.FShaderBinder binder)
        {
            foreach (var i in UsedUniformVars)
            {
                var desc = binder.FindField(i.Name);
                switch (desc.Type)
                {
                    case NxRHI.EShaderVarType.SVT_Float:
                        {
                            switch (desc.Columns)
                            {
                                case 1:
                                    {
                                        float v = 0;
                                        if(float.TryParse(i.Value, out v))
                                            cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                                case 2:
                                    {
                                        var v = Vector2.FromString(i.Value);
                                        cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                                case 3:
                                    {
                                        var v = Vector3.FromString(i.Value);
                                        cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                                case 4:
                                    {
                                        var v = Vector4.FromString(i.Value);
                                        cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                            }
                        }
                        break;
                    case NxRHI.EShaderVarType.SVT_Int:
                        {
                            switch (desc.Columns)
                            {
                                case 1:
                                    {
                                        int v = 0;
                                        if(int.TryParse(i.Value, out v))
                                            cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                                case 2:
                                    {
                                        var v = Vector2.FromString(i.Value);
                                        cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                                case 3:
                                    {
                                        var v = Vector3.FromString(i.Value);
                                        cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                                case 4:
                                    {
                                        var v = Vector4.FromString(i.Value);
                                        cBuffer.SetValue(desc, in v);
                                    }
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            var index = binder.FindField("MaterialRenderFlags");
            if (index.IsValidPointer)
            {
                cBuffer.SetValue(index, (uint)RenderFlags);
            }
        }

        /// <summary>
        /// Updates the SubsurfaceProfileIndex uniform in PerMaterialCBuffer at runtime.
        /// Because profile indices are assigned dynamically during asset loading,
        /// we must resolve the RName→index mapping each frame before draw.
        /// </summary>
        internal void UpdateSubsurfaceProfileIndex()
        {
            if (SubsurfaceProfileAsset == null)
                return;
            if (mPerMaterialCBuffer == null)
                return;

            var profileManager = TtEngine.Instance?.GfxDevice?.SubsurfaceProfileManager;
            if (profileManager == null)
                return;

            int profileIndex = profileManager.GetIndexByRName(SubsurfaceProfileAsset);

            var fieldDesc = mPerMaterialCBuffer.ShaderBinder.FindField(
                Bricks.CodeBuilder.ShaderNode.Var.TtSubsurfaceProfileIdNode.UniformVarName);
            if (fieldDesc.IsValidPointer)
            {
                float indexAsFloat = (float)profileIndex;
                mPerMaterialCBuffer.SetValue(fieldDesc, in indexAsFloat);
            }
        }
        #endregion

        #endregion

        #region RHIResource
        protected NxRHI.FGpuPipelineDesc mPipelineDesc;
        NxRHI.TtGpuPipeline mPipeline;
        public NxRHI.TtGpuPipeline Pipeline
        {
            get
            {
                if (mPipeline == null)
                    UpdatePipeline();
                return mPipeline;
            }
        }
        internal void UpdatePipeline()
        {
            mPipeline = TtEngine.Instance.GfxDevice.PipelineManager.GetPipelineState(TtEngine.Instance.GfxDevice.RenderContext, in mPipelineDesc);
        }
        NxRHI.TtCbView mPerMaterialCBuffer;
        [Browsable(false)]
        public NxRHI.TtCbView PerMaterialCBuffer 
        {
            get
            {
                return mPerMaterialCBuffer;
            }
        }
        [Rtti.Meta("")]
        public bool SetSrv(string name,
            [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
            RName srv)
        {
            foreach (var i in mUsedSrView)
            {
                if (i.Name == name)
                {
                    i.Value = srv;
                    return true;
                }
            }
            return false;
        }
        [Rtti.Meta("")]
        public bool SetColor4(string name, in Color4f color)
        {
            var v = FindVar(name);
            if (v == null)
                return false;
            v.SetValue(in color);
            PerMaterialCBuffer?.SetValue(name, in color);
            return true;
        }
        [Rtti.Meta("")]
        public bool SetColor3(string name, in Color3f color)
        {
            var v = FindVar(name);
            if (v == null)
                return false;
            v.SetValue(in color);
            PerMaterialCBuffer?.SetValue(name, in color);
            return true;
        }
        [Rtti.Meta("")]
        public bool SetFloat4(string name, in Vector4 value)
        {
            var v = FindVar(name);
            if (v == null)
                return false;
            v.SetValue(in value);
            PerMaterialCBuffer?.SetValue(name, in value);
            return true;
        }
        public bool CreateCBuffer(TtGraphicsEffect effect)
        {
            var binder = effect.ShaderEffect.FindBinder("cbPerMaterial");
            if (binder == null)
                return false;
            mPerMaterialCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            return true;
        }
        [Rtti.Meta("")]
        [Category("State")]
        public NxRHI.FRasterizerDesc Rasterizer
        {
            get => mPipelineDesc.m_Rasterizer;
            set
            {
                mPipelineDesc.m_Rasterizer = value; 
                UpdatePipeline();
                SerialId++;
            }
    }
        public const uint StencilWriteDisabled = 0xFFFFFFFF;
        protected uint mStencilWriteValue = StencilWriteDisabled;
        [Rtti.Meta("", Order = 1)]
        [Category("State")]
        [Description("Stencil tag written during BasePass (0~254). 0xFFFFFFFF = disabled")]
        public uint StencilWriteValue
        {
            get => mStencilWriteValue;
            set
            {
                if (mStencilWriteValue == value)
                    return;
                mStencilWriteValue = value;
                ref var dsDesc = ref mPipelineDesc.m_DepthStencil;
                if (value == StencilWriteDisabled)
                {
                    dsDesc.StencilEnable = 0;
                    dsDesc.StencilWriteMask = 0;
                    dsDesc.StencilRef = 0;
                    mPipelineDesc.StencilRef = 0;
                }
                else
                {
                    dsDesc.StencilEnable = 1;
                    dsDesc.StencilReadMask = 0xFF;
                    dsDesc.StencilWriteMask = 0xFF;
                    dsDesc.StencilRef = value;
                    mPipelineDesc.StencilRef = value;
                    var opDesc = new NxRHI.FStencilOpDesc();
                    opDesc.StencilFailOp = NxRHI.EStencilOp.STOP_KEEP;
                    opDesc.StencilDepthFailOp = NxRHI.EStencilOp.STOP_KEEP;
                    opDesc.StencilPassOp = NxRHI.EStencilOp.STOP_REPLACE;
                    opDesc.StencilFunc = NxRHI.EComparisionMode.CMP_ALWAYS;
                    dsDesc.FrontFace = opDesc;
                    dsDesc.BackFace = opDesc;
                }
                mPipelineDesc.m_DepthStencil = dsDesc;
                UpdatePipeline();
                SerialId++;
            }
        }
        [Rtti.Meta("")]
        [Category("State")]
        public NxRHI.FDepthStencilDesc DepthStencil
        {
            get => mPipelineDesc.m_DepthStencil;
            set
            {
                mPipelineDesc.m_DepthStencil = value;
                // Re-apply StencilWriteValue if active, because deserialization
                // order may cause DepthStencil to overwrite the stencil state
                // that StencilWriteValue setter already configured.
                if (mStencilWriteValue != StencilWriteDisabled)
                {
                    ref var ds = ref mPipelineDesc.m_DepthStencil;
                    ds.StencilEnable = 1;
                    ds.StencilReadMask = 0xFF;
                    ds.StencilWriteMask = 0xFF;
                    ds.StencilRef = mStencilWriteValue;
                    var opDesc = new NxRHI.FStencilOpDesc();
                    opDesc.StencilFailOp = NxRHI.EStencilOp.STOP_KEEP;
                    opDesc.StencilDepthFailOp = NxRHI.EStencilOp.STOP_KEEP;
                    opDesc.StencilPassOp = NxRHI.EStencilOp.STOP_REPLACE;
                    opDesc.StencilFunc = NxRHI.EComparisionMode.CMP_ALWAYS;
                    ds.FrontFace = opDesc;
                    ds.BackFace = opDesc;
                }
                UpdatePipeline();
                SerialId++;
            }
        }
        [Rtti.Meta("")]
        [Category("State")]
        public NxRHI.FBlendDesc Blend
        {
            get => mPipelineDesc.m_Blend;
            set
            {
                mPipelineDesc.m_Blend = value;
                UpdatePipeline();
                SerialId++;
            }
        }
        #endregion

        #region Utility
        public class FTextureSpaceResult
        {
            public TtAttachBuffer Buffer;
            public FSubResourceFootPrint Footprint;
            public void SavePng(string file)
            {

            }
        }
        public static async Thread.Async.TtTask<FTextureSpaceResult> GetTextureSpaceResult(TtMaterial material, int width = 512, int height = 512)
        {
            TtRenderPolicy policy = await TtRenderPolicy.CreatRenderPolicy(RName.GetRName("graphics/material_to_texture.rpolicy", RName.ERNameType.Engine));
            var m2t = policy.FindFirstNode<Graphics.Pipeline.Utility.TtMaterialToTextureNode>();
            if (m2t==null)
                return null;
            //m2t.MaterialName = materialName;
            m2t.SetMaterial(material);

            policy.IsSyncBuildDrawcall = true;
            policy.OnResize(width, height);

            var renderer = new GamePlay.Scene.TtWorldImmRenderer();
            TtWorld ttWorld = new TtWorld(null, false);
            await ttWorld.InitWorld();
            renderer.Initialize(ttWorld, policy);

            //TtEngine.Instance.GfxDevice.RenderSwapQueue.CaptureRenderDocFrame = true;
            //TtEngine.Instance.GfxDevice.RenderSwapQueue.BeginFrameCapture();
            renderer.TickLogic(0);
            //TtEngine.Instance.GfxDevice.RenderSwapQueue.EndFrameCapture("Mat2Textur");

            var fence = renderer.RenderPolicy.FindFirstNode<Graphics.Pipeline.Common.TtFenceIncreaseNode>();
            fence.WaitFence();

            //TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush(EQueueType.QU_ALL);
            var node = renderer.RenderPolicy.FindFirstNode<Graphics.Pipeline.Common.TtCopy2ReadbackNode>();
            var result = new FTextureSpaceResult();
            result.Buffer = node.ResultBuffer;
            result.Footprint = node.mCopyDrawcall.FootPrint;
            return result;
        }
        #endregion
    }
    public partial class TtMaterialManager : IDisposable
    {
        public void Dispose()
        {
            NavMeshDebugMaterial = null;
            NavMeshDebugWireMaterial = null;
            PxDebugMaterial = null;
            ScreenMaterial = null;
            VtxColorMaterial = null;

            foreach (var i in Materials)
            {
                foreach(var j in i.Value.UsedSrView)
                {
                    j.SrvObject = null;
                }
            }
            Materials.Clear();
        }
        public async Thread.Async.TtTask Initialize(TtGfxDevice device)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            ScreenMaterial = new TtMaterial();

            var dsDesc = new NxRHI.FDepthStencilDesc();
            dsDesc.SetDefault();
            dsDesc.m_DepthEnable = 0;
            dsDesc.m_DepthWriteMask = 0;
            ScreenMaterial.DepthStencil = dsDesc;

            PxDebugMaterial = await RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine).CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            VtxColorMaterial = await RName.GetRName("material/vfx_color.material", RName.ERNameType.Engine).CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            NavMeshDebugMaterial = await RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine).CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            NavMeshDebugMaterial.SetColor4("clr4_0", new Color4f(Color4b.White));
            NavMeshDebugWireMaterial = await RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine).CreateAsset<Graphics.Pipeline.Shader.TtMaterial>();
            NavMeshDebugWireMaterial.SetColor4("clr4_0", new Color4f(Color4b.PaleVioletRed));
            var rast = NavMeshDebugWireMaterial.Rasterizer;
            rast.FillMode = NxRHI.EFillMode.FMD_WIREFRAME;
            rast.CullMode = ECullMode.CMD_NONE;
            NavMeshDebugWireMaterial.Rasterizer = rast;
        }
        public TtMaterial ScreenMaterial;
        public TtMaterial PxDebugMaterial;
        public TtMaterial NavMeshDebugMaterial;
        public TtMaterial NavMeshDebugWireMaterial;
        public TtMaterial VtxColorMaterial;
        public Dictionary<RName, TtMaterial> Materials { get; } = new Dictionary<RName, TtMaterial>();
        public async Thread.Async.TtTask<TtMaterial> CreateMaterial(RName rn)
        {
            TtMaterial result;
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterial.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (result != null && result.AssetName != rn)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Material({rn}): AssetName({result.AssetName})");
            }
            return result;
        }
        public async Thread.Async.TtTask<bool> ReloadMaterial(RName rn)
        {
            TtMaterial result;
            if (Materials.TryGetValue(rn, out result) == false)
                return true;

            var ok = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return TtMaterial.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var effects = TtEngine.Instance.GfxDevice.EffectManager.GraphicsEffects;
            foreach (var i in effects)
            {
                if (i.Value.Desc.MaterialName == rn)
                {
                    if (i.Value.Desc.MaterialHash != result.MaterialHash)
                    {
                        await i.Value.RefreshEffect(result);
                    }
                }
            }

            return ok;
        }
        internal TtMaterial UnsafeRemove(RName name)
        {
            lock (Materials)
            {
                if (Materials.TryGetValue(name, out var result))
                {
                    Materials.Remove(name);
                    return result;
                }
                return null;
            }
        }
        internal void UnsafeAdd(RName name, TtMaterial obj)
        {
            lock (Materials)
            {
                Materials.Add(name, obj);
            }
        }
        [Rtti.Meta("")]
        public TtMaterial FindMaterial(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;
            return null;
        }
        private Thread.TtAwaitSessionManager<RName, TtMaterial> mCreatingSession = new ();
        [Rtti.Meta("")]
        public async Thread.Async.TtTask<TtMaterial> GetMaterial(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            Thread.TtSemaphore smp;
            var session = mCreatingSession.GetOrNewSession(rn, out smp);
            if (smp != null)
            {
                await smp.Await();
                return session.Result;
            }

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterial.LoadXnd(this, xnd.RootNode);
                        if (material == null)
                            return null;

                        material.AssetName = rn;
                        return material;
                    }
                    else
                    {
                        return null;
                    }
                }   
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                if (result.AssetName != rn)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"Material({rn}): AssetName({result.AssetName})");
                }
                Materials.Add(rn, result);
                session.FinishSession(rn, result);
                return result;
            }

            return null;
        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Graphics.Pipeline.Shader
{
	partial class TtMaterial
	{
		public unsafe void macross_SaveAssetTo (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName name) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SaveAssetTo(name);
		}
		public unsafe bool macross_SetSrv (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, RName srv) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = SetSrv(name, srv);
			return _return_value;
		}
		public unsafe bool macross_SetColor4 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, in Color4f color) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = SetColor4(name, in color);
			return _return_value;
		}
		public unsafe bool macross_SetColor3 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, in Color3f color) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = SetColor3(name, in color);
			return _return_value;
		}
		public unsafe bool macross_SetFloat4 (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, string name, in Vector4 value) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = SetFloat4(name, in value);
			return _return_value;
		}
	}
}


namespace EngineNS.Graphics.Pipeline.Shader
{
	partial class TtMaterialManager
	{
		public unsafe TtMaterial macross_FindMaterial (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName rn) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = FindMaterial(rn);
			return _return_value;
		}
		public async Thread.Async.TtTask<TtMaterial> macross_GetMaterial (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName rn) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await GetMaterial(rn);
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross