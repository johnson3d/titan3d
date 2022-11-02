using EngineNS.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Plugin
{
    public class UPluginLoader
    {
        public static Plugins.GameItem.UGameItemPlugin? mPluginObject = new Plugins.GameItem.UGameItemPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
}

namespace EngineNS.Plugins.GameItem
{
    public class UGameItemPlugin : Bricks.AssemblyLoader.UPlugin
    {
        public override void OnLoadedPlugin()
        {

        }
        public override void OnUnloadPlugin()
        {
            //UPluginDescriptor.mPluginObject = null;
        }
    }

    public class AGameItem
    {
    }
}
