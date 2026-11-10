using System;
using System.Collections.Generic;

namespace EngineNS.IO
{
    /// <summary>
    /// 单个 cook job 的终态。与命令行 exit code 的映射: Success → 0, 其余 → 1。
    /// </summary>
    public enum ECookJobStatus
    {
        Success,
        /// <summary>资产定位/meta/物化/cook 任一步失败</summary>
        Failed,
        /// <summary>被请求取消并在检查点正常退出 (不视为错误, 但也不算完成)</summary>
        Cancelled,
    }

    /// <summary>
    /// cook job 结果 (状态 + 失败摘要)。给 CookServer 决定 job.error 的 kind 用,
    /// 命令行只需 exit code, 故 <see cref="TtCookDriver"/> 同时保留 int 形态的入口。
    /// </summary>
    public struct FCookJobResult
    {
        public ECookJobStatus Status;
        public string Error;

        public bool IsSuccess => Status == ECookJobStatus.Success;
        public static FCookJobResult Success() => new FCookJobResult { Status = ECookJobStatus.Success };
        public static FCookJobResult Failed(string error) => new FCookJobResult { Status = ECookJobStatus.Failed, Error = error };
        public static FCookJobResult Cancelled() => new FCookJobResult { Status = ECookJobStatus.Cancelled };
    }

    /// <summary>
    /// 通用 cook 驱动 (下沉自 TitanCMD 的 Cook 命令, 逻辑与日志文案保持一致)。
    /// 本类不包含任何资产类型专有逻辑 —— 每个类型 cook 什么全部实现于
    /// <see cref="IAssetMeta.CookAsset"/> 的重写里, 新增类型只需改对应 TtXxxAMeta。
    /// 两个消费方:
    /// 1) <c>TitanCMD ExeCmd=Cook</c> (一次性批量/单资产, 走 <see cref="TtCookLogSink"/>);
    /// 2) <c>ExeCmd=CookServer</c> 常驻 worker (逐 job 调用, 走 <see cref="TtCookProtocolSink"/>)。
    /// 实例持有类型表与黑名单, 属进程级配置, 用 <see cref="Instance"/>。
    /// </summary>
    public class TtCookDriver
    {
        public static readonly TtCookDriver Instance = new TtCookDriver();

        private readonly Dictionary<string, Type> mAssetTypes = new Dictionary<string, Type>();
        private readonly List<string> mBlacklistEntries = new List<string>();

        #region 类型表 / 黑名单
        /// <summary>
        /// 反射枚举全部 IAssetMeta 子类, 建立 "类型名 → meta 类型" 表。
        /// 引擎启动后任意时刻可调, 重复调用只是重建表。
        /// </summary>
        public void CollectAssetTypes()
        {
            mAssetTypes.Clear();
            Rtti.TtTypeDescManager.Instance.InterateTypes((cb) =>
            {
                if (cb.SystemType.IsSubclassOf(typeof(IAssetMeta)))
                {
                    var ameta = Rtti.TtTypeDescManager.CreateInstance(cb) as IAssetMeta;
                    mAssetTypes[ameta.GetAssetTypeName()] = cb.SystemType;
                }
            });
        }

        /// <summary>类型名列表 (CookServer 的 hello 用它声明能力)</summary>
        public List<string> GetAssetTypeNames()
        {
            if (mAssetTypes.Count == 0)
                CollectAssetTypes();
            return new List<string>(mAssetTypes.Keys);
        }

        public bool TryGetAssetType(string typeName, out Type metaType)
        {
            if (mAssetTypes.Count == 0)
                CollectAssetTypes();
            return mAssetTypes.TryGetValue(typeName, out metaType);
        }

        /// <summary>
        /// 载入黑名单。blacklistFile 为空时回落到 DynConfig 的 "Blacklist" 配置项;
        /// 两者都没有则清空名单 (黑名单只在批量侧生效, 与既有行为一致)。
        /// </summary>
        public void LoadBlacklist(string blacklistFile)
        {
            mBlacklistEntries.Clear();
            if (string.IsNullOrEmpty(blacklistFile))
            {
                if (TtEngine.Instance.DynConfigData.TryGetConfig<string>("Blacklist", out var cfgBlacklist))
                {
                    blacklistFile = cfgBlacklist;
                }
            }
            if (string.IsNullOrEmpty(blacklistFile) || System.IO.File.Exists(blacklistFile) == false)
            {
                if (string.IsNullOrEmpty(blacklistFile) == false)
                {
                    Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Warning, $"blacklist file not found: {blacklistFile}");
                }
                return;
            }
            foreach (var i in System.IO.File.ReadAllLines(blacklistFile))
            {
                var line = i.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                mBlacklistEntries.Add(line.Replace('\\', '/'));
            }
        }

