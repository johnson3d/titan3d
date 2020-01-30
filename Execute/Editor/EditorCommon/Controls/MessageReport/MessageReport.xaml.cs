using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DockControl;
using EngineNS.IO;

namespace EditorCommon.Controls.MessageReport
{
    public class Message : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        ImageSource mIcon = null;
        public ImageSource Icon
        {
            get { return mIcon; }
            set
            {
                mIcon = value;
                OnPropertyChanged("Icon");
            }
        }

        public EngineNS.Profiler.ELogTag MessageType
        {
            get;
            private set;
        } = EngineNS.Profiler.ELogTag.Info;

        public DateTime Time
        {
            get;
        } = DateTime.Now;

        string mMessageStr = "";
        public string MessageStr
        {
            get { return mMessageStr; }
            set
            {
                mMessageStr = value;
                OnPropertyChanged("MessageStr");
            }
        }

        Brush mMessageBrush = Brushes.White;
        public Brush MessageBrush
        {
            get { return mMessageBrush; }
            set
            {
                mMessageBrush = value;
                OnPropertyChanged("MessageBrush");
            }
        }

        string mCategory = "";
        public string Category
        {
            get { return mCategory; }
            set
            {
                mCategory = value;
                OnPropertyChanged("Category");
            }
        }

        object[] mParams = null;
        public object[] Params
        {
            get { return mParams; }
            set
            {
                mParams = value;
                OnPropertyChanged("Params");
            }
        }

