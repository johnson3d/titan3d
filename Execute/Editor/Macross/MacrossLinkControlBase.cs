using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;
using EngineNS;
using Macross.ResourceInfos;

namespace Macross
{
    public class MacrossLinkControlBase : UserControl, INotifyPropertyChanged, IMacrossOperationContainer
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region IMacrossOperationContainer
        public NodesControlAssist NodesCtrlAssist
        {
            get;
            protected set;
        }

        public MacrossPanelBase MacrossOpPanel
        {
            get;
            protected set;
        }

        EngineNS.ECSType mCSType = EngineNS.ECSType.Common;
        public EngineNS.ECSType CSType
        {
            get => mCSType;
            set
            {
                mCSType = value;
                foreach (var nc in mNodesContainerDic)
                {
                    nc.Value.CSType = mCSType;
                }
            }
        }
        public string UndoRedoKey
        {
            get => HostControl?.UndoRedoKey;
        }

        Macross.ResourceInfos.MacrossResourceInfo mCurrentResourceInfo;
        public virtual Macross.ResourceInfos.MacrossResourceInfo CurrentResourceInfo
        {
            get => mCurrentResourceInfo;
            set
            {
                mCurrentResourceInfo = value;
                MacrossOpPanel.SetResourceInfo(value);
            }
        }

        public virtual void ShowNodesContainerEvent(CodeGenerateSystem.Controls.NodesContainerControl ctrl)
        {
        }

        public virtual async Task BindMainGrid(CategoryItem key)
        {
            mNodesContainerDic.Add(key, NodesCtrlAssist);
            NodesCtrlAssist.LinkedKey = key;
            await NodesCtrlAssist.Initialize();
            NodesCtrlAssist.NodesControl.GUID = key.Id;
        }

        public async Task<NodesControlAssist> CreateNodesContainer(Macross.INodesContainerDicKey graphKey, bool IsShow = true)
        {
            var ctrl = new NodesControlAssist();
            ctrl.ShowNodesContainerEvent += this.ShowNodesContainerEvent;
            ctrl.HostControl = this;
            ctrl.LinkedKey = graphKey;
            await ctrl.Initialize();
            //ctrl.LinkedCategoryItemName = graphKey.Name;
            ctrl.SetBinding(NodesControlAssist.LinkedCategoryItemNameProperty, new Binding("Name") { Source = graphKey });
            ctrl.SetBinding(NodesControlAssist.LinkedCategoryItemDisplayNameProperty, new Binding("ShowName") { Source = graphKey });
            mNodesContainerDic.Add(graphKey, ctrl);
            if (!IsShow)
                return ctrl;
            DockControl.Controls.DockAbleTabControl tabCtrl = null;
            DockControl.Controls.DockAbleContainerControl dockContainer = null;
            if (mNodesContainerDic.Count > 0)
            {
                var first = mNodesContainerDic.First().Value;
                var parent = EditorCommon.Program.GetParent(first, typeof(DockControl.Controls.DockAbleTabControl)) as DockControl.Controls.DockAbleTabControl;
                if (parent != null)
                {
                    tabCtrl = parent;
                }
                dockContainer = EditorCommon.Program.GetParent(parent, typeof(DockControl.Controls.DockAbleContainerControl)) as DockControl.Controls.DockAbleContainerControl;
            }
            if (tabCtrl != null)
            {
                var tabItem = new DockControl.Controls.DockAbleTabItem()
                {
                    Content = ctrl,
                };
                tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.HeaderProperty, new Binding("ShowName") { Source = graphKey, Mode = BindingMode.TwoWay });
                tabItem.CanClose += () =>
                {
                    if (ctrl.IsDirty)
                    {
                        var result = EditorCommon.MessageBox.Show($"{graphKey.Name}还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                        switch (result)
                        {
                            case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                                Save();
                                return true;
                            case EditorCommon.MessageBox.enMessageBoxResult.No:
                                return true;
                            case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                                return false;
                        }
                    }
                    return true;
                };
                tabItem.OnClose += () =>
                {
                    mNodesContainerDic.Remove(graphKey);
                };
                tabItem.DockGroup = dockContainer.Group;
                dockContainer.AddChild(tabItem);
            }
            return ctrl;
        }
        public void DeleteNode(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            NodesCtrlAssist.DeleteNode(node);
        }

        public CodeGenerateSystem.Base.BaseNodeControl FindControl(Guid id)
        {
            return NodesCtrlAssist.FindControl(id);
        }

