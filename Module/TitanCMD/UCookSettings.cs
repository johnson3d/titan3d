using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCooker
{
    public static class TtCommandExecutionPolicy
    {
        public static bool IsServiceCommand(string command)
        {
            switch (command)
            {
                case "StartDS":
                case "LoadPlugin":
                case "StartRobot":
                case "StartRootServer":
                case "StartLoginServer":
                case "StartLevelServer":
                case "StartGateServer":
                // cook worker: 常驻等 stdin 指令, 不能 one-shot 跑完就 PostQuitMessage
                case "CookServer":
                    return true;
                default:
                    return false;
            }
        }

        public static string ResolveNativeConfiguration(string requested, string cached)
        {
            var value = string.IsNullOrEmpty(requested) ? cached : requested;
            return string.Equals(value, "debug", StringComparison.OrdinalIgnoreCase) ? "debug" : "release";
        }

        public static bool IsEngineConfigurationAvailable(string configurationPath)
        {
            return string.IsNullOrEmpty(configurationPath) || System.IO.File.Exists(configurationPath);
        }
    }

    class TtCookCommand
    {
        public const string Param_Types = "AssetType=";
        //public const string Type_Texture = "Texture";
        //public const string Type_UVAnim = "UVAnim";
        //public const string Type_Mesh = "Mesh";
        //public const string Type_MeshPrimitive = "MeshPrimitive";
        //public const string Type_Material = "Material";
        //public const string Type_MaterialInst = "MaterialInst";
        //public const string Type_Scene = "Scene";
        //public const string Type_Prefab = "Prefab";
        //public const string Type_AnimClip = "AnimClip";
        //public const string Type_UI = "UI";
        //public const string Type_Macross = "Macross";

        public const string Param_Blacklist = "Blacklist=";
        /// <summary>单资源 cook: 指定资产相对路径 (可带扩展名), 优先于 AssetType 批量扫描</summary>
        public const string Param_Asset = "Asset=";
        public const string Param_CookCfg = "CookCfg=";
        public const string DS_Port = "DS_Port=";

        /// <summary>CookServer: 空闲多久后优雅退出 (秒), <=0 禁用</summary>
        public const string Param_WorkerIdleSec = "WorkerIdleSec=";
        /// <summary>CookServer: 单进程跑多少个 job 后在批次边界主动退出</summary>
        public const string Param_WorkerMaxJobs = "WorkerMaxJobs=";
        /// <summary>CookServer: 单 job 超时秒数, <=0 不限</summary>
        public const string Param_WorkerJobTimeout = "WorkerJobTimeout=";
        /// <summary>CookServer: 心跳间隔秒数</summary>
        public const string Param_WorkerBeatSec = "WorkerBeatSec=";

        public virtual async EngineNS.Thread.Async.TtTask<int> ExecuteCommand(string[] args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return 0;
        }
        public static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                {
                    return i.Substring(startWith.Length);
                }
            }
            return null;
        }
        public static string[] GetArguments(string[] args, string startWith, char split='+')
        {
            var types = FindArgument(args, startWith);
            if (types != null)
            {
                return types.Split(split);
            }
            return null;
        }
        /// <summary>取整数参数 (如 WorkerIdleSec=60), 缺失或非法时用 fallback</summary>
        public static int FindIntArgument(string[] args, string startWith, int fallback)
        {
            var value = FindArgument(args, startWith);
            if (string.IsNullOrEmpty(value))
                return fallback;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                return result;
            return fallback;
        }
    }
}
