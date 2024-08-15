using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    [Rtti.Meta]
    public partial class UEditorConfig
    {
        public void SaveConfig(string sltFile)
        {
            IO.TtFileManager.SaveObjectToXml(sltFile, this);
        }
        [Rtti.Meta]
        public string GameProject { get; set; }
        public string GameProjectPath 
        {
            get
            {
                if (string.IsNullOrEmpty(GameProject))
                    return "";
                return IO.TtFileManager.GetBaseDirectory(GameProject);
            }
        }
        [Rtti.Meta]
        public string GameModuleName { get; set; } = "GameProject.dll";
        public string GameAssembly 
        { 
            get
            {
                return $"binaries/{TtEngine.DotNetVersion}/{GameModuleName}";
            }
        }
        [Rtti.Meta]
        public RName PhyMaterialIconName { get; set; }
        [Rtti.Meta]
        public RName FontIconName { get; set; }
        [Rtti.Meta]
        public RName MacrossIconName { get; set; }
        //[Rtti.Meta]
        //public GamePlay.UWorld.UVisParameter.EVisCullFilter CullFilters { get; set; } = GamePlay.UWorld.UVisParameter.EVisCullFilter.All;
        //public bool IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter filters)
        //{
        //    return (CullFilters & filters) == filters;
        //}
    }

    public partial class UEditor : UModule<TtEngine>
    {
        public UEditorConfig Config { get; set; } = new UEditorConfig();
        public EGui.TtUVAnim PhyMaterialIcon { get; set; }
        public EGui.TtUVAnim FontIcon { get; set; }
        public EGui.TtUVAnim MacrossIcon { get; set; }
        public override void Cleanup(TtEngine host)
        {
            //CoreSDK.DisposeObject(ref RNamePopupContentBrowser);
            base.Cleanup(host);
        }

        public override async Task<bool> Initialize(TtEngine host)
        {
            var cfgFile = host.FileManager.GetRoot(IO.TtFileManager.ERootDir.Editor) + "EditorConfig.cfg";
            Config = IO.TtFileManager.LoadXmlToObject<UEditorConfig>(cfgFile);
            if (Config == null)
            {
                Config = new UEditorConfig();
                Config.GameProject = "Module/GameProject/GameProject.csproj";
                Config.PhyMaterialIconName = RName.GetRName("icons/phymaterialicon.uvanim", RName.ERNameType.Engine);
                Config.FontIconName = RName.GetRName("icons/font.uvanim", RName.ERNameType.Engine);
                Config.MacrossIconName = RName.GetRName("icons/macrossicon.uvanim", RName.ERNameType.Engine);
                Config.SaveConfig(cfgFile);
            }

            var gameAssembly = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + Config.GameAssembly;
            
            TtEngine.Instance.MacrossModule.ReloadAssembly(gameAssembly);

            //await RNamePopupContentBrowser.Initialize();

            return await base.Initialize(host);
        }
        public override async Task<bool> PostInitialize(TtEngine host)
        {
            if (Config.PhyMaterialIconName == null)
            {
                Config.PhyMaterialIconName = RName.GetRName("icons/phymaterialicon.uvanim", RName.ERNameType.Engine);
            }
            PhyMaterialIcon = await TtEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(Config.PhyMaterialIconName);

            if (Config.FontIconName == null)
            {
                Config.FontIconName = RName.GetRName("icons/font.uvanim", RName.ERNameType.Engine);
            }
            FontIcon = await TtEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(Config.FontIconName);

            if (Config.MacrossIconName == null)
            {
                Config.MacrossIconName = RName.GetRName("icons/macrossicon.uvanim", RName.ERNameType.Engine);
            }
            MacrossIcon = await TtEngine.Instance.GfxDevice.UvAnimManager.GetUVAnim(Config.MacrossIconName);

            return await base.Initialize(host);
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        public Editor.UEditor EditorInstance { get; } = new Editor.UEditor();
    }

}
