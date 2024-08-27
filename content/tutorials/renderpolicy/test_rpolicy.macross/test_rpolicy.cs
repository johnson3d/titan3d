namespace NS_tutorials.renderpolicy
{
    [EngineNS.Macross.UMacross]
    public partial class test_rpolicy : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlateWithScene_730474887 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlateWithScene_730474887");
        public EngineNS.Macross.UMacrossBreak breaker_return_1522025194 = new EngineNS.Macross.UMacrossBreak("breaker_return_1522025194");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/renderpolicy/test_rpolicy.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_3675910725 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.UScene tmp_r_InitViewportSlateWithScene_730474887 = default(EngineNS.GamePlay.Scene.UScene);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_InitViewportSlateWithScene_730474887", EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlateWithScene_730474887", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlateWithScene_730474887", 1f);
                mFrame_BeginPlay.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_730474887", true);
                breaker_InitViewportSlateWithScene_730474887.TryBreak();
                tmp_r_InitViewportSlateWithScene_730474887 = (EngineNS.GamePlay.Scene.UScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay.SetWatchVariable("tmp_r_InitViewportSlateWithScene_730474887", tmp_r_InitViewportSlateWithScene_730474887);
                ret_3675910725 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_3675910725_1522025194", ret_3675910725);
                breaker_return_1522025194.TryBreak();
                return ret_3675910725;
                return ret_3675910725;
            }
        }
    }
}
