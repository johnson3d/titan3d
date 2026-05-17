using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EngineNS.Bricks.AIGC;

namespace EngineNS.Plugins.AIAgent
{
    /// <summary>
    /// Event args for agent lifecycle events.
    /// </summary>
    public class TtAgentEventArgs : EventArgs
    {
        public string Message { get; set; }
        public int Round { get; set; }
    }

    /// <summary>
    /// Core AI Agent that implements a ReAct (Reason + Act) loop.
    /// Configurable with LLM providers, MCP tools, and skills.
    /// </summary>
    public class TtAIAgent : IDisposable
    {
        private TtAgentConfig mConfig;
        private readonly Dictionary<string, TtLLMProvider> mProviders = new();
        private readonly Dictionary<string, TtToolDefinition> mToolDefinitions = new();
        private readonly Dictionary<string, Func<string, string>> mToolExecutors = new();
        private readonly List<TtChatMessage> mConversationHistory = new();
        private TtSkill mActiveSkill;
        private CancellationTokenSource mCancellationSource;

        /// <summary>Fired when the agent produces a text chunk (streaming).</summary>
        public event FOnStreamChunk OnStreamChunk;

        /// <summary>Fired when the agent starts executing a tool.</summary>
        public event Action<string, string> OnToolExecuting;

        /// <summary>Fired when a tool execution completes.</summary>
        public event Action<string, string, string> OnToolExecuted;

        /// <summary>Fired when a ReAct round starts.</summary>
        public event EventHandler<TtAgentEventArgs> OnRoundStarted;

        /// <summary>Fired when the agent finishes its response.</summary>
        public event EventHandler<TtAgentEventArgs> OnCompleted;

        /// <summary>Current conversation history (read-only view).</summary>
        public IReadOnlyList<TtChatMessage> ConversationHistory => mConversationHistory;

        /// <summary>Whether the agent is currently processing a request.</summary>
        public bool IsBusy { get; private set; }

        /// <summary>Whether the agent has a valid LLM provider configured.</summary>
        public bool IsConfigured => mConfig != null && mProviders.Count > 0;

        public TtAgentConfig Config => mConfig;
        public TtSkill ActiveSkill => mActiveSkill;

        public void Initialize()
        {
            mConfig = TtAgentConfig.Load();
            InitializeProviders();
            CollectEngineTools();
            SwitchSkill(mConfig.DefaultSkill);
        }

        public void Dispose()
        {
            Cancel();
            foreach (var provider in mProviders.Values)
                provider.Dispose();
            mProviders.Clear();
        }

        #region Provider Management

