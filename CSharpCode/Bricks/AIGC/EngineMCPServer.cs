using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.Bricks.AIGC
{
    /// <summary>
    /// 引擎内置 MCP 工具服务: 监听 loopback TCP, 以"每行一个 JSON 请求"的轻协议
    /// 暴露注册的工具(日志读取/场景打开/goto 等), 供外部 AI 代理或调试脚本调用。
    /// 本类不依赖 Editor 模块; 编辑器相关工具在 Editor 模块的 TtEditorMCPTools 中
    /// 通过 RegisterTool 注册进来。
    /// 协议: 请求 {"tool":"name","args":{...}}  响应 {"success":true,"content":"..."} 或 {"success":false,"error":"..."}
    /// </summary>
    public static class TtEngineMCPServer
    {
        public class FLogEntry
        {
            public string Time;
            public string Tag;
            public string Category;
            public string MemberName;
            public string Text;
        }

        const int MaxKeptLogs = 4096;
        static readonly object mSync = new object();
        static readonly Queue<FLogEntry> mLogs = new Queue<FLogEntry>();
        static readonly Dictionary<string, Func<JsonElement, string>> mTools = new Dictionary<string, Func<JsonElement, string>>();
        static System.Net.Sockets.TcpListener mListener;
        static System.Threading.Thread mThread;
        static volatile bool mRunning = false;

        public static bool IsRunning => mRunning;
        public static int Port { get; private set; } = 8818;

        /// <summary>
        /// 注册或替换一个工具。handler 收到 args(JsonElement, 可能为 default), 返回文本结果。
        /// </summary>
        public static void RegisterTool(string toolName, Func<JsonElement, string> handler)
        {
            lock (mTools)
            {
                mTools[toolName] = handler;
            }
        }

        public static void Startup(int port = 8818)
        {
            if (mRunning)
                return;
            SureDefaultTools();
            Port = port;
            Profiler.Log.OnReportLog += OnReportLog;
            mRunning = true;
            mThread = new System.Threading.Thread(ServerLoop)
            {
                IsBackground = true,
                Name = "EngineMCPServer",
            };
            mThread.Start();
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                $"EngineMCPServer listening on 127.0.0.1:{Port}");
        }

        static void OnReportLog(Profiler.ELogTag tag, string category, string memberName, string sourceFilePath, int sourceLineNumber, string info)
        {
            lock (mSync)
            {
                mLogs.Enqueue(new FLogEntry
                {
                    Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                    Tag = tag.ToString(),
                    Category = category,
                    MemberName = memberName,
                    Text = info,
                });
                while (mLogs.Count > MaxKeptLogs)
                    mLogs.Dequeue();
            }
        }

        static void ServerLoop()
        {
            try
            {
                mListener = new TcpListener(IPAddress.Loopback, Port);
                mListener.Start();
                while (mRunning)
                {
                    var client = mListener.AcceptTcpClient();
                    var _ = Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                if (mRunning)
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                        "EngineMCPServer", $"server loop exit: {ex.Message}");
            }
        }

        static void HandleClient(System.Net.Sockets.TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                using (var writer = new System.IO.StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true })
                {
                    while (mRunning)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        var response = HandleRequest(line);
                        writer.WriteLine(response);
                    }
                }
            }
            catch
            {
                // 连接级错误(客户端断开等)直接结束本次连接
            }
        }

        static string HandleRequest(string line)
        {
            string tool = null;
            var args = default(JsonElement);
            try
            {
                using (var doc = JsonDocument.Parse(line))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("tool", out var toolEl))
                        tool = toolEl.GetString();
                    if (root.TryGetProperty("args", out var argsEl) && argsEl.ValueKind == JsonValueKind.Object)
                        args = argsEl.Clone();
                }
            }
            catch (Exception ex)
            {
                return SerializeResult(false, null, $"bad request json: {ex.Message}");
            }

            if (tool == null)
                return SerializeResult(false, null, "missing 'tool'");
            lock (mTools)
            {
                if (mTools.TryGetValue(tool, out var handler) == false)
                    return SerializeResult(false, null, $"tool '{tool}' not found");
                try
                {
                    var content = handler(args);
                    return SerializeResult(true, content, null);
                }
                catch (Exception ex)
                {
                    return SerializeResult(false, null, $"tool '{tool}' failed: {ex.Message}");
                }
            }
        }

        static string SerializeResult(bool success, string content, string error)
        {
            var dict = new Dictionary<string, object>
            {
                ["success"] = success,
            };
            if (content != null)
                dict["content"] = content;
            if (error != null)
                dict["error"] = error;
            return JsonSerializer.Serialize(dict);
        }

        // ---------- 内置工具 ----------

        static string ToolPing(JsonElement args)
        {
            return "pong";
        }

        /// <summary>
        /// 返回最近日志。args: count(默认100, 最大2000), filter(子串过滤, 可空)。
        /// </summary>
        static string ToolGetLogs(JsonElement args)
        {
            int count = 100;
            string filter = null;
            if (args.ValueKind == JsonValueKind.Object)
            {
                if (args.TryGetProperty("count", out var c) && c.TryGetInt32(out var ci))
                    count = ci;
                if (args.TryGetProperty("filter", out var f) && f.ValueKind == JsonValueKind.String)
                    filter = f.GetString();
            }
            count = Math.Clamp(count, 1, 2000);

            var lines = new List<string>();
            lock (mSync)
            {
                foreach (var entry in mLogs)
                {
                    if (filter != null &&
                        (entry.Text == null || entry.Text.Contains(filter, StringComparison.OrdinalIgnoreCase) == false) &&
                        (entry.Category == null || entry.Category.Contains(filter, StringComparison.OrdinalIgnoreCase) == false))
                        continue;
                    lines.Add($"[{entry.Time}][{entry.Tag}][{entry.Category}] {entry.Text}" +
                        (string.IsNullOrEmpty(entry.MemberName) || entry.MemberName == entry.Text ? "" : $" :: {entry.MemberName}"));
                }
            }
            if (lines.Count > count)
                lines.RemoveRange(0, lines.Count - count);
            return string.Join("\n", lines);
        }

        // ---------- 启动时的默认工具 ----------

        static bool mDefaultToolsRegistered = false;
        static void SureDefaultTools()
        {
            if (mDefaultToolsRegistered)
                return;
            mDefaultToolsRegistered = true;
            RegisterTool("ping", ToolPing);
            RegisterTool("get_logs", ToolGetLogs);
            RegisterTool("list_tools", (args) =>
            {
                lock (mTools)
                {
                    return string.Join(",", mTools.Keys);
                }
            });
        }
    }
}
