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
    public class CMeshPrimitives : AuxPtrType<IMeshPrimitives>, IO.IAsset
    {
        public const string AssetExt = ".vms";

        public class ImportAttribute : IO.CommonCreateAttribute
        {
            ~ImportAttribute()
            {
                mFileDialog.Dispose();
            }
            string mSourceFile;
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
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
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import MeshPrimitives", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var visible = true;
                if (ImGuiAPI.BeginPopupModal($"Import MeshPrimitives", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var sz = new Vector2(-1, 0);
                    if (ImGuiAPI.Button("Select FBX", ref sz))
                    {
                        mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".FBX,.fbx", ".");
                    }
                    // display
                    if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                    {
                        // action if OK
                        if (mFileDialog.IsOk() == true)
                        {
                            mSourceFile = mFileDialog.GetFilePathName();
                            string filePath = mFileDialog.GetCurrentPath();
                            if(!string.IsNullOrEmpty(mSourceFile))
                            {
                                mFBXImporter = UEngine.Instance.FBXFactoryModule.Instance.CreateImporter();
                                var fileDesc = mFBXImporter.PreImport(mSourceFile);
                                PGAsset.SingleTarget = fileDesc;
                                mName = IO.FileManager.GetPureName(mSourceFile);
                            }
                        }
                        // close
                        mFileDialog.CloseDialog();
                    }
                    if (eErrorType != enErrorType.None)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(ref clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(ref clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();

                    var buffer = BigStackBuffer.CreateInstance(256);
                    buffer.SetText(mName);
                    ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                    var name = buffer.AsText();
                    if (mName != name)
                    {
                        mName = name;
                        if (IO.FileManager.FileExists(mDir.Address + mName + RHI.CShaderResourceView.AssetExt))
                            eErrorType = enErrorType.IsExisting;
                    }
                    buffer.DestroyMe();

                    ImGuiAPI.Separator();
                    if (PGAsset.SingleTarget != null)
                    {
                        PGAsset.OnDraw(false, false, false);
                    }
                    ImGuiAPI.Separator();

                    sz = new Vector2(0, 0);
                    if (eErrorType == enErrorType.None)
                    {
                        if (ImGuiAPI.Button("Create Asset", ref sz))
                        {
                            if (ImportMesh())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                ContentBrowser.mAssetImporter = null;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", ref sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }
                    ImGuiAPI.EndPopup();
                }
            }
            private unsafe bool ImportMesh()
            {
                var fileDesc = mFBXImporter.GetFileImportDesc();
                for(uint i = 0;i< fileDesc.MeshNum;++i)
                {
                    using (var meshImporter = mFBXImporter.CreateMeshImporter(i))
                    {
                        meshImporter.Process(UEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                        var mesh = meshImporter.GetMeshPrimitives();
                        var rn = RName.GetRName(mDir.Name + mName + CMeshPrimitives.AssetExt);
                        var xnd = new IO.CXndHolder("CMeshPrimitives", 0, 0);
                        mesh.Save2Xnd(UEngine.Instance.GfxDevice.RenderContext.mCoreObject, xnd.RootNode.mCoreObject);
                        xnd.SaveXnd(rn.Address);
                        
                        var ameta = new CMeshPrimitivesAMeta();
                        ameta.SetAssetName(rn);
                        ameta.AssetId = Guid.NewGuid();
                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(CMeshPrimitives));
                        ameta.Description = $"This is a {typeof(CMeshPrimitives).FullName}\n";
                        ameta.SaveAMeta();
                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                        var partialSkeleton = meshImporter.GetPartialSkeleton();
                        Animation.Skeleton.USkeletonAsset ska = new Animation.Skeleton.USkeletonAsset();
                        ska.Skeleton.MergeWith(partialSkeleton);
                        rn = RName.GetRName(mDir.Name + mName + Animation.Skeleton.USkeletonAsset.AssetExt);
                        ska.SaveAssetTo(rn);
                        var sktameta = new Animation.Skeleton.USkeletonAssetAMeta();
                        sktameta.SetAssetName(rn);
                        sktameta.AssetId = Guid.NewGuid();
                        sktameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(Animation.Skeleton.USkeletonAssetAMeta));
                        sktameta.Description = $"This is a {typeof(Animation.Skeleton.USkeletonAssetAMeta).FullName}\n";
                        sktameta.SaveAMeta();
                        UEngine.Instance.AssetMetaManager.RegAsset(sktameta);

                        mesh.NativeSuper.Release();
                    }
                }
                mFBXImporter.Dispose();
                return true;
            }
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
