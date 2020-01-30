using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace CodeDomNode.Animation
{
    [EngineNS.Rtti.MetaClass]
    public class TransitionCrossfade : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public bool PerformanceFirst { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public float FadeTime { get; set; } = 0.1f;
    }
    public class LinkNodeTransitionCrossfadeShow : BaseNodeControl
    {
        [Browsable(false)]
        public LAGraphNodeControl LAGraphNodeControl { get; set; } = null;
        [Browsable(false)]
        public Guid TransitionID { get; set; } = Guid.Empty;
        #region property

        public bool PerformanceFirst
        {
            get { return (bool)GetValue(PerformanceFirstProperty); }
            set
            {
                SetValue(PerformanceFirstProperty, value);
                LAGraphNodeControl.TransitionCrossfadeDic[TransitionID].PerformanceFirst = value;
            }
        }
        public static readonly DependencyProperty PerformanceFirstProperty = DependencyProperty.Register("PerformanceFirst", typeof(bool), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(false, OnPerformanceFirstPropertyChanged));
        private static void OnPerformanceFirstPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.PerformanceFirst = (bool)e.NewValue;
        }
        public float FadeTime
        {
            get { return (float)GetValue(FadeTimeProperty); }
            set
            {
                SetValue(FadeTimeProperty, value);
                LAGraphNodeControl.TransitionCrossfadeDic[TransitionID].FadeTime = value;
            }
        }
        public static readonly DependencyProperty FadeTimeProperty = DependencyProperty.Register("FadeTime", typeof(float), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(0.1f, OnFadeTimePropertyChanged));
        private static void OnFadeTimePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.FadeTime = (float)e.NewValue;
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            BindingTemplateClassInstanceProperties();
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "PerformanceFirst", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "FadeTime", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            var clsType = mTemplateClassInstance.GetType();
            var performanceFirstPro = clsType.GetProperty("PerformanceFirst");
            performanceFirstPro.SetValue(mTemplateClassInstance, PerformanceFirst);
            SetBinding(TransitionCrossfadeShow.PerformanceFirstProperty, new Binding("PerformanceFirst") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var fadeTimePro = clsType.GetProperty("FadeTime");
            fadeTimePro.SetValue(mTemplateClassInstance, FadeTime);
            SetBinding(TransitionCrossfadeShow.FadeTimeProperty, new Binding("FadeTime") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion property
        public LinkNodeTransitionCrossfadeShow()
        {

        }
        public LinkNodeTransitionCrossfadeShow(ConstructionParams csParams, LAGraphNodeControl graphNodeControl, Guid transitionID) : base(csParams)
        {
            LAGraphNodeControl = graphNodeControl;
            TransitionID = transitionID;

            PerformanceFirst = LAGraphNodeControl.TransitionCrossfadeDic[transitionID].PerformanceFirst;
            FadeTime = LAGraphNodeControl.TransitionCrossfadeDic[transitionID].FadeTime;

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {

        }
    }
    public class TransitionCrossfadeShow : BaseNodeControl
    {
        [Browsable(false)]
        public LATransitionNodeControl LATransitionNodeControl { get; set; } = null;
        [Browsable(false)]
        public Guid TransitionID { get; set; } = Guid.Empty;
        #region property
        public bool TransitionWhenFinish
        {
            get { return (bool)GetValue(TransitionWhenFinishProperty); }
            set
            {
                SetValue(TransitionWhenFinishProperty, value);
                LATransitionNodeControl.TransitionWhenFinish = value;
            }
        }
        public static readonly DependencyProperty TransitionWhenFinishProperty = DependencyProperty.Register("TransitionWhenFinish", typeof(bool), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(false, OnTransitionWhenFinishPropertyChanged));
        private static void OnTransitionWhenFinishPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
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
                LATransitionNodeControl.Start = value;
            }
        }
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start", typeof(float), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(0.0f, OnStartPropertyChanged));
        private static void OnStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Start = (float)e.NewValue;
        }
        public float Duration
        {
            get { return (float)GetValue(DurationProperty); }
            set
            {
                SetValue(DurationProperty, value);
                LATransitionNodeControl.Duration = value;
            }
        }
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(float), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(1.0f, OnDurationPropertyChanged));
        private static void OnDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Duration = (float)e.NewValue;
        }
        public bool PerformanceFirst
        {
            get { return (bool)GetValue(PerformanceFirstProperty); }
            set
            {
                SetValue(PerformanceFirstProperty, value);
                LATransitionNodeControl.TransitionCrossfadeDic[TransitionID].PerformanceFirst = value;
            }
        }
        public static readonly DependencyProperty PerformanceFirstProperty = DependencyProperty.Register("PerformanceFirst", typeof(bool), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(false, OnPerformanceFirstPropertyChanged));
        private static void OnPerformanceFirstPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
            if (e.NewValue == e.OldValue)
                return;
            ctrl.PerformanceFirst = (bool)e.NewValue;
        }
        public float FadeTime
        {
            get { return (float)GetValue(FadeTimeProperty); }
            set
            {
                SetValue(FadeTimeProperty, value);
                LATransitionNodeControl.TransitionCrossfadeDic[TransitionID].FadeTime = value;
            }
        }
        public static readonly DependencyProperty FadeTimeProperty = DependencyProperty.Register("FadeTime", typeof(float), typeof(TransitionCrossfadeShow), new UIPropertyMetadata(0.1f, OnFadeTimePropertyChanged));
        private static void OnFadeTimePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as TransitionCrossfadeShow;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.FadeTime = (float)e.NewValue;
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            BindingTemplateClassInstanceProperties();
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Start", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Duration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "TransitionWhenFinish", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "PerformanceFirst", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "FadeTime", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            var clsType = mTemplateClassInstance.GetType();
            var startPro = clsType.GetProperty("Start");
            startPro.SetValue(mTemplateClassInstance, Start);
            SetBinding(TransitionCrossfadeShow.StartProperty, new Binding("Start") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var durationPro = clsType.GetProperty("Duration");
            durationPro.SetValue(mTemplateClassInstance, Duration);
            SetBinding(TransitionCrossfadeShow.DurationProperty, new Binding("Duration") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var transitionWhenFinishPro = clsType.GetProperty("TransitionWhenFinish");
            transitionWhenFinishPro.SetValue(mTemplateClassInstance, TransitionWhenFinish);
            SetBinding(TransitionCrossfadeShow.TransitionWhenFinishProperty, new Binding("TransitionWhenFinish") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var performanceFirstPro = clsType.GetProperty("PerformanceFirst");
            performanceFirstPro.SetValue(mTemplateClassInstance, PerformanceFirst);
            SetBinding(TransitionCrossfadeShow.PerformanceFirstProperty, new Binding("PerformanceFirst") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            var fadeTimePro = clsType.GetProperty("FadeTime");
            fadeTimePro.SetValue(mTemplateClassInstance, FadeTime);
            SetBinding(TransitionCrossfadeShow.FadeTimeProperty, new Binding("FadeTime") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion property
        public TransitionCrossfadeShow()
        {

        }
        public TransitionCrossfadeShow(ConstructionParams csParams, LATransitionNodeControl transitionNodeControl, Guid transitionID) : base(csParams)
        {
            LATransitionNodeControl = transitionNodeControl;
            TransitionID = transitionID;
            Start = LATransitionNodeControl.Start;
            Duration = LATransitionNodeControl.Duration;
            TransitionWhenFinish = LATransitionNodeControl.TransitionWhenFinish;
            PerformanceFirst = LATransitionNodeControl.TransitionCrossfadeDic[transitionID].PerformanceFirst;
            FadeTime = LATransitionNodeControl.TransitionCrossfadeDic[transitionID].FadeTime;

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {

        }
    }
    public class StringRegex
    {
        public static string GetValidName(string name)
        {
            return Regex.Replace(name, "[ \\[ \\] \\^ \\-*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
        }
    }
    public class Helper
    {
        public static async System.Threading.Tasks.Task<CodeExpression> GetEvaluateValueExpression(CodeTypeDeclaration codeClass, GenerateCodeContext_Method valueEvaluateMethodContex, CodeMemberMethod valueEvaluateMethod, LinkPinControl linkHandle, object defaultValue)
        {
            CodeExpression valueExpression = null;
            var valueLinkObj = linkHandle.GetLinkedObject(0, true);
            if (valueLinkObj == null)
            {
                valueExpression = new CodePrimitiveExpression(defaultValue);
            }
            else
            {
                var valueLinkElm = linkHandle.GetLinkedPinControl(0, true);
                if (!valueLinkObj.IsOnlyReturnValue)
                    await valueLinkObj.GCode_CodeDom_GenerateCode(codeClass, valueEvaluateMethod.Statements, valueLinkElm, valueEvaluateMethodContex);
                valueExpression = valueLinkObj.GCode_CodeDom_GetValue(valueLinkElm, valueEvaluateMethodContex);
            }
            return valueExpression;
        }
        public static async System.Threading.Tasks.Task<CodeMemberMethod> CreateEvaluateMethod(CodeTypeDeclaration codeClass, string methodName, Type returnType, object defaultValue, LinkPinControl linkHandle, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var value = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, linkHandle, defaultValue);
            valueEvaluateMethod.ReturnType = new CodeTypeReference(returnType);
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(value));
            codeClass.Members.Add(valueEvaluateMethod);

            return valueEvaluateMethod;
        }
    }
}
