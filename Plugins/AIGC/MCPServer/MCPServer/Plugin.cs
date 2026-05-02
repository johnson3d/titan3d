using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class TtMCPServerAssemblyDesc : TtAssemblyDesc
        {
            public TtMCPServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info, "Plugins:MCPServer AssemblyDesc Created");
            }
            ~TtMCPServerAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info, "Plugins:MCPServer AssemblyDesc Destroyed");
            }
            public override string Name { get => "MCPServer"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static TtMCPServerAssemblyDesc AssmblyDesc = new TtMCPServerAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.MCPServer
{
    [EngineNS.Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static TtMCPServerPlugin mPluginObject = new TtMCPServerPlugin();
        public static EngineNS.Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }

    /// <summary>
    /// Ring buffer that captures engine log entries via Profiler.Log.OnReportLog.
    /// Only collects logs with "MCP" category.
    /// </summary>
    internal static class TtLogCollector
    {
        private static readonly object LogLock = new object();
        private static readonly string[] LogBuffer = new string[500];
        private static int LogWriteIndex = 0;
        private static int LogCount = 0;

        public static void OnReportLog(Profiler.ELogTag tag, string category, string memberName, string sourceFilePath, int sourceLineNumber, string info)
        {
            if (category != "MCP")
                return;
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var entry = $"[{timestamp}][{tag}] {info}";
            lock (LogLock)
            {
                LogBuffer[LogWriteIndex] = entry;
                LogWriteIndex = (LogWriteIndex + 1) % LogBuffer.Length;
                if (LogCount < LogBuffer.Length)
                    LogCount++;
            }
        }

        public static List<string> GetRecentLogs(int count, string tagFilter = "")
        {
            var result = new List<string>();
            lock (LogLock)
            {
                int start = LogCount < LogBuffer.Length ? 0 : LogWriteIndex;
                int total = LogCount;
                for (int i = 0; i < total; i++)
                {
                    var idx = (start + i) % LogBuffer.Length;
                    var entry = LogBuffer[idx];
                    if (entry == null) continue;
                    if (!string.IsNullOrEmpty(tagFilter) && !entry.Contains(tagFilter))
                        continue;
                    result.Add(entry);
                }
            }
            if (result.Count > count)
                result = result.GetRange(result.Count - count, count);
            return result;
        }
    }

    /// <summary>
    /// MCP Server plugin that exposes engine capabilities via a simple HTTP REST API.
    /// A separate Python stdio MCP Server connects to this API and bridges to Copilot.
    ///
    /// Architecture:
    ///   Aone Copilot --stdio--> Python MCP Server --HTTP--> This Plugin (REST API)
    ///
    /// REST API endpoints:
    ///   GET  /           — Server info (status, tool count)
    ///   GET  /tools      — List all registered tools
    ///   POST /call       — Invoke a tool: {"name":"tool_name","arguments":{...}}
    /// </summary>
    public partial class TtMCPServerPlugin : EngineNS.Bricks.AIGC.TtMCPPlugin
    {
        private HttpListener mHttpListener;
        private System.Threading.Thread mListenerThread;

        public TtMCPServerPlugin()
        {
            ServerName = "TitanEngineMCPServer";
            ServerVersion = "1.0.0";
            ServerPort = 8818;
        }

        public override void OnLoadedPlugin()
        {
            base.OnLoadedPlugin();
            Profiler.Log.OnReportLog += TtLogCollector.OnReportLog;
            Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info,
                $"MCPServer plugin loaded, {ListTools().Count} tools registered");
        }

        public override void OnUnloadPlugin()
        {
            Profiler.Log.OnReportLog -= TtLogCollector.OnReportLog;
            base.OnUnloadPlugin();
            Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info,
                "MCPServer plugin unloaded");
        }

        #region HTTP Server Lifecycle

        public override bool StartServer(int port = 0)
        {
            if (IsRunning)
                return true;

            base.StartServer(port);

            try
            {
                mHttpListener = new HttpListener();
                mHttpListener.Prefixes.Add($"http://localhost:{ServerPort}/");
                mHttpListener.Start();

                mListenerThread = new System.Threading.Thread(ListenLoop);
                mListenerThread.IsBackground = true;
                mListenerThread.Name = "MCPServerListener";
                mListenerThread.Start();

                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info,
                    $"MCP REST API listening on http://localhost:{ServerPort}/");
                return true;
            }
            catch (Exception ex)
            {
                IsRunning = false;
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Error,
                    $"Failed to start MCP Server: {ex.Message}");
                return false;
            }
        }

        public override void StopServer()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            try
            {
                mHttpListener?.Stop();
                mHttpListener?.Close();
                mListenerThread?.Join(2000);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Warning,
                    $"Error stopping MCP Server: {ex.Message}");
            }
            finally
            {
                mHttpListener = null;
                mListenerThread = null;
            }

            base.StopServer();
        }

        private void ListenLoop()
        {
            while (IsRunning && mHttpListener != null && mHttpListener.IsListening)
            {
                try
                {
                    var context = mHttpListener.GetContext();
                    System.Threading.ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Warning,
                        $"Listener error: {ex.Message}");
                }
            }
        }

        #endregion

        #region HTTP Request Handling

        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var path = request.Url.AbsolutePath.TrimEnd('/');

            try
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    context.Response.Close();
                    return;
                }

                switch (path)
                {
                    case "" when request.HttpMethod == "GET":
                        SendJson(context, 200, BuildServerInfo());
                        break;

                    case "/tools" when request.HttpMethod == "GET":
                        SendJson(context, 200, BuildToolsList());
                        break;

                    case "/call" when request.HttpMethod == "POST":
                        HandleToolCall(context);
                        break;

                    default:
                        SendJson(context, 404, "{\"error\":\"Not found\"}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Warning,
                    $"Request error: {ex.Message}");
                try { SendJson(context, 500, $"{{\"error\":{JsonSerializer.Serialize(ex.Message)}}}"); }
                catch { try { context.Response.Close(); } catch { } }
            }
        }

        private void HandleToolCall(HttpListenerContext context)
        {
            string body;
            using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }

            Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info,
                $"Tool call: {body}");

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var toolName = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : "";
            Dictionary<string, object> arguments = null;

            if (root.TryGetProperty("arguments", out var argsElement))
            {
                arguments = new Dictionary<string, object>();
                foreach (var prop in argsElement.EnumerateObject())
                {
                    arguments[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.GetRawText()
                    };
                }
            }

            var result = InvokeTool(toolName, arguments);

            if (result.Success)
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Info,
                    $"Tool result: {result.Content}");
                SendJson(context, 200, $"{{\"success\":true,\"result\":{JsonSerializer.Serialize(result.Content)}}}");
            }
            else
            {
                Profiler.Log.WriteLine<Profiler.TtMCPGategory>(Profiler.ELogTag.Warning,
                    $"Tool error: {result.ErrorMessage}");
                SendJson(context, 400, $"{{\"success\":false,\"error\":{JsonSerializer.Serialize(result.ErrorMessage)}}}");
            }
        }

        #endregion

        #region Response Helpers

        private static void SendJson(HttpListenerContext context, int statusCode, string body)
        {
            var response = context.Response;
            try
            {
                response.StatusCode = statusCode;
                var buffer = Encoding.UTF8.GetBytes(body);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (HttpListenerException)
            {
                // Client disconnected before response was sent (e.g. timeout), safe to ignore
            }
            finally
            {
                try { response.Close(); } catch { }
            }
        }

        private string BuildServerInfo()
        {
            return JsonSerializer.Serialize(new
            {
                name = ServerName,
                version = ServerVersion,
                status = IsRunning ? "running" : "stopped",
                tools_count = ListTools().Count
            });
        }

        private string BuildToolsList()
        {
            var tools = ListTools();
            var toolsList = new List<Dictionary<string, object>>();
            foreach (var tool in tools)
            {
                var properties = new Dictionary<string, object>();
                var requiredList = new List<string>();
                foreach (var param in tool.Parameters)
                {
                    var paramEntry = new Dictionary<string, string>
                    {
                        { "type", MapCSharpTypeToJsonType(param.Type) }
                    };
                    if (!string.IsNullOrEmpty(param.Description))
                        paramEntry["description"] = param.Description;
                    properties[param.Name] = paramEntry;
                    if (param.Required)
                        requiredList.Add(param.Name);
                }

                var toolEntry = new Dictionary<string, object>
                {
                    { "name", tool.Name },
                    { "description", tool.Description },
                    { "parameters", new Dictionary<string, object>
                        {
                            { "type", "object" },
                            { "properties", properties },
                            { "required", requiredList }
                        }
                    }
                };
                if (!string.IsNullOrEmpty(tool.ReturnDescription))
                    toolEntry["returnDescription"] = tool.ReturnDescription;
                toolsList.Add(toolEntry);
            }

            return JsonSerializer.Serialize(toolsList);
        }

        private static string MapCSharpTypeToJsonType(string csharpType)
        {
            return csharpType.ToLower() switch
            {
                "string" => "string",
                "int32" or "int64" or "single" or "double" or "decimal" => "number",
                "boolean" => "boolean",
                _ => "string"
            };
        }

        #endregion

    }
}
