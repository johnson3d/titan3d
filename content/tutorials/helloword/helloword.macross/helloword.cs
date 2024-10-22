namespace NS_tutorials.helloword
{
    [EngineNS.Macross.TtMacross]
    public partial class helloword : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_3402544262 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_3402544262");
        public EngineNS.Macross.TtMacrossBreak breaker_Load_2325772087 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_2325772087");
        public EngineNS.Macross.TtMacrossBreak breaker_Add_1937078671 = new EngineNS.Macross.TtMacrossBreak("breaker_Add_1937078671");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1529912777 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1529912777");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_2115264093 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/helloword.macross", EngineNS.RName.ERNameType.Game));
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_2115264093))
            {
                System.Boolean ret_4129711059 = default(System.Boolean);
                mFrame_BeginPlay_2115264093.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_3402544262 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_2325772087 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_mapName_InitViewportSlateWithScene_3402544262", EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_zMin_InitViewportSlateWithScene_3402544262", 0f);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_zMax_InitViewportSlateWithScene_3402544262", 1f);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_3402544262", false);
                breaker_InitViewportSlateWithScene_3402544262.TryBreak();
                tmp_r_InitViewportSlateWithScene_3402544262 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game),0f,1f,false);
                mFrame_BeginPlay_2115264093.SetWatchVariable("tmp_r_InitViewportSlateWithScene_3402544262", tmp_r_InitViewportSlateWithScene_3402544262);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_name_Load_2325772087", EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_2325772087.TryBreak();
                tmp_r_Load_2325772087 = EngineNS.TtEngine.Instance.UIManager.Load(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_2115264093.SetWatchVariable("tmp_r_Load_2325772087", tmp_r_Load_2325772087);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_item_Add_1937078671", tmp_r_Load_2325772087);
                breaker_Add_1937078671.TryBreak();
                host.WorldViewportSlate.DefaultHUD.Children.Add(tmp_r_Load_2325772087);
                ret_4129711059 = true;
                mFrame_BeginPlay_2115264093.SetWatchVariable("ret_4129711059_1529912777", ret_4129711059);
                breaker_return_1529912777.TryBreak();
                return ret_4129711059;
                return ret_4129711059;
            }
        }
    }
}
