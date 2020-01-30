using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace EditorCommon
{
    /// <summary>
    /// MessageBoxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        public string MessageString
        {
            get { return (string)GetValue(MessageStringProperty); }
            set { SetValue(MessageStringProperty, value); }
        }
        public static readonly DependencyProperty MessageStringProperty =
            DependencyProperty.Register("MessageString", typeof(string), typeof(MessageBoxWindow), new PropertyMetadata(""));

        public Visibility OKButtonVisibility
        {
            get { return (Visibility)GetValue(OKButtonVisibilityProperty); }
            set { SetValue(OKButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty OKButtonVisibilityProperty =
            DependencyProperty.Register("OKButtonVisibility", typeof(Visibility), typeof(MessageBoxWindow), new PropertyMetadata(Visibility.Collapsed));

        public Visibility YesButtonVisibility
        {
            get { return (Visibility)GetValue(YesButtonVisibilityProperty); }
            set { SetValue(YesButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty YesButtonVisibilityProperty =
            DependencyProperty.Register("YesButtonVisibility", typeof(Visibility), typeof(MessageBoxWindow), new PropertyMetadata(Visibility.Collapsed));

        public Visibility YesAllButtonVisibility
        {
            get { return (Visibility)GetValue(YesAllButtonVisibilityProperty); }
            set { SetValue(YesAllButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty YesAllButtonVisibilityProperty =
            DependencyProperty.Register("YesAllButtonVisibility", typeof(Visibility), typeof(MessageBoxWindow), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NoButtonVisibility
        {
            get { return (Visibility)GetValue(NoButtonVisibilityProperty); }
            set { SetValue(NoButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty NoButtonVisibilityProperty =
            DependencyProperty.Register("NoButtonVisibility", typeof(Visibility), typeof(MessageBoxWindow), new PropertyMetadata(Visibility.Collapsed));

        public Visibility NoAllButtonVisibility
        {
            get { return (Visibility)GetValue(NoAllButtonVisibilityProperty); }
            set { SetValue(NoAllButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty NoAllButtonVisibilityProperty =
            DependencyProperty.Register("NoAllButtonVisibility", typeof(Visibility), typeof(MessageBoxWindow), new PropertyMetadata(Visibility.Collapsed));

        public Visibility CancelButtonVisibility
        {
            get { return (Visibility)GetValue(CancelButtonVisibilityProperty); }
            set { SetValue(CancelButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty CancelButtonVisibilityProperty =
            DependencyProperty.Register("CancelButtonVisibility", typeof(Visibility), typeof(MessageBoxWindow), new PropertyMetadata(Visibility.Collapsed));

        
        public MessageBox.enMessageBoxResult Result
        {
            get;
            protected set;
        } = MessageBox.enMessageBoxResult.None;

        public MessageBoxWindow()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBox.enMessageBoxResult.OK;
            CloseWindow();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBox.enMessageBoxResult.Yes;
            CloseWindow();
        }

        private void Button_YesAll_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBox.enMessageBoxResult.YesAll;
            CloseWindow();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBox.enMessageBoxResult.No;
            CloseWindow();
        }

        private void Button_NoAll_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBox.enMessageBoxResult.NoAll;
            CloseWindow();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBox.enMessageBoxResult.Cancel;
            CloseWindow();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        Dictionary<Window, Effect> mWinEffects = new Dictionary<Window, Effect>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mWinEffects.Clear();
            foreach(Window win in EditorCommon.Program.ShowedWindows)
            {
                if (win == this)
                    continue;

                mWinEffects[win] = Effect;
                win.Effect = new BlurEffect()
                {
                    Radius = 2,
                    KernelType = KernelType.Gaussian
                };
            }
        }
        
        private void CloseWindow()
        {
            var anim = TryFindResource("Storyboard_End") as Storyboard;
            
            anim.Completed += (senderObj, eArg) =>
            {
                this.Close();
            };
            anim.Begin(this);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            foreach(Window win in EditorCommon.Program.ShowedWindows)
            {
                if (win == this)
                    continue;
                Effect effect = null;
                mWinEffects.TryGetValue(win, out effect);
                win.Effect = effect;
            }
        }
    }
}
