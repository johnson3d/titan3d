using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Mesh.UMeshPrimitivesAMeta@EngineCore" })]
    public class TtMeshPrimitivesAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return TtMeshPrimitives.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "VMS";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(GetAssetName());
        }
        public override void OnBeforeRenamedAsset(IO.IAsset asset, RName name)
        {
            CoreSDK.CheckResult(TtEngine.Instance.GfxDevice.MeshPrimitiveManager.UnsafeRemove(name) == asset);
        }
        public override void OnAfterRenamedAsset(IO.IAsset asset, RName name)
        {
            TtEngine.Instance.GfxDevice.MeshPrimitiveManager.UnsafeAdd(name, (TtMeshPrimitives)asset);
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            return false;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    base.OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "vms", null);
        //}
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.MeshPrimitivesBoderColor;
        }

        [Rtti.Meta]
        public bool IsClustered { get; set; } = false;
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Mesh.UMeshPrimitives@EngineCore" })]
    [TtMeshPrimitives.Import]
    [IO.AssetCreateMenu(MenuName = "Mesh/Mesh")]
    public partial class TtMeshPrimitives : AuxPtrType<NxRHI.FMeshPrimitives>, IO.IAsset
    {
        public const string AssetExt = ".vms";

        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                //mFileDialog.Dispose();
            }
            string mSourceFile;
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            //EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
                await PGAsset.Initialize();
                //mDesc.Desc.SetDefault();
                //PGAsset.SingleTarget = mDesc;
            }
            public override unsafe bool OnDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                //we also can import from other types
                //return FBXCreateCreateDraw(ContentBrowser);
                return AssimpCreateCreateDraw(ContentBrowser);
            }

            public unsafe partial bool FBXCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser);
            public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser);
        }
        public TtMeshPrimitives()
        {
            mCoreObject = NxRHI.FMeshPrimitives.CreateInstance();
        }
        public TtMeshPrimitives(string name, uint atom)
        {
            mCoreObject = NxRHI.FMeshPrimitives.CreateInstance();
            mCoreObject.Init(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, name, atom);
        }
        public TtMeshPrimitives(NxRHI.FMeshPrimitives iMeshPrimitives)
        {
            mCoreObject = iMeshPrimitives;
            System.Diagnostics.Debug.Assert(mCoreObject.IsValidPointer);
        }
        public bool Init(string name, uint atom)
        {
            return mCoreObject.Init(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, name, atom);
        }
        public void SetTransientVertexBuffer(NxRHI.TtTransientBuffer buffer)
        {
            mCoreObject.SetTransientVertexBuffer(buffer.mCoreObject);
        }
        public void SetTransientIndexBuffer(NxRHI.TtTransientBuffer buffer)
        {
            mCoreObject.SetTransientIndexBuffer(buffer.mCoreObject);
        }
        public void PushAtom(uint index, in EngineNS.NxRHI.FMeshAtomDesc desc)
        {
            mCoreObject.PushAtom(index, in desc);
        }
        public uint NumAtom
        {
            get
            {
                return mCoreObject.GetAtomNumber();
            }
        }
        #region IAsset
        public override void Dispose()
        {
            base.Dispose();
        }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtMeshPrimitivesAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();

            var meshMeta = ameta as TtMeshPrimitivesAMeta;
            if (meshMeta != null && meshMeta.IsClustered)
            {
                ameta.RefAssetRNames.Add(RName.GetRName(AssetName.Name + ".clustermesh", AssetName.RNameType));
            }
        }
        public void SaveAssetTo(RName name)
        {
            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            //这里需要存盘的情况很少，正常来说vms是fbx导入的时候生成的，不是保存出来的
            var rc = TtEngine.Instance?.GfxDevice.RenderContext;
            var xnd = new IO.TtXndHolder("UMeshPrimitives", 0, 0);
            unsafe
            {
                mCoreObject.Save2Xnd(rc.mCoreObject, xnd.RootNode.mCoreObject);
            }
            var attr = xnd.RootNode.mCoreObject.GetOrAddAttribute("PartialSkeleton",0,0);
            using (var ar = attr.GetWriter(512))
            {
                ar.Write(PartialSkeleton);
            }
            xnd.SaveXnd(name.Address);
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        [Rtti.Meta]
        public Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton PartialSkeleton
        {
            get;
            set;
        }
        public unsafe static TtMeshPrimitives LoadXnd(UMeshPrimitiveManager manager, IO.TtXndHolder xnd)
        {
            var result = new TtMeshPrimitives();
            
            try
            {
                var ret = result.mCoreObject.LoadXnd(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, "", xnd.mCoreObject, true);
                if (ret == false)
                    return null;
                var attr = xnd.RootNode.mCoreObject.TryGetAttribute("PartialSkeleton");
                if (attr.IsValidPointer)
                {
                    IO.ISerializer partialSkeleton = null;
                    using (var ar = attr.GetReader(manager))
                    {
                        try
                        {
                            ar.Read(out partialSkeleton, manager);
                        }
                        catch (Exception exp)
                        {
                            Profiler.Log.WriteException(exp);
                        }
                    }
                    if(partialSkeleton is Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton)
                    {
                        result.PartialSkeleton = partialSkeleton as Animation.SkeletonAnimation.Skeleton.TtSkinSkeleton;
                    }
                }
                return result;
            }
            catch (Exception exp)
            {
                Profiler.Log.WriteException(exp);
                return null;
            }
        }

        private UMeshDataProvider mMeshDataProvider;
        public async System.Threading.Tasks.Task LoadMeshDataProvider()
        {
            if (mMeshDataProvider != null || AssetName == null)
                return;

            if (mMeshDataProvider == null)
            {
                var result = await TtEngine.Instance.EventPoster.Post((state) =>
                {
                    using (var xnd = IO.TtXndHolder.LoadXnd(AssetName.Address))
                    {
                        if (xnd != null)
                        {
                            var tmp = new UMeshDataProvider();

                            var ok = tmp.mCoreObject.LoadFromMeshPrimitive(xnd.RootNode.mCoreObject, NxRHI.EVertexStreamType.VST_FullMask);
                            if (ok == false)
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
        public UMeshDataProvider MeshDataProvider
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
            //foreach (var i in Meshes)
            //{
            //    int n = i.Value.Core_UnsafeGetRefCount();
            //    if (n != 1)
            //    {

            //    }
            //    else
            //    {
            //        i.Value.Dispose();
            //    }
            //}
            Meshes.Clear();
        }
        TtMeshPrimitives mUnitSphere;
        public TtMeshPrimitives UnitSphere
        {
            get
            {
                if (mUnitSphere == null)
                {
                    mUnitSphere = Graphics.Mesh.UMeshDataProvider.MakeSphere(1.0f, 15, 15, 0xfffffff).ToMesh();
                }
                return mUnitSphere;
            }
        }
        TtMeshPrimitives mUnitBox;
        public TtMeshPrimitives UnitBox
        {
            get
            {
                if (mUnitBox == null)
                {
                    mUnitBox = Graphics.Mesh.UMeshDataProvider.MakeBox(0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0xfffffff).ToMesh();
                }
                return mUnitBox;
            }
        }
        public Dictionary<RName, TtMeshPrimitives> Meshes { get; } = new Dictionary<RName, TtMeshPrimitives>();
        public async System.Threading.Tasks.Task Initialize()
        {
            var t = GetMeshPrimitive(RName.GetRName("axis/movex.vms", RName.ERNameType.Engine));
            await t;
        }
        public TtMeshPrimitives FindMeshPrimitive(RName name)
        {
            TtMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result))
                return result;
            return null;
        }
        internal TtMeshPrimitives UnsafeRemove(RName name)
        {
            lock (Meshes)
            {
                if (Meshes.TryGetValue(name, out var result))
                {
                    return result;
                }
                return null;
            }
        }
        internal void UnsafeAdd(RName name, TtMeshPrimitives obj)
        {
            lock (Meshes)
            {
                Meshes.Add(name, obj);
            }
        }
        //public async System.Threading.Tasks.Task<UMeshPrimitives> GetMeshPrimitive(RName name)
        public async Thread.Async.TtTask<TtMeshPrimitives> GetMeshPrimitive(RName name)
        {
            if (name == null)
                return null;
            TtMeshPrimitives result;
            if (Meshes.TryGetValue(name, out result))
                return result;

            result = await CreateMeshPrimitive(name);

            if (result != null)
            {
                Meshes[name] = result;
                return result;
            }

            return null;
        }
        public async Thread.Async.TtTask<TtMeshPrimitives> CreateMeshPrimitive(RName name)
        {
            TtMeshPrimitives result;
            result = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
                {
                    if (xnd != null)
                    {
                        var mesh = TtMeshPrimitives.LoadXnd(this, xnd);
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
                await result.TryLoadClusteredMesh();
                return result;
            }
            return null;
        }
        public void UnsafeRenameForCook(RName name, RName newName)
        {
            TtMeshPrimitives result;
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
    public partial class TtGfxDevice
    {
        public Mesh.UMeshPrimitiveManager MeshPrimitiveManager { get; } = new Mesh.UMeshPrimitiveManager();
    }
}
