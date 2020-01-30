using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("逻辑/Sequence(序列)", "")]
    public sealed partial class Sequence
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mSeqInPin = SeqIn;
            AddElementPin();
        }

        void AddElementPin()
        {
            var param = CreateConstructionParam(typeof(SequenceElement));
            param.CSType = CSParam.CSType;
            param.HostNodesContainer = this.HostNodesContainer;
            var elm = new SequenceElement(param);
            elm.ElementIdx = mChildNodes.Count;
            AddChildNode(elm, StackPanel_ElementPins);
        }
        private void Button_AddPin_Click(object sender, RoutedEventArgs e)
        {
            AddElementPin();
        }

        public void UpdateElementIndexes()
        {
            for(int i=0; i< mChildNodes.Count; i++)
            {
                var elm = mChildNodes[i] as SequenceElement;
                if (elm != null)
                    elm.ElementIdx = i;
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as Sequence;
            copyedNode.ClearChildNode();
            foreach(var child in mChildNodes)
            {
                var copyedChild = child.Duplicate(param);
                copyedNode.AddChildNode(copyedChild, copyedNode.StackPanel_ElementPins);
            }
            return copyedNode;
        }
    }

    public partial class SequenceElement
    {
        StackPanel mPanel;
        partial void InitConstruction()
        {
            Resources = new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/CodeGenerateSystem;component/Themes/Generic.xaml", UriKind.Absolute),
            };

            mPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 8, 8),
            };
            AddChild(mPanel);
            mElemPin = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
                PinType = CodeGenerateSystem.Base.LinkPinControl.enPinType.Exec,
            };
            mPanel.Children.Add(mElemPin);
            mElemPin.NameString = "[0]";
            mElemPin.OnCollectionContextMenus = (CodeGenerateSystem.Base.LinkPinControl linkControl) =>
            {
                mElemPin.AddContextMenuItem("移除节点", "Sequence", (obj, arg) =>
                {
                    RemoveFromParent();
                    ((Sequence)ParentNode).UpdateElementIndexes();
                });
            };
        }
    }
}
