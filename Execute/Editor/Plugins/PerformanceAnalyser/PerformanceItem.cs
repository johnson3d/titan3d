using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PerformanceAnalyser
{
    public class PerformanceItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        ObservableCollection<PerformanceItem> mChildrens = new ObservableCollection<PerformanceItem>();
        public ObservableCollection<PerformanceItem> Childrens
        {
            get { return mChildrens; }
            set
            {
                mChildrens = value;
                OnPropertyChanged("Childrens");
            }
        }

        public PerformanceItem Parent
        {
            get;
            set;
        }

        string mItemName = "";
        public string ItemName
        {
            get { return mItemName; }
            set
            {
                mItemName = value;
                OnPropertyChanged("ItemName");
            }
        }
        public string InheritName = "";

        public string KeyName = "";

        public Action<bool> OnListeningChanged;
        bool mListening = false;
        public virtual bool Listening
        {
            get { return mListening; }
            set
            {
                mListening = value;
                if (mListening)
                    DataShow = Visibility.Visible;
                else
                    DataShow = Visibility.Collapsed;
                OnListeningChanged?.Invoke(mListening);
                OnPropertyChanged("Listening");
            }
        }

        Visibility mListeningVisible = Visibility.Visible;
        public Visibility ListeningVisible
        {
            get { return mListeningVisible; }
            set
            {
                mListeningVisible = value;
                OnPropertyChanged("ListeningVisible");
            }
        }

        Visibility mDataShow = Visibility.Collapsed;
        public Visibility DataShow
        {
            get { return mDataShow; }
            set
            {
                mDataShow = value;
                OnPropertyChanged("DataShow");
            }
        }
        
        bool mIsExpanded;
        public bool IsExpanded
        {
            get { return mIsExpanded; }
            set
            {
                if (mIsExpanded != value)
                {
                    mIsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                if (mIsExpanded && Parent != null)
                    Parent.IsExpanded = true;
            }
        }

        Visibility mVisibility = Visibility.Visible;
        public Visibility Visibility
        {
            get { return mVisibility; }
            set
            {
                if (mVisibility != value)
                {
                    mVisibility = value;
                    OnPropertyChanged("Visibility");
                }
            }
        }

        string mToolTip = "";
        public string ToolTip
        {
            get { return mToolTip; }
            set
            {
                mToolTip = value;
                OnPropertyChanged("ToolTip");
            }
        }

        System.Windows.Media.Brush mTreeViewItemBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0));
        [Browsable(false)]
        public System.Windows.Media.Brush TreeViewItemBackground
        {
            get { return mTreeViewItemBackground; }
            set
            {
                mTreeViewItemBackground = value;
                OnPropertyChanged("TreeViewItemBackground");
            }
        }

        System.Windows.Media.Brush mTreeViewItemForeGround = System.Windows.Media.Brushes.White;
        [Browsable(false)]
        public System.Windows.Media.Brush TreeViewItemForeground
        {
            get { return mTreeViewItemForeGround; }
            set
            {
                mTreeViewItemForeGround = value;
                OnPropertyChanged("TreeViewItemForeground");
            }
        }

        string mHighLightString = "";
        [Browsable(false)]
        public string HighLightString
        {
            get { return mHighLightString; }
            set
            {
                mHighLightString = value;
                OnPropertyChanged("HighLightString");
            }
        }

        public readonly int SelectedStrokeThickness = 5;
        public readonly int UnSelectedStrokeThickness = 2;

        public virtual bool Selected
        {
            get;
            set;
        }
        public ChartPlotter Plotter;
        public virtual void ClearDataSource() { }
    }

    public class PerformanceItem_Parent : PerformanceItem
    {
        public override bool Listening
        {
            get { return false; }
            set { }
        }
    }

    public class PerformanceItem_PerfCount : PerformanceItem
    {
        public override bool Selected
        {
            get { return base.Selected; }
            set
            {
                base.Selected = value;

                if (value)
                {
                    if (mLineGraph_AvgT != null)
                        mLineGraph_AvgT.StrokeThickness = SelectedStrokeThickness;
                    if (mLineGraph_AvgC != null)
                        mLineGraph_AvgC.StrokeThickness = SelectedStrokeThickness;
                    if (mLineGraph_MaxT != null)
                        mLineGraph_MaxT.StrokeThickness = SelectedStrokeThickness;
                }
                else
                {
                    if (mLineGraph_AvgT != null)
                        mLineGraph_AvgT.StrokeThickness = UnSelectedStrokeThickness;
                    if (mLineGraph_AvgC != null)
                        mLineGraph_AvgC.StrokeThickness = UnSelectedStrokeThickness;
                    if (mLineGraph_MaxT != null)
                        mLineGraph_MaxT.StrokeThickness = UnSelectedStrokeThickness;
                }
            }
        }

        int mAvgTime = 0;
        public int AvgTime
        {
            get { return mAvgTime; }
            set
            {
                mAvgTime = value;
                OnPropertyChanged("AvgTime");
            }
        }

        LineGraph mLineGraph_AvgT;
        bool mAvgTimeShowInGraph = false;
        public bool AvgTimeShowInGraph
        {
            get { return mAvgTimeShowInGraph; }
            set
            {
                mAvgTimeShowInGraph = value;

                if (mAvgTimeShowInGraph)
                {
                    var color = Color.FromRgb((byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255));
                    mLineGraph_AvgT = Plotter.AddLineGraph(mAvgTimeDataSource, color, Selected ? SelectedStrokeThickness : UnSelectedStrokeThickness, this.ItemName + "_AvgTime");
                }
                else
                {
                    Plotter.Children.Remove(mLineGraph_AvgT);
                    mLineGraph_AvgT = null;
                }

                OnPropertyChanged("AvgTimeShowInGraph");
            }
        }

        LineGraph mLineGraph_AvgPerHitT;
        bool mAvgTimePerHitShowInGraph = false;
        public bool AvgTimePerHitShowInGraph
        {
            get { return mAvgTimePerHitShowInGraph; }
            set
            {
                mAvgTimePerHitShowInGraph = value;

                if (mAvgTimePerHitShowInGraph)
                {
                    var color = Color.FromRgb((byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255));
                    mLineGraph_AvgPerHitT = Plotter.AddLineGraph(mAvgTimeDataSource, color, Selected ? SelectedStrokeThickness : UnSelectedStrokeThickness, this.ItemName + "_AvgTimePerHit");
                }
                else
                {
                    Plotter.Children.Remove(mLineGraph_AvgPerHitT);
                    mLineGraph_AvgPerHitT = null;
                }

                OnPropertyChanged("AvgTimePerHitShowInGraph");
            }
        }

        int mAvgHit = 0;
        public int AvgHit
        {
            get { return mAvgHit; }
            set
            {
                mAvgHit = value;
                OnPropertyChanged("AvgHit");
            }
        }

        LineGraph mLineGraph_AvgC;
        bool mAvgHitShowInGraph = false;
        public bool AvgHitShowInGraph
        {
            get { return mAvgHitShowInGraph; }
            set
            {
                mAvgHitShowInGraph = value;

                if (mAvgHitShowInGraph)
                {
                    var color = Color.FromRgb((byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255));
                    mLineGraph_AvgC = Plotter.AddLineGraph(mAvgCounterDataSource, color, Selected ? SelectedStrokeThickness : UnSelectedStrokeThickness, this.ItemName + "_AvgCounter");
                }
                else
                {
                    Plotter.Children.Remove(mLineGraph_AvgC);
                    mLineGraph_AvgC = null;
                }

                OnPropertyChanged("AvgCounterShowInGraph");
            }
        }

        LineGraph mLineGraph_MaxT;
        bool mMaxTimeInCounterShowInGraph = false;
        public bool MaxTimeInCounterShowInGraph
        {
            get { return mMaxTimeInCounterShowInGraph; }
            set
            {
                mMaxTimeInCounterShowInGraph = value;

                if (mMaxTimeInCounterShowInGraph)
                {
                    var color = Color.FromRgb((byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255),
                                                (byte)EngineNS.MathHelper.RandomRange(0, 255));
                    mLineGraph_MaxT = Plotter.AddLineGraph(mMaxTimeInCounterDataSource, color, Selected ? SelectedStrokeThickness : UnSelectedStrokeThickness, this.ItemName + "_MaxTimeInCounter");
                }
                else
                {
                    Plotter.Children.Remove(mLineGraph_MaxT);
                    mLineGraph_MaxT = null;
                }

                OnPropertyChanged("MaxTimeInCounterShowInGraph");
            }
        }

        protected ObservableDataSource<Point> mAvgTimeDataSource = new ObservableDataSource<Point>();
        protected ObservableDataSource<Point> mAvgTimePerHitDataSource = new ObservableDataSource<Point>();
        protected ObservableDataSource<Point> mAvgCounterDataSource = new ObservableDataSource<Point>();
        protected ObservableDataSource<Point> mMaxTimeInCounterDataSource = new ObservableDataSource<Point>();

        public override void ClearDataSource()
        {
            mAvgTimeDataSource.Collection.Clear();
            mAvgTimePerHitDataSource.Collection.Clear();
            mAvgCounterDataSource.Collection.Clear();
            mMaxTimeInCounterDataSource.Collection.Clear();
        }

        public void UpdateData(UInt64 time, EngineNS.Profiler.PerfViewer.PfValue_PerfCounter value)
        {
            if (value == null)
                return;
            //var view = CCore.Game.PerformanceViewManager.Instance.GetPerformanceView(ItemName, InheritName);
            //if (view == null)
            //    return;

            //var datas = view.GetData();
            //double value = 0;
            //foreach(var data in datas)
            //{
            //    var dataType = data.GetType();
            //    if(dataType == typeof(sbyte) ||
            //       dataType == typeof(Int16) ||
            //       dataType == typeof(Int32) ||
            //       dataType == typeof(Int64) ||
            //        dataType == typeof(byte) ||
            //        dataType == typeof(UInt16) ||
            //        dataType == typeof(UInt32) ||
            //        dataType == typeof(UInt64) ||
            //        dataType == typeof(Single) ||
            //        dataType == typeof(Double))
            //    {
            //        value = System.Convert.ToDouble(data);
            //        break;
            //    }
            //}

            AvgTime = value.AvgTime;
            AvgHit = value.AvgHit;

            EngineNS.CEngine.Instance.EventPoster.RunOn(()=>
            {
                mAvgTimeDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, value.AvgTime));
                mAvgCounterDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, value.AvgHit));
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            //Application.Current.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    mAvgTimeDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, value.AvgTime));
            //    mAvgCounterDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, value.AvgHit));
            //}));
        }
        public void UpdateData(UInt64 time, EngineNS.Profiler.TimeScope value)
        {
            if (value == null)
            {
                AvgTime = -1;
                AvgHit = -1;

                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    mAvgTimeDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, -1));
                    mAvgCounterDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, -1));
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
            else
            {
                AvgTime = (int)value.AvgTime;
                AvgHit = value.AvgHit;

                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    if (Application.Current.MainWindow != null)
                    {
                        mAvgTimeDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, value.AvgTime));
                        mAvgCounterDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, value.AvgHit));
                    }
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }
        public void UpdateData(UInt64 time, int avgTime, int avgHit)
        {
            AvgTime = avgTime;
            AvgHit = avgHit;

            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                mAvgTimeDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, avgTime));
                mAvgCounterDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, avgHit));
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }
    }

    public class PerformanceItem_Data : PerformanceItem
    {
        public class ValueData : INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            string mValueName;
            public string ValueName
            {
                get { return mValueName; }
                set
                {
                    mValueName = value;
                    OnPropertyChanged("ValueName");
                }
            }
            object mValue;
            public object Value
            {
                get { return mValue; }
                set
                {
                    mValue = value;
                    if (mValue == null || mValue.GetType() == typeof(string))
                        ShowInGraphButtonVisible = Visibility.Collapsed;
                    else
                        ShowInGraphButtonVisible = Visibility.Visible;
                    OnPropertyChanged("Value");
                }
            }

            Visibility mShowInGraphButtonVisible = Visibility.Visible;
            public Visibility ShowInGraphButtonVisible
            {
                get { return mShowInGraphButtonVisible; }
                set
                {
                    mShowInGraphButtonVisible = value;
                    OnPropertyChanged("ShowInGraphButtonVisible");
                }
            }

            bool mShowInGraph = false;
            public bool ShowInGraph
            {
                get { return mShowInGraph; }
                set
                {
                    mShowInGraph = value;

                    if(mShowInGraph)
                    {
                        var color = Color.FromRgb((byte)EngineNS.MathHelper.RandomRange(0, 255),
                            (byte)EngineNS.MathHelper.RandomRange(0, 255),
                            (byte)EngineNS.MathHelper.RandomRange(0, 255));
                        LineGraph = HostPerformanceItem.Plotter.AddLineGraph(GraphDataSource, color, HostPerformanceItem.Selected ? HostPerformanceItem.SelectedStrokeThickness : HostPerformanceItem.UnSelectedStrokeThickness, HostPerformanceItem.ItemName + (string.IsNullOrEmpty(ValueName) ? "" : "_" + ValueName));
                    }
                    else
                    {
                        HostPerformanceItem.Plotter.Children.Remove(LineGraph);
                        LineGraph = null;
                    }

                    OnPropertyChanged("ShowInGraph");
                }
            }

            public ObservableDataSource<Point> GraphDataSource = new ObservableDataSource<Point>();
            public LineGraph LineGraph;
            public PerformanceItem HostPerformanceItem
            {
                get;
                private set;
            }

            public ValueData(PerformanceItem hostItem)
            {
                HostPerformanceItem = hostItem;
            }
        }

        ObservableCollection<ValueData> mDataValues = new ObservableCollection<ValueData>();
        public ObservableCollection<ValueData> DataValues
        {
            get { return mDataValues; }
            set
            {
                mDataValues = value;
                OnPropertyChanged("DataValues");
            }
        }

        public override void ClearDataSource()
        {
            foreach(var data in DataValues)
            {
                data.GraphDataSource.Collection.Clear();
            }
        }

        public void UpdateData(UInt64 time, EngineNS.Profiler.PerfViewer.PfValue_Data value)
        {
            if (value == null)
                return;
            EngineNS.CEngine.Instance.EventPoster.RunOn(()=>
            {
                if (value.ValueDatas.Count != DataValues.Count)
                {
                    DataValues.Clear();
                    //foreach (var data in value.ValueDatas)
                    for(int i=0;i< value.ValueDatas.Count;i++)
                    {
                        var vd = new ValueData(this);
                        vd.ValueName = value.ValueNames[i];
                        vd.Value = value.ValueDatas[i];
                        DataValues.Add(vd);
                    }
                }

                for (int i = 0; i < DataValues.Count; i++)
                {
                    DataValues[i].ValueName = value.ValueNames[i];
                    DataValues[i].Value = value.ValueDatas[i];
                    if (DataValues[i].ShowInGraphButtonVisible == Visibility.Visible)
                    DataValues[i].GraphDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, System.Convert.ToDouble(value.ValueDatas[i])));
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }

        public void UpdateData(UInt64 time, EngineNS.Profiler.PerfViewer.Viewer value)
        {
            if (value == null)
                return;

            var names = value.GetValueNameAction();
            var values = value.GetValueAction();
            if (names.Length != values.Length)
                return;

            EngineNS.CEngine.Instance.EventPoster.RunOn(()=>
            {
                if (names.Length != DataValues.Count)
                {
                    DataValues.Clear();
                    for(int i=0; i<names.Length; i++)
                    {
                        var vd = new ValueData(this);
                        vd.ValueName = names[i];
                        vd.Value = values[i];
                        DataValues.Add(vd);
                    }
                }

                for (int i = 0; i < DataValues.Count; i++)
                {
                    DataValues[i].ValueName = names[i];
                    DataValues[i].Value = values[i];
                    if (DataValues[i].ShowInGraphButtonVisible == Visibility.Visible)
                        DataValues[i].GraphDataSource.AppendAsync(Application.Current.MainWindow.Dispatcher, new Point(time, System.Convert.ToDouble(values[i])));
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);

        }
    }
}