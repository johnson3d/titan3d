namespace NS_utest
{
    [EngineNS.Macross.TtMacross]
    public partial class test_game01 : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_45717255 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_45717255");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_3421984778 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_3421984778");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacter_2682877719 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacter_2682877719");
        public EngineNS.Macross.TtMacrossBreak breaker_return_78211834 = new EngineNS.Macross.TtMacrossBreak("breaker_return_78211834");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_45717255.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_3886106516 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_3421984778 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_3421984778", EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_3421984778", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_3421984778", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_3421984778", true);
                breaker_InitViewportSlateWithScene_3421984778.TryBreak();
                tmp_r_InitViewportSlateWithScene_3421984778 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("utest/testscene.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_3421984778", tmp_r_InitViewportSlateWithScene_3421984778);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacter_2682877719", tmp_r_InitViewportSlateWithScene_3421984778);
                breaker_CreateCharacter_2682877719.TryBreak();
                await host.CreateCharacter(tmp_r_InitViewportSlateWithScene_3421984778);
                ret_3886106516 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_3886106516_78211834", ret_3886106516);
                breaker_return_78211834.TryBreak();
                return ret_3886106516;
                return ret_3886106516;
            }
        }
    }
}
