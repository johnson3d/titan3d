using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WPG.Data;

namespace WPG.Themes.TypeEditors
{
    public partial class CollectionEditorControl : UserControl
    {
        public Property MyProperty
        {
            get { return (Property)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(Property), typeof(CollectionEditorControl), new UIPropertyMetadata(null));

        
        public Type NumerableType
        {
            get { return (Type)GetValue(NumerableTypeProperty); }
            set { SetValue(NumerableTypeProperty, value); }
        }

        public static readonly DependencyProperty NumerableTypeProperty =
            DependencyProperty.Register("NumerableType", typeof(Type), typeof(CollectionEditorControl), new UIPropertyMetadata(null));

        public IEnumerable NumerableValue
        {
            get { return (IEnumerable)GetValue(NumerableValueProperty); }
            set { SetValue(NumerableValueProperty, value); }
        }
       
        public static readonly DependencyProperty NumerableValueProperty =
            DependencyProperty.Register("NumerableValue", typeof(IEnumerable), typeof(CollectionEditorControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnNumerableValuetChanged)));
        public static void OnNumerableValuetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CollectionEditorControl;
            if (ctrl == null)
                return;

            var newValue = e.NewValue as IEnumerable;
            //ctrl.mTriggerCollectionChange = false;
            ctrl.ListBox_Items.Items.Clear();
            if (newValue != null)
            {
                ctrl.ListBox_Items.ItemsSource = null;
                int i = 0;
                foreach (var item in newValue)
                {
                    var vi = new CollectionItem(ctrl, i, item);
                    ctrl.ListBox_Items.Items.Add(vi);
                    i++;
                }
            }
            //ctrl.mTriggerCollectionChange = true;
        }

        public CollectionEditorControl()
        {
            InitializeComponent();
            //txtTypeName.Text = NumerableType.GetType().ToString();
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    CollectionEditorWindow collEdt = new CollectionEditorWindow(this);
        //    if (collEdt.ShowDialog() == true)
        //    {
        //        //var newValue = NumerableType.Assembly.CreateInstance(NumerableType.FullName) as IEnumerable;

        //        var mClear = NumerableType.GetMethod("Clear");
        //        if (mClear != null)
        //            mClear.Invoke(NumerableValue, null);

        //        var mi = NumerableType.GetMethod("Add");
        //        if(mi == null)
        //            return;

        //        foreach (ValueInfo item in collEdt.ListBox_Values.Items)
        //        {
        //            mi.Invoke(NumerableValue, new object[] { item.ValueObject });
        //        }

        //        //NumerableValue = newValue;

        //        BindingExpression be = this.GetBindingExpression(NumerableValueProperty);
        //        be.UpdateSource();
        //    }
        //}

        private void Button_AddItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = NumerableType.GetMethod("Add");
            if (mi == null)
                return;

            Type providerType = null;
            foreach(var att in MyProperty.ValueAttributes)
            {
                if(att is EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute)
                {
                    providerType = ((EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute)att).ProviderType;
                }
            }
            if(providerType != null)
            {
                var provider = System.Activator.CreateInstance(providerType) as EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute.AddRemoveActionProviderBase;
                var objs = provider.Add();
                if (objs != null)
                {
                    foreach (var obj in objs)
                    {
                        var vi = new CollectionItem(this, ListBox_Items.Items.Count, obj);
                        ListBox_Items.Items.Add(vi);

                        mi.Invoke(NumerableValue, new object[] { obj });
                        //var be = this.GetBindingExpression(NumerableValueProperty);
                    }
                } 
            }
            else
            {
                var argType = MyProperty.PropertyType.GetGenericArguments()[0];
                object newElem = null;
                if (argType == typeof(System.String))
                    newElem = System.String.Empty;
                else
                    newElem = System.Activator.CreateInstance(argType);
                var vi = new CollectionItem(this, ListBox_Items.Items.Count, newElem);
                ListBox_Items.Items.Add(vi);

                mi.Invoke(NumerableValue, new object[] { newElem });
            }

            var be = this.GetBindingExpression(NumerableValueProperty);
            be.UpdateSource();
        }
        private void Button_ClearItems_Click(object sender, RoutedEventArgs e)
        {
            // 即将清除所有对象，是否继续
            if (EditorCommon.MessageBox.Show("即将清除所有对象，是否继续", EditorCommon.MessageBox.enMessageBoxButton.YesNo) != EditorCommon.MessageBox.enMessageBoxResult.Yes)
                return;

            var mClear = NumerableType.GetMethod("Clear");
            if (mClear != null)
            {
                mClear.Invoke(NumerableValue, null);
                ListBox_Items.Items.Clear();

                BindingExpression be = this.GetBindingExpression(NumerableValueProperty);
                be.UpdateSource();
            }
        }

