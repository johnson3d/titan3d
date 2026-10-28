using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EngineNS;

namespace BrickManager
{
    /// <summary>
    /// "Create Plugin..." 弹窗：收集参数后调用 BuildScript/scaffold/New-TitanPlugin.ps1，生成与 sln 登记都由脚本完成，这里只负责表单和日志。
    /// </summary>
    public class TtCreatePluginDialog
    {
        private const string PopupName = "Create Plugin";
        private readonly string engineRootPath;
        private bool requestOpen = false;

        private string pluginName = string.Empty;
        private string category = string.Empty;
        private string folder = string.Empty;
        private bool allPlatforms = true;
        private bool windowsPlatform = true;
        private bool androidPlatform = false;
        private bool loadOnInit = true;
        private bool leanClr = false;
        private bool buildAfterCreate = false;
        private readonly HashSet<string> selectedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private List<string> existingPlugins = new List<string>();
        private List<string> existingCategories = new List<string>();

        // logLines / scrollLogToBottom / pendingExitCode 由进程回调线程写入，统一用 logLock 保护。
        private readonly object logLock = new object();
        private readonly List<string> logLines = new List<string>();
        private bool scrollLogToBottom = false;
        private int? pendingExitCode = null;
        private Process runningProcess;
        // null: 尚未运行；0: 成功；其它: 失败
        private int? lastExitCode = null;
        private string lastCreatedName = string.Empty;

        public TtCreatePluginDialog(string engineRootPath)
        {
            this.engineRootPath = engineRootPath;
        }

        public bool IsRunning => runningProcess != null;

        public void Open()
        {
            RefreshExistingPlugins();
            requestOpen = true;
        }

        private string PluginsRoot => Path.Combine(engineRootPath, "Plugins");

