using EngineNS.UI.Bind;
using EngineNS.UI.Event;
using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipelines;
using System.Text;

namespace EngineNS.UI.Controls
{
    public unsafe class TtRoutedEventArgs : IPooledObject
    {
        public bool IsAlloc { get; set; } = false;
        object mSource = null;
        public object Source
        {
            get => mSource;
            set
            {
                mSource = value;
                if (OriginalSource == null)
                    OriginalSource = mSource;
            }
        }
        public object OriginalSource = null;
        //public TtUIHost Host = null;
        public TtRoutedEvent RoutedEvent = null;
        public Bricks.Input.Event* InputEventPtr = null;
        public bool Handled = false;

        public void Reset()
        {
            //IsAlloc = false;
            mSource = null;
            OriginalSource = null;
            //Host = null;
            RoutedEvent = null;
            InputEventPtr = null;
            Handled = false;
        }
    }

    //public class TtInputEventArgs : TtRoutedEventArgs
    //{

    //}
    //public class TtDeviceEventArgs : TtInputEventArgs
    //{

    //}
    //public class TtMouseEventArgs : TtDeviceEventArgs
    //{

    //}
    //public class TtTouchEventArgs : TtDeviceEventArgs
    //{

    //}

    public partial class TtUIElement
    {
        Dictionary<int, List<TtRoutedEventHandlerInfo>> mEventHandlers = new Dictionary<int, List<TtRoutedEventHandlerInfo>>();
        public void AddHandler(TtRoutedEvent routedEvent, Delegate handler, bool handleEventsToo = false)
        {
            if (routedEvent == null)
                throw new ArgumentNullException("routedEvent");
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (!routedEvent.IsLegalHandler(handler))
                throw new ArgumentException("handler is illegal");

            List<TtRoutedEventHandlerInfo> events;
            if(!mEventHandlers.TryGetValue(routedEvent.GlobalIndex, out events))
            {
                events = new List<TtRoutedEventHandlerInfo>();
                mEventHandlers.Add(routedEvent.GlobalIndex, events);
            }
            events.Add(new TtRoutedEventHandlerInfo(handler, handleEventsToo));
        }
        public void RemoveHandler(TtRoutedEvent routedEvent, Delegate handler)
        {
            if (routedEvent == null)
                throw new ArgumentNullException("routedEvent");
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (!routedEvent.IsLegalHandler(handler))
                throw new ArgumentException("handler is illegal");

            List<TtRoutedEventHandlerInfo> events;
            if(mEventHandlers.TryGetValue(routedEvent.GlobalIndex, out events))
            {
                for(int i=events.Count - 1; i >= 0; i--)
                {
                    if (events[i].Handler == handler)
                        events.RemoveAt(i);
                }
                if(events.Count == 0)
                {
                    mEventHandlers.Remove(routedEvent.GlobalIndex);
                }
            }
        }
        public void RaiseEvent(TtRoutedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            var route = UEngine.Instance.UIManager.QueryRouteSync(args.RoutedEvent);

            try
            {
                args.Source = this;
                BuildRoutedHelper(route, args);
                route.InvokeHandlers(this, args);
                args.Source = args.OriginalSource;
            }
            finally
            {

            }

            UEngine.Instance.UIManager.ReleaseRouteSync(route);
        }
        internal static readonly int MAX_ELEMENTS_IN_ROUTE = 4096;
        void BuildRoutedHelper(TtEventRoute route, TtRoutedEventArgs args)
        {
            if (route == null)
                throw new ArgumentNullException("route");
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Source == null)
                throw new ArgumentException("arg's source is null");
            if (args.RoutedEvent != route.RoutedEvent)
                throw new ArgumentException("mismatched with RoutedEvent");

