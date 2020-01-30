using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeGenerateSystem.Base
{
    public partial class LinkPinControl : UserControl
    {
        public float PinImageHeight
        {
            get { return (float)GetValue(PinImageHeightProperty); }
            set { SetValue(PinImageHeightProperty, value); }
        } 
        public static readonly DependencyProperty PinImageHeightProperty = DependencyProperty.Register("PinImageHeight", typeof(float), typeof(LinkPinControl), new FrameworkPropertyMetadata(16.0f));
        public float PinImageWidth
        {
            get { return (float)GetValue(PinImageWidthProperty); }
            set { SetValue(PinImageWidthProperty, value); }
        }
        public static readonly DependencyProperty PinImageWidthProperty = DependencyProperty.Register("PinImageWidth", typeof(float), typeof(LinkPinControl), new FrameworkPropertyMetadata(16.0f));
        public string NameString
        {
            get { return (string)GetValue(NameStringProperty); }
            set { SetValue(NameStringProperty, value); }
        }
        public static readonly DependencyProperty NameStringProperty = DependencyProperty.Register("NameString", typeof(string), typeof(LinkPinControl), new FrameworkPropertyMetadata(""));
        public Visibility NameStringVisible
        {
            get { return (Visibility)GetValue(NameStringVisibleProperty); }
            set { SetValue(NameStringVisibleProperty, value); }
        }
        public static readonly DependencyProperty NameStringVisibleProperty = DependencyProperty.Register("NameStringVisible", typeof(Visibility), typeof(LinkPinControl), new FrameworkPropertyMetadata(Visibility.Visible));

        public enBezierType Direction
        {
            get { return (enBezierType)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(enBezierType), typeof(LinkPinControl), new FrameworkPropertyMetadata(enBezierType.None, new PropertyChangedCallback(OnDirectionChanged)));
        public static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as LinkPinControl;

            ctrl.UpdateShow();
        }

        /// <summary>
        /// 是否允许选择
        /// </summary>
        public bool EnableSelected
        {
            get { return (bool)GetValue(EnableSelectedProperty); }
            set { SetValue(EnableSelectedProperty, value); }
        }
        public static readonly DependencyProperty EnableSelectedProperty =
            DependencyProperty.Register("EnableSelected", typeof(bool), typeof(LinkPinControl), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(bool), typeof(LinkPinControl), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnSelectedChanged)));
        public static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as LinkPinControl;

            ctrl.SelectedOperation((bool)e.NewValue);
        }

        public Brush BackBrush
        {
            get { return (Brush)GetValue(BackBrushProperty); }
            set { SetValue(BackBrushProperty, value); }
        }
        public static readonly DependencyProperty BackBrushProperty =
            DependencyProperty.Register("BackBrush", typeof(Brush), typeof(LinkPinControl), new PropertyMetadata(Brushes.Gray));

        public Brush StrokeBrush
        {
            get { return (Brush)GetValue(StrokeBrushProperty); }
            set { SetValue(StrokeBrushProperty, value); }
        }
        public static readonly DependencyProperty StrokeBrushProperty =
            DependencyProperty.Register("StrokeBrush", typeof(Brush), typeof(LinkPinControl), new PropertyMetadata(Brushes.Black));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(LinkPinControl), new PropertyMetadata(1.0));

        //public bool HasLink
        //{
        //    get { return (bool)GetValue(HasLinkProperty); }
        //    set { SetValue(HasLinkProperty, value); }
        //}

        //public static readonly DependencyProperty HasLinkProperty =
        //    DependencyProperty.Register("HasLink", typeof(bool), typeof(LinkPinControl), new PropertyMetadata(false, new PropertyChangedCallback(OnPinPropertyChanged)));

        public enum enPinType
        {
            Normal,
            Exec,
            Array,
            Ptr,
            AnimationPose,
        }
        public enPinType PinType
        {
            get { return (enPinType)GetValue(PinTypeProperty); }
            set { SetValue(PinTypeProperty, value); }
        }
        public static readonly DependencyProperty PinTypeProperty = DependencyProperty.Register("PinType", typeof(enPinType), typeof(LinkPinControl), new FrameworkPropertyMetadata(enPinType.Normal, new PropertyChangedCallback(OnPinPropertyChanged)));
        public static void OnPinPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as LinkPinControl;
            ctrl.UpdateImageShow();
        }
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(LinkPinControl), new FrameworkPropertyMetadata(null));
        public Visibility MouseAssistVisible
        {
            get { return (Visibility)GetValue(MouseAssistVisibleProperty); }
            set { SetValue(MouseAssistVisibleProperty, value); }
        }
        public static readonly DependencyProperty MouseAssistVisibleProperty = DependencyProperty.Register("MouseAssistVisible", typeof(Visibility), typeof(LinkPinControl), new FrameworkPropertyMetadata(Visibility.Hidden));
        public Brush MouseAssistBrush
        {
            get { return (Brush)GetValue(MouseAssistBrushProperty); }
            set { SetValue(MouseAssistBrushProperty, value); }
        }
        public static readonly DependencyProperty MouseAssistBrushProperty = DependencyProperty.Register("MouseAssistBrush", typeof(Brush), typeof(LinkPinControl), new FrameworkPropertyMetadata(Brushes.White));
        public string ToolTip_TypeAndName
        {
            get { return (string)GetValue(ToolTip_TypeAndNameProperty); }
            set { SetValue(ToolTip_TypeAndNameProperty, value); }
        }
        public static readonly DependencyProperty ToolTip_TypeAndNameProperty = DependencyProperty.Register("ToolTip_TypeAndName", typeof(string), typeof(LinkPinControl), new FrameworkPropertyMetadata(""));
        public object ToolTip_Value
        {
            get { return (object)GetValue(ToolTip_ValueProperty); }
            set { SetValue(ToolTip_ValueProperty, value); }
        }
        public static readonly DependencyProperty ToolTip_ValueProperty = DependencyProperty.Register("ToolTip_Value", typeof(object), typeof(LinkPinControl), new FrameworkPropertyMetadata());
        public Visibility ToolTip_ValueVisible
        {
            get { return (Visibility)GetValue(ToolTip_ValueVisibleProperty); }
            set { SetValue(ToolTip_ValueVisibleProperty, value); }
        }
        public static readonly DependencyProperty ToolTip_ValueVisibleProperty = DependencyProperty.Register("ToolTip_ValueVisible", typeof(Visibility), typeof(LinkPinControl), new FrameworkPropertyMetadata(Visibility.Collapsed));

        protected virtual void SelectedOperation(bool selected) { }
        protected virtual void UpdateShow() { }

        public virtual double GetPinWidth() { return 15; }
        public virtual double GetPinHeight() { return 11; }

        protected void UpdateImageShow()
        {
            switch(PinType)
            {
                case enPinType.Exec:
                    if (HasLink)
                        Image = TryFindResource("ExecPin_Connected") as ImageSource;
                    else
                        Image = TryFindResource("ExecPin_Disconnected") as ImageSource;
                    break;
                case enPinType.Normal:
                    if (HasLink)
                        Image = TryFindResource("VarPin_Connected") as ImageSource;
                    else
                        Image = TryFindResource("VarPin_Disconnected") as ImageSource;
                    break;
                case enPinType.Array:
                    if (HasLink)
                        Image = TryFindResource("ArrayPin_Connected") as ImageSource;
                    else
                        Image = TryFindResource("ArrayPin_Disconnected") as ImageSource;
                    break;
                case enPinType.Ptr:
                    if (HasLink)
                        Image = TryFindResource("RefPin_Connected") as ImageSource;
                    else
                        Image = TryFindResource("RefPin_Disconnected") as ImageSource;
                    break;
                case enPinType.AnimationPose:
                    if (HasLink)
                        Image = TryFindResource("PosePin_Connected_15x28") as ImageSource;
                    else
                        Image = TryFindResource("PosePin_Disconnected_15x28") as ImageSource;
                    break;
            }
        }

        public LinkPinControl()
        {
            GUID = Guid.NewGuid();
            Image = TryFindResource("VarPin_Disconnected") as ImageSource;
            mContextMenu.Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as Style;

            this.MouseRightButtonDown += (sender, e) =>
            {
                e.Handled = true;
            };
            this.MouseRightButtonUp += (sender, e) =>
            {
                if (mContextMenu != null)
                {
                    mContextMenu.Items.Clear();
                    AddContextMenuItem("删除所有链接", "Pin Action", (actionObj, arg) =>
                    {
                        var tempInfos = new List<LinkInfo>(LinkInfos);
                        var redoAction = new Action<object>((obj) =>
                        {
                            Clear();
                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                                        (obj) =>
                                                        {
                                                            foreach (var li in tempInfos)
                                                            {
                                                                LinkInfos.Add(new LinkInfo(HostNodesContainer.GetDrawCanvas(), li.m_linkFromObjectInfo, li.m_linkToObjectInfo));
                                                            }
                                                        }, "Remove Link");
                        HostNodeControl.IsDirty = true;
                    });

                    foreach (var linkInfo in LinkInfos)
                    {
                        BaseNodeControl linkNode = null;
                        switch(LinkOpType)
                        {
                            case enLinkOpType.Start:
                                linkNode = linkInfo.m_linkToObjectInfo.HostNodeControl;
                                break;
                            case enLinkOpType.End:
                                linkNode = linkInfo.m_linkFromObjectInfo.HostNodeControl;
                                break;
                            case enLinkOpType.Both:
                                linkNode = null;
                                break;
                        }
                        if (linkNode == null)
                            return;
                        var str = linkNode.GetNodeDescriptionString();
                        AddContextMenuItem("删除连接到 " + str, "Pin Action", (actionObj, arg) =>
                        {
                            var undoRedoData = new DelLinkUndoRedoData()
                            {
                                ProcessInfo = linkInfo
                            };

                            var redoAction = new Action<object>((obj) =>
                            {
                                undoRedoData.ProcessInfo.Clear();
                                LinkInfos.Remove(undoRedoData.ProcessInfo);
                            });
                            redoAction.Invoke(null);
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                                            (obj) =>
                                                            {
                                                                undoRedoData.ProcessInfo = new LinkInfo(HostNodesContainer.GetDrawCanvas(), undoRedoData.ProcessInfo.m_linkFromObjectInfo, undoRedoData.ProcessInfo.m_linkToObjectInfo);
                                                                LinkInfos.Add(undoRedoData.ProcessInfo);
                                                            }, "Remove Link");
                            HostNodeControl.IsDirty = true;
                        });
                    }
                    OnCollectionContextMenus?.Invoke(this);
                    mContextMenu.IsOpen = true;
                }
                e.Handled = true;
            };
        }
        class DelLinkUndoRedoData
        {
            public LinkInfo ProcessInfo;
        }
        public string GetLinkPinKeyName()
        {
            return Name + "_" + EngineNS.Editor.Assist.GetValuedGUIDString(HostNodeControl.Id);
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (HostNodesContainer == null)
                return;
            if (HostNodesContainer?.PreviewLinkCurve?.Visibility == Visibility.Visible)
            {
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(HostNodesContainer.StartLinkObj, this))
                    MouseAssistBrush = Brushes.Green;
                else
                    MouseAssistBrush = Brushes.Red;
            }
            else
                MouseAssistBrush = BackBrush;
            MouseAssistVisible = Visibility.Visible;

            bool bFind = false;
            var value = EngineNS.Editor.Runner.RunnerManager.Instance.GetDataValue(GetLinkPinKeyName(), out bFind);
            if (value == null)
                ToolTip_Value = "null";
            else
            {
                ToolTip_Value = null;
                var valueType = value.GetType();
                if (valueType == typeof(string))
                    ToolTip_Value = value;
                else if(valueType.GetInterface(typeof(EngineNS.Editor.IMacrossDebugValueShow).FullName) != null)
                {
                    var val = value as EngineNS.Editor.IMacrossDebugValueShow;
                    ToolTip_Value = val.GetShowValue();
                }
                else if (valueType.IsClass || (valueType.IsValueType && !valueType.IsPrimitive))
                {
                    foreach(var prop in valueType.GetProperties())
                    {
                        var atts = prop.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), true);
                        if (atts == null || atts.Length == 0)
                            continue;
                        if (((EngineNS.Editor.MacrossMemberAttribute)atts[0]).HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.NotShowInBreak))
                            continue;

                        var val = prop.GetValue(value);
                        if (val == null)
                            ToolTip_Value += prop.Name + ": null\r\n";
                        else
                            ToolTip_Value += prop.Name + ": " + val.ToString() + "\r\n";
                    }
                    foreach (var prop in valueType.GetFields())
                    {
                        var atts = prop.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), true);
                        if (atts == null || atts.Length == 0)
                            continue;
                        if (((EngineNS.Editor.MacrossMemberAttribute)atts[0]).HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.NotShowInBreak))
                            continue;

                        var val = prop.GetValue(value);
                        if (val == null)
                            ToolTip_Value += prop.Name + ": null\r\n";
                        else
                            ToolTip_Value += prop.Name + ": " + val.ToString() + "\r\n";
                    }
                }
                else
                    ToolTip_Value = value;
            }
            if (bFind)
                ToolTip_ValueVisible = Visibility.Visible;
            else
                ToolTip_ValueVisible = Visibility.Collapsed;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            MouseAssistVisible = Visibility.Hidden;
        }

        #region ContextMenu

        protected ContextMenu mContextMenu = new ContextMenu();
        public delegate void Delegate_CollectionContextMenus(LinkPinControl linkControl);
        public Delegate_CollectionContextMenus OnCollectionContextMenus;
        public void AddContextMenuItem(string menuName, string category, Action<object, RoutedEventArgs> clickAction)
        {
            int index = -1;
            foreach(var item in mContextMenu.Items)
            {
                var textSep = item as ResourceLibrary.Controls.Menu.TextSeparator;
                if (textSep == null)
                    continue;
                if(textSep.Text == category)
                {
                    index = mContextMenu.Items.IndexOf(textSep) + 1;
                    break;
                }
            }
            if(index < 0)
            {
                mContextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
                {
                    Text = category,
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                });
                index = mContextMenu.Items.Count;
                mContextMenu.Items.Add(new Separator());
            }
            var menu = new MenuItem();
            menu.Name = "LinkControlContextMenu_";// + menuName;
            menu.Header = menuName;
            menu.Click += new RoutedEventHandler(clickAction);
            menu.Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style;
            mContextMenu.Items.Insert(index, menu);
        }

        #endregion 

        public Point LinkElementOffset   // 链接的控制点的偏移
        {
            get
            {
                Point offSet = new Point(0, 0);
                switch (BezierType)
                {
                    case enBezierType.Left:
                        {
                            offSet = new Point(-1, ActualHeight * 0.5);
                        }
                        break;

                    case enBezierType.Right:
                        {
                            offSet = new Point(ActualWidth + 1, ActualHeight * 0.5);
                        }
                        break;

                    case enBezierType.Top:
                        {
                            offSet = new Point(ActualWidth * 0.5, -1);
                        }
                        break;

                    case enBezierType.Bottom:
                        {
                            offSet = new Point(ActualWidth * 0.5, ActualHeight + 1);
                        }
                        break;
                        //default:
                        //    {
                        //        offSet = new Point(0, 0);
                        //    }
                        //    break;
                }
                return offSet;
            }
        }

        public void UpdateLink()
        {
            foreach (var linkInfo in LinkInfos)
                linkInfo.UpdateLink();

            //             foreach(var node in m_virtualNodes)
            //             {
            //                 node.UpdateLink();
            //             }            
        }

        partial void UpdateToolTip()
        {
            UpdateToolTip_WPF();
        }
        public void UpdateToolTip_WPF()
        {
            if (string.IsNullOrEmpty(ClassType))
                ToolTip_TypeAndName = $"{LinkType.ToString()}";
            else
            {
                switch (LinkType)
                {
                    case enLinkType.Enumerable:
                        {
                            var ct = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(ClassType);
                            if (ct != null)
                                ToolTip_TypeAndName = $"{LinkType.ToString()} ({EngineNS.Rtti.RttiHelper.GetAppTypeString(ct)})";
                            else
                                ToolTip_TypeAndName = $"{LinkType.ToString()} ({ClassType})";
                        }
                        break;
                    default:
                        ToolTip_TypeAndName = $"{LinkType.ToString()} ({ClassType})";
                        break;
                }
            }

            BackBrush = GetPinBrush();
        }
        Brush GetPinBrush()
        {
            if (HostNodeControl == null)
                return null;
            switch (LinkType)
            {
                case enLinkType.Bool:
                    return HostNodeControl.TryFindResource("Pin_bool") as Brush;
                case enLinkType.Int16:
                case enLinkType.Int32:
                case enLinkType.Int64:
                case enLinkType.UInt16:
                case enLinkType.UInt32:
                case enLinkType.UInt64:
                    return HostNodeControl.TryFindResource("Pin_Int") as Brush;
                case enLinkType.Byte:
                case enLinkType.SByte:
                    return HostNodeControl.TryFindResource("Pin_Byte") as Brush;
                case enLinkType.Single:
                case enLinkType.Double:
                case enLinkType.Float1:
                    return HostNodeControl.TryFindResource("Pin_Float") as Brush;
                case enLinkType.String:
                    return HostNodeControl.TryFindResource("Pin_String") as Brush;
                case enLinkType.NumbericalValue:
                    return HostNodeControl.TryFindResource("Pin_NumbericalValue") as Brush;
                case enLinkType.Vector2:
                case enLinkType.Vector3:
                case enLinkType.Vector4:
                case enLinkType.Float2:
                case enLinkType.Float3:
                case enLinkType.Float4:
                case enLinkType.UInt2:
                case enLinkType.UInt3:
                case enLinkType.UInt4:
                    return HostNodeControl.TryFindResource("Pin_Vector") as Brush;
                case enLinkType.Method:
                    return HostNodeControl.TryFindResource("Link_MethodBrush") as Brush;
                case enLinkType.Delegate:
                    return HostNodeControl.TryFindResource("Link_DelegateBrush") as Brush;
                case enLinkType.Float4x4:
                    return HostNodeControl.TryFindResource("Pin_Transform") as Brush;
                case enLinkType.Struct:
                    return HostNodeControl.TryFindResource("Pin_Struct") as Brush;
                case enLinkType.Class:
                    {
                        var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(ClassType);
                        if (type == typeof(Type))
                            return HostNodeControl.TryFindResource("Pin_Type") as Brush;
                        else
                            return HostNodeControl.TryFindResource("Pin_ClassObject") as Brush;
                    }
                case enLinkType.IntPtr:
                    return HostNodeControl.TryFindResource("Pin_Intptr") as Brush;
                case enLinkType.Enumerable:
                    {
                        var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(ClassType);
                        if (type == null)
                            return Brushes.Gray;
                        else
                            return GetTypeBrush(type);
                    }
                default:
                    {
                        var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(ClassType);
                        return GetTypeBrush(type);
                    }
            }
        }

        Brush GetTypeBrush(Type type)
        {
            if (type == null)
                return Brushes.Gray;
            else if (type == typeof(object))
                return Brushes.Gray;
            else if (type == typeof(Type))
                return HostNodeControl.TryFindResource("Pin_Type") as Brush;
            else if (type == typeof(bool))
                return HostNodeControl.TryFindResource("Pin_bool") as Brush;
            else if (type == typeof(Int16) ||
                type == typeof(Int32) ||
                type == typeof(Int64) ||
                type == typeof(UInt16) ||
                type == typeof(UInt32) ||
                type == typeof(UInt64))
                return HostNodeControl.TryFindResource("Pin_Int") as Brush;
            else if (type == typeof(Byte) ||
                    type == typeof(SByte))
                return HostNodeControl.TryFindResource("Pin_Byte") as Brush;
            else if (type == typeof(float) ||
                    type == typeof(double))
                return HostNodeControl.TryFindResource("Pin_Float") as Brush;
            else if (type == typeof(string))
                return HostNodeControl.TryFindResource("Pin_String") as Brush;
            else if (type == typeof(EngineNS.Vector2) ||
                    type == typeof(EngineNS.Vector3) ||
                    type == typeof(EngineNS.Vector4))
                return HostNodeControl.TryFindResource("Pin_Vector") as Brush;
            else if (type == typeof(EngineNS.Quaternion))
                return HostNodeControl.TryFindResource("Pin_Rotator") as Brush;
            else if (type == typeof(EngineNS.Matrix) ||
                    type == typeof(EngineNS.Matrix3x2))
                return HostNodeControl.TryFindResource("Pin_Transform") as Brush;
            else if (type.IsEnum)
                return HostNodeControl.TryFindResource("Pin_Enum") as Brush;
            else if (type == typeof(IntPtr))
                return HostNodeControl.TryFindResource("Pin_Intptr") as Brush;
            else if (type.IsArray)
                return GetTypeBrush(type.GetElementType());
            else if (type.IsGenericType && type.GetInterface(typeof(IEnumerable).FullName) != null)
            {
                var argTypes = type.GetGenericArguments();
                if (argTypes.Length == 1)
                    return GetTypeBrush(argTypes[0]);
            }
            else if (type.IsValueType)
                return HostNodeControl.TryFindResource("Pin_Struct") as Brush;
            else if (type.IsClass)
                return HostNodeControl.TryFindResource("Pin_ClassObject") as Brush;

            return Brushes.Gray;
        }

        partial void AddLinkInfo_WPF(LinkInfo info)
        {
            //info.m_linkFromObjectInfo.HasLink = true;
            //info.m_linkToObjectInfo.HasLink = true;
            UpdateImageShow();
        }
        partial void RemoveLink_WPF(LinkInfo linkInfo)
        {
            //if (linkInfo.m_linkFromObjectInfo.LinkInfos.Count == 0)
            //    linkInfo.m_linkFromObjectInfo.HasLink = false;
            //if (linkInfo.m_linkToObjectInfo.LinkInfos.Count == 0)
            //    linkInfo.m_linkToObjectInfo.HasLink = false;
            UpdateImageShow();
        }
    }
}
