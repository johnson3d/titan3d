using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for IfNode.xaml
    /// </summary>
    //[PluginNodeAttribute(Path = "", EditorTypes = new string[] { "DelegateMethodEditor" })]
    [CodeGenerateSystem.ShowInNodeList("逻辑/if", "条件控制节点，根据条件控制脚本执行")]
    public partial class IfNode : CodeGenerateSystem.Base.IDebugableNode
    {
        bool mBreaked = false;
        public bool Breaked
        {
            get { return mBreaked; }
            set
            {
                if (mBreaked == value)
                    return;
                mBreaked = value;
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    BreakedPinShow = mBreaked;
                    ChangeParentLogicLinkLine(mBreaked);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }
        public void ChangeParentLogicLinkLine(bool change)
        {
            ChangeParentLogicLinkLine(change, MethodLink_Pre);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, MethodLink_Pre);
        }
        public bool CanBreak()
        {
            return true;
        }

        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlMethodLink_Pre = MethodLink_Pre;
            mCtrlMethodLink_False = MethodLink_False;

            mChildNodeContainer = ConditionStack;

            SetUpLinkElement(MethodLink_Pre);
        }
        
        private int mCCIndex = 0;
        private void AddConditionControl()
        {
            var ccP = BaseNodeControl.CreateConstructionParam(typeof(ConditionControl));
            ccP.CSType = mCSParam.CSType;
            ccP.HostNodesContainer = this.HostNodesContainer;
            ConditionControl cc = new ConditionControl(ccP);
            cc.Index = mCCIndex++;

            //ContextMenu menu = new ContextMenu();
            //MenuItem menuItem = new MenuItem();
            //menuItem.Header = "删除条件";
            //menuItem.Click += new RoutedEventHandler(MenuItem_Click_DelMethod);
            //menuItem.Tag = cc;
            //menu.Items.Add(menuItem);
            //cc.ContextMenu = menu;

            AddChildNode(cc, ConditionStack);
        }

        private void MenuItem_Click_DelMethod(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            ConditionControl node = item.Tag as ConditionControl;
            DelChildNode(node);
        }

        private void MenuItem_Click_AddLink(object sender, RoutedEventArgs e)
        {
            AddConditionControl();
        }

        protected override void CollectionErrorMsg()
        {
            foreach(var child in mChildNodes)
            {
                if(child.CheckError() == false)
                {
                    HasError = true;
                    ErrorDescription = child.ErrorDescription;
                    break;
                }
            }
        }
    }

    public partial class ConditionControl
    {
        //public bool m_bIsElseIf = false;

        partial void InitConstruction()
        {
            var resDic = new ResourceDictionary();
            resDic.Source = new Uri($"/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Relative);
            this.Resources = resDic;

            //m_IsOneInLink = false;
            NodeType = enNodeType.ChildNode;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0f, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0f, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0f, GridUnitType.Star) });
            AddChild(grid);

            mConditionPin = new CodeGenerateSystem.Controls.LinkInControl()
            {
                Margin = new System.Windows.Thickness(8, 8, 3, 8),
                NameString = "条件",
                //BackBrush = TryFindResource("Link_BoolBrush") as Brush,//new SolidColorBrush(Color.FromRgb(243, 146, 243)),
                HorizontalAlignment = HorizontalAlignment.Left,
                Direction = CodeGenerateSystem.Base.enBezierType.Left,
            };
            grid.Children.Add(mConditionPin);
            Grid.SetColumn(mConditionPin, 0);
            //m_conditionEllipse.MouseLeftButtonUp += new MouseButtonEventHandler(Condition_MouseLeftButtonUp);

            var cb = new CheckBox()
            {
                IsChecked = ConditionDefaultValue,
                Margin = new System.Windows.Thickness(0, 8, 8, 8),
                VerticalAlignment = VerticalAlignment.Center,
            };
            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding("ConditionDefaultValue") { Source = this, Mode = BindingMode.TwoWay });
            grid.Children.Add(cb);
            Grid.SetColumn(cb, 1);

            mResultMethod = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                Margin = new Thickness(8),
                NameString = "True",
                //BackBrush = new SolidColorBrush(Color.FromRgb(130, 130, 216)),
                HorizontalAlignment = HorizontalAlignment.Right,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
                PinType = LinkPinControl.enPinType.Exec,
            };
            grid.Children.Add(mResultMethod);
            Grid.SetColumn(mResultMethod, 2);
            //methodEll.MouseLeftButtonDown += new MouseButtonEventHandler(ResultTrue_MouseLeftButtonDown);
        }

        protected override void CollectionErrorMsg()
        {
            if(!mConditionPin.HasLink)
            {
                HasError = true;
                ErrorDescription = $"第{Index}个条件没有连接";
            }
        }
    }
}
