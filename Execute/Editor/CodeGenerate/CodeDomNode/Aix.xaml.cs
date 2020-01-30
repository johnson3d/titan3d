using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.CodeDom;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    public partial class Aix
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlvalue_VectorIn = value_VectorIn;
            mCtrlvalue_VectorOut = value_VectorOut;
            
        }
        
        partial void AddFloatValue_WPF(Action<CodeGenerateSystem.Base.LinkPinControl> inAction, Action<CodeGenerateSystem.Base.LinkPinControl> outAction, string keyName)
        {
            var param = CSParam as AixConstructionParams;
            var numberic = new EditorCommon.FloatNumericTypeEditor()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new System.Windows.Thickness(4, 3, 4, 3),
                MinWidth = 60,
            };
            if (param != null)
            {
                BindingOperations.SetBinding(numberic, EditorCommon.FloatNumericTypeEditor.NumericObjectProperty, new Binding(keyName) { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            }
            StackPanel_Value.Children.Add(numberic);

            var inCtrl = new CodeGenerateSystem.Controls.LinkInControl()
            {
                BackBrush = TryFindResource("Link_ValueBrush") as Brush,
                Margin = new System.Windows.Thickness(8, 3.5, 4, 3.5),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                NameString = keyName,
                Direction = CodeGenerateSystem.Base.enBezierType.Left,
            };
            StackPanel_In.Children.Add(inCtrl);
            inAction?.Invoke(inCtrl);

            var outCtrl = new CodeGenerateSystem.Controls.LinkOutControl()
            {
                BackBrush = TryFindResource("Link_ValueBrush") as Brush,
                Margin = new System.Windows.Thickness(4, 3.5, 8, 3.5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                NameString = keyName,
                Direction = CodeGenerateSystem.Base.enBezierType.Right,
            };
            StackPanel_Out.Children.Add(outCtrl);
            outAction?.Invoke(outCtrl);
        }
    }
}
