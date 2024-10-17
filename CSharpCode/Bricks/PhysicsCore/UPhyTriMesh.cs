using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    [Rtti.Meta]
    public class TtPhyTriMeshAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtPhyTriMesh.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //物理Mesh不会引用别的资产
            return false;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.PhyModule.PhyContext.PhyMeshManager.GetMesh(GetAssetName());
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "PhyMesh", null);
        //}
        public override string GetAssetTypeName()
        {
            return "Physic Mesh";
        }
    }
    [TtPhyTriMesh.UPhyMeshImport]
    [IO.AssetCreateMenu(MenuName = "Physics/PhysicsMesh")]
    [Editor.UAssetEditor(EditorType = typeof(UPhyTriMeshEditor))]

    public class TtPhyTriMesh : AuxPtrType<PhyTriMesh>, IO.IAsset, IO.ISerializer
    {
        public TtPhyTriMesh(PhyTriMesh self)
        {
            mCoreObject = self;
        }
        public TtPhyTriMesh()
        {
            mCoreObject = PhyTriMesh.CreateInstance();
        }
        public const string AssetExt = ".pxmesh";
        public string TypeExt { get => AssetExt; }
        public class UPhyMeshImportAttribute : IO.CommonCreateAttribute
        {
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
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
                mAsset = TtEngine.Instance.PhyModule.PhyContext.CookTriMesh(mMesh.MeshDataProvider, null, null, null);
                mAsset.AssetName = GetAssetRName();
                TtEngine.Instance.SourceControlModule.AddFile(mAsset.AssetName.Address);

                return base.DoImportAsset();
            }
            Graphics.Mesh.TtMeshPrimitives mMesh;
            [Rtti.Meta]
            [RName.PGRName(FilterExts=Graphics.Mesh.TtMeshPrimitives.AssetExt)]
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
                        mMesh = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(value);
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
            var result = new TtPhyTriMeshAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var typeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(this.GetType());
            var xnd = new IO.TtXndHolder(typeStr, 0, 0);
            using (var attr = xnd.NewAttribute("PhyTriMesh", 0, 0))
            {
                using (var ar = attr.GetWriter(512))
                {
                    ar.Write(this);
                }
                xnd.RootNode.AddAttribute(attr);
            }

            xnd.SaveXnd(name.Address);
            TtEngine.Instance.SourceControlModule.AddFile(name.Address);
        }
        public static TtPhyTriMesh LoadXnd(UPhyMeshManager manager, IO.TtXndNode node)
        {
            TtPhyTriMesh result = new TtPhyTriMesh();
            if (ReloadXnd(result, manager, node) == false)
                return null;
            return result;
        }
        public static bool ReloadXnd(TtPhyTriMesh mesh, UPhyMeshManager manager, IO.TtXndNode node)
        {
            var attr = node.TryGetAttribute("PhyTriMesh");
            if (attr.NativePointer != IntPtr.Zero)
            {
                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadTo(mesh, null);
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

        public class UMeshDataSave : IO.UCustomSerializerAttribute
        {
            public override unsafe void Save(IO.IWriter ar, object host, string propName)
            {
                System.Diagnostics.Debug.Assert(propName == "MeshDataSave");
                var mesh = host as TtPhyTriMesh;
                var blob = mesh.mCoreObject.GetCookedData();
                ar.Write(blob.GetSize());
                ar.WritePtr(blob.GetData(), (int)blob.GetSize());
            }
            public override unsafe object Load(IO.IReader ar, object host, string propName)
            {
                System.Diagnostics.Debug.Assert(propName == "MeshDataSave");
                var mesh = host as TtPhyTriMesh;
                var blob = new Support.TtBlobObject();
                uint size;
                ar.Read(out size);
                blob.mCoreObject.ReSize(size);
                ar.ReadPtr(blob.mCoreObject.GetData(), (int)size);
                mesh.mCoreObject.CreateFromCookedData(TtEngine.Instance.PhyModule.PhyContext.mCoreObject, blob.mCoreObject.GetData(), size);
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

        public Graphics.Mesh.TtMeshDataProvider ToMeshProvider()
        {
            var meshProvider = new Graphics.Mesh.TtMeshDataProvider(mCoreObject.CreateMeshProvider());
            return meshProvider;
        }
    }
    public class UPhyMeshManager
    {
        public Dictionary<RName, TtPhyTriMesh> TriMeshes { get; } = new Dictionary<RName, TtPhyTriMesh>();
        public void Cleanup()
        {
            foreach (var i in TriMeshes)
            {

            }
            TriMeshes.Clear();
        }
        public TtPhyTriMesh GetMeshSync(RName rn)
        {
            if (rn == null)
                return null;

            TtPhyTriMesh result;
            if (TriMeshes.TryGetValue(rn, out result))
                return result;

            using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
            {
                if (xnd != null)
                {
                    var mesh = TtPhyTriMesh.LoadXnd(this, xnd.RootNode);
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
        public async System.Threading.Tasks.Task<TtPhyTriMesh> GetMesh(RName rn)
        {
            if (rn == null)
                return null;

            TtPhyTriMesh result;
            if (TriMeshes.TryGetValue(rn, out result))
                return result;

            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(rn.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = TtPhyTriMesh.LoadXnd(this, xnd.RootNode);
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