        public bool IsBlacklisted(string relativePath)
        {
            if (mBlacklistEntries.Count == 0)
                return false;
            var normalizedPath = relativePath.Replace('\\', '/');
            foreach (var entry in mBlacklistEntries)
            {
                if (normalizedPath.Equals(entry, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (normalizedPath.StartsWith(entry + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        #endregion

        #region 单资产
        /// <summary>
        /// 单资产 cook, 返回 exit code 语义 (0 成功 / 1 失败)。给命令行用。
        /// </summary>
        public async Thread.Async.TtTask<int> CookSingleAssetAsync(string assetPath, TtCookContext ctx = null)
        {
            var result = await RunJobAsync(assetPath, ctx);
            return result.IsSuccess ? 0 : 1;
        }

        /// <summary>
        /// 单资产 cook 主体: 给定资产相对路径 (可带扩展名), 依次在 Game/Engine root 定位存在的资产文件,
        /// 经对应 IAssetMeta 走与批量完全相同的 CreateAsset → CookAsset 流程。
        /// ctx 为 null 时按 job 语义新建一个只写日志的上下文 (命令行行为)。
        /// </summary>
        public async Thread.Async.TtTask<FCookJobResult> RunJobAsync(string assetPath, TtCookContext ctx = null)
        {
            assetPath = assetPath.Replace('\\', '/');
            var rn = ResolveAssetRName(assetPath);
            if (rn == null)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"Cook: asset not found in Game/Engine root: {assetPath}");
                return FCookJobResult.Failed($"asset not found in Game/Engine root: {assetPath}");
            }

            ctx = ctx ?? new TtCookContext(0, rn.ToString(), null, false, TtCookLogSink.Instance);
            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info, $"Cook single asset begin: {rn}");

            var world = new GamePlay.TtWorld(null);
            await world.InitWorld();
            try
            {
                return await CookResolvedAsset(rn, world, ctx);
            }
            finally
            {
                world.Dispose();
            }
        }

        /// <summary>
        /// 在 Game/Engine 两个 root 下定位资产, 得到 RName; 找不到返回 null。
        /// 判据是资产的 .ameta 是否存在, 不能看主文件: Scene 这类资产的"文件"其实是个目录
        /// (FileExists 走 File.Exists, 对目录恒为 false), 而批量 cook 本来就是扫 *.ameta 得出资产的
        /// (见 CookAssetTypeAsync), 单资产与批量必须认同一份身份。
        /// </summary>
        public RName ResolveAssetRName(string assetPath)
        {
            assetPath = assetPath.Replace('\\', '/');
            // 兼容从资源管理器复制来的主文件写法 utest/x.scene/x.scene:
            // 末段去扩展名与父目录同名时取父目录, 否则不动 (避免误伤 a/b/c.texture 这种真文件资产)
            var canonical = assetPath;
            if (assetPath.IndexOf('/') > 0)
            {
                var parent = TtFileManager.GetParentPathName(assetPath);
                if (TtFileManager.GetPureNameImpl(assetPath) == TtFileManager.GetPureNameImpl(parent))
                    canonical = parent;
            }

            for (var t = TtFileManager.ERootDir.Game; t <= TtFileManager.ERootDir.Engine; t++)
            {
                var rnType = t == TtFileManager.ERootDir.Engine
                    ? RName.ERNameType.Engine
                    : RName.ERNameType.Game;
                foreach (var name in new[] { canonical, assetPath })
                {
                    var candidate = RName.GetRName(name, rnType);
                    if (candidate == null)
                        continue;
                    // Address = root + Name (Name 自带扩展名), ameta 与资产同名相邻:
                    // utest/x.scene -> <root>utest/x.scene.ameta
                    if (TtFileManager.FileExists(candidate.Address + IAssetMeta.MetaExt))
                        return candidate;
                }
            }
            return null;
        }

        /// <summary>
        /// 两段式执行: 引擎物化 (CreateAsset) → 引擎 cook (CookAsset)。
        /// 传入的实例由 CookAsset 实现方负责释放 (见 IAssetMeta.CookAsset 契约)。
        /// world 由调用方持有并负责 Dispose。
        /// </summary>
        async Thread.Async.TtTask<FCookJobResult> CookResolvedAsset(RName rn, GamePlay.TtWorld world, TtCookContext ctx)
        {
            try
            {
                if (ctx.IsCancellationRequested)
                    return FCookJobResult.Cancelled();

                var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                if (ameta == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"Cook: GetAssetMeta {rn} failed");
                    return FCookJobResult.Failed($"GetAssetMeta {rn} failed");
                }
                ctx.ReportStage("resolve meta");

                var asset = await ameta.CreateAsset(world);
                if (asset == null)
                {
                    Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"Cook: CreateAsset {rn} failed");
                    return FCookJobResult.Failed($"CreateAsset {rn} failed");
                }
                if (ctx.IsCancellationRequested)
                {
                    // 已物化但未 cook: 没人接手释放, 由驱动方释放, 不能把半成品留给 CookAsset
                    ReleaseUncookedAsset(asset);
                    return FCookJobResult.Cancelled();
                }
                ctx.ReportStage("create asset");

                bool ok;
                using (TtCookContext.PushAmbient(ctx))
                {
                    ok = await ameta.CookAsset(asset, ctx);
                }
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(ok ? Profiler.ELogTag.Info : Profiler.ELogTag.Error,
                    $"Cook single asset {(ok ? "done" : "failed")}: {rn}");
                if (ctx.IsCancellationRequested && ok == false)
                    return FCookJobResult.Cancelled();
                return ok ? FCookJobResult.Success() : FCookJobResult.Failed($"CookAsset {rn} returned false");
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"Cook: {rn} failed: {ex}");
                return FCookJobResult.Failed(ex.Message);
            }
        }

