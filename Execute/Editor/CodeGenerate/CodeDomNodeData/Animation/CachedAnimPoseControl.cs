using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using CodeGenerateSystem.Base;
using System.Threading.Tasks;


namespace CodeDomNode.Animation
{
    public class CachedAnimPoseConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(CachedAnimPoseConstructionParams))]
    public partial class CachedAnimPoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public CachedAnimPoseControl(CachedAnimPoseConstructionParams csParam) : base(csParam)
        {
            InitConstruction();

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            var clsType = mTemplateClassInstance.GetType();
            var xNamePro = clsType.GetProperty("Name");
            xNamePro.SetValue(mTemplateClassInstance, csParam.NodeName);

            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimPoseInHandle", mCtrlValueInputHandle, null);
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
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
            return null;//new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Class context)
        {
            if (!mCtrlValueInputHandle.HasLink)
                return;

            System.CodeDom.CodeMemberMethod initMethod =null;

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
            methodContext.InstanceAnimPoseReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "AnimationPoseProxy");
            methodContext.AnimAssetAnimPoseProxyReferenceExpression = methodContext.InstanceAnimPoseReferenceExpression;
            methodContext.AnimAssetTickHostReferenceExpression = new CodeThisReferenceExpression();
            await linkObj.GCode_CodeDom_GenerateCode(codeClass, initMethod.Statements, linkElm, methodContext);
            var returnExpression = linkObj.GCode_CodeDom_GetSelfRefrence(mCtrlValueInputHandle, methodContext);


            var createCachedPose = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetCachedAnimPose"), new CodePrimitiveExpression("CachedPose_" + NodeName));
            //CodeVariableDeclarationStatement cachedPoseRef = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Graphics.Mesh.Animation.CGfxAnimationPose)), NodeName);
            //CodeAssignStatement cachedPoseAssign = new CodeAssignStatement();
            //cachedPoseAssign.Left = cachedPoseRef;
            //cachedPoseAssign.Right = createCachedPose;
            //initMethod.Statements.Add(cachedPoseAssign);

            CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(returnExpression, "AnimationPoseProxy"),"Pose");
            CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            animPoseAssign.Left = animPoseField;
            animPoseAssign.Right = new CodeFieldReferenceExpression(createCachedPose, "CachedAnimationPose");
            initMethod.Statements.Add(animPoseAssign);
        }

        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            return;
            //if (!mCtrlValueInputHandle.HasLink)
            //    return;

            //var linkObj = mCtrlValueInputHandle.GetLinkedObject(0, true);
            //var linkElm = mCtrlValueInputHandle.GetLinkedPinControl(0, true);

            //var stateRef = context.AminStateReferenceExpression;

            //System.CodeDom.CodeMemberMethod method = new CodeMemberMethod()
            //{
            //    Name = stateRef.VariableName + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Lambda",
            //    Parameters =
            //    {
            //        new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.Pose.CGfxAnimationPoseProxy)), "animPoseProxy")
            //       ,new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationState)), "state")
            //    },
            //    Attributes = MemberAttributes.Public | MemberAttributes.Final,
            //    ReturnType = new CodeTypeReference(typeof(bool)),
            //};
            //codeClass.Members.Add(method);

            //context.AnimAssetAnimPoseProxyReferenceExpression = new CodeVariableReferenceExpression("animPoseProxy");
            //context.AnimAssetTickHostReferenceExpression = new CodeVariableReferenceExpression("state");
            //await linkObj.GCode_CodeDom_GenerateCode(codeClass, method.Statements, linkElm, context);

            //var codeReturn = new CodeMethodReturnStatement(new CodePrimitiveExpression(true));
            //method.Statements.Add(codeReturn);
            //CodeFieldReferenceExpression stateBeginFunctionField = new CodeFieldReferenceExpression(stateRef, "AnimStateInitializeFunction");
            //CodeAssignStatement stateBeginFunctionFieldAssign = new CodeAssignStatement();
            //stateBeginFunctionFieldAssign.Left = stateBeginFunctionField;
            //stateBeginFunctionFieldAssign.Right = new CodeMethodReferenceExpression(null, method.Name);
            //codeStatementCollection.Add(stateBeginFunctionFieldAssign);
            //return;
        }
    }
}
