using EngineNS;
using EngineNS.Bricks.Input;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace EngineNS.UI
{
    public partial class TtUIManager : IEventProcessor
    {
        public TtUIManager()
        {
            UEngine.Instance.EventProcessorManager.RegProcessor(this);
        }

        // 模态对话框
        internal Stack<TtUIHost> mDialogHosts = new Stack<TtUIHost>();
        internal List<TtUIHost> mPopupHosts = new List<TtUIHost>();
        TtObjectPool<TtRoutedEventArgs> mEventPool = new TtObjectPool<TtRoutedEventArgs>();

        TtUIHost mActiveHost = null;
        TtUIElement mKeyboardFocusUIElement = null;
        TtUIElement[] mCapturedElements = new TtUIElement[UInputSystem.MaxMultiTouchNumber + 1];
        TtUIElement[] mHoveredElements = new TtUIElement[UInputSystem.MaxMultiTouchNumber + 1];

        public void CaptureMouse(TtUIElement element)
        {
            mCapturedElements[UInputSystem.MaxMultiTouchNumber] = element;
        }
        public void CaptureTouch(int index, TtUIElement element)
        {
            if (index < 0 || index > UInputSystem.MaxMultiTouchNumber)
                return;
            mCapturedElements[index] = element;
        }

        TtUIElement CheckHoveredElement(in Event e)
        {
            Point2f pt;
            long index;
            switch(e.Type)
            {
                case EventType.MOUSEBUTTONDOWN:
                    {
                        pt = new Point2f(e.MouseButton.X, e.MouseButton.Y);
                        index = UInputSystem.MaxMultiTouchNumber;
                    }
                    break;
                case EventType.MOUSEMOTION:
                    {
                        pt = new Point2f(e.MouseMotion.X, e.MouseMotion.Y);
                        index = UInputSystem.MaxMultiTouchNumber;
                    }
                    break;
                case EventType.CONTROLLERTOUCHPADDOWN:
                case EventType.CONTROLLERTOUCHPADMOTION:
                    {
                        pt = new Point2f(e.TouchFinger.X, e.TouchFinger.Y);
                        index = e.TouchFinger.FingerId;
                    }
                    break;
                default:
                    return null;
            }

            if(mCapturedElements[index] != null)
                return mCapturedElements[index];

            TtUIHost procWin = GetFirstPointAtHost(in pt);
            if(mDialogHosts.Count > 0)
                procWin = mDialogHosts.Peek();
            if(mPopupHosts.Count > 0)
            {
                for(int i= mPopupHosts.Count - 1; i >= 0; i--)
                {
                    if (mPopupHosts[i].IsPointIn(in pt))
                    {
                        procWin = mPopupHosts[i];
                        break;
                    }
                }
            }

            if(procWin == null) 
                return null;

            System.Diagnostics.Debug.Assert(index < mHoveredElements.Length);
            var newStay = procWin.GetPointAtElement(in pt);
            var focusUIElement = mHoveredElements[index];
            if(newStay != focusUIElement)
            {
                if(e.Type == EventType.MOUSEBUTTONDOWN ||
                   e.Type == EventType.CONTROLLERTOUCHPADDOWN)
                {
                    mActiveHost = newStay.RootUIHost;
                }

                if (focusUIElement != null)
                {
                    var arg = mEventPool.QueryObjectSync();
                    arg.Source = focusUIElement;
                    arg.Host = procWin;
                    var parent = focusUIElement;
                    if(index == UInputSystem.MaxMultiTouchNumber)
                    {
                        while(!arg.Handled && parent != null)
                        {
                            parent.ProcessMouseLeave(arg, in e);
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        while(!arg.Handled && parent != null)
                        {
                            parent.ProcessTouchLeave(arg, in e);
                            parent = parent.Parent;
                        }
                    }
                    mEventPool.ReleaseObject(arg);
                }
                if(newStay != null)
                {
                    var arg = mEventPool.QueryObjectSync();
                    arg.Source = focusUIElement;
                    arg.Host = procWin;
                    var parent = newStay;
                    if(index == UInputSystem.MaxMultiTouchNumber)
                    {
                        while(!arg.Handled && parent != null)
                        {
                            parent.ProcessMouseEnter(arg, in e);
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        while(!arg.Handled && parent != null)
                        {
                            parent.ProcessTouchEnter(arg, in e);
                            parent = parent.Parent;
                        }
                    }
                    mEventPool.ReleaseObject(arg);
                }
                mHoveredElements[index] = newStay;
            }
            return newStay;
        }
        public bool OnEvent(in Event e)
        {
            // todo: drag drop
            switch (e.Type)
            {
                case EventType.MOUSEBUTTONDOWN:
                case EventType.MOUSEBUTTONUP:
                case EventType.MOUSEMOTION:
                    {
                        var element = CheckHoveredElement(e);
                        if(element != null)
                        {
                            var arg = mEventPool.QueryObjectSync();
                            arg.Host = mActiveHost;
                            arg.Source = element;
                            element.ProcessMessage(arg, in e);
                            mEventPool.ReleaseObject(arg);
                        }
                    }
                    break;
                case EventType.CONTROLLERTOUCHPADDOWN:
                case EventType.CONTROLLERTOUCHPADUP:
                case EventType.CONTROLLERTOUCHPADMOTION:
                    {
                        var element = CheckHoveredElement(e);
                        if(element != null)
                        {
                            var arg = mEventPool.QueryObjectSync();
                            arg.Host = mActiveHost;
                            arg.Source = element;
                            element.ProcessMessage(arg, in e);
                            mEventPool.ReleaseObject(arg);
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
                            var arg = mEventPool.QueryObjectSync();
                            arg.Host = procHost;
                            if(mKeyboardFocusUIElement != null)
                            {
                                arg.Source = mKeyboardFocusUIElement;
                                mKeyboardFocusUIElement.ProcessMessage(arg, in e);
                            }
                            else
                            {
                                arg.Source = procHost;
                                procHost.ProcessMessage(arg, in e);
                            }
                            mEventPool.ReleaseObject(arg);
                        }
                    }
                    break;
            }

            return true;
        }
    }
}
