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
        public string SourceFile { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string IntermediateFile { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string MaterialManifest { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string ImportMessage { get; set; } = "";
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
        public bool MergeMeshes { get; set; } = false;
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
            Queue<string> mImportSourceQueue = new Queue<string>();
            System.Threading.Tasks.Task<TtMeshImportPendingResult> mPendingImportTask;
            string mPendingImportSource = "";
            string mPendingImportMessage = "";
            string mImportError = "";
            class TtMeshImportPendingResult
            {
                public string SourceFile;
                public TtMeshImportSetting Setting;
                public string Error;
            }
            public unsafe partial bool AssimpCreateCreateDraw(EGui.Controls.TtContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import MeshPrimitives", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                var mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
                var visible = true;
                var retValue = false;
                EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
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
                                UpdatePendingImportSource();
                                if (!string.IsNullOrEmpty(ContentBrowser.CurrentImporterFile) &&
                                    MeshImportSettings.Count == 0 &&
                                    mPendingImportTask == null &&
                                    mImportSourceQueue.Count == 0 &&
                                    string.IsNullOrEmpty(mImportError))
                                {
                                    QueueImportSource(ContentBrowser.CurrentImporterFile);
                                }
                                //PGAsset.Target = null;
                                var sz = new Vector2(-1, 0);
                                if (string.IsNullOrEmpty(ContentBrowser.CurrentImporterFile) && ImGuiAPI.Button("Select File", in sz))
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
                                            QueueImportSource(path);
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
                    if (!string.IsNullOrEmpty(mPendingImportMessage))
                    {
                        var clr = new Vector4(1, 1, 0, 1);
                        ImGuiAPI.TextColored(in clr, mPendingImportMessage);
                        ImGuiAPI.Text(mPendingImportSource);
                        ImGuiAPI.Separator();
                    }
                    if (!string.IsNullOrEmpty(mImportError))
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, mImportError);
                        ImGuiAPI.Separator();
                    }

                    bool nameChanged = ImGuiAPI.InputText("##in_rname", ref mName);
                    if (nameChanged)
                    {
                        if (IO.TtFileManager.FileExists(mDir.Address + mName + TtMeshPrimitives.AssetExt))
                            eErrorType = enErrorType.IsExisting;
                    }
                    ImGuiAPI.Separator();

                    var canCreateAsset = eErrorType == enErrorType.None;
                    if (MeshType == "FromFile")
                    {
                        canCreateAsset = canCreateAsset &&
                            MeshImportSettings.Count > 0 &&
                            mPendingImportTask == null &&
                            mImportSourceQueue.Count == 0;
                    }
                    if (canCreateAsset)
                    {
                        if (ImGuiAPI.Button("Create Asset", in Vector2.Zero))
                        {
                            switch (MeshType)
                            {
                                case "FromFile":
                                    {
                                        var importSummary = GetImportSourceSummary();
                                        ContentBrowser.SetImportStatusMessage($"Importing {importSummary}...");
                                        var task = DoImport();
                                        task.AddWaitTask((finishedTask) =>
                                        {
                                            try
                                            {
                                                var importTask = (Thread.Async.TtTask<bool>)finishedTask;
                                                var succeeded = importTask.DirectResult;
                                                if (succeeded)
                                                    ContentBrowser.SetImportStatusMessage($"Imported {importSummary}.", false, true);
                                                else
                                                    ContentBrowser.SetImportStatusMessage($"Import failed: {importSummary}.", true);
                                            }
                                            catch (Exception ex)
                                            {
                                                ContentBrowser.SetImportStatusMessage($"Import failed: {ex.Message}", true);
                                                EngineNS.Profiler.Log.WriteException(ex);
                                            }
                                        });
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
                                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
                                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
                                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
                                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
                                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
                                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
                        ContentBrowser.SetImportStatusMessage("Import cancelled.");
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
                EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();

                return retValue;
            }

            public override bool IsAssetSource(string fileExt)
            {
                fileExt = fileExt.TrimStart('.').ToLower();
                if (TtAssetSourceImportPlugin.HasPluginForSource(fileExt))
                    return true;
                switch (fileExt)
                {
                    case "fbx":
                    case "obj":
                    case "gltf":
                    case "glb":
                    case "dae":
                    case "3ds":
                        return true;
                    default:
                        return false;
                }
            }

            void QueueImportSource(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return;

                foreach (var pendingPath in mImportSourceQueue)
                {
                    if (string.Equals(pendingPath, path, StringComparison.OrdinalIgnoreCase))
                        return;
                }
                if (string.Equals(mPendingImportSource, path, StringComparison.OrdinalIgnoreCase))
                    return;
                foreach (var setting in MeshImportSettings)
                {
                    if (string.Equals(setting.SourceFile, path, StringComparison.OrdinalIgnoreCase))
                        return;
                }

                mImportSourceQueue.Enqueue(path);
                mImportError = "";
                eErrorType = enErrorType.None;
                TryStartPendingImportSource();
            }

            void TryStartPendingImportSource()
            {
                if (mPendingImportTask != null || mImportSourceQueue.Count == 0)
                    return;

                var path = mImportSourceQueue.Dequeue();
                mPendingImportSource = path;
                var plugin = TtAssetSourceImportPlugin.GetPluginForSource(path);
                mPendingImportMessage = plugin?.GetImportingMessage(path) ?? "Reading source file...";
                mPendingImportTask = System.Threading.Tasks.Task.Run(() =>
                {
                    var setting = CreateMeshImportSetting(path, out var error);
                    return new TtMeshImportPendingResult()
                    {
                        SourceFile = path,
                        Setting = setting,
                        Error = error,
                    };
                });
            }

            void UpdatePendingImportSource()
            {
                if (mPendingImportTask == null)
                {
                    TryStartPendingImportSource();
                    return;
                }
                if (!mPendingImportTask.IsCompleted)
                    return;

                try
                {
                    var result = mPendingImportTask.GetAwaiter().GetResult();
                    if (result.Setting == null)
                    {
                        mImportError = result.Error;
                        eErrorType = enErrorType.EmptyName;
                    }
                    else
                    {
                        AppendImportSetting(result.Setting, result.SourceFile);
                    }
                }
                catch (Exception ex)
                {
                    mImportError = ex.Message;
                    eErrorType = enErrorType.EmptyName;
                    EngineNS.Profiler.Log.WriteException(ex);
                }
                finally
                {
                    mPendingImportTask = null;
                    mPendingImportSource = "";
                    mPendingImportMessage = "";
                    TryStartPendingImportSource();
                }
            }

            void AppendImportSetting(TtMeshImportSetting meshImportSetting, string path)
            {
                if (MeshImportSettings.Count == 0)
                {
                    PGAsset.Target = meshImportSetting;
                    mName = IO.TtFileManager.GetPureName(path);
                }
                MeshImportSettings.Add(meshImportSetting);
                mImportError = "";
                eErrorType = enErrorType.None;
            }

            static TtMeshImportSetting CreateMeshImportSetting(string path, out string error)
            {
                error = null;
                var plugin = TtAssetSourceImportPlugin.GetPluginForSource(path);
                if (plugin != null)
                {
                    var pluginSetting = plugin.CreateMeshImportSetting(path, out error);
                    if (pluginSetting != null)
                        return pluginSetting;
                    if (string.IsNullOrWhiteSpace(error))
                        error = $"Asset source import plugin failed to import source file: {path}";
                    return null;
                }

                var meshImportSetting = TtAssetImporter.CreateMeshImporter(path);
                if (meshImportSetting == null)
                {
                    error = $"Assimp failed to import source file: {path}";
                    return null;
                }
                meshImportSetting.SourceFile = path;
                return meshImportSetting;
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
                    if (!await importSetting.ImportAndSaveMesh(mDir))
                        return false;
                }
                return true;
            }

            string GetImportSourceSummary()
            {
                if (MeshImportSettings.Count <= 0)
                    return "source file";
                if (MeshImportSettings.Count > 1)
                    return $"{MeshImportSettings.Count} files";

                var importSetting = MeshImportSettings[0];
                var source = importSetting.SourceFile;
                if (string.IsNullOrWhiteSpace(source))
                    source = importSetting.FileName;

                var name = IO.TtFileManager.GetPureName(source);
                return string.IsNullOrWhiteSpace(name) ? "source file" : name;
            }
            public static async Thread.Async.TtTask<bool> ImportAndSaveMesh(RName mDir, TtMeshImportSetting improtSetting)
            {
                var AssetImportOption = new TtAssetImportOption_Mesh();
                AssetImportOption.UnitScale = improtSetting.UnitScale;
                AssetImportOption.AsStaticMesh = improtSetting.AsStaticMesh;
                AssetImportOption.ApplyTransformToVertex = improtSetting.ApplyTransformToVertex;
                AssetImportOption.GenerateUMS = improtSetting.GenerateUMS;
                var skeletons = AssetImportOption.AsStaticMesh ?
                    new List<TtSkinSkeleton>() :
                    SkeletonGenerater.Generate(improtSetting.AssetImporter.AiScene, AssetImportOption);
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

                // Let source import plugin resolve materials first (e.g. BlenderImporter handles manifest-based materials)
                var sourcePlugin = TtAssetSourceImportPlugin.GetPluginForSource(improtSetting.SourceFile);
                var pluginMaterials = sourcePlugin != null ? await sourcePlugin.ResolveMaterials(improtSetting, mDir) : null;
                if (pluginMaterials != null)
                {
                    for (int i = 0; i < materials.Length && i < pluginMaterials.Length; i++)
                        materials[i] = pluginMaterials[i];
                }
                else if (AssetImportOption.GenerateTexture && scene.HasMaterials)
                {
                    var baseMtl = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>().ImportBaseMaterial);

                    for (int i = 0; i < scene.Materials.Count; i++)
                    {
                        var m = scene.Materials[i];
                        var mtl = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(baseMtl);
                        var materialName = GetSafeImportAssetName($"{m.Name}_{i}");
                        mtl.AssetName = RName.GetRName($"{mDir.Name}{materialName}{Pipeline.Shader.TtMaterialInstance.AssetExt}", mDir.RNameType);

                        Func<TextureSlot, string, bool> setSrv = (TextureSlot slot, string shaderName) =>
                        {
                            if (slot.FilePath == null)
                                return false;
                            int textureIndex = -1;
                            if (slot.FilePath.StartsWith("*"))
                            {
                                textureIndex = int.Parse(slot.FilePath.Substring(1));
                            }
                            if (textureIndex >= 0 && textureIndex < scene.Textures.Count)
                            {
                                var texture = scene.Textures[textureIndex];
                                if (texture.HasCompressedData)
                                {
                                    if (string.Equals(texture.CompressedFormatHint, "png", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(texture.CompressedFormatHint, "jpg", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(texture.CompressedFormatHint, "jpeg", StringComparison.OrdinalIgnoreCase))
                                    {
                                        try
                                        {
                                            var imageData = texture.CompressedData;
                                            using (var stream = new MemoryStream(imageData))
                                            {
                                                var importer = new NxRHI.TtSrView.ImportAttribute();
                                                var textureName = GetImportTextureName(texture.Filename, m.Name, i, shaderName);
                                                importer.mSourceFile = textureName + ".png";
                                                importer.mDir = mDir;
                                                importer.mName = textureName;
                                                stream.Seek(0, SeekOrigin.Begin);
                                                var rn = importer.ImportImageImpl(stream);
                                                mtl.SetSrv(shaderName, rn);
                                                return rn != null;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"加载图像失败: {ex.Message}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var texturePath = ResolveImportTexturePath(slot.FilePath, improtSetting);
                                if (!string.IsNullOrEmpty(texturePath) && IO.TtFileManager.FileExists(texturePath))
                                {
                                    var textureName = GetImportTextureName(texturePath, m.Name, i, shaderName);
                                    var rn = ImportTextureFile(texturePath, mDir, textureName);
                                    if (rn != null)
                                    {
                                        mtl.SetSrv(shaderName, rn);
                                        return true;
                                    }
                                }
                            }
                            return false;
                        };

                        var diffuseSet = false;
                        var mraSet = false;
                        if (m.IsPBRMaterial)
                        {
                            diffuseSet = setSrv(m.PBR.TextureBaseColor, "TexDiffuse");
                            mraSet = setSrv(m.PBR.TextureMetalness, "TexMRA");
                        }
                        if (!diffuseSet)
                            diffuseSet = setSrv(m.TextureDiffuse, "TexDiffuse");
                        if (!diffuseSet)
                            diffuseSet = setSrv(m.TextureAmbient, "TexDiffuse");
                        if (!diffuseSet)
                        {
                            var colorTextureName = GetSafeImportAssetName($"{m.Name}_{i}_TexDiffuse");
                            var rn = ImportColorTexture(m.ColorDiffuse, mDir, colorTextureName);
                            if (rn != null)
                            {
                                mtl.SetSrv("TexDiffuse", rn);
                                diffuseSet = true;
                            }
                        }
                        if (!setSrv(m.TextureNormal, "TexNormal"))
                            setSrv(m.TextureHeight, "TexNormal");
                        if (!mraSet)
                            mraSet = setSrv(m.TextureSpecular, "TexMRA");

                        if (mtl.AssetName.AMeta == null)
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

                var meshPrimitives = improtSetting.MergeMeshes ?
                    MeshGenerater.GenerateMerged(improtSetting.FileName, improtSetting.AssetImporter.AiScene, AssetImportOption) :
                    MeshGenerater.Generate(skeletons, improtSetting.AssetImporter.AiScene, AssetImportOption);
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
                        ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMaterialMesh)).TypeString;
                        ameta.Description = $"This is a {typeof(TtMaterialMesh).FullName}\n";
                        ameta.SaveAMeta(ums);
                        TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                        ums.SaveAssetTo(umsRN);
                    }
                }
                return true;
            }

            static string GetImportTextureName(string textureName, string materialName, int materialIndex, string shaderName)
            {
                var name = IO.TtFileManager.GetPureName(textureName);
                if (string.IsNullOrWhiteSpace(name))
                    name = $"{materialName}_{materialIndex}";
                if (string.IsNullOrWhiteSpace(name))
                    name = $"material_{materialIndex}";

                if (!name.EndsWith(shaderName, StringComparison.OrdinalIgnoreCase))
                    name = $"{name}_{shaderName}";
                return GetSafeImportAssetName(name);
            }

            static string GetSafeImportAssetName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    return "material";

                var invalidChars = Path.GetInvalidFileNameChars();
                var builder = new StringBuilder(name.Length);
                for (int i = 0; i < name.Length; i++)
                {
                    var c = name[i];
                    if (char.IsWhiteSpace(c) || c == '/' || c == '\\' || Array.IndexOf(invalidChars, c) >= 0)
                        builder.Append('_');
                    else
                        builder.Append(c);
                }

                var result = builder.ToString().Trim().Trim('.');
                return string.IsNullOrWhiteSpace(result) ? "material" : result;
            }

            static string ResolveImportTexturePath(string texturePath, TtMeshImportSetting importSetting)
            {
                if (string.IsNullOrWhiteSpace(texturePath))
                    return null;

                texturePath = Uri.UnescapeDataString(texturePath).Replace('/', Path.DirectorySeparatorChar);
                if (Path.IsPathRooted(texturePath) && IO.TtFileManager.FileExists(texturePath))
                    return texturePath;

                string TryResolveNear(string file)
                {
                    if (string.IsNullOrEmpty(file))
                        return null;

                    var dir = Path.GetDirectoryName(file);
                    if (string.IsNullOrEmpty(dir))
                        return null;

                    var candidate = Path.Combine(dir, texturePath);
                    return IO.TtFileManager.FileExists(candidate) ? candidate : null;
                }

                return TryResolveNear(importSetting?.SourceFile) ??
                       TryResolveNear(importSetting?.IntermediateFile);
            }

            static RName ImportTextureFile(string texturePath, RName dir, string textureName)
            {
                if (string.IsNullOrWhiteSpace(texturePath) ||
                    dir == null ||
                    string.IsNullOrWhiteSpace(textureName) ||
                    !IO.TtFileManager.FileExists(texturePath))
                {
                    return null;
                }

                try
                {
                    using (var stream = System.IO.File.OpenRead(texturePath))
                    {
                        var importer = new NxRHI.TtSrView.ImportAttribute();
                        importer.mSourceFile = texturePath;
                        importer.mDir = dir;
                        importer.mName = textureName;
                        return importer.ImportImageImpl(stream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载图像失败: {ex.Message}");
                    return null;
                }
            }

            static RName ImportColorTexture(System.Numerics.Vector4 color, RName dir, string textureName)
            {
                if (dir == null || string.IsNullOrWhiteSpace(textureName))
                    return null;

                var image = StbImageSharp.TtMemImage.CreateImage(1, 1, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                image.Clear(new Color4b(ToColorByte(color.X), ToColorByte(color.Y), ToColorByte(color.Z), ToColorByte(color.W)));
                using (var stream = image.SaveToMem())
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var importer = new NxRHI.TtSrView.ImportAttribute();
                    importer.mSourceFile = textureName + ".png";
                    importer.mDir = dir;
                    importer.mName = textureName;
                    return importer.ImportImageImpl(stream);
                }
            }

            static byte ToColorByte(float value)
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    return 0;

                value = Math.Clamp(value, 0.0f, 1.0f);
                return (byte)Math.Round(value * 255.0f);
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
                    sktameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(Animation.Asset.TtSkeletonAsset)).TypeString;
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
                ameta.TypeStr = Rtti.TtTypeDesc.TypeOf(typeof(TtMeshPrimitives)).TypeString;
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
