using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace EditorCommon.Tree
{
    public class TreeListSortBehavior
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty HeaderSortProperty =
            DependencyProperty.RegisterAttached("HeaderSort", typeof(bool), typeof(TreeListSortBehavior), new UIPropertyMetadata(new PropertyChangedCallback(OnHeaderSortPropertyChanged)));

        /// <summary>
        /// 
        /// </summary>
        internal static readonly DependencyPropertyKey SortInfoProperty =
            DependencyProperty.RegisterAttachedReadOnly("SortInfo", typeof(ResourceLibrary.Sort.SortInfo), typeof(TreeListSortBehavior), new PropertyMetadata());

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SortFieldProperty =
            DependencyProperty.RegisterAttached("SortField", typeof(string), typeof(TreeListSortBehavior));

        public static bool GetHeaderSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(HeaderSortProperty);
        }

        public static void SetHeaderSort(DependencyObject obj, bool value)
        {
            obj.SetValue(HeaderSortProperty, value);
        }

        public static ResourceLibrary.Sort.SortInfo GetSortInfo(DependencyObject obj)
        {
            return (ResourceLibrary.Sort.SortInfo)obj.GetValue(SortInfoProperty.DependencyProperty);
        }

        internal static void SetSortInfo(DependencyObject obj, ResourceLibrary.Sort.SortInfo value)
        {
            obj.SetValue(SortInfoProperty.DependencyProperty, value);
        }

        public static string GetSortField(DependencyObject obj)
        {
            return (string)obj.GetValue(SortFieldProperty);
        }

        public static void SetSortField(DependencyObject obj, string value)
        {
            obj.SetValue(SortFieldProperty, value);
        }

        private static void OnHeaderSortPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var listView = sender as TreeListView.TreeList;
            if (listView == null)
                throw new InvalidOperationException("HeaderSort Property can only be set on a TreeList");

            if ((bool)e.NewValue)
            {
                listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnListViewHeaderClick));
            }
            else
            {
                listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnListViewHeaderClick));
            }
        }

        private static void OnListViewHeaderClick(object sender, RoutedEventArgs e)
        {
            var listView = e.Source as TreeListView.TreeList;
            GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
            var sortInfo = listView.GetValue(SortInfoProperty.DependencyProperty) as ResourceLibrary.Sort.SortInfo;

            if (sortInfo != null)
            {
                AdornerLayer.GetAdornerLayer(sortInfo.LastSortColumn).Remove(sortInfo.CurrentAdorner);
                listView.Items.SortDescriptions.Clear();
            }
            else
                sortInfo = new ResourceLibrary.Sort.SortInfo();
            if (sortInfo.CurrentAdorner == null)
                return;
            if (sortInfo.LastSortColumn == header)
                (sortInfo.CurrentAdorner.Child as ResourceLibrary.Sort.SortDecorator).SortDirection = (sortInfo.CurrentAdorner.Child as ResourceLibrary.Sort.SortDecorator).SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
                sortInfo.CurrentAdorner = new ResourceLibrary.Sort.UIElementAdorner_Sort(header, new ResourceLibrary.Sort.SortDecorator());

            sortInfo.LastSortColumn = header;
            listView.SetValue(SortInfoProperty, sortInfo);

            AdornerLayer.GetAdornerLayer(header).Add(sortInfo.CurrentAdorner);
            SortDescription sortDescriptioin = new SortDescription()
            {
                Direction = (sortInfo.CurrentAdorner.Child as ResourceLibrary.Sort.SortDecorator).SortDirection,
                PropertyName = header.Column.GetValue(SortFieldProperty) as string ?? header.Column.Header as string
            };
            listView.Items.SortDescriptions.Add(sortDescriptioin);
        }
    }
}
