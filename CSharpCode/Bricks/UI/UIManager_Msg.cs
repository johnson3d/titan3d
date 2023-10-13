using EngineNS;
using EngineNS.Bricks.Input;
using EngineNS.UI.Controls;
using EngineNS.UI.Event;
using NPOI.HSSF.Record.AutoFilter;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIManager : IEventProcessor
    {
        void UIManagerConstruct_Msg()
        {
            UEngine.Instance.EventProcessorManager.RegProcessor(this);
        }

        // 模态对话框
        internal Stack<TtUIHost> mDialogHosts = new Stack<TtUIHost>();
        internal List<TtUIHost> mPopupHosts = new List<TtUIHost>();
        TtObjectPool<TtRoutedEventArgs> mEventPool = new TtObjectPool<TtRoutedEventArgs>();
        public TtRoutedEventArgs QueryEventSync()
        {
            return mEventPool.QueryObjectSync();
        }
        public void ReleaseEventSync(TtRoutedEventArgs arg)
        {
            mEventPool.ReleaseObject(arg);
            arg.Reset();
        }

        TtObjectPool<Event.TtEventRoute> mRoutePool = new TtObjectPool<Event.TtEventRoute>();
        internal Event.TtEventRoute QueryRouteSync(TtRoutedEvent routedEvent)
        {
            var retVal = mRoutePool.QueryObjectSync();
            retVal.RoutedEvent = routedEvent;
            return retVal;
        }
        internal void ReleaseRouteSync(Event.TtEventRoute route)
        {
            route.Clear();
            mRoutePool.ReleaseObject(route);
        }


        TtUIHost mActiveHost = null;
        TtUIElement mKeyboardFocusUIElement = null;
        public TtUIElement KeyboardFocusUIElement => mKeyboardFocusUIElement;
        TtUIElement[] mCapturedElements = new TtUIElement[UInputSystem.MaxMultiTouchNumber + 1];
        TtUIElement[] mHoveredElements = new TtUIElement[UInputSystem.MaxMultiTouchNumber + 1];
        public bool IsMouseDirectlyHover(TtUIElement element)
        {
            return mHoveredElements[UInputSystem.MaxMultiTouchNumber] == element;
        }
        public bool IsTouchDirectlyHover(TtUIElement element)
        {
            for(int i=0; i<UInputSystem.MaxMultiTouchNumber; i++)
            {
                if (mHoveredElements[i] == element)
                    return true;
            }
            return false;
        }

        public bool IsKeyboardFocusable(TtUIElement element)
        {
            if (element == null)
                return false;
            if (element.IsEnabled == false)
                return false;
            if(element.Visibility != Visibility.Visible) 
                return false;

            return element.IsFocusable;
        }
        public void KeyboardFocus(TtRoutedEventArgs eventArgs, TtUIElement element)
        {
            if(element == null)
            {
                if(mKeyboardFocusUIElement != null)
                    mKeyboardFocusUIElement.ProcessOnLostFocus(eventArgs, element, mKeyboardFocusUIElement);
            }
            else if(IsKeyboardFocusable(element))
            {
                if (mKeyboardFocusUIElement != null)
                    mKeyboardFocusUIElement.ProcessOnLostFocus(eventArgs, element, mKeyboardFocusUIElement);
                element.ProcessOnFocus(eventArgs, element, mKeyboardFocusUIElement);
                mKeyboardFocusUIElement = element;
            }
            else if(element.TemplateChildIndex != -1)
            {
                if (mKeyboardFocusUIElement != null)
                    mKeyboardFocusUIElement.ProcessOnLostFocus(eventArgs, element, mKeyboardFocusUIElement);
                element.ProcessOnFocus(eventArgs, element, mKeyboardFocusUIElement);
                mKeyboardFocusUIElement = element.Parent;
            }
        }

        public TtUIElement MouseCaptured => mCapturedElements[UInputSystem.MaxMultiTouchNumber];
        public void CaptureMouse(TtRoutedEventArgs eventArgs, TtUIElement element)
        {
            var old = mCapturedElements[UInputSystem.MaxMultiTouchNumber];
            if (old != null)
                old.ProcessOnLostMouseCapture(eventArgs, element, old);
            mCapturedElements[UInputSystem.MaxMultiTouchNumber] = element;
            if (element != null)
                element.ProcessOnMouseCapture(eventArgs, element, old);
        }
        public void CaptureTouch(int index, TtUIElement element)
        {
            if (index < 0 || index > UInputSystem.MaxMultiTouchNumber)
                return;
            mCapturedElements[index] = element;
        }

        bool UIElementIsContainOrEqualSource(TtUIElement source, TtUIElement checkedElement)
        {
            if (source == null) 
                return false;
            if (checkedElement == null)
                return false;
            if (source == checkedElement)
                return true;
            return UIElementIsContainOrEqualSource(source.Parent, checkedElement);
        }
        ///////////////////////////////////////////
        public Vector2 DebugMousePt;
        public Vector2 DebugHitPt;
        ///////////////////////////////////////////
        TtUIElement CheckHoveredElement(in Bricks.Input.Event e)
        {
            Vector2 pt;
            long index;
            switch(e.Type)
            {
                case EventType.MOUSEBUTTONDOWN:
                case EventType.MOUSEBUTTONUP:
                case EventType.MOUSEMOTION:
                    {
                        pt = new Vector2(e.MouseButton.X, e.MouseButton.Y);
                        index = UInputSystem.MaxMultiTouchNumber;
                    }
                    break;
                case EventType.CONTROLLERTOUCHPADDOWN:
                case EventType.CONTROLLERTOUCHPADUP:
                case EventType.CONTROLLERTOUCHPADMOTION:
                    {
                        pt = new Vector2(e.TouchFinger.X, e.TouchFinger.Y);
                        index = e.TouchFinger.FingerId;
                    }
                    break;
                default:
                    return null;
            }
            if (mCapturedElements[index] != null)
                return mCapturedElements[index];

            //TtUIHost procWin = null;
            TtUIElement newStay = null;
            if(mPopupHosts.Count > 0)
            {
                // 后打开的在上面
                for(int i= mPopupHosts.Count - 1; i >= 0; i--)
                {
                    var element = mPopupHosts[i].GetPointAtElement(in pt);
                    if (element != null)
                    {
                        newStay = element;
                        break;
                    }
                }
            }
            if(mDialogHosts.Count > 0)
            {
                var procWin = mDialogHosts.Peek();
                newStay = procWin.GetPointAtElement(pt);
            }
            else
            {
                float minDistance = float.MaxValue;
                var data = new TtUIElement.RayIntersectData();
                for (int i = mUserUIList.Count - 1; i >= 0; i--)
                {
                    var ui = mUserUIList[i];
                    var element = ui.GetPointAtElement(in pt, ref data);
                    if (element != null && minDistance > data.Distance)
                    {
                        minDistance = data.Distance;
                        newStay = element;
                    }
                }
            }

            System.Diagnostics.Debug.Assert(index < mHoveredElements.Length);
            var hoveredUIElement = mHoveredElements[index];
            if(newStay != hoveredUIElement)
            {
                if(e.Type == EventType.MOUSEBUTTONDOWN ||
                   e.Type == EventType.CONTROLLERTOUCHPADDOWN)
                {
                    if (newStay != null)
                        mActiveHost = newStay.RootUIHost;
                    else
                        mActiveHost = null;
                }

                if (hoveredUIElement != null)
                {

                    var deviceLeaveArg = QueryEventSync();
                    deviceLeaveArg.Source = hoveredUIElement;
                    //deviceLeaveArg.Host = procWin;
                    deviceLeaveArg.RoutedEvent = TtUIElement.DeviceLeaveEvent;
                    unsafe
                    {
                        fixed(Bricks.Input.Event* ePtr = &e)
                            deviceLeaveArg.InputEventPtr = ePtr;
                    }

                    if(index == UInputSystem.MaxMultiTouchNumber)
                    {
                        var mouseDirLeaveArg = QueryEventSync();
                        ReleaseEventSync(mouseDirLeaveArg);

                        var mouseLeaveArg = QueryEventSync();
                        mouseLeaveArg.Source = hoveredUIElement;
                        //mouseLeaveArg.Host = procWin;
                        mouseLeaveArg.RoutedEvent = TtUIElement.MouseLeaveEvent;
                        unsafe
                        {
                            fixed (Bricks.Input.Event* ePtr = &e)
                                mouseLeaveArg.InputEventPtr = ePtr;
                        }

                        var parent = hoveredUIElement;
                        while(parent != null)
                        {
                            if (UIElementIsContainOrEqualSource(newStay, parent))
                            {
                                break;
                            }
                            else
                            {
                                parent.IsMouseOver = false;
                                parent.RaiseEvent(mouseLeaveArg);
                                parent.RaiseEvent(deviceLeaveArg);
                            }
                            parent = parent.Parent;
                        }

                        ReleaseEventSync(mouseLeaveArg);
                    }
                    else
                    {
                        var touchLeaveArg = QueryEventSync();
                        touchLeaveArg.Source = hoveredUIElement;
                        //touchLeaveArg.Host = procWin;
                        touchLeaveArg.RoutedEvent = TtUIElement.MouseLeaveEvent;
                        unsafe
                        {
                            fixed (Bricks.Input.Event* ePtr = &e)
                                touchLeaveArg.InputEventPtr = ePtr;
                        }
                        var parent = hoveredUIElement;
                        while(parent != null)
                        {
                            if (UIElementIsContainOrEqualSource(newStay, parent))
                            {
                                break;
                            }
                            else
                            {
                                parent.IsTouchOver = false;
                                parent.RaiseEvent(touchLeaveArg);
                                parent.RaiseEvent(deviceLeaveArg);
                            }
                            parent = parent.Parent;
                        }

                        ReleaseEventSync(touchLeaveArg);
                    }
                    ReleaseEventSync(deviceLeaveArg);
                }
                if(newStay != null)
                {
                    var deviceEnterArg = QueryEventSync();
                    deviceEnterArg.Source = hoveredUIElement;
                    //deviceEnterArg.Host = procWin;
                    deviceEnterArg.RoutedEvent = TtUIElement.DeviceEnterEvent;
                    unsafe
                    {
                        fixed (Bricks.Input.Event* ePtr = &e)
                            deviceEnterArg.InputEventPtr = ePtr;
                    }

                    if(index == UInputSystem.MaxMultiTouchNumber)
                    {
                        var mouseEnterArg = QueryEventSync();
                        mouseEnterArg.Source = hoveredUIElement;
                        //mouseEnterArg.Host = procWin;
                        mouseEnterArg.RoutedEvent = TtUIElement.MouseEnterEvent;
                        unsafe
                        {
                            fixed (Bricks.Input.Event* ePtr = &e)
                                mouseEnterArg.InputEventPtr = ePtr;
                        }

                        var parent = newStay;
                        while(parent != null)
                        {
                            if(parent.IsMouseOver == false)
                            {
                                parent.IsMouseOver = true;
                                parent.RaiseEvent(mouseEnterArg);
                                parent.RaiseEvent(deviceEnterArg);
                            }
                            parent = parent.Parent;
                        }
                        ReleaseEventSync(mouseEnterArg);
                    }
                    else
                    {
                        var touchEnterArg = QueryEventSync();
                        touchEnterArg.Source = hoveredUIElement;
                        //touchEnterArg.Host = procWin;
                        touchEnterArg.RoutedEvent = TtUIElement.MouseEnterEvent;
                        unsafe
                        {
                            fixed (Bricks.Input.Event* ePtr = &e)
                                touchEnterArg.InputEventPtr = ePtr;
                        }

                        var parent = newStay;
                        while(parent != null)
                        {
                            if(parent.IsTouchOver == false)
                            {
                                parent.IsTouchOver = true;
                                parent.RaiseEvent(touchEnterArg);
                                parent.RaiseEvent(deviceEnterArg);
                            }
                            parent = parent.Parent;
                        }
                        ReleaseEventSync(touchEnterArg);
                    }
                    ReleaseEventSync(deviceEnterArg);
                }
                mHoveredElements[index] = newStay;
            }
            return newStay;
        }
        public unsafe bool OnEvent(in Bricks.Input.Event e)
        {
            // todo: drag drop
            switch (e.Type)
            {
                case EventType.MOUSEBUTTONDOWN:
                    {
                        var element = CheckHoveredElement(e);
                        if (element != null)
                        {
                            var arg = QueryEventSync();
                            arg.Source = element;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                arg.InputEventPtr = ePtr;
                            //arg.Host = mActiveHost;
                            arg.RoutedEvent = TtUIElement.MouseButtonDownEvent;
                            element.RaiseEvent(arg);
                            ReleaseEventSync(arg);

                            var d_arg = QueryEventSync();
                            d_arg.Source = element;
                            //d_arg.Host = mActiveHost;
                            d_arg.RoutedEvent = TtUIElement.DeviceDownEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                d_arg.InputEventPtr = ePtr;
                            element.RaiseEvent(d_arg);
                            ReleaseEventSync(d_arg);

                            if(e.MouseButton.Button == (byte)EMouseButton.BUTTON_LEFT)
                            {
                                var larg = QueryEventSync();
                                larg.Source = element;
                                //larg.Host = mActiveHost;
                                larg.RoutedEvent = TtUIElement.MouseLeftButtonDownEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    larg.InputEventPtr = ePtr;
                                element.RaiseEvent(larg);
                                ReleaseEventSync(larg);
                            }
                            else if(e.MouseButton.Button == (byte)EMouseButton.BUTTON_RIGHT)
                            {
                                var rarg = QueryEventSync();
                                rarg.Source = element;
                                //rarg.Host = mActiveHost;
                                rarg.RoutedEvent = TtUIElement.MouseRightButtonDownEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    rarg.InputEventPtr = ePtr;
                                element.RaiseEvent(rarg);
                                ReleaseEventSync(rarg);
                            }
                            else if(e.MouseButton.Button == (byte)EMouseButton.BUTTON_MIDDLE)
                            {
                                var marg = QueryEventSync();
                                marg.Source = element;
                                //marg.Host = mActiveHost;
                                marg.RoutedEvent = TtUIElement.MouseMiddleButtonDownEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    marg.InputEventPtr = ePtr;
                                element.RaiseEvent(marg);
                                ReleaseEventSync(marg);
                            }
                        }
                    }
                    break;
                case EventType.MOUSEBUTTONUP:
                    {
                        var element = CheckHoveredElement(e);
                        if(element != null)
                        {
                            var arg = QueryEventSync();
                            arg.Source = element;
                            //arg.Host = mActiveHost;
                            arg.RoutedEvent = TtUIElement.MouseButtonUpEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                arg.InputEventPtr = ePtr;
                            element.RaiseEvent(arg);
                            ReleaseEventSync(arg);

                            var d_arg = QueryEventSync();
                            d_arg.Source = element;
                            //d_arg.Host = mActiveHost;
                            d_arg.RoutedEvent = TtUIElement.DeviceUpEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                d_arg.InputEventPtr = ePtr;
                            element.RaiseEvent(d_arg);
                            ReleaseEventSync(d_arg);

                            if (e.MouseButton.Button == (byte)EMouseButton.BUTTON_LEFT)
                            {
                                var larg = QueryEventSync();
                                larg.Source = element;
                                //larg.Host = mActiveHost;
                                larg.RoutedEvent = TtUIElement.MouseLeftButtonUpEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    larg.InputEventPtr = ePtr;
                                element.RaiseEvent(larg);
                                ReleaseEventSync(larg);
                            }
                            else if (e.MouseButton.Button == (byte)EMouseButton.BUTTON_RIGHT)
                            {
                                var rarg = QueryEventSync();
                                rarg.Source = element;
                                //rarg.Host = mActiveHost;
                                rarg.RoutedEvent = TtUIElement.MouseRightButtonUpEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    rarg.InputEventPtr = ePtr;
                                element.RaiseEvent(rarg);
                                ReleaseEventSync(rarg);
                            }
                            else if (e.MouseButton.Button == (byte)EMouseButton.BUTTON_MIDDLE)
                            {
                                var marg = QueryEventSync();
                                marg.Source = element;
                                //marg.Host = mActiveHost;
                                marg.RoutedEvent = TtUIElement.MouseMiddleButtonUpEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    marg.InputEventPtr = ePtr;
                                element.RaiseEvent(marg);
                                ReleaseEventSync(marg);
                            }
                        }
                    }
                    break;
                case EventType.MOUSEMOTION:
                    {
                        var element = CheckHoveredElement(e);
                        if(element != null)
                        {
                            var arg = QueryEventSync();
                            //arg.Host = mActiveHost;
                            arg.Source = element;
                            arg.RoutedEvent = TtUIElement.MouseMoveEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                arg.InputEventPtr = ePtr;
                            element.RaiseEvent(arg);
                            ReleaseEventSync(arg);

                            var d_arg = QueryEventSync();
                            //d_arg.Host = mActiveHost;
                            d_arg.Source = element;
                            d_arg.RoutedEvent = TtUIElement.DeviceMoveEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                d_arg.InputEventPtr = ePtr;
                            element.RaiseEvent(d_arg);
                            ReleaseEventSync(d_arg);
                        }
                    }
                    break;
                case EventType.CONTROLLERTOUCHPADDOWN:
                    {
                        var element = CheckHoveredElement(e);
                        if (element != null)
                        {
                            var arg = QueryEventSync();
                            //arg.Host = mActiveHost;
                            arg.Source = element;
                            arg.RoutedEvent = TtUIElement.TouchDownEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                arg.InputEventPtr = ePtr;
                            element.RaiseEvent(arg);
                            ReleaseEventSync(arg);

                            var d_arg = QueryEventSync();
                            //d_arg.Host = mActiveHost;
                            d_arg.Source = element;
                            d_arg.RoutedEvent = TtUIElement.DeviceDownEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                d_arg.InputEventPtr = ePtr;
                            element.RaiseEvent(d_arg);
                            ReleaseEventSync(d_arg);
                        }
                    }
                    break;
                case EventType.CONTROLLERTOUCHPADUP:
                    {
                        var element = CheckHoveredElement(e);
                        if (element != null)
                        {
                            var arg = QueryEventSync();
                            //arg.Host = mActiveHost;
                            arg.Source = element;
                            arg.RoutedEvent = TtUIElement.TouchUpEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                arg.InputEventPtr = ePtr;
                            element.RaiseEvent(arg);
                            ReleaseEventSync(arg);

                            var d_arg = QueryEventSync();
                            //d_arg.Host = mActiveHost;
                            d_arg.Source = element;
                            d_arg.RoutedEvent = TtUIElement.DeviceUpEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                d_arg.InputEventPtr = ePtr;
                            element.RaiseEvent(d_arg);
                            ReleaseEventSync(d_arg);
                        }
                    }
                    break;
                case EventType.CONTROLLERTOUCHPADMOTION:
                    {
                        var element = CheckHoveredElement(e);
                        if(element != null)
                        {
                            var arg = QueryEventSync();
                            //arg.Host = mActiveHost;
                            arg.Source = element;
                            arg.RoutedEvent = TtUIElement.TouchMoveEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                arg.InputEventPtr = ePtr;
                            element.RaiseEvent(arg);
                            ReleaseEventSync(arg);

                            var d_arg = QueryEventSync();
                            //d_arg.Host = mActiveHost;
                            d_arg.Source = element;
                            d_arg.RoutedEvent = TtUIElement.DeviceMoveEvent;
                            fixed (Bricks.Input.Event* ePtr = &e)
                                d_arg.InputEventPtr = ePtr;
                            element.RaiseEvent(d_arg);
                            ReleaseEventSync(d_arg);
                        }
                    }
                    break;
                case EventType.KEYDOWN:
                case EventType.KEYUP:
                    {
                        TtUIHost procHost = mActiveHost;
                        if(mDialogHosts.Count > 0)
                            procHost = mDialogHosts.Peek();
                        if(procHost != null)
                        {
                            // 处理快捷键
                            procHost.ProcessHotKey(in e);

                            // 处理焦点控件
                            if(e.Type == EventType.KEYDOWN)
                            {
                                var arg = QueryEventSync();
                                //arg.Host = procHost;
                                arg.RoutedEvent = TtUIElement.KeyDownEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    arg.InputEventPtr = ePtr;
                                if(mKeyboardFocusUIElement != null)
                                {
                                    arg.Source = mKeyboardFocusUIElement;
                                    mKeyboardFocusUIElement.RaiseEvent(arg);
                                }
                                else
                                {
                                    arg.Source = procHost;
                                    procHost.RaiseEvent(arg);
                                }
                                ReleaseEventSync(arg);
                            }
                            else if(e.Type == EventType.KEYUP)
                            {
                                var arg = QueryEventSync();
                                //arg.Host = procHost;
                                arg.RoutedEvent = TtUIElement.KeyUpEvent;
                                fixed (Bricks.Input.Event* ePtr = &e)
                                    arg.InputEventPtr = ePtr;
                                if(mKeyboardFocusUIElement != null)
                                {
                                    arg.Source = mKeyboardFocusUIElement;
                                    mKeyboardFocusUIElement.RaiseEvent(arg);
                                }
                                else
                                {
                                    arg.Source = procHost;
                                    procHost.RaiseEvent(arg);
                                }
                                ReleaseEventSync(arg);
                            }
                        }
                    }
                    break;
            }

            return true;
        }
    }
}
