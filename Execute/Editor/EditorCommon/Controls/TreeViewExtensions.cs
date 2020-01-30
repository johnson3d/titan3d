using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EditorCommon.Controls
{
    public class TreeViewExtensions : DependencyObject
    {
        public static bool GetEnableMultiSelect(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableMultiSelectProperty);
        }

        public static void SetEnableMultiSelect(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableMultiSelectProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableMultiSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableMultiSelectProperty =
            DependencyProperty.RegisterAttached("EnableMultiSelect", typeof(bool), typeof(TreeViewExtensions), new FrameworkPropertyMetadata(false)
            {
                PropertyChangedCallback = EnableMultiSelectChanged,
                BindsTwoWayByDefault = true
            });






        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(TreeViewExtensions), new PropertyMetadata(null));



        static TreeViewItem GetAnchorItem(DependencyObject obj)
        {
            return (TreeViewItem)obj.GetValue(AnchorItemProperty);
        }

        static void SetAnchorItem(DependencyObject obj, TreeViewItem value)
        {
            obj.SetValue(AnchorItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for AnchorItem.  This enables animation, styling, binding, etc...
        static readonly DependencyProperty AnchorItemProperty =
            DependencyProperty.RegisterAttached("AnchorItem", typeof(TreeViewItem), typeof(TreeViewExtensions), new PropertyMetadata(null));



        static void EnableMultiSelectChanged(DependencyObject s, DependencyPropertyChangedEventArgs args)
        {
            TreeView tree = (TreeView)s;
            var wasEnable = (bool)args.OldValue;
            var isEnabled = (bool)args.NewValue;
            if (wasEnable)
            {
                tree.RemoveHandler(TreeViewItem.MouseUpEvent, new MouseButtonEventHandler(ItemClicked));
                tree.RemoveHandler(TreeView.KeyDownEvent, new KeyEventHandler(KeyDown));
            }
            if (isEnabled)
            {
                tree.AddHandler(TreeViewItem.MouseUpEvent, new MouseButtonEventHandler(ItemClicked), true);
                tree.AddHandler(TreeView.KeyDownEvent, new KeyEventHandler(KeyDown));
            }
        }

        static TreeView GetTree(TreeViewItem item)
        {
            Func<DependencyObject, DependencyObject> getParent = (o) =>
            {
                if (o != null)
                    return VisualTreeHelper.GetParent(o);
                return null;
                };
            FrameworkElement currentItem = item;
            while (!(getParent(currentItem) is TreeView) && getParent(currentItem) != null)
                currentItem = (FrameworkElement)getParent(currentItem);
            return (TreeView)getParent(currentItem);
        }



        static void RealSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TreeViewItem item = (TreeViewItem)sender;
            var tree = GetTree(item);
            if (tree != null)
            {
                var selectedItems = GetSelectedItems(tree);
                if (selectedItems != null)
                {
                    var type = selectedItems.GetType().GenericTypeArguments[0];

                    Type headerType = null;
                    if (item.Header != null)
                        headerType= item.Header.GetType();
                    if (headerType!=null &&( headerType == type || headerType.IsSubclassOf(type)))
                    {
                        var isSelected = GetIsSelected(item);
                        if (isSelected)
                        {
                            try
                            {
                                if (item.Header != null && !selectedItems.Contains(item.Header))
                                    selectedItems.Add(item.Header);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                        else
                        {
                            if (item.Header != null)
                                selectedItems.Remove(item.Header);
                        }
                    }
                }
            }
        }

        static void KeyDown(object sender, KeyEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                foreach (var item in GetExpandedTreeViewItems(tree))
                {
                    SetIsSelected(item, true);
                }
                e.Handled = true;
            }
        }

        static void ItemClicked(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = FindTreeViewItem(e.OriginalSource);
            if (item == null)
                return;
            TreeView tree = (TreeView)sender;

            var mouseButton = e.ChangedButton;
            if (mouseButton != MouseButton.Left)
            {
                if ((mouseButton == MouseButton.Right) && ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None))
                {
                    if (GetIsSelected(item))
                    {
                        UpdateAnchorAndActionItem(tree, item);
                        return;
                    }
                    MakeSingleSelection(tree, item);
                }
                return;
            }
            if (mouseButton != MouseButton.Left)
            {
                if ((mouseButton == MouseButton.Right) && ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None))
                {
                    if (GetIsSelected(item))
                    {
                        UpdateAnchorAndActionItem(tree, item);
                        return;
                    }
                    MakeSingleSelection(tree, item);
                }
                return;
            }
            if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != (ModifierKeys.Shift | ModifierKeys.Control))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    MakeToggleSelection(tree, item);
                    return;
                }
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    MakeAnchorSelection(tree, item, true);
                    return;
                }
                MakeSingleSelection(tree, item);
                return;
            }
            //MakeAnchorSelection(item, false);


            //SetIsSelected(tree.SelectedItem
        }

        private static TreeViewItem FindTreeViewItem(object obj)
        {
            DependencyObject dpObj = obj as DependencyObject;
            if (dpObj == null)
                return null;
            if (dpObj is TreeViewItem)
                return (TreeViewItem)dpObj;
            return FindTreeViewItem(VisualTreeHelper.GetParent(dpObj));
        }



        private static IEnumerable<TreeViewItem> GetExpandedTreeViewItems(ItemsControl tree)
        {
            for (int i = 0; i < tree.Items.Count; i++)
            {
                var item = (TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(i);
                if (item == null)
                    continue;
                yield return item;
                if (item.IsExpanded)
                    foreach (var subItem in GetExpandedTreeViewItems(item))
                        yield return subItem;
            }
        }

        private static void MakeAnchorSelection(TreeView tree, TreeViewItem actionItem, bool clearCurrent)
        {
            if (GetAnchorItem(tree) == null)
            {
                var selectedItems = GetSelectedTreeViewItems(tree);
                if (selectedItems.Count > 0)
                {
                    SetAnchorItem(tree, selectedItems[selectedItems.Count - 1]);
                }
                else
                {
                    SetAnchorItem(tree, GetExpandedTreeViewItems(tree).Skip(3).FirstOrDefault());
                }
                if (GetAnchorItem(tree) == null)
                {
                    return;
                }
            }

            var anchor = GetAnchorItem(tree);

            var items = GetExpandedTreeViewItems(tree);
            bool betweenBoundary = false;
            //bool end = false;
            foreach (var item in items)
            {
                bool isBoundary = item == anchor || item == actionItem;
                if (isBoundary)
                {
                    betweenBoundary = !betweenBoundary;
                }
                if (betweenBoundary || isBoundary)
                    SetIsSelected(item, true);
                else
                    if (clearCurrent)
                    SetIsSelected(item, false);
                else
                    break;

            }
        }

        private static List<TreeViewItem> GetSelectedTreeViewItems(TreeView tree)
        {
            return GetExpandedTreeViewItems(tree).Where(i => GetIsSelected(i)).ToList();
        }

        private static void MakeSingleSelection(TreeView tree, TreeViewItem item)
        {
            foreach (TreeViewItem selectedItem in GetExpandedTreeViewItems(tree))
            {
                if (selectedItem == null)
                    continue;
                if (selectedItem != item)
                    SetIsSelected(selectedItem, false);
                else
                {
                    SetIsSelected(selectedItem, true);
                }
            }
            UpdateAnchorAndActionItem(tree, item);
        }

        private static void MakeToggleSelection(TreeView tree, TreeViewItem item)
        {
            SetIsSelected(item, !GetIsSelected(item));
            UpdateAnchorAndActionItem(tree, item);
        }

        private static void UpdateAnchorAndActionItem(TreeView tree, TreeViewItem item)
        {
            SetAnchorItem(tree, item);
        }




        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(TreeViewExtensions), new PropertyMetadata(false)
            {
                PropertyChangedCallback = RealSelectedChanged
            });
    }
    public class OutlinerTreeViewExtensions : DependencyObject
    {
        public static bool GetEnableMultiSelect(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableMultiSelectProperty);
        }

        public static void SetEnableMultiSelect(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableMultiSelectProperty, value);
        }

        // Using a DependencyProperty as the backing store for EnableMultiSelect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnableMultiSelectProperty =
            DependencyProperty.RegisterAttached("EnableMultiSelect", typeof(bool), typeof(OutlinerTreeViewExtensions), new FrameworkPropertyMetadata(false)
            {
                PropertyChangedCallback = EnableMultiSelectChanged,
                BindsTwoWayByDefault = true
            });






        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(OutlinerTreeViewExtensions), new PropertyMetadata(null));



        static TreeViewItem GetAnchorItem(DependencyObject obj)
        {
            return (TreeViewItem)obj.GetValue(AnchorItemProperty);
        }

        static void SetAnchorItem(DependencyObject obj, TreeViewItem value)
        {
            obj.SetValue(AnchorItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for AnchorItem.  This enables animation, styling, binding, etc...
        static readonly DependencyProperty AnchorItemProperty =
            DependencyProperty.RegisterAttached("AnchorItem", typeof(TreeViewItem), typeof(OutlinerTreeViewExtensions), new PropertyMetadata(null));



        static void EnableMultiSelectChanged(DependencyObject s, DependencyPropertyChangedEventArgs args)
        {
            TreeView tree = (TreeView)s;
            var wasEnable = (bool)args.OldValue;
            var isEnabled = (bool)args.NewValue;
            if (wasEnable)
            {
                tree.RemoveHandler(TreeViewItem.MouseUpEvent, new MouseButtonEventHandler(ItemClicked));
                tree.RemoveHandler(TreeView.KeyDownEvent, new KeyEventHandler(KeyDown));
            }
            if (isEnabled)
            {
                tree.AddHandler(TreeViewItem.MouseUpEvent, new MouseButtonEventHandler(ItemClicked), true);
                tree.AddHandler(TreeView.KeyDownEvent, new KeyEventHandler(KeyDown));
            }
        }

        static TreeView GetTree(TreeViewItem item)
        {
            Func<DependencyObject, DependencyObject> getParent = (o) =>
            {
                if (o != null)
                    return VisualTreeHelper.GetParent(o);
                return null;
            };
            FrameworkElement currentItem = item;
            while (!(getParent(currentItem) is TreeView) && getParent(currentItem) != null)
                currentItem = (FrameworkElement)getParent(currentItem);
            return (TreeView)getParent(currentItem);
        }



        static void RealSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TreeViewItem item = (TreeViewItem)sender;
            var tree = GetTree(item);
            if (tree != null)
            {
                var selectedItems = GetSelectedItems(tree);
                if (selectedItems != null)
                {
                    var type =selectedItems.GetType().GenericTypeArguments[0];
                    Type headerType = null;
                    if (item.Header != null)
                        headerType = item.Header.GetType();
                    if (headerType != null && (headerType == type || headerType.IsSubclassOf(type)))
                    {
                        var isSelected = GetIsSelected(item);
                        if (isSelected)
                        {
                            try
                            {
                                if (item.Header != null && !selectedItems.Contains(item.Header))
                                    selectedItems.Add(item.Header);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                        else
                        {
                            if (item.Header != null)
                                selectedItems.Remove(item.Header);
                        }
                    }
                }
            }
        }

        static void KeyDown(object sender, KeyEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                foreach (var item in GetExpandedTreeViewItems(tree))
                {
                    SetIsSelected(item, true);
                }
                e.Handled = true;
            }
        }

        static void ItemClicked(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var outlinerItemCheckedPri = false;
            TreeViewItem item = FindTreeViewItem(e.OriginalSource, ref outlinerItemCheckedPri);
            if (item == null)
                return;
            TreeView tree = (TreeView)sender;

            var mouseButton = e.ChangedButton;
            if (mouseButton != MouseButton.Left)
            {
                if ((mouseButton == MouseButton.Right) && ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None))
                {
                    if (GetIsSelected(item))
                    {
                        UpdateAnchorAndActionItem(tree, item);
                        return;
                    }
                    MakeSingleSelection(tree, item);
                }
                return;
            }
            if (mouseButton != MouseButton.Left)
            {
                if ((mouseButton == MouseButton.Right) && ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) == ModifierKeys.None))
                {
                    if (GetIsSelected(item))
                    {
                        UpdateAnchorAndActionItem(tree, item);
                        return;
                    }
                    MakeSingleSelection(tree, item);
                }
                return;
            }
            if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != (ModifierKeys.Shift | ModifierKeys.Control))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    MakeToggleSelection(tree, item);
                    return;
                }
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    MakeAnchorSelection(tree, item, true);
                    return;
                }
                MakeSingleSelection(tree, item);
                return;
            }
            //MakeAnchorSelection(item, false);


            //SetIsSelected(tree.SelectedItem
        }

        private static TreeViewItem FindTreeViewItem(object obj,ref bool outlinerItemCheckedPri)
        {
            FrameworkElement dpObj = obj as FrameworkElement;
            if (dpObj == null)
                return null;
            if(dpObj.Tag is string)
            {
                if("OutlinerItemCheckedPri" == dpObj.Tag as string)
                    outlinerItemCheckedPri = true;
            }
            if (dpObj is TreeViewItem)
            {
                if(outlinerItemCheckedPri)
                    return (TreeViewItem)dpObj;
                return null;
            }
            return FindTreeViewItem(VisualTreeHelper.GetParent(dpObj), ref outlinerItemCheckedPri);
        }



        private static IEnumerable<TreeViewItem> GetExpandedTreeViewItems(ItemsControl tree)
        {
            for (int i = 0; i < tree.Items.Count; i++)
            {
                var item = (TreeViewItem)tree.ItemContainerGenerator.ContainerFromIndex(i);
                if (item == null)
                    continue;
                yield return item;
                if (item.IsExpanded)
                    foreach (var subItem in GetExpandedTreeViewItems(item))
                        yield return subItem;
            }
        }

        private static void MakeAnchorSelection(TreeView tree, TreeViewItem actionItem, bool clearCurrent)
        {
            if (GetAnchorItem(tree) == null)
            {
                var selectedItems = GetSelectedTreeViewItems(tree);
                if (selectedItems.Count > 0)
                {
                    SetAnchorItem(tree, selectedItems[selectedItems.Count - 1]);
                }
                else
                {
                    SetAnchorItem(tree, GetExpandedTreeViewItems(tree).Skip(3).FirstOrDefault());
                }
                if (GetAnchorItem(tree) == null)
                {
                    return;
                }
            }

            var anchor = GetAnchorItem(tree);

            var items = GetExpandedTreeViewItems(tree);
            bool betweenBoundary = false;
            //bool end = false;
            foreach (var item in items)
            {
                bool isBoundary = item == anchor || item == actionItem;
                if (isBoundary)
                {
                    betweenBoundary = !betweenBoundary;
                }
                if (betweenBoundary || isBoundary)
                    SetIsSelected(item, true);
                else
                    if (clearCurrent)
                    SetIsSelected(item, false);
                else
                    break;

            }
        }

        private static List<TreeViewItem> GetSelectedTreeViewItems(TreeView tree)
        {
            return GetExpandedTreeViewItems(tree).Where(i => GetIsSelected(i)).ToList();
        }

        private static void MakeSingleSelection(TreeView tree, TreeViewItem item)
        {
            foreach (TreeViewItem selectedItem in GetExpandedTreeViewItems(tree))
            {
                if (selectedItem == null)
                    continue;
                if (selectedItem != item)
                    SetIsSelected(selectedItem, false);
                else
                {
                    SetIsSelected(selectedItem, true);
                }
            }
            UpdateAnchorAndActionItem(tree, item);
        }

        private static void MakeToggleSelection(TreeView tree, TreeViewItem item)
        {
            SetIsSelected(item, !GetIsSelected(item));
            UpdateAnchorAndActionItem(tree, item);
        }

        private static void UpdateAnchorAndActionItem(TreeView tree, TreeViewItem item)
        {
            SetAnchorItem(tree, item);
        }




        public static bool GetIsSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectedProperty);
        }

        public static void SetIsSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectedProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(OutlinerTreeViewExtensions), new PropertyMetadata(false)
            {
                PropertyChangedCallback = RealSelectedChanged
            });
    }
}
