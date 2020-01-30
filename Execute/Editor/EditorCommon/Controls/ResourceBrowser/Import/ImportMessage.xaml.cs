using EngineNS;
using EngineNS.Bricks.AssetImpExp;
using EngineNS.Bricks.AssetImpExp.Creater;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Shapes;

namespace EditorCommon.Controls.ResourceBrowser.Import
{
    /// <summary>
    /// Interaction logic for ImportMessage.xaml
    /// </summary>
    public partial class ImportMessage : ResourceLibrary.WindowBase, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public ImportMessage()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        protected override void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
