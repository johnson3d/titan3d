using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodeGenerateSystem
{
    // 在菜单中显示(注意：父级菜单不能重名)
    public class ShowInMenu : System.Attribute
    {
        List<String> m_menuList = new List<String>();
        public List<String> MenuList
        {
            get { return m_menuList; }
        }

        string mDescription = "";
        public string Description
        {
            get { return mDescription; }
        }

        public ShowInMenu(string showNames, string description = "")
        {
            string[] menuNames = showNames.Split('.');
            m_menuList.AddRange(menuNames);
            mDescription = description;
        }
    }

    /// <summary>
    /// 节点显示在节点列表中
    /// </summary>
    public class ShowInNodeList : System.Attribute
    {
        //List<String> mPathList = new List<String>();
        //public List<String> PathList
        //{
        //    get { return mPathList; }
        //}
        public string Path
        {
            get;
            private set;
        }

        string mDescription = "";
        public string Description
        {
            get { return mDescription; }
        }

        public ImageSource Icon = null;

        /// <summary>
        /// 节点显示在节点列表中
        /// </summary>
        /// <param name="path">显示路径(根路径.子路径.名称)</param>
        /// <param name="description">说明</param>
        public ShowInNodeList(string path, string description)
        {
            //string[] menuNames = path.Split('.');
            //mPathList.AddRange(menuNames);
            Path = path;
            mDescription = description;
        }

        public enum enIconType
        {
            Node,
            Comment,
        }
        public ShowInNodeList(string path, string description, enIconType iconType)
        {
            Path = path;
            mDescription = description;
            switch(iconType)
            {
                case enIconType.Node:
                    Icon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Graph/icon_Blueprint_Node_16x.png", UriKind.Relative));
                    break;
                case enIconType.Comment:
                    Icon = new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_Blueprint_CommentBubbleOn_16x.png", UriKind.Relative));
                    break;
            }
        }
    }
}
