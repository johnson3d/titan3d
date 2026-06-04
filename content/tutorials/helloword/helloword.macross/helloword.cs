namespace NS_tutorials.helloword
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/helloword/helloword.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class helloword : EngineNS.GamePlay.TtMacrossGame
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_121615181 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_121615181");
        public static EngineNS.Macross.TtMacrossBreak breaker_Load_3720817156 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_3720817156");
        public static EngineNS.Macross.TtMacrossBreak breaker_PushHUD_1721239009 = new EngineNS.Macross.TtMacrossBreak("breaker_PushHUD_1721239009");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_1164477446 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1164477446");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/helloword.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_ee0b9aa1_a1d5_4d8f_83c9_b708c4e9ddb9 && !disable_ui_e1520501_a052_4abe_a7d6_e9cfdf41e8e0
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_2105066555 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_121615181 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_3720817156 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_121615181", EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_121615181", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_121615181", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_121615181", true);
                breaker_InitViewportSlateWithScene_121615181.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_InitViewportSlateWithScene_121615181 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/helloword/map01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_121615181", tmp_r_InitViewportSlateWithScene_121615181);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_Load_3720817156", EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_3720817156.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_Load_3720817156 = EngineNS.TtEngine.Instance.UIManager.Load(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_Load_3720817156", tmp_r_Load_3720817156);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_hud_PushHUD_1721239009", tmp_r_Load_3720817156);
                breaker_PushHUD_1721239009.TryBreak(mStack_BeginPlay_1342966456, this);
                host.WorldViewportSlate.PushHUD(tmp_r_Load_3720817156);
                ret_2105066555 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_2105066555_1164477446", ret_2105066555);
                breaker_return_1164477446.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_2105066555;
            }
            #elif !(!disable_macross_ee0b9aa1_a1d5_4d8f_83c9_b708c4e9ddb9 && !disable_ui_e1520501_a052_4abe_a7d6_e9cfdf41e8e0)
            System.Boolean ret_2105066555 = default(System.Boolean);
            return ret_2105066555;
            #endif //!disable_macross_ee0b9aa1_a1d5_4d8f_83c9_b708c4e9ddb9 && !disable_ui_e1520501_a052_4abe_a7d6_e9cfdf41e8e0
        }
    }
}