        /// <summary>
        /// 与脚本一致：插件名取 .plugin 文件名，编译输出目录里的拷贝不算；.plugin 所在目录的上级路径（含各级前缀）视为 Category。
        /// </summary>
        private void RefreshExistingPlugins()
        {
            var plugins = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var categories = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(PluginsRoot))
            {
                foreach (var file in Directory.GetFiles(PluginsRoot, "*.plugin", SearchOption.AllDirectories))
                {
                    var relative = Path.GetRelativePath(PluginsRoot, file);
                    var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (parts.Any(p => p.Equals("binaries", StringComparison.OrdinalIgnoreCase) ||
                                       p.Equals("intermediate", StringComparison.OrdinalIgnoreCase) ||
                                       p.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                                       p.Equals("obj", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                    plugins.Add(Path.GetFileNameWithoutExtension(file));
                    // parts = [Category..., Folder, Name.plugin]
                    for (var depth = 1; depth <= parts.Length - 2; depth++)
                    {
                        categories.Add(string.Join("/", parts.Take(depth)));
                    }
                }
            }
            existingPlugins = plugins.ToList();
            existingCategories = categories.ToList();
            selectedDependencies.RemoveWhere(d => !plugins.Contains(d));
        }

        public unsafe void Draw()
        {
            ConsumeProcessExit();
            if (requestOpen)
            {
                ImGuiAPI.OpenPopup(PopupName, ImGuiPopupFlags_.ImGuiPopupFlags_None);
                requestOpen = false;
            }

            var size = new Vector2(720, 640);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var visible = true;
            EngineNS.EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
            if (ImGuiAPI.BeginPopupModal(PopupName, ref visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                DrawForm();
                ImGuiAPI.Separator();
                DrawActions();
                ImGuiAPI.Separator();
                DrawLog();
                ImGuiAPI.EndPopup();
            }
            EngineNS.EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
        }

        private void DrawForm()
        {
            var gray = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);

            ImGuiAPI.Text("Name:");
            ImGuiAPI.SameLine(110, -1);
            ImGuiAPI.InputText("##plugin_name", ref pluginName);
            var nameError = ValidateName();
            if (nameError != null && !IsJustCreated)
            {
                ImGuiAPI.SameLine(0, 10);
                ImGuiAPI.TextColored(new Vector4(1.0f, 0.4f, 0.4f, 1.0f), nameError);
            }

            ImGuiAPI.Text("Category:");
            ImGuiAPI.SameLine(110, -1);
            ImGuiAPI.InputText("##plugin_category", ref category);
            ImGuiAPI.SameLine(0, 10);
            ImGuiAPI.TextColored(gray, "(optional, e.g. Game or AIGC/Tools)");
            if (existingCategories.Count > 0)
            {
                ImGuiAPI.PushTextWrapPos(0.0f);
                ImGuiAPI.TextColored(gray, "  Existing: " + string.Join(", ", existingCategories));
                ImGuiAPI.PopTextWrapPos();
            }

            ImGuiAPI.Text("Folder:");
            ImGuiAPI.SameLine(110, -1);
            ImGuiAPI.InputText("##plugin_folder", ref folder);
            ImGuiAPI.SameLine(0, 10);
            ImGuiAPI.TextColored(gray, "(optional, defaults to Name)");
            if (!string.IsNullOrWhiteSpace(pluginName))
            {
                ImGuiAPI.TextColored(gray, $"  Target: {TargetRelativePath}/{pluginName}.plugin");
            }

            ImGuiAPI.Text("Platforms:");
            ImGuiAPI.SameLine(110, -1);
            if (ImGuiAPI.RadioButton("All (default)", allPlatforms))
            {
                allPlatforms = true;
            }
            ImGuiAPI.SameLine(0, 10);
            if (ImGuiAPI.RadioButton("Specific", !allPlatforms))
            {
                allPlatforms = false;
            }
            if (!allPlatforms)
            {
                ImGuiAPI.SameLine(0, 20);
                ImGuiAPI.Checkbox("Windows", ref windowsPlatform);
                ImGuiAPI.SameLine(0, 10);
                ImGuiAPI.Checkbox("Android", ref androidPlatform);
            }

            ImGuiAPI.Checkbox("Load on init", ref loadOnInit);
            ImGuiAPI.SameLine(0, 20);
            ImGuiAPI.Checkbox("LeanCLR stub", ref leanClr);
            ImGuiAPI.SameLine(0, 20);
            ImGuiAPI.Checkbox("Build after create", ref buildAfterCreate);

            ImGuiAPI.Text($"Dependencies ({selectedDependencies.Count} selected):");
            var listSize = new Vector2(0, 150);
            if (ImGuiAPI.BeginChild("PluginDependencies", in listSize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiAPI.Columns(3, "PluginDependencyColumns", false);
                foreach (var plugin in existingPlugins)
                {
                    var isSelected = selectedDependencies.Contains(plugin);
                    if (ImGuiAPI.Checkbox(plugin, ref isSelected))
                    {
                        if (isSelected)
                            selectedDependencies.Add(plugin);
                        else
                            selectedDependencies.Remove(plugin);
                    }
                    ImGuiAPI.NextColumn();
                }
                ImGuiAPI.Columns(1, null, false);
            }
            ImGuiAPI.EndChild();
        }

        private void DrawActions()
        {
            var formError = ValidateName() ?? ValidateOptions();
            if (IsRunning)
            {
                ImGuiAPI.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Running New-TitanPlugin.ps1 ...");
            }
            else
            {
                if (ImGuiAPI.Button("Create", in Vector2.Zero) && formError == null)
                {
                    StartCreate();
                }
                ImGuiAPI.SameLine(0, 10);
                if (ImGuiAPI.Button("Close", in Vector2.Zero))
                {
                    ImGuiAPI.CloseCurrentPopup();
                }
                if (formError != null && !IsJustCreated)
                {
                    ImGuiAPI.SameLine(0, 20);
                    ImGuiAPI.TextColored(new Vector4(1.0f, 0.4f, 0.4f, 1.0f), formError);
                }
            }

            if (lastExitCode == 0)
            {
                ImGuiAPI.PushTextWrapPos(0.0f);
                ImGuiAPI.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), $"Plugin '{lastCreatedName}' created.");
                ImGuiAPI.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), $"Add \"{lastCreatedName}\" to the Plugins list of your jscfg, then reload EngineAll.sln in Visual Studio.");
                ImGuiAPI.PopTextWrapPos();
            }
            else if (lastExitCode != null)
            {
                ImGuiAPI.TextColored(new Vector4(1.0f, 0.4f, 0.4f, 1.0f), $"Failed (exit {lastExitCode}). See log below.");
            }
        }

        private void DrawLog()
        {
            ImGuiAPI.Text("Log:");
            var logSize = new Vector2(0, 0);
            if (ImGuiAPI.BeginChild("CreatePluginLog", in logSize, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_HorizontalScrollbar))
            {
                lock (logLock)
                {
                    foreach (var line in logLines)
                    {
                        ImGuiAPI.TextUnformatted(line);
                    }
                    if (scrollLogToBottom)
                    {
                        ImGuiAPI.SetScrollHereY(1.0f);
                        scrollLogToBottom = false;
                    }
                }
            }
            ImGuiAPI.EndChild();
        }

        /// <summary>
        /// 刚创建成功且名字未改：刷新后的插件列表会让校验报"已存在"，此时只显示成功提示，不显示矛盾的红字。
        /// </summary>
        private bool IsJustCreated => lastExitCode == 0 && string.Equals(pluginName, lastCreatedName, StringComparison.OrdinalIgnoreCase);

