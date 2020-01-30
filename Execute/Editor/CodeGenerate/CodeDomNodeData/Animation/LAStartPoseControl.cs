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
    public class LAStartPoseControlConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAStartPoseControlConstructionParams))]
    public partial class LAStartPoseControl : BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public ImageSource BodyImage
        {
            get { return (ImageSource)GetValue(BodyImageProperty); }
            set { SetValue(BodyImageProperty, value); }
        }
        public static readonly DependencyProperty BodyImageProperty = DependencyProperty.Register("BodyImage", typeof(ImageSource), typeof(LAStartPoseControl), new FrameworkPropertyMetadata(null));
        partial void InitConstruction();
        public LAStartPoseControl(LAStartPoseControlConstructionParams csParam) : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimPoseOutHandle", mCtrlValueOutputHandle, null);
            BodyImage = TryFindResource("AnimationNode_Result") as ImageSource;
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
            CollectLinkPinInfo(smParam, "AnimPoseOutHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAStartPoseControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAStartPoseControl);
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
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            Type clipType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_CopyPose);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(clipType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(clipType)));
            codeStatementCollection.Add(stateVarDeclaration);

            var inPoseAssign = new CodeAssignStatement();
            inPoseAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "InPose");
            inPoseAssign.Right = new CodeVariableReferenceExpression("inPose");
            codeStatementCollection.Add(inPoseAssign);
            return;
        }
    }
}
