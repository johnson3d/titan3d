using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta]
    public class CMeshPrimitivesAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return CMeshPrimitives.AssetExt;
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(GetAssetName());
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            base.OnDrawSnapshot(in cmdlist, ref start, ref end);
            cmdlist.AddText(in start, 0xFFFFFFFF, "vms", null);
        }
    }

    [Rtti.Meta]
    [CMeshPrimitives.Import]
    [IO.AssetCreateMenu(MenuName = "Mesh")]
    public partial class CMeshPrimitives : AuxPtrType<IMeshPrimitives>, IO.IAsset
    {
        public const string AssetExt = ".vms";

        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                //mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                var noused = PGAsset.Initialize();
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe void OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                //we also can import from other types
                FBXCreateCreateDraw(ContentBrowser);
            }

            public unsafe partial void FBXCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser);
        }
        public CMeshPrimitives()
        {
            mCoreObject = IMeshPrimitives.CreateInstance();
        }
        public CMeshPrimitives(IMeshPrimitives iMeshPrimitives)
        {
            mCoreObject = iMeshPrimitives;
            System.Diagnostics.Debug.Assert(mCoreObject.IsValidPointer);
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new CMeshPrimitivesAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta();
            }
            //这里需要存盘的情况很少，正常来说vms是fbx导入的时候生成的，不是保存出来的
            var rc = UEngine.Instance?.GfxDevice.RenderContext;
            var xnd = new IO.CXndHolder("CMeshPrimitives", 0, 0);
            unsafe
            {
                mCoreObject.Save2Xnd(rc.mCoreObject, xnd.RootNode.mCoreObject);
            }
            var attr = xnd.RootNode.mCoreObject.GetOrAddAttribute("PartialSkeleton",0,0);
            var ar = attr.GetWriter(512);
            ar.Write(PartialSkeleton);
            attr.ReleaseWriter(ref ar);
            xnd.SaveXnd(name.Address);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        [Rtti.Meta]
        public Animation.SkeletonAnimation.Skeleton.USkinSkeleton PartialSkeleton
        {
            get;
            set;
        }
        public static CMeshPrimitives LoadXnd(UMeshPrimitiveManager manager, IO.CXndHolder xnd)
        {
            var result = new CMeshPrimitives();
            unsafe
            {
                var ret = result.mCoreObject.LoadXnd(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, "", xnd.mCoreObject, true);
                if (ret == 0)
                    return null;
                var attr = xnd.RootNode.mCoreObject.TryGetAttribute("PartialSkeleton");
                if (attr.IsValidPointer)
                {
                    var ar = attr.GetReader(manager);
                    IO.ISerializer partialSkeleton = null;
                    ar.Read(out partialSkeleton, manager);
                    attr.ReleaseReader(ref ar);
                    if(partialSkeleton is Animation.SkeletonAnimation.Skeleton.USkinSkeleton)
                    {
                        result.PartialSkeleton = partialSkeleton as Animation.SkeletonAnimation.Skeleton.USkinSkeleton;
                    }
                }
                return result;
            }
        }

        private CMeshDataProvider mMeshDataProvider;
        public async System.Threading.Tasks.Task LoadMeshDataProvider()
        {
            if (mMeshDataProvider != null || AssetName == null)
                return;

            if (mMeshDataProvider == null)
            {
                var result = await UEngine.Instance.EventPoster.Post(() =>
                {
                    using (var xnd = IO.CXndHolder.LoadXnd(AssetName.Address))
                    {
                        if (xnd != null)
                        {
                            var tmp = new CMeshDataProvider();

                            var ok = tmp.mCoreObject.LoadFromMeshPrimitive(xnd.RootNode.mCoreObject, EVertexStreamType.VST_FullMask);
                            if (ok == 0)
                                return false;

                            mMeshDataProvider = tmp;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }, Thread.Async.EAsyncTarget.AsyncIO);
            }
        }
        public CMeshDataProvider MeshDataProvider
        {
            get
            {
                return mMeshDataProvider;
            }
        }
    }
    public class UMeshPrimitiveManager
    {
        ~UMeshPrimitiveManager()
        {
            mUnitSphere?.Dispose();
            mUnitSphere = null;
        }
        CMeshPrimitives mUnitSphere;
        public CMeshPrimitives UnitSphere
        {
            get
            {
                if (mUnitSphere == null)
                {
                    mUnitSphere = Graphics.Mesh.CMeshDataProvider.MakeSphere(1.0f, 15, 15, 0xfffffff).ToMesh();
                }
                return mUnitSphere;
            }
        }
        CMeshPrimitives mUnitBox;
        public CMeshPrimitives UnitBox
        {
            get
            {
                if (mUnitBox == null)
                {
                    mUnitBox = Graphics.Mesh.CMeshDataProvider.MakeBox(0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0xfffffff).ToMesh();
                }
                return mUnitBox;
            }
        }
        public Dictionary<RName, CMeshPrimitives> Meshes { get; } = new Dictionary<RName, CMeshPrimitives>();
        public async System.Threading.Tasks.Task Initialize()
        {
            await GetMeshPrimitive(RName.GetRName("axis/movex.vms", RName.ERNameType.Engine));
        }
        public CMeshPrimitives FindMeshPrimitive(RName name)
        {
            CMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result))
                return result;
            return null;
        }
        public async System.Threading.Tasks.Task<CMeshPrimitives> GetMeshPrimitive(RName name)
        {
            CMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result))
                return result;

            result = await UEngine.Instance.EventPoster.Post(() =>
            {
                using (var xnd = IO.CXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = CMeshPrimitives.LoadXnd(this, xnd);
                        if (mesh == null)
                            return null;

                        mesh.AssetName = name;
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
                Meshes[name] = result;
                return result;
            }

            return null;
        }
        public void UnsafeRenameForCook(RName name, RName newName)
        {
            CMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result) == false)
                return;

            Meshes.Remove(name);
            result.GetAMeta().SetAssetName(newName);
            result.AssetName = newName;
            Meshes.Add(newName, result);
        }
    }
}

namespace EngineNS.Graphics.Pipeline
{
    public partial class UGfxDevice
    {
        public Mesh.UMeshPrimitiveManager MeshPrimitiveManager { get; } = new Mesh.UMeshPrimitiveManager();
    }
}
