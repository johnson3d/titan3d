namespace NS_survivor.ui
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/ui/ui_battle.ui", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class ui_battle : EngineNS.UI.TtUIMacrossBase
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("TtProgress_HP")]
        public EngineNS.UI.Controls.TtProgress ElementVar_4728652819903166736 { get; set; } = null;
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeEvents_2974981434 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_battle.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeEvents()
        {
            #if !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646
            using(var guard_InitializeEvents = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeEvents_2974981434))
            {
            }
            #endif //!disable_ui_327b07a9_4fc1_4de8_9946_32e512334646
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeBindings_3306941716 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_battle.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeBindings()
        {
            #if !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646
            using(var guard_InitializeBindings = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeBindings_3306941716))
            {
            }
            #endif //!disable_ui_327b07a9_4fc1_4de8_9946_32e512334646
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeUIElementVariables_515633741 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_battle.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeUIElementVariables()
        {
            #if !disable_ui_327b07a9_4fc1_4de8_9946_32e512334646
            using(var guard_InitializeUIElementVariables = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeUIElementVariables_515633741))
            {
                ElementVar_4728652819903166736 = (EngineNS.UI.Controls.TtProgress)HostElement.FindElement(4728652819903166736);
            }
            #endif //!disable_ui_327b07a9_4fc1_4de8_9946_32e512334646
        }
    }
}
