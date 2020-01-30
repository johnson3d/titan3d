using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodeGenerateSystem.Controls
{
    public interface NodesListOperator
    {
        void NodesListOperation(CodeGenerateSystem.Controls.NodeListControl nodesList, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData);
    }

    public class NodeListAttributeClass : EditorCommon.DragDrop.IDragAbleObject, INotifyPropertyChanged, EditorCommon.CodeGenerateSystem.INodeListAttribute
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region DragDrop

        public System.Windows.FrameworkElement GetDragVisual()
        {
            if (NodeType != null)
            {
                return NodeItem;
            }
            return null;
        }

        #endregion

        //private string mBindingFile = "";
        public string BindingFile
        {
            get;
            set;
        }

        public delegate void Delegate_FocusNode(NodeListAttributeClass nodeItem);
        public Delegate_FocusNode OnFocusNode;
        public NodesContainerControl.ContextMenuFilterData FilterData;

        internal NodeListAttributeClass()
        {

        }
        public NodeListAttributeClass(NodeListAttributeClass parent)
        {
            mParent = parent;
        }

        string mName = "";
        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }
        string mKeyFilterString = "";
        public string KeyFilterString
        {
            get { return mKeyFilterString; }
            set { mKeyFilterString = value; }
        }
        ImageSource mImageIcon = null;
        public ImageSource ImageIcon
        {
            get { return mImageIcon; }
            set { mImageIcon = value; }
        }

        Visibility mVisiblity = Visibility.Visible;
        public Visibility Visibility
        {
            get { return mVisiblity; }
            set
            {
                mVisiblity = value;
                OnPropertyChanged("Visibility");
            }
        }

        string mHighLightString = "";
        [Browsable(false)]
        public string HighLightString
        {
            get { return mHighLightString; }
            set
            {
                mHighLightString = value;
                OnPropertyChanged("HighLightString");
            }
        }

        bool mIsExpanded = false;
        public bool IsExpanded
        {
            get { return mIsExpanded; }
            set
            {
                mIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        NodeListAttributeClass mParent = null;
        public NodeListAttributeClass Parent
        {
            get => mParent;
            internal set
            {
                mParent = value;
            }
        }

        ObservableCollection<NodeListAttributeClass> mChildList = new ObservableCollection<NodeListAttributeClass>();
        public ObservableCollection<NodeListAttributeClass> ChildList
        {
            get { return mChildList; }
            set { mChildList = value; }
        }

        public Base.NodesContainer NodesContainer
        {
            get;
            set;
        }

        CodeGenerateSystem.Base.BaseNodeControl mNodeItem;
        public CodeGenerateSystem.Base.BaseNodeControl NodeItem
        {
            get
            {
                if (mNodeItem == null)
                {
                    mNodeItem = System.Activator.CreateInstance(NodeType, new object[] { CSParam }) as CodeGenerateSystem.Base.BaseNodeControl;
                }
                return mNodeItem;
            }
        }

        Type mNodeType = null;
        public Type NodeType
        {
            get { return mNodeType; }
            set
            {
                mNodeType = value;
                OnPropertyChanged("NodeType");
            }
        }

        string mDescription = "";
        public string Description
        {
            get { return mDescription; }
            set
            {
                mDescription = value;
                OnPropertyChanged("Description");
            }
        }

        public EditorCommon.CodeGenerateSystem.INodeConstructionParams CSParam
        {
            get;
            set;
        }
    }

    /// <summary>
    /// NodeListControl.xaml 的交互逻辑
    /// </summary>
    public partial class NodeListControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public enum enOperationMode
        {
            DragMode,
            SelectMode,
        }
        public enOperationMode OperationMode
        {
            get;
            set;
        } = enOperationMode.DragMode;

        /// <summary>
        /// 节点连线及绘制的控件
        /// </summary>
        private Base.NodesContainer a;
        public Base.NodesContainer NodesContainer
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
            }
        }

        // 菜单筛选-与当前图关联
        public bool Sensitive_ThisMacross
        {
            get { return (bool)GetValue(Sensitive_ThisMacrossProperty); }
            set { SetValue(Sensitive_ThisMacrossProperty, value); }
        }
        public static readonly DependencyProperty Sensitive_ThisMacrossProperty = DependencyProperty.Register("Sensitive_ThisMacross", typeof(bool), typeof(NodeListControl), new UIPropertyMetadata(true, new PropertyChangedCallback(OnSensitive_ThisMacrossChanged)));
        public static void OnSensitive_ThisMacrossChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as NodeListControl;
            if (ctrl.mCurrentFilterData != null)
            {
                ctrl.NodesListOperatorCtrl.NodesListOperation(ctrl, ctrl.mCurrentFilterData);
            }
        }
        public NodesListOperator NodesListOperatorCtrl;
        public CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData mCurrentFilterData;

        Point mMouseDownPos = new Point();

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                TreeView_NodeList.ItemsSource = null;
                ShowItemWithFilter(ChildList, mFilterString);
                TreeView_NodeList.ItemsSource = ChildList;
                OnPropertyChanged("FilterString");
            }
        }

        ObservableCollection<NodeListAttributeClass> mChildList = new ObservableCollection<NodeListAttributeClass>();
        public ObservableCollection<NodeListAttributeClass> ChildList
        {
            get { return mChildList; }
            set { mChildList = value; }
        }

        private bool ShowItemWithFilter(ObservableCollection<NodeListAttributeClass> items, string filter)
        {
            bool retValue = false;
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrEmpty(filter))
                {
                    item.Visibility = System.Windows.Visibility.Visible;
                    item.HighLightString = filter;
                    ShowItemWithFilter(item.ChildList, filter);
                }
                else
                {
                    if (item.ChildList.Count == 0)
                    {
                        if (item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1 || item.KeyFilterString.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            item.Visibility = System.Windows.Visibility.Visible;
                            item.HighLightString = filter;
                            retValue = true;
                        }
                        else
                            item.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        bool bFind = ShowItemWithFilter(item.ChildList, filter);
                        if (bFind == false)
                            item.Visibility = System.Windows.Visibility.Collapsed;
                        else
                        {
                            item.Visibility = Visibility.Visible;
                            item.IsExpanded = true;
                            retValue = true;
                        }
                    }
                }
            }

            return retValue;
        }

        public NodeListControl()
        {
            InitializeComponent();

            TreeView_NodeList.ItemsSource = ChildList;
        }

        public bool ContainNodes
        {
            get { return ChildList.Count > 0; }
        }

        public void ClearNodes()
        {
            ChildList.Clear();
            NodeListAttributeClassesDic.Clear();
            ParentNodeListAttributeClassesDic.Clear();
        }

        public void RemoveNode(NodeListAttributeClass attClass)
        {
            if (attClass != null && ChildList.Contains(attClass))
                ChildList.Remove(attClass);
        }

        public void UpdateTreeViewNode()
        {
            //TreeView_NodeList.;
        }

        /// <summary>
        /// 根据类型添加节点
        /// </summary>
        /// <param name="type">节点类型</param>
        /// <param name="path">节点显示路径</param>
        /// <param name="param">节点初始化参数</param>
        public NodeListAttributeClass AddNodesFromType(NodesContainerControl.ContextMenuFilterData filterData, Type type, string path, string param, string description,string nodeKeyFilterString = "", ImageSource image = null, string file = "")
        {
            var tempPath = "";
            var idx = path.IndexOf('(');
            if (idx < 0)
            {
                tempPath = path;
            }
            else
            {
                var preStr = path.Substring(0, idx);
                var endStr = path.Substring(idx, path.Length - idx);
                foreach (var split in preStr.Split('.'))
                {
                    tempPath += split + ".";
                }
                tempPath = tempPath.TrimEnd('.');
                tempPath += endStr;
            }
            return CreateNodeListAttributeClass(filterData, tempPath, ChildList, type, param, description, nodeKeyFilterString, image, file);
        }
        public NodeListAttributeClass AddNodesFromType(NodesContainerControl.ContextMenuFilterData filterData, Type type, string path, CodeGenerateSystem.Base.ConstructionParams param, string description, string nodeKeyFilterString = "", ImageSource image = null, string file = "")
        {
            return CreateNodeListAttributeClass(filterData, path, ChildList, type, param, description, nodeKeyFilterString, image, file);
        }
        /// <summary>
        /// 从程序集添加节点，此接口会从程序集中搜索打上CodeGenerateSystem.ShowInNodeList标志的对象
        /// </summary>
        /// <param name="assembly">程序集</param>
        public void AddNodesFromAssembly(NodesContainerControl.ContextMenuFilterData filterData, System.Reflection.Assembly assembly)
        {
            if (assembly == null)
                return;

            foreach (var type in assembly.GetTypes())
            {
                var atts = type.GetCustomAttributes(typeof(CodeGenerateSystem.ShowInNodeList), true);
                if (atts.Length == 0)
                    continue;

                var att = ((CodeGenerateSystem.ShowInNodeList)atts[0]);
                CreateNodeListAttributeClass(filterData, att.Path, ChildList, type, "", att.Description, "", att.Icon);
            }
        }

        class NodeListAttributeClassKey
        {
            public string NodePath;
            public Type NodeType;
            public string Param;
            public EngineNS.ECSType CSType;

            public override bool Equals(object obj)
            {
                var key = obj as NodeListAttributeClassKey;
                if (key == null)
                    return false;
                if ((NodePath == key.NodePath) &&
                   (NodeType == key.NodeType) &&
                   (Param == key.Param) &&
                   (CSType == key.CSType))
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (NodePath + NodeType.FullName + Param + CSType.ToString()).GetHashCode();
            }
        }
        Dictionary<NodeListAttributeClassKey, NodeListAttributeClass> NodeListAttributeClassesDic = new Dictionary<NodeListAttributeClassKey, NodeListAttributeClass>();
        Dictionary<string, NodeListAttributeClass> ParentNodeListAttributeClassesDic = new Dictionary<string, NodeListAttributeClass>();
        NodeListAttributeClass CreateNodeListAttributeClass(NodesContainerControl.ContextMenuFilterData filterData, string nodePath, ObservableCollection<NodeListAttributeClass> items, Type nodeType, CodeGenerateSystem.Base.ConstructionParams param, string description, string nodeKeyFilterString = "",ImageSource image = null, string file = "")
        {
            if (filterData == null)
                throw new ArgumentNullException("filterData不能未空！");
            var key = new NodeListAttributeClassKey();
            key.NodePath = nodePath;
            key.NodeType = nodeType;
            key.Param = param.ConstructParam;
            key.CSType = filterData.CSType;
            NodeListAttributeClass cls;
            if (!NodeListAttributeClassesDic.TryGetValue(key, out cls))
            {
                var name = nodePath.Substring(nodePath.LastIndexOf('/') + 1);

                cls = new NodeListAttributeClass();
                if (string.IsNullOrEmpty(param.DisplayName))
                {
                    cls.Name = name;
                }
                else
                {
                    cls.Name = name + "(" + param.DisplayName + ")";
                }
                cls.NodesContainer = NodesContainer;
                cls.NodeType = nodeType;
                cls.CSParam = param;
                cls.CSParam.CSType = filterData.CSType;
                cls.Description = description;
                cls.ImageIcon = image;
                cls.BindingFile = file;
                cls.FilterData = filterData;
                cls.KeyFilterString = nodeKeyFilterString;
                NodeListAttributeClassesDic[key] = cls;
            }

            CodeGenerateSystem.Base.BaseNodeControl.InvokeInitConstructionParamsType(cls.NodeType);
            CodeGenerateSystem.Base.BaseNodeControl.InvokeInitNodePinTypes(cls.NodeType, cls.CSParam);

            if (filterData.StartLinkObj != null)
            {
                CodeGenerateSystem.Base.BaseNodeControl.LinkPinDescDic dic;
                if (CodeGenerateSystem.Base.BaseNodeControl.LinkPinDescs.TryGetValue(cls.CSParam, out dic))
                {
                    var pinDescs = dic.LinkPinInfosDic.Values;
                    bool showWithFilter = false;
                    foreach (var pinDesc in pinDescs)
                    {
                        if ((pinDesc.PinOpType == filterData.StartLinkObj.LinkOpType) && (filterData.StartLinkObj.LinkOpType != (Base.enLinkOpType.Start | Base.enLinkOpType.End)))
                            continue;

                        if (!filterData.StartLinkObj.CanLinkWith(pinDesc))
                            continue;

                        showWithFilter = true;
                        break;
                    }

                    if (showWithFilter)
                    {
                        BuildNodeListAttributeClassTree(cls, nodePath, items);
                        return cls;
                    }
                }
            }
            else
            {
                BuildNodeListAttributeClassTree(cls, nodePath, items);
                return cls;
            }
            return null;
        }
        NodeListAttributeClass CreateNodeListAttributeClass(NodesContainerControl.ContextMenuFilterData filterData, string nodePath, ObservableCollection<NodeListAttributeClass> items, Type nodeType, string param, string description, string nodeKeyFilterString = "", ImageSource image = null, string file = "")
        {
            var csParam = CodeGenerateSystem.Base.BaseNodeControl.CreateConstructionParam(nodeType);
            csParam.ConstructParam = param;
            csParam.CSType = filterData.CSType;
            return CreateNodeListAttributeClass(filterData, nodePath, items, nodeType, csParam, description, nodeKeyFilterString, image, file);
        }
        void BuildNodeListAttributeClassTree(NodeListAttributeClass classData, string nodePath, ObservableCollection<NodeListAttributeClass> rootItems)
        {
            var splits = nodePath.Split('/');
            string pathStr = "";
            NodeListAttributeClass parentClass = null;
            for (int i = 0; i < splits.Length - 1; i++)
            {
                if (i == 0)
                    pathStr += splits[i];
                else
                    pathStr += "." + splits[i];

                NodeListAttributeClass tempAttClass;
                if (!ParentNodeListAttributeClassesDic.TryGetValue(pathStr, out tempAttClass))
                {
                    var tempClass = new NodeListAttributeClass(parentClass);
                    tempClass.Name = splits[i];
                    tempClass.NodesContainer = NodesContainer;

                    if (parentClass == null)
                        rootItems.Add(tempClass);
                    else
                        parentClass.ChildList.Add(tempClass);

                    ParentNodeListAttributeClassesDic[pathStr] = tempClass;

                    parentClass = tempClass;
                }
                else
                {
                    parentClass = tempAttClass;
                    if (!rootItems.Contains(parentClass) && i == 0)
                    {
                        rootItems.Add(parentClass);
                    }
                }
            }
            if (parentClass == null)
                rootItems.Add(classData);
            else
            {
                parentClass.ChildList.Add(classData);
            }
        }

        //// 创建节点列表
        //private void CreateNodeListItem(NodeListAttributeClass classData, List<string> nodePath, NodeListAttributeClass parent, ObservableCollection<NodeListAttributeClass> items, Type nodeType, string param, string description, ImageSource image = null, string file = "")
        //{
        //    if (nodePath.Count == 0)
        //        return;

        //    bool bFind = false;
        //    foreach (var item in items)
        //    {
        //        if (item.Name.Equals(nodePath[0]))
        //        {
        //            // 同名称的有可能是重载，所以这里继续往后加
        //            if(nodePath.Count == 1)
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                nodePath.RemoveAt(0);
        //                CreateNodeListItem(classData, nodePath, item, item.ChildList, nodeType, param, description, null, file);
        //                bFind = true;
        //                break;
        //            }
        //        }
        //    }

        //    if (!bFind)
        //    {
        //        var listItem = new NodeListAttributeClass(parent);
        //        listItem.Name = nodePath[0];
        //        listItem.NodesContainer = NodesContainer;
        //        items.Add(listItem);                

        //        if (nodePath.Count > 1)
        //        {
        //            nodePath.RemoveAt(0);
        //            CreateNodeListItem(classData, nodePath, listItem, listItem.ChildList, nodeType, param, description, null, file);
        //        }
        //        else
        //        {
        //            listItem.NodeType = nodeType;
        //            listItem.Param = param;
        //            listItem.Description = description;
        //            listItem.ImageIcon = image;
        //            listItem.BindingFile = file;
        //            attribute = listItem;
        //        }
        //    }
        //}

        private void Button_ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in ChildList)
            {
                ExpandItem(child, true);
            }
        }
        private void ExpandItem(NodeListAttributeClass item, bool expand)
        {
            if (item == null)
                return;

            item.IsExpanded = expand;
            foreach (var child in item.ChildList)
            {
                ExpandItem(child, expand);
            }
        }
        private void Button_FoldAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in ChildList)
            {
                ExpandItem(child, false);
            }
        }

        public void AddFocusMethod(NodeListAttributeClass item)
        {
            if (item != null)
            {
                item.OnFocusNode -= FocusNode;
                item.OnFocusNode += FocusNode;

                AddFocusMethod(item.ChildList);
            }
        }

        public void AddFocusMethod()
        {
            AddFocusMethod(ChildList);
        }
        void AddFocusMethod(ObservableCollection<NodeListAttributeClass> items)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    item.OnFocusNode -= FocusNode;
                    item.OnFocusNode += FocusNode;

                    AddFocusMethod(item.ChildList);
                }
            }
        }

        void FocusNode(NodeListAttributeClass nodeItem)
        {
            if (nodeItem == null)
                return;
            if (nodeItem.NodeType != null)
            {
                var paramSplits = nodeItem.CSParam.ConstructParam.Split(';');
                if (paramSplits.Length < 2)
                    return;
                var splits = paramSplits[1].Split(',');
                if (splits.Length < 4)
                    return;

                var node = NodesContainer.FindControl(EngineNS.Rtti.RttiHelper.GuidTryParse(splits[2]));
                NodesContainer.FocusNode(node);
                return;
            }
            foreach (var item in nodeItem.ChildList)
            {
                FocusNode(item);
            }
        }

        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as NodeListAttributeClass;
            if (listItem == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                mMouseDownPos = e.GetPosition(grid);
            }
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                listItem.OnFocusNode?.Invoke(listItem);
            }
        }

        public bool EnableDrag = true;
        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (!EnableDrag)
                return;
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as NodeListAttributeClass;
            if (listItem == null)
                return;

            if (listItem.ChildList.Count > 0)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(grid);
                if ((System.Math.Abs(pos.X - mMouseDownPos.X) > 3) ||
                   (System.Math.Abs(pos.Y - mMouseDownPos.Y) > 3))
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(Program.NodeDragType, new EditorCommon.DragDrop.IDragAbleObject[] { listItem });
                }
            }
        }

        // 初始化提示框
        private void InitializeToolTip(FrameworkElement e, NodeListAttributeClass listItem)
        {
            if (listItem.NodeType != null && e.ToolTip == null)
            {
                var toolTip = new ToolTip()
                {
                    Style = e.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ToolTipStyle_Default")) as System.Windows.Style,
                };
                e.ToolTip = toolTip;

                var stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
                toolTip.Content = stackPanel;

                var textBlock = new TextBlock();
                textBlock.Style = e.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as System.Windows.Style;
                textBlock.Text = listItem.Description;
                textBlock.Margin = new Thickness(2, 2, 2, 8);
                stackPanel.Children.Add(textBlock);
                if (listItem.NodeItem.Parent == null)
                //if (listItem.NodeItem != null)
                {
                    listItem.NodeItem.Margin = new Thickness(20);
                    listItem.NodeItem.HostNodesContainer = NodesContainer;
                    //listItem.NodeItem.InitializeUsefulLinkDatas();
                    stackPanel.Children.Add(listItem.NodeItem);
                }
            }
        }

        private void TreeViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as NodeListAttributeClass;
            if (listItem == null)
                return;
            InitializeToolTip(grid, listItem);
        }

        public delegate void Delegate_OnNodeListItemSelected(NodeListAttributeClass item);
        public Delegate_OnNodeListItemSelected OnNodeListItemSelected;
        private void TreeView_NodeList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (OperationMode != enOperationMode.SelectMode)
                return;
            var cls = e.NewValue as NodeListAttributeClass;
            if (cls == null)
                return;
            if (cls.NodeType != null)
                OnNodeListItemSelected?.Invoke(cls);
        }

        public void FocusSearchBox()
        {
            SearchBX.FocusInput();
        }
    }
}