        /// <summary>
        /// 取消发生在 CookAsset 之前时的释放入口。正常路径下实例由 CookAsset 实现方释放
        /// (见 IAssetMeta.CookAsset 契约), 只有那种情况下没人接手, 才由驱动方自己收尾。
        /// </summary>
        static void ReleaseUncookedAsset(IAsset asset)
        {
            if (asset is GamePlay.Scene.TtNode node)
                node.DisposeWithChildren();
            else if (asset is IDisposable disposable)
                disposable.Dispose();
        }
        #endregion

        #region 批量
        /// <summary>
        /// 按类型批量 cook (原 ExeCmd=Cook 的 AssetType 分支), 返回 0 全部成功 / 1 有失败类型。
        /// 未知类型名会报错并计入失败, 与既有行为一致。
        /// </summary>
        public async Thread.Async.TtTask<int> CookAssetTypesAsync(string[] typeNames)
        {
            int failedTypes = 0;
            foreach (var i in typeNames)
            {
                if (TryGetAssetType(i, out var metaType) == false)
                {
                    Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"Cook: unknown asset type '{i}'");
                    failedTypes++;
                    continue;
                }
                if (await CookAssetTypeAsync(i, metaType) == false)
                    failedTypes++;
            }
            return failedTypes == 0 ? 0 : 1;
        }

        /// <summary>
        /// 单类型通用 cook: 扫 Game/Engine 两个 root 下该类型扩展名的全部资产,
        /// 经 AssetMetaManager 取 meta 后统一 "CreateAsset → CookAsset"。
        /// 每个 root 复用同一个 TtWorld 作为上下文 (Scene 等物化时需要),
        /// 未知 world 的类型 CreateAsset 会忽略多余参数。
        /// </summary>
        public async Thread.Async.TtTask<bool> CookAssetTypeAsync(string typeName, Type metaType)
        {
            var proto = Rtti.TtTypeDescManager.CreateInstance(metaType) as IAssetMeta;
            if (proto == null)
                return false;
            var ext = proto.TypeExt;
            bool allOk = true;
            for (var t = TtFileManager.ERootDir.Game; t <= TtFileManager.ERootDir.Engine; t++)
            {
                var rnType = t == TtFileManager.ERootDir.Engine
                    ? RName.ERNameType.Engine
                    : RName.ERNameType.Game;

                var root = TtEngine.Instance.FileManager.GetRoot(t);
                var files = TtFileManager.GetFiles(root, "*" + ext + IAssetMeta.MetaExt, true);
                if (files.Length == 0)
                    continue;

                var world = new GamePlay.TtWorld(null);
                await world.InitWorld();
                int procNum = 0;
                int failedNum = 0;
                foreach (var i in files)
                {
                    var rp = TtFileManager.GetRelativePath(root, i);
                    rp = rp.Substring(0, rp.Length - IAssetMeta.MetaExt.Length);
                    if (IsBlacklisted(rp))
                    {
                        Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info, $"Skipped(blacklist): {rp}");
                        procNum++;
                        continue;
                    }
                    var rn = RName.GetRName(rp, rnType);
                    var ctx = new TtCookContext(0, rp, typeName, false, TtCookLogSink.Instance);
                    try
                    {
                        var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(rn);
                        if (ameta == null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"{typeName}: GetAssetMeta {rn} failed");
                            failedNum++;
                        }
                        else
                        {
                            // 两段式: 引擎物化 (CreateAsset) → 引擎 cook (CookAsset)
                            var asset = await ameta.CreateAsset(world);
                            if (asset == null)
                            {
                                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"{typeName}: CreateAsset {rn} failed");
                                failedNum++;
                            }
                            else
                            {
                                using (TtCookContext.PushAmbient(ctx))
                                {
                                    if (await ameta.CookAsset(asset, ctx) == false)
                                        failedNum++;
                                }
                            }
                            // 注意: 传入的实例由 CookAsset 实现方负责释放 (见 IAssetMeta.CookAsset 契约)
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error, $"{typeName}: cook {rn} failed: {ex}");
                        failedNum++;
                    }
                    procNum++;
                    Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info, $"Cook {typeName}: {procNum}/{files.Length}");
                }
                world.Dispose();
                if (failedNum > 0)
                    allOk = false;
            }
            return allOk;
        }
        #endregion
    }
}
