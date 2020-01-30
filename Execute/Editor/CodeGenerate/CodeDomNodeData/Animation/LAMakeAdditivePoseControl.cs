using CodeGenerateSystem.Base;
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

namespace CodeDomNode.Animation
{
    public class LAMakeAdditivePoseControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public float Alpha { get; set; } = 1.0f;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "MakeAdditivePose";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAMakeAdditivePoseControlConstructionParams;

            retVal.Alpha = Alpha;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAMakeAdditivePoseControlConstructionParams))]
    public partial class LAMakeAdditivePoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {

        public List<string> Notifies = new List<string>();

        partial void InitConstruction();

        #region DP

        public float Alpha
        {
            get { return (float)GetValue(AlphaProperty); }
            set
            {
                SetValue(AlphaProperty, value);
                var para = CSParam as LAMakeAdditivePoseControlConstructionParams;
                para.Alpha = value;
            }
        }
        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(float), typeof(LAMakeAdditivePoseControl), new UIPropertyMetadata(1.0f, OnAlphaPropertyyChanged));
        private static void OnAlphaPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAMakeAdditivePoseControl;
            if (float.IsNaN((float)e.NewValue) || (e.NewValue == e.OldValue))
                return;
            ctrl.Alpha = (float)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Alpha", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);


            CreateBinding(mTemplateClassInstance, "Alpha", LAMakeAdditivePoseControl.AlphaProperty, Alpha);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LAMakeAdditivePoseControl(LAMakeAdditivePoseControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            Alpha = csParam.Alpha;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }
        #region InitializeLinkControl
        CodeGenerateSystem.Base.LinkPinControl mRefLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mAdditiveLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LAMakeAdditivePoseControlConstructionParams csParam)
        {
            mRefLinkHandle = RefPoseHandle;
            mAdditiveLinkHandle = AdditivePoseHandle;
            mOutLinkHandle = OutPoseHandle;
            mRefLinkHandle.MultiLink = false;
            mAdditiveLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            mRefLinkHandle.NameStringVisible = Visibility.Visible;
            mRefLinkHandle.NameString = "ReferencePose";
            mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            mAdditiveLinkHandle.NameString = "AdditivePose";
            //mAdditiveLinkHandle.NameStringVisible = Visibility.Visible;
            //mAdditiveLinkHandle.NameString = "AdditivePose";
            AddLinkPinInfo("RefLinkHandle", mRefLinkHandle, null);
            AddLinkPinInfo("AdditiveLinkHandle", mAdditiveLinkHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "RefLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "AdditiveLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAMakeAdditivePoseControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAMakeAdditivePoseControl);
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
            var baseLinkObj = mRefLinkHandle.GetLinkedObject(0, true);
            var baseLinkElm = mRefLinkHandle.GetLinkedPinControl(0, true);
            if (baseLinkObj != null)
                await baseLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, baseLinkElm, context);
            var additiveLinkObj = mAdditiveLinkHandle.GetLinkedObject(0, true);
            var additiveLinkElm = mAdditiveLinkHandle.GetLinkedPinControl(0, true);
            if (additiveLinkObj != null)
                await additiveLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, additiveLinkElm, context);

            Type nodeType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_MakeAdditivePose);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);
            if (baseLinkObj != null)
            {
                var baseAssign = new CodeAssignStatement();
                baseAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "RefNode");
                baseAssign.Right = baseLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(baseAssign);
            }
            if (additiveLinkObj != null)
            {
                var additiveAssign = new CodeAssignStatement();
                additiveAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "AdditiveNode");
                additiveAssign.Right = additiveLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(additiveAssign);
            }
            return;
        }
    }
}
