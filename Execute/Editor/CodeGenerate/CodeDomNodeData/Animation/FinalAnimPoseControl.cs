using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using CodeGenerateSystem.Base;
using System.Threading.Tasks;

namespace CodeDomNode.Animation
{
    public class FinalAnimPoseConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(FinalAnimPoseConstructionParams))]
    public partial class FinalAnimPoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public FinalAnimPoseControl(FinalAnimPoseConstructionParams csParam) : base(csParam)
        {
            InitConstruction();
            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimPoseInHandle", mCtrlValueInputHandle, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimPoseInHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "AnimationPose";
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
            System.CodeDom.CodeMemberMethod initMethod = null;

            foreach (var member in codeClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == "Init")
                        initMethod = method;
                }
            }
            if (initMethod == null)
            {
                initMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                initMethod.Name = "Init";
                initMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                codeClass.Members.Add(initMethod);
            }


            var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);
            var methodContext = new GenerateCodeContext_Method(context, initMethod);

            await GenerateCachedPose(codeClass, initMethod.Statements, linkElm, methodContext);

            methodContext.InstanceAnimPoseReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "AnimationPoseProxy");
            methodContext.AnimAssetAnimPoseProxyReferenceExpression = methodContext.InstanceAnimPoseReferenceExpression;
            methodContext.AnimAssetTickHostReferenceExpression = new CodeThisReferenceExpression();
            await linkObj.GCode_CodeDom_GenerateCode(codeClass, initMethod.Statements, linkElm, methodContext);
            var returnExp = linkObj.GCode_CodeDom_GetSelfRefrence(mCtrlValueInputHandle, methodContext);
            if (returnExp != null)
            {
                CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(returnExp, "AnimationPoseProxy"),"Pose");
                CodeAssignStatement animPoseAssign = new CodeAssignStatement();
                animPoseAssign.Left = animPoseField;
                animPoseAssign.Right = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "AnimationPoseProxy"),"Pose");
                initMethod.Statements.Add(animPoseAssign);
            }
        }
        public async System.Threading.Tasks.Task GenerateCachedPose(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            List<string> cachedPoseNames = new List<string>();
            foreach (var ctrl in HostNodesContainer.CtrlNodeList)
            {
                if (ctrl is CachedAnimPoseControl)
                {
                    if (!cachedPoseNames.Contains(ctrl.NodeName))
                        cachedPoseNames.Add(ctrl.NodeName);
                }
            }
            foreach (var sub in HostNodesContainer.HostControl.SubNodesContainers)
            {
                foreach (var ctrl in sub.Value.CtrlNodeList)
                {
                    if (ctrl is CachedAnimPoseControl)
                    {
                        if (!cachedPoseNames.Contains(ctrl.NodeName))
                            cachedPoseNames.Add(ctrl.NodeName);
                    }
                }
            }
            foreach (var cachedPoseName in cachedPoseNames)
            {
                var createCachedPose = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateCachedAnimPose"), new CodePrimitiveExpression("CachedPose_" + cachedPoseName));
                codeStatementCollection.Add(createCachedPose);
            }
          
        }
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            if (!mCtrlValueInputHandle.HasLink)
                return;

            var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);

            var stateRef = context.AminStateReferenceExpression;

            System.CodeDom.CodeMemberMethod method = new CodeMemberMethod()
            {
                Name = stateRef.VariableName + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Lambda",
                Parameters =
                {
                    new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.Pose.CGfxAnimationPoseProxy)), "animPoseProxy")
                   ,new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationState)), "state")
                },
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                ReturnType = new CodeTypeReference(typeof(bool)),
            };
            codeClass.Members.Add(method);

            context.AnimAssetAnimPoseProxyReferenceExpression = new CodeVariableReferenceExpression("animPoseProxy");
            context.AnimAssetTickHostReferenceExpression = new CodeVariableReferenceExpression("state");
            await linkObj.GCode_CodeDom_GenerateCode(codeClass, method.Statements, linkElm, context);

            var codeReturn = new CodeMethodReturnStatement(new CodePrimitiveExpression(true));
            method.Statements.Add(codeReturn);
            CodeFieldReferenceExpression stateBeginFunctionField = new CodeFieldReferenceExpression(stateRef, "AnimStateInitializeFunction");
            CodeAssignStatement stateBeginFunctionFieldAssign = new CodeAssignStatement();
            stateBeginFunctionFieldAssign.Left = stateBeginFunctionField;
            stateBeginFunctionFieldAssign.Right = new CodeMethodReferenceExpression(null, method.Name);
            codeStatementCollection.Add(stateBeginFunctionFieldAssign);
            return;
        }
    }
}
