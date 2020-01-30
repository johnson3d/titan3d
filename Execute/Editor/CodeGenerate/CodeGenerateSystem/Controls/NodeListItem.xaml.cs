using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// NodeListItem.xaml 的交互逻辑
    /// </summary>
    public partial class NodeListItem : TreeViewItem, EditorCommon.DragDrop.IDragAbleObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        /// <summary>
        /// 节点连线及绘制的控件
        /// </summary>
        public Base.NodesContainer NodesContainer;

        string mNodeName = "";
        public string NodeName
        {
            get { return mNodeName; }
            set
            {
                mNodeName = value;
                TextBlock_Name.Text = mNodeName;
            }
        }

        string mSelectString = "";
        public string SelectString
        {
            get { return mSelectString; }
            set
            {
                mSelectString = value;

                if (string.IsNullOrEmpty(mSelectString))
                {
                    TextBlock_Name.Text = "";
                    TextBlock_Name.Inlines.Add(new Run(NodeName));
                }
                else
                {
                    TextBlock_Name.Inlines.Clear();
                    TextBlock_Name.Text = "";
                    
                    int startIdx = 0;
                    while (true)
                    {
                        var idx = NodeName.IndexOf(mSelectString, startIdx, StringComparison.OrdinalIgnoreCase);
                        if (idx < 0)
                            break;

                        TextBlock_Name.Inlines.Add(new Run(NodeName.Substring(startIdx, idx - startIdx)));
                        TextBlock_Name.Inlines.Add(new Run(NodeName.Substring(idx, mSelectString.Length)) { Background = new SolidColorBrush(Color.FromRgb(149, 96, 0)) });
                        startIdx = idx + mSelectString.Length;
                    }

                    TextBlock_Name.Inlines.Add(new Run(NodeName.Substring(startIdx)));
                }
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

        ImageSource mImageIcon;
        public ImageSource ImageIcon
        {
            get { return mImageIcon; }
            set
            {
                mImageIcon = value;
                OnPropertyChanged("ImageIcon");
            }
        }

        // 初始化提示框
        private void InitializeToolTip()
        {
            if(mNodeType != null && this.ToolTip == null)
            {
                var toolTip = new ToolTip()
                {
                    Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ToolTipStyle_Default")) as System.Windows.Style,
                };
                this.ToolTip = toolTip;

                var stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
                toolTip.Content = stackPanel;

                var textBlock = new TextBlock();
                textBlock.Style = this.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as System.Windows.Style;
                textBlock.Text = Description;
                textBlock.Margin = new Thickness(2, 2, 2, 8);
                stackPanel.Children.Add(textBlock);
                mNodeItem = System.Activator.CreateInstance(NodeType, new object[] { null, Param }) as CodeGenerateSystem.Base.BaseNodeControl;
                if (mNodeItem != null)
                {
                    mNodeItem.Margin = new Thickness(20);
                    mNodeItem.HostNodesContainer = NodesContainer;
                    //mNodeItem.InitializeUsefulLinkDatas();
                    stackPanel.Children.Add(mNodeItem);
                }
            }
        }

        CodeGenerateSystem.Base.BaseNodeControl mNodeItem;

        public string Param
        {
            get;
            set;
        }

        public NodeListItem()
        {
            InitializeComponent();
        }

        #region DragDrop

        public System.Windows.FrameworkElement GetDragVisual()
        {
            if(NodeType != null)
            {
                return mNodeItem;
            }
            else
            {
                var header = this.Header as System.Windows.FrameworkElement;
                return VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(header)) as System.Windows.FrameworkElement;
            }
        }

        #endregion

        public delegate void Delegate_FocusNode(NodeListItem nodeItem);
        public Delegate_FocusNode OnFocusNode;

        Point mMouseDownPos = new Point();
        private void TreeViewItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listItem = sender as NodeListItem;
            if (listItem == null)
                return;            

            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                mMouseDownPos = e.GetPosition(listItem);
            }
            if(e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                OnFocusNode?.Invoke(this);
            }
        }

        private void TreeViewItem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var listItem = sender as NodeListItem;
            if (listItem == null)
                return;

            if (listItem.HasItems)
                return;

            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(listItem);
                if(((pos.X - mMouseDownPos.X) > 3) ||
                   ((pos.Y - mMouseDownPos.Y) > 3))
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(Program.NodeDragType, new EditorCommon.DragDrop.IDragAbleObject[] { listItem });
                }
            }
        }

        private void TreeViewItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            InitializeToolTip();
        }
    }
}
