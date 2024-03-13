namespace NS_tutorials.renderpolicy
{
    [EngineNS.Macross.UMacross]
    public partial class test_rpolicy : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlate_1614313205 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlate_1614313205");
        public EngineNS.Macross.UMacrossBreak breaker_LoadScene_716616573 = new EngineNS.Macross.UMacrossBreak("breaker_LoadScene_716616573");
        public EngineNS.Macross.UMacrossBreak breaker_return_1522025194 = new EngineNS.Macross.UMacrossBreak("breaker_return_1522025194");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/renderpolicy/test_rpolicy.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_3675910725 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.UScene tmp_r_LoadScene_716616573 = default(EngineNS.GamePlay.Scene.UScene);
                mFrame_BeginPlay.SetWatchVariable("v_rPolicy_InitViewportSlate_1614313205", EngineNS.RName.GetRName("tutorials/renderpolicy/simple_ds.rpolicy", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlate_1614313205", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlate_1614313205", 0f);
                breaker_InitViewportSlate_1614313205.TryBreak();
                await host.InitViewportSlate(EngineNS.RName.GetRName("tutorials/renderpolicy/simple_ds.rpolicy", EngineNS.RName.ERNameType.Game),0f,0f);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_LoadScene_716616573", EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game));
                breaker_LoadScene_716616573.TryBreak();
                tmp_r_LoadScene_716616573 = (EngineNS.GamePlay.Scene.UScene)await host.LoadScene(EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("tmp_r_LoadScene_716616573", tmp_r_LoadScene_716616573);
                ret_3675910725 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_3675910725_1522025194", ret_3675910725);
                breaker_return_1522025194.TryBreak();
                return ret_3675910725;
            }
        }
    }
}