        private string ValidateName()
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                return "Name is required.";
            if (!IsIdentifier(pluginName))
                return "Use letters, digits and '_' only, not starting with a digit.";
            if (existingPlugins.Contains(pluginName, StringComparer.OrdinalIgnoreCase))
                return $"Plugin '{pluginName}' already exists.";
            return null;
        }

        private string[] CategorySegments => string.IsNullOrEmpty(category) ? Array.Empty<string>() : category.Split('/', '\\');

        private string EffectiveFolder => string.IsNullOrEmpty(folder) ? pluginName : folder;

        /// <summary>
        /// 与脚本的 Plugins\[Category]\&lt;Folder&gt; 一致，以 '/' 分隔，用于显示和目录占用检查。
        /// </summary>
        private string TargetRelativePath => string.Join("/", new[] { "Plugins" }.Concat(CategorySegments).Append(EffectiveFolder));

        private string ValidateOptions()
        {
            if (!string.IsNullOrEmpty(category) && !CategorySegments.All(IsIdentifier))
                return "Category levels must be folder names (letters, digits, '_') separated by '/'.";
            if (!string.IsNullOrEmpty(folder) && !IsIdentifier(folder))
                return "Folder must be a single folder name (letters, digits, '_').";
            if (Directory.Exists(Path.Combine(engineRootPath, TargetRelativePath)))
                return $"Target directory {TargetRelativePath} already exists.";
            if (!allPlatforms && !windowsPlatform && !androidPlatform)
                return "Select at least one platform.";
            if (leanClr && !allPlatforms && !windowsPlatform)
                return "LeanCLR stub requires the All or Windows platform.";
            return null;
        }

        private static bool IsIdentifier(string text)
        {
            if (string.IsNullOrEmpty(text) || char.IsDigit(text[0]))
                return false;
            return text.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_');
        }

        private List<string> BuildScriptArguments(string scriptPath)
        {
            var args = new List<string> { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", scriptPath, "-Name", pluginName };
            if (!string.IsNullOrEmpty(category))
            {
                args.Add("-Category");
                args.Add(category);
            }
            if (!string.IsNullOrEmpty(folder))
            {
                args.Add("-Folder");
                args.Add(folder);
            }
            var platforms = new List<string>();
            if (allPlatforms)
            {
                platforms.Add("All");
            }
            else
            {
                if (windowsPlatform)
                    platforms.Add("Windows");
                if (androidPlatform)
                    platforms.Add("Android");
            }
            // -File 模式下数组参数只能以逗号分隔的单个字符串传入，由脚本拆分。
            args.Add("-Platforms");
            args.Add(string.Join(",", platforms));
            if (selectedDependencies.Count > 0)
            {
                args.Add("-Dependencies");
                args.Add(string.Join(",", existingPlugins.Where(selectedDependencies.Contains)));
            }
            if (!loadOnInit)
                args.Add("-NoLoadOnInit");
            if (leanClr)
                args.Add("-LeanClr");
            if (buildAfterCreate)
                args.Add("-Build");
            return args;
        }

        private void AppendLog(string line)
        {
            if (line == null)
                return;
            lock (logLock)
            {
                logLines.Add(line);
                scrollLogToBottom = true;
            }
        }

        /// <summary>
        /// 在 UI 线程上处理进程退出：回调线程只记录退出码，状态切换和插件列表刷新都放到这里，避免与绘制并发访问。
        /// </summary>
        private void ConsumeProcessExit()
        {
            int? exitCode;
            lock (logLock)
            {
                exitCode = pendingExitCode;
                pendingExitCode = null;
            }
            if (exitCode == null)
                return;

            runningProcess?.Dispose();
            runningProcess = null;
            lastExitCode = exitCode;
            if (exitCode == 0)
            {
                RefreshExistingPlugins();
            }
        }

        private void StartCreate()
        {
            lock (logLock)
            {
                logLines.Clear();
            }
            lastExitCode = null;
            lastCreatedName = pluginName;

            var scriptPath = Path.GetFullPath(Path.Combine(engineRootPath, "BuildScript", "scaffold", "New-TitanPlugin.ps1"));
            if (!File.Exists(scriptPath))
            {
                AppendLog($"Script not found: {scriptPath}");
                lastExitCode = -1;
                return;
            }

            var startInfo = new ProcessStartInfo("powershell.exe")
            {
                WorkingDirectory = Path.GetFullPath(engineRootPath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var arg in BuildScriptArguments(scriptPath))
            {
                startInfo.ArgumentList.Add(arg);
            }
            // 让 dotnet build 输出英文，避免控制台代码页导致日志乱码。
            startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
            AppendLog("> powershell.exe " + string.Join(" ", startInfo.ArgumentList.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)));

            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            process.OutputDataReceived += (sender, e) => AppendLog(e.Data);
            process.ErrorDataReceived += (sender, e) => AppendLog(e.Data);
            process.Exited += (sender, e) =>
            {
                // 无参 WaitForExit 会等异步输出读完，保证退出码出现在日志最后。
                process.WaitForExit();
                var exitCode = process.ExitCode;
                AppendLog($"Exit code: {exitCode}");
                lock (logLock)
                {
                    pendingExitCode = exitCode;
                }
            };
            // 先置运行态再启动：进程瞬间退出时 Exited 回调也不会早于这里。
            runningProcess = process;
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to start powershell.exe: {ex.Message}");
                runningProcess = null;
                lastExitCode = -1;
                process.Dispose();
            }
        }
    }
}
