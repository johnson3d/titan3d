using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
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
using DockControl;
using EditorCommon.Resources;

namespace UIEditor
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "UIEditor")]
    [Guid("19271A43-4CE8-4479-8383-EF036F77D35A")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MainControl : UserControl, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region IEditorPlugin
        public string PluginName => "UI编辑器";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MainControl), new FrameworkPropertyMetadata(null));
        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/WidgetBlueprint_64x.png", UriKind.Absolute));
        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MainControl), new FrameworkPropertyMetadata(null));
        public UIElement InstructionControl => new System.Windows.Controls.TextBlock()
        {
            Text = PluginName,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public bool? CanClose()
        {
            if (mCurrentResInfo.IsDirty)
            {
                var result = EditorCommon.MessageBox.Show("该UI还有未保存的更改，是否保存后退出？\r\n(点否后会丢失所有未保存的更改)", EditorCommon.MessageBox.enMessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case EditorCommon.MessageBox.enMessageBoxResult.Yes:
                        var noUse = Save();
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.No:
                        mCurrentResInfo.IsDirty = false;
                        return true;
                    case EditorCommon.MessageBox.enMessageBoxResult.Cancel:
                        return false;
                }
            }

            return true;
        }
        public void Closed()
        {
            DesignPanel.ClearUndoRedo();
        }

        public void StartDrag()
        {
        }
        public void EndDrag()
        {
        }

        public void SaveElement(global::EngineNS.IO.XmlNode node, global::EngineNS.IO.XmlHolder holder)
        {
        }
        public IDockAbleControl LoadElement(global::EngineNS.IO.XmlNode node)
        {
            return null;
        }

        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }

        UIResourceInfo mCurrentResInfo;
        public UIResourceInfo CurrentResInfo
        {
            get => mCurrentResInfo;
        }
        internal EngineNS.UISystem.UIHost mCurrentUIHost;
        public async Task SetObjectToEdit(ResourceEditorContext context)
        {
            await DesignPanel.WaitViewportInitComplated();

            var rc = EngineNS.CEngine.Instance.RenderContext;

            mCurrentResInfo = context.ResInfo as UIResourceInfo;
            var userControl = await EngineNS.CEngine.Instance.UIManager.GetUIAsync(mCurrentResInfo.ResourceName, "editor_ui", true);

            SetBindOperationAction(userControl);
            var hostInit = new EngineNS.UISystem.UIHostInitializer();
            mCurrentUIHost = new EngineNS.UISystem.UIHost();
            await mCurrentUIHost.Initialize(rc, hostInit);
            mCurrentUIHost.ClearChildren();
            mCurrentUIHost.AddChild(userControl);
            mCurrentResInfo.CurrentUI = userControl;

            await DesignPanel.SetObjectToEditor(rc, context);
            await LogicPanel.SetObjectToEdit(rc, context);
        }

        // 设置属性自定义绑定时的操作函数
        internal void SetBindOperationAction(EngineNS.UISystem.UIElement uiElement)
        {
            uiElement.PropertyCustomBindAddAction = (element, propertyName, propertyType) =>
            {
                var noUse = LogicPanel.SetPropertyCustomBindOperation(element, propertyName, propertyType);
            };
            uiElement.PropertyCustomBindFindAction = (element, propertyName) =>
            {
                var noUse = LogicPanel.FindPropertyCustomBindFunc(element, propertyName);
            };
            uiElement.PropertyCustomBindRemoveAction = (element, propertyName) =>
            {
                LogicPanel.DelPropertyCustomBindFunc(element, propertyName);
            };

            uiElement.VariableBindAddAction = (bindInfo) =>
            {
                var noUse = LogicPanel.SetVariableBindOperation(bindInfo);
            };
            uiElement.VariableBindFindAction = (bindInfo) =>
            {
                var noUse = LogicPanel.FindVariableBindFunc(bindInfo);
            };
            uiElement.VariableBindRemoveAction = (bindInfo) =>
            {
                LogicPanel.DelVariableBindFunc(bindInfo);
            };

            var panel = uiElement as EngineNS.UISystem.Controls.Containers.Panel;
            if(panel != null)
            {
                foreach (var child in panel.ChildrenUIElements)
                {
                    SetBindOperationAction(child);
                }
            }
        }

        #endregion

        public MainControl()
        {
            InitializeComponent();

            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(UIResourceInfo));
            DesignPanel.HostControl = this;
            LogicPanel.HostControl = this;

            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(UIEditor.UIMacross.UIElementEventCategoryItemInitData));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(UIEditor.UIMacross.UIElementVariableCategoryItemInitData));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(UIEditor.UIMacross.UIElementPropertyCustomBindCategoryitemInitData));
            EngineNS.CEngine.Instance.MetaClassManager.GetMetaClass(typeof(UIEditor.UIMacross.UIElementVariableBindCategoryItemInitData));

            var template = TryFindResource("HorizontalAlignmentSetter") as DataTemplate;
            WPG.Program.RegisterDataTemplate("HorizontalAlignmentSetter", template);
            template = TryFindResource("VerticalAlignmentSetter") as DataTemplate;
            WPG.Program.RegisterDataTemplate("VerticalAlignmentSetter", template);
            template = TryFindResource("AnchorSelector") as DataTemplate;
            WPG.Program.RegisterDataTemplate("AnchorSelector", template);
        }

        public async Task Save()
        {
            if(mCurrentUIHost != null && mCurrentUIHost.ChildrenUIElements.Count > 0)
            {
//                await mCurrentResInfo.GenerateCode();
                await EngineNS.CEngine.Instance.UIManager.RefreshUI(mCurrentResInfo.ResourceName, "editor_ui");
            }
            await LogicPanel.Save();

            CurrentResInfo.ReferenceRNameList.Clear();
            List<EngineNS.RName> rnames = new List<EngineNS.RName>();
            {
                var codeGenerator = new Macross.CodeGenerator();
                await codeGenerator.CollectMacrossResource(LogicPanel.Macross_Client, rnames);
            }
           
     
            foreach (var rname in rnames)
            {
                CurrentResInfo.ReferenceRNameList.Add(rname);
            }
            await CurrentResInfo.Save(true);

            await LogicPanel.CompileCode();
        }

        internal void ChangeToDesign()
        {
            DesignPanel.Visibility = Visibility.Visible;
            LogicPanel.Visibility = Visibility.Collapsed;
        }
        internal void ChangeToLogic()
        {
            DesignPanel.Visibility = Visibility.Collapsed;
            LogicPanel.Visibility = Visibility.Visible;
        }
        internal void SetIsVariable(EngineNS.UISystem.UIElement element, bool setValue)
        {
            LogicPanel.SetIsVariable(element, setValue);
        }
        internal void SetEventFunction(EngineNS.UISystem.UIElement element, string functionName, Type eventType)
        {
            var noUse = LogicPanel.SetEventFunction(element, functionName, eventType);
        }
        internal void FindEventFunction(EngineNS.UISystem.UIElement element, string functionName)
        {
            var noUse = LogicPanel.FindEventFunction(element, functionName);
        }
        internal void DeleteEventFunction(EngineNS.UISystem.UIElement element, string functionName)
        {
            LogicPanel.DeleteEventFunction(element, functionName);
        }
    }
}