            if(args.RoutedEvent.RoutedType == ERoutedType.Direct)
            {
                AddToEventRoute(route, args);
            }
            else
            {
                int cElements = 0;
                var element = this;
                while(element != null)
                {
                    TtUIElement uiElement = element as TtUIElement;

                    if (cElements++ > MAX_ELEMENTS_IN_ROUTE)
                        throw new InvalidOperationException("tree loop");

                    object newSource = null;
                    if (uiElement != null)
                        newSource = AdjustEventSource(args);

                    if (newSource != null)
                        route.AddSource(newSource);

                    bool continuePastVisualTree = false;
                    if(uiElement != null)
                    {
                        continuePastVisualTree = uiElement.BuildRoute(route, args, true);
                        uiElement.AddToEventRoute(route, args);
                        if (continuePastVisualTree)
                            element = uiElement.Parent;
                        else
                            element = VisualTreeHelper.GetParent(uiElement);
                    }

                    if (element == args.Source)
                        route.AddSource(element);
                }
            }
        }
        protected virtual bool BuildRoute(TtEventRoute route, TtRoutedEventArgs args, bool addIntermediateElements)
        {
            bool continuePastCoreTree = false;
            var visualParent = VisualTreeHelper.GetParent(this);
            var designParent = GetUIDesignParent();

            var branchNode = route.PeekBranchNode() as TtUIElement;
            if(branchNode != null && IsLogicalDescendent(branchNode))
            {
                args.Source = route.PeekBranchSource();
                AdjustBranchSource(args);
                route.AddSource(args.Source);
                route.PopBranchNode();
                if(addIntermediateElements)
                {
                    var modelTreeNode = Parent;
                    while(modelTreeNode != null)
                    {
                        modelTreeNode.AddToEventRoute(route, args);
                        modelTreeNode = modelTreeNode.Parent;
                    }
                }
            }
            if (!IgnoreModelParentBuildRoute(args))
            {
                if (visualParent == null)
                    continuePastCoreTree = (designParent != null);
                else if(designParent != null)
                {
                    if (visualParent.IsLayoutIslandRoot)
                        continuePastCoreTree = true;
                    route.PushBranchNode(this, args.Source);
                    args.Source = visualParent;
                }
            }
            return continuePastCoreTree;
        }
        public void AddToEventRoute(TtEventRoute route, TtRoutedEventArgs args)
        {
            if (route == null)
                throw new ArgumentNullException("route");
            if(args == null)
                throw new ArgumentNullException("args");

            List<TtRoutedEventHandlerInfo> infos;
            if(mEventHandlers.TryGetValue(args.RoutedEvent.GlobalIndex, out infos))
            {
                for(int i=0; i<infos.Count; i++)
                {
                    route.Add(this, infos[i]);
                }
            }

            AddToEventRouteOverride(route, args);
        }
        protected virtual void AddToEventRouteOverride(TtEventRoute route, TtRoutedEventArgs args)
        {
        }
        protected virtual void AdjustBranchSource(TtRoutedEventArgs args)
        {
        }
        protected virtual bool IgnoreModelParentBuildRoute(TtRoutedEventArgs args)
        {
            return false;
        }
        bool IsLogicalDescendent(TtUIElement element)
        {
            while(element != null)
            {
                if (element == this)
                    return true;

                element = element.Parent;
            }
            return false;
        }
        protected virtual object AdjustEventSource(TtRoutedEventArgs args)
        {
            object source = null;

            if (Parent != null || HasLogicalChildren)
            {
                var logicSource = args.Source as TtUIElement;
                if(logicSource == null || !IsLogicalDescendent(logicSource))
                {
                    args.Source = this;
                    source = this;
                }
            }

            return source;
        }

        [Rtti.Meta, BindProperty]
        public bool IsFocusable
        {
            get => ReadFlag(ECoreFlags.IsFocusable);
            set
            {
                OnValueChange(value, IsFocusable);
                WriteFlag(ECoreFlags.IsFocusable, value);
            }
        }

        [Browsable(false), BindProperty]
        public bool IsMouseOver
        {
            get => ReadFlag(ECoreFlags.IsMouseOver);
            set
            {
                OnValueChange(value, IsMouseOver);
                WriteFlag(ECoreFlags.IsMouseOver, value);
            }
        }
        public bool IsMouseDirectlyOver
        {
            get => UEngine.Instance.UIManager.IsMouseDirectlyHover(this);
        }