        private void InitializeProviders()
        {
            foreach (var providerConfig in mConfig.Providers)
            {
                try
                {
                    var provider = CreateProvider(providerConfig.ProviderType);
                    provider.Initialize(providerConfig);
                    mProviders[providerConfig.Name] = provider;
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                        $"[AIAgent] Provider '{providerConfig.Name}' ({providerConfig.ProviderType}) initialized");
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                        $"[AIAgent] Failed to initialize provider '{providerConfig.Name}': {ex.Message}");
                }
            }
        }

        private static TtLLMProvider CreateProvider(string providerType)
        {
            return providerType?.ToLowerInvariant() switch
            {
                "openai" or "azure" or "qwen" or "dashscope" or "ollama" => new TtOpenAIProvider(),
                _ => new TtOpenAIProvider()
            };
        }

        private TtLLMProvider GetProvider(string name = null)
        {
            var providerName = name ?? mActiveSkill?.LLMProviderOverride ?? mConfig.DefaultProvider;
            if (mProviders.TryGetValue(providerName, out var provider))
                return provider;
            if (mProviders.Count > 0)
            {
                foreach (var kv in mProviders)
                    return kv.Value;
            }
            throw new InvalidOperationException($"No LLM provider available. Requested: '{providerName}'");
        }

        #endregion

        #region Tool Management

        private void CollectEngineTools()
        {
            if (!mConfig.AutoCollectEngineTools)
                return;

            var mcpPlugin = FindMCPPlugin();
            if (mcpPlugin == null)
                return;

            var tools = mcpPlugin.ListTools();
            foreach (var tool in tools)
            {
                var parametersSchema = BuildJsonSchema(tool.Parameters);
                mToolDefinitions[tool.Name] = new TtToolDefinition
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    ParametersJsonSchema = parametersSchema
                };
                var toolName = tool.Name;
                mToolExecutors[tool.Name] = (argsJson) =>
                {
                    var arguments = ParseToolArguments(argsJson);
                    var result = mcpPlugin.InvokeTool(toolName, arguments);
                    return result.Success ? result.Content : $"Error: {result.ErrorMessage}";
                };
            }

            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                $"[AIAgent] Collected {mToolDefinitions.Count} tools from MCP");
        }

        private static TtMCPPlugin FindMCPPlugin()
        {
            try
            {
                var module = TtEngine.Instance.PluginModuleManager.GetPluginModule("MCPServer");
                return module?.GetPluginObject<TtMCPPlugin>();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Register a custom tool that can be called by the agent.
        /// </summary>
        public void RegisterTool(string name, string description, string parametersJsonSchema,
            Func<string, string> executor)
        {
            mToolDefinitions[name] = new TtToolDefinition
            {
                Name = name,
                Description = description,
                ParametersJsonSchema = parametersJsonSchema
            };
            mToolExecutors[name] = executor;
        }

        private List<TtToolDefinition> GetAvailableTools()
        {
            if (mActiveSkill?.AllowedTools == null || mActiveSkill.AllowedTools.Count == 0)
                return new List<TtToolDefinition>(mToolDefinitions.Values);

            var filtered = new List<TtToolDefinition>();
            foreach (var toolName in mActiveSkill.AllowedTools)
            {
                if (mToolDefinitions.TryGetValue(toolName, out var definition))
                    filtered.Add(definition);
            }
            return filtered;
        }

        #endregion

        #region Skill Management

        public void SwitchSkill(string skillName)
        {
            mActiveSkill = mConfig.FindSkill(skillName);
            if (mActiveSkill == null)
            {
                mActiveSkill = new TtSkill { Name = skillName };
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning,
                    $"[AIAgent] Skill '{skillName}' not found, using default");
            }
            ClearHistory();
        }

        #endregion

        #region Conversation

        public void ClearHistory()
        {
            mConversationHistory.Clear();
        }

        public void Cancel()
        {
            mCancellationSource?.Cancel();
        }

        /// <summary>
        /// Send a user message and run the ReAct loop until the agent produces a final answer.
        /// Text is streamed via OnStreamChunk events.
        /// </summary>
        public async Task<string> ChatAsync(string userMessage)
        {
            if (IsBusy)
                throw new InvalidOperationException("Agent is already processing a request");

            IsBusy = true;
            mCancellationSource = new CancellationTokenSource();
            var cancellation = mCancellationSource.Token;

            try
            {
                mConversationHistory.Add(TtChatMessage.User(userMessage));

                var provider = GetProvider();
                var model = mActiveSkill?.ModelOverride;
                var temperature = mActiveSkill?.TemperatureOverride;
                var maxRounds = mActiveSkill?.MaxRounds ?? mConfig.MaxRounds;
                var tools = GetAvailableTools();

                for (int round = 0; round < maxRounds; round++)
                {
                    cancellation.ThrowIfCancellationRequested();

                    OnRoundStarted?.Invoke(this, new TtAgentEventArgs
                    {
                        Round = round + 1,
                        Message = $"Round {round + 1}/{maxRounds}"
                    });

                    var messages = BuildMessages();
                    var response = await provider.ChatStreamAsync(
                        messages,
                        chunk => OnStreamChunk?.Invoke(chunk),
                        tools.Count > 0 ? tools : null,
                        model,
                        temperature,
                        cancellation);

                    if (!response.HasToolCalls)
                    {
                        var finalText = response.Content ?? "";
                        mConversationHistory.Add(TtChatMessage.Assistant(finalText));
                        OnCompleted?.Invoke(this, new TtAgentEventArgs
                        {
                            Message = finalText,
                            Round = round + 1
                        });
                        return finalText;
                    }

                    var assistantMessage = new TtChatMessage
                    {
                        Role = "assistant",
                        Content = response.Content,
                        ToolCalls = response.ToolCalls
                    };
                    mConversationHistory.Add(assistantMessage);

                    foreach (var toolCall in response.ToolCalls)
                    {
                        var toolResult = ExecuteTool(toolCall);
                        mConversationHistory.Add(TtChatMessage.Tool(toolCall.Id, toolResult));
                    }
                }

                var maxRoundMessage = "[Agent reached maximum rounds without a final answer]";
                mConversationHistory.Add(TtChatMessage.Assistant(maxRoundMessage));
                OnCompleted?.Invoke(this, new TtAgentEventArgs { Message = maxRoundMessage });
                return maxRoundMessage;
            }
            catch (OperationCanceledException)
            {
                return "[Cancelled]";
            }
            catch (Exception ex)
            {
                var errorMessage = $"[Error: {ex.Message}]";
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error,
                    $"[AIAgent] {errorMessage}");
                return errorMessage;
            }
            finally
            {
                IsBusy = false;
                mCancellationSource?.Dispose();
                mCancellationSource = null;
            }
        }

        private List<TtChatMessage> BuildMessages()
        {
            var messages = new List<TtChatMessage>();
            messages.Add(TtChatMessage.System(mActiveSkill?.SystemPrompt ??
                "You are a helpful Titan Engine assistant."));
            messages.AddRange(mConversationHistory);
            return messages;
        }

        private string ExecuteTool(TtToolCall toolCall)
        {
            OnToolExecuting?.Invoke(toolCall.FunctionName, toolCall.ArgumentsJson);

            string result;
            if (mToolExecutors.TryGetValue(toolCall.FunctionName, out var executor))
            {
                try
                {
                    result = executor(toolCall.ArgumentsJson);
                }
                catch (Exception ex)
                {
                    result = $"Tool execution error: {ex.Message}";
                }
            }
            else
            {
                result = $"Unknown tool: {toolCall.FunctionName}";
            }

            OnToolExecuted?.Invoke(toolCall.FunctionName, toolCall.ArgumentsJson, result);
            return result;
        }

        #endregion

        #region Helpers

        private static string BuildJsonSchema(List<TtMCPToolParameter> parameters)
        {
            using var stream = new System.IO.MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();
            writer.WriteString("type", "object");
            writer.WriteStartObject("properties");

            var requiredList = new List<string>();
            foreach (var param in parameters)
            {
                writer.WriteStartObject(param.Name);
                writer.WriteString("type", MapTypeToJsonSchemaType(param.Type));
                if (!string.IsNullOrEmpty(param.Description))
                    writer.WriteString("description", param.Description);
                writer.WriteEndObject();
                if (param.Required)
                    requiredList.Add(param.Name);
            }

            writer.WriteEndObject();

            if (requiredList.Count > 0)
            {
                writer.WriteStartArray("required");
                foreach (var name in requiredList)
                    writer.WriteStringValue(name);
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
            writer.Flush();

            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }

        private static string MapTypeToJsonSchemaType(string csharpType)
        {
            return csharpType?.ToLowerInvariant() switch
            {
                "string" => "string",
                "int32" or "int64" or "single" or "double" or "decimal" => "number",
                "boolean" => "boolean",
                _ => "string"
            };
        }

        private static Dictionary<string, object> ParseToolArguments(string argsJson)
        {
            if (string.IsNullOrEmpty(argsJson))
                return new Dictionary<string, object>();

            var arguments = new Dictionary<string, object>();
            using var doc = JsonDocument.Parse(argsJson);
            foreach (var prop in doc.RootElement.EnumerateObject())
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
            return arguments;
        }

        #endregion
    }
}
