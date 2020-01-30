using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CodeDomNode.Animation
{
    public class LAStatesBridge
    {
        public CodeMemberMethod ConstructLAGraphMethod { get; set; } = null;
        public CodeVariableReferenceExpression LAStateMachine { get; set; } = null;
        public string LAStateMachineName { get; set; } = "";
        public Dictionary<string, CodeVariableReferenceExpression> LAStatesDic { get; set; } = new Dictionary<string, CodeVariableReferenceExpression>();
        public Dictionary<string, NodesContainer> LAStateNodesContainerDic { get; set; } = new Dictionary<string, NodesContainer>();
        public Dictionary<string, BaseNodeControl> LATransitionFinalResultNodesDic { get; set; } = new Dictionary<string, BaseNodeControl>();
        public Dictionary<string, BaseNodeControl> LATransitionExecuteNodesDic { get; set; } = new Dictionary<string, BaseNodeControl>();
        public Stack<CodeMemberMethod> LATransitionMethods { get; set; } = new Stack<CodeMemberMethod>();
        public Stack<string> LATransitionExecuteMethods { get; set; } = new Stack<string>();
        public CodeVariableReferenceExpression GetLAState(string name)
        {
            if (LAStatesDic.ContainsKey(name))
                return LAStatesDic[name];
            return null;
        }
        public NodesContainer GetLAStateNodesContainer(string name)
        {
            if (LAStateNodesContainerDic.ContainsKey(name))
                return LAStateNodesContainerDic[name];
            return null;
        }
        public BaseNodeControl GetLATransitionFinalResultNode(string name)
        {
            if (LATransitionFinalResultNodesDic.ContainsKey(name))
                return LATransitionFinalResultNodesDic[name];
            return null;
        }
        public BaseNodeControl GetLATransitionExecuteNode(string name)
        {
            if (LATransitionExecuteNodesDic.ContainsKey(name))
                return LATransitionExecuteNodesDic[name];
            return null;
        }
    }
    [EngineNS.Rtti.MetaClass]
    public class LACTransition : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public Guid CtrlID { get; set; }
        LATransitionNodeControl mHostLATNodeCtrl = null;
        public LATransitionNodeControl HostLATNodeCtrl
        {
            get => mHostLATNodeCtrl;
            set
            {
                mHostLATNodeCtrl = value;
                mHostLATNodeCtrl.LACTransition = this;
            }
        }
        public LACTransition Duplicate(CodeGenerateSystem.Base.BaseNodeControl.DuplicateParam param)
        {
            var copy = new LACTransition();
            copy.TrackIndex = TrackIndex;
            var ctrl = HostLATNodeCtrl.Duplicate(param) as LATransitionNodeControl;
            copy.HostLATNodeCtrl = ctrl;
            copy.CtrlID = ctrl.Id;
            copy.Position = Position;
            //param.TargetNodesContainer.AddNodeControl(copy.HostLATNodeCtrl, copy.Position.X, copy.Position.Y);
            return copy;
        }
        public void AddToNode(LAClipNodeControl node)
        {
            node.AddChildNode(HostLATNodeCtrl);
        }
        public LACTransitionArea HostLACTransitionArea { get; set; } = null;
        public int TrackIndex { get; set; } = 0;
        public Point Position
        {
            get
            {
                return HostLATNodeCtrl.GetLocation();
            }
            set
            {
                HostLATNodeCtrl.SetLocation(value.X, value.Y);
            }
        }
        public Size Size
        {
            get
            {
                if (HostLATNodeCtrl.ActualWidth == 0 || HostLATNodeCtrl.ActualHeight == 0)
                {
                    if (double.IsNaN(HostLATNodeCtrl.Width) || double.IsNaN(HostLATNodeCtrl.Height))
                        return new Size(0, 0);
                    else
                        return new Size(HostLATNodeCtrl.Width, HostLATNodeCtrl.Height);
                }
                else
                    return new Size(HostLATNodeCtrl.ActualWidth, HostLATNodeCtrl.ActualHeight);
            }
        }
        public Rect Rect
        {
            get
            {
                return new Rect(Position, Size);
            }
        }
        public void ReCorrect(LAClipNodeControl ctrl, double x, double y)
        {
            ctrl.TransitionNodes.Add(HostLATNodeCtrl);
            HostLATNodeCtrl.SetLocation(x, y);
        }
    }
    [EngineNS.Rtti.MetaClass]
    public class LACTransitionTrack : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public List<LACTransition> LACTransitions { get; set; } = new List<LACTransition>();
        [EngineNS.Rtti.MetaData]
        public int TrackIndex { get; set; }
        public float Width { get; set; }
        public float Height { get; set; } = 10;
        LACTransitionArea mHostLACTransitionArea = null;
        public LACTransitionArea HostLACTransitionArea
        {
            get => mHostLACTransitionArea;
            set
            {
                mHostLACTransitionArea = value;
                for (int i = 0; i < LACTransitions.Count; ++i)
                {
                    LACTransitions[i].HostLACTransitionArea = value;
                }
            }
        }
        public bool CanInsert(Point newPos, LACTransition transition, out LACTransition collisionTransition)
        {
            for (int i = 0; i < LACTransitions.Count; ++i)
            {
                if (LACTransitions[i] != transition)
                {
                    var rect = new Rect(newPos, transition.Size);
                    if (LACTransitions[i].Rect.IntersectsWith(rect))
                    {
                        collisionTransition = LACTransitions[i];
                        return false;
                    }
                }
            }
            collisionTransition = null;
            return true;
        }
        public LACTransitionTrack Duplicate(CodeGenerateSystem.Base.BaseNodeControl.DuplicateParam param)
        {
            var copy = new LACTransitionTrack();
            copy.TrackIndex = TrackIndex;
            copy.Width = Width;
            copy.Height = Height;
            for (int i = 0; i < LACTransitions.Count; ++i)
            {
                var trans = LACTransitions[i].Duplicate(param);
                copy.LACTransitions.Add(trans);
            }
            return copy;
        }
        public void AddToNode(LAClipNodeControl node)
        {
            for (int i = 0; i < LACTransitions.Count; ++i)
            {
                LACTransitions[i].AddToNode(node);
            }
        }

    }
    [EngineNS.Rtti.MetaClass]
    public class LACTransitionArea : EngineNS.IO.Serializer.Serializer
    {
        public LAClipNodeControl HostLAClipNodeControl { get; set; } = null;
        [EngineNS.Rtti.MetaData]
        public float TracksHeight { get; set; } = 10;
        [EngineNS.Rtti.MetaData]
        public List<LACTransitionTrack> Tracks { get; set; } = new List<LACTransitionTrack>();
        public void ForceMove(Point newPos, LACTransition transition)
        {
            var oriTrackIndex = transition.TrackIndex;
            LACTransition outTransition = null;
            if (Tracks[oriTrackIndex].CanInsert(newPos, transition, out outTransition))
            {
                transition.Position = newPos;
            }
            else
            {

            }
        }
        void RoundPos(ref Point point, LACTransition transition)
        {
            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;
            if (point.X + transition.Size.Width > HostLAClipNodeControl.TransitionCanvas.Width)
                point.X = HostLAClipNodeControl.TransitionCanvas.Width - transition.Size.Width;
            if (point.Y + transition.Size.Height > HostLAClipNodeControl.TransitionCanvas.Height)
                point.Y = HostLAClipNodeControl.TransitionCanvas.Height - transition.Size.Height;
        }
        public bool TryMoveX(Point newPos, LACTransition transition)
        {
            var delta = newPos - transition.Position;
            var oriTrackIndex = transition.TrackIndex;
            var pos = newPos;
            var wantTrackIndex = (int)(pos.Y / TracksHeight);
            var roundPos = new Point(pos.X, oriTrackIndex * TracksHeight);
            RoundPos(ref roundPos, transition);
            var trackIndex = oriTrackIndex;
            var newRoundPos = roundPos;
            LACTransition outTransition = null;
            if (Tracks[trackIndex].CanInsert(newRoundPos, transition, out outTransition))
            {
                transition.Position = newRoundPos;
                if (transition.TrackIndex != trackIndex)
                {
                    transition.TrackIndex = trackIndex;
                    Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                    Tracks[trackIndex].LACTransitions.Add(transition);
                }
                RefreshTracks();
                return true;
            }
            else
            {
                if (delta.X < 0)
                {
                    pos.X = outTransition.Rect.Right + 1;
                    pos.Y = wantTrackIndex * TracksHeight;
                    RoundPos(ref pos, transition);
                    if (Tracks[trackIndex].CanInsert(pos, transition, out outTransition))
                    {
                        transition.Position = pos;
                        if (transition.TrackIndex != trackIndex)
                        {
                            transition.TrackIndex = trackIndex;
                            Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                            Tracks[trackIndex].LACTransitions.Add(transition);
                        }
                        RefreshTracks();
                        return true;
                    }
                }
                else
                {
                    pos.X = outTransition.Rect.Left - transition.Size.Width - 1;
                    pos.Y = wantTrackIndex * TracksHeight;
                    RoundPos(ref pos, transition);
                    if (Tracks[trackIndex].CanInsert(pos, transition, out outTransition))
                    {
                        transition.Position = pos;
                        if (transition.TrackIndex != trackIndex)
                        {
                            transition.TrackIndex = trackIndex;
                            Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                            Tracks[trackIndex].LACTransitions.Add(transition);
                        }
                        RefreshTracks();
                        return true;
                    }
                }
                return false;
            }
        }
        public bool TryMoveY(Point newPos, LACTransition transition)
        {
            var delta = newPos - transition.Position;
            delta.X = 0;
            var oriTrackIndex = transition.TrackIndex;
            var pos = newPos;
            pos.X = transition.Position.X;
            var wantTrackIndex = (int)(pos.Y / TracksHeight);
            var roundPos = new Point(pos.X, wantTrackIndex * TracksHeight);
            RoundPos(ref roundPos, transition);
            if (wantTrackIndex > Tracks.Count - 1)
            {
                var track = new LACTransitionTrack() { TrackIndex = Tracks.Count };
                Tracks.Add(track);
                Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                transition.TrackIndex = track.TrackIndex;
                transition.Position = new Point(pos.X, wantTrackIndex * TracksHeight);
                track.LACTransitions.Add(transition);
                RefreshTracks();
                return true;
            }
            var trackIndex = (int)(roundPos.Y / TracksHeight);
            var newRoundPos = roundPos;
            LACTransition outTransition = null;
            if (Tracks[trackIndex].CanInsert(newRoundPos, transition, out outTransition))
            {
                transition.Position = newRoundPos;
                if (transition.TrackIndex != trackIndex)
                {
                    transition.TrackIndex = trackIndex;
                    Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                    Tracks[trackIndex].LACTransitions.Add(transition);
                }
                RefreshTracks();
                return true;
            }
            return false;
        }
        public bool TryMoveBoth(Point newPos, LACTransition transition)
        {
            var delta = newPos - transition.Position;
            var oriTrackIndex = transition.TrackIndex;
            var pos = newPos;
            var wantTrackIndex = (int)(pos.Y / TracksHeight);
            var roundPos = new Point(pos.X, wantTrackIndex * TracksHeight);
            RoundPos(ref roundPos, transition);
            if (wantTrackIndex > Tracks.Count - 1)
            {
                var track = new LACTransitionTrack() { TrackIndex = Tracks.Count };
                Tracks.Add(track);
                Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                transition.TrackIndex = track.TrackIndex;
                transition.Position = new Point(pos.X, wantTrackIndex * TracksHeight);
                track.LACTransitions.Add(transition);
                RefreshTracks();
                return true;
            }

            var trackIndex = (int)(roundPos.Y / TracksHeight);
            var newRoundPos = roundPos;
            LACTransition outTransition = null;
            if (Tracks[trackIndex].CanInsert(newRoundPos, transition, out outTransition))
            {
                transition.Position = newRoundPos;
                if (transition.TrackIndex != trackIndex)
                {
                    transition.TrackIndex = trackIndex;
                    Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                    Tracks[trackIndex].LACTransitions.Add(transition);
                }
                RefreshTracks();
                return true;
            }
            else
            {
                if (delta.X < 0)
                {
                    pos.X = outTransition.Rect.Right + 1;
                    pos.Y = wantTrackIndex * TracksHeight;
                    RoundPos(ref pos, transition);
                    if (Tracks[trackIndex].CanInsert(pos, transition, out outTransition))
                    {
                        transition.Position = pos;
                        if (transition.TrackIndex != trackIndex)
                        {
                            transition.TrackIndex = trackIndex;
                            Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                            Tracks[trackIndex].LACTransitions.Add(transition);
                        }
                        RefreshTracks();
                        return true;
                    }
                }
                else
                {
                    pos.X = outTransition.Rect.Left - transition.Size.Width - 1;
                    pos.Y = wantTrackIndex * TracksHeight;
                    RoundPos(ref pos, transition);
                    if (Tracks[trackIndex].CanInsert(pos, transition, out outTransition))
                    {
                        transition.Position = pos;
                        if (transition.TrackIndex != trackIndex)
                        {
                            transition.TrackIndex = trackIndex;
                            Tracks[oriTrackIndex].LACTransitions.Remove(transition);
                            Tracks[trackIndex].LACTransitions.Add(transition);
                        }
                        RefreshTracks();
                        return true;
                    }
                }
                return false;
            }
        }
        public void RefreshTracks()
        {
            int count = Tracks.Count;
            for (int i = 0; i < count; ++i)
            {
                if (Tracks[i].LACTransitions.Count == 0)
                {
                    Tracks.RemoveAt(i);
                    count--;
                }
            }
            for (int i = 0; i < Tracks.Count; ++i)
            {
                var trackIndex = Tracks[i].TrackIndex;
                //if (trackIndex != i)
                {
                    var track = Tracks[i];
                    track.TrackIndex = i;
                    var height = i * TracksHeight;
                    for (int j = 0; j < Tracks[i].LACTransitions.Count; ++j)
                    {
                        track.LACTransitions[j].TrackIndex = track.TrackIndex;
                        var newRoundPos = track.LACTransitions[j].Position;
                        newRoundPos.Y = height;
                        track.LACTransitions[j].Position = newRoundPos;
                    }
                }
            }
            HostLAClipNodeControl.TransitionCanvas.Height = Tracks.Count * TracksHeight;
        }
        public void Insert(LACTransition transition)
        {
            bool isInsertInExistTrack = false;
            var trackIndex = (int)(transition.Position.Y / TracksHeight);
            for (int i = trackIndex; i < Tracks.Count; ++i)
            {
                var pos = new Point(transition.Position.X, trackIndex * TracksHeight);
                LACTransition outTransition = null;
                if (Tracks[trackIndex].CanInsert(pos, transition, out outTransition))
                {
                    transition.TrackIndex = trackIndex;
                    transition.Position = pos;
                    Tracks[trackIndex].LACTransitions.Add(transition);
                    isInsertInExistTrack = true;
                    break;
                }
                else
                {
                    trackIndex++;
                }
            }
            if (!isInsertInExistTrack)
            {
                var track = new LACTransitionTrack() { TrackIndex = Tracks.Count };
                Tracks.Add(track);
                var finalPos = new Point(transition.Position.X, track.TrackIndex * TracksHeight);
                LACTransition outTransition = null;
                if (track.CanInsert(finalPos, transition, out outTransition))
                {
                    transition.TrackIndex = track.TrackIndex;
                    transition.Position = finalPos;
                    track.LACTransitions.Add(transition);
                }
            }
            if (HostLAClipNodeControl.TransitionCanvas.Height != Tracks.Count * TracksHeight)
                HostLAClipNodeControl.TransitionCanvas.Height = Tracks.Count * TracksHeight;
        }
        public LACTransitionArea Duplicate(CodeGenerateSystem.Base.BaseNodeControl.DuplicateParam param)
        {
            var copy = new LACTransitionArea();
            copy.TracksHeight = TracksHeight;
            for (int i = 0; i < Tracks.Count; ++i)
            {
                var trans = Tracks[i].Duplicate(param);
                trans.HostLACTransitionArea = copy;
                copy.Tracks.Add(trans);
            }
            return copy;
        }
        public void AddToNode(LAClipNodeControl node)
        {
            node.LACTransitionArea = this;
            for (int i = 0; i < Tracks.Count; ++i)
            {
                Tracks[i].AddToNode(node);
            }
        }
    }
    public class SkeletonConstructionParams : ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string SkeletonAsset { get; set; } = "";
    }
    public class LAClipNodeControlConstructionParams : SkeletonConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public bool Repeat { get; set; } = true;
        [EngineNS.Rtti.MetaData]
        public double Width { get; set; } = 200;
        [EngineNS.Rtti.MetaData]
        public double Height { get; set; } = 50;
        [EngineNS.Rtti.MetaData]
        public float Duration { get; set; } = 2;
        [EngineNS.Rtti.MetaData]
        public float WidthScale { get; set; } = 1;
        [EngineNS.Rtti.MetaData]
        public bool DefaultState { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public bool IsAdditive { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName RefAnimation { get; set; } = EngineNS.RName.EmptyName;
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName Animation { get; set; } = EngineNS.RName.EmptyName;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "LogicClip";
        [EngineNS.Rtti.MetaData]
        public LACTransitionArea LACTransitionArea { get; set; } = new LACTransitionArea();
        [EngineNS.Rtti.MetaData]
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        [EngineNS.Rtti.MetaData]
        public Guid CustomLAPoseGraphID { get; set; } = Guid.NewGuid();
        [EngineNS.Rtti.MetaData]
        public Guid EventGraphID { get; set; } = Guid.NewGuid();
        [EngineNS.Rtti.MetaData]
        public AI.CenterDataWarpper CenterDataWarpper { get; set; } = new AI.CenterDataWarpper();
        public Action<EngineNS.RName> OnAdded = null;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAClipNodeControlConstructionParams;
            retVal.Width = Width;
            retVal.Height = Height;
            retVal.Duration = Duration;
            retVal.WidthScale = WidthScale;
            retVal.Animation = Animation;
            retVal.RefAnimation = RefAnimation;
            retVal.IsAdditive = IsAdditive;
            retVal.LACTransitionArea = LACTransitionArea;
            retVal.DefaultState = DefaultState;
            retVal.Repeat = Repeat;
            retVal.OnAdded = OnAdded;
            retVal.SkeletonAsset = SkeletonAsset;
            retVal.LinkedCategoryItemID = LinkedCategoryItemID;
            retVal.CustomLAPoseGraphID = Guid.NewGuid();
            retVal.EventGraphID = Guid.NewGuid();
            retVal.CenterDataWarpper = CenterDataWarpper;
            return retVal;
        }
    }
    public class TimeLength
    {
        public static float Second2Pixel = 50.0f;
        public static float pixel2Second = 1 / Second2Pixel;
        public static double GetWidthByTime(float duration, float widthScale)
        {
            return (double)duration * TimeLength.Second2Pixel * widthScale;
        }
        public static float GeTimeByWidth(double width, float widthScale)
        {
            return (float)width * TimeLength.pixel2Second * (1 / widthScale);
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAClipNodeControlConstructionParams))]
    public partial class LAClipNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public AI.CenterDataWarpper CenterDataWarpper { get; set; } = null;
        public string GetValidClipName(string name)
        {
            if (HostNodesContainer == null)
                return name;
            var tempName = name;
            var nodeList = HostNodesContainer.CtrlNodeList;
            bool repetition = false;
            for (int i = 0; i < nodeList.Count; ++i)
            {
                if (nodeList[i] is LAClipNodeControl)
                {
                    var clip = nodeList[i] as LAClipNodeControl;
                    if (clip.NodeName == tempName)
                    {
                        repetition = true;
                        break;
                    }
                }
            }
            if (repetition)
            {
                for (int i = 1; i < int.MaxValue; ++i)
                {
                    tempName = name + i;
                    repetition = false;
                    for (int j = 0; j < nodeList.Count; ++j)
                    {
                        if (nodeList[j] is LAClipNodeControl)
                        {
                            var clip = nodeList[j] as LAClipNodeControl;
                            if (clip.NodeName == tempName)
                            {
                                repetition = true;
                                break;
                            }
                        }
                    }
                    if (!repetition)
                        break;
                }
            }
            return tempName;
        }
        public Guid CustomLAPoseGraphID { get; set; } = Guid.NewGuid();
        public Guid EventGraphID { get; set; } = Guid.NewGuid();
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public List<string> Notifies = new List<string>();

        partial void InitConstruction();

        public string SkeletonAsset { get; set; } = "";
        #region DP
        Visibility mRepeatVisibility = Visibility.Collapsed;
        public Visibility RepeatVisibility
        {
            get => mRepeatVisibility;
            set
            {
                mRepeatVisibility = value;
                OnPropertyChanged("RepeatVisibility");
            }
        }
        public bool IsAdditive
        {
            get => (CSParam as LAClipNodeControlConstructionParams).IsAdditive;
        }
        public bool Repeat
        {
            get { return (bool)GetValue(RepeatProperty); }
            set
            {
                SetValue(RepeatProperty, value);
                var para = CSParam as LAClipNodeControlConstructionParams;
                para.Repeat = value;
                if (value)
                    RepeatVisibility = Visibility.Visible;
                else
                    RepeatVisibility = Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty RepeatProperty = DependencyProperty.Register("Repeat", typeof(bool), typeof(LAClipNodeControl), new UIPropertyMetadata(true, RepeatPropertyChanged));
        private static void RepeatPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAClipNodeControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.Repeat = (bool)e.NewValue;
        }
        Visibility mDefaultStateVisibility = Visibility.Collapsed;
        public Visibility DefaultStateVisibility
        {
            get => mDefaultStateVisibility;
            set
            {
                mDefaultStateVisibility = value;
                OnPropertyChanged("DefaultStateVisibility");
            }
        }

        public bool DefaultState
        {
            get { return (bool)GetValue(DefaultStateProperty); }
            set
            {
                SetValue(DefaultStateProperty, value);
                var para = CSParam as LAClipNodeControlConstructionParams;
                para.DefaultState = value;
                if (value)
                    DefaultStateVisibility = Visibility.Visible;
                else
                    DefaultStateVisibility = Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty DefaultStateProperty = DependencyProperty.Register("DefaultState", typeof(bool), typeof(LAClipNodeControl), new UIPropertyMetadata(false, DefaultStatePropertyChanged));
        private static void DefaultStatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAClipNodeControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.DefaultState = (bool)e.NewValue;
        }
        public bool AdjustTimeToAnimation = true;
        public string RefAnimationFilePathString
        {
            get
            {
                if (RefAnimation != null)
                    return RefAnimation.Name;
                return "";
            }
        }
        public Action<EngineNS.RName> OnAdded = null;
        public EngineNS.RName RefAnimation
        {
            get { return (EngineNS.RName)GetValue(RefAnimationProperty); }
            set
            {
                SetValue(RefAnimationProperty, value);
                var animationClip = AnimationClip.CreateSync(value);
                if (animationClip == null)
                    return;

                var para = CSParam as LAClipNodeControlConstructionParams;
                para.RefAnimation = value;
            }
        }
        public static readonly DependencyProperty RefAnimationProperty = DependencyProperty.Register("RefAnimationProperty", typeof(EngineNS.RName), typeof(LAClipNodeControl), new UIPropertyMetadata(EngineNS.RName.EmptyName, RefAnimationPropertyChanged));
        private static void RefAnimationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAClipNodeControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.RefAnimation = (EngineNS.RName)e.NewValue;
        }
        public string AnimationFilePathString
        {
            get
            {
                if (Animation != null)
                    return Animation.Name;
                return "";
            }
        }
        public EngineNS.RName Animation
        {
            get { return (EngineNS.RName)GetValue(AnimationProperty); }
            set
            {
                SetValue(AnimationProperty, value);
                if (value == null)
                    return;
                var animationClip = AnimationClip.CreateSync(value);
                if (animationClip == null)
                    return;
                if (AdjustTimeToAnimation)
                {
                    if (animationClip.Duration != 0)
                        Duration = animationClip.Duration;
                }
                OnAdded?.Invoke(EngineNS.RName.GetRName(animationClip.GetElementProperty(ElementPropertyType.EPT_Skeleton)));
                var para = CSParam as LAClipNodeControlConstructionParams;
                para.Animation = value;
            }
        }
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("AnimationProperty", typeof(EngineNS.RName), typeof(LAClipNodeControl), new UIPropertyMetadata(EngineNS.RName.EmptyName, AnimationPropertyChanged));
        private static void AnimationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAClipNodeControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.Animation = (EngineNS.RName)e.NewValue;
        }
        public float WidthScale
        {
            get { return (float)GetValue(WidthScaleProperty); }
            set
            {
                SetValue(WidthScaleProperty, value);
                var para = CSParam as LAClipNodeControlConstructionParams;
                para.WidthScale = value;
                TransitionCanvas.Width = TimeLength.GetWidthByTime(Duration, value);
                Width = TimeLength.GetWidthByTime(Duration, value) + ExtraWidth;
                if (LACTransitionArea == null)
                    return;
                for (int i = 0; i < LACTransitionArea.Tracks.Count; ++i)
                {
                    for (int j = 0; j < LACTransitionArea.Tracks[i].LACTransitions.Count; ++j)
                    {
                        if (LACTransitionArea.Tracks[i].LACTransitions[j].HostLATNodeCtrl == null)
                        {
                            LACTransitionArea.Tracks[i].LACTransitions.RemoveAt(j);
                            j--;
                        }
                        else
                        {
                            LACTransitionArea.Tracks[i].LACTransitions[j].HostLATNodeCtrl.WidthScale = value;
                        }
                    }
                }
            }
        }
        public static readonly DependencyProperty WidthScaleProperty = DependencyProperty.Register("WidthScale", typeof(float), typeof(LAClipNodeControl), new UIPropertyMetadata(1.0f, OnWidthScalePropertyChanged));
        private static void OnWidthScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAClipNodeControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.WidthScale = (float)e.NewValue;
        }
        public float Duration
        {
            get { return (float)GetValue(DurationProperty); }
            set
            {
                SetValue(DurationProperty, value);
                var para = CSParam as LAClipNodeControlConstructionParams;
                para.Duration = value;
                TransitionCanvas.Width = TimeLength.GetWidthByTime(value, WidthScale);
                Width = TimeLength.GetWidthByTime(value, WidthScale) + ExtraWidth;
            }
        }
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(float), typeof(LAClipNodeControl), new UIPropertyMetadata(1.0f, OnDurationPropertyChanged));
        private static void OnDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAClipNodeControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Duration = (float)e.NewValue;
        }
        #endregion

        #region ShowProperty
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "Repeat", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "DefaultState", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Duration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "WidthScale", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(EngineNS.RName), "Animation", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip) }));
            if (IsAdditive)
                cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(EngineNS.RName), "RefAnimation", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip) }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            CreateBinding(mTemplateClassInstance, "Repeat", LAClipNodeControl.RepeatProperty, Repeat);
            CreateBinding(mTemplateClassInstance, "DefaultState", LAClipNodeControl.DefaultStateProperty, DefaultState);
            CreateBinding(mTemplateClassInstance, "Duration", LAClipNodeControl.DurationProperty, Duration);
            //CreateBinding(mTemplateClassInstance, "WidthScale", LAClipNodeControl.WidthScaleProperty, WidthScale);
            if (IsAdditive)
                CreateBinding(mTemplateClassInstance, "RefAnimation", LAClipNodeControl.RefAnimationProperty, RefAnimation);
            CreateBinding(mTemplateClassInstance, "Animation", LAClipNodeControl.AnimationProperty, Animation);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion



        public LAClipNodeControl(LAClipNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            mChildNodeContainer = TransitionCanvas;
            this.NodeName = GetValidClipName(csParam.NodeName);
            csParam.NodeName = this.NodeName;
            var duration = csParam.Duration;
            WidthScale = csParam.WidthScale;
            Animation = csParam.Animation;
            RefAnimation = csParam.RefAnimation;
            Duration = duration;
            LACTransitionArea = csParam.LACTransitionArea;
            DefaultState = csParam.DefaultState;
            Repeat = csParam.Repeat;
            LinkedCategoryItemID = csParam.LinkedCategoryItemID;
            CustomLAPoseGraphID = csParam.CustomLAPoseGraphID;
            EventGraphID = csParam.EventGraphID;
            CenterDataWarpper = csParam.CenterDataWarpper;
            //Width = TransitionCanvas.Width + ExtraWidth;
            OnAdded = csParam.OnAdded;
            LACTransitionArea.HostLAClipNodeControl = this;
            BindingTemplateClassInstanceProperties();
            IsOnlyReturnValue = true;

            AddLinkPinInfo("LogicClipLinkHandle", mCtrlValueLinkHandle, null);
        }
        #region Template
        float LeftExtraWith = 8;
        float RightExtraWith = 4;
        public float ExtraWidth
        {
            get => LeftExtraWith + RightExtraWith + 6;
        }
        public override void OnApplyTemplate()
        {
            switch (NodeType)
            {
                case enNodeType.CommentNode:
                    mDragHandle = Template.FindName("PART_HeaderGrid", this) as FrameworkElement;
                    SetDragObject(mDragHandle);
                    break;
                default:
                    mDragHandle = this;
                    SetDragObject(this);
                    break;
            }

            mBreakPoint = Template.FindName("PART_BREAKPT", this) as CodeGenerateSystem.Controls.BreakPoint;
            mBreakPoint?.Initialize(this);

            mCommentControl = Template.FindName("commentControl", this) as CodeGenerateSystem.Controls.CommentControl;
            var right = Template.FindName("PART_BORDER_Right", this) as Rectangle;
            RightExtraWith = (float)right.Width;
            if (right != null)
            {
                right.MouseLeftButtonDown += BorderCtrl_MouseLeftButtonDown;
                right.MouseLeftButtonUp += BorderCtrl_MouseLeftButtonUp;
                right.MouseMove += Right_MouseMove;
            }
            var rb = Template.FindName("PART_BORDER_RB", this) as Rectangle;
            if (rb != null)
            {
                rb.MouseLeftButtonDown += Rect_RB_MouseLeftButtonDown;
                rb.MouseLeftButtonUp += Rect_RB_MouseLeftButtonUp;
                rb.MouseMove += Rect_RB_MouseMove;
            }
            LeftExtraWith = (float)ValueLinkHandle.Width;
        }
        private void Right_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var deltaX = newPos.X - mMouseLeftButtonDownPos.X;
                var deltaWidth = TransitionCanvas.ActualWidth + deltaX;

                var param = CSParam as LAClipNodeControlConstructionParams;
                if (deltaWidth < TransitionCanvas.MinWidth)
                {
                    deltaWidth = TransitionCanvas.Width;
                }
                //TransitionCanvas.Width = deltaWidth;
                param.Duration = TimeLength.GeTimeByWidth(deltaWidth, WidthScale);
                Duration = param.Duration;
            }
            e.Handled = true;
        }
        Point mMouseLeftButtonDownPos;
        private void BorderCtrl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void BorderCtrl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            mMouseLeftButtonDownPos = e.GetPosition(elm);
            Mouse.Capture(elm);
            e.Handled = true;
        }
        private void Rect_RB_MouseMove(object sender, MouseEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            if (Mouse.Captured == elm)
            {
                var newPos = e.GetPosition(elm);
                var deltaX = newPos.X - mMouseLeftBottomButtonDownPos.X;
                var deltaWidth = TransitionCanvas.ActualWidth + deltaX;
                if (deltaWidth < TransitionCanvas.MinWidth)
                {
                    deltaWidth = TransitionCanvas.MinWidth;
                    //deltaX = this.Width - deltaWidth;
                }
                WidthScale = (float)(deltaWidth / TimeLength.GetWidthByTime(Duration, 1));
            }
            e.Handled = true;
        }
        Point mMouseLeftBottomButtonDownPos;
        private void Rect_RB_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            e.Handled = true;
        }

        private void Rect_RB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var elm = sender as System.Windows.IInputElement;
            mMouseLeftBottomButtonDownPos = e.GetPosition(elm);
            Mouse.Capture(elm);
            e.Handled = true;
        }

        protected override void OnSizeChanged(double width, double height)
        {

        }
        #endregion Template
        #region Duplicate
        //public override void SetLocation(double x, double y)
        //{
        //    base.SetLocation(x, y);
        //    if (NeedReCorrect)
        //    {
        //        LACTransitionArea.ReCorrect(this, x, y);
        //        NeedReCorrect = false;
        //    }
        //}
        public bool NeedReCorrect = false;
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var node = base.Duplicate(param) as LAClipNodeControl;
            var csParam = node.CSParam as LAClipNodeControlConstructionParams;
            csParam.LinkedCategoryItemID = param.TargetNodesContainer.HostControl.LinkedCategoryItemID;
            var tArea = LACTransitionArea.Duplicate(param);
            tArea.AddToNode(node);
            node.NeedReCorrect = true;
            Action action = async () =>
            {
                var customcontainer = await GetLAClipCustomContainer();
                var customCopy = customcontainer.Duplicate() as NodesContainerControl;
                customCopy.GUID = node.CustomLAPoseGraphID;
                customCopy.TitleString = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + node.CustomLAPoseGraphID.ToString();
                param.TargetNodesContainer.HostControl.SubNodesContainers.Add(node.CustomLAPoseGraphID, customCopy);
                customCopy.HostControl = param.TargetNodesContainer.HostControl;
                node.LinkedNodesContainer = customCopy;
                var eventcontainer = await GetLAClipStateEventContainer();
                var eventCopy = eventcontainer.Duplicate() as NodesContainerControl;
                eventCopy.GUID = node.EventGraphID;
                eventCopy.TitleString = HostNodesContainer.TitleString + "/" + this.NodeName + "_EventGraph" + ":" + node.EventGraphID.ToString();
                param.TargetNodesContainer.HostControl.SubNodesContainers.Add(node.EventGraphID, eventCopy);
                eventCopy.HostControl = param.TargetNodesContainer.HostControl;
                node.mEventGraphNodesContainer = eventCopy;
            };
            action.Invoke();
            return node;
        }
        #endregion Duplicate


        public override void ContainerLoadComplete(NodesContainer container)
        {
            LACTransitionArea.HostLAClipNodeControl = this;
            if (TransitionNodes.Count == 0)
            {
                for (int i = 0; i < LACTransitionArea.Tracks.Count; ++i)
                {
                    var track = LACTransitionArea.Tracks[i];
                    for (int j = 0; j < track.LACTransitions.Count; ++j)
                    {
                        var transiton = track.LACTransitions[j];
                        if (transiton.HostLATNodeCtrl == null)
                        {
                            var id = transiton.CtrlID;
                            var node = container.GetNodeWithGUID(ref id);
                            if (node == null)
                            {
                                track.LACTransitions.Remove(transiton);
                                j--;
                            }
                            else
                            {
                                var point = node.GetLocation();
                                var p = TransitionCanvas.TranslatePoint(point, node.ParentDrawCanvas);
                                node.ParentDrawCanvas.Children.Remove(node);
                                transiton.HostLATNodeCtrl = node as LATransitionNodeControl;
                                transiton.HostLACTransitionArea = LACTransitionArea;
                                node.NodeName = this.NodeName;
                                AddChildNode(node, mChildNodeContainer);
                                node.SetLocation(p.X, p.Y);
                            }
                        }
                    }
                    LACTransitionArea.RefreshTracks();
                    TransitionCanvas.Height = TransitionNodes.Count * LACTransitionArea.TracksHeight;
                }
            }
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "LogicClipLinkHandle", CodeGenerateSystem.Base.enLinkType.LAState, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        #region Transitions
        LACTransitionArea mLACTransitionArea = null;
        public LACTransitionArea LACTransitionArea
        {
            get => mLACTransitionArea;
            set
            {
                mLACTransitionArea = value;
                mLACTransitionArea.HostLAClipNodeControl = this;
            }
        }
        public override void OnOpenContextMenu(ContextMenu contextMenu)
        {
            var item = new MenuItem()
            {
                Name = "AddTransition",
                Header = "AddTransition",
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                AddTransitonNode();
            };
            contextMenu.Items.Add(item);
            item = new MenuItem()
            {
                Name = "RomoveTransition_",
                Header = "RomoveTransition_",
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {

            };
            //contextMenu.Items.Add(item);
            item = new MenuItem()
            {
                Name = "OpenEventGraph",
                Header = "OpenEventGraph",
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                var noUse = OpenEventGraph();
            };
            contextMenu.Items.Add(item);
            item = new MenuItem()
            {
                Name = "OpenCustomLAPose",
                Header = "OpenCustomLAPose",
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                var noUse = OpenCustomLAPoseGraph();
            };
            contextMenu.Items.Add(item);
        }
        public void AddTransitonNode()
        {
            var csParam = new LATransitionNodeControlConstructionParams();
            csParam.CSType = HostNodesContainer.CSType;
            csParam.WidthScale = WidthScale;
            csParam.Duration = Duration;
            csParam.LinkedCategoryItemID = LinkedCategoryItemID;
            csParam.DrawCanvas = TransitionCanvas;

            var ins = System.Activator.CreateInstance(typeof(LATransitionNodeControl), new object[] { csParam }) as BaseNodeControl;
            AddChildNode(ins, mChildNodeContainer);
            ins.SetLocation(0, 0);
            //var ins = HostNodesContainer.AddNodeControl(typeof(LATransitionNodeControl), csParam, 0, 0);
            ins.HostNodesContainer = HostNodesContainer;
            LACTransition lact = new LACTransition();
            lact.HostLATNodeCtrl = ins as LATransitionNodeControl;
            lact.HostLACTransitionArea = LACTransitionArea;
            lact.CtrlID = ins.Id;
            ins.NodeName = this.NodeName;
            LACTransitionArea.Insert(lact);
        }
        public List<CodeGenerateSystem.Base.BaseNodeControl> TransitionNodes
        {
            get => mChildNodes;
        }
        public void AddChildNode(BaseNodeControl node)
        {
            AddChildNode(node, TransitionCanvas);
        }
        protected override void AddChildNode(BaseNodeControl node, Panel container)
        {
            if (container == null)
                return;
            if (TransitionNodes.Contains(node))
                return;
            node.HostNodesContainer = HostNodesContainer;
            TransitionNodes.Add(node);
            node.SetParentNode(this, true, System.Windows.Visibility.Visible);

            //DecoratorNodes.Add(node);
            node.OnSelectNode += TransitionNode_OnSelectNode;
            node.OnMoveNode += ChildNode_OnMoveNode;
            node.OnStartLink += new Delegate_StartLink(StartLink);
            node.OnEndLink += new Delegate_EndLink(EndLink);
            container.Children.Add(node);
            mChildNodeContainer = container;
        }
        private void TransitionNode_OnSelectNode(BaseNodeControl node, bool bSelected, bool unselectedOther)
        {
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                TransitionNodes[i].Selected = false;
            }
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            assist.HostControl.OnSelectNodeControl(node);
        }
        private void ChildNode_OnMoveNode(BaseNodeControl node)
        {
            //var mousePos = Mouse.GetPosition(TransitionCanvas);
            //for (int i = 0; i < mChildNodeContainer.Children.Count; ++i)
            //{
            //    if (mChildNodeContainer.Children[i] == node)
            //        continue;
            //    var childWidth = (mChildNodeContainer.Children[i] as BaseNodeControl).ActualHeight;
            //    var childPos = mChildNodeContainer.Children[i].TranslatePoint(new Point(0, 0), mChildNodeContainer);
            //    if (mousePos.Y > childPos.Y && mousePos.Y < childPos.Y + childWidth)
            //    {
            //        if (mousePos.Y < childPos.Y + childWidth * 0.5f)
            //        {
            //            mChildNodeContainer.Children.Remove(node);
            //            mChildNodeContainer.Children.Insert(i, node);
            //            TransitionNodes.Remove(node);
            //            TransitionNodes.Insert(i, node);
            //            break;
            //        }
            //    }
            //}
            //ChildrenPanel.Children.Remove(node);
            //throw new NotImplementedException();
        }
        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                HostNodesContainer.DeleteNode(TransitionNodes[i]);
            }
            TransitionNodes.Clear();
        }
        protected override void StartDrag(UIElement dragObj, MouseButtonEventArgs e)
        {
            base.StartDrag(dragObj, e);
            int deepestZIndex = -10;
            // 计算所有在范围内的节点
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                var zIndex = Canvas.GetZIndex(TransitionNodes[i]);
                if (deepestZIndex > zIndex)
                    deepestZIndex = zIndex;
                TransitionNodes[i].CalculateDeltaPt(e);
            }
            Canvas.SetZIndex(this, deepestZIndex - 1);
        }
        protected override void DragMove(MouseEventArgs e)
        {
            base.DragMove(e);

            //var pt = e.GetPosition(ParentDrawCanvas);
            //for (int i = 0; i < TransitionNodes.Count; i++)
            //{
            //    TransitionNodes[i].MoveWithPt(pt);
            //}
        }
        protected override bool NodeNameCheckCanChange(string newVal)
        {
            return GetValidClipName(newVal) == newVal;
        }
        protected override void NodeNameChangedOverride(BaseNodeControl d, string oldVal, string newVal)
        {
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                TransitionNodes[i].NodeName = newVal;
            }
        }
        #endregion Transitions
        #region Save&Load
        BaseNodeControl GetNodeWithGUID(ref Guid id)
        {
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                if (TransitionNodes[i].Id == id)
                    return TransitionNodes[i];
            }
            return null;
        }
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            await base.Load(xndNode);
            LACTransitionArea.HostLAClipNodeControl = this;
            for (int i = 0; i < LACTransitionArea.Tracks.Count; ++i)
            {
                var track = LACTransitionArea.Tracks[i];
                for (int j = 0; j < track.LACTransitions.Count; ++j)
                {
                    var transiton = track.LACTransitions[j];
                    var id = transiton.CtrlID;
                    var node = GetNodeWithGUID(ref id);
                    if (node == null)
                    {

                    }
                    else
                    {
                        transiton.HostLATNodeCtrl = node as LATransitionNodeControl;
                        transiton.HostLACTransitionArea = LACTransitionArea;
                        node.NodeName = this.NodeName;
                    }
                }
            }
            TransitionCanvas.Height = TransitionNodes.Count * LACTransitionArea.TracksHeight;
        }
        #endregion Save&Load
        #region EventGraph
        NodesContainerControl mEventGraphNodesContainer = null;
        async System.Threading.Tasks.Task OpenEventGraph()
        {
            var assist = this.HostNodesContainer.HostControl;
            var data = new SubNodesContainerData()
            {
                ID = EventGraphID,
                Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_EventGraph" + ":" + EventGraphID.ToString(),
            };
            mEventGraphNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                CreateEventGraphDefaultNodes();
            }
            mEventGraphNodesContainer.HostNode = this;
            mEventGraphNodesContainer.OnFilterContextMenu = EventGraph_FilterContextMenu;
            mEventGraphNodesContainer.OnCheckDropAvailable = EventGraphCheckDropAvailable;
        }
        void CreateEventGraphDefaultNodes()
        {
            {
                var param = CSParam as LAClipNodeControlConstructionParams;
                var miAssist = new CustomMethodInfo();
                miAssist.MethodName = ValidName + "_OnEnter";
                miAssist.DisplayName = "OnEnter";
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = param.CSType,
                    HostNodesContainer = mEventGraphNodesContainer,
                    ConstructParam = "",
                    IsShowProperty = false,
                    MethodInfo = miAssist,
                };
                var node = mEventGraphNodesContainer.AddOrigionNode(nodeType, csParam, 0, 100);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;
            }
            {
                var param = CSParam as LAClipNodeControlConstructionParams;
                var miAssist = new CustomMethodInfo();
                miAssist.MethodName = ValidName + "_OnExit";
                miAssist.DisplayName = "OnExit";
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = param.CSType,
                    HostNodesContainer = mEventGraphNodesContainer,
                    ConstructParam = "",
                    IsShowProperty = false,
                    MethodInfo = miAssist,
                };
                var node = mEventGraphNodesContainer.AddOrigionNode(nodeType, csParam, 0, 400);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;
            }
            mEventGraphNodesContainer.HostNode = this;
        }
        bool EventGraphCheckDropAvailable(DragEventArgs e)
        {
            //if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            //{
            //    var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
            //    var data = dragObject[0] as EditorCommon.CodeGenerateSystem.INodeListAttribute;
            //    var ty = data.NodeType;
            //    if (data != null)
            //        return true;
            //}
            return true;

        }
        async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetLAClipStateEventContainer()
        {
            if (mEventGraphNodesContainer == null)
            {
                var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
                var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
                await assist.LoadSubLinks(tempFile);
                var data = new SubNodesContainerData()
                {
                    ID = EventGraphID,
                    Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_EventGraph" + ":" + EventGraphID.ToString(),
                };
                mEventGraphNodesContainer = await assist.GetSubNodesContainer(data);
                if (data.IsCreated)
                {
                    CreateEventGraphDefaultNodes();
                }
            }
            return mEventGraphNodesContainer;
        }
        private void EventGraph_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var assist = mEventGraphNodesContainer.HostControl;
            assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            var cdvCP = new CodeDomNode.CenterDataValueControl.CenterDataValueControlConstructParam()
            {
                CSType = CSParam.CSType,
                ConstructParam = "",
            };
            cdvCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CenterDataValueControl), "CenterDataValue", cdvCP, "");
        }
        #endregion EventGraph

        #region CustomLAPose
        private void Button_EditLANode_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var noUse = OpenCustomLAPoseGraph();
        }
        async System.Threading.Tasks.Task OpenCustomLAPoseGraph()
        {
            var assist = this.HostNodesContainer.HostControl;
            var data = new SubNodesContainerData()
            {
                ID = CustomLAPoseGraphID,
                Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + CustomLAPoseGraphID.ToString(),
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                CreateCustomLAPoseGraphDefaultNodes();
            }
            mLinkedNodesContainer.HostNode = this;
            mLinkedNodesContainer.OnFilterContextMenu = CustomLAClip_FilterContextMenu;
            mLinkedNodesContainer.OnCheckDropAvailable = CustomLAPoseGraphCheckDropAvailable;
        }
        async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetLAClipCustomContainer()
        {
            if (mLinkedNodesContainer == null)
            {
                var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
                var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
                await assist.LoadSubLinks(tempFile);
                var data = new SubNodesContainerData()
                {
                    ID = CustomLAPoseGraphID,
                    Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + CustomLAPoseGraphID.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (data.IsCreated)
                {
                    CreateCustomLAPoseGraphDefaultNodes();
                }
            }
            return mLinkedNodesContainer;
        }
        void CreateCustomLAPoseGraphDefaultNodes()
        {
            var csParam = new LAFinalPoseControlConstructionParams()
            {
                CSType = CSParam.CSType,
                NodeName = "FinalPose",
                HostNodesContainer = mLinkedNodesContainer,
                ConstructParam = "",
            };
            var node = mLinkedNodesContainer.AddOrigionNode(typeof(LAFinalPoseControl), csParam, 600, 300) as LAFinalPoseControl;
            node.IsDeleteable = false;
        }
        bool CustomLAPoseGraphCheckDropAvailable(DragEventArgs e)
        {
            if (EditorCommon.Program.ResourcItemDragType.Equals(EditorCommon.DragDrop.DragDropManager.Instance.DragType))
            {
                var dragObject = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList;
                var data = dragObject[0] as EditorCommon.CodeGenerateSystem.INodeListAttribute;
                var ty = data.NodeType;
                if (data != null)
                    return true;
            }
            return true;

        }
        private void CustomLAClip_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var ctrlAssist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            List<string> cachedPosesName = new List<string>();
            foreach (var ctrl in ctrlAssist.NodesControl.CtrlNodeList)
            {
                if (ctrl is CachedAnimPoseControl)
                {
                    if (!cachedPosesName.Contains(ctrl.NodeName))
                        cachedPosesName.Add(ctrl.NodeName);
                }
            }
            foreach (var sub in ctrlAssist.SubNodesContainers)
            {
                foreach (var ctrl in sub.Value.CtrlNodeList)
                {
                    if (ctrl is CachedAnimPoseControl)
                    {
                        if (!cachedPosesName.Contains(ctrl.NodeName))
                            cachedPosesName.Add(ctrl.NodeName);
                    }
                }
            }
            var assist = mLinkedNodesContainer.HostControl;
            assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
            //nodesList.ClearNodes();
            {
                var cp = new CodeDomNode.Animation.LAAdditivePoseControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAAdditivePoseControl), "BlendPose/ApplyAdditivePose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAMakeAdditivePoseControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAMakeAdditivePoseControl), "BlendPose/MakeAdditivePose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByBoolControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByBoolControl), "BlendPose/SelectPoseByBool", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByIntControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByIntControl), "BlendPose/SelectPoseByInt", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByEnumControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByEnumControl), "BlendPose/SelectPoseByEnum", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABlendPoseControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABlendPoseControl), "BlendPose/BlendPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABlendPosePerBoneControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABlendPosePerBoneControl), "BlendPose/LayeredBlendPerBone", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAMaskPoseControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAMaskPoseControl), "BlendPose/MaskPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAZeroPoseControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAZeroPoseControl), "Pose/ZeroPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABindedPoseControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABindedPoseControl), "Pose/TPose", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASelectPoseByBoolControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASelectPoseByBoolControl), "BlendPose/SelectPoseByBool", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LACCDIKControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LACCDIKControl), "SkeletonBoneControl/CCDIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAFABRIKControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAFABRIKControl), "SkeletonBoneControl/FABRIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LALookAtControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LALookAtControl), "SkeletonBoneControl/LookAt", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LALegIKControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LALegIKControl), "SkeletonBoneControl/LegIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LASplineIKControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LASplineIKControl), "SkeletonBoneControl/SplineIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LATwoBoneIKControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LATwoBoneIKControl), "SkeletonBoneControl/TwoBoneIK", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LABoneDrivenControllerControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LABoneDrivenControllerControl), "SkeletonBoneControl/BoneDrivenController", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAHandIKRetargetingControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAHandIKRetargetingControl), "SkeletonBoneControl/HandIKRetargeting", cp, "");
            }
            {
                var cp = new CodeDomNode.Animation.LAResetRootTransformControlConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    SkeletonAsset = this.SkeletonAsset,
                };
                nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.LAResetRootTransformControl), "SkeletonBoneControl/ResetRootTransform", cp, "");
            }
            foreach (var cachedPoseName in cachedPosesName)
            {
                var getCachedPose = new GetCachedAnimPoseConstructionParams()
                {
                    CSType = HostNodesContainer.CSType,
                    ConstructParam = "",
                    NodeName = "CachedPose_" + cachedPoseName,
                };
                nodesList.AddNodesFromType(filterData, typeof(GetCachedAnimPoseControl), "CachedAnimPose/" + getCachedPose.NodeName, getCachedPose, "");
            }
            var cdvCP = new CodeDomNode.CenterDataValueControl.CenterDataValueControlConstructParam()
            {
                CSType = HostNodesContainer.CSType,
                ConstructParam = "",
            };
            cdvCP.CenterDataWarpper.CenterDataName = this.CenterDataWarpper.CenterDataName;
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.CenterDataValueControl), "CenterDataValue", cdvCP, "");
        }
        #endregion CustomLAPose
        #region GCode_CodeDom
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LogicClipNode";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAClipNodeControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var validName = StringRegex.GetValidName(NodeName);
            return new CodeVariableReferenceExpression(validName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLAClipStateEvent(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LAStatesBridge lAStatesBridge)
        {
            if (mEventGraphNodesContainer == null)
            {
                var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
                var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
                await assist.LoadSubLinks(tempFile);
                var data = new SubNodesContainerData()
                {
                    ID = EventGraphID,
                    Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_EventGraph" + ":" + EventGraphID.ToString(),
                };
                mEventGraphNodesContainer = await assist.GetSubNodesContainer(data);
                if (data.IsCreated)
                {
                    CreateEventGraphDefaultNodes();
                }
            }
            foreach (var ctrl in mEventGraphNodesContainer.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            var constructLAGraphMethod = lAStatesBridge.ConstructLAGraphMethod;
            var validName = StringRegex.GetValidName(NodeName);
            var laStateName = "LAState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + validName;
            foreach (var ctrl in mEventGraphNodesContainer.CtrlNodeList)
            {
                if ((ctrl is MethodCustom))
                {
                    var methodCtrl = ctrl as MethodCustom;
                    await methodCtrl.GCode_CodeDom_GenerateCode(codeClass, null, context);
                    var initLambaAssign = new CodeAssignStatement();
                    initLambaAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(laStateName), methodCtrl.GetMethodInfo().DisplayName);
                    initLambaAssign.Right = new CodeVariableReferenceExpression(methodCtrl.GetMethodInfo().MethodName);
                    constructLAGraphMethod.Statements.Add(initLambaAssign);
                }
            }
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLAClipCustom(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LAStatesBridge lAStatesBridge)
        {
            if (mLinkedNodesContainer == null)
            {
                var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
                var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
                await assist.LoadSubLinks(tempFile);
                var data = new SubNodesContainerData()
                {
                    ID = CustomLAPoseGraphID,
                    Title = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + CustomLAPoseGraphID.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (data.IsCreated)
                {
                    CreateCustomLAPoseGraphDefaultNodes();
                }
            }
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            var constructLAGraphMethod = lAStatesBridge.ConstructLAGraphMethod;
            var validName = StringRegex.GetValidName(NodeName);
            var laStateName = "LAState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + validName;
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                if ((ctrl is CodeDomNode.Animation.LAFinalPoseControl))
                {
                    var laFinalPoseCtrl = ctrl as LAFinalPoseControl;
                    var initMethod = new CodeMemberMethod();

                    initMethod.Name = laStateName + "_InitCustom_Lambda";
                    initMethod.Attributes = MemberAttributes.Public;
                    var param = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationState)), "state");
                    initMethod.Parameters.Add(param);
                    codeClass.Members.Add(initMethod);
                    var methodContext = new GenerateCodeContext_Method(context, initMethod);
                    await laFinalPoseCtrl.GCode_CodeDom_GenerateCode_GenerateBlendTree(codeClass, initMethod.Statements, null, methodContext);
                    var initLambaAssign = new CodeAssignStatement();
                    initLambaAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(laStateName), "InitializeAction");
                    initLambaAssign.Right = new CodeVariableReferenceExpression(initMethod.Name);
                    constructLAGraphMethod.Statements.Add(initLambaAssign);
                }
            }
            if (DefaultState)
            {
                var setDefaultStateMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SetDefaultState"), new CodeExpression[] { new CodeVariableReferenceExpression(laStateName), new CodePrimitiveExpression(lAStatesBridge.LAStateMachineName) });
                constructLAGraphMethod.Statements.Add(setDefaultStateMethodInvoke);
            }
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLAStates(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LAStatesBridge lAStatesBridge)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var constructLAGraphMethod = lAStatesBridge.ConstructLAGraphMethod;

            //createLAStateWithAnimClip
            //LogicAnimationState "LAState_" + NodeName =  CreateLAStateWithAnimClip(string name,string animFilePath);
            var validName = StringRegex.GetValidName(NodeName);
            var laStateName = "LAState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + validName;
            var createLAStateWithAnimClipMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateLAStateWithAnimClip"), new CodeExpression[] { new CodePrimitiveExpression(NodeName), new CodePrimitiveExpression(Duration), new CodePrimitiveExpression(AnimationFilePathString), new CodePrimitiveExpression(lAStatesBridge.LAStateMachineName) });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(typeof(LogicAnimationState), laStateName, createLAStateWithAnimClipMethodInvoke);
            constructLAGraphMethod.Statements.Add(stateVarDeclaration);
            if (RefAnimationFilePathString != null && RefAnimationFilePathString != "" && RefAnimationFilePathString != "null")
            {
                var refAnimAssign = new CodeAssignStatement();
                refAnimAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(laStateName), "RefAnimation");
                refAnimAssign.Right = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(EngineNS.RName)), "GetRName", new CodeExpression[] { new CodePrimitiveExpression(RefAnimationFilePathString) });
                constructLAGraphMethod.Statements.Add(refAnimAssign);
            }
            if (!Repeat)
            {
                CodeFieldReferenceExpression wrapModeField = new CodeFieldReferenceExpression();
                wrapModeField.FieldName = "WrapMode";
                wrapModeField.TargetObject = new CodeVariableReferenceExpression(laStateName);
                CodeAssignStatement wrapModeAssign = new CodeAssignStatement();
                wrapModeAssign.Left = wrapModeField;
                //var w = EngineNS.Bricks.FSM.TFSM.TFSMClockWrapMode.TFSMWM_Clamp;
                //wrapModeAssign.Right = new CodePrimitiveExpression(w);
                wrapModeAssign.Right = new CodeSnippetExpression((typeof(EngineNS.Bricks.FSM.TFSM.TFSMClockWrapMode).FullName + "." + (EngineNS.Bricks.FSM.TFSM.TFSMClockWrapMode.TFSMWM_Clamp).ToString()));
                constructLAGraphMethod.Statements.Add(wrapModeAssign);
            }

            if (Animation != null && Animation != EngineNS.RName.EmptyName)
            {
                var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(Animation.Address + ".rinfo", null);
                if (info != null)
                {
                    var animationInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
                    Notifies.Clear();
                    foreach (var pair in animationInfo.NotifyTrackMap)
                    {
                        Notifies.Add(pair.NotifyName);
                    }
                    for (int i = 0; i < Notifies.Count; ++i)
                    {
                        var notify = Notifies[i];
                        var validNotifyName = StringRegex.GetValidName(notify);
                        validNotifyName = "Anim_Notify_" + validNotifyName;
                        if (hasTheNotifyMethod(codeClass, validNotifyName))
                        {
                            var attachNotifyEventExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(laStateName), "AnimationClip"), "AttachNotifyEvent"), new CodeExpression[] { new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
                            //CodeArrayIndexerExpression arrayIndex = new CodeArrayIndexerExpression(new CodeFieldReferenceExpression(animRef, "Notifies"), new CodePrimitiveExpression(i));
                            //CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(arrayIndex, "OnNotify", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName));
                            constructLAGraphMethod.Statements.Add(attachNotifyEventExp);
                        }
                    }
                }
            }
            lAStatesBridge.LAStatesDic.Add(laStateName, new CodeVariableReferenceExpression(laStateName));
            return;

        }
        string GetCategoryItemNameHierarchical(Macross.CategoryItem item, string name = "")
        {
            var temp = name + "_" + item.Name;
            if (item.Parent == null)
                return temp;
            return GetCategoryItemNameHierarchical(item.Parent, temp);
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLATransitions(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LAStatesBridge lAStatesBridge)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var constructLAGraphMethod = lAStatesBridge.ConstructLAGraphMethod;
            var laFromStateName = "LAState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + NodeName;
            var fromStateRef = lAStatesBridge.GetLAState(laFromStateName);
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                var laTranstionCtrl = TransitionNodes[i] as LATransitionNodeControl;
                var start = laTranstionCtrl.Start;
                var duration = laTranstionCtrl.Duration;
                var transitionWhenFinish = laTranstionCtrl.TransitionWhenFinish;
                var linkInfos = TransitionNodes[i].GetLinkPinInfos();
                for (int j = 0; j < linkInfos.Length; ++j)
                {
                    for (int k = 0; k < linkInfos[j].GetLinkInfosCount(); ++k)
                    {
                        lAStatesBridge.LATransitionMethods.Clear();
                        lAStatesBridge.LATransitionExecuteMethods.Clear();
                        var info = linkInfos[j].GetLinkInfo(k);
                        var title = GetTransitionName(info);
                        var finalResultNode = lAStatesBridge.GetLATransitionFinalResultNode(title) as LAFinalTransitionResultControl;
                        var codeExp = await finalResultNode.GCode_CodeDom_GenerateTransitionLambdaMethod(codeClass
                            , element, new GenerateCodeContext_Method(context, null));
                        lAStatesBridge.LATransitionMethods.Push(codeExp);
                        var executeNode = lAStatesBridge.GetLATransitionExecuteNode(title) as MethodCustom;
                        bool isLATransitionExecuteMethodExist = false;
                        foreach (var member in codeClass.Members)
                        {
                            if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                            {
                                var existMethod = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                                if (existMethod.Name == executeNode.GetMethodInfo().MethodName)
                                    isLATransitionExecuteMethodExist = true;
                            }
                        }
                        if (!isLATransitionExecuteMethodExist)
                            await executeNode.GCode_CodeDom_GenerateCode(codeClass, element, context);
                        lAStatesBridge.LATransitionExecuteMethods.Push(executeNode.GetMethodInfo().MethodName);
                        var toNode = info.m_linkToObjectInfo.HostNodeControl;
                        bool performanceFirst = false;
                        float fadeTime = 0.1f;
                        if (laTranstionCtrl.TransitionCrossfadeDic.ContainsKey(toNode.Id))
                        {
                            performanceFirst = laTranstionCtrl.TransitionCrossfadeDic[toNode.Id].PerformanceFirst;
                            fadeTime = laTranstionCtrl.TransitionCrossfadeDic[toNode.Id].FadeTime;
                        }
                        await GCode_CodeDom_GenerateCode_GenerateLATransitionsRecursion(toNode, start, duration, transitionWhenFinish, performanceFirst, fadeTime, fromStateRef, laFromStateName, constructLAGraphMethod, codeClass, element, context, lAStatesBridge);
                    }
                }
            }
        }
        string GetTransitionName(LinkInfo info)
        {
            string from, to;
            if (info.m_linkFromObjectInfo.HostNodeControl is LAGraphNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LAGraphNodeControl;
                from = gNode.LinkedCategoryItemID.ToString();
            }
            else if (info.m_linkFromObjectInfo.HostNodeControl is LATransitionNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LATransitionNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
            }
            else
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LAClipNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
                System.Diagnostics.Debug.Assert(false);
            }
            if (info.m_linkToObjectInfo.HostNodeControl is LAGraphNodeControl)
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LAGraphNodeControl;
                to = gNode.LinkedCategoryItemID.ToString();
            }
            else
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LAClipNodeControl;
                to = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkToObjectInfo.HostNodeControl.NodeName;
            }
            return from + "__To__ " + to;
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLATransitionsRecursion(BaseNodeControl ctrl, float start, float duration, bool transitionWhenFinish, bool performanceFirst, float fadeTime, CodeVariableReferenceExpression fromStateRef, string laFromStateName, CodeMemberMethod constructLAGraphMethod, CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LAStatesBridge lAStatesBridge)
        {
            if (ctrl is LAClipNodeControl)
            {
                var toClipNode = ctrl as LAClipNodeControl;
                var laToStateName = "LAState_" + toClipNode.HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + toClipNode.NodeName;
                //if (laFromStateName != laToStateName)
                {
                    var toStateRef = lAStatesBridge.GetLAState(laToStateName);
                    var createLATimedTransitionFunctionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateLATimedTransitionFunction"), new CodeExpression[] { new CodePrimitiveExpression(start), new CodePrimitiveExpression(duration), fromStateRef, toStateRef, new CodePrimitiveExpression(performanceFirst), new CodePrimitiveExpression(fadeTime) });
                    var transitionName = laFromStateName + "_To_" + laToStateName;
                    CodeVariableDeclarationStatement transitionVarDeclaration = new CodeVariableDeclarationStatement(typeof(LATimedTransitionFunction), transitionName, createLATimedTransitionFunctionMethodInvoke);
                    constructLAGraphMethod.Statements.Add(transitionVarDeclaration);

                    CodeFieldReferenceExpression transitionOnFinishField = new CodeFieldReferenceExpression();
                    transitionOnFinishField.FieldName = "TransitionOnFinish";
                    transitionOnFinishField.TargetObject = new CodeVariableReferenceExpression(transitionName);
                    CodeAssignStatement transitionOnFinishAssign = new CodeAssignStatement();
                    transitionOnFinishAssign.Left = transitionOnFinishField;
                    transitionOnFinishAssign.Right = new CodePrimitiveExpression(transitionWhenFinish);

                    constructLAGraphMethod.Statements.Add(transitionOnFinishAssign);

                    using (var it = lAStatesBridge.LATransitionMethods.GetEnumerator())
                    {
                        while (it.MoveNext())
                        {
                            var transitionMethod = it.Current;
                            CodeMethodReferenceExpression addTransitionConditionMethodRef = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(transitionName), "AddTransitionCondition");
                            var addTransitionConditionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addTransitionConditionMethodRef, new CodeExpression[] { new CodeMethodReferenceExpression(null, transitionMethod.Name) });
                            constructLAGraphMethod.Statements.Add(addTransitionConditionMethodInvoke);
                        }
                    }
                    using (var it = lAStatesBridge.LATransitionExecuteMethods.GetEnumerator())
                    {
                        while (it.MoveNext())
                        {
                            var transitionMethod = it.Current;
                            CodeMethodReferenceExpression addTransitionConditionMethodRef = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(transitionName), "AddTransitionExecuteAction");
                            var addTransitionConditionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addTransitionConditionMethodRef, new CodeExpression[] { new CodeMethodReferenceExpression(null, transitionMethod) });
                            constructLAGraphMethod.Statements.Add(addTransitionConditionMethodInvoke);
                        }
                    }
                    var stateRef = lAStatesBridge.GetLAState(laFromStateName);
                    CodeMethodReferenceExpression addTransitionMethodRef = new CodeMethodReferenceExpression(stateRef, "AddTransition");
                    var addTransitionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addTransitionMethodRef, new CodeExpression[] { new CodeVariableReferenceExpression(transitionName) });
                    constructLAGraphMethod.Statements.Add(addTransitionMethodInvoke);
                }
            }
            if (ctrl is LAGraphNodeControl)
            {
                var toGraphNode = ctrl as LAGraphNodeControl;
                var nodeContainer = lAStatesBridge.GetLAStateNodesContainer(toGraphNode.NodeName);
                for (int m = 0; m < nodeContainer.CtrlNodeList.Count; ++m)
                {
                    if (nodeContainer.CtrlNodeList[m] is LAGraphNodeControl)
                    {
                        var subGraphNode = nodeContainer.CtrlNodeList[m] as LAGraphNodeControl;
                        if (subGraphNode.IsSelfGraphNode)
                        {
                            var subObjects = subGraphNode.BottomValueLinkHandle.GetLinkInfosCount();
                            for (int k = 0; k < subGraphNode.BottomValueLinkHandle.GetLinkInfosCount(); ++k)
                            {
                                var info = subGraphNode.BottomValueLinkHandle.GetLinkInfo(k);
                                var title = GetTransitionName(info);
                                var finalResultNode = lAStatesBridge.GetLATransitionFinalResultNode(title) as LAFinalTransitionResultControl;
                                var codeExp = await finalResultNode.GCode_CodeDom_GenerateTransitionLambdaMethod(codeClass
                                    , element, new GenerateCodeContext_Method(context, null));
                                lAStatesBridge.LATransitionMethods.Push(codeExp);
                                var executeNode = lAStatesBridge.GetLATransitionExecuteNode(title) as MethodCustom;
                                bool isLATransitionExecuteMethodExist = false;
                                foreach (var member in codeClass.Members)
                                {
                                    if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                                    {
                                        var existMethod = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                                        if (existMethod.Name == executeNode.GetMethodInfo().MethodName)
                                            isLATransitionExecuteMethodExist = true;
                                    }
                                }
                                if (!isLATransitionExecuteMethodExist)
                                    await executeNode.GCode_CodeDom_GenerateCode(codeClass, element, context);
                                lAStatesBridge.LATransitionExecuteMethods.Push(executeNode.GetMethodInfo().MethodName);

                                var toNode = info.m_linkToObjectInfo.HostNodeControl;
                                //performanceFirst = subGraphNode.TransitionCrossfadeDic[toNode.Id].PerformanceFirst;
                                //fadeTime = subGraphNode.TransitionCrossfadeDic[toNode.Id].FadeTime;
                                await GCode_CodeDom_GenerateCode_GenerateLATransitionsRecursion(toNode, start, duration, transitionWhenFinish, performanceFirst, fadeTime, fromStateRef, laFromStateName, constructLAGraphMethod, codeClass, element, context, lAStatesBridge);
                                lAStatesBridge.LATransitionMethods.Pop();
                                lAStatesBridge.LATransitionExecuteMethods.Pop();
                            }
                        }
                    }
                }
            }
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            /*生成代码
           EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence batmanidle = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            EngineNS.RName animName;
            animName = EngineNS.RName.GetRName("titandemo/character/batman/animation/batman@idle.vanims");
            batmanidle.Init(animName);
            batmanidle.AnimationPose = animPose;
            state.AddTickComponent(batmanidle);
            batmanidle.Notifies[0].OnNotify += this.Anim_Notify_Walk;
            return true;
             */
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            CodeMemberMethod constructLAGraphMethod = null;
            foreach (var member in codeClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeMemberMethod;
                    if (method.Name == "ConstructLAGraph")
                        constructLAGraphMethod = method;
                }
            }
            if (constructLAGraphMethod == null)
            {
                constructLAGraphMethod = new CodeMemberMethod();
                constructLAGraphMethod.Name = "ConstructLAGraph";
                constructLAGraphMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                codeClass.Members.Add(constructLAGraphMethod);
            }

            //createLAStateWithAnimClip
            //CreateLAStateWithAnimClip(string name,string animFilePath);
            var validName = StringRegex.GetValidName(NodeName);
            var createLAStateWithAnimClipMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateLAStateWithAnimClip"), new CodeExpression[] { new CodeVariableReferenceExpression(NodeName), new CodeVariableReferenceExpression("AnimationFilePath") });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(typeof(LogicAnimationState), "LAState_" + NodeName, createLAStateWithAnimClipMethodInvoke);
            constructLAGraphMethod.Statements.Add(stateVarDeclaration);
            return;
            System.CodeDom.CodeVariableDeclarationStatement st = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimNode.AnimationClip)), validName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimNode.AnimationClip))));
            codeStatementCollection.Add(st);

            //var calcMethod = new CodeMethodInvokeExpression(new CodeSnippetExpression("EngineNS.RName"), "GetRName", new CodePrimitiveExpression(RNameNodeName.Name));
            //CodeAssignStatement nodeNameAssign = new CodeAssignStatement();

            //CodeVariableDeclarationStatement rNameSt = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.RName)), "animName");
            //nodeNameAssign.Left = new CodeVariableReferenceExpression("animName");
            //nodeNameAssign.Right = calcMethod;

            //codeStatementCollection.Add(rNameSt);
            //codeStatementCollection.Add(nodeNameAssign);

            CodeVariableReferenceExpression animRef = new CodeVariableReferenceExpression(validName);
            CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression(animRef, "Init");
            var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, new CodeExpression[] { new CodeVariableReferenceExpression("animName") });

            codeStatementCollection.Add(methodInvoke);
            var animCP = CSParam as AnimationAssetConstructionParams;
            CodeFieldReferenceExpression isLoopField = new CodeFieldReferenceExpression();
            isLoopField.FieldName = "IsLoop";
            isLoopField.TargetObject = animRef;
            CodeAssignStatement isLoopAssign = new CodeAssignStatement();
            isLoopAssign.Left = isLoopField;
            isLoopAssign.Right = new CodePrimitiveExpression((bool)mTemplateClassInstance.GetType().GetProperty("IsLoop").GetValue(mTemplateClassInstance));
            codeStatementCollection.Add(isLoopAssign);


            CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression();
            animPoseField.FieldName = "AnimationPoseProxy";
            animPoseField.TargetObject = animRef;

            CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            animPoseAssign.Left = animPoseField;
            animPoseAssign.Right = context.AnimAssetAnimPoseProxyReferenceExpression;
            codeStatementCollection.Add(animPoseAssign);

            CodeMethodReferenceExpression addAnimTickComponentRef = new CodeMethodReferenceExpression(context.AnimAssetTickHostReferenceExpression, "AddTickComponent");
            var addAnimTickComponentInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addAnimTickComponentRef, new CodeExpression[] { animRef });
            codeStatementCollection.Add(addAnimTickComponentInvoke);

            //var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(RNameNodeName.Address + ".rinfo", null);
            //var animationInfo = info as EditorCommon.ResourceInfos.AnimationSequenceResourceInfo;
            //animCP.Notifies.Clear();
            //foreach (var pair in animationInfo.NotifyTrackMap)
            //{
            //    animCP.Notifies.Add(pair.NotifyName);
            //}
            for (int i = 0; i < Notifies.Count; ++i)
            {
                var notify = Notifies[i];
                var validNotifyName = StringRegex.GetValidName(notify);
                validNotifyName = "Anim_Notify_" + validNotifyName;
                if (hasTheNotifyMethod(codeClass, validNotifyName))
                {
                    var attachNotifyEventExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(animRef, "AttachNotifyEvent"), new CodeExpression[] { new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
                    //CodeArrayIndexerExpression arrayIndex = new CodeArrayIndexerExpression(new CodeFieldReferenceExpression(animRef, "Notifies"), new CodePrimitiveExpression(i));
                    //CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(arrayIndex, "OnNotify", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName));
                    codeStatementCollection.Add(attachNotifyEventExp);
                }
            }
        }

        bool hasTheNotifyMethod(CodeTypeDeclaration codeClass, string name)
        {
            foreach (var member in codeClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == name)
                        return true;
                }
            }
            return false;
        }
        #endregion
    }
}
