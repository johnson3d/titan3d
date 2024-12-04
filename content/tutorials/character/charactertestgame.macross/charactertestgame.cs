namespace NS_tutorials.character
{
    [EngineNS.Macross.TtMacross]
    public partial class charactertestgame : EngineNS.GamePlay.TtMacrossGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_623502181 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_623502181");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_892557814 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_892557814");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefab_3923349983 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefab_3923349983");
        public EngineNS.Macross.TtMacrossBreak breaker_return_3547363734 = new EngineNS.Macross.TtMacrossBreak("breaker_return_3547363734");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/character/charactertestgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_623502181.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("tutorials/character/charactertestgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_2423809958 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_892557814 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_892557814", EngineNS.RName.GetRName("tutorials/character/charactertest.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_892557814", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_892557814", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_892557814", true);
                breaker_InitViewportSlateWithScene_892557814.TryBreak();
                tmp_r_InitViewportSlateWithScene_892557814 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("tutorials/character/charactertest.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_892557814", tmp_r_InitViewportSlateWithScene_892557814);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacterFromPrefab_3923349983", tmp_r_InitViewportSlateWithScene_892557814);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_prefabName_CreateCharacterFromPrefab_3923349983", EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                breaker_CreateCharacterFromPrefab_3923349983.TryBreak();
                await host.CreateCharacterFromPrefab(tmp_r_InitViewportSlateWithScene_892557814,EngineNS.RName.GetRName("project_factory/belica/prefab_belica.prefab", EngineNS.RName.ERNameType.Game));
                ret_2423809958 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_2423809958_3547363734", ret_2423809958);
                breaker_return_3547363734.TryBreak();
                return ret_2423809958;
            }
        }
    }
}
