using System;
using System.Collections.Generic;
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
using Macross;

namespace UIEditor.UIMacross
{

    /// <summary>
    /// Interaction logic for MacrossLinkControl.xaml
    /// </summary>
    public partial class MacrossLinkControl : Macross.MacrossLinkControlBase
    {
        public MacrossLinkControl()
        {
            InitializeComponent();

            NodesCtrlAssist = NodesCtrlAssistCtrl;
            MacrossOpPanel = MacrossOpPanelCtrl;
            mPG = PG;

            NodesCtrlAssist.HostControl = this;
            NodesCtrlAssist.LinkedCategoryItemName = Macross.MacrossPanel.MainGraphName;
            MacrossOpPanel.HostControl = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UISlider.Control = this;
        }
        public override async Task<bool> LoadData(string absFile)
        {
            var xndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            if(xndHolder != null)
            {
                mNodesContainerDic.Clear();
                Macross.CategoryItem mainGraphItem = null;
                foreach(var category in MacrossOpPanel.CategoryDic)
                {
                    category.Value.Items.Clear();
                    var graphCategoryNode = xndHolder.Node.FindNode("Category_" + category.Key);
                    if (graphCategoryNode == null)
                        continue;

                    var itemNodes = graphCategoryNode.GetNodes();
                    foreach(var itemNode in itemNodes)
                    {
                        var item = new Macross.CategoryItem(null, category.Value);
                        item.Load(itemNode, this);
                        switch(item.InitTypeStr)
                        {
                            case "UI_UIElement_Variable":
                                {
                                    var initData = item.InitData as UIElementVariableCategoryItemInitData;
                                    initData.UIElement = ((UIMacrossEditorControl)HostControl).HostControl.mCurrentUIHost.FindChildElement(initData.UIElementId);
                                }
                                break;
                            case "UI_UIElement_Event":
                                {
                                    var initData = item.InitData as UIElementEventCategoryItemInitData;
                                    initData.UIElement = ((UIMacrossEditorControl)HostControl).HostControl.mCurrentUIHost.FindChildElement(initData.UIElementId);
                                }
                                break;
                            case "UI_UIElement_PropertyCustomBind":
                                {
                                    var initData = item.InitData as UIElementPropertyCustomBindCategoryitemInitData;
                                    initData.UIElement = ((UIMacrossEditorControl)HostControl).HostControl.mCurrentUIHost.FindChildElement(initData.UIElementId);
                                }
                                break;
                            case "UI_UIElement_VariableBind":
                                {
                                    var initdata = item.InitData as UIElementVariableBindCategoryItemInitData;
                                    initdata.UIElement = ((UIMacrossEditorControl)HostControl).HostControl.mCurrentUIHost.FindChildElement(initdata.UIElementId);
                                    initdata.TargetUIElement = ((UIMacrossEditorControl)HostControl).HostControl.mCurrentUIHost.FindChildElement(initdata.TargetUIElementId);
                                }
                                break;
                            case "UI_UIElement_CustomEvent":
                                {
                                }
                                break;
                        }
                        item.Initialize(this, item.InitData);
                        category.Value.Items.Add(item);

                        switch(item.CategoryItemType)
                        {
                            case Macross.CategoryItem.enCategoryItemType.MainGraph:
                                {
                                    mainGraphItem = item;
                                }
                                break;
                        }
                    }
                }

                MacrossOpPanel.SetMainGridItem(mainGraphItem);
                return true;
            }
            return false;
        }

        public override async Task<NodesControlAssist> ShowNodesContainer(INodesContainerDicKey graphKey)
        {
            var retValue = await base.ShowNodesContainer(graphKey);

            Category bindFuncCategory;
            if(MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.UIBindFuncCategoryName, out bindFuncCategory))
            {
                foreach(var funcItem in bindFuncCategory.Items)
                {
                    CodeDomNode.CustomMethodInfo methodInfo = null;
                    if(funcItem.PropertyShowItem is UIElementVariableBindCategoryItemPropertys)
                    {
                        var pro = funcItem.PropertyShowItem as UIElementVariableBindCategoryItemPropertys;
                        methodInfo = pro.MethodInfo;
                    }
                    if(funcItem.PropertyShowItem is UIElementPropertyCustomBindCategoryItemPropertys)
                    {
                        var pro = funcItem.PropertyShowItem as UIElementPropertyCustomBindCategoryItemPropertys;
                        methodInfo = pro.MethodInfo;
                    }
                    if (methodInfo == null)
                        continue;
                    for (int i = 0; i < retValue.NodesControl.CtrlNodeList.Count; i++)
                    {
                        var node = retValue.NodesControl.CtrlNodeList[i];
                        if (node is CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode)
                        {
                            var funcNode = node as CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode;
                            var nodeMethodInfo = funcNode.GetMethodInfo();

                            if (nodeMethodInfo.MethodName == methodInfo.MethodName)
                            {
                                await funcNode.ResetMethodInfo(methodInfo);
                            }
                        }
                    }
                }
            }
            if(MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.UIEventFuncCategoryName, out bindFuncCategory))
            {
                foreach(var funcItem in bindFuncCategory.Items)
                {
                    var pro = funcItem.PropertyShowItem as UIElementEventCategoryItemPorpertys;
                    if (pro == null)
                        continue;
                    var methodInfo = pro.MethodInfo;
                    if (methodInfo == null)
                        continue;
                    for(int i=0; i<retValue.NodesControl.CtrlNodeList.Count; i++)
                    {
                        var node = retValue.NodesControl.CtrlNodeList[i];
                        if(node is CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode)
                        {
                            var funcNode = node as CodeDomNode.CustomMethodInfo.IFunctionResetOperationNode;
                            var nodeMethodInfo = funcNode.GetMethodInfo();

                            if(nodeMethodInfo.MethodName == methodInfo.MethodName)
                            {
                                await funcNode.ResetMethodInfo(methodInfo);
                            }
                        }
                    }
                }
            }

            return retValue;
        }
    }
}
