using System.Windows;
using System.Windows.Media;

namespace EditorCommon.Controls.Curves
{
    /// <summary>
    /// 带箭头的贝塞尔曲线
    /// </summary>
    public class BezierCurve : CurveBase
    {
        #region Fields

        #region DependencyProperty

        /// <summary>
        /// 控制点1
        /// </summary>
        public static readonly DependencyProperty ControlPoint1Property = DependencyProperty.Register(
            "ControlPoint1",
            typeof(Point),
            typeof(BezierCurve),
            new FrameworkPropertyMetadata(
                new Point(),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 控制点2
        /// </summary>
        public static readonly DependencyProperty ControlPoint2Property = DependencyProperty.Register(
            "ControlPoint2", typeof(Point), typeof(BezierCurve), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 结束点
        /// </summary>
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
            "EndPoint", typeof(Point), typeof(BezierCurve), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion DependencyProperty

        /// <summary>
        /// 贝塞尔曲线
        /// </summary>
        private readonly BezierSegment bezierSegment = new BezierSegment();

        #endregion Fields

        #region Properties

        /// <summary>
        /// 控制点1
        /// </summary>
        public Point ControlPoint1
        {
            get { return (Point)this.GetValue(ControlPoint1Property); }
            set { this.SetValue(ControlPoint1Property, value); }
        }

        /// <summary>
        /// 控制点2
        /// </summary>
        public Point ControlPoint2
        {
            get { return (Point)this.GetValue(ControlPoint2Property); }
            set { this.SetValue(ControlPoint2Property, value); }
        }

        /// <summary>
        /// 结束点
        /// </summary>
        public Point EndPoint
        {
            get { return (Point)this.GetValue(EndPointProperty); }
            set { this.SetValue(EndPointProperty, value); }
        }

        #endregion Properties

        #region Protected Methods

        /// <summary>
        /// 填充Figure
        /// </summary>
        /// <returns>PathSegment集合</returns>
        protected override PathSegmentCollection FillFigure()
        {
            this.bezierSegment.Point1 = this.ControlPoint1;
            this.bezierSegment.Point2 = this.ControlPoint2;
            this.bezierSegment.Point3 = this.EndPoint;

            return new PathSegmentCollection
            {
                this.bezierSegment
            };
        }

        /// <summary>
        /// 获取开始箭头处的结束点
        /// </summary>
        /// <returns>开始箭头处的结束点</returns>
        protected override Point GetStartArrowEndPoint()
        {
            return this.ControlPoint1;
        }

        /// <summary>
        /// 获取结束箭头处的开始点
        /// </summary>
        /// <returns>结束箭头处的开始点</returns>
        protected override Point GetEndArrowStartPoint()
        {
            return this.ControlPoint2;
        }

        /// <summary>
        /// 获取结束箭头处的结束点
        /// </summary>
        /// <returns>结束箭头处的结束点</returns>
        protected override Point GetEndArrowEndPoint()
        {
            return this.EndPoint;
        }

        #endregion  Protected Methods
    }
}
