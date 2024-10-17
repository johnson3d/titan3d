﻿
using EngineNS.EGui.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public partial class TtMeshPrimitives
    {
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            public ImportAttribute()
            {
                ExtName = TtMeshPrimitives.AssetExt;
            }
            AssetImportAndExport.FBX.FBXImporter mFBXImporter; //for now we only have one file to import
            string MeshType = "FromFile";
            TtMeshDataProvider.UMakeBoxParameter BoxParameter = new TtMeshDataProvider.UMakeBoxParameter();
            TtMeshDataProvider.UMakeRect2DParameter Rect2DParameter = new TtMeshDataProvider.UMakeRect2DParameter();
            TtMeshDataProvider.UMakeSphereParameter SphereParameter = new TtMeshDataProvider.UMakeSphereParameter();
            TtMeshDataProvider.UMakeCylinderParameter CylinderParameter = new TtMeshDataProvider.UMakeCylinderParameter();
            TtMeshDataProvider.UMakeTorusParameter TorusParameter = new TtMeshDataProvider.UMakeTorusParameter();
            TtMeshDataProvider.MakeCapsuleParameter CapsuleParameter = new TtMeshDataProvider.MakeCapsuleParameter(); 

            public unsafe partial bool FBXCreateCreateDraw(TtContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import MeshPrimitives", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
                var visible = true;
                var retValue = false;
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
                                //PGAsset.Target = null;
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
                                            mFBXImporter = TtEngine.Instance.FBXFactoryModule.Instance.CreateImporter();
                                            var fileDesc = mFBXImporter.PreImport(mSourceFile);
                                            PGAsset.Target = fileDesc;
                                            mName = IO.TtFileManager.GetPureName(mSourceFile);
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
                        if (IO.TtFileManager.FileExists(mDir.Address + mName + TtMeshPrimitives.AssetExt))
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
                                        retValue = true;
                                    }
                                    break;
                                case "Box":
                                    {
                                        var mesh = TtMeshDataProvider.MakeBox(BoxParameter.Position.X, BoxParameter.Position.Y, BoxParameter.Position.Z,
                                            BoxParameter.Extent.X, BoxParameter.Extent.Y, BoxParameter.Extent.Z, new Color4f(BoxParameter.Color).ToArgb(), BoxParameter.FaceFlags);

                                        var name = this.GetAssetRName();
                                        var ameta = new TtMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta((IO.IAsset)null);
                                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                                case "Rect2D":
                                    {
                                        var mesh = TtMeshDataProvider.MakeRect2D(Rect2DParameter.Position.X, Rect2DParameter.Position.Y, 
                                            Rect2DParameter.Width, Rect2DParameter.Height, Rect2DParameter.Position.Z);

                                        var name = this.GetAssetRName();
                                        var ameta = new TtMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta((IO.IAsset)null);
                                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                                case "Sphere":
                                    {
                                        var mesh = TtMeshDataProvider.MakeSphere(SphereParameter.Radius, SphereParameter.Slices,
                                            SphereParameter.Stacks, new Color4f(SphereParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new TtMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta((IO.IAsset)null);
                                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                                case "Cylinder":
                                    {
                                        var mesh = TtMeshDataProvider.MakeCylinder(CylinderParameter.Radius1, CylinderParameter.Radius2, CylinderParameter.Length,
                                            CylinderParameter.Slices, CylinderParameter.Stacks, new Color4f(CylinderParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new TtMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta((IO.IAsset)null);
                                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                                case "Torus":
                                    {
                                        var mesh = TtMeshDataProvider.MakeTorus(TorusParameter.InnerRadius, TorusParameter.OutRadius2,
                                            TorusParameter.Slices, TorusParameter.Rings, new Color4f(TorusParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new TtMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta((IO.IAsset)null);
                                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                                case "Capsule":
                                    {
                                        var mesh = TtMeshDataProvider.MakeCapsule(CapsuleParameter.Radius, CapsuleParameter.Depth,
                                            (int)CapsuleParameter.Latitudes, (int)CapsuleParameter.Longitudes, (int)CapsuleParameter.Rings, CapsuleParameter.UvProfile, new Color4f(TorusParameter.Color).ToArgb());

                                        var name = this.GetAssetRName();
                                        var ameta = new TtMeshPrimitivesAMeta();
                                        ameta.SetAssetName(name);
                                        ameta.AssetId = Guid.NewGuid();
                                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                                        ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                                        ameta.SaveAMeta((IO.IAsset)null);
                                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                                        mesh.ToMesh().SaveAssetTo(name);
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.Separator();
                    if (PGAsset.Target != null)
                    {
                        PGAsset.OnDraw(false, false, false);
                    }
                    
                    ImGuiAPI.EndPopup();
                }

                return retValue;
            }

            private async System.Threading.Tasks.Task<bool> FBXImportMesh()
            {
                var fileDesc = mFBXImporter.GetFileImportDesc();
                for (uint i = 0; i < fileDesc.MeshNum; ++i)
                {
                    using (var meshImporter = mFBXImporter.CreateMeshImporter(i))
                    {
                        meshImporter.Process(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject);
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
                            var rn = RName.GetRName(mDir.Name + meshName + Animation.Asset.TtSkeletonAsset.AssetExt, mDir.RNameType);
                            var fbxSkeletonDesc = meshImporter.GetSkeletonDesc();
                            await CreateOrMergeSkeleton(rn, fbxSkeletonDesc);
                        }
                        {
                            var rn = RName.GetRName(mDir.Name + meshName + TtMeshPrimitives.AssetExt, mDir.RNameType);
                            CreateMesh(rn, meshImporter.GetMeshPrimitives(), hasSkin, meshImporter.GetSkeletonDesc());
                        }
                    }
                }
                mFBXImporter.Dispose();
                return true;
            }

            private async System.Threading.Tasks.Task CreateOrMergeSkeleton(RName skeletonAsset, AssetImportAndExport.FBX.FBXSkeletonDesc fBXSkeletonDesc)
            {
                var ska = await EngineNS.TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(skeletonAsset);
                if (ska == null)
                {
                    Animation.Asset.TtSkeletonAsset newAsset = new Animation.Asset.TtSkeletonAsset();
                    newAsset.Skeleton = AssetImportAndExport.FBX.FBXMeshImportUtility.CreateSkinSkeleton(fBXSkeletonDesc);

                    newAsset.SaveAssetTo(skeletonAsset);
                    var sktameta = new Animation.Asset.TtSkeletonAssetAMeta();
                    sktameta.SetAssetName(skeletonAsset);
                    sktameta.AssetId = Guid.NewGuid();
                    sktameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(Animation.Asset.TtSkeletonAssetAMeta));
                    sktameta.Description = $"This is a {typeof(Animation.Asset.TtSkeletonAssetAMeta).FullName}\n";
                    sktameta.SaveAMeta(newAsset);
                    TtEngine.Instance.AssetMetaManager.RegAsset(sktameta);
                }
                else
                {
                    //System.Diagnostics.Debug.Assert(false);
                }
            }
            private void CreateMesh(RName name, NxRHI.FMeshPrimitives meshPrimitives, bool hasSkin, AssetImportAndExport.FBX.FBXSkeletonDesc fbxSkeletonDesc)
            {
                var ameta = new TtMeshPrimitivesAMeta();
                ameta.SetAssetName(name);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);

                var cMeshPrimitives = new TtMeshPrimitives(meshPrimitives);
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
        public static TtSkinSkeleton CreateSkinSkeleton(FBXSkeletonDesc fbxSkeletonDesc)
        {
            var skinSkeleton = new TtSkinSkeleton();
            for (int i = 0; i < fbxSkeletonDesc.GetBoneDescsNum(); ++i)
            {
                var fbxBoneDesc = fbxSkeletonDesc.GetBoneDesc(i);

                TtBoneDesc desc = new TtBoneDesc();
                desc.Name = fbxBoneDesc.m_Name.Text;
                desc.NameHash = fbxBoneDesc.m_NameHash;
                desc.ParentName = fbxBoneDesc.m_ParentName.Text;
                desc.ParentHash = fbxBoneDesc.m_ParentHash;
                desc.InitMatrix = fbxBoneDesc.m_InitMatrix;
                desc.InvInitMatrix = fbxBoneDesc.m_InvInitMatrix;
                desc.InvPos = fbxBoneDesc.m_InvPos;
                desc.InvQuat = fbxBoneDesc.m_InvQuat;
                desc.InvScale = fbxBoneDesc.m_InvScale;

                TtBone bone = new TtBone(desc);
                skinSkeleton.AddLimb(bone);
            }

            skinSkeleton.ConstructHierarchy();
            return skinSkeleton;          
        }
    }
}
