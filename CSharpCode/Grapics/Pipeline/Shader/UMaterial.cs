using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterialAMeta@EngineCore" })]
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
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(GetAssetName());
        }
        public override void OnBeforeRenamedAsset(IO.IAsset asset, RName name)
        {
            CoreSDK.CheckResult(TtEngine.Instance.GfxDevice.MaterialManager.UnsafeRemove(name) == asset);
        }
        public override void OnAfterRenamedAsset(IO.IAsset asset, RName name)
        {
            TtEngine.Instance.GfxDevice.MaterialManager.UnsafeAdd(name, (TtMaterial)asset);
        }
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.TtFileManager.DeleteFile(address);
            IO.TtFileManager.DeleteFile(address + IO.IAssetMeta.MetaExt);
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MaterialBoderColor;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "Mtl", null);
        //}
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial@EngineCore" })]
    [TtMaterial.MaterialImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/Material")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtMaterial : IO.ISerializer, IO.IAsset, IShaderCodeProvider
    {
        public const string AssetExt = ".material";
        public string TypeExt { get => AssetExt; }
        public TtMaterial()
        {
            mPipelineDesc.SetDefault();
        }
        public bool IsEditingMaterial { get; set; }

        #region SystemVar
        
        
        #endregion

        public override string ToString()
        {
            string result = $"Materai:{AssetName}\n";
            return result;
        }
        protected Hash160 mMaterialHash;
        [Rtti.Meta]
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
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            foreach (var i in UsedSrView)
            {
                if (i.Value == null)
                    continue;
                ameta.AddReferenceAsset(i.Value);
            }
        }
        [Rtti.Meta]
        public virtual void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }

            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
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
        [Rtti.Meta]
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
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

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
        public static string GenMateralGraphCode(TtMaterial Material, UHLSLCodeGenerator mHLSLCodeGen, 
            Bricks.CodeBuilder.ShaderNode.UMaterialGraph MaterialGraph, 
            Bricks.CodeBuilder.ShaderNode.UMaterialOutput MaterialOutput)
        {
            Material.UsedSrView.Clear();
            Material.UsedUniformVars.Clear();
            Material.UsedSamplerStates.Clear();

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
            string code = "";
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

            return code;
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
        public enum ELightingMode
        {
            Stand,
            Unlight,
            Skin,
            Transmit,
            Hair,
            Eye,
        }
        [Rtti.Meta]
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
            NormalNone,
        }
        ENormalMode mNormalMode = ENormalMode.NormalMap;
        [Rtti.Meta]
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        [Category("Option")]
        public virtual bool AlphaTest
        {
            get;
            set;
        } = true;

        [Flags]
        public enum ERenderFlags
        {
            None = 0,
            DisableEnvColor = 1,
        }
        private ERenderFlags mRenderFlags = ERenderFlags.None;
        public virtual ERenderFlags RenderFlags { get => mRenderFlags; }
        [Rtti.Meta]
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
        #endregion

        [Rtti.Meta]
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

            switch (LightingMode)
            {
                case ELightingMode.Unlight:
                    codeBuilder.AddLine("#define MTL_ID_UNLIT", ref sourceCode);
                    break;
                case ELightingMode.Skin:
                    codeBuilder.AddLine("#define MTL_ID_SKIN", ref sourceCode);
                    break;
                case ELightingMode.Transmit:
                    codeBuilder.AddLine("#define MTL_ID_TRANSMIT", ref sourceCode);
                    break;
                case ELightingMode.Hair:
                    codeBuilder.AddLine("#define MTL_ID_HAIR", ref sourceCode);
                    break;
                case ELightingMode.Eye:
                    codeBuilder.AddLine("#define MTL_ID_EYE", ref sourceCode);
                    break;
            }
            switch (NormalMode)
            {
                case ENormalMode.Normal:
                    codeBuilder.AddLine("#define MTL_NORMAL_MODE MTL_NORMAL", ref sourceCode);
                    break;
                case ENormalMode.NormalMap:
                    codeBuilder.AddLine("#define MTL_NORMAL_MODE MTL_NORMALMAP", ref sourceCode);
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
        [Browsable(false)]
        public NxRHI.TtShaderCode DefineCode { get; } = new NxRHI.TtShaderCode();
        [Browsable(false)]
        public NxRHI.TtShaderCode SourceCode { get; } = new NxRHI.TtShaderCode();
        [Category("Option")]
        public string DefineCodeText
        {
            get
            {
                return DefineCode.TextCode;
            }
        }
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
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        [Browsable(false)]
        public string GraphXMLString
        {
            get;
            set;
        }
        string mHLSLCode;
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
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
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        public List<string> IncludeFiles { get; set; } = new List<string>();
        [Rtti.Meta()]
        public List<EngineNS.NxRHI.EVertexStreamType> VSNeedStreams
        {
            get;
            set;
        } = null;        
        [Rtti.Meta()]
        public List<Graphics.Pipeline.Shader.EPixelShaderInput> PSNeedInputs 
        { 
            get; 
            set;
        } = null;
        #endregion
        #region Texture
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial.NameRNamePair@EngineCore" })]
        [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        public class NameRNamePair : IO.BaseSerializer
        {
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as TtMaterial;
            }
            TtMaterial HostMaterial;
            [Rtti.Meta()]
            [Category("Option")]
            [ReadOnly(true)]
            public string Name { get; set; }
            RName mValue;
            [Rtti.Meta]
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
            [Rtti.Meta]
            public string ShaderType { get; set; } = "Texture2D";
            public NameRNamePair Clone(TtMaterial mtl)
            {
                var result = new NameRNamePair();
                result.HostMaterial = mtl;
                result.Name = Name;
                result.mValue = mValue;
                return result;
            }
            public object SrvObject { get; set; } = null;
        }
        protected List<NameRNamePair> mUsedSrView = new List<NameRNamePair>();
        [Rtti.Meta(NameAlias = new string[] { "UsedRSView" })]
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
        public virtual async System.Threading.Tasks.Task<NxRHI.TtSrView> GetSRV(int index)
        {
            var srv = UsedSrView[index].SrvObject as NxRHI.TtSrView;
            if (srv != null)
                return srv;
            UsedSrView[index].SrvObject = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(UsedSrView[index].Value);
            return UsedSrView[index].SrvObject as NxRHI.TtSrView;
        }
        public NxRHI.TtSrView TryGetSRV(int index)
        {
            var srv = UsedSrView[index].SrvObject as NxRHI.TtSrView;
            if (srv != null)
                return srv;
            UsedSrView[index].SrvObject = TtEngine.Instance.GfxDevice.TextureManager.TryGetTexture(UsedSrView[index].Value);
            return UsedSrView[index].SrvObject as NxRHI.TtSrView;
        }
        #endregion
        #region Sampler
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial.NameSamplerStateDescPair@EngineCore" })]
        [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
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
            [Rtti.Meta()]
            [Category("Option")]
            [ReadOnly(true)]
            public string Name { get; set; }
            internal NxRHI.FSamplerDesc mValue;
            [Rtti.Meta]
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
        [Rtti.Meta]
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
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterial.NameValuePair@EngineCore" })]
        [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
        public class NameValuePair : IO.BaseSerializer
        {
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as TtMaterial;
            }
            TtMaterial HostMaterial;
            [Rtti.Meta]
            [Category("Option")]
            [ReadOnly(true)]
            public string VarType { get; set; }
            [Category("Option")]
            [ReadOnly(true)]
            public string Name { get; set; }
            string mValue;
            [Rtti.Meta]
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
        [Rtti.Meta]
        [Category("Variable")]
        public List<NameValuePair> UsedUniformVars { get => mUsedUniformVars; }
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
        [Rtti.Meta]
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
        [Rtti.Meta]
        public bool SetColor4(string name, in Color4f color)
        {
            var v = FindVar(name);
            if (v == null)
                return false;
            v.SetValue(in color);
            PerMaterialCBuffer?.SetValue(name, in color);
            return true;
        }
        [Rtti.Meta]
        public bool SetColor3(string name, in Color3f color)
        {
            var v = FindVar(name);
            if (v == null)
                return false;
            v.SetValue(in color);
            PerMaterialCBuffer?.SetValue(name, in color);
            return true;
        }
        [Rtti.Meta]
        public bool SetFloat4(string name, in Vector4 value)
        {
            var v = FindVar(name);
            if (v == null)
                return false;
            v.SetValue(in value);
            PerMaterialCBuffer?.SetValue(name, in value);
            return true;
        }
        public bool CreateCBuffer(TtEffect effect)
        {
            var binder = effect.ShaderEffect.FindBinder("cbPerMaterial");
            if (binder == null)
                return false;
            mPerMaterialCBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(binder);
            return true;
        }
        [Rtti.Meta]
        [Category("State")]
        public NxRHI.FRasterizerDesc Rasterizer
        {
            get => mPipelineDesc.m_Rasterizer;
            set
            {
                mPipelineDesc.m_Rasterizer = value; 
                UpdatePipeline();
            }
    }
        [Rtti.Meta]
        [Category("State")]
        public NxRHI.FDepthStencilDesc DepthStencil
        {
            get => mPipelineDesc.m_DepthStencil;
            set
            {
                mPipelineDesc.m_DepthStencil = value;
                UpdatePipeline();
            }
        }
        [Rtti.Meta]
        [Category("State")]
        public NxRHI.FBlendDesc Blend
        {
            get => mPipelineDesc.m_Blend;
            set
            {
                mPipelineDesc.m_Blend = value;
                UpdatePipeline();
            }
        }
        #endregion
    }
    public partial class TtMaterialManager
    {
        public void Cleanup()
        {
            foreach(var i in Materials)
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

            PxDebugMaterial = await this.CreateMaterial(RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine));
            VtxColorMaterial = await this.CreateMaterial(RName.GetRName("material/vfx_color.material", RName.ERNameType.Engine));
        }
        public TtMaterial ScreenMaterial;
        public TtMaterial PxDebugMaterial;
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

            var effects = TtEngine.Instance.GfxDevice.EffectManager.Effects;
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
        [Rtti.Meta]
        public TtMaterial FindMaterial(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;
            return null;
        }
        [Rtti.Meta]
        public async Thread.Async.TtTask<TtMaterial> GetMaterial(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;

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
                Materials[rn] = result;
                return result;
            }

            return null;
        }
    }
}
