using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// NodesContainerControl.xaml 的交互逻辑
    /// </summary>
    public partial class NodesContainerControl : CodeGenerateSystem.Base.NodesContainer
    {
        readonly float mScaleDeltaMin = 10;
        readonly float mScaleDeltaMax = 200;

        public string TypeString
        {
            get { return (string)GetValue(TypeStringProperty); }
            set { SetValue(TypeStringProperty, value); }
        }
        public static readonly DependencyProperty TypeStringProperty = DependencyProperty.Register("TypeString", typeof(string), typeof(NodesContainerControl), new UIPropertyMetadata(""));

        // 从UI节点获取NodesContainerControl
        public static NodesContainerControl GetNodesContainerControl(FrameworkElement element)
        {
            if (element == null)
                return null;

            var parent = element.Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent is NodesContainerControl)
                    break;

                parent = parent.Parent as FrameworkElement;
            }

            if (parent == null)
                return null;

            return parent as NodesContainerControl;
        }
        public Canvas _MainDrawCanvas
        {
            get { return MainDrawCanvas; }
        }
        public Canvas _RectCanvas
        {
            get { return RectCanvas; }
        }
        protected override NodesContainer CreateContainer()
        {
            return new NodesContainerControl();
        }
        public NodesContainerControl()
        {
            InitializeComponent();

            ContainerDrawCanvas = MainDrawCanvas;

            Canvas.SetLeft(ViewBoxMain, 0);
            Canvas.SetTop(ViewBoxMain, 0);

            ContextMenuNodeList.GetNodesList().OnNodeListItemSelected = _OnNodeListItemSelected;
            ContextMenuNodeList.OnCopy = () => 
            { 
                Copy();
                IsOpenContextMenu = false;
            };
            ContextMenuNodeList.OnPaste = () =>
            {
                Paste(mContextMenuOpenMousePos);
                IsOpenContextMenu = false;
            };
            ContextMenuNodeList.OnDelete = () =>
            {
                DeleteSelectedNodes();
                IsOpenContextMenu = false;
            };
        }
        void _OnNodeListItemSelected(NodeListAttributeClass item)
        {
            IsOpenContextMenu = false;
            var node = CreateNodeFromNodeListItem(item, mContextMenuOpenMousePos);
            if (node != null && item.FilterData.StartLinkObj != null)
            {
                var nodePins = node.GetLinkPinInfos();

                LinkPinControl nodeLink = null;
                foreach (var pin in nodePins)
                {
                    if (pin.Visibility != Visibility.Visible)
                        continue;
                    if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(item.FilterData.StartLinkObj, pin))
                    {
                        nodeLink = pin;
                        break;
                    }
                }
                if (nodeLink != null)
                {
                    if (nodeLink.Visibility == Visibility.Visible)
                    {
                        LinkPinControl startLink, endLink;
                        if (item.FilterData.StartLinkObj.LinkOpType == enLinkOpType.Start)
                        {
                            startLink = item.FilterData.StartLinkObj;
                            endLink = nodeLink;
                        }
                        else
                        {
                            startLink = nodeLink;
                            endLink = item.FilterData.StartLinkObj;
                        }
                        var linkInfo = LinkInfo.CreateLinkInfo(nodeLink.LinkCurveType, MainDrawCanvas, startLink, endLink);

                    }
                }
            }
        }
        //public void InitializeUsefulMember(Action<CodeGenerateSystem.Base.BaseNodeControl> action)
        //{
        //    foreach(var node in CtrlNodeList)
        //    {
        //        node.InitializeUsefulMember(action);
        //    }
        //}

        //         void NodesContainerControl_OnOperateNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        //         {
        //             ContainLinkNodes = mCtrlNodeList.Count != mOrigionNodeControls.Count;
        // 
        //             RefreshNodeProperty(node);
        //         }

        #region 鼠标键盘操作

        bool mMouseDown = false;
        bool m_MouseLeftButtonDownInRectCanvas = false;

        bool mShowRightButtonMenu = true;
        private void RightButtonMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // 在鼠标右键拖动时不显示右键菜单
            if (!mShowRightButtonMenu)
                e.Handled = true;
        }

        private void RectCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton == MouseButton.Right)
            {
                mShowRightButtonMenu = true;

                Mouse.Capture(sender as UIElement, CaptureMode.Element);
                mMouseRightButtonDownPt = e.GetPosition(ViewBoxMain);
                e.Handled = true;
                mMouseDown = true;
            }
        }

        private void RectCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            mMouseDown = false;
            if (e.ChangedButton == MouseButton.Right)
            {
                if (mShowRightButtonMenu)
                {
                    mStartLinkObj = null;
                    IsOpenContextMenu = true;
                }
            }
        }

        private void RectCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;


            //// 按下中键并滚动滚轮时进行缩放，防止缩放时鼠标进入子窗口导致被缩放目标变化的问题
            //if (e.MiddleButton == MouseButtonState.Pressed)
            {
                double delta = 1 + e.Delta / 1000.0;

                Point center = e.GetPosition(RectCanvas);//new Point(RectCanvas.ActualWidth / 2, RectCanvas.ActualHeight / 2);
                //Point center = new Point(RectCanvas.ActualWidth / 2, RectCanvas.ActualHeight / 2);
                //center = RectCanvas.TranslatePoint(center, ViewBoxMain);
                //ViewboxScaleTransform.CenterX = center.X;
                //ViewboxScaleTransform.CenterY = center.Y;

                Point deltaXY = new Point(center.X * delta, center.Y * delta);

                double left = Canvas.GetLeft(ViewBoxMain);
                double top = Canvas.GetTop(ViewBoxMain);

                if (Double.IsNaN(left))
                    left = 0;
                if (Double.IsNaN(top))
                    top = 0;

                left = (left / center.X) * deltaXY.X + center.X - deltaXY.X;
                top = (top / center.Y) * deltaXY.Y + center.Y - deltaXY.Y;


                var width = ViewBoxMain.ActualWidth * delta;

                // 微量改变容器的大小以便自动调用OnRenderSizeChanged来重新计算容器内部连线位置
                //this.Width = this.ActualWidth + 0.00001;

                var scaleDelta = width / MainDrawCanvas.Width * 100;
                //if (scaleDelta < mScaleMin)
                //{
                //    width = mScaleMin / 100 * MainDrawCanvas.Width;
                //}
                //if (scaleDelta > mScaleMax)
                //{
                //    width = mScaleMax / 100 * MainDrawCanvas.Width;
                //}
                if (scaleDelta < mScaleDeltaMin || scaleDelta > mScaleDeltaMax)
                    return;

                Canvas.SetLeft(ViewBoxMain, left);
                Canvas.SetTop(ViewBoxMain, top);
                ViewBoxMain.Width = width;
                UpdateScaleInfoShow();
            }
        }

        void UpdateScaleInfoShow()
        {
            var scaleDelta = (int)(ViewBoxMain.Width / MainDrawCanvas.Width * 100);
            TextBlock_Scale.Text = $"Zoom {scaleDelta}%";
            var sb = TryFindResource("Storyboard_ScaleInfoShow") as Storyboard;
            if (sb != null)
                sb.Begin();
            ScaleTips(scaleDelta);
        }

        private void RectCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (e.RightButton == MouseButtonState.Pressed && mMouseDown)
            {
                Point pt = e.GetPosition(RectCanvas);

                Point newPos = new Point(pt.X - mMouseRightButtonDownPt.X, pt.Y - mMouseRightButtonDownPt.Y);
                if ((newPos.X * newPos.X + newPos.Y * newPos.Y) > 5)
                    mShowRightButtonMenu = false;

                Canvas.SetLeft(ViewBoxMain, newPos.X);
                Canvas.SetTop(ViewBoxMain, newPos.Y);

                // 微量改变容器的大小以便自动调用OnRenderSizeChanged来重新计算容器内部连线位置
                //this.Width = this.ActualWidth + 0.00001;
            }
            else if (m_MouseLeftButtonDownInRectCanvas)
            {
                if (SelectedRect.Visibility == System.Windows.Visibility.Hidden)
                    SelectedRect.Visibility = System.Windows.Visibility.Visible;
                Point pt = e.GetPosition(MainDrawCanvas);

                // 设置选择框
                if (pt.X > mMouseLeftButtonDownPt.X)
                {
                    SelectedRect.Width = pt.X - mMouseLeftButtonDownPt.X;
                    Canvas.SetLeft(SelectedRect, mMouseLeftButtonDownPt.X);
                }
                else
                {
                    SelectedRect.Width = mMouseLeftButtonDownPt.X - pt.X;
                    Canvas.SetLeft(SelectedRect, pt.X);
                }

                if (pt.Y > mMouseLeftButtonDownPt.Y)
                {
                    SelectedRect.Height = pt.Y - mMouseLeftButtonDownPt.Y;
                    Canvas.SetTop(SelectedRect, mMouseLeftButtonDownPt.Y);
                }
                else
                {
                    SelectedRect.Height = mMouseLeftButtonDownPt.Y - pt.Y;
                    Canvas.SetTop(SelectedRect, pt.Y);
                }
            }

            // 设置预览的贝塞尔曲线位置
            if (m_enPreviewBezierType != CodeGenerateSystem.Base.enBezierType.None)
            {
                Point pt = e.GetPosition(MainDrawCanvas);
                UpdatePreviewCurve(pt);

            }
        }

        bool mLeftButtonIsDown = false;
        private void RectCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            m_MouseLeftButtonDownInRectCanvas = true;
            mMouseLeftButtonDownPt = e.GetPosition(MainDrawCanvas);
            Mouse.Capture(MainDrawCanvas, CaptureMode.Element);
            mLeftButtonIsDown = true;
        }

        private void RectCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            EndPreviewCurve(null);
            if (mLeftButtonIsDown)
            {
                if ((System.Windows.Input.Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != KeyStates.Down)
                {
                    // 取消选中链接点
                    SelectedLinkControl = null;
                }

                m_MouseLeftButtonDownInRectCanvas = false;
                SelectedRect.Visibility = System.Windows.Visibility.Hidden;
                Mouse.Capture(null);
                var Pt = e.GetPosition(MainDrawCanvas);
                var left = System.Math.Min(Pt.X, mMouseLeftButtonDownPt.X);
                var right = System.Math.Max(Pt.X, mMouseLeftButtonDownPt.X);
                var top = System.Math.Min(Pt.Y, mMouseLeftButtonDownPt.Y);
                var bottom = System.Math.Max(Pt.Y, mMouseLeftButtonDownPt.Y);
                if (!SelectNodes(left, right, top, bottom))
                    SelectNull();
                mLeftButtonIsDown = false;
            }
        }


        #endregion

        #region DragDrop

        public delegate bool Delegate_CheckDropAvailable(DragEventArgs e);
        public Delegate_CheckDropAvailable OnCheckDropAvailable;
        private bool CheckDropAvailable(DragEventArgs e)
        {
            if (OnCheckDropAvailable != null)
                return OnCheckDropAvailable.Invoke(e);
            else
            {
                if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
                {
                    var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
                    var data = dragObject[0] as EditorCommon.CodeGenerateSystem.INodeListAttribute;
                    if (data != null)
                        return true;
                }
                if (CodeGenerateSystem.Program.NodeDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
                    return true;
                return false;
            }
        }

        EditorCommon.DragDrop.DropAdorner mDropAdorner;
        private void RectCanvas_DragEnter(object sender, DragEventArgs e)
        {
            mDropAdorner = new EditorCommon.DragDrop.DropAdorner(RectCanvas);

            var pos = e.GetPosition(RectCanvas);
            if (pos.X > 0 && pos.X < RectCanvas.ActualWidth &&
               pos.Y > 0 && pos.Y < RectCanvas.ActualHeight)
            {
                var layer = AdornerLayer.GetAdornerLayer(RectCanvas);
                layer.Add(mDropAdorner);
            }

            mDropAdorner.IsAllowDrop = CheckDropAvailable(e);
        }

        private void RectCanvas_DragLeave(object sender, DragEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(RectCanvas);
            layer.Remove(mDropAdorner);
        }

        private void RectCanvas_DragOver(object sender, DragEventArgs e)
        {

        }

        BaseNodeControl CreateNodeFromNodeListItem(EditorCommon.CodeGenerateSystem.INodeListAttribute item, Point pos)
        {
            if (item.BindingFile != "")
            {
                //    var copyXmlHolder = EngineNS.IO.XmlHolder.LoadXML(dragedItem.BindingFile);
                //    if (copyXmlHolder != null)
                //    {
                //        string mXmlString = "";
                //        List<string> mCopyPasteIdStrList = new List<string>();

                //        EngineNS.IO.XmlHolder.GetXMLString(ref mXmlString, copyXmlHolder);

                //        // 提取ID信息
                //        var startIdx = 0;
                //        var idx = 0;
                //        do
                //        {
                //            idx = mXmlString.IndexOf("ID=\"", startIdx);
                //            if (idx >= 0)
                //            {
                //                var idStr = mXmlString.Substring(idx + 4, 36);
                //                mCopyPasteIdStrList.Add(idStr);
                //                startIdx = idx + 36;
                //            }
                //        }
                //        while (idx >= 0);

                //        // 替换所有ID
                //        var tempStr = mXmlString;
                //        var idList = new List<string>();
                //        foreach (var idStr in mCopyPasteIdStrList)
                //        {
                //            var id = Guid.NewGuid().ToString();
                //            tempStr = tempStr.Replace(idStr, id);
                //            idList.Add(id);
                //        }
                //        var xmlHolder = EngineNS.IO.XmlHolder.ParseXML(tempStr);

                //        CodeGenerateSystem.Base.BaseNodeControl nodeControl = null;

                //        // 读取节点信息并创建节点
                //        foreach (var xmlNode in xmlHolder.RootNode.GetNodes())
                //        {
                //            Type ctrlType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(xmlNode.FindAttrib("Type").Value);//  Program.GetType(xmlNode.FindAttrib("Type").Value);
                //            if (ctrlType == null)
                //                continue;
                //            //Type nodeType = Type.GetType(element.GetAttribute("Type"));

                //            //string strParam = element.GetAttribute("Params");
                //            var paramAtt = xmlNode.FindAttrib("Params");
                //            string strParam = null;
                //            if (paramAtt != null)
                //                strParam = paramAtt.Value;

                //            //double x = System.Convert.ToDouble(element.GetAttribute("X"));
                //            //double y = System.Convert.ToDouble(element.GetAttribute("Y"));
                //            double x = System.Convert.ToDouble(xmlNode.FindAttrib("X").Value);
                //            double y = System.Convert.ToDouble(xmlNode.FindAttrib("Y").Value);

                //            var csParam = new Base.ConstructionParams()
                //            {
                //                CSType = this.mCSType,
                //                HostNodesContainer = this,
                //                ConstructParam = strParam,
                //            };

                //            ////AddNodeControl(nodeType, paramList.ToArray(), x, y);
                //            nodeControl = AddNodeControl(ctrlType, csParam, x, y);
                //            nodeControl.Load(xmlNode);
                //        }

                //        // 读取链接信息

                //        foreach (var xmlNode in xmlHolder.RootNode.GetNodes())
                //        {
                //            nodeControl.LoadLinks(xmlNode);
                //        }

                //        nodeControl.InitializeUsefulLinkDatas();

                //        IsLoading = false;
                //        IsDirty = false;
                //        //RefreshNodeProperty(null, ENodeHandleType.UpdateNodeControl);

                //    }
            }
            else
            {
                var csParam = item.CSParam.Duplicate() as ConstructionParams;
                csParam.CSType = this.CSType;
                csParam.HostNodesContainer = this;
                csParam.DrawCanvas = ContainerDrawCanvas;
                RenameAnimGraphMacrossNodeName(csParam, item.NodeType);
                var ctrl = AddNodeControl(item.NodeType, csParam, pos.X, pos.Y);
                //ctrl.InitializeUsefulLinkDatas();
                return ctrl;
            }
            return null;
        }
        public void RenameAnimGraphMacrossNodeName(ConstructionParams cp, Type nodeType)
        {
            var animMac = cp as AnimMacrossConstructionParams;
            if (animMac != null)
            {
                foreach (var node in mCtrlNodeList)
                {
                    if (node.GetType() == nodeType)
                    {
                        if (node.NodeName == animMac.NodeName)
                        {
                            animMac.NodeName += "_Suffix";
                            //check new name has the same or not
                            RenameAnimGraphMacrossNodeName(cp, nodeType);
                        }
                    }
                }
            }
        }
        public delegate void Delegate_OnDrop(object sender, DragEventArgs e);
        public Delegate_OnDrop _OnDrop;
        bool isCanDrop = false;
        private void RectCanvas_Drop(object sender, DragEventArgs e)
        {
            isCanDrop = false;
            var layer = AdornerLayer.GetAdornerLayer(RectCanvas);
            layer.Remove(mDropAdorner);

            if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
                var data = dragObject[0] as EditorCommon.CodeGenerateSystem.INodeListAttribute;


                if (data != null)
                    isCanDrop = true;
            }
            if (CodeGenerateSystem.Program.NodeDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                isCanDrop = true;
            }
            if (isCanDrop)
            {
                // 节点拖放
                var formats = e.Data.GetFormats();
                if (formats == null || formats.Length == 0)
                    return;

                var datas = e.Data.GetData(formats[0]) as EditorCommon.DragDrop.IDragAbleObject[];
                if (datas == null)
                    return;

                var pos = e.GetPosition(MainDrawCanvas);

                foreach (var data in datas)
                {
                    var dragedItem = data as EditorCommon.CodeGenerateSystem.INodeListAttribute;
                    if (dragedItem == null)
                        continue;

                    CreateNodeFromNodeListItem(dragedItem, pos);
                }
            }

            _OnDrop?.Invoke(sender, e);
        }
        #endregion

        public override bool Save(EngineNS.IO.XndNode xndNode)
        {
            //xmlHolder.RootNode.AddAttrib("ViewBoxMain_X", Canvas.GetLeft(ViewBoxMain).ToString());
            //xmlHolder.RootNode.AddAttrib("ViewBoxMain_Y", Canvas.GetTop(ViewBoxMain).ToString());
            //xmlHolder.RootNode.AddAttrib("ViewBoxMain_Width", ViewBoxMain.ActualWidth.ToString());

            return base.Save(xndNode);
        }

        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            mIgnoreDirtySet = true;
            await base.Load(xndNode);
            mIgnoreDirtySet = false;

            // 读取完成之后显示全部节点
            FocusNodes(mCtrlNodeList.ToArray());
        }

        double mAnimTagLeft = 0, mAnimTagTop = 0, mAnimTagWidth = 1;
        // 定位到特定节点
        public override void FocusNode(Base.BaseNodeControl nodeControl)
        {
            if (nodeControl == null)
                return;

            var targetPos = RectCanvas.TranslatePoint(new Point(RectCanvas.ActualWidth * 0.5, RectCanvas.ActualHeight * 0.5), ViewBoxMain);
            var currentPos = nodeControl.TranslatePoint(new Point(nodeControl.GetWidth() * 0.5, nodeControl.GetHeight() * 0.5), ViewBoxMain);
            var left = Canvas.GetLeft(ViewBoxMain);
            var top = Canvas.GetTop(ViewBoxMain);
            mAnimTagLeft = left + targetPos.X - currentPos.X;
            mAnimTagTop = top + targetPos.Y - currentPos.Y;

            var transAnim = new DoubleAnimation();
            transAnim.To = mAnimTagLeft;
            transAnim.Duration = TimeSpan.FromSeconds(0.15);
            transAnim.AccelerationRatio = 0.3;
            transAnim.DecelerationRatio = 0.3;
            transAnim.Completed += TransAnimX_Completed;
            ViewBoxMain.BeginAnimation(Canvas.LeftProperty, transAnim);

            transAnim = new DoubleAnimation();
            transAnim.To = mAnimTagTop;
            transAnim.Duration = TimeSpan.FromSeconds(0.15);
            transAnim.AccelerationRatio = 0.3;
            transAnim.DecelerationRatio = 0.3;
            transAnim.Completed += TransAnimY_Completed;
            ViewBoxMain.BeginAnimation(Canvas.TopProperty, transAnim);
        }

        public override void FocusNodes(Base.BaseNodeControl[] nodeControls)
        {
            if (nodeControls == null || nodeControls.Length <= 0)
                return;

            // 当前结点的包围大小（MainDrawCanvas上的坐标）
            var curLeft = double.MaxValue;
            var curTop = double.MaxValue;
            var curRight = double.MinValue;
            var curBottom = double.MinValue;

            foreach (var node in nodeControls)
            {
                var loc = node.GetLocation();
                if (curLeft > loc.X)
                    curLeft = loc.X;
                if (curTop > loc.Y)
                    curTop = loc.Y;
                var right = loc.X + node.GetWidth();
                if (curRight < right)
                    curRight = right;
                var bottom = loc.Y + node.GetWidth();
                if (curBottom < bottom)
                    curBottom = bottom;
            }
            curLeft -= 50;
            curTop -= 50;
            curRight += 50;
            curBottom += 50;
            var tl = MainDrawCanvas.TranslatePoint(new Point(curLeft, curTop), ViewBoxMain);
            var rb = MainDrawCanvas.TranslatePoint(new Point(curRight, curBottom), ViewBoxMain);
            var tagVBMPt = new Point(tl.X + (rb.X - tl.X) * 0.5, tl.Y + (rb.Y - tl.Y) * 0.5);

            var viewStart = RectCanvas.TranslatePoint(new Point(0, 0), ViewBoxMain);
            var viewEnd = RectCanvas.TranslatePoint(new Point(RectCanvas.ActualWidth, RectCanvas.ActualHeight), ViewBoxMain);
            var curVBMPt = new Point(viewStart.X + (viewEnd.X - viewStart.X) * 0.5, viewStart.Y + (viewEnd.Y - viewStart.Y) * 0.5);

            //var scaleDeltaX = (viewEnd.X - viewStart.X) / (curRight - curLeft);
            //var scaleDeltaY = (viewEnd.Y - viewStart.Y) / (curBottom - curTop);
            var scaleDeltaX = (viewEnd.X - viewStart.X) / (rb.X - tl.X);
            var scaleDeltaY = (viewEnd.Y - viewStart.Y) / (rb.Y - tl.Y);
            var scaleDelta = Math.Min(scaleDeltaX, scaleDeltaY);
            var tagWidth = scaleDelta * ViewBoxMain.ActualWidth;
            var calScaleDelta = tagWidth / MainDrawCanvas.Width * 100;
            if (calScaleDelta < mScaleDeltaMin)
            {
                scaleDelta = mScaleDeltaMin / 100 * MainDrawCanvas.Width / ViewBoxMain.ActualWidth;
            }
            else if (calScaleDelta > mScaleDeltaMax)
            {
                scaleDelta = mScaleDeltaMax / 100 * MainDrawCanvas.Width / ViewBoxMain.ActualWidth;
            }
            mAnimTagLeft = Canvas.GetLeft(ViewBoxMain) + (curVBMPt.X - tagVBMPt.X);
            mAnimTagTop = Canvas.GetTop(ViewBoxMain) + (curVBMPt.Y - tagVBMPt.Y);

            var deltaX = tagVBMPt.X / ViewBoxMain.ActualWidth;
            var deltaY = tagVBMPt.Y / ViewBoxMain.ActualHeight;
            mAnimTagLeft -= (ViewBoxMain.ActualWidth * scaleDelta - ViewBoxMain.ActualWidth) * deltaX;
            mAnimTagTop -= (ViewBoxMain.ActualHeight * scaleDelta - ViewBoxMain.ActualHeight) * deltaY;

            if (double.IsNaN(mAnimTagLeft) || double.IsInfinity(mAnimTagLeft))
                return;
            if (double.IsNaN(mAnimTagTop) || double.IsInfinity(mAnimTagTop))
                return;

            var transAnim = new DoubleAnimation();
            transAnim.To = mAnimTagLeft;
            transAnim.Duration = TimeSpan.FromSeconds(0.15);
            transAnim.AccelerationRatio = 0.3;
            transAnim.DecelerationRatio = 0.3;
            transAnim.Completed += TransAnimX_Completed;
            ViewBoxMain.BeginAnimation(Canvas.LeftProperty, transAnim);

            transAnim = new DoubleAnimation();
            transAnim.To = mAnimTagTop;
            transAnim.Duration = TimeSpan.FromSeconds(0.15);
            transAnim.AccelerationRatio = 0.3;
            transAnim.DecelerationRatio = 0.3;
            transAnim.Completed += TransAnimY_Completed;
            ViewBoxMain.BeginAnimation(Canvas.TopProperty, transAnim);

            ViewBoxMain.Width = ViewBoxMain.ActualWidth;
            UpdateScaleInfoShow();
            mAnimTagWidth = scaleDelta * ViewBoxMain.ActualWidth;

            if (double.IsNaN(mAnimTagWidth) || double.IsInfinity(mAnimTagWidth))
                return;

            //ViewBoxMain.Width = mAnimTagWidth;
            var anim = new DoubleAnimation();
            anim.To = mAnimTagWidth;
            anim.Duration = TimeSpan.FromSeconds(0.15);
            anim.AccelerationRatio = 0.3;
            anim.DecelerationRatio = 0.3;
            anim.Completed += ScaleAnim_Completed;
            ViewBoxMain.BeginAnimation(Viewbox.WidthProperty, anim);
        }

        private void ScaleAnim_Completed(object sender, EventArgs e)
        {
            ViewBoxMain.BeginAnimation(Viewbox.WidthProperty, null);
            ViewBoxMain.Width = mAnimTagWidth;
            UpdateScaleInfoShow();
        }

        private void TransAnimX_Completed(object sender, EventArgs e)
        {
            ViewBoxMain.BeginAnimation(Canvas.LeftProperty, null);
            Canvas.SetLeft(ViewBoxMain, mAnimTagLeft);
        }

        private void TransAnimY_Completed(object sender, EventArgs e)
        {
            ViewBoxMain.BeginAnimation(Canvas.TopProperty, null);
            Canvas.SetTop(ViewBoxMain, mAnimTagTop);
        }

        #region 菜单操作

        //private void Button_Paste_Click(object sender, RoutedEventArgs e)
        //{
        //    var tagPt = RectCanvas.TranslatePoint(new Point(RectCanvas.ActualWidth * 0.5, RectCanvas.ActualHeight * 0.5), MainDrawCanvas);
        //    Paste(tagPt);
        //}

        //private void Button_Delete_Click(object sender, RoutedEventArgs e)
        //{
        //    DeleteSelectedNodes();
        //}

        //private void Button_Focus_Click(object sender, RoutedEventArgs e)
        //{
        //    List<CodeGenerateSystem.Base.BaseNodeControl> selectedNodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();

        //    foreach (var node in mCtrlNodeList)
        //    {
        //        if (node.Selected)
        //        {
        //            selectedNodes.Add(node);
        //        }
        //    }
        //    FocusNodes(selectedNodes.ToArray());
        //}

        //private void Button_ShowAll_Click(object sender, RoutedEventArgs e)
        //{
        //    FocusNodes(mCtrlNodeList.ToArray());
        //}

        //private void Button_AddComment_Click(object sender, RoutedEventArgs e)
        //{
        //    // 添加注释
        //    double commentLeft = double.MaxValue;
        //    double commentTop = double.MaxValue;
        //    double commentRight = double.MinValue;
        //    double commentBottom = double.MinValue;
        //    foreach (var node in mCtrlNodeList)
        //    {
        //        if (node.Selected)
        //        {
        //            var loc = node.GetLocation();
        //            if (commentLeft > loc.X)
        //                commentLeft = loc.X;
        //            if (commentTop > loc.Y)
        //                commentTop = loc.Y;
        //            var right = loc.X + node.GetWidth(false);
        //            if (commentRight < right)
        //                commentRight = right;
        //            var bottom = loc.Y + node.GetHeight(false);
        //            if (commentBottom < bottom)
        //                commentBottom = bottom;
        //        }
        //    }

        //    commentLeft -= 50;
        //    commentTop -= 50;
        //    commentRight += 50;
        //    commentBottom += 50;

        //    var commentNode = AddNodeControl(typeof(CommentNode), "", commentLeft, commentTop);
        //    commentNode.Width = commentRight - commentLeft;
        //    commentNode.Height = commentBottom - commentTop;
        //}

        //public void Button_Pack_Click(object sender, RoutedEventArgs e)
        //{
        //    OnPackage?.Invoke();
        //}

        //public void Button_Unpack_Click(object sender, RoutedEventArgs e)
        //{
        //    OnUnpackage?.Invoke();
        //}

        //private void Button_Pack_Method_Click(object sender, RoutedEventArgs e)
        //{
        //    OnPackageMethod?.Invoke();
        //}

        //private void Button_CheckError_Click(object sender, RoutedEventArgs e)
        //{
        //    CheckError();
        //}
        #endregion

        public void ProcessKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Delete:
                    DeleteSelectedNodes();
                    break;
                case Key.C:
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        Copy();
                    }
                    break;
                case Key.V:
                    {
                        if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            var pos = ResourceLibrary.Win32.GetCursorPosInWPF();
                            var pt = this.PointFromScreen(new Point(pos.X, pos.Y));
                            Paste(pt);
                        }
                    }
                    break;
                case Key.Z:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.Undo(HostControl.UndoRedoKey);
                        }
                    }
                    break;
                case Key.Y:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            EditorCommon.UndoRedo.UndoRedoManager.Instance.Redo(HostControl.UndoRedoKey);
                        }
                    }
                    break;
            }
        }

        private void Btn_ShowAll_Click(object sender, RoutedEventArgs e)
        {
            FocusNodes(mCtrlNodeList.ToArray());
        }
        private void Btn_Capture2Img_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "jpeg files(*.jpg)|*.jpg";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == true)
            {
                var brush = new VisualBrush()
                {
                    Stretch = Stretch.Fill,
                    ViewboxUnits = BrushMappingMode.Absolute,
                };
                brush.Visual = MainDrawCanvas;
                //brush.RelativeTransform = new ScaleTransform(2, 2);

                double viewLeft = double.MaxValue;
                double viewTop = double.MaxValue;
                double viewRight = double.MinValue;
                double viewBottom = double.MinValue;
                if (CtrlNodeList.Count > 0)
                {
                    foreach (var node in CtrlNodeList)
                    {
                        var left = node.GetLeftInCanvas(true);
                        if (viewLeft > left)
                            viewLeft = left;
                        var top = node.GetTopInCanvas(true);
                        if (viewTop > top)
                            viewTop = top;
                        var right = left + node.GetWidth(true);
                        if (viewRight < right)
                            viewRight = right;
                        var bottom = top + node.GetHeight(true);
                        if (viewBottom < bottom)
                            viewBottom = bottom;
                    }
                }
                else
                {
                    viewLeft = 0;
                    viewTop = 0;
                    viewRight = MainDrawCanvas.Width;
                    viewBottom = MainDrawCanvas.Height;
                }

                var width = viewRight - viewLeft;
                var height = viewBottom - viewTop;
                if (width < MainDrawCanvas.Width)
                    width = MainDrawCanvas.Width;
                if (height < MainDrawCanvas.Height)
                    height = MainDrawCanvas.Height;

                double delta = 30;
                var rect = new Rect(viewLeft - delta, viewTop - delta, width + delta * 2, height + delta * 2);
                var drawVisual = new DrawingVisual();

                //brush.Viewbox = new Rect(rect.Left, rect.Top, rect.Width, rect.Height);

                //using (var context = drawVisual.RenderOpen())
                //{
                //    context.DrawRectangle(brush, null, new Rect(0, 0, rect.Width, rect.Height));
                //    context.Close();
                //}

                //var rt = new RenderTargetBitmap((int)(rect.Width), (int)(rect.Height), 96, 96, PixelFormats.Default);
                //rt.Render(drawVisual);
                //var encoder = new JpegBitmapEncoder();
                //encoder.Frames.Add(BitmapFrame.Create(rt));
                //var file = System.IO.File.Create(dialog.FileName);
                //encoder.Save(file);
                //file.Close();

                var bmp = new WriteableBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Bgra32, null);

                var deltaSize = 512;
                int index = 0;
                for (double x = rect.Left; x < rect.Right; x += deltaSize)
                {
                    double tempWidth = deltaSize;
                    if ((x + deltaSize) > rect.Right)
                        tempWidth = rect.Right - x;
                    for (double y = rect.Top; y < rect.Bottom; y += deltaSize)
                    {
                        double tempHeight = deltaSize;
                        if ((y + deltaSize) > rect.Bottom)
                            tempHeight = rect.Bottom - y;

                        brush.Viewbox = new Rect(x, y, tempWidth, tempHeight);

                        using (var context = drawVisual.RenderOpen())
                        {
                            context.DrawRectangle(brush, null, new Rect(0, 0, tempWidth, tempHeight));
                            context.Close();
                        }

                        var pixelWidth = (int)(tempWidth);
                        var pixelHeight = (int)(tempHeight);
                        var stride = 4 * pixelWidth;
                        var rtt = new RenderTargetBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32);
                        rtt.Render(drawVisual);

                        var buffer = new Byte[stride * pixelHeight];
                        rtt.CopyPixels(buffer, stride, 0);
                        bmp.WritePixels(new Int32Rect((int)(x - rect.Left), (int)(y - rect.Top), pixelWidth, pixelHeight),
                                        buffer, stride, 0);
                        ;

                        index++;
                    }
                }

                var encoderT = new JpegBitmapEncoder();
                encoderT.Frames.Add(BitmapFrame.Create(bmp));
                var fileT = System.IO.File.Create(dialog.FileName + "_.jpg");
                encoderT.Save(fileT);
                fileT.Close();


            }

        }

        public class ContextMenuFilterData
        {
            public CodeGenerateSystem.Base.LinkPinControl StartLinkObj;
            public EngineNS.ECSType CSType = EngineNS.ECSType.Common;
            public NodesContainerControl HostContainerControl;

            public override bool Equals(object obj)
            {
                var data = obj as ContextMenuFilterData;
                if (data == null)
                    return false;
                return (CSType == data.CSType) && (data.StartLinkObj.LinkType == StartLinkObj.LinkType) && (data.StartLinkObj.ClassType == StartLinkObj.ClassType);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public delegate void Delegate_FilterContextMenu(NodeListContextMenu nodeList, ContextMenuFilterData filterData);
        public Delegate_FilterContextMenu OnFilterContextMenu;
        Point mContextMenuOpenMousePos;
        protected override void FilterContextMenu(LinkPinControl startLinkObj)
        {
            mContextMenuOpenMousePos = Mouse.GetPosition(MainDrawCanvas);
            ContextMenuNodeList.NodesList.FilterString = "";
            if (startLinkObj != null)
            {
                var data = new ContextMenuFilterData()
                {
                    StartLinkObj = startLinkObj,
                    CSType = CSType,
                    HostContainerControl = this,
                };
                OnFilterContextMenu?.Invoke(ContextMenuNodeList, data);
            }
            else
            {
                var data = new ContextMenuFilterData()
                {
                    CSType = CSType,
                    HostContainerControl = this,
                };
                OnFilterContextMenu?.Invoke(ContextMenuNodeList, data);
            }
        }

        public Point GetViewCenter()
        {
            return TranslatePoint(new Point(this.ActualWidth * 0.5, this.ActualHeight * 0.5), MainDrawCanvas);
        }
    }
}
