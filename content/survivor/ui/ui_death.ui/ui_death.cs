namespace NS_survivor.ui
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/ui/ui_death.ui", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class ui_death : EngineNS.UI.TtUIMacrossBase
    {
        public EngineNS.Macross.TtMacrossBreak breaker_Load_1998710933 = new EngineNS.Macross.TtMacrossBreak("breaker_Load_1998710933");
        public EngineNS.Macross.TtMacrossBreak breaker_PushHUD_1609100105 = new EngineNS.Macross.TtMacrossBreak("breaker_PushHUD_1609100105");
        EngineNS.Macross.TtMacrossStackFrame mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_death.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public async EngineNS.Thread.Async.TtTask On_MouseLeftButtonDown_6950276604528878629(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            #if !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_On_MouseLeftButtonDown_6950276604528878629 = new EngineNS.Macross.TtMacrossStackGuard(mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373))
            {
                mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373.SetWatchVariable("sender", sender);
                mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373.SetWatchVariable("args", args);
                EngineNS.UI.Controls.TtUIElement tmp_r_Load_1998710933 = default(EngineNS.UI.Controls.TtUIElement);
                mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373.SetWatchVariable("v_name_Load_1998710933", EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
                breaker_Load_1998710933.TryBreak();
                tmp_r_Load_1998710933 = EngineNS.UI.TtUIAssistFunctions.UIManager.Load(EngineNS.RName.GetRName("survivor/ui/splash_screen.ui", EngineNS.RName.ERNameType.Game));
                mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373.SetWatchVariable("tmp_r_Load_1998710933", tmp_r_Load_1998710933);
                mFrame_On_MouseLeftButtonDown_6950276604528878629_2366430373.SetWatchVariable("v_hud_PushHUD_1609100105", tmp_r_Load_1998710933);
                breaker_PushHUD_1609100105.TryBreak();
                if ((tmp_r_Load_1998710933 != null))
                {
                    EngineNS.TtEngine.Instance.GameInstance.WorldViewportSlate.SetHUD(tmp_r_Load_1998710933);
                }
            }
            #endif //!disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeEvents_2974981434 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_death.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeEvents()
        {
            #if !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_InitializeEvents = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeEvents_2974981434))
            {
                EngineNS.UI.Controls.TtImage var_TtImage_6950276604528878629 = (EngineNS.UI.Controls.TtImage)FindElement(6950276604528878629);
                if ((var_TtImage_6950276604528878629 != null))
                {
                    var_TtImage_6950276604528878629.MouseLeftButtonDown -= On_MouseLeftButtonDown_6950276604528878629;
                    var_TtImage_6950276604528878629.MouseLeftButtonDown += On_MouseLeftButtonDown_6950276604528878629;
                }
            }
            #endif //!disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeBindings_3306941716 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_death.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeBindings()
        {
            #if !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_InitializeBindings = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeBindings_3306941716))
            {
            }
            #endif //!disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeUIElementVariables_515633741 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_death.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeUIElementVariables()
        {
            #if !disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
            using(var guard_InitializeUIElementVariables = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeUIElementVariables_515633741))
            {
            }
            #endif //!disable_ui_2ff5d4d3_0313_42ec_aa10_d3713f77df08
        }
    }
}
