namespace NS_utest
{
    [EngineNS.Macross.UMacross]
    public partial class test_ui : EngineNS.UI.TtUIMacrossBase
    {
        EngineNS.Macross.UMacrossStackFrame mFrame_InitializeBindings = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/test_ui.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public override void InitializeBindings()
        {
            using(var guard_InitializeBindings = new EngineNS.Macross.UMacrossStackGuard(mFrame_InitializeBindings))
            {
            }
        }
    }
}
