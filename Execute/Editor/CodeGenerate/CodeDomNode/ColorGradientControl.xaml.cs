using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("数值/颜色(ColorGradient)", "颜色节点，设置或获取颜色(时间轴)")]
    public sealed partial class ColorGradientControl
    { 
        partial void InitConstruction()
        {
            this.InitializeComponent();

            var param = CSParam as ColorGradientControlConstructParam;
            this.Width = param.Width;
            UIColorGradientWidth.Width = new GridLength(param.Width - 112);
            this.Height = 312;
            mCtrlvalue_ColorOut = value_ColorOut;
            mCtrlValueIn = ValueIn;
            mCtrlValueOutR = ValueOutR;
            mCtrlValueOutG = ValueOutG;
            mCtrlValueOutB = ValueOutB;
            mCtrlValueOutA = ValueOutA;

            //this.Width = this.ActualWidth;
            //this.Height = this.ActualHeight;
            RegisterLeftButtonEvent(PART_BORDER_LEFT);
            RegisterRightButtonEvent(PART_BORDER_Right);

        }

        partial void GetGradientDatas()
        {
            GradientDatas = new List<GradientData>();

            var GradientStopColors = UIColorGradient.GetGradientStopColors();
            //UIColorGradient
            for (int i = 0; i < GradientStopColors.Count; i++)
            {
                var data = GradientStopColors[i];
                GradientDatas.Add(new GradientData((float)data.Offset, new EngineNS.Color4(data.Color.A / 255f, data.Color.R / 255f, data.Color.G / 255f, data.Color.B / 255f)));
            }

            GradientDatas.Sort((a, b) =>
            {
                if (b.Offset > a.Offset)
                    return 1;
                if (b.Offset == a.Offset)
                    return 0;
                else
                    return -1;
            });
        }

        partial void SetGradientDatas(List<GradientData> datas)
        {
            if (datas == null)
                return;

            GradientStopCollection GradientStopColors = new GradientStopCollection();
            //UIColorGradient
            for (int i = 0; i < datas.Count; i++)
            {
                Color color = new Color();
                color.A = (Byte)(datas[i].GradientColor.Alpha * 255);
                color.R = (Byte)(datas[i].GradientColor.Red * 255);
                color.G = (Byte)(datas[i].GradientColor.Green * 255);
                color.B = (Byte)(datas[i].GradientColor.Blue * 255);
                GradientStopColors.Add(new GradientStop(color, (float)datas[i].Offset));
            }

            UIColorGradient.SetPreDatas(GradientStopColors, UIColorGradientWidth.Width.Value);
        }

        protected override void OnSizeChanged(double width, double height)
        {
            var param = CSParam as ColorGradientControlConstructParam;
            param.Width = width;
            UIColorGradientWidth.Width = new GridLength(width - 112);
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ColorGradientControl;
            GetGradientDatas();
            copyedNode.GradientDatas = new List<GradientData>(GradientDatas);
            copyedNode.SetGradientDatas(copyedNode.GradientDatas);
            return copyedNode;
        }
    }
}
