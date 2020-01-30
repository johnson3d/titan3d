using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EditorCommon.Controls.ResourceBrowser
{
    internal class FilterResourceItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        bool mIsChecked = true;
        public bool IsChecked
        {
            get { return mIsChecked; }
            set
            {
                mIsChecked = value;
                mBrowserControl.UpdateFilter();
                OnPropertyChanged("IsChecked");
            }
        }

        public ImageSource Icon
        {
            get
            {
                if (mInfoData != null)
                    return mInfoData.ResInfo.ResourceIcon;
                return null;
            }
        }

        public string ResourceTypeName
        {
            get
            {
                if (mInfoData != null)
                    return mInfoData.ResInfo.ResourceTypeName;
                return "";
            }
        }

        EditorCommon.Resources.ResourceInfoMetaData mInfoData;
        IContentControlHost mBrowserControl;

        public string ResourceType
        {
            get
            {
                if (mInfoData != null)
                    return mInfoData.ResourceInfoTypeStr;

                return "";
            }
        }

        public FilterResourceItem(EditorCommon.Resources.ResourceInfoMetaData info, IContentControlHost ctrl)
        {
            mInfoData = info;
            mBrowserControl = ctrl;
        }
    }
}