        [Browsable(false), BindProperty]
        public bool IsTouchOver
        {
            get => ReadFlag(ECoreFlags.IsTouchOver);
            set
            {
                OnValueChange(value, IsTouchOver);
                WriteFlag(ECoreFlags.IsTouchOver, value);
            }
        }
        public bool IsTouchDirectlyOver
        {
            get => UEngine.Instance.UIManager.IsTouchDirectlyHover(this);
        }

        [Browsable(false), BindProperty]
        public bool IsSpaceKeyDown
        {
            get => ReadFlag(ECoreFlags.IsSpaceKeyDown);
            set
            {
                OnValueChange(value, IsSpaceKeyDown);
                WriteFlag(ECoreFlags.IsSpaceKeyDown, value);
            }
        }

        [Browsable(false), BindProperty]
        public bool IsMouseCaptured
        {
            get => UEngine.Instance.UIManager.MouseCaptured == this;
        }
        [Browsable(false), BindProperty]
        public bool IsKeyboardFocused
        {
            get => UEngine.Instance.UIManager.KeyboardFocusUIElement == this;
        }

        public delegate void Delegate_Focus(TtRoutedEventArgs eventArgs, TtUIElement newFocused, TtUIElement oldFocused);
        public Delegate_Focus OnLostFocus;
        public Delegate_Focus OnFocus;
        public virtual void ProcessOnLostFocus(TtRoutedEventArgs eventArgs, TtUIElement newFocused, TtUIElement oldFocused)
        {
            OnLostFocus?.Invoke(eventArgs, newFocused, oldFocused);
        }
        public virtual void ProcessOnFocus(TtRoutedEventArgs eventArgs, TtUIElement newFocused, TtUIElement oldFocused)
        {
            OnFocus?.Invoke(eventArgs, newFocused, oldFocused);
        }

        public delegate void Delegate_MouseCapture(TtRoutedEventArgs eventArgs, TtUIElement newCapture, TtUIElement oldCapture);
        public Delegate_MouseCapture OnMouseCapture;
        public Delegate_MouseCapture OnLostMouseCapture;
        public virtual void ProcessOnMouseCapture(TtRoutedEventArgs eventArgs, TtUIElement newCapture, TtUIElement oldCapture)
        {
            OnMouseCapture?.Invoke(eventArgs, newCapture, oldCapture);
        }
        public virtual void ProcessOnLostMouseCapture(TtRoutedEventArgs eventArgs, TtUIElement newCapture, TtUIElement oldCapture)
        {
            OnLostMouseCapture?.Invoke(eventArgs, newCapture, oldCapture);
        }

