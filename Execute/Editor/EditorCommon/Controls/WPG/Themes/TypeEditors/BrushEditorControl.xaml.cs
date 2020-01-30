using ICSharpCode.WpfDesign.Designer.PropertyGrid.Editors.BrushEditor;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for BrushEditorControl.xaml
    /// </summary>
    public partial class BrushEditorControl : UserControl
    {
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(Brush), typeof(BrushEditorControl), new UIPropertyMetadata(Brushes.AliceBlue));
       
        public BrushEditorControl()
        {
            InitializeComponent();
        }

        private BrushEditorView myBrshView = null;
        private Window BrshEdt =null;
        private bool MouseWasDown = false;

        private void Click(object sender, RoutedEventArgs e)
        {
            if (MouseWasDown)
            {
                MouseWasDown = false;
                showCol.Visibility = Visibility.Collapsed;
                doNothing.Visibility = System.Windows.Visibility.Visible;

                e.Handled = true;
                if (BrshEdt == null)
                {
                    myBrshView = new BrushEditorView();

                    myBrshView.BrushEditor.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(BrushEditor_PropertyChanged);
                    Brush myBrush = null;
                    if (this.Brush != null)
                        myBrush= this.Brush.Clone();
                    myBrshView.BrushEditor.Property = new PropertyNode() {Value = myBrush};                                        
                    myBrshView.VerticalAlignment = VerticalAlignment.Top;
                    myBrshView.HorizontalAlignment = HorizontalAlignment.Left;

                    BrshEdt = new Window();
                    BrshEdt.Content = myBrshView;
                    BrshEdt.WindowStyle = WindowStyle.None;
                    BrshEdt.VerticalContentAlignment = VerticalAlignment.Top;
                    BrshEdt.HorizontalContentAlignment = HorizontalAlignment.Left;

                    BrshEdt.BorderThickness = new Thickness(0);
                    BrshEdt.Height = 450;
                    BrshEdt.Width = 400;
                    BrshEdt.ResizeMode = ResizeMode.NoResize;
                    BrshEdt.Background = Brushes.Transparent;
                    BrshEdt.AllowsTransparency = true;
                    BrshEdt.WindowStartupLocation = WindowStartupLocation.Manual;

                    var tmp = System.Windows.SystemParameters.FullPrimaryScreenWidth;
                    BrshEdt.Left = myCtl.PointToScreen(new Point(0, 0)).X;
                    if (BrshEdt.Left + BrshEdt.Width > System.Windows.SystemParameters.MaximumWindowTrackWidth)
                        BrshEdt.Left -= BrshEdt.Width - 40;

                    BrshEdt.Top = myCtl.PointToScreen(new Point(0, 0)).Y + 13;
                    if (BrshEdt.Top + BrshEdt.Height > System.Windows.SystemParameters.MaximumWindowTrackHeight)
                    {
                        BrshEdt.Top -= BrshEdt.Height + 15;
                        BrshEdt.VerticalContentAlignment = VerticalAlignment.Bottom;
                        myBrshView.VerticalAlignment = VerticalAlignment.Bottom;
                    }

                    myBrshView.Background = Brushes.Transparent;

                    BrshEdt.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(BrshEdt_IsKeyboardFocusWithinChanged);

                    BrshEdt.Show();
                }
            }
            else
            {
                CloseIt();
            }
        }

        private void CloseIt()
        {
            try
            {
                MouseWasDown = false;
                showCol.Visibility = Visibility.Visible;
                doNothing.Visibility = Visibility.Collapsed;

                BrshEdt.Close();
                myBrshView = null;
                BrshEdt = null;
            }
            catch (Exception)
            { }
        }

        void BrshEdt_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((Window)sender).IsKeyboardFocusWithin == false)
            {
                CloseIt();
            }
        }

       void BrushEditor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Brush")
            {               
                BindingExpression bindingExpression = this.GetBindingExpression(BrushProperty);
                bindingExpression.UpdateSource();
                ((WPG.Data.Property)(bindingExpression.DataItem)).Value = myBrshView.BrushEditor.Property.Value;
            }
        }

       private void showCol_MouseDown(object sender, MouseButtonEventArgs e)
       {
           MouseWasDown = true;
       }       
    }
}
