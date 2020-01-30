using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CodeGenerateSystem.Base
{
    public class SubNodesContainerData
    {
        public Guid ID;
        public string Title;
        public bool IsCreated;  // out value
    }
    public interface INodesContainerHost
    {
        string UndoRedoKey { get; }
        string LinkedCategoryItemName { get; }
        Guid LinkedCategoryItemID { get; }
        string GetGraphFileName(string graphName);
        Dictionary<Guid, CodeGenerateSystem.Controls.NodesContainerControl> SubNodesContainers
        {
            get;
        }
        //CodeGenerateSystem.Controls.NodesContainerControl NodesControl { get; }
        //IMacrossOperationContainer HostControl { get; }
        Task<CodeGenerateSystem.Controls.NodesContainerControl> ShowSubNodesContainer(SubNodesContainerData data);
        Task<CodeGenerateSystem.Controls.NodesContainerControl> GetSubNodesContainer(SubNodesContainerData data);
        void NodesControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData);
        void InitializeSubLinkedNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl nodesControl);
        Task InitializeNodesContainer(CodeGenerateSystem.Controls.NodesContainerControl nodesControl);
    }

    // 节点容器的基类
    public partial class NodesContainer : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public delegate void Delegate_Link(NodesContainer sender, LinkPinControl startLinkObj, LinkPinControl endLinkObj);
        public event Delegate_Link OnLinkSuccess;
        public event Delegate_Link OnLinkFail;
        public delegate void Delegate_SelectedLinkInfo(LinkInfo linkInfo);
        public event Delegate_SelectedLinkInfo OnSelectLinkInfo;
        public event Delegate_SelectedLinkInfo OnLinkInfoDoubleClick;
        public event Delegate_OnOperateNodeControl OnSelectNodeControl;
        public delegate void Delegate_OnUnSelectNodes(List<CodeGenerateSystem.Base.BaseNodeControl> nodes);
        public event Delegate_OnUnSelectNodes OnUnSelectNodes;
        public event Delegate_OnOperateNodeControl OnSelectNull;
        public delegate void Delegate_MoveNode(NodesContainer sender, CodeGenerateSystem.Base.BaseNodeControl node);
        public event Delegate_MoveNode OnMoveNode;
        public delegate bool Delegate_OnPreSaved();
        public Delegate_OnPreSaved OnPreSaved;

        public delegate void Delegate_OnNew();
        public Delegate_OnNew OnNew;

        public delegate CodeGenerateSystem.Base.BaseNodeControl Delegate_OnPackage();
        public Delegate_OnPackage OnPackage;
        public Delegate_OnPackage OnPackageMethod;

        public delegate void Delegate_OnUnPackage();
        public Delegate_OnUnPackage OnUnpackage;

        protected Point mMouseLeftButtonDownPt;
        protected Point mMouseRightButtonDownPt;
        public Point MouseRightButtonDownPt
        {
            get { return mMouseRightButtonDownPt; }
        }
        protected Point mMouseMidButtonDownPt;

        Canvas m_ContainerDrawCanvas;
        public Canvas ContainerDrawCanvas     // 绘制节点的Canvas
        {
            get { return m_ContainerDrawCanvas; }
            set
            {
                m_ContainerDrawCanvas = value;

                //if (!m_bMouseCurveInitialized)
                //    InitialieMouseCurve();

                //m_ContainerDrawCanvas.Children.Add(PreviewBezierPath);
            }
        }

        bool mIsOpenContextMenu = false;
        public bool IsOpenContextMenu
        {
            get => mIsOpenContextMenu;
            set
            {
                mIsOpenContextMenu = value;
                if (mIsOpenContextMenu)
                {
                    FilterContextMenu(mStartLinkObj);
                }
                else
                {
                    if (m_PreviewLinkCurve != null)
                        m_PreviewLinkCurve.Visibility = Visibility.Hidden;
                    m_enPreviewBezierType = CodeGenerateSystem.Base.enBezierType.None;
                }
                OnPropertyChanged("IsOpenContextMenu");
            }
        }

        public string TitleString
        {
            get { return (string)GetValue(TitleStringProperty); }
            set { SetValue(TitleStringProperty, value); }
        }
        public static readonly DependencyProperty TitleStringProperty = DependencyProperty.Register("TitleString", typeof(string), typeof(NodesContainer), new UIPropertyMetadata(""));
        public Visibility TitleShow
        {
            get { return (Visibility)GetValue(TitleShowProperty); }
            set { SetValue(TitleShowProperty, value); }
        }
        public static readonly DependencyProperty TitleShowProperty = DependencyProperty.Register("TitleShow", typeof(Visibility), typeof(NodesContainer), new UIPropertyMetadata(Visibility.Visible));
        /// <summary>
        /// 上级容器
        /// </summary>
        public INodesContainerHost HostControl { get; set; }
        /// <summary>
        /// 上级节点
        /// </summary>
        public BaseNodeControl HostNode { get; set; }

        public NodesContainer()
        {
            EndPreviewLineFunc = EndPreviewLine;
        }
        public Canvas GetDrawCanvas()
        {
            return m_ContainerDrawCanvas;
        }

        //public Guid HostID = Guid.Empty;
        public object ExtendData;
        #region 链接线
        protected LinkInfo mSelectedLinkInfo;
        public LinkInfo SelectedLinkInfo
        {
            get
            {
                return mSelectedLinkInfo;
            }
            set
            {
                mSelectedLinkInfo = value;
                OnSelectLinkInfo?.Invoke(value);
            }
        }
        public void SetSelectedLinkInfo(LinkInfo linkInfo)
        {
            mSelectedLinkInfo = linkInfo;
        }
        public void LinkInfoDoubleClick(LinkInfo linkInfo)
        {
            OnLinkInfoDoubleClick?.Invoke(linkInfo);
        }
        #endregion
        #region 链接点操作

        // 选中的链接点（一次只能选中一个链接点）
        public LinkPinControl mSelectedLinkControl;
        public LinkPinControl SelectedLinkControl
        {
            get { return mSelectedLinkControl; }
            set
            {
                if (mSelectedLinkControl != null)
                    mSelectedLinkControl.Selected = false;

                mSelectedLinkControl = value;
                if (mSelectedLinkControl != null && !mSelectedLinkControl.Selected)
                    mSelectedLinkControl.Selected = true;

                UnselectNodes(mCtrlNodeList);
                //OnLinkControlSelected?.Invoke(mSelectedLinkControl);
            }
        }

        //public delegate void Delegate_OnLinkControlSelected(LinkControl linkCtrl);
        //public event Delegate_OnLinkControlSelected OnLinkControlSelected;


        #endregion

        #region 节点操作

        // 节点连接失败时的处理
        public virtual void OnLinkFailure(LinkPinControl startLinkObj, LinkPinControl endLinkObj)
        {
           
        }
        partial void SetConstructionParams_WPF(ConstructionParams csParam)
        {
            csParam.DrawCanvas = this.ContainerDrawCanvas;
        }
        partial void AddNodeControl_WPF(BaseNodeControl ins, bool addToCanvas, double x, double y)
        {
            ins.ParentDrawCanvas = ContainerDrawCanvas;

            ins.OnDirtyChange = new BaseNodeControl.Delegate_DirtyChanged(OnControlDirtyChanged);
            ins.OnSelectNode += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_SelectedNode(OnSelectNode);
            ins.OnStartLink += new CodeGenerateSystem.Base.Delegate_StartLink(StartPreviewCurve);
            ins.OnEndLink += new CodeGenerateSystem.Base.Delegate_EndLink(EndPreviewCurve);
            ins.OnStartDragMoveNode += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_StartDragMoveNode(OnStartDragMoveNode);
            ins.OnDragMoveNode += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_DragMoveNode(OnDragMoveNode);

            ins.CollectConstructionErrors();
            var redoAction = new Action<object>((obj) =>
            {
                if (addToCanvas)
                {
                    if (ContainerDrawCanvas != null)
                        ContainerDrawCanvas.Children.Add(ins);
                }
                if (!double.IsNaN(x) && !double.IsNaN(y))
                    ins.SetLocation(x, y);
            });
            redoAction.Invoke(null);

            if (HostControl != null)
            {
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, ins,
                                                                (obj) =>
                                                                {
                                                                    var ctrl = ins as BaseNodeControl;
                                                                    DeleteNode(ctrl, false);
                                                                }, $"Add Node {ins.NodeName}");
            }
        }

        // 各实例类在添加节点时的特殊操作
        protected virtual void AddControlNode_SpecialOperation(BaseNodeControl nodeIns) { }

        // 节点选中时进行的操作
        protected virtual void OnSelectNode(CodeGenerateSystem.Base.BaseNodeControl node, bool bSelected, bool unselectedOther)
        {
            if (unselectedOther && (System.Windows.Input.Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != KeyStates.Down)
            {
                // 取消选中链接点
                SelectedLinkControl = null;
            }

            OnSelectNodeControl?.Invoke(node);
        }

        // 取消选择指定节点
        protected virtual void UnselectNodes(List<CodeGenerateSystem.Base.BaseNodeControl> nodes)
        {
            foreach (var tempNode in nodes)
            {
                tempNode.Selected = false;
            }

            OnUnSelectNodes?.Invoke(nodes);
        }

        //public void DeleteNodeFromNodeContainer(CodeGenerateSystem.Base.BaseNodeControl node)
        //{
        //    OnDeleteNode(node);
        //}

        protected virtual void OnStartDragMoveNode(CodeGenerateSystem.Base.BaseNodeControl node, MouseButtonEventArgs e)
        {
            foreach (var vNode in mCtrlNodeList)
            {
                if (vNode.Selected && vNode != node && vNode.ParentNode == null)
                {
                    vNode.CalculateDeltaPt(e);
                }
            }
        }

        protected virtual void OnDragMoveNode(CodeGenerateSystem.Base.BaseNodeControl node, MouseEventArgs e)
        {
            if (ContainerDrawCanvas == null)
                return;

            foreach (var vNode in mCtrlNodeList)
            {
                if (vNode.Selected && vNode != node && vNode.ParentNode == null)
                {
                    var pt = e.GetPosition(ContainerDrawCanvas);
                    vNode.MoveWithPt(pt);
                }
            }
            OnMoveNode?.Invoke(this, node);
        }

        class UndoRedoData
        {
            public LinkPinControl StartObj;
            public LinkPinControl EndObj;
        }
        // 删除选中的节点
        public virtual void DeleteSelectedNodes()
        {
            List<CodeGenerateSystem.Base.BaseNodeControl> selectedNodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();

            foreach (var node in mCtrlNodeList)
            {
                if (node.Selected && node.CheckCanDelete())
                {
                    selectedNodes.Add(node);
                }
            }
            var undoRedoDatas = new List<UndoRedoData>();
            foreach (var node in selectedNodes)
            {
                foreach (var lPin in node.GetLinkPinInfos())
                {
                    for (int i = 0; i < lPin.GetLinkInfosCount(); i++)
                    {
                        var lInfo = lPin.GetLinkInfo(i);
                        var data = new UndoRedoData();
                        data.StartObj = lInfo.m_linkFromObjectInfo;
                        data.EndObj = lInfo.m_linkToObjectInfo;
                        undoRedoDatas.Add(data);
                    }
                }
            }
            var redoAction = new Action<object>((obj) =>
            {
                foreach (var node in selectedNodes)
                {
                    node.Clear();
                    mCtrlNodeList.Remove(node);
                    OnDeletedNodeControl?.Invoke(node);
                }
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                                    (obj) =>
                                    {
                                        foreach (var node in selectedNodes)
                                        {
                                            ContainerDrawCanvas.Children.Add(node);
                                            CtrlNodeList.Add(node);
                                        }
                                        foreach (var data in undoRedoDatas)
                                        {
                                            var linkInfo = new LinkInfo(ContainerDrawCanvas, data.StartObj, data.EndObj);
                                        }
                                    }, "Delete Select Nodes");

            IsDirty = true;
        }
        protected virtual void SelectNull()
        {
            OnSelectNull?.Invoke(null);
        }
        protected virtual bool SelectNodes(double Left, double Right, double Top, double Bottom)
        {
            bool selectNodes = false;
            foreach (var node in mCtrlNodeList)
            {
                var loc = node.GetLocation();
                var nRight = loc.X + node.GetWidth();
                var nBottom = loc.Y + node.GetHeight();

                if (Left < nRight &&
                    loc.X < Right &&
                    Top < nBottom &&
                    loc.Y < Bottom)
                {
                    node.Selected = true;
                    OnSelectNodeControl?.Invoke(node);
                    selectNodes = true;
                }
            }
            return selectNodes;
        }

        partial void CheckNodesError()
        {
            bool bRetValue = true;
            foreach (var node in mCtrlNodeList)
            {
                bool value = node.CheckError();
                bRetValue = bRetValue && value;
            }

            mCheckErrorResult = bRetValue;
        }

        // 节点居中显示
        public virtual void CenterNode(BaseNodeControl node)
        {

        }

        // 放大节点
        public virtual void ZoonNode(BaseNodeControl node)
        {

        }

        #endregion
        #region Duplicate
        protected virtual NodesContainer CreateContainer()
        {
            return new NodesContainer();
        }
        public virtual NodesContainer Duplicate()
        {
            List<BaseNodeControl> copyedNodes = new List<BaseNodeControl>();
            var container = CreateContainer();
            container.TitleString = TitleString;
            container.TitleShow = Visibility.Hidden;
            Dictionary<Guid, Guid> oldNewGuidDic = new Dictionary<Guid, Guid>();
            BaseNodeControl.DuplicateParam param = new BaseNodeControl.DuplicateParam();
            param.TargetNodesContainer = this;
            for (int i = 0; i < mCtrlNodeList.Count; i++)
            {
                var node = mCtrlNodeList[i];
                var copyedNode = node.Duplicate(param);
                copyedNode.CollectPinIds(node, oldNewGuidDic);
                var loc = node.GetLocation();
                var tempX = loc.X;
                var tempY = loc.Y;
                container.AddNodeControl(copyedNode,tempX,tempY);
                copyedNodes.Add(copyedNode);
            }
            // 连线
            for (int i = 0; i < mCtrlNodeList.Count; i++)
            {
                var node = mCtrlNodeList[i];
                var newNode = copyedNodes[i];
                newNode.CopyLink(node, copyedNodes, oldNewGuidDic, container.ContainerDrawCanvas);
            }
            return container;
        }
        #endregion Duplicate
        #region 复制粘贴

        class CopyPasteData : EditorCommon.ICopyPasteData
        {
            public List<CodeGenerateSystem.Base.BaseNodeControl> mCopyedNodes = new List<CodeGenerateSystem.Base.BaseNodeControl>();
            public Rect mCopySrcRect = new Rect(Double.MaxValue, Double.MaxValue, 1, 1);

            public CopyPasteData Duplicate()
            {
                var retVal = new CopyPasteData();
                retVal.mCopyedNodes = new List<BaseNodeControl>(mCopyedNodes);
                retVal.mCopySrcRect = mCopySrcRect;
                return retVal;
            }
        }

        public virtual void Copy(string keyName = "GraphLinks")
        {
            var cpData = new CopyPasteData();
            EditorCommon.Program.SetCopyPasteData(keyName, cpData);
            List<BaseNodeControl> copyedNodes = new List<BaseNodeControl>(20);
            BaseNodeControl.DuplicateParam param = new BaseNodeControl.DuplicateParam();
            param.TargetNodesContainer = this;
            foreach (var node in mCtrlNodeList)
            {
                if (node.Selected && node.CanDuplicate())
                {
                    var nodeLoc = node.GetLocation();
                    cpData.mCopySrcRect.X = (cpData.mCopySrcRect.X < nodeLoc.X) ? cpData.mCopySrcRect.X : nodeLoc.X;
                    cpData.mCopySrcRect.Y = (cpData.mCopySrcRect.Y < nodeLoc.Y) ? cpData.mCopySrcRect.Y : nodeLoc.Y;
                    var right = nodeLoc.X + node.GetWidth();
                    var bottom = nodeLoc.Y + node.GetHeight();
                    if (cpData.mCopySrcRect.X + cpData.mCopySrcRect.Width < right)
                    {
                        cpData.mCopySrcRect.Width = right - cpData.mCopySrcRect.X;
                    }
                    if (cpData.mCopySrcRect.Y + cpData.mCopySrcRect.Height < bottom)
                    {
                        cpData.mCopySrcRect.Height = bottom - cpData.mCopySrcRect.Y;
                    }
                    copyedNodes.Add(node);
                }
            }
            // 复制一遍节点，防止节点更改后再粘贴与复制时不一样
            Dictionary<Guid, Guid> oldNewGuidDic = new Dictionary<Guid, Guid>();
            foreach(var node in copyedNodes)
            {
                var copyedNode = node.Duplicate(param);
                copyedNode.CollectPinIds(node, oldNewGuidDic);
                var loc = node.GetLocation();
                copyedNode.SetLocation(loc.X, loc.Y);
                cpData.mCopyedNodes.Add(copyedNode);
            }
            // 连线
            for(int i=0; i<copyedNodes.Count; i++)
            {
                var node = copyedNodes[i];
                var newNode = cpData.mCopyedNodes[i];
                newNode.CopyLink(node, cpData.mCopyedNodes, oldNewGuidDic, null);
            }
        }

        public virtual void Paste(Point targetCenterPointInCanvas, string keyName = "GraphLinks")
        {
            var srcCpData = EditorCommon.Program.GetCopyPasteData(keyName) as CopyPasteData;
            if (srcCpData == null)
                return;
            BaseNodeControl.DuplicateParam param = new BaseNodeControl.DuplicateParam();
            param.TargetNodesContainer = this;
            var tempCPData = srcCpData.Duplicate();
            List<BaseNodeControl> copyedNodes = new List<BaseNodeControl>(tempCPData.mCopyedNodes.Count);
            var redoAction = new Action<Object>((obj) =>
            {
                var cpData = obj as CopyPasteData;
                UnselectNodes(mCtrlNodeList);

                Double deltaX = targetCenterPointInCanvas.X - (cpData.mCopySrcRect.X + cpData.mCopySrcRect.Width * 0.5);
                Double deltaY = targetCenterPointInCanvas.Y - (cpData.mCopySrcRect.Y + cpData.mCopySrcRect.Height * 0.5);

                List<Guid> includeIdList = new List<Guid>(cpData.mCopyedNodes.Count);
                Dictionary<Guid, Guid> oldNewGuidDic = new Dictionary<Guid, Guid>();
                // 复制节点
                for (int i = 0; i < cpData.mCopyedNodes.Count; i++)
                {
                    var node = cpData.mCopyedNodes[i];
                    var copyedNode = node.Duplicate(param);
                    copyedNode.HostNodesContainer = this;
                    copyedNode.CollectPinIds(node, oldNewGuidDic);

                    copyedNode.OnDirtyChange = new BaseNodeControl.Delegate_DirtyChanged(OnControlDirtyChanged);
                    copyedNode.ParentDrawCanvas = ContainerDrawCanvas;
                    copyedNode.OnSelectNode += new BaseNodeControl.Delegate_SelectedNode(OnSelectNode);
                    copyedNode.OnStartLink += new Delegate_StartLink(StartPreviewCurve);
                    copyedNode.OnEndLink += new Delegate_EndLink(EndPreviewCurve);
                    copyedNode.OnStartDragMoveNode += new BaseNodeControl.Delegate_StartDragMoveNode(OnStartDragMoveNode);
                    copyedNode.OnDragMoveNode += new BaseNodeControl.Delegate_DragMoveNode(OnDragMoveNode);
                
                    ContainerDrawCanvas?.Children.Add(copyedNode);

                    var loc = node.GetLocation();
                    var tempX = loc.X + deltaX;
                    var tempY = loc.Y + deltaY;
                    copyedNode.SetLocation(tempX, tempY);
                    OnInitializeNodeControl?.Invoke(copyedNode);
                    mCtrlNodeList.Add(copyedNode);
                    ContainLinkNodes = mCtrlNodeList.Count != OrigionNodeControls.Count;
                    RefreshNodeProperty(copyedNode, ENodeHandleType.AddNodeControl);
                    OnAddedNodeControl?.Invoke(copyedNode);
                    IsDirty = true;

                    ScaleChange -= copyedNode.ScaleTips;
                    ScaleChange += copyedNode.ScaleTips;

                    copyedNodes.Add(copyedNode);
                }
                // 连线
                for(int i=0; i<cpData.mCopyedNodes.Count; i++)
                {
                    var node = cpData.mCopyedNodes[i];
                    var newNode = copyedNodes[i];
                    newNode.CopyLink(node, copyedNodes, oldNewGuidDic, ContainerDrawCanvas);
                }
                for(int i=0; i<copyedNodes.Count; i++)
                {
                    copyedNodes[i].Selected = true;
                }
            });
            redoAction.Invoke(tempCPData);

            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, tempCPData, redoAction, copyedNodes, 
                            (obj)=>
                            {
                                var nodeList = obj as List<BaseNodeControl>;
                                foreach(var node in nodeList)
                                {
                                    DeleteNode(node, false);
                                }
                            }, "Copy nodes");
        }

        #endregion

        #region 贝塞尔绘制

        protected CodeGenerateSystem.Base.enBezierType m_enPreviewBezierType = CodeGenerateSystem.Base.enBezierType.None;
        public CodeGenerateSystem.Base.enBezierType enPreviewBezierType
        {
            get => m_enPreviewBezierType;
            set => m_enPreviewBezierType = value;
        }
        protected EditorCommon.Controls.Curves.CurveBase m_PreviewLinkCurve;
        public EditorCommon.Controls.Curves.CurveBase PreviewLinkCurve
        {
            get => m_PreviewLinkCurve;
            set => m_PreviewLinkCurve = value;
        }
        protected CodeGenerateSystem.Base.LinkPinControl mStartLinkObj;
        public CodeGenerateSystem.Base.LinkPinControl StartLinkObj
        {
            get => mStartLinkObj;
        }
        bool m_bMouseCurveInitialized = false;
        public void StartPreviewCurve(CodeGenerateSystem.Base.LinkPinControl objInfo)
        {
            if (objInfo.LinkCurveType == enLinkCurveType.Bezier)
            {
                StartPreviewBezier(objInfo);
            }
            else if (objInfo.LinkCurveType == enLinkCurveType.Line)
            {
                StartPreviewLine(objInfo);
            }
            else if (objInfo.LinkCurveType == enLinkCurveType.BrokenLine)
            {
                StartPreviewBrokenLine(objInfo);
            }
        }
        protected void UpdatePreviewCurve(Point tagPt)
        {
            if (mStartLinkObj == null)
                return;
            if (mStartLinkObj.LinkCurveType == enLinkCurveType.Bezier)
            {
                UpdatePreviewBezier(tagPt);
            }
            else if (mStartLinkObj.LinkCurveType == enLinkCurveType.Line)
            {
                UpdatePreviewLine(tagPt);
            }
            else if (mStartLinkObj.LinkCurveType == enLinkCurveType.BrokenLine)
            {
                UpdatePreviewBrokenLine(tagPt);
            }
        }
        public Action<LinkPinControl, NodesContainer> EndPreviewLineFunc { get; set; } = null;
        public void EndPreviewCurve(CodeGenerateSystem.Base.LinkPinControl objInfo)
        {
            if (mStartLinkObj == null)
                return;
            if (mStartLinkObj.LinkCurveType == enLinkCurveType.Bezier)
            {
                EndPreviewBezier(objInfo);
            }
            else if (mStartLinkObj.LinkCurveType == enLinkCurveType.Line)
            {
                EndPreviewLineFunc.Invoke(objInfo, this);
            }
            else if (mStartLinkObj.LinkCurveType == enLinkCurveType.BrokenLine)
            {
                EndPreviewBrokenLine(objInfo, this);
            }
        }
        #region Bezier
        // 初始化鼠标推拽的贝塞尔曲线
        protected void InitialMouseBezier()
        {
            m_PreviewLinkCurve = new EditorCommon.Controls.Curves.BezierCurve();
            m_PreviewLinkCurve.Visibility = Visibility.Hidden;
            m_PreviewLinkCurve.Stroke = Brushes.White;
            m_PreviewLinkCurve.StrokeThickness = 2;
            m_PreviewLinkCurve.IsHitTestVisible = false;
            m_ContainerDrawCanvas.Children.Add(m_PreviewLinkCurve);
            m_bMouseCurveInitialized = true;
        }
        // 开始绘制预览的贝塞尔曲线
        public void StartPreviewBezier(CodeGenerateSystem.Base.LinkPinControl objInfo)
        {
            if (ContainerDrawCanvas == null)
                return;
            if (!m_bMouseCurveInitialized)
                InitialMouseBezier();

            var bezierCurve = m_PreviewLinkCurve as EditorCommon.Controls.Curves.BezierCurve;
            Point startPt = objInfo.TranslatePoint(objInfo.LinkElementOffset, ContainerDrawCanvas);
            bezierCurve.StartPoint = startPt;
            bezierCurve.ControlPoint1 = startPt;
            bezierCurve.ControlPoint2 = startPt;
            bezierCurve.EndPoint = startPt;
            m_PreviewLinkCurve.Visibility = Visibility.Visible;
            m_enPreviewBezierType = objInfo.BezierType;

            mStartLinkObj = objInfo;
        }
        protected void UpdatePreviewBezier(Point tagPt)
        {
            var bezierCurve = m_PreviewLinkCurve as EditorCommon.Controls.Curves.BezierCurve;
            bezierCurve.EndPoint = tagPt;

            double delta = Math.Max(Math.Abs(tagPt.X - m_PreviewLinkCurve.StartPoint.X) / 2, 25);
            delta = Math.Min(150, delta);

            switch (m_enPreviewBezierType)
            {
                case CodeGenerateSystem.Base.enBezierType.Left:
                    bezierCurve.ControlPoint1 = new Point(m_PreviewLinkCurve.StartPoint.X - delta, m_PreviewLinkCurve.StartPoint.Y);
                    bezierCurve.ControlPoint2 = new Point(tagPt.X + delta, tagPt.Y);
                    break;

                case CodeGenerateSystem.Base.enBezierType.Right:
                    bezierCurve.ControlPoint1 = new Point(m_PreviewLinkCurve.StartPoint.X + delta, m_PreviewLinkCurve.StartPoint.Y);
                    bezierCurve.ControlPoint2 = new Point(tagPt.X - delta, tagPt.Y);
                    break;

                case CodeGenerateSystem.Base.enBezierType.Top:
                    bezierCurve.ControlPoint1 = new Point(m_PreviewLinkCurve.StartPoint.X, m_PreviewLinkCurve.StartPoint.Y - delta);
                    bezierCurve.ControlPoint2 = new Point(tagPt.X, tagPt.Y + delta);
                    break;

                case CodeGenerateSystem.Base.enBezierType.Bottom:
                    bezierCurve.ControlPoint1 = new Point(m_PreviewLinkCurve.StartPoint.X, m_PreviewLinkCurve.StartPoint.Y + delta);
                    bezierCurve.ControlPoint2 = new Point(tagPt.X, tagPt.Y - delta);
                    break;
            }
        }
        public void EndPreviewBezier(CodeGenerateSystem.Base.LinkPinControl objInfo)
        {
            if (ContainerDrawCanvas == null)
                return;

            if (objInfo == null)
            {
                if (mStartLinkObj != null && m_PreviewLinkCurve.Visibility == Visibility.Visible)
                {
                    IsOpenContextMenu = true;
                }
            }
            else if (mStartLinkObj != null && objInfo != null)
            {
                m_PreviewLinkCurve.Visibility = Visibility.Hidden;
                m_enPreviewBezierType = CodeGenerateSystem.Base.enBezierType.None;
                if (mStartLinkObj.LinkOpType == objInfo.LinkOpType) // 只有start和end能连
                    return;
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(mStartLinkObj, objInfo))
                {
                    objInfo.MouseAssistVisible = Visibility.Hidden;

                    var container = new LinkInfoContainer();
                    if (mStartLinkObj.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = mStartLinkObj;
                        container.End = objInfo;
                    }
                    else
                    {
                        container.Start = objInfo;
                        container.End = mStartLinkObj;
                    }

                    var redoAction = new Action<Object>((obj) =>
                    {
                        var linkInfo = new CodeGenerateSystem.Base.LinkInfo(ContainerDrawCanvas, container.Start, container.End);
                    });
                    redoAction.Invoke(null);
                    EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                                                (obj) =>
                                                {
                                                    for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                    {
                                                        var info = container.End.GetLinkInfo(i);
                                                        if (info.m_linkFromObjectInfo == container.Start)
                                                        {
                                                            info.Clear();
                                                            break;
                                                        }
                                                    }
                                                }, "Create Link");
                    IsDirty = true;
                }
                else
                    OnLinkFailure(mStartLinkObj, objInfo);
            }
        }
        #endregion
        #region Line
        protected void InitialMouseLine()
        {
            m_PreviewLinkCurve = new EditorCommon.Controls.Curves.LineWithText();
            m_PreviewLinkCurve.Visibility = Visibility.Hidden;
            m_PreviewLinkCurve.Stroke = Brushes.White;
            m_PreviewLinkCurve.StrokeThickness = 2;
            m_PreviewLinkCurve.IsHitTestVisible = false;
            m_ContainerDrawCanvas.Children.Add(m_PreviewLinkCurve);
            m_bMouseCurveInitialized = true;
        }
        public void StartPreviewLine(CodeGenerateSystem.Base.LinkPinControl objInfo)
        {
            if (ContainerDrawCanvas == null)
                return;
            if (!m_bMouseCurveInitialized)
                InitialMouseLine();

            var lineCurve = m_PreviewLinkCurve as EditorCommon.Controls.Curves.LineWithText;
            Point startPt = objInfo.TranslatePoint(objInfo.LinkElementOffset, ContainerDrawCanvas);
            lineCurve.StartPoint = startPt;
            lineCurve.EndPoint = startPt;
            lineCurve.HalfArrows = EditorCommon.Controls.Curves.ArrowHalf.Both;
            lineCurve.ArrowEnds = EditorCommon.Controls.Curves.ArrowEnds.End;
            lineCurve.ShowText = false;
            lineCurve.TextColor = Brushes.White;
            lineCurve.TextAlignment = TextAlignment.Center;
            m_PreviewLinkCurve.Visibility = Visibility.Visible;
            m_enPreviewBezierType = objInfo.BezierType;

            mStartLinkObj = objInfo;
        }
        public class TransitionStaeBaseNodeForUndoRedo
        {
            public BaseNodeControl TransitionStateNode = null;
        }
        protected void UpdatePreviewLine(Point tagPt)
        {
            var bezierCurve = m_PreviewLinkCurve as EditorCommon.Controls.Curves.LineWithText;

            var width = mStartLinkObj.ActualWidth;
            var height = mStartLinkObj.ActualHeight;
            var objLocPoint = ContainerDrawCanvas.TranslatePoint(tagPt, mStartLinkObj);
            var point = tagPt;
            if (objLocPoint.X < 0)
            {
                point.X = 0;
            }
            else if (objLocPoint.X > width)
            {
                point.X = width;
            }
            else
            {
                point.X = objLocPoint.X;
            }
            if (objLocPoint.Y < 0)
            {
                point.Y = 0;
            }
            else if (objLocPoint.Y > height)
            {
                point.Y = height;
            }
            else
            {
                point.Y = objLocPoint.Y;
            }
            bezierCurve.StartPoint = mStartLinkObj.TranslatePoint(point, ContainerDrawCanvas);
            bezierCurve.EndPoint = tagPt;
        }
        public virtual void EndPreviewLine(CodeGenerateSystem.Base.LinkPinControl objInfo, NodesContainer nodesContainer)
        {
            if (ContainerDrawCanvas == null)
                return;

            if (objInfo == null)
            {
                if (mStartLinkObj != null && m_PreviewLinkCurve.Visibility == Visibility.Visible)
                {
                    IsOpenContextMenu = true;
                }
            }
            else if (mStartLinkObj != null && objInfo != null)
            {
                m_PreviewLinkCurve.Visibility = Visibility.Hidden;
                m_enPreviewBezierType = CodeGenerateSystem.Base.enBezierType.None;

                if (mStartLinkObj.LinkOpType == objInfo.LinkOpType && objInfo.LinkOpType != enLinkOpType.Both) // 只有start和end能连 或者其中之一 和 Both
                    return;
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(mStartLinkObj, objInfo))
                {
                    objInfo.MouseAssistVisible = Visibility.Hidden;

                    var container = new LinkInfoContainer();
                    if (mStartLinkObj.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = mStartLinkObj;
                        container.End = objInfo;
                    }
                    else if (objInfo.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = objInfo;
                        container.End = mStartLinkObj;
                    }
                    else
                    {
                        container.Start = mStartLinkObj;
                        container.End = objInfo;
                    }

                    if (StartLinkObj.LinkCurveType == enLinkCurveType.Line)
                    {
                        var redoAction = new Action<Object>((obj) =>
                        {
                            var linkInfo = new CodeGenerateSystem.Base.ClickableLinkInfo(ContainerDrawCanvas, container.Start, container.End);
                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                                                    (obj) =>
                                                    {
                                                        for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                        {
                                                            var info = container.End.GetLinkInfo(i);
                                                            if (info.m_linkFromObjectInfo == container.Start)
                                                            {
                                                                info.Clear();
                                                                break;
                                                            }
                                                        }
                                                    }, "Create Link");
                    }
                    else
                    {
                        var redoAction = new Action<Object>((obj) =>
                        {
                            var linkInfo = new CodeGenerateSystem.Base.LinkInfo(ContainerDrawCanvas, container.Start, container.End);
                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                                                    (obj) =>
                                                    {
                                                        for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                        {
                                                            var info = container.End.GetLinkInfo(i);
                                                            if (info.m_linkFromObjectInfo == container.Start)
                                                            {
                                                                info.Clear();
                                                                break;
                                                            }
                                                        }
                                                    }, "Create Link");
                    }
                    IsDirty = true;
                }
                else
                {
                    OnLinkFailure(mStartLinkObj, objInfo);
                }
            }
        }
        #endregion
        #region BrokenLine
        protected void InitialMouseBrokenLine()
        {
            m_PreviewLinkCurve = new EditorCommon.Controls.Curves.BrokenLine();
            m_PreviewLinkCurve.Visibility = Visibility.Hidden;
            m_PreviewLinkCurve.Stroke = Brushes.White;
            m_PreviewLinkCurve.StrokeThickness = 2;
            m_PreviewLinkCurve.IsHitTestVisible = false;
            m_ContainerDrawCanvas.Children.Add(m_PreviewLinkCurve);
            m_bMouseCurveInitialized = true;
        }
        public void StartPreviewBrokenLine(CodeGenerateSystem.Base.LinkPinControl objInfo)
        {
            if (ContainerDrawCanvas == null)
                return;
            if (!m_bMouseCurveInitialized)
                InitialMouseBrokenLine();

            var lineCurve = m_PreviewLinkCurve as EditorCommon.Controls.Curves.BrokenLine;
            Point startPt = objInfo.TranslatePoint(objInfo.LinkElementOffset, ContainerDrawCanvas);
            lineCurve.StartPoint = startPt;
            lineCurve.ConnectionPoints.Clear();
            lineCurve.EndPoint = startPt;
            lineCurve.HalfArrows = EditorCommon.Controls.Curves.ArrowHalf.Both;
            lineCurve.ArrowEnds = EditorCommon.Controls.Curves.ArrowEnds.End;
            m_PreviewLinkCurve.Visibility = Visibility.Visible;
            m_enPreviewBezierType = objInfo.BezierType;

            mStartLinkObj = objInfo;
        }
        //public class TransitionStaeBaseNodeForUndoRedo
        //{
        //    public BaseNodeControl TransitionStateNode = null;
        //}
        protected void UpdatePreviewBrokenLine(Point tagPt)
        {
            var brokenLine = m_PreviewLinkCurve as EditorCommon.Controls.Curves.BrokenLine;
            var startNode = mStartLinkObj.HostNodeControl;
            var width = startNode.ActualWidth;
            var height = startNode.ActualHeight;
            EditorCommon.Controls.Curves.ConnectorInfo start = new EditorCommon.Controls.Curves.ConnectorInfo();
            var startLT = startNode.TransformToAncestor(ContainerDrawCanvas).Transform(new Point(0, 0));
            start.DesignerItemLeft = startLT.X;
            start.DesignerItemTop = startLT.Y;
            start.DesignerItemSize = startNode.RenderSize;
            start.Position = startNode.TransformToAncestor(ContainerDrawCanvas).Transform(new Point(width / 2, height / 2));
            start.Orientation = (EditorCommon.Controls.Curves.eLinkOrientation)mStartLinkObj.BezierType;

            var points = EditorCommon.Controls.Curves.PathFinder.GetConnectionLine(start, tagPt, EditorCommon.Controls.Curves.eLinkOrientation.Left);
            points.RemoveAt(points.Count - 1);
            brokenLine.ConnectionPoints = points;
            brokenLine.EndPoint = tagPt;
            //var objLocPoint = ContainerDrawCanvas.TranslatePoint(tagPt, mStartLinkObj);
            //var point = tagPt;
            //if (objLocPoint.X < 0)
            //{
            //    point.X = 0;
            //}
            //else if (objLocPoint.X > width)
            //{
            //    point.X = width;
            //}
            //else
            //{
            //    point.X = objLocPoint.X;
            //}
            //if (objLocPoint.Y < 0)
            //{
            //    point.Y = 0;
            //}
            //else if (objLocPoint.Y > height)
            //{
            //    point.Y = height;
            //}
            //else
            //{
            //    point.Y = objLocPoint.Y;
            //}
            //brokenLine.StartPoint = mStartLinkObj.TranslatePoint(point, ContainerDrawCanvas);

        }
        public virtual void EndPreviewBrokenLine(CodeGenerateSystem.Base.LinkPinControl objInfo, NodesContainer nodesContainer)
        {
            var brokenLine = m_PreviewLinkCurve as EditorCommon.Controls.Curves.BrokenLine;
            brokenLine.ConnectionPoints.Clear();
            if (ContainerDrawCanvas == null)
                return;

            if (objInfo == null)
            {
                if (mStartLinkObj != null && m_PreviewLinkCurve.Visibility == Visibility.Visible)
                {
                    IsOpenContextMenu = true;
                }
            }
            else if (mStartLinkObj != null && objInfo != null)
            {
                m_PreviewLinkCurve.Visibility = Visibility.Hidden;
                m_enPreviewBezierType = CodeGenerateSystem.Base.enBezierType.None;

                if (mStartLinkObj.LinkOpType == objInfo.LinkOpType && objInfo.LinkOpType != enLinkOpType.Both) // 只有start和end能连 或者其中之一 和 Both
                    return;
                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(mStartLinkObj, objInfo))
                {
                    objInfo.MouseAssistVisible = Visibility.Hidden;

                    var container = new LinkInfoContainer();
                    if (mStartLinkObj.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = mStartLinkObj;
                        container.End = objInfo;
                    }
                    else if (objInfo.LinkOpType == enLinkOpType.Start)
                    {
                        container.Start = objInfo;
                        container.End = mStartLinkObj;
                    }
                    else
                    {
                        container.Start = mStartLinkObj;
                        container.End = objInfo;
                    }

                    if (StartLinkObj.LinkCurveType == enLinkCurveType.BrokenLine)
                    {
                        var redoAction = new Action<Object>((obj) =>
                        {
                            var linkInfo = new CodeGenerateSystem.Base.ClickableLinkInfo(ContainerDrawCanvas, container.Start, container.End);
                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                                                    (obj) =>
                                                    {
                                                        for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                        {
                                                            var info = container.End.GetLinkInfo(i);
                                                            if (info.m_linkFromObjectInfo == container.Start)
                                                            {
                                                                info.Clear();
                                                                break;
                                                            }
                                                        }
                                                    }, "Create Link");
                        IsDirty = true;
                        //TransitionStaeBaseNodeForUndoRedo transCtrl = new TransitionStaeBaseNodeForUndoRedo();
                        //var redoAction = new Action<object>((obj) =>
                        //{
                        //    var linkInfo = new CodeGenerateSystem.Base.AnimStateLinkInfo(ContainerDrawCanvas, container.Start, container.End);
                        //    transCtrl.TransitionStateNode = linkInfo.AddTransition();
                        //});
                        //redoAction.Invoke(null);
                        //EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                        //                            (obj) =>
                        //                            {
                        //                                for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                        //                                {
                        //                                    var info = container.End.GetLinkInfo(i);
                        //                                    if (info.m_linkFromObjectInfo == container.Start)
                        //                                    {
                        //                                        var transitionInfo = info as CodeGenerateSystem.Base.AnimStateLinkInfo;
                        //                                        transitionInfo.RemoveTransition(transCtrl.TransitionStateNode);
                        //                                        break;
                        //                                    }
                        //                                }
                        //                            }, "Create Link");
                    }
                    else
                    {
                        var redoAction = new Action<Object>((obj) =>
                        {
                            var linkInfo = new CodeGenerateSystem.Base.LinkInfo(ContainerDrawCanvas, container.Start, container.End);
                        });
                        redoAction.Invoke(null);
                        EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostControl.UndoRedoKey, null, redoAction, null,
                                                    (obj) =>
                                                    {
                                                        for (int i = 0; i < container.End.GetLinkInfosCount(); i++)
                                                        {
                                                            var info = container.End.GetLinkInfo(i);
                                                            if (info.m_linkFromObjectInfo == container.Start)
                                                            {
                                                                info.Clear();
                                                                break;
                                                            }
                                                        }
                                                    }, "Create Link");
                    }
                    IsDirty = true;
                }
                else
                    OnLinkFailure(mStartLinkObj, objInfo);
            }
        }
        #endregion
        public class LinkInfoContainer
        {
            public CodeGenerateSystem.Base.LinkPinControl Start;
            public CodeGenerateSystem.Base.LinkPinControl End;
        }

        protected virtual void FilterContextMenu(CodeGenerateSystem.Base.LinkPinControl startLinkObj)
        {

        }

        // 停止绘制预览的贝塞尔曲线


        #endregion

        #region 代码生成

        public virtual System.IO.TextWriter GenerateCode()
        {
            return null;
        }

        #endregion

        #region 代码生成相关

        //protected virtual string OnGetOwnerStateSetClassTypeName() { return m_codeAIClassName; }

        #endregion

        #region 菜单操作

        // 新建
        protected virtual void MenuButton_New_Click(object sender, RoutedEventArgs e)
        {
            foreach (var node in mCtrlNodeList)
            {
                node.Clear();
                ////////////MainDrawCanvas.Children.Remove(node);
            }
            mCtrlNodeList.Clear();
            RefreshNodeProperty(null, ENodeHandleType.UpdateNodeControl);

            GUID = Guid.NewGuid();
            //m_strNickName = "未命名";

            if (OnNew != null)
                OnNew();
        }

        #endregion


        #region 菜单生成

        // 菜单相关
        protected class stMenuValue
        {
            public Type m_Type;
            public string m_Params = null;
            public string mDescription = "";
            //public Object[] m_Params = null;
        }
        protected Dictionary<string, stMenuValue> m_menuNodeInfos = new Dictionary<string, stMenuValue>();
        protected Dictionary<string, MenuItem> m_menuItems = new Dictionary<string, MenuItem>();

        // 查找并创建菜单
        protected void CreateMenu(List<string> menuNames, int nIdx, ItemCollection parMenuItems, string description = null)
        {
            bool bFinded = false;
            foreach (var item in parMenuItems)
            {
                if (item.GetType() != typeof(MenuItem))
                    continue;

                MenuItem menu = item as MenuItem;
                if (((string)menu.Header) == menuNames[nIdx])
                {
                    CreateMenu(menuNames, nIdx + 1, menu.Items, description);
                    bFinded = true;
                    break;
                }
            }

            if (!bFinded)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Name = "NodesContainerMenu_" + menuNames[nIdx];
                menuItem.Header = menuNames[nIdx];
                menuItem.Foreground = System.Windows.Media.Brushes.White;
                menuItem.Background = new SolidColorBrush(Color.FromArgb(255, 58, 58, 58));
                parMenuItems.Add(menuItem);
                m_menuItems[menuNames[nIdx]] = menuItem;

                if (nIdx < menuNames.Count - 1)
                    CreateMenu(menuNames, nIdx + 1, menuItem.Items, description);
                else
                {
                    menuItem.Click += new RoutedEventHandler(MenuItem_Click_AddNode);
                    if (!string.IsNullOrEmpty(description))
                        menuItem.ToolTip = description;
                }
            }
        }

        // 添加节点
        protected void MenuItem_Click_AddNode(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            //Type nodeType;
            stMenuValue menuValue = null;
            //if (!m_menuNodes.TryGetValue(item.Header.ToString(), out nodeType))
            //    return;
            if (!m_menuNodeInfos.TryGetValue(item.Header.ToString(), out menuValue))
                return;

            var csParam = BaseNodeControl.CreateConstructionParam(menuValue.m_Type);
            csParam.CSType = this.CSType;
            csParam.HostNodesContainer = this;
            csParam.ConstructParam = menuValue.m_Params;
            var node = AddNodeControl(menuValue.m_Type, csParam, mMouseRightButtonDownPt.X, mMouseRightButtonDownPt.Y);
            node.ToolTip = item.ToolTip;
        }

        protected virtual Type GetNodeTypeFromType(Type type)
        {
            return type;
        }
        protected virtual string GetNodeInitParamFromType(Type type, Type nodeType)
        {
            return null;
        }

        //protected void GetAllShowNodeInMenu(Type showInMenuClassType, ItemCollection parentMenuItems, string dllName)
        //{
        //    System.Reflection.Assembly assem = null;

        //    if (string.IsNullOrEmpty(dllName))
        //        assem = System.Reflection.Assembly.GetExecutingAssembly();
        //    else
        //        assem = CSUtility.Program.GetAssemblyFromDllFileName(dllName);

        //    var types = assem.GetTypes();
        //    foreach (var type in types)
        //    {
        //        CodeGenerateSystem.ShowInMenu[] showAttrs = (CodeGenerateSystem.ShowInMenu[])type.GetCustomAttributes(showInMenuClassType, false);
        //        if (showAttrs.Length < 1)
        //            continue;

        //        //SetMenuWithAttribute(showAttrs[0]);
        //        CreateMenu(showAttrs[0].MenuList, 0, parentMenuItems);

        //        stMenuValue mValue = new stMenuValue();

        //        mValue.m_Type = GetNodeTypeFromType(type);
        //        mValue.m_Params = GetNodeInitParamFromType(type, mValue.m_Type);
        //        string strKey = showAttrs[0].MenuList[showAttrs[0].MenuList.Count - 1];
        //        m_menuNodeInfos[strKey] = mValue;
        //    }
        //}

        #endregion

        public virtual void FocusNode(Base.BaseNodeControl nodeControl) { }
        public virtual void FocusNodes(Base.BaseNodeControl[] nodeControls) { }

        #region Debug处理

        BaseNodeControl mCurrentDebugNode = null;
        public virtual void SetNodeBreaked(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            if (GUID != context.DebuggerId)
                return;

            var ctrl = FindControl(context.BreakId);
            if (ctrl == mCurrentDebugNode)
                return;

            if (mCurrentDebugNode != null)
            {
                if (mCurrentDebugNode.Id == context.BreakId)
                    return;

                ((IDebugableNode)mCurrentDebugNode).Breaked = false;
            }

            var debugNode = ctrl as IDebugableNode;
            if (debugNode != null)
            {
                debugNode.Breaked = true;
            }
            mCurrentDebugNode = ctrl;
            FocusNode(mCurrentDebugNode);
        }
        public virtual void DebugResume(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            if (GUID != context.DebuggerId)
                return;
            if (mCurrentDebugNode != null)
            {
                ((IDebugableNode)mCurrentDebugNode).Breaked = false;
                mCurrentDebugNode = null;
            }
        }

        #endregion

        partial void AfterLoad_WPF()
        {
            if (HostControl != null)
                EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(HostControl.UndoRedoKey);
        }

    }
}
