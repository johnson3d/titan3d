using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// CollectionItem.xaml 的交互逻辑
    /// </summary>
    public partial class CollectionItem : UserControl
    {
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(CollectionItem), new UIPropertyMetadata(null));

        public object ValueObject
        {
            get { return GetValue(ValueObjectProperty); }
            private set { SetValue(ValueObjectProperty, value); }
        }
        public static readonly DependencyProperty ValueObjectProperty =
            DependencyProperty.Register("ValueObject", typeof(object), typeof(CollectionItem), new UIPropertyMetadata(null));

        CollectionEditorControl mHostCtrl;
        public CollectionItem(CollectionEditorControl hostCtrl, int idx, object val)
        {
            InitializeComponent();

            mHostCtrl = hostCtrl;
            Index = idx;
            ValueObject = val;
            PropertyGrid_Item.Instance = val;
        }

        private void Button_ItemInsert_Click(object sender, RoutedEventArgs e)
        {
            mHostCtrl?.ItemInsert(this);
        }
        private void Button_ItemRemove_Click(object sender, RoutedEventArgs e)
        {
            mHostCtrl?.ItemRemove(this);
        }
        private void Button_ItemClone_Click(object sender, RoutedEventArgs e)
        {
            mHostCtrl?.ItemClone(this);
        }
    }
}
