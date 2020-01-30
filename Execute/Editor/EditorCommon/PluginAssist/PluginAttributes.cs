using System;
using System.Windows.Media;

namespace EditorCommon.PluginAssist
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginMenuItemAttribute : Attribute
    {
        public string[] MenuString
        {
            get;
            private set;
        }
        public string[] Icons
        {
            get;
            private set;
        }
        public int Count
        {
            get;
            set;
        } = 1;
        public EditorCommon.Menu.MenuItemDataBase.enMenuItemType MenuType;

        public PluginMenuItemAttribute(EditorCommon.Menu.MenuItemDataBase.enMenuItemType menuType, string[] menuString)
        {
            MenuType = menuType;
            MenuString = menuString;
        }
        public PluginMenuItemAttribute(EditorCommon.Menu.MenuItemDataBase.enMenuItemType menuType, string[] menuString, string[] icons)
        {
            MenuType = menuType;
            MenuString = menuString;
            Icons = icons;
        }
    }
}
