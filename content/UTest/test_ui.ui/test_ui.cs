namespace NS_utest
{
    [EngineNS.Macross.UMacross]
    public partial class test_ui : EngineNS.UI.TtUIMacrossBase
    {
        EngineNS.Macross.UMacrossStackFrame mFrame_On_DeviceDown_8860288789123985274 = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/test_ui.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public void On_DeviceDown_8860288789123985274(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            using(var guard_On_DeviceDown_8860288789123985274 = new EngineNS.Macross.UMacrossStackGuard(mFrame_On_DeviceDown_8860288789123985274))
            {
                mFrame_On_DeviceDown_8860288789123985274.SetWatchVariable("sender", sender);
                mFrame_On_DeviceDown_8860288789123985274.SetWatchVariable("args", args);
            }
        }
        EngineNS.Macross.UMacrossStackFrame mFrame_InitializeEvents = new EngineNS.Macross.UMacrossStackFrame(EngineNS.RName.GetRName("utest/test_ui.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.Meta]
        public override void InitializeEvents()
        {
            using(var guard_InitializeEvents = new EngineNS.Macross.UMacrossStackGuard(mFrame_InitializeEvents))
            {
                EngineNS.UI.Controls.TtButton var_TtButton_8860288789123985274 = (EngineNS.UI.Controls.TtButton)HostElement.FindElement(8860288789123985274);
                if ((var_TtButton_8860288789123985274 != null))
                {
                    var_TtButton_8860288789123985274.DeviceDown -= On_DeviceDown_8860288789123985274;
                    var_TtButton_8860288789123985274.DeviceDown += On_DeviceDown_8860288789123985274;
                }
            }
        }
    }
}
