using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
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

namespace EditorCommon.Controls.Animation
{
    public class GridPoint
    {
        public float X = 0;
        public float Y = 0;
        public RName Name = RName.EmptyName;
    }
    public enum AnimationBlendSpaceDimension
    {
        ABSD_1D,
        ABSD_2D,
    }
    //指示器，显示当前的位置
    public class BlendSpaceIndicator
    {
        public class ValueEventArgs : EventArgs
        {
            public Vector3 Value = Vector3.Zero;
            public ValueEventArgs(Vector3 value)
            {
                Value = value;
            }
        }
        public event EventHandler<ValueEventArgs> OnValueChange;
        Vector3 mValue = Vector3.Zero;
        [Browsable(false)]
        public Vector3 Value
        {
            get => mValue;
            set
            {
                mValue = value;
                var margin = new System.Windows.Thickness();
                margin.Left = (value.X);
                margin.Top = (value.Y);
                mIndicatorCtrl.Margin = margin;
                OnValueChange?.Invoke(this, new ValueEventArgs(mValue));
            }
        }
        Vector3 mPercent = Vector3.Zero;
        [Browsable(false)]
        public Vector3 Percent { get => mPercent; set => mPercent = value; }
        AnimationBlendSpaceIndicator mIndicatorCtrl = new AnimationBlendSpaceIndicator();

        [Browsable(false)]
        public AnimationBlendSpaceIndicator IndicatorCtrl { get => mIndicatorCtrl; set => mIndicatorCtrl = value; }
        public BlendSpaceIndicator()
        {
            mIndicatorCtrl.HorizontalAlignment = HorizontalAlignment.Left;
            mIndicatorCtrl.VerticalAlignment = VerticalAlignment.Top;
            mIndicatorCtrl.HostBlendSpaceIndicator = this;
        }

        public void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        public void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        public void MouseEnter(object sender, MouseEventArgs e)
        {

        }

