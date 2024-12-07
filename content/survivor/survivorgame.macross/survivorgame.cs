namespace NS_survivor
{
    [EngineNS.Macross.TtMacross]
    public partial class survivorgame : Survivor.TtMacrossSurvivorGame
    {
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_640957440 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_640957440");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefabDetial_2527002779 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefabDetial_2527002779");
        public EngineNS.Macross.TtMacrossBreak breaker_LoadWeapons_477370860 = new EngineNS.Macross.TtMacrossBreak("breaker_LoadWeapons_477370860");
        public EngineNS.Macross.TtMacrossBreak breaker_return_1569429116 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1569429116");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/survivorgame.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_2076529031 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_640957440 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_640957440", EngineNS.RName.GetRName("survivor/maps/firstlevel/scene_firstlevel.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_640957440", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_640957440", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_640957440", true);
                breaker_InitViewportSlateWithScene_640957440.TryBreak();
                tmp_r_InitViewportSlateWithScene_640957440 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("survivor/maps/firstlevel/scene_firstlevel.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_640957440", tmp_r_InitViewportSlateWithScene_640957440);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacterFromPrefabDetial_2527002779", tmp_r_InitViewportSlateWithScene_640957440);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_prefabName_CreateCharacterFromPrefabDetial_2527002779", EngineNS.RName.GetRName("survivor/player/prefab_hero.prefab", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_orientCameraRoation_CreateCharacterFromPrefabDetial_2527002779", false);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_OrientToMovmement_CreateCharacterFromPrefabDetial_2527002779", true);
                breaker_CreateCharacterFromPrefabDetial_2527002779.TryBreak();
                await host.CreateCharacterFromPrefabDetial(tmp_r_InitViewportSlateWithScene_640957440,EngineNS.RName.GetRName("survivor/player/prefab_hero.prefab", EngineNS.RName.ERNameType.Game),false,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_LoadWeapons_477370860", EngineNS.RName.GetRName("survivor/skills/weapon.dataset", EngineNS.RName.ERNameType.Game));
                breaker_LoadWeapons_477370860.TryBreak();
                this.GameMode.LoadWeapons(EngineNS.RName.GetRName("survivor/skills/weapon.dataset", EngineNS.RName.ERNameType.Game));
                ret_2076529031 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_2076529031_1569429116", ret_2076529031);
                breaker_return_1569429116.TryBreak();
                return ret_2076529031;
            }
        }
    }
}
