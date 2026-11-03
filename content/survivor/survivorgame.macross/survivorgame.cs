namespace NS_survivor
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/survivorgame.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class survivorgame : Survivor.TtMacrossSurvivorGame
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_SetPropertyTarget_1517288698 = new EngineNS.Macross.TtMacrossBreak("breaker_SetPropertyTarget_1517288698");
        public static EngineNS.Macross.TtMacrossBreak breaker_CreateCharacterController_277788088 = new EngineNS.Macross.TtMacrossBreak("breaker_CreateCharacterController_277788088");
        public static EngineNS.Macross.TtMacrossBreak breaker_LoadWeapons_3161931186 = new EngineNS.Macross.TtMacrossBreak("breaker_LoadWeapons_3161931186");
        public static EngineNS.Macross.TtMacrossBreak breaker_LoadHeros_4043734391 = new EngineNS.Macross.TtMacrossBreak("breaker_LoadHeros_4043734391");
        public static EngineNS.Macross.TtMacrossBreak breaker_LoadMonsters_3782364258 = new EngineNS.Macross.TtMacrossBreak("breaker_LoadMonsters_3782364258");
        public static EngineNS.Macross.TtMacrossBreak breaker_InitControlledCharacter_2318701348 = new EngineNS.Macross.TtMacrossBreak("breaker_InitControlledCharacter_2318701348");
        public static EngineNS.Macross.TtMacrossBreak breaker_InitMonsterSpawner_1181717599 = new EngineNS.Macross.TtMacrossBreak("breaker_InitMonsterSpawner_1181717599");
        public static EngineNS.Macross.TtMacrossBreak breaker_Load_8178407 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_8178407");
        public static EngineNS.Macross.TtMacrossBreak breaker_SetHUD_269481762 = new EngineNS.Macross.TtMacrossBreak("breaker_SetHUD_269481762");
        public static EngineNS.Macross.TtMacrossBreak breaker_InitViewportSlate_2460084237 = new EngineNS.Macross.TtMacrossBreak("breaker_InitViewportSlate_2460084237");
        public static EngineNS.Macross.TtMacrossBreak breaker_Load_4252465771 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_4252465771");
        public static EngineNS.Macross.TtMacrossBreak breaker_PushHUD_3570258782 = new EngineNS.Macross.TtMacrossBreak("breaker_PushHUD_3570258782");
        public static EngineNS.Macross.TtMacrossBreak breaker_return_1569429116 = new EngineNS.Macross.TtMacrossBreak("breaker_return_1569429116");
        public static EngineNS.Macross.TtMacrossBreak breaker_Load_3345036021 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_3345036021");
        public static EngineNS.Macross.TtMacrossBreak breaker_SetHUD_2020034667 = new EngineNS.Macross.TtMacrossBreak("breaker_SetHUD_2020034667");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnLoadScene_145366808 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/survivorgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnLoadScene_145366808 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public async EngineNS.Thread.Async.TtTask OnLoadScene(EngineNS.GamePlay.Scene.TtScene scene)
        {
            #if !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_OnLoadScene = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnLoadScene_145366808,mFrame_OnLoadScene_145366808))
            {
                mFrame_OnLoadScene_145366808.SetWatchVariable("scene", scene);
                EngineNS.GamePlay.Controller.TtCharacterController tmp_r_CreateCharacterController_277788088 = default(EngineNS.GamePlay.Controller.TtCharacterController);
                System.Boolean tmp_r_InitControlledCharacter_2318701348 = default(System.Boolean);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_8178407 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_obj_SetPropertyTarget_1517288698", ((System.Object)scene));
                breaker_SetPropertyTarget_1517288698.TryBreak(mStack_OnLoadScene_145366808, this);
                EngineNS.GamePlay.TtMacrossFunctionLibrary.SetPropertyTarget(((System.Object)scene));
                this.SurvivorGameMode.CurrentScene = scene;
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_scene_CreateCharacterController_277788088", this.SurvivorGameMode.CurrentScene);
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_prefabName_CreateCharacterController_277788088", EngineNS.RName.GetRName("survivor/player/prefab_hero.prefab", EngineNS.RName.ERNameType.Game));
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_orientCameraRoation_CreateCharacterController_277788088", true);
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_OrientToMovmement_CreateCharacterController_277788088", true);
                breaker_CreateCharacterController_277788088.TryBreak(mStack_OnLoadScene_145366808, this);
                tmp_r_CreateCharacterController_277788088 = (EngineNS.GamePlay.Controller.TtCharacterController)await EngineNS.TtEngine.Instance.GameInstance.CreateCharacterController(this.SurvivorGameMode.CurrentScene,EngineNS.RName.GetRName("survivor/player/prefab_hero.prefab", EngineNS.RName.ERNameType.Game),true,true);
                mFrame_OnLoadScene_145366808.SetWatchVariable("tmp_r_CreateCharacterController_277788088", tmp_r_CreateCharacterController_277788088);
                this.SurvivorGameMode.CharacterController = tmp_r_CreateCharacterController_277788088;
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_name_LoadWeapons_3161931186", EngineNS.RName.GetRName("survivor/datatable/ttweapondata.dataset", EngineNS.RName.ERNameType.Game));
                breaker_LoadWeapons_3161931186.TryBreak(mStack_OnLoadScene_145366808, this);
                this.SurvivorGameMode.LoadWeapons(EngineNS.RName.GetRName("survivor/datatable/ttweapondata.dataset", EngineNS.RName.ERNameType.Game));
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_name_LoadHeros_4043734391", EngineNS.RName.GetRName("survivor/datatable/ttroledata.dataset", EngineNS.RName.ERNameType.Game));
                breaker_LoadHeros_4043734391.TryBreak(mStack_OnLoadScene_145366808, this);
                this.SurvivorGameMode.LoadHeros(EngineNS.RName.GetRName("survivor/datatable/ttroledata.dataset", EngineNS.RName.ERNameType.Game));
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_name_LoadMonsters_3782364258", EngineNS.RName.GetRName("survivor/datatable/ttmonsterdata.dataset", EngineNS.RName.ERNameType.Game));
                breaker_LoadMonsters_3782364258.TryBreak(mStack_OnLoadScene_145366808, this);
                this.SurvivorGameMode.LoadMonsters(EngineNS.RName.GetRName("survivor/datatable/ttmonsterdata.dataset", EngineNS.RName.ERNameType.Game));
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_cc_InitControlledCharacter_2318701348", this.SurvivorGameMode.CharacterController);
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_roleId_InitControlledCharacter_2318701348", 10001);
                breaker_InitControlledCharacter_2318701348.TryBreak(mStack_OnLoadScene_145366808, this);
                tmp_r_InitControlledCharacter_2318701348 = (System.Boolean)await this.SurvivorGameMode.InitControlledCharacter(this.SurvivorGameMode.CharacterController,10001);
                mFrame_OnLoadScene_145366808.SetWatchVariable("tmp_r_InitControlledCharacter_2318701348", tmp_r_InitControlledCharacter_2318701348);
                breaker_InitMonsterSpawner_1181717599.TryBreak(mStack_OnLoadScene_145366808, this);
                await this.SurvivorGameMode.InitMonsterSpawner();
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_name_Load_8178407", EngineNS.RName.GetRName("survivor/ui/ui_statics.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_8178407.TryBreak(mStack_OnLoadScene_145366808, this);
                tmp_r_Load_8178407 = EngineNS.UI.TtUIAssistFunctions.UIManager.Load(EngineNS.RName.GetRName("survivor/ui/ui_statics.ui", EngineNS.RName.ERNameType.Game));
                mFrame_OnLoadScene_145366808.SetWatchVariable("tmp_r_Load_8178407", tmp_r_Load_8178407);
                mFrame_OnLoadScene_145366808.SetWatchVariable("v_hud_SetHUD_269481762", tmp_r_Load_8178407);
                breaker_SetHUD_269481762.TryBreak(mStack_OnLoadScene_145366808, this);
                EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate.SetHUD(tmp_r_Load_8178407);
            }
            #endif //!disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/survivorgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginDestroy_2650419528 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void BeginDestroy(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_BeginDestroy = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginDestroy_2650419528,mFrame_BeginDestroy_2650419528))
            {
                mFrame_BeginDestroy_2650419528.SetWatchVariable("host", host);
            }
            #endif //!disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/survivorgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_BeginPlay_1342966456 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override async System.Threading.Tasks.Task<System.Boolean> BeginPlay(EngineNS.GamePlay.TtGameInstance host)
        {
            #if !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_BeginPlay = new EngineNS.Macross.TtMacrossStackGuard(mStack_BeginPlay_1342966456,mFrame_BeginPlay_1342966456))
            {
                System.Boolean ret_2076529031 = default(System.Boolean);
                mFrame_BeginPlay_1342966456.SetWatchVariable("host", host);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_4252465771 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_rPolicy_InitViewportSlate_2460084237", EngineNS.RName.GetRName("graphics/deferred_simple.rpolicy", EngineNS.RName.ERNameType.Engine));
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMin_InitViewportSlate_2460084237", 0f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_zMax_InitViewportSlate_2460084237", 1f);
                breaker_InitViewportSlate_2460084237.TryBreak(mStack_BeginPlay_1342966456, this);
                await host.InitViewportSlate(EngineNS.RName.GetRName("graphics/deferred_simple.rpolicy", EngineNS.RName.ERNameType.Engine),0f,1f);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_name_Load_4252465771", EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_4252465771.TryBreak(mStack_BeginPlay_1342966456, this);
                tmp_r_Load_4252465771 = EngineNS.UI.TtUIAssistFunctions.UIManager.Load(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
                mFrame_BeginPlay_1342966456.SetWatchVariable("tmp_r_Load_4252465771", tmp_r_Load_4252465771);
                mFrame_BeginPlay_1342966456.SetWatchVariable("v_hud_PushHUD_3570258782", tmp_r_Load_4252465771);
                breaker_PushHUD_3570258782.TryBreak(mStack_BeginPlay_1342966456, this);
                host.WorldViewportSlate.PushHUD(tmp_r_Load_4252465771);
                host.WorldViewportSlate.RenderPolicy.EnableGI = true;
                ret_2076529031 = true;
                mFrame_BeginPlay_1342966456.SetWatchVariable("ret_2076529031_1569429116", ret_2076529031);
                breaker_return_1569429116.TryBreak(mStack_BeginPlay_1342966456, this);
                return ret_2076529031;
            }
            #elif !(!disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08)
            System.Boolean ret_2076529031 = default(System.Boolean);
            return ret_2076529031;
            #endif //!disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnPlayerDead_3625537637 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/survivorgame.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnPlayerDead_3625537637 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void OnPlayerDead()
        {
            #if !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_OnPlayerDead = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnPlayerDead_3625537637,mFrame_OnPlayerDead_3625537637))
            {
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_3345036021 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_OnPlayerDead_3625537637.SetWatchVariable("v_name_Load_3345036021", EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_3345036021.TryBreak(mStack_OnPlayerDead_3625537637, this);
                tmp_r_Load_3345036021 = EngineNS.UI.TtUIAssistFunctions.UIManager.Load(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
                mFrame_OnPlayerDead_3625537637.SetWatchVariable("tmp_r_Load_3345036021", tmp_r_Load_3345036021);
                mFrame_OnPlayerDead_3625537637.SetWatchVariable("v_hud_SetHUD_2020034667", tmp_r_Load_3345036021);
                breaker_SetHUD_2020034667.TryBreak(mStack_OnPlayerDead_3625537637, this);
                EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate.SetHUD(tmp_r_Load_3345036021);
            }
            #endif //!disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
    }
}
