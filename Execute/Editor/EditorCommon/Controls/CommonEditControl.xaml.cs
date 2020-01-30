using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using EngineNS.IO;

namespace EditorCommon.Controls
{
    /// <summary>
    /// CommonEditControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "CommonEditor")]
    [Guid("DEDBEA3B-EE32-4D1F-A029-7A0E6D71183D")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CommonEditControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public string PluginName => "CommonEditor";

        public string Version => "1.0.0";

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CommonEditControl), new FrameworkPropertyMetadata(null));

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/Material_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(CommonEditControl), new FrameworkPropertyMetadata(null));

        public UIElement InstructionControl => throw new NotImplementedException();

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => "CommonEditor";

        public int Index { get; set; }

        public string DockGroup => "";

        public bool OnActive()
        {
            return true;
        }

        public bool OnDeactive()
        {
            return true;
        }

        ResourceEditorContext mContext;
        public async System.Threading.Tasks.Task SetObjectToEdit(ResourceEditorContext context)
        {
            mContext = context;
            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
            ProGrid.Instance = context.PropertyShowValue;

            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }

        public CommonEditControl()
        {
            InitializeComponent();
        }

        private void IconTextBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            mContext?.SaveAction?.Invoke();
        }

        private void IconTextBtn_Browse_Click(object sender, RoutedEventArgs e)
        {
            if (mContext == null || mContext.ResInfo == null)
                return;
            var noUse = EditorCommon.Controls.ResourceBrowser.BrowserControl.ShowResource(mContext.ResInfo.ResourceName);
        }

        private void IconTextBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mContext?.SaveAction?.Invoke();
        }

        public void SaveElement(XmlNode node, XmlHolder holder)
        {
        }

        public IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public void StartDrag()
        {
        }

        public void EndDrag()
        {
        }

        public bool? CanClose()
        {
            return true;
        }

        public void Closed()
        {
        }
    }
}
