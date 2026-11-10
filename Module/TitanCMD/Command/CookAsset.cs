using System;

namespace ProjectCooker.Command
{
    /// <summary>
    /// Cook 命令薄壳: 只做命令行参数解析, 实际的类型枚举/目录扫描/黑名单过滤/CreateAsset → CookAsset
    /// 全部在引擎内的 <see cref="EngineNS.IO.TtCookDriver"/> 里 (与 ExeCmd=CookServer 同源)。
    /// 每个类型 cook 什么 (见 IAssetMeta.CookAsset 重写) 全部实现于引擎内, 新增类型只需在对应
    /// TtXxxAMeta 重写 CookAsset, 这里无需改动。
    /// 用法: TitanCMD.exe CookCfg=... ExeCmd=Cook [AssetType=Scene+Texture ...] [Blacklist=...] [Asset=...]
    /// AssetType 缺省时按 DynConfig 的 CookAssetType 列表逐个 cook (由 Program.Run 回填)。
    /// </summary>
    class TtCookAsset : TtCookCommand
    {
        public override async EngineNS.Thread.Async.TtTask<int> ExecuteCommand(string[] args)
        {
            var driver = EngineNS.IO.TtCookDriver.Instance;
            driver.LoadBlacklist(FindArgument(args, Param_Blacklist));

            // 单资源模式: Asset=相对路径 (可带扩展名), 优先于 AssetType 批量扫描
            var singleAsset = FindArgument(args, Param_Asset);
            if (string.IsNullOrEmpty(singleAsset) == false)
                return await driver.CookSingleAssetAsync(singleAsset);

            var assetTypes = GetArguments(args, Param_Types);
            if (assetTypes == null)
                throw new Exception("Cook command requires AssetType= (e.g. AssetType=Scene)");
            return await driver.CookAssetTypesAsync(assetTypes);
        }
    }
}
