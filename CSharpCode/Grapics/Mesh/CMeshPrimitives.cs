using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta]
    public class CMeshPrimitivesAMeta : IO.IAssetMeta
    {
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
    [CMeshPrimitives.Import]
    [IO.AssetCreateMenu(MenuName = "Mesh")]
    public partial class CMeshPrimitives : AuxPtrType<IMeshPrimitives>, IO.IAsset
    {
        public const string AssetExt = ".vms";

        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = ImGui.ImGuiFileDialog.CreateInstance();
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override void DoCreate(RName dir, Rtti.UTypeDesc type, string ext)
            {
                mDir = dir;
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe void OnDraw(EGui.Controls.ContentBrowser ContentBrowser)
            {
                //we also can import from other types
                FBXCreateCreateDraw(ContentBrowser);
            }

            public unsafe partial void FBXCreateCreateDraw(EGui.Controls.ContentBrowser ContentBrowser);
        }
        public CMeshPrimitives()
        {
            mCoreObject = IMeshPrimitives.CreateInstance();
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
            //这里需要存盘的情况很少，正常来说vms是fbx导入的时候生成的，不是保存出来的
            var rc = UEngine.Instance?.GfxDevice.RenderContext;
            var xnd = new IO.CXndHolder("CMeshPrimitives", 0, 0);
            unsafe
            {
                mCoreObject.Save2Xnd(rc.mCoreObject, xnd.RootNode.mCoreObject);
            }

            xnd.SaveXnd(name.Address);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        public Animation.Skeleton.CPartialSkeleton PartialSkeleton
        {
            get { return Animation.Skeleton.CPartialSkeleton.Create(mCoreObject.GetPartialSkeleton()); }
        }
        public static CMeshPrimitives LoadXnd(UMeshPrimitiveManager manager, IO.CXndHolder xnd)
        {
            var result = new CMeshPrimitives();
            unsafe
            {
                var ret = result.mCoreObject.LoadXnd(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, "", xnd.mCoreObject, true);
                if (ret == 0)
                    return null;
                return result;
            }
        }
    }
    public class UMeshPrimitiveManager
    {
        public Dictionary<RName, CMeshPrimitives> Meshes { get; } = new Dictionary<RName, CMeshPrimitives>();
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
