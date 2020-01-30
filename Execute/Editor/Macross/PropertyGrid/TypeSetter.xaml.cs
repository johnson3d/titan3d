using EngineNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Macross.PropertyGrid
{
    /// <summary>
    /// Interaction logic for TypeSetter.xaml
    /// </summary>
    public partial class TypeSetter : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty = DependencyProperty.Register("BindInstance", typeof(object), typeof(TypeSetter), new UIPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChangedCallback)));
        static void OnBindInstanceChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TypeSetter;
            var vcip = e.NewValue as VariableCategoryItemPropertys;
            if (vcip != null)
            {
                if (vcip.VariableTypeReadOnly)
                {
                    ctrl.EnableEdit = false;
                }
            }
        }
        public bool EnableEdit
        {
            get { return (bool)GetValue(EnableEditProperty); }
            set { SetValue(EnableEditProperty, value); }
        }
        public static readonly DependencyProperty EnableEditProperty = DependencyProperty.Register("EnableEdit", typeof(bool), typeof(TypeSetter), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public CodeDomNode.VariableType VarType
        {
            get { return (CodeDomNode.VariableType)GetValue(VarTypeProperty); }
            set { SetValue(VarTypeProperty, value); }
        }

        public static readonly DependencyProperty VarTypeProperty =
            DependencyProperty.Register("VarType", typeof(CodeDomNode.VariableType), typeof(TypeSetter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnVarTypeChangedCallback)));

        static void OnVarTypeChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TypeSetter;
            BindingOperations.ClearBinding(ctrl, ArrayTypeProperty);
            BindingOperations.SetBinding(ctrl, ArrayTypeProperty, new Binding("ArrayType") { Source = ctrl.VarType });
            BindingOperations.ClearBinding(ctrl, CurrentTypeNameProperty);
            BindingOperations.SetBinding(ctrl, CurrentTypeNameProperty, new Binding("TypeName") { Source = ctrl.VarType });

            var newValue = e.NewValue as CodeDomNode.VariableType;
            ctrl.TPSelector.CSType = newValue.CSType;
            ctrl.VarValueType = ctrl.VarType.Type;
        }

        public Type VarValueType
        {
            get { return (Type)GetValue(VarValueTypeProperty); }
            set { SetValue(VarValueTypeProperty, value); }
        }
        public static readonly DependencyProperty VarValueTypeProperty = DependencyProperty.Register("VarValueType", typeof(Type), typeof(TypeSetter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnVarValueTypeChangedCallback)));
        static void OnVarValueTypeChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //var ctrl = sender as TypeSetter;
            //ctrl.VarType.Type = e.NewValue as Type;
        }

        public CodeDomNode.VariableType.enArrayType ArrayType
        {
            get { return (CodeDomNode.VariableType.enArrayType)GetValue(ArrayTypeProperty); }
            set { SetValue(ArrayTypeProperty, value); }
        }

        public static readonly DependencyProperty ArrayTypeProperty =
            DependencyProperty.Register("ArrayType", typeof(CodeDomNode.VariableType.enArrayType), typeof(TypeSetter), new PropertyMetadata(CodeDomNode.VariableType.enArrayType.Single, new PropertyChangedCallback(OnArrayTypeChangedCallback), new CoerceValueCallback(OnArrayTypeCVCallback)));

        static void OnArrayTypeChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TypeSetter;
            var newValue = (CodeDomNode.VariableType.enArrayType)e.NewValue;
            ctrl.VarType.ArrayType = newValue;

            ctrl.ProcessChangeType();
        }
        private static object OnArrayTypeCVCallback(DependencyObject d, object value)
        {
            //var ctrl = d as TypeSetter;
            //if (ctrl.CanChangedTypeValue())
            //    return value;
            //else
            //    return ctrl.ArrayType;
            return value;
        }
        public CodeDomNode.VariableType.enTypeState TypeState
        {
            get { return (CodeDomNode.VariableType.enTypeState)GetValue(TypeStateProperty); }
            set { SetValue(TypeStateProperty, value); }
        }

        public static readonly DependencyProperty TypeStateProperty = DependencyProperty.Register("TypeState", typeof(CodeDomNode.VariableType.enTypeState), typeof(TypeSetter), new PropertyMetadata(CodeDomNode.VariableType.enTypeState.ObjectReference));
        public string CurrentTypeName
        {
            get { return (string)GetValue(CurrentTypeNameProperty); }
            set { SetValue(CurrentTypeNameProperty, value); }
        }
        public static readonly DependencyProperty CurrentTypeNameProperty = DependencyProperty.Register("CurrentTypeName", typeof(string), typeof(TypeSetter), new PropertyMetadata(null));

        public TypeSetter()
        {
            InitializeComponent();
            //InitializeTypes();
            TPSelector.CanChangeType = () =>
            {
                if (EditorCommon.MessageBox.Show("即将更改变量类型，所有与此变量连接的节点均会断开，所有使用此变量的Macross都会收到影响，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) == EditorCommon.MessageBox.enMessageBoxResult.Yes)
                    return true;
                return false;
            };
            TPSelector.OnTypeChangedAction = (typeItem) =>
            {
                VarType.NotCreateInInitialize = typeItem.NotCreateInInitialize;
                VarType.IsMacrossGetter = typeItem.IsMacrossGetter;
                VarType.MacrossClassType = typeItem.MacrossClassType;
                if (VarType.IsMacrossGetter)
                {
                    VarType.TypeName = "MacrossGetter<" + typeItem.MacrossClassType.Name + ">";
                }
                else
                {
                    VarType.TypeName = typeItem.Type.Name;
                }

                VarType.Type = typeItem.Type;
                VarType.ArrayType = ArrayType;
                VarType.TypeState = TypeState;

                ProcessChangeType();
            };
        }
        void ProcessChangeType()
        {
            var be = GetBindingExpression(VarTypeProperty);
            be.UpdateSource();
            var ins = BindInstance as CodeDomNode.IVariableTypeChangeProcessClass;
            if (ins != null)
                ins.OnVariableTypeChanged(VarType);
        }

    }
}
