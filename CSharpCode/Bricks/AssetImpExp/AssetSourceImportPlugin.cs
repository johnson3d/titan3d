using EngineNS.Bricks.AssemblyLoader;
using EngineNS.Bricks.SourceControl;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.AssetImpExp
{
    public abstract class TtAssetSourceImportPlugin : IPlugin
    {
        public virtual void OnLoadedPlugin()
        {
        }

        public virtual void OnUnloadPlugin()
        {
        }

        public abstract bool CanImportSource(string fileExt);
        public virtual string GetImportingMessage(string sourceFile)
        {
            return "Reading source file...";
        }
        public abstract Graphics.Mesh.TtMeshImportSetting CreateMeshImportSetting(string sourceFile, out string error);

        /// <summary>
        /// Resolve materials for the imported mesh. Called by ImportAndSaveMesh before the default
        /// Assimp material pipeline. Return a non-null array to skip the default material creation.
        /// Each element corresponds to a scene material by index; null elements will use the default material.
        /// </summary>
        public virtual async System.Threading.Tasks.Task<Graphics.Pipeline.Shader.TtMaterialInstance[]> ResolveMaterials(
            Graphics.Mesh.TtMeshImportSetting importSetting,
            RName outputDir)
        {
            return null;
        }

        public static TtAssetSourceImportPlugin GetPluginForSource(string sourceFileOrExt)
        {
            var ext = NormalizeExtension(sourceFileOrExt);
            if (string.IsNullOrEmpty(ext) || TtEngine.Instance?.PluginModuleManager == null)
                return null;

            TtAssetSourceImportPlugin result = null;
            var plugin = TtEngine.Instance.PluginModuleManager.GetPluginModule("BlenderImporter");
            if (plugin != null)
                result = plugin.GetPluginObject<TtAssetSourceImportPlugin>();
            if (result != null && result.CanImportSource(ext))
                return result;
            return null;
        }

        public static bool HasPluginForSource(string sourceFileOrExt)
        {
            return GetPluginForSource(sourceFileOrExt) != null;
        }

        protected static string NormalizeExtension(string sourceFileOrExt)
        {
            if (string.IsNullOrWhiteSpace(sourceFileOrExt))
                return "";

            var ext = IO.TtFileManager.GetExtName(sourceFileOrExt);
            if (string.IsNullOrWhiteSpace(ext))
                ext = sourceFileOrExt;
            return ext.Trim().TrimStart('.').ToLowerInvariant();
        }
    }
}
