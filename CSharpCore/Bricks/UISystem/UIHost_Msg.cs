using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem
{
    public partial class UIHost
    {
        // 模态对话框,在模态对话框关闭完之前不处理当前模态对话框之外的消息
        public Stack<EngineNS.UISystem.Controls.UserControl> mDialogForms = new Stack<Controls.UserControl>();
        PooledObject<RoutedEventArgs> mRoutedEventPool = new PooledObject<RoutedEventArgs>();

        public bool IsInputActive = false;   // true才能接受消息

        UIElement mKeyboardFocusUIElement = null;
        // 捕获的对象
        UIElement[] mCapturedElements = new UIElement[Input.InputServer.MaxMultiTouchNumber + 1];   // 最后一位是鼠标
        UIElement[] mHoveredElements = new UIElement[Input.InputServer.MaxMultiTouchNumber + 1];    // 最后一位是鼠标

        public void CaptureMouse(UIElement item)
        {
            mCapturedElements[Input.InputServer.MaxMultiTouchNumber] = item;
        }
        public void CaptureTouch(int index, UIElement item)
        {
            if (index < 0 || index > Input.InputServer.MaxMultiTouchNumber)
                return;
            mCapturedElements[index] = item;
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void DispatchInputEvent(Input.Device.DeviceInputEventArgs inputArgs)
        {
            unsafe
            {
                UIElement procWin = this;
                if (mDialogForms.Count > 0)
                    procWin = mDialogForms.Peek();

                switch (inputArgs.DeviceType)
                {
                    case Input.Device.DeviceType.Keyboard:
                        {
                            var kbInputArgs = inputArgs as Input.Device.Keyboard.KeyboardInputEventArgs;
                            // 处理快捷键

                            // 处理焦点控件
                            var eventArg = mRoutedEventPool.QueryObjectSync();
                            eventArg.DeviceType = kbInputArgs.DeviceType;
                            eventArg.DeviceEventArgs = kbInputArgs.KeyboardEvent;
                            eventArg.UIHost = this;
                            if (mKeyboardFocusUIElement != null)
                            {
                                // 键盘焦点对象处理
                                eventArg.Source = mKeyboardFocusUIElement;
                                mKeyboardFocusUIElement.ProcessMessage(eventArg);
                            }
                            else
                            {
                                eventArg.Source = procWin;
                                procWin.ProcessMessage(eventArg);
                            }
                            mRoutedEventPool.ReleaseObject(eventArg);
                        }
                        break;
                    case Input.Device.DeviceType.Mouse:
                        {
                            var mouseInputArgs = inputArgs as Input.Device.Mouse.MouseInputEventArgs;

                            var eventArg = mRoutedEventPool.QueryObjectSync();
                            eventArg.DeviceType = mouseInputArgs.DeviceType;
                            var evt = mouseInputArgs.MouseEvent;
                            evt.X = (int)(evt.X / mDpiScale);
                            evt.Y = (int)(evt.Y / mDpiScale);
                            eventArg.DeviceEventArgs = evt;
                            eventArg.UIHost = this;

                            CheckHoveredElement(eventArg);
                            if (mCapturedElements[Input.InputServer.MaxMultiTouchNumber] != null)
                            {
                                // 鼠标捕获对象处理
                                eventArg.Source = mCapturedElements[Input.InputServer.MaxMultiTouchNumber];
                                mCapturedElements[Input.InputServer.MaxMultiTouchNumber].ProcessMessage(eventArg);
                            }
                            else if (mHoveredElements[Input.InputServer.MaxMultiTouchNumber] != null)
                            {
                                // 焦点对象处理
                                eventArg.Source = mHoveredElements[Input.InputServer.MaxMultiTouchNumber];
                                mHoveredElements[Input.InputServer.MaxMultiTouchNumber].ProcessMessage(eventArg);
                            }
                            else
                            {
                                eventArg.Source = procWin;
                                procWin.ProcessMessage(eventArg);
                            }
                            mRoutedEventPool.ReleaseObject(eventArg);
                        }
                        break;
                    case Input.Device.DeviceType.Touch:
                        {
                            var touchInputArgs = inputArgs as Input.Device.TouchDevice.TouchInputEventArgs;

                            int fingerIdx = touchInputArgs.TouchEvent.FingerIdx;
                            var eventArg = mRoutedEventPool.QueryObjectSync();
                            eventArg.DeviceType = touchInputArgs.DeviceType;
                            eventArg.DeviceEventArgs = touchInputArgs.TouchEvent;
                            eventArg.UIHost = this;

                            CheckHoveredElement(eventArg);
                            bool processed = false;
                            if (mCapturedElements[fingerIdx] != null)
                            {
                                // 捕获对象处理
                                var ui = mCapturedElements[fingerIdx];
                                eventArg.Source = ui;
                                ui.ProcessMessage(eventArg);
                                processed = true;
                            }
                            if(!processed)
                            {
                                if (mHoveredElements[fingerIdx] != null)
                                {
                                    // 焦点对象处理
                                    var ui = mHoveredElements[fingerIdx];
                                    eventArg.Source = ui;
                                    ui.ProcessMessage(eventArg);
                                    processed = true;
                                }
                            }
                            if (!processed)
                            {
                                eventArg.Source = procWin;
                                procWin.ProcessMessage(eventArg);
                            }
                            mRoutedEventPool.ReleaseObject(eventArg);
                        }
                        break;
                }
            }
        }

        void ProcessInputDown(int index, RoutedEventArgs eventArgs)
        {
            var deviceEventArg = eventArgs.DeviceEventArgs;
            var pt = new PointF(deviceEventArg.PointX, deviceEventArg.PointY);
            UIElement rootWin = this;
            if(mDialogForms.Count > 0)
            {
                rootWin = mDialogForms.Peek();
            }

            var popStay = PopupedUIElement(ref pt);
            UIElement newStay = null;
            if (popStay != null)
                newStay = popStay.GetPointAtElement(ref pt);
            else
                newStay = rootWin.GetPointAtElement(ref pt);

            var focusUIElement = mHoveredElements[index];
            if(newStay != focusUIElement)
            {
                if (focusUIElement != null)
                {
                    var arg = mRoutedEventPool.QueryObjectSync();
                    arg.Source = focusUIElement;
                    arg.UIHost = this;
                    var parent = focusUIElement;
                    if (index == Input.InputServer.MaxMultiTouchNumber)
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessMouseLeave(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessTouchLeave(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    mRoutedEventPool.ReleaseObject(arg);
                }
                if (newStay != null)
                {
                    var arg = mRoutedEventPool.QueryObjectSync();
                    arg.Source = newStay;
                    arg.UIHost = this;
                    var parent = newStay;
                    if (index == Input.InputServer.MaxMultiTouchNumber)
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessMouseEnter(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessTouchEnter(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    mRoutedEventPool.ReleaseObject(arg);
                }

                mHoveredElements[index] = newStay;
            }
        }
        void ProcessInputMove(int index, RoutedEventArgs eventArgs)
        {
            var deviceEventArg = eventArgs.DeviceEventArgs;
            var pt = new PointF(deviceEventArg.PointX, deviceEventArg.PointY);

            UIElement rootWin = this;
            if(mDialogForms.Count > 0)
            {
                rootWin = mDialogForms.Peek();
            }

            var popStay = PopupedUIElement(ref pt);
            UIElement newStay = null;
            if (popStay != null)
                newStay = popStay.GetPointAtElement(ref pt);
            else
                newStay = rootWin.GetPointAtElement(ref pt);

            var focusElem = mHoveredElements[index];
            if(newStay != focusElem)
            {
                if(focusElem != null)
                {
                    var arg = mRoutedEventPool.QueryObjectSync();
                    arg.Source = focusElem;
                    arg.UIHost = this;
                    var parent = focusElem;
                    if(index == Input.InputServer.MaxMultiTouchNumber)
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessMouseLeave(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        while(!arg.Handled && parent != null)
                        {
                            parent.ProcessTouchLeave(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    mRoutedEventPool.ReleaseObject(arg);
                }
                if(newStay != null)
                {
                    var arg = mRoutedEventPool.QueryObjectSync();
                    arg.Source = newStay;
                    arg.UIHost = this;
                    var parent = newStay;
                    if(index == Input.InputServer.MaxMultiTouchNumber)
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessMouseEnter(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        while (!arg.Handled && parent != null)
                        {
                            parent.ProcessTouchEnter(eventArgs);
                            parent = parent.Parent;
                        }
                    }
                    mRoutedEventPool.ReleaseObject(arg);
                }
                mHoveredElements[index] = newStay;
            }
        }

        void CheckHoveredElement(RoutedEventArgs eventArgs)
        {
            switch(eventArgs.DeviceType)
            {
                case Input.Device.DeviceType.Mouse:
                    {
                        var mouseArg = (Input.Device.Mouse.MouseEventArgs)eventArgs.DeviceEventArgs;
                        switch (mouseArg.State)
                        {
                            case Input.Device.Mouse.ButtonState.Down:
                                {
                                    ProcessInputDown(Input.InputServer.MaxMultiTouchNumber, eventArgs);
                                }
                                break;
                            case Input.Device.Mouse.ButtonState.Move:
                                {
                                    ProcessInputMove(Input.InputServer.MaxMultiTouchNumber, eventArgs);
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
                                    ProcessInputDown(touchArg.FingerIdx, eventArgs);
                                }
                                break;
                            case Input.Device.TouchDevice.enTouchState.Move:
                                {
                                    ProcessInputMove(touchArg.FingerIdx, eventArgs);
                                }
                                break;
                        }
                    }
                    break;
            }
        }
    }
}
