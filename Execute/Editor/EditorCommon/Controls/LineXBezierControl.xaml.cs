using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EditorCommon.Controls
{
    /// <summary>
    /// Interaction logic for LineXBezierControl.xaml
    /// </summary>
    public partial class LineXBezierControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public delegate void Delegate_OnDirtyChanged(bool dirty);
        public Delegate_OnDirtyChanged OnDirtyChanged;

        [EngineNS.Rtti.MetaClass]
        public class BezierPoint : EngineNS.BezierPointBase
        {
            public Ellipse PositionEllipse = null;
            public Ellipse ControlPointEllipse = null;
            public Line ControlPointLine = null;

            LineXBezierControl mHostControl = null;

            // Y反向位置点
            Point mOpYPosition = new Point();
            public Point OpYPosition
            {
                get { return mOpYPosition; }
                set
                {
                    mOpYPosition = value;
                    Position = new EngineNS.Vector2((float)(mOpYPosition.X), (float)(mHostControl.BezierHeight - mOpYPosition.Y));
                }
            }

            // Y反向控制点
            Point mOpYControlPoint = new Point();
            public Point OpYControlPoint
            {
                get { return mOpYControlPoint; }
                set
                {
                    mOpYControlPoint = value;
                    ControlPoint = new EngineNS.Vector2((float)(mOpYControlPoint.X), (float)(mHostControl.BezierHeight - mOpYControlPoint.Y));
                }
            }

            public BezierPoint(LineXBezierControl hostControl)
            {
                mHostControl = hostControl;
            }
            public BezierPoint(LineXBezierControl hostControl, EngineNS.Vector2 pos, EngineNS.Vector2 ctrlPos)
                : base(pos, ctrlPos)
            {
                mHostControl = hostControl;
                CalculateOpposite();
            }

            public void CalculateOpposite()
            {
                mOpYPosition = new Point(Position.X, mHostControl.BezierHeight - Position.Y);
                mOpYControlPoint = new Point(ControlPoint.X, mHostControl.BezierHeight - ControlPoint.Y);
            }
        }

        bool mIsDirty = false;
        public bool IsDirty
        {
            get { return mIsDirty; }
            set
            {
                mIsDirty = value;
                if (OnDirtyChanged != null)
                    OnDirtyChanged(mIsDirty);
            }
        }

        List<EngineNS.BezierPointBase> mBezierPoints = new List<EngineNS.BezierPointBase>();
        public List<EngineNS.BezierPointBase> BezierPoints
        {
            get { return mBezierPoints; }
        }
        List<Ellipse> mBezierControlPtEllipses = new List<Ellipse>();
        List<Line> mBezierControlPtLine = new List<Line>();
        List<Ellipse> mBezierPtEllipses = new List<Ellipse>();

        double mBezierWidth = 300;
        public double BezierWidth
        {
            get { return mBezierWidth; }
            set
            {
                mBezierWidth = value;
                OnPropertyChanged("BezierWidth");
            }
        }
        double mBezierHeight = 200;
        public double BezierHeight
        {
            get { return mBezierHeight; }
            set
            {
                mBezierHeight = value;
                OnPropertyChanged("BezierHeight");
            }
        }

        public LineXBezierControl()
        {
            InitializeComponent();

            // 增加起始点和结束点
            var startBPt = new BezierPoint(this)
            {
                OpYPosition = new Point(0, BezierHeight * 0.5),
                OpYControlPoint = new Point(15, BezierHeight * 0.5)
            };
            mBezierPoints.Add(startBPt);
            var endBPt = new BezierPoint(this)
            {
                OpYPosition = new Point(BezierWidth, BezierHeight * 0.5),
                OpYControlPoint = new Point(BezierWidth - 15, BezierHeight * 0.5)
            };
            mBezierPoints.Add(endBPt);

            UpdateShow();
        }

        public void Save(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.AddAttrib("_bezierPts");
            att.Version = 0;
            att.BeginWrite();
            int count = BezierPoints.Count;
            att.Write(count);
            foreach (var bPt in BezierPoints)
            {
                att.WriteMetaObject(bPt);
            }
            att.Write(mBezierWidth);
            att.Write(mBezierHeight);
            att.EndWrite();
        }

        public void Load(EngineNS.IO.XndNode xndNode)
        {
            BezierPoints.Clear();
            var att = xndNode.FindAttrib("_bezierPts");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            int count;
                            att.Read(out count);
                            for(int i=0; i<count; i++)
                            {
                                var bPt = att.ReadMetaObject() as BezierPoint;
                                if(bPt != null)
                                {
                                    bPt.CalculateOpposite();
                                    BezierPoints.Add(bPt);
                                }
                            }
                            att.Read(out mBezierWidth);
                            att.Read(out mBezierHeight);
                        }
                        break;
                }
                att.EndRead();
            }
        }


        #region 更新绘制

        public void UpdateShow()
        {
            //switch (ControlMode)
            //{
            //    case enControlMode.Bezier:
            //        {
            //            UpdateKeyPoint(true);
            //            UpdateBezierShow();
            //        }
            //        break;
            //    case enControlMode.LineXBezier:
                    {
                        UpdateKeyPoint(true);
                        UpdateLineXBezierShow();
                    }
                    //break;
            //}
        }

        // Y轴为贝塞尔曲线，X轴为线性
        private void UpdateLineXBezierShow()
        {
            if (Polyline_LineXBezier.Visibility != Visibility.Visible)
                return;

            Polyline_LineXBezier.Points.Clear();

            if (mBezierPoints.Count < 2)
                return;

            for (double x = 0; x < BezierWidth; x += 1)
            {
                var bezierPos = EngineNS.BezierCalculate.ValueOnBezier(mBezierPoints, x);
                Polyline_LineXBezier.Points.Add(new System.Windows.Point(x, BezierHeight - bezierPos.Y));
            }

            var posX = mBezierPoints[mBezierPoints.Count - 1].Position.X - 0.01;
            var bPos = EngineNS.BezierCalculate.ValueOnBezier(mBezierPoints, posX);
            Polyline_LineXBezier.Points.Add(new System.Windows.Point(posX, BezierHeight - bPos.Y));
        }

        //// 标准贝塞尔曲线
        //private void UpdateBezierShow()
        //{
        //    // todo: 处理反向问题

        //    if (BezierPath.Visibility != Visibility.Visible)
        //        return;

        //    BezierPathFigure.Segments.Clear();

        //    if (mBezierPoints.Count == 0)
        //        return;

        //    BezierPathFigure.StartPoint = mBezierPoints[0].Position;
        //    for (int i = 0; i < mBezierPoints.Count; i++)
        //    {
        //        if (i < mBezierPoints.Count - 1 && (i % 2 == 0))
        //        {
        //            var seg = new BezierSegment();
        //            seg.Point1 = mBezierPoints[i].ControlPoint;
        //            seg.Point2 = mBezierPoints[i + 1].ControlPoint;
        //            seg.Point3 = mBezierPoints[i + 1].Position;
        //            BindingOperations.SetBinding(seg, BezierSegment.Point1Property, new Binding("ControlPoint") { Source = mBezierPoints[i] });
        //            BindingOperations.SetBinding(seg, BezierSegment.Point2Property, new Binding("ControlPoint") { Source = mBezierPoints[i + 1] });
        //            BindingOperations.SetBinding(seg, BezierSegment.Point3Property, new Binding("Position") { Source = mBezierPoints[i + 1] });
        //            BezierPathFigure.Segments.Add(seg);
        //        }
        //    }
        //}

        private void UpdateKeyPoint(bool withControlPoint = false)
        {
            ControlLineGeoGroup.Children.Clear();

            foreach (var ellipse in mBezierControlPtEllipses)
            {
                MainCanvas.Children.Remove(ellipse);
            }
            mBezierControlPtEllipses.Clear();
            foreach(var line in mBezierControlPtLine)
            {
                MainCanvas.Children.Remove(line);
            }
            mBezierControlPtLine.Clear();
            foreach (var ellipse in mBezierPtEllipses)
            {
                MainCanvas.Children.Remove(ellipse);
            }
            mBezierPtEllipses.Clear();

            for (int i = 0; i < mBezierPoints.Count; i++)
            {
                var bzPoint = mBezierPoints[i] as BezierPoint;

                if (withControlPoint)
                {
                    var pos = bzPoint.OpYPosition;
                    var ctrlPos = bzPoint.OpYControlPoint;
                    var lineGeo = new LineGeometry(pos, ctrlPos);
                    if (i % 2 == 0 && i != 0)
                        BindingOperations.SetBinding(lineGeo, LineGeometry.StartPointProperty, new Binding("OpYPosition") { Source = mBezierPoints[i - 1] });
                    else
                        BindingOperations.SetBinding(lineGeo, LineGeometry.StartPointProperty, new Binding("OpYPosition") { Source = bzPoint });
                    BindingOperations.SetBinding(lineGeo, LineGeometry.EndPointProperty, new Binding("OpYControlPoint") { Source = bzPoint });
                    ControlLineGeoGroup.Children.Add(lineGeo);

                    var ctrlPtEl = new Ellipse()
                    {
                        Fill = this.FindResource("ControlPointColor") as Brush,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Tag = bzPoint,
                        Width = 8,
                        Height = 8,
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new TranslateTransform(-4, -4),
                    };
                    Canvas.SetLeft(ctrlPtEl, bzPoint.OpYControlPoint.X);
                    Canvas.SetTop(ctrlPtEl, bzPoint.OpYControlPoint.Y);
                    Canvas.SetZIndex(ctrlPtEl, 1);
                    ((BezierPoint)bzPoint).ControlPointEllipse = ctrlPtEl;
                    mBezierControlPtEllipses.Add(ctrlPtEl);
                    MainCanvas.Children.Add(ctrlPtEl);
                    ctrlPtEl.MouseDown += ControlPointEllipse_MouseDown;
                    ctrlPtEl.MouseMove += ControlPointEllipse_MouseMove;
                    ctrlPtEl.MouseUp += ControlPointEllipse_MouseUp;
                    ctrlPtEl.MouseEnter += ControlPointEllipse_MouseEnter;
                    ctrlPtEl.MouseLeave += ControlPointEllipse_MouseLeave;

                    var ctrlLine = new Line()
                    {
                        Stroke = this.FindResource("ControlPointColor") as Brush,
                        StrokeThickness = 1,
                        IsHitTestVisible = false,
                    };
                    ctrlLine.X1 = bzPoint.OpYControlPoint.X;
                    ctrlLine.Y1 = bzPoint.OpYControlPoint.Y;
                    ctrlLine.X2 = bzPoint.OpYPosition.X;
                    ctrlLine.Y2 = bzPoint.OpYPosition.Y;
                    ((BezierPoint)bzPoint).ControlPointLine = ctrlLine;
                    mBezierControlPtLine.Add(ctrlLine);
                    MainCanvas.Children.Add(ctrlLine);
                }

                if ((i % 2 != 0) || i == 0 || (i == (mBezierPoints.Count - 1)))
                {
                    var ptEl = new Ellipse()
                    {
                        Fill = this.FindResource("BezierPointColor") as Brush,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                        Tag = bzPoint,
                        Width = 10,
                        Height = 10,
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new TranslateTransform(-5, -5)
                    };
                    Canvas.SetZIndex(ptEl, 1);
                    Canvas.SetLeft(ptEl, bzPoint.OpYPosition.X);
                    Canvas.SetTop(ptEl, bzPoint.OpYPosition.Y);
                    ((BezierPoint)bzPoint).PositionEllipse = ptEl;
                    mBezierPtEllipses.Add(ptEl);
                    MainCanvas.Children.Add(ptEl);
                    ptEl.MouseDown += BezierPointEllipse_MouseDown;
                    ptEl.MouseMove += BezierPointEllipse_MouseMove;
                    ptEl.MouseUp += BezierPointEllipse_MouseUp;
                    ptEl.MouseEnter += BezierPointEllipse_MouseEnter;
                    ptEl.MouseLeave += BezierPointEllipse_MouseLeave;
                }
            }

        }


        #endregion

        #region 曲线鼠标操作

        Point mMouseDownPt = new Point();
        bool mBezierPointInside = true;
        void BezierPointEllipse_MouseLeave(object sender, MouseEventArgs e)
        {
            var elp = sender as Ellipse;
            elp.Fill = this.FindResource("BezierPointColor") as Brush;
        }

        void BezierPointEllipse_MouseEnter(object sender, MouseEventArgs e)
        {
            var elp = sender as Ellipse;
            elp.Fill = this.FindResource("MousePointAtColor") as Brush;
        }

        void BezierPointEllipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mBezierPointInside == false)
            {
                var elp = sender as Ellipse;
                var bPt = elp.Tag as BezierPoint;
                RemoveBezierPoint(bPt);
            }

            Mouse.Capture(null);
            e.Handled = true;
        }

        double DeleteSizeDelta = 30;
        void BezierPointEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var elp = sender as Ellipse;
                var pt = e.GetPosition(elp);
                var deltaX = pt.X - mMouseDownPt.X;
                var deltaY = pt.Y - mMouseDownPt.Y;
                var bPt = elp.Tag as BezierPoint;

                var idx = mBezierPoints.IndexOf(bPt);
                if (idx == 0 || idx == mBezierPoints.Count - 1)
                {
                    deltaX = 0;

                    if ((bPt.OpYPosition.Y + deltaY) < 0)
                    {
                        deltaY = -bPt.OpYPosition.Y;
                    }
                    else if ((bPt.OpYPosition.Y + deltaY) > BezierHeight)
                    {
                        deltaY = BezierHeight - bPt.OpYPosition.Y;
                    }
                }
                else
                {
                    if (mBezierPointInside)
                    {
                        // 超出范围删除该点
                        if (((bPt.OpYPosition.X + deltaX) < -DeleteSizeDelta) ||
                            ((bPt.OpYPosition.X + deltaX) > BezierWidth + DeleteSizeDelta) ||
                            ((bPt.OpYPosition.Y + deltaY) < -DeleteSizeDelta) ||
                            ((bPt.OpYPosition.Y + deltaY) > BezierHeight + DeleteSizeDelta))
                        {
                            mBezierPointInside = false;
                            elp.Fill = this.FindResource("DeletePointAtColor") as Brush;
                        }
                    }
                    else
                    {
                        // 回到范围内增加该点
                        if (!(((bPt.OpYPosition.X + deltaX) < -DeleteSizeDelta) ||
                            ((bPt.OpYPosition.X + deltaX) > BezierWidth + DeleteSizeDelta) ||
                            ((bPt.OpYPosition.Y + deltaY) < -DeleteSizeDelta) ||
                            ((bPt.OpYPosition.Y + deltaY) > BezierHeight + DeleteSizeDelta)))
                        {
                            mBezierPointInside = true;
                            elp.Fill = this.FindResource("MousePointAtColor") as Brush;
                        }
                    }

                    if ((bPt.OpYPosition.X + deltaX) < 0)
                        deltaX = -bPt.OpYPosition.X;
                    else if ((bPt.OpYPosition.X + deltaX) > BezierWidth)
                        deltaX = BezierWidth - bPt.OpYPosition.X;
                    if ((bPt.OpYPosition.Y + deltaY) < 0)
                        deltaY = -bPt.OpYPosition.Y;
                    else if ((bPt.OpYPosition.Y + deltaY) > BezierHeight)
                        deltaY = BezierHeight - bPt.OpYPosition.Y;
                }

                bPt.OpYPosition = new Point(bPt.OpYPosition.X + deltaX, bPt.OpYPosition.Y + deltaY);
                bPt.OpYControlPoint = new Point(bPt.OpYControlPoint.X + deltaX, bPt.OpYControlPoint.Y + deltaY);

                if (bPt.ControlPointEllipse != null)
                {
                    Canvas.SetLeft(bPt.ControlPointEllipse, bPt.OpYControlPoint.X);
                    Canvas.SetTop(bPt.ControlPointEllipse, bPt.OpYControlPoint.Y);
                    bPt.ControlPointLine.X1 = bPt.OpYControlPoint.X;
                    bPt.ControlPointLine.Y1 = bPt.OpYControlPoint.Y;
                }

                if (idx > 0 && idx < mBezierPoints.Count - 1)
                {
                    var nbPt = mBezierPoints[idx + 1] as BezierPoint;
                    nbPt.OpYPosition = new Point(bPt.OpYPosition.X, bPt.OpYPosition.Y);
                    nbPt.OpYControlPoint = new Point(nbPt.OpYControlPoint.X + deltaX, nbPt.OpYControlPoint.Y + deltaY);
                    if (nbPt.PositionEllipse != null)
                    {
                        Canvas.SetLeft(nbPt.PositionEllipse, nbPt.OpYPosition.X);
                        Canvas.SetTop(nbPt.PositionEllipse, nbPt.OpYPosition.Y);
                    }
                    if (nbPt.ControlPointEllipse != null)
                    {
                        Canvas.SetLeft(nbPt.ControlPointEllipse, nbPt.OpYControlPoint.X);
                        Canvas.SetTop(nbPt.ControlPointEllipse, nbPt.OpYControlPoint.Y);
                    }
                    nbPt.ControlPointLine.X1 = nbPt.OpYControlPoint.X;
                    nbPt.ControlPointLine.Y1 = nbPt.OpYControlPoint.Y;
                    nbPt.ControlPointLine.X2 = nbPt.OpYPosition.X;
                    nbPt.ControlPointLine.Y2 = nbPt.OpYPosition.Y;
                }
                else if (idx == 0)
                {
                    BezierPathFigure.StartPoint = bPt.OpYPosition;
                }

                Canvas.SetLeft(elp, Canvas.GetLeft(elp) + deltaX);
                Canvas.SetTop(elp, Canvas.GetTop(elp) + deltaY);
                bPt.ControlPointLine.X1 = bPt.OpYControlPoint.X;
                bPt.ControlPointLine.Y1 = bPt.OpYControlPoint.Y;
                bPt.ControlPointLine.X2 = bPt.OpYPosition.X;
                bPt.ControlPointLine.Y2 = bPt.OpYPosition.Y;

                IsDirty = true;

                UpdateLineXBezierShow();
            }
        }

        void BezierPointEllipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mBezierPointInside = true;
                var elp = sender as Ellipse;
                mMouseDownPt = e.GetPosition(elp);
                Mouse.Capture(elp);
            }

            e.Handled = true;
        }

        void ControlPointEllipse_MouseLeave(object sender, MouseEventArgs e)
        {
            var elp = sender as Ellipse;
            elp.Fill = this.FindResource("ControlPointColor") as Brush;
        }

        void ControlPointEllipse_MouseEnter(object sender, MouseEventArgs e)
        {
            var elp = sender as Ellipse;
            elp.Fill = this.FindResource("MousePointAtColor") as Brush;
        }

        void ControlPointEllipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        void ControlPointEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var elp = sender as Ellipse;
                var pt = e.GetPosition(elp);
                var deltaX = pt.X - mMouseDownPt.X;
                var deltaY = pt.Y - mMouseDownPt.Y;
                var bPt = elp.Tag as BezierPoint;

                if ((bPt.OpYControlPoint.X + deltaX) < 0)
                {
                    deltaX = -bPt.OpYControlPoint.X;
                }
                else if ((bPt.OpYControlPoint.X + deltaX) > BezierWidth)
                {
                    deltaX = BezierWidth - bPt.OpYControlPoint.X;
                }
                if ((bPt.OpYControlPoint.Y + deltaY) < 0)
                {
                    deltaY = -bPt.OpYControlPoint.Y;
                }
                else if ((bPt.OpYControlPoint.Y + deltaY) > BezierHeight)
                {
                    deltaY = BezierHeight - bPt.OpYControlPoint.Y;
                }

                var x = bPt.OpYControlPoint.X + deltaX;
                var y = bPt.OpYControlPoint.Y + deltaY;
                bPt.OpYControlPoint = new Point(x, y);
                Canvas.SetLeft(elp, Canvas.GetLeft(elp) + deltaX);
                Canvas.SetTop(elp, Canvas.GetTop(elp) + deltaY);
                bPt.ControlPointLine.X1 = x;
                bPt.ControlPointLine.Y1 = y;

                IsDirty = true;

                UpdateLineXBezierShow();
            }
        }

        void ControlPointEllipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var elp = sender as Ellipse;
                mMouseDownPt = e.GetPosition(elp);
                Mouse.Capture(elp);
            }

            e.Handled = true;
        }

        private void BezierPath_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = e.GetPosition((FrameworkElement)sender);
            Canvas.SetLeft(Ellipse_MousePoint, point.X);
            Canvas.SetTop(Ellipse_MousePoint, point.Y);
        }

        private void BezierPath_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Ellipse_MousePoint.Visibility = Visibility.Visible;
        }

        private void BezierPath_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Ellipse_MousePoint.Visibility = Visibility.Collapsed;
        }

        private void BezierPath_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var point = e.GetPosition((FrameworkElement)sender);
                AddBezierPoint(point);
            }
        }

        private void MainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (BezierPosTest.Visibility == System.Windows.Visibility.Visible)
            {
                var pos = e.GetPosition(MainCanvas);
                var bezierPos = EngineNS.BezierCalculate.ValueOnBezier(mBezierPoints, pos.X);
                Canvas.SetLeft(BezierPosTest, bezierPos.X);
                Canvas.SetTop(BezierPosTest, bezierPos.Y);
            }
        }

        #endregion

        #region 添加删除点
        private void AddBezierPoint(Point pt)
        {
            // 按顺序插入
            int idx = 0;
            foreach (BezierPoint bp in mBezierPoints)
            {
                if (bp.OpYPosition.X > pt.X)
                {
                    var bPt = new BezierPoint(this);
                    bPt.OpYPosition = pt;
                    var tempPt = pt;
                    tempPt.X += 15;
                    if (tempPt.X > BezierWidth)
                        tempPt.X = BezierWidth;
                    bPt.OpYControlPoint = tempPt;
                    mBezierPoints.Insert(idx, bPt);

                    bPt = new BezierPoint(this);
                    bPt.OpYPosition = pt;
                    tempPt = pt;
                    tempPt.X -= 15;
                    if (tempPt.X < 0)
                        tempPt.X = 0;
                    bPt.OpYControlPoint = tempPt;
                    mBezierPoints.Insert(idx, bPt);

                    break;
                }
                idx++;
            }

            IsDirty = true;

            UpdateShow();
        }

        private void RemoveBezierPoint(BezierPoint bPt)
        {
            var idx = mBezierPoints.IndexOf(bPt);
            if (idx == 0 || idx == mBezierPoints.Count - 1 || idx < 0)
                return;

            mBezierPoints.RemoveRange(idx, 2);

            IsDirty = true;

            UpdateShow();
        }

        #endregion

        private void MainCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            var width = e.NewSize.Width;// - MainCanvas.Margin.Left - MainCanvas.Margin.Right;
            if (width < MainCanvas.MinWidth)
                width = MainCanvas.MinWidth;
            if (width > MainCanvas.MaxWidth)
                width = MainCanvas.MaxWidth;
            //MainCanvas.Width = width;

            var height = e.NewSize.Height;// - MainCanvas.Margin.Top - MainCanvas.Margin.Bottom;
            if (height < MainCanvas.MinHeight)
                height = MainCanvas.MinHeight;
            if (height > MainCanvas.MaxHeight)
                height = MainCanvas.MaxHeight;
            //MainCanvas.Height = height;

            double widthDelta = 1.0;
            double heightDelta = 1.0;
            if(e.PreviousSize.Width != 0 && e.PreviousSize.Height != 0)
            {
                widthDelta = e.NewSize.Width / e.PreviousSize.Width;
                heightDelta = e.NewSize.Height / e.PreviousSize.Height;
            }
            else
            {
                widthDelta = e.NewSize.Width / BezierWidth;
                heightDelta = e.NewSize.Height / BezierHeight;
            }

            BezierWidth = width;
            BezierHeight = height;

            for (int i = 0; i < mBezierPoints.Count; i++)
            {
                var bzPoint = mBezierPoints[i] as BezierPoint;
                bzPoint.OpYPosition = new Point(bzPoint.OpYPosition.X * widthDelta, bzPoint.OpYPosition.Y * heightDelta);
                bzPoint.OpYControlPoint = new Point(bzPoint.OpYControlPoint.X * widthDelta, bzPoint.OpYControlPoint.Y * heightDelta);
            }

            UpdateKeyPoint(true);
            UpdateLineXBezierShow();
        }
    }
}
