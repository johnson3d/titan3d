using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.CodeDom;
using System.ComponentModel;

namespace CodeGenerateSystem.Base
{
    public interface IDebugableNode
    {
        bool Breaked { get; set; }
        void ChangeParentLogicLinkLine(bool change);
        bool CanBreak();
    }
    public partial class ConstructionParams
    {
        Canvas mDrawCanvas = null;
        public Canvas DrawCanvas
        {
            get => mDrawCanvas;
            set
            {
                mDrawCanvas = value;
            }
        }
    }

    public partial class BaseNodeControl : HeaderedContentControl, EditorCommon.ITickObject
    {
        public virtual object GetLeftNodeInst()
        {
            return null;
        }

        public virtual object GetRightNodeInst()
        {
            return null;
        }
        #region Content
        public object LeftContent
        {
            get { return GetValue(LeftContentProperty); }
            set { SetValue(LeftContentProperty, value); }
        }
        public static readonly DependencyProperty LeftContentProperty = DependencyProperty.Register("LeftContent", typeof(object), typeof(BaseNodeControl), new UIPropertyMetadata(null));
        public object RightContent
        {
            get { return GetValue(RightContentProperty); }
            set { SetValue(RightContentProperty, value); }
        }
        public static readonly DependencyProperty RightContentProperty = DependencyProperty.Register("RightContent", typeof(object), typeof(BaseNodeControl), new UIPropertyMetadata(null));
        public object CenterDownContent
        {
            get { return GetValue(CenterDownContentProperty); }
            set { SetValue(CenterDownContentProperty, value); }
        }
        public static readonly DependencyProperty CenterDownContentProperty = DependencyProperty.Register("CenterDownContent", typeof(object), typeof(BaseNodeControl), new UIPropertyMetadata(null));
        #endregion
        #region InOutContent
        public object TopContent
        {
            get { return GetValue(TopContentProperty); }
            set { SetValue(TopContentProperty, value); }
        }
        public static readonly DependencyProperty TopContentProperty = DependencyProperty.Register("TopContent", typeof(object), typeof(BaseNodeControl), new UIPropertyMetadata(null));
        public object BottomContent
        {
            get { return GetValue(BottomContentProperty); }
            set { SetValue(BottomContentProperty, value); }
        }
        public static readonly DependencyProperty BottomContentProperty = DependencyProperty.Register("BottomContent", typeof(object), typeof(BaseNodeControl), new UIPropertyMetadata(null));
        #endregion
        //original purpose:for post process to use;
        public virtual object AddToRenderList(EngineNS.GamePlay.GWorld world)
        {
            return null;
        }

        public virtual object SetPostProcessCallLayer(bool pre_light_on)
        {
            return null;
        }

        public delegate void Delegate_SelectedNode(BaseNodeControl node, bool bSelected, bool unselectedOther);
        public virtual event Delegate_SelectedNode OnSelectNode;
        public delegate void Delegate_DoubleClick(BaseNodeControl node, MouseButtonEventArgs e);
        public virtual event Delegate_DoubleClick OnMouseLeftButtonDoubleClick;

        public delegate void Delegate_MoveNode(BaseNodeControl node);
        public virtual event Delegate_MoveNode OnMoveNode;

        public delegate void Delegate_StartDragMoveNode(BaseNodeControl node, MouseButtonEventArgs e);
        public virtual event Delegate_StartDragMoveNode OnStartDragMoveNode;
        public delegate void Delegate_DragMoveNode(BaseNodeControl node, MouseEventArgs e);
        public virtual event Delegate_DragMoveNode OnDragMoveNode;

        protected bool m_bDragMove = false;
        protected bool m_bAlreadyDragMoved = false;

        public Brush BlendBrush
        {
            get { return (Brush)GetValue(BlendBrushProperty); }
            set { SetValue(BlendBrushProperty, value); }
        }
        public static readonly DependencyProperty BlendBrushProperty = DependencyProperty.Register("BlendBrush", typeof(Brush), typeof(BaseNodeControl), new UIPropertyMetadata(Brushes.White));
        
        // 所有标记为GenerateCodeBase=true的节点在代码生成时直接调用其GCode_GenerateCode函数
        public bool m_bGenerateCodeBase = false;
        
        partial void SetIsSelfDeleteable(bool able)
        {
            if (mDragObj != null)
            {
                if (able)
                    mDragObj.ContextMenu = mContextMenu;
                else
                    mDragObj.ContextMenu = null;
            }
        }
        private FrameworkElement mDragObj;

        Canvas mParentDrawCanvas;
        public Canvas ParentDrawCanvas    // 绘制此控件的Canvas
        {
            get { return mParentDrawCanvas; }
            set
            {
                if (mParentDrawCanvas == value)
                    return;
                mParentDrawCanvas = value;
            }
        }

        //
        public Point GetPositionInContainer()
        {
            if (PackagedNode != null)
            {
                return PackagedNode.GetPositionInContainer();
            }
            else
            {
                if (ParentDrawCanvas != null)
                {
                    return TranslatePoint(new System.Windows.Point(0, 0), ParentDrawCanvas);
                }
                return new Point();
            }
        }

        protected Point mDeltaPt;

        public string NodeNameBinder
        {
            get { return (string)GetValue(NodeNameBinderProperty); }
            set { SetValue(NodeNameBinderProperty, value); }
        }
        public static readonly DependencyProperty NodeNameBinderProperty = DependencyProperty.Register("NodeNameBinder", typeof(string), typeof(BaseNodeControl), new UIPropertyMetadata(Program.NodeDefaultName, OnNodeNameBinderPropertyChanged));
        private static void OnNodeNameBinderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BaseNodeControl;
            var newVal = (string)e.NewValue;
            if (newVal != ctrl.NodeName)
                ctrl.NodeName = newVal;
        }
        partial void SetNodeNamePartial()
        {
            if(NodeNameBinder != NodeName)
                NodeNameBinder = NodeName;
        }

