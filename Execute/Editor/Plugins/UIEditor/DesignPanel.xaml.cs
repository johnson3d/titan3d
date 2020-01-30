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

namespace UIEditor
{
    /// <summary>
    /// DesignPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DesignPanel : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        internal MainControl HostControl;

        public EngineNS.UISystem.Controls.UserControl CurrentUI
        {
            get
            {
                if(HostControl.mCurrentUIHost != null && HostControl.mCurrentUIHost.ChildrenUIElements.Count > 0)
                    return HostControl.mCurrentUIHost.ChildrenUIElements[0] as EngineNS.UISystem.Controls.UserControl;
                return null;
            }
        }

        string mSelectedControlName = "";
        public string SelectedControlName
        {
            get => mSelectedControlName;
            set
            {
                mSelectedControlName = value;
                if(mCurrentSelectedUIs != null)
                {
                    if(mCurrentSelectedUIs.Length == 1)
                    {
                        if(mCurrentSelectedUIs[0].Initializer.Name != mSelectedControlName)
                            mCurrentSelectedUIs[0].Initializer.Name = mSelectedControlName;
                    }
                }
                OnPropertyChanged("SelectedControlName");
            }
        }

        public DesignPanel()
        {
            InitializeComponent();

            EngineNS.CEngine.Instance.MacrossDataManager.OnRefreshedMacrossCollector += MacrossDataManager_OnRefreshedMacrossCollector;

            Hierarchy.HostDesignPanel = this;
            DrawPanel.HostDesignPanel = this;
            CtrlsPanel.HostDesignPanel = this;
        }
        void InitMenus()
        {
            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);
        }

        private void MacrossDataManager_OnRefreshedMacrossCollector()
        {
            if (CurrentUI == null)
                return;

            var noUse = RefreshMacrossUIControls();
        }
        async Task RefreshMacrossUIControls()
        {
            await RefreshMacrossUIControl(CurrentUI.ChildrenUIElements);
            BroadcastSelectedUI(null, null);
        }
        async Task RefreshMacrossUIControl(List<EngineNS.UISystem.UIElement> uiElements)
        {
            for(int i=0; i<uiElements.Count; i++)
            {
                var uiElement = uiElements[i];
                if (uiElement == null)
                    return;

                var uiElementType = uiElement.GetType();
                if (uiElementType.GetInterface(typeof(EngineNS.Macross.IMacrossType).FullName) != null)
                {
                    // Macross UI Control
                    var newType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(uiElementType.FullName);
                    var atts = newType.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIControlInitAttribute), false);
                    var newInitType = ((EngineNS.UISystem.Editor_UIControlInitAttribute)atts[0]).InitializerType;
                    var newInit = EngineNS.Rtti.RttiHelper.CreateInstance(newInitType) as EngineNS.UISystem.UIElementInitializer;

                    var oldInitType = uiElement.Initializer.GetType();
                    foreach(var pro in oldInitType.GetProperties())
                    {
                        if (pro.GetCustomAttributes(typeof(EngineNS.Rtti.MetaDataAttribute), false).Length == 0)
                            continue;

                        var newPro = newInitType.GetProperty(pro.Name);
                        if (newPro == null)
                            continue;
                        if (newPro.PropertyType != pro.PropertyType)
                            continue;

                        newPro.SetValue(newInit, pro.GetValue(uiElement.Initializer));
                    }

                    var newUIElement = EngineNS.Rtti.RttiHelper.CreateInstance(newType) as EngineNS.UISystem.UIElement;
                    await newUIElement.Initialize(EngineNS.CEngine.Instance.RenderContext, newInit);
                    var parent = uiElement.Parent;
                    if(parent != null)
                    {
                        parent.RemoveChild(uiElement, false);
                        parent.InsertChild(i, newUIElement);
                    }

                    Hierarchy.ReplaceUIElement(uiElement, newUIElement);
                }
                else
                {
                    var panel = uiElement as EngineNS.UISystem.Controls.Containers.Panel;
                    if (panel != null)
                    {
                        await RefreshMacrossUIControl(panel.ChildrenUIElements);
                    }
                }
            }
        }

        public async Task WaitViewportInitComplated()
        {
            await DrawPanel.WaitViewportInitComplated();
        }

        private void IconTextBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            //CurrentUI.SaveUI();
            var noUse = HostControl.Save();
        }

        public async Task SetObjectToEditor(EngineNS.CRenderContext rc, EditorCommon.Resources.ResourceEditorContext context)
        {
            UpdateUndoRedoKey();
            await DrawPanel.SetObjectToEditor(rc, context);
            await Hierarchy.SetObjectToEditor(rc, context);
            CtrlsPanel.CollectionUIControls();
        }

        EngineNS.UISystem.UIElement[] mCurrentSelectedUIs = null;
        internal void BroadcastSelectedUI(UserControl sourceControl, EngineNS.UISystem.UIElement[] selectedUIs)
        {
            bool isSame = true;
            if (mCurrentSelectedUIs != null && selectedUIs != null && mCurrentSelectedUIs.Length == selectedUIs.Length)
            {
                for (int i = 0; i < mCurrentSelectedUIs.Length; i++)
                {
                    if (mCurrentSelectedUIs[i] != selectedUIs[i])
                    {
                        isSame = false;
                        break;
                    }
                }
            }
            else if (mCurrentSelectedUIs == null && selectedUIs == null)
                isSame = true;
            else
                isSame = false;
            if (isSame)
                return;
            var lastSelectedUIs = mCurrentSelectedUIs;
            var redoAction = new Action<object>(async (obj) =>
            {
                mCurrentSelectedUIs = selectedUIs;
                UpdateSelectUIPropertyGridShow(mCurrentSelectedUIs);

                Hierarchy.OnReceiveSelectUIElements(mCurrentSelectedUIs);
                await DrawPanel.OnReceiveSelectUIs(mCurrentSelectedUIs);
            });
            redoAction?.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redoAction, null,
                                          async (obj) =>
                                          {
                                              mCurrentSelectedUIs = lastSelectedUIs;
                                              UpdateSelectUIPropertyGridShow(mCurrentSelectedUIs);

                                              Hierarchy.OnReceiveSelectUIElements(mCurrentSelectedUIs);
                                              await DrawPanel.OnReceiveSelectUIs(mCurrentSelectedUIs);
                                          }, "Select UIControl");
        }
        public bool? IsVariable
        {
            get { return (bool?)GetValue(IsVariableProperty); }
            set { SetValue(IsVariableProperty, value); }
        }
        public static readonly DependencyProperty IsVariableProperty = DependencyProperty.Register("IsVariable", typeof(bool?), typeof(DesignPanel), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsVariablePropertyChanged)));
        public static void OnIsVariablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as DesignPanel;
            if (!ctrl.mActiveIsVariablePropertyChangeProcess)
                return;

            bool newVal = (bool)e.NewValue;
            for(int i=0; i<ctrl.mCurrentSelectedUIs.Length; i++)
            {
                var uiElement = ctrl.mCurrentSelectedUIs[i];
                uiElement.Initializer.IsVariable = newVal;
                if(newVal && string.IsNullOrEmpty(uiElement.Initializer.Name))
                {
                    EngineNS.UISystem.UIElement root = uiElement;
                    while (root != null)
                    {
                        if (root.Parent == null)
                            break;
                        root = root.Parent;
                    }
                    int idx = 0;
                    ctrl.GetUIElementNewIdx(root, ref idx);
                    uiElement.Initializer.Name = uiElement.GetType().Name + "_" + idx++;
                }
                ctrl.HostControl.SetIsVariable(uiElement, newVal);
            }
        }
        bool mActiveIsVariablePropertyChangeProcess = true;
        internal void UpdateSelectUIPropertyGridShow(EngineNS.UISystem.UIElement[] selectedUIs)
        {
            mActiveIsVariablePropertyChangeProcess = false;
            mEventsShowUIElement = null;
            ShowUIElementEvents();
            if (selectedUIs == null || selectedUIs.Length == 0)
            {
                ProGrid.Instance = null;
                IsVariable = false;
                SelectedControlName = "";
            }
            else
            {
                ProGrid.Instance = selectedUIs;
                if (selectedUIs.Length == 1)
                {
                    SelectedControlName = selectedUIs[0].Initializer.Name;
                    IsVariable = selectedUIs[0].Initializer.IsVariable;
                    mEventsShowUIElement = selectedUIs[0];
                    ShowUIElementEvents();
                }
                else
                {
                    SelectedControlName = "";
                    bool hasMultiVal = false;
                    var firstVal = selectedUIs[0].Initializer.IsVariable;
                    for(int i=1; i<selectedUIs.Length; i++)
                    {
                        if(selectedUIs[i].Initializer.IsVariable != firstVal)
                        {
                            hasMultiVal = true;
                            break;
                        }
                    }
                    if (hasMultiVal)
                        IsVariable = null;
                    else
                        IsVariable = firstVal;
                }
            }
            mActiveIsVariablePropertyChangeProcess = true;
        }

        string mUIElementEventFilterString = "";
        public string UIElementEventFilterString
        {
            get => mUIElementEventFilterString;
            set
            {
                mUIElementEventFilterString = value;
                ShowUIElementEvents();
                OnPropertyChanged("FilterString");
            }
        }
        EngineNS.UISystem.UIElement mEventsShowUIElement;
        void ShowUIElementEvents()
        {
            StackPanel_Events.Children.Clear();
            if (mEventsShowUIElement == null)
                return;

            var filterString = mUIElementEventFilterString.ToLower();
            var uiElementType = mEventsShowUIElement.GetType();
            foreach(var field in uiElementType.GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIEvent), true);
                if (atts.Length == 0)
                    continue;
                if (!string.IsNullOrEmpty(filterString) && !field.Name.ToLower().Contains(filterString))
                    continue;
                if (!field.FieldType.IsSubclassOf(typeof(System.Delegate)))
                    continue;
                var uiEventAtt = atts[0] as EngineNS.UISystem.Editor_UIEvent;
                var item = new UIEventItem();
                item.HostDesignPanel = this;
                item.HostUIElement = mEventsShowUIElement;
                item.Description = uiEventAtt.Description;
                item.EventName = field.Name;
                item.HighLightString = filterString;
                item.EventType = field.FieldType;
                item.IsAdd = true;
                var key = new UIResourceInfo.UIEventDicKey(mEventsShowUIElement.Id, field.Name);
                if (HostControl.CurrentResInfo.UIEventsDic.ContainsKey(key))
                {
                    item.IsAdd = false;
                }
                StackPanel_Events.Children.Add(item);
            }
        }
        class UIDeleteData
        {
            public EngineNS.UISystem.UIElement Parent;
            public int index;
        }
        internal void BroadcastDeleteUIs(UserControl sourceControl, EngineNS.UISystem.UIElement[] processUIs)
        {
            Dictionary<EngineNS.UISystem.UIElement, UIDeleteData> parentDic = new Dictionary<EngineNS.UISystem.UIElement, UIDeleteData>();
            foreach(var ui in processUIs)
            {
                var data = new UIDeleteData();
                data.Parent = ui.Parent;
                data.index = ui.Parent.FindChildIndex(ui);
                parentDic[ui] = data;
            }

            var redoAction = new Action<object>((obj) =>
            {
                foreach (var ui in processUIs)
                {
                    var parent = ui.Parent as EngineNS.UISystem.Controls.Containers.Panel;
                    if (parent == null)
                        continue;

                    if (ui.Initializer.IsVariable)
                        HostControl.SetIsVariable(ui, false);
                    // 删除绑定
                    ClearBinds(ui);
                    parent.RemoveChild(ui);
                }
                mCurrentSelectedUIs = null;

                //if(sourceControl != Hierarchy)
                {
                    Hierarchy.OnReceiveDeleteUIElements(processUIs);
                }
                //if(sourceControl != DrawPanel)
                {
                    DrawPanel.OnReceiveDeleteUIs(processUIs);
                }

                mEventsShowUIElement = null;
                StackPanel_Events.Children.Clear();
                ProGrid.Instance = null;
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redoAction, null,
                                        async (obj) =>
                                        {
                                            var uis = new EngineNS.UISystem.UIElement[parentDic.Count];
                                            int i = 0;
                                            foreach (var uiData in parentDic)
                                            {
                                                UIDeleteData data;
                                                if (!parentDic.TryGetValue(uiData.Key, out data))
                                                    continue;
                                                if (data.index > -1)
                                                {
                                                    data.Parent.InsertChild(data.index, uiData.Key);
                                                }
                                                else
                                                {
                                                    data.Parent.AddChild(uiData.Key);
                                                }
                                                // todo: 恢复绑定
                                                if(uiData.Key.Initializer.IsVariable)
                                                {
                                                    HostControl.SetIsVariable(uiData.Key, true);
                                                }
                                                await DrawPanel.AddPanelRectShow(EngineNS.CEngine.Instance.RenderContext, uiData.Key);
                                                var tempChildren = new EngineNS.UISystem.UIElement[] { uiData.Key };
                                                Hierarchy.OnReceiveAddChildren(data.Parent, tempChildren, data.index);
                                                await DrawPanel.OnReceiveAddChildren(data.Parent, tempChildren, data.index);
                                                uis[i] = uiData.Key;
                                                i++;
                                            }

                                            mCurrentSelectedUIs = uis;
                                            UpdateSelectUIPropertyGridShow(uis);
                                        }, "Delete UIControl");
        }

        void ClearBinds(EngineNS.UISystem.UIElement uiElement)
        {
            // PropertyCustomBind
            var keys = uiElement.PropertyBindFunctions.Keys.ToArray();
            foreach (var key in keys)
            {
                uiElement.PropertyCustomBindRemoveAction(uiElement, key);
            }
            uiElement.PropertyBindFunctions.Clear();
            // EventBind
            foreach(var field in uiElement.GetType().GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.UISystem.Editor_UIEvent), true);
                if (atts.Length == 0)
                    continue;
                if (!field.FieldType.IsSubclassOf(typeof(System.Delegate)))
                    continue;
                var key = new UIResourceInfo.UIEventDicKey(uiElement.Id, field.Name);
                string funcName;
                if (HostControl.CurrentResInfo.UIEventsDic.TryGetValue(key, out funcName))
                {
                    HostControl.DeleteEventFunction(uiElement, field.Name);
                    HostControl.CurrentResInfo.UIEventsDic.Remove(key);
                }
            }
            // VariableBind
            uiElement.VariableBindInfosDic.Clear();
        }

        void GetUIElementNewIdx(EngineNS.UISystem.UIElement element, ref int curIdx)
        {
            try
            {
                var idx = element.Initializer.Name.LastIndexOf("_");
                if (idx >= 0)
                {
                    var idxStr = element.Initializer.Name.Substring(idx + 1);
                    bool hasChar = false;
                    foreach(var idxChar in idxStr)
                    {
                        var idxCharInt = (int)idxChar;
                        if (idxCharInt < 48 || idxCharInt > 58)
                        {
                            hasChar = true;
                            break;
                        }
                    }
                    if(!hasChar)
                    {
                        var tempIdx = System.Convert.ToInt32(idxStr);
                        if (curIdx <= tempIdx)
                            curIdx = tempIdx + 1;
                    }
                }
            }
            catch(System.Exception)
            {

            }

            var panel = element as EngineNS.UISystem.Controls.Containers.Panel;
            if(panel != null)
            {
                for(int i=0; i<panel.ChildrenUIElements.Count; i++)
                {
                    GetUIElementNewIdx(panel.ChildrenUIElements[i], ref curIdx);
                }
            }
        }
        internal void BroadcastAddChildren(UserControl sourceControl, EngineNS.UISystem.UIElement parent, EngineNS.PointF pos, EngineNS.UISystem.UIElement[] children, int insertIndex = -1)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            // 给对象设置名称，名称要保证不重复
            EngineNS.UISystem.UIElement root = parent;
            while(root != null)
            {
                if (root.Parent == null)
                    break;
                root = root.Parent;
            }
            int idx = 0;
            GetUIElementNewIdx(root, ref idx);
            foreach(var element in children)
            {
                bool needUpdateId = false;
                if (element.GetType().GetCustomAttributes(typeof(EngineNS.UISystem.Editor_BaseElementAttribute), true).Length > 0)
                    needUpdateId = true;
                if((element.Initializer.Id == int.MaxValue) || needUpdateId)
                {
                    element.Initializer.Id = Guid.NewGuid().ToString().GetHashCode();
                }
                element.Initializer.Name = element.GetType().Name + "_" + idx++;
            }

            List<EngineNS.UISystem.UIElement> lastSelectedUIS = null;
            if(mCurrentSelectedUIs != null)
                lastSelectedUIS = new List<EngineNS.UISystem.UIElement>(mCurrentSelectedUIs);
            var redoAction = new Action<object>(async (obj) =>
            {
                foreach (var element in children)
                {
                    if (parent is EngineNS.UISystem.Controls.Containers.Panel)
                    {
                        var ctrl = parent as EngineNS.UISystem.Controls.Containers.Panel;
                        if(insertIndex > -1)
                            ctrl.InsertChild(insertIndex, element);
                        else
                            ctrl.AddChild(element);
                        var offsetX = pos.X;// - ctrl.DesignRect.X;
                        var offsetY = pos.Y;// - ctrl.DesignRect.Y;
                        var dr = element.DesignRect;
                        var rect = new EngineNS.RectangleF(offsetX, offsetY, dr.Width, dr.Height);
                        element.Slot.ProcessSetContentDesignRect(ref rect);
                    }

                    if (element.Initializer.IsVariable)
                    {
                        HostControl.SetIsVariable(element, true);
                    }

                    HostControl.SetBindOperationAction(element);
                    await DrawPanel.AddPanelRectShow(rc, element);
                }
                mCurrentSelectedUIs = children;
                //UpdateSelectUIPropertyGridShow(mCurrentSelectedUIs);

                Hierarchy.OnReceiveAddChildren(parent, children, insertIndex);
                await DrawPanel.OnReceiveAddChildren(parent, children, insertIndex);

                mCurrentSelectedUIs = children;
                Hierarchy.OnReceiveSelectUIElements(children);
                await DrawPanel.OnReceiveSelectUIs(children);
                UpdateSelectUIPropertyGridShow(children);
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redoAction, null,
                                        (obj) =>
                                        {
                                            foreach(var element in children)
                                            {
                                                if (element.Initializer.IsVariable)
                                                    HostControl.SetIsVariable(element, false);
                                                parent.RemoveChild(element);
                                            }
                                            if (lastSelectedUIS != null)
                                                mCurrentSelectedUIs = lastSelectedUIS.ToArray();
                                            else
                                                mCurrentSelectedUIs = null;
                                            //UpdateSelectUIPropertyGridShow(mCurrentSelectedUIs);

                                            Hierarchy.OnReceiveDeleteUIElements(children);
                                            DrawPanel.OnReceiveDeleteUIs(children);

                                            Hierarchy.OnReceiveSelectUIElements(mCurrentSelectedUIs);
                                            var noUse = DrawPanel.OnReceiveSelectUIs(mCurrentSelectedUIs);
                                            UpdateSelectUIPropertyGridShow(mCurrentSelectedUIs);
                                        }, "Add UIControl");
        }

        private void Button_ChangeToLogic_Click(object sender, RoutedEventArgs e)
        {
            HostControl.ChangeToLogic();
        }

        #region UndoRedo

        public string UndoRedoKey
        {
            get
            {
                if (HostControl?.CurrentResInfo != null)
                    return HostControl.CurrentResInfo.Id.ToString();
                return "";
            }
        }
        void UpdateUndoRedoKey()
        {
            OnPropertyChanged("UndoRedoKey");
            ProGrid.UndoRedoKey = UndoRedoKey;
        }
        public void ClearUndoRedo()
        {
            EditorCommon.UndoRedo.UndoRedoManager.Instance.ClearCommands(UndoRedoKey);
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitMenus();
        }
    }
}
