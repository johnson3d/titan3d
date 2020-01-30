using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeGenerateSystem.Controls
{
    /// <summary>
    /// ErrorListItem.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorListItem : UserControl
    {
        Base.BaseNodeControl mNode;
        public Base.BaseNodeControl Node
        {
            get { return mNode; }
        }
        string mErrorMsg;

        public ErrorListItem(Base.BaseNodeControl node, string errorMsg)
        {
            InitializeComponent();

            mNode = node;
            mErrorMsg = errorMsg;
        }

        public void Update()
        {
            if (mNode != null)
            {
                NameTextBlock.Text = mNode.NodeName + ":  " + mErrorMsg;
                NameTextBlock.Foreground = Brushes.White;
                if (mNode.HasWarning)
                {
                    WarningEllipse.Visibility = Visibility.Visible;
                    NameTextBlock.Foreground = Brushes.Yellow;
                }
                else
                    WarningEllipse.Visibility = Visibility.Hidden;

                if (mNode.HasError)
                {
                    ErrorEllipse.Visibility = Visibility.Visible;
                    NameTextBlock.Foreground = Brushes.Red;
                }
                else
                    ErrorEllipse.Visibility = Visibility.Hidden;

                ToolTipRect.Width = mNode.GetWidth() + 30;
                ToolTipRect.Height = mNode.GetHeight() + 30;
                ToolTipRect.Fill = new VisualBrush(mNode);
            }
        }
    }
}
