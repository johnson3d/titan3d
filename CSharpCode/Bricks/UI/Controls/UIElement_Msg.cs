using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Controls
{
    public class TtRoutedEventArgs : IPooledObject
    {
        public bool IsAlloc { get; set; } = false;
        public TtUIElement Source = null;
        public TtUIHost Host = null;
        public bool Handled = false;
    }

    public partial class TtUIElement
    {
        public delegate void Delegate_MouseEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
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

        public delegate void Delegate_TouchEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
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
        public delegate void Delegate_DeviceEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
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

        public virtual void ProcessMouseLeave(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        {
            if (OnMouseLeave != null)
                OnMouseLeave.Invoke(this, eventArgs, e);
            if (OnDeviceLeave != null)
                OnDeviceLeave.Invoke(this, eventArgs, e);
        }
        public virtual void ProcessMouseEnter(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        {
            if (OnMouseEnter != null)
                OnMouseEnter.Invoke(this, eventArgs, e);
            if (OnDeviceEnter != null)
                OnDeviceEnter.Invoke(this, eventArgs, e);
        }
        public virtual void ProcessTouchLeave(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        {
            if (OnTouchLeave != null)
                OnTouchLeave.Invoke(this, eventArgs, e);
            if (OnDeviceLeave != null)
                OnDeviceLeave.Invoke(this, eventArgs, e);
        }
        public virtual void ProcessTouchEnter(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        {
            if (OnTouchEnter != null)
                OnTouchEnter.Invoke(this, eventArgs, e);
            if (OnDeviceEnter != null)
                OnDeviceEnter.Invoke(this, eventArgs, e);
        }

        public virtual void ProcessMessage(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        {
            switch (e.Type)
            {
                case EngineNS.Bricks.Input.EventType.MOUSEBUTTONDOWN:
                    {
                        if (OnMouseButtonDown != null)
                            OnMouseButtonDown.Invoke(this, eventArgs, e);
                        if (OnDeviceDown != null)
                            OnDeviceDown.Invoke(this, eventArgs, e);

                        if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_LEFT)
                        {
                            if (OnMouseLeftButtonDown != null)
                                OnMouseLeftButtonDown.Invoke(this, eventArgs, e);
                        }
                        else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_RIGHT)
                        {
                            if (OnMouseRightButtonDown != null)
                                OnMouseRightButtonDown.Invoke(this, eventArgs, e);
                        }
                        else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                        {
                            if (OnMouseMiddleButtonDown != null)
                                OnMouseMiddleButtonDown.Invoke(this, eventArgs, e);
                        }
                    }
                    break;
                case EngineNS.Bricks.Input.EventType.MOUSEBUTTONUP:
                    {
                        if (OnMouseButtonUp != null)
                            OnMouseButtonUp.Invoke(this, eventArgs, e);
                        if (OnDeviceUp != null)
                            OnDeviceUp.Invoke(this, eventArgs, e);

                        if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_LEFT)
                        {
                            if (OnMouseLeftButtonUp != null)
                                OnMouseLeftButtonUp.Invoke(this, eventArgs, e);
                        }
                        else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_RIGHT)
                        {
                            if (OnMouseRightButtonUp != null)
                                OnMouseRightButtonUp.Invoke(this, eventArgs, e);
                        }
                        else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_MIDDLE)
                        {
                            if (OnMouseMiddleButtonUp != null)
                                OnMouseMiddleButtonUp.Invoke(this, eventArgs, e);
                        }
                    }
                    break;
                case EngineNS.Bricks.Input.EventType.MOUSEMOTION:
                    {
                        if (OnMouseMove != null)
                            OnMouseMove.Invoke(this, eventArgs, e);
                        if (OnDeviceMove != null)
                            OnDeviceMove.Invoke(this, eventArgs, e);
                    }
                    break;
                case EngineNS.Bricks.Input.EventType.MOUSEWHEEL:
                    {
                        if (OnMouseWheel != null)
                            OnMouseWheel.Invoke(this, eventArgs, e);
                    }
                    break;
                case EngineNS.Bricks.Input.EventType.CONTROLLERTOUCHPADDOWN:
                    {
                        if (OnTouchDown != null)
                            OnTouchDown.Invoke(this, eventArgs, e);
                        if (OnDeviceDown != null)
                            OnDeviceDown.Invoke(this, eventArgs, e);
                    }
                    break;
                case EngineNS.Bricks.Input.EventType.CONTROLLERTOUCHPADUP:
                    {
                        if (OnTouchUp != null)
                            OnTouchUp.Invoke(this, eventArgs, e);
                        if (OnDeviceUp != null)
                            OnDeviceUp.Invoke(this, eventArgs, e);
                    }
                    break;
                case EngineNS.Bricks.Input.EventType.CONTROLLERTOUCHPADMOTION:
                    {
                        if (OnTouchMove != null)
                            OnTouchMove.Invoke(this, eventArgs, e);
                        if (OnDeviceMove != null)
                            OnDeviceMove.Invoke(this, eventArgs, e);
                    }
                    break;
            }

            if (!eventArgs.Handled)
                Parent?.ProcessMessage(eventArgs, e);
        }
    }

}
