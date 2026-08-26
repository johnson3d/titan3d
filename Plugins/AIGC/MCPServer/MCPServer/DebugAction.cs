using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 通用调试通路: 控制台命令执行、日志抓取、RenderDoc 抓帧。
    ///
    /// 这三样凑在一起才构成一个可自主迭代的闭环 —— 命令负责改状态, 日志负责看 CPU 侧结果,
    /// 抓帧负责看 GPU 侧结果 (.rdc 交给 renderdoc MCP 去解析)。
    ///
    /// 所有会改引擎状态的工具都必须走 TtMainThreadDispatcher, 原因见那个类的注释。
    /// </summary>
    public partial class TtMCPServerPlugin
    {
        [Bricks.AIGC.TtMCPTool("execute_console_command",
            "Executes an engine console command (same commands as the editor's LogWatcher command box) " +
            "on the engine main thread, then returns the log lines the command produced. " +
            "Use list_console_commands to discover available commands.",
            returnDescription: "{command: string - The command that was run, executed: boolean - " +
            "false if the command name was not found, output: string[] - Log lines emitted while the " +
            "command ran, outputCount: number, droppedCount: number - Log lines lost to ring buffer " +
            "overflow, error: string - Present only on failure}")]
        public static string ExecuteConsoleCommand(
            [Bricks.AIGC.TtMCPParameter("Full command line, e.g. 'TerrainEditTest Radius=200 Strength=300'")] string command,
            [Bricks.AIGC.TtMCPParameter("Only return log lines containing this substring; empty for all")] string outputFilter = "")
        {
            if (string.IsNullOrWhiteSpace(command))
                return FailJson("command is empty");

            command = command.Trim();
            var pos = command.IndexOf(' ');
            if (pos < 0)
                pos = command.Length;
            var cmdName = command.Substring(0, pos);
            var argsText = command.Length > pos ? command.Substring(pos + 1) : "";

            var cmd = TtCommandManager.Instance.TryGetCommand(cmdName);
            if (cmd == null)
            {
                return JsonSerializer.Serialize(new
                {
                    command,
                    executed = false,
                    error = $"command '{cmdName}' not found, use list_console_commands to see what exists",
                });
            }

            // 命令唯一的输出通道就是日志, 所以先记下游标, 执行完取增量 —— 这样一次调用就能
            // 拿到结果, 不必再去猜该从 get_recent_logs 里截哪一段。
            var seq = TtLogCollector.CurrentSequence;
            Exception cmdError = null;

            var completed = TtMainThreadDispatcher.Invoke(() =>
            {
                try
                {
                    cmd.Execute(argsText);
                }
                catch (Exception ex)
                {
                    // 命令自己抛异常不该让整个 MCP 调用变成"失败", 它的堆栈本身就是有用信息。
                    cmdError = ex;
                }
            });

            if (completed == false)
            {
                return JsonSerializer.Serialize(new
                {
                    command,
                    executed = false,
                    error = "timed out waiting for the engine main thread; is the engine frozen or " +
                            "stopped ticking (e.g. window minimized)?",
                });
            }

            var output = TtLogCollector.GetLogsSince(seq, outputFilter, out var dropped);
            return JsonSerializer.Serialize(new
            {
                command,
                executed = true,
                output,
                outputCount = output.Count,
                droppedCount = dropped,
                error = cmdError != null ? $"{cmdError.GetType().Name}: {cmdError.Message}" : null,
            });
        }

        [Bricks.AIGC.TtMCPTool("list_console_commands",
            "Lists registered engine console commands and their help text.",
            returnDescription: "{commands: [{name: string, help: string}], count: number}")]
        public static string ListConsoleCommands(
            [Bricks.AIGC.TtMCPParameter("Only list commands whose name contains this substring; empty for all")] string filter = "")
        {
            var list = new List<object>();
            foreach (var i in TtCommandManager.Instance.Commands)
            {
                if (!string.IsNullOrEmpty(filter) &&
                    i.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                list.Add(new { name = i.Key, help = i.Value.CmdHelp });
            }
            return JsonSerializer.Serialize(new { commands = list, count = list.Count });
        }

        [Bricks.AIGC.TtMCPTool("set_log_capture_filter",
            "Restricts which log categories the MCP log ring buffer collects. " +
            "Empty string means collect everything (the default). Useful to cut noise and overhead " +
            "when a high-frequency category floods the buffer. " +
            "Known categories: Debug, Exception, MCP, Thread, Graphics, IO.",
            returnDescription: "{activeFilter: string - Comma separated categories now collected, " +
            "or empty meaning all}")]
        public static string SetLogCaptureFilter(
            [Bricks.AIGC.TtMCPParameter("Comma separated category names, e.g. 'Debug,MCP'. Empty to collect all")] string categories = "")
        {
            var active = TtLogCollector.SetCategoryFilter(categories);
            return JsonSerializer.Serialize(new { activeFilter = active });
        }

        [Bricks.AIGC.TtMCPTool("capture_renderdoc_frame",
            "Triggers a RenderDoc frame capture and waits for the .rdc file to land on disk. " +
            "Returns the absolute path, which can be handed straight to the renderdoc MCP server's " +
            "open_capture. Requires the engine to have been launched with RenderDoc integration " +
            "available. Does NOT pop open the RenderDoc UI.",
            returnDescription: "{captured: boolean, file: string - Absolute .rdc path, fileName: string, " +
            "frameCount: number, error: string - Present only on failure}")]
        public static unsafe string CaptureRenderDocFrame(
            [Bricks.AIGC.TtMCPParameter("Tag appended to the capture filename, e.g. 'terrain_after_brush'")] string tag = "mcp",
            [Bricks.AIGC.TtMCPParameter("Number of consecutive frames to capture")] double frameCount = 1)
        {
            int frames = Math.Max(1, (int)frameCount);
            // tag 会直接进文件名, 挡掉路径分隔符之类会让 File.Move 失败的字符。
            tag = SanitizeCaptureTag(tag);

            string setupError = null;
            var started = TtMainThreadDispatcher.Invoke(() =>
            {
                var gfx = TtEngine.Instance.GfxDevice;
                var queue = gfx?.RenderQueue;
                if (queue == null)
                {
                    setupError = "GfxDevice.RenderQueue is null";
                    return;
                }
                if (queue.CaptureRenderDocFrame || queue.RemainingCaptureFrames > 0)
                {
                    setupError = "a capture is already in flight; retry in a moment";
                    return;
                }

                IRenderDocTool.GetInstance().SetGpuDevice(gfx.RenderContext.mCoreObject);
                var slate = gfx.SlateApplication;
                if (slate?.NativeWindow != null)
                    IRenderDocTool.GetInstance().SetActiveWindow(slate.NativeWindow.HWindow.ToPointer());

                queue.LastCaptureFile = null;
                queue.CaptureTagName = tag;
                // 自动化抓帧不要弹 RenderDoc UI, 否则每次调用都在用户桌面上开一个窗口。
                queue.OpenRenderDocAfterCapture = false;
                queue.RemainingCaptureFrames = frames - 1;
                queue.CaptureRenderDocFrame = true;
            });

            if (started == false)
                return FailJson("timed out waiting for the engine main thread to arm the capture");
            if (setupError != null)
                return FailJson(setupError);

            // 抓帧是跨帧的: BeginFrameCapture 在帧首, EndFrameCapture 在帧尾落盘。
            // 多留几帧余量, 首帧还可能撞上 IsFrameCapturing 而被跳过。
            var deadline = DateTime.Now.AddMilliseconds(2000 + frames * 2000);
            string file = null;
            while (DateTime.Now < deadline)
            {
                if (TtMainThreadDispatcher.WaitFrames(1, 5000) == false)
                    break;
                var queue = TtEngine.Instance.GfxDevice?.RenderQueue;
                if (queue == null)
                    break;
                // 多帧抓取要等整串抓完, 中途的 LastCaptureFile 是第一帧的。
                if (queue.CaptureRenderDocFrame == false && queue.RemainingCaptureFrames == 0)
                {
                    file = queue.LastCaptureFile;
                    if (file != null)
                        break;
                }
            }

            if (string.IsNullOrEmpty(file))
            {
                return FailJson("capture did not produce a file. Either RenderDoc is not attached to this " +
                            "process (launch the engine through RenderDoc or with its layer enabled), " +
                            "or the window is not rendering.");
            }

            var dir = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache,
                IO.TtFileManager.ESystemDir.RenderDoc);
            return JsonSerializer.Serialize(new
            {
                captured = true,
                file = System.IO.Path.GetFullPath(dir + file),
                fileName = file,
                frameCount = frames,
            });
        }

        [Bricks.AIGC.TtMCPTool("list_renderdoc_captures",
            "Lists .rdc capture files already on disk in the engine's RenderDoc cache directory, " +
            "newest first. Use this to pick up a capture taken manually via the editor's Cap menu.",
            returnDescription: "{directory: string, captures: [{file: string - Absolute path, " +
            "fileName: string, sizeBytes: number, modifiedUtc: string}], count: number}")]
        public static string ListRenderDocCaptures(
            [Bricks.AIGC.TtMCPParameter("Maximum number of captures to return")] double count = 10)
        {
            var dir = TtEngine.Instance.FileManager.GetPath(IO.TtFileManager.ERootDir.Cache,
                IO.TtFileManager.ESystemDir.RenderDoc);
            var absDir = System.IO.Path.GetFullPath(dir);
            if (System.IO.Directory.Exists(absDir) == false)
                return JsonSerializer.Serialize(new { directory = absDir, captures = new object[0], count = 0 });

            var files = new List<System.IO.FileInfo>();
            foreach (var f in System.IO.Directory.GetFiles(absDir, "*.rdc"))
                files.Add(new System.IO.FileInfo(f));
            files.Sort((a, b) => b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc));

            var max = Math.Max(1, (int)count);
            var list = new List<object>();
            for (int i = 0; i < files.Count && i < max; i++)
            {
                list.Add(new
                {
                    file = files[i].FullName,
                    fileName = files[i].Name,
                    sizeBytes = files[i].Length,
                    modifiedUtc = files[i].LastWriteTimeUtc.ToString("o"),
                });
            }
            return JsonSerializer.Serialize(new { directory = absDir, captures = list, count = list.Count });
        }

        private static string SanitizeCaptureTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return "mcp";
            var sb = new System.Text.StringBuilder();
            foreach (var c in tag)
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                    sb.Append(c);
            }
            return sb.Length > 0 ? sb.ToString() : "mcp";
        }

        /// <summary>
        /// 工具失败时统一的返回形状。Python 侧不区分 HTTP 状态, 一律看 JSON 里有没有 error。
        /// </summary>
        private static string FailJson(string error)
        {
            return JsonSerializer.Serialize(new { error });
        }
    }
}
