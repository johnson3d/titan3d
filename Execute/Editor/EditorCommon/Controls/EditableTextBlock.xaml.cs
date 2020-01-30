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

namespace EditorCommon.Controls
{
    /// <summary>
    /// EditableTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        string oldText = "";
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
                DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty =
                DependencyProperty.Register("HighLightString", typeof(string), typeof(EditableTextBlock), new UIPropertyMetadata(""));

        public bool Editable
        {
            get { return (bool)GetValue(EditableProperty); }
            set { SetValue(EditableProperty, value); }
        }
        public static readonly DependencyProperty EditableProperty = DependencyProperty.Register("Editable", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true));

        public System.Windows.TextWrapping TextWrapping
        {
            get { return (System.Windows.TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(System.Windows.TextWrapping), typeof(EditableTextBlock), new FrameworkPropertyMetadata(System.Windows.TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    {
                        BindingExpression bindingExpression = TextBox_Edit.GetBindingExpression(TextBox.TextProperty);
                        bindingExpression.UpdateSource();
                        TextBlock_Show.Visibility = Visibility.Visible;
                        TextBox_Edit.Visibility = Visibility.Collapsed;
                    }
                    break;
                case Key.Escape:
                    {
                        Text = oldText;
                        TextBlock_Show.Visibility = Visibility.Visible;
                        TextBox_Edit.Visibility = Visibility.Collapsed;
                    }
                    break;
            }

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            BindingExpression bindingExpression = TextBox_Edit.GetBindingExpression(TextBox.TextProperty);
            bindingExpression.UpdateSource();
            TextBlock_Show.Visibility = Visibility.Visible;
            TextBox_Edit.Visibility = Visibility.Collapsed;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && Editable)
            {
                if(e.ClickCount == 2)
                {
                    // 双击进行编辑
                    oldText = Text;
                    TextBlock_Show.Visibility = Visibility.Collapsed;
                    TextBox_Edit.Visibility = Visibility.Visible;
                    //TextBox_Edit.SelectAll();
                }
            }
        }
    }
}
