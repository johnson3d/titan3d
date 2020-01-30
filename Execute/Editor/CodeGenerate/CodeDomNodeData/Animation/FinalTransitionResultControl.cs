using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using CodeGenerateSystem.Base;
using System.Threading.Tasks;

namespace CodeDomNode.Animation
{
    public class FinalTransitionResultConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(FinalTransitionResultConstructionParams))]
    public partial class FinalTransitionResultControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public FinalTransitionResultControl(FinalTransitionResultConstructionParams csParam) : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("FinalTransitionResultInHandle", mCtrlValueInputHandle, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "FinalTransitionResultInHandle", CodeGenerateSystem.Base.enLinkType.Bool, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "FinalTransitionResult";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Single);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Class context)
        {
            if (!mCtrlValueInputHandle.HasLink)
                return;

            var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);
            System.CodeDom.CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "Init";
            method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(method);
            var methodContext = new GenerateCodeContext_Method(context, method);
            methodContext.InstanceAnimPoseReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "AnimationPoseProxy");
            methodContext.AnimAssetAnimPoseProxyReferenceExpression = methodContext.InstanceAnimPoseReferenceExpression;
            methodContext.AnimAssetTickHostReferenceExpression = new CodeThisReferenceExpression();
            await linkObj.GCode_CodeDom_GenerateCode(codeClass, method.Statements, linkElm, methodContext);

        }

        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            if (!mCtrlValueInputHandle.HasLink)
                return;

            var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);
            System.CodeDom.CodeMemberMethod method = new CodeMemberMethod()
            {
                Name = "Transition" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Lambda",
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                ReturnType = new CodeTypeReference(typeof(bool)),
            };
            codeClass.Members.Add(method);
            var methodContext = new GenerateCodeContext_Method(context.ClassContext, method);
            methodContext.AnimStateMachineReferenceExpression = context.AnimStateMachineReferenceExpression;
            methodContext.AminStateReferenceExpression = context.AminStateReferenceExpression;
            if (!linkObj.IsOnlyReturnValue)
                await linkObj.GCode_CodeDom_GenerateCode(codeClass, method.Statements, linkElm, methodContext);
            var exp = linkObj.GCode_CodeDom_GetValue(mCtrlValueInputHandle.GetLinkedPinControl(0), methodContext);
            method.Statements.Add(new CodeMethodReturnStatement(exp));
            context.StateTransitionMethodReferenceExpression = method;
            return;
        }
    }
}
