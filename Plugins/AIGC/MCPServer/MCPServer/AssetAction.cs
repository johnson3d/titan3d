using System;
using System.Collections.Generic;
using System.Text.Json;
using EngineNS.IO;

namespace EngineNS.Plugins.MCPServer
{
    public partial class TtMCPServerPlugin
    {
        [Bricks.AIGC.TtMCPTool("list_assets",
            "Lists registered engine assets with stable ordering and optional directory, name, RName root, " +
            "extension, and asset type filters. The legacy assets string array is retained; items contains " +
            "structured metadata suitable for scene-generation asset selection.",
            returnDescription: "{assets:string[] - Backward-compatible RName list, items:[{rname,rootType," +
            "extension,typeName,assetId}],total:int,returned:int,truncated:boolean,error:string - Present on failure}")]
        public static string ListAssets(
            [Bricks.AIGC.TtMCPParameter("Directory path relative to an RName root, empty for all directories")] string directory = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive keyword matched against the asset RName")] string filter = "",
            [Bricks.AIGC.TtMCPParameter("RName root type: Game, Engine, Transient, Cloud, or empty for all")] string assetType = "",
            [Bricks.AIGC.TtMCPParameter("Maximum number of assets to return")] double maxCount = 100,
            [Bricks.AIGC.TtMCPParameter("Asset extension with or without a leading dot, for example ums or .scene")] string extension = "",
            [Bricks.AIGC.TtMCPParameter("Case-insensitive substring of the asset metadata type name")] string typeNameFilter = "")
        {
            try
            {
                RName.ERNameType? rootTypeFilter = null;
                if (!string.IsNullOrWhiteSpace(assetType))
                {
                    if (!Enum.TryParse<RName.ERNameType>(assetType.Trim(), true, out var parsedType))
                    {
                        return JsonSerializer.Serialize(new
                        {
                            error = $"unknown assetType '{assetType}'",
                            validAssetTypes = Enum.GetNames(typeof(RName.ERNameType)),
                        });
                    }
                    rootTypeFilter = parsedType;
                }

                int limit = Math.Max(1, (int)maxCount);
                var directoryText = (directory ?? "").Trim().Replace('\\', '/').TrimStart('/');
                var nameText = (filter ?? "").Trim();
                var extensionText = (extension ?? "").Trim().TrimStart('.');
                var typeNameText = (typeNameFilter ?? "").Trim();
                var matches = new List<IAssetMeta>();

                var dispatched = TtMainThreadDispatcher.Invoke(() =>
                {
                    foreach (var meta in TtEngine.Instance.AssetMetaManager.Assets.Values)
                    {
                        var assetName = meta?.AssetName;
                        if (assetName == null)
                            continue;
                        if (rootTypeFilter.HasValue && assetName.RNameType != rootTypeFilter.Value)
                            continue;

                        var normalizedName = (assetName.Name ?? "").Replace('\\', '/');
                        if (directoryText.Length > 0 &&
                            !normalizedName.StartsWith(directoryText, StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (nameText.Length > 0 &&
                            normalizedName.IndexOf(nameText, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        var metaExtension = (meta.TypeExt ?? System.IO.Path.GetExtension(normalizedName) ?? "").TrimStart('.');
                        if (extensionText.Length > 0 &&
                            !string.Equals(metaExtension, extensionText, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var typeName = meta.GetAssetTypeName() ?? "";
                        if (typeNameText.Length > 0 &&
                            typeName.IndexOf(typeNameText, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        matches.Add(meta);
                    }
                });
                if (!dispatched)
                    return JsonSerializer.Serialize(new { error = "timed out waiting for the engine main thread" });

                matches.Sort((left, right) => string.Compare(
                    left.AssetName.ToString(), right.AssetName.ToString(), StringComparison.OrdinalIgnoreCase));

                var resultAssets = new List<string>();
                var items = new List<object>();
                int returned = Math.Min(matches.Count, limit);
                for (int i = 0; i < returned; ++i)
                {
                    var meta = matches[i];
                    var rname = meta.AssetName.ToString();
                    resultAssets.Add(rname);
                    items.Add(new
                    {
                        rname,
                        rootType = meta.AssetName.RNameType.ToString(),
                        extension = (meta.TypeExt ?? System.IO.Path.GetExtension(meta.AssetName.Name) ?? "").TrimStart('.'),
                        typeName = meta.GetAssetTypeName() ?? "",
                        assetId = meta.AssetId.ToString(),
                    });
                }

                return JsonSerializer.Serialize(new
                {
                    assets = resultAssets,
                    items,
                    total = matches.Count,
                    returned,
                    truncated = returned < matches.Count,
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }
    }
}
