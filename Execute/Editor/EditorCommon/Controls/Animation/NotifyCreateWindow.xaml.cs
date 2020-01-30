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

namespace EditorCommon.Controls.Animation
{
    /// <summary>
    /// Interaction logic for NotifyCreateWindow.xaml
    /// </summary>
    public partial class NotifyCreateWindow : ResourceLibrary.WindowBase
    {
        public NotifyCreateWindow()
        {
            InitializeComponent();
            this.KeyDown += NotifyCreateWindow_KeyDown;
        }
        void CloseWindow(bool result)
        {
            Mouse.Capture(null);
            Keyboard.Focus(null);
            this.KeyDown -= NotifyCreateWindow_KeyDown;
            this.DialogResult = result;
        }
        private void NotifyCreateWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                CloseWindow(false);
            }
            if(e.Key == Key.Enter)
            {
                CloseWindow(true);
            }
        }
        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(NotifyTextBox);
            NotifyTextBox.SelectAll();
            Mouse.Capture(this);
        }
        private void WindowBase_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(this);
            if(mousePos.X < 0 || mousePos.Y<0 || mousePos.X > ActualWidth || mousePos.Y >ActualHeight)
            {
                CloseWindow(false);
            }
        }
    }
}