        public Message(MessageReport report, EngineNS.Profiler.ELogTag type, string category, string message, object[] args)
        {
            MessageType = type;
            Category = category;
            MessageStr = message;
            var msgTypeStr = MessageType.ToString();
            Icon = (report.TryFindResource(msgTypeStr + "_Image") as Image).Source;
            var brush = report.TryFindResource(msgTypeStr + "_Brush") as Brush;
            MessageBrush = brush ?? (report.TryFindResource("Default_Brush") as Brush);
        }
    }

    public class FilterData : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        bool mShow = false;
        public bool Show
        {
            get => mShow;
            set
            {
                mShow = value;
                OnPropertyChanged("Show");
            }
        }

        public FilterButton FilterBtn
        {
            get;
            private set;
        }

        public EngineNS.Profiler.ELogTag LogTag
        {
            get;
            private set;
        }

        UInt64 mCount = 0;
        public UInt64 Count
        {
            get => mCount;
            set
            {
                mCount = value;
                OnPropertyChanged("Count");
            }
        }
        MessageReport mHostCtrl;
        public FilterData(MessageReport hostCtrl, EngineNS.Profiler.ELogTag logTag)
        {
            mHostCtrl = hostCtrl;
            FilterBtn = new FilterButton(hostCtrl, logTag);
            FilterBtn.SetBinding(FilterButton.CountProperty, new Binding("Count") { Source = this });
            LogTag = logTag;
        }
    }

    public class MessageData
    {
        public ObservableCollection<Message> MessageCollection
        {
            get;
            private set;
        } = new ObservableCollection<Message>();

        public Dictionary<EngineNS.Profiler.ELogTag, UInt64> CountDictionary
        {
            get;
            private set;
        } = new Dictionary<EngineNS.Profiler.ELogTag, UInt64>();

        public MessageData()
        {
            foreach (EngineNS.Profiler.ELogTag val in Enum.GetValues(typeof(EngineNS.Profiler.ELogTag)))
            {
                CountDictionary[val] = 0;
            }
        }
    }

    /// <summary>
    /// MessageReport.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "Message")]
    [EditorCommon.PluginAssist.PluginMenuItem(Menu.MenuItemDataBase.enMenuItemType.Checkable, new string[] { "Window", "General|Developer Tools", "Log|Output Log" })]
    [Guid("F2D4EF49-D013-4A25-B619-65050B856A5B")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MessageReport : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region EditorPlugin
        public string PluginName
        {
            get { return "Message"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        System.Windows.UIElement mInstructionControl;
        public System.Windows.UIElement InstructionControl
        {
            get { return mInstructionControl; }
        }

        public string Title => "Message Log";

        public ImageSource Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/icon_tab_MessageLog_40x.png", UriKind.Absolute));
        public Brush IconBrush => null;

        public bool OnActive()
        {
            return true;
        }
        public bool OnDeactive()
        {
            return true;
        }

        public async System.Threading.Tasks.Task SetObjectToEdit(EditorCommon.Resources.ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }

        #endregion

        #region Dockable
        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => "Message Report";

        public int Index { get; set; }

        public string DockGroup => "";

        public void Closed() { }

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

        bool? IDockAbleControl.CanClose()
        {
            return true;
        }

        #endregion

        Dictionary<string, MessageData> mMessages = new Dictionary<string, MessageData>();
        ObservableCollection<string> mCategorys = new ObservableCollection<string>();
        Dictionary<EngineNS.Profiler.ELogTag, FilterData> mFilterShow = new Dictionary<EngineNS.Profiler.ELogTag, FilterData>();

        public MessageReport()
        {
            InitializeComponent();

            mInstructionControl = new System.Windows.Controls.TextBlock()
            {
                Text = "编辑器输出信息",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            EngineNS.Profiler.Log.OnReportLog += ShowMessage;
            ListBox_Categroys.ItemsSource = mCategorys;
        }

        void Initialize()
        {
            mFilterShow.Clear();
            foreach (EngineNS.Profiler.ELogTag val in System.Enum.GetValues(typeof(EngineNS.Profiler.ELogTag)))
            {
                var filterData = new FilterData(this, val)
                {
                    Show = false,
                };
                mFilterShow[val] = filterData;
                var tgb = new ToggleButton();
                tgb.SetBinding(ToggleButton.IsCheckedProperty, new Binding("Show") { Source = filterData, Mode=BindingMode.TwoWay });
                tgb.Content = filterData.FilterBtn;
                tgb.Checked += (object sender, RoutedEventArgs e) =>
                {
                    ListBox_Messages.Items.Filter = new Predicate<object>(InfoFilter);
                };
                tgb.Unchecked += (object sender, RoutedEventArgs e) =>
                {
                    ListBox_Messages.Items.Filter = new Predicate<object>(InfoFilter);
                };
            }
        }

        bool InfoFilter(object de)
        {
            var msg = de as Message;
            if (msg == null)
                return false;

            if (mFilterShow.Count == 0)
                return true;
            foreach(var data in mFilterShow)
            {
                if ((data.Value.LogTag == msg.MessageType) && data.Value.Show)
                    return true;
            }

            return false;
        }

        public void ShowMessage(EngineNS.Profiler.ELogTag tag, string category, string format, params object[] args)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(()=>
            {
                MessageData msgCol;
                if (!mMessages.TryGetValue(category, out msgCol))
                {
                    msgCol = new MessageData();
                    mMessages[category] = msgCol;
                    mCategorys.Add(category);
                }

                var msg = new Message(this, tag, category, format, args);
                msgCol.MessageCollection.Add(msg);
                msgCol.CountDictionary[tag]++;
                if (string.Equals(category, ListBox_Categroys.SelectedItem))
                {
                    FilterData data;
                    if (mFilterShow.TryGetValue(tag, out data))
                    {
                        data.Count = msgCol.CountDictionary[tag];
                    }
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }

        private void ListBox_Categroys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox_Categroys.SelectedIndex < 0)
                return;

            var category = (string)ListBox_Categroys.SelectedItem;
            MessageData msg;
            if (mMessages.TryGetValue(category, out msg))
                ListBox_Messages.ItemsSource = msg.MessageCollection;
            else
                ListBox_Messages.ItemsSource = null;
            ListBox_Messages.Items.Filter = new Predicate<object>(InfoFilter);
            foreach(var filter in mFilterShow)
            {
                filter.Value.Count = msg.CountDictionary[filter.Key];
            }
        }
    }
}
