namespace NS_tutorials.viewinstance
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/viewinstance/game01.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class game01 : EngineNS.GamePlay.TtMacrossGame
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_2448865448 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_2448865448");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_4131849795 = new EngineNS.Macross.TtMacrossBreak("breaker_return_4131849795");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/viewinstance/game01.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_2313039c_2bda_4cfa_9953_e4e56363704b
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_4215747355 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_2448865448 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_2448865448", EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_2448865448", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_2448865448", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_2448865448", true);
                breaker_InitViewportSlateWithScene_2448865448.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_InitViewportSlateWithScene_2448865448 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_2448865448", tmp_r_InitViewportSlateWithScene_2448865448);
                ret_4215747355 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_4215747355_4131849795", ret_4215747355);
                breaker_return_4131849795.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_4215747355;
            }
            #elif !(!disable_macross_2313039c_2bda_4cfa_9953_e4e56363704b)
            System.Boolean ret_4215747355 = default(System.Boolean);
            return ret_4215747355;
            #endif //!disable_macross_2313039c_2bda_4cfa_9953_e4e56363704b
        }
    }
}
