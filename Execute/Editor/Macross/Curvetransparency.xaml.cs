using CodeGenerateSystem.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Macross
{
    /// <summary>
    /// Curvetransparency.xaml 的交互逻辑
    /// </summary>
    public partial class Curvetransparency : UserControl
    {
        public MacrossLinkControlBase Control;
        public Curvetransparency()
        {
            InitializeComponent();
        }

        private void UIDataSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded == false)
                return;

            DataSliderValueChanged(MacrossPanelBase.GraphCategoryName, UIDataSlider.Value, false);
            DataSliderValueChanged(MacrossPanelBase.FunctionCategoryName, UIDataSlider.Value, false);
            UIDataInfo.Text = "数据线透明度：" + Math.Floor(UIDataSlider.Value) + "%";
        }

        private void UIStepSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IsLoaded == false)
                return;

            DataSliderValueChanged(MacrossPanelBase.GraphCategoryName, UIStepSlider.Value, true);
            DataSliderValueChanged(MacrossPanelBase.FunctionCategoryName, UIStepSlider.Value, true);
            UIStepInfo.Text = "流程线透明度：" + Math.Floor(UIStepSlider.Value) + "%";
        }

        public void DataSliderValueChanged(string name, double value, bool call)
        {
            if (Control == null)
                return;
 
            Macross.Category category;
            if (!Control.MacrossOpPanel.CategoryDic.TryGetValue(name, out category))
                return;

            var noUse = ChangeItemsTransparency(category.Items, value, call);
        }

        public async Task ChangeItemsTransparency(ObservableCollection<Macross.CategoryItem> items, double value, bool call)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var nodesContainer = await Control.MacrossOpPanel.HostControl.GetNodesContainer(item, true);
                if (nodesContainer != null)
                {
                    foreach (var node in nodesContainer.NodesControl.CtrlNodeList)
                    {

                        DealNodeLinkPath(node, value, call);
                    }
                }

                if (item.Children != null && item.Children.Count > 0)
                {
                    await ChangeItemsTransparency(item.Children, value, call);
                }
            }
        }

        public void DealNodeLinkPath(BaseNodeControl node, double value, bool call)
        {
            LinkPinControl[] LinkPinControls = node.GetLinkPinInfos();
            for (int j = 0; j < LinkPinControls.Length; j++)
            {
                var linkPinControl = LinkPinControls[j];
                if (linkPinControl.LinkCurveType == enLinkCurveType.Bezier)
                {
                    foreach (var linkinfo in linkPinControl.LinkInfos)
                    {
                        //if (linkinfo.IsMethodLink == IsMethodLink)
                        //{
                        //    linkinfo.LinkPath.Opacity = value / 100.0f;
                        //}
                        if ((linkinfo.m_linkFromObjectInfo.LinkType & enLinkType.Method) != 0 && call)//&& (linkinfo.m_linkToObjectInfo.LinkType & enLinkType.Method) != 0
                        {

                            linkinfo.LinkPath.Opacity = value / 100.0f;
                        }
                        else if ((linkinfo.m_linkFromObjectInfo.LinkType & enLinkType.Method) == 0 && !call)
                        {
                            linkinfo.LinkPath.Opacity = value / 100.0f;
                        }
                    }
                }
            }

            var subnodes = node.GetChildNodes();
            for (int i = 0; i < subnodes.Count; i++)
            {
                DealNodeLinkPath(subnodes[i], value,call);
            }
        }
    }
}

