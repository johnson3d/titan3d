using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    [Rtti.Meta]
    public class UPhyTriMeshAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UPhyTriMesh.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //物理Mesh不会引用别的资产
            return false;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.PhyModue.PhyContext.PhyMeshManager.GetMesh(GetAssetName());
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            base.OnDrawSnapshot(in cmdlist, ref start, ref end);
            cmdlist.AddText(in start, 0xFFFFFFFF, "PhyMesh", null);
        }
    }
    [UPhyTriMesh.UPhyMeshImport]
    [IO.AssetCreateMenu(MenuName = "PhysicsMesh")]
    [Editor.UAssetEditor(EditorType = typeof(UPhyTriMeshEditor))]

    public class UPhyTriMesh : AuxPtrType<PhyTriMesh>, IO.IAsset, IO.ISerializer
    {
        public UPhyTriMesh(PhyTriMesh self)
        {
            mCoreObject = self;
        }
        public UPhyTriMesh()
        {
            mCoreObject = PhyTriMesh.CreateInstance();
        }
        public const string AssetExt = ".pxmesh";
        
        public class UPhyMeshImportAttribute : IO.CommonCreateAttribute
        {
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                ExtName = ext;
                mName = null;
                mDir = dir;
                TypeSlt.BaseType = type;
                TypeSlt.SelectedType = type;

                var task = PGAsset.Initialize();
                PGAsset.Target = this;
            }
            protected override bool CheckAsset()
            {
                if (mMesh == null || mMesh.MeshDataProvider == null)
                    return false;
                return true;
            }
            protected override bool DoImportAsset()
            {
                mAsset = UEngine.Instance.PhyModue.PhyContext.CookTriMesh(mMesh.MeshDataProvider, null, null, null);
                mAsset.AssetName = GetAssetRName();

                return base.DoImportAsset();
            }
            Graphics.Mesh.CMeshPrimitives mMesh;
            [Rtti.Meta]
            [RName.PGRName(FilterExts=Graphics.Mesh.CMeshPrimitives.AssetExt)]
            public RName MeshSource
            {
                get
                {
                    if (mMesh != null)
                        return mMesh.AssetName;
                    return null;
                }
                set
                {
                    Action action = async () =>
                    {
                        mMesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value);
                        await mMesh.LoadMeshDataProvider();
                    };
                    action();
                }
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
            var result = new UPhyTriMeshAMeta();
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
            var xnd = new IO.CXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("PhyTriMesh", 0, 0))
            {
                var ar = attr.GetWriter(512);
                ar.Write(this);
                attr.ReleaseWriter(ref ar);
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
        }
        public static UPhyTriMesh LoadXnd(UPhyMeshManager manager, IO.CXndNode node)
        {
            UPhyTriMesh result = new UPhyTriMesh();
            if (ReloadXnd(result, manager, node) == false)
                return null;
            return result;
        }
        public static bool ReloadXnd(UPhyTriMesh mesh, UPhyMeshManager manager, IO.CXndNode node)
        {
            var attr = node.TryGetAttribute("PhyTriMesh");
            if (attr.NativePointer != IntPtr.Zero)
            {
                var ar = attr.GetReader(null);
                try
                {
                    ar.ReadTo(mesh, null);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
                attr.ReleaseReader(ref ar);

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

        public class UMeshDataSave : IO.UCustomSerializerAttribute
        {
            public override unsafe void Save(IO.IWriter ar, object host, string propName)
            {
                System.Diagnostics.Debug.Assert(propName == "MeshDataSave");
                var mesh = host as UPhyTriMesh;
                var blob = mesh.mCoreObject.GetCookedData();
                ar.Write(blob.GetSize());
                ar.WritePtr(blob.GetData(), (int)blob.GetSize());
            }
            public override unsafe object Load(IO.IReader ar, object host, string propName)
            {
                System.Diagnostics.Debug.Assert(propName == "MeshDataSave");
                var mesh = host as UPhyTriMesh;
                var blob = new Support.CBlobObject();
                uint size;
                ar.Read(out size);
                blob.mCoreObject.ReSize(size);
                ar.ReadPtr(blob.mCoreObject.GetData(), (int)size);
                mesh.mCoreObject.CreateFromCookedData(UEngine.Instance.PhyModue.PhyContext.mCoreObject, blob.mCoreObject.GetData(), size);
                return null;
            }
        }
        [UMeshDataSave()]
        [Rtti.Meta]
        public object MeshDataSave
        {
            get { return null; }
            set {; }
        }

        public Graphics.Mesh.CMeshDataProvider ToMeshProvider()
        {
            var meshProvider = new Graphics.Mesh.CMeshDataProvider(mCoreObject.CreateMeshProvider());
            return meshProvider;
        }
    }
    public class UPhyMeshManager
    {
        public Dictionary<RName, UPhyTriMesh> TriMeshes { get; } = new Dictionary<RName, UPhyTriMesh>();
        public void Cleanup()
        {
            foreach (var i in TriMeshes)
            {

            }
            TriMeshes.Clear();
        }
        public UPhyTriMesh GetMeshSync(RName rn)
        {
            if (rn == null)
                return null;

            UPhyTriMesh result;
            if (TriMeshes.TryGetValue(rn, out result))
                return result;

            using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
            {
                if (xnd != null)
                {
                    var mesh = UPhyTriMesh.LoadXnd(this, xnd.RootNode);
                    if (mesh == null)
                        return null;

                    mesh.AssetName = rn;
                    TriMeshes[rn] = mesh;
                    return mesh;
                }
                else
                {
                    return null;
                }
            }
        }
        public async System.Threading.Tasks.Task<UPhyTriMesh> GetMesh(RName rn)
        {
            if (rn == null)
                return null;

            UPhyTriMesh result;
            if (TriMeshes.TryGetValue(rn, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = UPhyTriMesh.LoadXnd(this, xnd.RootNode);
                        if (mesh == null)
                            return null;

                        mesh.AssetName = rn;
                        return mesh;
                    }
                    else
                    {
                        return null;
                    }
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (result != null)
            {
                TriMeshes[rn] = result;
                return result;
            }

            return null;
        }
    }
}
