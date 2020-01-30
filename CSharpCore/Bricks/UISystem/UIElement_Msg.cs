using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem
{
    public class RoutedEventArgs
    {
        public UIElement Source = null;
        public bool Handled = false;
        public UIHost UIHost = null;

        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable | EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Input.Device.DeviceType DeviceType;
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable| EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public Input.Device.IDeviceEventArgs DeviceEventArgs;
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: EngineNS.Editor.Editor_TypeChangeWithParam(0)]
        public Input.Device.IDeviceEventArgs GetDeviceEventArgs(
            [EngineNS.Editor.Editor_TypeFilterAttribute(typeof(Input.Device.IDeviceEventArgs))]
            Type type)
        {
            return DeviceEventArgs;
        }
    }

    public partial class UIElement
    {
        public delegate void Delegate_MouseEvent(UIElement ui, RoutedEventArgs eventArgs, EngineNS.Input.Device.Mouse.MouseEventArgs mouseEvent);
        [Editor_UIEvent("鼠标进入时调用的方法")]
        public Delegate_MouseEvent OnMouseEnter;
        [Editor_UIEvent("鼠标离开时调用的方法")]
        public Delegate_MouseEvent OnMouseLeave;
        [Editor_UIEvent("鼠标按键按下时调用的方法")]
        public Delegate_MouseEvent OnMouseButtonDown;
        [Editor_UIEvent("鼠标左键按下时调用的方法")]
        public Delegate_MouseEvent OnMouseLeftButtonDown;
        [Editor_UIEvent("鼠标右键按下时调用的方法")]
        public Delegate_MouseEvent OnMouseRightButtonDown;
        [Editor_UIEvent("鼠标中键按下时调用的方法")]
        public Delegate_MouseEvent OnMouseMiddleButtonDown;
        [Editor_UIEvent("鼠标按键弹起时调用的方法")]
        public Delegate_MouseEvent OnMouseButtonUp;
        [Editor_UIEvent("鼠标左键弹起时调用的方法")]
        public Delegate_MouseEvent OnMouseLeftButtonUp;
        [Editor_UIEvent("鼠标右键弹起时调用的方法")]
        public Delegate_MouseEvent OnMouseRightButtonUp;
        [Editor_UIEvent("鼠标中键弹起时调用的方法")]
        public Delegate_MouseEvent OnMouseMiddleButtonUp;
        [Editor_UIEvent("鼠标移动时调用的方法")]
        public Delegate_MouseEvent OnMouseMove;
        [Editor_UIEvent("鼠标滚轮滚动时调用的方法")]
        public Delegate_MouseEvent OnMouseWheel;

        public delegate void Delegate_TouchEvent(UIElement ui, RoutedEventArgs eventArgs, EngineNS.Input.Device.TouchDevice.TouchEventArgs touchEvent);
        [Editor_UIEvent("触摸进入时调用的方法")]
        public Delegate_TouchEvent OnTouchEnter;
        [Editor_UIEvent("触摸离开时调用的方法")]
        public Delegate_TouchEvent OnTouchLeave;
        [Editor_UIEvent("触摸按下时调用的方法")]
        public Delegate_TouchEvent OnTouchDown;
        [Editor_UIEvent("触摸弹起时调用的方法")]
        public Delegate_TouchEvent OnTouchUp;
        [Editor_UIEvent("触摸移动时调用的方法")]
        public Delegate_TouchEvent OnTouchMove;

        // 不区分设备
        public delegate void Delegate_DeviceEvent(UIElement ui, RoutedEventArgs eventArgs);
        [Editor_UIEvent("进入时调用的方法")]
        public Delegate_DeviceEvent OnDeviceEnter;
        [Editor_UIEvent("离开时调用的方法")]
        public Delegate_DeviceEvent OnDeviceLeave;
        [Editor_UIEvent("按下时调用的方法")]
        public Delegate_DeviceEvent OnDeviceDown;
        [Editor_UIEvent("弹起时调用的方法")]
        public Delegate_DeviceEvent OnDeviceUp;
        [Editor_UIEvent("移动时调用的方法")]
        public Delegate_DeviceEvent OnDeviceMove;

        public virtual void ProcessMouseLeave(RoutedEventArgs eventArgs)
        {
            if (OnMouseLeave != null)
                OnMouseLeave.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
            if (OnDeviceLeave != null)
                OnDeviceLeave.Invoke(this, eventArgs);
        }
        public virtual void ProcessMouseEnter(RoutedEventArgs eventArgs)
        {
            if(OnMouseEnter != null)
                OnMouseEnter.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
            if (OnDeviceEnter != null)
                OnDeviceEnter.Invoke(this, eventArgs);
        }
        public virtual void ProcessTouchLeave(RoutedEventArgs eventArgs)
        {
            if (OnTouchLeave != null)
                OnTouchLeave.Invoke(this, eventArgs, (EngineNS.Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs);
            if (OnDeviceLeave != null)
                OnDeviceLeave.Invoke(this, eventArgs);
        }
        public virtual void ProcessTouchEnter(RoutedEventArgs eventArgs)
        {
            if(OnTouchEnter != null)
                OnTouchEnter.Invoke(this, eventArgs, (EngineNS.Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs);
            if (OnDeviceEnter != null)
                OnDeviceEnter.Invoke(this, eventArgs);
        }

        public virtual void ProcessMessage(RoutedEventArgs eventArgs)
        {
            switch(eventArgs.DeviceType)
            {
                case Input.Device.DeviceType.Mouse:
                    {
                        var mouseArg = (Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs;
                        switch(mouseArg.State)
                        {
                            case Input.Device.Mouse.ButtonState.Down:
                                {
                                    if (OnMouseButtonDown != null)
                                        OnMouseButtonDown.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    if (OnDeviceDown != null)
                                        OnDeviceDown.Invoke(this, eventArgs);

                                    if (mouseArg.Button == Input.Device.Mouse.MouseButtons.Left)
                                    {
                                        if (OnMouseLeftButtonDown != null)
                                            OnMouseLeftButtonDown.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                    else if(mouseArg.Button == Input.Device.Mouse.MouseButtons.Right)
                                    {
                                        if (OnMouseRightButtonDown != null)
                                            OnMouseRightButtonDown.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                    else if(mouseArg.Button == Input.Device.Mouse.MouseButtons.Middle)
                                    {
                                        if (OnMouseMiddleButtonDown != null)
                                            OnMouseMiddleButtonDown.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                }
                                break;
                            case Input.Device.Mouse.ButtonState.Up:
                                {
                                    if(OnMouseButtonUp != null)
                                        OnMouseButtonUp.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    if (OnDeviceUp != null)
                                        OnDeviceUp.Invoke(this, eventArgs);

                                    if(mouseArg.Button == Input.Device.Mouse.MouseButtons.Left)
                                    {
                                        if (OnMouseLeftButtonUp != null)
                                            OnMouseLeftButtonUp.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                    else if(mouseArg.Button == Input.Device.Mouse.MouseButtons.Right)
                                    {
                                        if (OnMouseRightButtonUp != null)
                                            OnMouseRightButtonUp.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                    else if(mouseArg.Button == Input.Device.Mouse.MouseButtons.Middle)
                                    {
                                        if (OnMouseMiddleButtonUp != null)
                                            OnMouseMiddleButtonUp.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    }
                                }
                                break;
                            case Input.Device.Mouse.ButtonState.Move:
                                {
                                    if (OnMouseMove != null)
                                        OnMouseMove.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
                                    if (OnDeviceMove != null)
                                        OnDeviceMove.Invoke(this, eventArgs);
                                }
                                break;
                            case Input.Device.Mouse.ButtonState.WheelScroll:
                                {
                                    if (OnMouseWheel != null)
                                        OnMouseWheel.Invoke(this, eventArgs, (EngineNS.Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs);
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
                                {
                                    if (OnTouchDown != null)
                                        OnTouchDown.Invoke(this, eventArgs, (EngineNS.Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs);
                                    if (OnDeviceDown != null)
                                        OnDeviceDown.Invoke(this, eventArgs);
                                }
                                break;
                            case Input.Device.TouchDevice.enTouchState.Up:
                                {
                                    if (OnTouchUp != null)
                                        OnTouchUp.Invoke(this, eventArgs, (EngineNS.Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs);
                                    if (OnDeviceUp != null)
                                        OnDeviceUp.Invoke(this, eventArgs);
                                }
                                break;
                            case Input.Device.TouchDevice.enTouchState.Move:
                                {
                                    if (OnTouchMove != null)
                                        OnTouchMove.Invoke(this, eventArgs, (EngineNS.Input.Device.TouchDevice.TouchEventArgs)eventArgs.DeviceEventArgs);
                                    if (OnDeviceMove != null)
                                        OnDeviceMove.Invoke(this, eventArgs);
                                }
                                break;
                        }
                    }
                    break;
            }

            if(!eventArgs.Handled)
                Parent?.ProcessMessage(eventArgs);
        }
    }
}