        //public delegate void Delegate_MouseEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
        //[Editor_UIEvent("鼠标进入时调用的方法")]
        //public Delegate_MouseEvent OnMouseEnter;
        //[Editor_UIEvent("鼠标离开时调用的方法")]
        //public Delegate_MouseEvent OnMouseLeave;
        //[Editor_UIEvent("鼠标按键按下时调用的方法")]
        //public Delegate_MouseEvent OnMouseButtonDown;
        //[Editor_UIEvent("鼠标左键按下时调用的方法")]
        //public Delegate_MouseEvent OnMouseLeftButtonDown;
        //[Editor_UIEvent("鼠标右键按下时调用的方法")]
        //public Delegate_MouseEvent OnMouseRightButtonDown;
        //[Editor_UIEvent("鼠标中键按下时调用的方法")]
        //public Delegate_MouseEvent OnMouseMiddleButtonDown;
        //[Editor_UIEvent("鼠标按键弹起时调用的方法")]
        //public Delegate_MouseEvent OnMouseButtonUp;
        //[Editor_UIEvent("鼠标左键弹起时调用的方法")]
        //public Delegate_MouseEvent OnMouseLeftButtonUp;
        //[Editor_UIEvent("鼠标右键弹起时调用的方法")]
        //public Delegate_MouseEvent OnMouseRightButtonUp;
        //[Editor_UIEvent("鼠标中键弹起时调用的方法")]
        //public Delegate_MouseEvent OnMouseMiddleButtonUp;
        //[Editor_UIEvent("鼠标移动时调用的方法")]
        //public Delegate_MouseEvent OnMouseMove;
        //[Editor_UIEvent("鼠标滚轮滚动时调用的方法")]
        //public Delegate_MouseEvent OnMouseWheel;
        public static readonly TtRoutedEvent MouseEnterEvent = TtEventManager.RegisterRoutedEvent("MouseEnter", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseEnter
        {
            add { AddHandler(MouseEnterEvent, value); }
            remove { RemoveHandler(MouseEnterEvent, value); }
        }
        protected virtual void OnMouseEnter(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseDirectylyEnterEvent = TtEventManager.RegisterRoutedEvent("MouseDirectylyEnter", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseDirectyly
        {
            add { AddHandler(MouseDirectylyEnterEvent, value); }
            remove { RemoveHandler(MouseDirectylyEnterEvent, value); }
        }

        public static readonly TtRoutedEvent MouseLeaveEvent = TtEventManager.RegisterRoutedEvent("MouseLeave", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseLeave
        {
            add { AddHandler(MouseLeaveEvent, value); }
            remove { RemoveHandler(MouseLeaveEvent, value); }
        }
        protected virtual void OnMouseLeave(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseDirectylyLeaveEvent = TtEventManager.RegisterRoutedEvent("MouseDirectylyLeave", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseDirectylyLeave
        {
            add { AddHandler(MouseDirectylyLeaveEvent, value); }
            remove { RemoveHandler(MouseDirectylyLeaveEvent, value); }
        }

        public static readonly TtRoutedEvent MouseButtonDownEvent = TtEventManager.RegisterRoutedEvent("MouseButtonDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseButtonDown
        {
            add { AddHandler(MouseButtonDownEvent, value); }
            remove { RemoveHandler(MouseButtonDownEvent, value); }
        }
        protected virtual void OnMouseButtonDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseLeftButtonDownEvent = TtEventManager.RegisterRoutedEvent("MouseLeftButtonDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseLeftButtonDown
        {
            add { AddHandler(MouseLeftButtonDownEvent, value); }
            remove { RemoveHandler(MouseLeftButtonDownEvent, value); }
        }
        protected virtual void OnMouseLeftButtonDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseRightButtonDownEvent = TtEventManager.RegisterRoutedEvent("MouseRightButtonDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseRightButtonDown
        {
            add { AddHandler(MouseRightButtonDownEvent, value); }
            remove { RemoveHandler(MouseRightButtonDownEvent, value); }
        }
        protected virtual void OnMouseRightButtonDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseMiddleButtonDownEvent = TtEventManager.RegisterRoutedEvent("MouseMiddleButtonDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseMiddleButtonDown
        {
            add { AddHandler(MouseMiddleButtonDownEvent, value); }
            remove { RemoveHandler(MouseMiddleButtonDownEvent, value); }
        }
        protected virtual void OnMouseMiddleButtonDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseButtonUpEvent = TtEventManager.RegisterRoutedEvent("MouseButtonUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseButtonUp
        {
            add { AddHandler(MouseButtonUpEvent, value); }
            remove { RemoveHandler(MouseButtonUpEvent, value); }
        }
        protected virtual void OnMouseButtonUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseLeftButtonUpEvent = TtEventManager.RegisterRoutedEvent("MouseLeftButtonUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseLeftButtonUp
        {
            add { AddHandler(MouseLeftButtonUpEvent, value); }
            remove { RemoveHandler(MouseLeftButtonUpEvent, value); }
        }
        protected virtual void OnMouseLeftButtonUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseRightButtonUpEvent = TtEventManager.RegisterRoutedEvent("MouseRightButtonUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseRightButtonUp
        {
            add { AddHandler(MouseRightButtonUpEvent, value); }
            remove { RemoveHandler(MouseRightButtonUpEvent, value); }
        }
        protected virtual void OnMouseRightButtonUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseMiddleButtonUpEvent = TtEventManager.RegisterRoutedEvent("MouseMiddleButtonUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseMiddleButtonUp
        {
            add { AddHandler(MouseMiddleButtonUpEvent, value); }
            remove { RemoveHandler(MouseMiddleButtonUpEvent, value); }
        }
        protected virtual void OnMouseMiddleButtonUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseMoveEvent = TtEventManager.RegisterRoutedEvent("MouseMove", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseMove
        {
            add { AddHandler(MouseMoveEvent, value); }
            remove { RemoveHandler(MouseMoveEvent, value); }
        }
        protected virtual void OnMouseMove(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent MouseWheelEvent = TtEventManager.RegisterRoutedEvent("MouseWheel", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler MouseWheel
        {
            add { AddHandler(MouseWheelEvent, value); }
            remove { RemoveHandler(MouseWheelEvent, value); }
        }
        protected virtual void OnMouseWheel(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        //public delegate void Delegate_TouchEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
        //[Editor_UIEvent("触摸进入时调用的方法")]
        //public Delegate_TouchEvent OnTouchEnter;
        //[Editor_UIEvent("触摸离开时调用的方法")]
        //public Delegate_TouchEvent OnTouchLeave;
        //[Editor_UIEvent("触摸按下时调用的方法")]
        //public Delegate_TouchEvent OnTouchDown;
        //[Editor_UIEvent("触摸弹起时调用的方法")]
        //public Delegate_TouchEvent OnTouchUp;
        //[Editor_UIEvent("触摸移动时调用的方法")]
        //public Delegate_TouchEvent OnTouchMove;
        public static readonly TtRoutedEvent TouchEnterEvent = TtEventManager.RegisterRoutedEvent("TouchEnter", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler TouchEnter
        {
            add { AddHandler(TouchEnterEvent, value); }
            remove { RemoveHandler(TouchEnterEvent, value); }
        }
        protected virtual void OnTouchEnter(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent TouchLeaveEvent = TtEventManager.RegisterRoutedEvent("TouchLeave", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler TouchLeave
        {
            add { AddHandler(TouchLeaveEvent, value); }
            remove { RemoveHandler(TouchLeaveEvent, value); }
        }
        protected virtual void OnTouchLeave(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent TouchDownEvent = TtEventManager.RegisterRoutedEvent("TouchDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler TouchDown
        {
            add { AddHandler(TouchDownEvent, value); }
            remove { RemoveHandler(TouchDownEvent, value); }
        }
        protected virtual void OnTouchDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent TouchUpEvent = TtEventManager.RegisterRoutedEvent("TouchUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler TouchUp
        {
            add { AddHandler(TouchUpEvent, value); }
            remove { RemoveHandler(TouchUpEvent, value); }
        }
        protected virtual void OnTouchUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent TouchMoveEvent = TtEventManager.RegisterRoutedEvent("TouchMove", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler TouchMove
        {
            add { AddHandler(TouchMoveEvent, value); }
            remove { RemoveHandler(TouchMoveEvent, value); }
        }
        protected virtual void OnTouchMove(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        // 不区分设备
        //public delegate void Delegate_DeviceEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
        //[Editor_UIEvent("进入时调用的方法")]
        //public Delegate_DeviceEvent OnDeviceEnter;
        //[Editor_UIEvent("离开时调用的方法")]
        //public Delegate_DeviceEvent OnDeviceLeave;
        //[Editor_UIEvent("按下时调用的方法")]
        //public Delegate_DeviceEvent OnDeviceDown;
        //[Editor_UIEvent("弹起时调用的方法")]
        //public Delegate_DeviceEvent OnDeviceUp;
        //[Editor_UIEvent("移动时调用的方法")]
        //public Delegate_DeviceEvent OnDeviceMove;
        public static readonly TtRoutedEvent DeviceEnterEvent = TtEventManager.RegisterRoutedEvent("DeviceEnter", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler DeviceEnter
        {
            add { AddHandler(DeviceEnterEvent, value); }
            remove { RemoveHandler(DeviceEnterEvent, value); }
        }
        protected virtual void OnDeviceEnter(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent DeviceLeaveEvent = TtEventManager.RegisterRoutedEvent("DeviceLeave", ERoutedType.Direct, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler DeviceLeave
        {
            add { AddHandler(DeviceLeaveEvent, value); }
            remove { RemoveHandler(DeviceLeaveEvent, value); }
        }
        protected virtual void OnDeviceLeave(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent DeviceDownEvent = TtEventManager.RegisterRoutedEvent("DeviceDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler DeviceDown
        {
            add { AddHandler(DeviceDownEvent, value); }
            remove { RemoveHandler(DeviceDownEvent, value); }
        }
        protected virtual void OnDeviceDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent DeviceUpEvent = TtEventManager.RegisterRoutedEvent("DeviceUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler DeviceUp
        {
            add { AddHandler(DeviceUpEvent, value); }
            remove { RemoveHandler(DeviceUpEvent, value); }
        }
        protected virtual void OnDeviceUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }
        public static readonly TtRoutedEvent DeviceMoveEvent = TtEventManager.RegisterRoutedEvent("DeviceMove", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler DeviceMove
        {
            add { AddHandler(DeviceMoveEvent, value); }
            remove { RemoveHandler(DeviceMoveEvent, value); }
        }
        protected virtual void OnDeviceMove(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        //public virtual void ProcessOnMouseLeave(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnMouseLeave(eventArgs);
        //    OnDeviceLeave(eventArgs);
        //}
        //public virtual void ProcessOnMouseEnter(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnMouseEnter(eventArgs);
        //    OnDeviceEnter(eventArgs);
        //}
        //public virtual void ProcessOnTouchLeave(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnTouchLeave(eventArgs);
        //    OnDeviceLeave(eventArgs);
        //}
        //public virtual void ProcessOnTouchEnter(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnTouchEnter(eventArgs);
        //    OnDeviceEnter(eventArgs);
        //}

        //public virtual void ProcessOnMouseLeftButtonDown(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnMouseButtonDown(eventArgs);
        //    OnMouseLeftButtonDown(eventArgs);
        //    OnDeviceDown(eventArgs);
        //}
        //public virtual void ProcessOnMouseRightButtonDown(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnMouseButtonDown(eventArgs);
        //    OnMouseRightButtonDown(eventArgs);
        //    OnDeviceDown(eventArgs);
        //}
        //public virtual void ProcessOnMouseMiddleButtonDown(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnMouseButtonDown(eventArgs);
        //    OnMouseMiddleButtonDown(eventArgs);
        //    OnDeviceDown(eventArgs);
        //}
        //public virtual void ProcessOnMouseLeftButtonUp(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnMouseButtonUp != null)
        //        OnMouseButtonUp.Invoke(this, eventArgs, e);
        //    if (OnMouseLeftButtonUp != null)
        //        OnMouseLeftButtonUp.Invoke(this, eventArgs, e);
        //    if (OnDeviceUp != null)
        //        OnDeviceUp.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnMouseRightButtonUp(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnMouseButtonUp != null)
        //        OnMouseButtonUp.Invoke(this, eventArgs, e);
        //    if (OnMouseRightButtonUp != null)
        //        OnMouseRightButtonUp.Invoke(this, eventArgs, e);
        //    if (OnDeviceUp != null)
        //        OnDeviceUp.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnMouseMiddleButtonUp(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnMouseButtonUp != null)
        //        OnMouseButtonUp.Invoke(this, eventArgs, e);
        //    if (OnMouseMiddleButtonUp != null)
        //        OnMouseMiddleButtonUp.Invoke(this, eventArgs, e);
        //    if (OnDeviceUp != null)
        //        OnDeviceUp.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnMouseMotion(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnMouseMove != null)
        //        OnMouseMove.Invoke(this, eventArgs, e);
        //    if (OnDeviceMove != null)
        //        OnDeviceMove.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnMouseWheel(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnMouseWheel != null)
        //        OnMouseWheel.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnTouchDown(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnTouchDown != null)
        //        OnTouchDown.Invoke(this, eventArgs, e);
        //    if (OnDeviceDown != null)
        //        OnDeviceDown.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnTouchUp(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnTouchUp != null)
        //        OnTouchUp.Invoke(this, eventArgs, e);
        //    if (OnDeviceUp != null)
        //        OnDeviceUp.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnTouchMove(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    if (OnTouchMove != null)
        //        OnTouchMove.Invoke(this, eventArgs, e);
        //    if (OnDeviceMove != null)
        //        OnDeviceMove.Invoke(this, eventArgs, e);
        //}

        //public delegate void Delegate_OnKeyEvent(TtUIElement ui, TtRoutedEventArgs eventArgs, in Bricks.Input.Event e);
        //[Editor_UIEvent("按键按下时调用的方法")]
        //public Delegate_OnKeyEvent OnKeyDown;
        //[Editor_UIEvent("按键抬起时调用的方法")]
        //public Delegate_OnKeyEvent OnKeyUp;

        public static readonly TtRoutedEvent KeyDownEvent = TtEventManager.RegisterRoutedEvent("KeyDown", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler KeyDown
        {
            add { AddHandler(KeyDownEvent, value); }
            remove { RemoveHandler(KeyDownEvent, value); }
        }
        protected virtual void OnKeyDown(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        public static readonly TtRoutedEvent KeyUpEvent = TtEventManager.RegisterRoutedEvent("KeyUp", ERoutedType.Bubble, typeof(TtRoutedEventHandler), typeof(TtUIElement));
        public event TtRoutedEventHandler KeyUp
        {
            add { AddHandler(KeyUpEvent, value); }
            remove { RemoveHandler(KeyUpEvent, value); }
        }
        protected virtual void OnKeyUp(TtRoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        //public virtual void ProcessOnKeyDown(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnKeyDown?.Invoke(this, eventArgs, e);
        //}
        //public virtual void ProcessOnKeyUp(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    OnKeyUp?.Invoke(this, eventArgs, e);
        //}

        //public virtual void ProcessMessage(TtRoutedEventArgs eventArgs, in Bricks.Input.Event e)
        //{
        //    switch (e.Type)
        //    {
        //        case EngineNS.Bricks.Input.EventType.MOUSEBUTTONDOWN:
        //            {
        //                if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_LEFT)
        //                {
        //                    ProcessOnMouseLeftButtonDown(eventArgs, e);
        //                }
        //                else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_RIGHT)
        //                {
        //                    ProcessOnMouseRightButtonDown(eventArgs, e);
        //                }
        //                else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_MIDDLE)
        //                {
        //                    ProcessOnMouseMiddleButtonDown(eventArgs, e);
        //                }
        //            }
        //            break;
        //        case EngineNS.Bricks.Input.EventType.MOUSEBUTTONUP:
        //            {
        //                if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_LEFT)
        //                {
        //                    ProcessOnMouseLeftButtonUp(eventArgs, e);
        //                }
        //                else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_RIGHT)
        //                {
        //                    ProcessOnMouseRightButtonDown(eventArgs, e);
        //                }
        //                else if (e.MouseButton.Button == (byte)EngineNS.Bricks.Input.EMouseButton.BUTTON_MIDDLE)
        //                {
        //                    ProcessOnMouseMiddleButtonDown(eventArgs, e);
        //                }
        //            }
        //            break;
        //        case EngineNS.Bricks.Input.EventType.MOUSEMOTION:
        //            {
        //                ProcessOnMouseMotion(eventArgs, e);
        //            }
        //            break;
        //        case EngineNS.Bricks.Input.EventType.MOUSEWHEEL:
        //            {
        //                ProcessOnMouseWheel(eventArgs, e);
        //            }
        //            break;
        //        case EngineNS.Bricks.Input.EventType.CONTROLLERTOUCHPADDOWN:
        //            {
        //                ProcessOnTouchDown(eventArgs, e);
        //            }
        //            break;
        //        case EngineNS.Bricks.Input.EventType.CONTROLLERTOUCHPADUP:
        //            {
        //                ProcessOnTouchUp(eventArgs, e);
        //            }
        //            break;
        //        case EngineNS.Bricks.Input.EventType.CONTROLLERTOUCHPADMOTION:
        //            {
        //                ProcessOnTouchMove(eventArgs, e);
        //            }
        //            break;
        //        case Bricks.Input.EventType.KEYDOWN:
        //            ProcessOnKeyDown(eventArgs, e);
        //            break;
        //        case Bricks.Input.EventType.KEYUP:
        //            ProcessOnKeyUp(eventArgs, e);
        //            break;
        //    }

        //    if (!eventArgs.Handled)
        //        Parent?.ProcessMessage(eventArgs, e);
        //}
    }

}
