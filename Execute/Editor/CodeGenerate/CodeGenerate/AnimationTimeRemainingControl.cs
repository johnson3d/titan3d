using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerateSystem.Controls
{
    public class AnimationTimeRemainingConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(AnimationTimeRemainingConstructionParams))]
    public partial class AnimationTimeRemainingControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public AnimationTimeRemainingControl(AnimationTimeRemainingConstructionParams csParam) : base(csParam)
        {
            InitConstruction();

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, true);

            var clsType = mTemplateClassInstance.GetType();
            var xNamePro = clsType.GetProperty("Name");
            xNamePro.SetValue(mTemplateClassInstance, csParam.NodeName);

            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("TimeRemainingHandle", mCtrlValueOutputHandle, null);
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TimeRemainingHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "TimeRemaining";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Single);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetAnimationSequenceRemainingTime"),new CodeExpression[] { new CodePrimitiveExpression(context.AnimStateMachineReferenceExpression.VariableName),new CodePrimitiveExpression(context.AminStateReferenceExpression.VariableName),new CodePrimitiveExpression(NodeName) });
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Class context)
        {
            if (!mCtrlValueOutputHandle.HasLink)
                return;

            //System.CodeDom.CodeMemberMethod initMethod = null;

            //foreach (var member in codeClass.Members)
            //{
            //    if (member.GetType() == typeof(CodeMemberMethod))
            //    {
            //        var method = member as CodeMemberMethod;
            //        if (method.Name == "Init")
            //            initMethod = method;
            //    }
            //}
            //if (initMethod == null)
            //{
            //    initMethod = new CodeMemberMethod();
            //    initMethod.Name = "Init";
            //    initMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            //    codeClass.Members.Add(initMethod);
            //}

            //var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            //var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);
            //var methodContext = new GenerateCodeContext_Method(context, initMethod);
            //methodContext.InstanceAnimPoseReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "AnimationPoseProxy");
            //methodContext.AnimAssetAnimPoseProxyReferenceExpression = methodContext.InstanceAnimPoseReferenceExpression;
            //methodContext.AnimAssetTickHostReferenceExpression = new CodeThisReferenceExpression();
            //await linkObj.GCode_CodeDom_GenerateCode(codeClass, initMethod.Statements, linkElm, methodContext);
            //var returnExpression = linkObj.GCode_CodeDom_GetSelfRefrence(mCtrlValueInputHandle, methodContext);


            //CodeMethodInvokeExpression createCachedPose = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetCachedAnimPose"), new CodePrimitiveExpression("CachedPose_" + NodeName));
            ////CodeVariableDeclarationStatement cachedPoseRef = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Graphics.Mesh.Animation.CGfxAnimationPose)), NodeName);
            ////CodeAssignStatement cachedPoseAssign = new CodeAssignStatement();
            ////cachedPoseAssign.Left = cachedPoseRef;
            ////cachedPoseAssign.Right = createCachedPose;
            ////initMethod.Statements.Add(cachedPoseAssign);

            //CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(returnExpression, "AnimationPoseProxy"), "Pose");
            //CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            //animPoseAssign.Left = animPoseField;
            //animPoseAssign.Right = new CodeFieldReferenceExpression(createCachedPose, "CachedAnimationPose");
            //initMethod.Statements.Add(animPoseAssign);
        }

        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, Base.LinkPinControl element, GenerateCodeContext_Method context)
        {
            return;
            if (!mCtrlValueOutputHandle.HasLink)
                return;

            var linkObj = mCtrlValueOutputHandle.GetLinkedObject(0, true);
            var linkElm = mCtrlValueOutputHandle.GetLinkedPinControl(0, true);

            var stateRef = context.AminStateReferenceExpression;

            System.CodeDom.CodeMemberMethod method = new CodeMemberMethod()
            {
                Name = stateRef.VariableName + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Lambda",
                Parameters =
                {
                    new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Graphics.Mesh.Animation.CGfxAnimationPoseProxy)), "animPoseProxy")
                   ,new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.GamePlay.StateMachine.AnimationState)), "state")
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
