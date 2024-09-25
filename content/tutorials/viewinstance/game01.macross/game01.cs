namespace NS_tutorials.viewinstance
{
    [EngineNS.Macross.TtMacross]
    public partial class game01 : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_2752037395 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_2752037395");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1838318065 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1838318065");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_2115264093 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/viewinstance/game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_2115264093))
            {
                System.Boolean ret_1749063466 = default(System.Boolean);
                mFrame_BeginPlay_2115264093.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_2752037395 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_mapName_InitViewportSlateWithScene_2752037395", EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_zMin_InitViewportSlateWithScene_2752037395", 0f);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_zMax_InitViewportSlateWithScene_2752037395", 1f);
                mFrame_BeginPlay_2115264093.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_2752037395", true);
                breaker_InitViewportSlateWithScene_2752037395.TryBreak();
                tmp_r_InitViewportSlateWithScene_2752037395 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_2115264093.SetWatchVariable("tmp_r_InitViewportSlateWithScene_2752037395", tmp_r_InitViewportSlateWithScene_2752037395);
                ret_1749063466 = true;
                mFrame_BeginPlay_2115264093.SetWatchVariable("ret_1749063466_1838318065", ret_1749063466);
                breaker_return_1838318065.TryBreak();
                return ret_1749063466;
                return ret_1749063466;
            }
        }
    }
}
