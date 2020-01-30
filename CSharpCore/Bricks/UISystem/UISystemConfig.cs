using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UISystem
{
    public class UISystemConfig
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
        // todo: DPI缩放曲线
        EngineNS.Support.CBezier mDPICurve;
        public EngineNS.Support.CBezier DPICurve
        {
            get => mDPICurve;
        }

        public SizeF DesignSize = new SizeF(1920, 1080);

        public UISystemConfig()
        {
            ResetDefault();
        }

        public void ResetDefault()
        {
            DPIScaleMode = enDPIScaleMode.ShortestSide;
            mDPICurve = new Support.CBezier();
            var pos = new Vector3(1080, 1, 0);
            //var pos = new Vector3(270, 0.25f, 0);
            var ctrlPos = new Vector3(0, 0, 0);
            mDPICurve.AddNode(ref pos, ref ctrlPos, ref ctrlPos);
            //pos = new Vector3(8640, 8, 0);
            pos = new Vector3(8640, 8, 0);
            mDPICurve.AddNode(ref pos, ref ctrlPos, ref ctrlPos);
        }

        public float GetDPIScaleAndDesignSize(float width, float height, out SizeF tagDesignSize)
        {
            float processValue = 0;
            bool useWidth = true;
            switch(DPIScaleMode)
            {
                case enDPIScaleMode.ShortestSide:
                    if(width < height)
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
                    if(width > height)
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
            float begin, end;
            mDPICurve.GetValueRangeX(out begin, out end);
            if (processValue < begin)
                processValue = 0;
            else if (processValue > end)
                processValue = 1;
            else
            {
                processValue = (processValue - begin) / (end - begin);
            }
            var retValue = mDPICurve.GetValue(processValue).Y;
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
