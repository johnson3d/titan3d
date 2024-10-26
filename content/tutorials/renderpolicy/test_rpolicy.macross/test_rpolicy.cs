namespace NS_tutorials.renderpolicy
{
    [EngineNS.Macross.TtMacross]
    public partial class test_rpolicy : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_2166288321 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_2166288321");
        public EngineNS.Macross.TtMacrossBreak breaker_return_3904048649 = new EngineNS.Macross.TtMacrossBreak("breaker_return_3904048649");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/renderpolicy/test_rpolicy.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_800416013 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_2166288321 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_2166288321", EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_2166288321", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_2166288321", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_2166288321", true);
                breaker_InitViewportSlateWithScene_2166288321.TryBreak();
                tmp_r_InitViewportSlateWithScene_2166288321 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_2166288321", tmp_r_InitViewportSlateWithScene_2166288321);
                ret_800416013 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_800416013_3904048649", ret_800416013);
                breaker_return_3904048649.TryBreak();
                return ret_800416013;
                return ret_800416013;
            }
        }
    }
}
