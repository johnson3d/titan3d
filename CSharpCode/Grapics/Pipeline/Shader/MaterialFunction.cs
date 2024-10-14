using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader
{
    public partial class TtMaterialFunctionAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtMaterialFunction.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "MaterialFunction";
        }
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MaterialFunctionBoderColor;
        }
    }
    [TtMaterialFunction.MaterialFunctionImport]
    [IO.AssetCreateMenu(MenuName = "Graphics/MaterialFunction")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtMaterialFunction : IO.ISerializer, IO.IAsset, IShaderCodeProvider
    {
        public const string AssetExt = ".mtlfunc";
        public string TypeExt { get => AssetExt; }
        #region Import
        public class MaterialFunctionImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                await base.DoCreate(dir, type, ext);

                var material = (mAsset as TtMaterialFunction);
            }
        }
        #endregion
        #region ISerializer
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        #endregion
        #region IAsset
        public virtual IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMaterialFunctionAMeta();
            return result;
        }
        public virtual IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
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
                using (var attr = xnd.NewAttribute("MaterialFunction", 0, 0))
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
        public static TtMaterialFunction LoadXnd(TtMaterialFunctionManager manager, IO.TtXndNode node)
        {
            IO.ISerializer result = null;
            var attr = node.TryGetAttribute("MaterialFunction");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    ar.Read(out result, null);
                }
            }

            var material = result as TtMaterialFunction;
            if (material != null)
            {
                //material.UpdateShaderCode(false);
                return material;
            }
            return null;
        }
        public static bool ReloadXnd(TtMaterialFunction material, TtMaterialFunctionManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("MaterialFunction");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(material, null);
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
        #region IShaderCodeProvider
        public NxRHI.TtShaderCode DefineCode { get; } = new NxRHI.TtShaderCode();
        public NxRHI.TtShaderCode SourceCode { get; } = new NxRHI.TtShaderCode();
        #endregion

        [Rtti.Meta]
        public string GraphXMLString { get; set; }
    }
    public partial class TtMaterialFunctionManager
    {
        public Dictionary<RName, TtMaterialFunction> MaterialFunctions { get; } = new Dictionary<RName, TtMaterialFunction>();
        public async Thread.Async.TtTask<TtMaterialFunction> CreateMaterialFunction(RName rn)
        {
            TtMaterialFunction result;
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterialFunction.LoadXnd(this, xnd.RootNode);
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
        public async Thread.Async.TtTask<TtMaterialFunction> GetMaterialFunction(RName rn)
        {
            if (rn == null)
                return null;

            TtMaterialFunction result;
            if (MaterialFunctions.TryGetValue(rn, out result))
                return result;

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = TtMaterialFunction.LoadXnd(this, xnd.RootNode);
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
                MaterialFunctions[rn] = result;
                return result;
            }

            return null;
        }
        public async Thread.Async.TtTask<bool> ReloadMaterialFuntion(RName rn)
        {
            TtMaterialFunction result;
            if (MaterialFunctions.TryGetValue(rn, out result) == false)
                return true;

            var ok = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return TtMaterialFunction.ReloadXnd(result, this, xnd.RootNode);
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
                    //if (i.Value.Desc.MaterialHash != result.MaterialHash)
                    //{
                    //    await i.Value.RefreshEffect(result);
                    //}
                }
            }

            return ok;
        }
    }
    
}
