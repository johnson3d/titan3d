namespace NS_tutorials.renderpolicy
{
    [EngineNS.Macross.TtMacross]
    public partial class test_rpolicy : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_3545170604 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_3545170604");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1522025194 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1522025194");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/renderpolicy/test_rpolicy.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_3675910725 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_3545170604 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_InitViewportSlateWithScene_3545170604", EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlateWithScene_3545170604", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlateWithScene_3545170604", 1f);
                mFrame_BeginPlay.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_3545170604", true);
                breaker_InitViewportSlateWithScene_3545170604.TryBreak();
                tmp_r_InitViewportSlateWithScene_3545170604 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay.SetWatchVariable("tmp_r_InitViewportSlateWithScene_3545170604", tmp_r_InitViewportSlateWithScene_3545170604);
                ret_3675910725 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_3675910725_1522025194", ret_3675910725);
                breaker_return_1522025194.TryBreak();
                return ret_3675910725;
                return ret_3675910725;
            }
        }
    }
}
