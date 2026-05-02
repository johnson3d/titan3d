using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Bricks.AIGC
{
    public abstract class TtAIGCPlugin : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public enum EMeshType
        {
            Unknown = 0,
            Obj,
            Glb,
            Fbx,
        }
        public struct FMeshResult
        {
            public EMeshType Type;
            public Support.TtBlobObject MeshData;
        }
        public abstract void Initialize(string appId, string apiKey);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateMeshFromImage(byte[] imageBytes);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateMeshFromText(string description);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateAnimationFromVideo(byte[] skeletalBytes, byte[] videoBytes);
        public abstract Thread.Async.TtTask<FMeshResult> GenerateAnimationFromText(byte[] skeletalBytes, string description);

        public static TtAIGCPlugin FindAIGCPlugin(string pluginName = "TencentAIGC")
        {
            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule(pluginName);
            if (serverPlugin != null)
            {
                return serverPlugin.GetPluginObject<TtAIGCPlugin>();
            }
            return null;
        }
    }

    /// <summary>
    /// Marks a static method as an MCP tool that can be invoked by an agent.
    /// The method must be static and belong to a class derived from TtMCPPlugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TtMCPTool : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReturnDescription { get; set; }
        public TtMCPTool() { }
        public TtMCPTool(string name, string description = "", string returnDescription = "")
        {
            Name = name;
            Description = description;
            ReturnDescription = returnDescription;
        }
    }

    /// <summary>
    /// Marks a method parameter with a description for MCP tool documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TtMCPParameter : Attribute
    {
        public string Description { get; set; }
        public TtMCPParameter(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Describes a single MCP tool definition exposed by the server.
    /// </summary>
    public class TtMCPToolDefinition
    {
        public string Name;
        public string Description;
        public string ReturnDescription;
        public List<TtMCPToolParameter> Parameters = new List<TtMCPToolParameter>();
    }

    /// <summary>
    /// Describes a parameter of an MCP tool.
    /// </summary>
    public class TtMCPToolParameter
    {
        public string Name;
        public string Type;
        public string Description;
        public bool Required;
    }

    /// <summary>
    /// Represents the result of an MCP tool invocation.
    /// </summary>
    public struct FMCPToolResult
    {
        public bool Success;
        public string Content;
        public string ErrorMessage;

        public static FMCPToolResult Ok(string content)
        {
            return new FMCPToolResult { Success = true, Content = content };
        }
        public static FMCPToolResult Fail(string error)
        {
            return new FMCPToolResult { Success = false, ErrorMessage = error };
        }
    }

    [IO.TtConfig(Path = "mcp.jscfg")]
    public class TtMCPConfig : IO.IConfig
    {
        [Rtti.Meta("")]
        public int Port { get; set; } = 8818;
        [Rtti.Meta("")]
        public bool AutoStart { get; set; } = true;
        [Rtti.Meta("")]
        public string ServerName { get; set; } = "TitanEngineMCPServer";
        [Rtti.Meta("")]
        public int HeartbeatIntervalMs { get; set; } = 15000;
    }

    /// <summary>
    /// Base class for MCP Server plugins. Provides lifecycle management,
    /// tool registration via reflection, and tool invocation for agent integration.
    /// </summary>
    public abstract class TtMCPPlugin : AssemblyLoader.IPlugin
    {
        protected string ServerName = "TitanEngineMCPServer";
        protected string ServerVersion = "1.0.0";
        protected int ServerPort = 8818;
        protected bool IsRunning = false;

        private TtMCPConfig mMCPConfig = null;
        public TtMCPConfig MCPConfig
        {
            get
            {
                if (mMCPConfig == null)
                {
                    mMCPConfig = TtEngine.Instance.ConfigManager.GetConfig<TtMCPConfig>();
                }
                return mMCPConfig;
            }
        }

        private Dictionary<string, System.Reflection.MethodInfo> mRegisteredTools = new Dictionary<string, System.Reflection.MethodInfo>();

        public virtual void OnLoadedPlugin()
        {
            CollectTools();

            var config = MCPConfig;
            if (config != null)
            {
                ServerName = config.ServerName;
                ServerPort = config.Port;
                if (config.AutoStart)
                {
                    StartServer(config.Port);
                }
            }
        }
        public virtual void OnUnloadPlugin()
        {
            StopServer();
        }

        /// <summary>
        /// Starts the MCP server. Override to provide custom server startup logic.
        /// </summary>
        public virtual bool StartServer(int port = 0)
        {
            if (port > 0)
                ServerPort = port;
            IsRunning = true;
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                $"MCP Server '{ServerName}' v{ServerVersion} started on port {ServerPort}");
            return true;
        }

        /// <summary>
        /// Stops the MCP server. Override to provide custom server shutdown logic.
        /// </summary>
        public virtual void StopServer()
        {
            IsRunning = false;
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                $"MCP Server '{ServerName}' stopped");
        }

        /// <summary>
        /// Lists all registered MCP tool definitions.
        /// </summary>
        public virtual List<TtMCPToolDefinition> ListTools()
        {
            var result = new List<TtMCPToolDefinition>();
            foreach (var kv in mRegisteredTools)
            {
                var method = kv.Value;
                var attr = method.GetCustomAttribute<TtMCPTool>();
                var definition = new TtMCPToolDefinition();
                definition.Name = string.IsNullOrEmpty(attr.Name) ? method.Name : attr.Name;
                definition.Description = attr.Description ?? "";
                definition.ReturnDescription = attr.ReturnDescription ?? "";
                foreach (var param in method.GetParameters())
                {
                    var paramAttr = param.GetCustomAttribute<TtMCPParameter>();
                    definition.Parameters.Add(new TtMCPToolParameter
                    {
                        Name = param.Name,
                        Type = param.ParameterType.Name,
                        Description = paramAttr?.Description ?? "",
                        Required = !param.HasDefaultValue,
                    });
                }
                result.Add(definition);
            }
            return result;
        }

        /// <summary>
        /// Invokes a registered MCP tool by name with the given arguments.
        /// </summary>
        public virtual FMCPToolResult InvokeTool(string toolName, Dictionary<string, object> arguments)
        {
            if (!mRegisteredTools.TryGetValue(toolName, out var method))
            {
                return FMCPToolResult.Fail($"Tool '{toolName}' not found");
            }

            try
            {
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramInfo = parameters[i];
                    if (arguments != null && arguments.TryGetValue(paramInfo.Name, out var value))
                    {
                        args[i] = Convert.ChangeType(value, paramInfo.ParameterType);
                    }
                    else if (paramInfo.HasDefaultValue)
                    {
                        args[i] = paramInfo.DefaultValue;
                    }
                    else
                    {
                        return FMCPToolResult.Fail($"Missing required parameter '{paramInfo.Name}' for tool '{toolName}'");
                    }
                }

                var returnValue = method.Invoke(null, args);
                var content = returnValue?.ToString() ?? "null";
                return FMCPToolResult.Ok(content);
            }
            catch (Exception ex)
            {
                return FMCPToolResult.Fail($"Tool '{toolName}' execution failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles a raw JSON-RPC style request string. Override for custom protocol handling.
        /// </summary>
        public virtual string HandleRequest(string jsonRequest)
        {
            return "{}";
        }

        /// <summary>
        /// Scans the current class hierarchy for static methods marked with [TtMCPTool]
        /// and registers them as invocable tools.
        /// </summary>
        protected void CollectTools()
        {
            mRegisteredTools.Clear();
            var type = this.GetType();
            var methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<TtMCPTool>();
                if (attr != null)
                {
                    var toolName = string.IsNullOrEmpty(attr.Name) ? method.Name : attr.Name;
                    mRegisteredTools[toolName] = method;
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                        $"MCP Tool registered: {toolName}");
                }
            }
        }

        /// <summary>
        /// Finds an MCP plugin instance by name from the engine's plugin module manager.
        /// </summary>
        public static TtMCPPlugin FindMCPPlugin(string pluginName = "MCPServer")
        {
            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule(pluginName);
            if (serverPlugin != null)
            {
                return serverPlugin.GetPluginObject<TtMCPPlugin>();
            }
            return null;
        }
    }
}

namespace EngineNS.UnitTest
{
    [UnitTest.TtTest(Enable = false)]
    public class TtTest_AIGCPlugin
    {
        public void UnitTestEntrance()
        {
            var action = async () =>
            {
                var plugin = Bricks.AIGC.TtAIGCPlugin.FindAIGCPlugin();
                if (plugin!=null)
                {
                    var root = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.PluginSource);
                    var text = IO.TtFileManager.ReadAllText(root + "/AIGC/TencentAIGC/AppKey.txt");
                    if (text!=null)
                    {
                        var segs = text.Split(';');
                        plugin.Initialize(segs[0], segs[1]);
                        var t = await plugin.GenerateMeshFromText("A flower");
                    }
                }
            };
            action();
        }
    }
}