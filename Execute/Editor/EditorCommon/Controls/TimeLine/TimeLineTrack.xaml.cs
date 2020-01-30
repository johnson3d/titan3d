using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using EngineNS.Bricks.Animation;

namespace EditorCommon.Controls.TimeLine
{
    /// <summary>
    /// Interaction logic for TimeLineTrack.xaml
    /// </summary>
    public partial class TimeLineTrack : UserControl
    {
        public TimeLineTrackItem.Delegate_OnUpdateTimeLinkTrackItemActiveShow OnUpdateTimeLinkTrackItemActiveShow;
        public TimeLineTrackItem.Delegate_RemoveTimeLineTrackItem OnRemoveTimeLineTrackItem;
        public TimeLineTrackItem.Delegate_OnSelected OnTimeLineTrackItemSelected;

        List<TimeLineTrackItem> mTrackItems = new List<TimeLineTrackItem>();
        TimeLineControl mHostControl = null;
        public TimeLineControl HostControl
        {
            get { return mHostControl; }
        }

        TimeLineListItem mListItem = null;
        public TimeLineListItem ListItem
        {
            get { return mListItem; }
        }

        private static int CompareTimeLineTrackItem(TimeLineTrackItem item0, TimeLineTrackItem item1)
        {
            if (item0 == item1)
                return 0;

            if (item0 == null)
            {
                if (item1 == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (item1 == null)
                    return 1;
                else
                {
                    if (item0.FrameStart > item1.FrameStart)
                        return 1;
                    else if (item0.FrameStart < item1.FrameStart)
                        return -1;
                    else
                        return 0;
                }
            }
        }

        public TimeLineTrack(TimeLineControl hostControl, TimeLineListItem item)
        {
            InitializeComponent();

            mHostControl = hostControl;
            mListItem = item;

            foreach (var keyFrame in item.TLObject.GetKeyFrames())
            {
                AddTrackItem(keyFrame);
            }
        }

        public void UpdateTrackShow()
        {
            // 计算每个Item位置
            foreach (var item in mTrackItems)
            {
                if (item.CanModifyLength && item.IsModifyLength)
                {                    
                    var frames = item.FrameEnd - item.FrameStart;
                    if (frames <= 0)
                    {
                        frames = 1;
                        item.FrameEnd = item.FrameStart + 1;
                    }        
                    if (item.FrameEnd > mHostControl.TotalFrame)
                    {
                        item.FrameEnd = mHostControl.TotalFrame;
                        item.FrameStart = item.FrameEnd - 1;
                    }

                    Canvas.SetLeft(item, item.FrameStart * Canvas_Track.ActualWidth / mHostControl.TotalFrame);
                    item.Border_LengthKerframe.Width = frames * Canvas_Track.ActualWidth / mHostControl.TotalFrame;
                }
                else
                {
                    Canvas.SetLeft(item, item.FrameStart * Canvas_Track.ActualWidth / mHostControl.TotalFrame);
                    Canvas.SetRight(item, item.FrameEnd * Canvas_Track.ActualWidth / mHostControl.TotalFrame);
                }                
            }
        }

        public void AddTrackItem(TimeLineKeyFrameObjectInterface keyFrameItem)
        {
            var item = new TimeLineTrackItem(this, keyFrameItem);
            BindingOperations.SetBinding(item, UserControl.HeightProperty, new Binding("ActualHeight") { Source = this });
            item.OnRemoveTimeLineTrackItem = new TimeLineTrackItem.Delegate_RemoveTimeLineTrackItem(_OnRemoveTimeLineTrackItem);
            item.OnUpdateTimeLineTrackItemActiveShow = new TimeLineTrackItem.Delegate_OnUpdateTimeLinkTrackItemActiveShow(_OnUpdateTimeLinkTrackItemActiveShow);
            item.OnSelected = new TimeLineTrackItem.Delegate_OnSelected(_OnTimeLineTrackItemSelected);           
            mTrackItems.Add(item);
            Canvas_Track.Children.Add(item);
            
            mTrackItems.Sort(CompareTimeLineTrackItem);

            UpdateTrackShow();

            mHostControl?.OnCreateTimeLineTrackItem?.Invoke(item);
        }

        private void _OnUpdateTimeLinkTrackItemActiveShow(TimeLineTrackItem item)
        {
            if (OnUpdateTimeLinkTrackItemActiveShow != null)
                OnUpdateTimeLinkTrackItemActiveShow(item);
        }

        private void _OnRemoveTimeLineTrackItem(TimeLineTrackItem item)
        {
            Canvas_Track.Children.Remove(item);
            mTrackItems.Remove(item);

            if (OnRemoveTimeLineTrackItem != null)
                OnRemoveTimeLineTrackItem(item);
        }

        private void _OnTimeLineTrackItemSelected(TimeLineTrackItem item)
        {
            if (OnTimeLineTrackItemSelected != null)
                OnTimeLineTrackItemSelected(item);
        }

        public void RemoveTrackItem(Int64 currertFrameTime)
        {
            TimeLineTrackItem trackItem = null;

            for (int i = 0; i < mTrackItems.Count; ++i)
            {
                if (mTrackItems[i].FrameStart == currertFrameTime)
                {
                    trackItem = mTrackItems[i];
                    break;
                }
            }

            Canvas_Track.Children.Remove(trackItem);
            mTrackItems.Remove(trackItem);
        }

        private void Canvas_Track_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateTrackShow();
        }

        // 更新每个TrackItem的激活状态
        public void UpdateTrackItemActive(Int64 currentFrame)
        {
            foreach (var item in mTrackItems)
            {
                item.UpdateTrackActive(currentFrame);
            }
        }

        //  移动后的帧起始时间是否合法,合法条件:移动后的帧开始时间
        //  不在前面帧之前或与之一致,不在后面帧之后或与之一致
        //  此方法逻辑严谨性依赖于不能在同一时间重复添加关键帧
        public bool MovedFrameStartIsLegitimate(Int64 originalStartTime, Int64 frameDelta)
        {
            int keyFrameIndex = 0;

            for (int i = 0; i < mTrackItems.Count; ++i)
            {
                if (mTrackItems[i].FrameStart == originalStartTime)
                {
                    keyFrameIndex = i;
                    break;
                }
            }

            if (keyFrameIndex > 0)
            {
                TimeLineTrackItem previousKeyFrameItem = mTrackItems[keyFrameIndex - 1];

                if (originalStartTime + frameDelta <= previousKeyFrameItem.FrameStart)
                    return false;
            }

            if (keyFrameIndex < mTrackItems.Count - 1)
            {
                TimeLineTrackItem nextKeyFrameItem = mTrackItems[keyFrameIndex + 1];

                if (originalStartTime + frameDelta >= nextKeyFrameItem.FrameStart)
                    return false;
            }

            return true;
        }

        public void UpdateKeyFrameToolTip(TimeLineKeyFrameObjectInterface keyFrameObj)
        {
            var trackItem = mTrackItems.FirstOrDefault(f => f.KeyFrameItem == keyFrameObj);
            if (trackItem != null)
                trackItem.ToolTip = keyFrameObj.UpdateToolTip();
        }
    }
}
