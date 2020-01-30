using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows;

namespace EditorCommon.TreeListView
{
	public interface ITreeModel
	{
		/// <summary>
		/// Get list of children of the specified parent
		/// </summary>
		IEnumerable GetChildren();

		/// <summary>
		/// returns wheather specified parent has any children or not.
		/// </summary>
		bool HasChildren();

        ITreeModel Parent { get; set; }
        int IndexOf(ITreeModel child);

        string Name { get; set; }

        #region DragDrop

        bool EnableDrop { get; }

        void OnDragEnter_Before(object sender, DragEventArgs e);
        void OnDragLeave_Before(object sender, DragEventArgs e);
        void OnDragOver_Before(object sender, DragEventArgs e);
        void OnDrop_Before(object sender, DragEventArgs e);

        void OnDragEnter_After(object sender, DragEventArgs e);
        void OnDragLeave_After(object sender, DragEventArgs e);
        void OnDragOver_After(object sender, DragEventArgs e);
        void OnDrop_After(object sender, DragEventArgs e);

        void OnDragEnter(object sender, DragEventArgs e);
        void OnDragLeave(object sender, DragEventArgs e);
        void OnDragOver(object sender, DragEventArgs e);
        void OnDrop(object sender, DragEventArgs e);
        
        #endregion
    }

    public class TreeItemViewModel : DependencyObject, ITreeModel
    {
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(TreeItemViewModel), new FrameworkPropertyMetadata(null));

        public ITreeModel Parent
        {
            get;
            set;
        }
        public ObservableCollectionAdv<ITreeModel> Children { get; set; } = new ObservableCollectionAdv<ITreeModel>();
        public IEnumerable GetChildren()
        {
            return Children;
        }

        public bool HasChildren()
        {
            return Children.Count > 0;
        }

        public int IndexOf(ITreeModel child)
        {
            return Children.IndexOf(child);
        }

        #region DragDrop

        public virtual bool EnableDrop => false;

        public virtual void OnDragEnter_Before(object sender, DragEventArgs e) { }
        public virtual void OnDragLeave_Before(object sender, DragEventArgs e) { }
        public virtual void OnDragOver_Before(object sender, DragEventArgs e) { }
        public virtual void OnDrop_Before(object sender, DragEventArgs e) { }

        public virtual void OnDragEnter_After(object sender, DragEventArgs e) { }
        public virtual void OnDragLeave_After(object sender, DragEventArgs e) { }
        public virtual void OnDragOver_After(object sender, DragEventArgs e) { }
        public virtual void OnDrop_After(object sender, DragEventArgs e) { }

        public virtual void OnDragEnter(object sender, DragEventArgs e) { }
        public virtual void OnDragLeave(object sender, DragEventArgs e) { }
        public virtual void OnDragOver(object sender, DragEventArgs e) { }
        public virtual void OnDrop(object sender, DragEventArgs e) { }
        

        #endregion

    }
}
