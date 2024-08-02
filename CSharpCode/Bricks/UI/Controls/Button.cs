using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtButtonBase : TtContainer
    {
        public enum EClickType
        {
            Release,
            Press,
            Hover,
        }
        EClickType mClickType = EClickType.Release;
        [Rtti.Meta, BindProperty]
        [Category("Behavior")]
        public EClickType ClickType
        {
            get => mClickType;
            set
            {
                OnValueChange(value, mClickType);
                mClickType = value;
            }
        }

        [Browsable(false), ReadOnly(true)]
        [Category("Behavior"), BindProperty]
        public bool IsPressed
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public TtButtonBase()
        {
            IsFocusable = true;

            MouseLeftButtonDown += TtButtonBase_MouseLeftButtonDown;
            TouchDown += TtButtonBase_MouseLeftButtonDown;
            MouseLeftButtonUp += TtButtonBase_MouseLeftButtonUp;
            TouchUp += TtButtonBase_MouseLeftButtonUp;
            MouseEnter += TtButtonBase_MouseEnter;
            TouchEnter += TtButtonBase_MouseEnter;
            MouseLeave += TtButtonBase_MouseLeave;
            TouchLeave += TtButtonBase_MouseLeave;
            MouseMove += TtButtonBase_MouseMove;
            TouchMove += TtButtonBase_MouseMove;
            KeyDown += TtButtonBase_KeyDown;
            KeyUp += TtButtonBase_KeyUp;
            OnLostFocus += TtButtonBase_OnLostFocus;
            OnLostMouseCapture += TtButtonBase_OnLostMouseCapture;
        }

        private void TtButtonBase_MouseLeftButtonDown(object sender, TtRoutedEventArgs eventArgs)
        {
            if (ClickType != EClickType.Hover)
            {
                eventArgs.Handled = true;
                UEngine.Instance.UIManager.KeyboardFocus(eventArgs, this);
                UEngine.Instance.UIManager.CaptureMouse(eventArgs, this);
                if (!IsPressed)
                    IsPressed = true;

                if(ClickType == EClickType.Press)
                {
                    try
                    {
                        OnClick(eventArgs);
                    }
                    catch (Exception)
                    {
                        IsPressed = false;
                        UEngine.Instance.UIManager.CaptureMouse(eventArgs, null);
                    }
                }
            }
        }
        private void TtButtonBase_MouseLeftButtonUp(object sender, TtRoutedEventArgs eventArgs)
        {
            if(ClickType != EClickType.Hover)
            {
                eventArgs.Handled = true;
                var shouldClick = IsPressed && ClickType == EClickType.Release;
                if (shouldClick)
                    OnClick(eventArgs);
                if (IsMouseCaptured)
                    UEngine.Instance.UIManager.CaptureMouse(eventArgs, null);
            }
        }

        public static readonly TtRoutedEvent ClickEvent = TtEventManager.RegisterRoutedEvent("Click", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtButtonBase));
        public event TtRoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        protected virtual void OnClick(TtRoutedEventArgs args)
        {
            var arg = UEngine.Instance.UIManager.QueryEventSync();
            //arg.Host = this.RootUIHost;
            arg.RoutedEvent = ClickEvent;
            arg.Source = this;
            unsafe
            {
                arg.InputEventPtr = args.InputEventPtr;
            }
            RaiseEvent(arg);
            UEngine.Instance.UIManager.ReleaseEventSync(arg);
        }

        bool HandleIsMouseOverChanged(TtRoutedEventArgs args)
        {
            if(ClickType == EClickType.Hover)
            {
                if(IsMouseOver)
                {
                    IsPressed = true;
                    OnClick(args);
                }
                else
                {
                    IsPressed = false;
                }
                return true;
            }
            return false;
        }
        private void TtButtonBase_MouseEnter(object sender, TtRoutedEventArgs eventArgs)
        {
            if(HandleIsMouseOverChanged(eventArgs))
            {
                eventArgs.Handled = true;
            }
        }

        private void TtButtonBase_MouseLeave(object sender, TtRoutedEventArgs eventArgs)
        {
            if(HandleIsMouseOverChanged(eventArgs))
            {
                eventArgs.Handled = true;
            }
        }

        private void TtButtonBase_MouseMove(object sender, TtRoutedEventArgs eventArgs)
        {
            if((ClickType != EClickType.Hover) && 
                IsMouseCaptured && 
                UEngine.Instance.InputSystem.Mouse.IsMouseButtonDown(Bricks.Input.EMouseButton.BUTTON_LEFT) && 
                !IsSpaceKeyDown)
            {
                UpdateIsPressed();
                eventArgs.Handled = true;
            }
        }
        private unsafe void TtButtonBase_KeyDown(object sender, TtRoutedEventArgs eventArgs)
        {
            if (ClickType == EClickType.Hover)
                return;
            if (eventArgs.InputEventPtr == null)
                return;
            if (eventArgs.InputEventPtr->Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_SPACE)
            {
                if (!UEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_LALT) &&
                   !UEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_RALT))
                {
                    if (!IsMouseCaptured && (eventArgs.Source == this))
                    {
                        IsSpaceKeyDown = true;
                        IsPressed = true;
                        UEngine.Instance.UIManager.CaptureMouse(eventArgs, this);

                        if (ClickType == EClickType.Press)
                            OnClick(eventArgs);

                        eventArgs.Handled = true;
                    }
                }
            }
            else if (eventArgs.InputEventPtr->Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_KP_ENTER)
            {
                if (eventArgs.Source == this)
                {
                    IsSpaceKeyDown = false;
                    IsPressed = false;
                    UEngine.Instance.UIManager.CaptureMouse(eventArgs, null);

                    OnClick(eventArgs);
                    eventArgs.Handled = true;
                }
            }
            else
            {
                if (IsSpaceKeyDown)
                {
                    IsPressed = false;
                    IsSpaceKeyDown = false;
                    if (IsMouseCaptured)
                        UEngine.Instance.UIManager.CaptureMouse(eventArgs, null);
                }
            }
        }
        private unsafe void TtButtonBase_KeyUp(object sender, TtRoutedEventArgs eventArgs)
        {
            if (ClickType == EClickType.Hover)
                return;
            if (eventArgs.InputEventPtr == null)
                return;
            if(eventArgs.InputEventPtr->Keyboard.Keysym.Sym == Bricks.Input.Keycode.KEY_SPACE && IsSpaceKeyDown)
            {
                if (!UEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_LALT) &&
                    !UEngine.Instance.InputSystem.IsKeyDown(Bricks.Input.Keycode.KEY_RALT))
                {
                    IsSpaceKeyDown = false;
                    if(UEngine.Instance.InputSystem.Mouse.IsMouseButtonUp(Bricks.Input.EMouseButton.BUTTON_LEFT))
                    {
                        var shouldClick = IsPressed && (ClickType == EClickType.Release);
                        if (IsMouseCaptured)
                            UEngine.Instance.UIManager.CaptureMouse(eventArgs, null);
                        if (shouldClick)
                            OnClick(eventArgs);
                    }
                    else
                    {
                        if (IsMouseCaptured)
                            UpdateIsPressed();
                    }
                    eventArgs.Handled = true;
                }
            }
        }
        void UpdateIsPressed()
        {
            if(IsMouseOver)
            {
                if (!IsPressed)
                    IsPressed = true;
            }
            else if(IsPressed)
            {
                IsPressed = false;
            }
        }
        private void TtButtonBase_OnLostFocus(object sender, TtRoutedEventArgs args)
        {
            if (ClickType == EClickType.Hover)
                return;

            if(args.Source == this)
            {
                if (IsPressed)
                    IsPressed = false;

                if (IsMouseCaptured)
                    UEngine.Instance.UIManager.CaptureMouse(args, null);

                IsSpaceKeyDown = false;
            }
        }

        private void TtButtonBase_OnLostMouseCapture(object sender, TtRoutedEventArgs args)
        {
            if((args.Source == this) && (ClickType != EClickType.Hover) && !IsSpaceKeyDown)
            {
                if (IsKeyboardFocused)
                    UEngine.Instance.UIManager.KeyboardFocus(args, null);

                IsPressed = false;
            }
        }
    }

    [Editor_UIControl("Controls.Button", "Button", "")]
    public partial class TtButton : TtButtonBase
    {
        public TtButton()
        {
            Template = UEngine.Instance.UIManager.GetDefaultTemplate(UTypeDesc.TypeOf(typeof(TtButton)));
            Template.Seal();

            //UEngine.Instance.UIManager.RegisterTickElement(this);
        }

        static TtButton()
        {
            InitSystemDefaultTemplate();
        }

        static void InitSystemDefaultTemplate()
        {
            var buttonTemplate = new Template.TtControlTemplate()
            {
                TargetType = UTypeDesc.TypeOf(typeof(TtButton)),
            };
            // 设置属性的默认值
            buttonTemplate.DefaultValues.Add(new Template.TtTemplateSimpleValue(new TtBrush(Color4b.White, TtBrush.EBrushType.Rectangle), TtButton.BackgroundProperty));
            buttonTemplate.DefaultValues.Add(new Template.TtTemplateSimpleValue(new TtBrush(Color4b.Tomato, TtBrush.EBrushType.Border), TtButton.BorderBrushProperty));

            // 添加模板控件
            var buttonRoot = new Template.TtUIElementFactory(UTypeDesc.TypeOf(typeof(Controls.Containers.TtBorder)));
            buttonRoot.SetValue(Controls.Containers.TtBorder.NameProperty, "border");
            buttonRoot.SetTemplateBindingValue<TtBrush, TtBrush>(Controls.Containers.TtBorder.BackgroundProperty, "Background", "Background");
            buttonRoot.SetTemplateBindingValue<TtBrush, TtBrush>(Controls.Containers.TtBorder.BorderBrushProperty, "BorderBrush", "BorderBrush");
            buttonRoot.SetTemplateBindingValue<Thickness, Thickness>(Controls.Containers.TtBorder.BorderThicknessProperty, "BorderThickness", "BorderThickness");
            buttonTemplate.TemplateRoot = buttonRoot;
            var content = new Template.TtUIElementFactory(UTypeDesc.TypeOf(typeof(Controls.TtContentsPresenter)));
            content.SetValue(Controls.TtContentsPresenter.NameProperty, "contentPresenter");
            content.SetTemplateBindingValue<Thickness, Thickness>(Controls.TtContentsPresenter.MarginProperty, "Margin", "Padding");
            content.SetValue(Controls.Containers.TtBorder.HorizontalAlignmentProperty, Controls.HorizontalAlignment.Center);
            content.SetValue(Controls.Containers.TtBorder.VerticalAlignmentProperty, Controls.VerticalAlignment.Center);
            buttonRoot.AppendChild(content);
            TtUIManager.SystemDefaultTemplates[UTypeDesc.TypeOf(typeof(TtButton))] = buttonTemplate;

            // 设置Trigger
            var proTrigger = new Trigger.TtUIPropertyTrigger();
            proTrigger.AddCondition(TtButton.IsEnabledProperty, false);
            proTrigger.AddTriggerValue(TtBorder.BackgroundProperty, new TtBrush(Color4b.Gray, TtBrush.EBrushType.Rectangle), "border");
            proTrigger.AddTriggerValue(TtBorder.BorderBrushProperty, new TtBrush(Color4b.DarkGray, TtBrush.EBrushType.Rectangle), "border");
            buttonTemplate.AddTrigger(proTrigger);

            var mouseOverTrigger = new Trigger.TtUIPropertyTrigger();
            mouseOverTrigger.AddCondition(TtButton.IsMouseOverProperty, true);
            mouseOverTrigger.AddTriggerValue(TtBorder.BackgroundProperty, new TtBrush(Color4b.GreenYellow, TtBrush.EBrushType.Rectangle), "border");
            mouseOverTrigger.AddTriggerValue(TtBorder.BorderBrushProperty, new TtBrush(Color4b.Yellow, TtBrush.EBrushType.Rectangle), "border");
            buttonTemplate.AddTrigger(mouseOverTrigger);

            var pressedTrigger = new Trigger.TtUIPropertyTrigger();
            pressedTrigger.AddCondition(TtButton.IsPressedProperty, true);
            pressedTrigger.AddTriggerValue(TtBorder.BackgroundProperty, new TtBrush(Color4b.Red, TtBrush.EBrushType.Rectangle), "border");
            pressedTrigger.AddTriggerValue(TtBorder.BorderBrushProperty, new TtBrush(Color4b.IndianRed, TtBrush.EBrushType.Rectangle), "border");
            buttonTemplate.AddTrigger(pressedTrigger);
        }

        public override void Tick(float elapsedSecond)
        {
        }
    }
}
