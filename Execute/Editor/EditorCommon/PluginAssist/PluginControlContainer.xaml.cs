using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EditorCommon.PluginAssist
{
    /// <summary>
    /// Interaction logic for PluginControlContainer.xaml
    /// </summary>
    public partial class PluginControlContainer : UserControl, EngineNS.ITickInfo, DockControl.IDockAbleControl
    {
        public bool IsShowing { get; set; }
        public string KeyValue
        {
            get
            {
                if (PluginObject != null)
                    return PluginObject.PluginName;

                return "";
            }
        }

        public EditorCommon.Resources.ResourceEditorContext Context;

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(PluginControlContainer),
                                                                                            new PropertyMetadata(false, new PropertyChangedCallback(IsActiveChanged)));
        public static void IsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PluginControlContainer;
            bool newValue = (bool)(e.NewValue);
            if(newValue)
            {
                EngineNS.CEngine.Instance.TickManager.AddTickInfo(ctrl);
                ctrl.PluginObject.OnActive();
            }
            else
            {
                EngineNS.CEngine.Instance.TickManager.RemoveTickInfo(ctrl);
                ctrl.PluginObject.OnDeactive();
            }
        }

        public void SaveElement(EngineNS.IO.XmlNode node, EngineNS.IO.XmlHolder holder)
        {
            if (PluginObject != null)
            {
                var uiElement = PluginObject as System.Windows.FrameworkElement;
                if (uiElement == null)
                    return;

                node.AddAttrib("Type", uiElement.GetType().Assembly.FullName + "|" + uiElement.GetType().FullName);
                node.AddAttrib("GridRow", Grid.GetRow(uiElement).ToString());
                node.AddAttrib("GridColumn", Grid.GetColumn(uiElement).ToString());
                node.AddAttrib("HorizontalAlignment", uiElement.HorizontalAlignment.ToString());
                node.AddAttrib("VerticalAlignment", uiElement.VerticalAlignment.ToString());

                var atts = PluginObject.GetType().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                if(atts.Length > 0)
                {
                    var guidAtt = (System.Runtime.InteropServices.GuidAttribute)atts[0];
                    node.AddAttrib("Id", guidAtt.Value);
                }
            }
        }
        public DockControl.IDockAbleControl LoadElement(EngineNS.IO.XmlNode node)
        {
            DockControl.IDockAbleControl ctr = null;
            if (node == null)
                return ctr;

            var childNode = node.FindNode("Element");
            if (childNode == null)
                return ctr;

            var att = childNode.FindAttrib("Id");
            if (att == null)
                return ctr;

            foreach (var i in EditorCommon.PluginAssist.PluginManager.Instance.PluginsDicWithIdKey.Values)
            {
                if (string.Equals(i.Id.ToString(), att.Value, System.StringComparison.OrdinalIgnoreCase))
                {
                    ctr = Process.GetPluginControl(i.PluginObject);
                    break;
                }
            }

            return ctr;            

            //att = node.FindAttrib("GridRow");
            //if (att != null)
            //    Grid.SetRow(element, System.Convert.ToInt32(att.Value));
            //att = node.FindAttrib("GridColumn");
            //if (att != null)
            //    Grid.SetColumn(element, System.Convert.ToInt32(att.Value));
            //att = node.FindAttrib("HorizontalAlignment");
            //if (att != null)
            //{
            //    HorizontalAlignment alg;
            //    System.Enum.TryParse<HorizontalAlignment>(att.Value, out alg);
            //    element.HorizontalAlignment = alg;
            //}
            //att = node.FindAttrib("VerticalAlignment");
            //if (att != null)
            //{
            //    VerticalAlignment alg;
            //    System.Enum.TryParse<VerticalAlignment>(att.Value, out alg);
            //    element.VerticalAlignment = alg;
            //}
        }

        public void StartDrag()
        {
            var ddop = Content as EditorCommon.PluginAssist.IEditorDockManagerDragDropOperation;
            if (ddop != null)
                ddop.StartDrag();
        }
        public void EndDrag()
        {
            var ddop = Content as EditorCommon.PluginAssist.IEditorDockManagerDragDropOperation;
            if (ddop != null)
                ddop.EndDrag();
        }
        public bool? CanClose()
        {
            return PluginObject?.CanClose();
        }
        public delegate void Delegate_OnClosed();
        public Delegate_OnClosed OnClosed;
        public void Closed()
        {
            PluginObject?.Closed();
            OnClosed?.Invoke();
        }

        public bool TryClose(bool force)
        {
            if (!force && CanClose() == false)
                return false;
            var tabItem = Program.GetParent(this, typeof(DockControl.Controls.DockAbleTabItem)) as DockControl.Controls.DockAbleTabItem;
            tabItem?.Close();
            return true;
        }

        public bool LayoutNeedSaveContent
        {
            get { return true; }
        }

        public string PluginName = "";

        public PluginControlContainer()
        {
            InitializeComponent();
        }
        ~PluginControlContainer()
        {
            Clear();
        }

        EditorCommon.PluginAssist.IEditorPlugin mPluginObject;
        public EditorCommon.PluginAssist.IEditorPlugin PluginObject
        {
            get { return mPluginObject; }
            set
            {
                mPluginObject = value;

                if(mPluginObject is FrameworkElement)
                    this.Content = mPluginObject;
            }
        }

        public int Index { get; set; }
        public bool EnableTick
        {
            get;
            set;
        } = true;

        public string DockGroup => "";

        public void Clear()
        {
            EngineNS.CEngine.Instance.TickManager.RemoveTickInfo(this);
        }

        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            return null;
        }
        public void BeforeFrame()
        {

        }
        void EngineNS.ITickInfo.TickLogic()
        {
            
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var parent = LogicalTreeHelper.GetParent(this);
            if (parent is DockControl.Controls.DockAbleTabItem)
            {
                IsActive = ((DockControl.Controls.DockAbleTabItem)parent).IsSelected;
                BindingOperations.ClearBinding(this, IsActiveProperty);
                BindingOperations.SetBinding(this, IsActiveProperty, new Binding("IsSelected") { Source = parent });
            }
        }

        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            IsActive = false;
            //BindingOperations.ClearBinding(this, IsActiveProperty);
        }

        public void TickRender()
        {
        }

        public void TickSync()
        {
        }
    }
}
