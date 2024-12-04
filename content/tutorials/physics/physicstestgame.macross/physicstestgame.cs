namespace NS_tutorials.physics
{
    [EngineNS.Macross.TtMacross]
    public partial class physicstestgame : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_814494378 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_814494378");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_1652194480 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_1652194480");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefab_746338167 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefab_746338167");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1027957986 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1027957986");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/physics/physicstestgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_814494378.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/physics/physicstestgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_49606019 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_1652194480 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_1652194480", EngineNS.RName.GetRName("tutorials/physics/physicstest.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_1652194480", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_1652194480", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_1652194480", true);
                breaker_InitViewportSlateWithScene_1652194480.TryBreak();
                tmp_r_InitViewportSlateWithScene_1652194480 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/physics/physicstest.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_1652194480", tmp_r_InitViewportSlateWithScene_1652194480);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacterFromPrefab_746338167", tmp_r_InitViewportSlateWithScene_1652194480);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_prefabName_CreateCharacterFromPrefab_746338167", EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                breaker_CreateCharacterFromPrefab_746338167.TryBreak();
                await host.CreateCharacterFromPrefab(tmp_r_InitViewportSlateWithScene_1652194480,EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                ret_49606019 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_49606019_1027957986", ret_49606019);
                breaker_return_1027957986.TryBreak();
                return ret_49606019;
                return ret_49606019;
            }
        }
    }
}
