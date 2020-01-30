using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DockControl;
using EditorCommon.Controls.Animation;
using EditorCommon.ResourceInfos;
using EditorCommon.Resources;
using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Notify;
using EngineNS.Bricks.Animation.Skeleton;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics.Mesh;
using EngineNS.IO;

namespace EditorCommon.Controls.Animation
{
    public class EditorAnimationClip
    {
        public AnimationClip AnimationClip { get; set; } = null;
        public Dictionary<Guid, EngineNS.Macross.MacrossGetter<CGfxNotify>> NofityPairs { get; set; } = new Dictionary<Guid, EngineNS.Macross.MacrossGetter<CGfxNotify>>();
        public void AddInstanceNotify(CGfxNotify notify, EngineNS.Macross.MacrossGetter<CGfxNotify> macrossGetter)
        {
            AnimationClip?.ClipInstance.Notifies.Add(notify);
            if (NofityPairs.ContainsKey(notify.ID))
            {
                NofityPairs[notify.ID] = macrossGetter;
            }
            else
                NofityPairs.Add(notify.ID, macrossGetter);
        }
        public void RemoveInstanceNotify(CGfxNotify notify)
        {
            AnimationClip?.ClipInstance.Notifies.Remove(notify);
            NofityPairs.Remove(notify.ID);
        }
        public void Save()
        {
            for (int i = 0; i < AnimationClip.InstanceNotifies.Count; ++i)
            {
                if (NofityPairs.ContainsKey(AnimationClip.InstanceNotifies[i].ID))
                {
                    var gettter = NofityPairs[AnimationClip.InstanceNotifies[i].ID];
                    if (gettter == null)
                    {
                        //AnimationClip.ClipInstance.Notifies[i] = AnimationClip.InstanceNotifies[i];
                    }
                    else
                    {
                        AnimationClip.ClipInstance.Notifies[i] = gettter.Get(false);
                    }
                }
            }
            AnimationClip.Save();
        }
    }
    /// <summary>
    /// Interaction logic for AnimationAssetEditeControl.xaml
    /// </summary>
    public partial class AnimationAssetEditeControl : UserControl, INotifyPropertyChanged, EngineNS.ITickInfo
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region TickInfo
        public WPG.PropertyGrid ProGrid { get; set; }
        public bool EnableTick { get; set; }
        public EngineNS.Profiler.TimeScope GetLogicTimeScope()
        {
            return null;
        }
        public void BeforeFrame()
        {

        }

