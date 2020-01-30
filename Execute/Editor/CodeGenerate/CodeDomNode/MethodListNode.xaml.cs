using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for MethodListNode.xaml
    /// </summary>
    //[CodeGenerateSystem.ShowInNodeList("逻辑.执行队列", "执行队列节点，控制逻辑按顺序执行")]
    public partial class MethodListNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        bool mUpLinkVisible = true;
        public bool UpLinkVisible
        {
            get { return mUpLinkVisible; }
            set
            {
                mUpLinkVisible = value;

                if (mUpLinkVisible)
                    UpLink.Visibility = System.Windows.Visibility.Visible;
                else
                    UpLink.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public MethodListNode(Canvas parentCanvas, string strParam)
            : base(parentCanvas, strParam)
        {
            InitializeComponent();

            SetDragObject(RectangleTitle);
            SetUpLinkElement(UpLink);
            NodeName = "执行队列";

            AddLinkObject(CodeGenerateSystem.Base.enLinkType.Method, UpLink, CodeGenerateSystem.Base.enBezierType.Top, CodeGenerateSystem.Base.enLinkOpType.Start | CodeGenerateSystem.Base.enLinkOpType.End, UpLink.BackBrush, false);

            MethodControl node = new MethodControl(parentCanvas);
            node.Idx = MethodList.Children.Count;
            AddChildNode(node, MethodList);
            //node = new MethodControl(parentCanvas);
            //AddChildNode(node, MethodList);
        }

        private void MenuItem_Click_AddMethod(object sender, RoutedEventArgs e)
        {
            MethodControl node = new MethodControl(ParentDrawCanvas);
            node.Idx = MethodList.Children.Count;

            ContextMenu menu = new System.Windows.Controls.ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "删除函数节点";
            menuItem.Click += new RoutedEventHandler(MenuItem_Click_DelMethod);
            menuItem.Tag = node;
            menu.Items.Add(menuItem);
            node.ContextMenu = menu;

            AddChildNode(node, MethodList);
        }

        private void MenuItem_Click_DelMethod(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            MethodControl node = item.Tag as MethodControl;
            DelChildNode(node);

            int i = 0;
            foreach(var child in MethodList.Children)
            {
                var mc = child as MethodControl;
                if (mc == null)
                    continue;

                mc.Idx = i;
                i++;
            }
        }

        public override void Save(CSUtility.Support.XmlNode xmlNode, bool newGuid, CSUtility.Support.XmlHolder holder)
        {
            xmlNode.AddAttrib("UpLinkVisible", UpLinkVisible.ToString());

            base.Save(xmlNode, newGuid, holder);
        }

        public override void Load(CSUtility.Support.XmlNode xmlNode, double deltaX, double deltaY)
        {
            var att = xmlNode.FindAttrib("UpLinkVisible");
            if (att != null)
            {
                UpLinkVisible = System.Convert.ToBoolean(att.Value);
            }

            base.Load(xmlNode, deltaX, deltaY);

            int i = 0;
            foreach (var child in this.mChildNodes)
            {
                if (child is MethodControl)
                {
                    if (i > 0)
                    {
                        ContextMenu menu = new System.Windows.Controls.ContextMenu();
                        MenuItem menuItem = new MenuItem();
                        menuItem.Header = "删除函数节点";
                        menuItem.Click += new RoutedEventHandler(MenuItem_Click_DelMethod);
                        menuItem.Tag = child;
                        menu.Items.Add(menuItem);
                        child.ContextMenu = menu;
                    }

                    i++;
                }
            } 
        }

        #region 代码生成

        public override void GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, FrameworkElement element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == UpLink || element == null)
            {
                foreach (MethodControl mc in mChildNodes)
                {
                    CodeGenerateSystem.Base.LinkObjInfo info = mc.GetLinkObjInfo(mc.methodEll);
                    if (!info.HasLink)
                        continue;

                    info.GetLinkObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, info.GetLinkElement(0, true), context);
                }
            }
        }

        #endregion

    }

    public class MethodControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public CodeGenerateSystem.Controls.LinkInControl methodEll;

        int mIdx = 0;
        public int Idx
        {
            get { return mIdx; }
            set
            {
                mIdx = value;
                OnPropertyChanged("Idx");
            }
        }

        public MethodControl(Canvas parentCanvas)
            : base(parentCanvas, null)
        {
            Grid grid = new Grid();
            grid.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            AddChild(grid);

            var label = new TextBlock()
            {
                Foreground = Brushes.White,//TryFindResource("TextForeground") as Brush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 2, 3, 2),
                //BorderBrush = Brushes.Black,
                //BorderThickness = new CSUtility.Support.Thickness(0, 0.5, 0, 0.5),
                //HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center
            };
            label.SetBinding(TextBlock.TextProperty, new Binding("Idx") { Source = this } );
            grid.Children.Add(label);

            methodEll = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Margin = new Thickness(0, 0, -15, 0),
                Width = 10,
                Height = 10,
                BackBrush = new SolidColorBrush(Color.FromRgb(130, 130, 216)),
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
            };
            grid.Children.Add(methodEll);
            //methodEll.MouseLeftButtonDown += new MouseButtonEventHandler(ResultTrue_MouseLeftButtonDown);
            //LinkObjInfo linkObj = new LinkObjInfo();
            //linkObj.m_linkType = enLinkType.Method; //enLinkType.MethodListOut;
            //linkObj.m_linkElement = methodEll;
            ////linkObj.m_linkElementOffset = new Point(methodEll.ActualWidth + 1, methodEll.ActualHeight * 0.5);
            //linkObj.m_bezierType = enBezierType.Right;
            //linkObj.m_linkOpType = enLinkOpType.End;
            //AddLinkObject(linkObj);
            AddLinkObject(CodeGenerateSystem.Base.enLinkType.Method, methodEll, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.End, null, false);
        }
    }
}
