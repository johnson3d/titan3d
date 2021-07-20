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
                            if (FBXImportMesh())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                ContentBrowser.mAssetImporter = null;
                            }
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

            private unsafe bool FBXImportMesh()
            {
                var fileDesc = mFBXImporter.GetFileImportDesc();
                for (uint i = 0; i < fileDesc.MeshNum; ++i)
                {
                    using (var meshImporter = mFBXImporter.CreateMeshImporter(i))
                    {
                        var meshDesc = mFBXImporter.GetFBXMeshDescs(i);
                        var meshName = meshDesc->NativeSuper->Name.Text;
                        meshImporter.Process(UEngine.Instance.GfxDevice.RenderContext.mCoreObject);
                        var mesh = meshImporter.GetMeshPrimitives();
                        var rn = RName.GetRName(mDir.Name + meshName + CMeshPrimitives.AssetExt);
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
                        if (partialSkeleton.IsValidPointer)
                        {
                            Animation.Skeleton.USkeletonAsset ska = new Animation.Skeleton.USkeletonAsset();
                            ska.Skeleton.MergeWith(partialSkeleton);
                            rn = RName.GetRName(mDir.Name + meshName + Animation.Skeleton.USkeletonAsset.AssetExt);
                            ska.SaveAssetTo(rn);
                            var sktameta = new Animation.Skeleton.USkeletonAssetAMeta();
                            sktameta.SetAssetName(rn);
                            sktameta.AssetId = Guid.NewGuid();
                            sktameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(Animation.Skeleton.USkeletonAssetAMeta));
                            sktameta.Description = $"This is a {typeof(Animation.Skeleton.USkeletonAssetAMeta).FullName}\n";
                            sktameta.SaveAMeta();
                            UEngine.Instance.AssetMetaManager.RegAsset(sktameta);
                        }

                        mesh.NativeSuper.Release();
                    }
                }
                mFBXImporter.Dispose();
                return true;
            }
        }
    }
}
