using System;
using System.Collections.Generic;
using System.Text.Json;
using EngineNS.IO;

namespace EngineNS.Plugins.MCPServer
{
    public partial class TtMCPServerPlugin
    {
        [Bricks.AIGC.TtMCPTool("list_assets", "Lists asset files under a given directory path in the engine",
            returnDescription: "{assets: string[] - Matched asset paths, " +
            "total: number - Total matched count, " +
            "returned: number - Number of assets returned (limited by maxCount)}")]
        public static string ListAssets(
            [Bricks.AIGC.TtMCPParameter("Directory path relative to game root, empty for all directories")] string directory = "",
            [Bricks.AIGC.TtMCPParameter("Filter keyword to match asset name, supports partial match, empty for no filter")] string filter = "",
            [Bricks.AIGC.TtMCPParameter("Asset source type: Game, Engine, Transient, Cloud, or empty for all")] string assetType = "",
            [Bricks.AIGC.TtMCPParameter("Maximum number of assets to return")] double maxCount = 100)
        {
            try
            {
                RName.ERNameType? typeFilter = null;
                if (!string.IsNullOrEmpty(assetType) &&
                    Enum.TryParse<RName.ERNameType>(assetType, true, out var parsedType))
                {
                    typeFilter = parsedType;
                }

                int limit = Math.Max(1, (int)maxCount);
                var resultAssets = new List<string>();
                int totalMatched = 0;

                foreach (var a in TtEngine.Instance.AssetMetaManager.Assets.Values)
                {
                    if (typeFilter.HasValue && a.AssetName.RNameType != typeFilter.Value)
                        continue;

                    var assetName = a.AssetName.Name;

                    if (!string.IsNullOrEmpty(directory) && !assetName.StartsWith(directory))
                        continue;

                    if (!string.IsNullOrEmpty(filter) &&
                        assetName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    totalMatched++;
                    if (resultAssets.Count < limit)
                        resultAssets.Add(a.AssetName.ToString());
                }

                return JsonSerializer.Serialize(new
                {
                    assets = resultAssets,
                    total = totalMatched,
                    returned = resultAssets.Count
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }
    }
}
