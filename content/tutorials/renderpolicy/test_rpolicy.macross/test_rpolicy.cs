namespace NS_tutorials.renderpolicy
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/renderpolicy/test_rpolicy.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class test_rpolicy : EngineNS.GamePlay.TtMacrossGame
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_1907725321 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_1907725321");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_3904048649 = new EngineNS.Macross.TtMacrossBreak("breaker_return_3904048649");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/renderpolicy/test_rpolicy.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_37c3834e_f24f_471b_b4d4_2c621f5d304c
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_800416013 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_1907725321 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_1907725321", EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_1907725321", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_1907725321", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_1907725321", true);
                breaker_InitViewportSlateWithScene_1907725321.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_InitViewportSlateWithScene_1907725321 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/renderpolicy/test01.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_1907725321", tmp_r_InitViewportSlateWithScene_1907725321);
                ret_800416013 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_800416013_3904048649", ret_800416013);
                breaker_return_3904048649.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_800416013;
            }
            #elif !(!disable_macross_37c3834e_f24f_471b_b4d4_2c621f5d304c)
            System.Boolean ret_800416013 = default(System.Boolean);
            return ret_800416013;
            #endif //!disable_macross_37c3834e_f24f_471b_b4d4_2c621f5d304c
        }
    }
}
