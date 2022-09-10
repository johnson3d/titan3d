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
        public string GameProjectPath 
        {
            get
            {
                if (string.IsNullOrEmpty(GameProject))
                    return "";
                return IO.FileManager.GetBaseDirectory(GameProject);
            }
        }
        [Rtti.Meta]
        public string GameAssembly { get; set; }
        [Rtti.Meta]
        public RName PhyMaterialIconName { get; set; }
        [Rtti.Meta]
        public RName MacrossIconName { get; set; }
        //[Rtti.Meta]
        //public GamePlay.UWorld.UVisParameter.EVisCullFilter CullFilters { get; set; } = GamePlay.UWorld.UVisParameter.EVisCullFilter.All;
        //public bool IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter filters)
        //{
        //    return (CullFilters & filters) == filters;
        //}
    }

    public partial class UEditor : UModule<UEngine>
    {
        public UEditorConfig Config { get; set; } = new UEditorConfig();
        public EGui.UUvAnim PhyMaterialIcon { get; set; }
        public EGui.UUvAnim MacrossIcon { get; set; }
        public override void Cleanup(UEngine host)
        {
            RNamePopupContentBrowser?.Cleanup();
            RNamePopupContentBrowser = null;
            base.Cleanup(host);
        }

        public override async Task<bool> Initialize(UEngine host)
        {
            var cfgFile = host.FileManager.GetRoot(IO.FileManager.ERootDir.Editor) + "EditorConfig.cfg";
            Config = IO.FileManager.LoadXmlToObject<UEditorConfig>(cfgFile);
            if (Config == null)
            {
                Config = new UEditorConfig();
                Config.GameProject = "Module/GameProject/GameProject.csproj";
                Config.GameAssembly = "binaries/net5.0/GameProject.dll";
                Config.PhyMaterialIconName = RName.GetRName("icons/phymaterialicon.uvanim", RName.ERNameType.Engine);
                IO.FileManager.SaveObjectToXml(cfgFile, Config);
            }

            var gameAssembly = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Root) + Config.GameAssembly;
            
            UEngine.Instance.MacrossModule.ReloadAssembly(gameAssembly);

            await RNamePopupContentBrowser.Initialize();

            return await base.Initialize(host);
        }
        public override async Task<bool> PostInitialize(UEngine host)
        {
            if (Config.PhyMaterialIconName == null)
            {
                Config.PhyMaterialIconName = RName.GetRName("icons/phymaterialicon.uvanim", RName.ERNameType.Engine);
            }
            PhyMaterialIcon = await UEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(Config.PhyMaterialIconName);

            if (Config.MacrossIconName == null)
            {
                Config.MacrossIconName = RName.GetRName("icons/macrossicon.uvanim", RName.ERNameType.Engine);
            }
            MacrossIcon = await UEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(Config.MacrossIconName);

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
