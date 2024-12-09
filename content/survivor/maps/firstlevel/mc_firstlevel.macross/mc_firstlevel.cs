namespace NS_survivor.maps.firstlevel
{
    [EngineNS.Macross.TtMacross]
    public partial class mc_firstlevel : EngineNS.GamePlay.TtMacrossGame
    {
        [EngineNS.Rtti.Meta]
        public Survivor.TtGameMode GameMode { get; set; } = null;
        public EngineNS.Macross.TtMacrossBreak breaker_FinalViewportSlate_560254116 = new EngineNS.Macross.TtMacrossBreak("breaker_FinalViewportSlate_560254116");
        public EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlateWithScene_4277137042 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlateWithScene_4277137042");
        public EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterFromPrefabDetial_863962771 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterFromPrefabDetial_863962771");
        public EngineNS.Macross.TtMacrossBreak breaker_creator_2702113056 = new EngineNS.Macross.TtMacrossBreak("breaker_creator_2702113056");
        public EngineNS.Macross.TtMacrossBreak breaker_LoadWeapons_2558297129 = new EngineNS.Macross.TtMacrossBreak("breaker_LoadWeapons_2558297129");
        public EngineNS.Macross.TtMacrossBreak breaker_return_2433468528 = new EngineNS.Macross.TtMacrossBreak("breaker_return_2433468528");
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/maps/firstlevel/mc_firstlevel.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
                breaker_FinalViewportSlate_560254116.TryBreak();
                host.FinalViewportSlate();
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/maps/firstlevel/mc_firstlevel.macross", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_1466362967 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.GamePlay.Scene.TtScene tmp_r_InitViewportSlateWithScene_4277137042 = default(EngineNS.GamePlay.Scene.TtScene);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_mapName_InitViewportSlateWithScene_4277137042", EngineNS.RName.GetRName("survivor/maps/firstlevel/scene_firstlevel.scene", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlateWithScene_4277137042", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlateWithScene_4277137042", 1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_bSetToWorld_InitViewportSlateWithScene_4277137042", true);
                breaker_InitViewportSlateWithScene_4277137042.TryBreak();
                tmp_r_InitViewportSlateWithScene_4277137042 = (EngineNS.GamePlay.Scene.TtScene)await host.InitViewportSlateWithScene(EngineNS.RName.GetRName("survivor/maps/firstlevel/scene_firstlevel.scene", EngineNS.RName.ERNameType.Game),0f,1f,true);
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_InitViewportSlateWithScene_4277137042", tmp_r_InitViewportSlateWithScene_4277137042);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_scene_CreateCharacterFromPrefabDetial_863962771", tmp_r_InitViewportSlateWithScene_4277137042);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_prefabName_CreateCharacterFromPrefabDetial_863962771", EngineNS.RName.GetRName("survivor/player/prefab_hero.prefab", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_orientCameraRoation_CreateCharacterFromPrefabDetial_863962771", false);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_OrientToMovmement_CreateCharacterFromPrefabDetial_863962771", true);
                breaker_CreateCharacterFromPrefabDetial_863962771.TryBreak();
                await host.CreateCharacterFromPrefabDetial(tmp_r_InitViewportSlateWithScene_4277137042,EngineNS.RName.GetRName("survivor/player/prefab_hero.prefab", EngineNS.RName.ERNameType.Game),false,true);
                Survivor.TtGameMode new_2702113056 = new Survivor.TtGameMode();
                mFrame_BeginPlay_1342966456.SetWatchVariable("Type_2702113056", new_2702113056);
                breaker_creator_2702113056.TryBreak();
                GameMode = new_2702113056;
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_LoadWeapons_2558297129", EngineNS.RName.GetRName("survivor/skills/weapon.dataset", EngineNS.RName.ERNameType.Game));
                breaker_LoadWeapons_2558297129.TryBreak();
                GameMode.LoadWeapons(EngineNS.RName.GetRName("survivor/skills/weapon.dataset", EngineNS.RName.ERNameType.Game));
                ret_1466362967 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_1466362967_2433468528", ret_1466362967);
                breaker_return_2433468528.TryBreak();
                return ret_1466362967;
            }
        }
    }
}
