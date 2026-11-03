namespace NS_survivor.ui
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/ui/ui_menu_button.ui", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class ui_menu_button : EngineNS.UI.TtUIMacrossBase
    {
        System.String mShowText = "888";
        [EngineNS.UI.Bind.BindPropertyAttribute]
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("ShowText")]
        public System.String ShowText
        {
            get => mShowText;
            set
            {
                OnValueChange(value, mShowText);
                mShowText = value;
            }
        }
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("TtImage")]
        [System.ComponentModel.Browsable(false)]
        public EngineNS.UI.Controls.TtImage ElementVar_14062278734384193017 { get; set; } = null;
        EngineNS.Color4b mTextColor = new EngineNS.Color4b(0,0,0,0);
        [EngineNS.UI.Bind.BindPropertyAttribute]
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("TextColor")]
        public EngineNS.Color4b TextColor
        {
            get => mTextColor;
            set
            {
                OnValueChange(value, mTextColor);
                mTextColor = value;
            }
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_On_DeviceEnter_14062278734384193017_1984232445 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_menu_button.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public async EngineNS.Thread.Async.TtTask On_DeviceEnter_14062278734384193017(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            #if !disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
            using(var guard_On_DeviceEnter_14062278734384193017 = new EngineNS.Macross.TtMacrossStackGuard(mFrame_On_DeviceEnter_14062278734384193017_1984232445))
            {
                mFrame_On_DeviceEnter_14062278734384193017_1984232445.SetWatchVariable("sender", sender);
                mFrame_On_DeviceEnter_14062278734384193017_1984232445.SetWatchVariable("args", args);
                ElementVar_14062278734384193017.UIBrush.Color = new EngineNS.Color4b(255,0,0,255);
            }
            #endif //!disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_On_DeviceLeave_14062278734384193017_252731222 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_menu_button.ui", EngineNS.RName.ERNameType.Game));
        [EngineNS.Rtti.MetaAttribute]
        public async EngineNS.Thread.Async.TtTask On_DeviceLeave_14062278734384193017(System.Object sender,EngineNS.UI.Controls.TtRoutedEventArgs args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            #if !disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
            using(var guard_On_DeviceLeave_14062278734384193017 = new EngineNS.Macross.TtMacrossStackGuard(mFrame_On_DeviceLeave_14062278734384193017_252731222))
            {
                mFrame_On_DeviceLeave_14062278734384193017_252731222.SetWatchVariable("sender", sender);
                mFrame_On_DeviceLeave_14062278734384193017_252731222.SetWatchVariable("args", args);
                ElementVar_14062278734384193017.UIBrush.Color = new EngineNS.Color4b(255,255,255,255);
            }
            #endif //!disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeEvents_2974981434 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_menu_button.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeEvents()
        {
            #if !disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
            using(var guard_InitializeEvents = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeEvents_2974981434))
            {
                EngineNS.UI.Controls.TtImage var_TtImage_14062278734384193017 = (EngineNS.UI.Controls.TtImage)FindElement(14062278734384193017);
                if ((var_TtImage_14062278734384193017 != null))
                {
                    var_TtImage_14062278734384193017.DeviceEnter -= On_DeviceEnter_14062278734384193017;
                    var_TtImage_14062278734384193017.DeviceEnter += On_DeviceEnter_14062278734384193017;
                    var_TtImage_14062278734384193017.DeviceLeave -= On_DeviceLeave_14062278734384193017;
                    var_TtImage_14062278734384193017.DeviceLeave += On_DeviceLeave_14062278734384193017;
                }
            }
            #endif //!disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeBindings_3306941716 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_menu_button.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeBindings()
        {
            #if !disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
            using(var guard_InitializeBindings = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeBindings_3306941716))
            {
                EngineNS.UI.Bind.IBindableObject object_14938425351295932799 = HostElement.FindBindObject(14938425351295932799);
                if ((object_14938425351295932799 != null))
                {
                    EngineNS.UI.Bind.TtBindingOperations.SetBinding<System.String,System.String>(object_14938425351295932799,"Text",this,"ShowText",EngineNS.UI.Bind.EBindingMode.TwoWay);
                }
                if ((object_14938425351295932799 != null))
                {
                    EngineNS.UI.Bind.TtBindingOperations.SetBinding<EngineNS.Color4b,EngineNS.Color4b>(object_14938425351295932799,"Color",this,"TextColor",EngineNS.UI.Bind.EBindingMode.OneWay);
                }
            }
            #endif //!disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_InitializeUIElementVariables_515633741 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/ui/ui_menu_button.ui", EngineNS.RName.ERNameType.Game));
        public override void InitializeUIElementVariables()
        {
            #if !disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
            using(var guard_InitializeUIElementVariables = new EngineNS.Macross.TtMacrossStackGuard(mFrame_InitializeUIElementVariables_515633741))
            {
                ElementVar_14062278734384193017 = (EngineNS.UI.Controls.TtImage)HostElement.FindElement(14062278734384193017);
            }
            #endif //!disable_ui_852c3b65_fbfc_4341_b8eb_4395cad2ec5a
        }
    }
}
