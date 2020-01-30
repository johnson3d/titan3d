using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EditorCommon.Controls
{
    public class CustomTextBlock : TextBlock, EditorCommon.DragDrop.IDragAbleObject
    {
        public Brush HighLightBrush
        {
            get { return (Brush)GetValue(HighLightBrushProperty); }
            set { SetValue(HighLightBrushProperty, value); }
        }
        public static readonly DependencyProperty HighLightBrushProperty =
                DependencyProperty.Register("HighLightBrush", typeof(Brush), typeof(CustomTextBlock), new UIPropertyMetadata(new SolidColorBrush(Color.FromRgb(149, 96, 0)), OnHighLightBrushPropertyChanged));
        private static void OnHighLightBrushPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public StringComparison HighLightStringComparison
        {
            get { return (StringComparison)GetValue(HighLightStringComparisonProperty); }
            set { SetValue(HighLightStringComparisonProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringComparisonProperty =
                DependencyProperty.Register("HighLightStringComparison", typeof(StringComparison), typeof(CustomTextBlock), new UIPropertyMetadata(StringComparison.OrdinalIgnoreCase));

        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty =
                DependencyProperty.Register("HighLightString", typeof(string), typeof(CustomTextBlock), new UIPropertyMetadata("", OnHighLightStringPropertyChanged));
        private static void OnHighLightStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as CustomTextBlock;

            var newValue = (string)(e.NewValue);

            var text = textBlock.Text;

            if (string.IsNullOrEmpty(newValue))
            {
                textBlock.Inlines.Clear();
                textBlock.Inlines.Add(new Run(text));
            }
            else
            {
                textBlock.Inlines.Clear();

                int startIdx = 0;
                while (true)
                {
                    var idx = text.IndexOf(newValue, startIdx, textBlock.HighLightStringComparison);
                    if (idx < 0)
                        break;

                    textBlock.Inlines.Add(new Run(text.Substring(startIdx, idx - startIdx)));
                    textBlock.Inlines.Add(new Run(text.Substring(idx, newValue.Length)) { Background = textBlock.HighLightBrush });
                    startIdx = idx + newValue.Length;
                }

                textBlock.Inlines.Add(new Run(text.Substring(startIdx)));
            }
        }
        
        public System.Windows.FrameworkElement GetDragVisual()
        {
            return this;
        }
    }
}
