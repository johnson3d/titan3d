using System.Windows;
using System.Windows.Controls;

namespace CodeGenerateSystem
{
    /// <summary>
    /// Interaction logic for CodeViewWindow.xaml
    /// </summary>
    public partial class CodeViewControl : UserControl
    {
        public string CodeString
        {
            get { return TextEditor_Code.Text; }
            set
            {
                TextEditor_Code.Text = value;
            }
        }

        public CodeViewControl()
        {
            InitializeComponent();
        }
    }
}
