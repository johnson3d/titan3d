using Assimp;
using EngineNS.Animation.Asset;
using EngineNS.Animation.SkeletonAnimation.Skeleton;
using EngineNS.Bricks.AssetImpExp;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using static EngineNS.Graphics.Mesh.TtMaterialMesh;

namespace EngineNS.Graphics.Mesh
{
    public class TtMeshImportSetting
    {
        [Category("FileInfo"), ReadOnly(true)]
        public string FileName { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string FileFormat { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string FileFormatVersion { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string Generator { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public int MeshesCount { get; set; } = 0;
        [Category("FileInfo"), ReadOnly(true)]
        public bool MeshesHaveScale { get; set; } = false;
        [Category("FileInfo"), ReadOnly(true)]
        public bool MeshesHaveTranslation { get; set; } = false;
        [Category("FileInfo"), ReadOnly(true)]
        public string UpAxis { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public float UnitScaleFactor { get; set; } = 1;
        [Category("ImportSetting"), ReadOnly(true)]
        public string DefaultImportRule { get; } = "Import mesh in Local Space";
        [Category("ImportSetting")]
        public float UnitScale { get; set; } = 0.01f;
        [Category("ImportSetting")]
        public bool AsStaticMesh { get; set; } = false;
        [Category("ImportSetting")]
        public bool ApplyTransformToVertex { get; set; } = false;
        [Category("ImportSetting")]
        public bool GenerateUMS { get; set; } = true;
        [Category("ImportSetting")]
        public bool JoinIdenticalVertices { get; set; } = true;
        [Category("ImportSetting"), Browsable(false)]
        public TtAssetImporter AssetImporter { get; set; } = null;

        public async System.Threading.Tasks.Task<bool> ImportAndSaveMesh(RName dir)
        {
            return await TtMeshPrimitives.ImportAttribute.ImportAndSaveMesh(dir, this);
        }
    }
    public partial class TtMeshPrimitives
    {
        public partial class ImportAttribute : IO.CommonCreateAttribute
        {
            //TtMeshImprotSetting MeshImprotSetting = new TtMeshImprotSetting();
            List<TtMeshImportSetting> MeshImportSettings = new List<TtMeshImportSetting>();
            public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.TtContentBrowser ContentBrowser)
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
                                if (ImGuiAPI.Button("Select File", in sz))
                                {
                                    mFileDialog.OpenModalWithMutiSelect("ChooseFileDlgKey", "Choose File", ".*", ".", int.MaxValue - 1);
                                }
                                // display
                                if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                                {
                                    // action if OK
                                    if (mFileDialog.IsOk() == true)
                                    {
                                        var count = mFileDialog.GetSelectedCount();
                                        for(int i = 0; i < count; ++i)
                                        {
                                            var path = mFileDialog.GetFilePathByIndex(i);
                                            var meshImprotSetting = TtAssetImporter.CreateMeshImporter(path);
                                            if (meshImprotSetting == null)
                                            {
                                                eErrorType = enErrorType.EmptyName;
                                            }
                                            else
                                            {
                                                if (i == 0)
                                                {
                                                    PGAsset.Target = meshImprotSetting;
                                                    mName = IO.TtFileManager.GetPureName(path);
                                                }
                                                MeshImportSettings.Add(meshImprotSetting);
                                            }
                                            //TtMeshImportSetting meshImprotSetting = new TtMeshImportSetting();
                                            //string filePath = mFileDialog.GetCurrentPath();
                                            //if (!string.IsNullOrEmpty(path))
                                            //{
                                            //    TtAssetImporter AssetImporter = new TtAssetImporter();
                                            //    var assetDescription = AssetImporter.PreImport(path);
                                            //    if (assetDescription == null)
                                            //    {
                                            //        eErrorType = enErrorType.EmptyName;
                                            //    }
                                            //    else
                                            //    {
                                            //        meshImprotSetting.FileName = assetDescription.FileName;
                                            //        meshImprotSetting.MeshesCount = assetDescription.MeshesCount;
                                            //        meshImprotSetting.MeshesHaveScale = assetDescription.MeshesHaveScale;
                                            //        meshImprotSetting.MeshesHaveTranslation = assetDescription.MeshesHaveTranslation;
                                            //        meshImprotSetting.UpAxis = assetDescription.UpAxis;
                                            //        meshImprotSetting.UnitScaleFactor = assetDescription.UnitScaleFactor;
                                            //        meshImprotSetting.Generator = assetDescription.Generator;
                                            //        meshImprotSetting.AssetImporter = AssetImporter;
                                            //        if ( i == 0)
                                            //        {
                                            //            PGAsset.Target = meshImprotSetting;
                                            //            mName = IO.TtFileManager.GetPureName(path);
                                            //        }
                                            //        MeshImprotSettings.Add(meshImprotSetting);
                                            //    }
                                            //}
                                            if (eErrorType != enErrorType.None)
                                            {
                                                var clr = new Vector4(1, 0, 0, 1);
                                                ImGuiAPI.TextColored(in clr, $"Source:{path}");
                                            }
                                            else
                                            {
                                                var clr = new Vector4(1, 1, 1, 1);
                                                ImGuiAPI.TextColored(in clr, $"Source:{path}");
                                            }
                                        }
                                    }
                                    // close
                                    mFileDialog.CloseDialog();
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
                                case "FromFile":
                                    {
                                        var task = DoImport();
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

            private async Thread.Async.TtTask<bool> DoImport()
            {
                foreach(var importSetting in MeshImportSettings)
                {
                    if(importSetting.JoinIdenticalVertices)
                    {
                        var sceneFlags = TtAssetImporter.DefaultSceneFlags | PostProcessSteps.JoinIdenticalVertices;
                        importSetting.AssetImporter.ReImport(sceneFlags);
                    }
                    await importSetting.ImportAndSaveMesh(mDir);
                }
                return true;
            }
            public static async Thread.Async.TtTask<bool> ImportAndSaveMesh(RName mDir, TtMeshImportSetting improtSetting)
            {
                var AssetImportOption = new TtAssetImportOption_Mesh();
                AssetImportOption.UnitScale = improtSetting.UnitScale;
                AssetImportOption.AsStaticMesh = improtSetting.AsStaticMesh;
                AssetImportOption.ApplyTransformToVertex = improtSetting.ApplyTransformToVertex;
                AssetImportOption.GenerateUMS = improtSetting.GenerateUMS;
                var skeletons = SkeletonGenerater.Generate(improtSetting.AssetImporter.AiScene, AssetImportOption);
                if (skeletons.Count == 0)
                {

                }
                else if (skeletons.Count == 1)
                {
                    var rn = RName.GetRName(mDir.Name + improtSetting.FileName + Animation.Asset.TtSkeletonAsset.AssetExt, mDir.RNameType);
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

                var scene = improtSetting.AssetImporter.AiScene;
                Pipeline.Shader.TtMaterialInstance[] materials = new Pipeline.Shader.TtMaterialInstance[scene.Materials.Count];
                
                if (AssetImportOption.GenerateTexture && scene.HasMaterials)
                {
                    var baseMtl = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>().ImportBaseMaterial);
                    
                    for (int i = 0; i<scene.Materials.Count; i++)
                    {
                        var m = scene.Materials[i];
                        if (m.IsPBRMaterial == false)
                            continue;
                        var mtl = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(baseMtl);
                        mtl.AssetName = RName.GetRName($"{mDir.Name}{m.Name}_{i}{Pipeline.Shader.TtMaterialInstance.AssetExt}", mDir.RNameType);

                        Action<TextureSlot, string> setSrv = (TextureSlot slot, string shaderName) =>
                        {
                            if (slot.FilePath==null)
                                return;
                            int textureIndex = -1;
                            if (slot.FilePath.StartsWith("*"))
                            {
                                textureIndex = int.Parse(slot.FilePath.Substring(1));
                            }
                            if (textureIndex>=0&&textureIndex<scene.Textures.Count)
                            {
                                var texture = scene.Textures[textureIndex];
                                if (texture.HasCompressedData)
                                {
                                    if (texture.CompressedFormatHint=="png" || texture.CompressedFormatHint=="jpg")
                                    {
                                        try
                                        {
                                            var imageData = texture.CompressedData;
                                            if (shaderName=="TexNormal")
                                            {
                                                //using (var stream = new MemoryStream(imageData))
                                                //{
                                                //    StbImageSharp.TtMemImage image = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);

                                                //    image = StbImageSharp.ImageProcessor.GetBoxDownSampler(image, image.Width/2, image.Height/2);
                                                //    imageData = image.SaveToMem().GetBuffer();
                                                //}
                                            }

                                            using (var stream = new MemoryStream(imageData))
                                            {
                                                //StbImageSharp.TtMemImage image = StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                                                var importer = new NxRHI.TtSrView.ImportAttribute();
                                                importer.mSourceFile = texture.Filename + $"_{shaderName}.png";
                                                importer.mDir = mDir;
                                                importer.mName = texture.Filename + $"_{shaderName}";
                                                //importer.mDesc.Width = image.Width;
                                                //importer.mDesc.Height = image.Height;
                                                stream.Seek(0, SeekOrigin.Begin);
                                                var rn = importer.ImportImageImpl(stream);
                                                mtl.SetSrv(shaderName, rn);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"加载图像失败: {ex.Message}");
                                        }
                                    }
                                }
                                else if (texture.HasNonCompressedData)
                                {

                                }
                            }
                        };
                        setSrv(m.PBR.TextureBaseColor, "TexDiffuse");
                        setSrv(m.TextureNormal, "TexNormal");
                        setSrv(m.PBR.TextureMetalness, "TexMRA");

                        //setSrv(m.Matal, "TexMRA");
                        //TextureSlot slot;
                        //m.GetMaterialTexture(m.TextureAmbient, 0, out slot);

                        if (mtl.AssetName.AMeta==null)
                        {
                            var ameta = mtl.CreateAMeta();
                            ameta.AssetId = Guid.NewGuid();
                            ameta.SetAssetName(mtl.AssetName);
                            ameta.SaveAMeta(mtl);
                            TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                        }   
                        
                        mtl.SaveAssetTo(mtl.AssetName);
                        materials[i] = mtl;
                    }
                }

                var meshPrimitives = MeshGenerater.Generate(skeletons, improtSetting.AssetImporter.AiScene, AssetImportOption);
                foreach (var mesh in meshPrimitives)
                {
                    var rn = RName.GetRName(mDir.Name + mesh.Mesh.mCoreObject.GetName() + TtMeshPrimitives.AssetExt, mDir.RNameType);
                    await SaveMesh(rn, mesh.Mesh);
                    if (AssetImportOption.GenerateUMS)
                    {
                        var umsRN = RName.GetRName(mDir.Name + mesh.Mesh.mCoreObject.GetName() + TtMaterialMesh.AssetExt, mDir.RNameType);
                        var ums = new TtMaterialMesh
                        {
                            AssetName = umsRN,
                        };
                        ums.SubMeshes[0].Mesh = mesh.Mesh;
                        for(int i = 0; i < ums.SubMeshes[0].Materials.Count; i++)
                        {
                            if (i < mesh.Materials.Count && mesh.Materials[i].MaterialIndex < materials.Length && materials[mesh.Materials[i].MaterialIndex]!=null)
                            {
                                ums.SubMeshes[0].Materials[i] = materials[mesh.Materials[i].MaterialIndex];
                            }
                            else
                            {
                                ums.SubMeshes[0].Materials[i] = await EngineNS.TtEngine.Instance.Config.DefaultMaterial.GetAsset<Pipeline.Shader.TtMaterial>(); //EngineNS.TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(EngineNS.TtEngine.Instance.Config.DefaultMaterial);
                            }
                        }
                        var ameta = new TtMaterialMeshAMeta();
                        ameta.SetAssetName(umsRN);
                        ameta.AssetId = Guid.NewGuid();
                        ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMaterialMesh));
                        ameta.Description = $"This is a {typeof(TtMaterialMesh).FullName}\n";
                        ameta.SaveAMeta(ums);
                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                        ums.SaveAssetTo(umsRN);
                    }
                }
                return true;
            }

            public static async Thread.Async.TtTask SaveSkeleton(RName skeletonAsset, TtSkinSkeleton skeleton, bool bIsNeedMerge = false)
            {
                if(!bIsNeedMerge || !EngineNS.TtEngine.Instance.AnimationModule.SkeletonAssetManager.SkeletonAssets.ContainsKey(skeletonAsset))
                {
                    Animation.Asset.TtSkeletonAsset newAsset = new Animation.Asset.TtSkeletonAsset();
                    newAsset.Skeleton = skeleton;

                    var sktameta = new Animation.Asset.TtSkeletonAssetAMeta();
                    sktameta.SetAssetName(skeletonAsset);
                    sktameta.AssetId = Guid.NewGuid();
                    sktameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(Animation.Asset.TtSkeletonAsset));
                    sktameta.Description = $"This is a {typeof(Animation.Asset.TtSkeletonAsset).FullName}\n";
                    sktameta.SaveAMeta(newAsset);
                    TtEngine.Instance.AssetMetaManager.RegAsset(sktameta);

                    newAsset.SaveAssetTo(skeletonAsset);

                    if (EngineNS.TtEngine.Instance.AnimationModule.SkeletonAssetManager.SkeletonAssets.ContainsKey(skeletonAsset))
                    {
                        EngineNS.TtEngine.Instance.AnimationModule.SkeletonAssetManager.SkeletonAssets[skeletonAsset] = newAsset;
                    }
                }
                else
                {
                    var result = await EngineNS.TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(skeletonAsset);
                    if(result != null)
                    {
                        //merge
                    }

                }
            }
            public static async Thread.Async.TtTask SaveMesh(RName name, TtMeshPrimitives meshPrimitives)
            {
                var ameta = new TtMeshPrimitivesAMeta();
                ameta.SetAssetName(name);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtMeshPrimitives));
                ameta.Description = $"This is a {typeof(TtMeshPrimitives).FullName}\n";
                ameta.SaveAMeta(meshPrimitives);
                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                meshPrimitives.AssetName = name;
                meshPrimitives.SaveAssetTo(name);
                TtEngine.Instance.GfxDevice.MeshPrimitiveManager.UnsafeRemove(name);
            }
        }
    }
}
