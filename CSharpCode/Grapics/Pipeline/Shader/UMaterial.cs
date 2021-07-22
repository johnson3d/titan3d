using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Rtti.Meta]
    public partial class UMaterialAMeta : IO.IAssetMeta
    {
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(GetAssetName());
        }
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.FileManager.DeleteFile(address);
            IO.FileManager.DeleteFile(address + ".ameta");
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override void OnDraw(ref ImDrawList cmdlist, ref Vector2 sz, EGui.Controls.ContentBrowser ContentBrowser)
        {
            base.OnDraw(ref cmdlist, ref sz, ContentBrowser);
        }
    }
    [Rtti.Meta]
    [UMaterial.MaterialImport]
    [IO.AssetCreateMenu(MenuName = "Material")]
    public partial class UMaterial : IO.ISerializer, IO.IAsset, IShaderCodeProvider
    {
        public const string AssetExt = ".material";

        #region SystemVar
        public class VSInput
        {
            public Vector3 vPosition;
	        public Vector3 vNormal;
	        public Vector4 vTangent;
	        public Vector4 vColor;
	        public Vector2 vUV;
	        public Vector2 vLightMap;
	        public UInt32_4 vSkinIndex;
	        public Vector4 vSkinWeight;
	        public UInt32_4 vTerrainIndex;
	        public UInt32_4 vTerrainGradient;
	        public Vector3 vInstPos;
	        public Vector4 vInstQuat;
	        public Vector4 vInstScale;
	        public UInt32_4 vF4_1;
	        public Vector4 vF4_2;
	        public Vector4 vF4_3;
            public uint vInstanceId;
        }
        public class PSInput
        {
            public Vector4 vPosition;
            public Vector3 vNormal;
	        public Vector4 vColor;
            public Vector2 vUV;
	        public Vector3 vWorldPos;   //the 4th channel is unused just for now;
	        public Vector4 vTangent;
	        public Vector4 vLightMap;
	        public Vector4 psCustomUV0;
	        public Vector4 psCustomUV1;
	        public Vector4 psCustomUV2;
	        public Vector4 psCustomUV3;
	        public Vector4 psCustomUV4;
	        public UInt32_4 PointLightIndices;
	        public UInt32_4 vF4_1;
	        public Vector4 vF4_2;
	        public Vector4 vF4_3;
            public UInt32_4 SpecialData;
        }
        public class MTLOutput
        {
            public Vector3 mAlbedo;
            public Vector3 mNormal;
            public float mMetallic;
            public float mRough;   //in the editer,we call it smoth,so rough = 1.0f - smoth;
            public float mAbsSpecular;
            public float mTransmit;
            public Vector3 mEmissive;
            public float mFuzz;
            public float mIridescence;
            public float mDistortion;
            public float mAlpha;
            public float mAlphaTest;
            public Vector3 mVertexOffset;
            public Vector3 mSubAlbedo;
            public float mAO;
            public float mMask;
            public Vector3 mShadowColor;
            public float mDeepShadow;
            public Vector3 mMoodColor;
        };
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
            string result = DefineCode?.AsText;
            result += SourceCode?.AsText;
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
            }
        }
        public class MaterialImportAttribute : IO.CommonCreateAttribute
        {
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                base.DoCreate(dir, type, ext);

                (mAsset as UMaterial).UpdateShaderCode(true);
            }
        }
        #region IAsset
        public virtual IO.IAssetMeta CreateAMeta()
        {
            var result = new UMaterialAMeta();
            return result;
        }
        public virtual IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public virtual void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.CXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("Material", 0, 0))
            {
                var ar = attr.GetWriter(512);
                ar.Write(this);
                attr.ReleaseWriter(ref ar);
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
        }
        [Rtti.Meta]
        [RName.PGRName(ReadOnly = true)]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public virtual UMaterial ParentMaterial
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
        public static UMaterial LoadXnd(UMaterialManager manager, IO.CXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("Material");
            if (attr.NativePointer != IntPtr.Zero)
            {
                var ar = attr.GetReader(null);
                ar.Read(out result, null);
                attr.ReleaseReader(ref ar);
            }

            var material = result as UMaterial;
            if (material != null)
            {
                material.UpdateShaderCode(false);
                return material;
            }
            return null;
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
        public ELightingMode LightingMode 
        { 
            get; 
            set; 
        } = ELightingMode.Stand;
        [Rtti.Meta]
        public ERenderLayer RenderLayer
        {
            get;
            set;
        } = ERenderLayer.RL_Opaque;
        [Rtti.Meta]
        public bool AlphaTest
        {
            get;
            set;
        } = false;
        internal virtual void UpdateShaderCode(bool EmptyMaterial)
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine($"#ifndef _Material_H_");
            codeBuilder.AddLine($"#define _Material_H_");

            foreach (var i in this.UsedRSView)
            {
                codeBuilder.AddLine($"Texture2D {i.Name};");
            }

            codeBuilder.AddLine("void DO_VS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl)");
            codeBuilder.PushBrackets();
            codeBuilder.PopBrackets();

            if (EmptyMaterial)
            {
                this.HLSLCode = "void DO_PS_MATERIAL_IMPL(in PS_INPUT input, inout MTL_OUTPUT mtl)\n"+
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

                codeBuilder.AppendCode(this.HLSLCode, false, true);
            }
            else
            {
                codeBuilder.AppendCode(this.HLSLCode, false, true);
            }

            codeBuilder.AddLine("#undef DO_VS_MATERIAL");
            codeBuilder.AddLine("#define DO_VS_MATERIAL DO_VS_MATERIAL_IMPL");
            codeBuilder.AddLine("#undef DO_PS_MATERIAL");
            codeBuilder.AddLine("#define DO_PS_MATERIAL DO_PS_MATERIAL_IMPL");

            switch (LightingMode)
            {
                case ELightingMode.Unlight:
                    codeBuilder.AddLine("#define MTL_ID_UNLIT");
                    break;
                case ELightingMode.Skin:
                    codeBuilder.AddLine("#define MTL_ID_SKIN");
                    break;
                case ELightingMode.Transmit:
                    codeBuilder.AddLine("#define MTL_ID_TRANSMIT");
                    break;
                case ELightingMode.Hair:
                    codeBuilder.AddLine("#define MTL_ID_HAIR");
                    break;
                case ELightingMode.Eye:
                    codeBuilder.AddLine("#define MTL_ID_EYE");
                    break;
            }

            if (AlphaTest)
            {
                codeBuilder.AddLine("#define ALPHA_TEST");
            }

            codeBuilder.AddLine("#endif//_Material_H_");
            SourceCode.SetText(codeBuilder.ClassCode);

            string uniformVarsCode = "";
            foreach (var i in this.UsedUniformVars)
            {
                uniformVarsCode += $"{i.VarType} {i.Name};";
            }
            DefineCode.SetText(uniformVarsCode);

            mMaterialHash = GetHash();
        }
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public IO.CMemStreamWriter DefineCode { get; } = new IO.CMemStreamWriter();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public IO.CMemStreamWriter SourceCode { get; } = new IO.CMemStreamWriter();

        #region Data
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        [Browsable(false)]
        public string GraphXMLString
        {
            get;
            set;
        }
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.DiscardWhenCooked)]
        [Browsable(false)]
        public string HLSLCode
        {
            get;
            set;
        }
        public class NameRNamePair : IO.BaseSerializer
        {
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as UMaterial;
            }
            UMaterial HostMaterial;
            [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
            public string Name { get; set; }
            RName mValue;
            [Rtti.Meta]
            [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
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
        }
        protected List<NameRNamePair> mUsedRSView = new List<NameRNamePair>();
        [Rtti.Meta]
        [Category("Variable")]
        public List<NameRNamePair> UsedRSView { get => mUsedRSView; }
        protected NameRNamePair FindSRV(string name)
        {
            foreach (var i in mUsedRSView)
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
                return UsedRSView.Count;
            }
        }
        public string GetNameOfSRV(int index)
        {
            if (index < 0 || index >= UsedRSView.Count)
                return null;
            return UsedRSView[index]?.Name;
        }
        public virtual async System.Threading.Tasks.Task<RHI.CShaderResourceView> GetSRV(int index)
        {
            return await UEngine.Instance.GfxDevice.TextureManager.GetTexture(UsedRSView[index].Value);
        }
        public RHI.CShaderResourceView TryGetSRV(int index)
        {
            return UEngine.Instance.GfxDevice.TextureManager.TryGetTexture(UsedRSView[index].Value);
        }
        public int NumOfSampler
        {
            get
            {
                return 0;
            }
        }
        public string GetNameOfSampler(int index)
        {
            return "";
        }
        public RHI.CSamplerState GetSampler(int index)
        {
            return null;
        }
        public class NameValuePair : IO.BaseSerializer
        {
            public override void OnPreRead(object tagObject, object hostObject, bool fromXml)
            {
                HostMaterial = hostObject as UMaterial;
            }
            UMaterial HostMaterial;
            [Rtti.Meta]
            public string VarType { get; set; }
            [Rtti.Meta]
            public string Name { get; set; }
            string mValue;
            [Rtti.Meta]
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
        internal virtual void UpdateUniformVars(RHI.CConstantBuffer cBuffer)
        {
            foreach (var i in UsedUniformVars)
            {
                var index = cBuffer.mCoreObject.FindVar(i.Name);
                var desc = new ConstantVarDesc();
                cBuffer.mCoreObject.GetVarDesc(index, ref desc);
                switch (desc.Type)
                {
                    case EShaderVarType.SVT_Float1:
                        {
                            float v = System.Convert.ToSingle(i.Value);
                            cBuffer.SetValue(index, ref v);
                        }
                        break;
                    case EShaderVarType.SVT_Float2:
                        {
                            Vector2 v = Vector2.FromString(i.Value);
                            cBuffer.SetValue(index, ref v);
                        }
                        break;
                    case EShaderVarType.SVT_Float3:
                        {
                            Vector3 v = Vector3.FromString(i.Value);
                            cBuffer.SetValue(index, ref v);
                        }
                        break;
                    case EShaderVarType.SVT_Float4:
                        {
                            Vector4 v = Vector4.FromString(i.Value);
                            cBuffer.SetValue(index, ref v);
                        }
                        break;
                }
            }
        }
        #endregion

        #region RHIResource
        [Browsable(false)]
        public RHI.CConstantBuffer PerMaterialCBuffer { get; set; }
        [Rtti.Meta]
        [Category("State")]
        public IRasterizerStateDesc Rasterizer
        {
            get
            {
                if (RasterizerState == null)
                {
                    IRasterizerStateDesc desc = new IRasterizerStateDesc();
                    desc.SetDefault();
                    return desc;
                }
                return RasterizerState.Desc;
            }
            set
            {
                GamePlay.Action.UAction.OnChanged(this, this, "Rasterizer", Rasterizer, value);
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                RasterizerState = UEngine.Instance.GfxDevice.RasterizerStateManager.GetPipelineState(rc, ref value);
                SerialId++;
            }
        }
        protected RHI.CRasterizerState mRasterizerState;
        [Browsable(false)]
        public virtual RHI.CRasterizerState RasterizerState
        {
            get => mRasterizerState;
            protected set => mRasterizerState = value;
        }
        [Rtti.Meta]
        [Category("State")]
        public IDepthStencilStateDesc DepthStencil
        {
            get
            {
                if (mDepthStencilState == null)
                {
                    IDepthStencilStateDesc desc = new IDepthStencilStateDesc();
                    desc.SetDefault();
                    return desc;
                }
                return mDepthStencilState.Desc;
            }
            set
            {
                GamePlay.Action.UAction.OnChanged(this, this, "DepthStencil", DepthStencil, value);
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                DepthStencilState = UEngine.Instance.GfxDevice.DepthStencilStateManager.GetPipelineState(rc, ref value);
                SerialId++;
            }
        }
        protected RHI.CDepthStencilState mDepthStencilState;
        [Browsable(false)]
        public virtual RHI.CDepthStencilState DepthStencilState
        {
            get => mDepthStencilState;
            protected set => mDepthStencilState = value;
        }
        [Rtti.Meta]
        [Category("State")]
        public IBlendStateDesc Blend
        {
            get
            {
                if (BlendState == null)
                {
                    IBlendStateDesc desc = new IBlendStateDesc();
                    desc.SetDefault();
                    return desc;
                }
                return BlendState.Desc;
            }
            set
            {
                GamePlay.Action.UAction.OnChanged(this, this, "Blend", Blend, value);
                var rc = UEngine.Instance.GfxDevice.RenderContext;
                BlendState = UEngine.Instance.GfxDevice.BlendStateManager.GetPipelineState(rc, ref value);
                SerialId++;
            }
        }
        protected RHI.CBlendState mBlendState;
        [Browsable(false)]
        public virtual RHI.CBlendState BlendState
        {
            get => mBlendState;
            protected set => mBlendState = value;
        }
        #endregion
    }
    public class UMaterialManager
    {
        public void Cleanup()
        {
            Materials.Clear();
        }
        public UMaterial NullMaterial = new UMaterial();
        public Dictionary<RName, UMaterial> Materials { get; } = new Dictionary<RName, UMaterial>();
        public async System.Threading.Tasks.Task<UMaterial> GetMaterial(RName rn)
        {
            if (rn == null)
                return null;

            UMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UMaterial.LoadXnd(this, xnd.RootNode);
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
