using System;
using System.Collections.Generic;
using System.Text.Json;
using EngineNS.IO;

namespace EngineNS.Plugins.MCPServer
{
    public partial class TtMCPServerPlugin
    {
        /// <summary>
        /// 资产迁移的标准入口: 把资产原样加载一遍, 再 SaveAssetTo 写回原地。
        ///
        /// 用途是引擎侧的数据结构改名/改版之后, 让盘上的旧资产把自己重新序列化一次。
        /// 这个工具的由来: 材质图里 UUniformVar 的 out pin 名字直接取自
        /// TtShaderDefineAttribute.ShaderName, UPinLinker 存盘存的是 pin 名字符串, 而
        /// TtMaterial.HLSLCode 更是把生成好的整段 HLSL 缓存在资产里。引擎侧把 vLightMap
        /// 改名成 vExtraUV 之后, 旧材质缓存的代码仍写着 input.vLightMap, 加载即编译失败;
        /// 而 SaveAssetTo 会走一遍 GenMateralGraphCode 重新生成代码, 存一次就修好了。
        ///
        /// 前提是引擎侧先做好读盘兼容 (比如 TtShaderDefineAttribute.LegacyShaderName 让旧
        /// 连线能回退到改名后的 pin)。否则加载阶段数据就已经丢了 —— Linker.SaveData 找不到
        /// pin 会静默 Remove 掉连线 —— 这时候重存只会把残缺状态固化到盘上, 比不动更糟。
        /// 所以默认 dryRun=true: 先看清要动哪些文件, 确认兼容做到位了再真写。
        /// </summary>
        [Bricks.AIGC.TtMCPTool("resave_assets",
            "Asset migration helper: load each target asset and write it straight back via SaveAssetTo, so the " +
            "on-disk data gets re-serialized by current engine code. Use it after renaming/reversioning engine-side " +
            "structures that assets cache by name (material graph pin names, cached generated HLSL, etc). " +
            "IMPORTANT: make sure engine-side load compatibility exists first (e.g. TtShaderDefineAttribute." +
            "LegacyShaderName) - if loading already drops data, resaving freezes the damage. Defaults to dryRun=true; " +
            "pass dryRun=false to actually write. Either 'assets' or 'directory' must be given; it will not resave the whole project.",
            returnDescription: "{dryRun, requested: number, planned: string[] - assets that would be/were touched, " +
            "resaved: number, failed: number, " +
            "results:[{asset, ok, error, sizeBefore, sizeAfter, lastWriteUtc, migratedSerializedData}]}")]
        public static string ResaveAssets(
            [Bricks.AIGC.TtMCPParameter("Comma separated asset RNames, e.g. 'tutorials/PivotPainter/house.material,tutorials/PivotPainter/pivot_wind.material'. Takes precedence over directory")] string assets = "",
            [Bricks.AIGC.TtMCPParameter("Resave every registered asset whose RName starts with this directory prefix. Ignored when 'assets' is given")] string directory = "",
            [Bricks.AIGC.TtMCPParameter("Extension filter used with 'directory', e.g. '.material'. Empty means any type")] string typeExt = "",
            [Bricks.AIGC.TtMCPParameter("When true (default) only report what would be resaved, without writing anything")] bool dryRun = true,
            [Bricks.AIGC.TtMCPParameter("Safety cap on how many assets one call may touch")] double maxCount = 32,
            [Bricks.AIGC.TtMCPParameter("Per-asset timeout in ms. Saving a material regenerates its HLSL, so keep this generous")] double timeoutMsPerAsset = 60000)
        {
            try
            {
                LogToolCall("resave_assets",
                    $"assets={assets}, directory={directory}, typeExt={typeExt}, dryRun={dryRun}, maxCount={maxCount}");

                if (string.IsNullOrEmpty(assets) && string.IsNullOrEmpty(directory))
                {
                    return JsonSerializer.Serialize(new
                    {
                        error = "Either 'assets' or 'directory' is required. Refusing to resave the whole project."
                    });
                }

                int limit = Math.Max(1, Math.Min(200, (int)maxCount));
                int timeout = Math.Max(1000, (int)timeoutMsPerAsset);

                var targets = CollectResaveTargets(assets, directory, typeExt, limit, out var collectError);
                if (collectError != null)
                    return JsonSerializer.Serialize(new { error = collectError });

                var planned = new List<string>();
                foreach (var rn in targets)
                    planned.Add(rn.ToString());

                if (dryRun)
                {
                    return JsonSerializer.Serialize(new
                    {
                        dryRun = true,
                        requested = targets.Count,
                        planned,
                        resaved = 0,
                        failed = 0,
                        results = new List<Dictionary<string, object>>(),
                        hint = "Nothing was written. Re-run with dryRun=false to actually resave."
                    });
                }

                int resaved = 0, failed = 0;
                var results = new List<Dictionary<string, object>>();
                foreach (var rn in targets)
                {
                    var entry = ResaveOne(rn, timeout);
                    if ((bool)entry["ok"])
                        resaved++;
                    else
                        failed++;
                    results.Add(entry);
                }

                return JsonSerializer.Serialize(new
                {
                    dryRun = false,
                    requested = targets.Count,
                    planned,
                    resaved,
                    failed,
                    results
                });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }

        /// <summary> 解析出要重存的资产列表。显式 assets 优先, 否则按目录前缀 + 扩展名筛。 </summary>
        private static List<RName> CollectResaveTargets(string assets, string directory, string typeExt,
            int limit, out string error)
        {
            error = null;
            var targets = new List<RName>();

            if (string.IsNullOrEmpty(assets) == false)
            {
                foreach (var s in assets.Split(','))
                {
                    var t = s.Trim();
                    if (t.Length == 0)
                        continue;
                    if (targets.Count >= limit)
                        break;
                    targets.Add(RName.GetRName(t));
                }
                if (targets.Count == 0)
                    error = "'assets' contained no usable asset name.";
                return targets;
            }

            // 遍历 AssetMetaManager.Assets 必须在主线程: 主线程随时可能因为资源加载往字典里插
            // 条目, 从 MCP 工作线程边改边遍历会抛 InvalidOperationException。
            var ok = TtMainThreadDispatcher.Invoke(() =>
            {
                foreach (var a in TtEngine.Instance.AssetMetaManager.Assets.Values)
                {
                    if (a == null || a.AssetName == null)
                        continue;
                    var name = a.AssetName.Name;
                    if (name.StartsWith(directory, StringComparison.OrdinalIgnoreCase) == false)
                        continue;
                    if (string.IsNullOrEmpty(typeExt) == false &&
                        name.EndsWith(typeExt, StringComparison.OrdinalIgnoreCase) == false)
                        continue;
                    if (targets.Count >= limit)
                        break;
                    targets.Add(a.AssetName);
                }
            });
            if (ok == false)
                error = "Timed out waiting for the engine main thread to enumerate registered assets.";
            else if (targets.Count == 0)
                error = $"No registered asset matches directory '{directory}' with typeExt '{typeExt}'.";
            return targets;
        }

        private static Dictionary<string, object> ResaveOne(RName rn, int timeoutMs)
        {
            var entry = new Dictionary<string, object>
            {
                ["asset"] = rn.ToString(),
                ["ok"] = false,
                ["error"] = "",
                ["sizeBefore"] = 0L,
                ["sizeAfter"] = 0L,
                ["lastWriteUtc"] = "",
                ["migratedSerializedData"] = false,
            };

            try
            {
                var meta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                if (meta == null)
                {
                    entry["error"] = "No .ameta registered for this asset.";
                    return entry;
                }
                // IAssetMeta.GetAsset 的基类实现是 Debug.Assert(false) + return null, 在 Debug 构建里
                // 会直接弹断言框把编辑器卡住。所以先反射确认这个资产类型真的重写了它, 宁可报错也不去踩。
                if (OverridesGetAsset(meta) == false)
                {
                    entry["error"] = $"AMeta type '{meta.GetType().Name}' does not override GetAsset, " +
                        "this asset type cannot be loaded generically.";
                    return entry;
                }

                string address = null;
                try
                {
                    address = rn.Address;
                    var fi = new System.IO.FileInfo(address);
                    if (fi.Exists)
                        entry["sizeBefore"] = fi.Length;
                }
                catch { }

                Exception failure = null;
                var migrated = false;
                var ok = TtMainThreadDispatcher.Invoke(() =>
                {
                    // GetAsset 是 async, 它的同步等待链依赖 TtContextThread.CurrentContext,
                    // 那是 [ThreadStatic] 且只在引擎线程上有值 —— 所以加载和保存都得在这个回调里做完。
                    var asset = meta.GetAsset().GetResultUntilCompleted();
                    if (asset == null)
                    {
                        failure = new Exception("GetAsset returned null (asset failed to load).");
                        return;
                    }
                    // SaveAssetTo 通常只重新生成派生数据 (材质是 HLSLCode), 资产内缓存的那份序列化
                    // 数据是原样写回的 —— 旧字段名会一直留着, 永久依赖加载期回退。实现了迁移接口的
                    // 资产类型在这里先把缓存按当前结构重写一遍, 旧名才真正落地成新名。
                    if (asset is IAssetSerializedDataMigration migration)
                        migrated = migration.MigrateSerializedData();
                    asset.SaveAssetTo(asset.AssetName ?? rn);
                }, timeoutMs);

                if (ok == false)
                {
                    entry["error"] = $"Timed out after {timeoutMs} ms on the engine main thread.";
                    return entry;
                }
                if (failure != null)
                {
                    entry["error"] = failure.Message;
                    return entry;
                }

                try
                {
                    var fi = new System.IO.FileInfo(address);
                    if (fi.Exists)
                    {
                        entry["sizeAfter"] = fi.Length;
                        entry["lastWriteUtc"] = fi.LastWriteTimeUtc.ToString("o");
                    }
                }
                catch { }

                entry["migratedSerializedData"] = migrated;
                entry["ok"] = true;
                return entry;
            }
            catch (Exception ex)
            {
                entry["error"] = ex.Message;
                return entry;
            }
        }

        private static bool OverridesGetAsset(IAssetMeta meta)
        {
            try
            {
                var m = meta.GetType().GetMethod("GetAsset", new Type[] { typeof(object[]) });
                return m != null && m.DeclaringType != typeof(IAssetMeta);
            }
            catch
            {
                return false;
            }
        }
    }
}
