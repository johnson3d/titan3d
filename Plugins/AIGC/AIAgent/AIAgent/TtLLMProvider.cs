using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.Plugins.AIAgent
{
    /// <summary>
    /// Represents a single message in an LLM conversation.
    /// </summary>
    public class TtChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public List<TtToolCall> ToolCalls { get; set; }
        public string ToolCallId { get; set; }
        public string Name { get; set; }

        public static TtChatMessage System(string content) => new TtChatMessage { Role = "system", Content = content };
        public static TtChatMessage User(string content) => new TtChatMessage { Role = "user", Content = content };
        public static TtChatMessage Assistant(string content) => new TtChatMessage { Role = "assistant", Content = content };
        public static TtChatMessage Tool(string toolCallId, string content) =>
            new TtChatMessage { Role = "tool", ToolCallId = toolCallId, Content = content };
    }

    /// <summary>
    /// Represents a tool call requested by the LLM.
    /// </summary>
    public class TtToolCall
    {
        public string Id { get; set; }
        public string FunctionName { get; set; }
        public string ArgumentsJson { get; set; }
    }

    /// <summary>
    /// Describes a tool that can be offered to the LLM via function calling.
    /// </summary>
    public class TtToolDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParametersJsonSchema { get; set; }
    }

    /// <summary>
    /// The response returned by an LLM chat completion request.
    /// </summary>
    public class TtChatResponse
    {
        public string Content { get; set; }
        public List<TtToolCall> ToolCalls { get; set; } = new List<TtToolCall>();
        public bool HasToolCalls => ToolCalls != null && ToolCalls.Count > 0;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public string FinishReason { get; set; }
    }

    /// <summary>
    /// Callback for streaming text chunks from the LLM.
    /// </summary>
    public delegate void FOnStreamChunk(string textChunk);

    /// <summary>
    /// Abstract base class for LLM providers.
    /// Implement this to add support for different LLM backends (OpenAI, Qwen, Ollama, etc.).
    /// </summary>
    public abstract class TtLLMProvider : IDisposable
    {
        public string ProviderName { get; protected set; }
        public TtLLMProviderConfig Config { get; private set; }

        public void Initialize(TtLLMProviderConfig config)
        {
            Config = config;
            ProviderName = config.Name;
            OnInitialize(config);
        }

        protected abstract void OnInitialize(TtLLMProviderConfig config);

        /// <summary>
        /// Sends a chat completion request to the LLM.
        /// </summary>
        /// <param name="messages">Conversation history.</param>
        /// <param name="tools">Available tools for function calling. Null to disable.</param>
        /// <param name="model">Model override. Null to use config default.</param>
        /// <param name="temperature">Temperature override. Null to use config default.</param>
        /// <param name="cancellation">Cancellation token.</param>
        public abstract Task<TtChatResponse> ChatAsync(
            List<TtChatMessage> messages,
            List<TtToolDefinition> tools = null,
            string model = null,
            float? temperature = null,
            CancellationToken cancellation = default);

        /// <summary>
        /// Sends a streaming chat completion request.
        /// Text chunks are delivered via onChunk callback.
        /// The final TtChatResponse contains aggregated tool calls and token usage.
        /// </summary>
        public abstract Task<TtChatResponse> ChatStreamAsync(
            List<TtChatMessage> messages,
            FOnStreamChunk onChunk,
            List<TtToolDefinition> tools = null,
            string model = null,
            float? temperature = null,
            CancellationToken cancellation = default);

        public virtual void Dispose() { }
    }
}
