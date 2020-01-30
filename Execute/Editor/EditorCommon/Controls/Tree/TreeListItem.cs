using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace EditorCommon.TreeListView
{
	public class TreeListItem : ListViewItem, INotifyPropertyChanged
    {
		#region Properties

		private TreeNode _node;
		public TreeNode Node
		{
			get { return _node; }
			internal set
			{
                if(_node != null)
                {
                    BindingOperations.ClearBinding(this, ListBoxItem.IsSelectedProperty);
                }
				_node = value;
                if(_node != null)
                {
                    BindingOperations.SetBinding(this, ListBoxItem.IsSelectedProperty, new Binding("IsSelected") { Source = _node });
                }
				OnPropertyChanged("Node");
			}
		}

        #endregion
        
        public TreeListItem()
		{
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Node != null)
			{
				switch (e.Key)
				{
					case Key.Right:
						e.Handled = true;
						if (!Node.IsExpanded)
						{
							Node.IsExpanded = true;
							ChangeFocus(Node);
						}
						else if (Node.Children.Count > 0)
							ChangeFocus(Node.Children[0]);
						break;

					case Key.Left:

						e.Handled = true;
						if (Node.IsExpanded && Node.IsExpandable)
						{
							Node.IsExpanded = false;
							ChangeFocus(Node);
						}
						else
							ChangeFocus(Node.Parent);
						break;

					case Key.Subtract:
						e.Handled = true;
						Node.IsExpanded = false;
						ChangeFocus(Node);
						break;

					case Key.Add:
						e.Handled = true;
						Node.IsExpanded = true;
						ChangeFocus(Node);
						break;
				}
			}

			if (!e.Handled)
				base.OnKeyDown(e);
		}

		private void ChangeFocus(TreeNode node)
		{
			var tree = node.Tree;
			if (tree != null)
			{
				var item = tree.ItemContainerGenerator.ContainerFromItem(node) as TreeListItem;
				if (item != null)
					item.Focus();
				else
					tree.PendingFocusNode = node;
			}
		}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var data = this.DataContext as ITreeModel;
            if(data != null)
            {
                if (data.EnableDrop)
                {
                    var dropUpShow = Template.FindName("PART_DropUpShow", this) as FrameworkElement;
                    var rowUp = Template.FindName("PART_RowUp", this) as FrameworkElement;
                    if (rowUp != null)
                    {
                        rowUp.DragEnter += (object sender, DragEventArgs e) =>
                        {
                            if (dropUpShow != null)
                                dropUpShow.Visibility = Visibility.Visible;
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"放入{data.Name}之前";
                            data.OnDragEnter_Before(sender, e);
                        };
                        rowUp.DragLeave += (object sender, DragEventArgs e) =>
                        {
                            if (dropUpShow != null)
                                dropUpShow.Visibility = Visibility.Collapsed;
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
                            data.OnDragLeave_Before(sender, e);
                        };
                        rowUp.DragOver += (object sender, DragEventArgs e) =>
                        {
                            data.OnDragOver_Before(sender, e);
                        };
                        rowUp.Drop += (object sender, DragEventArgs e) =>
                        {
                            data.OnDrop_Before(sender, e);
                        };
                    }

                    var dropDownShow = Template.FindName("PART_DropDownShow", this) as FrameworkElement;
                    var rowDown = Template.FindName("PART_RowDown", this) as FrameworkElement;
                    if(rowDown != null)
                    {
                        rowDown.DragEnter += (object sender, DragEventArgs e) =>
                        {
                            if (dropDownShow != null)
                                dropDownShow.Visibility = Visibility.Visible;
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"放入{data.Name}之后";
                            data.OnDragEnter_After(sender, e);
                        };
                        rowDown.DragLeave += (object sender, DragEventArgs e) =>
                        {
                            if (dropDownShow != null)
                                dropDownShow.Visibility = Visibility.Collapsed;
                            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = "";
                            data.OnDragLeave_After(sender, e);
                        };
                        rowDown.DragOver += (object sender, DragEventArgs e) =>
                        {
                            data.OnDragOver_After(sender, e);
                        };
                        rowDown.Drop += (object sender, DragEventArgs e) =>
                        {
                            data.OnDrop_After(sender, e);
                        };
                    }

                    var gvrp = Template.FindName("PART_GVRP", this) as FrameworkElement;
                    if(gvrp != null)
                    {
                        gvrp.DragEnter += (object sender, DragEventArgs e) =>
                        {
                            data.OnDragEnter(sender, e);
                        };
                        gvrp.DragLeave += (object sender, DragEventArgs e) =>
                        {
                            data.OnDragLeave(sender, e);
                        };
                        gvrp.DragOver += (object sender, DragEventArgs e) =>
                        {
                            data.OnDragOver(sender, e);
                        };
                        gvrp.Drop += (object sender, DragEventArgs e) =>
                        {
                            data.OnDrop(sender, e);
                        };
                    }
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string name)
		{
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, name, PropertyChanged);
        }

		#endregion
	}
}