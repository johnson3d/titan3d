namespace NS_utest
{
    [EngineNS.Macross.UMacross]
    public partial class test_game01 : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.UMacrossBreak breaker_FinalViewportSlate_1573568709 = new EngineNS.Macross.UMacrossBreak("breaker_FinalViewportSlate_1573568709");
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlate_2533298875 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlate_2533298875");
        public EngineNS.Macross.UMacrossBreak breaker_LoadScene_4192249908 = new EngineNS.Macross.UMacrossBreak("breaker_LoadScene_4192249908");
        public EngineNS.Macross.UMacrossBreak breaker_CreateCharacter_2336893621 = new EngineNS.Macross.UMacrossBreak("breaker_CreateCharacter_2336893621");
        public EngineNS.Macross.UMacrossBreak breaker_return_547057334 = new EngineNS.Macross.UMacrossBreak("breaker_return_547057334");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginDestroy = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        public override void BeginDestroy(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginDestroy))
            {
                mFrame_BeginDestroy.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_1573568709.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_2822948834 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.UScene tmp_r_LoadScene_4192249908 = default(EngineNS.GamePlay.Scene.UScene);
                mFrame_BeginPlay.SetWatchVariable("v_rPolicy_InitViewportSlate_2533298875", EngineNS.RName.GetRName("utest/deferred.rpolicy", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlate_2533298875", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlate_2533298875", 0f);
                breaker_InitViewportSlate_2533298875.TryBreak();
                await host.InitViewportSlate(EngineNS.RName.GetRName("utest/deferred.rpolicy", EngineNS.RName.ERNameType.Game),0f,0f);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_LoadScene_4192249908", EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game));
                breaker_LoadScene_4192249908.TryBreak();
                tmp_r_LoadScene_4192249908 = (EngineNS.GamePlay.Scene.UScene)await host.LoadScene(EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("tmp_r_LoadScene_4192249908", tmp_r_LoadScene_4192249908);
                mFrame_BeginPlay.SetWatchVariable("v_scene_CreateCharacter_2336893621", tmp_r_LoadScene_4192249908);
                breaker_CreateCharacter_2336893621.TryBreak();
                await host.CreateCharacter(tmp_r_LoadScene_4192249908);
                ret_2822948834 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_2822948834_547057334", ret_2822948834);
                breaker_return_547057334.TryBreak();
                return ret_2822948834;
                return ret_2822948834;
            }
        }
    }
}
