using Assimp;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Bricks.AssetImpExp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Mesh
{
    public class TtMeshImprotSetting
    {
        [Category("FileInfo"), ReadOnly(true)]
        public string FileName { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public int MeshesCount { get; set; } = 0;
        [Category("FileInfo"), ReadOnly(true)]
        public string UpAxis { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public float UnitScaleFactor { get; set; } = 1;
        [Category("FileInfo"), ReadOnly(true)]
        public string Generator { get; set; } = "";
        [Category("ImportSetting")]
        public float UnitScale { get; set; } = 0.01f;
        [Category("ImportSetting")]
        public bool AsStaticMesh { get; set; } = false;
    }
    public partial class UMeshPrimitives
    {
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            TtAssetImporter AssetImporter = new TtAssetImporter();
            TtMeshImprotSetting MeshImprotSetting = new TtMeshImprotSetting();
        public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.UContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import MeshPrimitives", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;
                var visible = true;
                var retValue = false;
                MeshType = "FromFile";
                if (ImGuiAPI.BeginPopupModal($"Import MeshPrimitives", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if (ImGuiAPI.BeginCombo("MeshType", MeshType, ImGuiComboFlags_.ImGuiComboFlags_None))
                    {
                        bool bSelected = false;
                        if (ImGuiAPI.Selectable("FromFile", ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            MeshType = "FromFile";
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
                        case "FromFile":
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
                                            var assetDescription = AssetImporter.PreImport(mSourceFile);
                                            if (assetDescription == null)
                                            {
                                                eErrorType = enErrorType.EmptyName;
                                            }
                                            else
                                            {
                                                MeshImprotSetting.FileName = assetDescription.FileName;
                                                MeshImprotSetting.MeshesCount = assetDescription.MeshesCount;
                                                MeshImprotSetting.UpAxis = assetDescription.UpAxis;
                                                MeshImprotSetting.UnitScaleFactor = assetDescription.UnitScaleFactor;
                                                MeshImprotSetting.Generator = assetDescription.Generator;
                                                PGAsset.Target = MeshImprotSetting;
                                                mName = IO.TtFileManager.GetPureName(mSourceFile);
                                            }
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
                        if (IO.TtFileManager.FileExists(mDir.Address + mName + UMeshPrimitives.AssetExt))
                            eErrorType = enErrorType.IsExisting;
                    }
                    ImGuiAPI.Separator();

                    if (eErrorType == enErrorType.None)
                    {
                        if (ImGuiAPI.Button("Create Asset", in Vector2.Zero))
                        {
                            switch (MeshType)
                            {
                                case "FromFile":
                                    {
                                        var task = DoImport();
                                        ImGuiAPI.CloseCurrentPopup();
                                        retValue = true;
                                    }
                                    break;
                                case "Box":
                                    {
                                        var mesh = UMeshDataProvider.MakeBox(BoxParameter.Position.X, BoxParameter.Position.Y, BoxParameter.Position.Z,
                                            BoxParameter.Extent.X, BoxParameter.Extent.Y, BoxParameter.Extent.Z, new Color4f(BoxParameter.Color).ToArgb(), BoxParameter.FaceFlags);

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
                                        retValue = true;
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
                                        retValue = true;
                                    }
                                    break;
                                case "Sphere":
                                    {
                                        var mesh = UMeshDataProvider.MakeSphere(SphereParameter.Radius, SphereParameter.Slices,
                                            SphereParameter.Stacks, new Color4f(SphereParameter.Color).ToArgb());

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
                                        retValue = true;
                                    }
                                    break;
                                case "Cylinder":
                                    {
                                        var mesh = UMeshDataProvider.MakeCylinder(CylinderParameter.Radius1, CylinderParameter.Radius2, CylinderParameter.Length,
                                            CylinderParameter.Slices, CylinderParameter.Stacks, new Color4f(CylinderParameter.Color).ToArgb());

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
                                        retValue = true;
                                    }
                                    break;
                                case "Torus":
                                    {
                                        var mesh = UMeshDataProvider.MakeTorus(TorusParameter.InnerRadius, TorusParameter.OutRadius2,
                                            TorusParameter.Slices, TorusParameter.Rings, new Color4f(TorusParameter.Color).ToArgb());

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
                                        retValue = true;
                                    }
                                    break;
                                case "Capsule":
                                    {
                                        var mesh = UMeshDataProvider.MakeCapsule(CapsuleParameter.Radius, CapsuleParameter.Depth,
                                            (int)CapsuleParameter.Latitudes, (int)CapsuleParameter.Longitudes, (int)CapsuleParameter.Rings, CapsuleParameter.UvProfile, new Color4f(TorusParameter.Color).ToArgb());

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

            private async System.Threading.Tasks.Task<bool> DoImport()
            {
                var AssetImportOption = new TtAssetImportOption_Mesh();
                AssetImportOption.UnitScale = MeshImprotSetting.UnitScale;
                AssetImportOption.AsStaticMesh = MeshImprotSetting.AsStaticMesh;
                var skeletons = SkeletonGenerater.Generate(AssetImporter.AiScene, AssetImportOption);
                if (skeletons.Count == 0)
                {
                    
                }
                else if(skeletons.Count == 1)
                {
                    var rn = RName.GetRName(mDir.Name + MeshImprotSetting.FileName + Animation.Asset.TtSkeletonAsset.AssetExt, mDir.RNameType);
                    await SaveSkeleton(rn, skeletons[0]);
                }
                else
                {
                    //TODO: muti skeletons in the scene
                    //var meshNode = SkeletonGenerater.FindSkeletonMeshNode(skeletons[0], AssetImporter.AiScene);
                    //foreach(var skeleton in skeletons)
                    //{
                    //    var rn = RName.GetRName(mDir.Name + meshPrimitives.mCoreObject.GetName() + Animation.Asset.USkeletonAsset.AssetExt, mDir.RNameType);
                    //    await SaveSkeleton(rn, meshPrimitives.PartialSkeleton);
                    //}
                }

                var meshPrimitives = MeshGenerater.Generate(skeletons ,AssetImporter.AiScene, AssetImportOption);
                foreach(var mesh in meshPrimitives)
                {
                    var rn = RName.GetRName(mDir.Name + mesh.mCoreObject.GetName()+ UMeshPrimitives.AssetExt, mDir.RNameType);
                    await SaveMesh(rn, mesh);
                }
                return true;
            }

            private async System.Threading.Tasks.Task SaveSkeleton(RName skeletonAsset, TtSkinSkeleton skeleton, bool bIsNeedMerge = false)
            {
                if(!bIsNeedMerge || !EngineNS.UEngine.Instance.AnimationModule.SkeletonAssetManager.SkeletonAssets.ContainsKey(skeletonAsset))
                {
                    Animation.Asset.TtSkeletonAsset newAsset = new Animation.Asset.TtSkeletonAsset();
                    newAsset.Skeleton = skeleton;
                    newAsset.SaveAssetTo(skeletonAsset);

                    var sktameta = new Animation.Asset.TtSkeletonAssetAMeta();
                    sktameta.SetAssetName(skeletonAsset);
                    sktameta.AssetId = Guid.NewGuid();
                    sktameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(Animation.Asset.TtSkeletonAsset));
                    sktameta.Description = $"This is a {typeof(Animation.Asset.TtSkeletonAsset).FullName}\n";
                    sktameta.SaveAMeta();
                    UEngine.Instance.AssetMetaManager.RegAsset(sktameta);

                    if (EngineNS.UEngine.Instance.AnimationModule.SkeletonAssetManager.SkeletonAssets.ContainsKey(skeletonAsset))
                    {
                        EngineNS.UEngine.Instance.AnimationModule.SkeletonAssetManager.SkeletonAssets[skeletonAsset] = newAsset;
                    }
                }
                else
                {
                    var result = await EngineNS.UEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(skeletonAsset);
                    if(result != null)
                    {
                        //merge
                    }

                }
            }
            private async System.Threading.Tasks.Task SaveMesh(RName name, UMeshPrimitives meshPrimitives)
            {
                var ameta = new UMeshPrimitivesAMeta();
                ameta.SetAssetName(name);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(typeof(UMeshPrimitives));
                ameta.Description = $"This is a {typeof(UMeshPrimitives).FullName}\n";
                ameta.SaveAMeta();
                UEngine.Instance.AssetMetaManager.RegAsset(ameta);


                meshPrimitives.SaveAssetTo(name);
            }
        }
    }
}
