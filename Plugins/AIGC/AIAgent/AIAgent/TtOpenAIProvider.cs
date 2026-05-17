using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EngineNS.Plugins.AIAgent
{
    /// <summary>
    /// LLM provider that speaks the OpenAI Chat Completions API.
    /// Compatible with OpenAI, Azure OpenAI, Qwen (DashScope), Ollama, and any
    /// service that implements the same REST contract.
    /// </summary>
    public class TtOpenAIProvider : TtLLMProvider
    {
        private HttpClient mHttpClient;
        private string mChatEndpoint;

        protected override void OnInitialize(TtLLMProviderConfig config)
        {
            mHttpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds > 0 ? config.TimeoutSeconds : 60)
            };
            if (!string.IsNullOrEmpty(config.ApiKey))
            {
                mHttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", config.ApiKey);
            }

            var baseUrl = config.BaseUrl.TrimEnd('/');
            mChatEndpoint = $"{baseUrl}/chat/completions";
        }

        public override async Task<TtChatResponse> ChatAsync(
            List<TtChatMessage> messages,
            List<TtToolDefinition> tools = null,
            string model = null,
            float? temperature = null,
            CancellationToken cancellation = default)
        {
            var requestBody = BuildRequestBody(messages, tools, model, temperature, stream: false);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var httpResponse = await mHttpClient.PostAsync(mChatEndpoint, content, cancellation);
            var responseText = await httpResponse.Content.ReadAsStringAsync();

            if (!httpResponse.IsSuccessStatusCode)
                throw new HttpRequestException($"LLM API error {httpResponse.StatusCode}: {responseText}");

            return ParseNonStreamResponse(responseText);
        }

        public override async Task<TtChatResponse> ChatStreamAsync(
            List<TtChatMessage> messages,
            FOnStreamChunk onChunk,
            List<TtToolDefinition> tools = null,
            string model = null,
            float? temperature = null,
            CancellationToken cancellation = default)
        {
            var requestBody = BuildRequestBody(messages, tools, model, temperature, stream: true);
            var request = new HttpRequestMessage(HttpMethod.Post, mChatEndpoint)
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            var httpResponse = await mHttpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead, cancellation);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorText = await httpResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"LLM API error {httpResponse.StatusCode}: {errorText}");
            }

            return await ParseStreamResponse(httpResponse, onChunk, cancellation);
        }

        #region Request Building

        private string BuildRequestBody(
            List<TtChatMessage> messages,
            List<TtToolDefinition> tools,
            string model,
            float? temperature,
            bool stream)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new Utf8JsonWriter(memoryStream);

            writer.WriteStartObject();
            writer.WriteString("model", model ?? Config.DefaultModel);
            writer.WriteNumber("temperature", temperature ?? Config.Temperature);
            if (Config.MaxTokens > 0)
                writer.WriteNumber("max_tokens", Config.MaxTokens);
            writer.WriteBoolean("stream", stream);

            WriteMessages(writer, messages);

            if (tools != null && tools.Count > 0)
                WriteTools(writer, tools);

            writer.WriteEndObject();
            writer.Flush();

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private static void WriteMessages(Utf8JsonWriter writer, List<TtChatMessage> messages)
        {
            writer.WriteStartArray("messages");
            foreach (var message in messages)
            {
                writer.WriteStartObject();
                writer.WriteString("role", message.Role);

                if (message.Content != null)
                    writer.WriteString("content", message.Content);

                if (message.ToolCallId != null)
                    writer.WriteString("tool_call_id", message.ToolCallId);

                if (message.Name != null)
                    writer.WriteString("name", message.Name);

                if (message.ToolCalls != null && message.ToolCalls.Count > 0)
                {
                    writer.WriteStartArray("tool_calls");
                    foreach (var toolCall in message.ToolCalls)
                    {
                        writer.WriteStartObject();
                        writer.WriteString("id", toolCall.Id);
                        writer.WriteString("type", "function");
                        writer.WriteStartObject("function");
                        writer.WriteString("name", toolCall.FunctionName);
                        writer.WriteString("arguments", toolCall.ArgumentsJson);
                        writer.WriteEndObject();
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        private static void WriteTools(Utf8JsonWriter writer, List<TtToolDefinition> tools)
        {
            writer.WriteStartArray("tools");
            foreach (var tool in tools)
            {
                writer.WriteStartObject();
                writer.WriteString("type", "function");
                writer.WriteStartObject("function");
                writer.WriteString("name", tool.Name);
                writer.WriteString("description", tool.Description);
                if (!string.IsNullOrEmpty(tool.ParametersJsonSchema))
                {
                    writer.WritePropertyName("parameters");
                    using var paramDoc = JsonDocument.Parse(tool.ParametersJsonSchema);
                    paramDoc.RootElement.WriteTo(writer);
                }
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        #endregion

        #region Response Parsing

        private static TtChatResponse ParseNonStreamResponse(string responseText)
        {
            using var doc = JsonDocument.Parse(responseText);
            var root = doc.RootElement;
            var choice = root.GetProperty("choices")[0];
            var message = choice.GetProperty("message");

            var response = new TtChatResponse();

            if (message.TryGetProperty("content", out var contentElement) &&
                contentElement.ValueKind == JsonValueKind.String)
            {
                response.Content = contentElement.GetString();
            }

            if (choice.TryGetProperty("finish_reason", out var finishElement))
                response.FinishReason = finishElement.GetString();

            if (message.TryGetProperty("tool_calls", out var toolCallsElement))
                response.ToolCalls = ParseToolCalls(toolCallsElement);

            if (root.TryGetProperty("usage", out var usageElement))
            {
                if (usageElement.TryGetProperty("prompt_tokens", out var promptTokens))
                    response.PromptTokens = promptTokens.GetInt32();
                if (usageElement.TryGetProperty("completion_tokens", out var completionTokens))
                    response.CompletionTokens = completionTokens.GetInt32();
            }

            return response;
        }

        private static List<TtToolCall> ParseToolCalls(JsonElement toolCallsElement)
        {
            var toolCalls = new List<TtToolCall>();
            foreach (var toolCallElement in toolCallsElement.EnumerateArray())
            {
                var functionElement = toolCallElement.GetProperty("function");
                toolCalls.Add(new TtToolCall
                {
                    Id = toolCallElement.GetProperty("id").GetString(),
                    FunctionName = functionElement.GetProperty("name").GetString(),
                    ArgumentsJson = functionElement.GetProperty("arguments").GetString()
                });
            }
            return toolCalls;
        }

        private static async Task<TtChatResponse> ParseStreamResponse(
            HttpResponseMessage httpResponse,
            FOnStreamChunk onChunk,
            CancellationToken cancellation)
        {
            var response = new TtChatResponse();
            var contentBuilder = new StringBuilder();
            var toolCallBuilders = new Dictionary<int, (string id, string name, StringBuilder args)>();

            using var stream = await httpResponse.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string line;
            while ((line = await reader.ReadLineAsync()) != null && !cancellation.IsCancellationRequested)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (!line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6);
                if (data == "[DONE]")
                    break;

                try
                {
                    using var doc = JsonDocument.Parse(data);
                    var choice = doc.RootElement.GetProperty("choices")[0];
                    var delta = choice.GetProperty("delta");

                    if (delta.TryGetProperty("content", out var deltaContent) &&
                        deltaContent.ValueKind == JsonValueKind.String)
                    {
                        var chunk = deltaContent.GetString();
                        if (!string.IsNullOrEmpty(chunk))
                        {
                            contentBuilder.Append(chunk);
                            onChunk?.Invoke(chunk);
                        }
                    }

                    if (delta.TryGetProperty("tool_calls", out var deltaToolCalls))
                    {
                        foreach (var deltaToolCall in deltaToolCalls.EnumerateArray())
                        {
                            var index = deltaToolCall.GetProperty("index").GetInt32();
                            if (!toolCallBuilders.ContainsKey(index))
                            {
                                var id = deltaToolCall.TryGetProperty("id", out var idEl) ? idEl.GetString() : "";
                                var name = "";
                                if (deltaToolCall.TryGetProperty("function", out var funcEl) &&
                                    funcEl.TryGetProperty("name", out var nameEl))
                                    name = nameEl.GetString();
                                toolCallBuilders[index] = (id, name, new StringBuilder());
                            }

                            if (deltaToolCall.TryGetProperty("function", out var funcElement) &&
                                funcElement.TryGetProperty("arguments", out var argsElement))
                            {
                                toolCallBuilders[index].args.Append(argsElement.GetString());
                            }
                        }
                    }

                    if (choice.TryGetProperty("finish_reason", out var finishElement) &&
                        finishElement.ValueKind == JsonValueKind.String)
                    {
                        response.FinishReason = finishElement.GetString();
                    }
                }
                catch (JsonException)
                {
                    // Malformed SSE chunk, skip
                }
            }

            response.Content = contentBuilder.Length > 0 ? contentBuilder.ToString() : null;

            if (toolCallBuilders.Count > 0)
            {
                response.ToolCalls = new List<TtToolCall>();
                foreach (var kv in toolCallBuilders)
                {
                    response.ToolCalls.Add(new TtToolCall
                    {
                        Id = kv.Value.id,
                        FunctionName = kv.Value.name,
                        ArgumentsJson = kv.Value.args.ToString()
                    });
                }
            }

            return response;
        }

        #endregion

        public override void Dispose()
        {
            mHttpClient?.Dispose();
            mHttpClient = null;
        }
    }
}
