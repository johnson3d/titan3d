using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SearchBox
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SearchBox"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SearchBox;assembly=SearchBox"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class SearchBox : Control
    {
        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSearchTextValueChanged), null, true, System.Windows.Data.UpdateSourceTrigger.PropertyChanged));

        public static void OnSearchTextValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as SearchBox;

            var newValue = (string)e.NewValue;

            if (string.IsNullOrEmpty(newValue))
            {
                if (ctrl.SearchImageVisibility != Visibility.Visible)
                    ctrl.SearchImageVisibility = Visibility.Visible;
                if (ctrl.ClearButtonVisibility != Visibility.Collapsed)
                    ctrl.ClearButtonVisibility = Visibility.Collapsed;
                if (ctrl.TipTextVisibility != Visibility.Visible)
                    ctrl.TipTextVisibility = Visibility.Visible;
            }
            else
            {
                if (ctrl.SearchImageVisibility != Visibility.Collapsed)
                    ctrl.SearchImageVisibility = Visibility.Collapsed;
                if (ctrl.ClearButtonVisibility != Visibility.Visible)
                    ctrl.ClearButtonVisibility = Visibility.Visible;
                if (ctrl.TipTextVisibility != Visibility.Collapsed)
                    ctrl.TipTextVisibility = Visibility.Collapsed;
            }
        }

        public Visibility SearchImageVisibility
        {
            get { return (Visibility)GetValue(SearchImageVisibilityProperty); }
            set { SetValue(SearchImageVisibilityProperty, value); }
        }
        public static readonly DependencyProperty SearchImageVisibilityProperty =
            DependencyProperty.Register("SearchImageVisibility", typeof(Visibility), typeof(SearchBox), new FrameworkPropertyMetadata(Visibility.Visible));

        public Visibility ClearButtonVisibility
        {
            get { return (Visibility)GetValue(ClearButtonVisibilityProperty); }
            set { SetValue(ClearButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ClearButtonVisibilityProperty =
            DependencyProperty.Register("ClearButtonVisibility", typeof(Visibility), typeof(SearchBox), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public string TipText
        {
            get { return (string)GetValue(TipTextProperty); }
            set { SetValue(TipTextProperty, value); }
        }
        public static readonly DependencyProperty TipTextProperty =
            DependencyProperty.Register("TipText", typeof(string), typeof(SearchBox), new FrameworkPropertyMetadata("Search"));
        public Visibility TipTextVisibility
        {
            get { return (Visibility)GetValue(TipTextVisibilityProperty); }
            set { SetValue(TipTextVisibilityProperty, value); }
        }
        public static readonly DependencyProperty TipTextVisibilityProperty =
            DependencyProperty.Register("TipTextVisibility", typeof(Visibility), typeof(SearchBox), new FrameworkPropertyMetadata(Visibility.Visible));

        TextBox mInputBox;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var clearElement = Template.FindName("PART_Clear", this) as FrameworkElement;
            if(clearElement != null)
                clearElement.MouseLeftButtonDown += ClearElement_MouseLeftButtonDown;

            mInputBox = Template.FindName("PART_TextBox", this) as TextBox;
            if(mNeedFocus)
                mInputBox?.Focus();
            mNeedFocus = false;
        }

        void ClearElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SearchText = "";
            e.Handled = true;
        }

        bool mNeedFocus = false;
        public void FocusInput()
        {
            if (mInputBox != null)
            {
                Keyboard.ClearFocus();
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                {
                    Keyboard.Focus(mInputBox);
                }));
            }
            else
                mNeedFocus = true;
        }

        public static bool FilterTreeItems<T>(T srcParent, ObservableCollection<T> srcItems, ref ObservableCollection<T> tagItems, string filterString)
            where T : IFilterTreeItem<T>, new()
        {
            tagItems.Clear();
            bool retValue = false;
            foreach(IFilterTreeItem<T> item in srcItems)
            {
                T tagItem = default(T);
                if(string.IsNullOrEmpty(filterString))
                {
                    tagItem = new T();
                    item.CopyTo(tagItem);
                    tagItem.HighLightString = filterString;
                    tagItems.Add(tagItem);
                    tagItem.Parent = srcParent;
                    retValue = true;
                }
                else if(item.ContainsFilterString(filterString))
                {
                    tagItem = new T();
                    item.CopyTo(tagItem);
                    tagItem.HighLightString = filterString;
                    tagItems.Add(tagItem);
                    tagItem.Parent = srcParent;
                    retValue = true;
                }

                if(item.Children.Count > 0)
                {
                    var children = new ObservableCollection<T>();
                    var bFind = FilterTreeItems<T>((T)item, item.Children, ref children, filterString);
                    if(bFind)
                    {
                        if(tagItem == null)
                        {
                            tagItem = new T();
                            item.CopyTo(tagItem);
                            tagItem.HighLightString = filterString;
                            tagItems.Add(tagItem);
                        }
                        tagItem.Children = children;
                        if(!string.IsNullOrEmpty(filterString))
                            tagItem.IsExpanded = true;
                        retValue = true;
                    }
                }
            }

            return retValue;
        }
        public static bool FilterListItems<T>(ObservableCollection<T> srcItems, ref ObservableCollection<T> tagItems, string filterString)
                where T : IFilterTreeItem<T>, new()
        {
            tagItems.Clear();
            bool retValue = false;
            foreach (IFilterListItem<T> item in srcItems)
            {
                T tagItem = default(T);
                if (string.IsNullOrEmpty(filterString))
                {
                    tagItem = new T();
                    item.CopyTo(tagItem);
                    tagItem.HighLightString = filterString;
                    tagItems.Add(tagItem);
                    retValue = true;
                }
                else if (item.ContainsFilterString(filterString))
                {
                    tagItem = new T();
                    item.CopyTo(tagItem);
                    tagItem.HighLightString = filterString;
                    tagItems.Add(tagItem);
                    retValue = true;
                }
            }

            return retValue;
        }
    }

    public interface IFilterTreeItem<T>
    {
        bool IsExpanded { get; set; }
        string HighLightString { get; set; }
        T Parent { get; set; }
        ObservableCollection<T> Children { get; set; }
        void CopyTo(T item);
        bool ContainsFilterString(string filterString);
    }
    public interface IFilterListItem<T>
    {
        string HighLightString { get; set; }
        void CopyTo(T item);
        bool ContainsFilterString(string filterString);
    }
}
