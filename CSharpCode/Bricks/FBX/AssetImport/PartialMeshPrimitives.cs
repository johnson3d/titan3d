
using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public partial class CMeshPrimitives
    {
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
            public unsafe partial void FBXCreateCreateDraw(ContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import MeshPrimitives", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
                var visible = true;
                if (ImGuiAPI.BeginPopupModal($"Import MeshPrimitives", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var sz = new Vector2(-1, 0);
                    if (ImGuiAPI.Button("Select FBX", in sz))
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
                            if (!string.IsNullOrEmpty(mSourceFile))
                            {
                                mFBXImporter = UEngine.Instance.FBXFactoryModule.Instance.CreateImporter();
                                var fileDesc = mFBXImporter.PreImport(mSourceFile);
                                PGAsset.Target = fileDesc;
                                mName = IO.FileManager.GetPureName(mSourceFile);
                            }
                        }
                        // close
                        mFileDialog.CloseDialog();
                    }
                    if (eErrorType != enErrorType.None)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
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
                    if (PGAsset.Target != null)
                    {
                        PGAsset.OnDraw(false, false, false);
                    }
                    ImGuiAPI.Separator();

                    sz = new Vector2(0, 0);
                    if (eErrorType == enErrorType.None)
                    {
                        if (ImGuiAPI.Button("Create Asset", in sz))
                        {
                            var task = FBXImportMesh();
                            ImGuiAPI.CloseCurrentPopup();
                            ContentBrowser.mAssetImporter = null;
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in sz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }
                    ImGuiAPI.EndPopup();
                }
            }

            private async System.Threading.Tasks.Task<bool> FBXImportMesh()
            {
                var fileDesc = mFBXImporter.GetFileImportDesc();
                for (uint i = 0; i < fileDesc.MeshNum; ++i)
                {
                    using (var meshImporter = mFBXImporter.CreateMeshImporter(i))
                    {
                        meshImporter.Process(UEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                        string meshName = "";
                        bool hasSkin = false;
                        unsafe
                        {
                            var meshDesc = mFBXImporter.GetFBXMeshDescs(i);
                            meshName = meshDesc->NativeSuper->Name.Text;
                            hasSkin = meshDesc->HaveSkin;
                        }
                        System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(meshName));
                        if (hasSkin)
                        {
                            var rn = RName.GetRName(mDir.Name + meshName + Animation.Asset.USkeletonAsset.AssetExt);
                            var fbxSkeletonDesc = meshImporter.GetSkeletonDesc();
                            await CreateOrMergeSkeleton(rn, fbxSkeletonDesc);
                        }
                        {
                            var rn = RName.GetRName(mDir.Name + meshName + CMeshPrimitives.AssetExt, mDir.RNameType);
                            CreateMesh(rn, meshImporter.GetMeshPrimitives(), hasSkin, meshImporter.GetSkeletonDesc());
                        }
                    }
                }
                mFBXImporter.Dispose();
                return true;
            }

            private async System.Threading.Tasks.Task CreateOrMergeSkeleton(RName skeletonAsset, AssetImportAndExport.FBX.FBXSkeletonDesc fBXSkeletonDesc)
            {
                var ska = await EngineNS.UEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(skeletonAsset);
                if (ska == null)
                {
                    Animation.Asset.USkeletonAsset newAsset = new Animation.Asset.USkeletonAsset();
                    newAsset.Skeleton = AssetImportAndExport.FBX.FBXMeshImportUtility.CreateSkinSkeleton(fBXSkeletonDesc);

                    newAsset.SaveAssetTo(skeletonAsset);
                    var sktameta = new Animation.Asset.USkeletonAssetAMeta();
                    sktameta.SetAssetName(skeletonAsset);
                    sktameta.AssetId = Guid.NewGuid();
                    sktameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(Animation.Asset.USkeletonAssetAMeta));
                    sktameta.Description = $"This is a {typeof(Animation.Asset.USkeletonAssetAMeta).FullName}\n";
                    sktameta.SaveAMeta();
                    UEngine.Instance.AssetMetaManager.RegAsset(sktameta);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
            }
            private void CreateMesh(RName name, IMeshPrimitives meshPrimitives, bool hasSkin, AssetImportAndExport.FBX.FBXSkeletonDesc fbxSkeletonDesc)
            {
                var cMeshPrimitives = new CMeshPrimitives(meshPrimitives);
                if (hasSkin)
                {
                    cMeshPrimitives.PartialSkeleton = AssetImportAndExport.FBX.FBXMeshImportUtility.CreateSkinSkeleton(fbxSkeletonDesc);
                }
                cMeshPrimitives.SaveAssetTo(name);

                var ameta = new CMeshPrimitivesAMeta();
                ameta.SetAssetName(name);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(CMeshPrimitives));
                ameta.Description = $"This is a {typeof(CMeshPrimitives).FullName}\n";
                ameta.SaveAMeta();
                UEngine.Instance.AssetMetaManager.RegAsset(ameta);
            }
        }
    }
}

namespace AssetImportAndExport.FBX
{
    using EngineNS.Animation.SkeletonAnimation.Skeleton;
    using EngineNS.Animation.SkeletonAnimation.Skeleton.Limb;

    public static class FBXMeshImportUtility
    {
        public static USkinSkeleton CreateSkinSkeleton(FBXSkeletonDesc fbxSkeletonDesc)
        {
            var skinSkeleton = new USkinSkeleton();
            for (int i = 0; i < fbxSkeletonDesc.GetBoneDescsNum(); ++i)
            {
                var fbxBoneDesc = fbxSkeletonDesc.GetBoneDesc(i);

                UBoneDesc desc = new UBoneDesc();
                desc.Name = fbxBoneDesc.m_Name.Text;
                desc.NameHash = fbxBoneDesc.m_NameHash;
                desc.ParentName = fbxBoneDesc.m_ParentName.Text;
                desc.ParentHash = fbxBoneDesc.m_ParentHash;
                desc.InitMatrix = fbxBoneDesc.m_InitMatrix;
                desc.InvInitMatrix = fbxBoneDesc.m_InvInitMatrix;
                desc.InvPos = fbxBoneDesc.m_InvPos;
                desc.InvQuat = fbxBoneDesc.m_InvQuat;
                desc.InvScale = fbxBoneDesc.m_InvScale;

                UBone bone = new UBone(desc);
                skinSkeleton.AddLimb(bone);
            }

            skinSkeleton.ConstructHierarchy();
            return skinSkeleton;          
        }
    }
}
