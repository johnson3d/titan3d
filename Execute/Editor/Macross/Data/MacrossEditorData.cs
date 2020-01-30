using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Macross.Data
{
    public class MacrossEditorData
    {
        public string EditorTypeName { get; set; }
        public string ValidResourceName => "Macross";
        public ImageSource ResourceIcon { get; set; }
    }
}
