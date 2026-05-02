using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EngineNS.Plugins.MCPServer
{
    public partial class TtMCPServerPlugin
    {
        [Bricks.AIGC.TtMCPTool("get_engine_info", "Returns basic information about the Titan Engine instance",
            returnDescription: "{engine: string - Engine name, " +
            "platform: string - Current platform identifier}")]
        public static string GetEngineInfo()
        {
            return JsonSerializer.Serialize(new
            {
                engine = "TitanEngine",
                platform = TtEngine.Instance.CurrentPlatform.ToString()
            });
        }

        [Bricks.AIGC.TtMCPTool("get_plugin_list", "Returns a list of all loaded plugin modules in the engine",
            returnDescription: "{plugins: [{name: string - Plugin module name, " +
            "state: string - Current module state}]}")]
        public static string GetPluginList()
        {
            try
            {
                var manager = TtEngine.Instance.PluginModuleManager;
                var plugins = new List<object>();
                foreach (var kv in manager.PluginModules)
                {
                    plugins.Add(new { name = kv.Key, state = kv.Value.ModuleSate.ToString() });
                }
                return JsonSerializer.Serialize(new { plugins });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message });
            }
        }
    }
}
