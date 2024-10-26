namespace NS_tutorials.viewinstance
{
    [EngineNS.Macross.TtMacross]
    public partial class game01 : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_961187540 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_961187540");
        public EngineNS.Macross.TtMacrossBreak breaker_return_4131849795 = new EngineNS.Macross.TtMacrossBreak("breaker_return_4131849795");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/viewinstance/game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_4215747355 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_961187540 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_961187540", EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_961187540", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_961187540", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_961187540", true);
                breaker_InitViewportSlateWithScene_961187540.TryBreak();
                tmp_r_InitViewportSlateWithScene_961187540 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_961187540", tmp_r_InitViewportSlateWithScene_961187540);
                ret_4215747355 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_4215747355_4131849795", ret_4215747355);
                breaker_return_4131849795.TryBreak();
                return ret_4215747355;
                return ret_4215747355;
            }
        }
    }
}
