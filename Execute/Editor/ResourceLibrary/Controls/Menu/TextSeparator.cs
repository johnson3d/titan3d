using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace ResourceLibrary.Controls.Menu
{
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public class TextSeparator : Separator
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextSeparator), new FrameworkPropertyMetadata(""));

        static TextSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextSeparator), new FrameworkPropertyMetadata(typeof(TextSeparator)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SeparatorAutomationPeer(this);
        }
    }
}
