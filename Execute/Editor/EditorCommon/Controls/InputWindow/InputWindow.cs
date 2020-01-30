using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace InputWindow
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:InputWindow"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:InputWindow;assembly=InputWindow"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    [TemplatePart(Name = "PART_Value", Type = typeof(TextBlock))]
    public class InputWindow : Window
    {
        Storyboard mStartStoryboard = new Storyboard();
        Storyboard mEndStoryboard = new Storyboard();

        bool? mResult = null;

        static InputWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InputWindow), new FrameworkPropertyMetadata(typeof(InputWindow)));
        }

        public InputWindow()
        {
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.AllowsTransparency = true;
            this.Loaded += InputWindow_Loaded;
            this.Closed += InputWindow_Closed;
            this.Closing += InputWindow_Closing;
        }

        bool mEndAnimPlayed = false;
        bool mEndAnimFinished = false;
        private void InputWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!mEndAnimFinished)
                e.Cancel = true;
            
            if (!mEndAnimPlayed && mMainBorder != null)
            {
                // end
                DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
                EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame()
                {
                    Value = 1,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.1)),
                };
                animation.KeyFrames.Add(frame);
                frame = new EasingDoubleKeyFrame()
                {
                    Value = 0,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)),
                };
                animation.KeyFrames.Add(frame);
                Storyboard.SetTargetName(animation, mMainBorder.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(0).(1)[0].(2)",
                                        new DependencyProperty[] {
                                            UIElement.RenderTransformProperty,
                                            TransformGroup.ChildrenProperty,
                                            ScaleTransform.ScaleXProperty,
                                        }));
                mEndStoryboard.Children.Add(animation);
                animation = new DoubleAnimationUsingKeyFrames();
                frame = new EasingDoubleKeyFrame()
                {
                    Value = 0.1,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.1)),
                };
                animation.KeyFrames.Add(frame);
                Storyboard.SetTargetName(animation, mMainBorder.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(0).(1)[0].(2)",
                                        new DependencyProperty[] {
                                            UIElement.RenderTransformProperty,
                                            TransformGroup.ChildrenProperty,
                                            ScaleTransform.ScaleYProperty,
                                        }));
                mEndStoryboard.Children.Add(animation);

                mEndStoryboard.Completed += (senderObj, eArg) =>
                {
                    mEndAnimFinished = true;
                    DialogResult = mResult;
                    this.Close();
                };
                mEndStoryboard.Begin(mMainBorder);
                mEndAnimPlayed = true;
            }
        }

        void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        Dictionary<Window, Effect> mWinEffects = new Dictionary<Window, Effect>();
        Delegate_ValidateCheck mOnValidateCheck = null;
        void InputWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (TextBox_Value != null)
            {
                var bindExp = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as RequiredRule;
                        if (rule != null)
                        {
                            rule.OnValidateCheck = mOnValidateCheck;
                        }
                    }
                }
            }

            mWinEffects.Clear();
            foreach (var win in DockControl.DockManager.Instance.DockableWindows)
            {
                mWinEffects[win] = Effect;
                win.Effect = new BlurEffect()
                {
                    Radius = 2,
                    KernelType = KernelType.Gaussian
                };
            }


            if (OwnerControl != null)
            {
                var oriPt = OwnerControl.PointToScreen(new Point(0, 0));
                this.Left = oriPt.X + OwnerControl.ActualWidth * 0.5 - this.ActualWidth * 0.5;
                this.Top = oriPt.Y + OwnerControl.ActualHeight * 0.5 - this.ActualHeight * 0.5;
            }
        }

        private void InputWindow_Closed(object sender, System.EventArgs e)
        {
            foreach (var win in DockControl.DockManager.Instance.DockableWindows)
            {
                Effect effect = null;
                mWinEffects.TryGetValue(win, out effect);
                win.Effect = effect;
            }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(InputWindow), new FrameworkPropertyMetadata("说明:"));
        public string Information
        {
            get { return (string)GetValue(InformationProperty); }
            set { SetValue(InformationProperty, value); }
        }

        public static readonly DependencyProperty InformationProperty =
            DependencyProperty.Register("Information", typeof(string), typeof(InputWindow), new FrameworkPropertyMetadata(""));
        public bool InformationVisible
        {
            get { return (bool)GetValue(InformationVisibleProperty); }
            set { SetValue(InformationVisibleProperty, value); }
        }

        public static readonly DependencyProperty InformationVisibleProperty =
            DependencyProperty.Register("InformationVisible", typeof(bool), typeof(InputWindow), new FrameworkPropertyMetadata(false));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(InputWindow), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));
        public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// 拥有此窗口的控件，输入窗口会在此控件中心显示，如果为空则在屏幕中心
        /// </summary>
        public Control OwnerControl
        {
            get;
            set;
        } = null;

        TextBox TextBox_Value;
        Border mMainBorder;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            mMainBorder = Template.FindName("PART_Border", this) as Border;
            if(mMainBorder != null)
            {
                // Start
                DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
                EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame()
                {
                    Value = 0,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                };
                animation.KeyFrames.Add(frame);
                frame = new EasingDoubleKeyFrame()
                {
                    Value = 1,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2)),
                };
                animation.KeyFrames.Add(frame);
                Storyboard.SetTargetName(animation, mMainBorder.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(0).(1)[0].(2)",
                                        new DependencyProperty[] {
                                            UIElement.RenderTransformProperty,
                                            TransformGroup.ChildrenProperty,
                                            ScaleTransform.ScaleXProperty,
                                        }));
                mStartStoryboard.Children.Add(animation);
                animation = new DoubleAnimationUsingKeyFrames();
                frame = new EasingDoubleKeyFrame()
                {
                    Value = 0.1,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                };
                animation.KeyFrames.Add(frame);
                frame = new EasingDoubleKeyFrame()
                {
                    Value = 0.1,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2)),
                };
                animation.KeyFrames.Add(frame);
                frame = new EasingDoubleKeyFrame()
                {
                    Value = 1,
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)),
                };
                animation.KeyFrames.Add(frame);
                Storyboard.SetTargetName(animation, mMainBorder.Name);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(0).(1)[0].(2)",
                                        new DependencyProperty[] {
                                            UIElement.RenderTransformProperty,
                                            TransformGroup.ChildrenProperty,
                                            ScaleTransform.ScaleYProperty,
                                        }));
                mStartStoryboard.Children.Add(animation);
                
                mStartStoryboard.Begin(mMainBorder);
            }


            var title = Template.FindName("PART_Title", this) as FrameworkElement;
            if(title != null)
                title.MouseMove += Title_MouseMove;

            TextBox_Value = Template.FindName("PART_Value", this) as TextBox;
            if (TextBox_Value != null)
            {
                TextBox_Value.KeyDown += TextBox_KeyDown;
            }

            var button_Cancel = Template.FindName("PART_Button_Cancel", this) as Button;
            if (button_Cancel != null)
                button_Cancel.Click += Button_Cancel_Click;

            var button_OK = Template.FindName("PART_Button_OK", this) as Button;
            if (button_OK != null)
                button_OK.Click += Button_OK_Click;

            var tgBtn = GetTemplateChild("PART_Button_TopMost") as System.Windows.Controls.Primitives.ToggleButton;
            if (tgBtn != null)
            {
                tgBtn.IsChecked = this.Topmost;
                tgBtn.SetBinding(System.Windows.Controls.Primitives.ToggleButton.IsCheckedProperty, new Binding("Topmost") { Source = this, Mode = BindingMode.TwoWay });
            }

            var btn = GetTemplateChild("PART_Button_Close") as Button;
            if (btn != null)
                btn.Click += Button_Close_Click;
        }

        private void Button_Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        var bindingExpression = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                        if (bindingExpression != null)
                            bindingExpression.UpdateSource();
                    }
                    break;
            }
        }

        private void Button_Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TextBox_Value != null)
            {
                var bindExp = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as RequiredRule;
                        if (rule != null)
                        {
                            rule.OnValidateCheck = null;
                        }
                    }
                }
            }

            mResult = false;
            this.Close();
        }

        private void Button_OK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (TextBox_Value != null)
            {
                if (Validation.GetHasError(TextBox_Value))
                    return;

                var bindExp = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                if (bindExp != null)
                {
                    if (bindExp.ParentBinding.ValidationRules.Count > 0)
                    {
                        var rule = bindExp.ParentBinding.ValidationRules[0] as RequiredRule;
                        if (rule != null)
                        {
                            rule.OnValidateCheck = null;
                        }
                    }
                }
            }

            var bindingExpression = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
                bindingExpression.UpdateSource();

            mResult = true;
            this.Close();
        }

        //
        // 摘要:
        //     打开窗口并返回，而不等待新打开的窗口关闭。
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     对正在关闭 (System.Windows.Window.Closing) 或已经关闭 (System.Windows.Window.Closed) 的窗口调用
        //     System.Windows.Window.Show。
        public new void Show()
        {
            ShowDialog();
        }

        public bool? ShowDialog(Delegate_ValidateCheck onCheck)
        {
            try
            {
                mOnValidateCheck = onCheck;
                return this.ShowDialog();
            }
            catch (System.Exception ex)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "InputWindow Exception", ex.ToString());
            }

            return false;
        }
    }
}
