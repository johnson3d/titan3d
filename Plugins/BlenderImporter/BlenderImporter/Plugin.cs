using EngineNS.Bricks.AssetImpExp;
using EngineNS.Graphics.Mesh;
using EngineNS.Profiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class TtBlenderImporterAssemblyDesc : TtAssemblyDesc
        {
            public TtBlenderImporterAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:BlenderImporter AssemblyDesc Created");
            }
            ~TtBlenderImporterAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:BlenderImporter AssemblyDesc Destroyed");
            }
            public override string Name { get => "BlenderImporter"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static TtBlenderImporterAssemblyDesc AssmblyDesc = new TtBlenderImporterAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.BlenderImporter
{
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static TtBlenderImporterPlugin mPluginObject = new TtBlenderImporterPlugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }

    public class TtBlenderImporterPlugin : TtAssetSourceImportPlugin
    {
        public override void OnLoadedPlugin()
        {
        }

        public override void OnUnloadPlugin()
        {
        }

        public override bool CanImportSource(string fileExt)
        {
            switch (NormalizeExtension(fileExt))
            {
                case "blend":
                case "gltf":
                case "glb":
                    return true;
                default:
                    return false;
            }
        }

        public override string GetImportingMessage(string sourceFile)
        {
            return string.Equals(NormalizeExtension(sourceFile), "blend", StringComparison.OrdinalIgnoreCase) ?
                "Converting Blender file..." :
                "Reading glTF file...";
        }

        public override TtMeshImportSetting CreateMeshImportSetting(string sourceFile, out string error)
        {
            error = null;
            var ext = NormalizeExtension(sourceFile);
            if (ext == "gltf" || ext == "glb")
                return CreateGltfImportSetting(sourceFile, out error);
            if (ext != "blend")
            {
                error = $"BlenderImporter does not support source file: {sourceFile}";
                return null;
            }

            var blendSetting = new TtBlendImportSetting()
            {
                SourceFile = sourceFile,
            };
            var exportResult = TtBlenderBridge.ExportToGlb(sourceFile, blendSetting);
            if (!exportResult.Succeeded)
            {
                error = exportResult.Error;
                return null;
            }

            var meshImportSetting = TtAssetImporter.CreateMeshImporter(exportResult.IntermediateFile);
            if (meshImportSetting == null)
            {
                error = $"Blender exported GLB but Assimp failed to import it: {exportResult.IntermediateFile}";
                return null;
            }

            meshImportSetting.SourceFile = sourceFile;
            meshImportSetting.IntermediateFile = exportResult.IntermediateFile;
            meshImportSetting.MaterialManifest = exportResult.MaterialManifest;
            meshImportSetting.ImportMessage = $"Converted from Blender by {exportResult.BlenderPath}";
            ApplySceneImportDefaults(meshImportSetting, sourceFile);
            return meshImportSetting;
        }

        public override async System.Threading.Tasks.Task<Graphics.Pipeline.Shader.TtMaterialInstance[]> ResolveMaterials(
            TtMeshImportSetting importSetting,
            RName outputDir)
        {
            var manifestPath = importSetting.MaterialManifest;
            if (string.IsNullOrWhiteSpace(manifestPath))
                return null;

            var blenderMaterialMap = LoadBlenderMaterialMap(manifestPath);
            if (blenderMaterialMap == null)
                return null;

            var scene = importSetting.AssetImporter.AiScene;
            if (scene == null || !scene.HasMaterials)
                return null;

            var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[scene.Materials.Count];
            var baseMtl = await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(
                TtEngine.Instance.ConfigManager.GetConfig<Editor.Forms.TtMeshPrimitiveEditorConfig>().ImportBaseMaterial);
            Graphics.Pipeline.Shader.TtMaterial transparentColorMtl = null;

            for (int i = 0; i < scene.Materials.Count; i++)
            {
                var sceneMaterial = scene.Materials[i];
                TtBlenderMaterialInfo blenderMaterial = null;
                blenderMaterialMap.TryGetValue(sceneMaterial.Name ?? "", out blenderMaterial);

                var useTransparentColor = TryGetBlenderTransparentColor(blenderMaterial, out var transparentColor);
                if (useTransparentColor && transparentColorMtl == null)
                {
                    transparentColorMtl = await RName.GetRName("material/sysdft_color.material", RName.ERNameType.Engine)
                        .GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
                    if (transparentColorMtl == null)
                        useTransparentColor = false;
                }

                var materialBase = useTransparentColor ? transparentColorMtl : baseMtl;
                var mtl = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(materialBase);
                var materialName = GetSafeImportAssetName($"{sceneMaterial.Name}_{i}");
                mtl.AssetName = RName.GetRName(
                    $"{outputDir.Name}{materialName}{Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt}",
                    outputDir.RNameType);

                if (useTransparentColor)
                {
                    ConfigureTransparentColorMaterial(mtl, transparentColor);
                }
                else
                {
                    var resolvedTextures = blenderMaterial?.ResolvedTextures;
                    var diffuseSet = false;
                    var mraSet = false;

                    if (resolvedTextures != null)
                    {
                        diffuseSet = TrySetManifestSrv(resolvedTextures.BaseColor, "TexDiffuse", mtl, importSetting, outputDir, sceneMaterial.Name, i);
                        var mra = ImportMraTexture(resolvedTextures, importSetting, outputDir, sceneMaterial.Name, i);
                        if (mra != null)
                        {
                            mtl.SetSrv("TexMRA", mra);
                            mraSet = true;
                        }
                    }

                    if (sceneMaterial.IsPBRMaterial)
                    {
                        if (!diffuseSet)
                            diffuseSet = TrySetAssimpSrv(sceneMaterial.PBR.TextureBaseColor, "TexDiffuse", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i);
                        if (!mraSet)
                            mraSet = TrySetAssimpSrv(sceneMaterial.PBR.TextureMetalness, "TexMRA", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i);
                    }
                    if (!diffuseSet)
                        diffuseSet = TrySetAssimpSrv(sceneMaterial.TextureDiffuse, "TexDiffuse", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i);
                    if (!diffuseSet)
                        diffuseSet = TrySetAssimpSrv(sceneMaterial.TextureAmbient, "TexDiffuse", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i);
                    if (!diffuseSet)
                    {
                        var colorTextureName = GetSafeImportAssetName($"{sceneMaterial.Name}_{i}_TexDiffuse");
                        var rn = ImportColorTexture(sceneMaterial.ColorDiffuse, outputDir, colorTextureName);
                        if (rn != null)
                        {
                            mtl.SetSrv("TexDiffuse", rn);
                            diffuseSet = true;
                        }
                    }

                    var normalSet = resolvedTextures != null &&
                        TrySetManifestSrv(resolvedTextures.Normal, "TexNormal", mtl, importSetting, outputDir, sceneMaterial.Name, i);
                    if (!normalSet && !TrySetAssimpSrv(sceneMaterial.TextureNormal, "TexNormal", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i))
                        TrySetAssimpSrv(sceneMaterial.TextureHeight, "TexNormal", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i);
                    if (!mraSet)
                        TrySetAssimpSrv(sceneMaterial.TextureSpecular, "TexMRA", mtl, importSetting, outputDir, scene, sceneMaterial.Name, i);
                }

                if (mtl.AssetName.AMeta == null)
                {
                    var ameta = mtl.CreateAMeta();
                    ameta.AssetId = IO.IAssetMeta.AcquireAssetId(mtl.AssetName);
                    ameta.SetAssetName(mtl.AssetName);
                    ameta.SaveAMeta(mtl);
                    TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }

                mtl.SaveAssetTo(mtl.AssetName);
                materials[i] = mtl;
            }

            return materials;
        }

        #region Material Resolution Helpers

        static Dictionary<string, TtBlenderMaterialInfo> LoadBlenderMaterialMap(string manifestPath)
        {
            var manifest = TtBlenderBridge.LoadMaterialManifest(manifestPath);
            if (manifest?.Materials == null || manifest.Materials.Count == 0)
                return null;

            var result = new Dictionary<string, TtBlenderMaterialInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var material in manifest.Materials)
            {
                if (material == null || string.IsNullOrWhiteSpace(material.Name))
                    continue;
                if (!result.ContainsKey(material.Name))
                    result.Add(material.Name, material);
            }
            return result.Count > 0 ? result : null;
        }

        static bool TryGetBlenderTransparentColor(TtBlenderMaterialInfo material, out Vector4 color)
        {
            color = Vector4.One;
            if (material == null || HasAnyTexture(material.ResolvedTextures))
                return false;

            var alpha = 1.0f;
            if (TryGetPrincipledFloat(material, "Alpha", out var principledAlpha))
                alpha = principledAlpha;

            if (alpha >= 0.98f)
                return false;

            var hasTransparentBlend = !string.IsNullOrWhiteSpace(material.BlendMethod) &&
                !string.Equals(material.BlendMethod, "OPAQUE", StringComparison.OrdinalIgnoreCase);
            if (!hasTransparentBlend)
                return false;

            if (!TryGetPrincipledColor(material, "Base Color", out color))
                color = Vector4.One;
            color.W = Math.Clamp(alpha, 0.0f, 1.0f);
            return true;
        }

        static void ConfigureTransparentColorMaterial(Graphics.Pipeline.Shader.TtMaterialInstance material, in Vector4 color)
        {
            if (material == null)
                return;

            material.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
            unsafe
            {
                var rsState = material.Rasterizer;
                rsState.CullMode = NxRHI.ECullMode.CMD_NONE;
                material.Rasterizer = rsState;

                var dsState = material.DepthStencil;
                dsState.DepthWriteMask = 0;
                material.DepthStencil = dsState;
            }

            var colorVar = material.FindVar("clr4_0");
            if (colorVar != null)
                colorVar.SetValue(color);
        }

        static bool HasAnyTexture(TtBlenderResolvedTextures textures)
        {
            return textures?.BaseColor != null ||
                   textures?.Normal != null ||
                   textures?.Metallic != null ||
                   textures?.Roughness != null ||
                   textures?.Specular != null;
        }

        static bool TryGetPrincipledFloat(TtBlenderMaterialInfo material, string name, out float value)
        {
            value = 0;
            if (material?.Principled == null ||
                !material.Principled.TryGetValue(name, out var element))
            {
                return false;
            }
            return TryReadFloat(element, out value);
        }

        static bool TryGetPrincipledColor(TtBlenderMaterialInfo material, string name, out Vector4 color)
        {
            color = Vector4.One;
            if (material?.Principled == null ||
                !material.Principled.TryGetValue(name, out var element) ||
                element.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var components = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            var index = 0;
            foreach (var component in element.EnumerateArray())
            {
                if (index >= components.Length)
                    break;
                if (TryReadFloat(component, out var v))
                    components[index] = v;
                index++;
            }
            if (index == 0)
                return false;

            color = new Vector4(components[0], components[1], components[2], components[3]);
            return true;
        }

        static bool TryReadFloat(JsonElement element, out float value)
        {
            value = 0;
            try
            {
                if (element.ValueKind == JsonValueKind.Number)
                {
                    value = (float)element.GetDouble();
                    return true;
                }
                if (element.ValueKind == JsonValueKind.String)
                {
                    return float.TryParse(element.GetString(),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out value);
                }
            }
            catch
            {
            }
            return false;
        }

        static bool TrySetManifestSrv(TtBlenderTextureRef texture, string shaderName,
            Graphics.Pipeline.Shader.TtMaterialInstance mtl,
            TtMeshImportSetting importSetting, RName outputDir,
            string materialName, int materialIndex)
        {
            if (texture == null)
                return false;

            var texturePath = ResolveBlenderTexturePath(texture, importSetting);
            if (string.IsNullOrEmpty(texturePath))
                return false;

            var textureNameSource = !string.IsNullOrWhiteSpace(texture.FilePath) ? texture.FilePath : texture.Image;
            var textureName = GetImportTextureName(textureNameSource, materialName, materialIndex, shaderName);
            var rn = ImportTextureFile(texturePath, outputDir, textureName);
            if (rn == null)
                return false;

            mtl.SetSrv(shaderName, rn);
            return true;
        }

        static bool TrySetAssimpSrv(Assimp.TextureSlot slot, string shaderName,
            Graphics.Pipeline.Shader.TtMaterialInstance mtl,
            TtMeshImportSetting importSetting, RName outputDir,
            Assimp.Scene scene, string materialName, int materialIndex)
        {
            if (slot.FilePath == null)
                return false;

            int textureIndex = -1;
            if (slot.FilePath.StartsWith("*"))
                textureIndex = int.Parse(slot.FilePath.Substring(1));

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
                            using (var stream = new MemoryStream(texture.CompressedData))
                            {
                                var importer = new NxRHI.TtSrView.ImportAttribute();
                                var textureName = GetImportTextureName(texture.Filename, materialName, materialIndex, shaderName);
                                importer.mSourceFile = textureName + ".png";
                                importer.mDir = outputDir;
                                importer.mName = textureName;
                                stream.Seek(0, SeekOrigin.Begin);
                                var srv = NxRHI.TtSrView.ImportImage(stream, importer, true);
                                if (srv == null)
                                    return false;
                                mtl.SetSrv(shaderName, srv.AssetName);
                                return true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }
                    }
                }
            }
            else
            {
                var texturePath = ResolveImportTexturePath(slot.FilePath, importSetting);
                if (!string.IsNullOrEmpty(texturePath) && IO.TtFileManager.FileExists(texturePath))
                {
                    var textureName = GetImportTextureName(texturePath, materialName, materialIndex, shaderName);
                    var rn = ImportTextureFile(texturePath, outputDir, textureName);
                    if (rn != null)
                    {
                        mtl.SetSrv(shaderName, rn);
                        return true;
                    }
                }
            }
            return false;
        }

        static string ResolveBlenderTexturePath(TtBlenderTextureRef texture, TtMeshImportSetting importSetting)
        {
            if (texture == null)
                return null;

            var texturePath = ResolveImportTexturePath(texture.FilePath, importSetting);
            if (string.IsNullOrEmpty(texturePath) && !string.IsNullOrWhiteSpace(texture.Image))
            {
                texturePath = ResolveImportTexturePath(texture.Image, importSetting) ??
                              ResolveImportTexturePath(Path.Combine("textures", texture.Image), importSetting);
            }
            return texturePath;
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

        static RName ImportMraTexture(TtBlenderResolvedTextures textures, TtMeshImportSetting importSetting, RName dir, string materialName, int materialIndex)
        {
            if (textures == null || dir == null)
                return null;

            var metallicPath = ResolveBlenderTexturePath(textures.Metallic, importSetting);
            var roughnessPath = ResolveBlenderTexturePath(textures.Roughness, importSetting);
            var specularPath = ResolveBlenderTexturePath(textures.Specular, importSetting);
            if (string.IsNullOrEmpty(metallicPath) && string.IsNullOrEmpty(roughnessPath) && string.IsNullOrEmpty(specularPath))
                return null;

            var metallicImage = LoadImage(metallicPath);
            var roughnessImage = LoadImage(roughnessPath);
            var specularImage = LoadImage(specularPath);
            var sourceImage = roughnessImage ?? metallicImage ?? specularImage;
            if (sourceImage == null)
                return null;

            // enginecontent/material/pbr.material reads TexMRA as AO, Roughness, Metallic.
            var image = StbImageSharp.TtMemImage.CreateImage(sourceImage.Width, sourceImage.Height, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var ao = (byte)255;
                    var roughness = SampleLuminance(roughnessImage, x, y, image.Width, image.Height, 255);
                    var metallic = SampleLuminance(metallicImage, x, y, image.Width, image.Height, 0);
                    image.SetPixel(x, y, new Color4b(ao, roughness, metallic, 255));
                }
            }

            var textureNameSource = roughnessPath ?? metallicPath ?? specularPath ?? materialName;
            var textureName = GetImportTextureName(textureNameSource, materialName, materialIndex, "TexMRA");
            using (var stream = image.SaveToMem())
            {
                stream.Seek(0, SeekOrigin.Begin);
                var importer = new NxRHI.TtSrView.ImportAttribute();
                importer.mSourceFile = textureName + ".png";
                importer.mDir = dir;
                importer.mName = textureName;
                var srv = NxRHI.TtSrView.ImportImage(stream, importer, true);
                if (srv == null)
                    return null;
                return srv.AssetName;
            }
        }

        static StbImageSharp.TtMemImage LoadImage(string texturePath)
        {
            if (string.IsNullOrWhiteSpace(texturePath) || !IO.TtFileManager.FileExists(texturePath))
                return null;

            try
            {
                using (var stream = File.OpenRead(texturePath))
                {
                    return StbImageSharp.TtMemImage.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                return null;
            }
        }

        static byte SampleLuminance(StbImageSharp.TtMemImage image, int x, int y, int width, int height, byte fallback)
        {
            if (image == null || image.Width <= 0 || image.Height <= 0 || width <= 0 || height <= 0)
                return fallback;

            var sx = Math.Clamp((int)((long)x * image.Width / width), 0, image.Width - 1);
            var sy = Math.Clamp((int)((long)y * image.Height / height), 0, image.Height - 1);
            var color = image.GetPixel(sx, sy);
            return (byte)Math.Round(color.R * 0.2126f + color.G * 0.7152f + color.B * 0.0722f);
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
                using (var stream = File.OpenRead(texturePath))
                {
                    var importer = new NxRHI.TtSrView.ImportAttribute();
                    importer.mSourceFile = texturePath;
                    importer.mDir = dir;
                    importer.mName = textureName;
                    var srv = NxRHI.TtSrView.ImportImage(stream, importer, true);
                    if (srv == null)
                    {
                        return null;
                    }
                    return srv.AssetName;
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
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
                var srv = NxRHI.TtSrView.ImportImage(stream, importer, true);
                if (srv == null)
                {
                    return null;
                }
                return srv.AssetName;
            }
        }

        static byte ToColorByte(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return 0;
            value = Math.Clamp(value, 0.0f, 1.0f);
            return (byte)Math.Round(value * 255.0f);
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
            var builder = new System.Text.StringBuilder(name.Length);
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

        #endregion

        static TtMeshImportSetting CreateGltfImportSetting(string sourceFile, out string error)
        {
            error = null;
            var meshImportSetting = TtAssetImporter.CreateMeshImporter(sourceFile);
            if (meshImportSetting == null)
            {
                error = $"Assimp failed to import glTF source file: {sourceFile}";
                return null;
            }

            meshImportSetting.SourceFile = sourceFile;
            meshImportSetting.IntermediateFile = sourceFile;
            meshImportSetting.ImportMessage = "Imported glTF directly by BlenderImporter";
            ApplySceneImportDefaults(meshImportSetting, sourceFile);
            return meshImportSetting;
        }

        static void ApplySceneImportDefaults(TtMeshImportSetting meshImportSetting, string sourceFile)
        {
            meshImportSetting.FileName = IO.TtFileManager.GetPureName(sourceFile);
            meshImportSetting.UnitScale = 1.0f;
            meshImportSetting.AsStaticMesh = true;
            meshImportSetting.ApplyTransformToVertex = true;
            meshImportSetting.MergeMeshes = true;
        }
    }
}
