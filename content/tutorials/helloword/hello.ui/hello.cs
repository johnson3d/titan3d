namespace NS_tutorials.helloword
{
    [EngineNS.Macross.UMacross]
    public partial class hello : EngineNS.UI.TtUIMacrossBase
    {
        EngineNS.Macross.UMacrossStackFrame mFrame_On_Click_2240035251220394824 = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
        public void On_Click_2240035251220394824(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            using(var guard_On_Click_2240035251220394824 = new EngineNS.Macross.UMacrossStackGuard(mFrame_On_Click_2240035251220394824))
            {
                mFrame_On_Click_2240035251220394824.SetWatchVariable("sender", sender);
                mFrame_On_Click_2240035251220394824.SetWatchVariable("args", args);
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_InitializeEvents = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeEvents()
        {
            using(var guard_InitializeEvents = new EngineNS.Macross.UMacrossStackGuard(mFrame_InitializeEvents))
            {
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_InitializeBindings = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeBindings()
        {
            using(var guard_InitializeBindings = new EngineNS.Macross.UMacrossStackGuard(mFrame_InitializeBindings))
            {
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_InitializeUIElementVariables = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeUIElementVariables()
        {
            using(var guard_InitializeUIElementVariables = new EngineNS.Macross.UMacrossStackGuard(mFrame_InitializeUIElementVariables))
            {
            }
        }
    }
}
