using System;
using System.Windows;
using System.Windows.Controls;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for VectorEditor.xaml
    /// </summary>
    public partial class VectorEditor : UserControl
    {
        public Type VectorType;

        public object VectorObject
        {
            get { return GetValue(VectorObjectProperty); }
            set
            {
                SetValue(VectorObjectProperty, value);
            }
        }
        public static readonly DependencyProperty VectorObjectProperty =
            DependencyProperty.Register("VectorObject", typeof(object), typeof(VectorEditor),
                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnVectorObjectChanged))
                                    );
        public static void OnVectorObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VectorEditor control = d as VectorEditor;

            if (e.NewValue == null)
                return;

            control.VectorType = e.NewValue.GetType();

            if (control.VectorType == typeof(EngineNS.Vector2))
            {
                var newValue = (EngineNS.Vector2)e.NewValue;
                control.NE_Z.Visibility = Visibility.Collapsed;
                control.NE_W.Visibility = Visibility.Collapsed;

                control.XValue = newValue.X;
                control.YValue = newValue.Y;
            }
            else if (control.VectorType == typeof(EngineNS.Vector3))
            {
                var newValue = (EngineNS.Vector3)e.NewValue;
                control.NE_W.Visibility = Visibility.Collapsed;

                control.XValue = newValue.X;
                control.YValue = newValue.Y;
                control.ZValue = newValue.Z;
            }
            else if (control.VectorType == typeof(EngineNS.Vector4))
            {
                var newValue = (EngineNS.Vector4)e.NewValue;
                control.XValue = newValue.X;
                control.YValue = newValue.Y;
                control.ZValue = newValue.Z;
                control.WValue = newValue.W;
            }
        }

        public float XValue
        {
            get { return (float)GetValue(XValueProperty); }
            set { SetValue(XValueProperty, value); }
        }
        public static readonly DependencyProperty XValueProperty =
                    DependencyProperty.Register("XValue", typeof(float), typeof(VectorEditor),
                            new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnXValueChanged)));
        public static void OnXValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VectorEditor control = d as VectorEditor;

            float newValue = (float)e.NewValue;
            float oldValue = (float)e.OldValue;

            if (control.VectorType == typeof(EngineNS.Vector2))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector2)(control.VectorObject);
                    var obj = new EngineNS.Vector2();
                    obj.X = newValue;
                    obj.Y = vec.Y; // control.YValue;
                    control.VectorObject = obj;
                }
            }
            else if (control.VectorType == typeof(EngineNS.Vector3))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector3)(control.VectorObject);
                    var obj = new EngineNS.Vector3();
                    obj.X = newValue;
                    obj.Y = vec.Y; //control.YValue;
                    obj.Z = vec.Z; //control.ZValue;
                    control.VectorObject = obj;
                }
            }
            else if (control.VectorType == typeof(EngineNS.Vector4))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector4)(control.VectorObject);
                    var obj = new EngineNS.Vector4();
                    obj.X = newValue;
                    obj.Y = vec.Y; //control.YValue;
                    obj.Z = vec.Z; //control.ZValue;
                    obj.W = vec.W; //control.WValue;
                    control.VectorObject = obj;
                }
            }
        }

        public float YValue
        {
            get { return (float)GetValue(YValueProperty); }
            set { SetValue(YValueProperty, value); }
        }
        public static readonly DependencyProperty YValueProperty =
                    DependencyProperty.Register("YValue", typeof(float), typeof(VectorEditor),
                            new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnYValueChanged)));
        public static void OnYValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VectorEditor control = d as VectorEditor;

            float newValue = (float)e.NewValue;
            float oldValue = (float)e.OldValue;

            if (control.VectorType == typeof(EngineNS.Vector2))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector2)(control.VectorObject);
                    var obj = new EngineNS.Vector2();
                    obj.X = vec.X; //control.XValue;
                    obj.Y = newValue;
                    control.VectorObject = obj;
                }
            }
            else if (control.VectorType == typeof(EngineNS.Vector3))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector3)(control.VectorObject);
                    var obj = new EngineNS.Vector3();
                    obj.X = vec.X; //control.XValue;
                    obj.Y = newValue;
                    obj.Z = vec.Z; //control.ZValue;
                    control.VectorObject = obj;
                }
            }
            else if (control.VectorType == typeof(EngineNS.Vector4))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector4)(control.VectorObject);
                    var obj = new EngineNS.Vector4();
                    obj.X = vec.X; //control.XValue;
                    obj.Y = newValue;
                    obj.Z = vec.Z; //control.ZValue;
                    obj.W = vec.W; //control.WValue;
                    control.VectorObject = obj;
                }
            }
        }

        public float ZValue
        {
            get { return (float)GetValue(ZValueProperty); }
            set { SetValue(ZValueProperty, value); }
        }
        public static readonly DependencyProperty ZValueProperty =
                    DependencyProperty.Register("ZValue", typeof(float), typeof(VectorEditor),
                            new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnZValueChanged)));
        public static void OnZValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VectorEditor control = d as VectorEditor;

            float newValue = (float)e.NewValue;
            float oldValue = (float)e.OldValue;

            if (control.VectorType == typeof(EngineNS.Vector3))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector3)(control.VectorObject);
                    var obj = new EngineNS.Vector3();
                    obj.X = vec.X; //control.XValue;
                    obj.Y = vec.Y; //control.YValue;
                    obj.Z = newValue;
                    control.VectorObject = obj;
                }
            }
            else if (control.VectorType == typeof(EngineNS.Vector4))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector4)(control.VectorObject);
                    var obj = new EngineNS.Vector4();
                    obj.X = vec.X; //control.XValue;
                    obj.Y = vec.Y; //control.YValue;
                    obj.Z = newValue;
                    obj.W = vec.W; //control.WValue;
                    control.VectorObject = obj;
                }
            }
        }

        public float WValue
        {
            get { return (float)GetValue(WValueProperty); }
            set { SetValue(WValueProperty, value); }
        }
        public static readonly DependencyProperty WValueProperty =
                    DependencyProperty.Register("WValue", typeof(float), typeof(VectorEditor),
                            new FrameworkPropertyMetadata(0.0f, new PropertyChangedCallback(OnWValueChanged)));
        public static void OnWValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VectorEditor control = d as VectorEditor;

            float newValue = (float)e.NewValue;
            float oldValue = (float)e.OldValue;

            if (control.VectorType == typeof(EngineNS.Vector4))
            {
                if (System.Math.Abs(newValue - oldValue) > 0.0001f)
                {
                    var vec = (EngineNS.Vector4)(control.VectorObject);
                    var obj = new EngineNS.Vector4();
                    obj.X = vec.X; //control.XValue;
                    obj.Y = vec.Y; //control.YValue;
                    obj.Z = vec.Z; //control.ZValue;
                    obj.W = newValue;
                    control.VectorObject = obj;
                }
            }
        }

        public VectorEditor()
        {
            InitializeComponent();
        }
    }
}
