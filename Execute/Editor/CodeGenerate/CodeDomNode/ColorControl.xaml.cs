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
    [CodeGenerateSystem.ShowInNodeList("数值/颜色(Color)", "颜色节点，设置或获取颜色")]
    public sealed partial class ColorControl
    {
        public object ColorObject
        {
            get { return GetValue(ColorObjectProperty); }
            set { SetValue(ColorObjectProperty, value); }
        }
        public static readonly DependencyProperty ColorObjectProperty =
            DependencyProperty.Register("ColorObject", typeof(object), typeof(ColorControl),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnColorObjectChanged)));

        public static void OnColorObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ColorControl;

            var newColor = (EngineNS.Color)e.NewValue;
            {
                control.ColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B));
            }

            control.IsDirty = true;
        }
        
        partial void InitConstruction()
        {
            this.InitializeComponent();

            mCtrlvalue_ColorIn = value_ColorIn;
            mCtrlvalue_ColorOut = value_ColorOut;
            mCtrlValueInR = ValueInR;
            mCtrlValueInG = ValueInG;
            mCtrlValueInB = ValueInB;
            mCtrlValueInA = ValueInA;
            mCtrlValueOutR = ValueOutR;
            mCtrlValueOutG = ValueOutG;
            mCtrlValueOutB = ValueOutB;
            mCtrlValueOutA = ValueOutA;

            var classType = mTemplateClassInstance.GetType();
            var property = classType.GetProperty("Color");
            property.SetValue(mTemplateClassInstance, EngineNS.Color.FromArgb(255, 255, 255, 255));
            BindingOperations.SetBinding(this, ColorControl.ColorObjectProperty, new Binding("Color") { Source = mTemplateClassInstance });
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ColorControl;
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            var classType = mTemplateClassInstance.GetType();
            var property = classType.GetProperty("Color");
            var color = (EngineNS.Color)property.GetValue(mTemplateClassInstance);
            copyedNode.ColorBrush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
            return copyedNode;
        }
    }
}
