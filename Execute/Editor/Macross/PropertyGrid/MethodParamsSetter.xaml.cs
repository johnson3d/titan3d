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
    /// Interaction logic for MethodParamsSetter.xaml
    /// </summary>
    public partial class MethodParamsSetter : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(MethodParamsSetter), new FrameworkPropertyMetadata(null));

        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty = DependencyProperty.Register("BindInstance", typeof(object), typeof(MethodParamsSetter), new UIPropertyMetadata(null));

        public ObservableCollection<CodeDomNode.CustomMethodInfo.FunctionParam> Params
        {
            get { return (ObservableCollection<CodeDomNode.CustomMethodInfo.FunctionParam>)GetValue(ParamsProperty); }
            set { SetValue(ParamsProperty, value); }
        }
        public static readonly DependencyProperty ParamsProperty = DependencyProperty.Register("Params", typeof(ObservableCollection<CodeDomNode.CustomMethodInfo.FunctionParam>), typeof(MethodParamsSetter), new UIPropertyMetadata(new ObservableCollection<CodeDomNode.CustomMethodInfo.FunctionParam>(), new PropertyChangedCallback(ParamsPropertyChangedCallback)));
        static void ParamsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        public MethodParamsSetter()
        {
            InitializeComponent();
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            var methodInfo = BindInstance as CodeDomNode.CustomMethodInfo;
            if (BindProperty.Name == "OutParams" && methodInfo.IsAsync && methodInfo.OutParams.Count > 0)
            {
                EditorCommon.MessageBox.Show("异步函数只能包含最多一个输出参数，不能添加多个输出参数!");
                return;
            }

            var paramName = BindProperty.Name + "_";
            bool repetition = false;
            int i = 0;
            do
            {
                repetition = false;
                foreach (var param in Params)
                {
                    if (param.ParamName == paramName + i)
                    {
                        repetition = true;
                        i++;
                        break;
                    }
                }
            } while (repetition);

            var funcParam = new CodeDomNode.CustomMethodInfo.FunctionParam()
            {
                ParamType = new CodeDomNode.VariableType(methodInfo.CSType),
                HostMethodInfo = methodInfo,
            };
            if (BindProperty.Name == "InParams")
            {
                funcParam.ParamName = paramName + i;
                Params.Add(funcParam);
                methodInfo._OnAddedInParam(funcParam);
            }
            else if (BindProperty.Name == "OutParams")
            {
                funcParam.ParamName = paramName + i;
                Params.Add(funcParam);
                methodInfo._OnAddedOutParam(funcParam);
            }
            var be = GetBindingExpression(ParamsProperty);
            be.UpdateSource();
        }

        private void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox_Params.SelectedItems.Count == 0)
                return;
            var items = new CodeDomNode.CustomMethodInfo.FunctionParam[ListBox_Params.SelectedItems.Count];
            var methodInfo = BindInstance as CodeDomNode.CustomMethodInfo;
            ListBox_Params.SelectedItems.CopyTo(items, 0);
            List<int> indexList = new List<int>(items.Length);
            foreach (var item in items)
            {
                var param = item as CodeDomNode.CustomMethodInfo.FunctionParam;
                if (param == null)
                    continue;
                var index = Params.IndexOf(param);
                indexList.Add(index);
                Params.Remove(param);
            }
            for(int i=0; i<indexList.Count; i++)
            {
                var item = items[i];
                var param = item as CodeDomNode.CustomMethodInfo.FunctionParam;
                if (param == null)
                    continue;
                if (BindProperty.Name == "InParams")
                    methodInfo._OnRemovedInParam(indexList[i], param);
                else if (BindProperty.Name == "OutParams")
                    methodInfo._OnRemovedOutParam(indexList[i], param);
            }
            var be = GetBindingExpression(ParamsProperty);
            be.UpdateSource();
        }
    }
}
