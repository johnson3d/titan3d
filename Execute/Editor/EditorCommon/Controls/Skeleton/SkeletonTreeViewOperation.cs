using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DockControl;
using EditorCommon.Controls.Animation;
using EditorCommon.Controls.Skeleton;
using EditorCommon.ResourceInfos;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using EngineNS.IO;

namespace EditorCommon.Controls.Skeleton
{
    public class SkeletonTreeViewOperation
    {
        public TreeView SkeletonTreeView { get; set; } = null;
        public CGfxSkeleton Skeleton { get; set; } = null;
        public EditorBoneDetial SelectBoneDetial { get; set; } = null;
        public TreeViewItem SelectedTreeViewItem { get; set; } = null;

        public DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        public ContextMenu CreateContextMenu(Style style)
        {
            var menu = new ContextMenu();
            menu.Style = style;

            return menu;
        }
        public void CreateExpandCollapseMenu(ContextMenu menu, bool IsExpand)
        {
            if (!IsExpand)
                CreateItmeToContextMenu("展开", menu, ExpandSelected_Click);
            else
                CreateItmeToContextMenu("折叠", menu, CollapseSelected_Click);
            CreateItmeToContextMenu("展开所有", menu, ExpandAll_Click);
            CreateItmeToContextMenu("折叠所有", menu, CollapseAll_Click); //CollapseAll
            CreateSeparator(menu);
        }
        public void CreateSeparator(ContextMenu menu)
        {
            menu.Items.Add(new Separator());
        }
        public void CreateItmeToContextMenu(string itemName, ContextMenu contextMenu, RoutedEventHandler handler)
        {
            MenuItem item = new MenuItem();
            item.Name = "MenuItem_SkeletonTreeViewOperation_ContextMenu_" + itemName;
            item.Header = itemName;
            item.Foreground = Brushes.White;
            item.Click += handler;
            contextMenu.Items.Add(item);
        }
        public void ExpandSelected_Click(object sender, RoutedEventArgs e)
        {
            ((TreeViewItem)SelectedTreeViewItem).ExpandSubtree();
        }
        public void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SkeletonTreeView.Items)
            {
                DependencyObject dObject = SkeletonTreeView.ItemContainerGenerator.ContainerFromItem(item);
                ((TreeViewItem)dObject).ExpandSubtree();
            }

        }
        public void CollapseSelected_Click(object sender, RoutedEventArgs e)
        {
            CollapseTreeviewItems(((TreeViewItem)SelectedTreeViewItem));
        }
        public void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SkeletonTreeView.Items)
            {
                DependencyObject dObject = SkeletonTreeView.ItemContainerGenerator.ContainerFromItem(item);
                CollapseTreeviewItems(((TreeViewItem)dObject));
            }
        }
        private void CollapseTreeviewItems(TreeViewItem Item)
        {
            Item.IsExpanded = false;
            foreach (var item in Item.Items)
            {
                DependencyObject dObject = Item.ItemContainerGenerator.ContainerFromItem(item);

                if (dObject != null)
                {
                    ((TreeViewItem)dObject).IsExpanded = false;

                    if (((TreeViewItem)dObject).HasItems)
                    {
                        CollapseTreeviewItems(((TreeViewItem)dObject));
                    }
                }
            }
        }
    }
}
