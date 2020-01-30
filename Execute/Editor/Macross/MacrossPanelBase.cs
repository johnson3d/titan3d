using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Macross
{
    public class MacrossPanelBase : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public readonly static string GraphCategoryName = "Graphs";
        public readonly static string FunctionCategoryName = "Functions";
        public readonly static string VariableCategoryName = "Variables";
        public readonly static string FunctionVariableCategoryName = "FunctionVariables";
        public readonly static string PropertyCategoryName = "Property";
        public readonly static string AttributeCategoryName = "Attributes";
        public readonly static string LogicAnimationGraphNodeCategoryName = "AnimationGraph";

        protected ResourceLibrary.Controls.Button.IconTextBtn mAddNewButtonControl;
        public ResourceLibrary.Controls.Button.IconTextBtn AddNewButtonControl => mAddNewButtonControl;

        protected Dictionary<string, Category> mCategoryDic = new Dictionary<string, Category>();
        public Dictionary<string, Category> CategoryDic
        {
            get { return mCategoryDic; }
        }

        public CategoryItem[] GetVariables()
        {
            Category category;
            if (!mCategoryDic.TryGetValue(VariableCategoryName, out category))
                return null;

            return category.Items.ToArray();
        }

        public CategoryItem[] GetProperties()
        {
            Category category;
            if (!mCategoryDic.TryGetValue(PropertyCategoryName, out category))
                return null;

            return category.Items.ToArray();
        }

        public readonly static string MainGraphName = "MainGraph";
        Macross.ResourceInfos.MacrossResourceInfo mCurrentResourceInfo;
        public void SetResourceInfo(Macross.ResourceInfos.MacrossResourceInfo resInfo)
        {
            mCurrentResourceInfo = resInfo;
            //mCurrentResourceInfo.ParentType
        }
        IMacrossOperationContainer mHostControl;
        CategoryItem mMainGridItem;
        public void SetMainGridItem(CategoryItem item)
        {
            if (mMainGridItem != null)
            {
                Category category;
                if (mCategoryDic.TryGetValue(GraphCategoryName, out category))
                {
                    category.Items.Remove(mMainGridItem);
                }
            }
            mMainGridItem = item;
            var noUse = HostControl.BindMainGrid(mMainGridItem);
        }

        public IMacrossOperationContainer HostControl
        {
            get => mHostControl;
            set
            {
                mHostControl = value;

                if (mMainGridItem == null)
                {
                    Category category;
                    if (mCategoryDic.TryGetValue(GraphCategoryName, out category))
                    {
                        mMainGridItem = new CategoryItem(null, category);
                        mMainGridItem.CategoryItemType = CategoryItem.enCategoryItemType.MainGraph;
                        mMainGridItem.Name = MainGraphName;
                        //mMainGridItem.Editable = false;
                        category.Items.Add(mMainGridItem);
                        var data = new CategoryItem.InitializeData();
                        data.Reset();
                        mMainGridItem.Initialize(HostControl, data);
                        var noUse = HostControl.BindMainGrid(mMainGridItem);
                    }
                }
            }
        }
        List<string> mCreatedOverrideMethods = new List<string>();
        public List<string> CreatedOverrideMethods
        {
            get { return mCreatedOverrideMethods; }
        }

        public string GetMethodKeyName(System.Reflection.MethodInfo method)
        {
            return method.Name;//.ToString();
        }
        public string GetMethodKeyName(CodeDomNode.CustomMethodInfo method)
        {
            return method.MethodName;
        }

        protected async Task InitializeOverrideAddNewMenu(MenuItem parentMenu)
        {
            if (mCurrentResourceInfo == null)
                return;

            var type = mCurrentResourceInfo.BaseType;
            parentMenu.Items.Clear();

            List<ResourceInfos.MacrossResourceInfo.CustomFunctionData> customFunctions = new List<ResourceInfos.MacrossResourceInfo.CustomFunctionData>();
            if (mCurrentResourceInfo.BaseTypeIsMacross)
            {
                var resInfo = await Macross.ResourceInfos.MacrossResourceInfo.GetBaseMacrossResourceInfo(mCurrentResourceInfo);
                switch (this.HostControl.CSType)
                {
                    case EngineNS.ECSType.Client:
                        customFunctions = resInfo.CustomFunctions_Client;
                        break;
                    case EngineNS.ECSType.Server:
                        customFunctions = resInfo.CustomFunctions_Server;
                        break;
                }

                // 处理Macross函数的override
                foreach (var func in customFunctions)
                {
                    if (!func.MethodInfo.OverrideAble)
                        continue;
                    var funcName = GetMethodKeyName(func.MethodInfo);
                    if (mCreatedOverrideMethods.Contains(funcName))
                        continue;

                    var menuItem = new MenuItem()
                    {
                        Name = "AddNewMenu_",
                        Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                        Header = GetMethodKeyName(func.MethodInfo),
                        Tag = func,
                    };

                    menuItem.Click += async (object sender, RoutedEventArgs e) =>
                    {
                        Category category;
                        if (mCategoryDic.TryGetValue(FunctionCategoryName, out category))
                        {
                            var item = new CategoryItem(null, category);
                            item.Name = funcName;
                            item.CanDrag = true;
                            item.CategoryItemType = CategoryItem.enCategoryItemType.OverrideFunction;
                            var data = new CategoryItem.InitializeData();
                            data.Reset();
                            item.Initialize(HostControl, data);
                            category.Items.Add(item);

                            var methodInfo = CodeDomNode.Program.GetMethodInfoAssistFromMethodInfo(func.MethodInfo, type, CodeDomNode.MethodInfoAssist.enHostType.Base, "");
                            methodInfo.IsFromMacross = true;
                            methodInfo.FuncId = func.Id;
                            var nodesContainer = await HostControl.CreateNodesContainer(item);
                            var nodeType = typeof(CodeDomNode.MethodOverride);
                            var csParam = new CodeDomNode.MethodOverride.MethodOverrideConstructParam()
                            {
                                CSType = HostControl.CSType,
                                HostNodesContainer = nodesContainer.NodesControl,
                                ConstructParam = "",
                                MethodInfo = methodInfo,
                            };

                            var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
                            node.IsDeleteable = false;

                            // 有输出参数时才默认添加return
                            if(func.MethodInfo.OutParams.Count > 0)
                            {
                                var retNodeType = typeof(CodeDomNode.Return);
                                var retParam = new CodeDomNode.Return.ReturnConstructParam()
                                {
                                    CSType = HostControl.CSType,
                                    HostNodesContainer = nodesContainer.NodesControl,
                                    ConstructParam = "",
                                    MethodInfo = methodInfo,
                                };
                                var retNode = nodesContainer.NodesControl.AddOrigionNode(retNodeType, retParam, 300, 0);
                                retNode.IsDeleteable = false;
                            }
                            nodesContainer.Save();

                            mCreatedOverrideMethods.Add(item.Name);
                        }
                    };
                    parentMenu.Items.Add(menuItem);
                }
            }
            var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                Attribute att = null;
                if(mCurrentResourceInfo.BaseTypeIsMacross)
                    att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(method, typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, true);
                else
                    att = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(method, typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, false);
                if (att == null)
                    continue;
                var methodKeyName = GetMethodKeyName(method);
                if (mCreatedOverrideMethods.Contains(methodKeyName))
                    continue;

                var hasTypeMethod = EngineNS.Rtti.AttributeHelper.GetCustomAttributeMethod(att, "HasType", new Type[] { typeof(EngineNS.Editor.MacrossMemberAttribute.enMacrossType) });
                var has = (bool)(hasTypeMethod.Invoke(att, new object[] { EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Overrideable }));
                if (!has)
                    continue;

                bool isMacrossOverride = false;
                if (mCurrentResourceInfo.BaseTypeIsMacross)
                {
                    var guidAtt = EngineNS.Rtti.AttributeHelper.GetCustomAttribute(method, typeof(EngineNS.Editor.Editor_Guid).FullName, false);
                    if (guidAtt != null)
                    {
                        // 从macross override的函数
                        var guidStr = ((string)EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(guidAtt, "Guid"));
                        var overrideSourceId = EngineNS.Rtti.RttiHelper.GuidTryParse(guidStr);

                        foreach (var func in customFunctions)
                        {
                            if (func.Id == overrideSourceId)
                            {
                                isMacrossOverride = true;
                                break;
                            }
                        }
                    }
                }
                // Macross函数的override单独处理
                if (isMacrossOverride)
                    continue;

                var attrs = method.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), false);
                string header = GetMethodKeyName(method);
                if (attrs.Length > 0)
                {
                    header = header+ "(" + ((System.ComponentModel.DisplayNameAttribute)attrs[0]).DisplayName + ")";
                }

                var menuItem = new MenuItem()
                {
                    Name = "AddNewMenu_",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                    Header = header,
                    Tag = method,
                };

                bool isEvent = true;
                if ((method.ReturnType != typeof(void)) && (method.ReturnType != typeof(System.Threading.Tasks.Task)))
                    isEvent = false;
                else
                {
                    foreach (var param in method.GetParameters())
                    {
                        if (param.IsOut)// || param.ParameterType.IsByRef)
                        {
                            isEvent = false;
                            break;
                        }
                    }
                }

                menuItem.Click += async (object sender, RoutedEventArgs e) =>
                {
                    if (isEvent)    // 无返回值函数，放入Graph  
                    {
                        var item = new CategoryItem(mMainGridItem, mMainGridItem.ParentCategory);
                        item.CanDrag = true;
                        //item.Editable = false;
                        item.CategoryItemType = CategoryItem.enCategoryItemType.Event;
                        
                        var nodesContainer = await HostControl.GetNodesContainer(mMainGridItem, false);
                        if (nodesContainer == null)
                            throw new InvalidOperationException("nodesContainer is null");

                        var nodeType = typeof(CodeDomNode.MethodOverride);
                        var csParam = new CodeDomNode.MethodOverride.MethodOverrideConstructParam();
                        if (attrs.Length > 0)
                        {
                            csParam.DisplayName = ((System.ComponentModel.DisplayNameAttribute)attrs[0]).DisplayName;
                            item.CommentName = csParam.DisplayName;
                        }
                        item.Name = GetMethodKeyName(method);
                        var data = new CategoryItem.InitializeData();
                        data.Reset();
                        item.Initialize(HostControl, data);

                        csParam.CSType = HostControl.CSType;
                        csParam.HostNodesContainer = nodesContainer.NodesControl;
                        csParam.ConstructParam = "";// CodeDomNode.Program.GetParamFromMethodInfo(method, "");
                        csParam.MethodInfo = CodeDomNode.Program.GetMethodInfoAssistFromMethodInfo(method, type, CodeDomNode.MethodInfoAssist.enHostType.Base, "");

                        var center = nodesContainer.NodesControl.GetViewCenter();
                        var node = nodesContainer.NodesControl.AddNodeControl(nodeType, csParam, center.X, center.Y) as CodeDomNode.MethodOverride;
                        node.IsEvent = true;
                        node.IsDeleteable = false;
                        item.AddInstanceNode(nodesContainer.NodesControl.GUID, node);
                        mMainGridItem.Children.Add(item);
                    }
                    else   // 有返回值函数，放入单独的节点
                    {
                        Category category;
                        if (mCategoryDic.TryGetValue(FunctionCategoryName, out category))
                        {
                            var item = new CategoryItem(null, category);
                            item.CanDrag = true;
                            //item.Editable = false;
                            item.CategoryItemType = CategoryItem.enCategoryItemType.OverrideFunction;
                            if (attrs.Length > 0)
                            {
                                item.CommentName = ((System.ComponentModel.DisplayNameAttribute)attrs[0]).DisplayName;
                            }
                            item.Name = GetMethodKeyName(method);
                            var data = new CategoryItem.InitializeData();
                            data.Reset();
                            item.Initialize(HostControl, data);
                            category.Items.Add(item);

                            var methodInfo = CodeDomNode.Program.GetMethodInfoAssistFromMethodInfo(method, type, CodeDomNode.MethodInfoAssist.enHostType.Base, "");
                            var nodesContainer = await HostControl.CreateNodesContainer(item);
                            var nodeType = typeof(CodeDomNode.MethodOverride);
                            var csParam = new CodeDomNode.MethodOverride.MethodOverrideConstructParam()
                            {
                                CSType = HostControl.CSType,
                                HostNodesContainer = nodesContainer.NodesControl,
                                ConstructParam = "",
                                MethodInfo = methodInfo,
                            };

                            if (attrs.Length > 0)
                            {
                                csParam.DisplayName = ((System.ComponentModel.DisplayNameAttribute)attrs[0]).DisplayName;
                            }

                            var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
                            node.IsDeleteable = false;

                            var retNodeType = typeof(CodeDomNode.Return);
                            var retParam = new CodeDomNode.Return.ReturnConstructParam()
                            {
                                CSType = HostControl.CSType,
                                HostNodesContainer = nodesContainer.NodesControl,
                                ConstructParam = "",
                                MethodInfo = methodInfo,
                            };
                            var retNode = nodesContainer.NodesControl.AddOrigionNode(retNodeType, retParam, 300, 0);
                            retNode.IsDeleteable = false;
                            nodesContainer.Save();
                            //var tempFile = $"{HostControl.CurrentResourceInfo.ResourceName.Address}/link_{item.Name}_{HostControl.CSType.ToString()}{Macross.ResourceInfos.MacrossResourceInfo.MacrossLinkExtension}";
                            //var xndHolder = EngineNS.IO.XndHolder.NewXNDHolder();
                            //var linkNode = xndHolder.Node.AddNode("Link", 0, 0);
                            //nodesContainer.Save(linkNode);
                            //EngineNS.IO.XndHolder.SaveXND(tempFile, xndHolder);
                        }
                    }

                    mCreatedOverrideMethods.Add(methodKeyName);
                };
                parentMenu.Items.Add(menuItem);
            }

        }

        bool mInitializedCategorys = false;
        protected void InitializeCategorys(StackPanel categoryPanel)
        {
            if (mInitializedCategorys)
                return;
            mInitializedCategorys = true;

            categoryPanel.Children.Clear();

            var names = new string[] { GraphCategoryName, FunctionCategoryName, VariableCategoryName, PropertyCategoryName, AttributeCategoryName };
            foreach (var name in names)
            {
                var category = new Category(this);
                category.CategoryName = name;
                mCategoryDic.Add(name, category);
                categoryPanel.Children.Add(category);
            }

            // Function variables
            var functionVariableCategory = new Category(this);
            functionVariableCategory.CategoryName = FunctionVariableCategoryName;
            mCategoryDic.Add(FunctionVariableCategoryName, functionVariableCategory);
            categoryPanel.Children.Add(functionVariableCategory);
            functionVariableCategory.Visibility = Visibility.Collapsed;

            foreach (var category in mCategoryDic)
            {
                category.Value.OnSelectedItemChanged = (categoryName) =>
                {
                    foreach (var cName in names)
                    {
                        if (cName == categoryName)
                            continue;

                        Category ctg;
                        if (mCategoryDic.TryGetValue(cName, out ctg))
                        {
                            ctg.UnSelectAllItems();
                        }
                    }
                };
            }
        }
        protected void InitializeCategorys(StackPanel categoryPanel, params string[] categorys)
        {
            if (mInitializedCategorys)
                return;
            mInitializedCategorys = true;

            categoryPanel.Children.Clear();

            foreach (var name in categorys)
            {
                var category = new Category(this);
                category.CategoryName = name;
                mCategoryDic.Add(name, category);
                categoryPanel.Children.Add(category);
            }

            // Function variables
            var functionVariableCategory = new Category(this);
            functionVariableCategory.CategoryName = FunctionVariableCategoryName;
            mCategoryDic.Add(FunctionVariableCategoryName, functionVariableCategory);
            categoryPanel.Children.Add(functionVariableCategory);
            functionVariableCategory.Visibility = Visibility.Collapsed;

            foreach (var category in mCategoryDic)
            {
                category.Value.OnSelectedItemChanged = (categoryName) =>
                {
                    foreach (var cName in categorys)
                    {
                        if (cName == categoryName)
                            continue;

                        Category ctg;
                        if (mCategoryDic.TryGetValue(cName, out ctg))
                        {
                            ctg.UnSelectAllItems();
                        }
                    }
                };
            }
        }

        public void RemoveOverrideMethod(string methodKeyName)
        {
            CreatedOverrideMethods.Remove(methodKeyName);
            Category category;
            if (mCategoryDic.TryGetValue(GraphCategoryName, out category))
            {
                category.RemoveItem(methodKeyName);
            }
        }

        protected void AddVariable(string categoryName)
        {
            Macross.Category category;
            if (!mCategoryDic.TryGetValue(categoryName, out category))
                return;

            int i = 0;
            string newName = "NewVar_";
            bool repetition = false;
            do
            {
                repetition = false;
                foreach (var cItem in category.Items)
                {
                    if (newName + i == cItem.Name)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Variable;
            item.Name = newName + i;
            item.CanDrag = true;
            var data = new Macross.CategoryItem.InitializeData();
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);
        }
        public CategoryItem AddFunctionVariable(string categoryName)
        {
            Category category;
            if (!mCategoryDic.TryGetValue(categoryName, out category))
                return null;

            int i = 0;
            string newName = "NewVar_";
            bool repetition = false;
            do
            {
                repetition = false;
                foreach (var cItem in category.Items)
                {
                    if (newName + i == cItem.Name)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);
            var item = new CategoryItem(null, category);
            item.CategoryItemType = CategoryItem.enCategoryItemType.FunctionVariable;
            item.Name = newName + i;
            item.CanDrag = true;
            var data = new CategoryItem.InitializeData();
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);
            return item;
        }
        protected async Task AddProperty(string categoryName)
        {
            Macross.Category category;
            if (!mCategoryDic.TryGetValue(categoryName, out category))
                return;

            int i = 0;
            string newName = "NewProperty_";
            bool repetition = false;
            do
            {
                repetition = false;
                foreach (var cItem in category.Items)
                {
                    if (newName + i == cItem.Name)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Property;
            item.Name = newName + i;
            item.CanDrag = true;
            var data = new Macross.CategoryItem.InitializeData();
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);

            var itemPro = item.PropertyShowItem as CategoryItemProperty_Property;

            // Get Method
            {
                var nodesContainer_Get = await HostControl.CreateNodesContainer(itemPro.GetMethodNodesKey);
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = HostControl.CSType,
                    HostNodesContainer = nodesContainer_Get.NodesControl,
                    ConstructParam = "",
                    MethodInfo = itemPro.GetMethodInfo,
                    IsShowProperty = false,
                };
                var node = nodesContainer_Get.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;

                var retNodeType = typeof(CodeDomNode.ReturnCustom);
                var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                {
                    CSType = HostControl.CSType,
                    HostNodesContainer = nodesContainer_Get.NodesControl,
                    ConstructParam = "",
                    MethodInfo = itemPro.GetMethodInfo,
                    ShowPropertyType = CodeDomNode.ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
                };
                var retNode = nodesContainer_Get.NodesControl.AddOrigionNode(retNodeType, retCSParam, 500, 0);
                retNode.IsDeleteable = false;
                nodesContainer_Get.Save();
            }
            // Set Method
            {
                var nodesContainer_Set = await HostControl.CreateNodesContainer(itemPro.SetMethodNodesKey);
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = HostControl.CSType,
                    HostNodesContainer = nodesContainer_Set.NodesControl,
                    ConstructParam = "",
                    MethodInfo = itemPro.SetMethodInfo,
                    IsShowProperty = false,
                };
                var node = nodesContainer_Set.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;
                nodesContainer_Set.Save();
            }
        }
        protected async Task AddFunction(string categoryName)
        {
            Macross.Category category;
            if (!mCategoryDic.TryGetValue(categoryName, out category))
                return;

            int i = 0;
            string newName = "NewFunction_";
            bool repetition = false;
            do
            {
                repetition = false;
                foreach (var cItem in category.Items)
                {
                    if (newName + i == cItem.Name)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.CustomFunction;
            item.Name = newName + i;
            item.CanDrag = true;
            var data = new Macross.CategoryItem.InitializeData();
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);

            var miAssist = item.PropertyShowItem as CodeDomNode.CustomMethodInfo;
            var nodesContainer = await HostControl.CreateNodesContainer(item);
            var nodeType = typeof(CodeDomNode.MethodCustom);
            var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
            {
                CSType = HostControl.CSType,
                HostNodesContainer = nodesContainer.NodesControl,
                ConstructParam = "",
                MethodInfo = miAssist,
            };
            var node = nodesContainer.NodesControl.AddOrigionNode(nodeType, csParam, 0, 0);
            node.IsDeleteable = false;
            node.NodeNameAddShowNodeName = false;

            nodesContainer.Save();
        }
        protected void AddGraph(string categoryName)
        {
            Macross.Category category;
            if (!mCategoryDic.TryGetValue(categoryName, out category))
                return;

            int i = 0;
            string newName = "NewGraph_";
            bool repetition = true;
            do
            {
                repetition = false;
                foreach (var cItem in category.Items)
                {
                    if (newName + i == cItem.Name)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);
            var item = new Macross.CategoryItem(null, category);
            item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Graph;
            item.Name = newName + i;
            var data = new Macross.CategoryItem.InitializeData();
            data.Reset();
            item.Initialize(HostControl, data);
            category.Items.Add(item);
        }
        [EngineNS.Rtti.MetaClass]
        public class AttributeCategoryItemInitData : Macross.CategoryItem.InitializeData
        {
            [EngineNS.Rtti.MetaData]
            public AttributeType AttType { get; set; }
        }
        protected void AddAttribute(string categoryName)
        {
            Macross.Category category;
            if (!mCategoryDic.TryGetValue(categoryName, out category))
                return;

            var window = new Macross.CreateAttribute(Macross.CreateAttribute.Attribute.Class);
            window.ShowDialog();
            //window.Dispose();
            if (window.CurrentAttributeTypes == null || window.CurrentAttributeTypes.Count == 0)
                return;

            foreach (var attributetype in window.CurrentAttributeTypes)
            {
                string name = attributetype.AttributeName;

                var item = new Macross.CategoryItem(null, category);
                item.CategoryItemType = Macross.CategoryItem.enCategoryItemType.Atrribute;
                item.Name = name;
                var data = new AttributeCategoryItemInitData();
                data.Reset();
                data.AttType = attributetype;
                item.Initialize(HostControl, data);
                category.Items.Add(item);
            }
        }

        NodesControlAssist.ProcessData mCurrentProcessData = null;
        public virtual void ProcessOnNodesContainerShow(Macross.INodesContainerDicKey key, NodesControlAssist.ProcessData processData)
        {
            if(mCurrentProcessData != null)
            {
                AddNewButtonControl.Items.Remove(mCurrentProcessData.MenuItem_Add);
            }
            mCurrentProcessData = processData;
            // 显示FunctionVariable Category
            Category functionVarCategory;
            if (CategoryDic.TryGetValue(MacrossPanelBase.FunctionVariableCategoryName, out functionVarCategory))
            {
                functionVarCategory.Items.Clear();
                functionVarCategory.Visibility = Visibility.Visible;
                foreach (var item in processData.FunctionVariableCategoryItems)
                {
                    item.SetParentCategory(functionVarCategory);
                    functionVarCategory.Items.Add(item);
                }
            }
            // 菜单显示添加FunctionVariable
            if (processData.MenuItem_Add == null)
            {
                var menuItem = new MenuItem();
                menuItem.Name = "Function_Variable";
                menuItem.Header = "Function Variable";
                ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(menuItem, new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/macross_Variable_add.png", UriKind.Absolute)));
                menuItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    var item = AddFunctionVariable(MacrossPanelBase.FunctionVariableCategoryName);
                    if (item != null)
                    {
                        processData.FunctionVariableCategoryItems.Add(item);
                    }
                };
                processData.MenuItem_Add = menuItem;
            }
            else
            {
                var menuItem = processData.MenuItem_Add.Parent as ItemsControl;
                menuItem?.Items.Remove(processData.MenuItem_Add);
            }
            AddNewButtonControl.Items.Add(processData.MenuItem_Add);
        }
        public virtual void ProcessOnNodesContainerHide(Macross.INodesContainerDicKey key, NodesControlAssist.ProcessData processData)
        {
            // 隐藏FunctionVariable Category
            Category functionVarCategory;
            if (CategoryDic.TryGetValue(Macross.MacrossPanelBase.FunctionVariableCategoryName, out functionVarCategory))
            {
                if(mCurrentProcessData == processData)
                {
                    functionVarCategory.Visibility = Visibility.Collapsed;
                    functionVarCategory.Items.Clear();
                }
            }
            // 菜单隐藏添加FunctionVariable
            AddNewButtonControl.Items.Remove(processData.MenuItem_Add);
        }
    }
}