        public virtual string GetGraphFileName(string graphName)
        {
            return $"{CurrentResourceInfo.ResourceName.Address}/link_{graphName}_{CSType.ToString()}{Macross.ResourceInfos.MacrossResourceInfo.MacrossLinkExtension}";
        }

        public Macross.INodesContainerDicKey GetNodesContainerKey(Guid keyId)
        {
            foreach (var category in MacrossOpPanel.CategoryDic.Values)
            {
                foreach (var item in category.Items)
                {
                    var result = GetNodesContainerKeyInCategoryItem(item, keyId);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
        Macross.INodesContainerDicKey GetNodesContainerKeyInCategoryItem(CategoryItem item, Guid keyId)
        {
            if (item.Id == keyId)
                return item;
            if (item.PropertyShowItem != null)
            {
                var result = item.PropertyShowItem.GetNodesContainerDicKey(keyId);
                if (result != null)
                    return result;
            }
            foreach (var child in item.Children)
            {
                var result = GetNodesContainerKeyInCategoryItem(child, keyId);
                if (result != null)
                    return result;
            }
            foreach (var child in item.Children)
            {
                if (child.Id == keyId)
                    return item;
            }
            return null;
        }
        public virtual async Task<NodesControlAssist> GetNodesContainer(Guid containerId, bool createAndLoadWhenNotFound, bool loadAll = false)
        {
            var key = GetNodesContainerKey(containerId);
            if (key == null)
                return null;
            return await GetNodesContainer(key, createAndLoadWhenNotFound, loadAll);
        }

        public virtual async Task<NodesControlAssist> GetNodesContainer(Macross.INodesContainerDicKey key, bool createAndLoadWhenNotFound, bool loadAll = false)
        {
            NodesControlAssist ctrl;
            mNodesContainerDic.TryGetValue(key, out ctrl);

            if (ctrl == null && createAndLoadWhenNotFound)
            {
                ctrl = new NodesControlAssist();
                ctrl.HostControl = this;
                ctrl.CSType = key.CSType;
                //ctrl.LinkedCategoryItemName = graphKey.Name;
                ctrl.SetBinding(NodesControlAssist.LinkedCategoryItemNameProperty, new Binding("Name") { Source = key });
                ctrl.SetBinding(NodesControlAssist.LinkedCategoryItemDisplayNameProperty, new Binding("ShowName") { Source = key });
                ctrl.LinkedKey = key;
                ctrl.LinkedCategoryItemID = key.Id;
                await ctrl.InitializeNodesContainer(ctrl.NodesControl);
                mNodesContainerDic.Add(key, ctrl);

                // 读取Graph
                await ctrl.Load(loadAll);
                //ctrl.CSType = key.CSType;
                ctrl.NodesControl.GUID = key.Id;
            }

            return ctrl;
        }
        public bool RemoveNodesContainer(INodesContainerDicKey key)
        {
            NodesControlAssist ctrl;
            if (mNodesContainerDic.TryGetValue(key, out ctrl))
            {
                mNodesContainerDic.Remove(key);
                var tabItem = EditorCommon.Program.GetParent(ctrl, typeof(DockControl.Controls.DockAbleTabItem)) as DockControl.Controls.DockAbleTabItem;
                var tabCtrl = EditorCommon.Program.GetParent(tabItem, typeof(DockControl.Controls.DockAbleTabControl)) as DockControl.Controls.DockAbleTabControl;
                if (tabCtrl != null && tabItem != null)
                    tabCtrl.Items.Remove(tabItem);
                return true;
            }

            return false;
        }

        public void RemoveOverrideMethod(string methodKeyName)
        {
            MacrossOpPanel.RemoveOverrideMethod(methodKeyName);
        }

        protected WPG.PropertyGrid mPG;
        public virtual void ShowItemPropertys(object item)
        {
            mPG.Instance = item;
        }
        public virtual void OnSelectNull(CodeGenerateSystem.Base.BaseNodeControl node)
        {

        }
        public virtual void OnSelectedLinkInfo(LinkInfo linkInfo)
        {

        }
        public virtual void OnDoubleCliclLinkInfo(LinkInfo linkInfo)
        {

        }
        public virtual void OnSelectNodeControl(CodeGenerateSystem.Base.BaseNodeControl node)
        {
            if (node == null)
                ShowItemPropertys(null);
            else
                ShowItemPropertys(node.GetShowPropertyObject());
        }
        public virtual void OnUnSelectNodes(List<CodeGenerateSystem.Base.BaseNodeControl> nodes)
        {
            ShowItemPropertys(null);
        }
        public virtual async Task<NodesControlAssist> ShowNodesContainer(INodesContainerDicKey graphKey)
        {
            DockControl.Controls.DockAbleTabControl tabCtrl = null;
            DockControl.Controls.DockAbleContainerControl dockContainer = null;
            if (mNodesContainerDic.Count > 0)
            {
                foreach (var data in mNodesContainerDic)
                {
                    var parent = EditorCommon.Program.GetParent(data.Value, typeof(DockControl.Controls.DockAbleTabControl)) as DockControl.Controls.DockAbleTabControl;
                    if (parent == null)
                        continue;

                    tabCtrl = parent;
                    dockContainer = EditorCommon.Program.GetParent(parent, typeof(DockControl.Controls.DockAbleContainerControl)) as DockControl.Controls.DockAbleContainerControl;
                    break;
                }
            }

            var ctrl = await GetNodesContainer(graphKey, true, true);

            if (tabCtrl != null)
            {
                var parentTabItem = EditorCommon.Program.GetParent(ctrl, typeof(DockControl.Controls.DockAbleTabItem)) as DockControl.Controls.DockAbleTabItem;
                if (parentTabItem == null)
                {
                    var tabItem = new DockControl.Controls.DockAbleTabItem()
                    {
                        Content = ctrl,
                    };
                    tabItem.SetBinding(DockControl.Controls.DockAbleTabItem.HeaderProperty, new Binding("ShowName") { Source = graphKey, Mode = BindingMode.TwoWay });
                    tabItem.CanClose += () =>
                    {
                        if (ctrl.IsDirty)
                        {
                            var result = EditorCommon.MessageBox.Show($"{graphKey.Name}还未保存，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                            switch (result)
                            {
                                case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                                    Save();
                                    return true;
                                case EditorCommon.MessageBox.enMessageBoxResult.No:
                                    return true;
                                case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                                    return false;
                            }
                        }
                        return true;
                    };
                    tabItem.OnClose += () =>
                    {
                        mNodesContainerDic.Remove(graphKey);
                    };
                    tabItem.DockGroup = dockContainer.Group;
                    dockContainer.AddChild(tabItem);
                }
                else
                    parentTabItem.IsSelected = true;
            }

            return ctrl;
        }
        #endregion

        MacrossControlBase mHostControl;
        public MacrossControlBase HostControl
        {
            get => mHostControl;
            set
            {
                mHostControl = value;
                BindingOperations.SetBinding(this, TitleProperty, new Binding("Title") { Source = mHostControl });
            }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MacrossLinkControlBase), new UIPropertyMetadata(""));

        public void UpdateUndoRedoKey()
        {
            foreach (var ctrl in mNodesContainerDic.Values)
            {
                ctrl.UpdateUndoRedoKey();
            }
        }

        public MacrossLinkControlBase()
        {
            EngineNS.Editor.Runner.RunnerManager.Instance.OnBreak += RunnerManagerOnBreak;
            EngineNS.Editor.Runner.RunnerManager.Instance.OnResume += RunnerManagerOnResume;
        }

        protected Dictionary<INodesContainerDicKey, NodesControlAssist> mNodesContainerDic = new Dictionary<INodesContainerDicKey, NodesControlAssist>();
        void RunnerManagerOnBreak(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                foreach (var nd in mNodesContainerDic)
                {
                    if (nd.Value.ContainBreakedNode(context))
                    {
                        await ShowNodesContainer(nd.Key);
                        var tabItemParent = EditorCommon.Program.GetParent(nd.Value, typeof(TabItem)) as TabItem;
                        while (tabItemParent != null)
                        {
                            tabItemParent.IsSelected = true;
                            tabItemParent = EditorCommon.Program.GetParent(tabItemParent, typeof(TabItem)) as TabItem;
                        }
                        var win = EditorCommon.Program.GetParent(nd.Value, typeof(Window)) as Window;
                        if (win != null)
                            ResourceLibrary.Win32.BringWindowToTop(new System.Windows.Interop.WindowInteropHelper(win).Handle);
                        nd.Value.SetNodeBreaked(context);
                        break;
                    }
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }
        void RunnerManagerOnResume(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            foreach (var nd in mNodesContainerDic.Values)
            {
                if (nd.DebugResume(context))
                    break;
            }
        }

        public void SaveData(string absFile)
        {
            var xndHoder = EngineNS.IO.XndHolder.NewXNDHolder();
            foreach (var category in MacrossOpPanel.CategoryDic)
            {
                var graphCategoryNode = xndHoder.Node.AddNode("Category_" + category.Key, 0, 0);
                foreach (var item in category.Value.Items)
                {
                    var node = graphCategoryNode.AddNode("itemNode", 0, 0);
                    item.Save(node);
                }
            }
            EngineNS.IO.XndHolder.SaveXND(absFile, xndHoder);
        }
        public virtual void Save()
        {
            if (!EngineNS.CEngine.Instance.FileManager.DirectoryExists(CurrentResourceInfo.ResourceName.Address))
                EngineNS.CEngine.Instance.FileManager.CreateDirectory(CurrentResourceInfo.ResourceName.Address);

            var saveFile = $"{CurrentResourceInfo.ResourceName.Address}/data_{CSType.ToString()}{EngineNS.CEngineDesc.MacrossExtension}";
            SaveData(saveFile);

            foreach (var nodesContainer in mNodesContainerDic)
            {
                nodesContainer.Value.Save();
                //var linkNodesHolder = EngineNS.IO.XndHolder.NewXNDHolder();
                //var linkNode = linkNodesHolder.Node.AddNode("Link", 0, 0);
                //nodesContainer.Value.Save(linkNode);
                //var tempFile = GetGraphFileName(nodesContainer.Key.Name);
                //EngineNS.IO.XndHolder.SaveXND(tempFile, linkNodesHolder);
            }

        }

        public virtual async Task<bool> LoadData(string absFile)
        {
            var xndHolder = await EngineNS.IO.XndHolder.LoadXND(absFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            if (xndHolder != null)
            {
                mNodesContainerDic.Clear();
                CategoryItem mainGraphItem = null;
                foreach (var category in MacrossOpPanel.CategoryDic)
                {
                    category.Value.Items.Clear();
                    var graphCategoryNode = xndHolder.Node.FindNode("Category_" + category.Key);
                    if (graphCategoryNode == null)
                        continue;

                    var itemNodes = graphCategoryNode.GetNodes();
                    foreach (var itemNode in itemNodes)
                    {
                        var item = new CategoryItem(null, category.Value);
                        item.Load(itemNode, this);
                        item.Initialize(this, item.InitData);
                        category.Value.Items.Add(item);

                        switch (item.CategoryItemType)
                        {
                            case CategoryItem.enCategoryItemType.MainGraph:
                                {
                                    mainGraphItem = item;
                                }
                                break;
                        }
                    }
                }

                MacrossOpPanel.SetMainGridItem(mainGraphItem);
                //BindMainGrid(mainGraphItem);

                if (mainGraphItem == null)
                {
                    //throw new InvalidOperationException("MainGraph丢失!");
                }

                xndHolder.Node.TryReleaseHolder();
                return true;
            }
            return false;
        }
        public virtual async Task Load()
        {
            if (CurrentResourceInfo == null)
                return;
            // 读取MainGraph连线
            await NodesCtrlAssist.Load(true);
            var file = $"{CurrentResourceInfo.ResourceName.Address}/data_{CSType.ToString()}{EngineNS.CEngineDesc.MacrossExtension}";
            await LoadData(file);
        }

        public void CollectFuncDatas(List<ResourceInfos.MacrossResourceInfo.CustomFunctionData> datas)
        {
            Category category;
            if (MacrossOpPanel.CategoryDic.TryGetValue(Macross.MacrossPanel.FunctionCategoryName, out category))
            {
                foreach (var item in category.Items)
                {
                    CollectFuncDatas(item, datas);
                }
            }
        }
        void CollectFuncDatas(CategoryItem item, List<ResourceInfos.MacrossResourceInfo.CustomFunctionData> datas)
        {
            var info = item.PropertyShowItem as CodeDomNode.CustomMethodInfo;
            if (info != null)
            {
                var data = new ResourceInfos.MacrossResourceInfo.CustomFunctionData();
                data.Id = item.Id;
                data.MethodInfo = info;
                datas.Add(data);
            }

            foreach (var childItem in item.Children)
            {
                CollectFuncDatas(childItem, datas);
            }
        }

        public virtual bool CheckError()
        {
            bool noError = true;
            foreach (var nd in mNodesContainerDic.Values)
            {
                noError = noError && nd.CheckError();
            }
            return noError;
        }

        public CategoryItem[] GetVariables()
        {
            return MacrossOpPanel.GetVariables();
        }
        public Category GetCategory(string categoryName)
        {
            Category retVal = null;
            MacrossOpPanel.CategoryDic.TryGetValue(categoryName, out retVal);
            return retVal;
        }

        object mCurrentClassInstance;
        public object GetShowMacrossClassPropertyClassInstance()
        {
            try
            {
                var nameSpace = Program.GetClassNamespace(CurrentResourceInfo, CSType);
                if (string.IsNullOrEmpty(nameSpace))
                    return null;
                var className = Program.GetClassName(CurrentResourceInfo, CSType);
                if (string.IsNullOrEmpty(className))
                    return null;
                var retObj = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.CreateInstance(nameSpace + "." + className);
                if (retObj == null)
                    return null;
                // 属性拷贝
                if (mCurrentClassInstance != null)
                {
                    var retObjType = retObj.GetType();
                    // Macross更新后会导致mCurrentClassInstance与retObj的类型不一致，所以这里逐属性拷贝
                    var properties = mCurrentClassInstance.GetType().GetProperties();
                    foreach (var pro in properties)
                    {
                        try
                        {
                            if (!pro.CanWrite)
                                continue;
                            var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                            if (atts.Length == 0)
                            {
                                atts = pro.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), false);
                                if (atts.Length == 0)
                                    continue;
                            }

                            var retPro = retObjType.GetProperty(pro.Name);
                            if (retPro == null || !retPro.CanWrite)
                                continue;
                            if (retPro.PropertyType == pro.PropertyType)
                            {
                                var val = pro.GetValue(mCurrentClassInstance);
                                retPro.SetValue(retObj, val);
                            }
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                }
                mCurrentClassInstance = retObj;
                return mCurrentClassInstance;
            }
            catch(System.Exception)
            {

            }
            return null;
        }
        public void GenerateClassDefaultValues(CodeStatementCollection codeStatementCollection)
        {
            var classInstance = GetShowMacrossClassPropertyClassInstance();
            if (classInstance == null)
                return;
            var curType = classInstance.GetType();
            var baseType = curType.BaseType;
            var baseTypeDefaultValue = System.Activator.CreateInstance(baseType);
            var curTypeVariables = MacrossOpPanel.GetVariables();
            var curTypeVariablesNameDic = new Dictionary<string, CategoryItem>(curTypeVariables.Length);
            foreach (var variable in curTypeVariables)
            {
                curTypeVariablesNameDic.Add(variable.Name, variable);
            }
            var curTypeProperties = MacrossOpPanel.GetProperties();
            var curTypePropertiesNameList = new List<string>(curTypeProperties.Length);
            foreach (var pro in curTypeProperties)
            {
                curTypePropertiesNameList.Add(pro.Name);
            }

            foreach (var curPro in curType.GetProperties())
            {
                try
                {
                    if (!curPro.CanWrite)
                        continue;
                    var atts = curPro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                    if (atts.Length == 0)
                    {
                        atts = curPro.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), false);
                        if (atts.Length == 0)
                            continue;
                    }
                    var browsableAtts = curPro.GetCustomAttributes(typeof(BrowsableAttribute), false);
                    if (browsableAtts.Length > 0)
                    {
                        if (((BrowsableAttribute)browsableAtts[0]).Browsable == false)
                            continue;
                    }
                    var proExp = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), curPro.Name);
                    var basePro = baseType.GetProperty(curPro.Name);
                    if (basePro == null)
                    {
                        // 排除已经删除的
                        if (!curTypeVariablesNameDic.ContainsKey(curPro.Name) && !curTypePropertiesNameList.Contains(curPro.Name))
                            continue;
                        // 排除类型不一样的
                        CategoryItem item;
                        if(curTypeVariablesNameDic.TryGetValue(curPro.Name, out item))
                        {
                            var proItem = item.PropertyShowItem as VariableCategoryItemPropertys;
                            if (proItem.VariableType.GetActualType() != curPro.PropertyType)
                                continue;
                        }

                        // 存储当前类型独有的属性
                        object val = null;
                        try
                        {
                            val = curPro.GetValue(classInstance);
                            if(val == null)
                            {
                                if(curPro.PropertyType.Name == "List`1")
                                {
                                    val = System.Activator.CreateInstance(curPro.PropertyType);
                                }
                            }
                        }
                        finally
                        {
                            if(!(curPro.PropertyType.IsValueType && val==null))
                            {
                                Macross.Program.GenerateSetValueCode(val, curPro.PropertyType, proExp, codeStatementCollection, false);
                            }
                        }
                    }
                    else
                    {
                        var val = curPro.GetValue(classInstance);
                        var baseVal = basePro.GetValue(baseTypeDefaultValue);
                        if (val == null && baseVal == null)
                            continue;
                        if (val != null && val.Equals(baseVal))
                            continue;

                        // 存储与基类不同的值
                        Macross.Program.GenerateSetValueCode(val, curPro.PropertyType, proExp, codeStatementCollection, false);
                    }
                }
                catch
                {

                }
            }
        }
    }
}
