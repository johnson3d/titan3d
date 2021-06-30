using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    [Rtti.Meta]
    public partial class UEditorConfig
    {
        [Rtti.Meta]
        public string GameProject { get; set; }
    }

    public partial class UEditor : UModule<UEngine>
    {
        public UEditorConfig Config { get; set; } = new UEditorConfig();

        public override async Task<bool> Initialize(UEngine host)
        {
            var cfgFile = host.FileManager.GetRoot(IO.FileManager.ERootDir.Editor) + "EditorConfig.cfg";
            Config = IO.FileManager.LoadXmlToObject<UEditorConfig>(cfgFile);
            if(Config == null)
            {
                Config = new UEditorConfig();
                Config.GameProject = "Module/GameProject/GameProject.csproj";
                IO.FileManager.SaveObjectToXml(cfgFile, Config);
            }

            return await base.Initialize(host);
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        public Editor.UEditor EditorInstance { get; } = new Editor.UEditor();
    }

}
