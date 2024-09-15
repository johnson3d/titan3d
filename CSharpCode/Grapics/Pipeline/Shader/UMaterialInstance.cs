﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterialInstanceAMeta@EngineCore" })]
    public partial class TtMaterialInstanceAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtMaterialInstance.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "MInst";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(GetAssetName());
        }
        public override void OnBeforeRenamedAsset(IO.IAsset asset, RName name)
        {
            CoreSDK.CheckResult(TtEngine.Instance.GfxDevice.MaterialInstanceManager.UnsafeRemove(name) == asset);
        }
        public override void OnAfterRenamedAsset(IO.IAsset asset, RName name)
        {
            TtEngine.Instance.GfxDevice.MaterialInstanceManager.UnsafeAdd(name, (TtMaterialInstance)asset);
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            if (ameta.GetAssetExtType() == NxRHI.TtSrView.AssetExt)
                return true;
            return false;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "MInst", null);
        //}
        public override void ResetSnapshot()
        {
            HasSnapshot = true;
            OnShowIconTimout(0);
        }
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MaterialInstanceBoderColor;
        }
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterialInstance@EngineCore" })]
    [TtMaterialInstance.MaterialInstanceImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/MaterialInstance")]
    public partial class TtMaterialInstance : TtMaterial
    {
        public new const string AssetExt = ".uminst";

        public class MaterialInstanceImportAttribute : IO.CommonCreateAttribute
        {
            protected override bool CheckAsset()
            {
                var material = mAsset as TtMaterialInstance;
                if (material == null)
                    return false;

                if (material.ParentMaterial == null)
                    return false;

                return true;
            }
        }
        #region IAsset
        public override IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMaterialInstanceAMeta();
            return result;
        }
        public override IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public override void SaveAssetTo(RName name)
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
                using (var attr = xnd.NewAttribute("MaterialInstance", 0, 0))
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
        public static bool ReloadXnd(TtMaterialInstance material, TtMaterialInstanceManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("MaterialInstance");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(material, null);
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
        public static TtMaterialInstance LoadXnd(TtMaterialInstanceManager manager, IO.TtXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("MaterialInstance");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.Read(out result, null);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }

            var material = result as TtMaterialInstance;
            if (material != null)
            {
                return material;
            }
            return null;
        }
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            ameta.AddReferenceAsset(MaterialName);
            foreach (var i in UsedRSView)
            {
                if (i.Value == null)
                    continue;
                ameta.AddReferenceAsset(i.Value);
            }
        }
        #endregion        
        public static TtMaterialInstance CreateMaterialInstance(TtMaterial mtl)
        {
            var result = new TtMaterialInstance();
            result.ParentMaterial = mtl;
            result.AssetState = IO.EAssetState.LoadFinished;
            
            foreach (var i in mtl.UsedRSView)
            {
                result.UsedRSView.Add(i.Clone(result));
            }

            foreach (var i in mtl.UsedSamplerStates)
            {
                result.UsedSamplerStates.Add(i.Clone(result));
            }

            foreach (var i in mtl.UsedUniformVars)
            {
                result.UsedUniformVars.Add(i.Clone(result));
            }

            result.Rasterizer = mtl.Rasterizer;
            result.DepthStencil = mtl.DepthStencil;
            result.Blend = mtl.Blend;

            result.SerialId++;
            return result;
        }
        public TtMaterialInstance CloneMaterialInstance()
        {
            var result = new TtMaterialInstance();
            result.AssetName = AssetName;
            result.ParentMaterial = ParentMaterial;
            result.MaterialHash = MaterialHash;
            result.RenderLayer = RenderLayer;

            foreach (var i in this.UsedRSView)
            {
                result.UsedRSView.Add(i.Clone(result));
            }

            foreach (var i in this.UsedSamplerStates)
            {
                result.UsedSamplerStates.Add(i.Clone(result));
            }

            foreach (var i in this.UsedUniformVars)
            {
                result.UsedUniformVars.Add(i.Clone(result));
            }

            result.mPipelineDesc = mPipelineDesc;
            result.UpdatePipeline();

            result.SerialId++;
            result.AssetState = IO.EAssetState.LoadFinished;
            return result;
        }
        [Browsable(false)]
        public IO.EAssetState AssetState { get; private set; } = IO.EAssetState.Initialized;
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Shader.UMaterialInstance.TSaveData@EngineCore" })]
        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public RName MaterialName { get; set; }
        }
        [Rtti.Meta(Order = 1)]
        [Browsable(false)]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.MaterialName = MaterialName;
                return tmp;
            }
            set
            {
                if (AssetState == IO.EAssetState.Loading)
                    return;
                AssetState = IO.EAssetState.Loading;
                System.Action exec = async () =>
                {
                    ParentMaterial = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(value.MaterialName);
                    if (ParentMaterial == null)
                    {
                        ParentMaterial = TtEngine.Instance.GfxDevice.MaterialManager.PxDebugMaterial;
                    }
                    bool isMatch = true;
                    #region UniformVars match parent
                    if (mUsedUniformVars.Count != ParentMaterial.UsedUniformVars.Count)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < mUsedUniformVars.Count; i++)
                        {
                            if (mUsedUniformVars[i].Name != ParentMaterial.UsedUniformVars[i].Name ||
                                    mUsedUniformVars[i].VarType != ParentMaterial.UsedUniformVars[i].VarType)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    if (isMatch == false)
                    {
                        var uniformVars = new List<NameValuePair>();
                        for (int i = 0; i < ParentMaterial.UsedUniformVars.Count; i++)
                        {
                            var uuv = this.FindVar(ParentMaterial.UsedUniformVars[i].Name);
                            if (uuv != null && uuv.VarType == ParentMaterial.UsedUniformVars[i].VarType)
                            {
                                uniformVars.Add(uuv);
                            }
                            else
                            {
                                uniformVars.Add(ParentMaterial.UsedUniformVars[i]);
                            }
                        }
                        mUsedUniformVars = uniformVars;
                    }
                    #endregion

                    #region SRV match parent
                    isMatch = true;
                    if (mUsedRSView.Count != ParentMaterial.UsedRSView.Count)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < mUsedRSView.Count; i++)
                        {
                            if (mUsedRSView[i].Name != ParentMaterial.UsedRSView[i].Name)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    if (isMatch == false)
                    {
                        var srvs = new List<NameRNamePair>();
                        for (int i = 0; i < ParentMaterial.UsedRSView.Count; i++)
                        {
                            var uuv = this.FindSRV(ParentMaterial.UsedRSView[i].Name);
                            if (uuv != null)
                            {
                                srvs.Add(uuv);
                            }
                            else
                            {
                                srvs.Add(ParentMaterial.UsedRSView[i]);
                            }
                        }
                        mUsedRSView = srvs;
                    }
                    #endregion

                    #region Samppler match parent
                    isMatch = true;
                    if (mUsedSamplerStates.Count != ParentMaterial.UsedSamplerStates.Count)
                    {
                        isMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < mUsedSamplerStates.Count; i++)
                        {
                            if (mUsedSamplerStates[i].Name != ParentMaterial.UsedSamplerStates[i].Name)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
                    if (isMatch == false)
                    {
                        var srvs = new List<NameSamplerStateDescPair>();
                        for (int i = 0; i < ParentMaterial.UsedSamplerStates.Count; i++)
                        {
                            var uuv = this.FindSampler(ParentMaterial.UsedSamplerStates[i].Name);
                            if (uuv != null)
                            {
                                srvs.Add(uuv);
                            }
                            else
                            {
                                srvs.Add(ParentMaterial.UsedSamplerStates[i]);
                            }
                        }
                        mUsedSamplerStates = srvs;
                    }
                    #endregion
                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
                
            }
        }
        [Category("Option")]
        [RName.PGRName(FilterExts = TtMaterial.AssetExt)]
        public RName MaterialName
        {
            get
            {
                if (ParentMaterial == null)
                    return null;
                return ParentMaterial.AssetName;
            }
            set
            {
                if (AssetState == IO.EAssetState.Loading)
                    return;
                AssetState = IO.EAssetState.Loading;
                System.Action exec = async () =>
                {
                    ParentMaterial = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(value);
                    AssetState = IO.EAssetState.LoadFinished;
                };
                exec();
            }
        }
        TtMaterial mParentMaterial;
        [Browsable(false)]
        public override TtMaterial ParentMaterial
        {
            get => mParentMaterial;
            protected set
            {
                mParentMaterial = value;
                if (value != null)
                    ParentMaterialSerialId = value.SerialId;
            }
        }

        #region override for Material settings
        public override ERenderFlags RenderFlags { get => mParentMaterial.RenderFlags; }
        public override bool DisableEnvColor
        {
            get
            {
                if (mParentMaterial == null)
                    return false;
                return mParentMaterial.DisableEnvColor;
            }
            set
            {
                
            }
        }
        public override ENormalMode NormalMode
        {
            get
            {
                if (mParentMaterial == null)
                    return ENormalMode.Normal;
                return mParentMaterial.NormalMode;
            }
            set
            {
                
            }
        }
        public override ELightingMode LightingMode
        {
            get
            {
                if (mParentMaterial == null)
                    return ELightingMode.Unlight;
                return mParentMaterial.LightingMode;
            }
            set
            {

            }
        }
        public override bool Is64bitVColorAlpha
        {
            get
            {
                if (mParentMaterial == null)
                    return false;
                return mParentMaterial.Is64bitVColorAlpha;
            }
            set
            {

            }
        }

        public override ERenderLayer RenderLayer
        {
            get
            {
                if (mParentMaterial == null)
                    return ERenderLayer.RL_Opaque;
                return mParentMaterial.RenderLayer;
            }
            set
            {
                
            }
        }
        public override bool AlphaTest
        {
            get
            {
                if (mParentMaterial == null)
                    return false;
                return mParentMaterial.AlphaTest;
            }
            set
            {

            }
        }
        [Browsable(false)]
        public override Hash160 MaterialHash
        {
            get
            {
                if (ParentMaterial == null)
                    return Hash160.Emtpy;
                return ParentMaterial.MaterialHash;
            }
        }
        
        internal uint ParentMaterialSerialId;
        [Browsable(false)]
        public override uint SerialId
        {
            get 
            {
                if (ParentMaterial != null && ParentMaterialSerialId != ParentMaterial.SerialId)
                {
                    ParentMaterialSerialId = ParentMaterial.SerialId;
                    if (PerMaterialCBuffer != null)
                        this.UpdateCBufferVars(PerMaterialCBuffer, PerMaterialCBuffer.ShaderBinder);
                    mSerialId++;
                    if (this.AssetName != null)
                        this.SaveAssetTo(this.AssetName);
                }
                return mSerialId;
            }
            set
            {
                mSerialId = value;
                if (PerMaterialCBuffer != null)
                    this.UpdateCBufferVars(PerMaterialCBuffer, PerMaterialCBuffer.ShaderBinder);
            }
        }
        #endregion
    }
    public class TtMaterialInstanceManager
    {
        TtMaterialInstance mWireColorMateria;
        public TtMaterialInstance WireColorMateria
        {
            get
            {
                return mWireColorMateria;
            }
        }
        public TtMaterialInstance WireVtxColorMateria
        {
            get;
            private set;
        }
        public async Thread.Async.TtTask<bool> Initialize(TtEngine engine)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            await GetMaterialInstance(RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine));
            await GetMaterialInstance(RName.GetRName("material/redcolor.uminst", RName.ERNameType.Engine));
            await GetMaterialInstance(RName.GetRName("axis/axis_x_d.uminst", RName.ERNameType.Engine));
            await GetMaterialInstance(RName.GetRName("axis/axis_face.uminst", RName.ERNameType.Engine));

            mWireColorMateria = await CreateMaterialInstance(RName.GetRName("material/whitecolor.uminst", RName.ERNameType.Engine));// engine.Config.DefaultMaterialInstance);
            if (mWireColorMateria == null)
                return false;

            var rast = mWireColorMateria.Rasterizer;
            rast.FillMode = NxRHI.EFillMode.FMD_WIREFRAME;
            rast.CullMode = NxRHI.ECullMode.CMD_NONE;
            mWireColorMateria.Rasterizer = rast;
            mWireColorMateria.RenderLayer = ERenderLayer.RL_Translucent;

            WireVtxColorMateria = await CreateMaterialInstance(RName.GetRName("material/wire_vfx_color.uminst", RName.ERNameType.Engine));

            return true;
        }
        public void Cleanup()
        {
            Materials.Clear();
        }
        public Dictionary<RName, TtMaterialInstance> Materials { get; } = new Dictionary<RName, TtMaterialInstance>();
        public TtMaterialInstance FindMaterialInstance(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterialInstance result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            var task = CreateMaterialInstance(rn);
            return null;
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
        internal void UnsafeAdd(RName name, TtMaterialInstance obj)
        {
            lock (Materials)
            {
                Materials.Add(name, obj);
            }
        }
        public async Thread.Async.TtTask<TtMaterialInstance> CreateMaterialInstance(RName rn)
        {
            var origin = await GetMaterialInstance(rn);
            if (origin == null)
                return null;
            return origin.CloneMaterialInstance();
            //UMaterialInstance result;
            //result = await TtEngine.Instance.EventPoster.Post((state) =>
            //{
            //    using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            //    {
            //        if (xnd != null)
            //        {
            //            var material = UMaterialInstance.LoadXnd(this, xnd.RootNode);
            //            if (material == null)
            //                return null;

            //            material.AssetName = rn;
            //            return material;
            //        }
            //        else
            //        {
            //            return null;
            //        }
            //    }
            //}, Thread.Async.EAsyncTarget.AsyncIO);
            //return result;
        }
        public async Thread.Async.TtTask<bool> ReloadMaterialInstance(RName rn)
        {
            TtMaterialInstance result;
            if (Materials.TryGetValue(rn, out result)==false)
                return true;

            var ok = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return TtMaterialInstance.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            return ok;
        }
        public async Thread.Async.TtTask<TtMaterialInstance> GetMaterialInstance(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterialInstance result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterialInstance.LoadXnd(this, xnd.RootNode);
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
