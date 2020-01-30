using DockControl;
using EditorCommon.Resources;
using EngineNS.IO;
using Macross.ResourceInfos;
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

namespace MacrossEnumEditor
{
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "MacrossEnumEditor")]
    [Guid("A192D498-82B7-434D-B9FC-09C6D1B73E4B")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    /// <summary>
    /// MacrossEnum.xaml 的交互逻辑
    /// </summary>
    public partial class MacrossEnumPanel : UserControl, EditorCommon.PluginAssist.IEditorPlugin, INotifyPropertyChanged
    {
        #region pluginInterface
        public string PluginName
        {
            get { return "MacrossEnumEditor"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        public bool OnActive()
        {
            return true;
        }
        public bool OnDeactive()
        {
            return true;
        }

        public void Tick()
        {

        }

        public object[] GetObjects(object[] param)
        {
            return null;
        }

        public bool RemoveObjects(object[] param)
        {
            return false;
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MacrossEnumPanel), new UIPropertyMetadata(""));
        public ImageSource Icon { get; }
        public Brush IconBrush { get; }

        void IDockAbleControl.StartDrag()
        {
            //throw new NotImplementedException();
        }

        void IDockAbleControl.EndDrag()
        {
            //throw new NotImplementedException();
        }

        bool? IDockAbleControl.CanClose()
        {
            return true;
            //throw new NotImplementedException();
        }

        void IDockAbleControl.Closed()
        {
            //throw new NotImplementedException();
        }

        void IDockAbleControl.SaveElement(XmlNode node, XmlHolder holder)
        {
            throw new NotImplementedException();
        }

        IDockAbleControl IDockAbleControl.LoadElement(XmlNode node)
        {
            throw new NotImplementedException();
        }

        EnumType GEnumType;
        string EnumFileName;
        ResourceInfo CurrentInfo;
        public async Task SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (context.ResInfo == null)
                return;

            CurrentInfo = context.ResInfo;

            SetBinding(TitleProperty, new Binding("ResourceName") { Source = context.ResInfo, Converter = new EditorCommon.Converter.RNameConverter_PureName() });
            GEnumType = new EnumType();

            EnumFileName = context.ResInfo.AbsInfoFileName.Replace(".rinfo", "");
            if (System.IO.File.Exists(EnumFileName + ".xml"))
            {
                GEnumType.LoadEnum(EnumFileName + ".xml");
            }
            else
            {

                //ClassInfo.AddNode(_ClassType.ClassName, _ClassType);

            }
            RefreshEnumTypePropertys();
        }

        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        public UIElement InstructionControl => throw new NotImplementedException();
        #endregion

        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public MacrossEnumResourceInfo HostResourceInfo;
        public MacrossEnumPanel()
        {
            EditorCommon.Resources.ResourceInfoManager.Instance.RegResourceInfo(typeof(MacrossEnumResourceInfo));
            InitializeComponent();
        }

        public string EnumNote
        {
            get
            {
                if (GEnumType == null)
                    return "";
                return GEnumType.EnumNote;
            }
            set
            {
                if (GEnumType == null)
                    return;

                GEnumType.EnumNote = value;
                if (CurrentInfo != null)
                {
                    //CurrentInfo.Describe
                }
                OnPropertyChanged("EnumNote");
            }
        }
        public bool Bitmask
        {
            get
            {
                if (GEnumType == null)
                    return false;
                return GEnumType.Bitmask;
            }
            set
            {
                if (GEnumType == null)
                    return;

                GEnumType.Bitmask = value;
                OnPropertyChanged("Bitmask");
            }
        }
        #region Event
        private void Button_Create(object sender, RoutedEventArgs e)
        {
            if (GEnumType == null)
                return;
            GEnumType.EnumTypePropertys.Add(new EnumType.EnumTypeProperty(GEnumType));
            RefreshEnumTypePropertys();
        }
        private void Button_Save(object sender, RoutedEventArgs e)
        {
            if (GEnumType == null)
                return;

            if (EnumFileName.Equals(""))
                return;

            GEnumType.SaveEnum(EnumFileName);
        }

        //private void TextChanged_Note(object sender, RoutedEventArgs e)
        //{
        //    if (GEnumType == null)
        //        return;

        //    GEnumType.EnumNote = UINote.Text;
        //}

        #endregion

        public void DisenableEnumEditorEnable()
        {
            if (GEnumType == null)
                return;

            if (UIPanel.Children.Count > 0)
            {
                foreach (EnumSetter i in UIPanel.Children)
                {
                    i.UIUp.IsEnabled = false;
                    i.UIDown.IsEnabled = false;
                }
            }
        }

        public void RefreshEnumTypePropertys()
        {
            UIPanel.Children.Clear();

            if (GEnumType == null)
                return;

            for (int i = 0; i < GEnumType.EnumTypePropertys.Count; i ++)
            {
                EnumSetter es = new EnumSetter();
                es.ChangeEnumProperTyIndex -= RefreshEnumTypePropertys;
                es.ChangeEnumProperTyIndex += RefreshEnumTypePropertys;

                es.ChangeEnumValue -= DisenableEnumEditorEnable;
                es.ChangeEnumValue += DisenableEnumEditorEnable;
                es.SetValue(GEnumType.EnumTypePropertys[i]);
                UIPanel.Children.Add(es);
            }

            UINote.Text = GEnumType.EnumNote;
        }
    }
}
