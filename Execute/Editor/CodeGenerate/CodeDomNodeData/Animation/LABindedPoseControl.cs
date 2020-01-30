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
    public class LABindedPoseControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "TPose";
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LABindedPoseControlConstructionParams;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LABindedPoseControlConstructionParams))]
    public partial class LABindedPoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public ImageSource TitleImage
        {
            get { return (ImageSource)GetValue(TitleImageProperty); }
            set { SetValue(TitleImageProperty, value); }
        }
        public static readonly DependencyProperty TitleImageProperty = DependencyProperty.Register("TitleImage", typeof(ImageSource), typeof(LABindedPoseControl), new FrameworkPropertyMetadata(null));
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();

        #region DP
       
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
        public LABindedPoseControl(LABindedPoseControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            BindingTemplateClassInstanceProperties();


            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);

            
        }
        void InitializeLinkControl(LABindedPoseControlConstructionParams csParam)
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
            return "LABindedPoseControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LABindedPoseControl);
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
            Type clipType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_BindedPose);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(clipType, ValidName,new CodeObjectCreateExpression(new CodeTypeReference(clipType)));
            codeStatementCollection.Add(stateVarDeclaration);
        }
    }
}
