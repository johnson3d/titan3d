namespace NS_tutorials.physics
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "tutorials/physics/physicstestgame.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class physicstestgame : EngineNS.GamePlay.TtMacrossGame
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_814494378 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_814494378");
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_3265070907 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_3265070907");
        public static EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefab_2342805749 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefab_2342805749");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_1027957986 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1027957986");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/physics/physicstestgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_30868408_be72_4779_b08c_c6729f57946a
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginDestroy_2650419528,mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_814494378.TryBreak(mStack_BeginDestroy_2650419528, this);
                host.FinalViewportSlate();
            }
            #endif //!disable_macross_30868408_be72_4779_b08c_c6729f57946a
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/physics/physicstestgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_30868408_be72_4779_b08c_c6729f57946a
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_49606019 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_3265070907 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_3265070907", EngineNS.RName.GetRName("tutorials/physics/physicstest.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_3265070907", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_3265070907", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_3265070907", true);
                breaker_InitViewportSlateWithScene_3265070907.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_InitViewportSlateWithScene_3265070907 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/physics/physicstest.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_3265070907", tmp_r_InitViewportSlateWithScene_3265070907);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacterFromPrefab_2342805749", null);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_prefabName_CreateCharacterFromPrefab_2342805749", EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                breaker_CreateCharacterFromPrefab_2342805749.TryBreak(mStack_BeginPlay_1342966456, this);
                await host.CreateCharacterFromPrefab(null,EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                ret_49606019 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_49606019_1027957986", ret_49606019);
                breaker_return_1027957986.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_49606019;
            }
            #elif !(!disable_macross_30868408_be72_4779_b08c_c6729f57946a)
            System.Boolean ret_49606019 = default(System.Boolean);
            return ret_49606019;
            #endif //!disable_macross_30868408_be72_4779_b08c_c6729f57946a
        }
    }
}