        public bool Selected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("Selected", typeof(bool), typeof(BaseNodeControl), new UIPropertyMetadata(false, OnSelectedPropertyChanged));
        private static void OnSelectedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as BaseNodeControl;
            var newValue = (bool)e.NewValue;
            foreach (var childNode in ctrl.mChildNodes)
            {
                childNode.Selected = newValue;
            }
                //                 foreach (var node in mMoveableChildNodes.Values)
                //                     node.Selected = m_bSelected;
        }

        public enum enNodeType
        {
            Normal,
            ChildNode,
            VarNode,
            CommentNode,
            AnimStateNode,
            AnimStateTransitionNode,
            LAGraphNode,
            LAClipNode,
            LAStateNode,
            LATransitionNode,
            LAPoseControlNode,
            BehaviorTreeControlNode,
            BehaviorTreeInnerControlNode,
            ColorGradienNode,
            StructNode,
        }
        public enNodeType NodeType
        {
            get { return (enNodeType)GetValue(NodeTypeProperty); }
            set { SetValue(NodeTypeProperty, value); }
        }
        public static readonly DependencyProperty NodeTypeProperty = DependencyProperty.Register("NodeType", typeof(enNodeType), typeof(BaseNodeControl), new UIPropertyMetadata(enNodeType.Normal));
        public bool BreakedPinShow
        {
            get { return (bool)GetValue(BreakedPinShowProperty); }
            set { SetValue(BreakedPinShowProperty, value); }
        }
        public static readonly DependencyProperty BreakedPinShowProperty = DependencyProperty.Register("BreakedPinShow", typeof(bool), typeof(BaseNodeControl), new UIPropertyMetadata(false));

        public bool BreakPointShow
        {
            get { return (bool)GetValue(BreakPointShowProperty); }
            set { SetValue(BreakPointShowProperty, value); }
        }
        public static readonly DependencyProperty BreakPointShowProperty = DependencyProperty.Register("BreakPointShow", typeof(bool), typeof(BaseNodeControl), new UIPropertyMetadata(false));

        //protected BaseNodeControl ParentNode
        //{
        //    get { return mParentNode; }
        //    set
        //    {
        //        mParentNode = value;

        //        // 父级连线
        //        //m_parentClassLinkLine.Stroke = System.Windows.Media.Brushes.LightGray;
        //        //m_parentClassLinkLine.Visibility = Visibility.Hidden;
        //        //ParentDrawCanvas.Children.Add(m_parentClassLinkLine);

        //        m_ParentLinkPath.Visibility = Visibility.Hidden;
        //        m_ParentLinkPath.Stroke = Brushes.LightGray;
        //        //m_ParentLinkPath.StrokeThickness = 3;
        //        m_ParentLinkPathFig.Segments.Add(m_ParentLinkBezierSeg);
        //        PathFigureCollection pfc = new PathFigureCollection();
        //        pfc.Add(m_ParentLinkPathFig);
        //        PathGeometry pg = new PathGeometry();
        //        pg.Figures = pfc;
        //        m_ParentLinkPath.Data = pg;
        //        ParentDrawCanvas.Children.Add(m_ParentLinkPath);
        //    }
        //}

        public string m_nodeName;                          // 节点名称，只有在此节点为可移动的子节点时才有意义
        public bool m_bNeedUpdateLocation = true;

        // 与父类的连线
        protected int mParentLinkIdx = 0;
        //Line m_parentClassLinkLine = new Line();
        protected Path mParentLinkPath = new Path();
        protected BezierSegment mParentLinkBezierSeg = new BezierSegment();
        //protected PathFigure mParentLinkPathFig = new PathFigure();

        //protected bool m_IsOneOutLink = true;
        //protected List<LinkInfo> m_outLinks = new List<LinkInfo>();
        //protected bool m_IsOneInLink = true;
        //protected List<LinkInfo> m_inLinks = new List<LinkInfo>();

        protected ContextMenu mContextMenu = new System.Windows.Controls.ContextMenu();
        public delegate void Delegate_CollectionContextMenu(BaseNodeControl node);
        public Delegate_CollectionContextMenu OnCollectionContextMenu;

        protected Brush m_selectedColor = Brushes.Orange;
        protected Brush mNormalColor = Brushes.DarkGray;
        protected Brush mErrorColor = Brushes.Red;

        protected ToolTip m_normalToolTip;
        protected ToolTip m_errorToolTip;

        static BaseNodeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseNodeControl), new FrameworkPropertyMetadata(typeof(BaseNodeControl)));

        }
        public BaseNodeControl()
        {
            mContextMenu.Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as Style;
        }

        protected FrameworkElement mDragHandle;
        protected CodeGenerateSystem.Controls.BreakPoint mBreakPoint;
        protected CodeGenerateSystem.Controls.CommentControl mCommentControl;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            switch(NodeType)
            {
                case enNodeType.CommentNode:
                    mDragHandle = Template.FindName("PART_HeaderGrid", this) as FrameworkElement;
                    SetDragObject(mDragHandle);
                    break;
                default:
                    mDragHandle = this;
                    SetDragObject(this);
                    break;
            }

            mBreakPoint = Template.FindName("PART_BREAKPT", this) as CodeGenerateSystem.Controls.BreakPoint;
            mBreakPoint?.Initialize(this);

            mCommentControl = Template.FindName("commentControl", this) as CodeGenerateSystem.Controls.CommentControl;
            OnApplyTemplate_Comment();


        }

        partial void InitConstruction()
        {
            if(mCSParam != null)
                ParentDrawCanvas = mCSParam.DrawCanvas;
            SetDragObject(mDragHandle);
            this.LayoutUpdated += BaseNodeControl_LayoutUpdated;
            IsVisibleChanged -= OnVisibleChanged;
            IsVisibleChanged += OnVisibleChanged;

            this.MouseRightButtonDown += BaseNodeControl_MouseRightButtonDown;
            this.MouseRightButtonUp += BaseNodeControl_MouseRightButtonUp;

            this.MouseLeftButtonUp += BaseNodeControl_MouseLeftButtonUp;
        }

        public virtual void ScaleTips(int scale)
        {
            if (mCommentControl == null)
                return;

            mCommentControl.ScaleTips(scale);
        }

        public virtual void ModifyCreatePosition(ref double x, ref double y)
        {

        }
        private void BaseNodeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndLink(null);
            //e.Handled = true;
        }

        private void BaseNodeControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NodeType == enNodeType.CommentNode)
            {
                var pos = e.GetPosition(mDragObj);
                if (pos.X < 0 || pos.Y < 0 || pos.X > mDragObj.ActualWidth || pos.Y > mDragObj.ActualHeight)
                    return;
            }
            e.Handled = true;
        }
        public virtual void OnOpenContextMenu(ContextMenu contextMenu)
        {

        }
        private void BaseNodeControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(NodeType == enNodeType.CommentNode)
            {
                var pos = e.GetPosition(mDragObj);
                if (pos.X < 0 || pos.Y < 0 || pos.X > mDragObj.ActualWidth || pos.Y > mDragObj.ActualHeight)
                    return;
            }
            if (mContextMenu != null)
            {
                if (mContextMenu.Style == null)
                    mContextMenu.Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as Style;
                var menuItemStyle = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style;
                mContextMenu.Items.Clear();
                OnOpenContextMenu(mContextMenu);
                // 节点操作
                mContextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
                {
                    Text = "节点操作",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                });
                if (IsDeleteable)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Name = "DeleteCurrentNode";
                    menuItem.Header = "删除当前节点";
                    menuItem.Click += new RoutedEventHandler(MenuItem_Click_Del);
                    menuItem.Style = menuItemStyle;
                    mContextMenu.Items.Add(menuItem);
                }
                var delSelItem = new MenuItem()
                {
                    Name = "DeleteAllSelectedNodes",
                    Header = "删除所有选中节点",
                    Style = menuItemStyle,
                };
                delSelItem.Click += (itemSender, itemE) =>
                {
                    this.HostNodesContainer.DeleteSelectedNodes();
                };
                mContextMenu.Items.Add(delSelItem);
                if (CanDuplicate())
                {
                    var copyItem = new MenuItem()
                    {
                        Name = "CopySelectedNodes",
                        Header = "复制",
                        Style = menuItemStyle,
                    };
                    copyItem.Click += (itemSender, itemE) =>
                    {
                        this.HostNodesContainer.Copy();
                    };
                    mContextMenu.Items.Add(copyItem);
                }
                mContextMenu.Items.Add(new Separator()
                {
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuSeparatorStyle")) as Style,
                });

                // 断点
                if (this.GetType().GetInterface(typeof(CodeGenerateSystem.Base.IDebugableNode).FullName) != null)
                {
                    var debugNode = this as CodeGenerateSystem.Base.IDebugableNode;
                    if (debugNode.CanBreak())
                    {
                        mContextMenu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
                        {
                            Text = "断点",
                            Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                        });
                        if (BreakPointShow)
                        {
                            var bpItem = new MenuItem()
                            {
                                Name = "DeleteBreakPoint",
                                Header = "删除断点",
                                Style = menuItemStyle,
                            };
                            mContextMenu.Items.Add(bpItem);
                            bpItem.Click += (itemSender, itemE) =>
                            {
                                BreakPointShow = false;
                                mBreakPoint.IsBreak = false;
                            };
                        }
                        else
                        {
                            var bpItem = new MenuItem()
                            {
                                Name = "AddBreakPoint",
                                Header = "添加断点",
                                Style = menuItemStyle,
                            };
                            mContextMenu.Items.Add(bpItem);
                            bpItem.Click += (itemSender, itemE) =>
                            {
                                BreakPointShow = true;
                                mBreakPoint.IsBreak = true;
                            };
                        }
                    }
                }

                mContextMenu.IsOpen = true;
            }
            e.Handled = true;
        }

        partial void SetToolTip_WPF(object toolTip)
        {
            this.ToolTip = toolTip;
        }
        partial void UpdateNodeAndLink_WPF()
        {
            // 连接节点列表
            List<CodeGenerateSystem.Base.LinkPinControl> linkObjList = new List<CodeGenerateSystem.Base.LinkPinControl>();

            // 当前结点的连接点
            foreach (var linkObj in mLinkPinInfoDic_Name.Values)
            {
                if (!linkObj.HasLink)
                    continue;
                linkObjList.Add(linkObj);
            }
               
            // 子节点的连接点
            foreach (var childNode in mChildNodes)
            {
                //var childLinkPins = childNode.GetLinkPinInfos();
                foreach (var linkObj in childNode.mLinkPinInfoDic_Name.Values)
                {
                    if (!linkObj.HasLink)
                        continue;
                    linkObjList.Add(linkObj);
                }
            }


            foreach (var linkObj in linkObjList)
            {
                // 一个节点最多只有一个显示的链接（这里逻辑到时候需要调整，如果一个节点引出多个链接）
                for(int i=0; i<linkObj.GetLinkInfosCount(); i++)
                {
                    var linkInfo = linkObj.GetLinkInfo(i);
                    var FromHostNode = linkInfo.m_linkFromObjectInfo.HostNodeControl;
                    var ToHostNode = linkInfo.m_linkToObjectInfo.HostNodeControl;

                    if (FromHostNode.IsVisible && ToHostNode.IsVisible)
                    {
                        linkInfo.Visible = Visibility.Visible;
                    }
                    else
                    {
                        linkInfo.Visible = Visibility.Hidden;
                    }
                }
            }

            BaseNodeControl fNode = this;
            while (fNode.PackagedNode != null)
                fNode = fNode.PackagedNode;
            mParentLinkPath.Visibility = fNode.Visibility;
            UpdateLink();
        }

        protected void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateNodeAndLink();
        }

        protected void AddContextMenu(MenuItem menuItem)
        {
            mContextMenu.Items.Add(menuItem);
        }
        protected void RemoveContextMenu(MenuItem menuItem)
        {
            mContextMenu.Items.Remove(menuItem);
        }

        protected bool mNeedLayoutUpdateLink = false;
        private void BaseNodeControl_LayoutUpdated(object sender, EventArgs e)
        {
            if(mNeedLayoutUpdateLink)
            {
                UpdateLink();
                mNeedLayoutUpdateLink = false;
            }
        }

        public void PlayShakeScaleAnimation()
        {
            this.RenderTransform = new ScaleTransform();
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation TransAnim = new DoubleAnimation();
            TransAnim.From = 1.0;
            TransAnim.To = 1.2;
            TransAnim.AutoReverse = true;
            TransAnim.Duration = TimeSpan.FromSeconds(0.15);
            TransAnim.Completed += new EventHandler(TransAnim_ScaleX_Completed);
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, TransAnim);

            TransAnim = new DoubleAnimation();
            TransAnim.From = 1.0;
            TransAnim.To = 1.2;
            TransAnim.AutoReverse = true;
            TransAnim.Duration = TimeSpan.FromSeconds(0.1);
            TransAnim.Completed += new EventHandler(TransAnim_ScaleY_Completed);
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, TransAnim);
        }

        void TransAnim_ScaleX_Completed(object sender, EventArgs e)
        {
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }

        void TransAnim_ScaleY_Completed(object sender, EventArgs e)
        {
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        //public BaseNodeControl(Canvas parentCanvas)
        //{
        //    ParentDrawCanvas = parentCanvas;

        //    GUID = System.Id.NewGuid();

        //    // 渐显动画
        //    DoubleAnimation OpacityAnim = new DoubleAnimation();
        //    OpacityAnim.From = 0.0;
        //    OpacityAnim.To = 1.0;
        //    OpacityAnim.Duration = TimeSpan.FromSeconds(0.8);
        //    this.BeginAnimation(Path.OpacityProperty, OpacityAnim);

        //    MenuItem menuItem = new MenuItem();
        //    menuItem.Header = "删除";
        //    menuItem.Click += new RoutedEventHandler(MenuItem_Click_Del);
        //    m_deleteMenu.Items.Add(menuItem);
        //}

        // 检测位置是否合法
        public bool IsLocationValued()
        {
            double x = Canvas.GetLeft(this);
            if(double.IsNaN(x))
                return false;
            double y = Canvas.GetTop(this);
            if(double.IsNaN(y))
                return false;

            if (x == 0 && y == 0)
                return false;

            return true;
        }
        
        public virtual void SetLocation(double x, double y)
        {
            if (Double.IsNaN(x) || Double.IsNaN(y))
                return;

            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);

            m_bNeedUpdateLocation = false;
        }

        public virtual Point GetLocation()
        {
            Point retPt = new Point();

            retPt.X = Canvas.GetLeft(this);
            retPt.Y = Canvas.GetTop(this);

            return retPt;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //double offset = 50;
            //foreach (var node in mMoveableChildNodes.Values)
            //{
            //    if (node.m_bNeedUpdateLocation && node.Visibility == System.Windows.Visibility.Visible)
            //    {
            //        Point pt = GetChildLinkPoint(node.mParentLinkIdx);
            //        node.SetLocation(pt.X, pt.Y + 50);
            //        offset += node.Height + 10;
            //    }
            //}

            UpdateLink();
        }

        public virtual void RemoveFromParent()
        {
            if (m_bMoveable)
            {
                if (ParentDrawCanvas != null)
                {
                    ParentDrawCanvas.Children.Remove(this);

                    if (mParentNode != null)
                    ParentDrawCanvas.Children.Remove(mParentLinkPath);
                }
            }
            else
            {
                var nodeContainer = ParentNode.GetChildNodeContainer(this);
                nodeContainer?.Children.Remove(this);
            }
            Visibility = Visibility.Hidden;
            ParentNode.mChildNodes.Remove(this);
        }

        partial void Clear_WPF()
        {
            if (m_bMoveable)
            {
                if (ParentDrawCanvas != null)
                {
                    ParentDrawCanvas.Children.Remove(this);

                    //if (mParentNode != null)
                    ParentDrawCanvas.Children.Remove(mParentLinkPath);
                }
            }

            foreach(var node in mChildNodes)
            {
                var nodeContainer = GetChildNodeContainer(node);
                nodeContainer?.Children.Remove(node);
            }

            //foreach(var node in mMoveableChildNodes.Values)
            //{
            //    if (ParentDrawCanvas != null)
            //        ParentDrawCanvas.Children.Remove(node);
            //}
        }

        partial void RemoveChildFromNodeContainerPanel(BaseNodeControl child)
        {
            var nodeContainer = GetChildNodeContainer(child);
            nodeContainer?.Children.Remove(child);
        }
        public virtual bool IsEqual(BaseNodeControl comNode) { return false; }
        public virtual string GetNodeDescriptionString()
        {
            //var linkNode = this;
            //while (linkNode != null && linkNode.ParentNode != null)
            //{
            //    linkNode = linkNode.ParentNode;
            //}
            //return linkNode.NodeName;
            return this.NodeName;
        }

        #region 链接相关

        partial void AddLinkPinInfo_WPF(LinkPinControl linkPin, Brush linkColor)
        {
            linkPin.MouseLeftButtonDown += new MouseButtonEventHandler(LinkObj_MouseLeftButtonDown);
            linkPin.MouseLeftButtonUp += new MouseButtonEventHandler(LinkObj_MouseLeftButtonUp);
            linkPin.MainDrawCanvas = ParentDrawCanvas;
            linkPin.UpdateToolTip_WPF();
            if (linkColor != null)
                linkPin.BackBrush = linkColor;
        }

        private void LinkObj_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var linkPin = sender as LinkPinControl;
            StartLink(linkPin);
            e.Handled = true;
        }
        private void LinkObj_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var linkPin = sender as LinkPinControl;
            EndLink(linkPin);
            e.Handled = true;
        }

        public virtual void UpdateLink()
        {
            //foreach (var link in m_outLinks)
            //    link.UpdateLink();

            //foreach (var link in m_inLinks)
            //    link.UpdateLink();
            foreach (var linkPin in mLinkPinInfoDic_Name.Values)
                linkPin.UpdateLink();

            foreach (var node in mChildNodes)
            {
                node.UpdateLink();
            }

            //foreach (var node in mMoveableChildNodes.Values)
            //{
            //    node.UpdateLink();
            //}

            //if (mParentNode != null)
            //{
            //    Point pt = mParentNode.GetChildLinkPoint(mParentLinkIdx);
            //    enBezierType parentBezType = mParentNode.GetChildLinkBezierType(mParentLinkIdx);

            //    Point pt2 = GetParentLinkPoint();//new Point(Canvas.GetLeft(this) + this.ActualWidth * 0.5, Canvas.GetTop(this));

            //    mParentLinkPathFig.StartPoint = pt;
            //    mParentLinkBezierSeg.Point3 = pt2;
            //    double delta = Math.Max(Math.Abs(mParentLinkBezierSeg.Point3.X - mParentLinkPathFig.StartPoint.X) / 2, 25);
            //    delta = Math.Min(150, delta);

            //    switch (parentBezType)
            //    {
            //        case enBezierType.Left:
            //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X - delta, mParentLinkPathFig.StartPoint.Y);
            //            break;
            //        case enBezierType.Right:
            //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X + delta, mParentLinkPathFig.StartPoint.Y);
            //            break;
            //        case enBezierType.Top:
            //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X, mParentLinkPathFig.StartPoint.Y - delta);
            //            break;
            //        case enBezierType.Bottom:
            //            mParentLinkBezierSeg.Point1 = new Point(mParentLinkPathFig.StartPoint.X, mParentLinkPathFig.StartPoint.Y + delta);
            //            break;
            //    }

            //    mParentLinkBezierSeg.Point2 = new Point(mParentLinkBezierSeg.Point3.X, mParentLinkBezierSeg.Point3.Y - delta);
            //}
        }

        public class UndoRedoData
        {
            public LinkPinControl StartObj;
            public LinkPinControl EndObj;
        }

        #endregion
        public Action<BaseNodeControl> OnDeleted { get; set; }
        protected virtual void MenuItem_Click_Del(object sender, RoutedEventArgs e)
        {
            //if (mMoveableChildNodes.Count > 0)
            //{
            //    EditorCommon.MessageBox.Show("该节点不能删除，请确保没有子节点");                
            //    return;
            //}
            if (!CheckCanDelete())
                return;
            
            List<UndoRedoData> undoRedoDatas = new List<UndoRedoData>();
            foreach(var lPin in mLinkPinInfoDic_Name.Values)
            {
                for(int i=0; i<lPin.GetLinkInfosCount(); i++)
                {
                    var lInfo = lPin.GetLinkInfo(i);
                    var data = new UndoRedoData();
                    data.StartObj = lInfo.m_linkFromObjectInfo;
                    data.EndObj = lInfo.m_linkToObjectInfo;
                    undoRedoDatas.Add(data);
                }
            }
            var redoAction = new Action<object>((obj) =>
            {
                HostNodesContainer.DeleteNode(this);
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                            (obj) =>
                                            {
                                                if(m_bMoveable)
                                                {
                                                    if(ParentDrawCanvas != null)
                                                    {
                                                        ParentDrawCanvas.Children.Add(this);
                                                        ParentDrawCanvas.Children.Add(mParentLinkPath);
                                                    }
                                                    HostNodesContainer.CtrlNodeList.Add(this);
                                                }
                                                foreach(var data in undoRedoDatas)
                                                {
                                                    var linkInfo = new LinkInfo(ParentDrawCanvas, data.StartObj, data.EndObj);
                                                }
                                            }, "Delete Node");
            IsDirty = true;
        }

        #region 鼠标拖动相关

        protected void SetDragObject(FrameworkElement dragObj)
        {
            if (dragObj == null)
                return;
            mDragObj = dragObj;

            dragObj.MouseLeftButtonDown += new MouseButtonEventHandler(DragObj_MouseLeftButtonDown);
            dragObj.MouseMove += new MouseEventHandler(DragObj_MouseMove);
            dragObj.MouseLeftButtonUp += new MouseButtonEventHandler(DragObj_MouseLeftButtonUp);
            
            // 添加删除菜单
            if(mSelfDeleteable)
                dragObj.ContextMenu = mContextMenu;
        }

        protected virtual Brush GetDragObjectBackground()
        {
            Brush retValue = null;

            var pro = mDragObj.GetType().GetProperty("Background");
            if (pro == null)
            {
                pro = mDragObj.GetType().GetProperty("Fill");
            }

            if (pro == null)
                return mNormalColor;

            if (pro.PropertyType == typeof(System.Windows.Media.Brush))
            {
                retValue = pro.GetValue(mDragObj, null) as Brush;
            }

            //if (m_dragObj.GetType().FullName.Contains("System.Windows.Controls"))
            //{
            //    retValue = ((System.Windows.Controls.Control)m_dragObj).Background;
            //}
            //else if (m_dragObj.GetType() == typeof(Rectangle))
            //{
            //    retValue = ((System.Windows.Shapes.Shape)m_dragObj).Fill;
            //}

            if (retValue == null)
                retValue = mNormalColor;

            return retValue;
        }

        protected virtual void DragObj_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndDrag();

            if((System.Windows.Input.Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down)
            {
                var tagSel = !Selected;
                SelectedNode(tagSel);
            }
            else
            {
                if (!m_bAlreadyDragMoved)
                    SelectedNode(true);
                else if (!Selected)
                    SelectedNode(true);
            }
            m_bAlreadyDragMoved = false;
            e.Handled = true;
        }
        Point oncePointRelativeCanvas = new Point(double.MaxValue, double.MaxValue);
        protected virtual void DragObj_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed /*&& Mouse.Captured == sender*/)
            {
                if (m_bDragMove && ParentDrawCanvas != null)
                {
                    var p = e.GetPosition(mParentDrawCanvas);
                    if (Math.Abs(p.X - oncePointRelativeCanvas.X) > 4 || Math.Abs(p.Y - oncePointRelativeCanvas.Y) > 4)
                    {
                        Mouse.Capture(sender as UIElement, CaptureMode.Element);
                        m_bAlreadyDragMoved = true;
                        DragMove(e);
                        e.Handled = true;
                    }
                }
            }
        }

        protected void SelectedNode(bool select)
        {
            // 有父节点的对象选择跟随父
            if (mParentNode == null)
            {
                if (Selected == select)
                    return;
                if (OnSelectNode != null)
                    OnSelectNode(this, select, true);

                Selected = select;
            }
        }
        protected void SelectedNodeIngoreParent()
        {
            // 有父节点的对象选择跟随父
            {
                // 为了选中节点，也可以取消选中节点，modify by ml
                bool IsSelected = Selected;
                if (OnSelectNode != null)
                    OnSelectNode(this, !Selected, true);
                Selected = !IsSelected;
            }
        }
        long onceTime = long.MaxValue;
        Point oncePoint = new Point(double.MaxValue, double.MaxValue);
        protected virtual void DragObj_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDrag(sender as UIElement, e);
            m_bAlreadyDragMoved = false;
            e.Handled = true;
            //双击,两次单机距离不超过4像素，时间再0.5秒以内视为双击
            var p = e.GetPosition(sender as UIElement);
            var pr = e.GetPosition(ParentDrawCanvas);
            var time = EngineNS.CEngine.Instance.EngineTime;
            if (Math.Abs(p.X - oncePoint.X) < 2 && Math.Abs(p.Y - oncePoint.Y) < 2 && (time - onceTime < Program.DoubleClickThreshold))
            {
                DragObj_MouseLeftButtonDoubleClick(sender, e);
            }
            onceTime = time;
            oncePoint = p;
            oncePointRelativeCanvas = pr;
        }

        void DragObj_MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseLeftButtonDoubleClick(sender, e);
            OnMouseLeftButtonDoubleClick?.Invoke(this, e);
        }
        public virtual void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        // 计算mDeltaPt,只有mDeltaPt被正确赋值之后移动才会正常
        public void CalculateDeltaPt(MouseButtonEventArgs e)
        {
            GetMousePosition(e);
            //foreach (var node in mMoveableChildNodes.Values)
            //{
            //    node.GetMousePosition(e);
            //}
        }

        private Point GetMousePosition(MouseButtonEventArgs e)
        {
            mDeltaPt = e.GetPosition(this);
            return mDeltaPt;
        }

        Point mDragBeforePos;   // 移动开始时记录一下位置，以便计算有没有移动
        int mZIndexStore = 0;
        protected virtual void StartDrag(UIElement dragObj, MouseButtonEventArgs e)
        {
            
            mDeltaPt = e.GetPosition(this);
            mZIndexStore = Canvas.GetZIndex(this);
            Canvas.SetZIndex(this, 1);
            mDragBeforePos = GetLocation();
            m_bDragMove = true;

            //StartDragChildren(e);
            if (OnStartDragMoveNode != null)
                OnStartDragMoveNode(this, e);
        }

        public void MoveWithPt(Point pt)
        {
            Point newPos = new Point(pt.X - mDeltaPt.X, pt.Y - mDeltaPt.Y);
            SetLocation(newPos.X, newPos.Y);
            //Canvas.SetLeft(this, newPos.X);
            //Canvas.SetTop(this, newPos.Y);

            //MoveChildren(pt);

            if (OnMoveNode != null)
                OnMoveNode(this);

            UpdateLink();
        }
        protected virtual void DragMove(MouseEventArgs e)
        {
            if (m_bDragMove && ParentDrawCanvas != null)
            {
                Point pt = e.GetPosition(ParentDrawCanvas);
                MoveWithPt(pt);
            }
            if (OnDragMoveNode != null && Selected)
                OnDragMoveNode(this, e);
        }
        protected void _OnMoveNode(BaseNodeControl node)
        {

            if (OnMoveNode != null)
                OnMoveNode(this);
        }
        protected void _OnDragMoveNode(BaseNodeControl node, MouseEventArgs e)
        {
            if (OnDragMoveNode != null && Selected)
                OnDragMoveNode(this, e);
        }
        protected virtual void EndDrag()
        {
            Mouse.Capture(null);
            Canvas.SetZIndex(this, mZIndexStore);

            var pos = GetLocation();
            if ((pos - mDragBeforePos).Length > 0.0001)
            {
                var action = new Action<object>((obj) =>
                {
                    var pt = (Point)obj;
                    SetLocation(pt.X, pt.Y);
                    UpdateLayout();
                    UpdateLink();
                });
                var key = HostNodesContainer.HostControl.UndoRedoKey;
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(key, pos, action, mDragBeforePos, action, "Move node");
                action.Invoke(pos);
                IsDirty = true;
            }

            m_bDragMove = false;
        }

        //protected virtual void StartDragChildren(MouseButtonEventArgs e)
        //{
        //    // 用于移动子对象
        //    foreach (var node in mMoveableChildNodes.Values)
        //    {
        //        //node.CalculateMoveSrcPt(e);
        //        node.GetMousePosition(e);
        //    }
        //}

        //public void InitializeUsefulMember(Action<CodeGenerateSystem.Base.BaseNodeControl> action)
        //{
        //    if(this is UsefulMember)
        //    {
        //        action(this);
        //    }

        //    foreach(var child in mChildNodes)
        //    {
        //        child.InitializeUsefulMember(action);
        //    }
        //    foreach(var child in mMoveableChildNodes.Values)
        //    {
        //        child.InitializeUsefulMember(action);
        //    }
        //}

        #endregion

        #region 父子节点相关

        partial void SetParentNode_WPF(BaseNodeControl parentNode, bool Moveable, Visibility Visible)
        {
            //if (Moveable && ParentDrawCanvas != null)
            //{
            //    // 父级连线
            //    //m_parentClassLinkLine.Stroke = System.Windows.Media.Brushes.LightGray;
            //    //m_parentClassLinkLine.Visibility = Visibility.Hidden;
            //    //ParentDrawCanvas.Children.Add(m_parentClassLinkLine);                

            //    //m_ParentLinkPath.Visibility = Visible;
            //    //BindingOperations.ClearBinding(this.mParentLinkPath, Path.VisibilityProperty);
            //    //BindingOperations.SetBinding(this.mParentLinkPath, Path.VisibilityProperty, new Binding("Visibility") { Source = this });
            //    mParentLinkPath.Stroke = Brushes.LightGray;
            //    mParentLinkPath.StrokeDashArray = new DoubleCollection(new double[] { 2, 4 });
            //    //m_ParentLinkPath.StrokeThickness = 3;
            //    mParentLinkPathFig.Segments.Add(mParentLinkBezierSeg);
            //    PathFigureCollection pfc = new PathFigureCollection();
            //    pfc.Add(mParentLinkPathFig);
            //    PathGeometry pg = new PathGeometry();
            //    pg.Figures = pfc;
            //    mParentLinkPath.Data = pg;
            //    ParentDrawCanvas.Children.Add(mParentLinkPath);
            //}

        }

        partial void AddChildNodeNoChanageContainer_WPF(BaseNodeControl node, Panel container)
        {
            //ConditionControl cc = new ConditionControl();
            //cc.OnStartLink += new Delegate_StartLink(StartLink);
            //cc.OnEndLink += new Delegate_EndLink(EndLink);
            //m_conditionControls.Add(cc);
            //LinkStack.Children.Add(cc);

            if (container != null)
                container.Children.Add(node);

            UpdateLink();
        }

        partial void AddChildNode_WPF(BaseNodeControl node, System.Windows.Controls.Panel container)
        {
            mChildNodeContainer = container;
            if (mChildNodeContainer != null)
                mChildNodeContainer.Children.Add(node);
            UpdateLink();
        }
        partial void InsertChildNodeNoChangeContainer_WPF(int index, BaseNodeControl node, System.Windows.Controls.Panel container)
        {
            if (index < 0 || index >= container.Children.Count)
                throw new InvalidOperationException("index超出范围");
            if (container != null)
                container.Children.Insert(index, node);
            UpdateLink();
        }
        partial void InsertChildNode_WPF(int index, BaseNodeControl node, System.Windows.Controls.Panel container)
        {
            if (index < 0 || index >= mChildNodeContainer.Children.Count)
                throw new InvalidOperationException("index超出范围");
            mChildNodeContainer = container;
            if (mChildNodeContainer != null)
                mChildNodeContainer.Children.Insert(index, node);
            UpdateLink();
        }

        //// 没有container的node属于能自主移动的node
        //protected void AddChildNode(string name, BaseNodeControl node, int linkHandleIdx, Visibility visible)
        //{
        //    BaseNodeControl existNode = null;
        //    if (mMoveableChildNodes.TryGetValue(name, out existNode))
        //        DelChildNode(existNode);

        //    mNodeContainer = null;
        //    mMoveableChildNodes[name] = node;

        //    node.m_nodeName = name;
        //    node.SetParentNode(this, true, visible);

        //    node.OnStartLink += new Delegate_StartLink(StartLink);
        //    node.OnEndLink += new Delegate_EndLink(EndLink);
        //    node.OnGetLinkObjectWithGUID += new Delegate_GetLinkObjectWithGUID(Child_OnGetLinkObjectWithGUID);
        //    node.OnAddErrorMsg = this.OnAddErrorMsg;

        //    node.mParentLinkIdx = linkHandleIdx;

        //    if(ParentDrawCanvas != null)
        //        ParentDrawCanvas.Children.Add(node);
        //}

        //public void ClearMoveableChildNode()
        //{
        //    foreach (var child in mMoveableChildNodes.Values)
        //    {
        //        child.OnStartLink -= new Delegate_StartLink(StartLink);
        //        child.OnEndLink -= new Delegate_EndLink(EndLink);

        //        child.Clear();
        //    }
        //    mMoveableChildNodes.Clear();
        //}

        protected void DelChildNode(BaseNodeControl node)
        {
            if(mChildNodes.Contains(node))
                mChildNodes.Remove(node);
            var nodeContainer = GetChildNodeContainer(node);
            nodeContainer?.Children.Remove(node);

            //if(mMoveableChildNodes.ContainsValue(node))
            //    mMoveableChildNodes.Remove(node.m_nodeName);

            ////////////if (ParentDrawCanvas != null)
            ////////////{
            ////////////    if (ParentDrawCanvas.Children.Contains(node))
            ////////////        ParentDrawCanvas.Children.Remove(node);
            ////////////}

            node.Clear();

            UpdateLink();
        }

        public virtual Point GetChildLinkPoint(int nIdx)
        {
            return TranslatePoint(new Point(0, 0), ParentDrawCanvas);
        }

        public virtual enBezierType GetChildLinkBezierType(int nIdx)
        {
            return enBezierType.Bottom;
        }

        //public BaseNodeControl GetChildNode(string name)
        //{
        //    BaseNodeControl node = null;
        //    mMoveableChildNodes.TryGetValue(name, out node);

        //    return node;
        //}

        public virtual Point GetParentLinkPoint()
        {
            return TranslatePoint(new Point(0, 0), ParentDrawCanvas);
        }

        //protected virtual void MoveChildren(Point pt)
        //{
        //    foreach (var node in mMoveableChildNodes.Values)
        //    {
        //        node.MoveWithPt(pt);
        //    }
        //}
        #endregion

        #region 储存读取

        partial void Save_WPF(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.AddAttrib("_baseNodeData_WPF");
            att.BeginWrite();
            int ver = 1;
            att.Write(ver);
            double left = Canvas.GetLeft(this);
            att.Write(left);
            double top = Canvas.GetTop(this);
            att.Write(top);
            att.Write(IsDeleteable);
            att.Write(Comment);
            att.Write(NodeNameAddShowNodeName);
            att.EndWrite();
        }
        partial void SetConstructionParams_WPF(ConstructionParams csParam)
        {
            csParam.DrawCanvas = this.ParentDrawCanvas;
        }
        partial void Load_WPF(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("_baseNodeData_WPF");
            if (att == null)
                return;
            att.BeginRead();
            int ver;
            att.Read(out ver);
            switch(ver)
            {
                case 0:
                    {
                        double left, top;
                        att.Read(out left);
                        att.Read(out top);
                        Canvas.SetLeft(this, left);
                        Canvas.SetTop(this, top);
                        bool deleteAble;
                        att.Read(out deleteAble);
                        IsDeleteable = deleteAble;
                        string comment;
                        att.Read(out comment);
                        Comment = comment;
                    }
                    break;
                case 1:
                    {
                        double left, top;
                        att.Read(out left);
                        att.Read(out top);
                        Canvas.SetLeft(this, left);
                        Canvas.SetTop(this, top);
                        bool deleteAble;
                        att.Read(out deleteAble);
                        IsDeleteable = deleteAble;
                        string comment;
                        att.Read(out comment);
                        Comment = comment;
                        att.Read(out NodeNameAddShowNodeName);
                    }
                    break;
            }
            att.EndRead();
        }

        #endregion

        #region 错误处理

        public string ErrorDescription
        {
            get { return (string)GetValue(ErrorDescriptionProperty); }
            set { SetValue(ErrorDescriptionProperty, value); }
        }
        public static readonly DependencyProperty ErrorDescriptionProperty = DependencyProperty.Register("ErrorDescription", typeof(string), typeof(BaseNodeControl), new PropertyMetadata(""));

        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }
        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register("HasError", typeof(bool), typeof(BaseNodeControl), new UIPropertyMetadata(false));

        bool mHasWarning;
        public bool HasWarning
        {
            get { return mHasWarning; }
            set
            {
                mHasWarning = value;
            }
        }

        public bool CheckError()
        {
            HasError = false;
            ErrorDescription = "";
            Selected = false;

            CollectionErrorMsg();

            //foreach (var cNode in mMoveableChildNodes.Values)
            //{
            //     cNode.CheckError();
            //}

            return !HasError && !HasWarning;
        }
        
        protected virtual void CollectionErrorMsg() { }

        // 收集构造时的错误信息
        public virtual void CollectConstructionErrors() { }

        #endregion

        /// <summary>
        /// 获取在画布上的左坐标值
        /// </summary>
        /// <param name="withChildren">是否计算子节点</param>
        /// <returns></returns>
        public double GetLeftInCanvas(bool withChildren)
        {
            if(withChildren)
            {
                var left = Canvas.GetLeft(this);
                //foreach(var child in mMoveableChildNodes.Values)
                //{
                //    var lt = child.GetLeftInCanvas(withChildren);
                //    if (left > lt)
                //        left = lt;
                //}
                return left;
            }
            else
            {
                return Canvas.GetLeft(this);
            }
        }
        /// <summary>
        /// 获取在画布上的上坐标值
        /// </summary>
        /// <param name="withChildren">是否计算子节点</param>
        /// <returns></returns>
        public double GetTopInCanvas(bool withChildren)
        {
            if(withChildren)
            {
                var top = Canvas.GetTop(this);
                //foreach(var child in mMoveableChildNodes.Values)
                //{
                //    var tp = child.GetTopInCanvas(withChildren);
                //    if (top > tp)
                //        top = tp;
                //}
                return top;
            }
            else
            {
                return Canvas.GetTop(this);
            }
        }
        /// <summary>
        /// 获取在画布上的宽度
        /// </summary>
        /// <param name="withChildren">是否计算子节点</param>
        /// <returns></returns>
        public virtual double GetWidth(bool withChildren = true)
        {
            if (NodeType == enNodeType.CommentNode)
                return Width;
            else
            {
                if (withChildren)
                {
                    var width = this.ActualWidth;
                    //foreach(var child in mMoveableChildNodes.Values)
                    //{
                    //    var rt = child.GetWidth(withChildren);
                    //    if (width < rt)
                    //        width = rt;
                    //}
                    return width;
                }
                else
                {
                    return this.ActualWidth;
                }
            }
        }
        /// <summary>
        /// 获取在画布上的高度
        /// </summary>
        /// <param name="withChildren">是否计算子节点</param>
        /// <returns></returns>
        public virtual double GetHeight(bool withChildren = true)
        {
            if(NodeType == enNodeType.CommentNode)
            {
                return Height;
            }
            else
            {
                if (withChildren)
                {
                    var height = this.ActualHeight;
                    //foreach(var child in mMoveableChildNodes.Values)
                    //{
                    //    var bt = child.GetHeight(withChildren);
                    //    if (height < bt)
                    //        height = bt;
                    //}
                    return height;
                }
                else
                {
                    return this.ActualHeight;
                }
            }
        }
        
        /// <summary>
        /// 获取节点在选中时显示属性的对象
        /// </summary>
        /// <returns></returns>
        public virtual object GetShowPropertyObject()
        {
            return null;
        }

        #region 特殊节点操作

        // 从函数连接串取得该节点所连接到的特殊类型节点
        public virtual BaseNodeControl GetSpecialTypeLinkedNodeFromMethodLinkStream(Type nodeType)
        {
            if (m_methodUpLinkElement == null || !m_methodUpLinkElement.HasLink)
                return null;

            if (m_methodUpLinkElement.GetLinkedObject(0, false).GetType() == nodeType)
                return m_methodUpLinkElement.GetLinkedObject(0, false);
            else
                return m_methodUpLinkElement.GetLinkedObject(0, false).GetSpecialTypeLinkedNodeFromMethodLinkStream(nodeType);
        }

        // StatementNode
        public virtual string GetStatementTypeName() { return "None"; }
        public virtual bool GetIsDefaultState() { return false; }
        public virtual FrameworkElement GetMirrorLinkElement() { return null; }
        
        public virtual LinkPinControl GetInsideLinkObjInfoFromOutsideLinkObjInfo(LinkPinControl linkObj) { return null; }

        // AnimNode
        public System.CodeDom.CodeVariableReferenceExpression m_ParentAnimNode_CodeReference;
        public virtual void AddAnimNode(System.CodeDom.CodeVariableReferenceExpression parentNode)
        {
            m_ParentAnimNode_CodeReference = parentNode;
        }

        #endregion

        DoubleCollection mDashArrayStore;
        double mThicknessStore;
        Brush mColorStore;
        protected void ChangeLinkLine(bool change, LinkPinControl linkOI, DoubleCollection dashArray, double thickness, Brush color)
        {
            if (linkOI.HasLink)
            {
                var info = linkOI.GetLinkInfo(0);
                if (change)
                {
                    if (info.DashArray != dashArray)
                    {
                        mDashArrayStore = info.DashArray;
                        info.DashArray = dashArray;
                    }
                    if(info.Thickness != thickness)
                    {
                        mThicknessStore = info.Thickness;
                        info.Thickness = thickness;
                    }
                    if(info.Color != color)
                    {
                        mColorStore = info.Color;
                        info.Color = color;
                    }
                }
                else
                {
                    info.DashArray = mDashArrayStore;
                    info.Thickness = mThicknessStore;
                    info.Color = mColorStore;
                }
            }
        }

        public virtual void RefreshFromLink(LinkPinControl pin, int linkIndex)
        {

        }

        partial void SetNameBinding_WPF(object source, string propertyName)
        {
            SetBinding(BaseNodeControl.NodeNameBinderProperty, new Binding(propertyName) { Source = source, Mode = BindingMode.TwoWay });
        }

        #region Tick
        public virtual void Tick(Int64 elapsedMillisecond)
        {

        }
        #endregion

        #region Debug

        protected void ChangeParentLogicLinkLine(bool change, LinkPinControl pin)
        {
            ChangeLinkLine(change, pin,
                CodeGenerateSystem.Program.DebugLineDashArray,
                CodeGenerateSystem.Program.DebugLineThickness,
                CodeGenerateSystem.Program.DebugLineColor);
            if (change)
                EditorCommon.TickManager.Instance.AddTickNode(this);
            else
                EditorCommon.TickManager.Instance.RemoveTickNode(this);

            var obj = pin.GetLinkedObject(0, true) as CodeGenerateSystem.Base.IDebugableNode;
            if (obj != null)
                obj.ChangeParentLogicLinkLine(change);
        }
        protected void TickDebugLine(long elapsedMillisecond, LinkPinControl pin)
        {
            if (pin.HasLink)
                pin.GetLinkInfo(0).Offset += elapsedMillisecond * CodeGenerateSystem.Program.DebugLineOffsetSpeed;
        }

        #endregion
    }
}
