namespace NS_tutorials.viewinstance
{
    [EngineNS.Macross.UMacross]
    public partial class game01 : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlate_4187562711 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlate_4187562711");
        public EngineNS.Macross.UMacrossBreak breaker_LoadScene_1426807147 = new EngineNS.Macross.UMacrossBreak("breaker_LoadScene_1426807147");
        public EngineNS.Macross.UMacrossBreak breaker_return_1838318065 = new EngineNS.Macross.UMacrossBreak("breaker_return_1838318065");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/viewinstance/game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_1749063466 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.UScene tmp_r_LoadScene_1426807147 = default(EngineNS.GamePlay.Scene.UScene);
                mFrame_BeginPlay.SetWatchVariable("v_rPolicy_InitViewportSlate_4187562711", EngineNS.RName.GetRName("tutorials/viewinstance/viewid.rpolicy", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlate_4187562711", 0.3f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlate_4187562711", 1000f);
                breaker_InitViewportSlate_4187562711.TryBreak();
                await host.InitViewportSlate(EngineNS.RName.GetRName("tutorials/viewinstance/viewid.rpolicy", EngineNS.RName.ERNameType.Game),0.3f,1000f);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_LoadScene_1426807147", EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game));
                breaker_LoadScene_1426807147.TryBreak();
                tmp_r_LoadScene_1426807147 = (EngineNS.GamePlay.Scene.UScene)await host.LoadScene(EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("tmp_r_LoadScene_1426807147", tmp_r_LoadScene_1426807147);
                ret_1749063466 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_1749063466_1838318065", ret_1749063466);
                breaker_return_1838318065.TryBreak();
                return ret_1749063466;
                return ret_1749063466;
            }
        }
    }
}
