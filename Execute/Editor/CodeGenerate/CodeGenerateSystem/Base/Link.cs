using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CodeGenerateSystem.Base
{
    public partial class LinkInfo : DependencyObject
    {
        public static LinkInfo CreateLinkInfo(enLinkCurveType type, Canvas drawCanvas, LinkPinControl startObj, LinkPinControl endObj)
        {
            LinkInfo linkInfo = null;
            switch(type)
            {
                case enLinkCurveType.Bezier:
                    {
                        linkInfo = new LinkInfo(drawCanvas, startObj, endObj);
                    }
                    break;
                case enLinkCurveType.Line:
                    {
                        linkInfo = new LinkInfo(drawCanvas, startObj, endObj);
                    }
                    break;
                case enLinkCurveType.BrokenLine:
                    {
                        linkInfo = new ClickableLinkInfo(drawCanvas, startObj, endObj);
                    }
                    break;
                default:
                    {
                        linkInfo = new LinkInfo(drawCanvas, startObj, endObj);
                    }
                    break;
            }
            return linkInfo;
        }
        // 贝塞尔曲线绘制部分
        protected Canvas m_drawCanvas;

        enLinkCurveType mLinkCurveType = enLinkCurveType.Bezier;
        public enLinkCurveType LinkCurveType
        {
            get => mLinkCurveType;
            set => mLinkCurveType = value;
        }
        protected EditorCommon.Controls.Curves.CurveBase m_LinkPath = null;
        public EditorCommon.Controls.Curves.CurveBase LinkPath
        {
            get { return m_LinkPath; }
        }


        public double Thickness
        {
            get { return m_LinkPath.StrokeThickness; }
            set { m_LinkPath.StrokeThickness = value; }
        }

        public double Offset
        {
            get { return m_LinkPath.StrokeDashOffset; }
            set { m_LinkPath.StrokeDashOffset = value; }
        }

        public Visibility Visible
        {
            get { return m_LinkPath.Visibility; }
            set
            {
                if (m_LinkPath != null)
                {
                    m_LinkPath.Visibility = value;
                    if (m_LinkPath.Visibility == Visibility.Visible)
                        UpdateLink();
                }
            }
        }
        public DoubleCollection DashArray
        {
            get { return m_LinkPath.StrokeDashArray; }
            set
            {
                m_LinkPath.StrokeDashArray = value;
            }
        }
        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(LinkInfo),
                                    new FrameworkPropertyMetadata(Brushes.Gray, new PropertyChangedCallback(OnColorChanged)));
        public static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var info = d as LinkInfo;
            var newValue = e.NewValue as Brush;
            if (info.m_LinkPath != null)
            {
                if (newValue == null)
                    info.m_LinkPath.Stroke = Brushes.White;
                else
                    info.m_LinkPath.Stroke = newValue;
            }
        }

        public void Hidden()
        {
            Visible = Visibility.Hidden;
        }

        partial void LinkInfoConstruction(Canvas drawCanvas)
        {
            m_drawCanvas = drawCanvas;
            if (m_drawCanvas != null)
            {
                // todo m_LinkPath.Opacity 绑定到NodesContainer里的属性上，以便修改连线变暗等效果
                if (m_linkFromObjectInfo.LinkCurveType == enLinkCurveType.Bezier)
                {
                    mLinkCurveType = enLinkCurveType.Bezier;
                    m_LinkPath = new EditorCommon.Controls.Curves.BezierCurve();
                }
                else if (m_linkFromObjectInfo.LinkCurveType == enLinkCurveType.Line)
                {
                    mLinkCurveType = enLinkCurveType.Line;
                    m_LinkPath = new ArrowLine();
                    var lineCurve = m_LinkPath as ArrowLine;

                    lineCurve.HalfArrows = EditorCommon.Controls.Curves.ArrowHalf.Both;
                    lineCurve.ArrowEnds = EditorCommon.Controls.Curves.ArrowEnds.End;
                }
                else if (m_linkFromObjectInfo.LinkCurveType == enLinkCurveType.BrokenLine)
                {
                    mLinkCurveType = enLinkCurveType.BrokenLine;
                    m_LinkPath = new EditorCommon.Controls.Curves.BrokenLine();
                    var lineCurve = m_LinkPath as EditorCommon.Controls.Curves.BrokenLine;
                    lineCurve.HalfArrows = EditorCommon.Controls.Curves.ArrowHalf.Both;
                    lineCurve.ArrowEnds = EditorCommon.Controls.Curves.ArrowEnds.End;
                }
                m_LinkPath.Visibility = Visibility.Hidden;
                m_LinkPath.Stroke = Brushes.Gray;
                Canvas.SetZIndex(m_LinkPath, -1);
                if (m_linkFromObjectInfo.LinkCurveType == enLinkCurveType.BrokenLine)
                {
                    Canvas.SetZIndex(m_LinkPath, 5);
                    m_LinkPath.IsHitTestVisible = true;
                    m_LinkPath.Stroke = Brushes.GhostWhite;
                }
                m_LinkPath.StrokeThickness = 3;

                m_drawCanvas.Children.Add(m_LinkPath);
            }
        }
        partial void SetColor(LinkPinControl pinCtrl)
        {
            BindingOperations.ClearBinding(this, ColorProperty);
            BindingOperations.SetBinding(this, ColorProperty, new Binding("BackBrush") { Source = pinCtrl });
        }
        partial void RemoveLinkPath()
        {
            if (m_drawCanvas != null)
                m_drawCanvas.Children.Remove(m_LinkPath);
        }
        partial void SetVisible(Visibility visible)
        {
            Visible = Visibility.Visible;
        }

        // 获得当前链接的两个位置点
        public void GetLinkPoints(out Point pt1, out Point pt2)
        {
            pt1 = new Point();
            pt2 = pt1;

            if (m_linkFromObjectInfo == null || m_linkToObjectInfo == null)
            {
                return;
            }

            pt1 = m_linkFromObjectInfo.TranslatePoint(m_linkFromObjectInfo.LinkElementOffset, m_drawCanvas);
            pt2 = m_linkToObjectInfo.TranslatePoint(m_linkToObjectInfo.LinkElementOffset, m_drawCanvas);
        }

        // 更新连线
        public void UpdateLink()
        {
            Point pt1, pt2;
            GetLinkPoints(out pt1, out pt2);

            if (mLinkCurveType == enLinkCurveType.Bezier)
            {
                var bezierCurve = m_LinkPath as EditorCommon.Controls.Curves.BezierCurve;
                bezierCurve.StartPoint = pt1;
                bezierCurve.EndPoint = pt2;
                double delta = Math.Max(Math.Abs(bezierCurve.EndPoint.X - bezierCurve.StartPoint.X) / 2, 25);
                delta = Math.Min(150, delta);

                switch (m_linkFromObjectInfo.BezierType)
                {
                    case enBezierType.Left:
                        bezierCurve.ControlPoint1 = new Point(bezierCurve.StartPoint.X - delta, bezierCurve.StartPoint.Y);
                        break;
                    case enBezierType.Right:
                        bezierCurve.ControlPoint1 = new Point(bezierCurve.StartPoint.X + delta, bezierCurve.StartPoint.Y);
                        break;
                    case enBezierType.Top:
                        bezierCurve.ControlPoint1 = new Point(bezierCurve.StartPoint.X, bezierCurve.StartPoint.Y - delta);
                        break;
                    case enBezierType.Bottom:
                        bezierCurve.ControlPoint1 = new Point(bezierCurve.StartPoint.X, bezierCurve.StartPoint.Y + delta);
                        break;
                }
                switch (m_linkToObjectInfo.BezierType)
                {
                    case enBezierType.Left:
                        bezierCurve.ControlPoint2 = new Point(bezierCurve.EndPoint.X - delta, bezierCurve.EndPoint.Y);
                        break;
                    case enBezierType.Right:
                        bezierCurve.ControlPoint2 = new Point(bezierCurve.EndPoint.X + delta, bezierCurve.EndPoint.Y);
                        break;
                    case enBezierType.Top:
                        bezierCurve.ControlPoint2 = new Point(bezierCurve.EndPoint.X, bezierCurve.EndPoint.Y - delta);
                        break;
                    case enBezierType.Bottom:
                        bezierCurve.ControlPoint2 = new Point(bezierCurve.EndPoint.X, bezierCurve.EndPoint.Y + delta);
                        break;
                }
            }
            if (mLinkCurveType == enLinkCurveType.Line)
            {
                //var LineCurve = m_LinkPath as EditorCommon.Controls.Curves.LineWithText;
                //pt1 = m_linkFromObjectInfo.TranslatePoint(new Point(0, 0), m_drawCanvas);
                //pt2 = m_linkToObjectInfo.TranslatePoint(new Point(0, 0), m_drawCanvas);
                ////LineCurve.StartPoint = GetLinePoint(m_linkFromObjectInfo,pt2);
                ////LineCurve.EndPoint = GetLinePoint(m_linkToObjectInfo, pt1);
                //LineCurve.StartPoint = pt1;
                //LineCurve.EndPoint = pt2;
                UpdateLineCurve();
            }
            if (mLinkCurveType == enLinkCurveType.BrokenLine)
            {
                UpdateBrokenLineCurve(pt1, pt2);
            }
        }
        void UpdateBrokenLineCurve(Point startPoint, Point endPoint)
        {
            var brokenLine = m_LinkPath as EditorCommon.Controls.Curves.BrokenLine;
            var startNode = m_linkFromObjectInfo.HostNodeControl;
            //if (!m_drawCanvas.Children.Contains(startNode))
            //    return;
            var width = startNode.ActualWidth;
            var height = startNode.ActualHeight;
            EditorCommon.Controls.Curves.ConnectorInfo start = new EditorCommon.Controls.Curves.ConnectorInfo();
            var startLT = new Point();
            if (m_drawCanvas.IsAncestorOf(startNode))
            {
                startLT  = startNode.TransformToAncestor(m_drawCanvas).Transform(new Point(0, 0));
                start.Position = startNode.TransformToAncestor(m_drawCanvas).Transform(new Point(width / 2, height / 2));
            }
            start.DesignerItemLeft = startLT.X;
            start.DesignerItemTop = startLT.Y;
            start.DesignerItemSize = startNode.RenderSize;
            start.Orientation = (EditorCommon.Controls.Curves.eLinkOrientation)m_linkFromObjectInfo.BezierType;

            var endNode = m_linkToObjectInfo.HostNodeControl;
            width = endNode.ActualWidth;
            height = endNode.ActualHeight;
            EditorCommon.Controls.Curves.ConnectorInfo end = new EditorCommon.Controls.Curves.ConnectorInfo();
            end.DesignerItemLeft = Canvas.GetLeft(endNode);
            end.DesignerItemTop = Canvas.GetTop(endNode);
            end.DesignerItemSize = endNode.RenderSize;
            if (m_drawCanvas.IsAncestorOf(endNode))
            {
                 var endLT = endNode.TransformToAncestor(m_drawCanvas).Transform(new Point(0, 0));
                end.DesignerItemLeft = endLT.X;
                end.DesignerItemTop = endLT.Y;
                end.Position = endNode.TransformToAncestor(m_drawCanvas).Transform(new Point(width / 2, height / 2));
            }
            end.Orientation = (EditorCommon.Controls.Curves.eLinkOrientation)m_linkToObjectInfo.BezierType;
            var points = EditorCommon.Controls.Curves.PathFinder.GetConnectionLine(start, end, true);
            points.RemoveAt(points.Count - 1);
            points.RemoveAt(0);
            brokenLine.StartPoint = startPoint;
            brokenLine.ConnectionPoints = points;
            brokenLine.EndPoint = endPoint;
        }
        //控件位置

        void UpdateLineCurve()
        {
            var lineCurve = m_LinkPath as ArrowLine;
            var pt1 = m_linkFromObjectInfo.TranslatePoint(new Point(0, 0), m_drawCanvas);
            var pt2 = m_linkToObjectInfo.TranslatePoint(new Point(0, 0), m_drawCanvas);
            //LineCurve.StartPoint = GetLinePoint(m_linkFromObjectInfo,pt2);
            //LineCurve.EndPoint = GetLinePoint(m_linkToObjectInfo, pt1);

            //x
            var fromLinkCtrlWidth = m_linkFromObjectInfo.ActualWidth;
            var toLinkCtrlWidth = m_linkToObjectInfo.ActualWidth;
            var fromLinkCtrlXStart = pt1.X;
            var toLinkCtrlXStart = pt2.X;
            var fromLinkCtrlXEnd = pt1.X + m_linkFromObjectInfo.ActualWidth;
            var toLinkCtrlXEnd = pt2.X + m_linkToObjectInfo.ActualWidth;
            //中心对齐to的位置
            var centerToLinkCtrlX = fromLinkCtrlXStart + (fromLinkCtrlWidth - toLinkCtrlWidth) * 0.5f;

            double xFrom = 0;
            double xTo = 0;
            xFrom = fromLinkCtrlXStart + fromLinkCtrlWidth * 0.5f + (toLinkCtrlXStart - centerToLinkCtrlX) * 0.5f;
            xTo = centerToLinkCtrlX + toLinkCtrlWidth *0.5f +(toLinkCtrlXStart - centerToLinkCtrlX) * 0.5f;
            if (xFrom > fromLinkCtrlXEnd)
            {
                xFrom = fromLinkCtrlXEnd;
            }
            if(xFrom< fromLinkCtrlXStart )
            {
                xFrom = fromLinkCtrlXStart;
            }
            if (xTo < toLinkCtrlXStart)
            {
                xTo = toLinkCtrlXStart;
            }
            if(xTo > toLinkCtrlXEnd)
            {
                xTo = toLinkCtrlXEnd;
            }


            //if (fromXStart > toXEnd)
            //{
            //    xFrom = fromXStart;
            //    xTo = toXEnd;
            //}
            //else if (fromXEnd < toXStart)
            //{
            //    xFrom = fromXEnd;
            //    xTo = toXStart;
            //}
            //else
            //{
            //    var p1 = Math.Max(fromXStart, toXStart);
            //    var p2 = Math.Min(fromXEnd, toXEnd);
            //    xFrom = xTo = (p1 + p2) * 0.5f;

            //}
            var fromLinkCtrlHeight = m_linkFromObjectInfo.ActualHeight;
            var toLinkCtrlHeight = m_linkToObjectInfo.ActualHeight;
            var fromLinkCtrlYStart = pt1.Y;
            var fromLinkCtrlYEnd = pt1.Y + m_linkFromObjectInfo.ActualHeight;
            var toLinkCtrlYStart = pt2.Y;
            var toLinkCtrlYEnd = pt2.Y + m_linkToObjectInfo.ActualHeight;
            double yFrom = 0;
            double yTo = 0;

            //中心对齐to的位置
            var centerToLinkCtrlY = fromLinkCtrlYStart + (fromLinkCtrlHeight - toLinkCtrlHeight) * 0.5f;

            yFrom = fromLinkCtrlYStart + fromLinkCtrlHeight * 0.5f + (toLinkCtrlYStart - centerToLinkCtrlY) * 0.5f;
            yTo = centerToLinkCtrlY + toLinkCtrlHeight * 0.5f + (toLinkCtrlYStart - centerToLinkCtrlY) * 0.5f;
            if (yFrom > fromLinkCtrlYEnd)
            {
                yFrom = fromLinkCtrlYEnd;
            }
            if (yFrom < fromLinkCtrlYStart)
            {
                yFrom = fromLinkCtrlYStart;
            }
            if (yTo < toLinkCtrlYStart)
            {
                yTo = toLinkCtrlYStart;
            }
            if (yTo > toLinkCtrlYEnd)
            {
                yTo = toLinkCtrlYEnd;
            }
            //if (fromLinkCtrlYStart > toLinkCtrlYEnd)
            //{
            //    yFrom = fromLinkCtrlYStart;
            //    yTo = toLinkCtrlYEnd;
            //}
            //else if (fromLinkCtrlYEnd < toLinkCtrlYStart)
            //{
            //    yFrom = fromLinkCtrlYEnd;
            //    yTo = toLinkCtrlYStart;
            //}
            //else
            //{
            //    var p1 = Math.Max(fromLinkCtrlYStart, toLinkCtrlYStart);
            //    var p2 = Math.Min(fromLinkCtrlYEnd, toLinkCtrlYEnd);
            //    yFrom = yTo = (p1 + p2) * 0.5f;
            //}

            lineCurve.StartPoint = new Point(xFrom, yFrom);
            lineCurve.EndPoint = new Point(xTo, yTo);
            lineCurve.UpdateTransitionPos();
            //var finalStartPoint = new Vector();
            //var finalEndPoint = new Vector();
            //var oldStartPoint = lineCurve.StartPoint;
            //var newStartPoint = GetLinePoint(m_linkFromObjectInfo, pt2);
            //finalStartPoint += (newStartPoint - oldStartPoint) * 0.5f;
            //pt1 -= (newStartPoint - oldStartPoint) * 0.5f;

            //var oldEndPoint = lineCurve.EndPoint;
            //var newEndPoint = GetLinePoint(m_linkToObjectInfo, pt1);
            //finalEndPoint += (newEndPoint - oldEndPoint) * 0.5f;
            //finalStartPoint -= (newEndPoint - oldEndPoint) * 0.5f;


            //lineCurve.StartPoint = lineCurve.StartPoint + finalStartPoint;
            //lineCurve.EndPoint = newEndPoint;
        }

        Point GetLinePoint(LinkPinControl linkObj, Point tagPt)
        {
            var width = linkObj.ActualWidth;
            var height = linkObj.ActualHeight;
            var objLocPoint = m_drawCanvas.TranslatePoint(tagPt, linkObj);
            var point = tagPt;
            if (objLocPoint.X < 0)
            {
                point.X = 0;
            }
            else if (objLocPoint.X > width)
            {
                point.X = width;
            }
            else
            {
                point.X = objLocPoint.X;
            }
            if (objLocPoint.Y < 0)
            {
                point.Y = 0;
            }
            else if (objLocPoint.Y > height)
            {
                point.Y = height;
            }
            else
            {
                point.Y = objLocPoint.Y;
            }
            return linkObj.TranslatePoint(point, m_drawCanvas); ;
        }
    }
}
