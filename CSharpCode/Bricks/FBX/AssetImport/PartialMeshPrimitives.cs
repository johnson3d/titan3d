
using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public partial class UMeshPrimitives
    {
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            public ImportAttribute()
            {
                ExtName = UMeshPrimitives.AssetExt;
            }
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
            string MeshType = "FBX";
            UMeshDataProvider.UMakeBoxParameter BoxParameter = new UMeshDataProvider.UMakeBoxParameter();
            UMeshDataProvider.UMakeRect2DParameter Rect2DParameter = new UMeshDataProvider.UMakeRect2DParameter();
            UMeshDataProvider.UMakeSphereParameter SphereParameter = new UMeshDataProvider.UMakeSphereParameter();
            UMeshDataProvider.UMakeCylinderParameter CylinderParameter = new UMeshDataProvider.UMakeCylinderParameter();
            UMeshDataProvider.UMakeTorusParameter TorusParameter = new UMeshDataProvider.UMakeTorusParameter();
            UMeshDataProvider.MakeCapsuleParameter CapsuleParameter = new UMeshDataProvider.MakeCapsuleParameter(); 

            public unsafe partial void FBXCreateCreateDraw(UContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import MeshPrimitives", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
                var visible = true;
                if (ImGuiAPI.BeginPopupModal($"Import MeshPrimitives", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (ImGuiAPI.BeginCombo("MeshType", MeshType, ImGuiComboFlags_.ImGuiComboFlags_None))
                    {
                        bool bSelected = false;
                        if (ImGuiAPI.Selectable("FBX", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "FBX";
                        }
                        if (ImGuiAPI.Selectable("Box", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "Box";
                        }
                        if (ImGuiAPI.Selectable("Rect2D", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "Rect2D";
                        }
                        if (ImGuiAPI.Selectable("Sphere", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "Sphere";
                        }
                        if (ImGuiAPI.Selectable("Cylinder", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "Cylinder";
                        }
                        if (ImGuiAPI.Selectable("Torus", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "Torus";
                        }
                        if (ImGuiAPI.Selectable("Capsule", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "Capsule";
                        }
                        ImGuiAPI.EndCombo();
                    }
                    switch (MeshType)
                    {
                        case "FBX":
                            {
                                PGAsset.Target = null;
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
                            }
                            break;
                        case "Box":
                            PGAsset.Target = BoxParameter;
                            break;
                        case "Rect2D":
                            PGAsset.Target = Rect2DParameter;
                            break;
                        case "Sphere":
                            PGAsset.Target = SphereParameter;
                            break;
                        case "Cylinder":
                            PGAsset.Target = CylinderParameter;
                            break;
                        case "Torus":
                            PGAsset.Target = TorusParameter;
                            break;
                        case "Capsule":
                            PGAsset.Target = CapsuleParameter;
                            break;
                    }
                    
                    ImGuiAPI.Separator();

                    bool nameChanged = ImGuiAPI.InputText("##in_rname", ref mName);
                    if (nameChanged)
                    {
                        if (IO.FileManager.FileExists(mDir.Address + mName + UMeshPrimitives.AssetExt))
                            eErrorType = enErrorType.IsExisting;
                    }
                    ImGuiAPI.Separator();

                    if (eErrorType == enErrorType.None)
                    {
                        if (ImGuiAPI.Button("Create Asset", in Vector2.Zero))
                        {
                            switch (MeshType)
                            {
                                case "FBX":
                                    {
                                        var task = FBXImportMesh();
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                                case "Box":
                                    {
                                        var mesh = UMeshDataProvider.MakeBox(BoxParameter.Position.X, BoxParameter.Position.Y, BoxParameter.Position.Z,
                                            BoxParameter.Extent.X, BoxParameter.Extent.Y, BoxParameter.Extent.Z, new Color4(BoxParameter.Color).ToArgb(), BoxParameter.FaceFlags);

                                        var name = this.GetAssetRName();
                                        var ameta = new UMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta();
                                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                                case "Rect2D":
                                    {
                                        var mesh = UMeshDataProvider.MakeRect2D(Rect2DParameter.Position.X, Rect2DParameter.Position.Y, 
                                            Rect2DParameter.Width, Rect2DParameter.Height, Rect2DParameter.Position.Z);

                                        var name = this.GetAssetRName();
                                        var ameta = new UMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta();
                                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                                case "Sphere":
                                    {
                                        var mesh = UMeshDataProvider.MakeSphere(SphereParameter.Radius, SphereParameter.Slices,
                                            SphereParameter.Stacks, new Color4(SphereParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new UMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta();
                                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                                case "Cylinder":
                                    {
                                        var mesh = UMeshDataProvider.MakeCylinder(CylinderParameter.Radius1, CylinderParameter.Radius2, CylinderParameter.Length,
                                            CylinderParameter.Slices, CylinderParameter.Stacks, new Color4(CylinderParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new UMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta();
                                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                                case "Torus":
                                    {
                                        var mesh = UMeshDataProvider.MakeTorus(TorusParameter.InnerRadius, TorusParameter.OutRadius2,
                                            TorusParameter.Slices, TorusParameter.Rings, new Color4(TorusParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new UMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta();
                                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                                case "Capsule":
                                    {
                                        var mesh = UMeshDataProvider.MakeCapsule(CapsuleParameter.Radius, CapsuleParameter.Depth,
                                            (int)CapsuleParameter.Latitudes, (int)CapsuleParameter.Longitudes, (int)CapsuleParameter.Rings, CapsuleParameter.UvProfile, new Color4(TorusParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new UMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta();
                                        UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        ContentBrowser.mAssetImporter = null;
                                    }
                                    break;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        ContentBrowser.mAssetImporter = null;
                    }

                    ImGuiAPI.Separator();
                    if (PGAsset.Target != null)
                    {
                        PGAsset.OnDraw(false, false, false);
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
                            var rn = RName.GetRName(mDir.Name + meshName + Animation.Asset.USkeletonAsset.AssetExt, mDir.RNameType);
                            var fbxSkeletonDesc = meshImporter.GetSkeletonDesc();
                            await CreateOrMergeSkeleton(rn, fbxSkeletonDesc);
                        }
                        {
                            var rn = RName.GetRName(mDir.Name + meshName + UMeshPrimitives.AssetExt, mDir.RNameType);
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
            private void CreateMesh(RName name, NxRHI.FMeshPrimitives meshPrimitives, bool hasSkin, AssetImportAndExport.FBX.FBXSkeletonDesc fbxSkeletonDesc)
            {
                var ameta = new UMeshPrimitivesAMeta();
                ameta.SetAssetName(name);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                ameta.SaveAMeta();
                UEngine.Instance.AssetMetaManager.RegAsset(ameta);

                var cMeshPrimitives = new UMeshPrimitives(meshPrimitives);
                if (hasSkin)
                {
                    cMeshPrimitives.PartialSkeleton = AssetImportAndExport.FBX.FBXMeshImportUtility.CreateSkinSkeleton(fbxSkeletonDesc);
                }
                cMeshPrimitives.SaveAssetTo(name);
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
