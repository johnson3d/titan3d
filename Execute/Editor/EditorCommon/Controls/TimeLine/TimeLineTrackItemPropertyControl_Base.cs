using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace EditorCommon.Controls.TimeLine
{
    public class TimeLineTrackItemPropertyControl_Base : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        protected Object mPropertyInstance = null;
        public virtual Object PropertyInstance
        {
            get { return mPropertyInstance; }
            set
            {
                mPropertyInstance = value;

                OnPropertyChanged("PropertyInstance");
            }
        }

        protected TimeLineTrackItem mHostTimeLineTrackItem = null;
        public TimeLineTrackItem HostTimeLineTrackItem
        {
            get { return mHostTimeLineTrackItem; }
            set
            {
                mHostTimeLineTrackItem = value;

                OnPropertyChanged("HostTimeLineTrackItem");
            }
        }

        // 当前节点处于激活状态
        protected bool mIsActive = false;
        public virtual bool IsActive
        {
            get { return mIsActive; }
            set
            {
                mIsActive = value;

                OnPropertyChanged("IsActive");
            }
        }
    }
}
