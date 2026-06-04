namespace NS_tutorials.character
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/character/charactertestgame.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class charactertestgame : EngineNS.GamePlay.TtMacrossGame
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_623502181 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_623502181");
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_1774414876 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_1774414876");
        public static EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefab_792651341 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefab_792651341");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_3547363734 = new EngineNS.Macross.TtMacrossBreak("breaker_return_3547363734");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/character/charactertestgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_33fd079b_722b_4ad2_8faf_a04f20cc9d2c
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginDestroy_2650419528,mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_623502181.TryBreak(mStack_BeginDestroy_2650419528, this);
                host.FinalViewportSlate();
            }
            #endif //!disable_macross_33fd079b_722b_4ad2_8faf_a04f20cc9d2c
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/character/charactertestgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_33fd079b_722b_4ad2_8faf_a04f20cc9d2c
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_2423809958 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_1774414876 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_1774414876", EngineNS.RName.GetRName("tutorials/character/charactertest.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_1774414876", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_1774414876", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_1774414876", true);
                breaker_InitViewportSlateWithScene_1774414876.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_InitViewportSlateWithScene_1774414876 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/character/charactertest.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_1774414876", tmp_r_InitViewportSlateWithScene_1774414876);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacterFromPrefab_792651341", tmp_r_InitViewportSlateWithScene_1774414876);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_prefabName_CreateCharacterFromPrefab_792651341", EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                breaker_CreateCharacterFromPrefab_792651341.TryBreak(mStack_BeginPlay_1342966456, this);
                await host.CreateCharacterFromPrefab(tmp_r_InitViewportSlateWithScene_1774414876,EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                ret_2423809958 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_2423809958_3547363734", ret_2423809958);
                breaker_return_3547363734.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_2423809958;
            }
            #elif !(!disable_macross_33fd079b_722b_4ad2_8faf_a04f20cc9d2c)
            System.Boolean ret_2423809958 = default(System.Boolean);
            return ret_2423809958;
            #endif //!disable_macross_33fd079b_722b_4ad2_8faf_a04f20cc9d2c
        }
    }
}
