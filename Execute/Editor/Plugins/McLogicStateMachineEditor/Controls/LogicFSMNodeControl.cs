using CodeDomNode;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using EngineNS.Bricks.FSM.SFSM;
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

namespace McLogicStateMachineEditor.Controls
{
    public class LFSMStatesBridge
    {
        public CodeMemberMethod ConstructLFSMGraphMethod { get; set; } = null;
        public CodeVariableReferenceExpression LFSMStateMachine { get; set; } = null;
        public string LFSMStateMachineName { get; set; } = "";
        public Dictionary<string, CodeVariableReferenceExpression> LFSMStatesDic { get; set; } = new Dictionary<string, CodeVariableReferenceExpression>();
        public Dictionary<string, NodesContainer> LFSMStateNodesContainerDic { get; set; } = new Dictionary<string, NodesContainer>();
        public Dictionary<string, BaseNodeControl> LFSMTransitionFinalResultNodesDic { get; set; } = new Dictionary<string, BaseNodeControl>();
        public Dictionary<string, BaseNodeControl> LFSMTransitionExecuteNodesDic { get; set; } = new Dictionary<string, BaseNodeControl>();
        public Stack<CodeMemberMethod> LFSMTransitionMethods { get; set; } = new Stack<CodeMemberMethod>();
        public Stack<string> LFSMTransitionExecuteMethods { get; set; } = new Stack<string>();
        public CodeVariableReferenceExpression GetLFSMState(string name)
        {
            if (LFSMStatesDic.ContainsKey(name))
                return LFSMStatesDic[name];
            return null;
        }
        public NodesContainer GetLFSMStateNodesContainer(string name)
        {
            if (LFSMStateNodesContainerDic.ContainsKey(name))
                return LFSMStateNodesContainerDic[name];
            return null;
        }
        public BaseNodeControl GetLFSMTransitionFinalResultNode(string name)
        {
            if (LFSMTransitionFinalResultNodesDic.ContainsKey(name))
                return LFSMTransitionFinalResultNodesDic[name];
            return null;
        }
        public BaseNodeControl GetLFSMTransitionExecuteNode(string name)
        {
            if (LFSMTransitionExecuteNodesDic.ContainsKey(name))
                return LFSMTransitionExecuteNodesDic[name];
            return null;
        }
    }
    [EngineNS.Rtti.MetaClass]
    public class LFSMTransition : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public Guid CtrlID { get; set; }
        LFSMTransitionNodeControl mHostLFSMTNodeCtrl = null;
        public LFSMTransitionNodeControl HostLFSMTNodeCtrl
        {
            get => mHostLFSMTNodeCtrl;
            set
            {
                mHostLFSMTNodeCtrl = value;
                mHostLFSMTNodeCtrl.LFSMTransition = this;
            }
        }
        public LFSMTransition Duplicate(CodeGenerateSystem.Base.BaseNodeControl.DuplicateParam param)
        {
            var copy = new LFSMTransition();
            copy.TrackIndex = TrackIndex;
            var ctrl = HostLFSMTNodeCtrl.Duplicate(param) as LFSMTransitionNodeControl;
            copy.HostLFSMTNodeCtrl = ctrl;
            copy.CtrlID = ctrl.Id;
            copy.Position = Position;
            return copy;
        }
        public void AddToNode(LogicFSMNodeControl node)
        {
            node.AddChildNode(HostLFSMTNodeCtrl);
        }
        public LFSMTransitionArea HostLFSMTransitionArea { get; set; } = null;
        public int TrackIndex { get; set; } = 0;
        public Point Position
        {
            get
            {
                return HostLFSMTNodeCtrl.GetLocation();
            }
            set
            {
                HostLFSMTNodeCtrl.SetLocation(value.X, value.Y);
            }
        }
        public Size Size
        {
            get
            {
                if (HostLFSMTNodeCtrl.ActualWidth == 0 || HostLFSMTNodeCtrl.ActualHeight == 0)
                {
                    if (double.IsNaN(HostLFSMTNodeCtrl.Width) || double.IsNaN(HostLFSMTNodeCtrl.Height))
                        return new Size(0, 0);
                    else
                        return new Size(HostLFSMTNodeCtrl.Width, HostLFSMTNodeCtrl.Height);
                }
                else
                    return new Size(HostLFSMTNodeCtrl.ActualWidth, HostLFSMTNodeCtrl.ActualHeight);
            }
        }
        public Rect Rect
        {
            get
            {
                return new Rect(Position, Size);
            }
        }
    }
    [EngineNS.Rtti.MetaClass]
    public class LFSMTransitionTrack : EngineNS.IO.Serializer.Serializer
    {
        LFSMTransitionArea mHostLFSMTransitionArea = null;
        public LFSMTransitionArea HostLFSMTransitionArea
        {
            get => mHostLFSMTransitionArea;
            set
            {
                mHostLFSMTransitionArea = value;
                for (int i = 0; i < LFSMTransitions.Count; ++i)
                {
                    LFSMTransitions[i].HostLFSMTransitionArea = value;
                }
            }
        }
        [EngineNS.Rtti.MetaData]
        public List<LFSMTransition> LFSMTransitions { get; set; } = new List<LFSMTransition>();
        [EngineNS.Rtti.MetaData]
        public int TrackIndex { get; set; }
        public float Width { get; set; }
        public float Height { get; set; } = 10;
        public bool CanInsert(Point newPos, LFSMTransition transition, out LFSMTransition collisionTransition)
        {
            for (int i = 0; i < LFSMTransitions.Count; ++i)
            {
                if (LFSMTransitions[i] != transition)
                {
                    var rect = new Rect(newPos, transition.Size);
                    if (LFSMTransitions[i].Rect.IntersectsWith(rect))
                    {
                        collisionTransition = LFSMTransitions[i];
                        return false;
                    }
                }
            }
            collisionTransition = null;
            return true;
        }
        public LFSMTransitionTrack Duplicate(CodeGenerateSystem.Base.BaseNodeControl.DuplicateParam param)
        {
            var copy = new LFSMTransitionTrack();
            copy.TrackIndex = TrackIndex;
            copy.Width = Width;
            copy.Height = Height;
            for (int i = 0; i < LFSMTransitions.Count; ++i)
            {
                var trans = LFSMTransitions[i].Duplicate(param);
                copy.LFSMTransitions.Add(trans);
            }
            return copy;
        }
        public void AddToNode(LogicFSMNodeControl node)
        {
            for (int i = 0; i < LFSMTransitions.Count; ++i)
            {
                LFSMTransitions[i].AddToNode(node);
            }
        }
    }
    [EngineNS.Rtti.MetaClass]
    public class LFSMTransitionArea : EngineNS.IO.Serializer.Serializer
    {
        public LogicFSMNodeControl HostLogicFSMNodeControl { get; set; } = null;
        [EngineNS.Rtti.MetaData]
        public float TracksHeight { get; set; } = 10;
        [EngineNS.Rtti.MetaData]
        public List<LFSMTransitionTrack> Tracks { get; set; } = new List<LFSMTransitionTrack>();
        public void ForceMove(Point newPos, LFSMTransition transition)
        {
            var oriTrackIndex = transition.TrackIndex;
            LFSMTransition outTransition = null;
            if (Tracks[oriTrackIndex].CanInsert(newPos, transition,out outTransition))
            {
                transition.Position = newPos;
            }
            else
            {

            }
        }
        void RoundPos(ref Point point, LFSMTransition transition)
        {
            if (point.X < 0)
                point.X = 0;
            if (point.Y < 0)
                point.Y = 0;
            if (point.X + transition.Size.Width > HostLogicFSMNodeControl.TransitionCanvas.Width)
                point.X = HostLogicFSMNodeControl.TransitionCanvas.Width - transition.Size.Width;
            if (point.Y + transition.Size.Height > HostLogicFSMNodeControl.TransitionCanvas.Height)
                point.Y = HostLogicFSMNodeControl.TransitionCanvas.Height - transition.Size.Height;
        }
        public bool TryMoveX(Point newPos, LFSMTransition transition)
        {
            var delta = newPos - transition.Position;
            var oriTrackIndex = transition.TrackIndex;
            var pos = newPos;
            var wantTrackIndex = (int)(pos.Y / TracksHeight);
            var roundPos = new Point(pos.X, oriTrackIndex * TracksHeight);
            RoundPos(ref roundPos, transition);
            var trackIndex = oriTrackIndex;
            var newRoundPos = roundPos;
            LFSMTransition outTransition = null;
            if (Tracks[trackIndex].CanInsert(newRoundPos, transition, out outTransition))
            {
                transition.Position = newRoundPos;
                if (transition.TrackIndex != trackIndex)
                {
                    transition.TrackIndex = trackIndex;
                    Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                    Tracks[trackIndex].LFSMTransitions.Add(transition);
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
                            Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                            Tracks[trackIndex].LFSMTransitions.Add(transition);
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
                            Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                            Tracks[trackIndex].LFSMTransitions.Add(transition);
                        }
                        RefreshTracks();
                        return true;
                    }
                }
                return false;
            }
        }
        public bool TryMoveY(Point newPos, LFSMTransition transition)
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
                var track = new LFSMTransitionTrack() { TrackIndex = Tracks.Count };
                Tracks.Add(track);
                Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                transition.TrackIndex = track.TrackIndex;
                transition.Position = new Point(pos.X, wantTrackIndex * TracksHeight);
                track.LFSMTransitions.Add(transition);
                RefreshTracks();
                return true;
            }
            var trackIndex = (int)(roundPos.Y / TracksHeight);
            var newRoundPos = roundPos;
            LFSMTransition outTransition = null;
            if (Tracks[trackIndex].CanInsert(newRoundPos, transition, out outTransition))
            {
                transition.Position = newRoundPos;
                if (transition.TrackIndex != trackIndex)
                {
                    transition.TrackIndex = trackIndex;
                    Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                    Tracks[trackIndex].LFSMTransitions.Add(transition);
                }
                RefreshTracks();
                return true;
            }
            return false;
        }
        public bool TryMoveBoth(Point newPos, LFSMTransition transition)
        {
            var delta = newPos - transition.Position;
            var oriTrackIndex = transition.TrackIndex;
            var pos = newPos;
            var wantTrackIndex = (int)(pos.Y / TracksHeight);
            var roundPos = new Point(pos.X, wantTrackIndex * TracksHeight);
            RoundPos(ref roundPos, transition);
            if (wantTrackIndex > Tracks.Count - 1)
            {
                var track = new LFSMTransitionTrack() { TrackIndex = Tracks.Count };
                Tracks.Add(track);
                Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                transition.TrackIndex = track.TrackIndex;
                transition.Position = new Point(pos.X, wantTrackIndex * TracksHeight);
                track.LFSMTransitions.Add(transition);
                RefreshTracks();
                return true;
            }

            var trackIndex = (int)(roundPos.Y / TracksHeight);
            var newRoundPos = roundPos;
            LFSMTransition outTransition = null;
            if (Tracks[trackIndex].CanInsert(newRoundPos, transition, out outTransition))
            {
                transition.Position = newRoundPos;
                if (transition.TrackIndex != trackIndex)
                {
                    transition.TrackIndex = trackIndex;
                    Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                    Tracks[trackIndex].LFSMTransitions.Add(transition);
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
                            Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                            Tracks[trackIndex].LFSMTransitions.Add(transition);
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
                            Tracks[oriTrackIndex].LFSMTransitions.Remove(transition);
                            Tracks[trackIndex].LFSMTransitions.Add(transition);
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
                if (Tracks[i].LFSMTransitions.Count == 0)
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
                    for (int j = 0; j < Tracks[i].LFSMTransitions.Count; ++j)
                    {
                        track.LFSMTransitions[j].TrackIndex = track.TrackIndex;
                        var newRoundPos = track.LFSMTransitions[j].Position;
                        newRoundPos.Y = height;
                        track.LFSMTransitions[j].Position = newRoundPos;
                    }
                }
            }
            HostLogicFSMNodeControl.TransitionCanvas.Height = Tracks.Count * TracksHeight;
        }
        public void Insert(LFSMTransition transition)
        {
            bool isInsertInExistTrack = false;
            var trackIndex = (int)(transition.Position.Y / TracksHeight);
            for (int i = trackIndex; i < Tracks.Count; ++i)
            {
                var pos = new Point(transition.Position.X, trackIndex * TracksHeight);
                LFSMTransition outTransition = null;
                if (Tracks[trackIndex].CanInsert(pos, transition, out outTransition))
                {
                    transition.TrackIndex = trackIndex;
                    transition.Position = pos;
                    Tracks[trackIndex].LFSMTransitions.Add(transition);
                    HostLogicFSMNodeControl.TransitionNodes.Add(transition.HostLFSMTNodeCtrl);
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
                var track = new LFSMTransitionTrack() { TrackIndex = Tracks.Count };
                Tracks.Add(track);
                var finalPos = new Point(transition.Position.X, track.TrackIndex * TracksHeight);
                LFSMTransition outTransition = null;
                if (track.CanInsert(finalPos, transition,out outTransition))
                {
                    transition.TrackIndex = track.TrackIndex;
                    transition.Position = finalPos;
                    track.LFSMTransitions.Add(transition);
                }
            }
            if (HostLogicFSMNodeControl.TransitionCanvas.Height != Tracks.Count * TracksHeight)
                HostLogicFSMNodeControl.TransitionCanvas.Height = Tracks.Count * TracksHeight;
        }

        public void Remove(LFSMTransition transition)
        {
            Tracks[transition.TrackIndex].LFSMTransitions.Remove(transition);
            HostLogicFSMNodeControl.TransitionNodes.Remove(transition.HostLFSMTNodeCtrl);
            if (HostLogicFSMNodeControl.TransitionCanvas.Height != Tracks.Count * TracksHeight)
                HostLogicFSMNodeControl.TransitionCanvas.Height = Tracks.Count * TracksHeight;
        }
        public LFSMTransitionArea Duplicate(CodeGenerateSystem.Base.BaseNodeControl.DuplicateParam param)
        {
            var copy = new LFSMTransitionArea();
            copy.TracksHeight = TracksHeight;
            for (int i = 0; i < Tracks.Count; ++i)
            {
                var trans = Tracks[i].Duplicate(param);
                trans.HostLFSMTransitionArea = copy;
                copy.Tracks.Add(trans);
            }
            return copy;
        }
        public void AddToNode(LogicFSMNodeControl node)
        {
            node.LFSMTransitionArea = this;
            for (int i = 0; i < Tracks.Count; ++i)
            {
                Tracks[i].AddToNode(node);
            }
        }
    }
    public class LogicFSMNodeControlConstructionParams : ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public double Width { get; set; } = 200;
        [EngineNS.Rtti.MetaData]
        public double Height { get; set; } = 50;
        [EngineNS.Rtti.MetaData]
        public bool DefaultState { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "LogicClip";
        [EngineNS.Rtti.MetaData]
        public LFSMTransitionArea LFSMTransitionArea { get; set; } = new LFSMTransitionArea();
        [EngineNS.Rtti.MetaData]
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        [EngineNS.Rtti.MetaData]
        public Guid CustomLAPoseGraphID { get; set; } = Guid.NewGuid();
        [EngineNS.Rtti.MetaData]
        public Guid EventGraphID { get; set; } = Guid.NewGuid();
        [EngineNS.Rtti.MetaData]
        public CodeDomNode.AI.CenterDataWarpper CenterDataWarpper { get; set; } = new CodeDomNode.AI.CenterDataWarpper();
        public Action<EngineNS.RName> OnAdded = null;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LogicFSMNodeControlConstructionParams;
            retVal.Width = Width;
            retVal.Height = Height;
            retVal.LFSMTransitionArea = LFSMTransitionArea;
            retVal.DefaultState = DefaultState;
            retVal.OnAdded = OnAdded;
            retVal.LinkedCategoryItemID = LinkedCategoryItemID;
            retVal.CustomLAPoseGraphID = Guid.NewGuid();
            retVal.EventGraphID = Guid.NewGuid();
            retVal.CenterDataWarpper = CenterDataWarpper;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LogicFSMNodeControlConstructionParams))]
    public partial class LogicFSMNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public CodeDomNode.AI.CenterDataWarpper CenterDataWarpper { get; set; } = null;
        public string GetValidClipName(string name)
        {
            if (HostNodesContainer == null)
                return name;
            var tempName = name;
            var nodeList = HostNodesContainer.CtrlNodeList;
            bool repetition = false;
            for (int i = 0; i < nodeList.Count; ++i)
            {
                if (nodeList[i] is LogicFSMNodeControl)
                {
                    var clip = nodeList[i] as LogicFSMNodeControl;
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
                        if (nodeList[j] is LogicFSMNodeControl)
                        {
                            var clip = nodeList[j] as LogicFSMNodeControl;
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
        float ExtraWidth = 10;
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        #region DP
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
                var para = CSParam as LogicFSMNodeControlConstructionParams;
                para.DefaultState = value;
                if (value)
                    DefaultStateVisibility = Visibility.Visible;
                else
                    DefaultStateVisibility = Visibility.Collapsed;
            }
        }
        public static readonly DependencyProperty DefaultStateProperty = DependencyProperty.Register("DefaultState", typeof(bool), typeof(LogicFSMNodeControl), new UIPropertyMetadata(false, DefaultStatePropertyChanged));
        private static void DefaultStatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LogicFSMNodeControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.DefaultState = (bool)e.NewValue;
        }
        public Action<EngineNS.RName> OnAdded = null;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "DefaultState", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Duration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "WidthScale", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            CreateBinding(mTemplateClassInstance, "DefaultState", LogicFSMNodeControl.DefaultStateProperty, DefaultState);
            //CreateBinding(mTemplateClassInstance, "Duration", LogicFSMNodeControl.DurationProperty, Duration);
            //CreateBinding(mTemplateClassInstance, "WidthScale", LogicFSMNodeControl.WidthScaleProperty, WidthScale);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion



        public LogicFSMNodeControl(LogicFSMNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            mChildNodeContainer = TransitionCanvas;
            this.NodeName = GetValidClipName(csParam.NodeName);
            csParam.NodeName = this.NodeName;
            LFSMTransitionArea = csParam.LFSMTransitionArea;
            DefaultState = csParam.DefaultState;
            LinkedCategoryItemID = csParam.LinkedCategoryItemID;
            CustomLAPoseGraphID = csParam.CustomLAPoseGraphID;
            EventGraphID = csParam.EventGraphID;
            CenterDataWarpper = csParam.CenterDataWarpper;
            Width = csParam.Width;
            OnAdded = csParam.OnAdded;
            LFSMTransitionArea.HostLogicFSMNodeControl = this;
            BindingTemplateClassInstanceProperties();
            IsOnlyReturnValue = true;

            AddLinkPinInfo("LogicClipLinkHandle", mCtrlValueLinkHandle, null);

        }
     
        #region Duplicate
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var node = base.Duplicate(param) as LogicFSMNodeControl;
            var csParam = node.CSParam as LogicFSMNodeControlConstructionParams;
            csParam.LinkedCategoryItemID = param.TargetNodesContainer.HostControl.LinkedCategoryItemID;
            var tArea = LFSMTransitionArea.Duplicate(param);
            tArea.AddToNode(node);
            Action action = async () =>
            {
                var customcontainer = await GetLogicGraphContainer();
                var customCopy = customcontainer.Duplicate() as NodesContainerControl;
                customCopy.GUID = node.CustomLAPoseGraphID;
                customCopy.TitleString = HostNodesContainer.TitleString + "/" + this.NodeName + "_Custom" + ":" + node.CustomLAPoseGraphID.ToString();
                param.TargetNodesContainer.HostControl.SubNodesContainers.Add(node.CustomLAPoseGraphID, customCopy);
                customCopy.HostControl = param.TargetNodesContainer.HostControl;
                node.LinkedNodesContainer = customCopy;
                var eventcontainer = await GetEventGraphContainer();
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
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        protected override void OnSizeChanged(double width, double height)
        {
            Width = width;
            var param = CSParam as LogicFSMNodeControlConstructionParams;
            param.Width = width;
            if (LFSMTransitionArea == null)
                return;
            for (int i = 0; i < LFSMTransitionArea.Tracks.Count; ++i)
            {
                for (int j = 0; j < LFSMTransitionArea.Tracks[i].LFSMTransitions.Count; ++j)
                {
                    if (LFSMTransitionArea.Tracks[i].LFSMTransitions[j].HostLFSMTNodeCtrl == null)
                    {
                        LFSMTransitionArea.Tracks[i].LFSMTransitions.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        LFSMTransitionArea.Tracks[i].LFSMTransitions[j].HostLFSMTNodeCtrl.Width = width - ExtraWidth;
                        var p = LFSMTransitionArea.Tracks[i].LFSMTransitions[j].HostLFSMTNodeCtrl.CSParam as LFSMTransitionNodeControlConstructionParams;
                        p.Width = width;
                    }
                }
            }
        }

        public override void ContainerLoadComplete(NodesContainer container)
        {
            
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
        public LFSMTransitionArea LFSMTransitionArea { get; set; } = null;
        public override void OnOpenContextMenu(ContextMenu contextMenu)
        {
            {
                if (TransitionNodes.Count == 0)
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
                }
            }
            {
                var item = new MenuItem()
                {
                    Name = "RomoveTransition_",
                    Header = "RomoveTransition_",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                item.Click += (itemSender, itemE) =>
                {

                };
                //contextMenu.Items.Add(item);
            }
            {
                var item = new MenuItem()
                {
                    Name = "OpenEventGraph",
                    Header = "OpenLogicGraph",
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                item.Click += (itemSender, itemE) =>
                {
                    var noUse = OpenEventGraph();
                };
                contextMenu.Items.Add(item);
            }
            //item = new MenuItem()
            //{
            //    Name = "OpenCustomLAPose",
            //    Header = "OpenLogicGraph",
            //    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            //};
            //item.Click += (itemSender, itemE) =>
            //{
            //    var noUse = OpenLogicGraph();
            //};
            //contextMenu.Items.Add(item);
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
            //node.OnMoveNode += ChildNode_OnMoveNode;
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
        public void AddTransitonNode()
        {
            var canvasWidth = TransitionCanvas.Width;
            var canvasHeight = TransitionCanvas.Height;
            var csParam = new LFSMTransitionNodeControlConstructionParams();
            csParam.CSType = HostNodesContainer.CSType;
            csParam.Width = TransitionCanvas.ActualWidth;
            csParam.LinkedCategoryItemID = LinkedCategoryItemID;

            var ins = System.Activator.CreateInstance(typeof(LFSMTransitionNodeControl), new object[] { csParam }) as LFSMTransitionNodeControl;
            AddChildNode(ins, mChildNodeContainer);
            ins.SetLocation(0, 0);
            ins.HostNodesContainer = HostNodesContainer;
            ins.HostLogicFSMNodeControl = this;
            LFSMTransition lact = new LFSMTransition();
            lact.HostLFSMTNodeCtrl = ins as LFSMTransitionNodeControl;
            lact.HostLFSMTransitionArea = LFSMTransitionArea;
            lact.CtrlID = ins.Id;
            ins.NodeName = this.NodeName;
            LFSMTransitionArea.Insert(lact);
        }
        public void AddTransitionNode(LFSMTransitionNodeControl ctrl)
        {
            AddChildNode(ctrl);
        }
        public void DeleteTransitionNode(LFSMTransitionNodeControl ctrl)
        {
            LFSMTransitionArea.Remove(ctrl.LFSMTransition);
            ctrl.ParentDrawCanvas.Children.Remove(ctrl);
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
            LFSMTransitionArea.HostLogicFSMNodeControl = this;
            for (int i = 0; i < LFSMTransitionArea.Tracks.Count; ++i)
            {
                var track = LFSMTransitionArea.Tracks[i];
                for (int j = 0; j < track.LFSMTransitions.Count; ++j)
                {
                    var transiton = track.LFSMTransitions[j];
                    var id = transiton.CtrlID;
                    var node = GetNodeWithGUID(ref id);
                    if (node == null)
                    {

                    }
                    else
                    {
                        transiton.HostLFSMTNodeCtrl = node as LFSMTransitionNodeControl;
                        transiton.HostLFSMTransitionArea = LFSMTransitionArea;
                        node.NodeName = this.NodeName;
                    }
                }
            }
            TransitionCanvas.Height = TransitionNodes.Count * LFSMTransitionArea.TracksHeight;
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
        async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetEventGraphContainer()
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
        void CreateEventGraphDefaultNodes()
        {
            {
                var param = CSParam as LogicFSMNodeControlConstructionParams;
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
                var param = CSParam as LogicFSMNodeControlConstructionParams;
                var miAssist = new CustomMethodInfo();
                miAssist.MethodName = ValidName + "_OnUpdate";
                miAssist.DisplayName = "OnUpdate";
                //var eTime = new CustomMethodInfo.FunctionParam();
                //eTime.ParamType = new VariableType(typeof(long), param.CSType);
                //eTime.ParamName = "elapseTime";
                //miAssist.InParams.Add(eTime);
                //var context = new CustomMethodInfo.FunctionParam();
                //context.ParamType = new VariableType(typeof(EngineNS.GamePlay.Actor.GCenterData), param.CSType);
                //context.ParamName = "context";
                //context.Attributes.Add(new EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute(CenterDataWarpper.CenterDataType));
                //miAssist.InParams.Add(context);
                var nodeType = typeof(CodeDomNode.MethodCustom);
                var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
                {
                    CSType = param.CSType,
                    HostNodesContainer = mEventGraphNodesContainer,
                    ConstructParam = "",
                    IsShowProperty = false,
                    MethodInfo = miAssist,
                };
                var node = mEventGraphNodesContainer.AddOrigionNode(nodeType, csParam, 0, 250);
                node.IsDeleteable = false;
                node.NodeNameAddShowNodeName = false;
            }
            {
                var param = CSParam as LogicFSMNodeControlConstructionParams;
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
            var noUse = OpenLogicGraph();
        }
        async System.Threading.Tasks.Task OpenLogicGraph()
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
                CreateLogicGraphDefaultNodes();
            }
            mLinkedNodesContainer.HostNode = this;
            mLinkedNodesContainer.OnFilterContextMenu = CustomLFSMClip_FilterContextMenu;
            mLinkedNodesContainer.OnCheckDropAvailable = CustomLFSMGraphCheckDropAvailable;
        }
        async System.Threading.Tasks.Task<CodeGenerateSystem.Controls.NodesContainerControl> GetLogicGraphContainer()
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
                    CreateLogicGraphDefaultNodes();
                }
            }
            return mLinkedNodesContainer;
        }
        void CreateLogicGraphDefaultNodes()
        {
            var param = CSParam as LogicFSMNodeControlConstructionParams;
            var miAssist = new CustomMethodInfo();
            miAssist.MethodName = "CustomTask_" + ValidName;
            miAssist.DisplayName = "CustomTask_" + NodeName;
            var eTime = new CustomMethodInfo.FunctionParam();
            eTime.ParamType = new VariableType(typeof(long), param.CSType);
            eTime.ParamName = "elapseTime";
            miAssist.InParams.Add(eTime);
            var context = new CustomMethodInfo.FunctionParam();
            context.ParamType = new VariableType(typeof(EngineNS.GamePlay.Actor.GCenterData), param.CSType);
            context.ParamName = "context";
            context.Attributes.Add(new EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute(CenterDataWarpper.CenterDataType));
            miAssist.InParams.Add(context);
            //var returnState = new CustomMethodInfo.FunctionParam();
            //returnState.ParamType = new VariableType(typeof(EngineNS.Bricks.AI.BehaviorTree.BehaviorStatus), param.CSType);
            //returnState.ParamName = "Return";
            //miAssist.OutParams.Add(returnState);
            var nodeType = typeof(CodeDomNode.MethodCustom);
            var csParam = new CodeDomNode.MethodCustom.MethodCustomConstructParam()
            {
                CSType = param.CSType,
                HostNodesContainer = mLinkedNodesContainer,
                ConstructParam = "",
                IsShowProperty = false,
                MethodInfo = miAssist,
            };
            var node = mLinkedNodesContainer.AddOrigionNode(nodeType, csParam, 0, 100);
            node.IsDeleteable = false;
            node.NodeNameAddShowNodeName = false;

            //var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
            //{
            //    CSType = param.CSType,
            //    HostNodesContainer = mLinkedNodesContainer,
            //    ConstructParam = "",
            //    ShowPropertyType = ReturnCustom.ReturnCustomConstructParam.enShowPropertyType.ReturnValue,
            //    MethodInfo = miAssist,
            //};
            //var retNode = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.ReturnCustom), retCSParam, 500, 100) as CodeDomNode.ReturnCustom;
            //retNode.IsDeleteable = false;


            mLinkedNodesContainer.HostNode = this;
        }
        bool CustomLFSMGraphCheckDropAvailable(DragEventArgs e)
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
        private void CustomLFSMClip_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var ctrlAssist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            var assist = mLinkedNodesContainer.HostControl;
            assist.NodesControl_FilterContextMenu(contextMenu, filterData);
            var nodesList = contextMenu.GetNodesList();
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
            return typeof(LogicFSMNodeControl);
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
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLAClipStateEvent(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LFSMStatesBridge lAStatesBridge)
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
            var constructLAGraphMethod = lAStatesBridge.ConstructLFSMGraphMethod;
            var validName = StringRegex.GetValidName(NodeName);
            var laStateName = "LFSMState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + validName;
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
            if (DefaultState)
            {
                var setDefaultStateMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "SetDefaultState"), new CodeExpression[] { new CodeVariableReferenceExpression(laStateName), new CodePrimitiveExpression(lAStatesBridge.LFSMStateMachineName) });
                constructLAGraphMethod.Statements.Add(setDefaultStateMethodInvoke);
            }
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLogicGraphCustom(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LFSMStatesBridge lAStatesBridge)
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
                    CreateLogicGraphDefaultNodes();
                }
            }
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            var constructLAGraphMethod = lAStatesBridge.ConstructLFSMGraphMethod;
            var validName = StringRegex.GetValidName(NodeName);
            var laStateName = "LFSMState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + validName;
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                System.Diagnostics.Debug.Assert(false);
                if ((ctrl is MethodCustom))
                {
                    //var laFinalPoseCtrl = ctrl as MethodCustom;
                    //var initMethod = new CodeMemberMethod();

                    //initMethod.Name = laStateName + "_InitCustom_Lambda";
                    //initMethod.Attributes = MemberAttributes.Public;
                    //var param = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationState)), "state");
                    //initMethod.Parameters.Add(param);
                    //codeClass.Members.Add(initMethod);
                    //var methodContext = new GenerateCodeContext_Method(context, initMethod);
                    //await laFinalPoseCtrl.GCode_CodeDom_GenerateCode_GenerateBlendTree(codeClass, initMethod.Statements, null, methodContext);
                    //var initLambaAssign = new CodeAssignStatement();
                    //initLambaAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(laStateName), "InitializeAction");
                    //initLambaAssign.Right = new CodeVariableReferenceExpression(initMethod.Name);
                    //constructLAGraphMethod.Statements.Add(initLambaAssign);
                }
            }

        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLAStates(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LFSMStatesBridge lAStatesBridge)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();

            var constructLAGraphMethod = lAStatesBridge.ConstructLFSMGraphMethod;

            //createLAStateWithAnimClip
            //LogicAnimationState "LFSMState_" + NodeName =  CreateLAStateWithAnimClip(string name,string animFilePath);
            var validName = StringRegex.GetValidName(NodeName);
            var laStateName = "LFSMState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + validName;
            var createLAStateWithAnimClipMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateState"), new CodeExpression[] { new CodePrimitiveExpression(NodeName) });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(typeof(StackBasedState), laStateName, createLAStateWithAnimClipMethodInvoke);
            constructLAGraphMethod.Statements.Add(stateVarDeclaration);
            lAStatesBridge.LFSMStatesDic.Add(laStateName, new CodeVariableReferenceExpression(laStateName));
            return;

        }
        string GetCategoryItemNameHierarchical(Macross.CategoryItem item, string name = "")
        {
            var temp = name + "_" + item.Name;
            if (item.Parent == null)
                return temp;
            return GetCategoryItemNameHierarchical(item.Parent, temp);
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLATransitions(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LFSMStatesBridge lAStatesBridge)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var constructLAGraphMethod = lAStatesBridge.ConstructLFSMGraphMethod;
            var laFromStateName = "LFSMState_" + HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + NodeName;
            var fromStateRef = lAStatesBridge.GetLFSMState(laFromStateName);
            for (int i = 0; i < TransitionNodes.Count; ++i)
            {
                var laTranstionCtrl = TransitionNodes[i] as LFSMTransitionNodeControl;
                var linkInfos = TransitionNodes[i].GetLinkPinInfos();
                for (int j = 0; j < linkInfos.Length; ++j)
                {
                    for (int k = 0; k < linkInfos[j].GetLinkInfosCount(); ++k)
                    {
                        lAStatesBridge.LFSMTransitionMethods.Clear();
                        var info = linkInfos[j].GetLinkInfo(k);
                        var title = GetTransitionName(info);
                        var finalResultNode = lAStatesBridge.GetLFSMTransitionFinalResultNode(title) as LFSMFinalTransitionResultControl;
                        var codeExp = await finalResultNode.GCode_CodeDom_GenerateTransitionLambdaMethod(codeClass
                            , element, new GenerateCodeContext_Method(context, null));
                        lAStatesBridge.LFSMTransitionMethods.Push(codeExp);
                        var executeNode = lAStatesBridge.GetLFSMTransitionExecuteNode(title) as MethodCustom;
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
                        lAStatesBridge.LFSMTransitionExecuteMethods.Push(executeNode.GetMethodInfo().MethodName);
                        var toNode = info.m_linkToObjectInfo.HostNodeControl;
                        bool performanceFirst = false;
                        float fadeTime = 0.1f;
                        await GCode_CodeDom_GenerateCode_GenerateLATransitionsRecursion(toNode, 0, 0, false, performanceFirst, fadeTime, fromStateRef, laFromStateName, constructLAGraphMethod, codeClass, element, context, lAStatesBridge);
                    }
                }
            }
        }
        string GetTransitionName(LinkInfo info)
        {
            string from, to;
            if (info.m_linkFromObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                from = gNode.LinkedCategoryItemID.ToString();
            }
            else if (info.m_linkFromObjectInfo.HostNodeControl is LFSMTransitionNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LFSMTransitionNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
            }
            else
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LogicFSMNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
                System.Diagnostics.Debug.Assert(false);
            }
            if (info.m_linkToObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                to = gNode.LinkedCategoryItemID.ToString();
            }
            else
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LogicFSMNodeControl;
                to = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkToObjectInfo.HostNodeControl.NodeName;
            }
            return from + "__To__ " + to;
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode_GenerateLATransitionsRecursion(BaseNodeControl ctrl, float start, float duration, bool transitionWhenFinish, bool performanceFirst, float fadeTime, CodeVariableReferenceExpression fromStateRef, string laFromStateName, CodeMemberMethod constructLAGraphMethod, CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, LFSMStatesBridge lAStatesBridge)
        {
            if (ctrl is LogicFSMNodeControl)
            {
                var toClipNode = ctrl as LogicFSMNodeControl;
                var laToStateName = "LFSMState_" + toClipNode.HostNodesContainer.HostControl.LinkedCategoryItemName + "_" + toClipNode.NodeName;
                if (laFromStateName != laToStateName)
                {
                    var toStateRef = lAStatesBridge.GetLFSMState(laToStateName);
                    var createLATimedTransitionFunctionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateTransitionFunction"), new CodeExpression[] { fromStateRef, toStateRef });
                    var transitionName = laFromStateName + "_To_" + laToStateName;
                    CodeVariableDeclarationStatement transitionVarDeclaration = new CodeVariableDeclarationStatement(typeof(StateBasedStateTransitionFunction), transitionName, createLATimedTransitionFunctionMethodInvoke);
                    constructLAGraphMethod.Statements.Add(transitionVarDeclaration);
                    using (var it = lAStatesBridge.LFSMTransitionMethods.GetEnumerator())
                    {
                        while (it.MoveNext())
                        {
                            var transitionMethod = it.Current;
                            CodeMethodReferenceExpression addTransitionConditionMethodRef = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(transitionName), "AddTransitionCondition");
                            var addTransitionConditionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addTransitionConditionMethodRef, new CodeExpression[] { new CodeMethodReferenceExpression(null, transitionMethod.Name) });
                            constructLAGraphMethod.Statements.Add(addTransitionConditionMethodInvoke);
                        }
                    }
                    using (var it = lAStatesBridge.LFSMTransitionExecuteMethods.GetEnumerator())
                    {
                        while (it.MoveNext())
                        {
                            var transitionMethod = it.Current;
                            CodeMethodReferenceExpression addTransitionConditionMethodRef = new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(transitionName), "AddTransitionExecuteAction");
                            var addTransitionConditionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addTransitionConditionMethodRef, new CodeExpression[] { new CodeMethodReferenceExpression(null, transitionMethod) });
                            constructLAGraphMethod.Statements.Add(addTransitionConditionMethodInvoke);
                        }
                    }
                    var stateRef = lAStatesBridge.GetLFSMState(laFromStateName);
                    CodeMethodReferenceExpression addTransitionMethodRef = new CodeMethodReferenceExpression(stateRef, "AddTransition");
                    var addTransitionMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addTransitionMethodRef, new CodeExpression[] { new CodeVariableReferenceExpression(transitionName) });
                    constructLAGraphMethod.Statements.Add(addTransitionMethodInvoke);
                }
            }
            if (ctrl is LogicFSMGraphNodeControl)
            {
                var toGraphNode = ctrl as LogicFSMGraphNodeControl;
                var nodeContainer = lAStatesBridge.GetLFSMStateNodesContainer(toGraphNode.NodeName);
                for (int m = 0; m < nodeContainer.CtrlNodeList.Count; ++m)
                {
                    if (nodeContainer.CtrlNodeList[m] is LogicFSMGraphNodeControl)
                    {
                        var subGraphNode = nodeContainer.CtrlNodeList[m] as LogicFSMGraphNodeControl;
                        if (subGraphNode.IsSelfGraphNode)
                        {
                            var subObjects = subGraphNode.BottomValueLinkHandle.GetLinkInfosCount();
                            for (int k = 0; k < subGraphNode.BottomValueLinkHandle.GetLinkInfosCount(); ++k)
                            {
                                var info = subGraphNode.BottomValueLinkHandle.GetLinkInfo(k);
                                var title = GetTransitionName(info);
                                var finalResultNode = lAStatesBridge.GetLFSMTransitionFinalResultNode(title) as LFSMFinalTransitionResultControl;
                                var codeExp = await finalResultNode.GCode_CodeDom_GenerateTransitionLambdaMethod(codeClass
                                    , element, new GenerateCodeContext_Method(context, null));
                                lAStatesBridge.LFSMTransitionMethods.Push(codeExp);
                                var executeNode = lAStatesBridge.GetLFSMTransitionExecuteNode(title) as MethodCustom;
                                bool isLATransitionExecuteMethodExist = false;
                                foreach (var member in codeClass.Members)
                                {
                                    if (member is CodeMemberMethod)
                                    {
                                        var existMethod = member as CodeMemberMethod;
                                        if (existMethod.Name == executeNode.GetMethodInfo().MethodName)
                                            isLATransitionExecuteMethodExist = true;
                                    }
                                }
                                if (!isLATransitionExecuteMethodExist)
                                    await executeNode.GCode_CodeDom_GenerateCode(codeClass, element, context);
                                lAStatesBridge.LFSMTransitionExecuteMethods.Push(executeNode.GetMethodInfo().MethodName);

                                var toNode = info.m_linkToObjectInfo.HostNodeControl;
                                //performanceFirst = subGraphNode.TransitionCrossfadeDic[toNode.Id].PerformanceFirst;
                                //fadeTime = subGraphNode.TransitionCrossfadeDic[toNode.Id].FadeTime;
                                await GCode_CodeDom_GenerateCode_GenerateLATransitionsRecursion(toNode, start, duration, transitionWhenFinish, performanceFirst, fadeTime, fromStateRef, laFromStateName, constructLAGraphMethod, codeClass, element, context, lAStatesBridge);
                                lAStatesBridge.LFSMTransitionMethods.Pop();
                                lAStatesBridge.LFSMTransitionExecuteMethods.Pop();
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
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == "ConstructLAGraph")
                        constructLAGraphMethod = method;
                }
            }
            if (constructLAGraphMethod == null)
            {
                constructLAGraphMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                constructLAGraphMethod.Name = "ConstructLAGraph";
                constructLAGraphMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                codeClass.Members.Add(constructLAGraphMethod);
            }

            //createLAStateWithAnimClip
            //CreateLAStateWithAnimClip(string name,string animFilePath);
            var validName = StringRegex.GetValidName(NodeName);
            var createLAStateWithAnimClipMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateLAStateWithAnimClip"), new CodeExpression[] { new CodeVariableReferenceExpression(NodeName), new CodeVariableReferenceExpression("AnimationFilePath") });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(typeof(LogicAnimationState), "LFSMState_" + NodeName, createLAStateWithAnimClipMethodInvoke);
            constructLAGraphMethod.Statements.Add(stateVarDeclaration);
            return;
        }
        #endregion
    }
}
