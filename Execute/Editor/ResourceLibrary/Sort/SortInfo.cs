using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ResourceLibrary.Sort
{
    public class SortInfo
    {
        public GridViewColumnHeader LastSortColumn { get; set; }

        public UIElementAdorner_Sort CurrentAdorner { get; set; }
    }
}
