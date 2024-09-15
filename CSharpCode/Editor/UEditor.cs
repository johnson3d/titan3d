using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor
{
    [Rtti.Meta]
    public partial class TtEditorConfig
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
        [Rtti.Meta]
        public Color4b PrefabBoderColor { get; set; } = Color4b.DeepPink;
        [Rtti.Meta]
        public Color4b SceneBoderColor { get; set; } = Color4b.Crimson;
        [Rtti.Meta]
        public Color4b RenderPolicyBoderColor { get; set; } = Color4b.Coral;
        [Rtti.Meta]
        public Color4b FontSDFBoderColor { get; set; } = Color4b.LightGray;
        [Rtti.Meta]
        public Color4b NebulaBoderColor { get; set; } = Color4b.HotPink;
        [Rtti.Meta]
        public Color4b MaterialMeshBoderColor { get; set; } = Color4b.OrangeRed;
        [Rtti.Meta]
        public Color4b MeshPrimitivesBoderColor { get; set; } = Color4b.LightYellow;
        [Rtti.Meta]
        public Color4b MaterialBoderColor { get; set; } = Color4b.Gold;
        [Rtti.Meta]
        public Color4b MaterialInstanceBoderColor { get; set; } = Color4b.Cyan;
        [Rtti.Meta]
        public Color4b TextureBoderColor { get; set; } = Color4b.LightPink;
        [Rtti.Meta]
        public Color4b PgcBoderColor { get; set; } = Color4b.Khaki;
    }

    public partial class TtEditor : TtModule<TtEngine>
    {
        public TtEditorConfig Config { get; set; } = new TtEditorConfig();
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
            Config = IO.TtFileManager.LoadXmlToObject<TtEditorConfig>(cfgFile);
            if (Config == null)
            {
                Config = new TtEditorConfig();
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
        public Editor.TtEditor EditorInstance { get; } = new Editor.TtEditor();
    }

}
