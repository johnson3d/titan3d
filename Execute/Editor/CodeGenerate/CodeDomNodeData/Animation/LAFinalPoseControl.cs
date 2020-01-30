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

namespace CodeDomNode.Animation
{
    public class LAFinalPoseControlConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAFinalPoseControlConstructionParams))]
    public partial class LAFinalPoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public ImageSource BodyImage
        {
            get { return (ImageSource)GetValue(BodyImageProperty); }
            set { SetValue(BodyImageProperty, value); }
        }
        public static readonly DependencyProperty BodyImageProperty = DependencyProperty.Register("BodyImage", typeof(ImageSource), typeof(LAFinalPoseControl), new FrameworkPropertyMetadata(null));
        partial void InitConstruction();
        public LAFinalPoseControl(LAFinalPoseControlConstructionParams csParam) : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimPoseInHandle", mCtrlValueInputHandle, null);
            BodyImage = TryFindResource("AnimationNode_Result") as ImageSource;
        }
        public override bool CanDuplicate()
        {
            return false;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBlock = Template.FindName("OutGridShowText", this) as TextBlock;
            textBlock.SetBinding(TextBlock.TextProperty, new Binding("NodeName") { Source = this, Mode = BindingMode.TwoWay });
            //this.SetBinding(NodeNameBinderProperty, new Binding("Text") { Source = textBlock, Mode = BindingMode.TwoWay });
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimPoseInHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAFinalPoseControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAFinalPoseControl);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }
        public async Task GCode_CodeDom_GenerateCode_GenerateBlendTree(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);
            if (linkObj != null)
            {
                await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm, context);
                var blendTreeRootAssign = new CodeAssignStatement();
                blendTreeRootAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("state"), "BlendTreeRoot");
                blendTreeRootAssign.Right = linkObj.GCode_CodeDom_GetSelfRefrence(null,null);
                codeStatementCollection.Add(blendTreeRootAssign);
            }
        }
        public async Task GCode_CodeDom_GenerateCode_GeneratePostProcessBlendTree(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);
            if (linkObj != null)
            {
                await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm, context);
                var blendTreeRootAssign = new CodeAssignStatement();
                blendTreeRootAssign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "PostProcessBlendTreeRoot");
                blendTreeRootAssign.Right = linkObj.GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(blendTreeRootAssign);
            }
        }
    }
}
