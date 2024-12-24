namespace NS_tutorials.helloword
{
    [EngineNS.Macross.TtMacross]
    public partial class helloword : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_3867568268 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_3867568268");
        public EngineNS.Macross.TtMacrossBreak breaker_Load_3720817156 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_3720817156");
        public EngineNS.Macross.TtMacrossBreak breaker_PushHUD_1721239009 = new EngineNS.Macross.TtMacrossBreak("breaker_PushHUD_1721239009");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1164477446 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1164477446");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/helloword.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_2105066555 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_3867568268 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_3720817156 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_3867568268", EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_3867568268", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_3867568268", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_3867568268", true);
                breaker_InitViewportSlateWithScene_3867568268.TryBreak();
                tmp_r_InitViewportSlateWithScene_3867568268 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_3867568268", tmp_r_InitViewportSlateWithScene_3867568268);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_Load_3720817156", EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_3720817156.TryBreak();
                tmp_r_Load_3720817156 = EngineNS.TtEngine.Instance.UIManager.Load(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_Load_3720817156", tmp_r_Load_3720817156);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_hud_PushHUD_1721239009", tmp_r_Load_3720817156);
                breaker_PushHUD_1721239009.TryBreak();
                host.WorldViewportSlate.PushHUD(tmp_r_Load_3720817156);
                ret_2105066555 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_2105066555_1164477446", ret_2105066555);
                breaker_return_1164477446.TryBreak();
                return ret_2105066555;
            }
        }
    }
}
