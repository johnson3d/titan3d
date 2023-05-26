using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    [Rtti.Meta]
    public class UPhyMaterialAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UPhyMaterial.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //物理材质不会引用别的资产
            return false;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.GetMaterial(GetAssetName());
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    UEngine.Instance.EditorInstance.PhyMaterialIcon?.OnDraw(cmdlist, in start, in end, 0);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "PhyMtl", null);
        //}
        public override string GetAssetTypeName()
        {
            return "PhyMtl";
        }
    }
    [Rtti.Meta]
    [UPhyMaterial.UPhyMaterialImport]
    [IO.AssetCreateMenu(MenuName = "PhysicsMaterial")]
    [Editor.UAssetEditor(EditorType = typeof(UPhyMaterialEditor))]
    public class UPhyMaterial : AuxPtrType<PhyMaterial>, IO.IAsset, IO.ISerializer
    {
        public const string AssetExt = ".pxmtl";
        public class UPhyMaterialImportAttribute : IO.CommonCreateAttribute
        {
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                PGAssetInitTask = PGAsset.Initialize();
                mAsset = UEngine.Instance.PhyModule.PhyContext.CreateMaterial(0, 0, 0);
                PGAsset.Target = mAsset;
            }
            protected override bool CheckAsset()
            {
                return true;
            }
        }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        #endregion

        #region IAsset
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UPhyMaterialAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
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
        public static UPhyMaterial LoadXnd(UPhyMaterialManager manager, IO.TtXndNode node)
        {
            UPhyMaterial result = UEngine.Instance.PhyModule.PhyContext.CreateMaterial(0, 0, 0);
            if (ReloadXnd(result, manager, node) == false)
                return null;
            return result;
        }
        public static bool ReloadXnd(UPhyMaterial material, UPhyMaterialManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("Material");
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
        public RName AssetName
        {
            get;
            set;
        }
        #endregion
        public UPhyMaterial(PhyMaterial self)
        {
            mCoreObject = self;
        }
        [Rtti.Meta]
        public float DynamicFriction
        {
            get { return mCoreObject.GetDynamicFriction(); }
            set { mCoreObject.SetDynamicFriction(value); }
        }
        [Rtti.Meta]
        public float StaticFriction
        {
            get { return mCoreObject.GetStaticFriction(); }
            set { mCoreObject.SetStaticFriction(value); }
        }
        [Rtti.Meta]
        public float Restitution
        {
            get { return mCoreObject.GetRestitution(); }
            set { mCoreObject.SetRestitution(value); }
        }
    }

    public class UPhyMaterialManager
    {
        public void Cleanup()
        {
            Materials.Clear();
        }
        UPhyMaterial mDefaultMaterial;
        public UPhyMaterial DefaultMaterial
        {
            get
            {
                if (mDefaultMaterial == null)
                {
                    mDefaultMaterial = UEngine.Instance.PhyModule.PhyContext.CreateMaterial(1, 1, 0.6f);
                }
                return mDefaultMaterial;
            }
        }
        public Dictionary<RName, UPhyMaterial> Materials { get; } = new Dictionary<RName, UPhyMaterial>();
        public UPhyMaterial GetMaterialSync(RName rn)
        {
            if (rn == null)
                return null;

            UPhyMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                if (xnd != null)
                {
                    var material = UPhyMaterial.LoadXnd(this, xnd.RootNode);
                    if (material == null)
                        return null;

                    material.AssetName = rn;

                    Materials[rn] = material;
                    return material;
                }
                else
                {
                    return null;
                }
            }
        }
        public async System.Threading.Tasks.Task<UPhyMaterial> GetMaterial(RName rn)
        {
            if (rn == null)
                return null;

            UPhyMaterial result;
            if (Materials.TryGetValue(rn, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UPhyMaterial.LoadXnd(this, xnd.RootNode);
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
        public async System.Threading.Tasks.Task<UPhyMaterial> CreateMaterial(RName rn)
        {
            UPhyMaterial result;
            result = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var material = UPhyMaterial.LoadXnd(this, xnd.RootNode);
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
        public async System.Threading.Tasks.Task<bool> ReloadMaterial(RName rn)
        {
            UPhyMaterial result;
            if (Materials.TryGetValue(rn, out result) == false)
                return true;

            var ok = await UEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        return UPhyMaterial.ReloadXnd(result, this, xnd.RootNode);
                    }
                    else
                    {
                        return false;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            return ok;
        }
    }
}
