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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EditorCommon.Controls.Debugger
{
    /// <summary>
    /// Interaction logic for PlayAndDebugToolbar.xaml
    /// </summary>
    public partial class PlayAndDebugToolbar : UserControl
    {
        public bool IsInPIEMode
        {
            get { return (bool)GetValue(IsInPIEModeProperty); }
            set { SetValue(IsInPIEModeProperty, value); }
        }
        public static readonly DependencyProperty IsInPIEModeProperty = DependencyProperty.Register("IsInPIEMode", typeof(bool), typeof(PlayAndDebugToolbar), new FrameworkPropertyMetadata(false, OnIsInPIEModePropertyChanged));
        static void OnIsInPIEModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as PlayAndDebugToolbar;
            if ((bool)e.NewValue)
            {
                ctrl.PIEBtn.Icon = ctrl.TryFindResource("StopPlay") as ImageSource;
                ctrl.PIEBtn.Text = "Stop";
            }
            else
            {
                ctrl.PIEBtn.Icon = ctrl.TryFindResource("PlayInWindow") as ImageSource;
                ctrl.PIEBtn.Text = "Play";
            }
        }

        public bool IsBreaking
        {
            get { return (bool)GetValue(IsBreakingProperty); }
            set { SetValue(IsBreakingProperty, value); }
        }
        public static readonly DependencyProperty IsBreakingProperty = DependencyProperty.Register("IsBreaking", typeof(bool), typeof(PlayAndDebugToolbar), new FrameworkPropertyMetadata(false));
        public bool ButtonEnable
        {
            get { return (bool)GetValue(ButtonEnableProperty); }
            set { SetValue(ButtonEnableProperty, value); }
        }
        public static readonly DependencyProperty ButtonEnableProperty = DependencyProperty.Register("ButtonEnable", typeof(bool), typeof(PlayAndDebugToolbar), new FrameworkPropertyMetadata(true));


        public PlayAndDebugToolbar()
        {
            InitializeComponent();

            //EditorCommon.GamePlay.Instance.PADToolbars.Add(this);
            BindingOperations.SetBinding(this, IsInPIEModeProperty, new Binding("IsInPIEMode") { Source = EditorCommon.GamePlay.Instance, Mode = BindingMode.TwoWay });

            //BindingOperations.SetBinding(this, IsBreakingProperty, new Binding("IsBreaked") { Source = EngineNS.Editor.Runner.RunnerManager.Instance });
            EngineNS.Editor.Runner.RunnerManager.Instance.OnBreak += RunnerManager_OnBreak;
            EngineNS.Editor.Runner.RunnerManager.Instance.OnResume += RunnerManager_OnResume;
        }

        private void RunnerManager_OnBreak(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                IsBreaking = true;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }
        private void RunnerManager_OnResume(EngineNS.Editor.Runner.RunnerManager.BreakContext context)
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                IsBreaking = false;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }
        void GameInstance_OnGameStateChanged(EngineNS.GamePlay.GGameInstance.enGameState state)
        {
            switch(state)
            {
                case EngineNS.GamePlay.GGameInstance.enGameState.Initialized:
                case EngineNS.GamePlay.GGameInstance.enGameState.Stopped:
                    ButtonEnable = true;
                    break;
                default:
                    ButtonEnable = false;
                    break;
            }
        }

        ~PlayAndDebugToolbar()
        {
            //EditorCommon.GamePlay.Instance.PADToolbars.Remove(this);
            EngineNS.Editor.Runner.RunnerManager.Instance.OnBreak -= RunnerManager_OnBreak;
            EngineNS.Editor.Runner.RunnerManager.Instance.OnResume -= RunnerManager_OnResume;
        }
        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (StackPanel_Btns.Children.Count > 0)
            {
                var hostToolbar = EditorCommon.Program.GetParent(this, typeof(ToolBar)) as ToolBar;
                var elements = new FrameworkElement[StackPanel_Btns.Children.Count];
                StackPanel_Btns.Children.CopyTo(elements, 0);
                foreach (var item in elements)
                {
                    if (EditorCommon.Program.RemoveElementFromParent(item))
                        hostToolbar.Items.Add(item);
                }
                this.UpdateLayout();
                hostToolbar.UpdateLayout();
            }
        }
        private void userControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void IconTextBtn_PlayInWindow_Click(object sender, RoutedEventArgs e)
        {
            if(EngineNS.CEngine.Instance.GameInstance != null)
            {
                switch (EngineNS.CEngine.Instance.GameInstance.GameState)
                {
                    case EngineNS.GamePlay.GGameInstance.enGameState.Initialized:
                    case EngineNS.GamePlay.GGameInstance.enGameState.Stopped:
                        break;
                    default:
                        return;
                }
            }
            var itb = sender as ResourceLibrary.Controls.Button.IconTextBtn;
            if (itb != null)
            {
                if (EditorCommon.GamePlay.Instance.IsInPIEMode)
                {
                    EditorCommon.GamePlay.Instance.StopPlayInWindow();
                }
                else
                {
                    var noUse = EditorCommon.GamePlay.Instance.ShowPlayInWindow();
                }
            }
        }

        private void IconTextBtn_Resume_Click(object sender, RoutedEventArgs e)
        {
            EngineNS.Editor.Runner.RunnerManager.Instance.Resume();
        }

    }
}
