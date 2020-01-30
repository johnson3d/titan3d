using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CoreEditor
{
    /// <summary>
    /// Interaction logic for GraphDebuggerWin.xaml
    /// </summary>
    public partial class GraphDebuggerWin : ResourceLibrary.WindowBase
    {
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(GraphDebuggerWin));


        public GraphDebuggerWin()
        {
            InitializeComponent();
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            DebugTargetEnum.SetBinding(WPG.Themes.TypeEditors.EnumEditor.EnumObjectProperty, new Binding("Target") { Source = EngineNS.CEngine.Instance.GraphicDebugger, Mode=BindingMode.TwoWay });
            Slider_Value.SetBinding(Slider.ValueProperty, new Binding("CurrentDrawCallStep") { Source = EngineNS.CEngine.Instance.GraphicDebugger, Mode = BindingMode.TwoWay });
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            MaxValue = EngineNS.CEngine.Instance.GraphicDebugger.StartDrawCallStep() + 1;
            TGB_SE.Content = "关闭";
            PG.Instance = EngineNS.CEngine.Instance.GraphicDebugger.CurrentPass;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            EngineNS.CEngine.Instance.GraphicDebugger.EndDrawCallStep();
            TGB_SE.Content = "调试";
        }

        [EngineNS.Editor.Editor_MenuMethod("Debug", "Engine|Render", "Graphics|GraphDebugger")]
        public static void OpenGraphDebuggerWin()
        {
            var win = new GraphDebuggerWin();
            win.Show();
        }

        private void Button_Sub_Click(object sender, RoutedEventArgs e)
        {
            if(Slider_Value.Value > 0)
                Slider_Value.Value--;
        }
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            if (Slider_Value.Value < MaxValue)
                Slider_Value.Value++;
        }

        private void Slider_Value_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                PG.Instance = EngineNS.CEngine.Instance.GraphicDebugger.CurrentPass;
                return null;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }
    }
}
