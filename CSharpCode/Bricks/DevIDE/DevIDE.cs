using EngineNS.Bricks.AIGC;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.DevIDE
{
    public abstract class TtDevIDEPlugin : AssemblyLoader.IPlugin
    {
        public virtual void OnLoadedPlugin()
        {

        }
        public virtual void OnUnloadPlugin()
        {

        }
        public abstract bool OpenFileAtLine(string filePath, int lineNumber);

        public static TtDevIDEPlugin FindDevIDEPlugin(string pluginName = "VisualStudioPlugin")
        {
            var serverPlugin = TtEngine.Instance.PluginModuleManager.GetPluginModule(pluginName);
            if (serverPlugin != null)
            {
                return serverPlugin.GetPluginObject<TtDevIDEPlugin>();
            }
            return null;
        }
    }
}
