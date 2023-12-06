using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI
{
    public class TtUIConfig
    {
        public enum enDPIScaleMode
        {
            ShortestSide,
            LongsetSide,
            Horizontal,
            vertical,
        }
        public enDPIScaleMode DPIScaleMode
        {
            get;
            set;
        } = enDPIScaleMode.ShortestSide;

        [Rtti.Meta]
        public List<BezierPointBase> BezierPoints = new List<BezierPointBase>();

        public TtUIConfig()
        {
            ResetDefault();
        }

        public void ResetDefault()
        {
            DPIScaleMode = enDPIScaleMode.ShortestSide;
            BezierPoints.Clear();
            BezierPoints.Add(new BezierPointBase()
            {
                Position = new Vector2(1080, 1),
                ControlPoint = new Vector2(0, 0)
            });
            BezierPoints.Add(new BezierPointBase()
            {
                Position = new Vector2(8640, 8),
                ControlPoint = new Vector2(0, 0)
            });
        }
        public float GetDPIScale(float width, float height)
        {
            float processValue = 0;
            switch (DPIScaleMode)
            {
                case enDPIScaleMode.ShortestSide:
                    if (width < height)
                    {
                        processValue = width;
                    }
                    else
                    {
                        processValue = height;
                    }
                    break;
                case enDPIScaleMode.LongsetSide:
                    if (width > height)
                    {
                        processValue = width;
                    }
                    else
                    {
                        processValue = height;
                    }
                    break;
                case enDPIScaleMode.Horizontal:
                    processValue = width;
                    break;
                case enDPIScaleMode.vertical:
                    processValue = height;
                    break;
            }
            return BezierCalculate.ValueOnBezier(BezierPoints, processValue).Y;
        }
        public float GetDPIScaleAndDesignSize(float width, float height, out SizeF tagDesignSize)
        {
            float processValue = 0;
            bool useWidth = true;
            switch (DPIScaleMode)
            {
                case enDPIScaleMode.ShortestSide:
                    if (width < height)
                    {
                        useWidth = true;
                        processValue = width;
                    }
                    else
                    {
                        useWidth = false;
                        processValue = height;
                    }
                    break;
                case enDPIScaleMode.LongsetSide:
                    if (width > height)
                    {
                        useWidth = true;
                        processValue = width;
                    }
                    else
                    {
                        useWidth = false;
                        processValue = height;
                    }
                    break;
                case enDPIScaleMode.Horizontal:
                    useWidth = true;
                    processValue = width;
                    break;
                case enDPIScaleMode.vertical:
                    useWidth = false;
                    processValue = height;
                    break;
            }
            var retValue = BezierCalculate.ValueOnBezier(BezierPoints, processValue, true).Y;
            tagDesignSize = new SizeF(width, height);
            if (useWidth)
            {
                tagDesignSize.Width = width / retValue;
                tagDesignSize.Height = tagDesignSize.Width * (height / width);
            }
            else
            {
                tagDesignSize.Height = height / retValue;
                tagDesignSize.Width = tagDesignSize.Height * (width / height);
            }
            return retValue;
        }
    }
}
