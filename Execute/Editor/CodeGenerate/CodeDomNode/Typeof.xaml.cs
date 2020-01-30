using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CodeDomNode
{
    public class TypeofTypeItem : DependencyObject
    {
        public Type TagType;

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(TypeofTypeItem), new FrameworkPropertyMetadata(null));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(TypeofTypeItem), new FrameworkPropertyMetadata(null));

        public Visibility Visible
        {
            get { return (Visibility)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(Visibility), typeof(TypeofTypeItem), new FrameworkPropertyMetadata(Visibility.Visible));
    }

    [CodeGenerateSystem.ShowInNodeList("类型/Typeof", "获取指定的类型")]
    public sealed partial class Typeof
    {
        ObservableCollection<TypeofTypeItem> mItems = new ObservableCollection<TypeofTypeItem>();
        ObservableCollection<TypeofTypeItem> mShowItems = new ObservableCollection<TypeofTypeItem>();

        string mFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                UpdateFilter(mFilterString);

                OnPropertyChanged("FilterString");
            }
        }
        void UpdateFilter(string filterStr)
        {
            mShowItems.Clear();
            if (string.IsNullOrEmpty(filterStr))
            {
                mShowItems = new ObservableCollection<TypeofTypeItem>(mItems);
            }
            else
            {
                var lowerFilter = filterStr.ToLower();
                foreach (var item in mItems)
                {
                    if (item.Name.ToString().ToLower().Contains(lowerFilter))
                    {
                        item.HighLightString = filterStr;
                        mShowItems.Add(item);
                    }
                    else
                    {
                        item.HighLightString = "";
                    }
                }
            }
            ListBox_Types.ItemsSource = mShowItems;
        }

        partial void InitConstruction()
        {
            this.InitializeComponent();
            mTypeValueLinkHandle = ValueLinkHandle;
            ListBox_Types.ItemsSource = mItems;
        }

        protected override void CollectionErrorMsg()
        {
            var csParam = CSParam as TypeofConstructionParams;
            if(csParam.ValueType == null)
            {
                HasError = true;
                ErrorDescription = "找不到类型" + csParam.ValueTypeFullName;
            }
        }

        private void ListBox_Types_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = ListBox_Types.SelectedItem as TypeofTypeItem;
            if (item == null)
                return;
            var param = CSParam as TypeofConstructionParams;
            param.ValueType = item.TagType;
            param.ValueTypeFullName = item.Name;
            NodeName = "typeof: " + item.Name;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            mItems.Clear();
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(sbyte)),
                TagType = typeof(sbyte),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(Int16)),
                TagType = typeof(Int16),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(Int32)),
                TagType = typeof(Int32),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(Int64)),
                TagType = typeof(Int64),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(byte)),
                TagType = typeof(byte),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(UInt16)),
                TagType = typeof(UInt16),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(UInt32)),
                TagType = typeof(UInt32),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(UInt64)),
                TagType = typeof(UInt64),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(Single)),
                TagType = typeof(Single),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(double)),
                TagType = typeof(double),
            });
            mItems.Add(new TypeofTypeItem()
            {
                Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(string)),
                TagType = typeof(string),
            });
            var types = EngineNS.Rtti.RttiHelper.GetTypes(CSParam.CSType);
            foreach (var type in types)
            {
                var item = new TypeofTypeItem()
                {
                    Name = EngineNS.Rtti.RttiHelper.GetAppTypeString(type),
                    TagType = type,
                };
                mItems.Add(item);
            }
            UpdateFilter(FilterString);

            SB_Type.Focus();
        }
    }
}
