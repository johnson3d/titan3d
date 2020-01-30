using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CodeDomNode.Animation
{
    public class LATransitionNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public double Width { get; set; } = 40;
        [EngineNS.Rtti.MetaData]
        public double Height { get; set; } = 9;
        [EngineNS.Rtti.MetaData]
        public float Start { get; set; } = 0;
        [EngineNS.Rtti.MetaData]
        public float Duration { get; set; } = 1;
        [EngineNS.Rtti.MetaData]
        public float WidthScale { get; set; } = 1;
        [EngineNS.Rtti.MetaData]
        public bool TransitionWhenFinish { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        [EngineNS.Rtti.MetaData]
        [EngineNS.IO.Serializer.CustomFieldSerializer(typeof(TransitionCrossfadeSerializer))]
        public Dictionary<Guid, TransitionCrossfade> TransitionCrossfadeDic { get; set; } = new Dictionary<Guid, TransitionCrossfade>();
        private class TransitionCrossfadeSerializer : EngineNS.IO.Serializer.CustomSerializer
        {
            public override object ReadValue(EngineNS.IO.Serializer.IReader pkg)
            {
                Dictionary<Guid, TransitionCrossfade> dict = new Dictionary<Guid, TransitionCrossfade>();
                int count;
                pkg.Read(out count);
                for (int i = 0; i < count; ++i)
                {
                    Guid id;
                    TransitionCrossfade cf = new TransitionCrossfade();
                    pkg.Read(out id);
                    pkg.Read(cf);
                    dict.Add(id, cf);
                }
                return dict;
            }
            public override void WriteValue(object obj, EngineNS.IO.Serializer.IWriter pkg)
            {
                var dict = obj as Dictionary<Guid, TransitionCrossfade>;
                pkg.Write(dict.Count);
                using (var it = dict.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        pkg.Write(it.Current.Key);
                        pkg.Write(it.Current.Value);
                    }
                }
            }
        }
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LATransitionNodeControlConstructionParams;
            retVal.Width = Width;
            retVal.Height = Height;
            retVal.Duration = Duration;
            retVal.WidthScale = WidthScale;
            retVal.TransitionWhenFinish = TransitionWhenFinish;
            retVal.LinkedCategoryItemID = LinkedCategoryItemID;
            retVal.TransitionCrossfadeDic = TransitionCrossfadeDic;
            return retVal;
        }
    }

    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LATransitionNodeControlConstructionParams))]
    public partial class LATransitionNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        bool mIsSelected = false;
        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsDeleteable = value;
                if (value == false)
                {
                    BackBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC80BAB5"));
                }
                if (value == true)
                {
                    BackBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC588C88"));
                }
            }
        }
        public Dictionary<Guid, TransitionCrossfade> TransitionCrossfadeDic { get; set; } = new Dictionary<Guid, TransitionCrossfade>();
        public LAClipNodeControl HostLAClipNodeControl { get => ParentNode as LAClipNodeControl; }
        public LACTransition LACTransition { get; set; } = null;
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl CtrlValueLinkHandle
        {
            get => mCtrlValueLinkHandle;
            set => mCtrlValueLinkHandle = value;
        }
        public EngineNS.RName RNameNodeName;
        public List<string> Notifies = new List<string>();
        partial void InitConstruction();

        #region DP
        public bool TransitionWhenFinish
        {
            get { return (bool)GetValue(TransitionWhenFinishProperty); }
            set
            {
                SetValue(TransitionWhenFinishProperty, value);
                var para = CSParam as LATransitionNodeControlConstructionParams;
                para.TransitionWhenFinish = value;
            }
        }
        public static readonly DependencyProperty TransitionWhenFinishProperty = DependencyProperty.Register("TransitionWhenFinish", typeof(bool), typeof(LATransitionNodeControl), new UIPropertyMetadata(false, OnTransitionWhenFinishPropertyChanged));
        private static void OnTransitionWhenFinishPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LATransitionNodeControl;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.TransitionWhenFinish = (bool)e.NewValue;
        }
        public float Start
        {
            get { return (float)GetValue(StartProperty); }
            set
            {
                SetValue(StartProperty, value);
                var para = CSParam as LATransitionNodeControlConstructionParams;
                para.Start = value;
                SetStartTime(value);
            }
        }
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start", typeof(float), typeof(LATransitionNodeControl), new UIPropertyMetadata(0.0f, OnStartPropertyChanged));
        private static void OnStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LATransitionNodeControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Start = (float)e.NewValue;
        }
        public float WidthScale
        {
            get { return (float)GetValue(WidthScaleProperty); }
            set
            {
                SetValue(WidthScaleProperty, value);
                var para = CSParam as LATransitionNodeControlConstructionParams;
                para.WidthScale = value;
                Width = TimeLength.GetWidthByTime(Duration, value);
                para.Width = Width;
            }
        }
        public static readonly DependencyProperty WidthScaleProperty = DependencyProperty.Register("WidthScale", typeof(float), typeof(LATransitionNodeControl), new UIPropertyMetadata(1.0f, OnWidthScalePropertyChanged));
        private static void OnWidthScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LATransitionNodeControl;
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
                var para = CSParam as LATransitionNodeControlConstructionParams;
                para.Duration = value;
                Width = TimeLength.GetWidthByTime(value, WidthScale);
                para.Width = Width;
            }
        }
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(float), typeof(LATransitionNodeControl), new UIPropertyMetadata(1.0f, OnDurationPropertyChanged));
        private static void OnDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LATransitionNodeControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Duration = (float)e.NewValue;
        }
        #endregion

        #region ShowProperty

        public object GetTransitionCrossfadeShowPropertyObject(Guid id)
        {
            if (!TransitionCrossfadeDic.ContainsKey(id))
            {
                TransitionCrossfadeDic.Add(id, new TransitionCrossfade());

            }
            var csParams = new ConstructionParams();
            csParams.CSType = CSParam.CSType;
            var tfs = new TransitionCrossfadeShow(csParams, this, id);
            return tfs.GetShowPropertyObject();
        }

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Start", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Duration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "TransitionWhenFinish", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            var clsType = mTemplateClassInstance.GetType();
            var startPro = clsType.GetProperty("Start");
            startPro.SetValue(mTemplateClassInstance, Start);
            SetBinding(LATransitionNodeControl.StartProperty, new Binding("Start") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var durationPro = clsType.GetProperty("Duration");
            durationPro.SetValue(mTemplateClassInstance, Duration);
            SetBinding(LATransitionNodeControl.DurationProperty, new Binding("Duration") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var transitionWhenFinishPro = clsType.GetProperty("TransitionWhenFinish");
            transitionWhenFinishPro.SetValue(mTemplateClassInstance, TransitionWhenFinish);
            SetBinding(LATransitionNodeControl.TransitionWhenFinishProperty, new Binding("TransitionWhenFinish") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });

        }
        #endregion
        public LATransitionNodeControl(LATransitionNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            Start = csParam.Start;
            Duration = csParam.Duration;
            WidthScale = csParam.WidthScale;
            TransitionWhenFinish = csParam.TransitionWhenFinish;
            LinkedCategoryItemID = csParam.LinkedCategoryItemID;
            TransitionCrossfadeDic = csParam.TransitionCrossfadeDic;
            Width = TimeLength.GetWidthByTime(Duration, WidthScale);
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("TransitionLinkHandle", mCtrlValueLinkHandle, null);
        }
        public override bool CanDuplicate()
        {
            return false;
        }
        public void SetStartTime(float startTime)
        {
            if (LACTransition == null)
                return;
            if (startTime < 0 || startTime > LACTransition.HostLACTransitionArea.HostLAClipNodeControl.Duration)
                return;
            var offset = TimeLength.GetWidthByTime(startTime, WidthScale);
            var pos = LACTransition.Position;
            pos.X = offset;
            LACTransition.HostLACTransitionArea.ForceMove(pos, LACTransition);
        }

        #region Move
        bool mIsLButtonDown = false;
        protected override void DragObj_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mIsLButtonDown = true;
            //base.DragObj_MouseLeftButtonDown(sender, e);
            Mouse.Capture(this);
            mDeltaPt = e.GetPosition(ParentDrawCanvas);
            m_bDragMove = true;
            e.Handled = true;
        }
        protected override void DragObj_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mIsLButtonDown = false;
            m_bDragMove = false;
            SelectedNodeIngoreParent();
            Mouse.Capture(null);
            e.Handled = true;
            //base.DragObj_MouseLeftButtonUp(sender, e);
        }
        protected override void DragObj_MouseMove(object sender, MouseEventArgs e)
        {
            if (mIsLButtonDown && m_bDragMove)
            {
                _OnMoveNode(this);
                DragMove(e);
                mDeltaPt.X = e.GetPosition(ParentDrawCanvas).X;
                e.Handled = true;
            }
        }
        protected override void StartDrag(UIElement dragObj, MouseButtonEventArgs e)
        {
            base.StartDrag(dragObj, e);
            mDeltaPt = e.GetPosition(ParentDrawCanvas);
        }
        protected override void DragMove(MouseEventArgs e)
        {
            var oldPos = GetLocation();
            Point pt = e.GetPosition(ParentDrawCanvas);
            Point newPos = new Point(pt.X - mDeltaPt.X + oldPos.X, pt.Y - mDeltaPt.Y + oldPos.Y);
            if (LACTransition.HostLACTransitionArea.TryMoveX(newPos, LACTransition))
            {
                newPos = GetLocation();
                newPos.Y = pt.Y - mDeltaPt.Y + oldPos.Y;
                Start = TimeLength.GeTimeByWidth(LACTransition.Position.X, WidthScale);
                _OnMoveNode(this);
                UpdateLink();
                _OnDragMoveNode(this, e);
            }
            if(LACTransition.HostLACTransitionArea.TryMoveY(newPos, LACTransition))
            {
                newPos = GetLocation();
                _OnMoveNode(this);
                UpdateLink();
                _OnDragMoveNode(this, e);
            }
            if (LACTransition.HostLACTransitionArea.TryMoveBoth(newPos, LACTransition))
            {
                Start = TimeLength.GeTimeByWidth(LACTransition.Position.X, WidthScale);
                _OnMoveNode(this);
                UpdateLink();
                _OnDragMoveNode(this, e);
            }
        }
        #endregion Move
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TransitionLinkHandle", CodeGenerateSystem.Base.enLinkType.LAState, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

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
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
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
            var validName = StringRegex.GetValidName(NodeName);
            System.CodeDom.CodeVariableDeclarationStatement st = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimNode.AnimationClip)), validName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimNode.AnimationClip))));
            codeStatementCollection.Add(st);

            var calcMethod = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeSnippetExpression("EngineNS.RName"), "GetRName", new CodePrimitiveExpression(RNameNodeName.Name));
            CodeAssignStatement nodeNameAssign = new CodeAssignStatement();

            CodeVariableDeclarationStatement rNameSt = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.RName)), "animName");
            nodeNameAssign.Left = new CodeVariableReferenceExpression("animName");
            nodeNameAssign.Right = calcMethod;

            codeStatementCollection.Add(rNameSt);
            codeStatementCollection.Add(nodeNameAssign);

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

            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(RNameNodeName.Address + ".rinfo", null);
            var animationInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
            animCP.Notifies.Clear();
            foreach (var pair in animationInfo.NotifyTrackMap)
            {
                animCP.Notifies.Add(pair.NotifyName);
            }
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
    }
}