        public void ItemInsert(CollectionItem item)
        {
            var mi = NumerableType.GetMethod("Insert");
            if (mi == null)
                return;

            var insertIdx = item.Index;

            Type providerType = null;
            foreach (var att in MyProperty.ValueAttributes)
            {
                if (att is EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute)
                {
                    providerType = ((EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute)att).ProviderType;
                }
            }
            if (providerType != null)
            {
                var provider = System.Activator.CreateInstance(providerType) as EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute.AddRemoveActionProviderBase;
                var objs = provider.Insert();
                if (objs != null)
                {
                    int oldindex = insertIdx;
                    foreach (var obj in objs)
                    {
                        var vi = new CollectionItem(this, insertIdx, obj);
                        ListBox_Items.Items.Insert(insertIdx, vi);

                        mi.Invoke(NumerableValue, new object[] { insertIdx, obj });
                        insertIdx++;//每次加入向后挪一位
                                    //var be = this.GetBindingExpression(NumerableValueProperty);
                    }

                    for (int i = insertIdx; i < ListBox_Items.Items.Count; i++)
                    {
                        ((CollectionItem)(ListBox_Items.Items[i])).Index += insertIdx - oldindex;
                    }
                }
            }
            else
            {
                var argType = MyProperty.PropertyType.GetGenericArguments()[0];
                object newElem = null;
                if (argType == typeof(System.String))
                    newElem = System.String.Empty;
                else
                    newElem = System.Activator.CreateInstance(argType);

                var vi = new CollectionItem(this, insertIdx, newElem);
                ListBox_Items.Items.Insert(insertIdx, vi);

                for (int i = insertIdx + 1; i < ListBox_Items.Items.Count; i++)
                {
                    ((CollectionItem)(ListBox_Items.Items[i])).Index++;
                }

                mi.Invoke(NumerableValue, new object[] { insertIdx, newElem });
            }
            
            var be = this.GetBindingExpression(NumerableValueProperty);
            be.UpdateSource();
        }

        public void ItemRemove(CollectionItem item)
        {
            var mi = NumerableType.GetMethod("RemoveAt");
            if (mi == null)
                return;

            var idx = item.Index;

            if (idx > 0)
            {
                bool needremove = true;
                Type providerType = null;
                foreach (var att in MyProperty.ValueAttributes)
                {
                    if (att is EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute)
                    {
                        providerType = ((EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute)att).ProviderType;
                    }
                }
                if (providerType != null)
                {
                    var provider = System.Activator.CreateInstance(providerType) as EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute.AddRemoveActionProviderBase;
                    CollectionItem citem = ListBox_Items.Items[idx] as CollectionItem;
                    needremove = provider.Remove(citem.ValueObject);
                   
                }
                if (needremove)
                {
                    ListBox_Items.Items.RemoveAt(idx);
                    for (int i = idx; i < ListBox_Items.Items.Count; i++)
                    {
                        ((CollectionItem)(ListBox_Items.Items[i])).Index--;
                    }

                    mi.Invoke(NumerableValue, new object[] { idx });
                    var be = this.GetBindingExpression(NumerableValueProperty);
                    be.UpdateSource();
                }
            }
        }
        public void ItemClone(CollectionItem item)
        {
            object newObj = null;
            var argType = MyProperty.PropertyType.GetGenericArguments()[0];
            if (argType == typeof(System.String))
            {
                newObj = (System.String)(item.ValueObject);
            }
            else if (argType.IsValueType)
            {
                newObj = item.ValueObject;
            }
            else
            {
                var mi = argType.GetMethod("CloneObject");
                if (mi == null)
                    return;

                newObj = mi.Invoke(item.ValueObject, null);
            }

            var miInsert = NumerableType.GetMethod("Insert");
            if (miInsert == null)
                return;

            var miAdd = NumerableType.GetMethod("Add");
            if (miAdd == null)
                return;

            var idx = item.Index;
            var newVi = new CollectionItem(this, idx + 1, newObj);

            if (idx == ListBox_Items.Items.Count - 1)
            {
                ListBox_Items.Items.Add(newVi);
                miAdd.Invoke(NumerableValue, new object[] { newObj });
            }
            else
            {
                ListBox_Items.Items.Insert(idx, newVi);
                miInsert.Invoke(NumerableValue, new object[] { idx, newObj });
            }

            var be = this.GetBindingExpression(NumerableValueProperty);
            be.UpdateSource();

        }
    }
}
