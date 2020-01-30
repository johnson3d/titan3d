using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CoreEditor
{
    public partial class MainWindow
    {
        PluginAssist.PluginManagerWindow mPluginWin;
        public PluginAssist.PluginManagerWindow PluginWin
        {
            get { return mPluginWin; }
        }
        private void MenuItem_PluginManager_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (mPluginWin == null)
                mPluginWin = new PluginAssist.PluginManagerWindow();
            mPluginWin.ShowDialog();
        }
    }
}
