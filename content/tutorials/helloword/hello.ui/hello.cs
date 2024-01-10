namespace NS_tutorials.helloword
{
    [EngineNS.Macross.UMacross]
    public partial class hello : EngineNS.UI.TtUIMacrossBase
    {
        public EngineNS.Macross.UMacrossBreak breaker_if_555846347 = new EngineNS.Macross.UMacrossBreak("breaker_if_555846347");
        EngineNS.Macross.UMacrossStackFrame mFrame_On_Click_13068764605028994591 = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public void On_Click_13068764605028994591(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            using(var guard_On_Click_13068764605028994591 = new EngineNS.Macross.UMacrossStackGuard(mFrame_On_Click_13068764605028994591))
            {
                mFrame_On_Click_13068764605028994591.SetWatchVariable("sender", sender);
                mFrame_On_Click_13068764605028994591.SetWatchVariable("args", args);
                mFrame_On_Click_13068764605028994591.SetWatchVariable("Condition0_555846347", true);
                breaker_if_555846347.TryBreak();
                if (true)
                {
                }
                else
                {
                }
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_InitializeEvents = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("tutorials/helloword/hello.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public override void InitializeEvents()
        {
            using(var guard_InitializeEvents = new EngineNS.Macross.UMacrossStackGuard(mFrame_InitializeEvents))
            {
                EngineNS.UI.Controls.TtButton var_TtButton_13068764605028994591 = (EngineNS.UI.Controls.TtButton)HostElement.FindElement(13068764605028994591);
                if ((var_TtButton_13068764605028994591 != null))
                {
                    var_TtButton_13068764605028994591.Click -= On_Click_13068764605028994591;
                    var_TtButton_13068764605028994591.Click += On_Click_13068764605028994591;
                }
            }
        }
    }
}
