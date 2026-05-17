using System;
using System.Collections.Generic;

namespace EngineNS.Plugins.AIAgent
{
    /// <summary>
    /// Defines a reusable skill that configures an agent's persona, allowed tools, and LLM overrides.
    /// Skills are loaded from .skill.json files or created programmatically.
    /// </summary>
    public class TtSkill
    {
        [Rtti.Meta("")]
        public string Name { get; set; } = "Default";
        [Rtti.Meta("")]
        public string Description { get; set; } = "";
        [Rtti.Meta("")]
        public string SystemPrompt { get; set; } = "You are a helpful Titan Engine assistant.";
        [Rtti.Meta("")]
        public List<string> AllowedTools { get; set; } = new List<string>();
        [Rtti.Meta("")]
        public int MaxRounds { get; set; } = 20;
        [Rtti.Meta("")]
        public string LLMProviderOverride { get; set; }
        [Rtti.Meta("")]
        public string ModelOverride { get; set; }
        [Rtti.Meta("")]
        public float? TemperatureOverride { get; set; }
    }

    /// <summary>
    /// Describes an LLM provider endpoint configuration.
    /// Multiple providers can be registered (e.g. OpenAI, Qwen, Ollama).
    /// </summary>
    public class TtLLMProviderConfig
    {
        [Rtti.Meta("")]
        public string Name { get; set; } = "deepseek";
        [Rtti.Meta("")]
        public string ProviderType { get; set; } = "OpenAI";
        [Rtti.Meta("")]
        public string BaseUrl { get; set; } = "https://api.deepseek.com";
        [Rtti.Meta("")]
        public string ApiKey { get; set; } = "";
        [Rtti.Meta("")]
        public string DefaultModel { get; set; } = "gpt-4o";
        [Rtti.Meta("")]
        public float Temperature { get; set; } = 0.7f;
        [Rtti.Meta("")]
        public int MaxTokens { get; set; } = 4096;
        [Rtti.Meta("")]
        public int TimeoutSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Describes an MCP server endpoint that the agent can connect to for additional tools.
    /// </summary>
    public class TtMCPEndpointConfig
    {
        public string Name { get; set; } = "TitanEngine";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 8818;
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Root configuration for the AI Agent plugin.
    /// Loaded by the plugin itself via TtConfigManager.TryLoad (not via engine-scanned ConfigManager)
    /// to avoid timing issues with plugin assembly loading.
    /// </summary>
    [IO.TtConfig(Path = "agent.cfg")]
    public class TtAgentConfig : IO.IConfig
    {
        [Rtti.Meta("")]
        public string DefaultProvider { get; set; } = "openai";
        [Rtti.Meta("")]
        public string DefaultSkill { get; set; } = "Default";
        [Rtti.Meta("")]
        public int MaxRounds { get; set; } = 20;
        [Rtti.Meta("")]
        public bool AutoCollectEngineTools { get; set; } = true;
        [Rtti.Meta("")]
        public List<TtLLMProviderConfig> Providers { get; set; } = new List<TtLLMProviderConfig>
        {
            new TtLLMProviderConfig()
        };
        [Rtti.Meta("")]
        public List<TtMCPEndpointConfig> MCPEndpoints { get; set; } = new List<TtMCPEndpointConfig>
        {
            new TtMCPEndpointConfig()
        };
        [Rtti.Meta("")]
        public List<TtSkill> Skills { get; set; } = new List<TtSkill>
        {
            new TtSkill
            {
                Name = "Default",
                Description = "General-purpose Titan Engine assistant",
                SystemPrompt = "You are an AI assistant integrated into Titan Engine. " +
                    "You can inspect and manipulate the scene, query assets, and help with engine tasks. " +
                    "Use the available tools to fulfill user requests. " +
                    "Always explain what you are doing before invoking a tool.",
            },
            new TtSkill
            {
                Name = "SceneBuilder",
                Description = "Specialized in building and editing 3D scenes",
                SystemPrompt = "You are a scene-building expert for Titan Engine. " +
                    "Help users create, position, and configure actors and assets in the scene. " +
                    "Ask clarifying questions when the request is ambiguous.",
                AllowedTools = new List<string>
                {
                    "create_actor", "set_transform", "set_material",
                    "list_assets", "get_engine_info"
                },
                MaxRounds = 30,
            }
        };

        /// <summary>
        /// Finds a provider config by name. Returns null if not found.
        /// </summary>
        public TtLLMProviderConfig FindProvider(string name)
        {
            foreach (var provider in Providers)
            {
                if (string.Equals(provider.Name, name, StringComparison.OrdinalIgnoreCase))
                    return provider;
            }
            return null;
        }

        /// <summary>
        /// Finds a skill by name. Returns null if not found.
        /// </summary>
        public TtSkill FindSkill(string name)
        {
            foreach (var skill in Skills)
            {
                if (string.Equals(skill.Name, name, StringComparison.OrdinalIgnoreCase))
                    return skill;
            }
            return null;
        }

        /// <summary>
        /// Load config via engine's TryLoad (handles json + patch), independent of ConfigManager scan timing.
        /// </summary>
        public static TtAgentConfig Load()
        {
            return IO.TtConfigManager.TryLoad(typeof(TtAgentConfig), null, null) as TtAgentConfig
                ?? new TtAgentConfig();
        }
    }
}
