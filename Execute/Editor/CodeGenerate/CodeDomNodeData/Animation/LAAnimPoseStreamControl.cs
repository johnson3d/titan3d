using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using CodeGenerateSystem.Base;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using EngineNS.Bricks.Animation.AnimStateMachine;

namespace CodeDomNode.Animation
{
    public class LAAnimPoseStreamControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "AnimStream";
        [EngineNS.Rtti.MetaData]
        public List<string> Notifies { get; set; }
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName FileName { get; set; } = EngineNS.RName.EmptyName;
        public Action<EngineNS.RName> OnAdded = null;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAAnimPoseStreamControlConstructionParams;
            retVal.FileName = FileName;
            retVal.NodeName = NodeName;
            retVal.Notifies = Notifies;
            retVal.OnAdded = OnAdded;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAAnimPoseStreamControlConstructionParams))]
    public partial class LAAnimPoseStreamControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public Action<EngineNS.RName> OnAdded = null;
        public ImageSource TitleImage
        {
            get { return (ImageSource)GetValue(TitleImageProperty); }
            set { SetValue(TitleImageProperty, value); }
        }
        public static readonly DependencyProperty TitleImageProperty = DependencyProperty.Register("TitleImage", typeof(ImageSource), typeof(LAAnimPoseStreamControl), new FrameworkPropertyMetadata(null));

        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public List<string> Notifies = new List<string>();

        partial void InitConstruction();

        #region DP
        public EngineNS.RName FileName { get; set; } = EngineNS.RName.EmptyName;
        //public bool DefaultState
        //{
        //    get { return (bool)GetValue(DefaultStateProperty); }
        //    set
        //    {
        //        SetValue(DefaultStateProperty, value);
        //        var para = CSParam as LAClipNodeControlConstructionParams;
        //        para.DefaultState = value;
        //    }
        //}
        //public static readonly DependencyProperty DefaultStateProperty = DependencyProperty.Register("DefaultState", typeof(bool), typeof(LAClipNodeControl), new UIPropertyMetadata(false, DefaultStatePropertyChanged));
        //private static void DefaultStatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = sender as LAClipNodeControl;
        //    if (e.NewValue == e.OldValue)
        //        return;
        //    ctrl.DefaultState = (bool)e.NewValue;
        //}

        //public float WidthScale
        //{
        //    get { return (float)GetValue(WidthScaleProperty); }
        //    set
        //    {
        //        SetValue(WidthScaleProperty, value);
        //        var para = CSParam as LAClipNodeControlConstructionParams;
        //        para.WidthScale = value;
        //        Width = TimeLength.GetWidthByTime(Duration, value) + ExtraWidth;
        //    }
        //}
        //public static readonly DependencyProperty WidthScaleProperty = DependencyProperty.Register("WidthScale", typeof(float), typeof(LAClipNodeControl), new UIPropertyMetadata(1.0f, OnWidthScalePropertyChanged));
        //private static void OnWidthScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = sender as LAClipNodeControl;
        //    if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
        //        return;
        //    ctrl.WidthScale = (float)e.NewValue;
        //}
        //public float Duration
        //{
        //    get { return (float)GetValue(DurationProperty); }
        //    set
        //    {
        //        SetValue(DurationProperty, value);
        //        var para = CSParam as LAClipNodeControlConstructionParams;
        //        para.Duration = value;
        //        Width = TimeLength.GetWidthByTime(value, WidthScale) + ExtraWidth;
        //    }
        //}
        //public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(float), typeof(LAClipNodeControl), new UIPropertyMetadata(1.0f, OnDurationPropertyChanged));
        //private static void OnDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = sender as LAClipNodeControl;
        //    if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
        //        return;
        //    ctrl.Duration = (float)e.NewValue;
        //}
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
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "Repeat", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "DefaultState", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Duration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "WidthScale", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(EngineNS.RName), "AnimationFilePath", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute(), new EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip) }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            //CreateBinding(mTemplateClassInstance, "DefaultState", LAClipNodeControl.DefaultStateProperty, DefaultState);
            //CreateBinding(mTemplateClassInstance, "Duration", LAClipNodeControl.DurationProperty, Duration);
            //CreateBinding(mTemplateClassInstance, "WidthScale", LAClipNodeControl.WidthScaleProperty, WidthScale);
            //CreateBinding(mTemplateClassInstance, "AnimationFilePath", LAClipNodeControl.AnimationFilePathProperty, AnimationFilePath);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion
        public LAAnimPoseStreamControl(LAAnimPoseStreamControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            FileName = csParam.FileName;
            //WidthScale = csParam.WidthScale;
            //AnimationFilePath = csParam.AnimationFilePath;

            //DefaultState = csParam.DefaultState;
            NodeName = csParam.NodeName;
            //Width = TimeLength.GetWidthByTime(Duration, WidthScale) + ExtraWidth;
            OnAdded = csParam.OnAdded;
            BindingTemplateClassInstanceProperties();


            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);

            var clip = EngineNS.Bricks.Animation.AnimNode.AnimationClip.CreateSync(FileName);
            if (clip != null)
                OnAdded?.Invoke(EngineNS.RName.GetRName(clip.GetElementProperty(EngineNS.Bricks.Animation.AnimNode.ElementPropertyType.EPT_Skeleton)));
        }
        void InitializeLinkControl(LAAnimPoseStreamControlConstructionParams csParam)
        {
            mCtrlValueLinkHandle = ValueLinkHandle;
            mCtrlValueLinkHandle.MultiLink = false;

            AddLinkPinInfo("AnimPoseOutHandle", mCtrlValueLinkHandle, null);
            TitleImage = TryFindResource("AnimationNode_AnimationClip_64x") as ImageSource;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimPoseOutHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAAnimPoseStreamControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAAnimPoseStreamControl);
        }
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Type clipType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_AnimationClip);
            var createLAClipMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(clipType), "CreateSync"), new CodeExpression[] { new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(EngineNS.RName)), "GetRName", new CodeExpression[] { new CodePrimitiveExpression(FileName.Name) }) });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(clipType, ValidName, createLAClipMethodInvoke);
            codeStatementCollection.Add(stateVarDeclaration);

            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(FileName.Address + ".rinfo", null);
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
                    var attachNotifyEventExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "AttachNotifyEvent"), new CodeExpression[] { new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
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
