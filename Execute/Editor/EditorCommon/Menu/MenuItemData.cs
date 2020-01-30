using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace EditorCommon.Menu
{
    public class MenuItemDataBase
    {
        public enum enMenuItemType
        {
            Checkable,
            OneClick,
            Radio,
        }

        public string KeyName
        {
            get;
            private set;
        } = "";
        // 使用|来设置Category，如：category|menuName
        public string[] MenuNames = new string[0];
        public int Count = 1;
        public ImageSource[] Icons = new ImageSource[0];
        public List<MenuItem[]> MenuItems = new List<MenuItem[]>();
        public DockControl.IDockAbleControl[] OperationControls;
        public Type OperationControlType;

        public enMenuItemType MenuItemType
        {
            get;
            protected set;
        } = enMenuItemType.OneClick;

        public MenuItemDataBase(string key)
        {
            KeyName = key;
        }

        public override bool Equals(object obj)
        {
            var tag = obj as MenuItemDataBase;
            if (tag == null)
                return false;
            if((tag.MenuNames.Length == MenuNames.Length) &&
               (tag.Count == Count) &&
               (tag.MenuItemType == MenuItemType))
            {
                var menuNamesLen = tag.MenuNames.Length;
                for(int i=0; i<menuNamesLen; i++)
                {
                    if (tag.MenuNames[i] != MenuNames[i])
                        return false;
                }
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static MenuItemDataBase CreateMenuData(enMenuItemType menuType, string keyName)
        {
            switch(menuType)
            {
                case enMenuItemType.Checkable:
                    {
                        var retVal = new MenuItemData_ShowHideControl(keyName);
                        return retVal;
                    }
                case enMenuItemType.OneClick:
                    {
                        var retVal = new MenuItemDataBase(keyName);
                        retVal.MenuItemType = menuType;
                        return retVal;
                    }
                case enMenuItemType.Radio:
                    {
                        var retVal = new MenuItemDataBase(keyName);
                        retVal.MenuItemType = menuType;
                        return retVal;
                    }
                default:
                    return null;
            }
        }
    }

    public class MenuItemData_CheckAble : MenuItemDataBase
    {
        public System.Reflection.PropertyInfo ProInfo;
        public MenuItemData_CheckAble(string keyName)
            : base(keyName)
        {
            MenuItemType = enMenuItemType.Checkable;
        }
    }

    public class MenuItemData_Function : MenuItemDataBase
    {
        public System.Reflection.MethodInfo Method;
        public MenuItemData_Function(string keyName)
            : base(keyName)
        {
            MenuItemType = enMenuItemType.OneClick;
        }

        public void ClickOperation(object sender, System.Windows.RoutedEventArgs e)
        {
            if(Method.ReturnType == typeof(void))
                Method?.Invoke(null, null);
            else
            {
                var retVal = Method?.Invoke(null, null);
                string str = System.Convert.ToString(retVal);
                if(!string.IsNullOrEmpty(str))
                {
                    var item = sender as MenuItem;
                    if (item != null)
                        item.Header = str;
                }
            }
        }
    }

    public class MenuItemData_ShowHideControl : MenuItemDataBase
    {
        public MenuItemData_ShowHideControl(string keyName)
            : base(keyName)
        {
            MenuItemType = enMenuItemType.Checkable;
        }

        public void BindOperationControl(int idx, DockControl.IDockAbleControl ctrl)
        {
            if (idx >= Count)
                return;
            if (MenuItems == null)
                return;

            if(OperationControls == null || OperationControls.Length != Count)
                OperationControls = new DockControl.IDockAbleControl[Count];

            OperationControls[idx] = ctrl;
            foreach(var menus in MenuItems)
            {
                if(idx < menus.Length)
                    menus[idx].SetBinding(MenuItem.IsCheckedProperty, new Binding("IsShowing") { Source = OperationControls[idx], Mode = BindingMode.OneWay });
            }
        }
    }
}
