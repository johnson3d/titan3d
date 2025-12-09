using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Support
{
    public interface IRemapCurve
    {
        public float Evaluate(float value);
    }
    public class TtBezierCurve : IRemapCurve
    {
        public List<BezierPointBase> BezierPoints { get; set; } = new List<BezierPointBase>();
        public bool IsScaleValue { get; set; } = false;
        public float DefaultControlPointExtension { get; set; } = 0.1f;
        float mMinX = 0.0f;
        public float MinX
        {
            get => mMinX;
            set
            {
                if (value == mMinX || value >= mMaxX)
                    return;

                if (IsScaleValue)
                {
                    var deltaX = (mMaxX - value) / (mMaxX - mMinX);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2((pt.Position.X - mMinX) * deltaX + value, pt.Position.Y);
                        pt.ControlPoint = new Vector2((pt.ControlPoint.X - mMinX) * deltaX + value, pt.ControlPoint.Y);
                    }
                }
                mMinX = value;
                DefaultControlPointExtension = (mMaxX - mMinX) * 0.1f;
            }
        }
        float mMinY = 0.0f;
        public float MinY
        {
            get => mMinY;
            set
            {
                if (value == mMinY || value >= mMaxY)
                    return;

                if (IsScaleValue)
                {
                    var deltaY = (mMaxY - value) / (mMaxY - mMinY);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2(pt.Position.X, (pt.Position.Y - mMinY) * deltaY + value);
                        pt.ControlPoint = new Vector2(pt.ControlPoint.X, (pt.ControlPoint.Y - mMinY) * deltaY + value);
                    }
                }
                mMinY = value;
            }
        }
        float mMaxX = 1.0f;
        public float MaxX
        {
            get => mMaxX;
            set
            {
                if (value == mMaxX || value <= mMinX)
                    return;

                if (IsScaleValue)
                {
                    var deltaX = (value - mMinX) / (mMaxX - mMinX);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2((pt.Position.X - mMinX) * deltaX + mMinX, pt.Position.Y);
                        pt.ControlPoint = new Vector2((pt.ControlPoint.X - mMinX) * deltaX + mMinX, pt.ControlPoint.Y);
                    }
                }
                mMaxX = value;
                DefaultControlPointExtension = (mMaxX - mMinX) * 0.1f;
            }
        }
        float mMaxY = 1.0f;
        public float MaxY
        {
            get => mMaxY;
            set
            {
                if (value == mMaxY || value <= mMinY)
                    return;

                if (IsScaleValue)
                {
                    var deltaY = (value - mMinY) / (mMaxY - mMinY);
                    for (int i = 0; i < BezierPoints.Count; i++)
                    {
                        var pt = BezierPoints[i];
                        pt.Position = new Vector2(pt.Position.X, (pt.Position.Y - mMinY) * deltaY + mMinY);
                        pt.ControlPoint = new Vector2(pt.ControlPoint.X, (pt.ControlPoint.Y - mMinY) * deltaY + mMinY);
                    }
                }
                mMaxY = value;
            }
        }
        public float Evaluate(float value)
        {
            return BezierCalculate.ValueOnBezier(BezierPoints, value).Y;
        }
    }
}
