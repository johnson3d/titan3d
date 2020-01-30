using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace ResourceLibrary
{
	public class TreeList: ListView
	{
		#region Properties

		/// <summary>
		/// Internal collection of rows representing visible nodes, actually displayed in the ListView
		/// </summary>
		internal ObservableCollectionAdv<TreeNode> Rows
		{
			get;
			private set;
		}

        Dictionary<ITreeModel, TreeNode> mTreeNodeDic = new Dictionary<ITreeModel, TreeNode>();
        ObservableCollectionAdv<ITreeModel> mTreeListItemsSource = new ObservableCollectionAdv<ITreeModel>();
        // 所有内容通过此属性进行绑定，不要直接操作Items
        public ObservableCollectionAdv<ITreeModel> TreeListItemsSource
        {
            get { return mTreeListItemsSource; }
            set
            {
                mTreeNodeDic.Clear();
                if (mTreeListItemsSource != null)
                    mTreeListItemsSource.CollectionChanged -= RootChildrenChanged;
                mTreeListItemsSource = value;
                if (mTreeListItemsSource != null)
                {
                    mTreeListItemsSource.CollectionChanged += RootChildrenChanged;
                    _root.Children.Clear();
                    Rows.Clear();
                    foreach (var item in mTreeListItemsSource)
                    {
                        var child = new TreeNode(this, item);
                        Root.Children.Add(child);
                        mTreeNodeDic[item] = child;
                    }
                    Rows.InsertRange(0, Root.Children.ToArray());
                }
            }
        }

		//private ITreeModel _model;
		//public ITreeModel Model
		//{
		//  get { return _model; }
		//  set 
		//  {
		//	  if (_model != value)
		//	  {
		//		  _model = value;
		//		  _root.Children.Clear();
		//		  Rows.Clear();
		//		  CreateChildrenNodes(_root);
		//	  }
		//  }
		//}

		private TreeNode _root;
		internal TreeNode Root
		{
			get { return _root; }
		}

		public ReadOnlyCollection<TreeNode> Nodes
		{
			get { return Root.Nodes; }
		}

		internal TreeNode PendingFocusNode
		{
			get;
			set;
		}

		public ICollection<TreeNode> SelectedNodes
		{
			get
			{
				return SelectedItems.Cast<TreeNode>().ToArray();
			}
		}

		public TreeNode SelectedNode
		{
			get
			{
				if (SelectedItems.Count > 0)
					return SelectedItems[0] as TreeNode;
				else
					return null;
			}
		}
		#endregion

		public TreeList()
		{
			Rows = new ObservableCollectionAdv<TreeNode>();
			_root = new TreeNode(this, null);
			_root.IsExpanded = true;
			ItemsSource = Rows;
			ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
            this.SelectionChanged += TreeList_SelectionChanged;
        }

        void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
		{
			if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated && PendingFocusNode != null)
			{
				var item = ItemContainerGenerator.ContainerFromItem(PendingFocusNode) as TreeListItem;
				if (item != null)
					item.Focus();
				PendingFocusNode = null;
			}
		}

		protected override DependencyObject GetContainerForItemOverride()
		{
			return new TreeListItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is TreeListItem;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			var ti = element as TreeListItem;
			var node = item as TreeNode;
			if (ti != null && node != null)
			{
				ti.Node = item as TreeNode;
				base.PrepareContainerForItemOverride(element, node.Tag);
			}
		}

		internal void SetIsExpanded(TreeNode node, bool value)
		{
			if (value)
			{
				if (!node.IsExpandedOnce)
				{
					node.IsExpandedOnce = true;
					node.AssignIsExpanded(value);
					CreateChildrenNodes(node);
				}
				else
				{
					node.AssignIsExpanded(value);
					CreateChildrenRows(node);
				}
			}
			else
			{
				DropChildrenRows(node, false);
				node.AssignIsExpanded(value);
			}
		}

		internal void CreateChildrenNodes(TreeNode node)
		{
			var children = GetChildren(node);
			if (children != null)
			{
				int rowIndex = Rows.IndexOf(node);
				node.ChildrenSource = children as INotifyCollectionChanged;
				foreach (ITreeModel obj in children)
				{
					TreeNode child = new TreeNode(this, obj);
					//child.HasChildren = HasChildren(child);
					node.Children.Add(child);
                    mTreeNodeDic[obj] = child;
                }
				Rows.InsertRange(rowIndex + 1, node.Children.ToArray());
			}
		}

		private void CreateChildrenRows(TreeNode node)
		{
			int index = Rows.IndexOf(node);
			if (index >= 0 || node == _root) // ignore invisible nodes
			{
				var nodes = node.AllVisibleChildren.ToArray();
				Rows.InsertRange(index + 1, nodes);
			}
		}

		internal void DropChildrenRows(TreeNode node, bool removeParent)
		{
			int start = Rows.IndexOf(node);
			if (start >= 0 || node == _root) // ignore invisible nodes
			{
				int count = node.VisibleChildrenCount;
				if (removeParent)
					count++;
				else
					start++;
				Rows.RemoveRange(start, count);
			}
		}

		private IEnumerable GetChildren(TreeNode parent)
		{
            if (parent == null || parent.Tag == null)
                return null;
            return parent.Tag.GetChildren();
		}

		private bool HasChildren(TreeNode parent)
		{
			if (parent == Root)
				return true;
            if (parent == null)
                return false;
            return parent.HasChildren;
		}

		internal void InsertNewNode(TreeNode parent, ITreeModel tag, int rowIndex, int index)
		{
			TreeNode node = new TreeNode(this, tag);
			if (index >= 0 && index < parent.Children.Count)
				parent.Children.Insert(index, node);
			else
			{
				index = parent.Children.Count;
				parent.Children.Add(node);
			}
            var idx = rowIndex + index + 1;
            if (idx < Rows.Count)
                Rows.Insert(idx, node);
            else
                Rows.Add(node);
            mTreeNodeDic[tag] = node;
        }

        void RootChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch(e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            if(e.NewItems != null)
                            {
                                int index = e.NewStartingIndex;
                                int rowIndex = this.Rows.Count - 1;
                                foreach (ITreeModel item in e.NewItems)
                                {
                                    this.InsertNewNode(_root, item, rowIndex, index);
                                    index++;
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            if(e.OldItems != null)
                            {
                                foreach (ITreeModel item in e.OldItems)
                                {
                                    if(_root.Tag == null)
                                    {
                                        var children = _root.Children.ToArray();
                                        for(int i=0; i<children.Length; i++)
                                        {
                                            if(children[i].Tag == item)
                                            {
                                                _root.RemoveChildAt(i);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var idx = _root.Tag.IndexOf(item);
                                        _root.RemoveChildAt(idx);
                                    }
                                    mTreeNodeDic.Remove(item);
                                }
                            }
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Reset:
                        mTreeNodeDic.Clear();
                        while (_root.Children.Count > 0)
                            _root.RemoveChildAt(0);
                        CreateChildrenNodes(_root);
                        break;
                }
            });
        }
        protected bool SelectionFromTry = false;
        public void TrySelectItems(List<ITreeModel> items, bool raiseSelectionChange = true)
        {
            if (items == null)
                return;

            if(!raiseSelectionChange)
                SelectionFromTry = true;
            TreeNode firstNode = null;
            // 判断是否在子集里，如果是则展开
            foreach (var item in items)
            {
                TreeNode node;
                if(mTreeNodeDic.TryGetValue(item, out node))
                {
                    var parent = node.Parent;
                    while(parent != null)
                    {
                        parent.IsExpanded = true;
                        parent = parent.Parent;
                    }

                    node.IsSelected = true;
                    if (firstNode != null)
                        firstNode = node;
                }
            }

            if(firstNode != null)
            {
                this.ScrollIntoView(firstNode);
            }
            SelectionFromTry = false;
        }
        public void TryUnSelectItems(List<ITreeModel> items, bool raiseSelectionChange = true)
        {
            if (items == null)
                return;

            if(!raiseSelectionChange)
                SelectionFromTry = true;
            foreach (var item in items)
            {
                TreeNode node;
                if(mTreeNodeDic.TryGetValue(item, out node))
                {
                    node.IsSelected = false;
                }
            }
            SelectionFromTry = false;
        }
        public void TryUnSelectAll(bool raiseSelectionChange = true)
        {
            if (!raiseSelectionChange)
                SelectionFromTry = true;
            base.UnselectAll();
            SelectionFromTry = false;
        }
        public void TrySelectAll(bool raiseSelectionChange = true)
        {
            if (!raiseSelectionChange)
                SelectionFromTry = true;
            base.SelectAll();
            SelectionFromTry = false;
        }
        public event SelectionChangedEventHandler CustomOnSelectionChanged;
        private void TreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!SelectionFromTry)
                CustomOnSelectionChanged?.Invoke(sender, e);
        }
    }
}
