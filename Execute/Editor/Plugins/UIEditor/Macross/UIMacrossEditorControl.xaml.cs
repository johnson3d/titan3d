using System;
using System.Collections.Generic;
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

namespace UIEditor.UIMacross
{
    /// <summary>
    /// UIMacrossEditorControl.xaml 的交互逻辑
    /// </summary>
    public partial class UIMacrossEditorControl : Macross.MacrossControlBase
    {
        internal MainControl HostControl;

        public ImageSource CompileStatusIcon
        {
            get { return (ImageSource)GetValue(CompileStatusIconProperty); }
            set { SetValue(CompileStatusIconProperty, value); }
        }
        public static readonly DependencyProperty CompileStatusIconProperty = DependencyProperty.Register("CompileStatusIcon", typeof(ImageSource), typeof(UIMacrossEditorControl), new FrameworkPropertyMetadata(null));

        public UIMacrossEditorControl()
        {
            InitializeComponent();
            Macross_Client.HostControl = this;

            Macross.CategoryItem.RegistInitAction("UI_UIElement_Variable", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                if (item.PropertyShowItem == null)
                    item.PropertyShowItem = new UIElementVariableCategoryItemPropertys();

                var varItemPro = item.PropertyShowItem as UIElementVariableCategoryItemPropertys;
                BindingOperations.SetBinding(varItemPro, UIElementVariableCategoryItemPropertys.VariableNameProperty, new Binding("Name") { Source = item });
                BindingOperations.SetBinding(varItemPro, UIElementVariableCategoryItemPropertys.TooltipProperty, new Binding("ToolTips") { Source = item, Mode=BindingMode.TwoWay });

                var initData = data as UIEditor.UIMacross.UIElementVariableCategoryItemInitData;

                BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("Name") { Source = initData.UIElement.Initializer });

                varItemPro.VariableType = initData.ElementType;
                varItemPro.ElementId = initData.UIElementId;
                var atts = initData.ElementType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlAttribute), false);
                if (atts.Length > 0)
                {
                    var att = atts[0] as EngineNS.UISystem.Editor_UIControlAttribute;
                    item.Icon = new BitmapImage(new Uri($"/UIEditor;component/Icons/{att.Icon}", UriKind.Relative));
                }

                var menuItem = new MenuItem()
                {
                    Name = "VariableFocus",
                    Header = "查找引用",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                };
                item.CategoryItemContextMenu.Items.Add(menuItem);

