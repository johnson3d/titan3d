using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using WPG.Themes.TypeEditors;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for EnumValue.xaml
    /// </summary>
    public partial class EnumValue
    {
        partial void OnChangedValue(Object value, Int64[] selects);
        partial void InitConstruction()
        {
            InitializeComponent();

            var param = CSParam as EnumConstructParam;
            ToolTip = param.EnumType.FullName;
            
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
        partial void SetComboItemsSource(System.Collections.IEnumerable items)
        {
            Combo_Keys.ItemsSource  = items;
        }

        partial void SetEnumFlags(bool flag)
        {
            Combo_Keys.Visibility = flag ? Visibility.Collapsed : Visibility.Visible;
            flagsenumeditor.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            if (flag)
            {
                flagsenumeditor.OnChangedValue -= OnChangedValue;
                flagsenumeditor.OnChangedValue += OnChangedValue;
            }

        }

        partial void SetFlagEnumObject(Object obj)
        {
            flagsenumeditor.FlagEnumObject = obj;
        }

    }
}
