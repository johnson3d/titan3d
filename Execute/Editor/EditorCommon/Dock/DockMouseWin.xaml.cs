using System.Windows;
using System.Windows.Controls;

namespace DockControl
{
    /// <summary>
    /// DockMouseWin.xaml 的交互逻辑
    /// </summary>
    public partial class DockMouseWin : Window
	{
        Controls.DockAbleTabItem mDockElement = null;
        public Controls.DockAbleTabItem DockElement
        {
            get => mDockElement;
            set
            {
                mDockElement = value;

                EditorCommon.Program.RemoveElementFromParent(mDockElement);

                if (mDockElement is TabItem)
                {
                    var tabCtrl = new DockControl.Controls.DockAbleTabControl();
                    if(!tabCtrl.Items.Contains(mDockElement))
                        tabCtrl.Items.Add(mDockElement);
                    tabCtrl.SelectedItem = mDockElement;
                    LayoutRoot.Children.Clear();
                    LayoutRoot.Children.Add(tabCtrl);
                    Show();
                }
                else
                {
                    LayoutRoot.Children.Clear();
                    if(mDockElement != null)
                        LayoutRoot.Children.Add(mDockElement);
                    Hide();
                }
            }
        }

		public DockMouseWin()
		{
			this.InitializeComponent();
		}

        public bool NeedClose = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            if(!NeedClose)
            {
                this.Hide();
                e.Cancel = true;
            }
		}
	}
}