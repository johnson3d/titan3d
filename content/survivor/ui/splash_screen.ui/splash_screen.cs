namespace NS_survivor.ui
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/ui/splash_screen.ui", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class splash_screen : EngineNS.UI.TtUIMacrossBase
    {
        public EngineNS.Macross.TtMacrossBreak breaker_PopHUD_35586286 = new EngineNS.Macross.TtMacrossBreak("breaker_PopHUD_35586286");
        public EngineNS.Macross.TtMacrossBreak breaker_LoadScene_3259278801 = new EngineNS.Macross.TtMacrossBreak("breaker_LoadScene_3259278801");
        public EngineNS.Macross.TtMacrossBreak breaker_SetRenderPolicyToViewport_3200628621 = new EngineNS.Macross.TtMacrossBreak("breaker_SetRenderPolicyToViewport_3200628621");
        public EngineNS.Macross.TtMacrossBreak breaker_OnLoadScene_2522011539 = new EngineNS.Macross.TtMacrossBreak("breaker_OnLoadScene_2522011539");
        public EngineNS.Macross.TtMacrossBreak breaker_Load_2507405909 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_2507405909");
        public EngineNS.Macross.TtMacrossBreak breaker_SetHUD_1426202043 = new EngineNS.Macross.TtMacrossBreak("breaker_SetHUD_1426202043");
        bool mIsLoadingFirstLevel;
        EngineNS.Macross.TtMacrossStackFrame mFrame_On_MouseButtonDown_13566274906715060368_1371187902 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public async EngineNS.Thread.Async.TtTask On_MouseButtonDown_13566274906715060368(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            #if !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            if (mIsLoadingFirstLevel)
                return;
            mIsLoadingFirstLevel = true;
            using(var guard_On_MouseButtonDown_13566274906715060368 = new EngineNS.Macross.TtMacrossStackGuard(mFrame_On_MouseButtonDown_13566274906715060368_1371187902))
            {
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("sender", sender);
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("args", args);
                EngineNS.GamePlay.Scene.TtScene tmp_r_LoadScene_3259278801 = default(EngineNS.GamePlay.Scene.TtScene);
                EngineNS.Graphics.Pipeline.TtRenderPolicy tmp_r_SetRenderPolicyToViewport_3200628621 = default(EngineNS.Graphics.Pipeline.TtRenderPolicy);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_2507405909 = default(EngineNS.UI.Controls.TtUIElement);
                breaker_PopHUD_35586286.TryBreak();
                EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate.PopHUD();
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("v_mapName_LoadScene_3259278801", EngineNS.RName.GetRName("survivor/maps/firstlevel/scene_firstlevel.scene", EngineNS.RName.ERNameType.Game));
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("v_bSetToWorld_LoadScene_3259278801", true);
                breaker_LoadScene_3259278801.TryBreak();
                tmp_r_LoadScene_3259278801 = (EngineNS.GamePlay.Scene.TtScene)await EngineNS.TtEngine.Instance.GameInstance.LoadScene(EngineNS.RName.GetRName("survivor/maps/firstlevel/scene_firstlevel.scene", EngineNS.RName.ERNameType.Game),true);
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("tmp_r_LoadScene_3259278801", tmp_r_LoadScene_3259278801);
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("v_slate_SetRenderPolicyToViewport_3200628621", ((EngineNS.Graphics.Pipeline.TtViewportSlate)EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate));
                breaker_SetRenderPolicyToViewport_3200628621.TryBreak();
                tmp_r_SetRenderPolicyToViewport_3200628621 = (EngineNS.Graphics.Pipeline.TtRenderPolicy)await tmp_r_LoadScene_3259278801.SetRenderPolicyToViewport(((EngineNS.Graphics.Pipeline.TtViewportSlate)EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate));
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("tmp_r_SetRenderPolicyToViewport_3200628621", tmp_r_SetRenderPolicyToViewport_3200628621);
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("v_scene_OnLoadScene_2522011539", tmp_r_LoadScene_3259278801);
                breaker_OnLoadScene_2522011539.TryBreak();
                await ((NS_survivor.survivorgame)EngineNS.TtEngine.Instance.GameInstance.MacrossGame).OnLoadScene(tmp_r_LoadScene_3259278801);
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("v_name_Load_2507405909", EngineNS.RName.GetRName("survivor/ui/ui_battle.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_2507405909.TryBreak();
                tmp_r_Load_2507405909 = EngineNS.UI.TtUIAssistFunctions.UIManager.Load(EngineNS.RName.GetRName("survivor/ui/ui_battle.ui", EngineNS.RName.ERNameType.Game));
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("tmp_r_Load_2507405909", tmp_r_Load_2507405909);
                mFrame_On_MouseButtonDown_13566274906715060368_1371187902.SetWatchVariable("v_hud_SetHUD_1426202043", tmp_r_Load_2507405909);
                breaker_SetHUD_1426202043.TryBreak();
                EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate.SetHUD(tmp_r_Load_2507405909);
                ((NS_survivor.survivorgame)EngineNS.TtEngine.Instance.GameInstance.MacrossGame).SurvivorGameMode.BattleUI = tmp_r_Load_2507405909;
            }
            #endif //!disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_On_MouseButtonUp_7740614240042391714_1627919305 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public async EngineNS.Thread.Async.TtTask On_MouseButtonUp_7740614240042391714(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            #if !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_On_MouseButtonUp_7740614240042391714 = new EngineNS.Macross.TtMacrossStackGuard(mFrame_On_MouseButtonUp_7740614240042391714_1627919305))
            {
                mFrame_On_MouseButtonUp_7740614240042391714_1627919305.SetWatchVariable("sender", sender);
                mFrame_On_MouseButtonUp_7740614240042391714_1627919305.SetWatchVariable("args", args);
            }
            #endif //!disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeEvents_2974981434 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeEvents()
        {
            #if !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_InitializeEvents = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeEvents_2974981434))
            {
                EngineNS.UI.Controls.TtImage var_TtImage_13566274906715060368 = (EngineNS.UI.Controls.TtImage)FindElement(13566274906715060368);
                if ((var_TtImage_13566274906715060368 != null))
                {
                    var_TtImage_13566274906715060368.MouseButtonDown -= On_MouseButtonDown_13566274906715060368;
                    var_TtImage_13566274906715060368.MouseButtonDown += On_MouseButtonDown_13566274906715060368;
                }
                if ((HostElement != null))
                {
                    HostElement.NoHitTest = false;
                    HostElement.MouseButtonDown -= On_MouseButtonDown_13566274906715060368;
                    HostElement.MouseButtonDown += On_MouseButtonDown_13566274906715060368;
                }
            }
            #endif //!disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeBindings_3306941716 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeBindings()
        {
            #if !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_InitializeBindings = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeBindings_3306941716))
            {
            }
            #endif //!disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeUIElementVariables_515633741 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeUIElementVariables()
        {
            #if !disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_InitializeUIElementVariables = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeUIElementVariables_515633741))
            {
                EngineNS.UI.Controls.TtImage var_TtImage_13566274906715060368 = (EngineNS.UI.Controls.TtImage)FindElement(13566274906715060368);
                if ((var_TtImage_13566274906715060368 != null))
                {
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorMin(var_TtImage_13566274906715060368, new EngineNS.Vector2(0.0f, 0.0f));
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorMax(var_TtImage_13566274906715060368, new EngineNS.Vector2(1.0f, 1.0f));
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorRectX(var_TtImage_13566274906715060368, 0.0f);
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorRectY(var_TtImage_13566274906715060368, 0.0f);
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorRectZ(var_TtImage_13566274906715060368, 0.0f);
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorRectW(var_TtImage_13566274906715060368, 0.0f);
                    EngineNS.UI.Controls.Containers.TtCanvasControl.SetAnchorCenter(var_TtImage_13566274906715060368, new EngineNS.Vector2(0.0f, 0.0f));
                    var_TtImage_13566274906715060368.NoHitTest = false;
                }
            }
            #endif //!disable_ui_4027783c_0b24_4d01_9c05_56fb864e82e8 && !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646 && !disable_macross_a00a9f95_1be2_4899_a745_80d2fa28e48a && !disable_ui_5c2fa082_e11b_4b8b_a090_4f7af6b8f639 && !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
    }
}
