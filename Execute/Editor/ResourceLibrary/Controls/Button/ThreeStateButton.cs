using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ResourceLibrary.Controls.Button
{
    public class ThreeStateButton : System.Windows.Controls.Button
    {
        public ImageSource NormalSource
        {
            get { return (ImageSource)GetValue(NormalSourceProperty); }
            set { SetValue(NormalSourceProperty, value); }
        }
        public static readonly DependencyProperty NormalSourceProperty = DependencyProperty.Register("NormalSource", typeof(ImageSource), typeof(ThreeStateButton), new PropertyMetadata(null));
        public ImageSource HoverSource
        {
            get { return (ImageSource)GetValue(HoverSourceProperty); }
            set { SetValue(HoverSourceProperty, value); }
        }
        public static readonly DependencyProperty HoverSourceProperty = DependencyProperty.Register("HoverSource", typeof(ImageSource), typeof(ThreeStateButton), new PropertyMetadata(null));
        public ImageSource PressedSource
        {
            get { return (ImageSource)GetValue(PressedSourceProperty); }
            set { SetValue(PressedSourceProperty, value); }
        }
        public static readonly DependencyProperty PressedSourceProperty = DependencyProperty.Register("PressedSource", typeof(ImageSource), typeof(ThreeStateButton), new PropertyMetadata(null));

        public ImageSource CurrentSource
        {
            get { return (ImageSource)GetValue(CurrentSourceProperty); }
            set { SetValue(CurrentSourceProperty, value); }
        }
        public static readonly DependencyProperty CurrentSourceProperty = DependencyProperty.Register("CurrentSource", typeof(ImageSource), typeof(ThreeStateButton), new PropertyMetadata(null));

        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);
            if (IsPressed == true)
                CurrentSource = PressedSource;
            else
                CurrentSource = NormalSource;
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            CurrentSource = HoverSource;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            CurrentSource = NormalSource;
        }
    }

    public class ThreeStateToggleButton : ToggleButton
    {
        public ImageSource NormalSource
        {
            get { return (ImageSource)GetValue(NormalSourceProperty); }
            set { SetValue(NormalSourceProperty, value); }
        }
        public static readonly DependencyProperty NormalSourceProperty = DependencyProperty.Register("NormalSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));
        public ImageSource HoverSource
        {
            get { return (ImageSource)GetValue(HoverSourceProperty); }
            set { SetValue(HoverSourceProperty, value); }
        }
        public static readonly DependencyProperty HoverSourceProperty = DependencyProperty.Register("HoverSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));
        public ImageSource PressedSource
        {
            get { return (ImageSource)GetValue(PressedSourceProperty); }
            set { SetValue(PressedSourceProperty, value); }
        }
        public static readonly DependencyProperty PressedSourceProperty = DependencyProperty.Register("PressedSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));
        public ImageSource CheckedNormalSource
        {
            get { return (ImageSource)GetValue(CheckedNormalSourceProperty); }
            set { SetValue(CheckedNormalSourceProperty, value); }
        }
        public static readonly DependencyProperty CheckedNormalSourceProperty = DependencyProperty.Register("CheckedNormalSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));
        public ImageSource CheckedHoverSource
        {
            get { return (ImageSource)GetValue(CheckedHoverSourceProperty); }
            set { SetValue(CheckedHoverSourceProperty, value); }
        }
        public static readonly DependencyProperty CheckedHoverSourceProperty = DependencyProperty.Register("CheckedHoverSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));
        public ImageSource CheckedPressedSource
        {
            get { return (ImageSource)GetValue(CheckedPressedSourceProperty); }
            set { SetValue(CheckedPressedSourceProperty, value); }
        }
        public static readonly DependencyProperty CheckedPressedSourceProperty = DependencyProperty.Register("CheckedPressedSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));
        public ImageSource CurrentSource
        {
            get { return (ImageSource)GetValue(CurrentSourceProperty); }
            set { SetValue(CurrentSourceProperty, value); }
        }
        public static readonly DependencyProperty CurrentSourceProperty = DependencyProperty.Register("CurrentSource", typeof(ImageSource), typeof(ThreeStateToggleButton), new FrameworkPropertyMetadata(null));

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            if (IsMouseOver)
                CurrentSource = CheckedHoverSource;
            else
                CurrentSource = CheckedNormalSource;
        }
        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            if (IsMouseOver)
                CurrentSource = HoverSource;
            else
                CurrentSource = NormalSource;
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (IsChecked == true)
                CurrentSource = CheckedHoverSource;
            else
                CurrentSource = HoverSource;
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (IsChecked == true)
                CurrentSource = CheckedNormalSource;
            else
                CurrentSource = NormalSource;
        }
        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);
            if (IsChecked == true)
                CurrentSource = CheckedPressedSource;
            else
                CurrentSource = PressedSource;
        }
    }
}
