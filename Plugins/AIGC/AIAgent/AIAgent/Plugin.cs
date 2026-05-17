using System;
using EngineNS.IO;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class TtAIAgentAssemblyDesc : TtAssemblyDesc
        {
            public TtAIAgentAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:AIAgent AssemblyDesc Created");
            }
            ~TtAIAgentAssemblyDesc()
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info, "Plugins:AIAgent AssemblyDesc Destroyed");
            }
            public override string Name { get => "AIAgent"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static TtAIAgentAssemblyDesc AssmblyDesc = new TtAIAgentAssemblyDesc();
        public static TtAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

namespace EngineNS.Plugins.AIAgent
{
    [Bricks.AssemblyLoader.TtPlugin]
    public class TtPluginLoader
    {
        public static TtAIAgentPlugin mPluginObject = new TtAIAgentPlugin();
        public static Bricks.AssemblyLoader.IPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }

    public class TtAIAgentPlugin : Bricks.AssemblyLoader.IPlugin, Bricks.AssemblyLoader.IPluginMenu
    {
        private TtAIAgent mAgent;
        private TtAIChatPanel mChatPanel;

        public TtAIAgent Agent => mAgent;
        public TtAIChatPanel ChatPanel => mChatPanel;

        #region IPlugin

        public void OnLoadedPlugin()
        {
            mAgent = new TtAIAgent();
            mAgent.Initialize();

            mChatPanel = new TtAIChatPanel();
            mChatPanel.Initialize(this);

            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
            mainEditor?.PluginMenuManager.RegisterMenuProvider(this);

            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                "AIAgent plugin loaded");
        }

        public void OnUnloadPlugin()
        {
            var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.TtMainEditorApplication;
            mainEditor?.PluginMenuManager.UnregisterMenuProvider(this);

            mChatPanel?.Dispose();
            mChatPanel = null;
            mAgent?.Dispose();
            mAgent = null;
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                "AIAgent plugin unloaded");
        }

        #endregion

        #region IPluginMenu

        public string PluginMenuName => "AI Agent";

        public unsafe void OnDrawPluginMenu()
        {
            if (mChatPanel != null)
            {
                bool chatVisible = mChatPanel.Visible;
                if (ImGuiAPI.MenuItem("Chat Panel", null, chatVisible, true))
                {
                    mChatPanel.Visible = !chatVisible;
                }
            }

            if (mAgent != null)
            {
                ImGuiAPI.Separator();
                var configLabel = mAgent.IsConfigured ? "Configured" : "Not Configured";
                var statusColor = mAgent.IsConfigured
                    ? new Vector4(0.4f, 1.0f, 0.4f, 1.0f)
                    : new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
                ImGuiAPI.TextColored(in statusColor, configLabel);
            }
        }

        #endregion

        /// <summary>
        /// Convenience accessor: find the AIAgent plugin from anywhere in the engine.
        /// </summary>
        public static TtAIAgentPlugin FindPlugin()
        {
            var module = TtEngine.Instance.PluginModuleManager.GetPluginModule("AIAgent");
            return module?.GetPluginObject<TtAIAgentPlugin>();
        }
    }
}
