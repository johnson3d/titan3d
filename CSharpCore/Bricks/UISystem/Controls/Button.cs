using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls
{
    public class ButtonInitializer : EngineNS.UISystem.Controls.Containers.BorderInitializer
    {
        [Rtti.MetaData]
        public Style.ButtonStyle ButtonStyle { get; set; } = new Style.ButtonStyle();

        [Rtti.MetaData]
        public bool Enable { get; set; } = true;
        public enum enSizeType
        {
            ImageSize,
            ContentSize,
            Manual,
        }
        [Rtti.MetaData]
        public enSizeType SizeType { get; set; } = enSizeType.ImageSize;

        public ButtonInitializer()
        {
            ButtonStyle.NormalBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_button_normal.uvanim", RName.enRNameType.Engine);
            ButtonStyle.HoveredBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_button_hovered.uvanim", RName.enRNameType.Engine);
            ButtonStyle.PressedBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_button_pressed.uvanim", RName.enRNameType.Engine);
            ButtonStyle.DisabledBrush.ImageName = EngineNS.RName.GetRName("ui/uv_ui_button_disable.uvanim", RName.enRNameType.Engine);
        }
    }
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor_UIControlInit(typeof(ButtonInitializer))]
    [Editor_UIControl("通用.按钮", "按钮控件", "Button.png")]
    public class Button : EngineNS.UISystem.Controls.Containers.Border
    {
        [EngineNS.Editor.UIEditor_BindingPropertyAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Style.ButtonStyle ButtonStyle
        {
            get => ((ButtonInitializer)mInitializer).ButtonStyle;
        }

        public bool Enable
        {
            get => ((ButtonInitializer)mInitializer).Enable;
            set
            {
                ((ButtonInitializer)mInitializer).Enable = value;
                UpdateCurrentBrush();
                OnPropertyChanged("Enable");
            }
        }
        public ButtonInitializer.enSizeType SizeType
        {
            get => ((ButtonInitializer)mInitializer).SizeType;
            set
            {
                ((ButtonInitializer)mInitializer).SizeType = value;
                UpdateLayout();
                OnPropertyChanged("SizeType");
            }
        }

        public override async Task<bool> Initialize(CRenderContext rc, UIElementInitializer init)
        {
            if (!await base.Initialize(rc, init))
                return false;

            var btnInit = init as ButtonInitializer;
            await btnInit.ButtonStyle.Initialize(rc, this);
            UpdateCurrentBrush();
            return true;
        }

        void UpdateCurrentBrush()
        {
            var btnInit = mInitializer as ButtonInitializer;
            if (!btnInit.Enable)
            {
                mCurrentBrush = btnInit.ButtonStyle.DisabledBrush;
                return;
            }
            if (mHasMouseDown || mHasTouchDown)
            {
                mCurrentBrush = btnInit.ButtonStyle.PressedBrush;
                return;
            }
            if(mIsHover)
            {
                mCurrentBrush = btnInit.ButtonStyle.HoveredBrush;
                return;
            }
            mCurrentBrush = btnInit.ButtonStyle.NormalBrush;
        }

        #region Msg

        public delegate void Delegate_OnClick(UIElement ui, RoutedEventArgs args);
        [Editor_UIEvent("点击时调用方法")]
        public Delegate_OnClick OnClick;
        [Editor_UIEvent("鼠标点击时调用方法")]
        public Delegate_MouseEvent OnMouseClick;
        [Editor_UIEvent("触摸点击时调用方法")]
        public Delegate_TouchEvent OnTouchClick;

        bool mIsHover = false;
        public override void ProcessMouseEnter(RoutedEventArgs eventArgs)
        {
            mIsHover = true;
            base.ProcessMouseEnter(eventArgs);
            UpdateCurrentBrush();
        }
        public override void ProcessMouseLeave(RoutedEventArgs eventArgs)
        {
            mIsHover = false;
            base.ProcessMouseLeave(eventArgs);
            mHasMouseDown = false;
            UpdateCurrentBrush();
        }
        public override void ProcessTouchEnter(RoutedEventArgs eventArgs)
        {
            mIsHover = true;
            base.ProcessTouchEnter(eventArgs);
            UpdateCurrentBrush();
        }
        public override void ProcessTouchLeave(RoutedEventArgs eventArgs)
        {
            mIsHover = false;
            base.ProcessTouchLeave(eventArgs);
            mHasTouchDown = false;
            UpdateCurrentBrush();
        }

        bool mHasMouseDown = false;
        bool mHasTouchDown = false;
        public override void ProcessMessage(RoutedEventArgs eventArgs)
        {
            switch(eventArgs.DeviceType)
            {
                case Input.Device.DeviceType.Mouse:
                    {
                        var mouseArg = (Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs;
                        switch(mouseArg.State)
                        {
                            case Input.Device.Mouse.ButtonState.Down:
                                mHasMouseDown = true;
                                UpdateCurrentBrush();
                                break;
                            case Input.Device.Mouse.ButtonState.Up:
                                {
                                    if(mHasMouseDown)
                                    {
                                        if (OnClick != null)
                                            OnClick.Invoke(this, eventArgs);
                                        if (OnMouseClick != null)
                                            OnMouseClick.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                    mHasMouseDown = false;
                                    UpdateCurrentBrush();
                                }
                                break;
                        }
                    }
                    break;
                case Input.Device.DeviceType.Touch:
                    {
                        var touchArg = (Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs;
                        switch(touchArg.State)
                        {
                            case Input.Device.TouchDevice.enTouchState.Down:
                                mHasTouchDown = true;
                                UpdateCurrentBrush();
                                break;
                            case Input.Device.TouchDevice.enTouchState.Up:
                                {
                                    if(mHasTouchDown)
                                    {
                                        if (OnClick != null)
                                            OnClick.Invoke(this, eventArgs);
                                        if (OnTouchClick != null)
                                            OnTouchClick.Invoke(this, eventArgs, (EngineNS.Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                    mHasTouchDown = false;
                                    UpdateCurrentBrush();
                                }
                                break;
                        }
                    }
                    break;
            }
            base.ProcessMessage(eventArgs);
        }

        #endregion

        #region layout

        public override SizeF MeasureOverride(ref SizeF availableSize)
        {
            switch(SizeType)
            {
                case ButtonInitializer.enSizeType.ContentSize:
                    {
                        var retSize = SizeF.Empty;
                        for (int i= 0; i<mChildrenUIElements.Count; i++)
                        {
                            var child = mChildrenUIElements[i];
                            child.Measure(ref availableSize);
                            retSize.Width = System.Math.Max(retSize.Width, child.DesiredSize.Width);
                            retSize.Height = System.Math.Max(retSize.Height, child.DesiredSize.Height);
                        }
                        return retSize;
                    }
                case ButtonInitializer.enSizeType.ImageSize:
                    {
                        var btnInit = this.Initializer as ButtonInitializer;
                        if(btnInit.ButtonStyle != null)
                        {
                            var imgSize = btnInit.ButtonStyle.GetImageSize();
                            return new SizeF(imgSize.X, imgSize.Y);
                        }
                    }
                    break;
                case ButtonInitializer.enSizeType.Manual:
                    return availableSize;
            }
            return availableSize;
        }

        #endregion
    }
}