        public void TickLogic()
        {
            AnimationPlayingCtrl.TickLogic();
            var playPercent = AnimationPlayingCtrl.PlayPercent;
            var time = mEditorAnimationClip.AnimationClip.Duration * playPercent;
            foreach (var notify in mNotifyEditeControlList)
            {
                notify.Value = AnimationPlayingCtrl.CurrentFrame;
                notify.TickLogic();
            }
            mEditorAnimationClip.AnimationClip.SeekForEditor(time);
        }
        public void TickRender()
        {

        }
        async System.Threading.Tasks.Task DirtyProcess(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        async System.Threading.Tasks.Task DirtyProcessAsync(bool force = false, bool needGenerateCode = true)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        //bool mNeedDirtyProcess = false;
        public void TickSync()
        {
            var noUse = DirtyProcess();
        }
        #endregion
        public AnimationAssetEditeControl()
        {
            InitializeComponent();
        }
        EditorAnimationClip mEditorAnimationClip = null;
        AnimationClipResourceInfo mCurrentResourceInfo = null;
        CGfxSkeleton mSkeletonAsset = null;
        public void SetObject(EditorAnimationClip animSequence, AnimationClipResourceInfo resourceInfo)
        {
            mEditorAnimationClip = animSequence;
            mCurrentResourceInfo = resourceInfo;
            mSkeletonAsset = EngineNS.CEngine.Instance.SkeletonAssetManager.GetSkeleton(EngineNS.CEngine.Instance.RenderContext, RName.GetRName(mCurrentResourceInfo.SkeletonAsset));
            AnimationPlayingCtrl.AnimationName = mEditorAnimationClip.AnimationClip.Name.PureName();
            AnimationPlayingCtrl.TotalFrame = mEditorAnimationClip.AnimationClip.KeyFrames;
            AnimationPlayingCtrl.Duration = mEditorAnimationClip.AnimationClip.Duration;
            AnimationPlayingCtrl.OnTickBarScaling += AnimationPlayingCtrl_OnTickBarScaling;
            if (mCurrentResourceInfo.NotifyTrackMap != null && mCurrentResourceInfo.NotifyTrackMap.Count != 0)
            {
                EngineNS.Thread.ASyncSemaphore smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(mCurrentResourceInfo.TrackCount);
                for (int i = 0; i < mCurrentResourceInfo.TrackCount; ++i)
                {
                    var notify = CreateNotifyTrack(i);
                    AddNotifyTrack(i, notify, false);
                    notify.OnLoaded += (sender, e) =>
                    {
                        smp.Release();
                    };
                }
                Action action = async () =>
                {
                    await smp.Await();
                    foreach (var notify in mEditorAnimationClip.AnimationClip.InstanceNotifies)
                    {
                        var trackId = GetTrackId(notify.ID);
                        if (trackId >= 0)
                        {
                            var getter = mNotifyEditeControlList[trackId].AddNotify(notify.NotifyName, notify);
                            mEditorAnimationClip.NofityPairs.Add(notify.ID, getter);
                        }
                    }
                    foreach (var pair in mCurrentResourceInfo.NotifyTrackMap)
                    {
                        var trackId = GetTrackId(pair.NotifyID);
                        if (trackId >= 0)
                        {

                        }
                    }
                };
                action.Invoke();
            }
            else
            {
                AddNotifyTrack(0, CreateNotifyTrack(0));
            }
        }
        #region NotifyTrackMap
        void AddToNotifyTrackMap(Guid notifyId, int trackNum, string notifyName)
        {
            var notifyTrackPair = mCurrentResourceInfo.NotifyTrackMap.Find((pair) =>
            {
                if (pair.NotifyID == notifyId)
                    return true;
                else
                    return false;
            });
            if (notifyTrackPair == null)
            {
                notifyTrackPair = new NotifyTrack();
                notifyTrackPair.NotifyID = notifyId;
                notifyTrackPair.TrackID = trackNum;
                notifyTrackPair.NotifyName = notifyName;
                mCurrentResourceInfo.NotifyTrackMap.Add(notifyTrackPair);
            }
            else
            {
                notifyTrackPair.TrackID = trackNum;
            }
        }
        void RefreshNotifyTrackMap(Guid notifyId, int trackNum)
        {
            var notifyTrackPair = mCurrentResourceInfo.NotifyTrackMap.Find((pair) =>
            {
                if (pair.NotifyID == notifyId)
                    return true;
                else
                    return false;
            });
            if (notifyTrackPair != null)
            {
                notifyTrackPair.TrackID = trackNum;
            }
        }
        int GetTrackId(Guid notifyId)
        {
            var notifyTrackPair = mCurrentResourceInfo.NotifyTrackMap.Find((pair) =>
            {
                if (pair.NotifyID == notifyId)
                    return true;
                else
                    return false;
            });
            if (notifyTrackPair != null)
            {
                return notifyTrackPair.TrackID;
            }
            return -1;
        }
        void RemoveFromNotifyTrackMap(Guid notifyId)
        {
            var notifyTrackPair = mCurrentResourceInfo.NotifyTrackMap.Find((pair) =>
            {
                if (pair.NotifyID == notifyId)
                    return true;
                else
                    return false;
            });
            if (notifyTrackPair != null)
            {
                mCurrentResourceInfo.NotifyTrackMap.Remove(notifyTrackPair);
            }
        }
        #endregion
        #region NotifyTrack
        void AddNotifyTrack(int index, NotifyEditControl notifyCtrl, bool changeTrackCount = true)
        {
            mNotifyEditeControlList.Insert(index, notifyCtrl);
            NotifyStackPanel.Children.Insert(index, notifyCtrl);
            for (int i = index + 1; i < mNotifyEditeControlList.Count; ++i)
            {
                mNotifyEditeControlList[i].TrackNum = i;
            }
            if (changeTrackCount)
                mCurrentResourceInfo.TrackCount = mNotifyEditeControlList.Count;
        }
        void RemoveNotifyTrack(int index)
        {
            NotifyStackPanel.Children.RemoveAt(index);
            mNotifyEditeControlList.RemoveAt(index);
            for (int i = index; i < mNotifyEditeControlList.Count; ++i)
            {
                mNotifyEditeControlList[i].TrackNum = i;
            }
            mCurrentResourceInfo.TrackCount = mNotifyEditeControlList.Count;
        }
        NotifyEditControl CreateNotifyTrack(int trackNum)
        {
            var notifyTrack = new NotifyEditControl();
            notifyTrack.TrackNum = trackNum;
            notifyTrack.SkeletonAsset = mSkeletonAsset;
            notifyTrack.EditorAnimationClip = mEditorAnimationClip;
            notifyTrack.AnimSlider.Maximum = AnimationPlayingCtrl.TotalFrame;
            notifyTrack.AnimSlider.AnimationDuration = mEditorAnimationClip.AnimationClip.DurationInMilliSecond;
            notifyTrack.OnTickBarScaling += NotifyTrack_OnTickBarScaling;
            notifyTrack.OnTrackAdd += NotifyTrack_OnTrackAdd;
            notifyTrack.OnTrackRemove += NotifyTrack_OnTrackRemove;
            notifyTrack.OnMouseIn += NotifyTrack_OnMouseIn;
            notifyTrack.OnMouseOut += NotifyTrack_OnMouseOut;
            notifyTrack.OnNotifyPickUp += NotifyTrack_OnNotifyPickUp;
            notifyTrack.OnNotifyDropDown += NotifyTrack_OnNotifyDropDown;
            notifyTrack.OnAddNotify += NotifyTrack_OnAddNotify;
            notifyTrack.OnRemoveNotify += NotifyTrack_OnRemoveNotify;
            notifyTrack.OnChangeNotifyTrack += NotifyTrack_OnChangeNotifyTrack;
            notifyTrack.OnNotifySelected += NotifyTrack_OnNotifySelected;
            return notifyTrack;
        }

        void DeleteNotifyTrack(int trackNum)
        {
            var notifyTrack = mNotifyEditeControlList[trackNum];
            notifyTrack.OnTickBarScaling -= NotifyTrack_OnTickBarScaling;
            notifyTrack.OnTrackAdd -= NotifyTrack_OnTrackAdd;
            notifyTrack.OnTrackRemove -= NotifyTrack_OnTrackRemove;
            notifyTrack.OnMouseIn -= NotifyTrack_OnMouseIn;
            notifyTrack.OnMouseOut -= NotifyTrack_OnMouseOut;
            notifyTrack.OnNotifyPickUp -= NotifyTrack_OnNotifyPickUp;
            notifyTrack.OnNotifyDropDown -= NotifyTrack_OnNotifyDropDown;
            notifyTrack.OnAddNotify -= NotifyTrack_OnAddNotify;
            notifyTrack.OnRemoveNotify -= NotifyTrack_OnRemoveNotify;
            notifyTrack.OnChangeNotifyTrack -= NotifyTrack_OnChangeNotifyTrack;
            notifyTrack.OnNotifySelected -= NotifyTrack_OnNotifySelected;
        }
        NotifyEditControl GetNofifyTrack(int trackNum)
        {
            return mNotifyEditeControlList[trackNum];
        }
        private void NotifyTrack_OnTrackAdd(object sender, NotifyTrackEventArgs e)
        {
            var ctrl = CreateNotifyTrack(e.TrackNum + 1);
            AddNotifyTrack(e.TrackNum + 1, ctrl);
        }
        private void NotifyTrack_OnTrackRemove(object sender, NotifyTrackEventArgs e)
        {
            if (mNotifyEditeControlList.Count == 1)
                return;
            DeleteNotifyTrack(e.TrackNum);
            RemoveNotifyTrack(e.TrackNum);
        }
        AnimNotifyNodeControl mPickedNotifyNodeCtrl = null;
        private void NotifyTrack_OnNotifyPickUp(object sender, AnimNotifyEditControlEventArgs e)
        {
            mPickedNotifyNodeCtrl = e.Ctrl;
        }
        private void NotifyTrack_OnNotifyDropDown(object sender, AnimNotifyEditControlEventArgs e)
        {
            mPickedNotifyNodeCtrl = null;
        }
        private void NotifyTrack_OnMouseIn(object sender, AnimNotifyEditControlEventArgs e)
        {
            if (mPickedNotifyNodeCtrl == null)
                return;
            if (mPickedNotifyNodeCtrl.TrackNum != e.TrackNum)
            {
                var fromCtrl = GetNofifyTrack(mPickedNotifyNodeCtrl.TrackNum);
                fromCtrl.RemoveNotifyNode(mPickedNotifyNodeCtrl);
                var notifyEditCtrl = GetNofifyTrack(e.TrackNum);
                notifyEditCtrl.ChangeNotifyNodeTrack(mPickedNotifyNodeCtrl);
                Console.WriteLine("NotifyCtrl_OnMouseIn");
            }
        }
        private void NotifyTrack_OnMouseOut(object sender, AnimNotifyEditControlEventArgs e)
        {

        }
        private void NotifyTrack_OnAddNotify(object sender, AnimNotifyEditControlEventArgs e)
        {
            AddToNotifyTrackMap(e.Ctrl.CGfxNotify.ID, e.TrackNum, e.Ctrl.CGfxNotify.NotifyName);
        }
        private void NotifyTrack_OnRemoveNotify(object sender, AnimNotifyEditControlEventArgs e)
        {
            RemoveFromNotifyTrackMap(e.Ctrl.CGfxNotify.ID);
        }
        private void NotifyTrack_OnChangeNotifyTrack(object sender, AnimNotifyEditControlEventArgs e)
        {
            AddToNotifyTrackMap(e.Ctrl.CGfxNotify.ID, e.TrackNum, e.Ctrl.CGfxNotify.NotifyName);
            //RefreshNotifyTrackMap(e.Ctrl.CGfxNotify.ID, e.TrackNum);
        }
        CGfxNotify mSelectNotify = null;
        public GActor PreviewActor { get; set; } = null;
        private void NotifyTrack_OnNotifySelected(object sender, AnimNotifyEditControlEventArgs e)
        {
            ProGrid.Instance = null;
            ProGrid.Instance = e.Ctrl.CGfxNotify;
            if (mSelectNotify != null)
            {
                mSelectNotify.OnEditor_UnSelected(mEditorAnimationClip.AnimationClip, PreviewActor);
            }
            mSelectNotify = e.Ctrl.CGfxNotify;
            mSelectNotify.OnEditor_Selected(mEditorAnimationClip.AnimationClip, PreviewActor);
        }
        #endregion
        private void NotifyTrack_OnTickBarScaling(object sender, TickBarScaleEventArgs e)
        {
            AnimationPlayingCtrl.TickBarScale(e.DeltaScale, e.Percent);
        }

        private void AnimationPlayingCtrl_OnTickBarScaling(object sender, TickBarScaleEventArgs e)
        {
            foreach (var notifyctrl in mNotifyEditeControlList)
            {
                notifyctrl.TickBarScale(e.DeltaScale, e.Percent);
            }
        }

        #region NotifyEdit
        List<NotifyEditControl> mNotifyEditeControlList = new List<NotifyEditControl>();
        #endregion
    }
}
