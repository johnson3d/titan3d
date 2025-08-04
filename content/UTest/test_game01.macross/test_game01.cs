namespace NS_utest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "utest/test_game01.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class test_game01 : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_45717255 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_45717255");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_bc3c0fba_c5f7_4c79_ad88_7c09531cb90b
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_45717255.TryBreak();
                host.FinalViewportSlate();
            }
            #endif //!disable_macross_bc3c0fba_c5f7_4c79_ad88_7c09531cb90b
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            #if !disable_macross_bc3c0fba_c5f7_4c79_ad88_7c09531cb90b
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_3886106516 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                return ret_3886106516;
            }
            #elif !(!disable_macross_bc3c0fba_c5f7_4c79_ad88_7c09531cb90b)
            System.Boolean ret_3886106516 = default(System.Boolean);
            return ret_3886106516;
            #endif //!disable_macross_bc3c0fba_c5f7_4c79_ad88_7c09531cb90b
        }
    }
}
