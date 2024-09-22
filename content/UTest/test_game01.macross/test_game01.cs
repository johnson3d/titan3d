namespace NS_utest
{
    [EngineNS.Macross.TtMacross]
    public partial class test_game01 : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_1573568709 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_1573568709");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_2813021583 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_2813021583");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacter_1172710680 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacter_1172710680");
        public EngineNS.Macross.TtMacrossBreak breaker_return_547057334 = new EngineNS.Macross.TtMacrossBreak("breaker_return_547057334");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        public override void BeginDestroy(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy))
            {
                mFrame_BeginDestroy.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_1573568709.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_2822948834 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_2813021583 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_InitViewportSlateWithScene_2813021583", EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlateWithScene_2813021583", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlateWithScene_2813021583", 1f);
                mFrame_BeginPlay.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_2813021583", true);
                breaker_InitViewportSlateWithScene_2813021583.TryBreak();
                tmp_r_InitViewportSlateWithScene_2813021583 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay.SetWatchVariable("tmp_r_InitViewportSlateWithScene_2813021583", tmp_r_InitViewportSlateWithScene_2813021583);
                mFrame_BeginPlay.SetWatchVariable("v_scene_CreateCharacter_1172710680", tmp_r_InitViewportSlateWithScene_2813021583);
                breaker_CreateCharacter_1172710680.TryBreak();
                await host.CreateCharacter(tmp_r_InitViewportSlateWithScene_2813021583);
                ret_2822948834 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_2822948834_547057334", ret_2822948834);
                breaker_return_547057334.TryBreak();
                return ret_2822948834;
                return ret_2822948834;
            }
        }
    }
}
