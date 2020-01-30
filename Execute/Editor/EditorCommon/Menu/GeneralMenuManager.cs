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
    public class GeneralMenuManager : EngineNS.Editor.IEditorInstanceObject
    {
        public static GeneralMenuManager Instance
        {
            get
            {
                var name = typeof(GeneralMenuManager).FullName;
                if (EngineNS.CEngine.Instance.GameEditorInstance[name] == null)
                    EngineNS.CEngine.Instance.GameEditorInstance[name] = new GeneralMenuManager();
                return EngineNS.CEngine.Instance.GameEditorInstance[name];
            }
        }
        public void FinalCleanup()
        {

        }

        Dictionary<string, MenuItemDataBase> mMenuItemDatas = new Dictionary<string, MenuItemDataBase>();

        public void RegisterMenuItem(MenuItemDataBase menuData)
        {
            mMenuItemDatas.Add(menuData.KeyName, menuData);
        }
        public MenuItemDataBase GetMenuData(string key)
        {
            MenuItemDataBase retVal = null;
            mMenuItemDatas.TryGetValue(key, out retVal);
            return retVal;
        }

        static void GetMenuNameAndCategory(string menuFullName, out string menuName, out string category)
        {
            var splits = menuFullName.Split('|');
            if (splits.Length > 1)
            {
                category = splits[0];
                menuName = splits[1];
            }
            else
            {
                category = "";
                menuName = menuFullName;
            }
        }

        static MenuItem[] CreateMenuItem(int deep, MenuItemDataBase menuData, ItemCollection menuItems)
        {
            if (menuData.MenuNames.Length == deep)
                return null;

            var menuFullName = menuData.MenuNames[deep];
            string menuName;
            string category;
            GetMenuNameAndCategory(menuFullName, out menuName, out category);

            foreach (var item in menuItems)
            {
                var menuItem = item as MenuItem;
                if (menuItem == null)
                    continue;
                if(menuItem.Header.ToString() == menuName)
                {
                    if(menuData.MenuNames.Length == (deep + 1))
                    {
                        // 菜单重名
                        break;
                    }
                    else
                        return CreateMenuItem(deep + 1, menuData, menuItem.Items);
                }
            }

            int lastCategoryIdx = -1;
            int tempIdx = 0;
            foreach(var item in menuItems)
            {
                var menuItem = item as MenuItem;
                if (menuItem == null)
                {
                    tempIdx++;
                    continue;
                }
                var data = menuItem.Tag as MenuItemDataBase;
                if (data == null || data.MenuNames.Length <= deep)
                {
                    tempIdx++;
                    continue;
                }
                var menuFName = data.MenuNames[deep];
                string tempMName, tempCategory;
                GetMenuNameAndCategory(menuFName, out tempMName, out tempCategory);
                if(string.Equals(category, tempCategory))
                {
                    lastCategoryIdx = tempIdx;
                    break;
                }

                tempIdx++;
            }
            lastCategoryIdx++;

            ImageSource icon = null;
            var iconArrayLen = menuData.Icons.Length;
            if(iconArrayLen > 0)
            {
                if (deep >= iconArrayLen)
                    icon = menuData.Icons[iconArrayLen - 1];
                else
                    icon = menuData.Icons[deep];
            }

            if (menuData.MenuNames.Length > (deep + 1))
            {
                var resultMenuItem = new MenuItem()
                {
                    Name = "GeneralMenu_",// + menuName,
                    Header = menuName,
                    Tag = menuData,
                };
                ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(resultMenuItem, icon);
                resultMenuItem.Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
                if(lastCategoryIdx == 0 && !string.IsNullOrEmpty(category))
                {
                    menuItems.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
                    {
                        Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                        Text = category,
                    });
                    menuItems.Add(resultMenuItem);
                }
                else if (lastCategoryIdx != 0 && lastCategoryIdx < menuItems.Count)
                    menuItems.Insert(lastCategoryIdx, resultMenuItem);
                else
                    menuItems.Add(resultMenuItem);
                return CreateMenuItem(deep + 1, menuData, resultMenuItem.Items);
            }
            else
            {
                var retVal = new MenuItem[menuData.Count];
                for(int i=0; i<menuData.Count; i++)
                {
                    var resultMenuItem = new MenuItem()
                    {
                        Header = menuName + ((menuData.Count > 1)? (i+1).ToString() : ""),
                        Tag = menuData,
                    };
                    resultMenuItem.Name = "GenericMenu_";// + resultMenuItem.Header;
                    ResourceLibrary.Controls.Menu.MenuAssist.SetIcon(resultMenuItem, icon);
                    resultMenuItem.Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as System.Windows.Style;
                    if(lastCategoryIdx == 0 && !string.IsNullOrEmpty(category))
                    {
                        menuItems.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
                        {
                            Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                            Text = category,
                        });
                        menuItems.Add(resultMenuItem);
                    }
                    else if (lastCategoryIdx != 0 && lastCategoryIdx < menuItems.Count)
                        menuItems.Insert(lastCategoryIdx++, resultMenuItem);
                    else
                        menuItems.Add(resultMenuItem);
                    retVal[i] = resultMenuItem;
                }
                return retVal;
            }
        }
        public void GenerateGeneralMenuItems(System.Windows.Controls.Menu menuRoot)
        {
            GenerateMenuItems(menuRoot, mMenuItemDatas);
        }
        public static void ShowOperationControl(DockControl.IDockAbleControl ctrl)
        {
            if (ctrl.IsShowing)
            {
                // 已经显示的放到前台
                var parentTabItem = EditorCommon.Program.GetParent((FrameworkElement)(ctrl), typeof(TabItem)) as TabItem;
                var parentTabControl = EditorCommon.Program.GetParent((FrameworkElement)(ctrl), typeof(TabControl)) as TabControl;
                if (parentTabControl != null && parentTabItem != null)
                {
                    parentTabControl.SelectedItem = parentTabItem;
                }

                // 将包含该控件的窗体显示到最前
                var parentWin = EditorCommon.Program.GetParent((FrameworkElement)(ctrl), typeof(Window)) as Window;
                ResourceLibrary.Win32.BringWindowToTop(new System.Windows.Interop.WindowInteropHelper(parentWin).Handle);
            }
            else
            {
                // 判断是不是已经加载了
                var dockWin = new DockControl.DockAbleWindow();
                dockWin.SetContent(ctrl);
                dockWin.Show();
                ctrl.IsShowing = true;
            }
        }
        public static void GenerateMenuItems(System.Windows.Controls.Menu menuRoot, Dictionary<string, MenuItemDataBase> menuItemDatas)
        {
            foreach(var dataVal in menuItemDatas)
            {
                var data = dataVal.Value;
                var items = CreateMenuItem(0, data, menuRoot.Items);
                data.MenuItems.Add(items);
                switch (data.MenuItemType)
                {
                    case MenuItemDataBase.enMenuItemType.Checkable:
                        {
                            var menuData = data as MenuItemDataBase;// MenuItemData_ShowHideControl;
                            if(menuData is MenuItemData_CheckAble)
                            {
                                var cData = data as MenuItemData_CheckAble;
                                var checkVal = (bool)(cData.ProInfo.GetValue(null));
                                for(int i=0; i<items.Length; i++)
                                {
                                    var item = items[i];
                                    item.IsCheckable = true;
                                    item.IsChecked = checkVal;
                                    int tempVal = i;
                                    item.Click += (object sender, System.Windows.RoutedEventArgs e) =>
                                    {
                                        var newVal = item.IsChecked;
                                        cData.ProInfo.SetValue(null, newVal);
                                        foreach(var ii in items)
                                        {
                                            ii.IsChecked = newVal;
                                        }
                                    };
                                }
                            }
                            else
                            {
                                for (int i = 0; i < items.Length; i++)
                                {
                                    var item = items[i];
                                    item.IsCheckable = true;
                                    int tempVal = i;
                                    if (menuData.OperationControls != null)
                                    {
                                        for (int ctrlIdx = 0; ctrlIdx < menuData.OperationControls.Length; ctrlIdx++)
                                        {
                                            var ctrl = menuData.OperationControls[ctrlIdx];
                                            if (ctrl == null)
                                                continue;
                                            foreach (var menus in data.MenuItems)
                                            {
                                                if (ctrlIdx < menus.Length)
                                                    menus[ctrlIdx].SetBinding(MenuItem.IsCheckedProperty, new Binding("IsShowing") { Source = ctrl, Mode = BindingMode.OneWay });
                                            }
                                        }
                                    }
                                    item.Click += (object sender, System.Windows.RoutedEventArgs e) =>
                                    {
                                        if (menuData.OperationControls == null || menuData.OperationControls.Length != menuData.Count)
                                        {
                                            menuData.OperationControls = new DockControl.IDockAbleControl[menuData.Count];
                                        }
                                        if (menuData.OperationControls[tempVal] == null)
                                        {
                                            menuData.OperationControls[tempVal] = System.Activator.CreateInstance(menuData.OperationControlType) as DockControl.IDockAbleControl;
                                            if (items.Length > 1)
                                                menuData.OperationControls[tempVal].Index = tempVal;
                                            foreach (var menus in data.MenuItems)
                                            {
                                                if (tempVal < menus.Length)
                                                    menus[tempVal].SetBinding(MenuItem.IsCheckedProperty, new Binding("IsShowing") { Source = menuData.OperationControls[tempVal], Mode = BindingMode.OneWay });
                                            }
                                        }
                                        var ctrl = menuData.OperationControls[tempVal];
                                        ShowOperationControl(ctrl);
                                    };
                                }
                            }
                        }
                        break;
                    case MenuItemDataBase.enMenuItemType.OneClick:
                        {
                            var menuData = data as MenuItemDataBase;
                            if(menuData is MenuItemData_Function)
                            {
                                var funcData = menuData as MenuItemData_Function;
                                for(int i=0; i<items.Length; i++)
                                {
                                    var item = items[i];
                                    item.Click += funcData.ClickOperation;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < items.Length; i++)
                                {
                                    var item = items[i];
                                    int tempVal = i;
                                    item.Click += (object sender, System.Windows.RoutedEventArgs e) =>
                                    {
                                        if (menuData.OperationControls == null || menuData.OperationControls.Length != menuData.Count)
                                        {
                                            menuData.OperationControls = new DockControl.IDockAbleControl[menuData.Count];
                                        }
                                        if (menuData.OperationControls[tempVal] == null)
                                        {
                                            menuData.OperationControls[tempVal] = System.Activator.CreateInstance(menuData.OperationControlType) as DockControl.IDockAbleControl;
                                            if (items.Length > 1)
                                                menuData.OperationControls[tempVal].Index = tempVal;
                                        }
                                        var ctrl = menuData.OperationControls[tempVal];
                                        ShowOperationControl(ctrl);
                                    };
                                }
                            }
                        }
                        break;
                    case MenuItemDataBase.enMenuItemType.Radio:
                        throw new InvalidOperationException();
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

    }
}
