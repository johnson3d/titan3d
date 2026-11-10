using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EngineNS.IO
{
    /// <summary>
    /// cook 进程间通讯错误类别。与协议字符串一一对应，见 <see cref="TtCookMessage.Kind"/>。
    /// </summary>
    public enum ECookErrorKind
    {
        Asset,
        Cancel,
        Crash,
        Timeout,
        Conflict,
    }

    /// <summary>
    /// CookManager 的行式 JSON IPC 协议消息 (纯数据, 不依赖引擎初始化)。
    /// 线上传输形态: 一行 "[TITAN-COOK]{json}", 见 <see cref="TtCookProtocol"/>。
    /// 字段全部可空且为 null 时不写出, 因此一条消息只带它真正需要的载荷。
    /// </summary>
    public class TtCookMessage
    {
        /// <summary>协议版本, 见 <see cref="TtCookProtocol.Version"/></summary>
        [JsonPropertyName("v")]
        public int Version { get; set; } = TtCookProtocol.Version;
        /// <summary>消息类型, 取值见 <see cref="TtCookMsgTypes"/></summary>
        [JsonPropertyName("t")]
        public string Type { get; set; }
        /// <summary>job id, 由提交侧 (编辑器) 分配, 单调递增</summary>
        [JsonPropertyName("id")]
        public long? JobId { get; set; }
        /// <summary>资产路径 (相对各自 root, 不带 .meta), 即 RName 字符串</summary>
        [JsonPropertyName("asset")]
        public string Asset { get; set; }
        /// <summary>资产类型名 (Scene/Texture/...), 来自 IAssetMeta.GetAssetTypeName</summary>
        [JsonPropertyName("type")]
        public string AssetType { get; set; }
        /// <summary>强制重烘, 绕过跳过判定 (仅 add 有效)</summary>
        [JsonPropertyName("force")]
        public bool? Force { get; set; }
        /// <summary>阶段文本 (job.stage)</summary>
        [JsonPropertyName("stage")]
        public string Stage { get; set; }
        /// <summary>子进度 0..1 (job.progress), 由 CookAsset 实现上报</summary>
        [JsonPropertyName("fraction")]
        public float? Fraction { get; set; }
        /// <summary>秒 (elapsed 类字段统一用秒, 保留一位小数)</summary>
        [JsonPropertyName("elapsed")]
        public double? ElapsedSec { get; set; }
        /// <summary>worker 侧待跑 job 数 (ready/idle/busy)</summary>
        [JsonPropertyName("queued")]
        public int? Queued { get; set; }
        /// <summary>心跳序号, 编辑器据此判断 worker 是否只在空转还是在推进</summary>
        [JsonPropertyName("seq")]
        public long? Seq { get; set; }
        /// <summary>产物路径列表, 一律为相对仓库根的路径 (job.done)</summary>
        [JsonPropertyName("artifacts")]
        public List<string> Artifacts { get; set; }
        /// <summary>错误类别 (job.error), 取值见 <see cref="ECookErrorKind"/></summary>
        [JsonPropertyName("kind")]
        public string Kind { get; set; }
        /// <summary>错误/退出原因文本</summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
        /// <summary>异常堆栈 (job.error)</summary>
        [JsonPropertyName("stack")]
        public string Stack { get; set; }
        /// <summary>优雅退出原因 (quit): idle / restart / manual</summary>
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
        /// <summary>stats: 本批成功数</summary>
        [JsonPropertyName("done")]
        public int? Done { get; set; }
        /// <summary>stats: 本批失败数</summary>
        [JsonPropertyName("failed")]
        public int? Failed { get; set; }
        /// <summary>stats: 本批跳过数 (黑名单/未找到/已在队列)</summary>
        [JsonPropertyName("skipped")]
        public int? Skipped { get; set; }
        /// <summary>hello: 对端应使用的协议版本</summary>
        [JsonPropertyName("proto")]
        public int? Proto { get; set; }
        /// <summary>hello: 引擎程序集版本</summary>
        [JsonPropertyName("engineVer")]
        public string EngineVer { get; set; }
        /// <summary>hello: 引擎配置内容 hash, 不一致的机器不得共写 cache</summary>
        [JsonPropertyName("cfgHash")]
        public string CfgHash { get; set; }
        /// <summary>hello: worker 的 Game root 绝对路径 (供编辑器映射相对产物路径)</summary>
        [JsonPropertyName("root")]
        public string Root { get; set; }
        /// <summary>hello: worker 支持的资产类型名列表</summary>
        [JsonPropertyName("types")]
        public List<string> Types { get; set; }
        /// <summary>hello: worker 进程 id</summary>
        [JsonPropertyName("pid")]
        public int? Pid { get; set; }
        /// <summary>log: 日志文本 (结构化转发 worker 日志, 可选)</summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
        /// <summary>log: 日志类别名 (如 Cook/Core)</summary>
        [JsonPropertyName("category")]
        public string Category { get; set; }
        /// <summary>log: 日志 tag (Info/Warning/Error/...)</summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        /// <summary>协议版本是否与本引擎实现一致 (本地判定, 不上线: 线上只带 v 一个版本字段)</summary>
        [JsonIgnore]
        public bool IsVersionSupported => Version == TtCookProtocol.Version;

        /// <summary>按 <see cref="ECookErrorKind"/> 解释 Kind; 无法识别时返回 null</summary>
        public ECookErrorKind? GetErrorKind()
        {
            if (string.IsNullOrEmpty(Kind))
                return null;
            if (Enum.TryParse<ECookErrorKind>(Kind, true, out var kind))
                return kind;
            return null;
        }
    }

    /// <summary>
    /// 协议消息类型名常量。字符串而非枚举出现在线上, 是为了让旧 worker 收到未知类型时
    /// 只忽略而不报错 (向前兼容), 也让手工构造的 stdin 命令行可读。
    /// </summary>
    public static class TtCookMsgTypes
    {
        #region 编辑器 / 提交侧 -> worker
        /// <summary>入队一个单资产 job: id, asset, type?, force?</summary>
        public const string Add = "add";
        /// <summary>取消: id (在飞则置协作式取消位, 在队列则出队)</summary>
        public const string Cancel = "cancel";
        /// <summary>批次边界: worker 排空队列后回 stats</summary>
        public const string Flush = "flush";
        /// <summary>优雅退出: reason</summary>
        public const string Quit = "quit";
        #endregion

        #region worker -> 编辑器
        /// <summary>握手: proto, engineVer, cfgHash, root, types, pid</summary>
        public const string Hello = "hello";
        /// <summary>可接活: queued</summary>
        public const string Ready = "ready";
        /// <summary>队列已空: queued, curJob 已无</summary>
        public const string Idle = "idle";
        /// <summary>心跳: id?, seq, elapsed</summary>
        public const string Beat = "beat";
        /// <summary>job 开始: id, asset, type</summary>
        public const string JobBegin = "job.begin";
        /// <summary>阶段变化: id, stage, elapsed</summary>
        public const string JobStage = "job.stage";
        /// <summary>子进度: id, fraction</summary>
        public const string JobProgress = "job.progress";
        /// <summary>job 成功: id, elapsed, artifacts</summary>
        public const string JobDone = "job.done";
        /// <summary>job 失败: id, kind, message, stack, elapsed</summary>
        public const string JobError = "job.error";
        /// <summary>worker 日志结构化转发: tag, category, text</summary>
        public const string Log = "log";
        /// <summary>flush 应答: done, failed, skipped, elapsed</summary>
        public const string Stats = "stats";
        /// <summary>队列已满, 拒绝本次 add: id, queued</summary>
        public const string Busy = "busy";
        #endregion
    }

    /// <summary>
    /// cook IPC 行协议编解码。纯静态、无引擎依赖, 可脱离 TtEngine 单测。
    /// 哨兵前缀风格对齐仓库既有的 [TITAN-STAGE] (Editor\AndroidBuild\TtAndroidBuildLogParser.cs):
    /// 不带哨兵的行不是错误, 由调用方当日志转发, 因此 worker 的人读输出零丢失。
    /// </summary>
    public static class TtCookProtocol
    {
        /// <summary>行首哨兵, 后面紧跟一行 JSON</summary>
        public const string Sentinel = "[TITAN-COOK]";
        /// <summary>
        /// 协议版本。改动消息语义 (字段含义/取值范围/必选性) 时必须 +1;
        /// 只加新的可选字段不必 +1 (旧端忽略未知字段是允许的)。
        /// </summary>
        public const int Version = 1;

        private static readonly JsonSerializerOptions sOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// 编码为一行 (不含换行符)。调用方自行 WriteLine。
        /// </summary>
        public static string Encode(TtCookMessage msg)
        {
            if (msg == null)
                return null;
            if (msg.Version == 0)
                msg.Version = Version;
            var json = JsonSerializer.Serialize(msg, sOptions);
            return Sentinel + json;
        }

        /// <summary>
        /// 解码一行。返回 false 的两种情况都不算异常: 非协议行 (人读日志) 与畸形 JSON,
        /// 调用方统一按日志处理即可 (out msg 为 null)。
        /// </summary>
        public static bool TryDecode(string line, out TtCookMessage msg)
        {
            msg = null;
            if (string.IsNullOrEmpty(line))
                return false;
            var text = line.Trim();
            if (text.StartsWith(Sentinel) == false)
                return false;
            var json = text.Substring(Sentinel.Length);
            if (string.IsNullOrEmpty(json))
                return false;
            try
            {
                msg = JsonSerializer.Deserialize<TtCookMessage>(json, sOptions);
            }
            catch (JsonException)
            {
                msg = null;
                return false;
            }
            if (msg == null || string.IsNullOrEmpty(msg.Type))
            {
                msg = null;
                return false;
            }
            return true;
        }

        /// <summary>该行是否带协议哨兵 (用于把协议行从日志流里分出来)</summary>
        public static bool IsProtocolLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return false;
            return line.TrimStart().StartsWith(Sentinel);
        }

        #region 消息工厂 (worker -> 编辑器 侧全部走这里, 避免各处手拼字段)
        public static TtCookMessage Hello(int proto, string engineVer, string cfgHash, string root, List<string> types, int pid)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.Hello,
                Proto = proto,
                EngineVer = engineVer,
                CfgHash = cfgHash,
                Root = root,
                Types = types,
                Pid = pid,
            };
        }

        public static TtCookMessage Ready(int queued)
        {
            return new TtCookMessage { Type = TtCookMsgTypes.Ready, Queued = queued };
        }

        public static TtCookMessage Idle(int queued)
        {
            return new TtCookMessage { Type = TtCookMsgTypes.Idle, Queued = queued };
        }

        public static TtCookMessage Busy(long id, int queued)
        {
            return new TtCookMessage { Type = TtCookMsgTypes.Busy, JobId = id, Queued = queued };
        }

        public static TtCookMessage Beat(long? id, long seq, double elapsedSec)
        {
            return new TtCookMessage { Type = TtCookMsgTypes.Beat, JobId = id, Seq = seq, ElapsedSec = elapsedSec };
        }

        public static TtCookMessage JobBegin(long id, string asset, string assetType)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.JobBegin,
                JobId = id,
                Asset = asset,
                AssetType = assetType,
            };
        }

        public static TtCookMessage JobStage(long id, string stage, double elapsedSec)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.JobStage,
                JobId = id,
                Stage = stage,
                ElapsedSec = elapsedSec,
            };
        }

        public static TtCookMessage JobProgress(long id, float fraction)
        {
            return new TtCookMessage { Type = TtCookMsgTypes.JobProgress, JobId = id, Fraction = fraction };
        }

        public static TtCookMessage JobDone(long id, double elapsedSec, List<string> artifacts)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.JobDone,
                JobId = id,
                ElapsedSec = elapsedSec,
                Artifacts = artifacts,
            };
        }

        public static TtCookMessage JobError(long id, ECookErrorKind kind, string message, string stack, double elapsedSec)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.JobError,
                JobId = id,
                Kind = kind.ToString(),
                Message = message,
                Stack = stack,
                ElapsedSec = elapsedSec,
            };
        }

        public static TtCookMessage Stats(int done, int failed, int skipped, double elapsedSec)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.Stats,
                Done = done,
                Failed = failed,
                Skipped = skipped,
                ElapsedSec = elapsedSec,
            };
        }

        public static TtCookMessage LogMessage(string tag, string category, string text)
        {
            return new TtCookMessage
            {
                Type = TtCookMsgTypes.Log,
                Tag = tag,
                Category = category,
                Text = text,
            };
        }

        public static TtCookMessage Quit(string reason)
        {
            return new TtCookMessage { Type = TtCookMsgTypes.Quit, Reason = reason };
        }
        #endregion
    }
}