        public void MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
    //编辑器格子上的动作点
    public class BlendSpaceEditorNode : DragDrop.IDragAbleObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public AnimationSample HostSample { get; set; } = null;
        bool mIsSelected = false;
        [Browsable(false)]
        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsSelected = value;
                mNodeCtrl.IsSelected = value;
            }
        }
        bool mIsValided = true;
        [Browsable(false)]
        public bool IsValided
        {
            get => mIsValided;
            set
            {
                mIsValided = value;
                mNodeCtrl.IsValided = value;
            }
        }
        int mIndexInGrid = -1;
        [Browsable(false)]
        public int IndexInGrid
        {
            get => mIndexInGrid;
            set
            {
                mIndexInGrid = value;
            }
        }
        int mIndex = -1;
        [Browsable(false)]
        public int Index
        {
            get => mIndex;
            set
            {
                mIndex = value;
            }
        }
        bool mIsShow = false;
        [Browsable(false)]
        public bool IsShow
        {
            get => mIsShow;
            set => mIsShow = value;
        }
        RName mName = RName.EmptyName;
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
        public RName Name
        {
            get => mName;
            set
            {
                mName = value;
                if (value == null)
                    return;
                mNodeCtrl.ContentName = value.PureName();
                if (HostSample == null)
                    return;
                if (HostSample.AnimationName == value)
                    return;
                var pose = HostSample.Animation.BindingSkeletonPose;
                var clip = AnimationClip.CreateSync(value);
                clip.Bind(pose);
                HostSample.Animation = clip;
            }
        }
        AnimationBlendSpaceNodeControl mNodeCtrl = new AnimationBlendSpaceNodeControl();
        Vector3 mValue = Vector3.Zero;
        [Browsable(false)]
        public Vector3 Value
        {
            get => mValue;
            set
            {
                mValue = value;
                Horizontal = value.X;
                Vertical = value.Y;
            }
        }
        public float Horizontal
        {
            get => mValue.X;
            set
            {
                OnPropertyChanged("Horizontal");
            }
        }
        public float Vertical
        {
            get => mValue.Y;
            set
            {
                OnPropertyChanged("Vertical");
            }
        }
        Vector3 mValueInGrid = Vector3.Zero;
        [Browsable(false)]
        public Vector3 ValueInGrid
        {
            get => mValueInGrid;
            set
            {
                mValueInGrid = value;
                var margin = new System.Windows.Thickness();
                margin.Left = (value.X);
                margin.Top = (value.Y);
                mNodeCtrl.Margin = margin;
            }
        }
        Vector3 mPercentInGrid = Vector3.Zero;
        [Browsable(false)]
        public Vector3 PercentInGrid { get => mPercentInGrid; set => mPercentInGrid = value; }

        [Browsable(false)]
        public AnimationBlendSpaceNodeControl NodeCtrl { get => mNodeCtrl; set => mNodeCtrl = value; }
        public BlendSpaceEditorNode()
        {
            mNodeCtrl.HorizontalAlignment = HorizontalAlignment.Left;
            mNodeCtrl.VerticalAlignment = VerticalAlignment.Top;
            mNodeCtrl.HostNode = this;
        }

        public FrameworkElement GetDragVisual()
        {
            return null;
        }
        //need drag and drop
    }

    public class AnimationAssetOperateEventArgs : RoutedEventArgs
    {
        public BlendSpaceEditorNode SrcNode;
        public BlendSpaceEditorNode DesNode;
    }


    public delegate void AnimationAssetOperationHandle(object sender, AnimationAssetOperateEventArgs e);
    /// <summary>
    /// Interaction logic for AnimationBlendSpaceControl.xaml
    /// </summary>
    public partial class AnimationBlendSpaceControl : UserControl, INotifyPropertyChanged, EngineNS.ITickInfo
    {
        public event AnimationAssetOperationHandle OnAnimationAssetDrop;    //资源放置
        //public event AnimationAssetOperationHandle OnAnimationAssetReplace; //资源替换
        public event AnimationAssetOperationHandle OnAnimationAssetMove; //资源移动位置
        public event AnimationAssetOperationHandle OnAnimationAssetDelete; //资源移动位置
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region TickInfo

        public bool EnableTick { get; set; }
        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            return null;
        }
        public void BeforeFrame()
        {

        }
        public void TickLogic()
        {
            AnimationPlayingCtrl.TickLogic();
            var playPercent = AnimationPlayingCtrl.PlayPercent;
            var time = mAnimationBlendSpace.DurationInMilliSecond * playPercent;
            mAnimationBlendSpace.CurrentTimeInMilliSecond = (uint)time;
            //if(mAnimationBlendSpace is IEditorAnimation)
            //{
            //    var bs = mAnimationBlendSpace as IEditorAnimation;
            //    bs.ManualUpdate(playPercent);
            //}
            mAnimationBlendSpace.TickFroEditor(playPercent);
        }
        public void TickRender()
        {

        }
        async System.Threading.Tasks.Task DirtyProcess(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        async System.Threading.Tasks.Task DirtyProcessAsync(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        //bool mNeedDirtyProcess = false;
        public void TickSync()
        {
            var noUse = DirtyProcess();
        }
        #endregion
        public AnimationBlendSpaceControl()
        {
            InitializeComponent();
            //mCurrentBlendIndicator.IndicatorCtrl.IsHitTestVisible = false;
            mCurrentBlendIndicator.IndicatorCtrl.MouseLeftButtonDown += IndicatorCtrl_MouseLeftButtonDown;
            mCurrentBlendIndicator.IndicatorCtrl.MouseLeftButtonUp += IndicatorCtrl_MouseLeftButtonUp;
            mCurrentBlendIndicator.IndicatorCtrl.MouseMove += IndicatorCtrl_MouseMove;
            mCurrentBlendIndicator.OnValueChange += CurrentBlendIndicator_OnValueChange;
            MainGrid.Children.Add(mCurrentBlendIndicator.IndicatorCtrl);
        }
        void SetAnimationPlayingCtrlParm()
        {
            AnimationPlayingCtrl.AnimationName = mAnimationBlendSpace.Name.PureName();
            AnimationPlayingCtrl.TotalFrame = 0;
            AnimationPlayingCtrl.Duration = 0;
        }
        AnimationBlendSpaceDetial mBlendSpaceDetial = null;
        public AnimationBlendSpaceDetial BlendSpaceDetial
        {
            get => mBlendSpaceDetial;
            set
            {
                mBlendSpaceDetial = value;
                IsBlendParamInited = true;
            }
        }
        BlendSpaceIndicator mCurrentBlendIndicator = new BlendSpaceIndicator();
        public BlendSpaceIndicator CurrentBlendIndicator
        {
            get => mCurrentBlendIndicator;
        }
        BlendSpace mAnimationBlendSpace = null;
        public BlendSpace AnimationBlendSpace
        {
            get => mAnimationBlendSpace;
            set
            {
                mAnimationBlendSpace = value;
                SetAnimationPlayingCtrlParm();
                for (int i = 0; i < mAnimationBlendSpace.Samples.Count; ++i)
                {
                    var sample = mAnimationBlendSpace.Samples[i];
                    var node = CreateNode();
                    node.HostSample = sample;
                    node.Name = sample.AnimationName;
                    node.Index = i;
                    node.Value = sample.Value;
                    node.ValueInGrid = GetPointInGridByRealValue(sample.Value);

                    node.PercentInGrid = GetPercentInGrid(node.ValueInGrid);
                    BlendCanvas.Children.Add(node.NodeCtrl);
                    mValidedNodesList.Add(node);
                }
            }
        }

        bool mHideContent = false;
        public bool HideContent
        {
            get => mHideContent;
            set
            {
                mHideContent = value;
                foreach (var node in mValidedNodesList)
                {
                    node.NodeCtrl.HideContent = value;
                }
            }
        }
        //从左到右从下到上的顺序排列
        List<BlendSpaceEditorNode> mValidedNodesList = new List<BlendSpaceEditorNode>();
        List<BlendSpaceEditorNode> mInValidedNodesList = new List<BlendSpaceEditorNode>();
        AnimationBlendSpaceDimension mBlendSpaceDimension = AnimationBlendSpaceDimension.ABSD_1D;
        public AnimationBlendSpaceDimension BlendSpaceDimension
        {
            get => mBlendSpaceDimension;
            set
            {
                mBlendSpaceDimension = value;
                if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
                {
                    var temp = DimensionPieces;
                    temp.Y = 2;
                    DimensionPieces = temp;
                    YMinTB.Visibility = Visibility.Hidden;
                    YMaxTB.Visibility = Visibility.Hidden;
                    YNameTB.Visibility = Visibility.Hidden;
                }
                OnPropertyChanged("BlendSpaceDimension");
            }
        }
        #region BlendAxis
        public float XMin
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return 0;
                return mAnimationBlendSpace.BlendAxises[0].Min;
            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[0].Min = value;
                OnPropertyChanged("XMin");
                ReCalculateSamplesValue();
            }
        }
        public float YMin
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return 0;
                return mAnimationBlendSpace.BlendAxises[1].Min;
            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[1].Min = value;
                OnPropertyChanged("YMin");
                ReCalculateSamplesValue();
            }
        }
        public float XMax
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return 100;
                return mAnimationBlendSpace.BlendAxises[0].Max;
            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[0].Max = value;
                OnPropertyChanged("XMax");
                ReCalculateSamplesValue();
            }
        }
        public float YMax
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return 100;
                return mAnimationBlendSpace.BlendAxises[1].Max;
            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[1].Max = value;
                OnPropertyChanged("YMax");
                ReCalculateSamplesValue();
            }
        }
        public string XDimensionName
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return "None";
                return mAnimationBlendSpace.BlendAxises[0].AxisName;
            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[0].AxisName = value;
                OnPropertyChanged("XDimensionName");
            }
        }
        public string YDimensionName
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return "None";
                return mAnimationBlendSpace.BlendAxises[1].AxisName;
            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[1].AxisName = value;
                OnPropertyChanged("YDimensionName");
            }
        }
        //五条线，四个格
        public Vector3 DimensionPieces
        {
            get
            {
                if (mAnimationBlendSpace == null)
                    return Vector3.UnitXYZ * 4;
                var yGridNum = 0;
                if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
                    yGridNum = 2;
                else
                    yGridNum = mAnimationBlendSpace.BlendAxises[1].GridNum;
                return new Vector3(mAnimationBlendSpace.BlendAxises[0].GridNum, yGridNum, 0);

            }
            set
            {
                if (mAnimationBlendSpace == null)
                    return;
                mAnimationBlendSpace.BlendAxises[0].GridNum = (int)value.X;
                mAnimationBlendSpace.BlendAxises[1].GridNum = (int)value.Y;
                OnPropertyChanged("DimensionPieces");
                ReDrawControl();
                //ReCalculateSamplesValue();
                ReLayOutSamples();
            }
        }
        #endregion
        #region DrawControl
        System.Windows.Media.Color mLineGridColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF2F2F2F");
        System.Windows.Media.Color mBorderGridColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF1B1B1B");
        int mGridLineThickness = 1;
        int mGridBorderLineThickness = 2;
        public void ReDrawControl()
        {
            DrawGrid();
            DrawNode();
        }
        void DrawGrid()
        {
            BlendCanvas.Children.Clear();
            var xDimensionMax = (float)BlendCanvas.ActualWidth;
            var yDimensionMax = (float)BlendCanvas.ActualHeight;
            var xGridSize = (float)xDimensionMax / DimensionPieces.X;
            var yGridSize = (float)yDimensionMax / DimensionPieces.Y;
            for (int xCount = 1; xCount < DimensionPieces.X; ++xCount)
            {
                var line = CreateLine(new Vector2(xGridSize * xCount, 0), new Vector2(xGridSize * xCount, yDimensionMax), mLineGridColor, mGridLineThickness);
                BlendCanvas.Children.Add(line);
            }
            for (int yCount = 1; yCount < DimensionPieces.Y; ++yCount)
            {
                var line = CreateLine(new Vector2(0, yGridSize * yCount), new Vector2(xDimensionMax, yGridSize * yCount), mLineGridColor, mGridLineThickness);
                BlendCanvas.Children.Add(line);
            }
            //draw border
            var borderline = CreateLine(new Vector2(0, 0), new Vector2(0, yDimensionMax), mBorderGridColor, mGridBorderLineThickness);
            BlendCanvas.Children.Add(borderline);
            borderline = CreateLine(new Vector2(0, 0), new Vector2(xDimensionMax, 0), mBorderGridColor, mGridBorderLineThickness);
            BlendCanvas.Children.Add(borderline);
            borderline = CreateLine(new Vector2(xDimensionMax, 0), new Vector2(xDimensionMax, yDimensionMax), mBorderGridColor, mGridBorderLineThickness);
            BlendCanvas.Children.Add(borderline);
            borderline = CreateLine(new Vector2(0, yDimensionMax), new Vector2(xDimensionMax, yDimensionMax), mBorderGridColor, mGridBorderLineThickness);
            BlendCanvas.Children.Add(borderline);
            //MainGrid.Children.Add(BlendCanvas);
        }
        Line CreateLine(Vector2 pStart, Vector2 pEnd, System.Windows.Media.Color strokeColor, int strokeThickness)
        {
            Line line = new Line();
            line.IsHitTestVisible = false;
            line.Stroke = new System.Windows.Media.SolidColorBrush(strokeColor);
            line.StrokeThickness = strokeThickness;
            line.HorizontalAlignment = HorizontalAlignment.Left;
            line.VerticalAlignment = VerticalAlignment.Center;
            line.X1 = pStart.X;
            line.Y1 = pStart.Y;
            line.X2 = pEnd.X;
            line.Y2 = pEnd.Y;
            return line;
        }
        public void DrawNode()
        {
            FlushNodes();
            foreach (var node in mValidedNodesList)
            {
                BlendCanvas.Children.Add(node.NodeCtrl);
            }
            foreach (var node in mInValidedNodesList)
            {
                BlendCanvas.Children.Add(node.NodeCtrl);
            }
            //BlendCanvas.Children.Add(mCurrentBlendPos.GridNode);
        }
        public void RemoveChild(DependencyObject parent, UIElement child)
        {
            var panel = parent as Panel;
            if (panel != null)
            {
                panel.Children.Remove(child);
                return;
            }

            var decorator = parent as Decorator;
            if (decorator != null)
            {
                if (decorator.Child == child)
                {
                    decorator.Child = null;
                }
                return;
            }

            var contentPresenter = parent as ContentPresenter;
            if (contentPresenter != null)
            {
                if (contentPresenter.Content == child)
                {
                    contentPresenter.Content = null;
                }
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                if (contentControl.Content == child)
                {
                    contentControl.Content = null;
                }
                return;
            }

            // maybe more
        }
        void FlushNodes()
        {
            foreach (var node in mValidedNodesList)
            {
                node.ValueInGrid = GetPointInGridByRealValue(node.Value);
                CheckAndCorrectContenNameSide(node);
                node.PercentInGrid = GetPercentInGrid(node.ValueInGrid);
            }
            foreach (var node in mInValidedNodesList)
            {
                node.ValueInGrid = GetPointInGridByPercent(node.PercentInGrid);
                CheckAndCorrectContenNameSide(node);
            }
            mCurrentBlendIndicator.Value = GetPointInGridByPercent(mCurrentBlendIndicator.Percent);
        }
        #endregion
        public BlendSpaceEditorNode CreateNode()
        {
            var node = new BlendSpaceEditorNode();
            node.NodeCtrl.OnSelected += BlendSpaceEditorNode_OnSelected;
            node.NodeCtrl.OnDelete += BlendSpaceEditorNode_OnDelete;
            node.NodeCtrl.HideContent = HideContent;
            return node;
        }

        private void BlendSpaceEditorNode_OnDelete(object sender, EventArgs e)
        {
            var node = sender as BlendSpaceEditorNode;
            if (node != null)
            {
                DeleteNode(node);
            }
        }
        public void DeleteNode(BlendSpaceEditorNode node)
        {
            var args = new AnimationAssetOperateEventArgs();
            args.SrcNode = node;
            mValidedNodesList.Remove(node);
            for (int i = 0; i < mValidedNodesList.Count; ++i)
            {
                mValidedNodesList[i].Index = i;
            }
            BlendCanvas.Children.Remove(node.NodeCtrl);
            mAnimationBlendSpace.RemoveSample(node.Index);
            OnAnimationAssetDelete?.Invoke(this, args);
        }
        public void DeleteSelectedNode()
        {
            DeleteNode(mCurrentSelectedNode);
            mCurrentSelectedNode = null;
        }
        BlendSpaceEditorNode mCurrentSelectedNode = null;
        private void BlendSpaceEditorNode_OnSelected(object sender, EventArgs e)
        {
            var node = sender as BlendSpaceEditorNode;
            if (node != mCurrentSelectedNode)
            {
                if (mCurrentSelectedNode != null)
                    mCurrentSelectedNode.IsSelected = false;
                node.IsSelected = true;
                mCurrentSelectedNode = node;
            }
        }
        Vector3 GetPointInGrid(float x, float y)
        {
            var xDimensionMax = (float)BlendCanvas.ActualWidth;
            var yDimensionMax = (float)BlendCanvas.ActualHeight;
            var xGridSize = (float)xDimensionMax / DimensionPieces.X;
            var yGridSize = (float)yDimensionMax / DimensionPieces.Y;
            var xGridNum = (int)Math.Round(x / xGridSize);
            var yGridNum = (int)Math.Round(y / yGridSize);
            if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
            {
                yGridNum = 1;
            }
            return new Vector3(xGridNum * xGridSize, yGridNum * yGridSize, 0);
        }
        Vector3 GetPointInGridByRealValue(Vector3 realVaule)
        {
            var xDimensionMax = (float)BlendCanvas.ActualWidth;
            var yDimensionMax = (float)BlendCanvas.ActualHeight;
            var xGridSize = (float)xDimensionMax / DimensionPieces.X;
            var yGridSize = (float)yDimensionMax / DimensionPieces.Y;
            var xRealMax = XMax - XMin;
            var yRealMax = YMax - YMin;
            var xSize = xRealMax / DimensionPieces.X;
            var ySize = yRealMax / DimensionPieces.Y;

            var gridX = (realVaule.X - XMin) / xSize;
            var gridY = (yRealMax - (realVaule.Y - YMin)) / ySize;
            if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
            {
                gridY = 1;
            }

            return new Vector3(xGridSize * gridX, yGridSize * gridY, 0);
        }
        Vector3 GetRealValueByPointInGrid(Vector3 pointInGrid)
        {
            var xDimensionMax = (float)BlendCanvas.ActualWidth;
            var yDimensionMax = (float)BlendCanvas.ActualHeight;
            var xGridSize = (float)xDimensionMax / DimensionPieces.X;
            var yGridSize = (float)yDimensionMax / DimensionPieces.Y;
            var xRealMax = XMax - XMin;
            var yRealMax = YMax - YMin;
            var xSize = xRealMax / DimensionPieces.X;
            var ySize = yRealMax / DimensionPieces.Y;

            //哪行哪列
            var gridX = pointInGrid.X / xGridSize;
            var gridY = (yDimensionMax - pointInGrid.Y) / yGridSize;
            if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
            {
                gridY = 0;
            }

            return new Vector3(xSize * gridX + XMin, ySize * gridY + YMin, 0);
        }
        Vector3 GetPercentInGrid(Vector3 pointInGrid)
        {
            var xDimensionMax = (float)BlendCanvas.ActualWidth;
            var yDimensionMax = (float)BlendCanvas.ActualHeight;

            return new Vector3(xDimensionMax > 0 ? pointInGrid.X / xDimensionMax : 0, yDimensionMax > 0 ? pointInGrid.Y / yDimensionMax : 0, 0);
        }
        Vector3 GetPointInGridByPercent(Vector3 percent)
        {
            var xDimensionMax = (float)BlendCanvas.ActualWidth;
            var yDimensionMax = (float)BlendCanvas.ActualHeight;

            return new Vector3(percent.X * xDimensionMax, percent.Y * yDimensionMax, 0);
        }
        bool IsBlendParamInited = false;
        //DimensionPieces change,ReLayOut
        void ReLayOutSamples()
        {
            if (mAnimationBlendSpace == null)
                return;
            if (!IsBlendParamInited)
                return;
            var tempInValidedList = new List<BlendSpaceEditorNode>();
            for (int i = 0; i < mValidedNodesList.Count; ++i)
            {
                var testNode = mValidedNodesList[i];
                var closePoint = GetPointInGrid(testNode.ValueInGrid.X, testNode.ValueInGrid.Y);
                var node = GetNodeByPointInGrid(closePoint);
                if (node != null && node != testNode)
                {
                    tempInValidedList.Add(testNode);
                    testNode.IsValided = false;
                    continue;
                }
                testNode.ValueInGrid = closePoint;
                testNode.Value = GetRealValueByPointInGrid(closePoint);
                mAnimationBlendSpace.Samples[i].Value = testNode.Value;
            }
            foreach (var node in tempInValidedList)
            {
                mValidedNodesList.Remove(node);
                mAnimationBlendSpace.Samples.RemoveAt(node.Index);
            }
            for (int i = 0; i < mValidedNodesList.Count; ++i)
            {
                mValidedNodesList[i].Index = i;
            }
            foreach (var node in tempInValidedList)
            {
                if (!mInValidedNodesList.Contains(node))
                    mInValidedNodesList.Add(node);
            }
            mAnimationBlendSpace.ReFresh();
        }
        //改变了Axis length,重新计算vaule值
        void ReCalculateSamplesValue()
        {
            if (mAnimationBlendSpace == null)
                return;
            if (!IsBlendParamInited)
                return;
            for (int i = 0; i < mValidedNodesList.Count; ++i)
            {
                mValidedNodesList[i].Value = GetRealValueByPointInGrid(mValidedNodesList[i].ValueInGrid);
                mAnimationBlendSpace.Samples[i].Value = mValidedNodesList[i].Value;
            }
            mAnimationBlendSpace.ReFresh();
        }
        BlendSpaceEditorNode GetNodeByPointInGrid(Vector3 pointInGrid)
        {
            foreach (var node in mValidedNodesList)
            {
                if (node.ValueInGrid == pointInGrid)
                {
                    return node;
                }
            }
            return null;
        }
        BlendSpaceEditorNode GetNodeByByRealValue(Vector3 realVaule)
        {
            foreach (var node in mValidedNodesList)
            {
                if (node.Value == realVaule)
                {
                    return node;
                }
            }
            return null;
        }
        BlendSpaceEditorNode GetNode(int indexInGrid)
        {
            foreach (var node in mValidedNodesList)
            {
                if (node.IndexInGrid == indexInGrid)
                    return node;
            }
            return null;
        }
        private void BlendCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //DrawGrid();
            ReDrawControl();
        }
        BlendSpaceEditorNode mPreViewNode = null;
        BlendSpaceEditorNode mDragingNode = null;
        Vector3 mLastDragOverPoint = Vector3.UnitXYZ * (-50);
        void EnableNodesHitVisiable(bool hitVisiable)
        {
            foreach (var node in mValidedNodesList)
            {
                node.NodeCtrl.IsHitTestVisible = hitVisiable;
            }
        }
        void AnimationAssetMove(object sender, BlendSpaceEditorNode node)
        {
            var args = new AnimationAssetOperateEventArgs();
            args.SrcNode = node;

            var sample = mAnimationBlendSpace.GetAnimationSample(node.Index);
            sample.Value = node.Value;
            mAnimationBlendSpace.ReFresh();

            OnAnimationAssetMove?.Invoke(sender, args);
        }
        void AnimationAssetDrop(object sender, BlendSpaceEditorNode node)
        {
            var assetName = node.Name;
            var firstAnim = AnimationClip.CreateSync(assetName);

            var animSample = mAnimationBlendSpace.AddSample(firstAnim, node.Value);
            node.HostSample = animSample;
            var args = new AnimationAssetOperateEventArgs();
            args.SrcNode = node;
            OnAnimationAssetDrop?.Invoke(sender, args);
        }
        void AnimationAssetDelete(object sender, BlendSpaceEditorNode node)
        {
            DeleteNode(node);
        }
        void CheckAndCorrectContenNameSide(BlendSpaceEditorNode node)
        {
            if (node == null)
                return;
            if (node.ValueInGrid.X + 10 > BlendCanvas.ActualWidth)
            {
                node.NodeCtrl.RightSide = false;
            }
            else
            {
                node.NodeCtrl.RightSide = true;
            }
        }
        private void BlendCanvas_DragEnter(object sender, DragEventArgs e)
        {
            EnableNodesHitVisiable(false);
            e.Handled = true;
            var canvasPos = e.GetPosition(BlendCanvas);
            var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            mPreViewNode = new BlendSpaceEditorNode();
            var info = dragObject[0] as Resources.ResourceInfo;
            if (info != null)
            {
                mPreViewNode.Name = info.ResourceName;
                mPreViewNode.NodeCtrl.IsHitTestVisible = false;
                mPreViewNode.NodeCtrl.HideContent = HideContent;
                var point = GetPointInGrid((float)canvasPos.X, (float)canvasPos.Y);
                var node = GetNodeByPointInGrid(point);
                if (node == null)
                {
                    mLastDragOverPoint = point;
                }
                mPreViewNode.ValueInGrid = mLastDragOverPoint;
                CheckAndCorrectContenNameSide(mPreViewNode);
                BlendCanvas.Children.Add(mPreViewNode.NodeCtrl);
            }
            var gridNode = dragObject[0] as BlendSpaceEditorNode;
            if (gridNode != null)
            {
                mDragingNode = gridNode;
                var point = GetPointInGrid((float)canvasPos.X, (float)canvasPos.Y);
                var node = GetNodeByPointInGrid(point);
                if (node == null)
                {
                    mDragingNode.ValueInGrid = point;
                    CheckAndCorrectContenNameSide(mDragingNode);
                    mDragingNode.PercentInGrid = GetPercentInGrid(mDragingNode.ValueInGrid);
                }
            }

        }
        private void BlendCanvas_DragLeave(object sender, DragEventArgs e)
        {
            EnableNodesHitVisiable(true);
            if (mPreViewNode != null)
            {
                RemoveChild(mPreViewNode.NodeCtrl.Parent, mPreViewNode.NodeCtrl);
                mPreViewNode = null;
            }
            if (mDragingNode != null)
            {
                mDragingNode = null;
            }
            mLastDragOverPoint = Vector3.UnitXYZ * (-50);
        }
        private void BlendCanvas_DragOver(object sender, DragEventArgs e)
        {
            var pos = e.GetPosition(MainGrid);
            e.Handled = true;
            var canvasPos = e.GetPosition(BlendCanvas);
            var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            var currentPoint = GetPointInGrid((float)canvasPos.X, (float)canvasPos.Y);
            var info = dragObject[0] as Resources.ResourceInfo;
            if (info != null)
            {
                var existNode = GetNodeByPointInGrid(currentPoint);
                if (mPreViewNode != null && mLastDragOverPoint != currentPoint && existNode == null)
                {
                    mPreViewNode.ValueInGrid = currentPoint;
                    CheckAndCorrectContenNameSide(mPreViewNode);
                    mLastDragOverPoint = currentPoint;
                }
            }
            var gridNode = dragObject[0] as BlendSpaceEditorNode;
            if (gridNode != null)
            {
                var existNode = GetNodeByPointInGrid(currentPoint);
                if (mDragingNode != null && existNode == null)
                {
                    if (mValidedNodesList.Contains(mDragingNode))
                    {
                        if (mDragingNode.ValueInGrid != currentPoint)
                        {
                            mDragingNode.ValueInGrid = currentPoint;
                            CheckAndCorrectContenNameSide(mDragingNode);
                            mDragingNode.PercentInGrid = GetPercentInGrid(mDragingNode.ValueInGrid);
                            mDragingNode.Value = GetRealValueByPointInGrid(mDragingNode.ValueInGrid);
                            AnimationAssetMove(this, mDragingNode);
                        }
                    }
                    else
                    {
                        //InValidedNode
                        if (mInValidedNodesList.Contains(mDragingNode))
                        {
                            mDragingNode.ValueInGrid = currentPoint;
                            CheckAndCorrectContenNameSide(mDragingNode);
                            mDragingNode.PercentInGrid = GetPercentInGrid(mDragingNode.ValueInGrid);
                            mDragingNode.Value = GetRealValueByPointInGrid(mDragingNode.ValueInGrid);
                            mDragingNode.IsValided = true;
                            mDragingNode.Index = mValidedNodesList.Count;
                            mValidedNodesList.Add(mDragingNode);
                            mInValidedNodesList.Remove(mDragingNode);
                            AnimationAssetDrop(this, mDragingNode);
                        }
                    }
                }
            }

        }
        private void BlendCanvas_Drop(object sender, DragEventArgs e)
        {
            EnableNodesHitVisiable(true);
            if (mPreViewNode != null)
            {
                RemoveChild(mPreViewNode.NodeCtrl.Parent, mPreViewNode.NodeCtrl);
                mPreViewNode = null;
            }
            var pos = e.GetPosition(MainGrid);
            e.Handled = true;
            var canvasPos = e.GetPosition(BlendCanvas);
            var currentPoint = GetPointInGrid((float)canvasPos.X, (float)canvasPos.Y);
            var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            var info = dragObject[0] as Resources.ResourceInfo;
            if (info != null)
            {
                var node = CreateNode();
                node.Name = info.ResourceName;
                node.ValueInGrid = currentPoint;
                CheckAndCorrectContenNameSide(node);
                node.PercentInGrid = GetPercentInGrid(node.ValueInGrid);
                node.Index = mValidedNodesList.Count;
                node.Value = GetRealValueByPointInGrid(node.ValueInGrid);
                mValidedNodesList.Add(node);
                BlendCanvas.Children.Add(node.NodeCtrl);
                AnimationAssetDrop(this, node);
            }
        }
        private void BlendCanvas_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        bool mIsSelectIndicator = false;
        private void IndicatorCtrl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(mCurrentBlendIndicator.IndicatorCtrl);
            var halfX = mCurrentBlendIndicator.IndicatorCtrl.ActualWidth;
            var halfY = mCurrentBlendIndicator.IndicatorCtrl.ActualHeight;
            if (pos.X >= -halfX && pos.X < halfX &&
                pos.Y >= -halfY && pos.Y < halfY)
            {
                mIsSelectIndicator = true;
            }
        }
        private void IndicatorCtrl_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(BlendCanvas);
            if (mIsSelectIndicator)
            {
                var finalPos = Vector3.Zero;
                finalPos.X = (float)pos.X;
                if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
                {
                    finalPos.Y = (float)BlendCanvas.ActualHeight * 0.5f;
                }
                if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_2D)
                {
                    finalPos.Y = (float)pos.Y;
                }
                mCurrentBlendIndicator.Value = finalPos;
                mCurrentBlendIndicator.Percent = GetPercentInGrid(mCurrentBlendIndicator.Value);
            }
        }
        private void IndicatorCtrl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mIsSelectIndicator = false;
        }
        private void BlendCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void BlendCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mIsSelectIndicator = false;
        }
        private void BlendCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(BlendCanvas);
            if (mIsSelectIndicator)
            {
                var finalPos = Vector3.Zero;
                finalPos.X = (float)pos.X;
                if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
                {
                    finalPos.Y = (float)BlendCanvas.ActualHeight * 0.5f;
                }
                if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_2D)
                {
                    finalPos.Y = (float)pos.Y;
                }
                mCurrentBlendIndicator.Value = finalPos;
                mCurrentBlendIndicator.Percent = GetPercentInGrid(mCurrentBlendIndicator.Value);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    var posInGrid = Vector3.Zero;
                    posInGrid.X = (float)pos.X;
                    if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
                    {
                        posInGrid.Y = (float)BlendCanvas.ActualHeight * 0.5f;
                    }
                    if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_2D)
                    {
                        posInGrid.Y = (float)pos.Y;
                    }
                    var margin = new System.Windows.Thickness();
                    margin.Left = (posInGrid.X);
                    margin.Top = (posInGrid.Y);
                    mCurrentBlendIndicator.Value = posInGrid;
                    mCurrentBlendIndicator.Percent = GetPercentInGrid(mCurrentBlendIndicator.Value);
                }
            }
        }
        private void CurrentBlendIndicator_OnValueChange(object sender, BlendSpaceIndicator.ValueEventArgs e)
        {
            if (mAnimationBlendSpace == null)
                return;
            var pos = e.Value;
            var realWidth = XMax - XMin;
            var percent = (float)(pos.X / BlendCanvas.ActualWidth);
            if (float.IsNaN(percent) || float.IsInfinity(percent))
                percent = 0;
            var realX = percent * realWidth + XMin;
            var input = Vector3.Zero;
            input.X = realX;
            if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_1D)
            {
                mAnimationBlendSpace.Input = input;
            }
            if (mBlendSpaceDimension == AnimationBlendSpaceDimension.ABSD_2D)
            {
                var realHeight = YMax - YMin;
                percent = 1 - (float)(pos.Y / BlendCanvas.ActualHeight);
                if (float.IsNaN(percent) || float.IsInfinity(percent))
                    percent = 0;
                var realY = percent * realHeight + YMin;
                input.Y = realY;
                mAnimationBlendSpace.Input = input;
            }
            AnimationPlayingCtrl.Duration = mAnimationBlendSpace.Duration;
            AnimationPlayingCtrl.TotalFrame = mAnimationBlendSpace.Duration * 30;
        }

        private void HideLabelBtn_Checked(object sender, RoutedEventArgs e)
        {
            HideContent = true;
        }

        private void HideLabelBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            HideContent = false;
        }

        private void BlendCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            var canvas = sender as Canvas;
            mCurrentBlendIndicator.Value = GetPointInGrid(0, (float)canvas.ActualHeight * 0.5f);
            mCurrentBlendIndicator.Percent = GetPercentInGrid(mCurrentBlendIndicator.Value);
        }
    }
}
