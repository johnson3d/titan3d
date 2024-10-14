namespace NS_utest
{
    [EngineNS.Macross.TtMacross]
    public partial class test_game01 : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_1573568709 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_1573568709");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_2813021583 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_2813021583");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacter_1172710680 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacter_1172710680");
        public EngineNS.Macross.TtMacrossBreak breaker_return_547057334 = new EngineNS.Macross.TtMacrossBreak("breaker_return_547057334");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2487905154 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/test_game01.macross", EngineNS.RName.ERNameType.Game));
        public override void BeginDestroy(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2487905154))
            {
                mFrame_BeginDestroy_2487905154.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_1573568709.TryBreak();
                host.FinalViewportSlate();
            }
        }
    }
}
