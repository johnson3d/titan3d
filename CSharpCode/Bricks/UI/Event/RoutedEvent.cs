using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace EngineNS.UI.Event
{
    public enum ERoutedType
    {
        Tunnel,
        Bubble,
        Direct
    }

    public delegate void TtRoutedEventHandler(object sender, TtRoutedEventArgs args);
    //public delegate void TtMouseEventHandler(object sender, TtMouseEventArgs args);
    //public delegate void TtTouchEventHandler(object sender, TtTouchEventArgs args);
    //public delegate void TtDeviceEventHandler(object sender, TtDeviceEventArgs args);

    public class PGRoutedEventHandlerEditorAttribute : PGCustomValueEditorAttribute
    {
        EngineNS.EGui.UIProxy.ImageButtonProxy mImageButton;
        protected override async Task<bool> Initialize_Override()
        {
            mImageButton = new EGui.UIProxy.ImageButtonProxy()
            {
                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                Size = new Vector2(0, 0),
                UVMin = new Vector2(299.0f / 1024, 4.0f / 1024),
                UVMax = new Vector2(315.0f / 1024, 20.0f / 1024),
                ImageSize = new Vector2(16, 16),
                ShowBG = true,
                ImageColor = 0xFFFFFFFF,
            };
            await mImageButton.Initialize();
            return await base.Initialize_Override();
        }
        void ProcessAddMacrossEvent(TtUIElement element, in EditorInfo info)
        {
            if (element == null)
                return;

            var editorUIHost = element.RootUIHost as Editor.EditorUIHost;
            editorUIHost.HostEditor.AddEventMethod(element, info.HostProperty.Name, info.HostProperty.PropertyType);
        }
        public override bool OnDraw(in EditorInfo info, out object newValue)
        {
            bool valueChanged = false;
            newValue = null;
            var drawList = ImGuiAPI.GetWindowDrawList();
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = Math.Min(ImGuiAPI.GetColumnWidth(index) - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X, 100.0f);
            mImageButton.Size = new Vector2(width, 0);// ImGuiAPI.GetFrameHeight());
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Button, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonActive, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGActiveColor);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_ButtonHovered, EGui.UIProxy.StyleConfig.Instance.PGCreateButtonBGHoverColor);
            if (mImageButton.OnDraw(in drawList, in Support.UAnyPointer.Default))
            {
                // crate new event macross method
                var enumrableInterface = info.ObjectInstance.GetType().GetInterface(typeof(IEnumerable).FullName, false);
                if(enumrableInterface != null)
                {
                    foreach(var ins in (IEnumerable)info.ObjectInstance)
                    {
                        var element = ins as TtUIElement;
                        ProcessAddMacrossEvent(element, info);
                    }
                }
                else
                {
                    var element = info.ObjectInstance as TtUIElement;
                    ProcessAddMacrossEvent(element, info);
                }

                valueChanged = true;
            }
            ImGuiAPI.PopStyleColor(3);
            return valueChanged;
        }
    }

    internal struct TtRouteItem
    {
        internal object Target;
        internal TtRoutedEventHandlerInfo Info;

        internal TtRouteItem(object target, TtRoutedEventHandlerInfo info)
        {
            Target = target;
            Info = info;
        }
        internal void InvokeHandler(TtRoutedEventArgs args)
        {
            Info.InvokeHandler(Target, args);
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return Equals((TtRouteItem)obj);
        }
        public bool Equals(in TtRouteItem item)
        {
            return (Target == item.Target) &&
                   (Info == item.Info);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(TtRouteItem routeItem1, TtRouteItem routeItem2)
        {
            return routeItem1.Equals(routeItem2);
        }

        public static bool operator !=(TtRouteItem routeItem1, TtRouteItem routeItem2)
        {
            return !routeItem1.Equals(routeItem2);
        }
    }
    internal struct TtSourceItem
    {
        public int StartIndex;
        public object Source;

        internal TtSourceItem(int startIndex, object source)
        {
            StartIndex = startIndex;
            Source = source;
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals((TtSourceItem)obj);
        }
        public bool Equals(in TtSourceItem item)
        {
            return (item.StartIndex == StartIndex) &&
                   (item.Source == Source);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(TtSourceItem sourceItem1, TtSourceItem sourceItem2)
        {
            return sourceItem1.Equals(sourceItem2);
        }
        public static bool operator !=(TtSourceItem sourceItem1, TtSourceItem sourceItem2)
        {
            return !sourceItem1.Equals(sourceItem2);
        }
    }

    public sealed class TtEventRoute : IPooledObject
    {
        public bool IsAlloc { get; set; } = false;

        public TtRoutedEvent RoutedEvent;
        List<TtSourceItem> mSourceItems;
        List<TtRouteItem> mRouteItems;

        private struct BranchNode
        {
            public object Node;
            public object Source;
        }
        Stack<BranchNode> mBranchNodeStack;
        private Stack<BranchNode> BranchNodeStack
        {
            get
            {
                if (mBranchNodeStack == null)
                    mBranchNodeStack = new Stack<BranchNode>();
                return mBranchNodeStack;
            }
        }

        public TtEventRoute()
        {
            mSourceItems = new List<TtSourceItem>(16);
            mRouteItems = new List<TtRouteItem>(16);
        }
        public TtEventRoute(TtRoutedEvent routedEvent)
        {
            RoutedEvent = routedEvent;
            mSourceItems = new List<TtSourceItem>(16);
            mRouteItems = new List<TtRouteItem>(16);
        }

        internal void InvokeHandlers(object source, TtRoutedEventArgs args, bool reRaised = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (args == null)
                throw new ArgumentNullException("args");
            if (args.Source == null)
                throw new ArgumentNullException("args Source not set");
            if (args.RoutedEvent != RoutedEvent)
                throw new ArgumentException("mismatched with RoutedEvent");

            switch(args.RoutedEvent.RoutedType)
            {
                case ERoutedType.Bubble:
                case ERoutedType.Direct:
                    {
                        int endSourceChangeIndex = 0;
                        for(int i=0; i<mRouteItems.Count; i++)
                        {
                            if(i >= endSourceChangeIndex)
                            {
                                var newSource = GetBubbleSource(i, out endSourceChangeIndex);

                                if(!reRaised)
                                {
                                    if (newSource == null)
                                        args.Source = source;
                                    else
                                        args.Source = newSource;
                                }
                            }

                            mRouteItems[i].InvokeHandler(args);
                        }
                    }
                    break;
                default:
                    {
                        int startSourceChangeIndex = mRouteItems.Count;
                        int endTargetIndex = mRouteItems.Count - 1;
                        int startTargetIndex;

                        while(endTargetIndex >= 0)
                        {
                            var curTarget = mRouteItems[endTargetIndex].Target;
                            for(startTargetIndex = endTargetIndex; startTargetIndex >= 0; startTargetIndex--)
                            {
                                if (mRouteItems[startTargetIndex].Target != curTarget)
                                    break;
                            }

                            for(int i= startTargetIndex + 1; i <= endTargetIndex; i++)
                            {
                                if(i < startSourceChangeIndex)
                                {
                                    var newSource = GetTunnelSource(i, out startSourceChangeIndex);
                                    if (newSource == null)
                                        args.Source = source;
                                    else
                                        args.Source = newSource;
                                }

                                mRouteItems[i].InvokeHandler(args);
                            }
                        }
                    }
                    break;
            }
        }

        object GetBubbleSource(int index, out int endIndex)
        {
            if (mSourceItems.Count == 0)
            {
                endIndex = mRouteItems.Count;
                return null;
            }

            if (index < mSourceItems[0].StartIndex)
            {
                endIndex = mSourceItems[0].StartIndex;
                return null;
            }

            for (int i = 0; i < mSourceItems.Count - 1; i++)
            {
                if (index >= mSourceItems[i].StartIndex &&
                    index < mSourceItems[i + 1].StartIndex)
                {
                    endIndex = mSourceItems[i + 1].StartIndex;
                    return mSourceItems[i].Source;
                }
            }

            // If we get here, we're on the last one,
            // so return that.            
            endIndex = mRouteItems.Count;
            return mSourceItems[mSourceItems.Count - 1].Source;
        }
        object GetTunnelSource(int index, out int startIndex)
        {
            if (mSourceItems.Count == 0)
            {
                startIndex = 0;
                return null;
            }

            if(index < mSourceItems[0].StartIndex)
            {
                startIndex = 0;
                return null;
            }

            for(int i=0; i<mSourceItems.Count - 1; i++)
            {
                if(index >= mSourceItems[i].StartIndex &&
                   index < mSourceItems[i + 1].StartIndex)
                {
                    startIndex = mSourceItems[i].StartIndex;
                    return mSourceItems[i].Source;
                }
            }

            startIndex = mSourceItems[mSourceItems.Count - 1].StartIndex;
            return mSourceItems[mSourceItems.Count - 1].Source;
        }

        public void Add(object target, in TtRoutedEventHandlerInfo info)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            mRouteItems.Add(new TtRouteItem(target, info));
        }
        public void AddSource(object source)
        {
            var startIndex = mRouteItems.Count;
            mSourceItems.Add(new TtSourceItem(startIndex, source));
        }

        internal void Clear()
        {
            RoutedEvent = null;
            mSourceItems.Clear();
            mRouteItems.Clear();
            mBranchNodeStack?.Clear();
        }

        public void PushBranchNode(object node, object source)
        {
            BranchNodeStack.Push(new BranchNode()
            {
                Node = node,
                Source = source,
            });
        }
        public object PopBranchNode()
        {
            if (BranchNodeStack.Count == 0)
                return null;
            var bn = BranchNodeStack.Pop();
            return bn.Node;
        }
        public object PeekBranchNode()
        {
            if (BranchNodeStack.Count == 0)
                return null;
            var bn = BranchNodeStack.Peek();
            return bn.Node;
        }
        public object PeekBranchSource()
        {
            if(BranchNodeStack.Count == 0)
                return null;
            var bn = BranchNodeStack.Peek();
            return bn.Source;
        }
    }

    public sealed class TtRoutedEvent
    {
        string mName;
        public string Name => mName;
        ERoutedType mRoutedType;
        public ERoutedType RoutedType => mRoutedType;
        UTypeDesc mHandlerType;
        public UTypeDesc HandlerType => mHandlerType;
        UTypeDesc mOwnerType;
        public UTypeDesc OwnerType => mOwnerType;
        int mGlobalIndex;
        public int GlobalIndex => mGlobalIndex;
        static int mGlobalIndexCounter = -1;

        internal TtRoutedEvent(string name, ERoutedType routedType, UTypeDesc handlerType, UTypeDesc ownerType)
        {
            mName = name;
            mRoutedType = routedType;
            mHandlerType = handlerType;
            mOwnerType = ownerType;
            mGlobalIndex = Interlocked.Increment(ref mGlobalIndexCounter);
        }

        public bool IsLegalHandler(Delegate handler)
        {
            var type = handler.GetType();
            return (mHandlerType.Equals(type)) || (type == typeof(TtRoutedEventHandler));
        }
    }

    public struct TtRoutedEventHandlerInfo
    {
        Delegate mHandler;
        public Delegate Handler => mHandler;
        bool mHandledEventsToo;
        public bool HandledEventsToo => mHandledEventsToo;
        internal TtRoutedEventHandlerInfo(Delegate handler, bool handledEventsToo)
        {
            mHandler = handler;
            mHandledEventsToo = handledEventsToo;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj == null || (obj is TtRoutedEventHandlerInfo))
                return false;

            return Equals((TtRoutedEventHandlerInfo)obj);
        }
        public bool Equals(in TtRoutedEventHandlerInfo other)
        {
            return (mHandler == other.mHandler) && (mHandledEventsToo == other.mHandledEventsToo);
        }

        public static bool operator == (TtRoutedEventHandlerInfo info1, TtRoutedEventHandlerInfo info2)
        {
            return info1.Equals(info2);
        }
        public static bool operator !=(TtRoutedEventHandlerInfo info1, TtRoutedEventHandlerInfo info2)
        {
            return !info1.Equals(info2);
        }

        internal void InvokeHandler(object target, TtRoutedEventArgs args)
        {
            if( (args.Handled == false) || (mHandledEventsToo == true))
            {
                if(mHandler is TtRoutedEventHandler)
                {
                    ((TtRoutedEventHandler)mHandler)(target, args);
                }
                else
                {
                    mHandler.DynamicInvoke(new object[] {target, args});
                }
            }
        }
        public override string ToString()
        {
            if (Handler == null || Handler.Target == null || Handler.Method == null)
                return base.ToString();
            return Handler.Target.ToString() + "(" + Handler.Method.Name.ToString() + ")[" + HandledEventsToo + "]";
        }
    }

    public static class TtEventManager
    {
        static Dictionary<UTypeDesc, Dictionary<string, TtRoutedEvent>> mEvents = new Dictionary<UTypeDesc, Dictionary<string, TtRoutedEvent>>();

        public static TtRoutedEvent RegisterRoutedEvent(string name, ERoutedType type, Type handlerType, Type ownerType)
        {
            return RegisterRoutedEvent(name, type, UTypeDesc.TypeOf(handlerType), UTypeDesc.TypeOf(ownerType));
        }
        public static TtRoutedEvent RegisterRoutedEvent(string name, ERoutedType type, UTypeDesc handlerType, UTypeDesc ownerType)
        {
            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (handlerType == null)
                throw new ArgumentNullException("handlerType");
            if (ownerType == null)
                throw new ArgumentNullException("ownerType");
            if (GetRoutedEventFromName(name, ownerType, false) != null)
                throw new ArgumentException($"{ownerType.FullName} already have event with name {name}");

            lock(mEvents)
            {
                var evt = new TtRoutedEvent(name, type, handlerType, ownerType);
                Dictionary<string, TtRoutedEvent> events;
                if(!mEvents.TryGetValue(ownerType, out events))
                {
                    events = new Dictionary<string, TtRoutedEvent>();
                    mEvents.Add(ownerType, events);
                }
                events.Add(name, evt);
                return evt;
            }
        }

        internal static TtRoutedEvent GetRoutedEventFromName(string name, UTypeDesc ownerType, bool includeSupers)
        {
            Dictionary<string, TtRoutedEvent> events;
            UTypeDesc type = ownerType;
            while(type != null)
            {
                if(mEvents.TryGetValue(type, out events))
                {
                    TtRoutedEvent retVal;
                    if (events.TryGetValue(name, out retVal))
                        return retVal;
                }

                type = includeSupers ? type.BaseType : null;
            }

            return null;
        }

        public static void QueryEvents(UTypeDesc type, Action<UTypeDesc, string, TtRoutedEvent> queryAction, bool includeBaseType = true)
        {
            if (queryAction == null)
                return;
            if(includeBaseType)
            {
                foreach(var e in mEvents)
                {
                    if((type == e.Key) || (type.IsSubclassOf(e.Key)))
                    {
                        foreach(var d in e.Value)
                        {
                            queryAction.Invoke(e.Key, d.Key, d.Value);
                        }
                    }
                }
            }
            else
            {
                Dictionary<string, TtRoutedEvent> dic;
                if(mEvents.TryGetValue(type, out dic))
                {
                    foreach (var d in dic)
                        queryAction.Invoke(type, d.Key, d.Value);
                }
            }
        }
    }
}
