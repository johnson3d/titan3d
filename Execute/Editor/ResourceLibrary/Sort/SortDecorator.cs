using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ResourceLibrary.Sort
{
    public class SortDecorator : Control
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adorner"></param>
        static SortDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SortDecorator), new FrameworkPropertyMetadata(typeof(SortDecorator)));
        }

        public ListSortDirection SortDirection
        {
            get { return (ListSortDirection)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register("SortDirection", typeof(ListSortDirection), typeof(SortDecorator));
    }
}
