namespace NS_tutorials.viewinstance
{
    [EngineNS.Macross.UMacross]
    public partial class game01 : EngineNS.GamePlay.UMacrossGame
    {
        public EngineNS.Macross.UMacrossBreak breaker_InitViewportSlateWithScene_2483316995 = new EngineNS.Macross.UMacrossBreak("breaker_InitViewportSlateWithScene_2483316995");
        public EngineNS.Macross.UMacrossBreak breaker_return_1838318065 = new EngineNS.Macross.UMacrossBreak("breaker_return_1838318065");
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/viewinstance/game01.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                System.Boolean ret_1749063466 = default(System.Boolean);
                mFrame_BeginPlay.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.UScene tmp_r_InitViewportSlateWithScene_2483316995 = default(EngineNS.GamePlay.Scene.UScene);
                mFrame_BeginPlay.SetWatchVariable("v_mapName_InitViewportSlateWithScene_2483316995", EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay.SetWatchVariable("v_zMin_InitViewportSlateWithScene_2483316995", 0f);
                mFrame_BeginPlay.SetWatchVariable("v_zMax_InitViewportSlateWithScene_2483316995", 1f);
                mFrame_BeginPlay.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_2483316995", true);
                breaker_InitViewportSlateWithScene_2483316995.TryBreak();
                tmp_r_InitViewportSlateWithScene_2483316995 = (EngineNS.GamePlay.Scene.UScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/viewinstance/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay.SetWatchVariable("tmp_r_InitViewportSlateWithScene_2483316995", tmp_r_InitViewportSlateWithScene_2483316995);
                ret_1749063466 = true;
                mFrame_BeginPlay.SetWatchVariable("ret_1749063466_1838318065", ret_1749063466);
                breaker_return_1838318065.TryBreak();
                return ret_1749063466;
                return ret_1749063466;
            }
        }
    }
}
