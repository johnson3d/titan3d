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

        // 使用外部轻量编辑器打开并跳转：优先 VSCode，未安装时回退到 notepad（仅打开文件）。
        // 与 OpenFileAtLine（Visual Studio）区分为两个独立的跳转操作。
        public virtual bool OpenFileAtLineExternal(string filePath, int lineNumber) => false;

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
