using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EditorCommon.Controls.Curves
{
    /// <summary>
    /// 箭头基类
    /// </summary>
    public abstract class CurveBase : Shape
    {
        #region DependencyProperty

        /// <summary>
        /// 箭头两边夹角的依赖属性
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register(
            "ArrowAngle",
            typeof(double),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 箭头长度的依赖属性
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register(
            "ArrowLength",
            typeof(double),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 箭头所在端的依赖属性
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty = DependencyProperty.Register(
            "ArrowEnds",
            typeof(ArrowEnds),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(ArrowEnds.None, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 箭头所在端的依赖属性
        /// </summary>
        public static readonly DependencyProperty HalfArrowsProperty = DependencyProperty.Register(
            "HalfArrows",
            typeof(ArrowHalf),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(ArrowHalf.Both, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 箭头是否闭合的依赖属性
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty = DependencyProperty.Register(
            "IsArrowClosed",
            typeof(bool),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 开始点
        /// </summary>
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
            "StartPoint",
            typeof(Point),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 结束点
        /// </summary>
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
            "EndPoint",
            typeof(Point),
            typeof(CurveBase),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion DependencyProperty
        #region Fields
        /// <summary>
        /// 整个形状(包含箭头和具体形状)
        /// </summary>
        private readonly PathGeometry geometryWhole = new PathGeometry();

        /// <summary>
        /// 除去箭头外的具体形状
        /// </summary>
        protected readonly PathFigure figureConcrete = new PathFigure();

        /// <summary>
        /// 开始处的箭头线段
        /// </summary>
        protected readonly PathFigure figureStart = new PathFigure();

        /// <summary>
        /// 结束处的箭头线段
        /// </summary>
        protected readonly PathFigure figureEnd = new PathFigure();

        #endregion Fields

        #region Constructor

        /// <summary>
        /// 构造函数
        /// </summary>
        protected CurveBase()
        {
            var polyLineSegStart = new PolyLineSegment();
            this.figureStart.Segments.Add(polyLineSegStart);

            var polyLineSegEnd = new PolyLineSegment();
            this.figureEnd.Segments.Add(polyLineSegEnd);

            this.geometryWhole.Figures.Add(this.figureConcrete);
            this.geometryWhole.Figures.Add(this.figureStart);
            this.geometryWhole.Figures.Add(this.figureEnd);
        }
        #endregion Constructor

        #region Properties

        /// <summary>
        /// 箭头两边夹角
        /// </summary>
        public double ArrowAngle
        {
            get { return (double)this.GetValue(ArrowAngleProperty); }
            set { this.SetValue(ArrowAngleProperty, value); }
        }

        /// <summary>
        /// 箭头两边的长度
        /// </summary>
        public double ArrowLength
        {
            get { return (double)this.GetValue(ArrowLengthProperty); }
            set { this.SetValue(ArrowLengthProperty, value); }
        }

        /// <summary>
        /// 箭头所在端
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            get { return (ArrowEnds)this.GetValue(ArrowEndsProperty); }
            set { this.SetValue(ArrowEndsProperty, value); }
        }
        /// <summary>
        /// 箭头所在端
        /// </summary>
        public ArrowHalf HalfArrows
        {
            get { return (ArrowHalf)this.GetValue(HalfArrowsProperty); }
            set { this.SetValue(HalfArrowsProperty, value); }
        }
        /// <summary>
        /// 箭头是否闭合
        /// </summary>
        public bool IsArrowClosed
        {
            get { return (bool)this.GetValue(IsArrowClosedProperty); }
            set { this.SetValue(IsArrowClosedProperty, value); }
        }

        /// <summary>
        /// 开始点
        /// </summary>
        public Point StartPoint
        {
            get { return (Point)this.GetValue(StartPointProperty); }
            set { this.SetValue(StartPointProperty, value); }
        }
        /// <summary>
        /// 结束点
        /// </summary>
        public Point EndPoint
        {
            get { return (Point)this.GetValue(EndPointProperty); }
            set { this.SetValue(EndPointProperty, value); }
        }

        /// <summary>
        /// 定义形状
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                this.figureConcrete.StartPoint = this.StartPoint;

                // 清空具体形状,避免重复添加
                this.figureConcrete.Segments.Clear();
                var segements = this.FillFigure();
                if (segements != null)
                {
                    foreach (var segement in segements)
                    {
                        this.figureConcrete.Segments.Add(segement);
                    }
                }

                CalculateArrow();
                return this.geometryWhole;
            }
        }

        #endregion Properties

        #region Protected Methods

        /// <summary>
        /// 获取具体形状的各个组成部分
        /// </summary>
        /// <returns>PathSegment集合</returns>
        protected abstract PathSegmentCollection FillFigure();

        /// <summary>
        /// 获取开始箭头处的结束点
        /// </summary>
        /// <returns>开始箭头处的结束点</returns>
        protected abstract Point GetStartArrowEndPoint();

        /// <summary>
        /// 获取结束箭头处的开始点
        /// </summary>
        /// <returns>结束箭头处的开始点</returns>
        protected abstract Point GetEndArrowStartPoint();

        /// <summary>
        /// 获取结束箭头处的结束点
        /// </summary>
        /// <returns>结束箭头处的结束点</returns>
        protected abstract Point GetEndArrowEndPoint();

        #endregion  Protected Methods

        #region Private Methods

        /// <summary>
        /// 计算两个点之间的有向箭头
        /// </summary>
        /// <param name="pathfig">箭头所在的形状</param>
        /// <param name="startPoint">开始点</param>
        /// <param name="endPoint">结束点</param>
        protected void CalculateArrow(PathFigure pathfig, Point startPoint, Point endPoint)
        {
            var polyseg = pathfig.Segments[0] as PolyLineSegment;
            if (polyseg != null)
            {
                var matx = new Matrix();
                Vector vect = startPoint - endPoint;

                // 获取单位向量
                vect.Normalize();
                vect *= this.ArrowLength;

                // 旋转夹角的一半
                matx.Rotate(this.ArrowAngle / 2);

                // 计算上半段箭头的点
                pathfig.StartPoint = endPoint + (vect * matx);

                polyseg.Points.Clear();
                if(HalfArrows == ArrowHalf.Up || HalfArrows == ArrowHalf.Both)
                    polyseg.Points.Add(endPoint);

                matx.Rotate(-this.ArrowAngle);

                // 计算下半段箭头的点
                if (HalfArrows == ArrowHalf.Down || HalfArrows == ArrowHalf.Both)
                    polyseg.Points.Add(endPoint + (vect * matx));
            }

            pathfig.IsClosed = this.IsArrowClosed;
        }

        #endregion Private Methods
        public virtual void CalculateArrow()
        {
            // 绘制开始处的箭头
            if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
            {
                CalculateArrow(this.figureStart, this.GetStartArrowEndPoint(), this.StartPoint);
            }

            // 绘制结束处的箭头
            if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
            {
                CalculateArrow(this.figureEnd, this.GetEndArrowStartPoint(), this.GetEndArrowEndPoint());
            }
        }
    }
}
