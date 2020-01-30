using DockControl;
using EditorCommon.PluginAssist;
using EditorCommon.Resources;
using EngineNS.IO;
using Microsoft.Research.DynamicDataDisplay;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PerformanceAnalyser
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    [EditorCommon.PluginAssist.EditorPlugin(PluginType = "PerformanceAnalyser")]
    [EditorCommon.PluginAssist.PluginMenuItem(EditorCommon.Menu.MenuItemDataBase.enMenuItemType.OneClick, new string[] { "Window", "General|性能分析器" })]
    [Guid("0B8E8A07-739F-4F34-B3B2-6A9D3BE88112")]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MainControl : UserControl, INotifyPropertyChanged, EditorCommon.PluginAssist.IEditorPlugin
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region pluginInterface
        public string PluginName
        {
            get { return "性能分析器"; }
        }
        public string Version
        {
            get { return "1.0.0"; }
        }

        System.Windows.UIElement mInstructionControl = new System.Windows.Controls.TextBlock()
        {
            Text = "性能分析器",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        public System.Windows.UIElement InstructionControl
        {
            get { return mInstructionControl; }
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

        public void SetObjectToEdit(object[] obj)
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
        #endregion

        //////////////////////////////////////////////////////

        string mSearchText = "";
        public string SearchText
        {
            get { return mSearchText; }
            set
            {
                mSearchText = value;

                var lowerText = mSearchText.ToLower();
                foreach(var item in mItems)
                {
                    UpdateVisibleWithSearchText(item, lowerText);
                }

                OnPropertyChanged("SearchText");
            }
        }

        bool UpdateVisibleWithSearchText(PerformanceItem item, string searchText)
        {
            bool contains = false;
            foreach (var childItem in item.Childrens)
            {
                var value = UpdateVisibleWithSearchText(childItem, searchText);
                contains = contains || value;
            }

            if(string.IsNullOrEmpty(searchText))
            {
                item.Visibility = Visibility.Visible;
                item.HighLightString = "";
                return true;
            }
            else
            {
                if(contains)
                {
                    item.IsExpanded = true;
                }
                if (item.ItemName.ToLower().Contains(searchText))
                {
                    item.Visibility = Visibility.Visible;
                    item.HighLightString = searchText;
                    contains = true;
                }
                else if(!contains)
                {
                    item.Visibility = Visibility.Collapsed;
                    item.HighLightString = "";
                }
                else
                {
                    item.HighLightString = "";
                }
            }

            return contains;
        }

        public MainControl()
        {
            try
            {
                InitializeComponent();

                plotter_Graph.Children.Add(new Microsoft.Research.DynamicDataDisplay.Charts.Navigation.CursorCoordinateGraph());

                mStartTime = DateTime.Now;
                //MemoryInfoListView.ItemsSource = MemoryInfoList;
                //dataGrid.ItemsSource = SelectedMemoryInfo;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }

        bool mInitialized = false;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (mInitialized)
                return;

            InitializeTreeView();
            InitializePerformanceSettings();
            ProGrid.Instance = EngineNS.CEngine.Instance.Stat;

            mInitialized = true;
        }

        ObservableCollection<PerformanceItem> mItems = new ObservableCollection<PerformanceItem>();
        List<PerformanceItem> mListenedItems = new List<PerformanceItem>();

        ObservableCollection<SpecialInterest> mSpecialInterests = new ObservableCollection<SpecialInterest>();

        void InitializeTreeView()
        {
            var rn = EngineNS.RName.GetRName("GameTable/perfview.cfg");
            var xmlHolder = EngineNS.IO.XmlHolder.LoadXML(rn.Address);
            if (xmlHolder == null)
                return;
            LoadPerformanceTree(xmlHolder.RootNode, null, "");

            TreeView_Items.ItemsSource = mItems;
            ListView_SpInterest.ItemsSource = mSpecialInterests;
        }

        void InitializePerformanceSettings()
        {
            PerformanceSettingPanel.Children.Clear();
            //var enumType = typeof(CCore.Performance.PerformanceSetting.EPerfSettingType);
            //foreach (var val in System.Enum.GetValues(enumType))
            //{
            //    var checkBox = new CheckBox()
            //    {
            //        Content = val.ToString(),
            //        Foreground = Brushes.White,
            //        Tag = val,
            //    };
            //    var fileInfo = enumType.GetField(val.ToString());
            //    if(fileInfo != null)
            //    {
            //        var att = (DescriptionAttribute)(Attribute.GetCustomAttribute(fileInfo, typeof(DescriptionAttribute)));
            //        if(att != null)
            //            checkBox.ToolTip = att.Description;
            //    }
            //    checkBox.Checked += (sender, e) =>
            //    {
            //        var cb = sender as CheckBox;
            //        var value = (CCore.Performance.PerformanceSetting.EPerfSettingType)(cb.Tag);
            //        if(mListeningLocal)
            //            CCore.Performance.PerformanceSetting.ChangePerformanceValue(value, true);
            //        else
            //            CCore.Performance.PerformanceSetting.GM_ChangeTargetPerfValue(GUIDTargetAccount, value, true);
            //    };
            //    checkBox.Unchecked += (sender, e) =>
            //    {
            //        var cb = sender as CheckBox;
            //        var value = (CCore.Performance.PerformanceSetting.EPerfSettingType)(cb.Tag);
            //        if(mListeningLocal)
            //            CCore.Performance.PerformanceSetting.ChangePerformanceValue(value, false);
            //        else
            //            CCore.Performance.PerformanceSetting.GM_ChangeTargetPerfValue(GUIDTargetAccount, value, false);
            //    };
            //    PerformanceSettingPanel.Children.Add(checkBox);
            //}
        }

        void LoadPerformanceTree(EngineNS.IO.XmlNode parentNode, PerformanceItem parent, string inheritName)
        {
            foreach(var node in parentNode.GetNodes())
            {
                PerformanceItem item = new PerformanceItem_Parent();

                if (node.GetAttribs().Count > 0)
                {
                    string desc = "";
                    var att = node.FindAttrib("Desc");
                    if (att != null)
                        desc = att.Value;
                    att = node.FindAttrib("Value");
                    if(att != null)
                    {
                        var pf = new PerformanceItem_PerfCount();
                        pf.KeyName = att.Value;
                        pf.ToolTip = desc;
                        //item.onav
                        item = pf;
                    }
                    att = node.FindAttrib("Variable");
                    if(att != null)
                    {
                        var nor = new PerformanceItem_Data();
                        nor.KeyName = att.Value;

                        var viewer = EngineNS.CEngine.Instance.Stat.PViewer.CreateViewer(nor.KeyName);
                        if (viewer == null)
                            continue;

                        Func<string[]> getValueAction = viewer.GetValueAction;
                        Func<string[]> getNameAction = viewer.GetValueNameAction;

                        if (getValueAction == null)
                        {
                            nor.ListeningVisible = Visibility.Collapsed;
                        }
                        nor.ToolTip = desc;
                        item = nor;
                    }
                }

                item.Plotter = plotter_Graph;
                item.OnListeningChanged = (listening) =>
                {
                    if (listening)
                    {
                        if (!mListenedItems.Contains(item))
                            mListenedItems.Add(item);
                    }
                    else
                    {
                        mListenedItems.Remove(item);
                    }
                };
                item.ItemName = node.Name;
                item.InheritName = inheritName;

                if (parent == null)
                    mItems.Add(item);
                else
                {
                    parent.Childrens.Add(item);
                    item.Parent = parent;
                }

                LoadPerformanceTree(node, item, inheritName + "|" + node.Name);
            }
        }

        private void Button_ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in mItems)
            {
                ExpandItem(child as PerformanceItem, true);
            }
        }
        private void ExpandItem(PerformanceItem item, bool expand)
        {
            if (item == null)
                return;

            item.IsExpanded = expand;
            foreach (var child in item.Childrens)
            {
                ExpandItem(child as PerformanceItem, expand);
            }
        }
        private void Button_FoldAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in mItems)
            {
                ExpandItem(child as PerformanceItem, false);
            }
        }

        PerformanceItem mCurrentSelected = null;
        private void TreeView_Items_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = TreeView_Items.SelectedItem as PerformanceItem;
            if (mCurrentSelected != null)
            {
                mCurrentSelected.Selected = false;
                mCurrentSelected = null;
            }

            mCurrentSelected = item;
            if(mCurrentSelected != null)
            {
                mCurrentSelected.Selected = true;
            }
        }

        #region 监听功能

        Guid mGUIDTargetAccount = Guid.Empty;
        public Guid GUIDTargetAccount
        {
            get { return mGUIDTargetAccount; }
            set
            {
                mGUIDTargetAccount = value;
                OnPropertyChanged("GUIDTargetAccount");
            }
        }

        public string TargetAccount
        {
            get
            {
                if(EngineNS.CEngine.Instance.RemoteServices!=null)
                    return EngineNS.CEngine.Instance.RemoteServices.ProfilerTarget;
                return "";
            }
            set
            {
                EngineNS.CEngine.Instance.RemoteServices.ProfilerTarget = value;
                OnPropertyChanged("TargetAccount");
            }
        }

        string IEditorPlugin.PluginName => "PerfViewer";

        string IEditorPlugin.Version => "1.0.0";

        string IEditorPlugin.Title => "PerfViewer";

        ImageSource IEditorPlugin.Icon => new BitmapImage(new System.Uri("pack://application:,,,/ResourceLibrary;component/Icons/Icons/AssetIcons/WidgetBlueprint_64x.png", UriKind.Absolute));

        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }
        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register("IconBrush", typeof(Brush), typeof(MainControl), new FrameworkPropertyMetadata(null));


        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue => PluginName;

        public int Index { get; set; }

        public string DockGroup => "";

        bool mListeningLocal = true;
        bool mIsListening = false;
        System.Threading.Thread mTimerTickThread;
        string[] mListenedNames;
        private void Button_Query_Click(object sender, RoutedEventArgs e)
        {
            if (TargetAccount != "")
            {
                mListeningLocal = false;
                Guid.TryParse(TargetAccount, out mGUIDTargetAccount);
                ChangePerfSwitch();
            }
            else
            {
                mListeningLocal = true;
                ChangePerfSwitch();
            }
        }
        private void ChangePerfSwitch()
        {
            mIsListening = !mIsListening;
            if (mIsListening)
            {
                mListenedNames = new string[mListenedItems.Count];
                for (int i = 0; i < mListenedNames.Length; i++)
                {
                    mListenedNames[i] = mListenedItems[i].KeyName;
                    mListenedItems[i].ClearDataSource();
                }

                mStartTime = DateTime.Now;
                if (mListeningLocal)
                {
                    mTimerTickThread = new System.Threading.Thread(Timer_Tick);
                    mTimerTickThread.IsBackground = true;
                    mTimerTickThread.Name = "Performance Analyser Tick Thread";
                    RunSpyThread = true;
                    mTimerTickThread.Start();
                }
                else
                {
                    EngineNS.CEngine.Instance.RemoteServices.OnReciveReportPerfCounter = this.OnReciveReportPerfCounter;
                }
                this.Dispatcher.BeginInvoke(new Action(() => 
                {
                    Button_Query.Content = "结束监听";
                    Button_Query.Foreground = Brushes.Red;
                    if (EngineNS.CEngine.Instance.GameInstance == null)
                        EngineNS.CEngine.Instance.MacrossDataManager.CleanDebugContextGCRefrence();
                    System.GC.Collect();
                }));
            }
            else
            {
                if (mListeningLocal)
                {
                    if (mTimerTickThread!=null && mTimerTickThread.IsAlive)
                    {
                        RunSpyThread = false;
                        while(mTimerTickThread.IsAlive)
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    }

                    for (byte i = 0; i < mListenedNames.Length; i++)
                    {
                        if (mListenedItems[i] is PerformanceItem_PerfCount)
                        {
                            //var perf = EngineNS.Profiler.TimeScopeManager.GetTimeScope(mListenedNames[i], false);
                            //if (perf != null)
                            //    perf.Enable = false;
                        }
                        else if (mListenedItems[i] is PerformanceItem_Data)
                        {
                            var pfView = EngineNS.CEngine.Instance.Stat.PViewer.GetViewer(mListenedNames[i]);
                            if (pfView != null)
                            {
                                var pc = mListenedItems[i] as PerformanceItem_Data;
                                pc.UpdateData(0, pfView);
                            }
                        }
                    }
                }
                else
                {
                    EngineNS.CEngine.Instance.RemoteServices.OnReciveReportPerfCounter = null;
                }
                this.Dispatcher.BeginInvoke(new Action(() => 
                {
                    Button_Query.Content = "开始监听";
                    Button_Query.Foreground = Brushes.Lime;
                }));
            }
        }
        public PerformanceItem_PerfCount FindPerf(string name)
        {
            foreach (var i in mListenedItems)
            {
                if (i is PerformanceItem_PerfCount)
                {
                    if (i.KeyName == name)
                        return i as PerformanceItem_PerfCount;
                }
            }
            return null;
        }
        public PerformanceItem_PerfCount FindPerf(UInt32 nameHash)
        {
            foreach (var i in mListenedItems)
            {
                if (i is PerformanceItem_PerfCount)
                {
                    if (EngineNS.UniHash.APHash(i.KeyName) == nameHash)
                        return i as PerformanceItem_PerfCount;
                }
            }
            return null;
        }
        public PerformanceItem_Data FindData(string name)
        {
            foreach (var i in mListenedItems)
            {
                if (i is PerformanceItem_Data)
                {
                    if (i.KeyName == name)
                        return i as PerformanceItem_Data;
                }
            }
            return null;
        }
        private void OnReciveReportPerfCounter(string reporterName, List<EngineNS.Profiler.PerfViewer.PfValue_PerfCounter> scopes, List<EngineNS.Profiler.PerfViewer.PfValue_Data> datas)
        {
            if (TargetAccount != reporterName)
                return;
            if (scopes == null)
                return;

            var now = DateTime.Now;
            var delta = now - mStartTime;
            for(int i=0; i< scopes.Count; i++)
            {
                var perf = FindPerf(scopes[i].NameHash);
                if (perf == null)
                    continue;
                perf.UpdateData((UInt64)delta.TotalSeconds, scopes[i]);
            }
            if (datas == null)
            {
                return;
            }
            for (int i = 0; i < datas.Count; i++)
            {
                var perf = FindData(datas[i].Name);
                if (perf == null)
                    continue;
                EngineNS.Profiler.PerfViewer.Viewer viewer = null;
                foreach (var j in EngineNS.CEngine.Instance.Stat.PViewer.Datas)
                {
                    if (j.Name == datas[i].Name)
                    {
                        viewer = j;
                        break;
                    }
                }
                if (viewer == null)
                    continue;

                datas[i].ValueNames = new List<string>(viewer.GetValueNameAction());
                perf.UpdateData((UInt64)delta.TotalSeconds, datas[i]);
            }
        }
        UInt64 mTime = 0;
        DateTime mStartTime;
        bool RunSpyThread = false;
        private void Timer_Tick()
        {
            while (RunSpyThread)
            {
                try
                {
                    var now = DateTime.Now;
                    var delta = now - mStartTime;
                    if (delta.TotalSeconds >= 1)
                    {
                        if (mListenedNames.Length != mListenedItems.Count)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Button_Query_Click(null, new RoutedEventArgs());
                            }));
                            return;
                        }

                        for (byte i = 0; i < mListenedNames.Length; i++)
                        {
                            if (mListenedItems[i] is PerformanceItem_PerfCount)
                            {
                                var pc = mListenedItems[i] as PerformanceItem_PerfCount;
                                var perf = EngineNS.Profiler.TimeScopeManager.GetTimeScope(mListenedNames[i], EngineNS.Profiler.TimeScope.EProfileFlag.FlagsAll, false);
                                if (perf == null)
                                    continue;
                                if (!perf.Enable)
                                    perf.Enable = true;
                                pc.UpdateData((UInt64)delta.TotalSeconds, perf);
                            }
                            else if (mListenedItems[i] is PerformanceItem_Data)
                            {
                                var pfView = EngineNS.CEngine.Instance.Stat.PViewer.GetViewer(mListenedNames[i]);
                                var pc = mListenedItems[i] as PerformanceItem_Data;
                                pc.UpdateData((UInt64)delta.TotalSeconds, pfView);
                            }
                        }

                        mTime++;
                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.ToString());
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace.ToString());
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        #endregion

        private void Button_AddToInterest_Click(object sender, RoutedEventArgs e)
        {
            if(TreeView_Items.SelectedItem == null)
            {
                EditorCommon.MessageBox.Show("未选择任何对象, 请先在所有监控中选择一个，然后再进行添加!");
                return;
            }
            var item = TreeView_Items.SelectedItem as PerformanceItem;
            if(item != null)
            {
                var data = new SpecialInterest();
                data.Data = item;
                data.Name = item.ItemName;
                var parent = item.Parent;
                while(parent != null)
                {
                    data.Name = parent.ItemName + "/" + data.Name;
                    parent = parent.Parent;
                }
                mSpecialInterests.Add(data);
            }
        }

        private void Button_RemoFromInterest_Click(object sender, RoutedEventArgs e)
        {
            if(ListView_SpInterest.SelectedIndex < 0)
            {
                EditorCommon.MessageBox.Show("未选择任何对象, 请先在特别关注中选择一个进行移除!");
                return;
            }

            mSpecialInterests.RemoveAt(ListView_SpInterest.SelectedIndex);
        }

        public void SetChildrenListening(bool listening, PerformanceItem item, bool withChildChildren)
        {
            foreach (var c in item.Childrens) 
            {
                c.Listening = listening;
                if (withChildChildren)
                {
                    SetChildrenListening(listening, c, withChildChildren);
                }
            }
        }
        private void MenuItem_CheckAllChildren_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var element = sender as System.Windows.FrameworkElement;
            if(element != null)
            {
                var item = element.DataContext as PerformanceItem;
                if(item != null)
                {
                    item.Listening = true;
                    SetChildrenListening(true, item, true);
                }
            }
        }
        private void MenuItem_UnCheckAllChildren_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var element = sender as System.Windows.FrameworkElement;
            if (element != null)
            {
                var item = element.DataContext as PerformanceItem;
                if (item != null)
                {
                    item.Listening = false;
                    SetChildrenListening(false, item, true);
                }
            }
        }

        private void MenuItem_CheckAllCurrentLevelChildren_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var element = sender as System.Windows.FrameworkElement;
            if (element != null)
            {
                var item = element.DataContext as PerformanceItem;
                if (item != null)
                {
                    item.Listening = true;
                    SetChildrenListening(true, item, false);
                }
            }
        }
        private void MenuItem_UnCheckAllCurrentLevelChildren_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var element = sender as System.Windows.FrameworkElement;
            if (element != null)
            {
                var item = element.DataContext as PerformanceItem;
                if (item != null)
                {
                    item.Listening = false;
                    SetChildrenListening(false, item, false);
                }
            }
        }

        bool IEditorPlugin.OnActive()
        {
            throw new NotImplementedException();
        }

        bool IEditorPlugin.OnDeactive()
        {
            throw new NotImplementedException();
        }

        async System.Threading.Tasks.Task IEditorPlugin.SetObjectToEdit(ResourceEditorContext context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            throw new NotImplementedException();
        }

        void IDockAbleControl.SaveElement(XmlNode node, XmlHolder holder)
        {
            throw new NotImplementedException();
        }

        IDockAbleControl IDockAbleControl.LoadElement(XmlNode node)
        {
            throw new NotImplementedException();
        }

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
            if (mTimerTickThread != null)
            {
                mTimerTickThread.Abort();
                mTimerTickThread = null;
            }
        }
    }
}