                item.OnDropToNodesControlAction = (dropData) =>
                {
                    var nodesControlAssist = dropData.NodesContainerHost as Macross.NodesControlAssist;
                    nodesControlAssist.ShowGetButton = true;
                    nodesControlAssist.ShowSetButton = false;
                    nodesControlAssist.InitVariableDropShow(dropData.DropPos);
                };
                item.OnDropVariableGetNodeControlAction = (actionData) =>
                {
                    var nodeType = typeof(CodeDomNode.PropertyNode);
                    var csParam = new CodeDomNode.PropertyNode.PropertyNodeConstructionParams();
                    csParam.CSType = ctrl.CSType;
                    csParam.HostNodesContainer = actionData.NodesContainer;
                    csParam.ConstructParam = "";
                    csParam.PropertyInfo = new CodeDomNode.PropertyInfoAssist()
                    {
                        PropertyName = item.Name,
                        PropertyType = varItemPro.VariableType,
                        HostType = CodeDomNode.MethodInfoAssist.enHostType.This,
                        MacrossClassType = HostControl.CurrentResInfo.ResourceName.PureName(),
                        Direction = CodeDomNode.PropertyInfoAssist.enDirection.Get,
                    };

                    var pos = actionData.NodesContainer._RectCanvas.TranslatePoint(actionData.Pos, actionData.NodesContainer._MainDrawCanvas);
                    var node = actionData.NodesContainer.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                    item.AddInstanceNode(actionData.NodesContainer.GUID, node);
                    //nc.VariablePopupGetProcess(pos);
                };
            }));
            Macross.CategoryItem.RegistInitAction("UI_UIElement_Event", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data)=>
            {
                var initData = data as UIElementEventCategoryItemInitData;
                item.Icon = TryFindResource("Icon_Function") as ImageSource;
                item.Name = initData.FunctionName + "___" + item.Id.ToString().Replace("-", "_");
                item.ShowName = $"{initData.FunctionName}({initData.UIElement.Initializer.Name})";
                Macross.CategoryItem.Delegate_OnDoubleClick action = async (categoryItem) =>
                {
                    var noUse = await ctrl.ShowNodesContainer(categoryItem);
                };
                item.OnDoubleClick += action;
                if (item.PropertyShowItem == null)
                    item.PropertyShowItem = new UIElementEventCategoryItemPorpertys();
                var varItemPro = item.PropertyShowItem as UIElementEventCategoryItemPorpertys;

                varItemPro.MethodInfo.MethodName = item.Name;// initData.FunctionName + "_" + item.Id.ToString().Replace("-", "_");
                varItemPro.MethodInfo.DisplayName = item.ShowName;// $"{initData.FunctionName}({initData.UIElement.Initializer.Name})";

                var invokeMethod = initData.EventType.GetMethod("Invoke");
                var methodParams = invokeMethod.GetParameters();
                foreach(var methodParam in methodParams)
                {
                    var funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    funcParam.HostMethodInfo = varItemPro.MethodInfo;
                    funcParam.ParamName = methodParam.Name;
                    funcParam.ParamType = new CodeDomNode.VariableType(methodParam.ParameterType, ctrl.CSType);
                    if(methodParam.IsOut)
                    {
                        varItemPro.MethodInfo.OutParams.Add(funcParam);
                    }
                    else
                    {
                        varItemPro.MethodInfo.InParams.Add(funcParam);
                    }
                }
                if (invokeMethod.ReturnType != typeof(void) && invokeMethod.ReturnType != typeof(System.Threading.Tasks.Task))
                {
                    var funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                    funcParam.HostMethodInfo = varItemPro.MethodInfo;
                    funcParam.ParamName = "Return";
                    if (invokeMethod.ReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                    {
                        var genericType = invokeMethod.ReturnType.GetGenericArguments()[0];
                        funcParam.ParamType = new CodeDomNode.VariableType(genericType, ctrl.CSType);
                        varItemPro.MethodInfo.IsAsync = true;
                    }
                    else
                    {
                        funcParam.ParamType = new CodeDomNode.VariableType(invokeMethod.ReturnType, ctrl.CSType);
                    }
                    varItemPro.MethodInfo.OutParams.Add(funcParam);
                }
                else if (invokeMethod.ReturnType == typeof(System.Threading.Tasks.Task))
                    varItemPro.MethodInfo.IsAsync = true;

                initData.UIElement.Initializer.PropertyChanged += (sender, e) =>
                {
                    switch(e.PropertyName)
                    {
                        case "Name":
                            item.ShowName = $"{initData.FunctionName}({initData.UIElement.Initializer.Name})";
                            varItemPro.MethodInfo.DisplayName = item.ShowName;
                            break;
                    }
                };

                BindingOperations.SetBinding(varItemPro.MethodInfo, CodeDomNode.CustomMethodInfo.TooltipProperty, new Binding("ToolTips") { Source = item, Mode = BindingMode.TwoWay });

                var menuItem = new MenuItem()
                {
                    Name = "UIElementEventOpenGraph",
                    Header = "打开",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    action.Invoke(item);
                };
                if(data.Deleteable)
                {
                    menuItem = new MenuItem()
                    {
                        Name = "UIElementEventDelete",
                        Header = "删除",
                        Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                    };
                    ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                    menuItem.Click += (object sender, RoutedEventArgs e) =>
                    {
                        if (EditorCommon.MessageBox.Show($"即将删除{item.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                        {
                            item.ParentCategory.Items.Remove(item);
                            ctrl.RemoveNodesContainer(item);
                            var fileName = ctrl.GetGraphFileName(item.Name);
                            EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);

                            // 从UIResourceInfo中删除Event标记
                            var key = new UIResourceInfo.UIEventDicKey(initData.UIElementId, initData.FunctionName);
                            HostControl.CurrentResInfo.UIEventsDic.Remove(key);
                        }
                    };
                    item.CategoryItemContextMenu.Items.Add(menuItem);
                }
            }));
            Macross.CategoryItem.RegistInitAction("UI_UIElement_PropertyCustomBind", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                var initData = data as UIElementPropertyCustomBindCategoryitemInitData;
                item.Icon = TryFindResource("Icon_Function") as ImageSource;
                item.Name = initData.FunctionName;
                item.ShowName = $"UIBindFunc_{initData.PropertyName}({initData.UIElement.Initializer.Name})";
                Macross.CategoryItem.Delegate_OnDoubleClick action = async (categoryItem) =>
                {
                    var noUse = await ctrl.ShowNodesContainer(categoryItem);
                };
                item.OnDoubleClick += action;
                if (item.PropertyShowItem == null)
                    item.PropertyShowItem = new UIElementPropertyCustomBindCategoryItemPropertys();
                BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("FunctionName") { Source = initData });

                var varItemPro = item.PropertyShowItem as UIElementPropertyCustomBindCategoryItemPropertys;
                varItemPro.MethodInfo.MethodName = item.Name;
                varItemPro.MethodInfo.DisplayName = item.ShowName;
                BindingOperations.SetBinding(varItemPro.MethodInfo, CodeDomNode.CustomMethodInfo.MethodNameProperty, new Binding("Name") { Source = item });
                initData.UIElement.Initializer.PropertyChanged += (sender, e) =>
                {
                    switch(e.PropertyName)
                    {
                        case "Name":
                            item.ShowName = $"UIBindFuc_{initData.PropertyName}({initData.UIElement.Initializer.Name})";
                            varItemPro.MethodInfo.DisplayName = item.ShowName;
                            break;
                    }
                };

                var funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                funcParam.HostMethodInfo = varItemPro.MethodInfo;
                funcParam.ParamName = "uiElement";
                funcParam.ParamType = new CodeDomNode.VariableType(initData.UIElement.GetType(), ctrl.CSType);
                varItemPro.MethodInfo.InParams.Add(funcParam);
                funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                funcParam.HostMethodInfo = varItemPro.MethodInfo;
                funcParam.ParamName = "value";
                funcParam.ParamType = new CodeDomNode.VariableType(initData.PropertyType, ctrl.CSType);
                varItemPro.MethodInfo.InParams.Add(funcParam);
                
                var menuItem = new MenuItem()
                {
                    Name = "UIElementPropertyCustomBindFuncOpenGraph",
                    Header = "打开",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    action.Invoke(item);
                };
            }));
            Macross.CategoryItem.RegistInitAction("UI_UIElement_VariableBind", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                var initData = data as UIElementVariableBindCategoryItemInitData;
                item.Icon = TryFindResource("Icon_Function") as ImageSource;
                item.Name = initData.FunctionName;
                item.ShowName = $"UIVarBind_({initData.TargetUIElement.Initializer.Name}.{initData.TargetPropertyName})";
                Macross.CategoryItem.Delegate_OnDoubleClick action = async (categoryItem) =>
                {
                    var noUse = await ctrl.ShowNodesContainer(categoryItem);
                };
                item.OnDoubleClick += action;
                if (item.PropertyShowItem == null)
                    item.PropertyShowItem = new UIElementVariableBindCategoryItemPropertys();
                BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("FunctionName") { Source = initData });

                var varItemPro = item.PropertyShowItem as UIElementVariableBindCategoryItemPropertys;
                varItemPro.MethodInfo.MethodName = item.Name;
                varItemPro.MethodInfo.DisplayName = item.ShowName;
                BindingOperations.SetBinding(varItemPro.MethodInfo, CodeDomNode.CustomMethodInfo.MethodNameProperty, new Binding("Name") { Source = item });
                initData.TargetUIElement.Initializer.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case "Name":
                            item.ShowName = $"UIVarBind_({initData.TargetUIElement.Initializer.Name}.{initData.TargetPropertyName})";
                            varItemPro.MethodInfo.DisplayName = item.ShowName;
                            break;
                    }
                };

                var funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                funcParam.HostMethodInfo = varItemPro.MethodInfo;
                funcParam.ParamName = "uiElement";
                funcParam.ParamType = new CodeDomNode.VariableType(initData.UIElement.GetType(), ctrl.CSType);
                varItemPro.MethodInfo.InParams.Add(funcParam);
                funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                funcParam.HostMethodInfo = varItemPro.MethodInfo;
                funcParam.ParamName = "inValue";
                funcParam.ParamType = new CodeDomNode.VariableType(initData.PropertyType, ctrl.CSType);
                varItemPro.MethodInfo.InParams.Add(funcParam);

                var retParam = new CodeDomNode.CustomMethodInfo.FunctionParam();
                retParam.HostMethodInfo = varItemPro.MethodInfo;
                retParam.ParamName = "outValue";
                retParam.ParamType = new CodeDomNode.VariableType(initData.TargetPropertyType, ctrl.CSType);
                varItemPro.MethodInfo.OutParams.Add(retParam);

                var menuItem = new MenuItem()
                {
                    Name = "UIElementVariableBindFuncOpenGraph",
                    Header = "打开",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    action.Invoke(item);
                };
            }));
            Macross.CategoryItem.RegistInitAction("UI_UIElement_CustomEvent", new Action<Macross.CategoryItem, Macross.IMacrossOperationContainer, Macross.CategoryItem.InitializeData>((item, ctrl, data) =>
            {
                var initData = data as UIElementCustomEventCategoryItemInitData;
                item.Icon = TryFindResource("Icon_Function") as ImageSource;
                item.Name = initData.DisplayName;

                if (item.PropertyShowItem == null)
                    item.PropertyShowItem = new UIElementCustomEventCategoryItemPropertys();
                var itemPro = item.PropertyShowItem as UIElementCustomEventCategoryItemPropertys;
                itemPro.Name = initData.DisplayName;
                itemPro.MethodInfo.MethodName = initData.EventName;
                itemPro.MethodInfo.DisplayName = initData.DisplayName;
                itemPro.MethodInfo.CSType = ctrl.CSType;
                itemPro.MethodInfo.IsDelegateEvent = true;
                BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("DisplayName") { Source = itemPro.MethodInfo, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(itemPro, UIElementCustomEventCategoryItemPropertys.NameProperty, new Binding("Name") { Source = item, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(itemPro.MethodInfo, CodeDomNode.CustomMethodInfo.TooltipProperty, new Binding("ToolTips") { Source = item, Mode = BindingMode.TwoWay });

                item.OnNameChangedEvent += async (categoryItem, newValue, oldValue) =>
                {
                    foreach (var insData in categoryItem.InstanceNodes)
                    {
                        var container = await categoryItem.ParentCategory.HostControl.HostControl.GetNodesContainer(insData.ContainerKeyId, true);
                        var node = container.FindControl(insData.NodeId);
                        var param = node.CSParam as CodeDomNode.MethodCustomInvoke.MethodCustomInvokeConstructParam;
                        param.MethodInfo.DisplayName = newValue;
                    }
                };
                item.OnDropToNodesControlAction = (dropData) =>
                {
                    var nodeCtrl = EditorCommon.Program.GetParent(dropData.DropCanvas, typeof(CodeGenerateSystem.Controls.NodesContainerControl)) as CodeGenerateSystem.Controls.NodesContainerControl;
                    var nodeType = typeof(CodeDomNode.MethodCustomInvoke);
                    var csParam = new CodeDomNode.MethodCustomInvoke.MethodCustomInvokeConstructParam();
                    csParam.CSType = ctrl.CSType;
                    csParam.HostNodesContainer = nodeCtrl;
                    csParam.ConstructParam = "";
                    csParam.MethodInfo = itemPro.MethodInfo;
                    var pos = nodeCtrl._RectCanvas.TranslatePoint(dropData.DropPos, nodeCtrl._MainDrawCanvas);
                    var node = nodeCtrl.AddNodeControl(nodeType, csParam, pos.X, pos.Y);
                    node.NodeNameAddShowNodeName = false;
                    item.AddInstanceNode(nodeCtrl.GUID, node);
                };

                var menuItem = new MenuItem()
                {
                    Name = "UI_CustomEvent_Delete",
                    Header = "删除",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Edit_Delete_40x.png", UriKind.Relative)));
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    if (EditorCommon.MessageBox.Show($"即将删除{this.Name}，删除后无法恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                    {
                        item.RemoveFromParent();
                        ctrl.RemoveNodesContainer(item);
                        var fileName = ctrl.GetGraphFileName(this.Name);
                        EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);

                        item.ParentCategory?.HostControl.HostControl.ShowItemPropertys(null);
                    }
                };
                item.CategoryItemContextMenu.Items.Add(menuItem);
            }));
        }

        private void Btn_Compile_Click(object sender, MouseButtonEventArgs e)
        {
            var noUse = CompileCode();
        }
        private void Button_ChangeToDesign_Click(object sender, RoutedEventArgs e)
        {
            HostControl.ChangeToDesign();
        }

        private void RadioButton_Compile_Debug_Checked(object sender, RoutedEventArgs e)
        {
            mCompileType = enCompileType.Debug;
        }
        private void RadioButton_Compile_Release_Checked(object sender, RoutedEventArgs e)
        {
            mCompileType = enCompileType.Release;
        }
        private void Btn_Save_Click(object sender, MouseButtonEventArgs e)
        {
            var noUse = HostControl.Save();
        }
        public async Task Save()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Macross_Client.Save();
            HostControl.CurrentResInfo.CustomFunctions_Client.Clear();
            Macross_Client.CollectFuncDatas(HostControl.CurrentResInfo.CustomFunctions_Client);
        }

        public async Task CompileCode()
        {
            await HostControl.CurrentResInfo.GenerateCode(Macross_Client);
            Macross_Client.PG.Instance = null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CompileStatusIcon = TryFindResource("Good") as ImageSource;
            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);
        }

        internal void SetIsVariable(EngineNS.UISystem.UIElement element, bool setValue)
        {
            var category = Macross_Client.GetCategory(MacrossPanel.UIElementVariableCategoryName);

            var elementType = element.GetType();
            var variables = category.Items.ToArray();
            for(int i=0; i<variables.Length; i++)
            {
                var pro = variables[i].PropertyShowItem as UIElementVariableCategoryItemPropertys;
                if (pro == null)
                    continue;

                if (pro.ElementId == element.Id)
                {
                    if (!setValue)
                        category.Items.Remove(variables[i]);
                    return;
                }
            }

            if(setValue)
            {
                // 没找到则创建一个
                var item = new Macross.CategoryItem(null, category);
                item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Unknow;
                BindingOperations.SetBinding(item, Macross.CategoryItem.NameProperty, new Binding("Name") { Source = element.Initializer, Mode = BindingMode.TwoWay });
                item.CanDrag = true;
                item.InitTypeStr = "UI_UIElement_Variable";
                var data = new UIElementVariableCategoryItemInitData();
                data.Reset();
                data.UIElement = element;
                data.ElementType = elementType;
                data.UIElementId = element.Id;
                data.Deleteable = false;
                item.Initialize(Macross_Client, data);

                category.Items.Add(item);
            }
        }
        internal async Task SetEventFunction(EngineNS.UISystem.UIElement element, string functionName, Type eventType)
        {
            Macross.Category category = Macross_Client.GetCategory(MacrossPanel.UIEventFuncCategoryName);
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Unknow;
            item.CanDrag = true;
            item.InitTypeStr = "UI_UIElement_Event";
            var data = new UIElementEventCategoryItemInitData();
            data.Reset();
            data.UIElement = element;
            data.UIElementId = element.Id;
            data.FunctionName = functionName;
            data.EventType = eventType;
            item.Initialize(Macross_Client, data);
            category.Items.Add(item);
            string dicValue;
            var key = new UIResourceInfo.UIEventDicKey(element.Id, functionName);
            if (!HostControl.CurrentResInfo.UIEventsDic.TryGetValue(key, out dicValue))
            {
                HostControl.CurrentResInfo.UIEventsDic[key] = item.Name;
            }

            // 创建NodesContainer
            var varItemPro = item.PropertyShowItem as UIElementEventCategoryItemPorpertys;
            var nodesContainer = await Macross_Client.CreateNodesContainer(item);
            var nodeType = typeof(CodeDomNode.MethodCustom);
            var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
            {
                CSType = Macross_Client.CSType,
                HostNodesContainer = nodesContainer.NodesControl,
                ConstructParam = "",
                MethodInfo = varItemPro.MethodInfo,
                IsShowProperty = false,
            };
            var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
            node.IsDeleteable = false;
            node.NodeNameAddShowNodeName = false;

            // 有输出参数时才默认添加return
            if (varItemPro.MethodInfo.OutParams.Count > 0)
            {
                var retNodeType = typeof(CodeDomNode.ReturnCustom);
                var retParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                {
                    CSType = Macross_Client.CSType,
                    HostNodesContainer = nodesContainer.NodesControl,
                    ConstructParam = "",
                    MethodInfo = varItemPro.MethodInfo,
                    ShowPropertyType = CodeDomNode.ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                };
                var retNode = nodesContainer.NodesControl.AddOrigionNode(retNodeType, retParam, 300, 0) as CodeDomNode.ReturnCustom;
                retNode.IsDeleteable = false;
            }
            nodesContainer.Save();

            await Macross_Client.ShowNodesContainer(item);
        }
        internal async Task FindEventFunction(EngineNS.UISystem.UIElement element, string functionName)
        {
            var key = new UIResourceInfo.UIEventDicKey(element.Id, functionName);
            string dicValue;
            if(HostControl.CurrentResInfo.UIEventsDic.TryGetValue(key, out dicValue))
            {
                var category = Macross_Client.GetCategory(MacrossPanel.UIEventFuncCategoryName);
                var item = category.FindItem(dicValue);
                if (item != null)
                {
                    await Macross_Client.ShowNodesContainer(item);
                    HostControl.ChangeToLogic();
                }
            }
        }
        internal void DeleteEventFunction(EngineNS.UISystem.UIElement element, string functionName)
        {
            var key = new UIResourceInfo.UIEventDicKey(element.Id, functionName);
            string dicValue;
            if (HostControl.CurrentResInfo.UIEventsDic.TryGetValue(key, out dicValue))
            {
                var category = Macross_Client.GetCategory(MacrossPanel.UIEventFuncCategoryName);
                var item = category.FindItem(dicValue);
                if (item != null)
                {
                    Macross_Client.RemoveNodesContainer(item);
                    category.RemoveItem(dicValue);
                    var fileName = Macross_Client.GetGraphFileName(item.Name);
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                }
                HostControl.CurrentResInfo.UIEventsDic.Remove(key);
            }
        }

        internal async Task SetPropertyCustomBindOperation(EngineNS.UISystem.UIElement uiElement, string bindPropertyName, Type bindPropertyType)
        {
            Macross.Category category = Macross_Client.GetCategory(MacrossPanel.UIBindFuncCategoryName);
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Unknow;
            item.CanDrag = false;
            item.InitTypeStr = "UI_UIElement_PropertyCustomBind";
            var data = new UIElementPropertyCustomBindCategoryitemInitData();
            data.Reset();
            data.UIElement = uiElement;
            data.UIElementId = uiElement.Id;
            data.PropertyName = bindPropertyName;
            data.PropertyType = bindPropertyType;
            data.FunctionName = $"UIBindFunc_{bindPropertyName}_{uiElement.Id.ToString().Replace("-","_")}_{item.Id.ToString().Replace("-", "_")}";// bindFunctionName;
            item.Initialize(Macross_Client, data);
            category.Items.Add(item);
            uiElement.PropertyBindFunctions[bindPropertyName] = data.FunctionName;

            var varItemPro = item.PropertyShowItem as UIElementPropertyCustomBindCategoryItemPropertys;
            var nodesContainer = await Macross_Client.CreateNodesContainer(item);
            var nodeType = typeof(CodeDomNode.MethodCustom);
            var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
            {
                CSType = Macross_Client.CSType,
                HostNodesContainer = nodesContainer.NodesControl,
                ConstructParam = "",
                MethodInfo = varItemPro.MethodInfo,
                IsShowProperty = false,
            };
            var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
            node.IsDeleteable = false;
            node.NodeNameAddShowNodeName = false;
            nodesContainer.Save();

            await Macross_Client.ShowNodesContainer(item);
        }
        internal async Task FindPropertyCustomBindFunc(EngineNS.UISystem.UIElement uiElement, string propertyName)
        {
            string funcName;
            if(uiElement.PropertyBindFunctions.TryGetValue(propertyName, out funcName))
            {
                Macross.Category category = Macross_Client.GetCategory(MacrossPanel.UIBindFuncCategoryName);
                var item = category.FindItem(funcName);
                if (item != null)
                {
                    await Macross_Client.ShowNodesContainer(item);
                    HostControl.ChangeToLogic();
                }
            }
        }
        internal void DelPropertyCustomBindFunc(EngineNS.UISystem.UIElement uiElement, string propertyName)
        {
            string funcName;
            if(uiElement.PropertyBindFunctions.TryGetValue(propertyName, out funcName))
            {
                Macross.Category category = Macross_Client.GetCategory(MacrossPanel.UIBindFuncCategoryName);
                var item = category.FindItem(funcName);
                if(item != null)
                {
                    category.RemoveItem(funcName);
                    Macross_Client.RemoveNodesContainer(item);
                    var fileName = Macross_Client.GetGraphFileName(item.Name);
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                }

                uiElement.PropertyBindFunctions.Remove(propertyName);
            }
        }

        internal async Task SetVariableBindOperation(EngineNS.UISystem.VariableBindInfo bindInfo)
        {
            var category = Macross_Client.GetCategory(MacrossPanel.UIBindFuncCategoryName);
            var bindFromUI = HostControl.mCurrentUIHost.FindChildElement(bindInfo.BindFromUIElementId);
            var bindToUI = HostControl.mCurrentUIHost.FindChildElement(bindInfo.BindToUIElementId);
            // from->to
            if (bindInfo.BindMode == EngineNS.UISystem.enBindMode.OnWayToSource ||
               bindInfo.BindMode == EngineNS.UISystem.enBindMode.TwoWay)
            {
                var item = new Macross.CategoryItem(null, category);
                item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Unknow;
                item.CanDrag = false;
                item.InitTypeStr = "UI_UIElement_VariableBind";
                var data = new UIElementVariableBindCategoryItemInitData();
                data.Reset();

                data.UIElement = bindFromUI;
                data.UIElementId = bindInfo.BindFromUIElementId;
                data.PropertyName = bindInfo.BindFromPropertyName;
                data.PropertyType = bindInfo.BindFromPropertyType;
                data.TargetUIElement = bindToUI;
                data.TargetUIElementId = bindInfo.BindToUIElementId;
                data.TargetPropertyName = bindInfo.VariableName;
                data.TargetPropertyType = bindInfo.BindToVariableType;
                data.FunctionName = $"UIVariableBindFunc_Set_{bindInfo.BindFromUIElementId.ToString().Replace("-", "_")}_{item.Id.ToString().Replace("-", "_")}";
                bindInfo.FunctionName_Set = data.FunctionName;
                item.Initialize(Macross_Client, data);
                category.Items.Add(item);

                var varItemPro = item.PropertyShowItem as UIElementVariableBindCategoryItemPropertys;
                var nodesContainer = await Macross_Client.CreateNodesContainer(item);
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = Macross_Client.CSType,
                    HostNodesContainer = nodesContainer.NodesControl,
                    ConstructParam = "",
                    MethodInfo = varItemPro.MethodInfo,
                    IsShowProperty = false,
                };
                var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;

                var retNodeType = typeof(CodeDomNode.ReturnCustom);
                var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                {
                    CSType = Macross_Client.CSType,
                    HostNodesContainer = nodesContainer.NodesControl,
                    ConstructParam = "",
                    MethodInfo = varItemPro.MethodInfo,
                    ShowPropertyType = CodeDomNode.ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                };
                var retNode = nodesContainer.NodesControl.AddOrigionNode(retNodeType, retCSParam, 500, 0) as CodeDomNode.ReturnCustom;
                retNode.IsDeleteable = false;

                // 相同或可强转类型自动连接
                var nodePins = node.GetLinkPinInfos();
                var retNodePins = retNode.GetLinkPinInfos();
                foreach(var pin in nodePins)
                {
                    if (pin.Visibility != Visibility.Visible)
                        continue;
                    foreach(var retPin in retNodePins)
                    {
                        if (retPin.Visibility != Visibility.Visible)
                            continue;
                        if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(pin, retPin))
                        {
                            var linkInfo = new CodeGenerateSystem.Base.LinkInfo(nodesContainer.NodesControl.GetDrawCanvas(), pin, retPin);
                        }
                    }
                }

                nodesContainer.Save();

                await Macross_Client.ShowNodesContainer(item);
            }

            // to->from
            if(bindInfo.BindMode == EngineNS.UISystem.enBindMode.OnWay ||
               bindInfo.BindMode == EngineNS.UISystem.enBindMode.TwoWay)
            {
                var item = new Macross.CategoryItem(null, category);
                item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Unknow;
                item.CanDrag = false;
                item.InitTypeStr = "UI_UIElement_VariableBind";
                var data = new UIElementVariableBindCategoryItemInitData();
                data.Reset();
                data.UIElement = bindToUI;
                data.UIElementId = bindInfo.BindToUIElementId;
                data.PropertyName = bindInfo.VariableName;
                data.PropertyType = bindInfo.BindToVariableType;
                data.TargetUIElement = bindFromUI;
                data.TargetUIElementId = bindInfo.BindFromUIElementId;
                data.TargetPropertyName = bindInfo.BindFromPropertyName;
                data.TargetPropertyType = bindInfo.BindFromPropertyType;
                data.FunctionName = $"UIVariableBindFunc_Get_{bindInfo.BindToUIElementId.ToString().Replace("-", "_")}_{item.Id.ToString().Replace("-", "_")}";
                bindInfo.FunctionName_Get = data.FunctionName;
                item.Initialize(Macross_Client, data);
                category.Items.Add(item);

                var varItemPro = item.PropertyShowItem as UIElementVariableBindCategoryItemPropertys;
                var nodesContainer = await Macross_Client.CreateNodesContainer(item);
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = Macross_Client.CSType,
                    HostNodesContainer = nodesContainer.NodesControl,
                    ConstructParam = "",
                    MethodInfo = varItemPro.MethodInfo,
                    IsShowProperty = false,
                };
                var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;

                var retNodeType = typeof(CodeDomNode.ReturnCustom);
                var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                {
                    CSType = Macross_Client.CSType,
                    HostNodesContainer = nodesContainer.NodesControl,
                    ConstructParam = "",
                    MethodInfo = varItemPro.MethodInfo,
                    ShowPropertyType = CodeDomNode.ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                };
                var retNode = nodesContainer.NodesControl.AddOrigionNode(retNodeType, retCSParam, 500, 0) as CodeDomNode.ReturnCustom;
                retNode.IsDeleteable = false;

                // 相同或可强转类型自动连接
                bool childLinked = false;
                var nodeChildren = node.GetChildNodes();
                var retNodeChildren = retNode.GetChildNodes();
                foreach(var nodeChild in nodeChildren)
                {
                    foreach(var retNodeChild in retNodeChildren)
                    {
                        foreach (var pin in nodeChild.GetLinkPinInfos())
                        {
                            if (pin.Visibility != Visibility.Visible)
                                continue;
                            foreach (var retPin in retNodeChild.GetLinkPinInfos())
                            {
                                if (retPin.Visibility != Visibility.Visible)
                                    continue;
                                if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(pin, retPin))
                                {
                                    var linkInfo = new CodeGenerateSystem.Base.LinkInfo(nodesContainer.NodesControl.GetDrawCanvas(), pin, retPin);
                                    childLinked = true;
                                }
                            }
                        }
                    }
                }
                if(childLinked)
                {
                    var nodePins = node.GetLinkPinInfos();
                    var retNodePins = retNode.GetLinkPinInfos();
                    foreach (var pin in nodePins)
                    {
                        if (pin.Visibility != Visibility.Visible)
                            continue;
                        foreach (var retPin in retNodePins)
                        {
                            if (retPin.Visibility != Visibility.Visible)
                                continue;
                            if (CodeGenerateSystem.Base.LinkInfo.CanLinkWith(pin, retPin))
                            {
                                var linkInfo = new CodeGenerateSystem.Base.LinkInfo(nodesContainer.NodesControl.GetDrawCanvas(), pin, retPin);
                            }
                        }
                    }
                }

                nodesContainer.Save();

                await Macross_Client.ShowNodesContainer(item);
            }

            List<EngineNS.UISystem.VariableBindInfo> infos;
            if(!bindFromUI.VariableBindInfosDic.TryGetValue(bindInfo.BindFromPropertyName, out infos))
            {
                infos = new List<EngineNS.UISystem.VariableBindInfo>();
                bindFromUI.VariableBindInfosDic[bindInfo.BindFromPropertyName] = infos;
            }
            infos.Add(bindInfo);

            HostControl.ChangeToLogic();
        }

        internal async Task FindVariableBindFunc(EngineNS.UISystem.VariableBindInfo bindInfo)
        {
            var category = Macross_Client.GetCategory(MacrossPanel.UIBindFuncCategoryName);
            var item_Get = category.FindItem(bindInfo.FunctionName_Get);
            if(item_Get != null)
            {
                await Macross_Client.ShowNodesContainer(item_Get);
            }
            var item_Set = category.FindItem(bindInfo.FunctionName_Set);
            if(item_Set != null)
            {
                await Macross_Client.ShowNodesContainer(item_Set);
            }
            HostControl.ChangeToLogic();
        }
        internal void DelVariableBindFunc(EngineNS.UISystem.VariableBindInfo bindInfo)
        {
            var uiElement = HostControl.mCurrentUIHost.FindChildElement(bindInfo.BindFromUIElementId);

            List<EngineNS.UISystem.VariableBindInfo> infos = new List<EngineNS.UISystem.VariableBindInfo>();
            if (uiElement.VariableBindInfosDic.TryGetValue(bindInfo.BindFromPropertyName, out infos))
            {
                var category = Macross_Client.GetCategory(MacrossPanel.UIBindFuncCategoryName);
                var item_Get = category.FindItem(bindInfo.FunctionName_Get);
                if(item_Get != null)
                {
                    category.RemoveItem(bindInfo.FunctionName_Get);
                    Macross_Client.RemoveNodesContainer(item_Get);
                    var fileName = Macross_Client.GetGraphFileName(item_Get.Name);
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                }
                var item_Set = category.FindItem(bindInfo.FunctionName_Set);
                if(item_Set != null)
                {
                    category.RemoveItem(bindInfo.FunctionName_Set);
                    Macross_Client.RemoveNodesContainer(item_Set);
                    var fileName = Macross_Client.GetGraphFileName(item_Set.Name);
                    EngineNS.CEngine.Instance.FileManager.DeleteFile(fileName);
                }

                infos.Remove(bindInfo);
                if (infos.Count == 0)
                    uiElement.VariableBindInfosDic.Remove(bindInfo.BindFromPropertyName);
            }
        }

        public async Task SetObjectToEdit(EngineNS.CRenderContext rc, EditorCommon.Resources.ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Macross_Client.CurrentResourceInfo = HostControl.CurrentResInfo;
            await Macross_Client.Load();
        }
    }
}
